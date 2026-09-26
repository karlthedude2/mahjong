using System;
using System.Collections.Generic;
using System.Linq;

namespace Mahjong.Core
{
    /// <summary>The tiles currently on the table.</summary>
    public sealed class Board
    {
        private readonly Dictionary<Position, Tile> tiles = new Dictionary<Position, Tile>();
        private readonly HashSet<Position> occupied = new HashSet<Position>();

        // Guaranteed-winnable deals (and reshuffles), or purely random ones.
        private bool winnable = true;

        // Which game's rules apply (solitaire or Connect), and Connect's grid and gravity.
        private LayoutDefinition layout;

        private Board()
        {
        }

        /// <summary>
        /// Deals a new board for <paramref name="layout"/>: guaranteed solvable, or (if
        /// <paramref name="winnable"/> is false) purely random, which may have no solution.
        /// </summary>
        public static Board Deal(LayoutDefinition layout, TileSet tileSet, Random random, bool winnable = true)
        {
            var pairFaces = tileSet.CreatePairs(layout.Positions.Count / 2, random);
            var deal = !winnable ? RandomDealer.Deal(layout.Positions, pairFaces, random)
                : layout.Kind == GameKind.Connect ? ConnectDealer.Deal(layout, layout.Positions, pairFaces, random)
                : WinnableDealer.Deal(layout.Positions, pairFaces, random);

            var board = new Board { LastDeal = deal, winnable = winnable, layout = layout };
            int id = 0;
            foreach (var position in layout.Positions)
            {
                board.Add(new Tile(id++, position, deal.Faces[position]));
            }

            return board;
        }

        public IEnumerable<Tile> Tiles => tiles.Values;

        public int Count => tiles.Count;

        public bool IsComplete => tiles.Count == 0;

        /// <summary>
        /// The result of the last deal or reshuffle. For a winnable deal, its <see cref="DealResult.Solution"/>
        /// clears the board as long as no tiles have been removed since; a random deal has none.
        /// </summary>
        public DealResult LastDeal { get; private set; }

        public bool Contains(Tile tile)
        {
            return tile != null && tiles.TryGetValue(tile.Position, out var found) && found == tile;
        }

        /// <summary>The tile with this <see cref="Tile.Id"/> if it's still on the board.</summary>
        public Tile FindTile(int id) => tiles.Values.FirstOrDefault(t => t.Id == id);

        public Tile At(Position position)
        {
            tiles.TryGetValue(position, out var tile);
            return tile;
        }

        /// <summary>Whether the tile can be picked: in Connect every tile can; in solitaire only free ones.</summary>
        public bool IsFree(Tile tile) =>
            Contains(tile) && (layout?.Kind == GameKind.Connect || BoardRules.IsFree(tile.Position, occupied));

        public bool CanRemove(Tile first, Tile second)
        {
            return first != second && first.Face.Matches(second.Face) && IsFree(first) && IsFree(second)
                && (layout?.Kind != GameKind.Connect || FindPath(first, second) != null);
        }

        /// <summary>
        /// For Connect: the line joining two tiles (its corner points, in cells), or null if they
        /// can't be joined. Always null in solitaire.
        /// </summary>
        public IReadOnlyList<Cell> FindPath(Tile first, Tile second)
        {
            return layout?.Kind == GameKind.Connect && Contains(first) && Contains(second)
                ? ConnectRules.FindPath(layout, occupied, first.Position, second.Position)
                : null;
        }

        public bool HasAvailableMove() => FindMove() != null;

        /// <summary>A pair of tiles that can be removed now, or null if there's none (a hint).</summary>
        public (Tile First, Tile Second)? FindMove()
        {
            foreach (var group in Tiles.Where(IsFree).GroupBy(t => t.Face.Name).Where(g => g.Count() > 1))
            {
                var tiles = group.OrderBy(t => t.Id).ToList();
                for (int i = 0; i < tiles.Count; i++)
                {
                    for (int j = i + 1; j < tiles.Count; j++)
                    {
                        if (CanRemove(tiles[i], tiles[j]))
                        {
                            return (tiles[i], tiles[j]);
                        }
                    }
                }
            }

            return null;
        }

        internal void Add(Tile tile)
        {
            tiles.Add(tile.Position, tile);
            occupied.Add(tile.Position);
        }

        internal void Remove(Tile tile)
        {
            tiles.Remove(tile.Position);
            occupied.Remove(tile.Position);
        }

        /// <summary>
        /// For Connect with gravity: slides the remaining tiles to close the gaps. Returns the moves,
        /// so they can be undone with <see cref="Unsettle"/>.
        /// </summary>
        internal IReadOnlyList<(Tile Tile, Position From, Position To)> Settle()
        {
            if (layout?.Kind != GameKind.Connect || layout.Gravity == Gravity.None)
            {
                return Array.Empty<(Tile, Position, Position)>();
            }

            var moves = ConnectRules.Settle(layout, occupied).Select(m => (tiles[m.From], m.From, m.To)).ToList();
            MoveTiles(moves.Select(m => (m.Item1, m.To)));
            return moves;
        }

        internal void Unsettle(IReadOnlyList<(Tile Tile, Position From, Position To)> moves)
        {
            MoveTiles(moves.Select(m => (m.Tile, m.From)));
        }

        private void MoveTiles(IEnumerable<(Tile Tile, Position To)> moves)
        {
            var list = moves.ToList();
            foreach (var (tile, _) in list)
            {
                tiles.Remove(tile.Position);
                occupied.Remove(tile.Position);
            }

            foreach (var (tile, to) in list)
            {
                tile.Position = to;
                Add(tile);
            }
        }

        /// <summary>
        /// Redistributes the faces of the remaining tiles: so the board can still be cleared, or at
        /// random for a random deal. Returns the faces as they were before, for undo.
        /// </summary>
        internal Dictionary<Tile, TileFace> Reshuffle(Random random)
        {
            var before = CurrentFaces();

            var pairFaces = tiles.Values
                .GroupBy(t => t.Face.Name)
                .SelectMany(g => Enumerable.Repeat(g.First().Face, g.Count() / 2))
                .ToList();
            random.Shuffle(pairFaces);

            LastDeal = !winnable ? RandomDealer.Deal(tiles.Keys.ToList(), pairFaces, random)
                : layout?.Kind == GameKind.Connect ? ConnectDealer.Deal(layout, tiles.Keys.ToList(), pairFaces, random)
                : WinnableDealer.Deal(tiles.Keys.ToList(), pairFaces, random);
            foreach (var tile in tiles.Values)
            {
                tile.Face = LastDeal.Faces[tile.Position];
            }

            return before;
        }

        internal Dictionary<Tile, TileFace> CurrentFaces() => tiles.Values.ToDictionary(t => t, t => t.Face);

        internal void SetFaces(IReadOnlyDictionary<Tile, TileFace> faces)
        {
            foreach (var entry in faces)
            {
                entry.Key.Face = entry.Value;
            }
        }
    }
}
