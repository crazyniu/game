# Language switch and icon-only home buttons

- The game ships two painted-word packs, `en` and `zh`, in `PerchSort/art/locale/`. The player picks one in Settings (a pill that shows the language in use); the choice is saved as `language` in the profile ("" follows the device). `--lang=zh` forces a locale for captures.
- Chinese words are baked offline from Noto Sans SC Bold (SIL OFL), subset by `tools/kit/subset_cjk_font.py`, by `tools/kit/build_localized_labels.py zh` and `build_documents.py zh`. Sources: `tools/kit/locales/zh.json`, `zh_docs.json`. The game never loads a font at runtime.
- Home and map-bottom buttons are icon-only: `UI.icon_tile` draws a rimless cream plate in code and the painted icon `art/home_icon_<name>.png` on it. Play is `home_play_plate` plus the `action.play` word of the current locale.
- The daily challenge is named "Daily Challenge" / "每日挑战" (it is today's dealt puzzle, not a daily gift).
