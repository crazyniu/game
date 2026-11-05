# Animated outfit art contract

Produce cheer (six frames) and blink (three frames) for all nine wearable species at each of the three shop tiers: 27 outfits, 54 actions, 243 PNG frames. Try the built-in image generator for a GIF pilot first. If native animation is unsupported, generate each complete action as one pose sheet from the exact bare pose frames and outfit still, then use the existing slicing/registration method. Retain offline review GIFs under `art/cosmetics/gif/` and numbered PNGs in the canonical Birds resource directory. Only built-in Codex image generation is authorized; no external or paid generation service.

Use `docs/work-log/cosmetic-art/spec.md` for the exact accessories and the existing outfit still for identity. Each frame must repeat the corresponding bare-bird pose exactly, preserving the 320 x 320 RGBA canvas, scale, feet baseline and centered pivot. Accessories follow their attachment points. Copy import settings from the same species' idle metadata with unique asset GUIDs. Require light/dark transparency inspection and 64 px readability.

Own only new outfit animation resources and metadata, associated raw/prompt/QA/GIF assets under `art/cosmetics/`, and this work-log directory. Do not edit code or existing artwork. Deliver and push tier 1, then tier 2, then tier 3. Evidence contact sheets compare bare frames above outfit frames on light and dark backgrounds. Asset evidence does not establish integrated Unity motion acceptance.

The service capability pilot is not permission to relax native detail, exact pose matching, frame counts, appearance or transparency requirements. Reject nonconforming output before writing runtime assets.
