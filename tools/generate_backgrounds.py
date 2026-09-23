"""Generates the web game's SVG backgrounds into web/Mahjong.Web.Client/wwwroot/backgrounds.

Run: python tools/generate_backgrounds.py

"Dragon Mountains" is an original scene: a golden sky, misty stone spires, cliff-top temples and a
pagoda, and a green dragon winding through the clouds. "Jade Silk" is a calm, low-contrast option.
"""
import math
import os
import random

OUT = os.path.join(os.path.dirname(__file__), "..", "web", "Mahjong.Web.Client", "wwwroot", "backgrounds")
W, H = 1920, 1080


def f(v):
    return f"{v:.1f}".rstrip("0").rstrip(".")


def pts(points):
    return " ".join(f"{f(x)},{f(y)}" for x, y in points)


# ---------------------------------------------------------------- scenery helpers

def spires(rng, x0, x1, base, min_h, max_h, width_range, fill, opacity=1.0, blur=None):
    """A ridge of tall, narrow karst peaks, smoothed with quadratic curves."""
    d = [f"M {f(x0)} {f(H)} L {f(x0)} {f(base)}"]
    x = x0
    while x < x1:
        w = rng.uniform(*width_range)
        h = rng.uniform(min_h, max_h)
        top_x = x + w * rng.uniform(0.35, 0.65)
        d.append(f"Q {f(x + w * 0.15)} {f(base - h * 0.55)} {f(top_x - w * 0.12)} {f(base - h)}")
        d.append(f"Q {f(top_x)} {f(base - h - w * 0.12)} {f(top_x + w * 0.12)} {f(base - h)}")
        d.append(f"Q {f(x + w * 0.85)} {f(base - h * 0.55)} {f(x + w)} {f(base - rng.uniform(0, 30))}")
        x += w
    d.append(f"L {f(x1)} {f(H)} Z")
    filt = f' filter="url(#{blur})"' if blur else ""
    return f'<path d="{" ".join(d)}" fill="{fill}" opacity="{opacity}"{filt}/>'


def rock(rng, cx, top, bottom, top_w, bottom_w, fill, jag=10):
    """A cliff: a jagged column from a flat-ish top down to the bottom of the scene."""
    left, right = [], []
    steps = 14
    for i in range(steps + 1):
        t = i / steps
        y = top + (bottom - top) * t
        half = (top_w + (bottom_w - top_w) * (t ** 1.4)) / 2
        left.append((cx - half - rng.uniform(0, jag), y))
        right.append((cx + half + rng.uniform(0, jag), y))
    outline = left + list(reversed(right))
    strata = []
    for i in range(1, 7):
        y = top + (bottom - top) * i / 7
        half = (top_w + (bottom_w - top_w) * ((i / 7) ** 1.4)) / 2
        strata.append(f'<path d="M {f(cx - half + 8)} {f(y)} q {f(half * 0.5)} {f(rng.uniform(-6, 6))} {f(half * 0.9)} {f(rng.uniform(-4, 8))}" '
                      f'stroke="#2a1a12" stroke-width="2" opacity="0.25" fill="none"/>')
    return f'<polygon points="{pts(outline)}" fill="{fill}"/>' + "".join(strata)


def roof(cx, y, w, rh, over, fill="#2f211c", edge="#c9a24a"):
    """A sweeping roof with upturned eaves; y is the eave line."""
    x0, x1 = cx - w / 2 - over, cx + w / 2 + over
    d = (f"M {f(x0 - 10)} {f(y - 18)} Q {f(x0 + over * 0.5)} {f(y + 3)} {f(cx - w * 0.32)} {f(y - rh)} "
         f"L {f(cx + w * 0.32)} {f(y - rh)} Q {f(x1 - over * 0.5)} {f(y + 3)} {f(x1 + 10)} {f(y - 18)} "
         f"L {f(x1 - 6)} {f(y + 5)} L {f(x0 + 6)} {f(y + 5)} Z")
    return (f'<path d="{d}" fill="{fill}"/>'
            f'<path d="M {f(x0 - 10)} {f(y - 18)} Q {f(x0 + over * 0.5)} {f(y + 3)} {f(cx - w * 0.32)} {f(y - rh)}" stroke="{edge}" stroke-width="2" fill="none"/>'
            f'<path d="M {f(x1 + 10)} {f(y - 18)} Q {f(x1 - over * 0.5)} {f(y + 3)} {f(cx + w * 0.32)} {f(y - rh)}" stroke="{edge}" stroke-width="2" fill="none"/>')


