"""Size generated T36 raster assets and produce static QA without replacing originals."""
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
LANCZOS = Image.Resampling.LANCZOS
FONT = ImageFont.truetype('/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf', 16)
SMALL = ImageFont.truetype('/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf', 13)
VARIANTS = {2: ['mist', 'noon', 'lanterns', 'drizzle'],
            3: ['morning', 'noon', 'sunset', 'windy'],
            4: ['morning', 'noon', 'bluehour', 'aurora']}
PARTICLES = {2: 'leaf_bamboo', 3: 'leaf_maple', 4: 'snowflake'}
SPECIES = ['sunny', 'sky', 'berry', 'mint', 'cocoa', 'snow']


def expected():
    result = {}
    for w, variants in VARIANTS.items():
        result.update({f'bg_w{w}_{v}': (1080, 1920) for v in variants})
        for prefix, size in [('branch_left', (640, 120)), ('twig_left', (420, 90)),
                             ('bush_edge', (360, 300)), ('limb_top_plain', (1080, 320))]:
            result[f'{prefix}_w{w}'] = size
    result.update(leaf_bamboo=(48, 96), leaf_maple=(64, 64), snowflake=(48, 48))
    return result


def digest(p):
    return hashlib.sha256(p.read_bytes()).hexdigest()


def solid_bounds(im):
    return Image.fromarray((np.array(im.getchannel('A')) >= 128).astype('uint8')).getbbox()


def clean(im):
    a = np.array(im.convert('RGBA'))
    # Match the accepted T33 low-alpha cleanup; preserve antialiased contour coverage.
    a[:, :, 3] = np.clip((a[:, :, 3].astype(float) - 20) * 255 / 233, 0, 255).astype('uint8')
    a[a[:, :, 3] == 0, :3] = 0
    return Image.fromarray(a)


def surface(im, start, end):
    a = np.array(im.getchannel('A')) >= 128
    assert a[:, start:end].any(axis=0).all()
    return a[:, start:end].argmax(axis=0)


