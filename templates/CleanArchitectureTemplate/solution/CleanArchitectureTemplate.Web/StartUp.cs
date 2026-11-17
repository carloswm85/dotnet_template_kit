using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;
using CleanArchitectureTemplate.ApplicationCore.Constants;
using CleanArchitectureTemplate.ApplicationCore.Extensions;
using CleanArchitectureTemplate.ApplicationCore.Interfaces;
using CleanArchitectureTemplate.ApplicationCore.Interfaces.ContosoUniversity;
using CleanArchitectureTemplate.ApplicationCore.Interfaces.Identity;
using CleanArchitectureTemplate.ApplicationCore.Mappings;
using CleanArchitectureTemplate.Infrastructure.Authorization.TestContactAuthorization;
using CleanArchitectureTemplate.Infrastructure.Interfaces.Identity;
using CleanArchitectureTemplate.Infrastructure.Models;
using CleanArchitectureTemplate.Infrastructure.Models.Identity;
using CleanArchitectureTemplate.Infrastructure.Services;
using CleanArchitectureTemplate.Infrastructure.Services.ContosoUniversity;
using CleanArchitectureTemplate.Infrastructure.Services.External;
using CleanArchitectureTemplate.Infrastructure.Services.Identity;
using CleanArchitectureTemplate.Web.HealthChecks;
using CleanArchitectureTemplate.Web.Mappings;
using CleanArchitectureTemplate.Web.Middleware;
using CleanArchitectureTemplate.Web.Services;
using CleanArchitectureTemplate.Web.Services.Interfaces;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

namespace CleanArchitectureTemplate.Web;

public class StartUp
{
    private const string DataProtectionKeysDirectory = "/app/DataProtection-Keys";

    public StartUp(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; set; }

    private static bool IsRunningInContainer =>
        string.Equals(
            Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
            "true",
            StringComparison.OrdinalIgnoreCase
        );

    // === SERVICES
    // Called by the runtime to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        #region Database and identity

