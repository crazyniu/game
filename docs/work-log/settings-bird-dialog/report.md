# Settings and bird information spacing report

## Scope
- Settings close disc is wholly inside the card with 48 logical pixels of top clearance. Existing toggles/actions and right inset are unchanged.
- Bird information uses an 840 px card, smaller centered bird, compact vertical rhythm, no corner X, and the existing green painted OK button outside the frame. Both effects and effect-free birds retain their original illustration/ability and animated bird.
- Removed the redundant Outfits heading in both languages. Outfit tiles, unlock/equip/purchase behavior are unchanged.
- No assets, economy, saves, gameplay or neighboring popup layout were changed.

## Verification
Godot 4.7.2 stable, isolated fake profiles and writable XDG directories.
- `scripts/run_tests.sh`: passed naming/painted-word guards, launcher recovery, smoke, core, save recovery UI, bird animation and goal validation (94 checks). Expected invalid-goal fixtures emit diagnostic errors while the suite reports zero failures.
- `run_gameplay_dialogs.gd`: 84 checks, zero failures, including external OK dismissal and absent Close.
- `run_purchase_close.gd`: 1836 checks, zero failures, including Settings close containment.
- `run_aviary_actions.gd`: 1203 checks, zero failures. Existing exit-time ObjectDB/resource leak diagnostics remain; not represented as a clean resource-lifetime check.
- `run_aviary_text.gd`: 290 checks, zero failures, including all species/both languages without the redundant heading.
- `git diff --check`: passed.

Real OpenGL GUI captures in Chinese/English: settings sound-on/muted, Minty/Rook information, Hoot outfit detail. Visually checked close inset, title/OK alignment, compact frame and retained outfit tiles. A timed 12-frame Minty capture preserves bird blinking/idle movement without displaced contents. This is scoped UI evidence, not full playthrough or user acceptance.

## Evidence and limitations
Local ignored `build/screens/` contains runtime PNGs, `compare-settings.png`, `compare-bird.png`, `compare-aviary.png`, and `motion-contact.png`. Side-by-side comparisons use the user's supplied screenshots and actual runtime captures. The repository capture wrapper's Xvfb path was unavailable in the executor; captures were run in the cloud desktop's real DISPLAY with the same canonical preview parameters, then composed with the repository comparison approach. No evidence/screenshots are committed. Graphical driver emits a VSync/XServer warning on this cloud display; captures complete successfully.

User visual confirmation is still pending. This change is for review, not automatic merge or deployment.
