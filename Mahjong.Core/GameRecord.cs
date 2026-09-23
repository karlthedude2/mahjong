using System.Collections.Generic;

namespace Mahjong.Core
{
    public enum RecordedMoveKind
    {
        Remove,
        Shuffle,
        Undo,
        Redo
    }

    /// <summary>One player action, with the game second it happened in.</summary>
    public sealed class RecordedMove
    {
        public RecordedMoveKind Kind { get; set; }

        /// <summary><see cref="GameClock.Seconds"/> when the move was made.</summary>
        public int Second { get; set; }

        /// <summary>For <see cref="RecordedMoveKind.Remove"/>: the two tiles' <see cref="Tile.Id"/>s.</summary>
        public int? TileA { get; set; }

        public int? TileB { get; set; }
    }

    /// <summary>
    /// Everything needed to replay a game exactly: the layout, the seed the board was dealt from,
    /// and every move. The server replays records to verify scores.
    /// </summary>
    public sealed class GameRecord
    {
        public string LayoutName { get; set; }
        public long Seed { get; set; }
        public List<RecordedMove> Moves { get; set; } = new List<RecordedMove>();
    }
}
