"""Style preview images for the user, composed from the real kit (scene painting, cut icons and letters, birds)
and the shapes of docs/design/ui-system.md. Not used by the game.

python3 tools/kit/style_preview.py   -> build/style/board.png, win.png, map.png
"""
import json
import os

from PIL import Image, ImageDraw, ImageFilter

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
P = os.path.join(ROOT, "PerchSort")
OUT = os.path.join(ROOT, "build", "style")
W, H = 1080, 1920

C = {
    "cream": ((255, 247, 232), (233, 214, 176)),
    "green": ((108, 194, 74), (76, 150, 49)),
    "orange": ((255, 165, 60), (218, 122, 20)),
    "blue": ((93, 185, 238), (60, 147, 201)),
    "red": ((239, 85, 72), (194, 58, 47)),
}
INK = (90, 58, 30)
GOLD = (255, 200, 61)
LETTERS = json.load(open(os.path.join(P, "art", "letters", "letters.json")))


def img(rel):
    return Image.open(os.path.join(P, rel)).convert("RGBA")


def tint(im, color, alpha=1.0):
    r, g, b = color
    out = Image.new("RGBA", im.size, (r, g, b, 0))
    a = im.split()[3].point(lambda v: int(v * alpha))
    out.putalpha(a)
    return out


def shadow(base, box, radius):
    sh = Image.new("RGBA", base.size, (0, 0, 0, 0))
    d = ImageDraw.Draw(sh)
    x0, y0, x1, y1 = box
    d.rounded_rectangle((x0, y0 + 8, x1, y1 + 8), radius, fill=(0, 0, 0, 46))
    base.alpha_composite(sh.filter(ImageFilter.GaussianBlur(12)))


def shape(base, box, radius, kind, edge):
    """Rounded flat fill with a darker bottom edge and one soft shadow."""
    fill, rim = C[kind]
    x0, y0, x1, y1 = box
    shadow(base, box, radius)
    d = ImageDraw.Draw(base)
    d.rounded_rectangle((x0, y0, x1, y1), radius, fill=rim)
    d.rounded_rectangle((x0, y0, x1, y1 - edge), radius, fill=fill)


def circle(base, cx, cy, size, kind, edge=10):
    shape(base, (cx - size / 2, cy - size / 2, cx + size / 2, cy + size / 2), size / 2, kind, edge)


def paste_fit(base, im, box, align="center"):
    x0, y0, x1, y1 = box
    im = im.crop(im.getbbox())
    k = min((x1 - x0) / im.width, (y1 - y0) / im.height)
    im = im.resize((max(1, int(im.width * k)), max(1, int(im.height * k))), Image.LANCZOS)
    x = x0 + ((x1 - x0) - im.width) / 2 if align == "center" else x0
    base.alpha_composite(im, (int(x), int(y0 + ((y1 - y0) - im.height) / 2)))


def icon(base, name, cx, cy, size, color=(255, 255, 255)):
    im = tint(img("art/icons/icon_%s.png" % name), color)
    paste_fit(base, im, (cx - size / 2, cy - size / 2, cx + size / 2, cy + size / 2))


def button(base, cx, cy, icon_name, kind="orange", size=128, badge=None):
    circle(base, cx, cy, size, kind)
    color = INK if kind == "cream" else (255, 255, 255)
    icon(base, icon_name, cx, cy - 5, size * 0.56, color)
    if badge is not None:
        bx, by = cx + size * 0.5 * 0.7071, cy - size * 0.5 * 0.7071
        circle(base, bx, by, 56, "red", 5)
        text(base, str(badge), bx, by - 3, 30, (255, 255, 255), "center")


def word_image(t, cap, color):
    k = cap / LETTERS["cap"]
    parts, x = [], 0.0
    for ch in t:
        if ch == " ":
            x += LETTERS["space"] * k
            continue
        g = LETTERS["glyphs"][str(ord(ch))]
        im = img("art/letters/letter_%04x.png" % ord(ch))
        im = im.resize((max(1, round(g["w"] * k)), max(1, round(g["h"] * k))), Image.LANCZOS)
        parts.append((tint(im, color), x, g["dy"] * cap))
        x += im.width + 0.07 * cap
    out = Image.new("RGBA", (int(x) + 2, int(cap * 1.5)), (0, 0, 0, 0))
    for im, px, dy in parts:
        out.alpha_composite(im, (int(px), int(cap * 1.15 + dy)))
    return out, cap * 1.15   # image, baseline y inside it


def text(base, t, x, cy, cap, color, align="left"):
    """Text whose cap height is centred on cy."""
    im, baseline = word_image(t, cap, color)
    top = cy + cap / 2 - baseline
    if align == "center":
        x -= im.width / 2
    elif align == "right":
        x -= im.width
    base.alpha_composite(im, (int(x), int(top)))
    return im.width


def counter(base, x, y, w, icon_im, number, icon_color=None, extra=0):
    """Pill counter; w=None sizes it to the number. extra adds room on the right. Returns the right edge."""
    if w is None:
        w = 100 + word_image(number, 44, INK)[0].width + 32 + extra
    shape(base, (x, y, x + w, y + 96), 48, "cream", 8)
    box = (x + 16, y + 8, x + 88, y + 80)
    if icon_color:
        paste_fit(base, tint(icon_im, icon_color), box)
    else:
        paste_fit(base, icon_im, box)
    text(base, number, x + 100, y + 44, 44, INK)
    return x + w