def hall(cx, y_bottom, w, h):
    """One storey: red pillared walls with lit windows; y_bottom is the floor."""
    parts = [f'<rect x="{f(cx - w / 2)}" y="{f(y_bottom - h)}" width="{f(w)}" height="{f(h)}" fill="#8e2b1d"/>']
    cols = max(3, int(w / 22))
    for i in range(cols):
        x = cx - w / 2 + (i + 0.5) * w / cols
        parts.append(f'<rect x="{f(x - w / cols * 0.28)}" y="{f(y_bottom - h * 0.78)}" width="{f(w / cols * 0.56)}" height="{f(h * 0.55)}" fill="#f3c06b" opacity="0.85"/>')
        parts.append(f'<rect x="{f(x - 1.5)}" y="{f(y_bottom - h * 0.78)}" width="3" height="{f(h * 0.55)}" fill="#5b1a12"/>')
    parts.append(f'<rect x="{f(cx - w / 2 - 6)}" y="{f(y_bottom - 4)}" width="{f(w + 12)}" height="6" fill="#3b2a22"/>')
    return "".join(parts)


def pagoda(cx, base, tiers, bottom_w, tier_h, shrink):
    parts, y, w = [], base, bottom_w
    for _ in range(tiers):
        parts.append(hall(cx, y, w, tier_h))
        parts.append(roof(cx, y - tier_h, w, tier_h * 0.45, w * 0.28))
        y -= tier_h + tier_h * 0.45
        w *= shrink
    parts.append(f'<path d="M {f(cx)} {f(y - 55)} L {f(cx - 4)} {f(y + 4)} L {f(cx + 4)} {f(y + 4)} Z" fill="#c9a24a"/>')
    for k in range(3):
        parts.append(f'<circle cx="{f(cx)}" cy="{f(y - 12 - k * 12)}" r="{f(6 - k)}" fill="#c9a24a"/>')
    return "".join(parts)


def temple(cx, base, w):
    """A two-storey hall with a railing, sitting on a cliff top."""
    parts = [f'<rect x="{f(cx - w / 2 - 20)}" y="{f(base - 10)}" width="{f(w + 40)}" height="12" fill="#4a3428"/>']
    for i in range(int((w + 40) / 14) + 1):
        x = cx - w / 2 - 20 + i * 14
        parts.append(f'<rect x="{f(x)}" y="{f(base - 26)}" width="3" height="16" fill="#6e2418"/>')
    parts.append(f'<rect x="{f(cx - w / 2 - 20)}" y="{f(base - 28)}" width="{f(w + 40)}" height="4" fill="#6e2418"/>')
    parts.append(hall(cx, base - 10, w, 58))
    parts.append(roof(cx, base - 68, w, 34, w * 0.18))
    parts.append(hall(cx, base - 102, w * 0.62, 44))
    parts.append(roof(cx, base - 146, w * 0.62, 38, w * 0.2))
    return "".join(parts)


def pine(x, base, h):
    """A windswept pine: a thin trunk and flat, layered boughs."""
    parts = [f'<path d="M {f(x)} {f(base)} q {f(-4)} {f(-h * 0.5)} {f(6)} {f(-h)}" stroke="#3a2a1e" stroke-width="{f(h * 0.07)}" fill="none"/>']
    for i, (dy, w) in enumerate([(0.95, 0.55), (0.72, 0.8), (0.5, 0.65), (0.3, 0.45)]):
        cy = base - h * dy
        shift = 6 if i % 2 else -4
        parts.append(f'<ellipse cx="{f(x + shift)}" cy="{f(cy)}" rx="{f(h * w * 0.5)}" ry="{f(h * 0.09)}" fill="#2f4a2c"/>')
        parts.append(f'<ellipse cx="{f(x + shift - 3)}" cy="{f(cy - 3)}" rx="{f(h * w * 0.38)}" ry="{f(h * 0.05)}" fill="#46683c"/>')
    return "".join(parts)


