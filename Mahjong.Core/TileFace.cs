namespace Mahjong.Core
{
    /// <summary>What is printed on a tile. Tiles match when their faces have the same name.</summary>
    public sealed class TileFace
    {
        public TileFace(string name, int value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>Image name, e.g. "2bams" (the app loads Graphics\2bams.png).</summary>
        public string Name { get; }

        /// <summary>Points scored for each tile of this face that is removed.</summary>
        public int Value { get; }

        public bool Matches(TileFace other) => other != null && other.Name == Name;

        public override string ToString() => Name;
    }
}
