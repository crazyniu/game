"""T40 concept compositor, explicitly authorized by coordinator msg_db524938f2df.

All illustrations and UI surfaces come from imagegen or read-only project art.
This script only slices, resizes, masks portraits, places rasters, and typesets.
It is not runtime code and makes no animation or gameplay-validity claims.
"""
from pathlib import Path
import hashlib
import json
import shutil
import numpy as np
from PIL import Image, ImageDraw, ImageFont, ImageOps, ImageFilter

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'art/v6'
MOCK = OUT / 'mock'
QA = OUT / 'qa'
PARTS = OUT / 'derived'
ART = ROOT / 'PerchSort/Assets/Resources/V5/Art'
FONT = ROOT / 'PerchSort/Assets/Fonts/Fredoka-Medium.ttf'
for folder in (MOCK, QA, PARTS):
    folder.mkdir(parents=True, exist_ok=True)
RESAMPLE = Image.Resampling.LANCZOS
BROWN = '#593B27'
RECORD = {}
SOURCE_PATHS = set()


def read(path):
    path = Path(path)
    SOURCE_PATHS.add(path)
    return Image.open(path).convert('RGBA')


def trim(im, threshold=16):
    a = np.array(im.getchannel('A'))
    bbox = Image.fromarray((a > threshold).astype('uint8') * 255).getbbox()
    if bbox is None:
        raise ValueError('Empty sprite')
    im = im.crop(bbox)
    a = np.array(im.getchannel('A'))
    a[a < threshold] = 0
    im.putalpha(Image.fromarray(a))
    return im


def grid(name, cols, rows, names, ycuts=None):
    atlas = read(OUT / 'raw' / name)
    result = {}
    for idx, key in enumerate(names):
        x, y = idx % cols, idx // cols
        box = (round(x * atlas.width / cols), ycuts[y] if ycuts else round(y * atlas.height / rows),
               round((x + 1) * atlas.width / cols), ycuts[y + 1] if ycuts else round((y + 1) * atlas.height / rows))
        result[key] = trim(atlas.crop(box))
        result[key].save(PARTS / f'{key}.png')
    return result


ICONS = grid('tools_atlas.png', 4, 2,
             ['undo', 'hint', 'add_branch', 'peek', 'wake', 'scarecrow', 'rain', 'gear'])
ICONS.update(grid('utility_atlas.png', 4, 4,
                  ['coin', 'star', 'trophy', 'check', 'replay', 'home', 'sound', 'close',
                   'plus', 'unused_note', 'no_count', 'forward', 'dot_empty', 'dot_full', 'unlock', 'badge_red'],
                  ycuts=[0, 360, 675, 970, 1254]))
ICONS.update(grid('map_ui_atlas.png', 4, 3,
                  ['shop', 'aviary', 'play', 'lock', 'disc_green', 'disc_cream',
                   'disc_gray', 'star_empty', 'gate_closed', 'gate_open', 'moves', 'bounce_shadow']))
GIFTS = grid('gifts_atlas.png', 3, 3,
             ['wake_song', 'breeze', 'superseded_encore', 'gardener', 'stare',
              'light_step', 'warm_glow', 'night_call', 'moonbeam'])
GIFTS['no_count'] = ICONS['no_count']
SPECIES = ['sunny', 'berry', 'mint', 'sky', 'cocoa', 'snow', 'ember', 'ink', 'hoot']
HOUSES = grid('houses_atlas.png', 3, 3, [f'house_{s}' for s in SPECIES])
GIFT_BY_SPECIES = dict(zip(SPECIES, ['wake_song', 'no_count', 'gardener', 'breeze', 'stare',
                                   'light_step', 'warm_glow', 'night_call', 'moonbeam']))
surfaces = read(OUT / 'raw/surfaces_atlas.png')
SURFACES = {}
for name, box in {
    'cream': (30, 40, 500, 540), 'green': (530, 40, 1000, 540),
    'bark': (1030, 40, 1510, 540), 'leaf': (30, 590, 480, 970),
    'ribbon': (480, 665, 1080, 900), 'badge': (1140, 630, 1430, 950),
}.items():
    SURFACES[name] = trim(surfaces.crop(box), threshold=100)
    SURFACES[name].save(PARTS / f'surface_{name}.png')


