#!/usr/bin/env python3
"""Compare live capture mesh/RectTransform bounds with the measured HUD layouts.

Run with uv run --with pillow python <this-file> build/screens/board-hud/*/unity.log.
The capture-only HudView logger supplies transformed mesh rectangles. Alpha bounds
come from the existing sprite files; portrait masks clip the larger animated birds.
No screenshot pixels are substituted, and absent/state-dependent elements stay explicit.
"""
import argparse
import json
import re
from functools import lru_cache
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[4]
OUTPUT = Path(__file__).resolve().parent
LAYOUT = ROOT / 'docs/design/layout'
SCREENS = ['board_base', 'board_stuck_hint', 'board_sleeping_bubble', 'board_many_branches']

# All IDs in each supplied JSON are listed in layout-map.md, including unowned art.
GROUPS = {
    'coin_pill': ['coin_pill'], 'coin_disc': ['coin_disc', 'coin'],
    'coin_value': ['coin_value', 'coins'], 'move_pill': ['move_pill', 'moves_pill'],
    'footprints': ['footprints', 'move_feet', 'footprint'],
    'move_value': ['move_value', 'moves_value', 'moves'],
    'move_divider': ['move_divider', 'moves_divider'],
    'sun_pill': ['sun_pill', 'extra_pill'], 'sun_icon': ['sun_icon', 'extra_sun', 'sun'],
    'sun_value': ['sun_value', 'extra_value', 'sun_count'],
    'settings': ['settings'], 'settings_gear': ['settings_gear', 'gear'],
    'goal_rope': ['goal_rope', 'house_rope', 'rope'], 'goal_house': ['goal_house'],
    'tools_bar': ['tools_bar', 'tool_bar', 'tools'],
    'tool_undo': ['tool_undo', 'undo'], 'tool_bulb': ['tool_bulb', 'tool_hint', 'hint'],
    'tool_branch': ['tool_branch', 'branch', 'branch_plus'],
    'tool_icon_undo': ['tool_icon_undo', 'undo_glyph', 'undo_icon'],
    'tool_icon_bulb': ['tool_icon_bulb', 'hint_bulb', 'hint_glyph', 'hint_icon'],
    'tool_icon_branch': ['tool_icon_branch', 'branch_glyph', 'branch_plus_icon'],
    'undo_glow': ['undo_glow'], 'undo_pointer': ['undo_pointer'],
}
for i in range(1, 4):
    GROUPS[f'star_{i}'] = [f'move_star_{i}', f'star_{i}', f'threshold_star_{i}']
    GROUPS[f'threshold_{i}'] = [f'threshold_{i}']
for color in ['yellow', 'blue', 'owl']:
    GROUPS[f'ring_{color}'] = [f'goal_ring_{color}', f'goal_{color}']
    GROUPS[f'bird_{color}'] = [f'goal_bird_{color}', f'goal_{color}_bird', f'portrait_{color}']
    GROUPS[f'gift_{color}'] = [f'goal_badge_{color}', f'goal_{color}_gift', f'gift_badge_{color}', f'gift_{color}']
ALIASES = {alias: key for key, aliases in GROUPS.items() for alias in aliases}
PATHS = {
    'coin_pill': 'Board controls/Coin pill', 'coin_disc': 'Board controls/Coin disc',
    'coin_value': 'Board controls/Coins *',
    'move_pill': 'Board controls/Move pill', 'footprints': 'Board controls/Painted footprints',
    'move_value': 'Board controls/Counted moves *',
    'move_divider': 'Board controls/Move divider/Painted divider',
    'sun_pill': 'Board controls/Day night counter/Phase pill',
    'sun_icon': 'Board controls/Day night counter/Sun moon',
    'sun_value': 'Board controls/Day night counter/Moves until phase',
    'settings': 'Board controls/Settings', 'settings_gear': 'Board controls/Settings/Icon',
    'goal_rope': 'Species goals/Single house rope/Rope artwork',
    'goal_house': 'Species goals/Single goal house/Birdhouse/House art crop/House body',
    'tools_bar': 'Board controls/Padded cream bar',
    'tool_undo': 'Board controls/Undo', 'tool_bulb': 'Board controls/Firefly',
    'tool_branch': 'Board controls/Sapling',
    'tool_icon_undo': 'Board controls/Undo/Icon', 'tool_icon_bulb': 'Board controls/Firefly/Icon',
    'tool_icon_branch': 'Board controls/Sapling/Icon',
    'undo_glow': 'Board controls/Recommended tool gold glow',
    'undo_pointer': 'Board controls/Recommended tool plush pointer',
}
for i in range(1, 4):
    PATHS[f'star_{i}'] = f'Board controls/Reachable star {i-1}'
    PATHS[f'threshold_{i}'] = f'Board controls/Star threshold {i-1}'