        // Connection string "DefaultConnection" comes from configuration.
        var connectionString =
            Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found."
            );

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure())
        );

        services
            .AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<IdentityOptions>(options =>
        {
            // Sign-in
            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedPhoneNumber = false;

            // Password
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 1;

            // Lockout
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 3;
            options.Lockout.AllowedForNewUsers = true;
        });

        // Re-validate the auth cookie against the security stamp every minute,
        // so deactivating a user or changing their roles ends existing sessions
        // quickly (see IdentityController.UserEdit).
        services.Configure<SecurityStampValidatorOptions>(options =>
        {
            options.ValidationInterval = TimeSpan.FromMinutes(1);
        });

        services.AddAuthentication();
        services.AddAuthorization(options => options.AddApplicationPolicies());

        // Resource-based authorization handlers (TestContact example)
        services.AddScoped<IAuthorizationHandler, TestContactIsOwnerAuthorizationHandler>();
        services.AddSingleton<
            IAuthorizationHandler,
            TestContactAdministratorsAuthorizationHandler
        >();
        services.AddSingleton<IAuthorizationHandler, TestContactManagerAuthorizationHandler>();

        #endregion

        #region Data protection

        // In a container, persist keys to a mounted volume so auth cookies and
        // antiforgery tokens survive restarts and redeploys.
        AddContainerDataProtection(services);

        #endregion

        #region Hosting: forwarded headers and rate limiting

        AddForwardedHeaders(services);
        AddRateLimiting(services);

        #endregion

        #region MVC and localization

        services.AddLocalization(options => options.ResourcesPath = "Resources");

        services
            .AddControllersWithViews(config =>
            {
                // Every page requires a signed-in user unless marked [AllowAnonymous].
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            })
            .AddViewLocalization()
            .AddDataAnnotationsLocalization()
            .AddXmlDataContractSerializerFormatters();

        #endregion

        #region Mapping and caching

        var mapsterConfig = TypeAdapterConfig.GlobalSettings;
        mapsterConfig.Scan(
            typeof(AppMappingRegister).Assembly,
            typeof(WebMappingRegister).Assembly
        );

        services.AddSingleton(mapsterConfig);
        services.AddScoped<IMapper, ServiceMapper>();
        services.AddMapster();

        services.AddMemoryCache();

        #endregion

        #region Health checks

        services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("database", tags: ["ready"]);

        #endregion

        #region HTTP client

        services.AddHttpClient(
            "ApiClient",
            (provider, client) =>
            {
                var apiBaseUrl = Configuration["ApiBaseUrl"];
                if (string.IsNullOrWhiteSpace(apiBaseUrl))
                {
                    throw new InvalidOperationException(
                        "ApiBaseUrl configuration is missing or empty."
                    );
                }
                client.BaseAddress = new Uri(apiBaseUrl);
            }
        );

        #endregion

        #region Identity and external services

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<IUserManagerService, UserManagerService>();
        services.AddScoped<ISignInManagerService, SignInManagerService>();
        services.AddScoped<IRoleManagerService, RoleManagerService>();

        services.AddTransient<IEmailSender, MessagingService>();
        services.AddTransient<ISmsSender, MessagingService>();

        #endregion

        #region Application services

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IPaginationService, PaginationService>();
        services.AddSingleton<IImageStorageService, ImageStorageService>();

        // Runtime settings (AppSetting table), used by AppAvailabilityMiddleware
        services.AddScoped<IAppSettingService, AppSettingService>();

        // Migrations and sample data at startup (skipped in Production)
        services.AddHostedService<DatabaseInitializerHostedService>();

        #endregion

        #region Example services

        services.AddScoped<IContosoUniversityService, ContosoUniversityService>();
        services.AddScoped<IWeatherForecastService, WeatherForecastService>();

        #endregion
    }

    // === REQUEST PIPELINE
    // Called by the runtime to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        #region Proxy and path base

        // Must run first so later middleware sees the client's scheme and IP.
        if (Configuration.GetValue<bool>("Hosting:ForwardedHeaders:Enabled"))
        {
            app.UseForwardedHeaders();
        }

        // For hosting under a sub-path behind a reverse proxy, e.g. "/apps/web".
        var pathBase = Configuration["Hosting:PathBase"];
        if (!string.IsNullOrWhiteSpace(pathBase))
        {
            app.UsePathBase(pathBase);
        }

        #endregion

        #region Error handling and transport security

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");

            // Re-execute the pipeline through ErrorController for status codes;
            // {0} is the status code.
            app.UseStatusCodePagesWithReExecute("/Error/{0}");

            // HTTP Strict Transport Security (default 30 days).
            app.UseHsts();
        }

        // Convert SQL Server connectivity failures into a database-independent
        // 503 page. Must run before authentication and any middleware that can
        // access the database.
        app.UseMiddleware<DatabaseUnavailableMiddleware>();

        // Only redirect to HTTPS when NOT running in a container
        if (!IsRunningInContainer)
        {
            app.UseHttpsRedirection();
        }

        #endregion

        #region Static files and localization

        app.UseStaticFiles();

        // Uploaded images, from "Uploads:RootPath" (a volume in Docker)
        var uploadsRoot = ImageStorageService.GetRootPath(env, Configuration);
        Directory.CreateDirectory(uploadsRoot);
        app.UseStaticFiles(
            new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(uploadsRoot),
                RequestPath = ImageStorageService.RequestPath,
            }
        );

        app.UseRequestLocalization(CreateLocalizationOptions());

        #endregion

        #region Routing and authorization

        app.UseRouting();
        app.UseRateLimiter();

        app.UseAuthentication();

        // Runs after authentication so SuperAdmin users can bypass maintenance.
        app.UseMiddleware<AppAvailabilityMiddleware>();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints
                .MapHealthChecks(
                    ServiceStatusResponse.LivePath,
                    new HealthCheckOptions
                    {
                        // Liveness runs no checks: it only proves the process responds.
                        Predicate = _ => false,
                        ResponseWriter = (context, _) =>
                            ServiceStatusResponse.WriteLiveHealthAsync(
                                context,
                                context.RequestAborted
                            ),
                    }
                )
                .AllowAnonymous();

            endpoints
                .MapHealthChecks(
                    ServiceStatusResponse.ReadyPath,
                    new HealthCheckOptions
                    {
                        Predicate = registration => registration.Tags.Contains("ready"),
                        ResponseWriter = (context, report) =>
                            ServiceStatusResponse.WriteReadyHealthAsync(
                                context,
                                report.Status,
                                context.RequestAborted
                            ),
                    }
                )
                .AllowAnonymous();

            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );
        });

        #endregion
    }

    #region Private helpers

    private void AddContainerDataProtection(IServiceCollection services)
    {
        if (!IsRunningInContainer)
        {
            return;
        }

        var applicationName = Configuration["DataProtection:ApplicationName"];

        if (string.IsNullOrWhiteSpace(applicationName))
        {
            throw new InvalidOperationException(
                "DataProtection:ApplicationName is required when running in a container."
            );
        }

        services
            .AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(DataProtectionKeysDirectory))
            .SetApplicationName(applicationName);
    }

    /// <summary>
    /// Trusts X-Forwarded-For/Proto from a reverse proxy. Configure
    /// "Hosting:ForwardedHeaders:KnownProxies" with the proxy IPs; when the list
    /// is empty every source is trusted, which is safe only if the app is
    /// reachable exclusively through the proxy (e.g. a private Docker network).
    /// </summary>
    private void AddForwardedHeaders(IServiceCollection services)
    {
        var section = Configuration.GetSection("Hosting:ForwardedHeaders");
        if (!section.GetValue<bool>("Enabled"))
        {
            return;
        }

        var knownProxies = section.GetSection("KnownProxies").Get<string[]>() ?? [];

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();

            foreach (var proxy in knownProxies)
            {
                options.KnownProxies.Add(IPAddress.Parse(proxy));
            }
        });
    }

    /// <summary>
    /// Fixed-window limit per client IP for authentication endpoints
    /// (see [EnableRateLimiting] in AccountController).
    /// </summary>
    private void AddRateLimiting(IServiceCollection services)
    {
        var section = Configuration.GetSection("RateLimiting:Authentication");
        var permitLimit = section.GetValue("PermitLimit", 10);
        var window = TimeSpan.FromSeconds(section.GetValue("WindowSeconds", 60));

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(
                RateLimitPolicyNames.Authentication,
                httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = permitLimit,
                            Window = window,
                            QueueLimit = 0,
                        }
                    )
            );
        });
    }

    /// <summary>
    /// Cultures from "Localization:DefaultCulture" and
    /// "Localization:SupportedCultures" (defaults to en-US).
    /// </summary>
    private RequestLocalizationOptions CreateLocalizationOptions()
    {
        var defaultCulture = Configuration["Localization:DefaultCulture"] ?? "en-US";
        var cultures = (
            Configuration.GetSection("Localization:SupportedCultures").Get<string[]>()
            ?? [defaultCulture]
        )
            .Select(name => new CultureInfo(name))
            .ToList();

        return new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture(defaultCulture),
            SupportedCultures = cultures,
            SupportedUICultures = cultures,
        };
    }

    #endregion
}
