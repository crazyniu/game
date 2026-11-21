#!/usr/bin/env python3
"""paint_frames.py <species> <reference.png> <costume-text-file> [--workers N] [frame...]: Codex paints the costume onto each plain frame.

One local Codex job per frame: input = the plain frame (1024) and the reference picture of the costumed bird. The result
out/<species>_<frame>_worn.png is the whole frame with the costume; make_parts.py cuts the costume layer out of it.
Resumable: frames with an output are skipped. Run from the repository root.
"""
import glob, os, shutil, subprocess, sys, threading, queue
from PIL import Image


def run_codex(cmd, **kw):
    try:
        subprocess.run(cmd, timeout=1500, **kw)
    except subprocess.TimeoutExpired:
        pass

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
D = os.path.join(ROOT, "tools/birds/costume_parts")
BIRDS = os.path.join(ROOT, "PerchSort/assets/resources/birds")
HEAD = ("Edit the first input image: one frame of a cartoon plush bird. The second input image shows the same bird from the "
        "front wearing a costume part. Use your image generation tool once to paint the SAME bird in exactly the same pose, "
        "position, size, framing, head angle, body tilt, wing and feet positions, eyes, eyelids, beak and lighting as the first image, "
        "now wearing the costume part exactly as designed in the second image (same shapes, panels, colours, button and size relative "
        "to the head), seen from THIS frame's angle: when the head turns, tilts, bows or lifts, the costume part sits on the head and "
        "turns, tilts, bows or lifts with it, with its own perspective and thickness. The bird's face, eyes, eyelids, beak, wings, "
        "body and feet stay exactly as in the first image. Exactly one bird, no ground shadow, no motion blur, no extra copies, no "
        "props. Keep the whole bird inside the picture with margin, fully transparent background, nothing else added. ")


SKIN_FRAMES = ['front', 'front_blink_01', 'front_blink_02'] + ['flight_flap_%02d' % i for i in range(1, 7)]

# A world bird keeps the actions the match game uses: standing, blinking, hopping, cheering, flying (the left-facing flight frames are mirrored).
WORLD_FRAMES = (['front', 'front_blink_01', 'front_blink_02'] + ['hop_%02d' % i for i in range(1, 7)] + ['cheer_%02d' % i for i in range(1, 7)]
                + ['flight_flap_%02d' % i for i in range(1, 7)] + ['flight_takeoff_%02d' % i for i in range(1, 4)] + ['flight_land_%02d' % i for i in range(1, 4)])
MIRRORED = ['flight_flap_%02d' % i for i in range(1, 7)] + ['flight_takeoff_%02d' % i for i in range(1, 4)] + ['flight_land_%02d' % i for i in range(1, 4)]
FRAME_SETS = {'skin': SKIN_FRAMES, 'world': WORLD_FRAMES}


def plain_image(species, frame):
    """The plain frame of a bird as RGBA, from a single file or from the bird's sheet."""
    sys.path.insert(0, os.path.join(ROOT, "tools/birds"))
    from pack_sheets import frame_image
    return frame_image("%s_%s" % (species, frame))


def frames_of(species):
    names = []
    for f in sorted(glob.glob(os.path.join(BIRDS, species + "_*.png")) + glob.glob(os.path.join(BIRDS, species + "_*.webp"))):
        n = os.path.splitext(os.path.basename(f))[0][len(species) + 1:]
        if "outfit" in n or n in ("face", "fluff", "idle") or n.endswith("overlay"):
            continue
        names.append(n)
    return names


def main():
    args = sys.argv[1:]
    workers = 12
    if "--workers" in args:
        i = args.index("--workers"); workers = int(args[i + 1]); del args[i:i + 2]
    species, ref, text = args[:3]
    only = args[3:]
    costume = open(text).read().strip()
    os.makedirs(D + "/src/frames", exist_ok=True)
    jobs = queue.Queue()
    for f in only or frames_of(species):
        out = "%s/out/%s_%s_worn.png" % (D, species, f)
        if os.path.exists(out):
            continue
        src = "%s/src/frames/%s_%s.png" % (D, species, f)
        if not os.path.exists(src):
            plain_image(species, f).resize((1024, 1024), Image.LANCZOS).save(src)
        jobs.put((f, src, out))
    print("jobs", jobs.qsize(), flush=True)

    def work():
        while True:
            try:
                f, src, out = jobs.get_nowait()
            except queue.Empty:
                return
            log = out[:-4] + ".log"
            cmd = ["codex", "exec", "--skip-git-repo-check", "--dangerously-bypass-approvals-and-sandbox", "-C", ROOT,
                   "-i", src, "-i", ref, "--", HEAD + "The costume part: " + costume]
            with open(log, "w") as fh:
                subprocess.run(["timeout", "1500"] + cmd, stdin=subprocess.DEVNULL, stdout=fh, stderr=subprocess.STDOUT)
            sid = ""
            for line in open(log, errors="ignore"):
                if line.startswith("session id:"):
                    sid = line.split()[2]; break
            imgs = sorted(glob.glob(os.path.expanduser("~/.codex/generated_images/%s/*.png" % sid)), key=os.path.getmtime)
            if imgs:
                shutil.copy(imgs[-1], out)
            print("%s %s (%d left)" % (f, "ok" if imgs else "FAILED", jobs.qsize()), flush=True)

    ts = [threading.Thread(target=work) for _ in range(workers)]
    [t.start() for t in ts]; [t.join() for t in ts]


if __name__ == "__main__":
    main()
