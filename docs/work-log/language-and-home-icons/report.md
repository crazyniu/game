# Report

Done: zh pack (176 labels, 4 documents, numerals from Nunito), language row in Settings, profile field `language`, icon tiles on home and map, plain Play plate with localized word. `bash scripts/run_tests.sh` passes.
Evidence: `build/design/settings_zh.png`, `zh_contact.png`, `home_new_check.png` (ignored by git).
Open: the six icons are cut from the retired tiles. A repainted set (prize wheel for Spin, calendar-and-chick for Daily Challenge; prompt in `tools/home/sketch/icons.txt`) could not be generated: local Codex could not reach its image service (network timeouts, 2026-11-09). Drop the new PNGs over `art/home_icon_*.png` when available. Screens other than home, map, settings, shop, aviary, win, chest were not re-checked in zh. Not yet tried by the user.
