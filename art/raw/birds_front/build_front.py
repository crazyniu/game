from pathlib import Path
import json
from PIL import Image, ImageDraw, ImageFilter, ImageChops

ROOT = Path(__file__).parent
STATES = ('', '_blink', '_sleep', '_fly')
# Source measurements: folded body bounds, soles, and eye-only patches.
CONFIG = {
    'bird_01': dict(scale=.70, feet_x=(400, 880), eye_boxes=[(477, 437, 584, 538), (699, 437, 804, 538)]),
    'bird_02': dict(scale=760/949, feet_x=(400, 880), eye_boxes=[(443, 401, 565, 516), (690, 400, 807, 516)]),
    'bird_03': dict(scale=760/1105, feet_x=(400, 880), eye_boxes=[(467, 453, 585, 571), (695, 454, 806, 571)]),
    'bird_04': dict(scale=.70, feet_x=(420, 840), eye_boxes=[(459, 259, 549, 374), (698, 259, 793, 374)]),
    'bird_05': dict(scale=735/908, feet_x=(400, 880), eye_boxes=[(477, 392, 582, 500), (672, 392, 777, 500)], head_top=250),
    'bird_06': dict(scale=760/1135, feet_x=(450, 810), eye_boxes=[(545, 232, 647, 330), (720, 236, 802, 330)]),
    'bird_07': dict(scale=760/1071, feet_x=(400, 880), eye_boxes=[(431, 346, 579, 460), (687, 346, 830, 460)]),
    'bird_08': dict(scale=760/1189, feet_x=(400, 880), eye_boxes=[(397, 226, 559, 394), (710, 226, 872, 394)]),
    'bird_09': dict(scale=760/953, feet_x=(400, 880), eye_boxes=[(449, 351, 565, 470), (690, 351, 807, 470)]),
    'bird_10': dict(scale=760/1073, feet_x=(400, 880), eye_boxes=[(430, 343, 570, 465), (698, 343, 833, 465)]),
}

def load(name):
    im = Image.open(ROOT / 'source' / (name + '.png')).convert('RGBA')
    im.putalpha(im.getchannel('A').point(lambda x: 0 if x < 8 else x))
    return im

def resized(im, size):
    return im.convert('RGBa').resize(size, Image.Resampling.LANCZOS).convert('RGBA')

def export(im, scale, xy):
    scaled = resized(im, (round(im.width * scale), round(im.height * scale)))
    out = Image.new('RGBA', (1024, 1024))
    out.alpha_composite(scaled, xy)
    out.putalpha(out.getchannel('A').point(lambda x: 0 if x < 8 else x))
    return out

def soles(im, x_range):
    a = im.getchannel('A').point(lambda x: 255 if x > 7 else 0)
    return a.crop((x_range[0], int(im.height * .7), x_range[1], im.height)).getbbox()[3] + int(im.height * .7) - 1

def scale_about_feet(im, factor):
    size = round(1024 * factor)
    out = Image.new('RGBA', (1024, 1024))
    out.alpha_composite(resized(im, (size, size)), (round((1024-size)/2), round(870-870*size/1024)))
    out.putalpha(out.getchannel('A').point(lambda x: 0 if x < 8 else x))
    return out

