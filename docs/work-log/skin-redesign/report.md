# Skin redesign: report

## Done
- Skin particles, the glow aura and the overlay (`front_overlay` + `outfit_anchors.json`) path are removed from `BirdView`; the
  `costume_alive` effect and the overlay measurement tools are deleted. A skin is a whole-bird painting per frame.
- 45 new skins specified (`spec.md`), all references painted and reviewed (colour families re-checked: berry 1, mint 3 and 5 replaced).
- Pipeline in `tools/birds/costume_parts`: `refs.py` (front references), `paint_skin.py` (70 frames per skin), `align.py` (registration
  to the plain frame and a continuity check), `run_skins.py` (batch with repaint of flagged frames), `pack_skin.py` / `install_skin.py`
  / `publish_skin.py` (WebP, quality 85), `sync_skins.py` (installs finished skins as they arrive), `mac_driver.sh` / `ship_skins.sh`
  (the same on the Mac, results go through the LAN transfer gateway).
- Skin frames are WebP (about 13 KB each instead of 80-90 KB); the asset tools accept WebP.

## Numbers
- A skin is 69-70 frames of 320x320, about 0.9 MB as WebP.
- Painting speed on two machines with 12-25 Codex sessions each: 10-13 frames per minute when the service is healthy.
- The Codex account has a 24 hour usage limit; one window held about 2,000 frames and 45 references. All painting stopped at
  20:28 until the limit reset at 23:50.

## Open
- Owner review of the painted skins on device (frame continuity, costume shapes).
- Frames that failed repeatedly leave a skin with a missing frame (berry_1, berry_2 at the time of writing); they are repainted after the reset.
- Plain frames still PNG; converting `birds/` to WebP waits until the last skin has been aligned (the alignment reads the PNG frames).
