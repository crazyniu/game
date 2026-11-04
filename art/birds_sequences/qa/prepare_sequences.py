"""Slice drawn sheets, register exports and build offline animation QA."""
from pathlib import Path
import hashlib
import json
import re
import sys
import uuid

from PIL import Image, ImageDraw, ImageOps

ROOT = Path(__file__).resolve().parents[3]
WORK = ROOT / 'art/birds_sequences'
OUT = ROOT / 'PerchSort/Assets/Resources/Birds'
EVIDENCE = ROOT / 'tasks/evidence/T54G'
SEQUENCES = {'flap': (6, 3, 2), 'hop': (6, 3, 2),
             'turn_left': (4, 2, 2), 'turn_right': (4, 2, 2),
             'cheer': (6, 3, 2), 'preen': (5, 3, 2),
             'peek': (4, 2, 2), 'blink': (3, 3, 1)}


def feet(im):
    points = [(x, y) for y in range(round(im.height * .65), im.height)
              for x in range(im.width)
              if (lambda r, g, b, a: a > 235 and r > 180 and
                  60 < g < 165 and b < 70 and r > 1.6 * g)(*im.getpixel((x, y)))]
    bottom = max(y for x, y in points)
    low = [(x, y) for x, y in points if y >= bottom - im.height * .03]
    return (min(x for x, y in low), min(y for x, y in low),
            max(x for x, y in low) + 1, bottom + 1)


def background(im, color):
    canvas = Image.new('RGBA', im.size, color)
    canvas.alpha_composite(im)
    return canvas.convert('RGB')


species = sys.argv[1]
animations = sys.argv[2:] or list(SEQUENCES)
reference = Image.open(OUT / f'{species}_idle.png').convert('RGBA')
ref_feet = feet(reference)
meta = (OUT / 'sunny_idle.png.meta').read_text()
records = []
inventory = WORK / 'qa' / f'{species}_inventory.json'
if inventory.exists():
    records = [r for r in json.loads(inventory.read_text()) if r['animation'] not in animations]

for animation in animations:
    count, columns, rows = SEQUENCES[animation]
    source = WORK / 'raw' / f'{species}_{animation}.png'
    sheet = Image.open(source).convert('RGBA')
    cw, ch = sheet.width // columns, sheet.height // rows
    cells = [sheet.crop(((i % columns) * cw, (i // columns) * ch,
                         (i % columns + 1) * cw, (i // columns + 1) * ch))
             for i in range(count)]
    anchor = feet(cells[0])
    scale = (ref_feet[2] - ref_feet[0]) / (anchor[2] - anchor[0])
    frames = []
    for index, cell in enumerate(cells):
        foot = feet(cell)
        dx = round(160 - (foot[0] + foot[2]) * .5 * scale)
        # Airborne hop drawings retain the displacement drawn relative to the ground.
        base = anchor[3] if animation == 'hop' and index in (1, 2, 3, 4) else foot[3]
        dy = round(300 - base * scale)
        sprite = Image.new('RGBA', (320, 320))
        sprite.alpha_composite(cell.resize((round(cw * scale), round(ch * scale)),
                                         Image.Resampling.LANCZOS), (dx, dy))
        name = f'{species}_{animation}_{index + 1:02}'
        target = OUT / f'{name}.png'
        sprite.save(target)
        metadata = target.with_suffix('.png.meta')
        if not metadata.exists():
            # Preserve importer settings, replace only asset/subsprite identities.
            fresh = re.sub(r'^guid: .*$', 'guid: ' + uuid.uuid4().hex, meta, flags=re.M)
            fresh = fresh.replace('sunny_idle', name)
            fresh = re.sub(r'spriteID: [0-9a-f]+',
                           lambda match: 'spriteID: ' + uuid.uuid4().hex, fresh)
            metadata.write_text('\n'.join(line.rstrip() for line in fresh.splitlines()) + '\n')
        visible = sprite.getchannel('A').point(lambda a: 255 if a > 12 else 0).getbbox()
        records.append({'file': str(target.relative_to(ROOT)), 'animation': animation,
                        'source': str(source.relative_to(ROOT)), 'cell': index + 1,
                        'size': list(sprite.size), 'mode': sprite.mode,
                        'alpha_extrema': list(sprite.getchannel('A').getextrema()),
                        'visible_bounds': visible, 'scale': scale, 'translation': [dx, dy],
                        'source_feet': foot, 'ground_baseline': 300,
                        'drawn_airborne': animation == 'hop' and index in (1, 2, 3, 4),
                        'sha256': hashlib.sha256(target.read_bytes()).hexdigest()})
        frames.append(sprite)

    contact = Image.new('RGB', (160 * (count + 1), 460), '#f7f3eb')
    draw = ImageDraw.Draw(contact)
    for index, (name, frame) in enumerate([('approved idle', reference)] +
                                         [(f'{animation} {i + 1:02}', f) for i, f in enumerate(frames)]):
        thumb = frame.resize((160, 160), Image.Resampling.LANCZOS)
        contact.paste(background(thumb, '#f7f3eb'), (index * 160, 20))
        contact.paste(background(thumb, '#17202b'), (index * 160, 200))
        gray = ImageOps.grayscale(background(frame.resize((64, 64), Image.Resampling.LANCZOS), '#eeeeee'))
        contact.paste(gray, (index * 160 + 48, 386))
        draw.text((index * 160 + 6, 4), name, fill='#302e2c')
        draw.text((index * 160 + 6, 366), '64 px grayscale', fill='#302e2c')
    filename = f'{species}_{animation}_contact.jpg'
    contact.save(WORK / 'qa' / filename, quality=91)
    contact.save(EVIDENCE / filename, quality=88)
    order = list(range(count))
    if animation in ('turn_left', 'turn_right', 'blink', 'peek', 'preen'):
        order += list(range(count - 2, 0, -1))
    preview = []
    for index in order:
        panel = Image.new('RGB', (640, 346), '#f7f3eb')
        panel.paste(background(frames[index], '#f7f3eb'), (0, 0))
        panel.paste(background(frames[index], '#17202b'), (320, 0))
        ImageDraw.Draw(panel).text((12, 328), f'{species} {animation} {index + 1:02} - offline 10 fps', fill='#302e2c')
        preview.append(panel)
    preview[0].save(WORK / 'qa' / f'{species}_{animation}_preview.apng', save_all=True,
                    append_images=preview[1:], duration=100, loop=0, format='PNG')

inventory.write_text(json.dumps(records, indent=2) + '\n')
print(json.dumps({'species': species, 'frames': len(records),
                  'clipped': [r['file'] for r in records if r['visible_bounds'] and
                              (r['visible_bounds'][0] == 0 or r['visible_bounds'][1] == 0 or
                               r['visible_bounds'][2] == 320 or r['visible_bounds'][3] == 320)]}))
