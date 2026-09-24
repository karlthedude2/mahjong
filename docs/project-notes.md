# Project notes: where things stand

A handover record of the work done on 23 September 2026, so the project can be picked up from a fresh
clone. It covers what exists, why it was built that way, and what's left to do.

For step-by-step instructions, see:
- [README.md](../README.md): project overview, adding layouts, tile sets and backgrounds.
- [deploy.md](deploy.md): running the web version locally, and deploying it to Azure.

## Current status

- **Branches:** all work is merged into `main` (pull request #1, from the `web-version` branch).
  This document and a correction to `deploy.md` are on the `docs-handover` branch.
- **CI:** the `CI` workflow passes on GitHub for both the web app and the desktop app.
- **Deployment:** not live yet. The `Deploy web app` workflow runs on every push to `main`, but it
  fails at the Azure sign-in step until the one-time setup in `deploy.md` is done. Nothing is
  deployed or changed when it fails. GitHub has already created the `production` environment.
- **Tests:** 68 rules tests (`Mahjong.Core.Tests`) and 15 web integration tests
  (`web/Mahjong.Web.Tests`) all pass.

## What was built

### 1. The desktop game: bugs fixed and code reorganised

The original WinForms game (`MahjongSpriteVersion`) had several bugs.
- **Deals that couldn't be won.** The old generator grew the board outward from "start" tiles and
  never checked that both tiles of a pair would be free at the same time.
- **A blocking bug.** It checked one "tile above" position twice and missed another, so some
  half-covered tiles counted as free.
- **Clicks and drawing.** Clicks could pick the wrong tile where tiles overlap, and tiles in
  half-offset rows were drawn in the wrong order, so a left tile could cover a right tile's face.
- **Smaller problems:** a broken Test layout (24 positions, 48 tiles), undo/redo score mistakes,
  counters not reset on New Game, a clock that misread times past an hour, and slow image handling.

The rules moved into a new shared library, **`Mahjong.Core`**, which has no user interface. The
desktop app became a thin layer over it, and the web version reuses it unchanged.

### 2. Layouts as text files

Each layout is a `.layout` text file in `Mahjong.Core/Layouts/`: a `name:` line and one `x y z`
line per tile. Files are built into the program and discovered automatically.
- **Existing layouts renamed:** "Number One" became **Twin Peaks** and "The Runner Up" became
  **Temple**. The desktop game converts saved high scores to the new names.
- **Added:** **Standard Turtle** (the classic 144-tile layout), plus ten shape layouts: Arena,
  Bridge, Butterfly, Cat, Cloud, Crab, Fortress, Pyramid, Spider and Tower. The shape layouts are
  drawn as text grids in `tools/generate_layouts.py`.
- **Hidden layouts:** `Test` has a `hidden: true` line, which keeps it out of the menus. The
  desktop game shows it with `--show-hidden-layouts`; the web version shows it in Development.

### 3. The web version

- **Game:** runs in the browser (Blazor WebAssembly) using `Mahjong.Core`, so it has the same rules
  and the same guaranteed-winnable deals.
- **Accounts:** email sign-up with confirmation, plus Google, Microsoft and Facebook sign-in, which
  switch on once their keys are configured. Each player has a display name for the leaderboards.
- **Guests:** can play without an account, see more ads, and can't post scores.
- **Leaderboards:** the top 20 verified scores per layout. The server replays every ranked game
  before accepting it.
- **Stats:** games played and won, and best scores per layout, for signed-in players.
- **Score panel:** explains each bonus as it builds up, and a breakdown dialog appears at the end.
- **Tile sets:** "Classic" (the desktop artwork) and "Simple" (bold, colour-coded text faces).
- **Backgrounds:** "Dragon Valley" (the supplied painting, the default), "Dragon Mountains" (an
  original vector scene) and "Jade Silk".
- **Other controls:** a speaker icon in the header toggles sound on every page. The clock starts on
  the first move. Pause hides the board and works for ranked games too.
- **Ads:** Google AdSense slots, which show placeholders until configured.
- **Deployment:** Azure App Service and Azure SQL, set up by Bicep and deployed by GitHub Actions.
  Live at **https://mahjong.haus** (a Cloudflare-registered domain with a free Azure certificate);
  the `SITE_HOST` variable makes other addresses redirect there.

## How the pieces fit

| Folder | What it is |
|---|---|
| `Mahjong.Core/` | The rules: board, blocking rule, winnable dealer, scoring and bonuses, undo/redo, game records and replay, shared tile geometry, layout files. Targets netstandard2.0, so both the .NET Framework desktop app and .NET 10 web app use it. |
| `Mahjong.Core.Tests/` | xUnit tests for the rules, including dealing and clearing every layout 1,000 times. |
| `MahjongSpriteVersion/` | The desktop game (WinForms, .NET Framework 4.8). |
| `web/Mahjong.Web/` | The server: sign-in (ASP.NET Core Identity), database (EF Core, SQL Server), the game API that verifies scores, the leaderboard and stats pages, and the site layout. |
| `web/Mahjong.Web.Client/` | The game in the browser: board, score panel, tile sets, backgrounds, sound, ad slots. |
| `web/Mahjong.Web.Tests/` | Integration tests that host the whole server in memory. |
| `infra/` | `main.bicep` (the Azure resources) and `bootstrap.ps1` (one-time GitHub-to-Azure sign-in setup). |
| `.github/workflows/` | `ci.yml` (tests and builds), `deploy.yml` (migrate and deploy on push to `main`), `infra.yml` (create or update Azure resources, run by hand). |
| `tools/` | `generate_layouts.py` (shape layouts) and `generate_backgrounds.py` (the vector backgrounds). |

## Key decisions and why

- **Winnable deals.** The dealer plays the game on the empty layout first, removing random pairs of
  free positions until it's empty, then gives each removed pair matching faces. Replaying that order
  always clears the board. Shuffle uses the same method, so it can't create an unwinnable board.
  With four copies of most tiles, a player can still reach a dead end by taking the "wrong" pair;
  Undo or Shuffle gets them out.
- **Drawing order.** Tiles are drawn by layer, then by X + Y, then by X. That's the order in which
  each tile's image covers only its neighbours' edges, including in half-offset rows. Clicking uses
  the same order in reverse, so the tile on top is the one clicked.
- **One blocking rule** (`BoardRules.IsFree`), shared by clicking, dealing, shuffling and the
  dead-end check. A tile is blocked by any overlapping tile on the layer above, or by neighbours on
  both its left and right.
- **Scores the server can trust.**
  1. The server picks each ranked game's seed.
  2. A seeded generator (`DeterministicRandom`, SplitMix64) deals the same board in the browser and
     on the server.
  3. The browser records every move and the second it happened.
  4. The server replays the moves, rejects impossible ones, and computes the score itself; any
     score the browser claims is ignored.
  5. The game clock must match the real time played, within 30 seconds.

  Seeds are kept below 2^53 so they survive JavaScript numbers.
- **Pauses are timed by the server.** Pause and resume are sent to the server, which records the
  times from its own clock and leaves paused time out of the clock check. A ranked game starts out
  paused, and the first move resumes it, which is how the clock can wait for the first move. A
  determined cheater could still study the hidden board during a pause through the browser's
  developer tools; that was accepted as the cost of allowing pauses.
- **Clock counts real time.** The browser's game clock follows elapsed real time, not timer ticks,
  so it stays right when browsers slow down timers in background tabs.
- **The board is one SVG.** It scales to any screen, and each tile is its own element, ready for
  future animations. The hook for them is `ITileEffects` in
  `web/Mahjong.Web.Client/Game/TileEffects.cs`.
- **Bonus names.** They were renamed for clarity in both versions: speed bonus, pace bonus, time
  bonus, quick-finish bonus and no-shuffle bonus. The scoring rules themselves didn't change; the
  full rules are in the `Scoring` class comments.
- **Ads.** AdSense forbids ads in pop-up windows. The "ad after a win" for guests is an ordinary ad
  unit inside the results dialog, next to the score breakdown, with a 5-second wait before
  "Play again".
- **Cheapest Azure setup.** App Service F1 plus the Azure SQL free offer. Running the game in the
  browser keeps server use tiny, which suits F1's 60 CPU-minutes a day. To upgrade, run `infra.yml`
  and pick a larger plan.
- **Images and copyright.** The desktop game's dragon background came from the internet, so the web
  version doesn't use it. The web default is the supplied painting, `dragon-valley.jpg`, resized
  from the original 5504 × 3072 PNG to 2560 × 1429. Confirm you have the rights to publish it.

## Things that will trip you up

- **Visual Studio 2022 can't open the web projects.** They target .NET 10, which needs VS 2026,
  VS Code or the `dotnet` command line. VS 2022 still builds and runs the desktop game.
- **Local database:** development uses SQL Server LocalDB, and the database is created and
  migrated automatically. Tests use an in-memory SQLite database.
- **Local email:** with no email service configured, the confirmation link appears on the page
  after registering.
- **The deploy workflow fails until Azure is set up.** That's expected and harmless. It can be
  disabled on GitHub (Actions → Deploy web app → Disable workflow) until then.
- **Free databases must share a region.** A subscription can have up to 10 free Azure SQL
  databases, all in one region. If you already have one, run `bootstrap.ps1` with that region.
- **Tile images exist twice.** The classic PNGs are copied into
  `web/Mahjong.Web.Client/wwwroot/tilesets/classic/`, because linking them from the desktop
  project served empty files. If you change the desktop artwork, copy it there too.
- **No `appsettings.json` in the web client.** The .NET 10 SDK breaks fresh builds when a
  WebAssembly project has `wwwroot/appsettings*.json`, so the client has none. It gets its settings
  from the server at `/api/client-config`.
- **Desktop high scores are saved per exe location.** Running the game from a different folder
  starts a separate score list.

## What's left to do

1. **Azure setup:** follow `deploy.md`.
   1. Run `bootstrap.ps1`, using your existing free database's region.
   2. Add the GitHub secrets and variables it prints.
   3. Run the `Deploy Azure infrastructure` workflow.
   4. Re-run `Deploy web app`, or push to `main`.
2. **Sign-in providers:** create the Google, Microsoft and Facebook apps and add their keys.
   Facebook needs business verification before the public can use it.
3. **AdSense:** apply once the site is live, then add the client ID and slot IDs as GitHub
   variables and re-run the infrastructure workflow.
4. **Not yet verified against Azure:** the Bicep template (the Bicep CLI wasn't installed locally),
   real email sending, and the external sign-in providers.
5. **Optional ideas raised along the way:**
   - switch the desktop game to the new Dragon Valley background;
   - add tile animations through `ITileEffects`;
   - add more tile sets and backgrounds (see the README).

## Day-to-day commands

```powershell
# Run the web version at http://localhost:5011
dotnet run --project web/Mahjong.Web

# Tests
dotnet test Mahjong.Core.Tests
dotnet test web/Mahjong.Web.Tests

# After changing the database model
dotnet tool restore
dotnet ef migrations add <Name> --project web/Mahjong.Web --output-dir Data/Migrations

# Regenerate the shape layouts or the vector backgrounds
python tools/generate_layouts.py
python tools/generate_backgrounds.py
```
