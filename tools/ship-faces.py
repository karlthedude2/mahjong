"""Ships the face art for Mahjong 3D and the Mahjong Cube.

Reads the faces from cube-faces/ (plain) and cube-faces/dark/ (with the white glow, for dark stones)
and writes them, 512 px tall, as WebP to a face set's folder, web/Mahjong.Web.Client/wwwroot/faces/<set>/
and <set>/dark/, named exactly as the game names each face. Run it after changing the art, then commit
the result (a new set also needs a line in FaceSets.cs):

    python tools/ship-faces.py            # the Standard set (faces/standard)
    python tools/ship-faces.py <set>      # another set (faces/<set>)

Needs Pillow (pip install pillow). Stops, writing nothing, if a face is missing.
"""

import sys
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / "cube-faces"
FACE_SET = sys.argv[1] if len(sys.argv) > 1 else "standard"
TARGET = ROOT / "web" / "Mahjong.Web.Client" / "wwwroot" / "faces" / FACE_SET
HEIGHT = 512
QUALITY = 88

# Every face the game deals (Mahjong.Core TileSet.Standard), as it names them.
FACES = (
    [f"{n}bams" for n in range(2, 10)]
    + [f"{n}dots" for n in range(1, 10)]
    + [f"{n}crack" for n in range(1, 10)]
    + ["Bdragon", "Cdragon", "Fdragon", "peacock", "North", "South", "East", "West"]
    + [f"{n}flower" for n in range(1, 5)]
    + [f"{n}wizard" for n in range(1, 5)]
)


def pictures(folder):
    """The pictures in a folder, by lower-case name (the art's file names vary in case)."""
    return {p.stem.lower(): p for p in folder.iterdir() if p.suffix.lower() in (".png", ".webp", ".jpg", ".jpeg")}


def main():
    sets = {"": pictures(SOURCE), "dark": pictures(SOURCE / "dark")}
    missing = [f"{(name + '/') if name else ''}{face}" for name, found in sets.items() for face in FACES if face.lower() not in found]
    if missing:
        sys.exit("Missing from cube-faces: " + ", ".join(missing))

    for name, found in sets.items():
        folder = TARGET / name
        folder.mkdir(parents=True, exist_ok=True)
        for old in folder.glob("*.webp"):
            old.unlink()
        for face in FACES:
            image = Image.open(found[face.lower()]).convert("RGBA")
            width = round(image.width * HEIGHT / image.height)
            image.resize((width, HEIGHT), Image.LANCZOS).save(folder / f"{face}.webp", quality=QUALITY, method=6)

    size = sum(p.stat().st_size for p in TARGET.rglob("*.webp"))
    print(f"Shipped {len(FACES)} faces, plain and dark, to {TARGET.relative_to(ROOT)} ({size / 1e6:.1f} MB)")


if __name__ == "__main__":
    main()