def build(name, size):
    source = ROOT / 'raw' / (name + '.png')
    original = Image.open(source)
    info = dict(source=str(source.relative_to(REPO)), raw_size=list(original.size), raw_mode=original.mode)
    im = original.convert('RGBA')
    if name.startswith('bg_'):
        # Full-frame resize preserves all corner foliage and the low landscape band.
        out = im.resize(size, LANCZOS)
        out.putalpha(255)
        info['operation'] = 'Full-frame resize, opaque RGBA; no painting or replacement sky'
        return out, info
    im = clean(im)
    out = Image.new('RGBA', size)
    if name.startswith(('branch_left', 'twig_left')):
        is_branch = name.startswith('branch_left')
        bounds = solid_bounds(im)
        cropped = im.crop((bounds[0], 0, bounds[2], im.height))
        scale = size[0] / cropped.width
        scaled = cropped.resize((size[0], round(cropped.height * scale)), LANCZOS)
        start, end, target = (85, 555, 40) if is_branch else (60, 365, 32)
        shift = target - round(float(np.median(surface(scaled, start, end))))
        out.alpha_composite(scaled, (0, shift))
        info.update(operation='Uniform resize from solid horizontal bounds; vertical registration only',
                    raw_horizontal_crop=[bounds[0], bounds[2]], scale=scale,
                    scaled_size=list(scaled.size), translation=[0, shift], surface_target_y=target)
    elif name.startswith('limb_top'):
        scaled = im.resize((size[0], round(im.height * size[0] / im.width)), LANCZOS)
        out.alpha_composite(scaled, (0, 0))
        info['operation'] = 'Uniform resize anchored at top left; transparent lower excess outside target canvas'
    elif name.startswith('bush_edge'):
        out = im.resize(size, LANCZOS)
        info['operation'] = 'Full-frame resize, left-edge root maintained'
    else:
        bounds = im.getbbox()
        cropped = im.crop(bounds)
        scaled = ImageOps.contain(cropped, (size[0] - 6, size[1] - 6), LANCZOS)
        position = ((size[0] - scaled.width) // 2, (size[1] - scaled.height) // 2)
        out.alpha_composite(scaled, position)
        info.update(operation='Uniform resize from alpha bounds, centered with at least 3px margin',
                    raw_crop=list(bounds), placed_size=list(scaled.size), translation=list(position))
    a = np.array(out)
    a[a[:, :, 3] < 3, 3] = 0
    a[a[:, :, 3] == 0, :3] = 0
    return Image.fromarray(a), info


def metrics(name, im):
    a = np.array(im)
    alpha = a[:, :, 3]
    result = dict(size=list(im.size), mode=im.mode, alpha_min=int(alpha.min()), alpha_max=int(alpha.max()),
                  visible_bounds=im.getbbox(), solid_bounds=solid_bounds(im),
                  partial_alpha_pixels=int(((alpha > 0) & (alpha < 255)).sum()),
                  hidden_rgb_zero=bool(np.all(a[alpha == 0, :3] == 0)))
    if name.startswith(('branch_left', 'twig_left')):
        branch = name.startswith('branch_left')
        start, end = (85, 555) if branch else (60, 365)
        ys = surface(im, start, end)
        sample_x = [110, 230, 350, 470, 540] if branch else [90, 200, 310, 350]
        result['foot_surface'] = dict(alpha_threshold=128, x_range=[start, end - 1],
                                      min_y=int(ys.min()), median_y=float(np.median(ys)), max_y=int(ys.max()),
                                      samples={str(x): int(surface(im, x, x + 1)[0]) for x in sample_x})
        result['root_touches_left'] = bool((alpha[:, 0] >= 128).any())
        result['suggested_pivot_normalized_bottom_left'] = [0, (im.height - (40 if branch else 32)) / im.height]
    elif name.startswith('limb_top'):
        result['alpha_pixels_at_or_below_y260'] = int((alpha[260:] > 0).sum())
        result['suggested_pivot_normalized_bottom_left'] = [0.5, 1]
    else:
        result['suggested_pivot_normalized_bottom_left'] = [0.5, 0.5]
    return result


def contact(w, outputs):
    sheet = Image.new('RGBA', (1080, 1400), '#d6d8d5')
    d = ImageDraw.Draw(sheet)
    d.text((18, 14), f'T36 W{w} | static raster QA | light / dark', font=FONT, fill='#172a30')
    for i, variant in enumerate(VARIANTS[w]):
        name = f'bg_w{w}_{variant}'
        x = 14 + i * 268
        d.text((x, 44), name, font=SMALL, fill='#172a30')
        sheet.alpha_composite(outputs[name].resize((250, 444), LANCZOS), (x, 68))
    entries = [(f'branch_left_w{w}', (500, 94)), (f'twig_left_w{w}', (420, 90)),
               (f'bush_edge_w{w}', (260, 217)), (f'limb_top_plain_w{w}', (510, 151)),
               (PARTICLES[w], None)]
    y = 530
    for name, target in entries:
        thumb = outputs[name] if target is None else outputs[name].resize(target, LANCZOS)
        row_height = max(thumb.height + 32, 94)
        for side, bg in enumerate(('#fff4df', '#1b2730')):
            tile = Image.new('RGBA', (534, row_height), bg)
            td = ImageDraw.Draw(tile)
            td.text((9, 5), name, font=SMALL, fill='#203138' if side == 0 else '#f7f4ec')
            tile.alpha_composite(thumb, ((534 - thumb.width) // 2, 27))
            sheet.alpha_composite(tile, (side * 540 + 3, y))
        y += row_height + 4
    # Expand as needed rather than clipping the final particle preview.
    if y > sheet.height:
        raise ValueError(f'Contact layout overflow: {y}')
    sheet.convert('RGB').save(QA / f'w{w}_contact_light_dark.png')


def geometry(w, outputs):
    sheet = Image.new('RGBA', (1280, 480), '#fff4df')
    sheet.paste(Image.new('RGBA', (640, 480), '#1b2730'), (640, 0))
    for side in range(2):
        d = ImageDraw.Draw(sheet)
        ink = '#203138' if side == 0 else '#fff4df'
        for row, prefix in enumerate(('branch_left', 'twig_left')):
            im = outputs[f'{prefix}_w{w}']
            x, y = side * 640, 65 + row * 210
            d.text((x + 10, y - 36), f'{prefix}_w{w}: native size, cyan = solid top', font=SMALL, fill=ink)
            sheet.alpha_composite(im, (x, y))
            start, end = (85, 555) if row == 0 else (60, 365)
            ys = surface(im, start, end)
            points = [(x + start + i, y + int(v) - 2) for i, v in enumerate(ys)]
            d.line(points, fill='#00d5dc', width=1)
            d.text((x + 10, y + im.height + 12), f'Surface y: {ys.min()}..{ys.max()}, median {np.median(ys):g}', font=FONT, fill=ink)
    sheet.convert('RGB').save(QA / f'w{w}_perch_geometry.png')


def composite(w, outputs):
    variant = {2: 'lanterns', 3: 'sunset', 4: 'aurora'}[w]
    scene = outputs[f'bg_w{w}_{variant}'].copy()
    scene.alpha_composite(outputs[f'limb_top_plain_w{w}'], (0, 0))
    placements = []
    for row, (bx, by) in enumerate(((0, 830), (440, 1110), (0, 1390))):
        branch = outputs[f'branch_left_w{w}']
        bush = outputs[f'bush_edge_w{w}']
        if row == 1:
            branch, bush = ImageOps.mirror(branch), ImageOps.mirror(bush)
        scene.alpha_composite(branch, (bx, by))
        scene.alpha_composite(bush, (930 if row == 1 else -210, by - 60))
        for slot in range(2):
            species = SPECIES[row * 2 + slot]
            bird = Image.open(ART / f'bird_{species}_idle.png').convert('RGBA').resize((150, 150), LANCZOS)
            cx = [650, 830][slot] if row == 1 else [250, 430][slot]
            foot_y = int(round(float(np.median(surface(branch, cx - bx - 18, cx - bx + 19)))))
            bird_bottom = solid_bounds(bird)[3] - 1
            pos = (cx - 75, by + foot_y - bird_bottom)
            scene.alpha_composite(bird, pos)
            placements.append(dict(species=species, branch=row + 1, bird_position=list(pos), bird_size=[150, 150],
                                   bird_solid_bottom=bird_bottom, branch_surface_y=by + foot_y,
                                   vertical_solid_contact_error_px=pos[1] + bird_bottom - by - foot_y))
    scene.save(QA / f'w{w}_six_species_composite.png')
    annotated = scene.copy()
    d = ImageDraw.Draw(annotated)
    d.rectangle((28, 510, 1052, 586), fill='#fff4df')
    d.text((48, 524), f'T36 W{w}: six species / three branches / STATIC ASSET COMPOSITE', font=FONT, fill='#22343b')
    d.text((48, 553), 'Not a Unity screenshot, runtime layout, or animation acceptance.', font=FONT, fill='#22343b')
    annotated.save(QA / f'w{w}_six_species_annotated.png')
    return dict(background=f'bg_w{w}_{variant}.png', placements=placements,
                six_distinct_species=len({p['species'] for p in placements}) == 6,
                three_branches=len({p['branch'] for p in placements}) == 3,
                static_contact_all_zero=all(p['vertical_solid_contact_error_px'] == 0 for p in placements))


def small_scale(outputs):
    names = [f'{p}_w{w}' for w in [2, 3, 4] for p in ['branch_left', 'twig_left', 'bush_edge', 'limb_top_plain']]
    names += list(PARTICLES.values())
    sheet = Image.new('RGBA', (1080, 640), '#d6d8d5')
    d = ImageDraw.Draw(sheet)
    d.text((15, 12), 'T36 64px-long-edge grayscale / native particle recognition', font=FONT, fill='#203138')
    for i, name in enumerate(names):
        x, y = i % 3 * 360, i // 3 * 118 + 45
        im = ImageOps.contain(outputs[name], (64, 64), LANCZOS)
        gray = ImageOps.grayscale(im).convert('RGBA')
        gray.putalpha(im.getchannel('A'))
        d.text((x + 8, y), name, font=SMALL, fill='#203138')
        for j, bg in enumerate(('#fff4df', '#1b2730')):
            tile = Image.new('RGBA', (168, 80), bg)
            tile.alpha_composite(im, ((80 - im.width) // 2, (80 - im.height) // 2))
            tile.alpha_composite(gray, (84 + (80 - im.width) // 2, (80 - im.height) // 2))
            sheet.alpha_composite(tile, (x + j * 174 + 4, y + 24))
    sheet.convert('RGB').save(QA / 'recognition_64_grayscale.png')


def main(publish):
    STAGE.mkdir(exist_ok=True)
    outputs, records, normalization = {}, {}, {}
    for name, size in expected().items():
        im, info = build(name, size)
        outputs[name], normalization[name] = im, info
        p = STAGE / (name + '.png')
        pending = p.with_suffix('.pending.png')
        im.save(pending, compress_level=6)
        pending.replace(p)
        r = metrics(name, im)
        r['sha256'] = digest(p)
        checks = dict(size=list(size) == r['size'], rgba=r['mode'] == 'RGBA', hidden_rgb_zero=r['hidden_rgb_zero'],
                      alpha=(r['alpha_min'], r['alpha_max']) == ((255, 255) if name.startswith('bg_') else (0, 255)))
        if 'foot_surface' in r:
            target = 40 if name.startswith('branch_left') else 32
            s = r['foot_surface']
            checks.update(root_touches_left=r['root_touches_left'],
                          surface_registered=s['median_y'] == target,
                          surface_near_target=target - 5 <= s['min_y'] <= s['max_y'] <= target + 5)
        if name.startswith('limb_top'):
            checks['clear_below_y260'] = r['alpha_pixels_at_or_below_y260'] == 0
        r['checks'] = checks
        records[name + '.png'] = r
    for w in [2, 3, 4]:
        contact(w, outputs)
        geometry(w, outputs)
    composites = {f'w{w}': composite(w, outputs) for w in [2, 3, 4]}
    small_scale(outputs)
    baseline = json.loads((QA / 'baseline.json').read_text())['files']
    changed = [p for p, h in baseline.items() if not (REPO / p).exists() or digest(REPO / p) != h]
    failures = {n: [k for k, ok in r['checks'].items() if not ok] for n, r in records.items()}
    failures = {n: bad for n, bad in failures.items() if bad}
    result = dict(status='PASS' if not failures and not changed else 'FAIL', runtime='NOT_RUN: no Unity Editor on Linux',
                  requested_count=27, generated_count=len(outputs), missing_files=[], files=records,
                  failed_checks=failures, preexisting_files_checked=len(baseline), changed_preexisting_files=changed,
                  normalization=normalization, composites=composites, published=False)
    if publish:
        assert result['status'] == 'PASS', result['failed_checks']
        for name in outputs:
            dest = ART / (name + '.png')
            data = (STAGE / dest.name).read_bytes()
            if dest.exists():
                assert dest.read_bytes() == data, f'Refusing to replace {dest}'
            else:
                with dest.open('xb') as stream:
                    stream.write(data)
        result['published'] = True
    (QA / 'verification.json').write_text(json.dumps(result, indent=2) + '\n')
    (ROOT / 'delivered_files.txt').write_text(''.join(str((ART / (n + '.png')).relative_to(REPO)) + '\n' for n in outputs) if publish else '')
    prompts = ['# T36 exact image-generation prompts', '', 'Tool: built-in image_gen.imagegen.',
               'All raster content is generated; references are project-owned accepted World 1 art or derived world scenes.', '']
    logs = []
    for p in sorted((ROOT / 'raw').glob('*.json')):
        item = json.loads(p.read_text())
        logs.append(item)
        prompts += ['## ' + item['name'], '', 'References: ' + ', '.join(item['refs']), '', '```text', item['prompt'], '```', '']
    (ROOT / 'prompts.md').write_text('\n'.join(prompts))
    (ROOT / 'generation_log.json').write_text(json.dumps(logs, indent=2) + '\n')
    print(json.dumps({k: result[k] for k in ['status', 'requested_count', 'generated_count', 'failed_checks',
                                           'preexisting_files_checked', 'changed_preexisting_files', 'published']}, indent=2))


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--publish', action='store_true')
    main(parser.parse_args().publish)
