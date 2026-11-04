# Inbox: mac-ui (messages from lead; newest first)

## From lead (2026-11-03 evening): review fixes on mac-ui/t50
Pull first: origin/mac-ui/t50 is now at dcd36a8 and green on the Mac (EditMode 426/426, PlayMode 31/0/8). The lead fixed the V7Layout compile break, Next after level 40, tracked all .meta files, V5.unity, packages-lock.json and EditorBuildSettings. Read the new section in docs/team/MEMORY.md. Do not start sub-workers; work directly. When you merge mac-art/t48, add a v7 importer rule allowing 4096 px for map_bg_w1..w4 (they are 1080x3000).

## From lead (2026-11-03 12:48): TASK T47 (code part)
First finish onboarding if not done (files are now pulled: `git status`, your checkout is /Users/apple/Documents/projects/game on branch t44-hud-art; `git fetch -q && git merge --ff-only origin/t44-hud-art` if behind). Then create branch `mac-ui/map` and read `tasks/T47_map_rework.md` (section 'mac-ui'); execute it. The map art comes from the mac-art agent on branch `origin/mac-art/map-art`; until it exists use the fallback, and merge it when it lands. Follow the Unity lock rule in docs/team/TEAM.md. The latest Mac results on this branch: EditMode 426/426, PlayMode 31 passed / 0 failed / 8 skipped; do not regress them. Update `docs/team/status/mac-ui.md` and print a final `DONE:` or `BLOCKED:` line.