def fitted(im, size):
    return ImageOps.contain(im, size, RESAMPLE)


def stamp(canvas, im, center, size, opacity=1):
    sprite = fitted(im, size)
    if opacity != 1:
        sprite = sprite.copy()
        sprite.putalpha(sprite.getchannel('A').point(lambda a: round(a * opacity)))
    x, y = round(center[0] - sprite.width / 2), round(center[1] - sprite.height / 2)
    canvas.alpha_composite(sprite, (x, y))
    return [x, y, sprite.width, sprite.height]


def surface(kind, size, corner=34):
    # Nine-slice the generated surface: preserve corners and extend its own fill.
    base = SURFACES[kind].resize((116, 116), RESAMPLE)
    w, h = size
    c = min(corner, w // 2 - 1, h // 2 - 1)
    if size == (116, 116):
        return base
    xs, ys = [0, c, 116 - c, 116], [0, c, 116 - c, 116]
    xd, yd = [0, c, w - c, w], [0, c, h - c, h]
    result = Image.new('RGBA', size)
    for row in range(3):
        for col in range(3):
            tile = base.crop((xs[col], ys[row], xs[col + 1], ys[row + 1]))
            tile = tile.resize((xd[col + 1] - xd[col], yd[row + 1] - yd[row]), RESAMPLE)
            result.alpha_composite(tile, (xd[col], yd[row]))
    return result


def panel(canvas, box, kind='cream'):
    x, y, w, h = box
    sprite = surface(kind, (w, h))
    shadowed_stamp(canvas, sprite, x, y)


def shadowed_stamp(canvas, sprite, x, y):
    # Shadow is derived only from the generated surface silhouette.
    shadow = Image.new('RGBA', (sprite.width + 12, sprite.height + 12), '#59432E00')
    alpha = Image.new('L', shadow.size)
    alpha.paste(sprite.getchannel('A').point(lambda a: round(a * .18)), (6, 6))
    shadow.putalpha(alpha.filter(ImageFilter.GaussianBlur(3)))
    canvas.alpha_composite(shadow, (x - 6, y))
    canvas.alpha_composite(sprite, (x, y))


def label(canvas, text, center, size, fill=BROWN, bold=False):
    font = ImageFont.truetype(str(FONT), size)
    ImageDraw.Draw(canvas).text(center, text, font=font, fill=fill, anchor='mm',
                               stroke_width=1 if bold else 0, stroke_fill=fill)


def icon(canvas, key, center, size=64, opacity=1):
    return stamp(canvas, ICONS[key], center, (size, size), opacity)


def button(canvas, key, center, size=116, kind='cream', badge=None, opacity=1):
    tile = Image.new('RGBA', (size + 44, size + 44))
    shadowed_stamp(tile, surface(kind, (size, size)), 10, 24)
    icon(tile, key, (10 + size / 2, 24 + size / 2), 64 if size >= 100 else 40)
    if badge is not None:
        bc = (10 + size - 2, 24 + 2)
        stamp(tile, SURFACES['badge'] if badge == '+' else ICONS['badge_red'], bc, (40, 40))
        if badge == '+':
            icon(tile, 'plus', bc, 24)
        else:
            label(tile, str(badge), bc, 26, bold=True)
    if opacity != 1:
        tile.putalpha(tile.getchannel('A').point(lambda a: round(a * opacity)))
    canvas.alpha_composite(tile, (round(center[0] - size / 2 - 10), round(center[1] - size / 2 - 24)))


def portrait(species):
    # Read-only original head crop; no new character drawing or recoloring.
    bird = read(ART / f'bird_{species}_idle.png')
    return bird.crop((62, 45, 196, 170))


def goal_chip(species, state='empty'):
    chip = Image.new('RGBA', (104, 120))
    house = fitted(HOUSES[f'house_{species}'], (88, 90))
    hx, hy = (104 - house.width) // 2, 8
    chip.alpha_composite(house, (hx, hy))
    # Clip original portrait into the generated house's dark doorway.
    face = ImageOps.fit(portrait(species), (40, 37), RESAMPLE)
    face_mask = Image.new('L', face.size)
    ImageDraw.Draw(face_mask).rounded_rectangle((0, 0, 39, 42), radius=19, fill=255)
    face.putalpha(Image.fromarray(np.minimum(np.array(face.getchannel('A')), np.array(face_mask))))
    chip.alpha_composite(face, (32, 51))
    filled = {'empty': 0, 'half': 2, 'home': 4}[state]
    for index, x in enumerate([25, 43, 61, 79]):
        icon(chip, 'dot_full' if index < filled else 'dot_empty', (x, 108), 11)
    if state == 'home':
        icon(chip, 'check', (90, 85), 20)
    stamp(chip, SURFACES['badge'], (82, 22), (44, 44))
    stamp(chip, GIFTS[GIFT_BY_SPECIES[species]], (82, 22), (32, 32))
    return chip


def background():
    return read(ART / 'bg_w1_morning.png')


def hud(canvas, k, skin='cream'):
    rope = read(ART / 'rope_segment.png').resize((12, 110), RESAMPLE)
    for x in (180, 900):
        canvas.alpha_composite(rope, (x - 6, 75))
    panel(canvas, (40, 64, 190, 72), skin)
    icon(canvas, 'coin', (76, 100), 48)
    label(canvas, '240' if k == 3 else '1280', (159, 100), 38, bold=True)
    stamp(canvas, SURFACES['leaf'], (540, 100), (72, 72))
    label(canvas, '5' if k == 3 else '37', (540, 102), 38, bold=True)
    button(canvas, 'gear', (1000, 100), 80, skin)
    panel(canvas, (80, 175, 920, 150), skin)
    centers = [280, 540, 800] if k == 3 else [180, 300, 420, 540, 660, 780, 900]
    species = ['sunny', 'berry', 'sky'] if k == 3 else SPECIES[:k]
    home_species = {'sky'} if k == 3 else {'sunny', 'mint'}
    for x, name in zip(centers, species):
        canvas.alpha_composite(goal_chip(name, 'home' if name in home_species else 'empty'), (x - 52, 190))
    if k == 7:
        stamp(canvas, SURFACES['ribbon'], (900, 359), (112, 44))
        label(canvas, 'x2', (900, 357), 25, bold=True)
    for i, name in enumerate(['undo', 'hint', 'add_branch'] + (['scarecrow'] if k == 7 else [])):
        button(canvas, name, (225 + i * 210, 1745), kind=skin,
               badge=[None, 2, '+', 1][i])
    RECORD[f'hud_k{k}_{skin}'] = {
        'coin': [40, 64, 190, 72], 'coin_icon_frame': [52, 76, 48, 48],
        'level_frame': [504, 64, 72, 72], 'settings': [960, 60, 80, 80],
        'goal_tray': [80, 175, 920, 150], 'goal_chip_centers': centers,
        'goal_chip_frame': [104, 120], 'goal_chip_y': 190,
        'gift_badge_diameter': 44, 'gift_glyph_frame': [32, 32],
        'bottom_button_centers': [[225 + i * 210, 1745] for i in range(3 if k == 3 else 4)],
        'button_frame': [116, 116], 'icon_frame': [64, 64], 'count_badge': [40, 40],
        'song_meter': None, 'chain_ribbon': [844, 337, 112, 44] if k == 7 else None,
    }


def branch(canvas, side, foot_y, birds):
    original = read(ART / 'branch_left.png')
    branch_im = original.resize((510, 96), RESAMPLE)
    if side == 'right':
        branch_im = ImageOps.mirror(branch_im)
    x = 0 if side == 'left' else 570
    y = foot_y - 32
    canvas.alpha_composite(branch_im, (x, y))
    bush = read(ART / 'bush_edge.png')
    bush = fitted(bush, (156, 130))
    if side == 'right':
        bush = ImageOps.mirror(bush)
    canvas.alpha_composite(bush, (0 if side == 'left' else 1080 - bush.width, foot_y - 65))
    slots = [190, 350] if len(birds) == 2 else [130, 270, 410]
    if side == 'right':
        slots = [1080 - q for q in slots][::-1]
    placed = []
    a = np.array(branch_im.getchannel('A'))
    for cx, entry in zip(slots, birds):
        name, pose = (entry.split(':') + ['idle'])[:2] if ':' in entry else (entry, 'idle')
        im = read(ART / f'bird_{name}_{pose}.png').resize((136, 136), RESAMPLE)
        local_x = cx - x
        wood_top = int(np.where(a[:, local_x] > 128)[0][0])
        by = y + wood_top - 128
        canvas.alpha_composite(im, (cx - 68, by))
        if pose == 'sleep':
            stamp(canvas, read(ART / 'zzz.png'), (cx + 50, by + 16), (36, 36))
        placed.append({'species_pose': entry, 'frame': [cx - 68, by, 136, 136], 'foot_y': y + wood_top})
    return {'side': side, 'branch_frame': [x, y, 510, 96], 'birds': placed}


def level(k, skin='cream'):
    canvas = background()
    branches = []
    if k == 3:
        configs = [('left', 710, ['sunny', 'berry']), ('right', 770, ['berry', 'sunny']),
                   ('left', 1180, ['berry', 'sunny']), ('right', 1240, ['sunny', 'berry'])]
    else:
        # Two species have already homed; remaining five have four birds each.
        remaining = ['berry', 'sky', 'cocoa', 'snow:sleep', 'ember']
        birds = remaining + ['berry', 'sky', 'cocoa', 'snow', 'ember'] * 3
        birds.insert(7, 'crow')
        counts = [2, 2, 2, 3, 2, 2, 2, 2, 2, 2]
        configs, cursor = [], 0
        for row in range(5):
            for side_idx, side in enumerate(['left', 'right']):
                n = counts[row * 2 + side_idx]
                configs.append((side, 610 + row * 200 + side_idx * 60, birds[cursor:cursor + n]))
                cursor += n
    for side, y, birds in configs:
        branches.append(branch(canvas, side, y, birds))
    hud(canvas, k, skin)
    RECORD[f'board_k{k}_{skin}'] = {'band': [0, 420, 1080, 1180], 'tip_gap': 60, 'branches': branches}
    return canvas


def dim(canvas):
    # UI_SPEC's black overlay at alpha .72 in linear light, encoded back to sRGB.
    rgba = np.array(canvas).astype(float) / 255
    rgb = rgba[:, :, :3]
    linear = np.where(rgb <= .04045, rgb / 12.92, ((rgb + .055) / 1.055) ** 2.4)
    linear *= .28
    rgb = np.where(linear <= .0031308, linear * 12.92, 1.055 * linear ** (1 / 2.4) - .055)
    rgba[:, :, :3] = rgb
    return Image.fromarray(np.round(rgba * 255).astype('uint8'))


def dialogs(base):
    cleared = background()
    hud(cleared, 3)
    for x, species in zip([280, 540, 800], ['sunny', 'berry', 'sky']):
        # Replace only the chip area; its prior contents are inside an opaque tray.
        panel_patch = surface('cream', (920, 150)).crop((x - 132, 15, x - 28, 135))
        cleared.alpha_composite(panel_patch, (x - 52, 190))
        cleared.alpha_composite(goal_chip(species, 'home'), (x - 52, 190))
    win = dim(cleared)
    panel(win, (130, 580, 820, 760))
    stamp(win, SURFACES['ribbon'], (540, 640), (700, 145))
    icon(win, 'trophy', (540, 632), 64)
    for cx, cy, sz in [(350, 850, 150), (540, 825, 180), (730, 850, 150)]:
        icon(win, 'star', (cx, cy), sz)
    panel(win, (425, 1000, 230, 72))
    icon(win, 'coin', (468, 1036), 48)
    label(win, '+80', (563, 1036), 42, bold=True)
    button(win, 'replay', (285, 1220), 100)
    panel(win, (410, 1162, 260, 116), 'green')
    icon(win, 'forward', (540, 1220), 64)
    button(win, 'home', (795, 1220), 100)
    win.save(MOCK / 'win.png')
    settings = dim(base)
    panel(settings, (260, 730, 560, 460))
    button(settings, 'close', (780, 770), 80)
    button(settings, 'sound', (430, 960), 116, 'green')
    button(settings, 'replay', (650, 960), 116)
    settings.save(MOCK / 'settings.png')
    unlock = dim(base)
    panel(unlock, (190, 600, 700, 720))
    icon(unlock, 'peek', (540, 805), 192)
    label(unlock, 'Owl Eye', (540, 975), 52, bold=True)
    label(unlock, 'See inside every egg.', (540, 1045), 32)
    button(unlock, 'check', (540, 1205), 116, 'green')
    unlock.save(MOCK / 'unlock_card.png')
    info = base.copy()
    panel(info, (220, 380, 640, 280))
    stamp(info, read(ART / 'bird_sunny_idle.png'), (330, 520), (160, 160))
    label(info, 'Sunny', (570, 450), 42, bold=True)
    stamp(info, GIFTS['wake_song'], (460, 532), (42, 42))
    label(info, 'Wake sleeping birds.', (631, 532), 32)
    info.save(MOCK / 'goal_info.png')
    RECORD['dialogs'] = {
        'win': {'panel': [130, 580, 820, 760], 'ribbon_frame': [190, 567, 700, 145],
                'trophy_frame': [508, 600, 64, 64], 'star_centers_sizes': [[350, 850, 150], [540, 825, 180], [730, 850, 150]],
                'reward_chip': [425, 1000, 230, 72], 'replay': [235, 1170, 100, 100],
                'next': [410, 1162, 260, 116], 'home': [745, 1170, 100, 100]},
        'settings': {'panel': [260, 730, 560, 460], 'close': [740, 730, 80, 80],
                     'sound': [372, 902, 116, 116], 'restart': [592, 902, 116, 116]},
        'unlock': {'panel': [190, 600, 700, 720], 'icon_frame': [444, 709, 192, 192],
                   'title_center_size': [540, 975, 52], 'body_center_size': [540, 1045, 32],
                   'check_button': [482, 1147, 116, 116]},
        'goal_info': {'panel': [220, 380, 640, 280], 'portrait_frame': [250, 440, 160, 160],
                      'name_center_size': [570, 450, 42], 'gift_frame': [439, 511, 42, 42],
                      'body_center_size': [631, 532, 32]},
        'overlay': 'black, alpha=0.72, applied in linear light; no blur',
    }


def icon_sheet():
    sheet = Image.new('RGBA', (1080, 1920), '#FFF9EB')
    label(sheet, 'PERCH SORT / TOOL & GIFT STUDY', (540, 46), 36)
    headings = [(130, 'TOOL'), (380, '128 / cream'), (570, '64 / cream'),
                (760, '128 / forest'), (950, '64 / forest')]
    for x, text in headings:
        label(sheet, text, (x, 102), 22)
    tool_names = [('undo', 'Undo'), ('hint', 'Hint'), ('add_branch', 'Add branch'),
                  ('peek', 'Peek eggs'), ('wake', 'Morning bell'), ('scarecrow', 'Scarecrow'), ('rain', 'Spring rain')]
    forest = background().crop((400, 500, 800, 830))
    for row, (key, text) in enumerate(tool_names):
        cy = 195 + row * 150
        label(sheet, text, (130, cy), 28)
        for cx, size, surface_kind in [(380, 128, 'cream'), (570, 64, 'cream'), (760, 128, 'forest'), (950, 64, 'forest')]:
            if surface_kind == 'cream':
                panel(sheet, (cx - 85, cy - 70, 170, 140))
            else:
                sheet.alpha_composite(ImageOps.fit(forest, (170, 140), RESAMPLE), (cx - 85, cy - 70))
            icon(sheet, key, (cx, cy), size)
    label(sheet, 'GOAL CHIP / ARRIVAL STATES', (540, 1213), 28)
    for cx, state, name in [(310, 'empty', 'Empty'), (540, 'half', 'Arriving 2/4'), (770, 'home', 'Home')]:
        sheet.alpha_composite(goal_chip('sunny', state), (cx - 52, 1250))
        label(sheet, name, (cx, 1400), 25)
    label(sheet, 'NINE GIFTS / 64 px GLYPH + 44 px BADGE', (540, 1460), 28)
    gift_names = ['Wake song', 'No-count', 'Gardener', 'Breeze', 'Stare', 'Light step', 'Warm glow', 'Night call', 'Moonbeam']
    for idx, (species, name) in enumerate(zip(SPECIES, gift_names)):
        cx = 68 + idx * 118
        glyph = GIFTS[GIFT_BY_SPECIES[species]]
        stamp(sheet, glyph, (cx, 1530), (64, 64))
        stamp(sheet, SURFACES['badge'], (cx, 1610), (44, 44))
        stamp(sheet, glyph, (cx, 1610), (32, 32))
        label(sheet, name, (cx, 1660), 20)
    label(sheet, 'STOCK & DISABLED', (165, 1780), 25)
    button(sheet, 'hint', (440, 1800), badge=2, opacity=.45)
    label(sheet, '45% opacity', (440, 1890), 22)
    button(sheet, 'add_branch', (740, 1800), badge='+')
    label(sheet, 'Zero stock', (740, 1890), 22)
    sheet.save(MOCK / 'icon_sheet.png')
    # Independent grayscale contact sheet at the actual requested 64 px icon size.
    gray = Image.new('RGB', (560, 140), '#F9F3E4')
    for idx, (key, _) in enumerate(tool_names):
        tile = Image.new('RGBA', (80, 140), '#F9F3E4')
        icon(tile, key, (40, 45), 64)
        label(tile, str(idx + 1), (40, 108), 20)
        gray.paste(ImageOps.grayscale(tile.convert('RGB')).convert('RGB'), (idx * 80, 0))
    gray.save(QA / 'tools_grayscale_64.png')
    RECORD['icon_sheet'] = {'canvas': [1080, 1920], 'row_centers_y': [195 + i * 150 for i in range(7)],
                            'columns_x': [380, 570, 760, 950], 'frames_px': [128, 64, 128, 64],
                            'goal_chip_top_y': 1250, 'goal_chip_centers_x': [310, 540, 770],
                            'gift_centers_x': [68 + i * 118 for i in range(9)],
                            'gift_y_sizes': [[1530, 64], [1610, 32]], 'gift_badge_diameter': 44,
                            'disabled_opacity': .45, 'state_button_centers': [[440, 1800], [740, 1800]]}


def map_scene(card_state=False):
    canvas = ImageOps.fit(read(OUT / 'raw/map_background.png'), (1080, 1920), RESAMPLE)
    nodes = [(7, 395, 1480, 'cleared', 3), (8, 300, 1180, 'cleared', 1),
             (9, 735, 915, 'current', 0), (10, 750, 650, 'unlocked', 0),
             (11, 400, 365, 'locked', 0), (12, 300, 215, 'locked', 0)]
    if card_state:
        nodes = [(n, x, y, 'cleared', 2 if n == 12 else 3) for n, x, y, _, _ in nodes]
    for number, x, y, state, stars in nodes:
        if state == 'current':
            stamp(canvas, ICONS['bounce_shadow'], (x, y + 78), (128, 28))
            stamp(canvas, read(ART / 'bird_sunny_idle.png'), (x, y - 58), (112, 112))
        disc = 'disc_green' if state == 'cleared' else 'disc_gray' if state == 'locked' else 'disc_cream'
        icon(canvas, disc, (x, y), 132)
        label(canvas, str(number), (x, y + 4), 44, bold=True)
        if state == 'locked':
            icon(canvas, 'lock', (x + 43, y + 43), 32)
        if stars:
            for i in range(3):
                icon(canvas, 'star' if i < stars else 'star_empty', (x - 34 + i * 34, y + 87), 28)
    stamp(canvas, ICONS['gate_open' if card_state else 'gate_closed'], (585, 495), (250, 225))
    panel(canvas, (40, 64, 190, 72))
    icon(canvas, 'coin', (76, 100), 48)
    label(canvas, '240', (159, 100), 38, bold=True)
    button(canvas, 'gear', (1000, 100), 80)
    for x, name in zip([330, 540, 750], ['shop', 'aviary', 'gear']):
        button(canvas, name, (x, 1745))
    RECORD['map_card_backdrop' if card_state else 'map'] = {
        'canvas': [1080, 1920], 'background_method': 'uniform cover resize then symmetric vertical crop; no aspect warp',
        'nodes': [{'number': n, 'center': [x, y], 'state': state, 'stars': stars} for n, x, y, state, stars in nodes],
        'node_frame': [132, 132], 'node_number_font': 44, 'star_size': 28, 'star_spacing': 34, 'star_y_offset': 87,
        'lock_size': 32, 'lock_offset': [43, 43], 'current_bird_frame': [112, 112], 'bird_y_offset': -58,
        'bounce_shadow_frame': [128, 28], 'shadow_y_offset': 78, 'gate_frame': [460, 382, 250, 225],
        'bottom_centers': [[330, 1745], [540, 1745], [750, 1745]], 'bottom_button_size': 116,
        'coin': [40, 64, 190, 72], 'settings': [960, 60, 80, 80],
    }
    return canvas


def map_and_card():
    map_scene().save(MOCK / 'map.png')
    card = dim(map_scene(card_state=True))
    panel(card, (160, 650, 760, 620))
    stamp(card, SURFACES['leaf'], (540, 695), (72, 72))
    label(card, '12', (540, 697), 38, bold=True)
    button(card, 'close', (880, 690), 64)
    for x, key in [(488, 'star'), (540, 'star'), (592, 'star_empty')]:
        icon(card, key, (x, 772), 40)
    panel(card, (370, 823, 340, 64))
    icon(card, 'moves', (408, 855), 36)
    label(card, 'BEST 7 / PAR 5', (562, 855), 30)
    for x, species in zip([340, 440, 540, 640, 740], ['sunny', 'sky', 'berry', 'mint', 'cocoa']):
        icon(card, 'disc_cream', (x, 962), 80)
        face = ImageOps.fit(portrait(species), (68, 68), RESAMPLE)
        mask = Image.new('L', face.size)
        ImageDraw.Draw(mask).ellipse((0, 0, 67, 67), fill=255)
        face.putalpha(Image.fromarray(np.minimum(np.array(face.getchannel('A')), np.array(mask))))
        card.alpha_composite(face, (x - 34, 928))
    stamp(card, read(ART / 'bird_sky_sleep.png'), (485, 1060), (80, 80))
    stamp(card, read(ART / 'zzz.png'), (511, 1030), (28, 28))
    stamp(card, read(ART / 'egg_idle.png'), (595, 1060), (64, 64))
    panel(card, (410, 1140, 260, 100), 'green')
    icon(card, 'play', (540, 1190), 64)
    card.save(MOCK / 'level_card.png')
    RECORD['level_card'] = {
        'panel': [160, 650, 760, 620], 'badge_frame': [504, 659, 72, 72],
        'close': [848, 658, 64, 64], 'stars': [[488, 772, 40], [540, 772, 40], [592, 772, 40]],
        'best_stars': 2, 'moves_chip': [370, 823, 340, 64], 'moves_icon_frame': [390, 837, 36, 36],
        'moves_text': 'BEST 7 / PAR 5', 'moves_font': 30, 'best_moves_is_placeholder': True,
        'species': ['sunny', 'sky', 'berry', 'mint', 'cocoa'], 'portrait_centers_x': [340, 440, 540, 640, 740],
        'portrait_y': 962, 'portrait_disc_size': 80, 'face_crop_size': 68,
        'sleeper_frame': [445, 1020, 80, 80], 'zzz_frame': [497, 1016, 28, 28], 'egg_frame': [563, 1028, 64, 64],
        'play_button': [410, 1140, 260, 100], 'play_icon_frame': [508, 1158, 64, 64],
        'level_12_source_note': 'Read-only current data has par 5 and these five species, with sleep but no eggs; egg icon is explicitly requested concept content, not a data change.',
    }


k3 = level(3)
k3.save(MOCK / 'level_k3_a.png')
shutil.copyfile(MOCK / 'level_k3_a.png', MOCK / 'level_k3.png')
# Existing B is an archived comparison; user selected A before map work.
if not (MOCK / 'level_k3_b.png').exists():
    level(3, 'bark').save(MOCK / 'level_k3_b.png')
level(7).save(MOCK / 'level_k7.png')
dialogs(k3)
icon_sheet()
map_and_card()
(QA / 'layout_measurements.json').write_text(json.dumps(RECORD, indent=2) + '\n')
hashes = {str(p.relative_to(ROOT)): hashlib.sha256(p.read_bytes()).hexdigest() for p in sorted(SOURCE_PATHS)}
(QA / 'source_hashes.json').write_text(json.dumps(hashes, indent=2) + '\n')
print('Composed', len(list(MOCK.glob('*.png'))), 'opaque 1080x1920 concept PNGs')
