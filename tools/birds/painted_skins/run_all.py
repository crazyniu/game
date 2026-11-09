#!/usr/bin/env python3
"""run_all.py [--workers N] [skin ...]: paint every missing skin frame with local Codex, check, collect.

Resumable: frames whose out/<skin>_<frame>.png exists and passes the check are skipped. A frame fails the check when
its picture is not one connected bird of about the plain frame's size (two birds, fragments, empty). Failed frames are
removed and retried up to --tries times. Finished skins are collected into PerchSort/birds by collect.py.
Skins default to every costume of every species (outfits 1 to 5), Ember first.
"""
import argparse, os, subprocess, sys, threading, queue, shutil
import numpy as np
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))))
D = os.path.join(ROOT, "tools/birds/painted_skins")
BIRDS = os.path.join(ROOT, "PerchSort/birds")
REF_DIRS = [os.environ.get("SKIN_REF_ROOT", "/home/developer/Projects/game")]
SPECIES = "ember berry cocoa hoot ink mint sky snow sunny".split()
FRAMES = open(os.path.join(D, "jobs.txt")).read().split() + ["front", "front_blink_01", "front_blink_02"]
PER_JOB = 7


def reference_source(sp, n):
    for root in REF_DIRS:
        for rel in ("tools/birds/outfit_turns/src/%s_outfit_%d_front.png" % (sp, n),
                    "tools/birds/costumes/out/%s_outfit_%d_raw.png" % (sp, n)):
            if os.path.exists(os.path.join(root, rel)):
                return os.path.join(root, rel)
    return None


def prepare(skin):
    sp, n = skin.split("_")[0], int(skin.split("_")[-1])
    os.makedirs(os.path.join(D, "src"), exist_ok=True)
    os.makedirs(os.path.join(D, "out"), exist_ok=True)
    ref = os.path.join(D, "src", skin + "_reference.png")
    if not os.path.exists(ref):
        src = reference_source(sp, n)
        if src is None:
            raise SystemExit("no reference for " + skin)
        Image.open(src).convert("RGBA").resize((1024, 1024), Image.LANCZOS).save(ref)
    for f in FRAMES:
        dst = os.path.join(D, "src", "%s_%s.png" % (sp, f))
        if not os.path.exists(dst):
            Image.open(os.path.join(BIRDS, "%s_%s.png" % (sp, f))).convert("RGBA").resize((1024, 1024), Image.LANCZOS).save(dst)


def component_sizes(mask):
    """Sizes of the 4-connected regions of a boolean mask (iterative flood fill)."""
    seen = np.zeros_like(mask)
    h, w = mask.shape
    sizes = []
    for y0, x0 in zip(*np.nonzero(mask)):
        if seen[y0, x0]:
            continue
        stack, n = [(y0, x0)], 0
        seen[y0, x0] = True
        while stack:
            y, x = stack.pop()
            n += 1
            for ny, nx in ((y + 1, x), (y - 1, x), (y, x + 1), (y, x - 1)):
                if 0 <= ny < h and 0 <= nx < w and mask[ny, nx] and not seen[ny, nx]:
                    seen[ny, nx] = True
                    stack.append((ny, nx))
        sizes.append(n)
    return sizes


def good(skin, frame):
    sp = skin.split("_")[0]
    path = os.path.join(D, "out", "%s_%s.png" % (skin, frame))
    if not os.path.exists(path):
        return False
    try:
        im = Image.open(path).convert("RGBA").resize((256, 256))
        src = Image.open(os.path.join(D, "src", "%s_%s.png" % (sp, frame))).convert("RGBA").resize((256, 256))
    except Exception:
        return False
    a = np.asarray(im.getchannel("A")) > 128
    b = np.asarray(src.getchannel("A")) > 128
    if a.sum() < 500:
        return False
    big = [n for n in component_sizes(a) if n > 0.02 * a.sum()]
    if len(big) > 1:           # a second bird or loose pieces
        return False
    ratio = a.sum() / max(b.sum(), 1)
    if not 0.85 < ratio < 2.4:  # a costume adds area; far more or less means a wrong picture
        return False
    return True


def paint(skin, frames):
    subprocess.run(["bash", os.path.join(D, "paint.sh"), skin] + frames, cwd=ROOT, stdout=subprocess.DEVNULL,
                   stderr=subprocess.DEVNULL, check=False)


LOCK = threading.Lock()
COLLECTED = set()


def try_collect(skin):
    with LOCK:
        if skin in COLLECTED or any(not good(skin, f) for f in FRAMES):
            return
        COLLECTED.add(skin)
        subprocess.run([sys.executable, os.path.join(D, "collect.py"), skin], stdout=subprocess.DEVNULL, check=True)
        print("COLLECTED %s" % skin, flush=True)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--workers", type=int, default=12)
    ap.add_argument("--tries", type=int, default=3)
    ap.add_argument("skins", nargs="*")
    args = ap.parse_args()
    skins = args.skins or ["%s_outfit_%d" % (sp, n) for sp in SPECIES for n in range(1, 6)]
    for skin in skins:
        prepare(skin)
    for attempt in range(args.tries):
        jobs = queue.Queue()
        for skin in skins:
            todo = [f for f in FRAMES if not good(skin, f)]
            for f in todo:
                p = os.path.join(D, "out", "%s_%s.png" % (skin, f))
                if os.path.exists(p):
                    os.rename(p, p + ".rejected")
            for k in range(0, len(todo), PER_JOB):
                jobs.put((skin, todo[k:k + PER_JOB]))
        total = jobs.qsize()
        print("attempt %d: %d jobs" % (attempt + 1, total), flush=True)
        if total == 0:
            break

        def worker():
            while True:
                try:
                    skin, frames = jobs.get_nowait()
                except queue.Empty:
                    return
                paint(skin, frames)
                try_collect(skin)
                print("done %s %s.. (%d left)" % (skin, frames[0], jobs.qsize()), flush=True)
        threads = [threading.Thread(target=worker) for _ in range(args.workers)]
        for t in threads:
            t.start()
        for t in threads:
            t.join()
    for skin in skins:
        bad = [f for f in FRAMES if not good(skin, f)]
        if bad:
            print("INCOMPLETE %s: %s" % (skin, " ".join(bad)), flush=True)
        else:
            try_collect(skin)


if __name__ == "__main__":
    main()
