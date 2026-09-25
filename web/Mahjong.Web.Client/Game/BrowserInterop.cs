using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Mahjong.Web.Client.Game;

/// <summary>The few things the game needs from the browser: sound, local storage and ads.</summary>
public sealed class BrowserInterop(IJSRuntime js) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> module =
        new(() => js.InvokeAsync<IJSObjectReference>("import", "./js/mahjong.js").AsTask());

    public async ValueTask PreloadSoundAsync(string url) => await (await module.Value).InvokeVoidAsync("preloadSound", url);

    public async ValueTask PlaySoundAsync(string url) => await (await module.Value).InvokeVoidAsync("playSound", url);

    public async ValueTask<string?> GetSettingAsync(string key) =>
        await (await module.Value).InvokeAsync<string?>("getSetting", key);

    public async ValueTask SetSettingAsync(string key, string value) =>
        await (await module.Value).InvokeVoidAsync("setSetting", key, value);

    public async ValueTask SetBackgroundAsync(string url) => await (await module.Value).InvokeVoidAsync("setBackground", url);

    /// <summary>Hands the header's New game and Settings clicks to the game page (null when the page goes away).</summary>
    public async ValueTask SetPlayActionsAsync(object? dotnetReference) =>
        await (await module.Value).InvokeVoidAsync("setPlayActions", dotnetReference);

    public async ValueTask PushAdAsync(ElementReference slot) => await (await module.Value).InvokeVoidAsync("pushAd", slot);

    public async ValueTask DisposeAsync()
    {
        if (module.IsValueCreated)
        {
            try
            {
                await (await module.Value).DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
            }
        }
    }
}
