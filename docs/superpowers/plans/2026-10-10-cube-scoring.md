# Scored Mahjong Cube Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make Mahjong Cube a scored game with the same rules, controls, deals, verification and leaderboards as Mahjong Solitaire.

**Architecture:** The cube becomes a third game kind in `Mahjong.Core` (`GameKind.Cube`) with two layouts, "Cube Small" (4×4×4) and "Cube Large" (6×6×6). Each layout's positions are the cube's cells. `Board` uses the cube's free rule and dealer for those layouts. Everything else (`MahjongGame`, `Scoring`, `GameRecord`, `GameReplayer`, `GameService`, the leaderboards) works by layout name and is shared. On the client, the cube is an ordinary session game: `CubeBoard` draws a `MahjongGame`, and the casual cube mode is removed.

**Tech Stack:** .NET 10, Blazor WebAssembly, xUnit, Three.js (`wwwroot/js/cube.js`), `Mahjong.Core` on netstandard2.0 with `LangVersion latest`.

**Spec:** `docs/superpowers/specs/2026-10-10-cube-scoring-design.md`

## Global Constraints

- Layout names are exactly `Cube Small` (size 4) and `Cube Large` (size 6), with `CubeLayouts.Prefix = "Cube"`.
- Scoring, controls and costs are unchanged: shuffle 1,000, hint 500, the same bonuses.
- No database migration.
- Cube layouts are not in `LayoutCatalog.All` or `LayoutCatalog.Visible`. They are in `LayoutCatalog.Cube` and found by `LayoutCatalog.Find`.
- The cube is always drawn in the player's `Preferences.LastStone`.
- Run tests with `-c Release`, because the user's own copy on port 5011 locks the Debug build.
- Never stage the user's local files:
  - `tilesets/New/`
  - `tilesets/classic/unused/`
  - `tilesets/classic/8bamslayout.png`
  - the deleted `2wizard2.png` files
  - `logo-header.png`, `logo-header-old.png`
  - `C Horizontal Header Lockup@1x.png`

  Stage files by name, never with `git add -A`.
- Commit messages end with `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- Merge to main (which deploys) only when the user says so.

## Review Focus

Most likely problems first:

1. **Undo after taking a pair.** The two tiles must come back on the cube without the view jumping back to its starting angle. This is pinned by `CubeRedrawTests` (Task 3) and checked by hand (Task 4).
2. **A shuffle changes faces.** The block must redraw with the new faces and keep the current view. This is pinned by `CubeRedrawTests` (Task 3) and checked by hand.
3. **A returning player whose last game was a cube.** They should get a cube game, not Twin Peaks. `LastLayout` is "Cube Small", so `AllLayouts` must include the cube layouts. Checked by hand (Task 4).
4. **Pausing and resuming a cube game.** The view is rebuilt from the board, so tiles already taken stay gone. Checked by hand.
5. **The demo on a cube.** It plays the deal's solution to the end. This is pinned by `AWinnableDealClearsByTheBoardsOwnRules` (Task 1) and checked by hand.

---

## File Structure

- `Mahjong.Core/ConnectRules.cs`: add `GameKind.Cube`.
- `Mahjong.Core/CubeRules.cs` (new): `CubeLayouts` (the two blocks) and `CubeRules.IsFree`.
- `Mahjong.Core/LayoutDefinition.cs`: a cube constructor and `Size`.
- `Mahjong.Core/LayoutCatalog.cs`: `Cube`, and `Find` looks there too.
- `Mahjong.Core/WinnableDealer.cs`: the dealer takes a free rule.
- `Mahjong.Core/Board.cs`: the free rule and dealer for each kind.
- `Mahjong.Core.Tests/CubeTests.cs` (new).
- `web/Mahjong.Web.Client/Strings.cs`: the cube's displayed name.
- `web/Mahjong.Web.Client/Resources/Strings.es.resx`: one key added, cube-only keys removed.
- `web/Mahjong.Web/Components/Pages/Leaderboards.razor`: the Mahjong Cube tab.
- `web/Mahjong.Web.Client/Components/LeaderboardDialog.razor`: the Mahjong Cube group.
- `web/Mahjong.Web.Tests/CubeApiTests.cs` (new) and `SpanishTests.cs`.
- `web/Mahjong.Web.Client/Game/CubeRedraw.cs` (new): how the view catches up with the game.
- `web/Mahjong.Web.Client/Components/CubeBoard.razor` and `.razor.css`: draw a `MahjongGame`.
- `web/Mahjong.Web.Client/wwwroot/js/cube.js`: `setBoard` can keep the view.
- `web/Mahjong.Web.Client/Pages/Cube.razor`: the test bench runs a `MahjongGame`.
- `web/Mahjong.Web.Client/Pages/Play.razor`: the cube as a session game; casual mode removed.
- `web/Mahjong.Web.Client/Components/SettingsDialog.razor`: the Cube tab picks a layout; Deal is back.
- `web/Mahjong.Web.Client/Game/LayoutPreviews.cs` (`SettingsChoice`), `PlayerPreferences.cs`, `GameSession.cs`: casual-cube members removed.
- Deleted: `web/Mahjong.Web.Client/Game/CubeGame.cs` and `web/Mahjong.Web.Tests/CubeGameTests.cs`.
- `web/Mahjong.Web.Tests/CubeRedrawTests.cs` (new).

---

### Task 1: The cube in the core

**Files:**
- Create: `Mahjong.Core/CubeRules.cs`
- Modify: `Mahjong.Core/ConnectRules.cs` (the `GameKind` enum, lines 8–18)
- Modify: `Mahjong.Core/LayoutDefinition.cs` (constructors and properties)
- Modify: `Mahjong.Core/LayoutCatalog.cs` (`Connect` and `Find`, lines 25–33)
- Modify: `Mahjong.Core/WinnableDealer.cs` (`Deal`, `TryFindRemovalOrder` and `TryRemoveAll`)
- Modify: `Mahjong.Core/Board.cs` (`Deal`, `IsFree` and `Reshuffle`)
- Test: `Mahjong.Core.Tests/CubeTests.cs`

**Interfaces:**
- Consumes: nothing new.
- Produces:
  - `GameKind.Cube`.
  - `CubeLayouts`: `Prefix` (`"Cube"`), `All` (`IReadOnlyList<LayoutDefinition>`: Small, then Large) and `ForSize(int size) → LayoutDefinition` (Small for any size other than 6).
  - `CubeRules.IsFree(Position p, ISet<Position> occupied) → bool`.
  - `LayoutDefinition.Size` (int; 0 except for cube layouts).
  - `LayoutCatalog.Cube`.
  - `WinnableDealer.Deal(positions, pairFaces, random, Func<Position, ISet<Position>, bool> isFree)`.

- [ ] **Step 1: Write the failing tests**

Create `Mahjong.Core.Tests/CubeTests.cs`:

```csharp
using System.Collections.Generic;
using System.Linq;
using Mahjong.Core;
using Xunit;

namespace Mahjong.Core.Tests
{
    public class CubeTests
    {
        private static readonly LayoutDefinition Small = LayoutCatalog.Find("Cube Small");

