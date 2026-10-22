# Game settings redesign

Date: 2026-10-21
Mockups: [Game Settings Redesign](https://claude.ai/artifact/9rgz3WYJLKk8QFFmNk24ae) (Claude Design canvas). Artboard **B** (`Grid.dc.html`) is the chosen direction. **Picker sheet** (`Picker.dc.html`) is the chosen picker.

## Why

On a phone, the settings dialog ([SettingsDialog.razor](../../../web/Mahjong.Web.Client/Components/SettingsDialog.razor)) makes you scroll to reach **Start game**. Most of the time a player changes nothing: they're playing the same game again, so the screen is mostly a quick "this is what you're about to play" before tapping Start. When they do change something, it's usually the layout, the tiles (stones in 3D) or the background. Each of those must be **equally easy** to change. The screen shouldn't push people into one way of working.

## Goals

- **Start game** is always on screen without scrolling, on a phone and everywhere else.
- The screen shows a picture of what you'll play: the chosen layout drawn over the chosen background.
- Changing the layout, tiles or background takes the same effort: one tap opens the choice.
- Layouts and backgrounds are still chosen from pictures.
- A new dark look replaces the current cream styling of this dialog.

## Non-goals

- Pictures for tile sets. Tiles stay a dropdown for now; pictures come later.
- Restyling the rest of the site. This covers the settings dialog only.
- Changing what the settings are, or how they're saved, applied or used for leaderboards.

## Anti-goals (a failure even if it works)

- Having to scroll to find Start game on a phone.
- One choice being noticeably harder to reach than the others, such as background buried below the fold or layout needing two taps while background needs one.
- A layout picture that crops the layout. The whole layout must show.

## Look

This is the dark style from the mockups. The user loves it, and the current styling doesn't need to be kept.

- Ground `#171310`, card surface `#201A16`, raised surface `#2B231E`, borders `#3A312B` / `#2E2621`.
- Text: cream `#F3EEDC`, muted `#B8AE9C`.
- Accent: red `#D7263D` (Start button, chevrons, selected rings, switches), with white text on red.
- Font: **Outfit** (Google Fonts), weights 400–700.
- Rounded corners: 12–14px on controls and cards, 22px on the top of the picker sheet.
- The Mahjong Haus logo-header.png sits top-left. The close (×) button is top-right, at least 44px.
- Touch targets are at least 44px.

## The screen (direction B)

From top to bottom:

1. **Header:** the logo and the close button. Closing works like Cancel today.
2. **Game dropdown:** a full-width styled native `<select>` with a small "GAME" label above the value and a red chevron. Options: Mahjong Solitaire, Mahjong 3D, Mahjong Cube, Connect (Shisen-Sho). The game decides which settings show below it (see "Per game").
   - **Info button** (ⓘ, at least 44px to tap) right of the dropdown. It shows the chosen game's brief rules: today's one-line descriptions ("Remove matching pairs of free tiles from the stack.", the Cube and Connect sentences, and so on). It works like today's Guaranteed winnable info button: hovering or keyboard focus shows the note on a computer, and a tap toggles it on a phone. The note floats over the content below it as a popover, so it takes no space on the screen. It's styled dark (surface `#2B231E`, cream text). The description line under the game choice is removed.
3. **Layout card** (full width, about 244px tall on a phone): the chosen background fills the card (cover), and the layout picture sits on top, fitted so the **whole layout** shows (contain). Two small dark chips run along the bottom: "LAYOUT · <name>" on the left and "Change" on the right. Tapping anywhere on the card opens the layout picker.
4. **Two half-width cards side by side:**
   - **Tiles:** a small drawing of tiles (the mockup uses 中 發 東 on cream tiles) above a native dropdown of the tile sets. No picker sheet for now.
   - **Background:** the background picture above "BACKGROUND · <name>". Tapping opens the background picker.
5. **Guaranteed winnable** switch, with the short line under it ("Every deal can be cleared." or the random-deal text, as today). Keep the info button and its note.
6. **Pinned footer:** a "Skip this screen next time" checkbox (today's "Don't show settings for every new game"), then a full-width red **Start game** button (54px). When the dialog opens from the gear rather than for a new game, the button reads **OK**, as today.

The footer is pinned. On short screens, or with a game that needs more rows, the middle section scrolls while the footer stays put.

## Per game

| Game | Big card | Small card 1 | Extra rows |
|---|---|---|---|
| Mahjong Solitaire | Layout (layout picker) | Tiles dropdown (flat sets) | – |
| Mahjong 3D | Layout (same layouts) | **Stone** dropdown (stones) | – |
| Mahjong Cube | **Board size**: Small (4×4×4) and Large (6×6×6) as a segmented choice in the card, over the chosen background | **Stone** dropdown | – |
| Connect (Shisen-Sho) | **Board** (board picker with the Connect boards) | Tiles dropdown | **Gravity**: Off / Down / Left segmented, with its hint line |

The second small card is always Background.

As today, each game keeps its own last choice: the flat set and the stone are remembered separately, and switching games in the dropdown doesn't lose them.

## Picker sheet

This is the "perfect" one from the mockup.

- A bottom sheet that slides up over the settings screen, with a dimmed area above it. It has a grab handle, a title ("Layout", "Board" or "Background") and a close (×) button.
- A grid of pictures, three across on a phone, each with its name underneath. The current choice has a red outline ring and a red check badge.
- Tapping a picture selects it and closes the sheet. The settings screen then shows the new choice. Nothing is applied to the game until Start game / OK, as today.
- The grid scrolls inside the sheet when there are more pictures than fit.
- The layout picker keeps today's "Playing now" badge on the layout currently being played.
- Layout and board pictures in the picker are drawn over the **currently chosen background**, the same way as on the Layout card.
- A missing picture shows the existing "No picture yet" placeholder.
- Escape, or tapping the dimmed area, closes the sheet without changing anything. Escape closes only the sheet, not the whole dialog.

## Layout pictures without a background

Today's layout previews (300×223 PNGs in `wwwroot/layouts/previews/`, drawn by [tools/LayoutPreviews](../../../tools/LayoutPreviews/Program.cs)) have the Dragon Valley background baked in. The Connect board previews come from the same tool.

- Change the tool to draw the tiles on a **transparent** background, and regenerate every preview (`--all`).
- The settings screen and picker then put the chosen background behind the picture, so a player sees their layout on their background.

## Larger screens

The same single column, centered as a dialog about 440px wide, with the footer pinned to the dialog's bottom. *(My call: the mockups are phone-only.)*

## Calls made after review

- **Tiles card picture** *(my call)*: three real faces from the chosen flat set (`TileSets.ImageUrl`), so the picture changes with the set. For Mahjong 3D and Cube, the card reads **Stone** and shows a tile in that stone's color, not cream tiles.
- **Cancel** *(my call)*: the header × replaces the Cancel button and behaves exactly like it.
- **Gear hint** *(my call)*: "You can always change these with the settings gear in the header." stays, as small muted text under the skip checkbox in the pinned footer.

## Keep as is

- What `SettingsChoice` carries, and how [Play.razor](../../../web/Mahjong.Web.Client/Pages/Play.razor) opens the dialog (for a new game, from the gear, or `?settings=1`) and applies it.
- Translation: every new or changed piece of text goes through `L[...]`, with Spanish entries added in the resx files (the existing translation test checks this).

## Left to the builder

- Whether the picker sheet is a new component, and whether `PreviewStrip` is reused, reshaped or retired.
- The picker sheet's animation, if any.
- How the dark-style colors are organized in the CSS (scoped to the dialog).
