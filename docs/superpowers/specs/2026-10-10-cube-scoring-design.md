# Scored Mahjong Cube — design

Date: 2026-10-10. Branch: `board-3d`.

## Goal

Mahjong Cube is playable today from the new-game dialog's Cube tab, but only casually: there's no score,
no verification and no leaderboard. The user wants it scored "in a similar manner as the others".

What the user decided:

- **Scoring:** the same rules as Mahjong Solitaire: the 5-minute bonus clock, the pace bonus, speed
  windows, a 1,000 shuffle penalty and a 500 hint penalty.
- **Controls:** all of Solitaire's: shuffle, undo, redo, hint, demo and pause, at the same costs.
- **Deals:** the Deal setting works as it does in the other games. Winnable or random, with random-deal
  wins marked on the leaderboard.

Success means a cube game is played with the usual score bar and toolbar, a signed-in win is verified by
the server and appears on a "Mahjong Cube" leaderboard (Small and Large), and everything that works for
Solitaire wins (the results dialog, claiming a guest win, "Play this deal") works for the cube.

## Approach

The cube becomes a third **game kind** in `Mahjong.Core`, next to Solitaire and Connect, with two
layouts: **Cube Small** (4×4×4, 64 tiles) and **Cube Large** (6×6×6, 216 tiles). Connect already fits in
this way, so the same engine handles everything that isn't specific to the cube:

- `MahjongGame` (moves, undo, redo, shuffle, hints)
- `Scoring`
- `GameRecord` and `GameReplayer`
- the server's `GameService.Verify`
- the leaderboards, which are keyed by layout name, so **no database change**.

We rejected two other approaches. A separate cube engine would duplicate scoring, records, verification
and leaderboard tables. Scoring on the device only would have no verification and no leaderboards, so it
isn't "like the others".

## 1. Core (`Mahjong.Core`)

**Game kind.** Add `GameKind.Cube`: "A cube of tiles turned in 3D. A tile can be taken when two of its
sides that meet at an edge are open."

**Layouts.**
- A new `CubeLayouts` class (alongside `ConnectLayouts`). It provides `All = [Cube Small, Cube Large]`
  and `Prefix = "Cube"`.
- Each layout's positions are the cells `(x, y, z)` for 0 ≤ x, y, z < size, in whole-cell units (not
  Solitaire's half-tile units). The layout also records its `Size` (the tiles along an edge).
- The layouts are built with the internal `LayoutDefinition` constructor, as Connect's are, so they never
  go through `Validate()`. Solitaire's overlap rule doesn't apply to them.
