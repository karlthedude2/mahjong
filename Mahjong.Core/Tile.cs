namespace Mahjong.Core
{
    public sealed class Tile
    {
        internal Tile(int id, Position position, TileFace face)
        {
            Id = id;
            Position = position;
            Face = face;
        }

        public int Id { get; }
        /// <summary>Where the tile is. Only Connect's gravity ever moves a tile.</summary>
        public Position Position { get; internal set; }
        public TileFace Face { get; internal set; }

        public override string ToString() => $"{Face} {Position}";
    }
}
