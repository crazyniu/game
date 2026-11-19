#!/usr/bin/env python3
"""run_skins.py [--workers N] <species_slot>...: paint, repaint and align whole skins one after another.

For each skin: paint_skin.py until every frame exists (up to 4 passes), align.py, then delete the paintings of frames flagged
"check" and paint those again (up to 2 rounds). Progress lines go to out/run_skins.status. Keys listed in out/skip.txt are skipped (another machine paints them). Resumable. Run from the repository root.
"""
import json, os, subprocess, sys, time
HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "../../.."))
sys.path.insert(0, HERE)
from paint_frames import SKIN_FRAMES
PY = [sys.executable]


def log(msg):
    with open(HERE + "/out/run_skins.status", "a") as fh:
        fh.write("%s %s\n" % (time.strftime("%m-%d %H:%M:%S"), msg))
    print(msg, flush=True)


def missing(species, slot):
    return [f for f in SKIN_FRAMES if not os.path.exists("%s/out/%s_%s_%s_worn.png" % (HERE, species, slot, f))]


def run(script, *args):
    subprocess.run(PY + [HERE + "/" + script] + list(args), cwd=ROOT)


args = sys.argv[1:]
workers = "12"
if "--workers" in args:
    i = args.index("--workers"); workers = args[i + 1]; del args[i:i + 2]
for key in args:
    skip = open(HERE + "/out/skip.txt").read().split() if os.path.exists(HERE + "/out/skip.txt") else []
    if key in skip:
        log("skip " + key); continue
    species, slot = key.split("_")
    log("start " + key)
    for rnd in range(3):
        for _ in range(4):
            if not missing(species, slot):
                break
            run("paint_skin.py", species, slot, "--workers", workers)
        run("align.py", species, slot)
        table = json.load(open("%s/out/skin/%s_%s.json" % (HERE, species, slot)))
        bad = [f for f, v in table.items() if v["status"] != "ok"]
        log("%s round %d: %d frames, %d flagged %s, %d missing" % (key, rnd, len(table), len(bad), bad, len(missing(species, slot))))
        if not bad or rnd == 2:
            break
        for f in bad:
            os.remove("%s/out/%s_%s_%s_worn.png" % (HERE, species, slot, f))
    log("done " + key)
