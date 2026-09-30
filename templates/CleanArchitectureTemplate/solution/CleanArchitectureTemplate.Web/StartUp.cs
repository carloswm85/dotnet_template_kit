using CleanArchitectureTemplate.ApplicationCore.Constants;
using CleanArchitectureTemplate.ApplicationCore.Interfaces;
using CleanArchitectureTemplate.ApplicationCore.Interfaces.ContosoUniversity;
using CleanArchitectureTemplate.ApplicationCore.Interfaces.Identity;
using CleanArchitectureTemplate.ApplicationCore.Mappings;
using CleanArchitectureTemplate.Infrastructure.Authorization.TestContactAuthorization;
using CleanArchitectureTemplate.Infrastructure.Data;
using CleanArchitectureTemplate.Infrastructure.Interfaces.IdentityInterfaces;
using CleanArchitectureTemplate.Infrastructure.Model;
using CleanArchitectureTemplate.Infrastructure.Model.Identity;
using CleanArchitectureTemplate.Infrastructure.Services;
using CleanArchitectureTemplate.Infrastructure.Services.ContosoServices;
using CleanArchitectureTemplate.Infrastructure.Services.ExternalServices;
using CleanArchitectureTemplate.Infrastructure.Services.IdentityServices;
using CleanArchitectureTemplate.Web.Mappings;
using CleanArchitectureTemplate.Web.Services;
using CleanArchitectureTemplate.Web.Services.Interfaces;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureTemplate.Web;

public class StartUp
{
    public StartUp(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; set; }

    // === SERVICES
    // This method gets called by the runtime. Use this method to add services to the container.
    public async void ConfigureServices(IServiceCollection services)
    {
        #region Services Configuration

        // Connection string "DefaultConnection" is pulled from configuration (appsettings.json).
        var connectionString =
            Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found."
            );

        // Register DbContext with SQL Server as the database provider.
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure())
        );

        // === IDENTITY API CONFIGURATION ===
        //services.AddIdentityCore<ApplicationUser>(
        //        options => options.SignIn.RequireConfirmedAccount = true
        //    )
        //    .AddRoles<ApplicationRole>()
        //    .AddEntityFrameworkStores<ApplicationDbContext>()
        //    .AddSignInManager()
        //    .AddDefaultTokenProviders();

        services
            .AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddAuthentication();
        services.AddAuthorization();

        services.Configure<IdentityOptions>(options =>
        {
            // Default SignIn settings
            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedPhoneNumber = false;

            // Default Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 1;

            // Default Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 3;
            options.Lockout.AllowedForNewUsers = true;
        });

        // === MAPPINGS ===
        var mapsterConfig = TypeAdapterConfig.GlobalSettings;
        mapsterConfig.Scan(
            typeof(ApplicationMappingRegister).Assembly,
            typeof(WebMappingRegister).Assembly
        );

        services.AddSingleton(mapsterConfig);
        services.AddScoped<IMapper, ServiceMapper>();

        // === AUTHORIZATION HANDLERS ===
        services.AddScoped<IAuthorizationHandler, TestContactIsOwnerAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, TestContactAdministratorsAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, TestContactManagerAuthorizationHandler>();

        // === HTTP CLIENT ===
        // Configure HttpClient with base address from configuration
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

        services.AddLocalization(options => options.ResourcesPath = "Resources");

        services
            .AddControllersWithViews(config =>
            {
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            })
            .AddViewLocalization()
            .AddDataAnnotationsLocalization()
            .AddXmlDataContractSerializerFormatters();

        services.AddMapster();

        // Identity Services
        services.AddScoped<IUserManagerService, UserManagerService>();
        services.AddScoped<ISignInManagerService, SignInManagerService>();
        services.AddScoped<IRoleManagerService, RoleManagerService>();

        // External Services
        services.AddTransient<IEmailSender, MessagingService>();
        services.AddTransient<ISmsSender, MessagingService>();

        // Application services
        services.AddScoped<IPaginationService, PaginationService>();
        services.AddScoped<IContosoUniversityService, ContosoUniversityService>();
        services.AddScoped<IWeatherForecastService, WeatherForectastService>();

        #endregion
    }

    // === REQUEST PIPELINE
    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public async void Configure(
        IApplicationBuilder app,
        IWebHostEnvironment env,
        ILoggerFactory loggerFactory
    )
    {
        #region Middleware Configuration

        if (env.IsDevelopment())
        {
            // Enable the Developer Exception Page in the development environment.
            app.UseDeveloperExceptionPage();
        }
        else
        {
            // Use a custom error handling page for production.
            // app.UseExceptionHandler("/Home/Error");
            app.UseExceptionHandler("/Error");

            // REMEMBER THIS WORKS ONLY WITH:
            // "ASPNETCORE_ENVIRONMENT": "Production"

            // Returns error as plain text
            // app.UseStatusCodePages(); // (1)

            // Intersect error and return a view
            // {0} is a placeholder for the status code
            // app.UseStatusCodePagesWithRedirects("/Error/{0}"); // (2) Redirect to the string controller
            app.UseStatusCodePagesWithReExecute("/Error/{0}"); // (3) Re-executes the pipeline

            // Enable HTTP Strict Transport Security (HSTS) for enhanced security in production.
            // The default duration is 30 days; you can adjust this value based on your requirements.
            app.UseHsts();
        }

        app.UseStaticFiles();

        // Only redirect to HTTPS when NOT running in a container
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER")))
            app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseRequestLocalization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );
        });

        // Add Contoso University test data to the database
        // Generate identity users with roles
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var serviceProvider = scope.ServiceProvider;
            var logger = serviceProvider.GetRequiredService<ILogger<StartUp>>();
            try
            {
                // Set password with the Secret Manager tool.
                //  `dotnet user-secrets set "SeedUserPW" "!Abc123Antartica" --project .\solution\CleanArchitectureTemplate.Web\`
                var testUserPw =
                    Configuration.GetValue<string>("SeedUserPW")
                    ?? ApplicationConstants.TestPassword;
                await DbInitializer.Initialize(serviceProvider, testUserPw);

                logger.LogDebug("DB successfully initialized from the MVC layer.");
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "An error occurred while creating the database from the MVC layer."
                );
            }
        }

        #endregion
    }
}
