# Skin redesign: report

## Done
- Skin particles and the glow aura are removed from `BirdView`: a skin is worn on the body only.
- All 45 skins are whole-bird skins now (the owl propeller beanie, `cocoa_outfit_4`, was already one): 23 with the full 70 frames, 22 with the
  reduced set of 9 painted frames plus 6 mirrored (see `spec.md`, "Frame set of a skin"). The overlay path (`front_overlay`,
  `outfit_anchors.json`, the `costume_alive` effect, the measurement tools) is removed. The front references stay local in
  `tools/birds/costume_parts/out`.
- 45 new skins specified (`spec.md`), all references painted and reviewed (colour families re-checked: berry 1, mint 3 and 5 replaced).
- Pipeline in `tools/birds/costume_parts`: `refs.py` (front references), `paint_skin.py` (70 frames per skin), `align.py` (registration
  to the plain frame and a continuity check), `run_skins.py` (batch with repaint of flagged frames), `pack_skin.py` / `install_skin.py`
  / `publish_skin.py` (WebP, quality 85), `sync_skins.py` (installs finished skins as they arrive), `mac_driver.sh` / `ship_skins.sh`
  (the same on the Mac, results go through the LAN transfer gateway).
- Skin frames are WebP (about 13 KB each instead of 80-90 KB); the asset tools accept WebP.

## Numbers
- A skin is 69-70 frames of 320x320, about 0.9 MB as WebP.
- The reduced skin set was the owner's answer to the cost: 22 skins took about 40 minutes and 198 paintings.
- Painting speed on two machines with 12-25 Codex sessions each: 10-13 frames per minute when the service is healthy.
- The Codex account has a 24 hour usage limit; one window held about 2,000 frames and 45 references. All painting stopped at
  20:28 until the limit reset at 23:50.

## Open
- Owner review of the painted skins on device (frame continuity, costume shapes).
- berry_1 and berry_2 miss a few non-essential frames of the full set (the 15 files every skin needs are present).
- The plain frames, `cocoa_outfit_4` and the other bird art are still PNG; `tools/birds/to_webp.py` converts them (the painting tools read the PNG plain frames).
