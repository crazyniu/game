"""Build painted word sprites from the lettering glyphs.

python3 tools/kit/build_words.py [--check]

Phrases: every string literal passed as the second argument of UI.word( ... ) in PerchSort/game/*.gd, plus each
line of tools/kit/words.txt (data-driven text such as bird, item and world names). Output:
PerchSort/art/words/word_<slug>.png: white letters with a warm outline and soft shadow, 1.5 caps plus 2 x PAD
high with the baseline at PAD + 1.15 caps, built at cap height 96 (UI.WORD_CAP, UI.WORD_PAD). The slug matches UI.slug. --check only reports phrases without a sprite.
"""
import glob
import json
import os
import re
import sys

from PIL import Image, ImageFilter

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
P = os.path.join(ROOT, "PerchSort")
CAP = 96.0
GAP = 0.17                 # letter gap in caps (room for the outline)
PAD = 29                   # canvas padding (UI.WORD_PAD) so outline and shadow are never cut
EDGE = (150, 82, 28)       # warm brown outline (UI.EDGE)
SHADOW = (60, 30, 10)


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
    """White lettering with a warm outline and a soft shadow, baseline at PAD + 1.15 caps."""
    k = CAP / meta["cap"]
    parts, x = [], 0.0
    for ch in text:
        if ch == " ":
            x += meta["space"] * k
            continue
        g = meta["glyphs"].get(str(ord(ch)))
        if g is None:
            raise SystemExit("no glyph for %r in %r" % (ch, text))
        im = Image.open(os.path.join(letters_dir, "letter_%04x.png" % ord(ch))).split()[3]
        im = im.resize((max(1, round(g["w"] * k)), max(1, round(g["h"] * k))), Image.LANCZOS)
        parts.append((im, x, g["dy"] * CAP))
        x += im.width + GAP * CAP
    w = int(round(x - GAP * CAP)) + 2 * PAD
    mask = Image.new("L", (max(w, 4), int(CAP * 1.5) + 2 * PAD), 0)
    for im, px, dy in parts:
        mask.paste(im, (int(round(px)) + PAD, int(round(PAD + CAP * 1.15 + dy))), im)
    r = int(round(meta.get("edge", 0.11) * CAP))
    edge = mask.filter(ImageFilter.MaxFilter(2 * r + 1))
    out = Image.new("RGBA", mask.size, (0, 0, 0, 0))
    shadow = Image.new("RGBA", mask.size, SHADOW + (0,))
    shadow.putalpha(edge.filter(ImageFilter.GaussianBlur(CAP * 0.04)).point(lambda v: int(v * 0.35)))
    out.alpha_composite(shadow, (0, int(CAP * 0.06)))
    out.paste(Image.new("RGBA", mask.size, EDGE + (255,)), (0, 0), edge)
    out.paste(Image.new("RGBA", mask.size, (255, 255, 255, 255)), (0, 0), mask)
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