def cloud(cx, cy, s, opacity=0.9):
    """A soft stylised cloud: overlapping puffs plus a curl."""
    puffs = [(-60, 10, 42), (-20, -10, 52), (30, -4, 48), (70, 12, 36), (0, 20, 50)]
    parts = [f'<g opacity="{opacity}" transform="translate({f(cx)} {f(cy)}) scale({f(s)})">']
    for dx, dy, r in puffs:
        parts.append(f'<ellipse cx="{dx}" cy="{dy}" rx="{r * 1.35}" ry="{r * 0.8}" fill="url(#cloudFill)"/>')
    parts.append('<path d="M -40 8 c -10 -22 24 -30 30 -10 c 4 14 -14 18 -18 8" stroke="#e7cfae" stroke-width="4" fill="none"/>')
    parts.append('<path d="M 30 10 c -6 -18 20 -24 24 -8 c 3 11 -11 14 -14 6" stroke="#e7cfae" stroke-width="4" fill="none"/>')
    parts.append("</g>")
    return "".join(parts)


# ---------------------------------------------------------------- the dragon

DRAGON_PATH = ("M 620 330 C 760 200, 930 250, 1030 360 S 1240 520, 1380 400 "
               "S 1560 170, 1700 300 S 1760 560, 1560 600 S 1180 560, 1080 700")


def dragon():
    body = [
        # Belly and back colours come from two strokes of the same path.
        f'<path d="{DRAGON_PATH}" stroke="#2e5130" stroke-width="86" fill="none" stroke-linecap="round"/>',
        f'<path d="{DRAGON_PATH}" stroke="url(#dragonBody)" stroke-width="72" fill="none" stroke-linecap="round"/>',
        # Scale bands along the body.
        f'<path d="{DRAGON_PATH}" stroke="#a7c47a" stroke-width="72" fill="none" stroke-dasharray="4 16" opacity="0.3"/>',
        f'<path d="{DRAGON_PATH}" stroke="#1f3a22" stroke-width="72" fill="none" stroke-dasharray="2 18" stroke-dashoffset="9" opacity="0.35"/>',
        # Pale belly stripe.
        f'<path d="{DRAGON_PATH}" stroke="#e6d49a" stroke-width="16" fill="none" transform="translate(0 20)" stroke-dasharray="14 6" opacity="0.8"/>',
        # Flame-coloured spines along the back.
        f'<path d="{DRAGON_PATH}" stroke="#c8502a" stroke-width="20" fill="none" transform="translate(0 -40)" stroke-dasharray="10 22" stroke-linecap="round"/>',
    ]

    # The head faces left, drawn at the origin and placed at the start of the body.
    head = [
        '<g transform="translate(610 320) rotate(-18) scale(1.45)">',
        # Mane.
        '<path d="M 4 -34 Q 26 -58 30 -36 Q 48 -52 50 -24 Q 70 -30 60 -6 Q 80 4 56 16 Q 66 36 38 28 Q 34 46 18 30 Z" fill="#c8502a"/>',
        '<path d="M 14 -24 Q 30 -36 32 -20 Q 44 -26 42 -8 Q 54 0 38 10 Q 40 22 24 18 Z" fill="#e0823e"/>',
        # Horns.
        '<path d="M -10 -32 C 10 -70, 40 -90, 70 -96" stroke="#e1c98f" stroke-width="9" fill="none" stroke-linecap="round"/>',
        '<path d="M 4 -26 C 30 -56, 58 -66, 86 -64" stroke="#cdb277" stroke-width="7" fill="none" stroke-linecap="round"/>',
        # Mouth, lower jaw, upper jaw.
        '<path d="M -8 14 C -50 24, -90 34, -118 30 L -116 8 C -80 10, -40 6, -4 0 Z" fill="#8f1d1d"/>',
        '<path d="M -6 16 C -46 36, -86 52, -114 46 L -112 32 C -84 36, -46 28, -8 10 Z" fill="#355f35"/>',
        '<path d="M 6 -34 C -38 -42, -92 -30, -124 -8 L -128 6 C -92 8, -44 12, 8 22 Z" fill="url(#dragonHead)"/>',
        # Teeth.
        '<path d="M -112 6 l 4 10 l 4 -10 M -96 7 l 4 9 l 4 -9 M -80 8 l 3 8 l 3 -8 M -104 32 l 4 -9 l 4 9 M -88 34 l 3 -8 l 3 8" fill="#fbf4dc" stroke="#fbf4dc" stroke-width="1"/>',
        # Brow, eye, nostril.
        '<path d="M -40 -30 C -56 -40, -74 -36, -84 -26" stroke="#1f3a22" stroke-width="5" fill="none" stroke-linecap="round"/>',
        '<ellipse cx="-58" cy="-18" rx="11" ry="7" fill="#f6d34a"/>',
        '<ellipse cx="-58" cy="-18" rx="3" ry="6" fill="#1a1a1a"/>',
        '<circle cx="-116" cy="-8" r="3" fill="#1f3a22"/>',
        # Whiskers.
        '<path d="M -120 -6 C -170 -30, -150 -90, -200 -110" stroke="#f4dc92" stroke-width="4.5" fill="none" stroke-linecap="round"/>',
        '<path d="M -114 4 C -170 30, -170 80, -230 90" stroke="#f4dc92" stroke-width="4.5" fill="none" stroke-linecap="round"/>',
        # Beard.
        '<path d="M -50 40 Q -62 60 -58 78 Q -46 62 -40 70 Q -36 56 -26 64 Q -28 46 -20 36 Z" fill="#c8502a"/>',
        "</g>",
    ]

    # The tail ends in a flame tuft.
    tail = ('<g transform="translate(1080 700) rotate(125)">'
            '<path d="M 0 0 C 20 -20, 50 -10, 70 -40 C 60 -5, 90 0, 100 -20 C 95 20, 50 30, 0 20 Z" fill="#c8502a"/>'
            '</g>')
    return "".join(body) + "".join(head) + tail


