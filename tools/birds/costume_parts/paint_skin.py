#!/usr/bin/env python3
"""paint_skin.py <species> <slot> [--workers N] [frame...]: Codex paints every frame of one skin.

One local Codex job per frame: input 1 = the plain frame (1024), input 2 = the approved reference of the costumed bird
(out/ref_<species>_<slot>.png). Result: out/<species>_<slot>_<frame>_worn.png. Resumable: frames with an output are skipped.
Run from the repository root.
"""
import glob, os, shutil, subprocess, sys, threading, queue
from PIL import Image


def run_codex(cmd, **kw):
    try:
        subprocess.run(cmd, timeout=1500, **kw)
    except subprocess.TimeoutExpired:
        pass
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from paint_frames import SKIN_FRAMES, BIRDS, D, ROOT
from refs import skins

HEAD = ("Edit the first input image: one frame of a cartoon plush bird. The second input image shows the same bird from the front "
        "wearing a costume. Use your image generation tool once to paint the SAME bird in exactly the same pose, position, size, framing, "
        "head angle, body tilt, wing and feet positions, eyes, eyelids, beak and lighting as the first image, now wearing the costume "
        "exactly as designed in the second image (same shapes, panels, colours, trims and size relative to the body), seen from THIS "
        "frame's angle: when the head turns, tilts, bows or lifts, the head part of the costume turns, tilts, bows or lifts with it; "
        "when the wings move, any sleeve or trim on them moves with them. The costume is worn on the body only and follows its shape. "
        "The bird's face, eyes, eyelids, beak and feet stay exactly as in the first image. Exactly one bird, no ground shadow, no motion "
        "blur, no extra copies, no props, nothing floating around the bird, no leaves, petals, sparkles or glow. Keep the whole bird "
        "inside the picture with margin, fully transparent background. ")


def main():
    args = sys.argv[1:]
    workers = 12
    if "--workers" in args:
        i = args.index("--workers"); workers = int(args[i + 1]); del args[i:i + 2]
    species, slot = args[0], args[1]; only = args[2:]
    spec = next(s for s in skins() if s["species"] == species and s["slot"] == int(slot))
    ref = "%s/out/ref_%s_%s.png" % (D, species, slot)
    assert os.path.exists(ref), "no approved reference " + ref
    os.makedirs(D + "/src/frames", exist_ok=True)
    jobs = queue.Queue()
    for f in only or SKIN_FRAMES:
        out = "%s/out/%s_%s_%s_worn.png" % (D, species, slot, f)
        if os.path.exists(out):
            continue
        src = "%s/src/frames/%s_%s.png" % (D, species, f)
        if not os.path.exists(src):
            Image.open("%s/%s_%s.png" % (BIRDS, species, f)).convert("RGBA").resize((1024, 1024), Image.LANCZOS).save(src)
        jobs.put((f, src, out))
    print("jobs", jobs.qsize(), flush=True)

    def work():
        while True:
            try:
                f, src, out = jobs.get_nowait()
            except queue.Empty:
                return
            log = out[:-4] + ".log"
            with open(log, "w") as fh:
                run_codex(["codex", "exec", "--skip-git-repo-check", "--dangerously-bypass-approvals-and-sandbox",
                                "-C", ROOT, "-i", src, "-i", ref, "--", HEAD + "Costume %s: %s. Palette: %s." % (spec["name"], spec["text"], spec["palette"])],
                               stdin=subprocess.DEVNULL, stdout=fh, stderr=subprocess.STDOUT)
            sid = next((l.split()[2] for l in open(log, errors="ignore") if l.startswith("session id:")), "")
            imgs = sorted(glob.glob(os.path.expanduser("~/.codex/generated_images/%s/*.png" % sid)), key=os.path.getmtime)
            if imgs:
                shutil.copy(imgs[-1], out)
            print("%s %s (%d left)" % (f, "ok" if imgs else "FAILED", jobs.qsize()), flush=True)

    ts = [threading.Thread(target=work) for _ in range(workers)]
    [t.start() for t in ts]; [t.join() for t in ts]


if __name__ == "__main__":
    main()
