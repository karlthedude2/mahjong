using Mahjong.Web.Client.Api;
using Mahjong.Web.Client.Game;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

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

await builder.Build().RunAsync();