for color, species in [('yellow', 'sunny'), ('blue', 'sky'), ('owl', 'hoot')]:
    prefix = f'Species goals/Single goal house/{species} goal info/'
    PATHS[f'ring_{color}'] = prefix + 'Gold ring'
    PATHS[f'bird_{color}'] = prefix + 'Portrait crop/Front portrait'
    PATHS[f'gift_{color}'] = prefix + 'Species gift badge'

@lru_cache(None)
def alpha(sprite):
    if not sprite:
        return (0, 0, 1, 1)
    matches = list((ROOT/'PerchSort/Assets/Resources').rglob(sprite + '.png'))
    if not matches:
        return (0, 0, 1, 1)
    with Image.open(matches[0]) as image:
        box = image.convert('RGBA').getchannel('A').point(lambda p: 255 if p > 16 else 0).getbbox()
        x, y, r, b = box
        return x/image.width, y/image.height, (r-x)/image.width, (b-y)/image.height

def intersection(a, b):
    x, y = max(a[0], b[0]), max(a[1], b[1])
    return [x, y, max(0, min(a[0]+a[2], b[0]+b[2])-x), max(0, min(a[1]+a[3], b[1]+b[3])-y)]

def union(rects):
    x, y = min(r[0] for r in rects), min(r[1] for r in rects)
    return [x, y, max(r[0]+r[2] for r in rects)-x, max(r[1]+r[3] for r in rects)-y]

def visible(entry, entries):
    x, y, w, h = entry['rect']
    ax, ay, aw, ah = alpha(entry['sprite'])
    if entry['path'].endswith('Recommended tool plush pointer'):
        ax, ay, aw, ah = 1-ay-ah, ax, ah, aw  # clockwise quarter turn
    result = [x+ax*w, y+ay*h, aw*w, ah*h]
    if '/Portrait crop/' in entry['path']:
        mask = next(e['rect'] for e in entries if e['path'] == entry['path'].split('/Front portrait')[0])
        result = intersection(result, mask)
    if entry.get('clip'):
        result = intersection(result, entry['clip'])
    return result

def find_rect(key, entries):
    pattern = '^'+re.escape(PATHS[key]).replace(r'\*', '.*')+'$'
    found = [e for e in entries if e['active'] and (re.match(pattern, e['path']) or (key in ['settings', 'tool_undo', 'tool_bulb', 'tool_branch'] and e['path'] == PATHS[key]+'/Gold rim'))]
    return union([visible(e, entries) for e in found]) if found else None

def reference(state):
    if 'Sleeping' in state or 'Nocturnal' in state:
        return 'board_sleeping_bubble'
    if any(x in state for x in ['Many_branches', '5_rows', '7_species', '4_rows', 'Eleventh_branch', 'All_authored']):
        return 'board_many_branches'
    if any(x in state for x in ['Stuck', 'No_solution', 'Timeout', 'Failed_request', 'Pending']):
        return 'board_stuck_hint'
    return 'board_base'

def key_for(screen, element):
    if screen == 'board_stuck_hint' and element == 'branch_plus':
        return None  # White plus is baked into the branch glyph.
    if screen == 'board_sleeping_bubble' and element in ['gift_yellow', 'gift_blue', 'gift_owl']:
        return None  # Wrapped-gift detail is baked into the badge in the reference.
    return ALIASES.get(element)

