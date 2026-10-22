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

    [Fact]
    public void PreviewPicturesHaveSeeThroughBackgrounds()
    {
        // PNG IHDR: colour type is byte 25; 6 = RGBA, so the chosen background shows behind the tiles.
        Assert.All(Directory.GetFiles(PreviewFolder(), "*.png"), file => Assert.Equal(6, File.ReadAllBytes(file)[25]));
    }

    [Fact]
    public void EveryVisibleLayoutAndBoardHasAPicture()
    {
        var missing = LayoutCatalog.Visible.Concat(ConnectLayouts.BaseBoards)
            .Where(l => !File.Exists(Path.Combine(PreviewFolder(), LayoutPreviews.Slug(l.PreviewName) + ".png")))
            .Select(l => l.Name)
            .ToList();

        Assert.True(missing.Count == 0, "No picture (draw it at /dev/layout-previews): " + string.Join(", ", missing));
    }

    [Fact]
    public void PicturesAre400By300()
    {
        // PNG IHDR: width and height are big-endian at bytes 16 and 20.
        Assert.All(Directory.GetFiles(PreviewFolder(), "*.png"), file =>
        {
            byte[] png = File.ReadAllBytes(file);
            Assert.Equal((400, 300), (System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16)),
                System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20))));
        });
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
