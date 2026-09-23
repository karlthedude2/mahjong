using System;
using System.Collections.Generic;
using System.Linq;

namespace Mahjong.Core
{
    /// <summary>The shape of a board: a name and the positions tiles go in.</summary>
    public sealed class LayoutDefinition
    {
        public LayoutDefinition(string name, IEnumerable<Position> positions, bool hidden = false)
        {
            Name = name;
            Positions = positions.ToList();
            Hidden = hidden;
        }

        public string Name { get; }
        public IReadOnlyList<Position> Positions { get; }

        /// <summary>Hidden layouts (e.g. for debugging) are left out of the layout menu.</summary>
        public bool Hidden { get; }

        /// <summary>Returns a description of every problem that would stop this layout from being dealt.</summary>
        public IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();

            if (Positions.Count == 0)
            {
                errors.Add("The layout has no tiles.");
            }

            if (Positions.Count % 2 != 0)
            {
                errors.Add($"The layout has {Positions.Count} tiles; it needs an even number.");
            }

            foreach (var duplicate in Positions.GroupBy(p => p).Where(g => g.Count() > 1))
            {
                errors.Add($"Position {duplicate.Key} is listed {duplicate.Count()} times.");
            }

            var distinct = Positions.Distinct().ToList();
            for (int i = 0; i < distinct.Count; i++)
            {
                for (int j = i + 1; j < distinct.Count; j++)
                {
                    if (BoardRules.Overlaps(distinct[i], distinct[j]))
                    {
                        errors.Add($"Tiles at {distinct[i]} and {distinct[j]} overlap.");
                    }
                }
            }

            if (errors.Count == 0 && !WinnableDealer.TryFindRemovalOrder(Positions, new Random(0), out _))
            {
                errors.Add("No way to clear this layout was found.");
            }

            return errors;
        }

        public override string ToString() => Name;
    }
}
