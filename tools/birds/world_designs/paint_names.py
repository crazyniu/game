#!/usr/bin/env python3
"""paint_names.py <world|skins1..skins5> <en|zh>: paint the name labels of one world's birds, or of nine skin designs, as a lettering sheet with local Codex.

Input: names.tsv (world, bird id, English, Chinese) or skin_names.tsv (slot, design, English, Chinese; nine rows per skins set) and the style reference (the existing gold labels). Output:
out/names/job_<lang>_gold_<bird ids of the world>/words.png + texts.json, the format of tools/island/install_painted_words.py. The sheet has one name
per line, top to bottom. Check the spelling letter by letter before installing. Run from the repository root.
"""
import glob, json, os, shutil, subprocess, sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "../../.."))
world, lang = sys.argv[1], sys.argv[2]
if world.startswith("skins"):
    k = int(world[5:])
    rows = [l.rstrip("\n").split("\t") for l in open(HERE + "/skin_names.tsv")][(k - 1) * 9:k * 9]
else:
    rows = [l.rstrip("\n").split("\t") for l in open(HERE + "/names.tsv") if l.split("\t")[0] == world]
texts = [r[2] if lang == "en" else r[3] for r in rows]
style = ROOT + "/docs/work-log/world-birds/evidence/name_style.png"
job = "%s/out/names/job_%s_gold_%s" % (HERE, lang, world if world.startswith("skins") else "world" + world)
os.makedirs(job, exist_ok=True)
lines = "\n".join("%d. %s" % (i + 1, t) for i, t in enumerate(texts))
prompt = ("Create one image: a lettering sheet for a mobile game. The reference image shows the lettering style: glossy golden-yellow bubble letters with a thick "
          "dark brown outline and a soft highlight. Paint exactly these %d words or names, one per line, top to bottom, each on its own row with a lot of empty "
          "space between the rows, in that style, all with the same letter height, every line horizontally centered, on a fully transparent background, nothing "
          "else in the picture. Spell every line exactly as written, %s:\n%s" % (len(texts), "in English" if lang == "en" else "in Simplified Chinese characters", lines))
log = job + "/codex.log"
with open(log, "w") as fh:
    try:
        subprocess.run(["codex", "exec", "--skip-git-repo-check", "--dangerously-bypass-approvals-and-sandbox", "-C", ROOT, "-i", style, "--", prompt],
                       stdin=subprocess.DEVNULL, stdout=fh, stderr=subprocess.STDOUT, timeout=1500)
    except subprocess.TimeoutExpired:
        pass
sid = next((l.split()[2] for l in open(log, errors="ignore") if l.startswith("session id:")), "")
imgs = sorted(glob.glob(os.path.expanduser("~/.codex/generated_images/%s/*.png" % sid)), key=os.path.getmtime)
if not imgs:
    sys.exit("no picture; see " + log)
shutil.copy(imgs[-1], job + "/words.png")
json.dump(texts, open(job + "/texts.json", "w"), ensure_ascii=False)
print("sheet", job + "/words.png", texts)
