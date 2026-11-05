"""Reproduce bounded art composites; run from the repository root with Pillow and NumPy."""
from pathlib import Path
import json
import re
import sys
import uuid
import numpy as np
from PIL import Image

ROOT = Path('art/map_seams')
ART = Path('PerchSort/Assets/Resources/Art')

def edges(row):
    rgb = row[:, :3].astype(int)
    sand = (rgb[:, 0] > rgb[:, 1] + 18) & (rgb[:, 1] > rgb[:, 2] + 30) & (rgb[:, 0] > 200)
    left = right = 540
    while sand[left - 1]:
        left -= 1
    while sand[right + 1]:
        right += 1
    return left, right

def smooth(t):
    t = np.clip(t, 0, 1)
    return t * t * (3 - 2 * t)

for lower in map(int, sys.argv[1:]):
    upper = lower + 1
    name = f'boundary_w{upper}_w{lower}'
    original = np.array(Image.open(ROOT / 'raw' / f'{name}_input.png')).astype(float)
    generated = np.array(Image.open(ROOT / 'raw' / f'{name}_generated.png').convert('RGB').resize((1080, 720), Image.Resampling.LANCZOS))
    # Align only the generated patch. The original strip middle is never resampled.
    aligned = generated.copy()
    for y in range(120, 600):
        left, right = edges(generated[y])
        strength = smooth((y - 120) / 160) * smooth((599 - y) / 160)
        phase = np.clip((y - 280) / 159, 0, 1) * 2 * np.pi
        target_left = left * (1 - strength) + (449 + np.rint(np.sin(phase))) * strength
        target_right = right * (1 - strength) + (638 - np.rint(np.sin(phase))) * strength
        sampling = np.interp(np.arange(1080), [0, target_left, target_right, 1079], [0, left, right, 1079])
        for channel in range(3):
            aligned[y, :, channel] = np.rint(np.interp(sampling, np.arange(1080), generated[y, :, channel])).astype(np.uint8)
    yy = np.arange(720)
    weight = (smooth((yy - 120) / 96) * smooth((599 - yy) / 96))[:, None, None]
    panel = np.rint(original * (1 - weight) + aligned * weight).astype(np.uint8)
    Image.fromarray(panel).save(ROOT / 'qa' / f'{name}_panel.png')
    for world, dest_y, patch in [(upper, 2680, panel[120:440]), (lower, 0, panel[280:600])]:
        output = ART / f'map_bg_continuous_w{world}.png'
        source = ART / f'map_bg_w{world}.png'
        pixels = np.array(Image.open(output if output.exists() else source).convert('RGBA'))
        pixels[dest_y:dest_y + 320, :, :3] = patch
        Image.fromarray(pixels).save(output)
        meta = output.with_suffix('.png.meta')
        if not meta.exists():
            meta.write_text(re.sub(r'^guid: .*$', 'guid: ' + uuid.uuid4().hex, source.with_suffix('.png.meta').read_text(), count=1, flags=re.M))
    Image.fromarray(panel).save(Path('docs/work-log/map-seam-art/evidence') / f'{name}.jpg', quality=95, subsampling=0)
    print(name, 'shared overlap RGB identical; original alpha retained')
