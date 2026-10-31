# Third-party licenses

## Fredoka
- Author: The Fredoka Project Authors (https://github.com/hafontia/Fredoka-One).
- Source: https://github.com/google/fonts/tree/main/ofl/fredoka
- Font source: https://raw.githubusercontent.com/google/fonts/main/ofl/fredoka/Fredoka%5Bwdth,wght%5D.ttf
- License: SIL Open Font License 1.1 (OFL), full text at `PerchSort/Assets/Fonts/OFL.txt`.
- Bundled static instance: `Fredoka-Medium.ttf`, weight 500 / width 100; TextMeshPro SDF atlas generated locally by `UIAssetBuilder`.

## UI geometry
Rounded panel and icon PNGs were created for this project by `art/ui_sources/generate_ui.py`. They contain no external source artwork. Existing bird and environment originals are unchanged; see `art/ASSETS.md` for their provenance.

## TextMeshPro essential resources
`Assets/Fonts/TextMesh Pro` contains Unity's bundled TextMeshPro essential resources from the installed `com.unity.ugui` package (shaders, settings and default fallback assets). Source: https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/TMP.html ; distributed under the Unity Companion License, copied to `Assets/Fonts/TextMesh Pro/Unity Companion License.md`. The bundled Liberation Sans fallback has its own SIL OFL text at `Assets/Fonts/TextMesh Pro/Fonts/LiberationSans - OFL.txt`; player UI explicitly uses Fredoka.

## T10a v2 original game artwork — 2026-10-31

The 25 new production PNGs documented in `art/ASSETS.md` were individually generated or edited for this project using OpenAI's built-in `image_gen.imagegen` tool. They comprise four environment sprites (crate, basket, nest, nest_done), nine effects (zzz, sparkle, check, feather, ring, three confetti shapes, closed_eye), two reward sprites (trophy, star), and ten sleeping bird variants (`bird_01s` through `bird_10s`). No external stock artwork, third-party characters, or third-party font files were used to create these sprites.

Sleeping birds use only the project's existing original bird sprites as edit targets; nest_done uses the newly generated original empty nest. Three drafts with white sticker borders were revised through the same tool before delivery. Selected unmodified generation masters are retained in `art/source/v2/`; exact prompts, edit-target paths and generation provenance are in `art/prompts/generation_v2.json`. Local Pillow processing only packages dimensions, transparency, alignment and review sheets. This is an artwork provenance record; no separate third-party asset license is being bundled for these generated sprites.

## T13a original synthesized audio — 2026-10-31

All 18 WAV files in `audio_src/` were composed and synthesized specifically for
Perch Sort by the reproducible project script `scripts/audio/generate.py`.
This includes `bgm_menu`, `bgm_game`, `chirp_01` through `chirp_05`, `flutter`,
`sleep_z`, `nest_home`, `crate_open`, `star_1` through `star_3`, `win_fanfare`,
`hint`, `ui_tap`, and `ui_back`. Original melodies, oscillators, envelopes,
filtered seeded noise and delay taps supply all audible content; no third-party
samples, recordings, soundfonts, borrowed melodies or copyrighted source audio
were used. These are project-original assets, with no separate third-party
audio-asset license or attribution obligation introduced by source media.

NumPy, SciPy and pyloudnorm are build/measurement dependencies only and are not
embedded in the WAV files or added to the game runtime. Their pinned versions
are listed in `scripts/audio/requirements.txt`; their software licenses remain
applicable to those tools themselves. Source and reproduction instructions are
in `scripts/audio/README.md`; measured per-file metadata is in
`audio_src/MANIFEST.md` and `audio_src/analysis.json`.

## LitMotion 2.0.2 — first-level motion dependency

- Author/copyright: Yusuke Nakada, 2023; package by Annulus Games.
- Source: [immutable commit 0b4c588ee75a07198841d92aab653e6b39445089](https://github.com/annulusgames/LitMotion/tree/0b4c588ee75a07198841d92aab653e6b39445089).
- License: MIT. The exact upstream root LICENSE is retained at [licenses/LitMotion-MIT.txt](licenses/LitMotion-MIT.txt), because the UPM subdirectory package does not include that root file. Preserve this notice when redistributing the package code.
- Scope: the sole selected primary tween dependency; [T21](../tasks/T21_motion_dependency.md) records its pin and scoped desktop verification. The code license does not license third-party artwork/audio or certify runtime/mobile behavior.
