# Settings Redesign Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the settings dialog with the dark, phone-first "direction B" screen: a Game dropdown, a big Layout card drawn over the chosen background, Tiles/Stone and Background cards, a picker sheet, and a pinned Start game button.

**Architecture:** `SettingsDialog.razor` keeps its parameters, state and `SettingsChoice` output. Only its markup and CSS are rewritten. A new `PickerSheet` component replaces `PreviewStrip` (which only this dialog uses). A small `SettingsArt` helper supplies the Tiles/Stone card pictures. The layout preview tool draws transparent PNGs, so the dialog can put any background behind them.

**Tech Stack:** Blazor WebAssembly (.NET 10) with scoped CSS, xUnit (`web/Mahjong.Web.Tests`), System.Drawing (`tools/LayoutPreviews`).

**Spec:** `docs/superpowers/specs/2026-10-21-settings-redesign-design.md` (mockups: https://claude.ai/artifact/9rgz3WYJLKk8QFFmNk24ae, artboards B `Grid.dc.html` and `Picker.dc.html`).

## Global Constraints

- Colors: ground `#171310`, card `#201A16`, raised `#2B231E`, borders `#3A312B` / `#2E2621`, text `#F3EEDC`, muted `#B8AE9C`, accent `#D7263D`, white text on the accent.
- Font: Outfit, weights 400–700, scoped to the dialog. It's loaded with a Google Fonts `<link>` in `web/Mahjong.Web/Components/App.razor` *(my call: the simplest way to load it)*.
- Corners 12–14px on cards and controls, 22px on the top of the sheet. Touch targets at least 44px. Start button 54px tall.
- Every shown text goes through `L[...]`, and every new key gets a Spanish entry in `web/Mahjong.Web.Client/Resources/Strings.es.resx` (`SpanishTests` enforces this).
- No change to `SettingsChoice`, to `Play.razor`'s handling of the dialog, or to saved preferences.
- Never stage the user's uncommitted tile/logo experiments: `git add` named files only.

## Review Focus

1. **A short phone** (375×667 viewport, with the browser bars taking room): Start game stays visible and the middle scrolls, for every game, including Connect with its Gravity row. Covered in Task 5.
2. **Escape while the picker sheet is open** closes only the sheet, and the dialog stays open with its choices kept. Task 3 wires it, Task 5 checks it.
3. **Switching games back and forth** keeps the flat set and the stone separately, and Cube's size and Connect's board and gravity. Task 5.
4. **A layout or board with no picture** shows the "No picture yet" placeholder on the big card and in the sheet. Task 3 and Task 4.
5. **Spanish at phone width:** longer labels ("Fondo", "Camino ganador garantizado", the game names) wrap and don't push Start off-screen or overflow the chips. Task 5.

---

### Task 1: Transparent layout previews

**Files:**
- Modify: `tools/LayoutPreviews/Program.cs` (the header comment, the `background` load at line 74, the frame drawing at about lines 113–121)
- Regenerate: `web/Mahjong.Web.Client/wwwroot/layouts/previews/*.png`
- Test: `web/Mahjong.Web.Tests/LayoutPreviewTests.cs`

**Interfaces:**
- Produces: every preview PNG is 300×223 RGBA with transparent pixels around the board. Later tasks layer it over a background with `object-fit: contain`.

- [ ] **Step 1: Write the failing test** in `LayoutPreviewTests`:

```csharp
[Fact]
public void PreviewPicturesHaveSeeThroughBackgrounds()
{
    // PNG IHDR: colour type is byte 25; 6 = RGBA (today's pictures are 2, RGB with the background baked in).
    Assert.All(Directory.GetFiles(PreviewFolder(), "*.png"), file =>
    {
        byte[] png = File.ReadAllBytes(file);
        Assert.Equal(6, png[25]);
    });
}
```

- [ ] **Step 2: Run it.** `dotnet test web/Mahjong.Web.Tests --filter PreviewPicturesHaveSeeThroughBackgrounds`. Expected: FAIL (today's previews are 24bpp, colour type 2).
- [ ] **Step 3: Change the tool.** Drop the Dragon Valley background. Clear the frame to `Color.Transparent`, keep the board fitted and centered, and save the downscaled preview as `PixelFormat.Format32bppArgb`. Update the header comment ("…on a transparent background…").
- [ ] **Step 4: Regenerate.** `dotnet run --project tools/LayoutPreviews -- --all`. Open one PNG to check that the tiles sit on transparency.
- [ ] **Step 5: Run the test again.** Expected: PASS. Run the whole `LayoutPreviewTests` class too, also PASS.
- [ ] **Step 6: Commit** `tools/LayoutPreviews/Program.cs`, `web/Mahjong.Web.Client/wwwroot/layouts/previews/`, `web/Mahjong.Web.Tests/LayoutPreviewTests.cs`: "Layout previews: draw on a transparent background".

### Task 2: Pictures for the Tiles and Stone cards

**Files:**
- Create: `web/Mahjong.Web.Client/Game/SettingsArt.cs`
- Test: `web/Mahjong.Web.Tests/SettingsArtTests.cs`

**Interfaces:**
- Produces:
  - `public static IReadOnlyList<string> SettingsArt.SampleFaces` = `["Cdragon", "Fdragon", "East"]`, the three faces the Tiles card shows. Image sets use `TileSets.ImageUrl(set, face)`, and the Glyphs set uses `TileSets.Glyph(face)`.
  - `public static string SettingsArt.StoneCss(TileSetInfo stone)` returns the CSS `background` value for a stone tile: ivory `#EFE6D0`, onyx `#1E1C1F`, beige `#D8C3A0`, squareMarble `url('textures/square-marble.jpg') center / cover`. Any other value falls back to ivory.

- [ ] **Step 1: Write the failing tests:**

```csharp
[Fact]
public void EveryImageSetHasTheSampleFaces() =>
    Assert.All(TileSets.Flat.Where(s => s.Kind == TileSetKind.Images), set =>
        Assert.All(SettingsArt.SampleFaces, face => Assert.True(File.Exists(Wwwroot(TileSets.ImageUrl(set, face))), $"{set.Id}/{face}.png")));

[Theory]
[InlineData("3d-ivory", "#EFE6D0")]
[InlineData("3d-onyx", "#1E1C1F")]
[InlineData("3d-beige", "#D8C3A0")]
[InlineData("3d-marble", "url('textures/square-marble.jpg') center / cover")]
public void EachStoneHasItsOwnLook(string id, string css) =>
    Assert.Equal(css, SettingsArt.StoneCss(TileSets.Find(id)));
```

(`Wwwroot(path)` finds `Mahjong.Web.Client/wwwroot/<path>` by walking up from `AppContext.BaseDirectory`, like `LayoutPreviewTests.PreviewFolder`.)

- [ ] **Step 2: Run them.** `dotnet test web/Mahjong.Web.Tests --filter SettingsArtTests`. Expected: FAIL to compile (no `SettingsArt`).
- [ ] **Step 3: Implement `SettingsArt`** with the two members above.
- [ ] **Step 4: Run them again.** Expected: PASS.
- [ ] **Step 5: Commit** both files: "Settings: tile and stone sample pictures".

### Task 3: Picker sheet

**Files:**
- Create: `web/Mahjong.Web.Client/Components/PickerSheet.razor`, `PickerSheet.razor.css`
- Delete: `web/Mahjong.Web.Client/Components/PreviewStrip.razor`, `PreviewStrip.razor.css` (in Task 4, once the dialog no longer uses them)

**Interfaces:**
- Consumes: `PreviewItem(Id, Name, ImageUrl, Badge)` from `Game/LayoutPreviews.cs`.
- Produces `<PickerSheet>` with these parameters:
  - `string Title` (EditorRequired)
  - `IReadOnlyList<PreviewItem> Items` (EditorRequired)
  - `string? SelectedId`
  - `string? BackdropUrl`: when set, each picture is drawn over this background (layouts and boards). Null for backgrounds.
  - `EventCallback<string> OnPick`: a tap picks and closes.
  - `EventCallback OnClose`: ×, Escape, or a tap on the dimmed area.

- [ ] **Step 1: Build the sheet** as in `Picker.dc.html`. It's a fixed overlay inside the dialog: dimmed (`rgba(0,0,0,0.55)`) above, a sheet `#1E1814` from the bottom, at most `calc(100% - 56px)` tall. It has a grab handle (40×5, `#4A3F37`), an `<h2>` with the title, and a 44px × button. The grid is 3 columns on a phone (`repeat(3, minmax(0, 1fr))`, gap `12px 10px`) and scrolls inside. Each item is a `<button aria-pressed>` with a 74px-tall rounded picture (10px corners) and its name below. The selected item gets `outline: 3px solid #D7263D; outline-offset: 2px` plus a 22px red check badge top-right. `Badge` ("Playing now") is a small dark chip bottom-left. Carry over `PreviewStrip`'s broken-image placeholder (`@onerror` → "No picture yet" SVG). The sheet is `role="dialog" aria-modal="true" aria-labelledby` its title. On open, focus the selected item. `@onkeydown` Escape calls `OnClose` and stops propagation, so the settings dialog doesn't also close.
- [ ] **Step 2: Build.** `dotnet build web/Mahjong.Web.Client`. Expected: 0 errors. (It's checked visually in Task 5.)
- [ ] **Step 3: Commit** the two new files: "Settings: picker sheet".

### Task 4: The new settings screen

**Files:**
- Modify: `web/Mahjong.Web.Client/Components/SettingsDialog.razor` (markup and the small UI state; `OnInitialized` and `ApplyAsync` keep their logic), and rewrite `SettingsDialog.razor.css`
- Modify: `web/Mahjong.Web/Components/App.razor` (Outfit `<link>` before `app.css`)
- Modify: `web/Mahjong.Web.Client/Resources/Strings.es.resx`
- Delete: `PreviewStrip.razor`, `PreviewStrip.razor.css`
- Test: `web/Mahjong.Web.Tests/SpanishTests.cs` (existing, no edits)

**Interfaces:**
- Consumes: `PickerSheet` (Task 3), `SettingsArt` (Task 2), the existing `Backgrounds`, `TileSets`, `LayoutPreviews`, `ConnectLayouts`, `CubeLayouts`.
- New UI state in the dialog: `enum Sheet { None, Layout, Board, Background }` and `Sheet sheet`; `bool showGameInfo`, alongside the existing `showDealInfo`.

- [ ] **Step 1: Rewrite the markup** following `Grid.dc.html`, top to bottom (the spec's "The screen" and "Per game" are the source of truth):
  1. Header: `logo-header.png` (20px tall, `alt="Mahjong Haus"`) and the × (`aria-label=L["Close"]`, calls `CancelAsync`). Remove the Cancel button.
  2. Game: a styled native `<select>` bound to `tab` (the overline `L["Game"]`, red chevron, 54px tall), then the ⓘ button. The note shows today's per-game sentence (the existing `tab switch` strings), using the same hover / focus-within / `.open` pattern as the deal info. Remove the description `<p>` under the game.
  3. Big card, 244px tall:
     - **Solitaire and 3D:** a `<button>` with the chosen background (cover) and `LayoutPreviews.Url(layout.PreviewName)` on top (contain, 10px inset, 40px clear at the bottom, drop shadow). Chips: "LAYOUT · <name>" and `L["Change"]`. It opens `Sheet.Layout`.
     - **Connect:** the same card with the board, "BOARD · <name>", opening `Sheet.Board`.
     - **Cube:** the background, with the existing Small/Large segmented choice centered on it and a "BOARD SIZE" chip.
  4. Two cards side by side:
     - **Tiles:** for an image set, the three `SettingsArt.SampleFaces` PNGs. For Simple, the three glyphs. Under them, the native `<select>` of `TileSets.Flat`.
     - **Stone (3D and Cube):** three blank tiles in `SettingsArt.StoneCss(stone)`, and the `<select>` of `TileSets.Stones`.
     - **Background:** the picture (80px), "BACKGROUND · <name>", opening `Sheet.Background`.
  5. **Connect only:** the Gravity segmented row and its hint, as today.
  6. The Guaranteed winnable switch, its info button and its hint, as today.
  7. A pinned footer: the skip checkbox (label `L["Skip this screen next time"]`), the gear hint in small muted text, and the Start game / OK button.

  The `PickerSheet` renders when `sheet != Sheet.None`. Layout and Board pass `BackdropUrl` = the chosen background's `Url`. `OnPick` sets `layoutName`, `boardName` or `backgroundId` and closes the sheet. Escape on the dialog closes the sheet first if one is open (`OnKeyDownAsync`). Drop `layoutStrip` and the focus-on-first-render code.
- [ ] **Step 2: Rewrite the CSS** to the Global Constraints colors. The backdrop is a full-viewport flex column. `.settings` is `width: min(440px, 100%)`, `height: min(100%, 100dvh)` on a phone, and `max-height: calc(100dvh - 2rem)` centered on wider screens, as a flex column: header, then a middle with `flex: 1; min-height: 0; overflow-y: auto`, then the footer `flex-shrink: 0`. Below 600px, no outer padding and no corner radius (full screen). Restyle `.segmented`, `.info-*`, the switches (`accent-color`/checked = `#D7263D`) and the selects in the dark palette. Info notes are `#2B231E` with cream text and `z-index` above the cards.
- [ ] **Step 3: Add the Outfit link** in `App.razor`: `https://fonts.googleapis.com/css2?family=Outfit:wght@400;500;600;700&display=swap`, plus the two `preconnect` links. The dialog's CSS sets `font-family: Outfit, system-ui, sans-serif`.
- [ ] **Step 4: Run the Spanish test.** `dotnet test web/Mahjong.Web.Tests --filter EveryTextInTheCodeHasASpanishTranslation`. Expected: FAIL, listing the new keys: "Change", "Skip this screen next time", "Board size" labels if new, "About this game", and so on.
- [ ] **Step 5: Add the Spanish entries** to `Strings.es.resx` (es-MX tone, matching the existing entries). Remove entries that no code uses any more, such as "Don't show settings for every new game", if the test file or other code doesn't reference them.
- [ ] **Step 6: Run all the web tests.** `dotnet test web/Mahjong.Web.Tests`. Expected: all PASS.
- [ ] **Step 7: Commit** the dialog, its CSS, `App.razor`, the resx and the PreviewStrip deletions: "Settings: dark, phone-first settings screen".

### Task 5: Check it in the browser

**Files:** none (fix-ups go back to Task 3 or 4's files, in a follow-up commit).

- [ ] **Step 1: Start the app** (the site runs locally on port 5011; see the project's launch settings) and open it in the browser pane.
- [ ] **Step 2: Phone check** (`resize_window` 375×667, then the mobile preset 375×812). For each game (Solitaire, 3D, Cube, Connect), with "New game": Start game is visible without scrolling, and the middle scrolls if it needs to. Expected: true for all four, Connect included.
- [ ] **Step 3: Sheet checks.** Open Layout, Board and Background. A tap picks and closes, and the card updates. Escape closes only the sheet. A tap on the dimmed area closes it. "Playing now" shows when a game is in progress. Layout pictures sit on the chosen background.
- [ ] **Step 4: Choices kept.** Pick the Simple set, switch to 3D and pick Onyx, then switch back to Solitaire: Simple is still chosen. Do the same for Cube Large and Connect Down gravity. Apply, reopen from the gear (the button reads OK), and the choices are as applied.
- [ ] **Step 5: Info buttons.** On the desktop size, hovering ⓘ shows the game rules. At phone size, a tap toggles them. The note doesn't move the layout.
- [ ] **Step 6: Spanish.** Switch to Spanish at 375×667 and repeat Step 2 for Connect. Nothing overflows, and Start stays visible.
- [ ] **Step 7: Desktop.** At 1280×800, the dialog is about 440px wide and centered, and its footer is pinned.
- [ ] **Step 8:** reset the viewport (`resize_window` preset desktop). Commit any fixes by name.