        private static HashSet<Position> Block(int size) => new HashSet<Position>(CubeLayouts.ForSize(size).Positions);

        private static void PlaySolution(MahjongGame game)
        {
            foreach (var (first, second) in game.Board.LastDeal.Solution)
            {
                game.Tick();
                Assert.True(game.TryRemovePair(game.Board.At(first), game.Board.At(second)), $"{first} + {second}");
            }
        }

        [Fact]
        public void TheBlocksAreSmallAndLargeCubes()
        {
            Assert.Equal(new[] { "Cube Small", "Cube Large" }, LayoutCatalog.Cube.Select(l => l.Name));
            Assert.Equal(new[] { 64, 216 }, LayoutCatalog.Cube.Select(l => l.Positions.Count));
            Assert.Equal(new[] { 4, 6 }, LayoutCatalog.Cube.Select(l => l.Size));
            Assert.All(LayoutCatalog.Cube, l => Assert.Equal(GameKind.Cube, l.Kind));
            Assert.Same(LayoutCatalog.Cube[1], LayoutCatalog.Find("cube large"));
            Assert.Same(LayoutCatalog.Cube[1], CubeLayouts.ForSize(6));
            Assert.Same(LayoutCatalog.Cube[0], CubeLayouts.ForSize(5));
            Assert.DoesNotContain(LayoutCatalog.All, l => l.Kind == GameKind.Cube);
        }

        [Fact]
        public void OnlyCornersAndEdgesOfAFullBlockAreFree()
        {
            var full = Block(4);

            // 8 corners and 12 edges of 2 tiles each; the middles of the faces have one open side only.
            Assert.Equal(8 + 12 * 2, full.Count(p => CubeRules.IsFree(p, full)));
            Assert.True(CubeRules.IsFree(new Position(0, 0, 0), full));
            Assert.True(CubeRules.IsFree(new Position(1, 0, 0), full));
            Assert.False(CubeRules.IsFree(new Position(0, 1, 1), full));
            Assert.False(CubeRules.IsFree(new Position(1, 1, 1), full));
        }

        [Fact]
        public void OpenSidesOnOneAxisOnlyDontFreeATile()
        {
            // (1,1,1) with its left and right neighbours gone: open on one axis, covered on the others.
            var block = Block(4);
            block.Remove(new Position(0, 1, 1));
            block.Remove(new Position(2, 1, 1));
            Assert.False(CubeRules.IsFree(new Position(1, 1, 1), block));

            // Open below as well: two sides that meet at an edge.
            block.Remove(new Position(1, 0, 1));
            Assert.True(CubeRules.IsFree(new Position(1, 1, 1), block));
        }

        [Fact]
        public void TheBoardUsesTheCubesFreeRule()
        {
            var game = new MahjongGame(Small, 5);

            Assert.Equal(64, game.Board.Count);
            Assert.Equal(8 + 12 * 2, game.Board.Tiles.Count(game.Board.IsFree));
            Assert.NotNull(game.Board.FindMove());
        }

        [Theory]
        [InlineData("Cube Small", 1)]
        [InlineData("Cube Small", 2)]
        [InlineData("Cube Small", 3)]
        [InlineData("Cube Large", 1)]
        [InlineData("Cube Large", 2)]
        public void AWinnableDealClearsByTheBoardsOwnRules(string name, int seed)
        {
            var game = new MahjongGame(LayoutCatalog.Find(name), seed);

            PlaySolution(game);

            Assert.True(game.Board.IsComplete, name);
        }

        [Fact]
        public void TheSameSeedDealsTheSameCube()
        {
            string Faces(MahjongGame g) => string.Join(",", g.Board.Tiles.OrderBy(t => t.Id).Select(t => t.Face.Name));

            Assert.Equal(Faces(new MahjongGame(Small, 42)), Faces(new MahjongGame(Small, 42)));
            Assert.NotEqual(Faces(new MahjongGame(Small, 42)), Faces(new MahjongGame(Small, 43)));
        }

        [Fact]
        public void AShuffleMidGameCanStillBeCleared()
        {
            var game = new MahjongGame(CubeLayouts.ForSize(6), 7);
            var solution = game.Board.LastDeal.Solution;
            for (int i = 0; i < 10; i++)
            {
                Assert.True(game.TryRemovePair(game.Board.At(solution[i].First), game.Board.At(solution[i].Second)));
            }

            game.Shuffle();
            Assert.Equal(216 - 20, game.Board.Count);
            PlaySolution(game);

            Assert.True(game.Board.IsComplete);
        }

        [Fact]
        public void ARandomDealFillsTheBlockWithPairs()
        {
            var game = new MahjongGame(Small, 3, winnable: false);

            Assert.Equal(64, game.Board.Count);
            Assert.Empty(game.Board.LastDeal.Solution);
            Assert.All(game.Board.Tiles.GroupBy(t => t.Face.Name), g => Assert.Equal(0, g.Count() % 2));
        }

        [Fact]
        public void ACubeGameWithAnUndoAHintAndAShuffleReplaysToTheSameScore()
        {
            var game = new MahjongGame(Small, 2026);
            var solution = game.Board.LastDeal.Solution;
            for (int i = 0; i < 4; i++)
            {
                game.Tick();
                game.Tick();
                Assert.True(game.TryRemovePair(game.Board.At(solution[i].First), game.Board.At(solution[i].Second)));
            }

            game.Tick();
            Assert.True(game.Undo());
            Assert.NotNull(game.Hint());
            game.Shuffle();
            PlaySolution(game);
            game.Finish();

            var result = GameReplayer.Replay(game.Record);

            Assert.True(result.Valid, result.Error);
            Assert.True(result.Complete);
            Assert.Equal(game.Scoring.Card.Score, result.Score);
        }
    }
}
```

- [ ] **Step 2: Run the tests to check they fail**

Run: `dotnet test E:/data/dev/repos/mahjong/Mahjong.Core.Tests -c Release --filter FullyQualifiedName~CubeTests`

Expected: a build error, because `CubeLayouts`, `CubeRules`, `LayoutCatalog.Cube`, `GameKind.Cube` and `LayoutDefinition.Size` don't exist yet.

- [ ] **Step 3: Add the game kind**

In `Mahjong.Core/ConnectRules.cs`, make the end of `enum GameKind` read:

```csharp
        /// <summary>
        /// Connect (Shisen-Sho): a flat grid; matching tiles are removed if a line with at most two
        /// turns joins them through empty cells (it may run around the outside of the board).
        /// </summary>
        Connect,

        /// <summary>
        /// Mahjong Cube: a block of tiles turned in 3D. A tile can be taken when two of its sides
        /// that meet at an edge are open.
        /// </summary>
        Cube
    }
```

- [ ] **Step 4: Add the cube layout constructor and Size**

In `Mahjong.Core/LayoutDefinition.cs`, add this after the Connect constructor (the one ending `PreviewName = previewName;`):

```csharp
        /// <summary>A Mahjong Cube block: <paramref name="size"/> tiles along each edge.</summary>
        internal LayoutDefinition(string name, IEnumerable<Position> positions, int size)
            : this(name, positions)
        {
            Kind = GameKind.Cube;
            Size = size;
        }
