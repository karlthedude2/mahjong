using Mahjong.Core;
using Mahjong.Web.Client.Game;

namespace Mahjong.Web.Tests;

public sealed class LayoutPreviewTests
{
    [Theory]
    [InlineData("Arena", "layouts/previews/arena.png")]
    [InlineData("Standard Turtle", "layouts/previews/standard-turtle.png")]
    [InlineData("Twin Peaks", "layouts/previews/twin-peaks.png")]
    public void PreviewUrlsUseTheLayoutSlug(string layout, string expected) =>
        Assert.Equal(expected, LayoutPreviews.Url(layout));

    [Fact]
    public void EveryPreviewPictureMatchesALayout()
    {
        var slugs = LayoutCatalog.All.Concat(LayoutCatalog.Connect).Select(l => LayoutPreviews.Slug(l.PreviewName)).ToHashSet();
        var pictures = Directory.GetFiles(PreviewFolder(), "*.png").Select(Path.GetFileNameWithoutExtension).ToList();

        Assert.NotEmpty(pictures);
        Assert.All(pictures, p => Assert.Contains(p!, slugs));
    }

    private static string PreviewFolder()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
        {
            string folder = Path.Combine(dir.FullName, "Mahjong.Web.Client", "wwwroot", LayoutPreviews.Folder);
            if (Directory.Exists(folder))
            {
                return folder;
            }
        }

        throw new DirectoryNotFoundException("Couldn't find the layout preview folder.");
    }
}
