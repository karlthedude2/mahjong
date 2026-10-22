# Live Preview Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** The settings card shows a live, read-only board drawn with every setting. The layout and board pickers show blank-stone 3D pictures made by the game's own renderer.

**Architecture:**
- `board3d.js` and `cube.js` take an `options` argument (`readOnly`, `pixelRatio`) and `board3d.js` gains `snapshot()`. `Board3D` and `CubeBoard` pass `ReadOnly` through.
- A `BoardPreview` component picks `Board`, `Board3D` or `CubeBoard` from the dialog's choices.
- A development-only page and endpoint render each layout and save its PNG. The save logic lives in a testable `LayoutPreviewFiles` class in Mahjong.Web.

**Tech Stack:** Blazor WebAssembly (.NET 10), three.js r180, xUnit.

**Spec:** `docs/superpowers/specs/2026-10-22-live-preview-design.md`

## Global Constraints

- Pictures: 400×300 RGBA PNG, transparent background, blank Ivory stones, at the 3D board's starting angle. Saved to `web/Mahjong.Web.Client/wwwroot/layouts/previews/<slug>.png`, plus an optional copy `<Layout name>.png` in `LayoutPreviews:CopyTo`.
- The dev endpoint is mapped only in Development, writes only for known layouts, and only into those two folders.
- `SettingsChoice`, preferences and Play.razor's apply logic are unchanged.
- Start game stays visible without scrolling at 375×667.
- Stage files by name; the user's tile/logo experiments stay unstaged.

## Review Focus

1. The live preview never stacks WebGL views: switching games or layouts repeatedly disposes the old one (Task 4, checked in Task 5 by counting canvases).
2. The preview never takes clicks or drags, and a tap on the card still opens the picker (Task 4; Task 5 check).
3. Picture saving refuses an unknown slug, a non-PNG, a wrong size and a path in the name (Task 2 tests).
4. Every visible layout and Connect board has a 400×300 RGBA picture (Task 3 test).
5. The preview redraws on each setting (layout, tile set, stone, face set, background, cube size) without flicker that hides Start (Task 5).

---

### Task 1: Read-only and snapshot modes for the 3D renderers

**Files:** `wwwroot/js/board3d.js`, `wwwroot/js/cube.js`, `wwwroot/js/stone.js` (`createStage(host, fov, pixelRatio?)`), `Components/Board3D.razor`, `Components/CubeBoard.razor`.

**Produces:**
- `board3d.create(host, dotnet, options)` and `cube.create(host, dotnet, options)`, where options is `{ readOnly: bool, pixelRatio?: number }`. With `readOnly`: no OrbitControls input (`controls.enabled = false`), no pointer, wheel or touch listeners, and tiles appear at full size (no grow-in).
- `board3d` `snapshot()`: renders now and returns `canvas.toDataURL("image/png")`.
- `[Parameter] bool ReadOnly` on Board3D and CubeBoard, passed into `create`. Board3D also gets `[Parameter] bool Blank` (faces with no art and empty glyphs) and `[Parameter] double? PixelRatio`, plus `public ValueTask<string> SnapshotAsync()`.

- [ ] Implement. Defaults keep today's behaviour (no options → interactive).
- [ ] `dotnet build web/Mahjong.Web -c Release`: 0 errors. Commit. (This is JS behaviour, checked in the browser in Task 3/5.)

### Task 2: Saving pictures (endpoint and its rules)

**Files:** Create `web/Mahjong.Web/Api/LayoutPreviewFiles.cs` and `web/Mahjong.Web.Tests/LayoutPreviewFilesTests.cs`. Modify `Api/DevPictureEndpoints.cs` (map `POST /dev/layout-previews/{slug}`, body: the PNG bytes) and `appsettings.Development.json` (`"LayoutPreviews": { "CopyTo": "" }`). Test the mapping in `DevPictureTests`.

