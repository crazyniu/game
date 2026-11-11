# Modal material repair delivery

## Implemented

- `PerchSort/game/popup.gd`: explicit reusable wooden frame at source corner scale (previous shared
  shape routing used 0.75 scale), and a compact native-proportion wooden name plank with a clear
  title area. Every frame piece stays under `Panel`; every title piece stays under `TitleGroup`.
- `PerchSort/game/popups.gd`, Win only: place the hero bird's feet at the plank top, and combine
  moves and coin rewards into one reading row. Stars, live birds, booster badges, count-up,
  rays, confetti, action hierarchy and disabled Next remain on the existing paths.
- Settings only: replace the separate simple cream card with the same wooden frame. Four compact
  rows retain Music, Sound, Vibration and Language; use existing painted title/row/action words,
  the painted close tile and equal action pills in one row. Retain the existing switches and
  language selector, callbacks, in-place locale refresh and quick modal transition.
- Added `docs/design/modal-materials.md` and this subject's spec/report. No art was generated,
  copied from a design, modified or overwritten. No shared-helper request is needed.

## Compatibility and shared work

The existing Modal signatures (including the working-tree `decorate_title` parameter), `closed`
signal, Panel/TitleGroup/Close node names, absolute content coordinate convention, metadata and
open/close tween ownership are retained. Settings now records its own displayed frame and group/action
coordinates. The base frame and title can still be removed by ShopSkin, draw and chest owners.

The `popups.gd` edit acquired `build/ui-page-locks/popups.lock` atomically, recorded its owner,
reread current content, replaced only the Win/Settings block and released in `finally`. Draw, chest,
generic rewards, shared content/motion helpers and previews were not edited. Existing unrelated
uncommitted work is retained. Home, rules, economy, levels, saves, audio and locale manifests were
not edited. No pull, reset, stash, branch switch, commit, merge, publish or release was performed.

## Inspection and acceptance boundary

Read-only inspection used `cat`, `sed`, `rg`, `git status --short`, `git diff -- PerchSort/game/popup.gd`
and `view_image` on existing production artwork. Source inspection confirmed the ownership boundaries,
existing painted-label entries and the callers that replace base panel/title nodes. These are source
observations, not runtime acceptance.

As instructed, no tests, lint, naming checks, builds, exports, game launches, UI automation, screenshots,
timed captures or temporary validation programs were written or run. Runtime layout and lettering fit,
interaction, localization behavior, animation timing, audio, accessibility and device behavior are
NOT ASSESSED. User approval and any new runtime visual baseline remain pending.

The unchanged previews include `win_three_stars`, `win_two_stars`, `win_one_star`, `win_zero_reward`,
`win_milestone`, `settings_sound_on` and `settings_muted`. Actual Settings navigation supplies the
existing language callback; this task does not expand or repair the shared preview data.
