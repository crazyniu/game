"""Import level-map paintings: python3 tools/map/import_maps.py <world> [<world> ...]

tools/map/map_world_wN_raw.png (1024x1536) -> PerchSort/art/map_world_wN.png (1080x1620, RGBA). The top 120 rows fade in
(smoothstep alpha), so each painting blends over the world above it on the scrolling map.
"""
import os
import sys

import numpy as np
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
FADE = 120
for w in sys.argv[1:]:
    im = Image.open(os.path.join(ROOT, "tools", "map", "map_world_w%s_raw.png" % w)).convert("RGB").resize((1080, 1620), Image.LANCZOS)
    a = np.array(im.convert("RGBA"))
    t = np.clip(np.arange(1620) / FADE, 0.0, 1.0)
    a[..., 3] = np.tile(((t * t * (3.0 - 2.0 * t)) * 255.0).astype(np.uint8)[:, None], (1, 1080))
    Image.fromarray(a, "RGBA").save(os.path.join(ROOT, "PerchSort", "art", "map_world_w%s.png" % w))
    print("map world", w)
