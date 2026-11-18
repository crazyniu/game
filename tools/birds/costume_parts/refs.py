#!/usr/bin/env python3
"""refs.py [--workers N] [species_slot...]: one Codex painting per skin of docs/work-log/skin-redesign/spec.md.

Reads the skin table of the spec, paints the plain front frame of the species wearing the costume, and writes
out/ref_<species>_<slot>.png. Resumable: finished references are skipped. Run from the repository root.
"""
import glob, os, re, shutil, subprocess, sys, threading, queue
from PIL import Image


def run_codex(cmd, **kw):
    try:
        subprocess.run(cmd, timeout=1500, **kw)
    except subprocess.TimeoutExpired:
        pass

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
D = os.path.join(ROOT, "tools/birds/costume_parts")
BIRDS = os.path.join(ROOT, "PerchSort/assets/resources/birds")
SPEC = os.path.join(ROOT, "docs/work-log/skin-redesign/spec.md")
HEAD = ("Edit the input image: the front view of a cartoon plush bird. Use your image generation tool once to paint the SAME bird "
        "in exactly the same pose, position, size, framing, face, eyes, eyelids, beak, wings and feet as the input, now wearing the "
        "costume below. The costume is worn on the body only: it sits on the head, face frame, chest and back of the bird and follows its "
        "round shape. Nothing floats around the bird: no props, no objects beside it, no leaves, petals, sparkles, glow or particles, "
        "no ground shadow. The costume is clearly different in colour and shape from the plain bird. Exactly one bird, fully "
        "transparent background, keep the whole bird inside the picture with margin. ")


def skins():
    rows = []
    for line in open(SPEC):
        c = [x.strip() for x in line.strip().strip("|").split("|")]
        if len(c) == 6 and c[1].isdigit():
            rows.append(dict(species=c[0], slot=int(c[1]), name=c[2], cls=c[3], text=c[4], palette=c[5]))
    return rows


def main():
    args = sys.argv[1:]
    workers = 12
    if "--workers" in args:
        i = args.index("--workers"); workers = int(args[i + 1]); del args[i:i + 2]
    jobs = queue.Queue()
    for s in skins():
        key = "%s_%d" % (s["species"], s["slot"])
        if args and key not in args:
            continue
        out = "%s/out/ref_%s.png" % (D, key)
        if os.path.exists(out):
            continue
        src = "%s/src/frames/%s_front.png" % (D, s["species"])
        if not os.path.exists(src):
            Image.open("%s/%s_front.png" % (BIRDS, s["species"])).convert("RGBA").resize((1024, 1024), Image.LANCZOS).save(src)
        jobs.put((key, src, out, HEAD + "Costume %s (%s): %s. Palette: %s." % (s["name"], s["cls"], s["text"], s["palette"])))
    print("jobs", jobs.qsize(), flush=True)

    def work():
        while True:
            try:
                key, src, out, prompt = jobs.get_nowait()
            except queue.Empty:
                return
            log = out[:-4] + ".log"
            with open(log, "w") as fh:
                run_codex(["codex", "exec", "--skip-git-repo-check", "--dangerously-bypass-approvals-and-sandbox",
                                "-C", ROOT, "-i", src, "--", prompt], stdin=subprocess.DEVNULL, stdout=fh, stderr=subprocess.STDOUT)
            sid = next((l.split()[2] for l in open(log, errors="ignore") if l.startswith("session id:")), "")
            imgs = sorted(glob.glob(os.path.expanduser("~/.codex/generated_images/%s/*.png" % sid)), key=os.path.getmtime)
            if imgs:
                shutil.copy(imgs[-1], out)
            print("%s %s (%d left)" % (key, "ok" if imgs else "FAILED", jobs.qsize()), flush=True)

    ts = [threading.Thread(target=work) for _ in range(workers)]
    [t.start() for t in ts]; [t.join() for t in ts]


if __name__ == "__main__":
    main()
