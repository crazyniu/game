# Repository branch reconciliation — 2026-11-04

The user authorized PR integration of outstanding repository work and updating local main, including the Mac. The coordinator audited local and origin refs plus PR history. Final accepted integration `bf31ba2` was published through [PR36](https://github.com/crazyniu/game/pull/36), main code checkpoint `2a5bc259187cc924d1d5a9c666a8385b95217327`.

## Included in the final integration

`mac-art/t48`, `mac-art/world`, `mac-birds/t49`, `mac-map-motion/t50`, `mac-motion/t50`, `mac-notifications/t50`, `mac-popups/t50`, `mac-ui/t50`, and `t46a-icons` are ancestors of the reviewed integration. B's locally delivered correction `4256de6` is included even though its peer remote branch stopped at `7ddcb721` during a transport failure. The later B transport-only record is historical, not missing product work. The temporary `mac-review/t50-integration` ref contains an earlier included review snapshot.

`t50-codex-handoff` was published by [PR35](https://github.com/crazyniu/game/pull/35); current AGENTS/CLAUDE/TEAM directives are preserved byte-for-byte. T44's `78ac97d` popup-trigger policy is restored in `tasks/T50_ui_code.md` by this documentation closeout. No missing runtime change was found in T44.

## Previously published branches

The branch tips below have identical trees to their recorded squash-merge commits. Their remaining branch refs do not represent missing changes.

| Branch | Published PR |
| --- | --- |
| `t16-bird-home-prd` | [PR1](https://github.com/crazyniu/game/pull/1) |
| `ci/record-validation` | [PR3](https://github.com/crazyniu/game/pull/3) |
| `t30-v5-first-level` | [PR18](https://github.com/crazyniu/game/pull/18) |
| `t30-v5-art-final` | [PR19](https://github.com/crazyniu/game/pull/19) |
| `t31-v5-layout` | [PR28](https://github.com/crazyniu/game/pull/28) |
| `t32-core-v53` | [PR29](https://github.com/crazyniu/game/pull/29) |
| `t35-levels` | [PR30](https://github.com/crazyniu/game/pull/30) |
| `t39-polish` | [PR33](https://github.com/crazyniu/game/pull/33) |

## Preserved historical or cancelled work

- `t21-first-level-slice` / [closed PR7](https://github.com/crazyniu/game/pull/7), `t22-reviewed-first-level-save`, and local T21–T28 foundation variants are superseded by the current Unity/v7 implementation.
- `t30-godot-migration` / [closed PR8](https://github.com/crazyniu/game/pull/8) is a retired engine direction. Merging it would reintroduce obsolete work.
- `mac-ui/map` is explicitly cancelled by the current T50 task. `mac-art/map-art` records a paused/withdrawn trail-and-banner UI. Both belong to the superseded T47/v6 direction.
- `mac-art/ui7` is the earlier A1 concept, superseded by the approved v2/v7 artwork.

These branches are preserved without restoring their cancelled implementation. No branch was deleted. All valid current-stage changes are accounted for; future work begins from current main on the Mac.

This was a read-only branch/history/source audit. No Unity, compiler, code tests or gameplay execution was performed.
