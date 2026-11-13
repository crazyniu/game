# Offline lettering fonts

`nunito.ttf` is the Nunito variable weight font from the official Google Fonts repository, downloaded on 2026-11-07:

- Font: https://raw.githubusercontent.com/google/fonts/main/ofl/nunito/Nunito%5Bwght%5D.ttf
- License: https://raw.githubusercontent.com/google/fonts/main/ofl/nunito/OFL.txt

The font is licensed under the SIL Open Font License 1.1; see `OFL.txt`. It supplies whole labels and mathematical symbols.

`lilita_one.ttf` supplies the rounded display numerals 0–9 and labels listed in the catalog's `display_labels`, including the board HUD's Level label. It was downloaded from the official Google Fonts repository on 2026-11-07:

- Font: https://raw.githubusercontent.com/google/fonts/main/ofl/lilitaone/LilitaOne-Regular.ttf
- License: https://raw.githubusercontent.com/google/fonts/main/ofl/lilitaone/OFL.txt (retained as `lilita_one_license.txt`)

Both fonts are used only by `tools/kit/build_documents.py` for the long baked document pages (terms, credits, contact). No visible UI word or number uses a font: those are hand-painted pictures.

Pillow's macOS wheels require [FriBiDi for RAQM shaping](https://pillow.readthedocs.io/en/stable/installation/basic-installation.html). The isolated environment avoids reusing a system Pillow without RAQM support.

Append `--numbers-only` to regenerate the 17 numeral/symbol cells and their manifest metrics while retaining existing labels and headings.

Append `--labels-only hud.level` to regenerate only that complete label and its manifest entry. This preserves all other labels and numeral cells.

Source catalogs in `tools/kit/locales/` declare `font` and `number_font` paths relative to the catalog and can include explicit `\n` line breaks in phrases. The renderer shapes each complete phrase through Pillow's RAQM layout engine and writes white ink on transparency. Labels retain their line metrics. Numerals use a separate 128 px ink-height canvas, a common digit baseline and equal digit advances; symbols have proportional advances and are centred within the digit band.

`noto_sans_sc_bold.otf` is the Simplified Chinese face of Noto Sans CJK Bold (Google and Adobe, SIL Open Font License 1.1; see `OFL.txt`), cut down to the characters the zh locale uses by `tools/kit/subset_cjk_font.py` (needs fonttools and the system `NotoSansCJK-Bold.ttc`). Re-run it after adding Chinese phrases. Used only by the offline builders.

English labels and legal pages use `lilita_one.ttf`, the same display face as the logo. Nunito remains only for numeral cells. Chinese uses the quieter `noto_sans_sc_bold.otf` above (the earlier ZCOOL KuaiLe lettering was judged too cartoonish).
