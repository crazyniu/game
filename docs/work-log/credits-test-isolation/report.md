# Credits fixture verification

Base main 44de2ed. The original actual cloud OpenGL test passed all 20 UI assertions (2.32s), but exit logged two ObjectDB instances and one resource still in use. An isolated headless verbose reproduction named AudioStreamPlaybackWAV and res://audio/bgm_menu.wav. The fixture lacked the mute fields used by run_gameplay_dialogs and other nearby GUI checks.

Only the synthetic profile now sets muted/music_muted before startup. The same 20 assertions pass in headless verbose mode and actual cloud Godot 4.7.2 OpenGL (2.22s); those exit diagnostics no longer appear. Unsupported VSync warning remains. This avoids playback in this UI-only fixture; it does not establish that normal unmuted production teardown is correct. That audio investigation is explicitly outside this task.

Actual gameplay-dialog coverage also passed 84 assertions (6.56s) without edits. Aggregate scripts/run_tests.sh passed unchanged (exit 0) for this test-only patch. No real profile, store, ad, chest, artwork, sound code or production setting changed. #254 remains open: this is not full-sweep or physical-device acceptance.

Ignored evidence: build/heading/remaining-dialogs contains the original GUI results/logs; build/heading/aux-cleanup/{baseline.log,muted.log} the verbose comparison; gui/results.json and run_aux_dialogs.log the actual corrected GUI run; aggregate.log the unchanged regression suite. No captures or runtime artifacts are committed.