```

Then add this after the `Gravity` property:

```csharp
        /// <summary>For Mahjong Cube: the tiles along each edge of the block.</summary>
        public int Size { get; }
```

- [ ] **Step 5: Create the layouts and the free rule**

Create `Mahjong.Core/CubeRules.cs`:

```csharp
using System.Collections.Generic;
using System.Linq;

namespace Mahjong.Core
{
    /// <summary>Mahjong Cube's blocks: Small (4×4×4) and Large (6×6×6). Positions are whole cells.</summary>
    public static class CubeLayouts
    {
        public const string Prefix = "Cube";

        /// <summary>"Cube Small" and "Cube Large", in menu order.</summary>
        public static IReadOnlyList<LayoutDefinition> All { get; } = new[]
        {
            Create("Small", 4),
            Create("Large", 6),
        };

        /// <summary>The block with <paramref name="size"/> tiles along each edge; Small for any size it doesn't have.</summary>
        public static LayoutDefinition ForSize(int size) => All.FirstOrDefault(l => l.Size == size) ?? All[0];

        private static LayoutDefinition Create(string name, int size)
        {
            var cells = Enumerable.Range(0, size).SelectMany(x =>
                Enumerable.Range(0, size).SelectMany(y =>
                    Enumerable.Range(0, size).Select(z => new Position(x, y, z))));
            return new LayoutDefinition($"{Prefix} {name}", cells, size);
        }
    }

    /// <summary>Mahjong Cube's rule for which tiles can be taken.</summary>
    public static class CubeRules
    {
        private static readonly (int X, int Y, int Z)[] Axes = { (1, 0, 0), (0, 1, 0), (0, 0, 1) };

        /// <summary>
        /// Free when open along two different axes: one open side across (left or right) and one open
        /// side up or down, say, which is two sides meeting at an edge. Open sides on one axis only
        /// (left and right, but covered above, below, in front and behind) don't count.
        /// </summary>
        public static bool IsFree(Position p, ISet<Position> occupied)
        {
            int openAxes = 0;
            foreach (var (dx, dy, dz) in Axes)
            {
                if (!occupied.Contains(new Position(p.X + dx, p.Y + dy, p.Z + dz))
                    || !occupied.Contains(new Position(p.X - dx, p.Y - dy, p.Z - dz)))
                {
                    openAxes++;
                }
            }

            return openAxes >= 2;
        }
    }
}
```

- [ ] **Step 6: Add the cube layouts to the catalog**

In `Mahjong.Core/LayoutCatalog.cs`, add this after the `Connect` property:

```csharp
        /// <summary>Mahjong Cube's blocks. Like the Connect boards, they're kept apart from the solitaire layouts.</summary>
        public static IReadOnlyList<LayoutDefinition> Cube => CubeLayouts.All;
```

Then replace `Find` with:

```csharp
        /// <summary>A solitaire layout, Connect board or cube by name.</summary>
        public static LayoutDefinition Find(string name)
        {
            return layouts.FirstOrDefault(l => string.Equals(l.Name, name, StringComparison.OrdinalIgnoreCase))
                ?? ConnectLayouts.All.FirstOrDefault(l => string.Equals(l.Name, name, StringComparison.OrdinalIgnoreCase))
                ?? CubeLayouts.All.FirstOrDefault(l => string.Equals(l.Name, name, StringComparison.OrdinalIgnoreCase));
        }
```

- [ ] **Step 7: Let the winnable dealer use another free rule**

In `Mahjong.Core/WinnableDealer.cs`:

1. Replace the start of the existing `Deal` (its signature line through the `TryFindRemovalOrder` call) with the following. Keep the rest of the old `Deal` body as the body of the new overload.

```csharp
        public static DealResult Deal(IReadOnlyCollection<Position> positions, IList<TileFace> pairFaces, Random random) =>
            Deal(positions, pairFaces, random, BoardRules.IsFree);

        /// <summary>The same, under another game's rule for which tiles are free (Mahjong Cube's).</summary>
        public static DealResult Deal(IReadOnlyCollection<Position> positions, IList<TileFace> pairFaces, Random random,
            Func<Position, ISet<Position>, bool> isFree)
        {
            if (positions.Count != pairFaces.Count * 2)
            {
                throw new ArgumentException($"{positions.Count} positions need {positions.Count / 2} pairs, got {pairFaces.Count}.");
            }

            if (!TryFindRemovalOrder(positions, random, isFree, out var order))
```

2. Replace `TryFindRemovalOrder` with these two methods:

```csharp
        public static bool TryFindRemovalOrder(IReadOnlyCollection<Position> positions, Random random, out List<(Position First, Position Second)> order) =>
            TryFindRemovalOrder(positions, random, BoardRules.IsFree, out order);

        public static bool TryFindRemovalOrder(IReadOnlyCollection<Position> positions, Random random,
            Func<Position, ISet<Position>, bool> isFree, out List<(Position First, Position Second)> order)
        {
            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                if (TryRemoveAll(positions, random, isFree, out order))
                {
                    return true;
                }
            }

            order = null;
            return false;
        }
```

3. In `TryRemoveAll`, add a `Func<Position, ISet<Position>, bool> isFree` parameter after `random`, and change `BoardRules.IsFree(p, occupied)` to `isFree(p, occupied)`.

- [ ] **Step 8: Have the board use the cube's rules**

In `Mahjong.Core/Board.cs`:

1. In `Deal`, replace the `var deal = ...` statement with:

```csharp
            var deal = !winnable ? RandomDealer.Deal(layout.Positions, pairFaces, random)
                : layout.Kind == GameKind.Connect ? ConnectDealer.Deal(layout, layout.Positions, pairFaces, random)
                : layout.Kind == GameKind.Cube ? WinnableDealer.Deal(layout.Positions, pairFaces, random, CubeRules.IsFree)
                : WinnableDealer.Deal(layout.Positions, pairFaces, random);
```

2. Replace `IsFree` (and its summary) with:

```csharp
        /// <summary>
        /// Whether the tile can be picked: in Connect every tile can; in solitaire and the cube only
        /// free ones, by each game's rule.
        /// </summary>
        public bool IsFree(Tile tile) =>
            Contains(tile) && (layout?.Kind switch
            {
                GameKind.Connect => true,
                GameKind.Cube => CubeRules.IsFree(tile.Position, occupied),
                _ => BoardRules.IsFree(tile.Position, occupied),
            });
```

3. In `Reshuffle`, replace the `LastDeal = ...` statement with:

```csharp
            LastDeal = !winnable ? RandomDealer.Deal(tiles.Keys.ToList(), pairFaces, random)
                : layout?.Kind == GameKind.Connect ? ConnectDealer.Deal(layout, tiles.Keys.ToList(), pairFaces, random)
                : layout?.Kind == GameKind.Cube ? WinnableDealer.Deal(tiles.Keys.ToList(), pairFaces, random, CubeRules.IsFree)
                : WinnableDealer.Deal(tiles.Keys.ToList(), pairFaces, random);
