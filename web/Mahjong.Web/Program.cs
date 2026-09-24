using System.Security.Claims;
using System.Threading.RateLimiting;
using Azure.Communication.Email;
using Mahjong.Web.Api;
using Mahjong.Web.Components;
using Mahjong.Web.Components.Account;
using Mahjong.Web.Data;
using Mahjong.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();

// Sign-in: Identity cookies, plus each external provider whose keys are configured.
var authentication = builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    });
authentication.AddIdentityCookies();
AddExternalLogins(authentication, config);
builder.Services.AddAuthorization();

builder.Services.ConfigureApplicationCookie(options =>
{
    // The API answers 401/403 instead of redirecting to the login page.
    options.Events.OnRedirectToLogin = context => ApiAwareRedirect(context, StatusCodes.Status401Unauthorized);
    options.Events.OnRedirectToAccessDenied = context => ApiAwareRedirect(context, StatusCodes.Status403Forbidden);
});

var connectionString = config.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.User.RequireUniqueEmail = true;
        options.Stores.SchemaVersion = ApplicationDbContext.IdentitySchemaVersion;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Email: Azure Communication Services when configured, otherwise the log (development).
builder.Services.Configure<EmailOptions>(config.GetSection(EmailOptions.Section));
var emailConnection = config.GetSection(EmailOptions.Section).Get<EmailOptions>()?.ConnectionString;
if (string.IsNullOrEmpty(emailConnection))
{
    builder.Services.AddSingleton<IEmailSender<ApplicationUser>, LoggingEmailSender>();
}
else
{
    builder.Services.AddSingleton(new EmailClient(emailConnection));
    builder.Services.AddSingleton<IEmailSender<ApplicationUser>, AcsEmailSender>();
}

builder.Services.Configure<AdsOptions>(config.GetSection(AdsOptions.Section));
builder.Services.Configure<GameOptions>(config.GetSection(GameOptions.Section));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<GameService>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(ApiEndpoints.GamesRateLimit, context => RateLimitPartition.GetFixedWindowLimiter(
        context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 60, Window = TimeSpan.FromMinutes(1) }));
});

builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();

    // Keep the local database current; production is migrated by the deploy workflow.
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// Friendly error pages for the site; the API keeps its plain status codes.
app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/api"),
    site => site.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true));
app.UseHttpsRedirection();
RedirectToCanonicalHost(app, config["Site:CanonicalHost"]);
app.UseRateLimiter();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Mahjong.Web.Client._Imports).Assembly);

app.MapAdditionalIdentityEndpoints();
app.MapMahjongApi();
app.MapHealthChecks("/healthz");

app.Run();

// Sends visitors on any other address (e.g. the azurewebsites.net one) to the site's main domain,
// keeping the path. The health check is left alone so monitoring and the deploy smoke test work.
static void RedirectToCanonicalHost(WebApplication app, string? canonicalHost)
{
    if (string.IsNullOrWhiteSpace(canonicalHost))
    {
        return;
    }

    app.Use(async (context, next) =>
    {
        var request = context.Request;
        if (!request.Host.Host.Equals(canonicalHost, StringComparison.OrdinalIgnoreCase)
            && !request.Path.StartsWithSegments("/healthz"))
        {
            context.Response.Redirect($"https://{canonicalHost}{request.PathBase}{request.Path}{request.QueryString}", permanent: true);
            return;
        }

        await next();
    });
}

static void AddExternalLogins(AuthenticationBuilder authentication, IConfiguration config)
{
    // Keys live in configuration (user secrets locally, App Service settings in Azure).
    if (config["Authentication:Google:ClientId"] is { Length: > 0 } googleId)
    {
        authentication.AddGoogle(o =>
        {
            o.ClientId = googleId;
            o.ClientSecret = config["Authentication:Google:ClientSecret"]!;
        });
    }

    if (config["Authentication:Microsoft:ClientId"] is { Length: > 0 } microsoftId)
    {
        authentication.AddMicrosoftAccount(o =>
        {
            o.ClientId = microsoftId;
            o.ClientSecret = config["Authentication:Microsoft:ClientSecret"]!;
        });
    }

    if (config["Authentication:Facebook:AppId"] is { Length: > 0 } facebookId)
    {
        authentication.AddFacebook(o =>
        {
            o.AppId = facebookId;
            o.AppSecret = config["Authentication:Facebook:AppSecret"]!;
        });
    }
}

static Task ApiAwareRedirect(Microsoft.AspNetCore.Authentication.RedirectContext<Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationOptions> context, int apiStatus)
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.StatusCode = apiStatus;
    }
    else
    {
        context.Response.Redirect(context.RedirectUri);
    }

    return Task.CompletedTask;
}

// Lets the integration tests host the app with WebApplicationFactory<Program>.
public partial class Program;
