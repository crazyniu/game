# Task chinese: a Simplified Chinese version

1. Fonts (offline baking only, OFL licensed, record source URL and licence in `tools/kit/fonts/README.md` and add
   them to the Credits document): a rounded friendly Chinese display face for UI words (for example ZCOOL KuaiLe from
   the official google/fonts repository) and a readable body face for documents (for example Noto Sans SC). Download
   from the official repositories only.
2. `tools/kit/locales/zh.json`: every id of en.json translated into natural, short, friendly Simplified Chinese for a
   casual game (bird names may stay as they are or get cute Chinese names; keep them consistent everywhere). Same for
   `zh_docs.json`. Bake with the existing scripts extended for the zh locale (font per locale, CJK line wrapping in
   documents) into `PerchSort/art/locale/zh/`.
3. Runtime: the game picks zh when the OS locale is Chinese, else en; a Language row in Settings switches between
   English and 中文 (saved in profile: add the field `language`, the only new save field allowed in this task) and
   every label refreshes. Numbers stay the numeral sprites.
4. Check every screen in zh: nothing overflows its box (labels shrink to fit; long phrases get shorter wording).
Evidence: `bash scripts/screenshots.sh` run in zh (add a way to force the locale, for example `--locale=zh`), contact
sheet of all states, both languages switching live.
