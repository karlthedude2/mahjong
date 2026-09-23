using System;
using System.Collections.Generic;

namespace Mahjong.Core
{
    public enum GameStatus
    {
        InProgress,
        NoMovesLeft,
        Complete
    }

    /// <summary>One game: the board, the score, and undo/redo. Knows nothing about drawing.</summary>
    public sealed class MahjongGame
    {
        private readonly Random random;
        private readonly Stack<Move> undoStack = new Stack<Move>();
        private readonly Stack<Move> redoStack = new Stack<Move>();

        public MahjongGame(LayoutDefinition layout, TileSet tileSet = null, Random random = null)
        {
            this.random = random ?? new Random();
            Layout = layout;
            Board = Board.Deal(layout, tileSet ?? TileSet.Standard, this.random);
        }

        public LayoutDefinition Layout { get; }
        public Board Board { get; }
        public Scoring Scoring { get; } = new Scoring();

        public bool CanUndo => undoStack.Count > 0;
        public bool CanRedo => redoStack.Count > 0;

        public GameStatus Status
        {
            get
            {
                if (Board.IsComplete)
                {
                    return GameStatus.Complete;
                }

                return Board.HasAvailableMove() ? GameStatus.InProgress : GameStatus.NoMovesLeft;
            }
        }

        public void Tick() => Scoring.Tick();

        /// <summary>Removes the two tiles if they match and are both free.</summary>
        public bool TryRemovePair(Tile first, Tile second)
        {
            if (!Board.CanRemove(first, second))
            {
                return false;
            }

            var move = new RemovePairMove(first, second) { Before = Scoring.Snapshot() };
            move.Apply(Board);
            Scoring.TileRemoved(first.Face);
            Scoring.TileRemoved(second.Face);
            Record(move);
            return true;
        }

        /// <summary>Redeals the faces of the remaining tiles so the board can still be cleared.</summary>
        public void Shuffle()
        {
            var before = Scoring.Snapshot();
            var facesBefore = Board.Reshuffle(random);
            Scoring.Shuffled();
            Record(new ShuffleMove(facesBefore, Board.CurrentFaces()) { Before = before });
        }

        public bool Undo()
        {
            if (undoStack.Count == 0)
            {
                return false;
            }

            var move = undoStack.Pop();
            move.Revert(Board);
            Scoring.Restore(move.Before);
            redoStack.Push(move);
            return true;
        }

        public bool Redo()
        {
            if (redoStack.Count == 0)
            {
                return false;
            }

            var move = redoStack.Pop();
            move.Apply(Board);
            Scoring.Restore(move.After);
            undoStack.Push(move);
            return true;
        }

        /// <summary>Adds the end-of-game bonuses. Call once, when <see cref="Status"/> is Complete.</summary>
        public void Finish() => Scoring.ApplyFinalBonus();

        private void Record(Move move)
        {
            move.After = Scoring.Snapshot();
            undoStack.Push(move);
            redoStack.Clear();
        }
    }
}
