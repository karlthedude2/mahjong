using System.Diagnostics;
using Mahjong.Core;
using Mahjong.Web.Client.Api;

namespace Mahjong.Web.Client.Game;

/// <summary>
/// How a game is played: signed in (verified and ranked), as a guest (verified by the server, and
/// the win can be claimed after signing in), or offline (in the browser only).
/// </summary>
public enum GameMode
{
    Offline,
    Guest,
    SignedIn
}

public enum SessionState
{
    NotStarted,
    Playing,
    Paused,
    NoMovesLeft,
    Won
}

/// <summary>
/// One game in the browser: the board, the clock, the selected tile, and (for signed-in
/// players) the server round trips that start the game and verify the finished result.
/// </summary>
public sealed class GameSession(GameApi api, ITileEffects effects, GuestClaims guestClaims) : IDisposable
{
    private readonly Stopwatch playTime = new();
    private Timer? timer;
    private Guid? serverGameId;
    private string? guestToken;

    /// <summary>Raised whenever the UI should redraw. May fire from the clock timer.</summary>
    public event Action? Changed;

    public MahjongGame? Game { get; private set; }

    public Tile? Selected { get; private set; }

    public SessionState State { get; private set; } = SessionState.NotStarted;

    /// <summary>True if this game's score will be verified and can reach the leaderboard.</summary>
    public bool IsRanked => serverGameId.HasValue && guestToken == null;

    /// <summary>True for a guest's game the server is verifying, so a win can be saved by signing in.</summary>
    public bool IsGuestVerified => serverGameId.HasValue && guestToken != null;

    /// <summary>False until the player's first move: the clock doesn't run before then.</summary>
    public bool ClockStarted { get; private set; }

    public bool CanPause => ClockStarted && State is SessionState.Playing or SessionState.NoMovesLeft or SessionState.Paused;

    /// <summary>A problem to show the player (e.g. a pause that couldn't reach the server), or null.</summary>
    public string? Message { get; private set; }

    /// <summary>The leaderboard deal this game replays, or null for a normal game.</summary>
    public ReplayInfo? Replay { get; private set; }

    /// <summary>The server's result for a finished ranked game (null for guests or if it failed).</summary>
    public FinishGameResponse? Result { get; private set; }

    public bool SubmittingResult { get; private set; }

    /// <summary>
    /// Deals a new game: guaranteed winnable, or a random deal if <paramref name="randomDeal"/> is set.
    /// With a replay, it's that leaderboard game's deal (and kind of deal): a verified replay goes on
    /// the deal's replay list; an offline one deals the same tiles locally.
    /// </summary>
    public async Task StartAsync(LayoutDefinition layout, GameMode mode, ReplayInfo? replay = null, bool randomDeal = false)
    {
        StopClock();
        serverGameId = null;
        guestToken = null;
        Result = null;
        Selected = null;
        Message = null;
        Replay = replay;

        randomDeal = replay?.RandomDeal ?? randomDeal;
        long seed;
        if (mode != GameMode.Offline)
        {
            var started = await api.StartGameAsync(layout.Name, replay?.OriginalGameId, guest: mode == GameMode.Guest, randomDeal);
            serverGameId = started.GameId;
            guestToken = started.GuestToken;
            seed = started.Seed;
            randomDeal = started.RandomDeal;
        }
        else
        {
            seed = replay?.Seed ?? Random.Shared.NextInt64();
        }

        Game = new MahjongGame(layout, seed, winnable: !randomDeal);
        State = SessionState.Playing;
        ClockStarted = false;
        playTime.Reset();
        timer = new Timer(_ => OnTick(), null, TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(250));
        Changed?.Invoke();
    }

    public async Task ClickAsync(Tile tile)
    {
        if (Game == null || State is not (SessionState.Playing or SessionState.NoMovesLeft) || !Game.Board.IsFree(tile))
        {
            return;
        }

        // Feedback first, so the click sounds immediately (starting a ranked game's clock
        // waits for the server). Clicking the selected tile again just deselects it, silently.
        if (Selected != tile)
        {
            await effects.OnClickedAsync(tile);
        }

        if (!await StartClockAsync())
        {
            return;
        }

        CatchUpClock();

        if (Selected == null)
        {
            Selected = tile;
        }
        else if (Selected == tile)
        {
            Selected = null;
        }
        else if (Game.TryRemovePair(Selected, tile))
        {
            var first = Selected;
            Selected = null;
            await effects.OnPairRemovedAsync(first, tile);
            await UpdateStateAsync();
        }
        else
        {
            Selected = tile;
        }

        Changed?.Invoke();
    }

    public async Task ShuffleAsync() => await ChangeBoardAsync(game => { game.Shuffle(); return true; });

