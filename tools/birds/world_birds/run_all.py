#!/usr/bin/env python3
"""run_all.py [--workers N] [--tries N] bird...: paint every missing frame of world birds with local Codex, check, collect.

A world bird is <species>_<family> (sunny_budgie): the bird that stands in a species slot in one world. It needs
src/<bird>_reference.png (its front design, 1024). Every frame of the plain species is repainted as the new bird
in the same pose (paint.sh); frames that are not one connected bird of plausible size are retried. Finished birds
are aligned to the plain frames (feet and lower-body centre) and written to PerchSort/birds/<bird>_<frame>.png.
Resumable. Reuses the checks of tools/birds/painted_skins.
"""
import argparse, os, subprocess, sys, threading, queue
import numpy as np
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))))
D = os.path.join(ROOT, "tools/birds/world_birds")
BIRDS = os.path.join(ROOT, "PerchSort/birds")
JOBS = os.path.join(ROOT, "tools/birds/painted_skins/jobs.txt")
FRAMES = open(JOBS).read().split() + ["front", "front_blink_01", "front_blink_02",
    "preen", "face"]
PER_JOB = 7
SIZE = 320
sys.path.insert(0, os.path.join(ROOT, "tools/birds/painted_skins"))
from run_all import component_sizes  # noqa: E402


def prepare(bird):
    sp = bird.split("_")[0]
    if not os.path.exists(os.path.join(D, "src", bird + "_reference.png")):
        raise SystemExit("no reference for " + bird)
    for f in FRAMES:
        dst = os.path.join(D, "src", "%s_%s.png" % (sp, f))
        if not os.path.exists(dst):
            Image.open(os.path.join(BIRDS, "%s_%s.png" % (sp, f))).convert("RGBA").resize((1024, 1024), Image.LANCZOS).save(dst)


def good(bird, frame):
    sp = bird.split("_")[0]
    path = os.path.join(D, "out", "%s_%s.png" % (bird, frame))
    if not os.path.exists(path):
        return False
    try:
        im = Image.open(path).convert("RGBA").resize((256, 256))
        src = Image.open(os.path.join(D, "src", "%s_%s.png" % (sp, frame))).convert("RGBA").resize((256, 256))
    except Exception:
        return False
    a = np.asarray(im.getchannel("A")) > 128
    b = np.asarray(src.getchannel("A")) > 128
    if a.sum() < 300:
        return False
    big = [n for n in component_sizes(a) if n > 0.02 * a.sum()]
    if len(big) > 1:
        return False
    return 0.7 < a.sum() / max(b.sum(), 1) < 1.8   # a different bird, but about the same size


def anchor(img):
    a = np.asarray(img.getchannel("A")) > 128
    rows = np.where(a.any(axis=1))[0]
    top, bottom = rows[0], rows[-1]
    ys, xs = np.nonzero(a[(top + bottom) // 2:bottom + 1])
    return float(xs.mean()), float(bottom)


def collect(bird):
    sp = bird.split("_")[0]
    for f in FRAMES:
        src = Image.open(os.path.join(BIRDS, "%s_%s.png" % (sp, f))).convert("RGBA")
        out = Image.open(os.path.join(D, "out", "%s_%s.png" % (bird, f))).convert("RGBA").resize((SIZE, SIZE), Image.LANCZOS)
        if f != "face":   # the face is a head icon, kept where it was painted
            sx, sb = anchor(src)
            ox, ob = anchor(out)
            dx, dy = round(sx - ox), round(sb - ob)
            fixed = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
            fixed.alpha_composite(out, (max(dx, 0), max(dy, 0)), (max(-dx, 0), max(-dy, 0)))
            out = fixed
        out.save(os.path.join(BIRDS, "%s_%s.png" % (bird, f)))
    print("COLLECTED %s" % bird, flush=True)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--workers", type=int, default=12)
    ap.add_argument("--tries", type=int, default=3)
    ap.add_argument("birds", nargs="+")
    args = ap.parse_args()
    for bird in args.birds:
        prepare(bird)
    for attempt in range(args.tries):
        jobs = queue.Queue()
        for bird in args.birds:
            todo = [f for f in FRAMES if not good(bird, f)]
            for f in todo:
                p = os.path.join(D, "out", "%s_%s.png" % (bird, f))
                if os.path.exists(p):
                    os.rename(p, p + ".rejected")
            for k in range(0, len(todo), PER_JOB):
                jobs.put((bird, todo[k:k + PER_JOB]))
        print("attempt %d: %d jobs" % (attempt + 1, jobs.qsize()), flush=True)
        if jobs.empty():
            break

        def worker():
            while True:
                try:
                    bird, frames = jobs.get_nowait()
                except queue.Empty:
                    return
                subprocess.run(["bash", os.path.join(D, "paint.sh"), bird] + frames, cwd=ROOT,
                               stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL, check=False)
                print("done %s %s.. (%d left)" % (bird, frames[0], jobs.qsize()), flush=True)
        threads = [threading.Thread(target=worker) for _ in range(args.workers)]
        for t in threads:
            t.start()
        for t in threads:
            t.join()
    for bird in args.birds:
        bad = [f for f in FRAMES if not good(bird, f)]
        if bad:
            print("INCOMPLETE %s: %s" % (bird, " ".join(bad)), flush=True)
        else:
            collect(bird)


if __name__ == "__main__":
    main()
