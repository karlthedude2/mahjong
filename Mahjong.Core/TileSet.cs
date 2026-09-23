using System;
using System.Collections.Generic;
using System.Linq;

namespace Mahjong.Core
{
    /// <summary>The faces available for dealing, expressed as one entry per matching pair.</summary>
    public sealed class TileSet
    {
        public TileSet(IEnumerable<TileFace> pairFaces)
        {
            PairFaces = pairFaces.ToList();
            if (PairFaces.Count == 0)
            {
                throw new ArgumentException("A tile set needs at least one pair.", nameof(pairFaces));
            }
        }

        public IReadOnlyList<TileFace> PairFaces { get; }

        /// <summary>
        /// The classic 144-tile set: suits, dragons and peacocks appear 4 times (2 pairs),
        /// winds, flowers and wizards appear twice (1 pair).
        /// </summary>
        public static TileSet Standard { get; } = CreateStandard();

        /// <summary>
        /// Returns exactly <paramref name="pairCount"/> pair faces in random order. Smaller layouts get
        /// a random subset of the set; larger layouts reuse the set as many times as needed.
        /// </summary>
        public List<TileFace> CreatePairs(int pairCount, Random random)
        {
            var result = new List<TileFace>(pairCount);
            while (result.Count < pairCount)
            {
                var batch = PairFaces.ToList();
                random.Shuffle(batch);
                result.AddRange(batch.Take(pairCount - result.Count));
            }

            random.Shuffle(result);
            return result;
        }

        private static TileSet CreateStandard()
        {
            var singles = new List<TileFace>();

            foreach (string suit in new[] { "bams", "dots", "crack" })
            {
                for (int n = suit == "bams" ? 2 : 1; n <= 9; n++)
                {
                    singles.Add(new TileFace(n + suit, 10));
                }
            }

            foreach (string name in new[] { "Bdragon", "Cdragon", "Fdragon", "peacock" })
            {
                singles.Add(new TileFace(name, 20));
            }

            var pairs = new List<TileFace>();
            pairs.AddRange(singles);
            pairs.AddRange(singles);

            foreach (string name in new[] { "North", "South", "East", "West",
                                             "1flower", "2flower", "3flower", "4flower",
                                             "1wizard", "2wizard", "3wizard", "4wizard" })
            {
                pairs.Add(new TileFace(name, 40));
            }

            return new TileSet(pairs);
        }
    }
}
