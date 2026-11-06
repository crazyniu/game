#!/usr/bin/env python3
"""Compare actual Unity world-corner dumps with the approved top-left rectangles.

Usage: python3 docs/work-log/plush-ui-fixes/popup-frame/compare_rects.py build/screens/popup-frame
Alternatively pass this report's rect-input directory to replay the committed dumps.
No Unity invocation.
"""
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[4]
DOCS = Path(__file__).resolve().parent
MAP = json.loads((DOCS / 'element-map.json').read_text())

def groups(log):
    current = None
    pending = None
    for line in log.splitlines():
        if line.startswith('POPUP_RECTS_BEGIN\t'):
            current = {'screen': line.split('\t')[1], 'rects': {}}
        elif line.startswith('POPUP_RECT\t') and current is not None:
            _, _, name, values = line.split('\t')
            current['rects'][name] = [float(x) for x in values.split(',')]
        elif line.startswith('POPUP_RECTS_END\t'):
            pending = current
            current = None
        elif line.startswith('UiStateCapture: ') and '.png' in line and pending:
            filename = line.split(' ', 2)[2]
            yield filename, pending
            pending = None

def compare(folder):
    summary = []
    logs = list(folder.glob('*/unity.log')) + list(folder.glob('*.txt'))
    for log in sorted(logs):
        for capture, group in groups(log.read_text()):
            screen = group['screen']
            refs = json.loads((ROOT / 'docs/design/layout' / (screen + '.json')).read_text())['elements']
            errors = []
            print('\nSTATE\t' + capture + '\t' + screen)
            for element in refs:
                name = element['id']
                mapping = MAP[screen][name]
                targets = mapping.get('runtime', [])
                actual = []
                for target in targets:
                    actual.extend(rect for path, rect in group['rects'].items() if re.fullmatch(target, path))
                if not actual:
                    print(name + '\tN/A\t' + mapping['note'])
                    continue
                x = min(r[0] for r in actual); y = min(r[1] for r in actual)
                right = max(r[0] + r[2] for r in actual); bottom = max(r[1] + r[3] for r in actual)
                rect = [x, y, right - x, bottom - y]
                expected = mapping.get('target', element['rect'])
                error = max(abs(a - b) for a, b in zip(rect, expected))
                errors.append(error)
                print(f'{name}\t{error:.2f}px\tactual={rect}\ttarget={expected}\t' + mapping['note'])
            maximum = max(errors, default=0)
            print(f'MAX\t{maximum:.2f}px\t{len(errors)} measured elements')
            summary.append(dict(capture=capture, screen=screen, maximum=round(maximum, 2), measured=len(errors)))
    return summary

if __name__ == '__main__':
    result = compare(Path(sys.argv[1]))
    (DOCS / 'rect-summary.json').write_text(json.dumps(result, indent=2) + '\n')
