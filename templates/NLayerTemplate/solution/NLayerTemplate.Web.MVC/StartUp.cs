using NLayerTemplate.Data.Datum;
using NLayerTemplate.Data.Model;
using NLayerTemplate.Repository;
using NLayerTemplate.Service.Services.ExampleServices;
using NLayerTemplate.Service.Services.ExampleServices.Interfaces;
using NLayerTemplate.Web.MVC.Services;
using NLayerTemplate.Web.MVC.Services.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;

namespace NLayerTemplate.Web.MVC;

public class StartUp
{
    public StartUp(IWebHostEnvironment env)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();

        if (env.IsDevelopment())
        {
            builder.AddUserSecrets<StartUp>();
        }

        Configuration = builder.Build();
    }

    public IConfiguration Configuration { get; set; }

    // === SERVICES
    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        #region Services Configuration

        // Connection string "DefaultConnection" is pulled from configuration (appsettings.json).
        var connectionString =
            Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found."
            );

        // Register DbContext with SQL Server as the database provider.
        services.AddDbContext<NLayerTemplateDbContext>(options =>
            options.UseSqlServer(connectionString)
        );

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
            .AddControllersWithViews()
            .AddViewLocalization()
            .AddDataAnnotationsLocalization()
            .AddXmlDataContractSerializerFormatters();

        // === Source code services ===
        services.AddScoped<IUnitOfWork, UnitOfWorkEF>();

        services.AddMapster();

        // Application services
        services.AddScoped<IContosoUniversityService, ContosoUniversityService>();
        services.AddScoped<IWeatherForecastService, WeatherForectastService>();

        #endregion
    }

    // === REQUEST PIPELINE
    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(
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
                DbInitializer.Initialize(serviceProvider).GetAwaiter().GetResult();

                logger.LogInformation("Database successfully initialized (Web MVC).");

                if (env.IsDevelopment())
                {
                    var server = app.ApplicationServices.GetRequiredService<IServer>();
                    var addresses = server.Features.Get<IServerAddressesFeature>();

                    if (addresses != null)
                    {
                        foreach (var address in addresses.Addresses)
                        {
                            logger.LogInformation("MVC application running at: {Address}", address);
                        }
                    }
                }
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
