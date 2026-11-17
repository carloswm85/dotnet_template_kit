using CleanArchitectureTemplate.ApplicationCore.Constants;
using CleanArchitectureTemplate.Infrastructure.Data;

namespace CleanArchitectureTemplate.Web.Services;

/// <summary>
/// Applies migrations and seeds sample data when the host starts.
///
/// - Awaited before the web server accepts requests, so the database is ready
///   for the first request and failures are logged.
/// - Skipped in Production, where migrations run as a deployment step.
/// - Not started by `dotnet ef`, which builds the host without starting it.
/// </summary>
public sealed class DatabaseInitializerHostedService(
    IServiceProvider serviceProvider,
    IHostEnvironment environment,
    IConfiguration configuration,
    ILogger<DatabaseInitializerHostedService> logger
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (environment.IsProduction())
        {
            logger.LogInformation(
                "Skipping automatic database migration and seeding in production."
            );
            return;
        }

        using var scope = serviceProvider.CreateScope();

        try
        {
            // Set the password with the Secret Manager tool:
            // dotnet user-secrets set "SeedUserPW" "<password>" --project <Web project>
            var testUserPw =
                configuration.GetValue<string>("SeedUserPW") ?? ApplicationConstants.TestPassword;

            await DbInitializer.Initialize(scope.ServiceProvider, testUserPw);

            logger.LogDebug("Database initialized from the Web host.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}