using System.Collections.Generic;

namespace Mahjong.Core
{
    /// <summary>An undoable change to the board, with the score before and after it.</summary>
    internal abstract class Move
    {
        public ScoreCard Before { get; set; }
        public ScoreCard After { get; set; }

        public abstract void Apply(Board board);
        public abstract void Revert(Board board);
    }

    internal sealed class RemovePairMove : Move
    {
        private readonly Tile first;
        private readonly Tile second;

        // Where Connect's gravity slid the other tiles after the pair went (none otherwise).
        private IReadOnlyList<(Tile Tile, Position From, Position To)> settled = new List<(Tile, Position, Position)>();

        public RemovePairMove(Tile first, Tile second)
        {
            this.first = first;
            this.second = second;
        }

        public override void Apply(Board board)
        {
            board.Remove(first);
            board.Remove(second);
            settled = board.Settle();
        }

        public override void Revert(Board board)
        {
            board.Unsettle(settled);
            board.Add(first);
            board.Add(second);
        }
    }

    internal sealed class ShuffleMove : Move
    {
        private readonly IReadOnlyDictionary<Tile, TileFace> facesBefore;
        private readonly IReadOnlyDictionary<Tile, TileFace> facesAfter;

        public ShuffleMove(IReadOnlyDictionary<Tile, TileFace> facesBefore, IReadOnlyDictionary<Tile, TileFace> facesAfter)
        {
            this.facesBefore = facesBefore;
            this.facesAfter = facesAfter;
        }

        public override void Apply(Board board) => board.SetFaces(facesAfter);

        public override void Revert(Board board) => board.SetFaces(facesBefore);
    }
}