```

- [ ] **Step 9: Run the cube tests to check they pass**

Run: `dotnet test E:/data/dev/repos/mahjong/Mahjong.Core.Tests -c Release --filter FullyQualifiedName~CubeTests`

Expected: all 13 pass. (The Theory counts as 5.)

- [ ] **Step 10: Run the whole core suite**

Run: `dotnet test E:/data/dev/repos/mahjong/Mahjong.Core.Tests -c Release`

Expected: all pass (82 existing plus 13 new).

- [ ] **Step 11: Commit**

```bash
cd E:/data/dev/repos/mahjong
git add Mahjong.Core/ConnectRules.cs Mahjong.Core/CubeRules.cs Mahjong.Core/LayoutDefinition.cs Mahjong.Core/LayoutCatalog.cs Mahjong.Core/WinnableDealer.cs Mahjong.Core/Board.cs Mahjong.Core.Tests/CubeTests.cs
git commit -m "Core: Mahjong Cube as a game kind with Small and Large blocks

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 2: Server, names and leaderboards

**Files:**
- Create: `web/Mahjong.Web.Tests/CubeApiTests.cs`
- Modify: `web/Mahjong.Web.Client/Strings.cs` (`LayoutName`, around lines 50–62)
- Modify: `web/Mahjong.Web.Client/Resources/Strings.es.resx` (after the `Mahjong Cube` entry, line 232)
- Modify: `web/Mahjong.Web/Components/Pages/Leaderboards.razor` (lines 16–26, 82 and 89)
- Modify: `web/Mahjong.Web.Client/Components/LeaderboardDialog.razor` (the `<select>`, lines 17–29)
- Modify: `web/Mahjong.Web.Tests/SpanishTests.cs` (`NamesChosenInTheGameHaveSpanishTranslations`)

**Interfaces:**
- Consumes (from Task 1): `LayoutCatalog.Cube`, `CubeLayouts.Prefix`, `GameKind.Cube`, `LayoutCatalog.Find("Cube Small")`.
- Produces: `L.LayoutName(cubeLayout)` returns "Mahjong Cube · Small" or "Mahjong Cube · Large" (translated).

- [ ] **Step 1: Write the failing tests**

Create `web/Mahjong.Web.Tests/CubeApiTests.cs`:

```csharp
using System.Net;
using System.Net.Http.Json;
using Mahjong.Core;
using Mahjong.Web.Client.Api;

namespace Mahjong.Web.Tests;

/// <summary>Mahjong Cube games are started, verified and ranked like solitaire ones.</summary>
public sealed class CubeApiTests : IAsyncLifetime
{
    private const string Cube = "Cube Small";

    private readonly MahjongAppFactory app = new();

    public Task InitializeAsync() => app.AddUserAsync("alice", "Alice");

    public Task DisposeAsync()
    {
        app.Dispose();
        return Task.CompletedTask;
    }

    private static async Task<StartGameResponse> StartAsync(HttpClient client, StartGameRequest request)
    {
        var response = await client.PostAsJsonAsync("api/games", request);
        response.EnsureSuccessStatusCode();
        var started = (await response.Content.ReadFromJsonAsync<StartGameResponse>())!;
        (await client.PostAsync($"api/games/{started.GameId}/resume", null)).EnsureSuccessStatusCode();
        return started;
    }

    [Fact]
    public async Task ACubeWinWithAShuffleIsVerifiedAndRankedOnItsSize()
    {
        var client = app.ClientFor("alice");
        var started = await StartAsync(client, new StartGameRequest(Cube));

        var game = new MahjongGame(LayoutCatalog.Find(Cube)!, started.Seed);
        game.Shuffle();
        foreach (var (first, second) in game.Board.LastDeal.Solution)
        {
            game.Tick();
            app.Time.Advance(TimeSpan.FromSeconds(1));
            Assert.True(game.TryRemovePair(game.Board.At(first), game.Board.At(second)));
        }

        game.Finish();
        var finish = await client.PostAsJsonAsync($"api/games/{started.GameId}/finish", new FinishGameRequest(game.Record!));

        Assert.Equal(HttpStatusCode.OK, finish.StatusCode);
        var result = (await finish.Content.ReadFromJsonAsync<FinishGameResponse>())!;
        Assert.Equal((true, game.Scoring.Card.Score, 1), (result.Won, result.Score, result.Rank));
        Assert.Equal(ScoreCard.NoShuffleBonusStart - Scoring.ShufflePenalty, result.Breakdown.NoShuffleBonus);

        var small = await client.GetFromJsonAsync<List<LeaderboardEntry>>($"api/leaderboards/{Uri.EscapeDataString(Cube)}");
        Assert.Single(small!);
        var large = await client.GetFromJsonAsync<List<LeaderboardEntry>>($"api/leaderboards/{Uri.EscapeDataString("Cube Large")}");
        Assert.Empty(large!);
    }

    [Fact]
    public async Task ARandomCubeDealStartsAsARandomDeal()
    {
        var started = await StartAsync(app.ClientFor("alice"), new StartGameRequest(Cube, RandomDeal: true));

        Assert.True(started.RandomDeal);
    }

    [Fact]
    public async Task TheLeaderboardPageHasACubeTab()
    {
        string html = await app.ClientFor(null).GetStringAsync($"leaderboards?layout={Uri.EscapeDataString("Cube Large")}");

        Assert.Contains("Mahjong Cube", html);
        Assert.Contains("<option value=\"Cube Large\" selected", html);
        Assert.Contains("<option value=\"Cube Small\"", html);
    }
}
```

In `web/Mahjong.Web.Tests/SpanishTests.cs`, in `NamesChosenInTheGameHaveSpanishTranslations`, change the loop header to:

```csharp
        foreach (var layout in LayoutCatalog.Visible.Concat(ConnectLayouts.All).Concat(CubeLayouts.All))
```

- [ ] **Step 2: Run the tests to check they fail**

Run: `dotnet test E:/data/dev/repos/mahjong/web/Mahjong.Web.Tests -c Release --filter "FullyQualifiedName~CubeApiTests|FullyQualifiedName~SpanishTests"`

Expected:
- `ACubeWinWithAShuffleIsVerifiedAndRankedOnItsSize` and `ARandomCubeDealStartsAsARandomDeal` pass already. Task 1 made `LayoutCatalog.Find` know the cubes, and `GameService` uses it. That's expected.
- `TheLeaderboardPageHasACubeTab` fails, because the page falls back to Twin Peaks.
- `NamesChosenInTheGameHaveSpanishTranslations` fails on "Cube Small" and "Cube Large".

- [ ] **Step 3: Name cube layouts "Mahjong Cube · Small" and "Mahjong Cube · Large"**

In `web/Mahjong.Web.Client/Strings.cs`, in `LayoutName(this IStringLocalizer localizer, LayoutDefinition? layout)`, replace:

```csharp
        if (layout.Kind != GameKind.Connect)
        {
            return localizer[layout.Name];
        }
```

with:

