using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CleanArchitectureTemplate.API.Middleware;

/// <summary>
/// Writes JSON status responses (health checks and database outages) that do
/// not depend on the database.
/// </summary>
public static class ServiceStatusResponse
{
    // `dotnet new` replaces this value with the generated project name.
    public const string ApplicationName = "CleanArchitectureTemplate API";

    public const string LivePath = "/health/live";
    public const string ReadyPath = "/health/ready";
    public const string DatabaseUnavailableDiagnosticPath =
        "/health/database-unavailable-test";

    public const string RetryAfterSeconds = "300";

    private const string DatabaseUnavailableTitle = "Service temporarily unavailable";
    private const string DatabaseUnavailableMessage =
        "The API cannot connect to the database right now.";
    private const string DatabaseUnavailableDetail =
        "We are working to restore the service. Please try again later.";

    public static Task WriteDatabaseUnavailableAsync(
        HttpContext context,
        CancellationToken cancellationToken = default
    )
    {
        return WriteJsonAsync(
            context,
            new StatusBody(
                StatusCodes.Status503ServiceUnavailable,
                "HEALTH-004",
                "ACTUAL SQL CONNECTIVITY OUTAGE",
                DatabaseUnavailableTitle,
                DatabaseUnavailableMessage,
                DatabaseUnavailableDetail
            ),
            includeRetryAfter: true,
            cancellationToken
        );
    }

    public static Task WriteDatabaseUnavailableDiagnosticAsync(
        HttpContext context,
        CancellationToken cancellationToken = default
    )
    {
        return WriteJsonAsync(
            context,
            new StatusBody(
                StatusCodes.Status200OK,
                "HEALTH-003",
                "DATABASE-ABSENCE TEST",
                "Database-absence test",
                "The API is prepared to handle a missing database.",
                "This route does not query the database and confirms that the "
                    + "fallback response can be returned correctly."
            ),
            includeRetryAfter: false,
            cancellationToken
        );
    }

    public static Task WriteLiveHealthAsync(
        HttpContext context,
        CancellationToken cancellationToken = default
    )
    {
        return WriteJsonAsync(
            context,
            new StatusBody(
                StatusCodes.Status200OK,
                "HEALTH-001",
                "LIVE HEALTH",
                "API running",
                "The API service is up and can respond to requests.",
                "This check confirms that the API process is running. "
                    + "It does not check the database connection."
            ),
            includeRetryAfter: false,
            cancellationToken
        );
    }

    public static Task WriteReadyHealthAsync(
        HttpContext context,
        HealthStatus healthStatus,
        CancellationToken cancellationToken = default
    )
    {
        if (healthStatus == HealthStatus.Healthy)
        {
            return WriteJsonAsync(
                context,
                new StatusBody(
                    StatusCodes.Status200OK,
                    "HEALTH-002",
                    "READY AND HEALTHY",
                    "API ready",
                    "The API service and the database connection are available.",
                    "This check confirms that the API can serve requests that "
                        + "need the database."
                ),
                includeRetryAfter: false,
                cancellationToken
            );
        }

        return WriteJsonAsync(
            context,
            new StatusBody(
                StatusCodes.Status503ServiceUnavailable,
                "HEALTH-002",
                "READY BUT UNHEALTHY",
                DatabaseUnavailableTitle,
                DatabaseUnavailableMessage,
                "The API is running, but it is not ready to serve requests "
                    + "that need the database."
            ),
            includeRetryAfter: true,
            cancellationToken
        );
    }

    private static async Task WriteJsonAsync(
        HttpContext context,
        StatusBody body,
        bool includeRetryAfter,
        CancellationToken cancellationToken
    )
    {
        DateTimeOffset now = DateTimeOffset.Now;

        context.Response.Clear();
        context.Response.StatusCode = body.Status;
        context.Response.Headers.CacheControl = "no-store, no-cache";
        context.Response.Headers.Pragma = "no-cache";

        if (includeRetryAfter)
        {
            context.Response.Headers.RetryAfter = RetryAfterSeconds;
        }

        await context.Response.WriteAsJsonAsync(
            new
            {
                applicationName = ApplicationName,
                timestamp = now,
                timeZone = TimeZoneInfo.Local.Id,
                status = body.Status,
                displayCode = body.DisplayCode,
                headerMessage = body.HeaderMessage,
                title = body.Title,
                message = body.Message,
                detail = body.Detail,
            },
            cancellationToken
        );
    }

    private sealed record StatusBody(
        int Status,
        string DisplayCode,
        string HeaderMessage,
        string Title,
        string Message,
        string Detail
    );
}
