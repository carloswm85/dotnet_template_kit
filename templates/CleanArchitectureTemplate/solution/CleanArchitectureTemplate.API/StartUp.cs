using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Asp.Versioning;
using CleanArchitectureTemplate.API.Constants;
using CleanArchitectureTemplate.API.HealthChecks;
using CleanArchitectureTemplate.API.Middleware;
using CleanArchitectureTemplate.ApplicationCore.Constants;
using CleanArchitectureTemplate.ApplicationCore.Extensions;
using CleanArchitectureTemplate.ApplicationCore.Interfaces;
using CleanArchitectureTemplate.ApplicationCore.Interfaces.ContosoUniversity;
using CleanArchitectureTemplate.ApplicationCore.Interfaces.Identity;
using CleanArchitectureTemplate.Infrastructure.Models;
using CleanArchitectureTemplate.Infrastructure.Models.Identity;
using CleanArchitectureTemplate.Infrastructure.Services;
using CleanArchitectureTemplate.Infrastructure.Services.ContosoUniversity;
using CleanArchitectureTemplate.Infrastructure.Services.External;
using CleanArchitectureTemplate.Infrastructure.Services.Identity;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace CleanArchitectureTemplate.API;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    // ------------------------------------------------------------
    // Service registration
    // ------------------------------------------------------------
    public void ConfigureServices(IServiceCollection services)
    {
        #region Database, errors and hosting

        // Connection string validation (fail fast)
        var connectionString =
            Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Missing connection string 'DefaultConnection'."
            );

        // DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString)
        );

        // RFC 7807 problem details for unhandled exceptions. This also enables
        // the parameterless UseExceptionHandler() used outside Development.
        services.AddProblemDetails();

        // Data Protection: in a container, persist keys to a mounted volume so
        // Identity tokens survive restarts and redeploys.
        AddContainerDataProtection(services);

        // Health checks
        services
            .AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"]);

        // Reverse proxy support and rate limiting for authentication endpoints
        AddForwardedHeaders(services);
        AddRateLimiting(services);

        #endregion

        #region Identity

        // Identity endpoints
        services
            .AddIdentityApiEndpoints<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        #endregion

        #region Authentication and Authorization

        // Authorization + fallback policy
        services.AddAuthorization(options =>
        {
            // Only requires auth when [Authorize] is explicitly declared
            options.FallbackPolicy = null; // default behavior, no global enforcement

            // DefaultPolicy applies when [Authorize] is used with no parameters
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            options.AddApplicationPolicies();
        });

        // JWT Authentication: fail fast on missing or weak settings
        var jwtSettings = Configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];

        if (string.IsNullOrWhiteSpace(secretKey))
            throw new InvalidOperationException("JWT SecretKey is not configured.");
        if (Encoding.UTF8.GetByteCount(secretKey) < 32)
            throw new InvalidOperationException("JWT SecretKey must be at least 32 bytes.");
        if (string.IsNullOrWhiteSpace(issuer))
            throw new InvalidOperationException("JWT Issuer is not configured.");
        if (string.IsNullOrWhiteSpace(audience))
            throw new InvalidOperationException("JWT Audience is not configured.");
        if (
            !double.TryParse(jwtSettings["ExpirationMinutes"], out var expirationMinutes)
            || expirationMinutes <= 0
        )
            throw new InvalidOperationException(
                "JWT ExpirationMinutes must be greater than zero."
            );
        if (
            !double.TryParse(
                jwtSettings["RefreshTokenLifetimeMinutes"],
                out var refreshTokenLifetimeMinutes
            )
            || refreshTokenLifetimeMinutes <= 0
        )
            throw new InvalidOperationException(
                "JWT RefreshTokenLifetimeMinutes must be greater than zero."
            );

        // Configure authentication services and define the default scheme
        services
            .AddAuthentication(options =>
            {
                // Specifies which scheme will be used to authenticate the user by default
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

                // Specifies which scheme will be used when the framework challenges an unauthenticated user
                // (for example, when accessing an endpoint decorated with [Authorize])
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            // Register JWT Bearer authentication handler
            .AddJwtBearer(options =>
            {
                // Enforces HTTPS when retrieving metadata (should be true in production)
                // Can be disabled in development environments if needed
                options.RequireHttpsMetadata = true;

                // Stores the token inside AuthenticationProperties after successful authentication
                // Useful if you need to access the raw token later in the request pipeline
                options.SaveToken = true;

                // Defines how incoming JWT tokens should be validated
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true, // Ensures the token was signed with a valid security key

                    IssuerSigningKey = new SymmetricSecurityKey( // Defines the symmetric key used to validate the token signature
                        Encoding.UTF8.GetBytes(secretKey)
                    ),

                    ValidateIssuer = true, // Ensures the token was issued by a trusted authority
                    ValidateAudience = true, // Ensures the token is intended for this specific audience (your API)
                    ValidateLifetime = true, // Ensures the token has not expired

                    ValidIssuer = issuer, // Expected issuer value (must match token's "iss" claim)
                    ValidAudience = audience, // Expected audience value (must match token's "aud" claim)
                    ClockSkew = TimeSpan.Zero, // Expire exactly at "exp" (default allows 5 extra minutes)
                    NameClaimType = ClaimTypes.NameIdentifier, // Defines which claim represents the user's unique identifier
                    RoleClaimType = ClaimTypes.Role, // Defines which claim represents user roles
                };

                // Configure JWT authentication events
                options.Events = new JwtBearerEvents
                {
                    // Triggered after the token has passed signature and validation checks
                    OnTokenValidated = async context =>
                    {
                        // Extract the JTI (unique token identifier) claim
                        var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                        // If the token does not contain a JTI, reject it
                        if (string.IsNullOrEmpty(jti))
                        {
                            context.Fail("Token missing jti");
                            return;
                        }

                        // Resolve your custom token service from dependency injection
                        var tokenService =
                            context.HttpContext.RequestServices.GetRequiredService<ITokenService>();

                        // Check whether this token has been revoked (e.g., logout, blacklist, security event)
                        if (
                            await tokenService.IsTokenRevokedAsync(
                                jti,
                                context.HttpContext.RequestAborted
                            )
                        )
                        {
                            // Reject the token if it is found in the revocation store
                            context.Fail("Token has been revoked");
                            return;
                        }

                        // Reject every token issued before a user-wide revocation
                        // ("log out everywhere", password change, deactivation).
                        var userId =
                            context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? context.Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                        var issuedAtMilliseconds = context
                            .Principal?.FindFirst(TokenService.IssuedAtMillisecondsClaimType)
                            ?.Value;

                        if (
                            string.IsNullOrEmpty(userId)
                            || !long.TryParse(issuedAtMilliseconds, out var issuedAt)
                        )
                        {
                            context.Fail("Token missing user or issuance metadata");
                            return;
                        }

                        if (
                            await tokenService.IsUserRevokedAsync(
                                userId,
                                issuedAt,
                                context.HttpContext.RequestAborted
                            )
                        )
                        {
                            context.Fail("All user tokens have been revoked");
                        }
                    },
                };
            });

        #endregion

        // Caching (optional, but can improve performance for certain scenarios)
        services.AddDistributedMemoryCache();

        services.AddResponseCaching(options =>
        {
            options.MaximumBodySize = 1024 * 1024; // 1 MiB
            options.UseCaseSensitivePaths = true;
        });

        #region Application Services

        // Services Layer
        services.AddScoped<IContosoUniversityService, ContosoUniversityService>();

        // Web.API Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailSender, MessagingService>();
        services.AddScoped<ISmsSender, MessagingService>();

        // Runtime settings (AppSetting table), used by AppAvailabilityMiddleware
        services.AddMemoryCache();
        services.AddScoped<IAppSettingService, AppSettingService>();

        #endregion

        #region Controllers, OpenAPI, versioning and CORS

        services.AddMapster();

        // Controllers
        services
            .AddControllers(options =>
            {
                options.CacheProfiles.Add(CacheProfiles.Default10Sec, CacheProfiles.Profile10);
                options.CacheProfiles.Add(CacheProfiles.Default60Sec, CacheProfiles.Profile60);
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

        // OpenAPI
        // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/docs/configure-and-customize-swaggergen.md
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Version = "v1",
                    Title = "CleanArchitectureTemplate API V1",
                    Description = "An ASP.NET Core Web API for your resourses.",
                    TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "Example TestContact",
                        Url = new Uri("https://example.com/contact"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Example Use License",
                        Url = new Uri("https://example.com/license"),
                    },
                }
            );

            options.SwaggerDoc(
                "v2",
                new OpenApiInfo
                {
                    Version = "v2",
                    Title = "CleanArchitectureTemplate API V2",
                    Description = "An ASP.NET Core Web API for your resourses.",
                    TermsOfService = new Uri("https://example.com/terms"),
                }
            );

            options.AddSecurityDefinition(
                "bearer",
                new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme.",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    In = ParameterLocation.Header,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                }
            );

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("bearer", document)] = [],
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
            options.CustomSchemaIds(t => t.FullName!.Replace("+", "."));
        });

        var apiVersioningBuilder = services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;

            // Enabling multiple versioning methods (optional, choose based on your needs)
            /* In this case: With query string versioning results:
             *      ?api-version=1.0 //IN QUERY STRING
             *      v{version} //IN PARAMETER PATH
             */
            //options.ApiVersionReader = ApiVersionReader.Combine(new QueryStringApiVersionReader("api-version"));
        });
        apiVersioningBuilder.AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV"; // e.g., "v1", "v2", etc.
            options.SubstituteApiVersionInUrl = true; // api/v{version}/resource
        });

        services.AddCors(options =>
        {
            options.AddPolicy(
                PolicyNames.AllowSpecificOrigin,
                builder =>
                {
                    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
                }
            );
        });

        #endregion
    }

    private const string DataProtectionKeysDirectory = "/app/DataProtection-Keys";

    private void AddContainerDataProtection(IServiceCollection services)
    {
        var isRunningInContainer = string.Equals(
            Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
            "true",
            StringComparison.OrdinalIgnoreCase
        );

        if (!isRunningInContainer)
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
    /// "Swagger:Enabled" when set; otherwise enabled only in Development.
    /// </summary>
    private bool IsSwaggerEnabled(IWebHostEnvironment env) =>
        Configuration.GetValue<bool?>("Swagger:Enabled") ?? env.IsDevelopment();

    /// <summary>
    /// Trusts X-Forwarded-For/Proto from a reverse proxy. Configure
    /// "Hosting:ForwardedHeaders:KnownProxies" with the proxy IPs; when the list
    /// is empty every source is trusted, which is safe only if the API is
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
    /// (see [EnableRateLimiting] in AccountApiController).
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

    // ------------------------------------------------------------
    // Middleware pipeline configuration
    // ------------------------------------------------------------
    public void ConfigurePipeline(IApplicationBuilder app, IWebHostEnvironment env)
    {
        #region Proxy and path base

        // Must run first so later middleware sees the client's scheme and IP.
        if (Configuration.GetValue<bool>("Hosting:ForwardedHeaders:Enabled"))
        {
            app.UseForwardedHeaders();
        }

        // For hosting under a sub-path behind a reverse proxy, e.g. "/apis/app".
        var pathBase = Configuration["Hosting:PathBase"];
        if (!string.IsNullOrWhiteSpace(pathBase))
        {
            app.UsePathBase(pathBase);
        }

        #endregion

        #region Error handling and OpenAPI

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler();
        }

        // Swagger UI and JSON only when enabled (Development by default; see
        // IsSwaggerEnabled). Publishing the API description in production gives
        // attackers a map of every endpoint.
        if (IsSwaggerEnabled(env))
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                // Relative URLs so the UI also works under a path base.
                options.SwaggerEndpoint("v1/swagger.json", "v1");
                options.SwaggerEndpoint("v2/swagger.json", "v2");

                // Also, versions can be separated in folders by controller grouping
                /*  Controllers/
                 *    ├── V1/
                 *    │   └── ProductsController.cs
                 *    ├── V2/
                 *    │   └── ProductsController.cs
                 *   etc.
                 */
            });
        }

        app.UseStaticFiles();

        #endregion

        // Convert SQL Server connectivity failures into a 503 JSON response.
        // Registered after the exception handlers so it sees these exceptions
        // first, then short-circuit every request while in maintenance.
        app.UseMiddleware<DatabaseUnavailableMiddleware>();
        app.UseMiddleware<AppAvailabilityMiddleware>();

        app.UseRouting();
        app.UseRateLimiter();

        // UseCors must be called before UseResponseCaching
        app.UseCors(PolicyNames.AllowSpecificOrigin);

        app.UseResponseCaching();

        app.Use(
            async (context, next) =>
            {
                context.Response.GetTypedHeaders().CacheControl =
                    new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromSeconds(10),
                    };
                context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] = new string[]
                {
                    "Accept-Encoding",
                };

                await next();
            }
        );

        app.UseAuthentication();
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

            endpoints
                .MapGet(
                    ServiceStatusResponse.DatabaseUnavailableDiagnosticPath,
                    context =>
                        ServiceStatusResponse.WriteDatabaseUnavailableDiagnosticAsync(
                            context,
                            context.RequestAborted
                        )
                )
                .AllowAnonymous();

            // Docker Desktop and browsers open the root URL: send them to Swagger
            // when it is enabled.
            if (IsSwaggerEnabled(env))
            {
                endpoints
                    .MapGet(
                        "/",
                        context =>
                        {
                            context.Response.Redirect(
                                $"{context.Request.PathBase}/swagger/index.html"
                            );
                            return Task.CompletedTask;
                        }
                    )
                    .AllowAnonymous();
            }

            // Standard MVC controller routing (if you have API controllers)
            endpoints.MapControllers();

            // ------------------------------------------------------------
            // Identity API group setup
            //
            // MapGroup("/identity") creates a route group. Anything you map
            // on this group will expose endpoints under the `/identity/*` path.
            //
            // WithTags("Identity API") shows this group as "Identity API" in Swagger
            // (if enabled).
            //
            // AllowAnonymous() means the built-in Identity endpoints such as
            // login/register/etc. do not require authorization.
            // ------------------------------------------------------------
            var identityGroup = endpoints
                .MapGroup("/api/identity")
                /* Use Identity + Cookies (recommended)
                 * Best for MVC + SPA on same domain
                 */
                // See: AccessTokenResponse class at Microsoft.AspNetCore.Authentication.BearerToken namespace
                .WithTags("Default Identity API Endpoints (using Cookies and opaque access tokens)")
                .AllowAnonymous();

            // ------------------------------------------------------------
            // This extension method wires up Microsoft's built-in Identity minimal APIs.
            //
            // It adds endpoints for:
            // - POST /identity/login
            // - POST /identity/register
            // - POST /identity/refresh
            // - POST /identity/password/forgot
            // - POST /identity/password/reset
            // ... plus 2FA and email confirmation flows depending on config.
            //
            // NOTE:
            // - You do NOT write controllers for these.
            // - They are generated at runtime via minimal APIs.
            // ------------------------------------------------------------
            identityGroup.MapIdentityApi<ApplicationUser>();

            // ------------------------------------------------------------
            // CUSTOM EXTENSION: Add your own profile endpoint
            //
            // Purpose:
            // - Return the authenticated user's profile information.
            //
            // Behavior:
            // - Requires a valid authenticated user RequireAuthorization()
            //   for selected endpoints.
            // - If no user is logged in, returns 401.
            // ------------------------------------------------------------
            identityGroup
                .MapGet(
                    "/profile",
                    async (UserManager<ApplicationUser> userManager, ClaimsPrincipal user) =>
                    {
                        // Resolve the ApplicationUser from ClaimsPrincipal
                        var appUser = await userManager.GetUserAsync(user);

                        if (appUser is null)
                            return Results.Unauthorized();

                        return Results.Ok(
                            new
                            {
                                appUser.Id,
                                appUser.UserName,
                                appUser.Email,
                            }
                        );
                    }
                )
                .RequireAuthorization(); // ensures only logged-in users can call /identity/profile

            // Fallback for unmatched endpoints
            endpoints.MapFallback(() => Results.NotFound("Endpoint not found."));
        });
    }
}
