#!/usr/bin/env python3
"""Compare capture-time shop RectTransforms with the approved top-left pixel rects."""
import argparse
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[4]
OUT = Path(__file__).resolve().parent
CAPTURES = ROOT / 'build/screens/states'
PAGE = 'Shop page/'
HEADER = PAGE + 'Shop header/'
GRID = PAGE + 'Merchandise/'
COSMETICS = GRID + 'Cosmetic viewport/Cosmetic grid/'
DIALOG = 'Purchase dim/Purchase panel/'
BOOSTERS = dict(zip(('hint', 'branch', 'reveal', 'bell', 'shoo', 'rain'),
                    ('Firefly', 'Sapling', 'OwlEye', 'MorningBell', 'Scarecrow', 'SpringRain')))
OUTFITS = dict(zip(('straw', 'scarf', 'glasses', 'flower'),
                  ('outfit.sunny.1', 'outfit.sky.1', 'outfit.berry.1', 'outfit.mint.1')))
HOUSES = dict(zip(('mush', 'tea', 'lan', 'acorn'), ('mushroom', 'teapot', 'lantern', 'acorn')))


def mapping(screen, elements):
    paths = {}
    def put(path, *ids):
        for key in ids:
            paths[key] = ('rect', path)
    put(PAGE + 'Noon forest', 'background', 'bg', 'forest')
    put(HEADER + 'Close shop', 'close')
    put(HEADER + 'Close shop/Icon', 'close_x')
    if screen == 'shop_boosters':
        put(HEADER + 'Coin pill', 'balance')
        put(HEADER + 'Coin', 'balance_coin')
        put(HEADER + 'Coins', 'balance_value')
        put(HEADER + 'Selected tab glow', 'tab_glow')
        tabs = ('tab_boosters', 'tab_outfits', 'tab_home')
        glyphs = tuple(t + '_glyph' for t in tabs)
        for key, kind in BOOSTERS.items():
            card = GRID + kind + ' card'
            put(card, key + '_card', key + '_frame')
            for suffix, child in (('ribbon', 'Painted name ribbon'), ('ring', 'Gold rim'),
                                  ('price', 'Price pill'), ('coin', 'Price pill/Coin'),
                                  ('price_value', 'Price pill/Price'), ('owned', 'Owned badge'),
                                  ('owned_value', 'Owned badge/Owned')):
                put(card + '/' + child, key + '_' + suffix)
        illustrations = {
            'hint': ('hint_bulb', 'hint_rays'),
            'branch': ('branch_log', 'branch_sprout', 'branch_plus'),
            'reveal': ('reveal_eye', 'reveal_egg', 'reveal_nest'),
            'bell': ('bell_icon',), 'shoo': ('shoo_bird', 'shoo_x'),
            'rain': ('rain_cloud', 'rain_drop_left', 'rain_drop_middle', 'rain_drop_right', 'rain_sprout', 'rain_soil')}
        for key, ids in illustrations.items():
            for element in ids:
                paths[element] = ('rect' if len(ids) == 1 else 'baked', GRID + BOOSTERS[key] + ' card/Booster illustration')
    elif screen == 'shop_outfits':
        put(HEADER + 'Coin pill', 'hud_coin')
        put(HEADER + 'Coin', 'hud_leaf_coin')
        put(HEADER + 'Coins', 'hud_value')
        tabs = ('tab_light', 'tab_outfits', 'tab_home')
        glyphs = tuple(t + '_glyph' for t in tabs)
        for key, item in OUTFITS.items():
            card = COSMETICS + item
            put(card, 'card_' + key, key + '_frame')
            put(card + '/Painted name ribbon', key + '_ribbon')
            put(card + '/Cosmetic picture', key + '_bird')
            put(card + '/Ownership state/Price pill', key + '_price')
            put(card + '/Ownership state/Price pill/Coin', key + '_coin')
            put(card + '/Ownership state/Price pill/Price', key + '_amount')
        put(COSMETICS + 'outfit.mint.1/Ownership state/Wear outfit.mint.1', 'flower_wear')
        put(COSMETICS + 'outfit.mint.1/Ownership state/Wear outfit.mint.1/Icon', 'wear_hanger')
        put(COSMETICS + 'outfit.sky.1/Ownership state/Equipped check', 'equipped_badge')
        put(COSMETICS + 'outfit.sky.1 glow', 'equipped_glow')
    elif screen == 'shop_houses':
        put(HEADER + 'Coin pill', 'hud')
        put(HEADER + 'Coin', 'hud_coin')
        put(HEADER + 'Coins', 'hud_num')
        put(HEADER + 'Selected tab glow', 'selected_glow')
        tabs = ('tab_boost', 'tab_clothes', 'tab_houses')
        glyphs = ('icon_bulb', 'icon_hanger', 'icon_home')
        for key, style in HOUSES.items():
            card = COSMETICS + 'house.' + style
            put(card, key + '_card', key + '_frame')
            for suffix, child in (('ribbon', 'Painted name ribbon'), ('house', 'Cosmetic picture'),
                                  ('price', 'Ownership state/Price pill'), ('coin', 'Ownership state/Price pill/Coin'),
                                  ('num', 'Ownership state/Price pill/Price')):
                put(card + '/' + child, key + '_' + suffix)
    else:
        base = mapping('shop_boosters', json.loads((ROOT / 'docs/design/layout/shop_boosters.json').read_text())['elements'])
        aliases = {'hud': 'balance', 'hudcoin': 'balance_coin', 'hudnum': 'balance_value', 'pagex': 'close', 'pagexg': 'close_x',
                   'tabhint': 'tab_boosters', 'tabhintg': 'tab_boosters_glyph', 'tabward': 'tab_outfits', 'tabwardg': 'tab_outfits_glyph',
                   'tabhome': 'tab_home', 'tabhomeg': 'tab_home_glyph', 'selected': 'tab_selected',
                   'cardhint': 'hint_card', 'cardbranch': 'branch_card', 'cardmidl': 'reveal_card', 'cardmidr': 'bell_card',
                   'cardshoo': 'shoo_card', 'cardrain': 'rain_card', 'rh': 'hint_ribbon', 'rb': 'branch_ribbon',
                   'rs': 'shoo_ribbon', 'rr': 'rain_ribbon', 'bh': 'hint_owned', 'bhn': 'hint_owned_value',
                   'bb': 'branch_owned', 'bbn': 'branch_owned_value', 'bm': 'bell_owned', 'bmn': 'bell_owned_value',
                   'midlp': 'reveal_price', 'midlpc': 'reveal_coin', 'midlpn': 'reveal_price_value',
                   'midrp': 'bell_price', 'midrpc': 'bell_coin', 'midrpn': 'bell_price_value',
                   'shoop': 'shoo_price', 'shoopc': 'shoo_coin', 'shoopn': 'shoo_price_value',
                   'rainp': 'rain_price', 'rainpc': 'rain_coin', 'rainpn': 'rain_price_value',
                   'shooface': 'shoo_ring', 'rainface': 'rain_ring'}
        for key, target in aliases.items():
            paths[key] = ('backdrop', base[target][1])
        put('Purchase dim', 'dim')
        put(DIALOG.rstrip('/'), 'panel')
        paths['frame'] = ('shared', DIALOG + 'Panel surface')
        put(DIALOG + 'Wood disc', 'hintdisc')
        put(DIALOG + 'Gold rim', 'hintring')
        put(DIALOG + 'Booster illustration', 'bulb')
        put(DIALOG + 'Price pill', 'price')
        put(DIALOG + 'Price pill/Coin', 'pricecoin')
        put(DIALOG + 'Price pill/Price', 'pricenum')
        put(DIALOG + 'Painted name ribbon', 'ribbon')
        put(DIALOG + 'Cancel purchase', 'close')
        put(DIALOG + 'Cancel purchase/Icon', 'closeg')
        put(DIALOG + 'Confirm purchase', 'confirm')
        put(DIALOG + 'Confirm purchase/Icon', 'confirmg')
        tabs, glyphs = (), ()
    for tab, glyph, name in zip(tabs, glyphs, ('Boosters tab', 'Outfits tab', 'Birdhouses tab')):
        put(HEADER + name, tab)
        put(HEADER + name + '/Icon', glyph)
    if screen != 'shop_confirm':
        put(HEADER + 'Selected green pill', 'selected_pill' if screen == 'shop_houses' else 'tab_selected')
    by_id = {e['id']: e for e in elements}
    for element in elements:
        if element['id'] in paths:
            continue
        ancestor = element
        while ancestor['parent'] and ancestor['id'] not in paths:
            ancestor = by_id[ancestor['parent']]
        path = paths.get(ancestor['id'], ('baked', PAGE + 'Noon forest'))[1]
        paths[element['id']] = ('baked', path)
    return paths


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--pairs', action='store_true', help='Also write mockup/capture JPEG pairs at height 1280.')
    args = parser.parse_args()
    layouts = {s: json.loads((ROOT / 'docs/design/layout' / (s + '.json')).read_text())['elements']
               for s in ('shop_boosters', 'shop_outfits', 'shop_houses', 'shop_confirm')}
    maps = {s: mapping(s, elements) for s, elements in layouts.items()}
    table = ['# Element mapping', '', 'Every JSON entry is listed. Baked details share an existing image; they have no independent RectTransform. Shared popup art is outside this worker’s ownership.', '',
             '| Screen / element | Target x, y, w, h | Kind | Runtime object |', '| --- | --- | --- | --- |']
    for screen, elements in layouts.items():
        for e in elements:
            mode, path = maps[screen][e['id']]
            table.append(f"| {screen}/{e['id']} | {e['rect']} | {mode} | {path} |")
    (OUT / 'element-map.md').write_text('\n'.join(table) + '\n')
    states = {}
    for line in (CAPTURES / 'shop-rects.jsonl').read_text().splitlines():
        dump = json.loads(line); states[dump['state']] = dump
    summary = []
    lines = ['Max deviation = max(abs(actual - measured)) across x, y, width and height, in 1080x1920 design pixels.',
             'BAKED: no independent transform; see the mapped composite and side-by-side image.',
             'BACKDROP: actual shop page retained behind confirmation; the separately painted confirmation mockup differs.',
             'STATE: absent due to ownership, stock, category or catalog scroll; not a missing runtime object.']
    for state, dump in states.items():
        screen = 'shop_confirm' if state.startswith('Confirm') else 'shop_houses' if state.startswith('Houses') else 'shop_outfits' if state.startswith(('Outfits', 'Names')) else 'shop_boosters'
        rects = {r['path']: r['rect'] for r in dump['elements']}
        catalog_slots = {}
        if state.startswith('Names'):
            visible = sorted((r for r in dump['elements'] if r['path'].startswith(COSMETICS)
                              and '/' not in r['path'][len(COSMETICS):] and not r['path'].endswith(' glow')
                              and 400 <= r['rect'][1] <= 1741), key=lambda r: (r['rect'][1], r['rect'][0]))
            catalog_slots = {COSMETICS + item: row['path'] for item, row in zip(OUTFITS.values(), visible)}
        slug = re.sub('[^a-z0-9]+', '-', state.lower()).strip('-')
        capture = re.sub('[^A-Za-z0-9_-]+', '_', state).strip('_') + '.png'
        lines.extend(('', state + ' -> ' + screen))
        maximum = 0; failures = []; backdrop_max = 0
        for e in layouts[screen]:
            mode, path = maps[screen][e['id']]
            if mode in ('baked', 'effect'):
                lines.append(f"{e['id']}: {mode.upper()} max=N/A -> {path}")
                continue
            if state.startswith('Names'):
                for item in OUTFITS.values():
                    original = COSMETICS + item
                    if path == original or path.startswith(original + '/'):
                        replacement = catalog_slots.get(original)
                        path = replacement + path[len(original):] if replacement else 'Offscreen catalog slot'
                        break
            actual = rects.get(path)
            if actual is None:
                lines.append(f"{e['id']}: STATE max=N/A -> {path}")
                continue
            delta = max(abs(a-b) for a, b in zip(actual, e['rect']))
            lines.append(f"{e['id']}: {mode.upper()} max={delta:.2f}px measured={e['rect']} rendered={[round(v,2) for v in actual]}")
            if mode == 'backdrop':
                backdrop_max = max(backdrop_max, delta)
            else:
                maximum = max(maximum, delta)
                if delta > 20: failures.append((e['id'], round(delta, 2)))
        if args.pairs:
            from PIL import Image
            reference = Image.open('/tmp/' + screen + '.png').convert('RGB').resize((720, 1280), Image.Resampling.LANCZOS)
            rendered = Image.open(CAPTURES / 'Shop' / capture).convert('RGB').resize((720, 1280), Image.Resampling.LANCZOS)
            pair = Image.new('RGB', (1440, 1280)); pair.paste(reference, (0, 0)); pair.paste(rendered, (720, 0))
            pair.save(OUT / (slug + '.jpg'), quality=90)
        summary.append(dict(state=state, screen=screen, pair=slug+'.jpg', maximum=round(maximum,2), backdrop_max=round(backdrop_max,2), above_tolerance=failures))
        lines.append(f'STATE MAX {maximum:.2f}px; backdrop {backdrop_max:.2f}px; over tolerance {failures}')
    output = '\n'.join(lines) + '\n'
    (OUT / 'rect-deviations.txt').write_text(output)
    (OUT / 'rect-summary.json').write_text(json.dumps(summary, indent=2) + '\n')
    print(output)


if __name__ == '__main__':
    main()
