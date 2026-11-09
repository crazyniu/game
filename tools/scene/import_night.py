"""Import the night paintings of the worlds as full-screen backgrounds.

python3 tools/scene/import_night.py <world> [<world> ...]

tools/scene/night/board_wN_night.png (1024x1536 painting, screen = centred 9:16 area) ->
PerchSort/world/scene/board_wN_night_bg.png (1080x1920), cropped and scaled exactly like the day background.
"""
import os
import sys

from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
for w in map(int, sys.argv[1:]):
    im = Image.open(os.path.join(ROOT, "tools", "scene", "night", "board_w%d_night.png" % w)).convert("RGB")
    width = int(im.height * 9 / 16)
    ox = (im.width - width) // 2
    im.crop((ox, 0, ox + width, im.height)).resize((1080, 1920), Image.LANCZOS).save(
        os.path.join(ROOT, "PerchSort", "world", "scene", "board_w%d_night_bg.png" % w))
    print("night world", w)
