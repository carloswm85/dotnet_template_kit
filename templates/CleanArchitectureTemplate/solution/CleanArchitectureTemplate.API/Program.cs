using CleanArchitectureTemplate.API;
using CleanArchitectureTemplate.ApplicationCore.Constants;
using CleanArchitectureTemplate.Infrastructure.Data;
using CleanArchitectureTemplate.Infrastructure.Model;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// Logging
// ------------------------------------------------------------
var startupLogger = LoggerFactory
    .Create(logging =>
    {
        logging.AddConsole();
        logging.AddDebug();
    })
    .CreateLogger("Startup");

startupLogger.LogInformation("API starting...");

// ------------------------------------------------------------
// Build app & middleware
// ------------------------------------------------------------

// Configure services
var startup = new Startup(builder.Configuration);

startup.ConfigureServices(builder.Services);

var app = builder.Build();

// Configure middleware pipeline
startup.ConfigurePipeline(app, app.Environment);

// ------------------------------------------------------------
// Database seeding (with optional migrations)
// ------------------------------------------------------------
// Skipped at EF Core design time (`dotnet ef`) and in production, where
// migrations are applied explicitly as part of the deployment.
if (EF.IsDesignTime || app.Environment.IsProduction())
{
    startupLogger.LogInformation(
        "Skipping automatic database migration and seeding (design time or production)."
    );
}
else
{
    using var scope = app.Services.CreateScope();
    var serviceProvider = scope.ServiceProvider;

    try
    {
        var db = serviceProvider.GetRequiredService<ApplicationDbContext>();

        // Set password with the Secret Manager tool.
        //  `dotnet user-secrets set "SeedUserPW" "!Abc123Antartica" --project .\solution\CleanArchitectureTemplate.API\`

        var testUserPw =
            app.Configuration.GetValue<string>("SeedUserPW") ?? ApplicationConstants.TestPassword;
        await DbInitializer.Initialize(serviceProvider, testUserPw);

        startupLogger.LogInformation("Database successfully initialized.");
    }
    catch (Exception ex)
    {
        startupLogger.LogError(ex, "Database initialization failed.");
    }
}

// ------------------------------------------------------------
app.Run();
