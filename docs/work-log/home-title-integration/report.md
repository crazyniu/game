# Connected Home title delivery

Implemented the approved raised composition in `home_view.gd` with the independent transparent
`panel_home_perch_title.png` and the independent `background_home_beach.png`. Four existing animated
birds occupy measured branch contacts; the intact foreground branch covers their tails naturally.
The roof-only mascots and separate large house are absent. The title/branch group uses the design's
upper placement; Play remains at y=1360. Proportional outlined icons and 48 px badges remain.
The root Home ambience has zero falling leaves/extra sunlight; level ambience is unchanged.

Assets were generated with the native image tool, imported with Godot 4.7.2 and are documented with
exact source paths/prompts in the design doc. No reference image was cut or pasted into runtime.

Focused checks used `/Applications/Godot.app/Contents/MacOS/Godot`, the real main scene and fake
capture profiles, with `--path PerchSort --audio-driver Dummy --rendering-method gl_compatibility
--resolution 1080x1920`:

- `-- --preview=home_view:home --lang=en --capture=<repo>/build/home-title-review/home.png --wait=0.5`:
  exit 0; actual frame inspected for hierarchy, contacts, proportions, clear actions and sandy foreground.
- `--script <repo>/build/home-title-review/check_home.gd -- --preview=home_view:home --lang=en
  --capture=<repo>/build/home-title-review/input.png --wait=6`: final run passed all seven icon viewport
  clicks exactly once, proportional art, Play's level-12 signal, 220 px activity spacing, four birds,
  empty Home particle list and both locale swaps without resizing. This checks Home's signal contract,
  not downstream purchase/reward/level completion flows. Log: `build/home-title-review/input.log`.
- `-- --preview=home_view:home_new --lang=zh --capture=<unused> --frames=<repo>/build/home-title-review/motion
  --motion --wait=4 --step=0.25`: 16 actual 1080 × 1920 frames, each bird crop had 16 distinct rendered
  samples. Inspected f005/f012 for visible pose changes and planted contacts. Frame saving adds overhead,
  so this is not a claim of exact four-second wall timing. Log: `build/home-title-review/motion.log`.

Earlier temporary input-harness failures came from coordinate/timing handling; the final harness
uses viewport-local transformed positions and passed. No whole-game suite, export, audio listening
or device performance check was run. No user-approved runtime baseline or Git commit was created.
