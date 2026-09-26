# Mahjong By Karl

Mahjong solitaire for Windows (WinForms, .NET Framework 4.8) and the web (Blazor, .NET 10). Both versions share the same rules and layouts.

## Projects

- **Mahjong.Core**: the game rules, with no UI. Both versions use it. It covers:
  - the board and the blocking rule;
  - the winnable dealer;
  - scoring, with a breakdown of each bonus;
  - undo/redo;
  - game records and replay;
  - shared tile geometry;
  - the layout files.
- **Mahjong.Core.Tests**: tests for the rules. Run them with `dotnet test Mahjong.Core.Tests`.
- **MahjongSpriteVersion**: the desktop game window. It handles drawing, clicks, sound and high scores.
- **web/Mahjong.Web**: the web server. It provides accounts (email, Google, Microsoft, Facebook), the API that verifies games, leaderboards and player stats. It runs on Azure App Service with Azure SQL.
- **web/Mahjong.Web.Client**: the game in the browser (Blazor WebAssembly). It provides the board, score panel, tile sets, sound and ad slots.
- **web/Mahjong.Web.Tests**: integration tests for the web API.

See [docs/deploy.md](docs/deploy.md) for running the web version locally and for deploying to Azure (CI/CD with GitHub Actions).

Picking the project back up? [docs/project-notes.md](docs/project-notes.md) records what has been built, the key decisions, the current status, and what's left to do.

## Adding a layout

Create a `.layout` text file:

```
name: My Layout
# x y z, one tile per line
0 1 0
2 1 0
1 1 1
```

- `x` and `y` are in half-tile units. A tile is 2 wide and 2 tall, so `2 1 0` sits right next to `0 1 0`, and odd values offset a tile by half a tile.
- `z` is the layer; 0 is the table.
- A layout needs an even number of tiles, and no two tiles on the same layer may overlap.
- To keep a layout out of the menu (like `Test`, which is kept for debugging), add a `hidden: true` line. Hidden layouts are still checked by the tests. To play them, start the game with `MahjongByKarl.exe --show-hidden-layouts`.

To include the layout in the game, put the file in `Mahjong.Core/Layouts/` and rebuild. To try it without rebuilding, put it in a `Layouts` folder next to `MahjongByKarl.exe`. Either way, it shows up in the layout dropdown.

The `EveryDealCanBeCleared` test checks every built-in layout automatically.

The shape layouts (Pyramid, Fortress, Cat and so on) are drawn as text grids in `tools/generate_layouts.py`, one grid per layer. Editing a grid and running `python tools/generate_layouts.py` is often easier than typing coordinates by hand.

### The layout's picture (web)

The web settings dialog shows a 300×223 picture of each layout, from `web/Mahjong.Web.Client/wwwroot/layouts/previews/<slug>.png` (the name in lower case with hyphens for spaces: `Standard Turtle` is `standard-turtle.png`). A layout without one shows a placeholder. To draw the pictures (Classic tiles over the Dragon Valley background, the layout centered), run this on Windows:

```
dotnet run --project tools/LayoutPreviews                        # layouts that don't have a picture yet
dotnet run --project tools/LayoutPreviews -- "My Layout"         # just these (hidden layouts too)
dotnet run --project tools/LayoutPreviews -- --all               # redraw every visible layout
dotnet run --project tools/LayoutPreviews -- --copy-to <folder>  # also save "<Layout name>.png" there
```

You can also make a picture yourself: any 300×223 PNG with the right name works.

## Adding a tile set or background (web)

- **Tile set:** put one PNG per face (`2bams.png`, `Cdragon.png`, ...) in `web/Mahjong.Web.Client/wwwroot/tilesets/<id>/`, then add a line to `TileSets.All` in `web/Mahjong.Web.Client/Game/TileSets.cs`.
- **Background:** put a wide image (about 16:9) in `web/Mahjong.Web.Client/wwwroot/backgrounds/`, then add a line to `Backgrounds.All` in `web/Mahjong.Web.Client/Game/Backgrounds.cs`. SVG, JPG, PNG and WebP all work; for anything but SVG, also set `Url`.

Both show up in the game's menus. Players' choices are saved in their browser, and in their profile when signed in.

The built-in backgrounds are drawn by `tools/generate_backgrounds.py`. Run `python tools/generate_backgrounds.py` to regenerate them after editing it.

## How winnable deals work

The dealer removes random pairs of free positions from the empty layout until the layout is empty. Each removed pair then gets the same face, so replaying that order clears the board. Shuffle uses the same method on the tiles that are left.

With **Guaranteed winnable path** switched off (web settings), tiles are dealt at random instead (`RandomDealer`), so a deal may have no solution.

## Connect (Shisen-Sho)

The web version's second game, chosen under **Game** in the settings dialog. Tiles lie flat in a grid; two matching tiles can be removed if a line with at most two turns joins them through empty cells, including around the outside of the board.

- **Boards:** Small (12×6), Medium (14×8) and Classic (18×8), each with **Gravity** off, down or left (tiles slide to close the gaps). Each combination is its own "layout", named like `Connect Small (Gravity Down)`, so it has its own leaderboard. They're defined in `Mahjong.Core/ConnectRules.cs` (`ConnectLayouts`) and found with `LayoutCatalog.Find` alongside the solitaire layouts; `LayoutCatalog.All` and `Visible` stay solitaire-only, so the desktop game doesn't show them.
- **Rules:** `ConnectRules.FindPath` finds the line (the web board draws it briefly), and `ConnectRules.Settle` applies gravity; gravity moves are undoable.
- **Winnable deals:** `ConnectDealer` works like the solitaire dealer: it removes random joinable pairs from the faceless board (applying gravity) until it's empty, then gives each pair the same face.
- **Hints** (both games) cost 500 of the no-shuffle bonus; undo doesn't give it back. Hints are recorded, so the server's replay check sees them.
- **Pictures:** one per board size (`connect-small.png` and so on), drawn by `tools/LayoutPreviews`.
