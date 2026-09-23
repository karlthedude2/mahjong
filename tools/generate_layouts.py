"""Generates the shape-based .layout files in Mahjong.Core/Layouts from per-layer text masks.

Run: python tools/generate_layouts.py   (rewrites only the layouts defined below)

Each layer is (z, dx, dy, rows): '#' is a tile, '.' is empty. Column c becomes x = 2c + dx and
row r becomes y = 2r + dy, so dx/dy = 1 shifts a whole layer by half a tile.
"""
import os
import sys

OUT = sys.argv[1] if len(sys.argv) > 1 else os.path.join(os.path.dirname(__file__), "..", "Mahjong.Core", "Layouts")


def mask(z, rows, dx=0, dy=0):
    out = []
    for r, line in enumerate(rows):
        for c, ch in enumerate(line):
            if ch == "#":
                out.append((2 * c + dx, 2 * r + dy, z))
    return out


def rect(z, col0, row0, w, h):
    """w x h block whose top-left tile is at (col0, row0); halves allowed."""
    return [(int(2 * (col0 + i)), int(2 * (row0 + j)), z) for j in range(h) for i in range(w)]


LAYOUTS = {}


def layout(name, description, *layers):
    tiles = [t for layer in layers for t in layer]
    LAYOUTS[name] = (description, tiles)


# Pyramid: five centred layers, each shifted half a tile so it sits on four tiles below.
layout("Pyramid", "A stepped pyramid; each layer sits half a tile in from the one below.",
       rect(0, 0, 0, 9, 6), rect(1, 0.5, 0.5, 8, 5), rect(2, 1, 1, 7, 4),
       rect(3, 1.5, 1.5, 6, 3), rect(4, 2.5, 2.5, 4, 1))

# Fortress: a walled square with a four-storey tower at each corner.
wall = [(c, r) for r in range(8) for c in range(12) if r in (0, 7) or c in (0, 11)]
corners = [(0, 0), (11, 0), (0, 7), (11, 7)]
layout("Fortress", "A walled keep with a tall tower at each corner.",
       rect(0, 0, 0, 12, 8),
       [(2 * c, 2 * r, 1) for c, r in wall],
       *[[(2 * c, 2 * r, z) for c, r in corners] for z in (2, 3, 4)])

# Bridge: two piers joined by a deck over the gap, with a cable tower on each pier.
layout("Bridge", "Two piers joined by a deck across the river, with a tower on each pier.",
       rect(0, 0, 0, 4, 8), rect(0, 11, 0, 4, 8),
       rect(1, 0, 1, 4, 6), rect(1, 11, 1, 4, 6),
       rect(2, 0, 3, 15, 2),
       rect(3, 1.5, 3.5, 1, 1), rect(3, 12.5, 3.5, 1, 1))

layout("Cloud", "A billowing cloud built from rounded puffs.",
       mask(0, [
           "..####...####..",
           ".######.######.",
           "###############",
           "###############",
           "###############",
           ".#############.",
           "..###########..",
       ]),
       mask(1, [
           "...............",
           "..####...####..",
           ".#############.",
           ".#############.",
           "..###########..",
       ]),
       mask(2, [
           "...............",
           "...##.....##...",
           ".....######....",
       ], dy=1),
       )

layout("Cat", "A cat's face: pointed ears, eyes, nose and whiskers.",
       mask(0, [
           ".##.........##.",
           ".###.......###.",
           ".#############.",
           "###############",
           ".#############.",
           "###############",
           "..###########..",
           "....#######....",
       ]),
       mask(1, [
           ".#...........#.",
           ".##.........##.",
           "...#########...",
           "...##.###.##...",
           "...#########...",
           "...####.####...",
           "....#######....",
       ]),
       mask(2, [
           ".#...........#.",
           "...............",
           ".....#####.....",
           "...............",
           "....##...##....",
           "......###......",
       ]),
       )

