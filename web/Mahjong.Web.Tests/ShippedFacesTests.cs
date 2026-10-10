using Mahjong.Core;
using Mahjong.Web.Client.Game;

namespace Mahjong.Web.Tests;

/// <summary>
/// The face art for Mahjong 3D and the cube ships with the site (made from cube-faces by
/// tools/ship-faces.py): every face the game deals needs a plain picture and a dark-stone one.
/// </summary>
public sealed class ShippedFacesTests
{
    private static readonly string Faces = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..",
        "Mahjong.Web.Client", "wwwroot", FaceArt.ShippedFolder));

    public static TheoryData<string> AllFaces() => new(TileSet.Standard.PairFaces.Select(f => f.Name).Distinct());

    [Theory]
    [MemberData(nameof(AllFaces))]
    public void EveryFaceShipsForLightAndDarkStones(string face)
    {
        Assert.True(File.Exists(Path.Combine(Faces, face + FaceArt.ShippedExtension)), $"No plain picture for {face}");
        Assert.True(File.Exists(Path.Combine(Faces, "dark", face + FaceArt.ShippedExtension)), $"No dark-stone picture for {face}");
    }
}
