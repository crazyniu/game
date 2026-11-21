# Task center, combo, skin gifts and the flat UI: report

Scope and rules: [spec.md](spec.md). Branch `task-center`.

| Change | Files |
| --- | --- |
| Combo `xN +5` beside the house; the played world's birds cheer the win | `game/BoardView.ts`, `game/Popups.ts`, `Boot.ts` |
| Task rules, save schema 7, counters, common skins (week chest, world first clear, check-in day 7) | `meta/Tasks.ts`, `meta/Profile.ts`, `meta/Progress.ts`, `core/GameSession.ts`, `Boot.ts` |
| Task page in fishsort's layout | `game/TaskView.ts` |
| Flat kit, game font, twelve languages | `game/Flat.ts`, `game/I18n.ts`, `i18n/strings.csv`, `tools/ui/{strings.py,font_subset.py}`, `resources/{art/flat,font,i18n}` |
| Home, settings, languages, free feathers; shared Kit helpers on the flat kit | `game/HomeView.ts`, `game/Popups.ts`, `game/UI.ts`, `game/MapView.ts`, `game/ShopView.ts`, `game/AviaryView.ts`, `game/Tutorial.ts` |
| Checks: Labels only in `Flat.text`; ttf metas; capture languages and the task page | `ci/check_painted_words.py`, `tools/cocos/meta.mjs`, `scripts/capture.sh` |

## Verification (2026-11-21)

- `./run.sh check` passes on Linux (11 core tests, painted-word guard, names). Web build and full Creator typecheck pass on the Mac
  (`/Volumes/Lexar/projects/game-task-center`).
- Captured in the built game: home (en, zh, ja, ru, ko), settings at home and in a level, language list, free feathers, task page
  (real save and preview), aviary list and detail, shop, map strip and world page, daily reward, lucky draw, star chest, win, level
  card, new bird, level 5 board, combo pop. Every character of the twelve languages is in the fonts.

Not checked: tapping through on a phone, the task counters over a real day of play, the week reset, WeChat fonts, native builds.
