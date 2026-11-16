using System.Net.Mime;
using CleanArchitectureTemplate.ApplicationCore.Constants;
using CleanArchitectureTemplate.ApplicationCore.Interfaces;

namespace CleanArchitectureTemplate.Web.Middleware;

/// <summary>
/// Puts the site in maintenance mode when the <c>IsAppOnline</c> row of the
/// <c>AppSetting</c> table is <c>false</c>.
///
/// - Browser requests are redirected to <c>/out-of-service</c>.
/// - Non-GET, AJAX and JSON requests receive a 503 JSON body.
/// - SuperAdmin users, health checks and the out-of-service page always pass.
/// - Must run after <c>UseAuthentication</c> so the user's roles are known.
/// </summary>
public sealed class AppAvailabilityMiddleware
{
    public const string OutOfServicePath = "/out-of-service";

    private static readonly PathString[] AlwaysAvailablePaths =
    [
        new(OutOfServicePath),
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
        if (IsAlwaysAvailablePath(context.Request.Path) || IsSuperAdmin(context))
        {
            await _next(context);
            return;
        }

        string value;

        try
        {
            // Read the setting fresh so an outage state propagates immediately
            // between the Web and API hosts without relying on local caches.
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

        context.Response.Headers.CacheControl = "no-store, no-cache";
        context.Response.Headers.Pragma = "no-cache";
        context.Response.Headers.RetryAfter = ServiceStatusResponse.RetryAfterSeconds;

        if (ShouldReturnServiceUnavailable(context.Request))
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(
                new
                {
                    status = StatusCodes.Status503ServiceUnavailable,
                    title = "Service unavailable",
                    detail = "The application is temporarily unavailable due to maintenance.",
                }
            );
            return;
        }

        PathString redirectPath = context.Request.PathBase.Add(OutOfServicePath);
        context.Response.Redirect(redirectPath.Value!);
    }

    private static bool IsAlwaysAvailablePath(PathString path)
    {
        return AlwaysAvailablePaths.Any(availablePath => path.Equals(availablePath));
    }

    private static bool IsSuperAdmin(HttpContext context)
    {
        return context.User.IsInRole(RoleConstants.SuperAdministratorsRole);
    }

    private static bool ShouldReturnServiceUnavailable(HttpRequest request)
    {
        if (!HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method))
        {
            return true;
        }

        if (
            string.Equals(
                request.Headers.XRequestedWith,
                "XMLHttpRequest",
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return true;
        }

        return request
                .GetTypedHeaders()
                .Accept?.Any(header =>
                    header.MediaType.Value?.Equals(
                        MediaTypeNames.Application.Json,
                        StringComparison.OrdinalIgnoreCase
                    ) == true
                ) == true;
    }
}
