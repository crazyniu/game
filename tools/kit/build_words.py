"""Build painted word sprites from the lettering glyphs.

python3 tools/kit/build_words.py [--check]

Phrases: every string literal passed as the second argument of UI.word( ... ) in PerchSort/game/*.gd, plus each
line of tools/kit/words.txt (data-driven text such as bird, item and world names). Output:
PerchSort/art/words/word_<slug>.png, white with alpha, 1.5 caps high with the baseline at 1.15 caps, built at
cap height 96 (UI.WORD_CAP). The slug matches UI.slug. --check only reports phrases without a sprite.
"""
import glob
import json
import os
import re
import sys

from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
P = os.path.join(ROOT, "PerchSort")
CAP = 96.0
GAP = 0.07


def slug(text):
    out = "".join(ch if ch.isascii() and ch.isalnum() else "_" for ch in text.lower())
    while "__" in out:
        out = out.replace("__", "_")
    return out.strip("_")


def phrases():
    found = set()
    for path in glob.glob(os.path.join(P, "game", "*.gd")):
        src = open(path, encoding="utf-8").read()
        for m in re.finditer(r'UI\.word\(\s*[^,]+,\s*"((?:[^"\\]|\\.)*)"', src):
            found.add(m.group(1))
        for m in re.finditer(r'#\s*words:\s*(.+)', src):   # "# words: A | B" lists phrases built at runtime
            found.update(w.strip() for w in m.group(1).split("|") if w.strip())
    words_txt = os.path.join(ROOT, "tools", "kit", "words.txt")
    if os.path.exists(words_txt):
        for line in open(words_txt, encoding="utf-8"):
            line = line.strip()
            if line and not line.startswith("#"):
                found.add(line)
    return sorted(found)


def build(text, meta, letters_dir):
    k = CAP / meta["cap"]
    parts, x = [], 0.0
    for ch in text:
        if ch == " ":
            x += meta["space"] * k
            continue
        g = meta["glyphs"].get(str(ord(ch)))
        if g is None:
            raise SystemExit("no glyph for %r in %r" % (ch, text))
        im = Image.open(os.path.join(letters_dir, "letter_%04x.png" % ord(ch))).convert("RGBA")
        im = im.resize((max(1, round(g["w"] * k)), max(1, round(g["h"] * k))), Image.LANCZOS)
        parts.append((im, x, g["dy"] * CAP))
        x += im.width + GAP * CAP
    w = int(round(x - GAP * CAP)) + 4
    out = Image.new("RGBA", (max(w, 4), int(CAP * 1.5)), (255, 255, 255, 0))
    for im, px, dy in parts:
        out.alpha_composite(im, (int(round(px)) + 2, int(round(CAP * 1.15 + dy))))
    return out


def main():
    letters_dir = os.path.join(P, "art", "letters")
    meta = json.load(open(os.path.join(letters_dir, "letters.json")))
    out_dir = os.path.join(P, "art", "words")
    os.makedirs(out_dir, exist_ok=True)
    wanted = {slug(t): t for t in phrases()}
    if "--check" in sys.argv:
        missing = [t for s, t in wanted.items() if not os.path.exists(os.path.join(out_dir, "word_%s.png" % s))]
        print("missing word sprites:", missing)
        sys.exit(1 if missing else 0)
    for s, text in wanted.items():
        build(text, meta, letters_dir).save(os.path.join(out_dir, "word_%s.png" % s))
    stale = [f for f in os.listdir(out_dir) if f.endswith(".png") and f[5:-4] not in wanted]
    for f in stale:
        os.remove(os.path.join(out_dir, f))
        if os.path.exists(os.path.join(out_dir, f + ".import")):
            os.remove(os.path.join(out_dir, f + ".import"))
    print("words: %d built, %d stale removed" % (len(wanted), len(stale)))


main()