# ---------------------------------------------------------------- scenes

def dragon_mountains():
    rng = random.Random(7)
    defs = f'''
  <defs>
    <linearGradient id="sky" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0" stop-color="#f6e2b0"/>
      <stop offset="0.45" stop-color="#f2b86a"/>
      <stop offset="0.8" stop-color="#e08a4e"/>
      <stop offset="1" stop-color="#b8603a"/>
    </linearGradient>
    <radialGradient id="sun" cx="0.62" cy="0.3" r="0.45">
      <stop offset="0" stop-color="#fffbe8" stop-opacity="1"/>
      <stop offset="0.25" stop-color="#ffe7a8" stop-opacity="0.8"/>
      <stop offset="1" stop-color="#ffd27a" stop-opacity="0"/>
    </radialGradient>
    <linearGradient id="dragonBody" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0" stop-color="#6d9a55"/>
      <stop offset="1" stop-color="#3f6b3a"/>
    </linearGradient>
    <linearGradient id="dragonHead" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0" stop-color="#7fae60"/>
      <stop offset="1" stop-color="#3f6b3a"/>
    </linearGradient>
    <radialGradient id="cloudFill" cx="0.5" cy="0.35" r="0.7">
      <stop offset="0" stop-color="#fffaf0"/>
      <stop offset="1" stop-color="#f1dcc0"/>
    </radialGradient>
    <linearGradient id="rockNear" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0" stop-color="#6b4a36"/>
      <stop offset="1" stop-color="#2c1c14"/>
    </linearGradient>
    <linearGradient id="mist" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0" stop-color="#fff4e0" stop-opacity="0"/>
      <stop offset="1" stop-color="#fff4e0" stop-opacity="0.85"/>
    </linearGradient>
    <radialGradient id="vignette" cx="0.5" cy="0.5" r="0.75">
      <stop offset="0.6" stop-color="#000" stop-opacity="0"/>
      <stop offset="1" stop-color="#1a0d06" stop-opacity="0.55"/>
    </radialGradient>
    <filter id="soft"><feGaussianBlur stdDeviation="3"/></filter>
    <filter id="haze"><feGaussianBlur stdDeviation="8"/></filter>
  </defs>'''

    rays = "".join(
        f'<path d="M 1190 320 L {f(1190 + math.cos(a) * 1600)} {f(320 + math.sin(a) * 1600)} '
        f'L {f(1190 + math.cos(a + 0.05) * 1600)} {f(320 + math.sin(a + 0.05) * 1600)} Z" fill="#fff6d8" opacity="0.12" filter="url(#haze)"/>'
        for a in [1.9, 2.3, 2.75, 0.5, 0.95])

    layers = [
        f'<rect width="{W}" height="{H}" fill="url(#sky)"/>',
        f'<rect width="{W}" height="{H}" fill="url(#sun)"/>',
        rays,
        spires(rng, -40, W + 40, 820, 180, 420, (70, 150), "#c9956f", 0.55, "haze"),
        spires(rng, -40, W + 40, 900, 140, 330, (90, 170), "#a36d4f", 0.7, "soft"),
        spires(rng, -40, W + 40, 980, 120, 260, (110, 190), "#8a5540", 0.8),
        f'<rect y="560" width="{W}" height="{H - 560}" fill="url(#mist)"/>',
        cloud(1500, 250, 1.3, 0.8),
        dragon(),
        # Clouds in front of the body make it weave through the sky.
        cloud(1240, 470, 1.5),
        cloud(1720, 520, 1.2),
        cloud(820, 250, 0.9, 0.85),
        # Right: a tall cliff with a pagoda, and a lower rock with a small shrine.
        rock(rng, 1640, 520, H + 20, 190, 330, "url(#rockNear)"),
        pagoda(1640, 522, 5, 150, 40, 0.8),
        rock(rng, 860, 820, H + 20, 150, 260, "url(#rockNear)"),
        temple(860, 822, 110),
        pine(780, 820, 70),
        # Left: a broad cliff with a two-storey temple.
        rock(rng, 330, 600, H + 20, 330, 470, "url(#rockNear)"),
        temple(330, 602, 230),
        pine(190, 600, 110),
        pine(470, 600, 85),
        pine(1560, 522, 80),
        # Foreground mist, then darkened edges to frame the board.
        cloud(560, 930, 2.2, 0.75),
        cloud(1500, 1000, 2.6, 0.7),
        f'<rect y="880" width="{W}" height="200" fill="url(#mist)"/>',
        f'<rect width="{W}" height="{H}" fill="url(#vignette)"/>',
    ]
    return svg(defs, layers)


