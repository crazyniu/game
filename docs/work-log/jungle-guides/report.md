# Jungle guide dialogs report

## Changes

- Opted booster unlock, tutorial/lesson, stuck and undo/tool offer constructors into the existing JungleDialog. Shared shell and artwork are unchanged.
- Removed unlock sparkle/halo and offer product glow. Retained item illustrations, description lettering, prices, +1 count and callbacks.
- Matched the tutorial scene's rounded inner surround to the green frame; retained the complete animated teaching scene.
- Chose Next/Play/Got it lettering from tutorial state instead of node names. Godot temporarily renames duplicate action nodes while their predecessors await deletion; deriving wording from the node name could incorrectly show OK for Next.

## Verification

`run_guide_dialogs.gd` covers both languages, all six unlocks, four tutorial steps, seven lessons, undo purchase/ad and stuck rescue. Checks include shared frame, action bounds outside the panel, title presence, sequential tutorial navigation and repeated-click callback deduplication.

Runtime screenshots are local under build/screens, not committed. No gameplay data, rules, profile schema or economy changed. No commit or push performed. First-use tool spotlight is an in-board teaching overlay, not a framed modal, and remains unchanged. Integration and aggregate validation belong to the coordinator.

## Final local result

- Guide dialog checks: 309 checks, 0 failures. Existing first-use tool guide regression: 56 checks, 0 failures.
- Naming guard and its five self-tests passed; painted words guard reports 0 problems; `git diff --check` passed.
- Captured 40 actual 1080x1920 OpenGL frames across 20 states in English and Chinese. Reviewed overview and representative full-resolution unlock, undo, tutorial and mechanic lesson images; no overlap/clipping in the changed dialog surfaces.
- Captured two 3-second motion sequences (12 frames each) for a tutorial and item unlock. All 11 consecutive frame pairs in each sequence differ; the tutorial scene and illustration still animate. Exit during a live sequence reports one resource still in use and two ObjectDB instances; this abrupt-capture shutdown warning remains, and is not reported as a clean runtime shutdown.
- Added a separate Chinese `white_tutorial_next` sprite reading 下一步, AI-generated with the existing 下一关 painting as style reference (ivory chunky Chinese, dark forest-green outline, transparent background). No existing accepted art was altered. English uses an alias to its existing Next sprite. Both manifest entries are tutorial-specific, leaving level-completion Next unchanged. Final Chinese tutorial screenshots verify legibility and visual centering.

Generation used the built-in image tool; source output `exec-25321ada-ea7b-4dcf-9b3a-483cac84a9c5.png` was copied unchanged into the Chinese locale. Runtime RGBA is 2017x780 with alpha spanning 0–255. Prompt requested exact three characters 下一步, one horizontal line, matched chunky ivory/bevel/dark-green-outline styling, no button or scenery, true transparent background.

## Follow-up visual audit

The six unlock illustrations still used legacy `booster_*_illus` art (including an old light bulb for Firefly). Unlock cards now read the shop's existing `BOOSTER_ILLUS` mapping and use the exact `island_icon_*` textures used by product cards: lantern, sprout, owl with magnifier, bell, scarecrow and rain. No texture was redrawn and no reward or gameplay mapping changed.

Tutorial and mechanic lessons now use a stronger 0.82 backdrop shade instead of 0.55, keeping the underlying large Play control visually subordinate to the foreground Next/Play action. The shade still covers the full 1080x1920 canvas with `MOUSE_FILTER_STOP`. Pointer-event checks prove the page button works before opening, receives no click outside the panel while the tutorial is open, and works again after closing.

Latest focused suite: **330 checks, 0 failures**, including exact texture equality with all six shop illustrations in both languages, backdrop/input isolation and prior callbacks. Re-captured six unlocks and tutorial in both languages for inspection.