def bird(base, species, cx, feet_y, scale=0.88, pose="idle"):
    im = img("birds/%s_%s.png" % (species, pose))
    k = 170.0 / 320.0 * 1.25 * scale
    im = im.resize((int(im.width * k), int(im.height * k)), Image.LANCZOS)
    base.alpha_composite(im, (int(cx - im.width / 2), int(feet_y - 300 * k)))


def scene():
    base = img("world/scene/board_w1_bg.png")
    slots = json.load(open(os.path.join(P, "world", "scene", "board_w1_slots.json")))["slots"]
    for s in slots:
        base.alpha_composite(img("world/scene/%s.png" % s["name"]), tuple(s["at"]))
    return base, {(s["side"], s["row"]): s for s in slots}


def board():
    base, slots = scene()
    flock = {("L", 0): ["sunny", "sky", "hoot"], ("R", 0): ["sky", "sunny", "sky", "hoot"],
             ("L", 1): ["hoot", "sunny", "sky"], ("R", 1): ["sunny", "hoot"],
             ("L", 2): ["sky", "sky", "sunny", "hoot"], ("R", 2): ["sunny"],
             ("L", 3): ["hoot", "sunny"], ("R", 3): []}
    for (side, row), birds in flock.items():
        s = slots[(side, row)]
        for i, sp in enumerate(birds):
            d = 124 + 126 * i * 0.88
            bird(base, sp, d if side == "L" else W - d, s["perch"] + 6)
    # house and goals
    rope = img("art/house_rope.png")
    paste_fit(base, rope, (528, -10, 552, 120))
    paste_fit(base, img("art/goal_house_single.png"), (420, 90, 660, 330))
    for i, sp in enumerate(["sunny", "sky", "hoot"]):
        cx = 540 + (i - 1) * 136
        circle(base, cx, 410, 112, "cream", 8)
        paste_fit(base, img("birds/%s_face.png" % sp), (cx - 42, 410 - 48, cx + 42, 410 + 36))
    # top bar
    right = counter(base, 48, 56, None, img("art/icons/icon_footprint.png"), "12", INK, extra=150)
    for i in range(3):
        icon(base, "star", right - 156 + i * 46, 102, 44, GOLD if i < 2 else (233, 214, 176))
    counter(base, 48, 176, None, img("art/icons/icon_sun.png"), "4", (255, 176, 40))
    button(base, 968, 104, "gear", "cream")
    # tools
    for i, (name, kind, badge) in enumerate([("undo", "cream", None), ("bulb", "orange", 3), ("sapling", "orange", 2)]):
        button(base, 540 + (i - 1) * 176, 1792, name, kind, badge=badge)
    return base


def win():
    base = board()
    dim = Image.new("RGBA", base.size, (30, 42, 26, 140))
    base.alpha_composite(dim)
    x0, y0, x1, y1 = 108, 560, 972, 1220
    shape(base, (x0, y0, x1, y1), 56, "cream", 12)
    text(base, "Well done!", 540, y0 + 92, 64, INK, "center")
    for i, (dx, size, dy) in enumerate([(-180, 150, 40), (0, 190, 0), (180, 150, 40)]):
        icon(base, "star", 540 + dx, y0 + 290 + dy, size, GOLD)
    icon(base, "footprint", 470, y0 + 470, 56, INK)
    text(base, "14", 512, y0 + 470, 52, INK)
    shape(base, (380, y0 + 530, 700, y0 + 626), 48, "blue", 8)
    paste_fit(base, img("art/coin.png"), (396, y0 + 538, 468, y0 + 610))
    text(base, "+30", 486, y0 + 574, 48, (255, 255, 255))
    by = y1 + 48 + 80
    button(base, 540 - 200, by, "retry", "orange")
    button(base, 540, by, "play", "green", 160)
    button(base, 540 + 200, by, "home", "orange")
    return base


def map_screen():
    m = img("art/map_world_w1.png")
    top = 1080
    base = m.crop((0, top, W, top + H))
    nodes = [[514, 2700], [310, 2440], [744, 2180], [660, 1920], [252, 1660], [672, 1400], [640, 1140]]
    stars = [3, 3, 2, 3]
    for i, (x, y) in enumerate(nodes):
        y -= top
        level = i + 1
        if level <= 4:
            circle(base, x, y, 132, "cream")
            text(base, str(level), x, y - 6, 52, INK, "center")
            for k in range(3):
                icon(base, "star", x - 48 + 48 * k, y + 88, 46, GOLD if k < stars[i] else (233, 214, 176))
        elif level == 5:
            circle(base, x, y, 168, "green")
            text(base, str(level), x, y - 6, 64, (255, 255, 255), "center")
            bird(base, "sunny", x, y - 70, 0.8)
        else:
            circle(base, x, y, 132, "cream")
            icon(base, "lock", x, y - 6, 56, (200, 180, 150))
    coin_w = 100 + word_image("1240", 44, INK)[0].width + 32
    counter(base, W - 48 - coin_w, 56, None, img("art/coin.png"), "1240")
    title_w = word_image("Green Forest", 40, INK)[0].width + 64
    shape(base, (48, 56, 48 + title_w, 152), 48, "cream", 8)
    text(base, "Green Forest", 48 + title_w / 2, 104, 40, INK, "center")
    for i, (name, kind) in enumerate([("shop", "orange"), ("aviary", "orange"), ("gear", "cream")]):
        button(base, 540 + (i - 1) * 200, 1792, name, kind)
    return base


def main():
    os.makedirs(OUT, exist_ok=True)
    board().save(os.path.join(OUT, "board.png"))
    win().save(os.path.join(OUT, "win.png"))
    map_screen().save(os.path.join(OUT, "map.png"))
    print("saved to", OUT)


main()
