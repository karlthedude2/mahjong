# Face sets for Mahjong 3D and the Cube

Date: 2026-10-22
Mockups: [Game Settings Redesign](https://claude.ai/artifact/9rgz3WYJLKk8QFFmNk24ae), artboards "Stone + Face · Mahjong 3D" and "Stone + Face · Mahjong Cube" (approved).

## Why

The stones in Mahjong 3D and the Cube show face art, but today there's only one art set and no way to choose. The settings card for stones also shows blank stones. Players should pick a **face set** the same way they pick a stone, and the card should show the stone and face together.

## What a face set is

- Each set is a folder under `web/Mahjong.Web.Client/wwwroot/faces/<set>/`. It holds the light pictures (`<face>.webp`) and a `dark/` folder with the versions for dark stones (`dark/<face>.webp`).
- Today's art becomes the first set, **Standard**, moved to `faces/standard/` and `faces/standard/dark/`.
- Adding a set means adding a folder plus a line in the client's catalog of face sets, the same pattern as tile sets and backgrounds.
- **The player chooses only the set.** Whether a stone shows the light or dark version stays the server's configuration (`Cube:StoneFaces`); players never choose it.
- The development drop folder (`cube-faces/`, served at `/dev/faces…`) still overrides pictures for whichever set is chosen while trying new art.
- `tools/ship-faces.py` writes to `faces/standard/` by default. An optional set name writes to `faces/<set>/`.

## Settings screen

- For **Mahjong 3D** and **Mahjong Cube**, the Tiles/Stone card becomes one **Stone and Face card**. It's hidden for Mahjong Solitaire and Connect, which keep the Tiles card.
  - The top shows three stones in the chosen stone with faces (red dragon, green dragon, East) from the chosen set. A dark stone shows the set's dark pictures.
  - Mahjong 3D draws the stones tile-shaped (34×46). **Mahjong Cube draws them square** (42×42), as the Cube's pieces are cubes.
  - Below the stones it splits into two halves: **STONE · <name> ▾** on the left and **FACE · <set> ▾** on the right. Each half is a dropdown, and a tap anywhere on a half opens its list.
- The row splits 5:3, so the Stone and Face card is wider than Background (3:2 cut off "Standard" at phone width). Background's name ends in "…" if it doesn't fit.
- Strings: "Face" (es: "Cara") and "Standard" (es: "Estándar").

## Saving and using the choice

- The chosen set is saved in the browser like the stone (`faceSet`), not to the profile. *(My call: it matches how the stone is kept today.)*
- `SettingsChoice` gains `FaceSetId`. Mahjong 3D, the Cube and the `/cube` test page draw with the chosen set.
- Changing the set mid-game redraws the 3D board or Cube with the new art.
- An unknown or retired set id falls back to Standard.

## Not in scope

- Pictures for face sets in a picker sheet. With one set, a dropdown is enough.
- Changing which stones use light or dark pictures.
