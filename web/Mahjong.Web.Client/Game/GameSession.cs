using System.Diagnostics;
using Mahjong.Core;
using Mahjong.Web.Client.Api;

namespace Mahjong.Web.Client.Game;

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
public sealed class GameSession(GameApi api, ITileEffects effects) : IDisposable
{
    private readonly Stopwatch playTime = new();
    private Timer? timer;
    private Guid? serverGameId;

    /// <summary>Raised whenever the UI should redraw. May fire from the clock timer.</summary>
    public event Action? Changed;

    public MahjongGame? Game { get; private set; }

    public Tile? Selected { get; private set; }

    public SessionState State { get; private set; } = SessionState.NotStarted;

    /// <summary>
    /// True if this game's score will be verified and can reach the leaderboard. Ranked games
    /// can't be paused: the server checks the game clock against real time.
    /// </summary>
    public bool IsRanked => serverGameId.HasValue;

    public bool CanPause => !IsRanked && State is SessionState.Playing or SessionState.Paused;

    /// <summary>The server's result for a finished ranked game (null for guests or if it failed).</summary>
    public FinishGameResponse? Result { get; private set; }

    public bool SubmittingResult { get; private set; }

    public async Task StartAsync(LayoutDefinition layout, bool ranked)
    {
        StopClock();
        serverGameId = null;
        Result = null;
        Selected = null;

        long seed;
        if (ranked)
        {
            var started = await api.StartGameAsync(layout.Name);
            serverGameId = started.GameId;
            seed = started.Seed;
        }
        else
        {
            seed = Random.Shared.NextInt64();
        }

        Game = new MahjongGame(layout, seed);
        State = SessionState.Playing;
        playTime.Restart();
        timer = new Timer(_ => OnTick(), null, TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(250));
        Changed?.Invoke();
    }

    public async Task ClickAsync(Tile tile)
    {
        if (Game == null || State is not (SessionState.Playing or SessionState.NoMovesLeft) || !Game.Board.IsFree(tile))
        {
            return;
        }

        CatchUpClock();

        if (Selected == null)
        {
            Selected = tile;
            await effects.OnSelectedAsync(tile);
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
            await effects.OnSelectedAsync(tile);
        }

        Changed?.Invoke();
    }

    public async Task ShuffleAsync() => await ChangeBoardAsync(game => { game.Shuffle(); return true; });

    public async Task UndoAsync() => await ChangeBoardAsync(game => game.Undo());

    public async Task RedoAsync() => await ChangeBoardAsync(game => game.Redo());

    public void TogglePause()
    {
        if (!CanPause)
        {
            return;
        }

        if (State == SessionState.Playing)
        {
            CatchUpClock();
            playTime.Stop();
            State = SessionState.Paused;
        }
        else
        {
            playTime.Start();
            State = SessionState.Playing;
        }

        Changed?.Invoke();
    }

    public void Dispose() => StopClock();

    private async Task ChangeBoardAsync(Func<MahjongGame, bool> change)
    {
        if (Game == null || State is not (SessionState.Playing or SessionState.NoMovesLeft))
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
            Result = await api.FinishGameAsync(id, new FinishGameRequest(Game.Record));
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