```csharp
        if (layout.Kind == GameKind.Cube)
        {
            return localizer["Mahjong Cube · {0}", localizer[layout.Name[(CubeLayouts.Prefix.Length + 1)..]]];
        }

        if (layout.Kind != GameKind.Connect)
        {
            return localizer[layout.Name];
        }
```

In `web/Mahjong.Web.Client/Resources/Strings.es.resx`, add this line right after the `<data name="Mahjong Cube" ...>` line:

```xml
  <data name="Mahjong Cube · {0}" xml:space="preserve"><value>Cubo Mahjong · {0}</value></data>
```

- [ ] **Step 4: Add the Mahjong Cube tab to the leaderboards page**

In `web/Mahjong.Web/Components/Pages/Leaderboards.razor`:

1. After the Connect `<li class="nav-item">…</li>`, add:

```razor
    <li class="nav-item">
        <a class="nav-link @(game == GameKind.Cube ? "active" : null)" href="leaderboards?layout=@Uri.EscapeDataString(LayoutCatalog.Cube[0].Name)">@L["Mahjong Cube"]</a>
    </li>
```

2. Replace the label line with:

```razor
    <label for="layout" class="form-label mb-0">@(game switch { GameKind.Connect => L["Board"], GameKind.Cube => L["Board size"], _ => L["Layout"] })</label>
```

3. Replace `Choices` with:

```csharp
    // The layouts (or Connect boards, or cubes) of the game being shown.
    private IEnumerable<LayoutDefinition> Choices => game switch
    {
        GameKind.Connect => LayoutCatalog.Connect,
        GameKind.Cube => LayoutCatalog.Cube,
        _ => LayoutCatalog.Visible,
    };
```

4. In `OnInitializedAsync`, change `LayoutCatalog.Visible.Concat(LayoutCatalog.Connect)` to `LayoutCatalog.Visible.Concat(LayoutCatalog.Connect).Concat(LayoutCatalog.Cube)`.

- [ ] **Step 5: Add the Mahjong Cube group to the leaderboards dialog**

In `web/Mahjong.Web.Client/Components/LeaderboardDialog.razor`, after the Connect `</optgroup>`, add:

```razor
                <optgroup label="@L["Mahjong Cube"]">
                    @foreach (var l in Layouts.Where(l => l.Kind == GameKind.Cube))
                    {
                        <option value="@l.Name">@L.LayoutName(l)</option>
                    }
                </optgroup>
```

- [ ] **Step 6: Run the tests to check they pass**

Run: `dotnet test E:/data/dev/repos/mahjong/web/Mahjong.Web.Tests -c Release --filter "FullyQualifiedName~CubeApiTests|FullyQualifiedName~SpanishTests|FullyQualifiedName~ConnectApiTests"`

Expected: all pass.

- [ ] **Step 7: Commit**

```bash
cd E:/data/dev/repos/mahjong
git add web/Mahjong.Web.Tests/CubeApiTests.cs web/Mahjong.Web.Tests/SpanishTests.cs web/Mahjong.Web.Client/Strings.cs web/Mahjong.Web.Client/Resources/Strings.es.resx web/Mahjong.Web/Components/Pages/Leaderboards.razor web/Mahjong.Web.Client/Components/LeaderboardDialog.razor
git commit -m "Mahjong Cube leaderboards: verified cube games, a Cube tab and names

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 3: Play the cube as a scored game

**Files:**
- Create: `web/Mahjong.Web.Client/Game/CubeRedraw.cs`
- Create: `web/Mahjong.Web.Tests/CubeRedrawTests.cs`
- Modify: `web/Mahjong.Web.Client/Components/CubeBoard.razor` (rewritten)
- Modify: `web/Mahjong.Web.Client/Components/CubeBoard.razor.css` (drop the bar's styles)
- Modify: `web/Mahjong.Web.Client/wwwroot/js/cube.js` (`setBoard`, lines 110–139)
- Modify: `web/Mahjong.Web.Client/Pages/Cube.razor` (the test bench)
- Modify: `web/Mahjong.Web.Client/Pages/Play.razor`
- Modify: `web/Mahjong.Web.Client/Components/SettingsDialog.razor`
- Modify: `web/Mahjong.Web.Client/Game/LayoutPreviews.cs` (`SettingsChoice`, lines 26–30)
- Modify: `web/Mahjong.Web.Client/Game/PlayerPreferences.cs` (remove `CubeKey`, `CubeSize` and `SetCubeSizeAsync`)
- Modify: `web/Mahjong.Web.Client/Game/GameSession.cs` (remove `Clear()`)
- Modify: `web/Mahjong.Web.Client/Resources/Strings.es.resx` (remove cube-only keys)
- Delete: `web/Mahjong.Web.Client/Game/CubeGame.cs`, `web/Mahjong.Web.Tests/CubeGameTests.cs`

**Interfaces:**
- Consumes:
  - From Task 1: `CubeLayouts.ForSize(int)`, `CubeLayouts.All`, `LayoutDefinition.Size`, `LayoutCatalog.Cube` and `GameKind.Cube`.
  - From Task 2: `L.LayoutName(cube)`.
  - Existing: `GameSession.ClickAsync(Tile)`, `GameSession.Selected` (`Tile?`), `GameSession.HintedTiles` (`IReadOnlySet<int>?`) and `PlayerPreferences.SetStoneAsync(TileSetInfo)`.
- Produces:
  - `CubeRedraw.Compare(IReadOnlyDictionary<int,string> drawn, IReadOnlyDictionary<int,string> now) → (bool Redraw, int[] Removed)`.
  - `CubeBoard` parameters: `Game` (MahjongGame, required), `Selected` (Tile?), `Hinted` (IReadOnlySet<int>?), `OnTileClick` (EventCallback<Tile>), `Stone` (string), `StoneUrl` (string?) and `StoneFacesKey` (string?).
  - `cube.js` `setBoard(size, tiles, freeIds, keepView)`.
  - `SettingsChoice(LayoutName, TileSetId, BackgroundId, SkipSettings, GuaranteedWinnable, string? StoneId = null)`.

- [ ] **Step 1: Write the failing tests for how the view catches up**

Create `web/Mahjong.Web.Tests/CubeRedrawTests.cs`:

```csharp
using Mahjong.Web.Client.Game;

namespace Mahjong.Web.Tests;

/// <summary>How the cube view catches up with the game: shrink away a taken pair, or draw the block again.</summary>
public sealed class CubeRedrawTests
{
    private static readonly Dictionary<int, string> Drawn = new() { [1] = "1dot", [2] = "1dot", [3] = "2bam", [4] = "2bam" };

    [Fact]
    public void NothingChangedNeedsNothing()
    {
        var (redraw, removed) = CubeRedraw.Compare(Drawn, new Dictionary<int, string>(Drawn));

        Assert.False(redraw);
        Assert.Empty(removed);
    }

    [Fact]
    public void ATakenPairShrinksAway()
    {
        var (redraw, removed) = CubeRedraw.Compare(Drawn, new Dictionary<int, string> { [3] = "2bam", [4] = "2bam" });

        Assert.False(redraw);
        Assert.Equal(new[] { 1, 2 }, removed.Order());
    }

