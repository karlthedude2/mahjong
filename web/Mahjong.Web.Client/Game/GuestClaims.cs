using System.Text.Json;
using Mahjong.Web.Client.Api;

namespace Mahjong.Web.Client.Game;

/// <summary>
/// Remembers a guest's verified win in the browser so that, once they sign in (which leaves the
/// game page), the win can be saved to their account. The server holds such wins for a day.
/// </summary>
public sealed class GuestClaims(BrowserInterop browser, GameApi api)
{
    private const string Key = "pendingClaim";

    private sealed record PendingClaim(Guid GameId, string GuestToken);

    public async Task RememberAsync(Guid gameId, string guestToken) =>
        await browser.SetSettingAsync(Key, JsonSerializer.Serialize(new PendingClaim(gameId, guestToken)));

    /// <summary>
    /// Saves a remembered win to the signed-in player's account. Returns where it landed, or null
    /// if there was nothing to claim or the server turned it down (for example, it was too old).
    /// If the server can't be reached, the win is kept to try again next time.
    /// </summary>
    public async Task<ClaimGameResponse?> ClaimPendingAsync()
    {
        PendingClaim? pending;
        try
        {
            pending = await browser.GetSettingAsync(Key) is { Length: > 0 } json ? JsonSerializer.Deserialize<PendingClaim>(json) : null;
        }
        catch (JsonException)
        {
            pending = null;
        }

        if (pending == null)
        {
            return null;
        }

        try
        {
            var claimed = await api.ClaimGameAsync(pending.GameId, pending.GuestToken);
            await browser.SetSettingAsync(Key, "");
            return claimed;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}
