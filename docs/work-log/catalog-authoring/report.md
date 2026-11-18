# Current catalog authoring correction

## Result

Based on merged main `da2f3e5e03b60264c948c9ad0aa3eaf893cb12fb` (Cocos migration #314), the surviving Python
reorder tool now handles 210 levels and 21 positions per world. It requires the catalog, hints and measurement
JSON to cover each current ID exactly once before output is written. Every route is remapped; authored pars
remain unchanged. Cached route action counts and supplied counted-move measurements are separate, and input
proof flags are retained without implying a new search.

The old expansion/candidate mode was removed. It was built to fill a smaller historical catalog and could
replace current unmeasured puzzles silently. The README now describes the remaining command as an
unapproved heuristic proposal and records the removed Godot tooling and current `Profile.ts` migration owner.
No shipped puzzle, hint, par, ID, world assignment, save schema, runtime code or artwork changed.

## Issue disposition

- **#247:** completed historical measurement/sibling work remains the source record; its old command paths
  are no longer executable after the Cocos migration.
- **#248:** open. The ten-world mechanic table still needs confirmation, including the precise combinations
  for worlds 8–10. Replaced-puzzle score ownership conflicts with the identity requirement in #239/#250 and
  needs a product decision. Current generator work also needs a Cocos-compatible implementation.
- **#249:** open and downstream of #248. The latest coordinator amendment is 1.0–1.15 times level 20,
  superseding the original 1.2–1.5 times median band. Finale catalog, reward and approved map art are outstanding.
- **#250:** open. This PR fixes only its concrete authoring prerequisite. The agreed difficulty measure,
  tolerance, world/finale dependencies, per-transition investigation, final order, save migration and
  acceptance are not delivered here. Future adjacency checks must include all 209 actual neighbours and
  all nine local-21 to local-1 boundaries.
- **#239:** open as the unique audit evidence and requirement tracked by #250. The latest #250 comment
  explicitly requires both issues to stay open until full acceptance; this correction does not resolve either.

## Verification boundary

Reviewed the current tool, TypeScript level constants and board search-key/gift representation, current
launchers, historical measurement reports, all comments on #248/#249/#250/#239, and #247's closed status.
No local tests, lint, builds, generation, reorder execution, screenshots or temporary verification programs
were run, in accordance with the current task policy. Existing remote CI is the PR merge gate; it checks the
runtime catalog and core suite but does not execute this proposal-only authoring command. User authoring
acceptance and subjective difficulty remain unverified.
