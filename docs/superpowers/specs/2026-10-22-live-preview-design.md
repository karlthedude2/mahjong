# Live board preview in settings

Date: 2026-10-22
Builds on: [settings redesign](2026-10-21-settings-redesign-design.md) and [face sets](2026-10-22-face-sets-design.md).
Mockups: [Game Settings Redesign](https://claude.ai/artifact/9rgz3WYJLKk8QFFmNk24ae) (artboard B for the card layout; this change replaces its picture with a live board).

## Why

The big card on the settings screen shows a layout picture with the Dragon Valley art baked in, drawn in Classic tiles, whatever the player has chosen. The player should see what they're about to play: the real board, drawn with every current setting, redrawn as they change things.

The layout picker can't do that for every layout. There will be at least 40 layouts, and a live 3D view for each would be slow on a phone and would exceed the browser's limit of about 16 WebGL views per page. A layout's shape never changes, though, so its picker picture can be made once, ahead of time.

## Goals

- The big card shows a **live, read-only board** for every game, drawn with the current settings, and redrawn whenever one changes.
- The layout and board pickers show **pictures of each layout in blank 3D stones**, made by the game's own 3D renderer, over the player's chosen background.
- Making the pictures stays a one-step job when layouts are added. A test fails if a layout has no picture.
- The Cube's Small/Large switch becomes smaller and moves to the card's lower-right corner.

## Non-goals

- Live previews in the picker sheet.
- Turning, tilting or tapping the preview. It's for looking only.
- Changing how the game itself draws boards.

## Anti-goals

- A preview that differs from what the game will show: a different stone, face set, tile set or background, or a different board.
- A settings screen that's slow to open or to change a setting on a phone.
- Start game moving or needing a scroll (the earlier redesign's rule still holds).

## The big card: live preview

- **What it draws, by game:**
  - **Mahjong Solitaire:** the chosen layout in the chosen flat tile set (Classic images or Simple glyphs), using the game's own flat board (`Board`).
  - **Connect:** the chosen board in the chosen flat tile set, flat as the game draws Connect.
  - **Mahjong 3D:** the chosen layout in the chosen stone and face set, using the game's own 3D board (`Board3D`) at its starting angle.
  - **Mahjong Cube:** the cube of the chosen size in the chosen stone and face set, using the game's own `CubeBoard` at its starting angle.
  - All four draw over the chosen background, which fills the card behind a transparent board.
- **Which tiles:** a fixed deal (a constant seed per layout), so the preview stays the same while the player changes other settings. It isn't the deal they'll actually get.
- **Read-only:** no tap, drag, turn, zoom or hover reacts. For 3D and Cube, the renderers take a read-only option that leaves out their camera controls and click handling. For flat boards, the board takes no clicks.
- **Redraws when** the game, layout, board, tile set, stone, face set, background or Cube size changes.
- **The labels stay:**
  - Solitaire, 3D and Connect keep the "LAYOUT · name" / "BOARD · name" chip at lower left and "Change" at lower right. A tap anywhere on the card opens the picker, as now.
  - **Cube:** the "BOARD SIZE" chip stays at lower left. The **Small/Large switch moves to the lower right**, smaller to sit where "Change" is: chip-height, about 30px tall, 13px text. The card itself doesn't open anything for Cube.
- **While a 3D or Cube preview is starting,** the card shows the background alone (no spinner), and the board fades in when drawn. *(My call.)*
- **One WebGL preview at a time.** Changing a setting reuses or replaces the preview; it never stacks them up.

## Picker pictures: blank 3D stones

- **Look:** each layout (and each Connect board size) drawn by the game's own 3D renderer: blank stones (no picture, no symbol) in the default stone (Ivory), at the 3D board's starting angle, with its usual soft shadow, on a **transparent** background.
- **Size:** 400×300 PNG *(my call: sharper than today's 300×223 on high-density phones, still small)*, named as today: `wwwroot/layouts/previews/<slug>.png`.
- **How they're made:** a **development-only page** in the app (`/dev/layout-previews`, available only when the site runs in Development) draws each layout in turn with `Board3D` in read-only, blank, snapshot mode. It saves each through a **development-only endpoint** that writes the PNG into `wwwroot/layouts/previews/`. A "Redraw all" button redraws every layout; a "Missing only" button redraws layouts without a picture. The page lists each layout as it's saved, and you commit the pictures. *(My call: this uses the real renderer without adding a headless browser to the tools.)*
- **A copy outside the repo:** if the development setting `LayoutPreviews:CopyTo` (in `appsettings.Development.json` or user secrets) names a folder, the endpoint also saves each picture there as `<Layout name>.png`. It's empty by default, so nothing is copied until you set it. The page shows where copies went.
- No single-layout redraw; "Missing only" covers adding new layouts.
- Hidden layouts get pictures too, so they're ready if shown, but the test requires them only for visible layouts. *(My call.)*
- The picker sheet already draws each picture over the chosen background (contain, with a shadow); that stays.
- **Retire** `tools/LayoutPreviews` (the System.Drawing tool), and update the README's "The layout's picture" section to describe the page. *(My call: one way to make the pictures, not two.)*
- **Tests:**
  - Every visible layout and every Connect base board has a picture.
  - Every picture is a 400×300 RGBA PNG.
  - Every picture belongs to a layout (the existing test).
  - The dev endpoint isn't mapped outside Development.

## Constraints

- Blazor WebAssembly (.NET 10). The 3D views are three.js r180 in `wwwroot/js/board3d.js`, `cube.js` and `stone.js`.
- Blank stones need no new art. A face with no picture and an empty glyph draws as plain stone.
- Keep `SettingsChoice`, preferences and `Play.razor`'s handling unchanged; the preview only reads the dialog's current choices.
- Every new piece of text goes through `L[...]`, with Spanish entries (the page in development needs none if it isn't localized; the builder decides).
- The dev endpoint writes only into `wwwroot/layouts/previews/` (and the configured copy folder, if any), only for a known layout, and only in Development.

## Left to the builder

- Whether the preview is one new component that picks `Board`, `Board3D` or `CubeBoard`, or lives inline in the dialog.
- How the read-only and snapshot options are passed to the renderers, and how the page knows a board has finished drawing (stone texture loaded) before taking its picture.
- The fade-in timing.
