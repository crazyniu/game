# Island homepage report

## Latest complete-button integration
The user approved the complete Chinese/English button comparison and authorized continuation. The approved PNGs are now the homepage's `Play/Face` textures. Each contains its entire triangle-and-word composition, so a separate word node can no longer drift relative to the icon. The old homepage word shader and its UID are removed.

- Both production PNGs are byte-identical to the approved candidates. The accepted base and painted word sources remain unchanged. All base pixels outside the overlaid word alpha, including the baked triangle, are unchanged.
- The group is centered against the whole visible green face, including its dark lower bevel. The earlier bright-surface-only assumption is superseded. Source face and group share center `(996,372.5)`; exact bounds and hashes are recorded in the manifest/validation file.
- Preserve the 460x191 hit area, 440x171 face container, native aspect ratio (`KEEP_ASPECT_CENTERED`), callback and pressed tint. The source artwork is 1993x757; the container does not stretch it to a new aspect ratio.
- The locale notification swaps the complete texture in the same face node. English remains the natural game label `Play`; Chinese is `开始`.
- This change does not touch coins, parrots, other pages, shared UI helpers, game logic, levels, save schema or audio code.

## Retained homepage refinements
- Exact-source `Bird Sorting` title at the smaller phone-reviewed size. All 166,511 visible original RGB pixels are preserved; only transparency/display size differ from the source.
- Approved feather-coin HUD, independent numeral area and retained green plus. No recharge action is implemented. Plain digits are capped at 99999 in presentation only; the real balance, economy and save data are unchanged.
- Four bottom faces remain 190x174, inside unchanged 208x191 touch targets. Reward badges follow the visible faces. Daily Challenge, Lucky Draw, Map, Shop, Aviary and Chest remain, as do Play and Settings.
- Both parrots remain planted using their existing per-pose claw anchors, with their PNGs and original display scale unchanged. The yellow logo bird remains seated on the smaller title. Other birds, background, house and scenery remain unchanged.

## Verification for this integration
User authority covers a focused actual homepage run, image comparison and Settings language switching before submission. Full suites are not part of this run.

Ran `build/island-home/layout-review/run-buttons.sh` using Godot 4.7.2, isolated XDG data/config/cache and fake profiles without save writes:
1. `Godot_v4.7.2-stable_linux.x86_64 --headless --editor --path PerchSort --import`.
2. Two canonical `main.tscn` captures under `xvfb-run -a -s '-screen 0 1280x2000x24'`, with `--path PerchSort --audio-driver Dummy --rendering-driver opengl3 --resolution 1080x1920 -- --capture=<output> --lang=<locale> --profile=<JSON> --wait=0.8`.
3. A focused SceneTree harness instantiating the same canonical main scene with `--script build/island-home/layout-review/check_language.gd`. It triggers the actual Settings and Language button signals, not a separate UI implementation.

Chinese capture: balance 50585, display 50585. English capture: balance 1234567, display 99999. Both fresh 1080x1920 images were personally inspected: the full icon/word group is centered and readable, white fill and dark edge remain clear, native proportions are preserved, and parrot contacts plus six navigation entries remain intact. A repeated ordinary capture avoids a transient drifting petal covering the English label; ambient code is unchanged. These are game renders, not composited mockups.

The focused language check passed all 15 assertions, including English to Chinese and back, unchanged HomeView and face instances, correct complete textures, unchanged face/hit bounds, and no separate word node. It verifies the Settings signal path, not physical pointer dispatch. The first harness exit left the currently playing menu WAV referenced; verbose output identified the audio playback. Muting and settling audio during harness teardown resolved it. No production audio code changed. The final run has no script/parse/resource-leak errors; the only warning is virtual X11's unsupported V-Sync change.

Current evidence: `evidence/layout-review/{home-zh.jpg,home-en.jpg,home_zh.log,home_en.log,language.log,check_language.gd}`. The full-resolution PNGs and import log remain in `build/island-home/layout-review/`.

**NOT RUN:** regression/core/smoke suites, lint, build/export, unrelated navigation or physical-input dispatch, full animation acceptance and audio listening/timing. Updated regression assertions remain unexecuted. These focused checks are not a full pass. Runtime visual acceptance remains with the user; no approved-runtime baseline was created.

## Earlier parrot evidence
Commit `21932a2f` independently corrected the parrot contacts and captured 40 real runtime samples. The full contact sheet was viewed, with independent inspection of f000, f010, f022 and f039; both parrots meet the branch in those samples. See `evidence/perch-review/contact-sheet.jpg` and `runtime.log`. PNG saving/rendering adds time after each configured 0.125-second wait, so this is not proof of a five-second fixed-8fps sequence or coverage of all eight poses. It was not rerun for the button-only integration; current stills confirm the resting layout only.

Earlier baseline `e4d98be73f9e24954f25aa1933ed5431b30b2975` test results are historical and do not verify this integration.

## User handover
Pull `island-ui-redesign` and open `PerchSort/project.godot` in Godot 4.7.2. Check the whole button's balance, switch language from Settings without leaving Home, and review the retained parrot contacts. Keep PR #209 a draft. No merge, deployment or release is authorized.
