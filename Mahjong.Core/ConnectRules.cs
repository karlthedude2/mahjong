using System;
using System.Collections.Generic;
using System.Linq;

namespace Mahjong.Core
{
    /// <summary>Which game a layout is for.</summary>
    public enum GameKind
    {
        /// <summary>Mahjong solitaire: stacked tiles; free matching tiles are removed.</summary>
        Solitaire,

        /// <summary>
        /// Connect (Shisen-Sho): a flat grid; matching tiles are removed if a line with at most two
        /// turns joins them through empty cells (it may run around the outside of the board).
        /// </summary>
        Connect
    }

    /// <summary>Connect variants where the remaining tiles slide to close the gaps.</summary>
    public enum Gravity
    {
        None,
        Down,
        Left
    }

    /// <summary>A grid cell for Connect: column and row, where -1 and Columns/Rows are just outside the board.</summary>
    public readonly struct Cell : IEquatable<Cell>
    {
        public Cell(int column, int row)
        {
            Column = column;
            Row = row;
        }

        public int Column { get; }
        public int Row { get; }

        public static Cell Of(Position p) => new Cell(p.X / 2, p.Y / 2);

        public Position ToPosition() => new Position(Column * 2, Row * 2, 0);

        public bool Equals(Cell other) => Column == other.Column && Row == other.Row;

        public override bool Equals(object obj) => obj is Cell other && Equals(other);

        public override int GetHashCode() => unchecked(Column * 397 ^ Row);

        public override string ToString() => $"({Column}, {Row})";
    }

    /// <summary>The Connect boards: three sizes, each with and without gravity.</summary>
    public static class ConnectLayouts
    {
        public const string Prefix = "Connect";

        private static readonly (string Size, int Columns, int Rows)[] Sizes =
        {
            ("Small", 12, 6),
            ("Medium", 14, 8),
            ("Classic", 18, 8),
        };

        /// <summary>Every Connect board, in menu order (size, then gravity).</summary>
        public static IReadOnlyList<LayoutDefinition> All { get; } =
            Sizes.SelectMany(s => new[] { Gravity.None, Gravity.Down, Gravity.Left }
                .Select(g => Create(s.Size, s.Columns, s.Rows, g))).ToList();

        /// <summary>The board sizes (the no-gravity boards), e.g. for a size menu.</summary>
        public static IReadOnlyList<LayoutDefinition> BaseBoards => All.Where(l => l.Gravity == Gravity.None).ToList();

        /// <summary>"Connect Small", "Connect Small (Gravity Down)"...</summary>
        public static string Name(string size, Gravity gravity) =>
            gravity == Gravity.None ? $"{Prefix} {size}" : $"{Prefix} {size} (Gravity {gravity})";

        /// <summary>The same size of board with another gravity setting.</summary>
        public static LayoutDefinition WithGravity(LayoutDefinition layout, Gravity gravity) =>
            All.First(l => l.PreviewName == layout.PreviewName && l.Gravity == gravity);

        private static LayoutDefinition Create(string size, int columns, int rows, Gravity gravity)
        {
            var cells = Enumerable.Range(0, rows).SelectMany(r => Enumerable.Range(0, columns).Select(c => new Cell(c, r).ToPosition()));
            return new LayoutDefinition(Name(size, gravity), cells, GameKind.Connect, columns, rows, gravity, $"{Prefix} {size}");
        }
    }

