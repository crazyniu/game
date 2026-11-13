# Gameplay dialogs restoration report

Restored goal details to the existing family panel without a glow or boxed ability line. Added an optional `external_actions` decoration parameter, defaulting to the prior behavior; actionless goal details opt out so the ability remains inside the panel. Level roster frames reuse the existing clean Home icon frame and retain meaningful ability icons.

Tutorial and lessons reuse the family shell and green stage border. Caption wording is unchanged and uses the existing product ink material for contrast. Play uses ChestLettering, Next uses the existing Family painted word, and Got It uses its existing semantic painted asset. Only action content is replaced; faces, hit areas and callbacks are unchanged. Lesson world selection remains untouched.

Aviary card names extend to y=480 and locked markers to about y=490 despite nominal card height 468. Increased trailing scroll padding from 16 to 64 so the final row has visible clearance without changing card layout or selection.

Verification: scripts/run_tests.sh passed naming, painted-word, smoke and all 100 core levels. Existing family suite passed 2306 checks. New gameplay dialog regression passed 72 checks using UiText.set_language for both locales, all tutorial and lesson actions, goal bounds and profile preservation. Seven real Xorg dummy/OpenGL captures are in build/dialogs/screens, including Chinese tutorial Play and Got It. The refreshed tutorial uses the restored forest world assets, with no ocean or foreground trunks visible. Initial capture log includes an incorrect aviary preview identifier; final capture uses aviary_view:list_scrolled. New world resources are separately owned by the world restoration agent.

No commit, push, merge or deployment. Static captures do not establish animation/audio acceptance or physical-device safe-area behavior.
