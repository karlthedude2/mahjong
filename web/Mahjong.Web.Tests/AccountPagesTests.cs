using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;

namespace Mahjong.Web.Tests;

/// <summary>
/// The sign-in and sign-up pages in the settings' dark style: one compact column, the sign-in services
/// first (Google live when configured; Facebook and Apple shown, not yet available), then the form.
/// </summary>
public sealed partial class AccountPagesTests : IDisposable
{
    private readonly MahjongAppFactory app = new();

    public void Dispose() => app.Dispose();

    [Fact]
    public async Task SignInIsOneDarkColumnWithTheForm()
    {
        string html = await app.CreateClient().GetStringAsync("/Account/Login");

        Assert.Contains("class=\"auth-page\"", html);
        Assert.Matches("<h1[^>]*>Sign in</h1>", html);
        Assert.Contains("name=\"Input.Email\"", html);
        Assert.Contains("name=\"Input.Password\"", html);
        Assert.Matches("<button type=\"submit\" class=\"auth-primary\"[^>]*>Sign in</button>", html);
        Assert.Contains("href=\"Account/Register", html);
        Assert.Contains("href=\"Account/ForgotPassword\"", html);
    }

    [Fact]
    public async Task FacebookAndAppleAreShownButNotYetAvailable()
    {
        string html = await app.CreateClient().GetStringAsync("/Account/Login");

        Assert.Matches("<button type=\"button\" class=\"provider facebook\" disabled", html);
        Assert.Matches("<button type=\"button\" class=\"provider apple\" disabled", html);
    }

    [Fact]
    public async Task GoogleIsLiveWhenItsConfigured()
    {
        using var withGoogle = app.WithWebHostBuilder(b => b
            .UseSetting("Authentication:Google:ClientId", "test-id")
            .UseSetting("Authentication:Google:ClientSecret", "test-secret"));

        string html = await withGoogle.CreateClient().GetStringAsync("/Account/Login");

        Assert.Matches("<button type=\"submit\" class=\"provider google\" name=\"provider\" value=\"Google\"", html);
        Assert.Contains("action=\"Account/PerformExternalLogin\"", html);
    }

    [Fact]
    public async Task SignUpHasTheSameLook()
    {
        string html = await app.CreateClient().GetStringAsync("/Account/Register");

        Assert.Matches("class=\"auth-page[ \"]", html);
        Assert.Matches("<h1[^>]*>Create an account</h1>", html);
        Assert.Matches("<button type=\"button\" class=\"provider facebook\" disabled", html);
        foreach (var field in new[] { "Input.DisplayName", "Input.Email", "Input.Password", "Input.ConfirmPassword", "Input.Avatar" })
        {
            Assert.Contains($"name=\"{field}\"", html);
        }

        Assert.Matches("<button type=\"submit\" class=\"auth-primary\"[^>]*>Create account</button>", html);
        Assert.Contains("href=\"Account/Login", html);
    }
}
