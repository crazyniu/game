# Perch Sort conventions

One game, one current implementation: Cocos Creator 3.8.8 with TypeScript, portrait 1080x1920, 210 fixed levels in ten sequential forest worlds, 21 levels per world. No day/night gameplay. These conventions supersede historical engine instructions; explicit current user instructions take precedence.

## Names

1. Name files, folders, classes, methods, assets, scenes, docs, evidence, branches, and tests by their purpose.
2. Do not add version-number or task-number identities, numbered final copies, archive folders, or alternate runnable implementations. History belongs in Git.
3. Allowed numerals include domain ordinals (worlds, levels, animation frames), dates, third-party package identifiers, and exact existing storage keys.
4. TypeScript class modules use their class names. Preserve existing core/meta snake_case API names and persisted field names. Follow neighboring code for other identifiers; tests are named after behavior.
5. Asset names stay lowercase snake_case, without spaces. UI assets keep role prefixes such as `icon_`, `btn_`, `panel_`, `house_`, or `gift_`; bird sequences use `<species>_<action>_<NN>`. Resource moves must preserve the artwork and identifiers.
6. Keep Cocos `.meta` identifiers stable. Task branches are temporary; commits and PR titles explain the actual change in plain English.

## Repository layout

- `PerchSort/` is the only Creator project. `package.json` identifies it; `assets/scenes/main.scene` is its entry scene; `assets/scripts/Boot.ts` owns startup and navigation.
- `assets/scripts/core/` owns rules and solver; `assets/scripts/meta/` owns persistence and economy; both are independent of Cocos. `assets/scripts/game/` owns presentation and audio.
- `assets/resources/` holds `art/`, `birds/`, `world/`, `audio/`, `layout/`, and `levels/`. `levels/levels.json` is the 210-level catalog; `levels/hints.json` holds stored solutions.
- `art/approved_ui/` holds design references; `art/approved_runtime/` holds user-approved runtime baselines. Design specifications live only in `docs/design/`.
- Each feature uses `docs/work-log/<topic>/{spec.md,report.md}`. Historical work logs remain intact, but old source paths and commands are not current execution instructions.
- `build/`, `PerchSort/build/`, Creator caches, and feature `evidence/` directories are generated local output, never committed.

## Code and data

1. Avoid defensive programming, unnecessary abstraction, and redundant comments. Comments explain non-obvious reasons in English.
2. Keep rules and presentation separate. Views show state and emit actions; core/meta code never imports scene nodes or screens.
3. Preserve `Profile.ts` JSON keys, schema migrations, and `Board.ts` numeric enum order. Browser storage uses `perch-sort.profile`.
4. The local launcher may import the prior Godot profile once when browser storage is empty. It must not modify the original file, reset records, grant coins, or silently unlock levels. Test profiles must be isolated.
5. Cancel Cocos tweens, callbacks, and frame updates when their owner is removed or inactive. Honor the development-only instant mode without presenting it as animation evidence.
6. Payment and rewarded-ad providers are not wired. Grant purchased currency or ad rewards only after a confirmed provider callback; mock confirmations are restricted to explicitly authorized testing.
7. Respect parallel ownership. Request changes in another worker's files rather than editing them.

## UI standards

1. Preserve the current screen contracts and approved runtime baselines. A new or changed layout first becomes a clickable animated shell in its canonical screen class, using fake data through the real view interface. User approval precedes logic wiring. The Cocos migration preserves existing UI and does not authorize redesign.
2. Mockups establish structure, proportions, scale, materials, and mood. Never crop mockup pixels into runtime assets or embed a design screenshot in the game. Use existing clean assets or authorized newly generated pieces.
3. Words and numbers are painted sprites. Fit visible ink into the intended boxes; preserve English and Chinese packs. Invisible input text must stay invisible. Do not add redundant explanations or small helper copy.
4. Reuse the current control family. Same-role controls have identical sizes across states; disabled means dimmed and non-interactive. A primary action is at most 1.25 times the size of sibling actions.
5. The shared feather counter retains its artwork, purchase entry, and display-only 99999 cap. Never clamp the actual stored balance to that display limit.
6. Modal input blocking, shade, panel, and controls sit above the entire underlying scene. Preserve each modal family's current shade and layout. Settings use one panel transition, not independent control entrances.
7. Map content stays between the top and bottom bars, with the current node and bird fully visible.
8. Goal houses start with empty doorways. Residents appear only after delivery and disappear again when the delivery is undone.

## Animation and audio

1. Bird actions use distinct drawn frames. Never mirror a bird or substitute transforms of one still for turning, hopping, or flight. Breathing and a brief landing squash accompany frame animation.
2. Perched birds breathe continuously and perform frequent idles. Preserve the cadence and neighbor coordination defined in `BirdView.ts`; missing sequences are omitted, not faked.
3. Entrance takes 3.5–4.5 seconds, in houses/branches/birds order, and can be skipped by a tap. Input unlocks only after the entrance completes.
4. Preserve the win celebration: frame-based bird entrances and idles, sequential stars, counting rewards, rotating rays, and brief confetti.
5. Use Cocos animation and tween ownership. A fixed image is not an animation.
6. Menu/map pages play `bgm_menu`; the board plays `bgm_game`, with looping and page crossfades. Popups do not restart music. Preserve the current saved music, sound, and vibration settings. Pause on focus loss and resume through user input where browser policy requires it.

## Verification and evidence

Testing and acceptance belong to the user unless explicitly requested. Do not automatically write or run tests, lint, verification builds, screenshots, UI automation, or temporary validation programs. Explicit build/package/release requests authorize only their necessary steps. The 2026-11-18 migration request explicitly authorizes self-testing for that task.

When authorized:

- Run relevant checks with `npm --prefix PerchSort test`, `npm --prefix PerchSort run typecheck`, and `./run.sh build` as appropriate.
- Exercise the real built Cocos game for runtime claims. Tests and build success do not establish UI or device acceptance.
- Compare captures with current contracts and approved baselines. Motion/audio require timed evidence; static screenshots prove neither.
- Do not weaken or delete checks to make them pass. Use production constants where appropriate and avoid unnecessary repeated tests.
- Report commands, results, evidence paths, and unchecked boundaries honestly. Captures and frame sheets stay local and are never committed or pushed.

## Collaboration and delivery

1. Use non-overlapping ownership and one shared contract for parallel tasks. Bounded workers do not spawn workers or expand their scope.
2. The coordinator integrates and reviews concrete results. A worker's report is not runtime acceptance.
3. Preserve the user's checkout, unrelated edits, local configuration, and save data. Use isolated worktrees when appropriate; do not silently switch the user's working branch.
4. Never force-push, deploy, or release without explicit authority. Do not bypass branch protection.
5. Speak Chinese with the user; write agent instructions, specs, reports, comments, and commits in English. No emoji in deliveries.