manifest = {'canvas': [1024, 1024], 'coordinates': 'top-left; bbox right/bottom exclusive', 'sample_only': False, 'assets': {}}
manifest['acceptedOverrides'] = [
    'Coordinator approved front samples bird_01/bird_04 and retains their visual proportions.',
    'Species width differences accepted in place of strict 40-55%; visual distinction and no row overlap take priority.',
    'bird_05 all states reduced uniformly to <=62% total visible width; shorter height accepted.',
    'bird_06 standing/blink/sleep enlarged 8%; flight retains approximately980px span to fit canvas, center aligned to enlarged standing.',
    'Flying footAnchorPixels is the common virtual anchor; feet are tucked and do not contact y870.',
]
for name, cfg in CONFIG.items():
    base = load(name)
    scale = cfg['scale']
    foot = soles(base, cfg['feet_x'])
    bounds = base.getchannel('A').getbbox()
    cx = (bounds[0] + bounds[2]) / 2
    xy = (round(512 - cx * scale), round(870 - foot * scale))
    center_y = (bounds[1] + bounds[3]) / 2 * scale + xy[1]
    base_out = export(base, scale, xy)
    for state in STATES:
        source = load(name + state)
        if state == '_blink':
            mask = Image.new('L', base.size)
            d = ImageDraw.Draw(mask)
            for box in cfg['eye_boxes']:
                d.rounded_rectangle(box, radius=14, fill=255)
            mask = mask.filter(ImageFilter.GaussianBlur(3))
            source = Image.composite(source, base, mask)
            out = export(source, scale, xy)
        elif state == '_sleep':
            b = source.getchannel('A').getbbox()
            foot = soles(source, cfg['feet_x'])
            out = export(source, scale, (round(512 - (b[0] + b[2]) / 2 * scale), round(870 - foot * scale)))
        elif state == '_fly':
            b = source.getchannel('A').getbbox()
            fs = 980 / (b[2] - b[0])
            out = export(source, fs, (round(512 - (b[0] + b[2]) / 2 * fs), round(center_y - (b[1] + b[3]) / 2 * fs)))
        else:
            out = base_out
        if name == 'bird_05':
            out = scale_about_feet(out, 634/980)
        elif name == 'bird_06' and state != '_fly':
            out = scale_about_feet(out, 1.08)
        elif name == 'bird_06' and state == '_fly':
            aligned = Image.new('RGBA', (1024, 1024))
            aligned.alpha_composite(out, (0, round((center_y-870)*.08)))
            out = aligned
        if state != '_fly':
            dy = 870 - soles(out, (350, 680))
            aligned = Image.new('RGBA', (1024, 1024))
            aligned.alpha_composite(out, (0, dy))
            out = aligned
            if state == '':
                base_out = out
        out.save(ROOT / (name + state + '.png'))
        entry = {'file': name + state + '.png', 'size': [1024, 1024], 'mode': 'RGBA', 'footAnchorPixels': [512, 870], 'visibleBounds': out.getchannel('A').getbbox(), 'recommendedDisplayWidth': 140, 'displayWidthMeaning': 'full 1024px canvas width', 'airborne': state == '_fly', 'unityPivot': [0.5, 1-870/1024]}
        if state == '':
            factor = 634/980 if name == 'bird_05' else 1.08 if name == 'bird_06' else 1
            entry['standingHeightWithoutTail'] = round((soles(base, cfg['feet_x']) - cfg.get('head_top', bounds[1]) + 1) * scale * factor)
        if name == 'bird_05':
            entry['coordinatorAdjustment'] = 'All four states uniformly reduced by 634/980; total width <=62% of canvas; shorter height accepted.'
        elif name == 'bird_06':
            entry['coordinatorAdjustment'] = 'Standing/blink/sleep uniformly enlarged 8%; height tolerance exception approved. Flight retains original approximately 980px wingspan to avoid clipping, with center shifted to match enlarged standing sprite.'
        if state == '_blink':
            diff = ImageChops.difference(base_out, out)
            entry['changedBounds'] = diff.convert('RGB').getbbox()
        manifest['assets'][name + state] = entry
(ROOT / 'manifest_front.json').write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + '\n')

sheet = Image.new('RGB', (1200, 3400), '#F7EFDB')
d = ImageDraw.Draw(sheet)
for row, name in enumerate(CONFIG):
    for col, state in enumerate(STATES):
        tile = resized(Image.open(ROOT / (name + state + '.png')), (300, 300))
        sheet.paste(tile, (col * 300, row * 340 + 30), tile)
        d.text((col * 300 + 16, row * 340 + 12), name + (state or '_stand'), fill='#5A3A22')
sheet.save(ROOT / 'contact_front.png')

rows = [['bird_01', 'bird_03', 'bird_04', 'bird_05', 'bird_06', 'bird_08'], ['bird_02', 'bird_07', 'bird_09', 'bird_10', 'bird_05', 'bird_06']] + [[name] * 6 for name in CONFIG]
row_image = Image.new('RGB', (960, len(rows) * 170 + 10), '#F7EFDB')
d = ImageDraw.Draw(row_image)
perch = Image.open(ROOT.parent / 'env' / 'perch.png').convert('RGBA')
for r, names in enumerate(rows):
    y = 15 + r * 170
    d.text((12, y - 10), 'mixed' if r < 2 else names[0], fill='#5A3A22')
    branch = resized(perch, (940, 100))
    row_image.paste(branch, (10, y + 119 - 41), branch)
    for i, name in enumerate(names):
        tile = resized(Image.open(ROOT / (name + '.png')), (140, 140))
        row_image.paste(tile, (20 + i * 156, y), tile)
row_image.save(ROOT / 'contact_front_row.png')

gray = Image.new('L', (360, 910), 239)
d = ImageDraw.Draw(gray)
for row, name in enumerate(CONFIG):
    for col, state in enumerate(STATES):
        im = resized(Image.open(ROOT / (name + state + '.png')), (64, 64))
        gray.paste(im.convert('L'), (16 + col * 88, 25 + row * 90), im.getchannel('A'))
        d.text((16 + col * 88, 9 + row * 90), name[-2:] + (state or '_stand'), fill=60)
gray.save(ROOT / 'contact_front_gray64.png')
print('Exported', len(manifest['assets']), 'assets and 3 contact sheets')