def jade_silk():
    defs = '''
  <defs>
    <radialGradient id="jade" cx="0.5" cy="0.4" r="0.8">
      <stop offset="0" stop-color="#2f7d67"/>
      <stop offset="0.6" stop-color="#1b5a4a"/>
      <stop offset="1" stop-color="#0d3129"/>
    </radialGradient>
    <pattern id="lattice" width="60" height="60" patternUnits="userSpaceOnUse">
      <path d="M 30 0 L 60 30 L 30 60 L 0 30 Z" fill="none" stroke="#9fd8c2" stroke-width="1" opacity="0.12"/>
      <circle cx="30" cy="30" r="3" fill="#9fd8c2" opacity="0.1"/>
    </pattern>
  </defs>'''
    return svg(defs, [f'<rect width="{W}" height="{H}" fill="url(#jade)"/>', f'<rect width="{W}" height="{H}" fill="url(#lattice)"/>'])


def svg(defs, layers):
    return (f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {W} {H}" preserveAspectRatio="xMidYMid slice">'
            f"{defs}\n  " + "\n  ".join(layers) + "\n</svg>\n")


if __name__ == "__main__":
    os.makedirs(OUT, exist_ok=True)
    for name, build in [("dragon-mountains", dragon_mountains), ("jade-silk", jade_silk)]:
        path = os.path.join(OUT, name + ".svg")
        with open(path, "w", encoding="utf-8", newline="\n") as fh:
            fh.write(build())
        print(f"{name}.svg  {os.path.getsize(path) / 1024:.1f} KB")
