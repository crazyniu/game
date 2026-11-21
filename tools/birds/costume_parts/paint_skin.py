#!/usr/bin/env python3
"""paint_skin.py <species> <slot> [--workers N] [frame...]: Codex paints all frames of one skin in a single picture.

The nine plain frames of the skin set are laid out as a 3x3 sheet with an empty border round each frame (input 1); input 2, when it exists, is a
reference of the costumed bird (out/ref_<species>_<slot>.png); without it the costume text of the skin table alone designs the costume, and the nine
cells of one painting agree with each other. One Codex painting dresses every cell; the frame area of each cell is cut, cleaned of stray specks and written as
out/<species>_<slot>_<frame>_worn.png, the format align.py reads. Only missing frames are written, so a repaint of flagged frames costs one
painting. --workers is accepted for run_skins.py and ignored. Run from the repository root with numpy, scipy and pillow.
"""
import glob, os, shutil, subprocess, sys
import numpy as np
from PIL import Image
from scipy import ndimage
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from paint_frames import SKIN_FRAMES, D, ROOT, plain_image
from refs import skins

CELL = 512
FRAME = 400   # each plain frame sits in the middle of its cell; the empty border keeps neighbouring birds apart
SHEET = ("Edit the first input image: a 3x3 sheet of nine animation frames of one cartoon plush bird, one frame per cell, read left to right, top to "
         "bottom. %s Use your image generation tool once to paint the SAME 3x3 sheet: keep the grid, keep every cell's pose, position, size, head angle, "
         "body tilt, wing and feet positions, eyes, eyelids, beak and lighting exactly as in the first image, every bird exactly the same size as in its "
         "cell of the first image (never enlarged to fill the cell), now with the bird in every cell wearing the costume %s, seen from that cell's angle; "
         "when the wings move, any sleeve or trim on them moves with them. The costume is identical in all nine cells and worn on the body only: it sits "
         "on the head, face frame, chest and back and follows the round body. The bird's face, eyes, eyelids, beak and feet stay exactly as in the first "
         "image. Each bird stays fully inside its own cell with margin; nothing crosses into another cell. No grid lines, no borders, no ground shadow, "
         "no motion blur, no extra copies, no props, nothing floating around the birds, no leaves, petals, sparkles or glow, fully transparent "
         "background. Costume %s: %s. Palette: %s.")
WITH_REF = ("The second input image shows the same bird from the front wearing a costume.",
            "exactly as designed in the second image (same shapes, panels, colours, trims, size relative to the body)")
NO_REF = ("", "described below, clearly different in colour and shape from the plain bird")


def clean(cell):
    """Keep the bird: the largest blob of solid alpha and what touches it; faint specks and loose bits elsewhere become transparent."""
    px = np.array(cell)
    solid = px[..., 3] > 40
    lab, n = ndimage.label(ndimage.binary_dilation(solid, iterations=3))
    if n:
        keep = lab == 1 + int(np.argmax(ndimage.sum(solid, lab, range(1, n + 1))))
        px[~keep | (px[..., 3] <= 24)] = 0
    return Image.fromarray(px, "RGBA")


def main():
    args = sys.argv[1:]
    if "--workers" in args:
        i = args.index("--workers"); del args[i:i + 2]
    species, slot = args[0], args[1]
    spec = next(s for s in skins() if s["species"] == species and s["slot"] == int(slot))
    ref = "%s/out/ref_%s_%s.png" % (D, species, slot)
    has_ref = os.path.exists(ref)
    out = lambda f: "%s/out/%s_%s_%s_worn.png" % (D, species, slot, f)
    todo = [f for f in (args[2:] or SKIN_FRAMES) if not os.path.exists(out(f))]
    if not todo:
        return
    stem = "%s/out/%s_%s_sheet" % (D, species, slot)
    grid = Image.new("RGBA", (CELL * 3, CELL * 3), (0, 0, 0, 0))
    for i, f in enumerate(SKIN_FRAMES):
        grid.alpha_composite(plain_image(species, f).resize((FRAME, FRAME), Image.LANCZOS), ((i % 3) * CELL + (CELL - FRAME) // 2, (i // 3) * CELL + (CELL - FRAME) // 2))
    grid.save(stem + "_plain.png")
    with open(stem + ".log", "w") as fh:
        try:
            subprocess.run(["codex", "exec", "--skip-git-repo-check", "--dangerously-bypass-approvals-and-sandbox", "-C", ROOT, "-i", stem + "_plain.png"]
                           + (["-i", ref] if has_ref else []) + ["--", SHEET % ((WITH_REF if has_ref else NO_REF) + (spec["name"], spec["text"], spec["palette"]))],
                           stdin=subprocess.DEVNULL, stdout=fh, stderr=subprocess.STDOUT, timeout=1500)
        except subprocess.TimeoutExpired:
            pass
    sid = next((l.split()[2] for l in open(stem + ".log", errors="ignore") if l.startswith("session id:")), "")
    imgs = sorted(glob.glob(os.path.expanduser("~/.codex/generated_images/%s/*.png" % sid)), key=os.path.getmtime)
    if not imgs:
        print("%s_%s FAILED" % (species, slot), flush=True)
        return
    shutil.copy(imgs[-1], stem + ".png")
    sheet = Image.open(stem + ".png").convert("RGBA")
    w, h = sheet.size
    for f in todo:
        i = SKIN_FRAMES.index(f)
        cx, cy, half = (i % 3 + 0.5) * w / 3, (i // 3 + 0.5) * h / 3, FRAME / CELL * w / 6
        cell = sheet.crop((round(cx - half), round(cy - half), round(cx + half), round(cy + half)))
        clean(cell.resize((1024, 1024), Image.LANCZOS)).save(out(f))
    print("%s_%s ok (%d frames)" % (species, slot, len(todo)), flush=True)


if __name__ == "__main__":
    main()
