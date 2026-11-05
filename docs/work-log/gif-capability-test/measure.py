"""Offline measurement and evidence layout only; never synthesizes animation."""
from pathlib import Path
import hashlib
import json
import struct

import numpy as np
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[3]
RAW = ROOT / 'art/gif_tests'
EVIDENCE = Path(__file__).parent / 'evidence'
BIRDS = ROOT / 'PerchSort/Assets/Resources/Birds'
FONT = ImageFont.truetype('/System/Library/Fonts/Supplemental/Arial.ttf', 18)
TITLE = ImageFont.truetype('/System/Library/Fonts/Supplemental/Arial.ttf', 23)
source = RAW / 'sunny_flight_output.png'
generated = Image.open(source).convert('RGBA')
idle = Image.open(BIRDS / 'sunny_idle.png').convert('RGBA')
hat = Image.open(BIRDS / 'sunny_outfit_1.png').convert('RGBA')
flaps = [Image.open(BIRDS / f'sunny_flap_{i:02}.png').convert('RGBA') for i in range(1, 7)]
leaf = Image.open(ROOT / 'art/approved_ui/ui_win.png').convert('RGBA').crop((0, 450, 320, 770))


def stats(path):
    im = Image.open(path)
    a = np.array(im.convert('RGBA'))
    alpha = a[:, :, 3]
    return dict(path=str(path.relative_to(ROOT)), format=im.format, mode=im.mode,
                resolution=list(im.size), frame_count=im.n_frames,
                duration_ms=im.info.get('duration'), fps=None,
                file_bytes=path.stat().st_size,
                sha256=hashlib.sha256(path.read_bytes()).hexdigest(),
                distinct_visible_rgb=int(len(np.unique(a[:, :, :3][alpha > 0], axis=0))),
                distinct_visible_rgba=int(len(np.unique(a[alpha > 0], axis=0))),
                alpha_levels=np.unique(alpha).tolist(),
                visible_alpha_percentiles=np.percentile(alpha[alpha > 0], [0, 10, 50, 90, 99, 100]).tolist(),
                transparent_pixels=int((alpha == 0).sum()),
                opaque_pixels=int((alpha == 255).sum()),
                partial_alpha_pixels=int(((alpha > 0) & (alpha < 255)).sum()))


def composite(im, background=(30, 33, 40)):
    bg = (background.resize(im.size) if isinstance(background, Image.Image)
          else Image.new('RGBA', im.size, (*background, 255)))
    return Image.alpha_composite(bg, im).convert('RGB')


def sheet(size, title):
    im = Image.new('RGB', size, (239, 239, 235))
    ImageDraw.Draw(im).text((20, 15), title, font=TITLE, fill='black')
    return im


def label(im, xy, text):
    ImageDraw.Draw(im).text(xy, text, font=FONT, fill='black')


def save(im, name):
    im.save(EVIDENCE / name, quality=98, subsampling=0)


# Extract every actual temporal frame. There is exactly one in the returned PNG.
with Image.open(source) as im:
    for index in range(im.n_frames):
        im.seek(index)
        im.convert('RGBA').save(RAW / f'sunny_flight_frame_{index + 1:02}.png')

chunks = []
data = source.read_bytes()
offset = 8
while offset < len(data):
    length = struct.unpack('>I', data[offset:offset + 4])[0]
    chunks.append(data[offset + 4:offset + 8].decode('ascii'))
    offset += 12 + length
metrics = dict(generated=stats(source), png_signature_hex=data[:8].hex(),
               png_chunks=chunks,
               references=[stats(BIRDS / name) for name in
                           ['sunny_idle.png', 'sunny_outfit_1.png'] +
                           [f'sunny_flap_{i:02}.png' for i in range(1, 7)]],
               colour_definition='Distinct RGB tuples only where alpha > 0; no quantization.',
               zoom_definition='100% is one source pixel per sheet pixel; 400% is nearest-neighbour x4.',
               leaf_background_crop=[0, 450, 320, 770])
(RAW / 'measurements.json').write_text(json.dumps(metrics, indent=2) + '\n')

