using Microsoft.AspNetCore.StaticFiles;

namespace Mahjong.Web.Api;

/// <summary>
/// Development only: pictures for the Mahjong Cube prototype, read from folders on disk so a new one
/// can be tried by dropping it in and refreshing the page.
/// <list type="bullet">
/// <item><c>/dev/stones</c>: stone textures, from <c>Cube:StonesFolder</c> or <c>cube-stones</c> at the top of the repository.</item>
/// <item><c>/dev/faces</c>: face art named after the face (<c>2bams.png</c>), from <c>Cube:FacesFolder</c> or <c>cube-faces</c>.
/// Its <c>light</c> and <c>dark</c> subfolders (<c>/dev/faces/light</c>, <c>/dev/faces/dark</c>) hold the art for stones
/// configured with light or dark faces; a picture in the folder itself serves either.</item>
/// <item><c>POST /dev/layout-previews/{slug}</c>: saves a layout's picker picture (see <see cref="LayoutPreviewFiles"/>)
/// into the client's wwwroot/layouts/previews, and a copy into <c>LayoutPreviews:CopyTo</c> if that's set.</item>
/// </list>
/// </summary>
public static class DevPictureEndpoints
{
    private static readonly string[] Extensions = [".png", ".jpg", ".jpeg", ".webp"];

    public static void MapDevPictures(this IEndpointRouteBuilder app, IConfiguration config, IWebHostEnvironment environment)
    {
        MapFolder(app, "/dev/stones", Folder(config["Cube:StonesFolder"], "cube-stones", environment));
        string faces = Folder(config["Cube:FacesFolder"], "cube-faces", environment);
        MapFolder(app, "/dev/faces", faces);
        foreach (var set in new[] { "light", "dark" })
        {
            string folder = Path.Combine(faces, set);
            Directory.CreateDirectory(folder);
            MapFolder(app, "/dev/faces/" + set, folder);
        }

        string previews = Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "Mahjong.Web.Client", "wwwroot", "layouts", "previews"));
        app.MapPost("/dev/layout-previews/{slug}", async (string slug, HttpRequest request) =>
        {
            using var body = new MemoryStream();
            await request.Body.CopyToAsync(body);
            try
            {
                return Results.Ok(LayoutPreviewFiles.Save(slug, body.ToArray(), previews, config["LayoutPreviews:CopyTo"]));
            }
            catch (ArgumentException e)
            {
                return Results.BadRequest(e.Message);
            }
        }).DisableAntiforgery();
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
