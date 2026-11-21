#!/usr/bin/env python3
"""ui_words.py paint <style> <en|zh> | install <style>: paint and install the screen words listed in ui_words.tsv.

ui_words.tsv: key, style (lettering: the gold sign titles; caption: the dark brown lines), English, Chinese.
paint: one lettering sheet per style and language with local Codex, the existing words of that style as the style reference, written to
tools/island/out/ui_words/<style>_<lang>/words.png + texts.json. Check the spelling letter by letter before installing.
install: cuts each sheet into one picture per line (cut_rows.py), scales every line of a sheet by one factor to the stored cap height and
registers it in PerchSort/assets/resources/art/locale/<lang>/manifest.json as painted "<style>_<key>" and as word "<key>" of that style.
Then run `node tools/cocos-assets.mjs` and `node tools/cocos/meta.mjs`. Run from the repository root.
"""
import glob, json, os, shutil, subprocess, sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "../.."))
LOCALE = ROOT + "/PerchSort/assets/resources/art/locale/%s"
CAP = {"en": 96.0, "zh": 80.0}
REFERENCE = {"lettering": "lettering_daily_reward_title", "caption": "caption_chest_line"}
LOOK = {
    "lettering": "glossy golden-yellow bubble letters with a thick dark green outline and a soft highlight",
    "caption": "plain rounded dark chocolate-brown letters, no outline, no shine, no shadow",
}


def rows(style):
    return [r for r in (l.rstrip("\n").split("\t") for l in open(HERE + "/ui_words.tsv")) if r[1] == style]


def job(style, lang):
    return "%s/out/ui_words/%s_%s" % (HERE, style, lang)


def paint(style, lang):
    texts = [r[2] if lang == "en" else r[3] for r in rows(style)]
    out = job(style, lang)
    os.makedirs(out, exist_ok=True)
    lines = "\n".join("%d. %s" % (i + 1, t) for i, t in enumerate(texts))
    prompt = ("Create one image: a lettering sheet for a mobile game. The reference image shows the lettering style: %s. Paint exactly these %d "
              "words or phrases, one per line, top to bottom, each on its own row with a lot of empty space between the rows, in that style, all with "
              "the same letter height, every line horizontally centered, on a fully transparent background, nothing else in the picture. Spell every "
              "line exactly as written, %s:\n%s" % (LOOK[style], len(texts), "in English" if lang == "en" else "in Simplified Chinese characters", lines))
    reference = "%s/island_words/%s.png" % (LOCALE % lang, REFERENCE[style])
    log = out + "/codex.log"
    with open(log, "w") as fh:
        try:
            subprocess.run(["codex", "exec", "--skip-git-repo-check", "--dangerously-bypass-approvals-and-sandbox", "-C", ROOT, "-i", reference, "--", prompt],
                           stdin=subprocess.DEVNULL, stdout=fh, stderr=subprocess.STDOUT, timeout=1500)
        except subprocess.TimeoutExpired:
            pass
    sid = next((l.split()[2] for l in open(log, errors="ignore") if l.startswith("session id:")), "")
    images = sorted(glob.glob(os.path.expanduser("~/.codex/generated_images/%s/*.png" % sid)), key=os.path.getmtime)
    if not images:
        sys.exit("no picture; see " + log)
    shutil.copy(images[-1], out + "/words.png")
    json.dump(texts, open(out + "/texts.json", "w"), ensure_ascii=False)
    print("sheet", out + "/words.png", texts)


def install(style):
    import numpy as np
    from PIL import Image
    sys.path.insert(0, HERE)
    import cut_rows

    def body_height(piece):
        cover = (piece[..., 3] > 128).sum(axis=1).astype(float)
        lines = np.nonzero(cover >= 0.3 * cover.max())[0]
        return float(lines[-1] - lines[0] + 1)

    keys = [r[0] for r in rows(style)]
    for lang in ("en", "zh"):
        texts = json.load(open(job(style, lang) + "/texts.json"))
        assert len(texts) == len(keys)
        pieces = cut_rows.cut(job(style, lang) + "/words.png", len(texts))
        scale = CAP[lang] / sorted(body_height(p) for p in pieces)[len(pieces) // 2]
        folder = LOCALE % lang
        manifest = json.load(open(folder + "/manifest.json"))
        for key, piece in zip(keys, pieces):
            image = Image.fromarray(piece, "RGBA")
            image = image.resize((max(1, round(image.width * scale)), max(1, round(image.height * scale))), Image.LANCZOS)
            rel = "island_words/%s_%s.png" % (style, key)
            image.save("%s/%s" % (folder, rel))
            manifest["painted"]["%s_%s" % (style, key)] = {"file": rel}
            manifest["words"][key] = {"gold" if style == "lettering" else "caption": {"file": rel, "cap": CAP[lang]}}
        json.dump(manifest, open(folder + "/manifest.json", "w"), ensure_ascii=False, indent=2)
        print(lang, "installed", len(keys), style, "words")


if __name__ == "__main__":
    if sys.argv[1] == "paint":
        paint(sys.argv[2], sys.argv[3])
    else:
        install(sys.argv[2])
