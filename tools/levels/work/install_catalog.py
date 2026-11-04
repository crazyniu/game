#!/usr/bin/env python3
"""Install only after complete proof and preservation checks have succeeded."""
import hashlib
import json
import os
from pathlib import Path

WORK = Path(__file__).resolve().parent
ROOT = WORK.parents[2]
RESOURCE = ROOT / 'PerchSort/Assets/Resources/Levels/levels.json'
PROTECTED = {1, 2, 3, 4, 5, 6, 11, 15, 21, 24, 31, 33}


def evidence(name):
    report = json.loads((WORK / name).read_text())
    if report['CatalogFailures'] or [r['Id'] for r in report['Levels']] != list(range(1, 41)):
        raise RuntimeError(f'{name}: incomplete or invalid catalog')
    return report


def main():
    baseline_bytes = (WORK / 't35-levels.json').read_bytes()
    staged_bytes = (WORK / 'staged-levels.json').read_bytes()
    baseline = {l['id']: l for l in json.loads(baseline_bytes)['levels']}
    staged = {l['id']: l for l in json.loads(staged_bytes)['levels']}
    for level_id in PROTECTED:
        if staged[level_id] != baseline[level_id]:
            raise RuntimeError(f'Protected L{level_id} changed')
    fast = json.loads((WORK / 'final-fast-screen.json').read_text())
    replaced = {n for n in range(1, 41) if staged[n] != baseline[n]}
    if fast['MaxStates'] != 200000 or fast['Milliseconds'] != 1000:
        raise RuntimeError('The strict solver screen budget changed')
    if {r['Id'] for r in fast['Levels']} != replaced or any(not r['Pass'] for r in fast['Levels']):
        raise RuntimeError('Every replacement must pass the strict 1000 ms Core solver screen')
    for name in ('staged-deep-evidence.json', 'live-deep-evidence.json'):
        report = evidence(name)
        if report['GateSet'] != 'DEEP':
            raise RuntimeError(f'{name}: wrong gates')
        for row in report['Levels']:
            if not row['DataCheck'].startswith('Proven '):
                raise RuntimeError(f'{name}: data proof missing at L{row["Id"]}')
            if row['Failures']:
                raise RuntimeError(f'{name}: L{row["Id"]} failed: {row["Failures"]}')
    expected = hashlib.sha256(baseline_bytes).hexdigest()
    prior_install = WORK / 'install-result.json'
    if prior_install.exists():
        expected = json.loads(prior_install.read_text())['Sha256']
    if hashlib.sha256(RESOURCE.read_bytes()).hexdigest() != expected:
        raise RuntimeError('Runtime catalog differs from baseline or recorded T42 installation; do not overwrite concurrent work')
    temporary = WORK / 'validated-install.json'
    temporary.write_bytes(staged_bytes)
    os.replace(temporary, RESOURCE)
    result = {'Sha256': hashlib.sha256(staged_bytes).hexdigest(),
              'ChangedIds': [n for n in range(1, 41) if staged[n] != baseline[n]],
              'ProtectedIdsUnchanged': sorted(PROTECTED)}
    (WORK / 'install-result.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(result, indent=2))


if __name__ == '__main__':
    main()
