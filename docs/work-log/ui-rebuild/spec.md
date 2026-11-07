# UI rebuild: shared contract for screen workers

The user approved a new cartoon UI (simple, clean, nothing fancy) and handed the whole UI to the coordinator. The
board, the popups and the kit are done on branch `ui-rebuild`. Each worker rebuilds ONE screen from scratch on the
same system. Do not follow any earlier layout of that screen.

## Read first

- `CLAUDE.md` (hard constraints: painted words and numbers only, never a font; same-role buttons identical in size;
  primary at most 1.25x secondary; disabled = dimmed, same size; birds never static; no version names).
- `docs/design/ui-system.md`: the binding system (canvas, grid, colours, shapes, lettering, icons, motion).
- `PerchSort/game/ui.gd`, section "cartoon kit": `UI.shape`, `UI.button`, `UI.badge`, `UI.counter`, `UI.icon`,
  `UI.number` / `UI.set_number` / `UI.number_width`, `UI.word` / `UI.word_width`, `UI.stars`, `UI.set_active`,
  `UI.art` (illustrations, keeps aspect), constants `UI.PRIMARY` 160, `UI.SECONDARY` 128, `UI.COUNTER_H` 96,
  `UI.INK`, `UI.GOLD`, `UI.FADED`, `UI.FILL` / `UI.RIM`.
- `PerchSort/game/popup.gd` (`Modal.open(host, title, heights, close_button, action_h, above, instant, quick)` and
  `modal.actions([[kind, icon, callback], ...])`) and `PerchSort/game/popups.gd`: the reference implementation
  of the style. Copy its patterns (live BirdView via `_bird_view`, `_fit`, breathing, twinkle).
- `tools/kit/style_preview.py`: the approved style images (board, win popup, map), the visual target.

## Assets you may use

- Icons (white, tinted by the kit): undo bulb sapling gear home play retry check close sound_on sound_off shop
  aviary lock left right footprint sun moon star eye bell scarecrow rain hanger hop plus info gift egg feather
  trophy pin coins (`UI.icon(parent, "<name>", centre, size, color)`).
- Words: call `UI.word(parent, "Literal text", at, cap, color, align)` with a string literal, then run
  `python3 tools/kit/build_words.py`; it builds every literal it finds. Text chosen at runtime (names, items) must be
  listed in `tools/kit/words.txt` (append lines only; keep it sorted). Available characters: A-Z a-z 0-9 . , ! ? ' -
  : / + & ( ) % # and the multiplication and infinity signs.
- Illustrations already in `PerchSort/art` and `PerchSort/birds`: coin, booster_<hint|sapling|owl_eye|bell|scarecrow|rain>_illus,
  house_default, house_style_<acorn|teapot|mushroom|lantern>_body, gate_*, gift_*, birds (`<species>_front`,
  `<species>_face`, outfits through BirdView), map_bg_continuous_w1..w4. No new painted art: if something is
  missing, ask the coordinator with `ask` and continue with the rest.

## Ownership

- Edit only your screen file (and its `.uid` if Godot creates one), `tools/kit/words.txt` (append), and your report
  `docs/work-log/ui-rebuild/<screen>.md`. Do not edit ui.gd, popup.gd, popups.gd, board_view.gd, main.gd, art.gd
  or any art. If the kit lacks something, write a small private helper in your file and mention it in the report.
- Keep the screen's public interface unchanged: class name, signals, `setup(...)`, the `preview(main, state)` states
  listed for it in `scripts/screenshots.sh`, and everything `main.gd` calls.
- Keep core/ and meta/ untouched; keep save fields and behaviour (buying, equipping, scrolling, navigation).

## Verify (all required)

1. `godot --headless --path PerchSort --import` once, then
   `godot --headless --path PerchSort --audio-driver Dummy -- --smoke` must print `smoke checks: 0 failures`
   (script errors about worlds 3 and 4 boards missing `board_w3_slots.json` / `board_w4_slots.json` are expected
   until the coordinator adds those scenes; nothing else).
2. `python3 tools/kit/build_words.py --check` reports no missing words.
3. `bash scripts/screenshots.sh <screen>` captures every state of your screen into build/screens/. Open and look at
   every capture at full size. Check: nothing overlaps or is clipped, margins 48, 8 px grid, sizes of same-role
   elements identical, text readable, consistent with the board and popups.
4. Timed evidence for anything that moves: `godot --path PerchSort -- --frames=<dir> --wait=S --step=0.25
   --preview=<screen>:<state>` (run under `xvfb-run -a -s "-screen 0 1280x2000x24"` with `--rendering-driver opengl3
   --resolution 1080x1920 --audio-driver Dummy`). Look at the frames.
5. `python3 ci/check_current_game_names.py`.

## Deliver

Commit on your own branch (the worktree's branch) with an English message, push it to origin, and finish with
`worker_done` listing: branch and commit, files changed, capture paths, what you checked, open issues.
Never merge, never push to `ui-rebuild` or `main`, never force-push.