    /// <summary>Connect's rules on a set of occupied cells: finding paths, and gravity.</summary>
    public static class ConnectRules
    {
        /// <summary>
        /// The shortest line from <paramref name="a"/> to <paramref name="b"/> with at most two turns
        /// through empty cells (or just outside the board), as its corner points from a to b; null if
        /// there's none. The tiles' faces aren't checked here.
        /// </summary>
        public static IReadOnlyList<Cell> FindPath(LayoutDefinition layout, ISet<Position> occupied, Position a, Position b)
        {
            if (a == b)
            {
                return null;
            }

            var from = Cell.Of(a);
            var to = Cell.Of(b);
            bool Empty(int c, int r) => c < 0 || r < 0 || c >= layout.Columns || r >= layout.Rows || !occupied.Contains(new Cell(c, r).ToPosition());

            // Every cell strictly between two points on a line is empty.
            bool ClearRow(int row, int c1, int c2)
            {
                for (int c = Math.Min(c1, c2) + 1; c < Math.Max(c1, c2); c++)
                {
                    if (!Empty(c, row))
                    {
                        return false;
                    }
                }

                return true;
            }

            bool ClearColumn(int column, int r1, int r2)
            {
                for (int r = Math.Min(r1, r2) + 1; r < Math.Max(r1, r2); r++)
                {
                    if (!Empty(column, r))
                    {
                        return false;
                    }
                }

                return true;
            }

            // A straight line.
            if (from.Row == to.Row && ClearRow(from.Row, from.Column, to.Column))
            {
                return new[] { from, to };
            }

            if (from.Column == to.Column && ClearColumn(from.Column, from.Row, to.Row))
            {
                return new[] { from, to };
            }

            // One turn: the corner is where a's row meets b's column, or a's column meets b's row.
            if (Empty(to.Column, from.Row) && ClearRow(from.Row, from.Column, to.Column) && ClearColumn(to.Column, from.Row, to.Row))
            {
                return new[] { from, new Cell(to.Column, from.Row), to };
            }

            if (Empty(from.Column, to.Row) && ClearColumn(from.Column, from.Row, to.Row) && ClearRow(to.Row, from.Column, to.Column))
            {
                return new[] { from, new Cell(from.Column, to.Row), to };
            }

            // Two turns: across to some column (or row), along it, and back. Try every one, including
            // the lanes just outside the board, and keep the shortest.
            IReadOnlyList<Cell> best = null;
            int bestLength = int.MaxValue;
            for (int c = -1; c <= layout.Columns; c++)
            {
                if (c != from.Column && c != to.Column && Empty(c, from.Row) && Empty(c, to.Row)
                    && ClearRow(from.Row, from.Column, c) && ClearColumn(c, from.Row, to.Row) && ClearRow(to.Row, c, to.Column))
                {
                    int length = Math.Abs(c - from.Column) + Math.Abs(to.Row - from.Row) + Math.Abs(to.Column - c);
                    if (length < bestLength)
                    {
                        bestLength = length;
                        best = new[] { from, new Cell(c, from.Row), new Cell(c, to.Row), to };
                    }
                }
            }

            for (int r = -1; r <= layout.Rows; r++)
            {
                if (r != from.Row && r != to.Row && Empty(from.Column, r) && Empty(to.Column, r)
                    && ClearColumn(from.Column, from.Row, r) && ClearRow(r, from.Column, to.Column) && ClearColumn(to.Column, r, to.Row))
                {
                    int length = Math.Abs(r - from.Row) + Math.Abs(to.Column - from.Column) + Math.Abs(to.Row - r);
                    if (length < bestLength)
                    {
                        bestLength = length;
                        best = new[] { from, new Cell(from.Column, r), new Cell(to.Column, r), to };
                    }
                }
            }

            return best;
        }

        /// <summary>
        /// Where each remaining tile slides to under the layout's gravity: down (or left), keeping
        /// their order, until they close the gaps. Only tiles that move are listed.
        /// </summary>
        public static IReadOnlyList<(Position From, Position To)> Settle(LayoutDefinition layout, IEnumerable<Position> occupied)
        {
            var moves = new List<(Position, Position)>();
            var cells = occupied.Select(Cell.Of).ToList();
            if (layout.Gravity == Gravity.Down)
            {
                foreach (var column in cells.GroupBy(c => c.Column))
                {
                    int row = layout.Rows - 1;
                    foreach (var cell in column.OrderByDescending(c => c.Row))
                    {
                        Add(cell, new Cell(cell.Column, row--));
                    }
                }
            }
            else if (layout.Gravity == Gravity.Left)
            {
                foreach (var row in cells.GroupBy(c => c.Row))
                {
                    int column = 0;
                    foreach (var cell in row.OrderBy(c => c.Column))
                    {
                        Add(cell, new Cell(column++, cell.Row));
                    }
                }
            }

            return moves;

            void Add(Cell from, Cell to)
            {
                if (!from.Equals(to))
                {
                    moves.Add((from.ToPosition(), to.ToPosition()));
                }
            }
        }
    }

