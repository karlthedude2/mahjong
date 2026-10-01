using Microsoft.AspNetCore.StaticFiles;

namespace Mahjong.Web.Api;

/// <summary>
/// Development only: stone textures for the Mahjong Cube prototype, read from a folder on disk so a
/// new picture can be tried by dropping it in and refreshing the page. The folder is
/// <c>Cube:StonesFolder</c>, or <c>cube-stones</c> at the top of the repository.
/// </summary>
public static class DevStoneEndpoints
{
    private static readonly string[] Extensions = [".png", ".jpg", ".jpeg", ".webp"];

    public static void MapDevStones(this IEndpointRouteBuilder app, IConfiguration config, IWebHostEnvironment environment)
    {
        string folder = Path.GetFullPath(config["Cube:StonesFolder"] is { Length: > 0 } configured
            ? configured
            : Path.Combine(environment.ContentRootPath, "..", "..", "cube-stones"));
        Directory.CreateDirectory(folder);

        app.MapGet("/dev/stones", () => Directory.EnumerateFiles(folder)
            .Where(IsPicture)
            .Select(Path.GetFileName)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList());

        var contentTypes = new FileExtensionContentTypeProvider();
        app.MapGet("/dev/stones/{name}", (string name, HttpContext http) =>
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
