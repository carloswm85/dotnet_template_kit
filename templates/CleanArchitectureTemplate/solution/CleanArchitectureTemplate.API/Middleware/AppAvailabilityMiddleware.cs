using CleanArchitectureTemplate.ApplicationCore.Constants;
using CleanArchitectureTemplate.ApplicationCore.Interfaces;

namespace CleanArchitectureTemplate.API.Middleware;

/// <summary>
/// Returns 503 JSON for every request, except health checks, when the
/// <c>IsAppOnline</c> row of the <c>AppSetting</c> table is <c>false</c>.
/// </summary>
public sealed class AppAvailabilityMiddleware
{
    private static readonly PathString[] AlwaysAvailablePaths =
    [
        new(ServiceStatusResponse.LivePath),
        new(ServiceStatusResponse.ReadyPath),
        new(ServiceStatusResponse.DatabaseUnavailableDiagnosticPath),
    ];

    private readonly RequestDelegate _next;
    private readonly ILogger<AppAvailabilityMiddleware> _logger;

    public AppAvailabilityMiddleware(
        RequestDelegate next,
        ILogger<AppAvailabilityMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAppSettingService appSettings)
    {
        if (AlwaysAvailablePaths.Any(path => context.Request.Path.Equals(path)))
        {
            await _next(context);
            return;
        }

        string value;

        try
        {
            value = appSettings.GetSingleUncached(AppSettingConstants.IsAppOnline).Value;
        }
        catch (Exception exception)
            when (!DatabaseUnavailableExceptionDetector.IsDatabaseUnavailable(exception))
        {
            // Database outages propagate to DatabaseUnavailableMiddleware.
            // Any other failure fails open so a bad setting cannot lock users out.
            _logger.LogError(
                exception,
                "The {SettingName} setting could not be read.",
                AppSettingConstants.IsAppOnline
            );

            await _next(context);
            return;
        }

        if (!bool.TryParse(value, out bool isAppOnline))
        {
            _logger.LogError(
                "The {SettingName} setting value is invalid: {SettingValue}",
                AppSettingConstants.IsAppOnline,
                value
            );

            await _next(context);
            return;
        }

        if (isAppOnline)
        {
            await _next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Response.Headers.CacheControl = "no-store, no-cache";
        context.Response.Headers.Pragma = "no-cache";
        context.Response.Headers.RetryAfter = ServiceStatusResponse.RetryAfterSeconds;

        await context.Response.WriteAsJsonAsync(
            new
            {
                status = StatusCodes.Status503ServiceUnavailable,
                title = "Service unavailable",
                detail = "The API is temporarily unavailable due to maintenance.",
            }
        );
    }
}