**Produces:**
- `LayoutPreviewFiles.Save(string slug, byte[] png, string previewsFolder, string? copyTo) → IReadOnlyList<string>` (the paths written). It throws `ArgumentException` for a slug that isn't a known layout's (`LayoutCatalog.All` + `ConnectLayouts.BaseBoards`, by `LayoutPreviews.Slug(PreviewName)`), for bytes that aren't a PNG, and for a picture that isn't 400×300 RGBA (read from the IHDR). The copy is named after the layout's `PreviewName`.
- The endpoint returns 200 with the paths, or 400 with the reason.

- [ ] Tests first:
  - `SavesAKnownLayout` (writes `<slug>.png`; with copyTo, also writes `Standard Turtle.png`).
  - `RefusesUnknownSlugs` (`"../x"`, `"nope"`).
  - `RefusesNonPng`, `RefusesWrongSize` (a generated 300×223 RGBA PNG), `RefusesRgb` (colour type 2).
  - `DevPictureTests` adds `POST /dev/layout-previews/arena` → 404 outside Development.
  - Use temp folders.
- [ ] Run: FAIL. Implement. Run: PASS. Commit.

### Task 3: The dev page, the pictures, retiring the old tool

**Files:** Create `web/Mahjong.Web.Client/Pages/DevLayoutPreviews.razor` (`@page "/dev/layout-previews"`, not linked; WebAssembly render mode). Delete `tools/LayoutPreviews/`. Update README "The layout's picture". Regenerate `wwwroot/layouts/previews/*.png`. Test: `LayoutPreviewTests`.

- [ ] Tests first in `LayoutPreviewTests`:
  - `EveryVisibleLayoutAndBoardHasAPicture` (`LayoutCatalog.Visible` + `ConnectLayouts.BaseBoards`).
  - `PicturesAre400By300` (IHDR width/height).
  - Run: FAIL (today's are 300×223).
- [ ] Page: buttons "Redraw all" and "Missing only". For each layout: render a 400×300 `Board3D` (ReadOnly, Blank, PixelRatio 1, stone Ivory, a seeded game), wait for two animation frames after `show`, `SnapshotAsync`, POST the bytes, and list the saved paths or the error. It covers hidden layouts too. When the endpoint is missing (404), it says "Only in development".
- [ ] Run it on the 5031 test copy in the browser. Check a few pictures by eye. Run the tests: PASS. Delete the tool, update the README. Commit the page, the pictures, the README and the deletion.

### Task 4: Live preview in the big card, and the Cube switch

**Files:** Create `Components/BoardPreview.razor` (+ `.razor.css`). Modify `Components/SettingsDialog.razor` and `.razor.css`.

**Produces:**
- `BoardPreview`, with parameters `LayoutDefinition Layout`, `TileSetInfo TileSet` (flat set or stone), `FaceSetInfo FaceSet`, `bool ThreeD`.
- It deals `new MahjongGame(Layout, PreviewSeed)` (a constant), memoized per layout.
- It renders `Board` (no click handler; `pointer-events: none`), `Board3D` (ReadOnly) or `CubeBoard` (ReadOnly, Stone).
- It's keyed on (layout, face set) so a change replaces the one view. The 3D views fade in.

- [ ] In the dialog, the big card's layout `<img>` becomes `<BoardPreview>` over the background `<img>`. The card stays a `<button>` (the preview is inert inside it).
- [ ] For Cube: the preview fills the card. `Segmented` gets the class `compact`, positioned at lower right (chip-height about 30px, 13px text), and the "BOARD SIZE" chip stays at lower left.
- [ ] Run the full web suite: PASS. Commit.

### Task 5: Browser check, review

- [ ] On the 5031 test copy at 375×667, in English and Spanish:
  - For each game, change each setting and see the preview redraw.
  - Count `canvas` elements in the dialog (never more than 1 for the preview).
  - A tap on the card opens the picker (not Cube).
  - The Cube switch works from the corner.
  - Start game stays visible.
  - The picker shows the new pictures over the background.
- [ ] Desktop check. Fresh reviewer on the most capable model; fix Critical/Important with a test or a browser repro first. Commit.
