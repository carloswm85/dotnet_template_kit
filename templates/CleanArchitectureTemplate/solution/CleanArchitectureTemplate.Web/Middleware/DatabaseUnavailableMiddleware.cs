namespace CleanArchitectureTemplate.Web.Middleware;

/// <summary>
/// Converts SQL Server connectivity failures into a 503 response that does
/// not depend on the database. Other exceptions are rethrown unchanged.
/// </summary>
public sealed class DatabaseUnavailableMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DatabaseUnavailableMiddleware> _logger;

    public DatabaseUnavailableMiddleware(
        RequestDelegate next,
        ILogger<DatabaseUnavailableMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
            when (DatabaseUnavailableExceptionDetector.IsDatabaseUnavailable(exception))
        {
            _logger.LogError(
                exception,
                "The database is unavailable while processing {Method} {Path}.",
                context.Request.Method,
                context.Request.Path
            );

            if (context.Response.HasStarted)
            {
                _logger.LogCritical(
                    "The database-unavailable response could not be written "
                        + "because the response had already started."
                );
                throw;
            }

            await ServiceStatusResponse.WriteDatabaseUnavailableAsync(
                context,
                context.RequestAborted
            );
        }
    }
}
