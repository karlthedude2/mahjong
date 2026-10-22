using System.Net.Http.Json;
using Mahjong.Web.Client.Api;

namespace Mahjong.Web.Client.Game;

/// <summary>
/// Face art and stone settings for the 3D views (the Mahjong Cube and the 3D board), loaded once.
/// <list type="bullet">
/// <item>Face pictures ship with the site, one folder per face set (see <see cref="FaceSets"/>; made from
/// cube-faces by tools/ship-faces.py): the plain pictures, and in its dark folder the ones for dark stones,
/// with a white glow. The player chooses the set.</item>
/// <item>In development, a face with any picture in the cube-faces folder uses those instead: its light
/// and dark subfolders for stones with light or dark faces, and the folder itself for either. So new
/// art can be tried by dropping it in, before it's shipped.</item>
/// <item>Which face set each stone uses ("light" or "dark") is the server's configuration
/// (Cube:StoneFaces); players can't change it.</item>
/// </list>
/// </summary>
public sealed class FaceArt(HttpClient http, GameApi api)
{
    private static readonly string[] Sets = ["any", "light", "dark"];

    private readonly Dictionary<string, Dictionary<string, string>> pictures = Sets.ToDictionary(s => s, _ => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
    private Dictionary<string, string> stoneFaces = new(StringComparer.OrdinalIgnoreCase);
    private Task? loading;

    public Task LoadAsync() => loading ??= LoadOnceAsync();

    /// <summary>A face's pictures in the chosen face set, by stone shade: { light, dark, any }, each a URL or null.</summary>
    public object Pictures(string face, FaceSetInfo faceSet)
    {
        if (Sets.Any(set => pictures[set].ContainsKey(face)))
        {
            return new
            {
                light = pictures["light"].GetValueOrDefault(face),
                dark = pictures["dark"].GetValueOrDefault(face),
                any = pictures["any"].GetValueOrDefault(face),
            };
        }

        return new { light = (string?)null, dark = FaceSets.PictureUrl(faceSet, face, dark: true), any = FaceSets.PictureUrl(faceSet, face, dark: false) };
    }

    /// <summary>The configured face set ("light" or "dark") of a stone, by its name or a tried picture's file name.</summary>
    public string? StoneFaces(string stone) => stoneFaces.GetValueOrDefault(stone);

    /// <summary>A development folder's picture files (cube-stones, cube-faces); none on the live site.</summary>
    public async Task<List<string>> ListAsync(string list)
    {
        try
        {
            var response = await http.GetAsync(list);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<List<string>>() ?? [] : [];
        }
        catch (Exception e) when (e is HttpRequestException or System.Text.Json.JsonException)
        {
            return [];
        }
    }

    private async Task LoadOnceAsync()
    {
        foreach (var (set, list) in new[] { ("any", "dev/faces"), ("light", "dev/faces/light"), ("dark", "dev/faces/dark") })
        {
            foreach (var file in await ListAsync(list))
            {
                pictures[set][Path.GetFileNameWithoutExtension(file)] = list + "/" + Uri.EscapeDataString(file);
            }
        }

        try
        {
            var config = await api.GetConfigAsync();
            stoneFaces = new Dictionary<string, string>(config.CubeStoneFaces ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase);
        }
        catch (HttpRequestException)
        {
            // Without the configuration, each stone's faces follow how dark it looks.
        }
    }
}
