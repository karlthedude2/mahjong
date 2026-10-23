# How-to-play pages

Date: 2026-10-22
Context: AdSense turned the site down for "low value content". The home page now has readable text, `robots.txt` and a sitemap (2026-10-22). These pages are the next step: real, useful content that people search for.

## Why

- Players: clear rules with pictures help a newcomer start, and help a returning player understand Mahjong Cube and Connect, which are less familiar than Solitaire.
- Search and ads crawlers: substantial, original, server-rendered text on pages with their own addresses, in English and Spanish.

## Goals

- **Five pages per language:**
  - `/how-to-play`: a hub page introducing the four games and linking to each.
  - `/how-to-play/mahjong-solitaire`, `/how-to-play/mahjong-3d`, `/how-to-play/mahjong-cube`, `/how-to-play/connect`.
- **Spanish at its own addresses:** `/es/how-to-play` and `/es/how-to-play/<game>`. The URL slugs stay English for now *(my call)*.
- **Pictures and plenty of relevant text:** each game page reads properly on its own, roughly 700–1,200 words.
- **Easy to reach:**
  - A **?** icon in the header.
  - A "How to play" link in the settings ⓘ note, for the chosen game.
  - Links from the home page's "About Mahjong Haus" text.
- **All ten pages in the sitemap.**

## Non-goals

- Layout pages (decided against: little value to a player).
- Other languages now. The structure must make adding one straightforward (a new prefix and translated page text).
- Changing the game's rules or screens, apart from the settings note's link and the header icon.

## Anti-goals

- Filler or keyword-stuffed text. Every paragraph should help a player.
- Rules that don't match the game. Check them against Mahjong.Core and the client before writing. For example, tiles match only an identical twin, flowers and seasons included.
- Losing a game in progress by tapping help (see "Links").
- A Spanish page Google can't tell is the translation of an English one.

## Page content

Every page is server-rendered (static SSR, like `Privacy.razor`) and written out in full in each language, not assembled from `L[...]` fragments. One `<h1>`, sensible `<h2>`/`<h3>` sections, real paragraphs and lists.

**The hub, `/how-to-play`:**
- A short introduction to mahjong solitaire and the site.
- One card per game: its picture, two or three sentences, and a link to its page.
- A few lines that apply to every game: tiles and suits; matching; hints, undo and shuffle; guaranteed winnable deals; scores and leaderboards.

**Each game page has these sections, in this order:**
1. **What it is:** a short introduction, and how this game differs from the others.
2. **The goal.**
3. **The tiles:** suits (bamboo, dots, characters), winds, dragons, flowers and seasons. Each tile matches only its identical twin. A small grid of real tile pictures. The hub and each game page may share this section's wording.
4. **The rules:** when a tile is free (Solitaire and 3D), when a cube tile can be taken (Cube), or the path rule and gravity (Connect). Each rule is illustrated.
5. **How to play here:** choosing the game, layout, tiles and background in settings; tapping tiles; hints, undo and shuffle; pausing; for 3D and Cube, turning the view.
6. **Scoring and leaderboards:** what's scored on this site (time, speed and pace bonuses, any no-shuffle bonus). Check it against the scoring code. Per-layout leaderboards; signing in to keep wins.
7. **Tips and strategy:** practical, game-specific advice. For Solitaire, for example: free the long rows' end tiles, unlock tall stacks first, and don't take a pair when the other two of its kind stay blocked.
8. **FAQ:** three to six short questions and answers ("Can every game be won?", "What does the hint cost?", ...).
9. **A "Play now" link** to the game, opening it with that game chosen if the game page supports that cheaply. Otherwise a plain link to the home page *(builder decides)*.

## Pictures

- **Board pictures:** one per game, taken from the real game.
  - Mahjong Solitaire: a layout in Classic tiles.
  - Mahjong 3D: the same layout in stone.
  - Mahjong Cube.
  - A Connect board, ideally with its path line showing.
  - About 800×600, WebP, in `wwwroot/how-to-play/`, each with descriptive alt text in the page's language.
  - How they're captured is left to the builder; for 3D and Cube, a snapshot mode like the picker pictures' is a natural route.
- **Rule diagrams:** small inline SVG in the page, so they're sharp, light and translatable. At least:
  - free and blocked tiles, with a tick or cross on each (Solitaire, 3D)
  - a matching pair, and a pair that doesn't match
  - a Cube tile with two open sides meeting at an edge, next to one that can't be taken
  - a Connect line with zero, one and two turns, and one with three, which is not allowed
  - gravity, with tiles sliding down or left to fill a gap
- **Tile grid:** real tile PNGs from the Classic set, one of each kind.
- The diagrams use the site's colours and are readable on a phone.

## Languages and addresses

- **The address decides the language.** On pages under `/es/` the page renders in Spanish, header and footer included, whatever the cookie or browser says. The English addresses render in English. *(My call: one address, one language, as Google expects.)* This is done with a request culture provider that reads the `/es` prefix ahead of the cookie.
- **Every page's `<head>`** has a canonical link to itself, plus `hreflang` alternates (`en`, `es` and `x-default` → English) pointing at both versions.
- **The header's EN/ES switch** on these pages goes to the same page in the other language (setting the cookie as today).
- **Adding a language** later means adding its prefix to the provider's list and writing its page text.

## Links

- **Header:** a **?** icon (an icon link with a tooltip, like the others), labelled "How to play" / "Cómo jugar". It goes to the hub in the current language.
- **Settings:** the ⓘ note next to the game gets a "How to play" link to the chosen game's page in the current language.
- **On the game page, both open in a new tab** (`target="_blank"`, with `rel="noopener"`), so a game in progress is never lost. Elsewhere the header icon opens in the same tab. *(My call.)*
- **The home page's About text** links each game name to its page, plus a "How to play" link to the hub.
- **Every how-to-play page** links to the other games' pages and back to the hub.

## Decisions after review

- **Spanish goes live with English.** The reviewer reads the live pages, and the proofreading guide gets a step for them. AdSense won't review the site again for a while, so there's time to fix the Spanish before then. *(The user's decision.)*
- **Visiting a help page sets the site's language** (the culture cookie) to that page's language, so a Spanish reader who taps "Play now" gets the game in Spanish. *(My call; the user didn't pick, so this is the recommended option.)*
- **Pictures:**
  - Solitaire and Connect are drawn in the page itself by the game's own flat board component, server-rendered, with Connect showing its path line.
  - Mahjong 3D and Cube are snapshots saved from the development picture page as 800×600 PNGs. *(My call: PNG, so the existing saver's checks apply.)*

## Sitemap and robots

- **The sitemap** gains all ten pages (`/how-to-play`, its four game pages, and the `/es/` twins).
- **`robots.txt`** is unchanged; it already allows them.

## Tests

- **Each of the ten pages** returns 200 with its `<h1>`, the right `lang`, a canonical link to itself and `hreflang` links to both versions.
- **An English page** stays English even with a Spanish cookie, and a `/es/` page is Spanish without one.
- **The sitemap** lists all ten.
- **The header** has the How to play link, and on the game page it opens in a new tab.
- **The settings note** includes the link. This is checked in the browser if no component test fits.
- **The existing Spanish-translation test** still passes. New `L[...]` keys (the header label, the settings link) get Spanish entries.

## Left to the builder

- The exact wording, within the section list and the checked rules, written for players, not crawlers.
- How the board pictures are captured.
- The page components' structure (one per game per language, or shared parts).
- The diagrams' exact drawing.