im = sheet((1534, 2100), 'ACTUAL OUTPUT: one static PNG, not GIF | all images below at native 100%')
im.paste(composite(generated), (20, 55))
label(im, (1290, 70), '1254 x 1254\n1 frame\nNo FPS metadata\nFour simultaneous\nspatial poses\nNo animation')
for i, frame in enumerate(flaps):
    x, y = 20 + (i % 3) * 345, 1360 + (i // 3) * 365
    label(im, (x, y), f'Existing sunny_flap_{i + 1:02} | 320 x 320')
    im.paste(composite(frame), (x, y + 28))
label(im, (1080, 1360), 'Existing sunny_outfit_1 | 320 x 320')
im.paste(composite(hat), (1080, 1388))
label(im, (1080, 1730), 'Existing sunny_idle | 320 x 320')
im.paste(composite(idle), (1080, 1758))
save(im, 'native_comparison.jpg')

# Explicit raw crop coordinates; no recolouring, dehalo or generated retouching.
crop_specs = [
    ('Generated face', generated, (550, 795, 646, 891)),
    ('Existing flap face', flaps[0], (120, 135, 216, 231)),
    ('Existing hat face', hat, (120, 130, 216, 226)),
    ('Generated feather edge', generated, (445, 850, 541, 946)),
    ('Existing flap feather edge', flaps[0], (28, 175, 124, 271)),
    ('Existing hat / feather edge', hat, (60, 100, 156, 196)),
]
im = sheet((1260, 1140), 'Colour / texture review | crop at 100% above, same crop at 400% below')
for index, (name, frame, box) in enumerate(crop_specs):
    x, y = 20 + (index % 3) * 415, 65 + (index // 3) * 535
    crop = composite(frame.crop(box))
    label(im, (x, y), name + f' | {box[0]},{box[1]}')
    im.paste(crop, (x, y + 28))
    im.paste(crop.resize((384, 384), Image.Resampling.NEAREST), (x, y + 132))
save(im, 'texture_comparison.jpg')

im = sheet((1260, 1450), 'Alpha composite review | raw RGBA over dark, light and approved green leaves')
rows = [('Generated landing crop', generated.crop((430, 680, 830, 1080)), (8, 160, 104, 256)),
        ('Existing flap', flaps[0], (25, 145, 121, 241)),
        ('Existing straw hat', hat, (60, 95, 156, 191))]
for row, (name, frame, box) in enumerate(rows):
    for col, (bgname, background) in enumerate([('dark', (30, 33, 40)), ('light', (245, 245, 240)), ('leaves', leaf)]):
        x, y = 20 + col * 415, 65 + row * 455
        comp = composite(frame, background)
        label(im, (x, y), name + ' / ' + bgname)
        # Context is labelled reduced preview; measured detail has exact zoom.
        preview = comp.resize((160, 160), Image.Resampling.LANCZOS)
        im.paste(preview, (x, y + 30))
        label(im, (x + 170, y + 30), 'Context preview / crop 100%')
        im.paste(comp.crop(box), (x + 170, y + 60))
        label(im, (x + 170, y + 165), 'Detail below: 400%')
        detail = comp.crop(box).resize((384, 384), Image.Resampling.NEAREST)
        im.paste(detail.crop((0, 0, 384, 240)), (x, y + 200))
save(im, 'alpha_comparison.jpg')

# Normalize only for static identity comparison, never claim these are temporal frames.
poses = [('Upper-left clipped pose', (0, 0, 175, 315)),
         ('Upper complete pose', (45, 155, 400, 515)),
         ('Middle complete pose', (240, 385, 635, 750)),
         ('Landing complete pose', (440, 680, 825, 1040))]
reference = idle.crop((57, 68, 265, 305)).resize((320, 320), Image.Resampling.LANCZOS)
im = sheet((1420, 1200), 'Static identity review ONLY | spatial pose crops normalized to 320 x 320')
for index, (name, box) in enumerate(poses):
    x = 20 + index * 350
    pose = generated.crop(box).resize((320, 320), Image.Resampling.LANCZOS)
    label(im, (x, 60), name)
    im.paste(composite(pose), (x, 90))
    label(im, (x, 430), '50% output + 50% idle reference')
    im.paste(Image.blend(composite(pose), composite(reference), .5), (x, 460))
    label(im, (x, 800), 'Idle reference (normalized)')
    im.paste(composite(reference), (x, 830))
save(im, 'identity_overlay.jpg')

# Actual sole frame over a canvas-aligned reference, at exactly 50% each.
reference_canvas = Image.new('RGBA', generated.size)
reference_canvas.alpha_composite(reference.resize((385, 360)), (440, 680))
im = sheet((1294, 1340), 'Sole actual frame: 50% output + 50% reference | 100% canvas pixels')
im.paste(Image.blend(composite(generated), composite(reference_canvas), .5), (20, 60))
save(im, 'frame_overlay.jpg')
print(json.dumps({k: v for k, v in metrics['generated'].items() if k != 'alpha_levels'}, indent=2))
print('Alpha levels:', len(metrics['generated']['alpha_levels']))
print('PNG chunks:', chunks)
