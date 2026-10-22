using Mahjong.Web.Client.Game;

namespace Mahjong.Web.Tests;

/// <summary>The pictures on the settings screen's Tiles and Stone cards.</summary>
public sealed class SettingsArtTests
{
    [Fact]
    public void EveryImageSetHasTheSampleFaces() =>
        Assert.All(TileSets.Flat.Where(s => s.Kind == TileSetKind.Images), set =>
            Assert.All(SettingsArt.SampleFaces, face =>
                Assert.True(File.Exists(Wwwroot(TileSets.ImageUrl(set, face))), $"{set.Id}/{face}.png")));

    [Theory]
    [InlineData("3d-ivory", "#EFE6D0")]
    [InlineData("3d-onyx", "#1E1C1F")]
    [InlineData("3d-beige", "#D8C3A0")]
    [InlineData("3d-marble", "url('textures/square-marble.jpg') center / cover")]
    public void EachStoneHasItsOwnLook(string id, string css) =>
        Assert.Equal(css, SettingsArt.StoneCss(TileSets.Find(id)));

    [Theory]
    [InlineData("3d-onyx", null, true)]
    [InlineData("3d-ivory", null, false)]
    [InlineData("3d-ivory", "dark", true)]
    [InlineData("3d-onyx", "light", false)]
    public void DarkStonesShowTheDarkPictures(string stoneId, string? configured, bool dark) =>
        Assert.Equal(dark, SettingsArt.IsDark(TileSets.Find(stoneId), configured));

    [Fact]
    public void FaceUrlsComeFromTheChosenSet() =>
        Assert.Equal("faces/standard/dark/East.webp", SettingsArt.FaceUrl(FaceSets.Standard, "East", darkStone: true));

    private static string Wwwroot(string path)
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
        {
            string root = Path.Combine(dir.FullName, "Mahjong.Web.Client", "wwwroot");
            if (Directory.Exists(root))
            {
                return Path.Combine(root, path);
            }
        }

        throw new DirectoryNotFoundException("Couldn't find the client's wwwroot.");
    }
}
