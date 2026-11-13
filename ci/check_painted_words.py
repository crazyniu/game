#!/usr/bin/env python3
"""Guards the painted-words rule (CLAUDE.md): every visible word is an AI-painted image, one set per language.

1. Every entry of every locale manifest ("labels", "numbers", "painted") points at an image file that exists
   (a regenerated or removed image can no longer leave a word invisible).
2. Font-rendered word call sites (UI.label, UI.heading, UiText.label, TextArt.label) are counted per file and may
   never go up: ci/font_label_baseline.json is a ratchet that only shrinks while the screens move to painted
   words. Lower a count when you remove call sites; the check fails if a file has more than its baseline.

python3 ci/check_painted_words.py [--update]   (--update lowers the baseline to the current counts)
"""
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
GAME = ROOT / "PerchSort"
BASELINE = ROOT / "ci" / "font_label_baseline.json"
CALL = re.compile(r"\b(?:UI|UiText|TextArt)\.(?:label|heading)\(")


def check_files():
    problems = []
    for pack in sorted((GAME / "art" / "locale").glob("*/manifest.json")):
        folder = pack.parent
        manifest = json.loads(pack.read_text(encoding="utf-8"))
        files = []
        for entry in manifest.get("labels", {}).values():
            files += [entry.get("file"), entry.get("heading_file")]
        for entry in manifest.get("painted", {}).values():
            files.append(entry.get("file"))
        for name in manifest.get("numbers", {}).get("glyphs", {}).values():
            files.append(name)
        for name in files:
            if name and not (folder / name).exists():
                problems.append("%s: missing image %s" % (folder.name, name))
    return problems


def count_calls():
    counts = {}
    for path in sorted((GAME / "game").glob("*.gd")):
        n = sum(1 for line in path.read_text(encoding="utf-8").splitlines()
                if CALL.search(line) and not line.lstrip().startswith(("#", "##", "static func", "func")))
        if n:
            counts[path.name] = n
    return counts


def main():
    problems = check_files()
    counts = count_calls()
    base = json.loads(BASELINE.read_text()) if BASELINE.exists() else {}
    for name, n in counts.items():
        if n > base.get(name, 0):
            problems.append("%s: %d font-rendered word call sites, baseline %d (paint the word instead)" % (name, n, base.get(name, 0)))
    if "--update" in sys.argv:
        BASELINE.write_text(json.dumps({k: min(v, base.get(k, v)) for k, v in counts.items()}, indent=1, sort_keys=True) + "\n")
    print("Painted words: %d font-rendered call sites left (%s), %d problem(s)"
          % (sum(counts.values()), ", ".join("%s %d" % kv for kv in counts.items()), len(problems)))
    for p in problems:
        print("  " + p)
    return 1 if problems else 0


sys.exit(main())
