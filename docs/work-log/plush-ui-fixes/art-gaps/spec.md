# Missing plush art

Paint and wire the coordinator's missing world ribbons, world captions, booster title ribbons, hanger and bulb glyphs, colored ability badges, and day/night mechanic disc.

World order is fixed: Green Forest, Bamboo Forest, Autumn Forest, Snow Forest. Use the existing map and gate art in that order. Preserve replacement canvas dimensions and alpha bounds within four pixels. All player-visible lettering is image-generated with its backing; geometry-only processing may crop, scale, mask, and compose the generated images.

Code ownership is limited to PaintedLabels, MapView's world title, PanelsView's WorldGate and Unlock methods and LevelCard mechanic strip, and ShopView/HudView icon references. Do not change rules, saves, levels, animation, or other layouts.

Review every delivered image on dark and light sheets, compare replacements against originals, and inspect every painted letter at 300 percent. Run only the requested silent state captures for World, Unlock, Level card, Outfits, Map, and Aviary. Do not run EditMode or PlayMode suites. The capture script's existing naming checks are part of that requested command. Preserve each filtered capture before the next run, which clears previous outputs.

Deliver assets, new-asset metadata, assigned code, review sheets, and report on mac-art-gaps/work. Push that branch without a PR.
