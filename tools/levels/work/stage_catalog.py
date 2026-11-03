#!/usr/bin/env python3
"""Stage accepted candidates and assert protected levels before validation."""
import hashlib
import json
from pathlib import Path

WORK = Path(__file__).resolve().parent
ROOT = WORK.parents[2]
PROTECTED = {1, 2, 3, 4, 5, 6, 11, 15, 21, 24, 31, 33}


def main():
    baseline_bytes = (WORK / 't35-levels.json').read_bytes()
    catalog = json.loads(baseline_bytes)
    original = {level['id']: level for level in catalog['levels']}
    replacements = {}
    for path in sorted(WORK.glob('accepted-l*.json')):
        candidate = json.loads(path.read_text())
        level = candidate['Level']
        level_id = level['id']
        if level_id in PROTECTED:
            raise RuntimeError(f'Attempt to change protected L{level_id}')
        if not candidate['Evidence']['Pass']:
            raise RuntimeError(f'L{level_id} candidate has failed gates')
        replacements[level_id] = level
    catalog['levels'] = [replacements.get(level['id'], level) for level in catalog['levels']]
    for level in catalog['levels']:
        if level['id'] in PROTECTED and level != original[level['id']]:
            raise RuntimeError(f'Protected L{level["id"]} changed')
    if [level['id'] for level in catalog['levels']] != list(range(1, 41)):
        raise RuntimeError('Catalog is not exactly IDs 1..40')
    (WORK / 'staged-levels.json').write_text(json.dumps(catalog, indent=2) + '\n')
    summary = {'ProtectedIds': sorted(PROTECTED), 'ReplacedIds': sorted(replacements),
               'BaselineSha256': hashlib.sha256(baseline_bytes).hexdigest(),
               'Changes': [{'Id': n, 'OldPar': original[n]['par'], 'NewPar': replacements[n]['par']}
                           for n in sorted(replacements)]}
    (WORK / 'stage-summary.json').write_text(json.dumps(summary, indent=2) + '\n')
    print(json.dumps(summary, indent=2))


if __name__ == '__main__':
    main()