def mapping():
    lines = ['# Board HUD element map', '', 'All rectangles are absolute top-left pixels on 1080 x 1920. Runtime anchors are top-left; anchoredPosition is (rect.center.x, -rect.center.y) relative to the parent. Image rectangles compensate existing transparent canvas padding.', '']
    for screen in SCREENS:
        lines += ['## '+screen, '', '| JSON element | Measured rect | Runtime object / scope |', '| --- | --- | --- |']
        for e in json.loads((LAYOUT/(screen+'.json')).read_text())['elements']:
            key = key_for(screen, e['id'])
            if key:
                target = PATHS[key]
                if screen == 'board_many_branches' and key == 'coin_pill': target += ' + Coin disc (compound bounds)'
            elif e['id'].startswith(('house_', 'coin_leaf', 'goal_gift', 'gift_icon')) or e['id'].endswith('_gift_icon') or (screen == 'board_sleeping_bubble' and e['id'] in ['gift_yellow', 'gift_blue', 'gift_owl']):
                target = 'Baked into existing sprite; no independent RectTransform'
            elif e['id'] == 'branch_plus': target = 'Baked into branch icon'
            elif e['id'] == 'pointer_shadow': target = 'Existing arrow outline; separate diffuse shadow art unavailable'
            else:
                target = 'Outside HUD ownership: board, birds, scenery or sleep bubble'
            lines += [f"| {e['id']} | {e['rect']} | {target} |"]
        lines += ['']
    (OUTPUT/'layout-map.md').write_text('\n'.join(lines))

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('logs', nargs='*', type=Path)
    parser.add_argument('--evidence', action='store_true', help='Write full-height side-by-side JPEGs from the matching capture folder.')
    args = parser.parse_args()
    mapping()
    states = {}
    for log in args.logs:
        for line in log.read_text().splitlines():
            if line.startswith('BOARD_HUD_RECTS|'):
                _, state, payload = line.split('|', 2)
                states[state] = (json.loads(payload)['elements'], log.parent)
    report = []
    for state, (entries, folder) in sorted(states.items()):
        screen = reference(state)
        results = []
        print(f'\n{state} -> {screen}')
        for e in json.loads((LAYOUT/(screen+'.json')).read_text())['elements']:
            key = key_for(screen, e['id'])
            if not key:
                continue
            actual = find_rect(key, entries)
            if screen == 'board_many_branches' and key == 'coin_pill' and actual:
                actual = union([actual, find_rect('coin_disc', entries)])
            if actual is None:
                print(f"{e['id']}: NOT VISIBLE")
                continue
            deviation = max(abs(a-b) for a,b in zip(actual,e['rect']))
            result = {'element': e['id'], 'expected': e['rect'], 'actual': [round(n, 2) for n in actual], 'max': round(deviation, 2)}
            # Keep the direct mockup error, then separately quantify the required
            # layout adaptation for a different species count or hidden controls.
            expected = list(e['rect'])
            goal_count = sum(e['active'] and e['path'].endswith('/Gold ring') for e in entries)
            tool_count = sum(e['active'] and e['path'] in [PATHS['tool_undo'], PATHS['tool_bulb'], PATHS['tool_branch']] for e in entries)
            if key.startswith(('ring_', 'bird_', 'gift_')):
                color = key.split('_', 1)[1]
                species = {'yellow': 'sunny', 'blue': 'sky', 'owl': 'hoot'}[color]
                goals = [e['path'].split('/')[-2].split(' ')[0] for e in entries if e['active'] and e['path'].endswith('/Gold ring')]
                index = goals.index(species)
                reference_index = ['yellow', 'blue', 'owl'].index(color)
                expected[0] += (index-(goal_count-1)/2-reference_index+1)*139.5
            if key.startswith('tool_'):
                slot = 0 if key.endswith('undo') else 1 if key.endswith('bulb') else 2
                live = [i for i, name in enumerate(['tool_undo', 'tool_bulb', 'tool_branch']) if find_rect(name, entries)]
                expected[0] += (live.index(slot)-(tool_count-1)/2-slot+1)*240
            if key in ['undo_glow', 'undo_pointer']:
                glow = find_rect('undo_glow', entries)
                live = [find_rect(name, entries) for name in ['tool_undo', 'tool_bulb', 'tool_branch']]
                target = min((r for r in live if r), key=lambda r: abs(r[0]+r[2]/2-glow[0]-glow[2]/2))
                expected[0] += target[0]+target[2]/2-301
            if key == 'tools_bar':
                expected[0] += (3-tool_count)*120
                expected[2] -= (3-tool_count)*240
            result['adapted_max'] = round(max(abs(a-b) for a,b in zip(actual,expected)), 2)
            results.append(result)
            print(f"{e['id']}: max={deviation:.2f}px actual={result['actual']}")
        maximum = max((r['max'] for r in results), default=0)
        adapted = max((r['adapted_max'] for r in results), default=0)
        print(f'MAX raw={maximum:.2f}px adapted={adapted:.2f}px')
        filename = state.removesuffix('.png').replace('/', '-').lower()+'.jpg'
        if args.evidence:
            with Image.open('/tmp/'+screen+'.png') as mockup, Image.open(folder/state) as capture:
                sheet = Image.new('RGB', (1440,1280))
                sheet.paste(mockup.convert('RGB').resize((720,1280),Image.Resampling.LANCZOS), (0,0))
                sheet.paste(capture.convert('RGB').resize((720,1280),Image.Resampling.LANCZOS), (720,0))
                sheet.save(OUTPUT/filename, quality=88)
        report.append({'state': state, 'mockup': screen, 'side_by_side': filename, 'max': maximum, 'adapted_max': adapted, 'elements': results})
    if states:
        (OUTPUT/'rect-results.json').write_text(json.dumps(report, indent=2)+'\n')

if __name__ == '__main__':
    main()
