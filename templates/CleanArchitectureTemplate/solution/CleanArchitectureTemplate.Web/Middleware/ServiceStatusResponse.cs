using System.Globalization;
using System.Net;
using System.Text;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CleanArchitectureTemplate.Web.Middleware;

/// <summary>
/// Writes self-contained HTML status pages (health checks and database
/// outages). The pages use no layout, static files or database access, so
/// they render even when the rest of the application cannot.
/// </summary>
public static class ServiceStatusResponse
{
    // `dotnet new` replaces this value with the generated project name.
    public const string ApplicationName = "CleanArchitectureTemplate";

    public const string LivePath = "/health/live";
    public const string ReadyPath = "/health/ready";
    public const string DatabaseUnavailableDiagnosticPath =
        "/health/database-unavailable-test";

    public const string RetryAfterSeconds = "300";

    private const string DatabaseUnavailableTitle = "Service temporarily unavailable";
    private const string DatabaseUnavailableMessage =
        "The application cannot connect to the database right now.";
    private const string DatabaseUnavailableDetail =
        "We are working to restore the service. Please try again later.";

    public static Task WriteDatabaseUnavailableAsync(
        HttpContext context,
        CancellationToken cancellationToken = default
    )
    {
        return WritePageAsync(
            context,
            new StatusPage(
                StatusCodes.Status503ServiceUnavailable,
                "HEALTH-004",
                DatabaseUnavailableTitle,
                DatabaseUnavailableMessage,
                DatabaseUnavailableDetail,
                "#dc3545",
                "#ffffff",
                "!",
                "ACTUAL SQL CONNECTIVITY OUTAGE",
                IncludeRetryAfter: true
            ),
            cancellationToken
        );
    }

    public static Task WriteDatabaseUnavailableDiagnosticAsync(
        HttpContext context,
        CancellationToken cancellationToken = default
    )
    {
        return WritePageAsync(
            context,
            new StatusPage(
                StatusCodes.Status200OK,
                "HEALTH-003",
                "Database-absence test",
                "The application is prepared to handle a missing database.",
                "This page does not query the database and confirms that the "
                    + "fallback response can be displayed correctly.",
                "#6f42c1",
                "#ffffff",
                "T",
                "DATABASE-ABSENCE TEST",
                IncludeRetryAfter: false
            ),
            cancellationToken
        );
    }

    public static Task WriteLiveHealthAsync(
        HttpContext context,
        CancellationToken cancellationToken = default
    )
    {
        return WritePageAsync(
            context,
            new StatusPage(
                StatusCodes.Status200OK,
                "HEALTH-001",
                "Application running",
                "The web service is up and can respond to requests.",
                "This check confirms that the application process is running. "
                    + "It does not check the database connection.",
                "#0d6efd",
                "#ffffff",
                "i",
                "LIVE HEALTH",
                IncludeRetryAfter: false
            ),
            cancellationToken
        );
    }

    public static Task WriteReadyHealthAsync(
        HttpContext context,
        HealthStatus healthStatus,
        CancellationToken cancellationToken = default
    )
    {
        StatusPage page =
            healthStatus == HealthStatus.Healthy
                ? new StatusPage(
                    StatusCodes.Status200OK,
                    "HEALTH-002",
                    "Application ready",
                    "The web service and the database connection are available.",
                    "This check confirms that the application can serve "
                        + "requests that need the database.",
                    "#198754",
                    "#ffffff",
                    "✓",
                    "READY AND HEALTHY",
                    IncludeRetryAfter: false
                )
                : new StatusPage(
                    StatusCodes.Status503ServiceUnavailable,
                    "HEALTH-002",
                    DatabaseUnavailableTitle,
                    DatabaseUnavailableMessage,
                    "The application is running, but it is not ready to serve "
                        + "requests that need the database.",
                    "#fd7e14",
                    "#111111",
                    "!",
                    "READY BUT UNHEALTHY",
                    IncludeRetryAfter: true
                );

        return WritePageAsync(context, page, cancellationToken);
    }

    private static async Task WritePageAsync(
        HttpContext context,
        StatusPage page,
        CancellationToken cancellationToken
    )
    {
        context.Response.Clear();
        context.Response.StatusCode = page.HttpStatusCode;
        context.Response.ContentType = "text/html; charset=utf-8";
        context.Response.Headers.CacheControl = "no-store, no-cache";
        context.Response.Headers.Pragma = "no-cache";

        if (page.IncludeRetryAfter)
        {
            context.Response.Headers.RetryAfter = RetryAfterSeconds;
        }

        await context.Response.WriteAsync(CreateHtml(page), Encoding.UTF8, cancellationToken);
    }

