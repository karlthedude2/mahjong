# Mahjong By Karl

Mahjong solitaire for Windows (WinForms, .NET Framework 4.8).

## Projects

- **MahjongSpriteVersion**: the game window. It handles drawing, clicks, sound and high scores.
- **Mahjong.Core**: the game rules, with no UI. It covers the board, the blocking rule, the winnable dealer, scoring, undo/redo and the layout files.
- **Mahjong.Core.Tests**: tests for the rules. Run them with `dotnet test Mahjong.Core.Tests`.

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

## How winnable deals work

The dealer removes random pairs of free positions from the empty layout until the layout is empty. Each removed pair then gets the same face, so replaying that order clears the board. Shuffle uses the same method on the tiles that are left.
