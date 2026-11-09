# Skins report

Added 4 birdhouses and 18 bird costumes (outfits 4 and 5 for each of nine species), with painted name labels.
Checks: `python3 ci/check_current_game_names.py`, `bash scripts/run_tests.sh` (core: 0 failures), captures of shop houses, bird outfits and the aviary detail page.
Boundaries: costume turns and some flap/hop frames fall back to the nearest reliable frame (anchor "repaired" lists); not yet checked in GUI motion; no user acceptance yet.
