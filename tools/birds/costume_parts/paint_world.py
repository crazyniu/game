#!/usr/bin/env python3
"""paint_world.py <slot> <bird> [--workers N] [frame...]: Codex paints every frame of one world bird.

A world bird stands in a species slot of a world (oriole stands in the sunny slot of world 2). One local Codex job per frame: input 1 = the plain
frame of the slot species (the pose), input 2 = the design of the world bird (tools/birds/world_designs/birds/w<world>_<bird>.png). Result:
out/<slot>_<bird>_<frame>_worn.png. Resumable. Run from the repository root.
"""
import glob, os, shutil, subprocess, sys, threading, queue
from PIL import Image
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from paint_frames import WORLD_FRAMES, BIRDS, D, ROOT, plain_image

DESIGNS = os.path.join(ROOT, "tools/birds/world_designs")


def run_codex(cmd, **kw):
    try:
        subprocess.run(cmd, timeout=1500, **kw)
    except subprocess.TimeoutExpired:
        pass


HEAD = ("Edit the first input image: a cartoon plush bird in one pose. The second input image is the design of a DIFFERENT bird. Use your image "
        "generation tool once to paint the bird of the second image in exactly the pose of the first image: same position, size, framing, head angle, "
        "body tilt, wing and feet positions, eyelid state and lighting. The new bird keeps its own colours, markings, beak, crest, tail and eye colour "
        "from the second image, drawn in the same soft plush cartoon style, render quality and body proportions as the first bird. It is a whole new bird, "
        "not a costume on the first. Keep any crest or long tail but inside the picture with margin. Exactly one bird, no ground shadow, no motion blur, "
        "no extra copies, no props, nothing floating around the bird, fully transparent background. ")


def design_of(bird):
    for line in open(DESIGNS + "/birds.tsv"):
        c = line.rstrip("\n").split("\t")
        if len(c) >= 3 and c[1] == bird:
            return "%s/birds/w%s_%s.png" % (DESIGNS, c[0], bird), c[2]
    raise SystemExit("unknown world bird " + bird)


def main():
    args = sys.argv[1:]
    workers = 12
    if "--workers" in args:
        i = args.index("--workers"); workers = int(args[i + 1]); del args[i:i + 2]
    slot, bird = args[:2]; only = args[2:]
    ref, text = design_of(bird)
    os.makedirs(D + "/src/frames", exist_ok=True)
    jobs = queue.Queue()
    for f in only or WORLD_FRAMES:
        out = "%s/out/%s_%s_%s_worn.png" % (D, slot, bird, f)
        if os.path.exists(out):
            continue
        src = "%s/src/frames/%s_%s.png" % (D, slot, f)
        if not os.path.exists(src):
            plain_image(slot, f).resize((1024, 1024), Image.LANCZOS).save(src)
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
                run_codex(["codex", "exec", "--skip-git-repo-check", "--dangerously-bypass-approvals-and-sandbox", "-C", ROOT, "-i", src, "-i", ref,
                           "--", HEAD + "The second bird: " + text + "."], stdin=subprocess.DEVNULL, stdout=fh, stderr=subprocess.STDOUT)
            sid = next((l.split()[2] for l in open(log, errors="ignore") if l.startswith("session id:")), "")
            imgs = sorted(glob.glob(os.path.expanduser("~/.codex/generated_images/%s/*.png" % sid)), key=os.path.getmtime)
            if imgs:
                shutil.copy(imgs[-1], out)
            print("%s %s (%d left)" % (f, "ok" if imgs else "FAILED", jobs.qsize()), flush=True)

    ts = [threading.Thread(target=work) for _ in range(workers)]
    [t.start() for t in ts]; [t.join() for t in ts]


if __name__ == "__main__":
    main()
