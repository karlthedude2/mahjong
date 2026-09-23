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
        public Position Position { get; }
        public TileFace Face { get; internal set; }

        public override string ToString() => $"{Face} {Position}";
    }
}
