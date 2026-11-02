"""Register T33 generated raster art and build static asset QA, never overwrite originals."""
import argparse
import hashlib
import json
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFont, ImageOps

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parents[2]
ART = REPO / 'PerchSort/Assets/Resources/V5/Art'
QA = ROOT / 'qa'
STAGE = QA / 'staged'
STAGE.mkdir(exist_ok=True)
LANCZOS = Image.Resampling.LANCZOS
SPECIES = ['sunny', 'sky', 'berry', 'mint', 'cocoa', 'hoot', 'snow', 'ember', 'ink', 'crow']
POSES = ['idle', 'blink', 'quirk1', 'quirk2', 'fly_up', 'fly_down', 'cheer', 'sleep']
FONT = ImageFont.truetype('/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf', 14)


def expected():
    names = {f'bird_{s}_{p}': (256, 256) for s in SPECIES for p in POSES
             if s not in ('sunny', 'sky') or p in ('quirk1', 'quirk2', 'cheer', 'sleep')}
    names.update({f'egg_{p}': (256, 256) for p in ['idle', 'wobble_l', 'wobble_r', 'crack', 'shell']})
    names.update(zzz=(128, 128), twig_left=(420, 90), roof_band=(300, 60))
    names.update({f'icon_{p}': (192, 192) for p in ['firefly', 'sapling', 'owl_eye', 'bell', 'scarecrow', 'rain']})
    names.update(badge_count=(64, 64), badge_plus=(64, 64))
    names.update({f'bg_w1_{p}': (1080, 1920) for p in ['morning', 'golden', 'rain']})
    names.update(petal=(64, 64), raindrop=(32, 64))
    names.update(mama_bow=(96,96), chick_fluff=(128,128), swivel_knot=(160,160))
    names.update({f'gift_{p}': (128,128) for p in ['wake','breeze','encore','gardener','stare','lightstep','warm','night','moonbeam']})
    names.update(dial_sun=(128,128), dial_moon=(128,128), night_overlay=(1080,1920))
    return names


def clean(im, soft=False):
    a = np.array(im.convert('RGBA'))
    if not soft:
        a[:, :, 3] = np.clip((a[:, :, 3].astype(float) - 20) * 255 / 233, 0, 255).astype('uint8')
    a[a[:, :, 3] == 0, :3] = 0
    return Image.fromarray(a)


def solid_bounds(im):
    return Image.fromarray((np.array(im.getchannel('A')) >= 128).astype('uint8') * 255).getbbox()


def shifted(im, dx, dy, size):
    out = Image.new('RGBA', size)
    out.alpha_composite(im, (dx, dy))
    return out


