using CleanArchitectureTemplate.Infrastructure.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CleanArchitectureTemplate.API.HealthChecks;

public sealed class DatabaseHealthCheck(ApplicationDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            bool canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

            return canConnect
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("The database could not be reached.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("The database health check failed.", exception);
        }
    }
}
