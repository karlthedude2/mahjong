using System.Globalization;
using Mahjong.Web.Client;
using Mahjong.Web.Client.Api;
using Mahjong.Web.Client.Game;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

// Same-origin API calls; the Identity cookie authenticates signed-in players.
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<GameApi>();
builder.Services.AddScoped<BrowserInterop>();
builder.Services.AddScoped<PlayerPreferences>();
builder.Services.AddScoped<ITileEffects, SoundTileEffects>();
builder.Services.AddScoped<GuestClaims>();
builder.Services.AddScoped<GameSession>();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Speak the page's language: the server chose it (the player's choice, else the browser's) and
// wrote it into <html lang>. Set before the app starts, so it also loads the Spanish text.
var host = builder.Build();
string? language = await host.Services.GetRequiredService<IJSRuntime>().InvokeAsync<string?>("mahjongLanguage");
var culture = CultureInfo.GetCultureInfo(Strings.Cultures.Contains(language) ? language! : "en");
CultureInfo.DefaultThreadCurrentCulture = Strings.FormattingCulture(culture);
CultureInfo.DefaultThreadCurrentUICulture = culture;

await host.RunAsync();
