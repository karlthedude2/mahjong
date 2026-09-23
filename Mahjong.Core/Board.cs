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

        private Board()
        {
        }

        /// <summary>Deals a new, guaranteed-solvable board for <paramref name="layout"/>.</summary>
        public static Board Deal(LayoutDefinition layout, TileSet tileSet, Random random)
        {
            var pairFaces = tileSet.CreatePairs(layout.Positions.Count / 2, random);
            var deal = WinnableDealer.Deal(layout.Positions, pairFaces, random);

            var board = new Board { LastDeal = deal };
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
        /// The result of the last deal or reshuffle. Its <see cref="DealResult.Solution"/> clears the
        /// board as long as no tiles have been removed since.
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

        public bool IsFree(Tile tile) => Contains(tile) && BoardRules.IsFree(tile.Position, occupied);

        public bool CanRemove(Tile first, Tile second)
        {
            return first != second && first.Face.Matches(second.Face) && IsFree(first) && IsFree(second);
        }

        public bool HasAvailableMove()
        {
            return Tiles.Where(IsFree).GroupBy(t => t.Face.Name).Any(g => g.Count() > 1);
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
        /// Redistributes the faces of the remaining tiles so the board can still be cleared.
        /// Returns the faces as they were before, for undo.
        /// </summary>
        internal Dictionary<Tile, TileFace> Reshuffle(Random random)
        {
            var before = CurrentFaces();

            var pairFaces = tiles.Values
                .GroupBy(t => t.Face.Name)
                .SelectMany(g => Enumerable.Repeat(g.First().Face, g.Count() / 2))
                .ToList();
            random.Shuffle(pairFaces);

            LastDeal = WinnableDealer.Deal(tiles.Keys.ToList(), pairFaces, random);
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
