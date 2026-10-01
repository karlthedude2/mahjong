using Microsoft.AspNetCore.StaticFiles;

namespace Mahjong.Web.Api;

/// <summary>
/// Development only: pictures for the Mahjong Cube prototype, read from folders on disk so a new one
/// can be tried by dropping it in and refreshing the page.
/// <list type="bullet">
/// <item><c>/dev/stones</c>: stone textures, from <c>Cube:StonesFolder</c> or <c>cube-stones</c> at the top of the repository.</item>
/// <item><c>/dev/faces</c>: face art named after the face (<c>2bams.png</c>), from <c>Cube:FacesFolder</c> or <c>cube-faces</c>.</item>
/// </list>
/// </summary>
public static class DevPictureEndpoints
{
    private static readonly string[] Extensions = [".png", ".jpg", ".jpeg", ".webp"];

    public static void MapDevPictures(this IEndpointRouteBuilder app, IConfiguration config, IWebHostEnvironment environment)
    {
        MapFolder(app, "/dev/stones", Folder(config["Cube:StonesFolder"], "cube-stones", environment));
        MapFolder(app, "/dev/faces", Folder(config["Cube:FacesFolder"], "cube-faces", environment));
    }

    private static string Folder(string? configured, string name, IWebHostEnvironment environment)
    {
        string folder = Path.GetFullPath(configured is { Length: > 0 } ? configured : Path.Combine(environment.ContentRootPath, "..", "..", name));
        Directory.CreateDirectory(folder);
        return folder;
    }

    private static void MapFolder(IEndpointRouteBuilder app, string route, string folder)
    {
        app.MapGet(route, () => Directory.EnumerateFiles(folder)
            .Where(IsPicture)
            .Select(Path.GetFileName)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList());

        var contentTypes = new FileExtensionContentTypeProvider();
        app.MapGet(route + "/{name}", (string name, HttpContext http) =>
        {
            // Only a picture directly in the folder, never a path out of it.
            string path = Path.Combine(folder, name);
            if (name != Path.GetFileName(name) || !IsPicture(path) || !File.Exists(path) || !contentTypes.TryGetContentType(path, out var contentType))
            {
                return Results.NotFound();
            }

            // A picture replaced under the same name shows on the next refresh.
            http.Response.Headers.CacheControl = "no-cache";
            return Results.File(path, contentType, lastModified: File.GetLastWriteTimeUtc(path));
        });
    }

    private static bool IsPicture(string path) => Extensions.Contains(Path.GetExtension(path).ToLowerInvariant());
}
