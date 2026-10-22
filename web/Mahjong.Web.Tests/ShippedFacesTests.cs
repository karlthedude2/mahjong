using Mahjong.Core;
using Mahjong.Web.Client.Game;

namespace Mahjong.Web.Tests;

/// <summary>
/// The face art for Mahjong 3D and the cube ships with the site, one folder per face set (made from
/// cube-faces by tools/ship-faces.py): every face the game deals needs a plain picture and a dark-stone
/// one in every set.
/// </summary>
public sealed class ShippedFacesTests
{
    private static readonly string Wwwroot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..",
        "Mahjong.Web.Client", "wwwroot"));

    public static TheoryData<string, string> AllFaces()
    {
        var data = new TheoryData<string, string>();
        foreach (var set in FaceSets.All)
        {
            foreach (var face in TileSet.Standard.PairFaces.Select(f => f.Name).Distinct())
            {
                data.Add(set.Id, face);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(AllFaces))]
    public void EveryFaceShipsForLightAndDarkStones(string setId, string face)
    {
        var set = FaceSets.Find(setId);
        Assert.True(File.Exists(Path.Combine(Wwwroot, FaceSets.PictureUrl(set, face, dark: false))), $"No plain picture for {face} in {setId}");
        Assert.True(File.Exists(Path.Combine(Wwwroot, FaceSets.PictureUrl(set, face, dark: true))), $"No dark-stone picture for {face} in {setId}");
    }
}