    [Fact]
    public void AnUndoBringsTilesBackSoTheBlockIsRedrawn()
    {
        var afterTaking = new Dictionary<int, string> { [3] = "2bam", [4] = "2bam" };
        var (redraw, removed) = CubeRedraw.Compare(afterTaking, Drawn);

        Assert.True(redraw);
        Assert.Empty(removed);
    }

    [Fact]
    public void AShuffleChangesFacesSoTheBlockIsRedrawn()
    {
        var shuffled = new Dictionary<int, string> { [1] = "2bam", [2] = "1dot", [3] = "1dot", [4] = "2bam" };
        var (redraw, removed) = CubeRedraw.Compare(Drawn, shuffled);

        Assert.True(redraw);
        Assert.Empty(removed);
    }

    [Fact]
    public void TheFirstDrawIsARedraw()
    {
        var (redraw, removed) = CubeRedraw.Compare(new Dictionary<int, string>(), Drawn);

        Assert.True(redraw);
        Assert.Empty(removed);
    }
}
```

- [ ] **Step 2: Run them to check they fail**

Run: `dotnet test E:/data/dev/repos/mahjong/web/Mahjong.Web.Tests -c Release --filter FullyQualifiedName~CubeRedrawTests`

Expected: a build error, because `CubeRedraw` doesn't exist.

- [ ] **Step 3: Add CubeRedraw**

Create `web/Mahjong.Web.Client/Game/CubeRedraw.cs`:

```csharp
namespace Mahjong.Web.Client.Game;

