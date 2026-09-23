using System;

namespace Mahjong.Core
{
    /// <summary>
    /// A tile slot on the board. X and Y are in half-tile units (a tile is 2 wide and 2 tall),
    /// so odd values place a tile half a tile over. Z is the layer, 0 being the table.
    /// </summary>
    public readonly struct Position : IEquatable<Position>
    {
        public Position(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public int X { get; }
        public int Y { get; }
        public int Z { get; }

        public bool Equals(Position other) => X == other.X && Y == other.Y && Z == other.Z;

        public override bool Equals(object obj) => obj is Position other && Equals(other);

        public override int GetHashCode() => unchecked((X * 397 ^ Y) * 397 ^ Z);

        public static bool operator ==(Position a, Position b) => a.Equals(b);

        public static bool operator !=(Position a, Position b) => !a.Equals(b);

        public override string ToString() => $"({X}, {Y}, {Z})";
    }
}
