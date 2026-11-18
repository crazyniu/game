#!/usr/bin/env python3
"""preview.py <species> <name> <propeller.png> <out-dir>: render the whole-bird skin as the game draws it.

The whole painted bird (out/whole) plus a propeller on the painted cap button. The propeller is a top-down painted picture that
really rotates in a plane squashed to the view angle (no mirroring, no still) and follows each frame's button. Writes reel.gif and a sheet.
"""
import json, math, os, sys
from PIL import Image, ImageDraw

HERE = os.path.dirname(os.path.abspath(__file__))
BIRDS = os.path.abspath(os.path.join(HERE, "../../../PerchSort/assets/resources/birds"))
BG = (205, 215, 195)
SQUASH = 0.30


def crop(img):
    return img.crop(img.getbbox())


def propeller(prop, size, phase, tilt):
    """The propeller picture rotated by `phase` in its own plane, viewed at an angle, then tilted with the cap."""
    s = prop.resize((size, size), Image.LANCZOS)
    r = s.rotate(math.degrees(phase), resample=Image.BICUBIC, expand=False)
    v = r.resize((size, max(2, round(size * SQUASH))), Image.LANCZOS)
    return v.rotate(tilt, resample=Image.BICUBIC, expand=True)


def render(species, name, table, prop, frame, t, ghost=True):
    base = Image.new("RGBA", (320, 320), BG + (255,))
    part = Image.open("%s/out/whole/%s_%s_%s.png" % (HERE, species, frame, name)).convert("RGBA")
    base.alpha_composite(part)
    info = table[frame]
    if info["hub"]:
        width = 148
        size = round(width * 0.78)
        tilt = -max(-25, min(25, info.get("up") or 0))
        hx, hy = info["hub"][0], info["hub"][1] - 5
        spin = t * 2 * math.pi * 3.2                       # 3.2 turns per second
        layers = [(spin, 255)]
        if ghost:
            layers = [(spin - 0.9, 70), (spin - 0.45, 120), (spin, 255)]
        for ph, alpha in layers:
            img = propeller(prop, size, ph, tilt)
            if alpha < 255:
                img.putalpha(img.getchannel("A").point(lambda v, a=alpha: v * a // 255))
            base.alpha_composite(img, (round(hx - img.width / 2), round(hy - img.height / 2)))
    return base


def main(species, name, prop_path, out):
    table = json.load(open("%s/out/%s/%s_%s.json" % (HERE, "whole", species, name)))
    prop = crop(Image.open(prop_path).convert("RGBA"))
    side = max(prop.size)
    sq = Image.new("RGBA", (side, side)); sq.alpha_composite(prop, ((side - prop.width) // 2, (side - prop.height) // 2))
    prop = sq
    os.makedirs(out, exist_ok=True)
    # reel: a short, game-like routine; each painted frame holds 3 ticks (25 ticks per second)
    seq = (["front"] * 8 + ["front_blink_01", "front_blink_02", "front_blink_01"] + ["front"] * 6
           + ["turn_left_01", "turn_left_02", "turn_left_03", "turn_left_04", "turn_left_04", "turn_left_03", "turn_left_02", "turn_left_01"]
           + ["front"] * 4
           + ["turn_right_01", "turn_right_02", "turn_right_03", "turn_right_04", "turn_right_04", "turn_right_03", "turn_right_02", "turn_right_01"]
           + ["tilt_left", "tilt_left", "front", "tilt_right", "tilt_right", "front", "look_up", "look_up", "front", "look_down", "look_down", "front"]
           + ["hop_01", "hop_02", "hop_03", "hop_04", "hop_05", "hop_06"] * 2
           + ["flap_01", "flap_02", "flap_03", "flap_04", "flap_05", "flap_06"] * 2
           + ["cheer_01", "cheer_02", "cheer_03", "cheer_04", "cheer_05", "cheer_06"] * 2
           + ["preen_01", "preen_02", "preen_03", "preen_04", "preen_05"] + ["front"] * 6
           + ["flight_takeoff_01", "flight_takeoff_02", "flight_takeoff_03"] + ["flight_flap_0%d" % i for i in range(1, 7)] * 2
           + ["flight_land_01", "flight_land_02", "flight_land_03", "front"] * 1)
    frames = []
    tick = 0
    for f in seq:
        for _ in range(3):
            frames.append(render(species, name, table, prop, f, tick / 25.0).convert("RGB"))
            tick += 1
    frames[0].save(os.path.join(out, "%s_%s_reel.gif" % (species, name)), save_all=True, append_images=frames[1:], duration=40, loop=0, optimize=False)
    print("reel", len(frames), "ticks")
    # still sheet: hard frames at three spin phases
    hard = ["front", "turn_left_03", "turn_right_04", "look_down", "tilt_right", "hop_02", "preen_03", "flight_takeoff_l_01", "flight_land_l_02", "cheer_03"]
    sheet = Image.new("RGB", (320 * len(hard), 320 * 2), BG)
    for i, f in enumerate(hard):
        sheet.paste(render(species, name, table, prop, f, 0.00).convert("RGB"), (i * 320, 0))
        sheet.paste(render(species, name, table, prop, f, 0.07).convert("RGB"), (i * 320, 320))
    sheet.save(os.path.join(out, "%s_%s_sheet.png" % (species, name)))


if __name__ == "__main__":
    main(*sys.argv[1:5])
