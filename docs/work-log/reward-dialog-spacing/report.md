# Reward dialog spacing report

## Changes
- Win results now use an 880-wide frame and a 72-pixel quiet footer, replacing 960/168. Stars, painted labels, birds and action sizes are not reduced.
- Generic reward grids choose 840 or 880 width according to distinct reward count; existing row calculation still handles larger prize sets.
- New-bird and tool-unlock frames use 880 and 900 respectively. World-gate frames retain 960 for animated doors and only lose excess footer space.
- Defaults and dense level-entry allocation are unchanged. Purchase dialogs, settings, chest/draw-specific shells and all game/economy rules are untouched.
- Family assertions now follow the existing content-width-aware single-action sizing introduced before this change, instead of the obsolete primary constant.

## Verification
- Real desktop OpenGL captures: 18 Chinese/English screenshots under ignored `build/reward-spacing`, covering zero-reward/milestone/normal wins, daily rewards, chest baseline, new bird, tool unlock and world gates. Reviewed content fit and exterior action clearance.
- Jungle contract: 531 checks, zero failures.
- Family contract: 1330 checks, zero failures.
- Gameplay dialog contract: 84 checks, zero failures.
- New spacing contract: 154 checks, zero failures, including seven distinct reward types across three rows.
- Live reward-motion scene: 110 checks, zero failures. Duplicate activation, interrupted claim/win, reward counting and navigation callbacks remain covered.
- Core suite: exit 0. Existing intentional validator error messages remain in its log.
- Runtime screenshots/logs are local evidence only, not repository assets.

No merge or deployment performed. Existing other PRs are not incorporated by overwrite.
