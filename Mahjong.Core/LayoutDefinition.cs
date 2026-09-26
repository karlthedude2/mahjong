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
            PreviewName = name;
        }

        /// <summary>A Connect board: a full grid of <paramref name="columns"/> by <paramref name="rows"/> cells.</summary>
        internal LayoutDefinition(string name, IEnumerable<Position> positions, GameKind kind, int columns, int rows, Gravity gravity, string previewName)
            : this(name, positions)
        {
            Kind = kind;
            Columns = columns;
            Rows = rows;
            Gravity = gravity;
            PreviewName = previewName;
        }

        public string Name { get; }
        public IReadOnlyList<Position> Positions { get; }

        /// <summary>Hidden layouts (e.g. for debugging) are left out of the layout menu.</summary>
        public bool Hidden { get; }

        /// <summary>Which game the layout is for; .layout files are solitaire layouts.</summary>
        public GameKind Kind { get; } = GameKind.Solitaire;

        /// <summary>For Connect: the grid's size in cells.</summary>
        public int Columns { get; }

        public int Rows { get; }

        /// <summary>For Connect: whether (and which way) tiles slide to close gaps.</summary>
        public Gravity Gravity { get; }

        /// <summary>
        /// The name the layout's picture goes by. Connect boards share one picture per size, whatever
        /// the gravity ("Connect Small" for "Connect Small (Gravity Down)").
        /// </summary>
        public string PreviewName { get; }

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
