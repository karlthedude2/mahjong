using System.Buffers.Binary;
using Mahjong.Core;
using Mahjong.Web.Client.Game;

namespace Mahjong.Web.Api;

/// <summary>
/// Development only: saves a layout's picker picture, drawn by the /dev/layout-previews page with the
/// game's own 3D renderer. Only for a known layout, and only a 400×300 PNG with transparency.
/// </summary>
public static class LayoutPreviewFiles
{
    public const int Width = 400;
    public const int Height = 300;

    private static readonly byte[] Signature = [0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A];

    /// <summary>
    /// Writes {slug}.png to the previews folder and, if copyTo is set, "{Layout name}.png" there too.
    /// Returns the paths written; throws <see cref="ArgumentException"/> for an unknown layout or a wrong picture.
    /// </summary>
    public static IReadOnlyList<string> Save(string slug, byte[] png, string previewsFolder, string? copyTo)
    {
        var layout = LayoutCatalog.All.Concat(ConnectLayouts.BaseBoards).FirstOrDefault(l => LayoutPreviews.Slug(l.PreviewName) == slug)
            ?? throw new ArgumentException($"No layout's picture is called \"{slug}\".");
        Check(png);

        var written = new List<string> { Path.Combine(previewsFolder, slug + ".png") };
        if (!string.IsNullOrWhiteSpace(copyTo))
        {
            Directory.CreateDirectory(copyTo);
            written.Add(Path.Combine(copyTo, layout.PreviewName + ".png"));
        }

        foreach (var path in written)
        {
            File.WriteAllBytes(path, png);
        }

        return written;
    }

    // The PNG header: the signature, then IHDR's width, height and colour type (6 = RGBA).
    private static void Check(byte[] png)
    {
        if (png.Length < 33 || !png.AsSpan(0, 8).SequenceEqual(Signature))
        {
            throw new ArgumentException("That isn't a PNG.");
        }

        int width = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16));
        int height = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20));
        if (width != Width || height != Height)
        {
            throw new ArgumentException($"The picture is {width}×{height}; it should be {Width}×{Height}.");
        }

        if (png[25] != 6)
        {
            throw new ArgumentException("The picture needs a transparent background (RGBA).");
        }
    }
}
