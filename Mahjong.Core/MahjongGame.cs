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

        /// <summary>
        /// Deals from <paramref name="seed"/> with <see cref="DeterministicRandom"/> and records every
        /// move in <see cref="Record"/>, so the game can be replayed exactly (e.g. by the server).
        /// </summary>
        public MahjongGame(LayoutDefinition layout, long seed)
            : this(layout, TileSet.Standard, new DeterministicRandom(seed))
        {
            Record = new GameRecord { LayoutName = layout.Name, Seed = seed };
        }

        public LayoutDefinition Layout { get; }

        /// <summary>The moves so far, for games created from a seed; otherwise null.</summary>
        public GameRecord Record { get; }
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
            Push(move);
            RecordMove(RecordedMoveKind.Remove, first.Id, second.Id);
            return true;
        }

        /// <summary>Redeals the faces of the remaining tiles so the board can still be cleared.</summary>
        public void Shuffle()
        {
            var before = Scoring.Snapshot();
            var facesBefore = Board.Reshuffle(random);
            Scoring.Shuffled();
            Push(new ShuffleMove(facesBefore, Board.CurrentFaces()) { Before = before });
            RecordMove(RecordedMoveKind.Shuffle);
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
            RecordMove(RecordedMoveKind.Undo);
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
            RecordMove(RecordedMoveKind.Redo);
            return true;
        }

        /// <summary>Adds the end-of-game bonuses once <see cref="Status"/> is Complete. Safe to call more than once.</summary>
        public void Finish()
        {
            if (Board.IsComplete)
            {
                Scoring.ApplyFinalBonus();
            }
        }

        private void Push(Move move)
        {
            move.After = Scoring.Snapshot();
            undoStack.Push(move);
            redoStack.Clear();
        }

        private void RecordMove(RecordedMoveKind kind, int? tileA = null, int? tileB = null)
        {
            Record?.Moves.Add(new RecordedMove { Kind = kind, Second = Scoring.Clock.Seconds, TileA = tileA, TileB = tileB });
        }
    }
}
