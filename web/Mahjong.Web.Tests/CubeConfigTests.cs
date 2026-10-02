using System.Net.Http.Json;
using Mahjong.Web.Client.Api;

namespace Mahjong.Web.Tests;

/// <summary>Which face set (light or dark) each Mahjong Cube stone uses comes from the server's settings.</summary>
public sealed class CubeConfigTests : IDisposable
{
    private readonly MahjongAppFactory app = new();

    public void Dispose() => app.Dispose();

    [Fact]
    public async Task TheBuiltInStonesHaveTheirFaceSets()
    {
        var config = await app.CreateClient().GetFromJsonAsync<ClientConfig>("/api/client-config");

        var faces = new Dictionary<string, string>(config!.CubeStoneFaces!, StringComparer.OrdinalIgnoreCase);
        Assert.Equal("light", faces["marble"]);
        Assert.Equal("light", faces["ivory"]);
        Assert.Equal("dark", faces["darkMarble"]);
        Assert.Equal("dark", faces["onyx"]);
    }

    [Fact]
    public async Task OnlyLightOrDarkIsPassedOn()
    {
        using var configured = app.WithWebHostBuilder(b => b
            .UseSetting("Cube:StoneFaces:jade.png", " Dark ")
            .UseSetting("Cube:StoneFaces:sand.png", "medium"));

        var config = await configured.CreateClient().GetFromJsonAsync<ClientConfig>("/api/client-config");

        var faces = new Dictionary<string, string>(config!.CubeStoneFaces!, StringComparer.OrdinalIgnoreCase);
        Assert.Equal("dark", faces["jade.png"]);
        Assert.False(faces.ContainsKey("sand.png"));
    }
}