def build(name, size):
    selections = json.loads((ROOT/'selections.json').read_text()) if (ROOT/'selections.json').exists() else {}
    source = ROOT / 'raw' / (selections.get(name, name) + '.png')
    im = Image.open(source).convert('RGBA')
    if name.startswith('bg_'):
        im = ImageOps.fit(im, size, LANCZOS)
        im.putalpha(255)
        return im
    if name == 'night_overlay':
        # Exclude the generator's low-alpha bottom fringe before setting tint opacity.
        im = im.crop((0, 0, im.width, round(im.height * .80))).resize(size, LANCZOS)
        im.putalpha(140)
        return im
    im = clean(im, name in ('zzz', 'raindrop'))
    if name.startswith('bird_'):
        # Register the full reference canvas: never shrink the torso to fit wider wings.
        im = im.resize(size, LANCZOS)
        species = name.split('_')[1]
        if '_fly_' in name:
            idle = clean(Image.open(ROOT / 'raw' / f'bird_{species}_idle.png')).resize(size, LANCZOS)
            dy = 244 - (solid_bounds(idle)[3] - 1)
        else:
            dy = 244 - (solid_bounds(im)[3] - 1)
        im = shifted(im, 0, dy, size)
    elif name.startswith('egg_'):
        im = im.resize(size, LANCZOS)
        im = shifted(im, 0, 244 - (solid_bounds(im)[3] - 1), size)
    elif name == 'twig_left':
        crop = im.crop(im.getbbox()).resize((420, 48), LANCZOS)
        im = shifted(crop, 0, 32, size)
    elif name == 'roof_band':
        alpha = im.getchannel('A')
        im = ImageOps.grayscale(im).convert('RGBA')
        im.putalpha(alpha)
        crop = im.crop(im.getbbox()).resize((292, 52), LANCZOS)
        im = shifted(crop, 4, 4, size)
    else:
        padding = 4 if size[0] >= 128 else 2
        crop = im.crop(im.getbbox())
        crop = ImageOps.contain(crop, (size[0]-padding*2, size[1]-padding*2), LANCZOS)
        im = shifted(crop, (size[0]-crop.width)//2, (size[1]-crop.height)//2, size)
    a = np.array(im)
    a[a[:, :, 3] < 3, 3] = 0
    a[a[:, :, 3] == 0, :3] = 0
    return Image.fromarray(a)


def metrics(name, im):
    a = np.array(im)
    result = dict(file=name+'.png', size=list(im.size), mode=im.mode,
                  alpha_min=int(a[:, :, 3].min()), alpha_max=int(a[:, :, 3].max()),
                  visible_bounds=im.getbbox(), solid_bounds=solid_bounds(im),
                  partial_alpha_pixels=int(((a[:, :, 3]>0)&(a[:, :, 3]<255)).sum()),
                  hidden_rgb_zero=bool(np.all(a[a[:, :, 3]==0, :3]==0)))
    if name.startswith(('bird_', 'egg_')):
        result['solid_bottom_y'] = result['solid_bounds'][3]-1
    return result


def get_asset(name):
    path = STAGE / (name+'.png')
    return Image.open(path if path.exists() else ART / (name+'.png')).convert('RGBA')


def sheets(records):
    cols, cw, ch = 4, 300, 190
    sheet = Image.new('RGB', (cols*cw, ((len(records)+cols-1)//cols)*ch+35), '#c9c9c4')
    draw = ImageDraw.Draw(sheet)
    draw.text((10, 8), 'T33 | light + dark backgrounds | STATIC ASSET QA', font=FONT, fill='#222222')
    for i, r in enumerate(records):
        x, y = i%cols*cw, i//cols*ch+35
        draw.text((x+5, y+3), r['file'], font=FONT, fill='#222222')
        thumb = ImageOps.contain(get_asset(r['file'][:-4]), (142, 154), LANCZOS)
        for j, bg in enumerate(('#fff4df', '#1b2730')):
            tile = Image.new('RGBA', (146, 158), bg)
            tile.alpha_composite(thumb, ((146-thumb.width)//2, (158-thumb.height)//2))
            sheet.paste(tile.convert('RGB'), (x+j*150+2, y+25))
    sheet.save(QA/'contact.png')
    gray = Image.new('RGB', (len(SPECIES)*152, 105), '#c9c9c9')
    d = ImageDraw.Draw(gray)
    for i, species in enumerate(SPECIES):
        try:
            im = get_asset(f'bird_{species}_idle').resize((64,64), LANCZOS)
        except FileNotFoundError:
            continue
        mono = ImageOps.grayscale(im).convert('RGBA')
        mono.putalpha(im.getchannel('A'))
        for j, bg in enumerate(('#f8f8f8', '#202020')):
            tile = Image.new('RGBA', (64,64), bg)
            tile.alpha_composite(mono)
            gray.paste(tile.convert('RGB'), (i*152+j*72+4, 5))
        d.text((i*152+8, 76), species, font=FONT, fill='#222222')
    gray.save(QA/'grayscale_64_all.png')
    for species in SPECIES:
        sheet = Image.new('RGB', (8*260, 286), '#263239')
        draw = ImageDraw.Draw(sheet)
        for i, pose in enumerate(POSES):
            try:
                im = get_asset(f'bird_{species}_{pose}')
            except FileNotFoundError:
                continue
            tile = Image.new('RGBA', (256,256), '#263239')
            tile.alpha_composite(im)
            sheet.paste(tile.convert('RGB'), (i*260,0))
            draw.text((i*260+4,261), species+' '+pose, font=FONT, fill='white')
        sheet.save(QA/f'bird_{species}_frames.png')


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--publish', action='store_true')
    parser.add_argument('--publish-prefix', default='')
    args = parser.parse_args()
    records = []
    for name, size in expected().items():
        if not (ROOT/'raw'/(name+'.png')).exists():
            continue
        im = build(name, size)
        temporary = STAGE/(name+'.pending.png')
        im.save(temporary, optimize=True)
        temporary.replace(STAGE/(name+'.png'))
        records.append(metrics(name, im))
        if args.publish or (args.publish_prefix and name.startswith(args.publish_prefix)):
            dest = ART/(name+'.png')
            data = (STAGE/(name+'.png')).read_bytes()
            if dest.exists():
                assert dest.read_bytes()==data, f'Refusing to overwrite {dest}'
            else:
                with dest.open('xb') as stream:
                    stream.write(data)
    (QA/'asset_metrics.json').write_text(json.dumps(records, indent=2)+'\n')
    prompts = ['# T33 exact image-generation prompts', '', 'Tool: built-in `image_gen.imagegen`; no competitor images supplied.', '']
    logs = []
    for path in sorted((ROOT/'raw').glob('*.json')):
        item = json.loads(path.read_text())
        logs.append(item)
        prompts.extend(['## '+item['name'], '', 'References: '+', '.join('`'+p+'`' for p in item['refs']), '', '```text', item['prompt'], '```', ''])
    (ROOT/'prompts.md').write_text('\n'.join(prompts))
    (ROOT/'generation_log.json').write_text(json.dumps(logs, indent=2)+'\n')
    sheets(records)
    print(json.dumps({'built':len(records), 'expected':len(expected()), 'published':args.publish}))


if __name__ == '__main__':
    main()
