using System.Collections;
using System.Globalization;
using System.Net;
using System.Resources;
using System.Text.RegularExpressions;
using Mahjong.Core;
using Mahjong.Web.Client;
using Mahjong.Web.Client.Game;
using Mahjong.Web.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Localization;

namespace Mahjong.Web.Tests;

/// <summary>Every text the site shows has a Spanish translation, and the language switch works.</summary>
public sealed partial class SpanishTests : IDisposable
{
    private static readonly HashSet<string> Spanish = LoadSpanish();

    private readonly MahjongAppFactory app = new();

    public void Dispose() => app.Dispose();

    private static HashSet<string> LoadSpanish()
    {
        var resources = new ResourceManager("Mahjong.Web.Client.Resources.Strings", typeof(Strings).Assembly);
        var set = resources.GetResourceSet(CultureInfo.GetCultureInfo("es"), createIfNotExists: true, tryParents: false)
            ?? throw new InvalidOperationException("Strings.es.resx isn't in the client's satellite assembly.");
        return set.Cast<DictionaryEntry>().Select(e => (string)e.Key).ToHashSet();
    }

    private static void AssertTranslated(IEnumerable<string> keys)
    {
        var missing = keys.Where(k => k.Length > 0 && !Spanish.Contains(k)).Distinct().Order().ToList();
        Assert.True(missing.Count == 0, "Missing from Strings.es.resx:\n" + string.Join("\n", missing));
    }

    // L["..."], L.Html("..."), [Display(Name = "...")] and validation ErrorMessage = "..." in the site's code.
    [GeneratedRegex("""(?:\bL(?:\.Html\(|\[)|Display\(Name = |ErrorMessage = )\s*"((?:[^"\\]|\\.)*)"(?!\s*\+)""")]
    private static partial Regex KeyPattern();

    [Fact]
    public void EveryTextInTheCodeHasASpanishTranslation()
    {
        string web = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        var sources = new[] { "Mahjong.Web", "Mahjong.Web.Client" }
            .SelectMany(p => Directory.EnumerateFiles(Path.Combine(web, p), "*.*", SearchOption.AllDirectories))
            .Where(f => (f.EndsWith(".razor") || f.EndsWith(".cs")) && !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .ToList();
        Assert.NotEmpty(sources);

        var keys = sources
            .SelectMany(f => KeyPattern().Matches(File.ReadAllText(f)))
            .Select(m => Regex.Unescape(m.Groups[1].Value))
            .ToList();

        Assert.True(keys.Count > 300, $"Only found {keys.Count} texts; has the pattern stopped matching?");
        AssertTranslated(keys);
    }

    [Fact]
    public void NamesChosenInTheGameHaveSpanishTranslations()
    {
        var recorder = new RecordingLocalizer();
        foreach (var layout in LayoutCatalog.Visible.Concat(ConnectLayouts.All))
        {
            recorder.LayoutName(layout);
        }

        foreach (var face in TileSet.Standard.PairFaces)
        {
            _ = recorder[TileSets.Glyph(face.Name).Caption];
        }

        var names = Backgrounds.All.Select(b => b.Name)
            .Concat(TileSets.All.Select(s => s.Name))
            .Concat(Avatars.All.Select(a => a.Name));

        AssertTranslated(recorder.Keys.Concat(names));
    }

    [Fact]
    public void AccountMessagesHaveSpanishTranslations()
    {
        var recorder = new RecordingLocalizer();
        var errors = new LocalizedIdentityErrors(recorder);
        foreach (var method in typeof(LocalizedIdentityErrors).GetMethods().Where(m => m.DeclaringType == typeof(LocalizedIdentityErrors)))
        {
            method.Invoke(errors, method.GetParameters().Select(p => p.ParameterType == typeof(int) ? (object)6 : "x").ToArray());
        }

        var nameMessages = new[] { "", new string('a', 25), "a\u0007", "!!!", "admin", "shithead" }
            .Select(n => DisplayNames.Check(n, out _))
            .ToList();
        Assert.All(nameMessages, Assert.NotNull);

        AssertTranslated(recorder.Keys.Concat(nameMessages!)
            .Concat(["The {0} field is required.", "The {0} field is not a valid email address.", "The {0} must be at most {1} characters long.", "The {0} field doesn't match."]));
    }

    [Fact]
    public async Task TheLanguageLinkSwitchesTheSiteToSpanish()
    {
        var client = app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/language/es?returnUrl=%2Fleaderboards");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/leaderboards", response.Headers.Location?.OriginalString);
        string cookie = response.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith(".AspNetCore.Culture="));

        var page = new HttpRequestMessage(HttpMethod.Get, "/leaderboards");
        page.Headers.Add("Cookie", cookie.Split(';')[0]);
        string html = await (await client.SendAsync(page)).Content.ReadAsStringAsync();

        Assert.Contains("<html lang=\"es\"", html);
        Assert.Contains("Clasificaciones", html);
    }

    [Fact]
    public async Task SpanishBrowsersGetSpanishAndOthersGetEnglish()
    {
        var client = app.CreateClient();

        var spanish = new HttpRequestMessage(HttpMethod.Get, "/privacy");
        spanish.Headers.Add("Accept-Language", "es-MX,es;q=0.9");
        Assert.Contains("Política de privacidad", await (await client.SendAsync(spanish)).Content.ReadAsStringAsync());

        var german = new HttpRequestMessage(HttpMethod.Get, "/privacy");
        german.Headers.Add("Accept-Language", "de-DE");
        Assert.Contains("Privacy policy", await (await client.SendAsync(german)).Content.ReadAsStringAsync());
    }

    [Fact]
    public void SpanishWritesDecimalsWithAPoint()
    {
        // Blazor writes numbers into SVG and style attributes in the current culture: "86,92" would
        // be read as two numbers.
        var formatting = Strings.FormattingCulture(CultureInfo.GetCultureInfo("es"));

        Assert.Equal("86.92", 86.92.ToString(formatting));
        Assert.Equal("septiembre", new DateTime(2026, 9, 28).ToString("MMMM", formatting));
    }

    [Theory]
    [InlineData("https://evil.example/")]
    [InlineData("//evil.example/")]
    [InlineData("/\\evil.example/")]
    public async Task TheLanguageLinkOnlyReturnsToThisSite(string returnUrl)
    {
        var client = app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/language/es?returnUrl=" + Uri.EscapeDataString(returnUrl));

        Assert.Equal("/", response.Headers.Location?.OriginalString);
    }

    /// <summary>Returns each key as its own text, remembering the keys it was asked for.</summary>
    private sealed class RecordingLocalizer : IStringLocalizer<Strings>
    {
        public List<string> Keys { get; } = [];

        public LocalizedString this[string name]
        {
            get
            {
                Keys.Add(name);
                return new LocalizedString(name, name);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                Keys.Add(name);
                return new LocalizedString(name, string.Format(name, arguments));
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }
}
