#!/usr/bin/env python3
"""run_skins.py [--set skin|world] [--workers N] <species_slot>...: paint, repaint and align skins or world birds one after another.

--set skin (default): keys are <species>_<slot> (sunny_1); frames are the skin set; paint_skin.py paints them.
--set world: keys are <slot>_<bird> (sunny_oriole); frames are the world set; paint_world.py paints them.
For each key: paint until every frame exists (up to 4 passes), align.py, then delete the paintings of frames flagged "check" and paint those again
(up to 2 rounds). When Codex reports its usage limit the driver sleeps until it resets and goes on without counting the pass. Progress lines go to
out/run_skins.status. Keys listed in out/skip.txt are skipped (another machine paints them). Resumable. Run from the repository root.
"""
import glob, json, os, re, subprocess, sys, time
HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "../../.."))
sys.path.insert(0, HERE)
from paint_frames import FRAME_SETS
PY = [sys.executable]


def log(msg):
    with open(HERE + "/out/run_skins.status", "a") as fh:
        fh.write("%s %s\n" % (time.strftime("%m-%d %H:%M:%S"), msg))
    print(msg, flush=True)


def missing(frames, species, slot):
    return [f for f in frames if not os.path.exists("%s/out/%s_%s_%s_worn.png" % (HERE, species, slot, f))]


def run(script, *args):
    subprocess.run(PY + [HERE + "/" + script] + list(args), cwd=ROOT)


def limit_wait(species, slot):
    """Seconds until the Codex usage limit resets, read from the newest logs of this key, or 0."""
    logs = sorted(glob.glob("%s/out/%s_%s_*.log" % (HERE, species, slot)), key=os.path.getmtime)[-6:]
    seconds = [int(m) for path in logs for m in re.findall(r'resets_in_seconds\D{0,6}(\d+)', open(path, errors="ignore").read())]
    return max(seconds) if seconds else 0


args = sys.argv[1:]
workers = "12"
kind = "skin"
if "--workers" in args:
    i = args.index("--workers"); workers = args[i + 1]; del args[i:i + 2]
if "--set" in args:
    i = args.index("--set"); kind = args[i + 1]; del args[i:i + 2]
frames = FRAME_SETS[kind]
painter = "paint_skin.py" if kind == "skin" else "paint_world.py"
for key in args:
    skip = open(HERE + "/out/skip.txt").read().split() if os.path.exists(HERE + "/out/skip.txt") else []
    if key in skip:
        log("skip " + key); continue
    species, slot = key.split("_")
    log("start " + key)
    for rnd in range(3):
        passes = 0
        while missing(frames, species, slot) and passes < 4:
            run(painter, species, slot, "--workers", workers)
            wait = limit_wait(species, slot) if missing(frames, species, slot) else 0
            if wait:
                log("usage limit reached: sleeping %d s" % (wait + 120))
                for old in glob.glob("%s/out/%s_%s_*.log" % (HERE, species, slot)):
                    os.remove(old)
                time.sleep(wait + 120)
            else:
                passes += 1
        run("align.py", species, slot, kind)
        table = json.load(open("%s/out/skin/%s_%s.json" % (HERE, species, slot)))
        bad = [f for f, v in table.items() if v["status"] != "ok"]
        log("%s round %d: %d frames, %d flagged %s, %d missing" % (key, rnd, len(table), len(bad), bad, len(missing(frames, species, slot))))
        if not bad or rnd == 2:
            break
        for f in bad:
            os.remove("%s/out/%s_%s_%s_worn.png" % (HERE, species, slot, f))
    log("done " + key)
