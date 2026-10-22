using System.Net;

namespace Mahjong.Web.Tests;

/// <summary>The cube-stones and cube-faces folders are a development tool, never served by the live site.</summary>
public sealed class DevPictureTests : IDisposable
{
    private readonly MahjongAppFactory app = new();

    public void Dispose() => app.Dispose();

    [Theory]
    [InlineData("/dev/stones")]
    [InlineData("/dev/stones/marble.png")]
    [InlineData("/dev/faces")]
    [InlineData("/dev/faces/2bams.png")]
    [InlineData("/dev/faces/dark")]
    [InlineData("/dev/faces/dark/2bams.png")]
    public async Task ThePictureFoldersAreOnlyServedInDevelopment(string path)
    {
        var response = await app.CreateClient().GetAsync(path);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task LayoutPicturesCanOnlyBeSavedInDevelopment()
    {
        var response = await app.CreateClient().PostAsync("/dev/layout-previews/arena", new ByteArrayContent([1, 2, 3]));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
