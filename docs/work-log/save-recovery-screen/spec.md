# Save recovery startup gate

Issue #272. A profile whose primary and backup are both unreadable must never enter
normal play with an unsavable fresh fallback. Keep all existing files, migrations,
backup behavior and ordinary first launch unchanged. Only expose the existing
runtime blocked-save status; do not persist a new field.

The recovery UI reuses the game's shared JungleDialog green frame/wooden heading, forest backdrop
and green button family; both actions sit outside the frame. It offers Retry (reload the canonical scene and reread the files)
and Exit only. No dismissal, reset, data deletion or automatic restoration write.
A readable backup continues through the existing profile loader. An unreadable
pair remains blocked until a readable save/backup is restored externally.

New English and Chinese lettering is AI-generated as transparent text atlases.
No accepted UI/artwork is replaced. The new screen needs visual review; no merge
or submission until the coordinator's combined delivery is ready.
