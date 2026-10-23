using System.IO.Compression;
using System.Buffers.Binary;
using Mahjong.Web.Api;

namespace Mahjong.Web.Tests;

/// <summary>Saving a layout's picker picture from the development page: only a known layout, only the right kind of picture.</summary>
public sealed class LayoutPreviewFilesTests : IDisposable
{
    private readonly string folder = Directory.CreateTempSubdirectory("previews").FullName;
    private readonly string copies = Directory.CreateTempSubdirectory("preview-copies").FullName;

    public void Dispose()
    {
        Directory.Delete(folder, recursive: true);
        Directory.Delete(copies, recursive: true);
    }

    [Fact]
    public void SavesAKnownLayout()
    {
        var written = LayoutPreviewFiles.Save("standard-turtle", Png(400, 300, colorType: 6), folder, copies);

        Assert.True(File.Exists(Path.Combine(folder, "standard-turtle.png")));
        Assert.True(File.Exists(Path.Combine(copies, "Standard Turtle.png")));
        Assert.Equal(2, written.Count);
    }

    [Fact]
    public void SavesWithoutACopyFolder()
    {
        var written = LayoutPreviewFiles.Save("arena", Png(400, 300, colorType: 6), folder, copyTo: null);

        Assert.Equal([Path.Combine(folder, "arena.png")], written);
    }

    [Theory]
    [InlineData("../x")]
    [InlineData("nope")]
    [InlineData("")]
    public void RefusesUnknownLayouts(string slug) =>
        Assert.Throws<ArgumentException>(() => LayoutPreviewFiles.Save(slug, Png(400, 300, colorType: 6), folder, null));

    [Fact]
    public void RefusesSomethingThatIsntAPng() =>
        Assert.Throws<ArgumentException>(() => LayoutPreviewFiles.Save("arena", [1, 2, 3, 4, 5, 6, 7, 8, 9, 10], folder, null));

    [Fact]
    public void RefusesTheWrongSize() =>
        Assert.Throws<ArgumentException>(() => LayoutPreviewFiles.Save("arena", Png(300, 223, colorType: 6), folder, null));

    [Fact]
    public void RefusesAPictureWithoutTransparency() =>
        Assert.Throws<ArgumentException>(() => LayoutPreviewFiles.Save("arena", Png(400, 300, colorType: 2), folder, null));

    [Fact]
    public void SavesAHowToPlayPicture()
    {
        string path = LayoutPreviewFiles.SaveHelpPicture("mahjong-cube", Png(800, 600, colorType: 6), folder);

        Assert.Equal(Path.Combine(folder, "mahjong-cube.png"), path);
        Assert.True(File.Exists(path));
    }

    [Theory]
    [InlineData("chess", 800, 600)]
    [InlineData("../mahjong-cube", 800, 600)]
    [InlineData("mahjong-3d", 400, 300)]
    public void RefusesAWrongHowToPlayPicture(string name, int width, int height) =>
        Assert.Throws<ArgumentException>(() => LayoutPreviewFiles.SaveHelpPicture(name, Png(width, height, colorType: 6), folder));

    // A minimal valid PNG of the given size: signature, IHDR, one empty-ish IDAT, IEND.
    private static byte[] Png(int width, int height, byte colorType)
    {
        using var stream = new MemoryStream();
        stream.Write([0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A]);
        var header = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header, width);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), height);
        header[8] = 8;
        header[9] = colorType;
        Chunk(stream, "IHDR", header);
        using (var pixels = new MemoryStream())
        {
            using (var zip = new ZLibStream(pixels, CompressionLevel.Fastest, leaveOpen: true))
            {
                zip.Write(new byte[1 + width * (colorType == 6 ? 4 : 3)]);
            }

            Chunk(stream, "IDAT", pixels.ToArray());
        }

        Chunk(stream, "IEND", []);
        return stream.ToArray();
    }

    private static void Chunk(Stream stream, string type, byte[] data)
    {
        var length = new byte[4];
        BinaryPrimitives.WriteInt32BigEndian(length, data.Length);
        stream.Write(length);
        stream.Write(System.Text.Encoding.ASCII.GetBytes(type));
        stream.Write(data);
        stream.Write(new byte[4]); // CRC: not checked by the saver
    }
}
