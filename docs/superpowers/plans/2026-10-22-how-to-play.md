# How-to-play Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans. Steps use checkbox (`- [ ]`) syntax.

**Goal:** Five how-to-play pages in English and five in Spanish under `/es/`, server-rendered, with pictures, linked from the header, settings and home page, and listed in the sitemap.

**Architecture:**
- A request culture provider makes the path decide the language for help pages.
- A small middleware sets the culture cookie on those visits.
- The pages are static SSR components in `Mahjong.Web/Components/HowToPlay/`: a hub and a game page (each with both routes), the content written out in full per language, and shared bilingual diagram and tile-guide components.
- Solitaire and Connect pictures are server-rendered `Board` components. The 3D and Cube pictures are PNG snapshots saved from the dev picture page.

**Spec:** `docs/superpowers/specs/2026-10-22-how-to-play-design.md`

## Global Constraints

- **Pages:** `/how-to-play`, `/how-to-play/{mahjong-solitaire|mahjong-3d|mahjong-cube|connect}`, and the same under `/es/`. Any other game slug returns 404.
- **The address decides the language** on these pages, and a visit sets the culture cookie to it.
- **Every page has** one `<h1>`, `lang`, a canonical link, and `hreflang` en, es and x-default.
- **Facts are checked against the code:**
  - free tile: nothing overlapping on the layer above, and the left or right side open
  - Cube: open sides on two different axes
  - Connect: at most two turns through empty cells, around the edge allowed; boards 12×6, 14×8, 18×8
  - tile points: suits 10, dragons and the peacock 20, winds, flowers and seasons 40
  - each tile matches only its identical twin
  - no-shuffle bonus 3,000, less 1,000 per shuffle and 500 per hint
  - bonus clock 5:00; pace bonus every 20 tiles; speed bonus for 20 tiles inside a 30-second window, 100 per second left
  - time bonus 30 per second left; quick finish 5,000, dropping after 2:40, paid only under 5:00
- Stage files by name only.

## Review Focus

1. A Spanish cookie on an English help page still shows English, and vice versa (Task 1 test).
2. An unknown game slug returns 404 in both languages (Task 1 test).
3. The header's EN/ES switch on a help page lands on the twin page (Task 1 test on TopNav's link).
4. The help links on the game page open in a new tab, so no game is lost (Task 4 test / browser).
5. The pages read well on a 375px phone: diagrams scale, the boards fit (Task 5).

---

### Task 1: Addresses, language, head links, sitemap

- [ ] Tests first in `HowToPlayTests`:
  - all ten pages return 200 with the right `lang`, an `<h1>`, a canonical link and hreflang links
  - English with a Spanish cookie stays English
  - `/es/` with no cookie is Spanish
  - a visit sets the cookie
  - an unknown slug returns 404
  - the sitemap lists all ten
  - the language switch link on a help page points at its twin
- [ ] Implement:
  - `HelpCultureProvider` (first in the provider list) and the cookie middleware
  - the routes, with placeholder content
  - `HelpHead` for canonical and hreflang
  - the TopNav twin link
  - the sitemap entries
- [ ] Tests pass. Commit.

### Task 2: English pages and pictures

- [ ] Shared components:
  - `TileGuide`: real Classic tile images, one of each kind, labelled
  - the diagrams `FreeTiles`, `Matching`, `CubeFree`, `ConnectPaths`, `Gravity`: inline SVG, labels by language
  - `HelpNav`: links to the other games and the hub
- [ ] Hub and four game pages in English, following the spec's sections.
- [ ] Pictures:
  - Solitaire and Connect: a server-rendered `Board` with a fixed deal (Connect with a `Path`).
  - 3D and Cube: add `snapshot()` to cube.js, an "Instructions pictures" button on `/dev/layout-previews`, and `POST /dev/how-to-play-pictures/{name}` (names `mahjong-3d` and `mahjong-cube`; 800×600 PNG; saved to `wwwroot/how-to-play/`).
  - Generate the pictures and commit them.
- [ ] Tests pass. Commit.

### Task 3: Spanish pages

- [ ] Write the five Spanish texts (es-MX, *tú*, the game's own terms: Fichas, Tablero, Solución garantizada, …). Tests pass. Commit.

### Task 4: Links

- [ ] Header **?** icon (`L["How to play"]`, es "Cómo jugar") to the hub in the current language. On the game page it opens in a new tab.
- [ ] Settings ⓘ note: a link to the chosen game's page (new tab).
- [ ] Home page About text: links to the hub and each game page.
- [ ] Spanish resx entries. Tests pass. Commit.

### Task 5: Browser check, review, ship

- [ ] Check every page at 375px and on desktop, in both languages: pictures, diagrams, and the links from the header, settings and home.
- [ ] Fresh reviewer; fix Critical/Important. Merge, push, check live.
- [ ] Add a step to the Spanish proofreading guide with the five `/es/` links.
