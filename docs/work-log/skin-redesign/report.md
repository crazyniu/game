# Skin redesign: report

## Done
- Skin particles and the glow aura are removed from `BirdView`: a skin is worn on the body only.
- 23 of the 45 skins are repainted as whole-bird skins and installed (sunny 1-5, cocoa 1, 2, 3, 5, berry 1-3, mint 1-5, hoot 3-5,
  ink 4-5; the owl propeller beanie, `cocoa_outfit_4`, was already whole-bird), plus the older unapproved trial `ember_outfit_5`.
- 21 skins are still the old overlay skins (berry 4-5, snow 1-5, sky 1-5, ember 1-4, hoot 1-2, ink 1-3), so the overlay path
  (`front_overlay`, `outfit_anchors.json`, the `costume_alive` effect) stays in `BirdView` until they are repainted. Their new designs
  are in `spec.md` and their front references are painted (kept locally in `tools/birds/costume_parts/out`).
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

## Stopped
The owner stopped painting on 2026-11-18 after the Codex usage limit was reached (about 1,460 frames were left for the 22 unfinished skins).

## Open
- Repaint the 22 unfinished skins (about 1,460 frames, one Codex usage window) and then retire the overlay path.
- Owner review of the painted skins on device (frame continuity, costume shapes).
- A few frames of berry_1 and berry_2 failed repeatedly and are missing; the game falls back to a neighbouring frame of the sequence.
- Plain frames still PNG; converting `birds/` to WebP waits until the last skin has been aligned (the alignment reads the PNG frames).
