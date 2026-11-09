# Chinese labels and sleep deadlock

- Chinese (`zh`) is a second locale beside English: catalogs `tools/kit/locales/zh.json` and `zh_docs.json`, baked packs in `PerchSort/art/locale/zh`, listed in `art/locale/catalog.json`. The game follows the system language and falls back to English. Captures take `--lang=zh_CN`.
- All baked words use rounded game lettering: Lilita One (English) and ZCOOL KuaiLe (Chinese), including the settings documents. Nunito remains only for numeral cells.
- Home and map tile pictures with painted-in English get Chinese copies derived by `tools/kit/localize_home_buttons.py`; originals are untouched.
- Rules: when no move exists and only sleeping birds remain, they wake; if only daytime-sleeping nocturnal birds remain, night falls (event cause `stuck`, longer fade, existing "Night falls" banner).
