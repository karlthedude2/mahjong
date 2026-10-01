using Mahjong.Core;

namespace Mahjong.Web.Client.Game;

/// <summary>A tile in the cube game: a cube with the same face printed on all six sides.</summary>
public sealed record CubeTile(int Id, int X, int Y, int Z, TileFace Face);

/// <summary>
/// Prototype: Mahjong Cube. Tiles are cubes packed into a Size × Size × Size block that the player
/// turns around. A tile can be taken when two of its sides that meet at an edge are open (say its
/// right and its top), so outer corners and edges come off first and the block wears away.
/// Deals are winnable the same way as the solitaire dealer's: pairs of free positions are removed
/// from the full block until it's empty, then each removed pair gets the same face.
/// </summary>
public sealed class CubeGame
{
    private static readonly (int X, int Y, int Z)[] Axes = [(1, 0, 0), (0, 1, 0), (0, 0, 1)];

    private readonly Dictionary<(int X, int Y, int Z), CubeTile> tiles;

    private CubeGame(int size, IEnumerable<CubeTile> tiles)
    {
        Size = size;
        this.tiles = tiles.ToDictionary(t => (t.X, t.Y, t.Z));
    }

    public int Size { get; }

    public IReadOnlyCollection<CubeTile> Tiles => tiles.Values;

    public bool IsCleared => tiles.Count == 0;

    public bool IsFree(CubeTile tile) => IsFree((tile.X, tile.Y, tile.Z), tiles.ContainsKey);

    public bool Contains(CubeTile tile) => tiles.TryGetValue((tile.X, tile.Y, tile.Z), out var here) && here == tile;

    /// <summary>Removes the two tiles if they match and are both free.</summary>
    public bool TryRemovePair(CubeTile first, CubeTile second)
    {
        if (first == second || !first.Face.Matches(second.Face) || !Contains(first) || !Contains(second) || !IsFree(first) || !IsFree(second))
        {
            return false;
        }

        tiles.Remove((first.X, first.Y, first.Z));
        tiles.Remove((second.X, second.Y, second.Z));
        return true;
    }

    /// <summary>Two free tiles that match, or null if there's no move.</summary>
    public (CubeTile First, CubeTile Second)? FindPair()
    {
        var free = Tiles.Where(IsFree).GroupBy(t => t.Face.Name).FirstOrDefault(g => g.Count() >= 2);
        return free == null ? null : (free.First(), free.Skip(1).First());
    }

    public static CubeGame Deal(int size, Random random)
    {
        if (size * size * size % 2 != 0)
        {
            throw new ArgumentException("The cube needs an even number of tiles.", nameof(size));
        }

        var positions = new List<(int X, int Y, int Z)>();
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    positions.Add((x, y, z));
                }
            }
        }

        for (int attempt = 0; attempt < WinnableDealer.MaxAttempts; attempt++)
        {
            if (TryFindRemovalOrder(positions, random) is { } order)
            {
                var faces = TileSet.Standard.CreatePairs(order.Count, random);
                int id = 0;
                var dealt = order.SelectMany((pair, i) => new[]
                {
                    new CubeTile(id++, pair.First.X, pair.First.Y, pair.First.Z, faces[i]),
                    new CubeTile(id++, pair.Second.X, pair.Second.Y, pair.Second.Z, faces[i]),
                });
                return new CubeGame(size, dealt);
            }
        }

        throw new InvalidOperationException("Couldn't deal a winnable cube.");
    }

    private static List<((int X, int Y, int Z) First, (int X, int Y, int Z) Second)>? TryFindRemovalOrder(
        IReadOnlyCollection<(int X, int Y, int Z)> positions, Random random)
    {
        var left = positions.ToHashSet();
        var order = new List<((int, int, int), (int, int, int))>();
        while (left.Count > 0)
        {
            var free = left.Where(p => IsFree(p, left.Contains)).ToList();
            if (free.Count < 2)
            {
                return null;
            }

            int a = random.Next(free.Count);
            int b = random.Next(free.Count - 1);
            if (b >= a)
            {
                b++;
            }

            left.Remove(free[a]);
            left.Remove(free[b]);
            order.Add((free[a], free[b]));
        }

        return order;
    }

    // Free when open along two different axes: one open side across (left or right) and one open
    // side up or down, say, which is two sides meeting at an edge. Open sides on one axis only
    // (left and right, but covered above, below, in front and behind) don't count.
    private static bool IsFree((int X, int Y, int Z) p, Func<(int X, int Y, int Z), bool> occupied)
    {
        int openAxes = 0;
        foreach (var (dx, dy, dz) in Axes)
        {
            if (!occupied((p.X + dx, p.Y + dy, p.Z + dz)) || !occupied((p.X - dx, p.Y - dy, p.Z - dz)))
            {
                openAxes++;
            }
        }

        return openAxes >= 2;
    }
}
