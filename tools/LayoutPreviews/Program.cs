using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using Mahjong.Core;

// Renders the 300x223 layout pictures for the web settings dialog, drawn like the web board:
// Classic tiles in the shared draw order over the Dragon Valley background, with the whole layout
// fitted and centered in the frame.
//
//   dotnet run --project tools/LayoutPreviews                     layouts without a picture yet
//   dotnet run --project tools/LayoutPreviews -- "My Layout" Cat  just these (hidden ones too)
//   dotnet run --project tools/LayoutPreviews -- --all            every visible layout, replacing pictures
//   ... -- --copy-to "E:\Art\LayoutPreviews"                      also save "<Layout name>.png" there
//
// Pictures go to web/Mahjong.Web.Client/wwwroot/layouts/previews/<slug>.png.

const int Width = 300, Height = 223;
const int Oversample = 4;     // drawn 4x larger, then scaled down for smooth edges
const int ImageHeight = 115;  // Board.razor draws the 66x86 tile PNGs at 88x115
const int Margin = 8;         // Board.razor's viewBox margin around the layout
const long Seed = 2026;       // any fixed seed, so a picture comes out the same every time

bool all = false;
string? copyTo = null;
var names = new List<string>();
for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--all":
            all = true;
            break;
        case "--copy-to" when i + 1 < args.Length:
            copyTo = args[++i];
            break;
        case "--help" or "-h":
            Console.WriteLine("Usage: dotnet run --project tools/LayoutPreviews -- [--all] [--copy-to <folder>] [layout name ...]");
            return 0;
        default:
            names.Add(args[i]);
            break;
    }
}

string repo = FindRepo();
string web = Path.Combine(repo, "web", "Mahjong.Web.Client", "wwwroot");
string outFolder = Path.Combine(web, "layouts", "previews");
Directory.CreateDirectory(outFolder);
if (copyTo != null)
{
    Directory.CreateDirectory(copyTo);
}

var unknown = names.Where(n => LayoutCatalog.Find(n) == null).ToList();
if (unknown.Count > 0)
{
    Console.Error.WriteLine($"Unknown layout(s): {string.Join(", ", unknown)}. Is the .layout file in Mahjong.Core/Layouts/?");
    return 1;
}

var layouts = names.Count > 0 ? names.Select(n => LayoutCatalog.Find(n)!).ToList()
    : all ? LayoutCatalog.Visible.ToList()
    : LayoutCatalog.Visible.Where(l => !File.Exists(Path.Combine(outFolder, Slug(l.Name) + ".png"))).ToList();

if (layouts.Count == 0)
{
    Console.WriteLine("Every layout already has a picture. Name layouts, or use --all, to redraw them.");
    return 0;
}

using var background = Image.FromFile(Path.Combine(web, "backgrounds", "dragon-valley.jpg"));
var tileImages = new Dictionary<string, Image>();
foreach (var layout in layouts)
{
    using var preview = Render(layout);
    string file = Path.Combine(outFolder, Slug(layout.Name) + ".png");
    preview.Save(file, ImageFormat.Png);
    if (copyTo != null)
    {
        preview.Save(Path.Combine(copyTo, layout.Name + ".png"), ImageFormat.Png);
    }

    Console.WriteLine($"{layout.Name} -> {Path.GetRelativePath(repo, file)}");
}

foreach (var image in tileImages.Values)
{
    image.Dispose();
}

return 0;

Bitmap Render(LayoutDefinition layout)
{
    // The board at full size, as Board.razor lays it out.
    var game = new MahjongGame(layout, Seed);
    var bounds = BoardGeometry.Bounds(layout.Positions);
    int boardW = bounds.Width + 2 * Margin, boardH = bounds.Height + 2 * Margin;

    using var board = new Bitmap(boardW, boardH, PixelFormat.Format32bppArgb);
    using (var g = HighQuality(Graphics.FromImage(board)))
    {
        foreach (var tile in BoardGeometry.InDrawOrder(game.Board.Tiles))
        {
            var b = BoardGeometry.Bounds(tile.Position);
            g.DrawImage(TileImage(tile.Face.Name), new Rectangle(b.Left - bounds.Left + Margin, b.Top - bounds.Top + Margin, BoardGeometry.TileWidth, ImageHeight));
        }
    }

    // The frame: the background covering it, the board fitted inside and centered both ways.
    using var frame = new Bitmap(Width * Oversample, Height * Oversample, PixelFormat.Format32bppArgb);
    using (var g = HighQuality(Graphics.FromImage(frame)))
    {
        g.DrawImage(background, Centered(frame.Size, background.Size, Math.Max((double)frame.Width / background.Width, (double)frame.Height / background.Height)));
        g.DrawImage(board, Centered(frame.Size, board.Size, Math.Min((double)frame.Width / boardW, (double)frame.Height / boardH)));
    }

    var preview = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
    using (var g = HighQuality(Graphics.FromImage(preview)))
    {
        g.DrawImage(frame, new Rectangle(0, 0, Width, Height));
    }

    return preview;
}

Image TileImage(string face)
{
    if (!tileImages.TryGetValue(face, out var image))
    {
        tileImages[face] = image = Image.FromFile(Path.Combine(web, "tilesets", "classic", face + ".png"));
    }

    return image;
}

static Rectangle Centered(Size frame, Size content, double scale)
{
    int w = (int)Math.Round(content.Width * scale), h = (int)Math.Round(content.Height * scale);
    return new Rectangle((frame.Width - w) / 2, (frame.Height - h) / 2, w, h);
}

static Graphics HighQuality(Graphics g)
{
    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
    g.SmoothingMode = SmoothingMode.HighQuality;
    return g;
}

// Must match LayoutPreviews.Slug in web/Mahjong.Web.Client/Game/LayoutPreviews.cs.
static string Slug(string layoutName) => Regex.Replace(layoutName.ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');

// The repository root: the first folder above the current one (or this program) with Mahjong.Core in it.
static string FindRepo()
{
    foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
    {
        for (var dir = new DirectoryInfo(start); dir != null; dir = dir.Parent)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, "Mahjong.Core")) && Directory.Exists(Path.Combine(dir.FullName, "web")))
            {
                return dir.FullName;
            }
        }
    }

    throw new DirectoryNotFoundException("Run this from inside the mahjong repository.");
}