- `LayoutDefinition` gains `int Size` (the cube's edge, 0 for other kinds).
- `LayoutCatalog.Cube` returns `CubeLayouts.All`, and `LayoutCatalog.Find` looks in the cube layouts
  too. Cube layouts aren't in `All` or `Visible`, just as Connect's aren't.

**Free rule.** `CubeRules.IsFree(position, occupied)`: a tile is free when it's open along at least two
of the three axes. That is, on two axes at least one neighbour is missing. This is `CubeGame.IsFree`,
moved into the core. `Board.IsFree` uses it when `layout.Kind == GameKind.Cube`. `Board.CanRemove` needs
no other change, since there's no path rule. `Board.FindMove` then works unchanged for hints and the
demo.

**Dealing.** `CubeDealer.Deal(positions, pairFaces, random)` returns a `DealResult` with a `Solution`.
It's `CubeGame.TryFindRemovalOrder` moved into the core: remove two random free positions until none are
left, then give each removed pair the same face, with up to `WinnableDealer.MaxAttempts` tries. It uses
only the `Random` it's given, so a seeded `DeterministicRandom` deals the same cube in the browser and on
the server.

`Board.Deal` and `Board.Reshuffle` choose the dealer by kind:
- a random deal: `RandomDealer`, for every kind
- Connect: `ConnectDealer`
- Cube: `CubeDealer`
- Solitaire: `WinnableDealer`

The demo plays `LastDeal.Solution`, which `CubeDealer` provides like the other dealers.

**Faces.** `TileSet.CreatePairs` already deals more pairs than one standard set holds: the Large cube
needs 108 pairs. The cube uses it as the casual cube does today.

**Nothing else changes in the core.** That covers `MahjongGame`, `Scoring`, `GameRecord`, `GameReplayer`
and `Board.FindMove`. They only see tiles, positions and `Board.IsFree`.

## 2. Server (`Mahjong.Web`)

**Games.** Starting, finishing, verifying and claiming games already go by layout name through
`LayoutCatalog.Find`, and verification replays through the core. Once `Find` knows the cube layouts,
cube games are started, verified and saved like any other game. `GameService` already looks layouts up
with `LayoutCatalog.Find`.

**Leaderboards page** (`Components/Pages/Leaderboards.razor`):
- Add a third pill, "Mahjong Cube", linking to `Cube Small`.
- `game` covers `GameKind.Cube`.
- `Choices` returns `LayoutCatalog.Cube` for it.
- The layout lookup searches `LayoutCatalog.Cube` too. Today it searches only `Visible` and
  `Connect`.
- The picker's label is "Board size".
- Random-deal wins are marked as they are for Solitaire.

**No migration.** The game, high-score and replay-score tables are keyed by layout name.

## 3. Game page and dialog (`Mahjong.Web.Client`)

**The cube is an ordinary game.** Choosing the cube starts a `Session` game on `Cube Small` or
`Cube Large`, like any other layout. On the play page, a game whose layout kind is Cube shows the cube
view in the board area. The usual score bar, toolbar (shuffle, undo, redo, hint, demo and pause), paused
screen, results dialog and guest claiming all apply.

**Cube view.** `CubeBoard.razor` is reworked to draw a `MahjongGame`, as `Board3D` does, instead of
running its own `CubeGame`:
- Its parameters are `Game`, `Selected`, `Hinted`, `OnTileClick` and `Stone`, plus the test bench's
  `StoneUrl` and `StoneFacesKey`.
- Tile positions come straight from the cube layout's `(x, y, z)`.
- After each change, it sends `cube.js` the tiles still on the board, which are free, the selected tile
  and the hinted tiles. A removed pair shrinks away (the existing `remove`). An undo, redo or shuffle
  redraws the cube (the existing `setBoard`, which redraws every tile still present).
- The bar with the tile count, Hint and New game goes. The page's score bar and toolbar take its place.
- The help line about turning the cube stays.
- `cube.js` keeps reporting the tapped tile's id to the component, which passes it on through
  `OnTileClick`. The component keeps the disposed guard it has now.

**Removed.** The casual cube mode goes from `Play.razor`: the `cubeSize`, `pendingCubeSize` and
`cubeDeal` fields, the pending-cube notice and `ApplyCubeAsync`. So do `Preferences.CubeSize` and
`GameSession.Clear()`, if nothing else uses them, and `CubeGame.cs`. A queued cube choice now works
like any other queued layout.

**New-game dialog, Cube tab.**
- **Board size** (Small or Large) picks the layout, `Cube Small` or `Cube Large`.
- **Stone** and **Background** stay as they are.
- **Deal** (winnable or random) comes back.
- Apply returns a normal `SettingsChoice` with the cube layout's name. `CubeSize` is removed from
  `SettingsChoice`.
- The dialog opens on the Cube tab when the current game's layout is a cube.

**Stone.** The cube draws in `Preferences.LastStone`, as it does today. The cube is always drawn in
stone, whatever the 3D or flat tile set.

**Title and links.**
- The title shows "Mahjong Cube · Small" or "Mahjong Cube · Large", from the layout.
- `L.LayoutName` translates cube layout names.
- Leaderboard "Play this deal" links work by layout name and seed, so they deal the same cube.

**Leaderboards dialog** (`LeaderboardDialog.razor`). Add a "Mahjong Cube" option group with Small and
Large.

**Test bench.** `/cube` stays unlinked. It runs a local `MahjongGame` on a cube layout through the same
`CubeBoard`, with its size buttons and stone picker.

## 4. Translations

`L.LayoutName` translates a non-Connect layout by its name. So add Spanish for "Cube Small" and "Cube
Large" ("Cubo pequeño", "Cubo grande"). "Mahjong Cube", "Small", "Large" and "Board size" are translated
already. Remove keys that only the casual
cube used, such as "You cleared the cube!" and "Your next game will be Mahjong Cube.". The existing
translation tests catch any that are missing or unused.

## 5. Testing

**Core** (`Mahjong.Core.Tests`, new `CubeTests`):
- The two layouts have 64 and 216 positions, are kind Cube, and are found by name but aren't in `All`.
- The free rule: a corner is free, a face centre with only one open side isn't, and a tile open on two
  opposite sides of one axis isn't.
- A winnable deal's `Solution` clears the board by `Board`'s own rules, for both sizes and several seeds.
- The same seed deals the same faces twice.
- A shuffle keeps the board clearable.
- A recorded cube game (with an undo, a shuffle and a hint) replays through `GameReplayer` to the same
  score.

**Server** (`Mahjong.Web.Tests`):
- A full cube game is started, finished and verified, and appears on the `Cube Small` leaderboard.
- A random-deal cube win is marked as random.

**Client** (`Mahjong.Web.Tests`): the Cube tab's Apply returns the right layout name for each size, and
the deal setting is kept.

**By hand.** On my 5031 copy I'll start a cube game from the dialog and check the score bar and toolbar,
the leaderboards page, and the results dialog by automation. Tapping cube tiles can't be done by
automation, so the user should play one game by hand before merging.

## 6. Shipping

The work is committed on `board-3d`. It's merged to main, which deploys, only when the user says so.

## Out of scope

- A phone layout tuned for the cube.
- Connect in 3D.
- Cube sizes other than Small and Large.
