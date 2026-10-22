using Mahjong.Web.Client.Game;

namespace Mahjong.Web.Tests;

/// <summary>Face sets for Mahjong 3D and the cube: one folder each, light pictures with a dark folder beside them.</summary>
public sealed class FaceSetsTests
{
    [Fact]
    public void UnknownSetFallsBackToStandard()
    {
        Assert.Same(FaceSets.Standard, FaceSets.Find("gone"));
        Assert.Same(FaceSets.Standard, FaceSets.Find(null));
        Assert.Same(FaceSets.Standard, FaceSets.Default);
    }

    [Fact]
    public void PictureUrlsUseTheSetFolder()
    {
        Assert.Equal("faces/standard/Cdragon.webp", FaceSets.PictureUrl(FaceSets.Standard, "Cdragon", dark: false));
        Assert.Equal("faces/standard/dark/Cdragon.webp", FaceSets.PictureUrl(FaceSets.Standard, "Cdragon", dark: true));
    }
}
