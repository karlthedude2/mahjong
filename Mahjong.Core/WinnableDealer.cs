using System;
using System.Collections.Generic;
using System.Linq;

namespace Mahjong.Core
{
    public sealed class DealResult
    {
        internal DealResult(IReadOnlyDictionary<Position, TileFace> faces, IReadOnlyList<(Position First, Position Second)> solution)
        {
            Faces = faces;
            Solution = solution;
        }

        public IReadOnlyDictionary<Position, TileFace> Faces { get; }

        /// <summary>One order of pair removals that clears the board.</summary>
        public IReadOnlyList<(Position First, Position Second)> Solution { get; }
    }

    /// <summary>
    /// Deals boards that are guaranteed to be solvable. It plays the game on the empty layout first:
    /// it repeatedly removes two random free positions until nothing is left, then gives each
    /// removed pair the same face. Replaying that removal order always clears the board.
    /// </summary>
    public static class WinnableDealer
    {
        public const int MaxAttempts = 1000;

        public static DealResult Deal(IReadOnlyCollection<Position> positions, IList<TileFace> pairFaces, Random random)
        {
            if (positions.Count != pairFaces.Count * 2)
            {
                throw new ArgumentException($"{positions.Count} positions need {positions.Count / 2} pairs, got {pairFaces.Count}.");
            }

            if (!TryFindRemovalOrder(positions, random, out var order))
            {
                throw new InvalidOperationException(
                    $"Could not find a way to clear this layout after {MaxAttempts} attempts.");
            }

            var faces = new Dictionary<Position, TileFace>();
            for (int i = 0; i < order.Count; i++)
            {
                faces[order[i].First] = pairFaces[i];
                faces[order[i].Second] = pairFaces[i];
            }

            return new DealResult(faces, order);
        }

        public static bool TryFindRemovalOrder(IReadOnlyCollection<Position> positions, Random random, out List<(Position First, Position Second)> order)
        {
            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                if (TryRemoveAll(positions, random, out order))
                {
                    return true;
                }
            }

            order = null;
            return false;
        }

        private static bool TryRemoveAll(IReadOnlyCollection<Position> positions, Random random, out List<(Position First, Position Second)> order)
        {
            var occupied = new HashSet<Position>(positions);
            order = new List<(Position, Position)>(positions.Count / 2);

            while (occupied.Count > 0)
            {
                var free = occupied.Where(p => BoardRules.IsFree(p, occupied)).ToList();
                if (free.Count < 2)
                {
                    return false;
                }

                int first = random.Next(free.Count);
                int second = random.Next(free.Count - 1);
                if (second >= first)
                {
                    second++;
                }

                order.Add((free[first], free[second]));
                occupied.Remove(free[first]);
                occupied.Remove(free[second]);
            }

            return true;
        }
    }
}