    private static string CreateHtml(StatusPage page)
    {
        DateTimeOffset now = DateTimeOffset.Now;
        string localizedDateTime = now.ToString(
            "dddd, MMMM d, yyyy, HH:mm:ss",
            CultureInfo.GetCultureInfo("en-US")
        );
        string timeZone = WebUtility.HtmlEncode(TimeZoneInfo.Local.DisplayName);

        return $$"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1">
                <meta name="robots" content="noindex, nofollow">
                <title>{{page.Title}} - {{ApplicationName}}</title>
                <style>
                    :root { color-scheme: light dark; font-family: system-ui, -apple-system, "Segoe UI", sans-serif; }
                    * { box-sizing: border-box; }
                    body { align-items: center; background: Canvas; color: CanvasText; display: flex; flex-direction: column; gap: 1.5rem; justify-content: center; margin: 0; min-height: 100vh; padding: 1.5rem; }
                    .application-context { background: color-mix(in srgb, CanvasText 4%, transparent); border: 1px solid color-mix(in srgb, CanvasText 24%, transparent); border-radius: 0.75rem; max-width: 42rem; padding: 1.25rem 1.5rem; text-align: center; width: 100%; }
                    .application-name { font-size: clamp(1.5rem, 4vw, 2rem); font-weight: 700; line-height: 1.25; margin: 0 0 0.65rem; }
                    .application-datetime { color: color-mix(in srgb, CanvasText 72%, transparent); font-size: 0.95rem; line-height: 1.5; margin: 0; }
                    main { border: 1px solid color-mix(in srgb, CanvasText 18%, transparent); border-radius: 1rem; box-shadow: 0 1rem 2.5rem color-mix(in srgb, CanvasText 14%, transparent); max-width: 42rem; overflow: hidden; text-align: center; width: 100%; }
                    header { align-items: center; background: {{page.AccentColor}}; color: {{page.AccentTextColor}}; display: flex; flex-direction: column; gap: 0.35rem; padding: 1.25rem 1.5rem; }
                    .header-icon { font-size: 2.75rem; font-weight: 700; line-height: 1; }
                    .header-message { font-size: 1rem; font-weight: 700; letter-spacing: 0.04em; line-height: 1.4; }
                    section { padding: 2rem; }
                    h1 { font-size: clamp(1.75rem, 5vw, 2.5rem); margin: 0 0 1rem; }
                    p { font-size: 1.1rem; line-height: 1.6; margin: 0.75rem 0; }
                    .status-code { background: color-mix(in srgb, {{page.AccentColor}} 18%, transparent); border: 1px solid color-mix(in srgb, {{page.AccentColor}} 55%, transparent); border-radius: 999px; display: inline-block; font-family: ui-monospace, "Cascadia Code", monospace; font-size: 0.85rem; font-weight: 700; letter-spacing: 0.08em; margin: 0 0 1rem; padding: 0.35rem 0.75rem; }
                    .detail { color: color-mix(in srgb, CanvasText 72%, transparent); }
                </style>
            </head>
            <body>
                <section class="application-context" aria-label="Application information">
                    <p class="application-name">{{ApplicationName}}</p>
                    <p class="application-datetime">
                        Current date and time:
                        <time datetime="{{now:O}}">{{localizedDateTime}}</time>
                        ({{timeZone}})
                    </p>
                </section>
                <main role="status" aria-live="polite">
                    <header>
                        <span class="header-icon" aria-hidden="true">{{page.Icon}}</span>
                        <span class="header-message">{{page.HeaderMessage}}</span>
                    </header>
                    <section>
                        <p class="status-code">{{page.DisplayCode}}</p>
                        <h1>{{page.Title}}</h1>
                        <p>{{page.Message}}</p>
                        <p class="detail">{{page.Detail}}</p>
                    </section>
                </main>
            </body>
            </html>
            """;
    }

    private sealed record StatusPage(
        int HttpStatusCode,
        string DisplayCode,
        string Title,
        string Message,
        string Detail,
        string AccentColor,
        string AccentTextColor,
        string Icon,
        string HeaderMessage,
        bool IncludeRetryAfter
    );
}
