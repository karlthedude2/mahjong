using System.Net;

namespace Mahjong.Web.Tests;

/// <summary>The cube-stones folder is a development tool, never served by the live site.</summary>
public sealed class DevStoneTests : IDisposable
{
    private readonly MahjongAppFactory app = new();

    public void Dispose() => app.Dispose();

    [Theory]
    [InlineData("/dev/stones")]
    [InlineData("/dev/stones/marble.png")]
    public async Task TheStoneFolderIsOnlyServedInDevelopment(string path)
    {
        var response = await app.CreateClient().GetAsync(path);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