layout("Crab", "A crab with raised claws, a domed shell and legs down each side.",
       mask(0, [
           "##...........##",
           "##...........##",
           ".##.........##.",
           "..###########..",
           "#.###########.#",
           ".#############.",
           "#.###########.#",
           "..#.........#..",
       ]),
       mask(1, [
           "##...........##",
           "##...........##",
           ".##.........##.",
           "...#########...",
           "...#########...",
           "...#########...",
           "...#########...",
       ]),
       rect(2, 3.5, 3.5, 8, 2),
       rect(3, 5.5, 3.5, 4, 1),
       )

layout("Spider", "A spider with a raised body and eight legs.",
       mask(0, [
           "#.............#",
           ".#....###....#.",
           "..#.#######.#..",
           "###.#######.###",
           "###.#######.###",
           "..#.#######.#..",
           ".#....###....#.",
           "#.............#",
       ]),
       mask(1, [
           "...............",
           "......###......",
           ".....#####.....",
           ".....#####.....",
           ".....#####.....",
           ".....#####.....",
           "......###......",
       ]),
       rect(1, 0, 3, 3, 2), rect(1, 12, 3, 3, 2),
       rect(2, 5.5, 2.5, 4, 3),
       rect(3, 6, 3, 3, 2),
       rect(4, 6.5, 3.5, 2, 1),
       )

layout("Butterfly", "A butterfly with layered wings and a tall body.",
       mask(0, [
           "####.......####",
           "#####.....#####",
           "######.#.######",
           ".######.######.",
           ".######.######.",
           "######.#.######",
           "#####.....#####",
           "####.......####",
       ]),
       mask(1, [
           "...............",
           ".###.......###.",
           ".####.....####.",
           "..###.....###..",
           "..###.....###..",
           ".####.....####.",
           ".###.......###.",
       ]),
       rect(1, 7, 1, 1, 6),
       rect(2, 1.5, 1.5, 2, 1), rect(2, 11.5, 1.5, 2, 1),
       rect(2, 1.5, 5.5, 2, 1), rect(2, 11.5, 5.5, 2, 1),
       rect(2, 7, 2, 1, 4),
       )

layout("Arena", "An oval stadium: raised stands all round and a stage in the middle.",
       mask(0, [
           "..###########..",
           ".#############.",
           "###############",
           "###############",
           "###############",
           "###############",
           ".#############.",
           "..###########..",
       ]),
       mask(1, [
           "..###########..",
           ".#...........#.",
           "#.............#",
           "#.............#",
           "#.............#",
           "#.............#",
           ".#...........#.",
           "..###########..",
       ]),
       rect(1, 6.5, 3.5, 2, 1),
       )

layout("Tower", "A tall central tower rising from a wide base.",
       rect(0, 2, 1, 11, 6),
       rect(1, 4.5, 1, 6, 6),
       rect(2, 5.5, 1, 4, 6),
       rect(3, 6, 2, 3, 4),
       rect(4, 6.5, 2.5, 2, 3))


for name, (description, tiles) in LAYOUTS.items():
    assert len(set(tiles)) == len(tiles), f"{name}: duplicate positions"
    by_layer = {}
    for t in tiles:
        by_layer[t[2]] = by_layer.get(t[2], 0) + 1
    counts = ", ".join(f"layer {z}: {n}" for z, n in sorted(by_layer.items()))
    print(f"{name:10} {len(tiles):4}  ({counts})")

    lines = [f"name: {name}", f"# {description}", "# x y z  (x and y in half-tile units; a tile is 2 wide and 2 tall)"]
    for z in sorted(by_layer):
        lines += ["", f"# Layer {z}"]
        lines += [f"{x} {y} {zz}" for x, y, zz in sorted(tiles, key=lambda t: (t[1], t[0])) if zz == z]
    with open(os.path.join(OUT, name + ".layout"), "w", newline="\n") as f:
        f.write("\n".join(lines) + "\n")
