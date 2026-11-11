# Settings restoration

Restored the complete Settings constants/function block from the pre-change command output in
chat `01a1cae2-0d86-7bd2-84b2-cfa0cfec385e`. It also exactly matches the tracked Settings block.
The original Simple card, painted labels, round close control, 132 px rows and 340 × 116 actions
are back. Existing switches and callbacks remain. The hidden generic modal panel makes the later
shared frame changes irrelevant to Settings; other modal implementations were not reset.

The shared `popups.lock` was acquired, current content reread, only the Settings block replaced,
then the lock released in finally. No Git reset, commit, merge, asset edit or Shop code change.

Ran Godot 4.7.2 with `--path PerchSort --audio-driver Dummy --rendering-method gl_compatibility
--resolution 1080x1920 -- --preview=popups:settings_sound_on --lang=en
--capture=<repo>/build/settings-restore-review/settings.png --wait=0.5`.
Exit 0; no script/resource errors in `build/settings-restore-review/runtime.log`. Inspected the
actual capture: original cream panel, four rows, language selector, Restart/Home and Help layouts
are restored. This checks the displayed board-context Settings, not real audio/vibration hardware
or every callback. No full-game suite or export was run. Settings is frozen again.
