# Skins: bird costumes and birdhouses

- Birdhouses: 8 styles (mushroom, teapot, lantern, acorn, pumpkin, beehive, teacup, pinecone), 600 coins each. Art is painted by Codex in the style of the existing houses and registered so the doorway matches (`tools/home/houses`).
- Bird outfits: 5 per species (was 3). Outfits 4 and 5 are large whole-body costumes in the species' own colour family (hood, helmet, collar) and read clearly different from the plain bird. Prices 1000 and 1300 (`Shop.OUTFIT_PRICES`, `Shop.OUTFIT_COUNT`).
- Pipeline: `tools/birds/costumes` (`paint.sh` Codex job, `collect.py`, `make_overlay.py` extracts the overlay and front/blink frames, then `tools/birds/measure_outfit_anchors.py --only ...`).
- Detail page shows six tiles in two rows of three. Save fields are unchanged; owned outfit ids are the same format.
