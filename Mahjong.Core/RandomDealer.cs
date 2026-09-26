using System;
using System.Collections.Generic;
using System.Linq;

namespace Mahjong.Core
{
    /// <summary>
    /// Deals tiles in a purely random order, like shuffling a real set. Unlike
    /// <see cref="WinnableDealer"/>, the board may have no way to be cleared, so there's no solution.
    /// </summary>
    public static class RandomDealer
    {
        public static DealResult Deal(IReadOnlyCollection<Position> positions, IList<TileFace> pairFaces, Random random)
        {
            if (positions.Count != pairFaces.Count * 2)
            {
                throw new ArgumentException($"{positions.Count} positions need {positions.Count / 2} pairs, got {pairFaces.Count}.");
            }

            // Each pair's face twice, shuffled across the positions.
            var faces = pairFaces.SelectMany(face => new[] { face, face }).ToList();
            random.Shuffle(faces);

            var dealt = new Dictionary<Position, TileFace>();
            int i = 0;
            foreach (var position in positions)
            {
                dealt[position] = faces[i++];
            }

            return new DealResult(dealt, Array.Empty<(Position, Position)>());
        }
    }
}