/// <summary>How the cube view catches up with the game after a move.</summary>
public static class CubeRedraw
{
    /// <summary>
    /// Compares the tiles drawn (id → face) with the tiles on the board now. Tiles that are only gone
    /// (a pair taken) shrink away. Any tile that's new or has a new face (an undo, a redo, a shuffle)
    /// means drawing the block again.
    /// </summary>
    public static (bool Redraw, int[] Removed) Compare(IReadOnlyDictionary<int, string> drawn, IReadOnlyDictionary<int, string> now)
    {
        bool redraw = now.Any(t => !drawn.TryGetValue(t.Key, out var face) || face != t.Value);
        return (redraw, redraw ? [] : drawn.Keys.Where(id => !now.ContainsKey(id)).ToArray());
    }
}
```

- [ ] **Step 4: Run the tests to check they pass**

Run: `dotnet test E:/data/dev/repos/mahjong/web/Mahjong.Web.Tests -c Release --filter FullyQualifiedName~CubeRedrawTests`

Expected: 5 pass.

- [ ] **Step 5: Let `setBoard` keep the view**

In `web/Mahjong.Web.Client/wwwroot/js/cube.js`:

1. Change the comment and signature of `setBoard` to:

```js
        // Draws the block: tiles = [{ id, x, y, z, face, art: { light, dark, any }, glyph: { main, caption, color } }].
        // keepView redraws it as it's being looked at (an undo or a shuffle) instead of a new deal's view.
        setBoard(size, tiles, freeIds, keepView) {
```

2. Wrap the camera lines in `if (!keepView)`, leaving `resize(); changed();` after it:

```js
            if (!keepView) {
                const distance = size * Spacing * 2.6;
                camera.position.set(distance * 0.6, distance * 0.5, distance * 0.62);
                controls.target.set(0, 0, 0);
                controls.minDistance = size * 1.2;
                controls.maxDistance = size * 5;
            }
            resize();
            changed();
```

- [ ] **Step 6: Rewrite CubeBoard to draw a MahjongGame**

Replace all of `web/Mahjong.Web.Client/Components/CubeBoard.razor` with:

```razor
@using Mahjong.Core
@using Mahjong.Web.Client.Game
@implements IAsyncDisposable
@inject IJSRuntime JS
@inject FaceArt FaceArt

@*
    Mahjong Cube: a game on a cube layout (Cube Small or Cube Large) drawn with Three.js
    (wwwroot/js/cube.js). The game, its score and its controls are the page's; this only draws the
    block, lets it be turned, and reports which tile was tapped.
*@
<div class="cube-board">
    <div class="cube-view" @ref="view" role="img" aria-label="@L["Mahjong Cube"]"></div>

    <p class="cube-help">@L["Drag to turn the cube. A tile can be taken when two of its sides that meet at an edge are open."]</p>
</div>

@code {
    private ElementReference view;
    private IJSObjectReference? module;
    private IJSObjectReference? scene;
    private DotNetObjectReference<CubeBoard>? self;
    // The tiles the script is drawing (id → face), and whether it has drawn the block yet.
    private IReadOnlyDictionary<int, string> drawn = new Dictionary<int, string>();
    private bool drawnOnce;
    private string? shownState;
    private (string?, string?) shownStone;
    private bool disposed;

    [Parameter, EditorRequired] public MahjongGame Game { get; set; } = default!;

    [Parameter] public Tile? Selected { get; set; }

    [Parameter] public IReadOnlySet<int>? Hinted { get; set; }

    [Parameter] public EventCallback<Tile> OnTileClick { get; set; }

    /// <summary>The stone (TileSets.Stones' Stone), or a picture's name with <see cref="StoneUrl"/>.</summary>
    [Parameter] public string Stone { get; set; } = TileSets.DefaultStone.Stone!;

    /// <summary>Development: a stone picture to try, from the cube-stones folder.</summary>
    [Parameter] public string? StoneUrl { get; set; }

    /// <summary>The picture's file name when trying one, for its configured face set.</summary>
    [Parameter] public string? StoneFacesKey { get; set; }

    // The page redraws every clock tick; only talk to the script when something on the cube has changed.
    protected override bool ShouldRender() => scene == null || State() != shownState || (Stone, StoneUrl) != shownStone;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // A new game replaces the cube (it's keyed to the game), so this one may be thrown away
            // while it's still starting: after each step, stop if it has been.
            await FaceArt.LoadAsync();
            if (disposed)
            {
                return;
            }

            self = DotNetObjectReference.Create(this);
            module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/cube.js");
            if (disposed)
            {
                await ReleaseAsync();
                return;
            }

            scene = await module.InvokeAsync<IJSObjectReference>("create", view, self);
            if (disposed)
            {
                await ReleaseAsync();
                return;
            }
        }

        if (scene == null)
        {
            return;
        }

        if ((Stone, StoneUrl) != shownStone)
        {
            shownStone = (Stone, StoneUrl);
            await scene.InvokeVoidAsync("setStone", Stone, StoneUrl, FaceArt.StoneFaces(StoneFacesKey ?? Stone));
        }

        string state = State();
        if (state == shownState)
        {
            return;
        }

        shownState = state;
        var now = Game.Board.Tiles.ToDictionary(t => t.Id, t => t.Face.Name);
        var (redraw, removed) = CubeRedraw.Compare(drawn, now);
        if (redraw)
        {
            await scene.InvokeVoidAsync("setBoard", Game.Layout.Size, TilesForScript(), FreeIds(), drawnOnce);
            drawnOnce = true;
        }
        else if (removed.Length > 0)
        {
            await scene.InvokeVoidAsync("remove", removed);
        }

        drawn = now;
        await scene.InvokeVoidAsync("update", Selected?.Id, FreeIds(), Hinted?.ToArray() ?? []);
    }

    // What's on the cube: each tile's id and face, the selection and the hint.
    private string State()
    {
        var text = new System.Text.StringBuilder();
        foreach (var t in Game.Board.Tiles.OrderBy(t => t.Id))
        {
            text.Append(t.Id).Append(':').Append(t.Face.Name).Append(';');
        }

        text.Append('|').Append(Selected?.Id).Append('|');
        if (Hinted != null)
        {
            text.AppendJoin(',', Hinted.Order());
        }

        return text.ToString();
    }

    // Each tile with its cell, its pictures and its drawn symbol.
    private List<object> TilesForScript() => Game.Board.Tiles.Select(t =>
    {
        var glyph = TileSets.Glyph(t.Face.Name);
        return (object)new
        {
            id = t.Id, x = t.Position.X, y = t.Position.Y, z = t.Position.Z, face = t.Face.Name,
            art = FaceArt.Pictures(t.Face.Name),
            glyph = new { main = glyph.Main, caption = L[glyph.Caption].Value, color = glyph.ColorClass },
        };
    }).ToList();

    private int[] FreeIds() => Game.Board.Tiles.Where(Game.Board.IsFree).Select(t => t.Id).ToArray();

    [JSInvokable]
    public async Task OnTileTapped(int id)
    {
        if (Game.Board.FindTile(id) is { } tile)
        {
            await OnTileClick.InvokeAsync(tile);
        }
    }

    public async ValueTask DisposeAsync()
    {
        disposed = true;
        await ReleaseAsync();
    }

    // Lets go of the script's cube and the module; safe to call more than once.
    private async Task ReleaseAsync()
    {
        var (releasedScene, releasedModule) = (scene, module);
        (scene, module) = (null, null);
        try
        {
            if (releasedScene != null)
            {
                await releasedScene.InvokeVoidAsync("dispose");
                await releasedScene.DisposeAsync();
            }

            if (releasedModule != null)
            {
                await releasedModule.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
        }

        self?.Dispose();
        self = null;
    }
}
```

In `web/Mahjong.Web.Client/Components/CubeBoard.razor.css`:
- delete the `.cube-bar`, `.cube-count` and `.cube-message` rules
- change the top comment to `/* Mahjong Cube: the 3D view filling the board area, with a line of help under it. */`

- [ ] **Step 7: Make the test bench run a MahjongGame**

In `web/Mahjong.Web.Client/Pages/Cube.razor`:

1. Add `@using Mahjong.Core` under `@using Mahjong.Web.Client.Game`.
2. Change the comment's first sentence to: `Test bench for Mahjong Cube (the scored game is played from the Play page's Mahjong Cube tab, through the same CubeBoard).`
3. Change the two size buttons' `@onclick` to `() => SetSize(4)` and `() => SetSize(6)`.
4. Just before the `<select class="cube-stone" …>`, add:

```razor
        <span>@L["{0} left", game.Board.Count]</span>
        <button type="button" class="btn btn-sm btn-primary" @onclick="NewGame">@L["New game"]</button>
```

5. Replace `<CubeBoard Size="size" Stone="@stone" StoneUrl="@StoneUrl" StoneFacesKey="@StoneFacesKey" />` with:

```razor
        <CubeBoard @key="game" Game="game" Selected="selected" OnTileClick="Click" Stone="@stone" StoneUrl="@StoneUrl" StoneFacesKey="@StoneFacesKey" />
```

6. In `@code`, add after `private int size = 4;`:

```csharp
    // A plain game on the block: no clock, score or server, just taking pairs.
    private MahjongGame game = new(CubeLayouts.ForSize(4));
    private Tile? selected;

    private void SetSize(int newSize)
    {
        size = newSize;
        NewGame();
    }

    private void NewGame()
    {
        game = new MahjongGame(CubeLayouts.ForSize(size));
        selected = null;
    }

    private void Click(Tile tile)
    {
        if (!game.Board.IsFree(tile))
        {
            return;
        }

        if (selected == null || selected == tile)
        {
            selected = selected == tile ? null : tile;
        }
        else if (game.TryRemovePair(selected, tile))
        {
            selected = null;
        }
        else
        {
            selected = tile;
        }
    }
```

- [ ] **Step 8: Make the Cube tab choose a cube layout and bring back Deal**

In `web/Mahjong.Web.Client/Game/LayoutPreviews.cs`, replace the `SettingsChoice` doc comment and record with:

```csharp
/// <summary>What the player chose in the settings dialog.</summary>
/// <param name="StoneId">For Mahjong Cube: the stone chosen for it (the tile set of the other games stays as it was).</param>
public sealed record SettingsChoice(string LayoutName, string TileSetId, string BackgroundId, bool SkipSettings, bool GuaranteedWinnable,
    string? StoneId = null);
```

In `web/Mahjong.Web.Client/Components/SettingsDialog.razor`:

1. Remove the `@if (tab != GameTab.Cube)` line, with its `{` and `}`, around the Deal `<section>`. Keep the section and un-indent it one level.
2. In the `GameTab` doc comment, change `(casual: no score yet), with a size and the same stones.` to `with a size (Cube Small or Cube Large), its own leaderboards and the same stones.`
3. Delete the `CubeSize` parameter and its doc comment.
4. In `OnInitialized`, replace `tab = CubeSize != null ? GameTab.Cube` with `tab = chosen?.Kind == GameKind.Cube ? GameTab.Cube`, and replace `cubeSize = CubeSize ?? 4;` with `cubeSize = chosen?.Kind == GameKind.Cube ? chosen.Size : 4;`.
5. In `ApplyAsync`, replace the `if (tab == GameTab.Cube) { … }` block with:

```csharp
        if (tab == GameTab.Cube)
        {
            // The block of the chosen size, in the chosen stone; the other games' tile set stays as it is.
            return OnApply.InvokeAsync(new SettingsChoice(CubeLayouts.ForSize(cubeSize).Name, TileSets.Find(TileSetId).Id, backgroundId, skipSettings, winnable,
                StoneId: stoneId));
        }
```

- [ ] **Step 9: Play the cube as a session game on the Play page**

In `web/Mahjong.Web.Client/Pages/Play.razor`:

1. **Title.** Replace the `@if (cubeSize is { } titleSize) { … } else { <text>@L.LayoutName(PlayingLayout)</text> }` block with:

```razor
                    <text>@L.LayoutName(PlayingLayout)</text>
```

2. **Notice.** Delete the whole `@if (pendingCubeSize != null) { <div class="notice info"> … </div> }` block.

3. **Board.** Replace:

```razor
                @if (cubeSize is { } size)
                {
                    <CubeBoard @key="cubeDeal" Size="size" Stone="@Preferences.LastStone.Stone" />
                }
                else if (Session.Game is { } game)
```

with:

```razor
                @if (Session.Game is { } game)
```

Then, inside `board-area`, add this branch between the `Paused` branch and the `TileSetKind.ThreeD` branch:

```razor
                        else if (game.Layout.Kind == GameKind.Cube)
                        {
                            @* Mahjong Cube: always drawn in stone, in the stone chosen for 3D. *@
                            <CubeBoard @key="game" Game="game" Selected="Session.Selected" Hinted="Session.HintedTiles" OnTileClick="Session.ClickAsync"
                                       Stone="@(Preferences.LastStone.Stone!)" />
                        }
```

4. **Dialog.** In the `<SettingsDialog …>` tag, delete the line `CubeSize="@(pendingCubeSize ?? cubeSize)"`.

5. **Layouts.** Replace the `AllLayouts` line and its comment with:

```csharp
    // Solitaire layouts, Connect boards and the cubes: anything the player can choose.
    private IEnumerable<LayoutDefinition> AllLayouts => layouts.Concat(LayoutCatalog.Connect).Concat(LayoutCatalog.Cube);
```

6. **Fields.** Delete the `cubeSize`, `pendingCubeSize` and `cubeDeal` fields and their two comment lines.

7. **Start-up.** Delete `// Back to the cube if it was the last game played.` and `cubeSize = Preferences.CubeSize;` (and the blank line after them).

8. **New game.** In `NewGameAsync`, delete the `if (pendingCubeSize != null) { … }` block and the `if (cubeSize != null) { … return; }` block.

9. **Settings.** In `ApplySettingsAsync`, replace everything from `if (choice.CubeSize is { } size)` through the closing `}` of `if (cubeSize != null) { … settingsForNewGame = true; }` with:

```csharp
        // The cube's stone (the Cube tab), kept apart from the other games' tile set.
        if (choice.StoneId != null && TileSets.Find(choice.StoneId) is { Kind: TileSetKind.ThreeD } stone && stone != Preferences.LastStone)
        {
            await Preferences.SetStoneAsync(stone);
        }
```

10. **Cube code.** Delete `ApplyCubeAsync` and its comment.

11. **Cancel.** In `CancelSettingsAsync`, change `if (Session.Game == null && cubeSize == null)` to `if (Session.Game == null)`.

- [ ] **Step 10: Remove the casual cube's leftovers**

1. In `web/Mahjong.Web.Client/Game/PlayerPreferences.cs`:
   - delete `private const string CubeKey = "cube";`
   - delete the `CubeSize` property and its doc comment
   - delete the `CubeSize = await browser.GetSettingAsync(CubeKey) …` line in `LoadAsync`
   - delete `SetCubeSizeAsync` and its doc comment
2. In `web/Mahjong.Web.Client/Game/GameSession.cs`, delete `Clear()` and its doc comment.
3. Delete the old casual cube files:

   ```bash
   git rm web/Mahjong.Web.Client/Game/CubeGame.cs web/Mahjong.Web.Tests/CubeGameTests.cs
   ```

   Their rules are now in `Mahjong.Core/CubeRules.cs`, and their tests are in `CubeTests`.
4. In `web/Mahjong.Web.Client/Resources/Strings.es.resx`, delete the entries for:
   - `You cleared the cube!`
   - `Your next game will be Mahjong Cube.`
   - `No pairs can be taken. Start a new game.`

   First grep each key under `web/` to confirm no `.razor` or `.cs` file uses it any more. Keep `{0} left` and `Prototype`, which the test bench still uses.

- [ ] **Step 11: Build and run all the tests**

Run:

```bash
dotnet build E:/data/dev/repos/mahjong/web/Mahjong.Web/Mahjong.Web.csproj -c Release
dotnet test E:/data/dev/repos/mahjong/web/Mahjong.Web.Tests -c Release
dotnet test E:/data/dev/repos/mahjong/Mahjong.Core.Tests -c Release
```

Expected:
- The build has no errors and no new warnings.
- All web tests pass. That's 160 before this work, minus the 3 deleted `CubeGameTests` tests (count the `[Fact]` and `[InlineData]` lines in the file before deleting it), plus 3 `CubeApiTests` and 5 `CubeRedrawTests`.
- All core tests pass.

- [ ] **Step 12: Commit**

```bash
cd E:/data/dev/repos/mahjong
git add web/Mahjong.Web.Client/Game/CubeRedraw.cs web/Mahjong.Web.Tests/CubeRedrawTests.cs web/Mahjong.Web.Client/Components/CubeBoard.razor web/Mahjong.Web.Client/Components/CubeBoard.razor.css web/Mahjong.Web.Client/wwwroot/js/cube.js web/Mahjong.Web.Client/Pages/Cube.razor web/Mahjong.Web.Client/Pages/Play.razor web/Mahjong.Web.Client/Components/SettingsDialog.razor web/Mahjong.Web.Client/Game/LayoutPreviews.cs web/Mahjong.Web.Client/Game/PlayerPreferences.cs web/Mahjong.Web.Client/Game/GameSession.cs web/Mahjong.Web.Client/Resources/Strings.es.resx
git commit -m "Mahjong Cube is scored: played as a session game with the usual controls

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

(The `git rm` in Step 10 already staged the two deletions.)

---

### Task 4: Check it running

**Files:** none changed, unless a check fails. A failure is fixed in the task that owns the code, re-tested, and committed separately.

- [ ] **Step 1: Start the test copy on port 5031**

Stop any earlier test copy on 5031. Then, in PowerShell:

```powershell
Start-Process -WindowStyle Hidden dotnet -ArgumentList 'run','-c','Release','--no-build','--project','E:/data/dev/repos/mahjong/web/Mahjong.Web/Mahjong.Web.csproj','--launch-profile','http','--urls','http://localhost:5031'
```

Wait until `http://localhost:5031/` answers 200.

- [ ] **Step 2: Check the pages in Chrome**

Use automation through the DOM; screenshots are stale in a hidden tab. Check:
- On `/`, the new-game dialog's Cube tab shows Board size, Stone, **Deal** and Background.
- Choosing Large and starting the game:
  - shows the title "Mahjong Cube · Large"
  - shows the score bar
  - enables the toolbar's shuffle, undo, redo, hint, demo and pause buttons
  - shows a `canvas` in `.cube-view`
- Hint lights two tiles. Shuffle redraws the cube. Pause shows "Paused", and resume brings the cube back with the same tile count.
- Demo plays to the end, and the game shows as won.
- After a reload, the game is a cube again (`LastLayout` = "Cube Large").
- `/leaderboards?layout=Cube%20Small` shows the Mahjong Cube tab, with Small and Large.
- The game page's leaderboard dialog lists the Mahjong Cube group.
- `/cube` (the test bench) shows a cube and a tile count, and New game deals again.
- The console shows no errors (`read_console_messages`, pattern `error|Error|exception`).

- [ ] **Step 3: Stop the test copy, and ask the user to play by hand**

Stop the test copy on 5031. Tell the user what was checked. Ask them to restart their 5011 copy (Razor and C# changed) and play one cube game by hand, because tile taps can't be automated:
- take a few pairs
- undo once (the pair comes back and the view doesn't jump)
- shuffle (the faces change and the view stays)
- finish the game, or let the demo do it
- check the results dialog

Merge to main only when they say so.
