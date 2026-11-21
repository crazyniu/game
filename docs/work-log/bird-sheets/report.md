# Bird sheets: report

## Done
- 4,684 bird frames (120 MB, PNG and WebP) packed into 136 sheets (82 MB WebP, quality 90, alpha exact). Spot checks against the
  originals: alpha identical, colour within about 3/255 on opaque pixels.
- `Art.ts` cuts frames from sheets; `BirdView` and `UI.silhouette` follow; `paint_frames.plain_image` reads sheets.

## Evidence
- Mac worktree `game-bird-sheets`: `./run.sh build` exit 0; `scripts/capture.sh` (WEB_PORT=8772) for level 1, level 30, home, aviary
  (met and unmet), shop: every capture rc 0 (the script fails on any Art load error). Captures (local, not committed) show birds standing
  on their branches as before and the unmet silhouettes with their full rim.

## Open
- Owner review on device: flight, hop and cheer motion, skins in the aviary and on the board, the propeller skin.
