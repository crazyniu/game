#!/usr/bin/env python3
"""strings.py: build PerchSort/assets/resources/i18n/strings.json ({language: {key: text}}) from i18n/strings.csv.

The CSV has one row per key and one column per language (zh_CN, en, zh_TW, ja, ko, de, fr, es, pt, it, ru, tr); %d and %s are
filled in order at runtime (game/I18n.ts). Run from the repository root, then `node tools/cocos-assets.mjs` (the manifest embeds
the table) and `python3 tools/ui/font_subset.py` (the font keeps only the characters in use).
"""
import csv, json, os

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "../.."))
rows = list(csv.DictReader(open(ROOT + "/i18n/strings.csv", encoding="utf-8")))
table = {lang: {r["keys"]: r[lang] for r in rows if r[lang]} for lang in rows[0] if lang != "keys"}
out = ROOT + "/PerchSort/assets/resources/i18n/strings.json"
os.makedirs(os.path.dirname(out), exist_ok=True)
json.dump(table, open(out, "w", encoding="utf-8"), ensure_ascii=False, indent=1)
print(len(rows), "keys,", len(table), "languages ->", out)
