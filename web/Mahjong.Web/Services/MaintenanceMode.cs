namespace Mahjong.Web.Services;

/// <summary>
/// Shows a "back soon" page while a flag file exists. The deploy workflow creates the file before
/// it migrates the database and deletes it once the new version is healthy. On App Service the
/// file is /home/site/maintenance.on, which is shared storage that survives the deploy; elsewhere
/// set Site:MaintenanceFlag to a path. A flag older than <see cref="MaxAge"/> is ignored, so a
/// failed deploy can't leave the site stuck in maintenance.
/// </summary>
public sealed class MaintenanceMode(string? flagPath, TimeProvider time)
{
    public static readonly TimeSpan MaxAge = TimeSpan.FromMinutes(30);

    // The flag lives on network storage, so look at it at most every few seconds.
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(3);

    // What the maintenance page itself needs, plus the health check the deploy waits on.
    private static readonly string[] AlwaysAllowed = ["/healthz", "/logo-text.png", "/favicon.ico", "/favicon.png"];

    private DateTimeOffset nextCheck = DateTimeOffset.MinValue;
    private bool active;

    public static string? DefaultFlagPath(IConfiguration config) =>
        config["Site:MaintenanceFlag"] is { Length: > 0 } path ? path
        : Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") is { Length: > 0 } && Environment.GetEnvironmentVariable("HOME") is { Length: > 0 } home
            ? Path.Combine(home, "site", "maintenance.on")
            : null;

    public bool IsActive
    {
        get
        {
            if (flagPath is null)
            {
                return false;
            }

            var now = time.GetUtcNow();
            if (now >= nextCheck)
            {
                var file = new FileInfo(flagPath);
                active = file.Exists && now - file.LastWriteTimeUtc < MaxAge;
                nextCheck = now + CheckInterval;
            }

            return active;
        }
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var path = context.Request.Path;
        if (!IsActive || AlwaysAllowed.Any(allowed => path.Equals(allowed, StringComparison.OrdinalIgnoreCase)))
        {
            await next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Response.Headers.RetryAfter = "60";
        context.Response.Headers.CacheControl = "no-store";

        if (path.StartsWithSegments("/api"))
        {
            await context.Response.WriteAsync("The site is being updated. Please try again in a minute or two.");
            return;
        }

        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync(Page);
    }

    private const string Page = """
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1" />
            <meta http-equiv="refresh" content="30" />
            <title>Back soon - Mahjong Haus</title>
            <link rel="icon" href="/favicon.ico" sizes="16x16 32x32 48x48" />
            <style>
                body {
                    margin: 0;
                    min-height: 100vh;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    padding: 1rem;
                    box-sizing: border-box;
                    background: radial-gradient(circle at 50% 30%, #1f5a4a, #0e2a23);
                    color: #2b2116;
                    font-family: system-ui, -apple-system, "Segoe UI", Roboto, Arial, sans-serif;
                }
                main {
                    max-width: 28rem;
                    padding: 2rem 1.75rem;
                    border-radius: 1rem;
                    background: #fffaf0;
                    box-shadow: 0 1rem 3rem rgba(0, 0, 0, 0.4);
                    text-align: center;
                }
                img { display: block; width: 100%; max-width: 20rem; height: auto; margin: 0 auto 0.5rem; }
                h1 { margin: 0.5rem 0 0.75rem; font-size: 1.6rem; }
                p { margin: 0.5rem 0; line-height: 1.5; }
                .small { color: #6b5a44; font-size: 0.9rem; }
            </style>
        </head>
        <body>
            <main>
                <img src="/logo-text.png" alt="Mahjong Haus" width="684" height="98" />
                <h1>We'll be right back</h1>
                <p>Mahjong Haus is being updated. This usually takes a minute or two.</p>
                <p class="small">This page will reload by itself when the site is ready.</p>
            </main>
        </body>
        </html>
        """;
}
