"""Compare capture-time rendered map rectangles with the authored top-left layout."""
import argparse
import csv
import json
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument('--layout', type=Path, default=Path('docs/design/layout/map.json'))
parser.add_argument('--captures', type=Path, default=Path('build/screens/states'))
args = parser.parse_args()
elements = json.loads(args.layout.read_text())['elements']
fixed = {e['id'] for e in elements if e['z'] >= 30 or e['id'] == 'forest' or 10 <= e['z'] <= 13}
reference_states = {'Forest_trail', 'Maximum_coins', 'Mixed_cleared_stars', 'Disabled_navigation'}
failures = 0
for path in sorted(args.captures.glob('Map/*.rects.tsv')) + sorted(args.captures.glob('Real_game/Map.rects.tsv')):
    lines = path.read_text().splitlines()
    rows = {r['element']: r for r in csv.DictReader((line for line in lines if not line.startswith('#')), delimiter='\t')}
    exact_scene = path.parent.name == 'Map' and path.name.removesuffix('.rects.tsv') in reference_states
    print(f'\n{path.parent.name}/{path.name.removesuffix(".rects.tsv")} ({"full reference" if exact_scene else "fixed HUD; progression differs"})')
    maximum = 0.0
    compared = 0
    for e in elements:
        key = e['id']
        comparable = exact_scene or key in fixed
        r = rows.get(key)
        if r is None or r['visible'] != 'True':
            print(f'{key:18} absent or outside viewport')
            failures += int(comparable)
            continue
        actual = [float(r[k]) for k in ('x', 'y', 'width', 'height')]
        delta = max(abs(a - b) for a, b in zip(actual, e['rect']))
        if comparable:
            compared += 1
            maximum = max(maximum, delta)
            failures += int(delta > 20)
        print(f'{key:18} max={delta:7.2f}px  actual={actual}  {"PASS" if delta <= 20 else "FAIL" if comparable else "STATE OFFSET"}')
    print(f'COMPARABLE MAX {maximum:.2f}px; compared={compared}/{len(elements) if exact_scene else len(fixed)}')
    print(next(line for line in lines if line.startswith('# scroll')))
raise SystemExit(bool(failures))