    public async Task UndoAsync() => await ChangeBoardAsync(game => game.Undo());

    public async Task RedoAsync() => await ChangeBoardAsync(game => game.Redo());

    /// <summary>
    /// Pauses or resumes. The board is hidden while paused and the clock stops. For ranked games
    /// the server records the pause too (it times pauses itself), so the verified clock still
    /// matches; if the server can't be reached, nothing changes and a message is shown.
    /// </summary>
    public async Task TogglePauseAsync()
    {
        if (!CanPause || Game == null)
        {
            return;
        }

        bool pausing = State != SessionState.Paused;
        if (pausing)
        {
            CatchUpClock();
        }

        if (serverGameId is { } id && !await api.SetPausedAsync(id, pausing, guestToken) && !GoOfflineIfGuest())
        {
            Message = pausing
                ? "Couldn't pause: the server didn't respond. Check your connection and try again."
                : "Couldn't resume: the server didn't respond. Check your connection and try again.";
            Changed?.Invoke();
            return;
        }

        Message = null;
        if (pausing)
        {
            playTime.Stop();
            Selected = null;
            State = SessionState.Paused;
        }
        else
        {
            playTime.Start();
            State = Game.Status == GameStatus.NoMovesLeft ? SessionState.NoMovesLeft : SessionState.Playing;
        }

        Changed?.Invoke();
    }

    /// <summary>Loads the tile effects' sounds ahead of the first click.</summary>
    public ValueTask PreloadEffectsAsync() => effects.PreloadAsync();

    public void Dispose() => StopClock();

    private async Task ChangeBoardAsync(Func<MahjongGame, bool> change)
    {
        if (Game == null || State is not (SessionState.Playing or SessionState.NoMovesLeft))
        {
            return;
        }

        if (!await StartClockAsync())
        {
            return;
        }

        Selected = null;
        CatchUpClock();
        if (change(Game))
        {
            await UpdateStateAsync();
        }

        Changed?.Invoke();
    }

    /// <summary>
    /// Starts the clock on the player's first move. A ranked game starts out paused on the server,
    /// so this also resumes it there; if the server can't be reached, the move is held back.
    /// </summary>
    private async Task<bool> StartClockAsync()
    {
        if (ClockStarted)
        {
            return true;
        }

        if (serverGameId is { } id && !await api.SetPausedAsync(id, paused: false, guestToken) && !GoOfflineIfGuest())
        {
            Message = "Couldn't start the game: the server didn't respond. Check your connection and try again.";
            Changed?.Invoke();
            return false;
        }

        Message = null;
        ClockStarted = true;
        playTime.Start();
        return true;
    }

    private async Task UpdateStateAsync()
    {
        switch (Game!.Status)
        {
            case GameStatus.Complete:
                Game.Finish();
                State = SessionState.Won;
                playTime.Stop();
                StopClock();
                await SubmitResultAsync();
                break;

            case GameStatus.NoMovesLeft:
                State = SessionState.NoMovesLeft;
                break;

            default:
                State = SessionState.Playing;
                break;
        }
    }

    private async Task SubmitResultAsync()
    {
        if (serverGameId is not { } id || Game?.Record == null)
        {
            return;
        }

        SubmittingResult = true;
        Changed?.Invoke();
        try
        {
            Result = await api.FinishGameAsync(id, new FinishGameRequest(Game.Record), guestToken);
            if (Result is { Won: true } && guestToken != null)
            {
                await guestClaims.RememberAsync(id, guestToken);
            }
        }
        catch (HttpRequestException)
        {
            Result = null;
        }
        finally
        {
            SubmittingResult = false;
        }
    }

    /// <summary>
    /// A guest's game carries on in the browser if the server stops answering (it just can't be
    /// saved any more); a signed-in game waits, so its score isn't lost. Returns true if it went offline.
    /// </summary>
    private bool GoOfflineIfGuest()
    {
        if (guestToken == null)
        {
            return false;
        }

        serverGameId = null;
        guestToken = null;
        return true;
    }

    private void OnTick()
    {
        if (CatchUpClock())
        {
            Changed?.Invoke();
        }
    }

    /// <summary>
    /// Brings the game clock up to the real time played. Counting real time (rather than timer
    /// ticks) keeps the clock right when the browser slows timers in background tabs.
    /// </summary>
    private bool CatchUpClock()
    {
        if (Game == null || State is not (SessionState.Playing or SessionState.NoMovesLeft))
        {
            return false;
        }

        int target = (int)playTime.Elapsed.TotalSeconds;
        bool advanced = false;
        while (Game.Scoring.Clock.Seconds < target)
        {
            Game.Tick();
            advanced = true;
        }

        return advanced;
    }

    private void StopClock()
    {
        timer?.Dispose();
        timer = null;
    }
}
