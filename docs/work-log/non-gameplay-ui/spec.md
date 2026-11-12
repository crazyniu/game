# Non-gameplay UI integration

Continue the existing cloud implementation without uploading or publishing code. Starting
point: the previous local integration on commit 57445427, preserved by the complete patch
with SHA-256 909313222bfc03563a46504b45278bded6af921df2b18f0f688fb006e02fbf9c.

Handle the unfinished non-gameplay surfaces: three Shop categories, item/outfit/nest and
simulated-currency purchase dialogs, Draw results, win/level-card/generic-reward/new-bird/
booster-unlock/world-complete/world-unlock dialogs, and the specifically requested Chest
word/progress correction. Close and Back follow the current coin-shop family through an
explicit MenuNavigation helper. Isolated workers own non-overlapping functions; integrate
their deltas without replacing shared files.

Preserve finalized Home and Settings, accepted source images, gameplay, economy, saves,
controller and shared Modal. Tutorial.open serves both Help and first-level teaching, so
leave tutorial.gd and mechanic lessons unchanged. Goal information remains a protected
in-game surface. Do not add new exit behavior where a dialog had only an action to dismiss.

Reuse suitable accepted assets. A genuinely missing independent asset must be completed,
visually reviewed and hash-recorded before wiring; do not redraw finalized UI or substitute
approximate images for accepted pieces. No approved-runtime baseline is auto-written.

Run per-component and final combined checks: imports, naming/painted-word guards, smoke,
100 fixed levels, focused UI contracts, retained-source/asset hashes, raw graphical state
captures, live return/close/repeat-input flows and relevant timed animation evidence. Use
fake profiles, keep actual capture dimensions, preserve failed diagnostic samples and state
what was not tested. No commit, upload, push, new authorization, merge or deployment.
