# Bird sheets

## Goal
A bird or a skin downloads as one picture instead of one file per frame, with no loss of clarity and no change on screen.

## Contract
- `tools/birds/pack_sheets.py` packs the frames of every bird (`<bird>_*`) and every skin (`<bird>_outfit_<n>_*`) in
  `PerchSort/assets/resources/birds` into `birds/sheets/<group>.webp`: each frame is trimmed to its ink plus a 4-pixel transparent margin
  (the aviary silhouette draws a 3-pixel rim), pieces are 2 pixels apart, sheets are 2048 wide (the ten world-1 birds with all 71 actions are
  a little over 2048 high). Frames are not scaled. The single files are deleted; `tools/birds/sheets.json` records every piece.
- New frames (painting, `install_world.py`, `publish_skin.py`) arrive as single files; `pack_sheets.py` folds them into the bird's sheet
  (a frame with the same name replaces the old piece). Then `node tools/cocos-assets.mjs` and `python3 tools/cocos-meta.py`.
- `tools/cocos-assets.mjs` writes one manifest entry per frame: `path` is the sheet, `size` and `ink` are the frame's, `piece` is
  `[x, y, left, top, width, height]` (position in the sheet, then in the frame), `tex` the sheet size.
- `Art.load` downloads a sheet once and cuts frames from it as full-size sprite frames (rect, original size, offset), so untrimmed sprites
  draw exactly as before. `Art.cache` is keyed by manifest key. `UI.silhouette` uses the sheet size for its texel step.
- Painting tools read a frame with `pack_sheets.frame_image(name)` (single file or sheet).
- Eggs, the chick fluff, the mama bow and the home island idles stay single pictures.