    /// <summary>
    /// Deals Connect boards that are guaranteed to be clearable, the same way
    /// <see cref="WinnableDealer"/> does for solitaire: it plays the game on the faceless board first,
    /// removing random pairs that can be joined (with gravity, if the board has it) until nothing is
    /// left, then gives each removed pair the same face. Replaying that order always clears the board.
    /// </summary>
    public static class ConnectDealer
    {
        public const int MaxAttempts = 200;

        public static DealResult Deal(LayoutDefinition layout, IReadOnlyCollection<Position> positions, IList<TileFace> pairFaces, Random random)
        {
            if (positions.Count != pairFaces.Count * 2)
            {
                throw new ArgumentException($"{positions.Count} positions need {positions.Count / 2} pairs, got {pairFaces.Count}.");
            }

            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                if (TryPlay(layout, positions, random, out var removals, out var startOf))
                {
                    var faces = new Dictionary<Position, TileFace>();
                    for (int i = 0; i < removals.Count; i++)
                    {
                        faces[startOf[removals[i].TokenA]] = pairFaces[i];
                        faces[startOf[removals[i].TokenB]] = pairFaces[i];
                    }

                    return new DealResult(faces, removals.Select(r => (r.A, r.B)).ToList());
                }
            }

            throw new InvalidOperationException($"Could not find a way to clear this Connect board after {MaxAttempts} attempts.");
        }

        // Plays the faceless board to the end. Each position holds a token (its starting position's
        // index) so tiles can be followed as gravity moves them.
        private static bool TryPlay(LayoutDefinition layout, IReadOnlyCollection<Position> positions, Random random,
            out List<(Position A, Position B, int TokenA, int TokenB)> removals, out List<Position> startOf)
        {
            startOf = positions.ToList();
            var tokenAt = new Dictionary<Position, int>();
            for (int i = 0; i < startOf.Count; i++)
            {
                tokenAt[startOf[i]] = i;
            }

            removals = new List<(Position, Position, int, int)>();
            var occupied = new HashSet<Position>(startOf);
            while (occupied.Count > 0)
            {
                // A random tile, and a random partner it can be joined to (the first that can, in a
                // random order, which is as fair as picking from all of them and much quicker).
                var candidates = occupied.ToList();
                random.Shuffle(candidates);
                Position? first = null, second = null;
                foreach (var a in candidates)
                {
                    var others = candidates.Where(b => b != a).ToList();
                    random.Shuffle(others);
                    foreach (var b in others)
                    {
                        if (ConnectRules.FindPath(layout, occupied, a, b) != null)
                        {
                            first = a;
                            second = b;
                            break;
                        }
                    }

                    if (first != null)
                    {
                        break;
                    }
                }

                if (first == null)
                {
                    return false;
                }

                removals.Add((first.Value, second.Value, tokenAt[first.Value], tokenAt[second.Value]));
                occupied.Remove(first.Value);
                occupied.Remove(second.Value);
                tokenAt.Remove(first.Value);
                tokenAt.Remove(second.Value);

                var moved = ConnectRules.Settle(layout, occupied);
                var tokens = moved.Select(m => tokenAt[m.From]).ToList();
                foreach (var (from, _) in moved)
                {
                    occupied.Remove(from);
                    tokenAt.Remove(from);
                }

                for (int i = 0; i < moved.Count; i++)
                {
                    occupied.Add(moved[i].To);
                    tokenAt[moved[i].To] = tokens[i];
                }
            }

            return true;
        }
    }
}
