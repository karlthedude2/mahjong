using System.Linq;

namespace Mahjong.Core
{
    public sealed class ReplayResult
    {
        private ReplayResult(bool valid, string error, MahjongGame game)
        {
            Valid = valid;
            Error = error;
            Complete = valid && game.Board.IsComplete;
            Breakdown = game?.Scoring.Card.Breakdown;
            ElapsedSeconds = game?.Scoring.Clock.Seconds ?? 0;
        }

        /// <summary>False if any move was impossible or the record was malformed.</summary>
        public bool Valid { get; }

        public string Error { get; }

        /// <summary>True if the moves cleared the board.</summary>
        public bool Complete { get; }

        public int Score => Breakdown?.Total ?? 0;

        public ScoreBreakdown Breakdown { get; }

        public int ElapsedSeconds { get; }

        internal static ReplayResult Ok(MahjongGame game) => new ReplayResult(true, null, game);

        internal static ReplayResult Fail(string error, MahjongGame game = null) => new ReplayResult(false, error, game);
    }

    /// <summary>Replays a <see cref="GameRecord"/> move by move and recomputes the score.</summary>
    public static class GameReplayer
    {
        public static ReplayResult Replay(GameRecord record)
        {
            var layout = record?.LayoutName == null ? null : LayoutCatalog.Find(record.LayoutName);
            if (layout == null)
            {
                return ReplayResult.Fail($"Unknown layout \"{record?.LayoutName}\".");
            }

            var game = new MahjongGame(layout, record.Seed);

            foreach (var move in record.Moves ?? Enumerable.Empty<RecordedMove>())
            {
                if (move == null || move.Second < game.Scoring.Clock.Seconds)
                {
                    return ReplayResult.Fail("Moves are out of order.", game);
                }

                if (game.Board.IsComplete)
                {
                    return ReplayResult.Fail("Moves continue after the board was cleared.", game);
                }

                while (game.Scoring.Clock.Seconds < move.Second)
                {
                    game.Tick();
                }

                if (!Apply(game, move))
                {
                    return ReplayResult.Fail($"Move {move.Kind} at {move.Second}s is not allowed.", game);
                }
            }

            if (game.Board.IsComplete)
            {
                game.Finish();
            }

            return ReplayResult.Ok(game);
        }

        private static bool Apply(MahjongGame game, RecordedMove move)
        {
            switch (move.Kind)
            {
                case RecordedMoveKind.Remove:
                    var a = move.TileA.HasValue ? game.Board.FindTile(move.TileA.Value) : null;
                    var b = move.TileB.HasValue ? game.Board.FindTile(move.TileB.Value) : null;
                    return a != null && b != null && game.TryRemovePair(a, b);

                case RecordedMoveKind.Shuffle:
                    game.Shuffle();
                    return true;

                case RecordedMoveKind.Undo:
                    return game.Undo();

                case RecordedMoveKind.Redo:
                    return game.Redo();

                default:
                    return false;
            }
        }
    }
}
