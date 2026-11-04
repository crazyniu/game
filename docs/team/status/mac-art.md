# Status: mac-art

DONE: seven T49 eggs/overlays delivered to current Codex coordinator for review/integration on `mac-art/world`. Asset delivery SHA: `34eddebeca3b75fa5e39019090c8b486ba99911f`. This follow-up status commit records the exact delivered asset SHA; both commits are pushed to the task branch. No merge to main performed. PAUSED for coordinator review and user testing after handoff; no subsequent work started.

Files: `Resources/Birds/egg_idle.png`, `egg_wobble_l.png`, `egg_wobble_r.png`, `egg_crack.png`, `egg_shell.png`, `mama_bow.png`, `chick_fluff.png`, plus their seven PNG metas. Full repository prefix is `PerchSort/Assets/`.

Eggs 320x320; fixed nest support y=300, identical bottom band, five distinct frame hashes. Overlays 128x128 with exact per-species anchors and display sizes in `art/approved_ui/eggs_overlays/manifest.json`. Seven RGBA/size/alpha checks and static contact/game-size/grayscale/representative-bird QA passed. Sources/prompt records, QA and report isolated under authorized paths. Report: `tasks/T49_eggs_report.md`.

Integration limitation: current BirdView fixed overlay anchors do not implement the new per-species placements. Current Codex/mac-ui must wire the documented anchors and verify other species/poses. QA uses read-only bird snapshots from main commit `3e82c64734f3e7291a332d1c0e90263954ce40cd`. No species PNGs edited.

No unfinished owned asset files or external blocker. Runtime anchor consumption, Unity import, all-pose attachment, animation timing and device acceptance remain NOT_RUN. No sub-workers, code/Unity tests, Unity session/lock or Dispatch. Never contacted old Claude. Only seven runtime PNGs/metas, isolated sources/QA/manifest, task/report and this status changed.
