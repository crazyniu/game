#!/usr/bin/env python3
"""Search the isolated Core-backed tool; never write the runtime resource."""
import argparse
import json
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
WORK = ROOT / 'tools/levels/work'
DOTNET = '/tmp/perch-t32-sdk/dotnet'
TOOL = WORK / 'DeepLevelTool/bin/Release/net8.0/V5LevelTool.dll'
ROLES = ['T','T','T','R','R','T','P','C','R','C','T','P','P','R','T','P','C','R','C','C','T','P','P','T','C','P','C','R','C','C','T','P','T','P','R','C','P','C','R','C']


def band(level_id):
    role = ROLES[level_id - 1]
    if level_id < 7 or role == 'T':
        return (0, 999)
    if role == 'R':
        return (3, 6)
    if role == 'P':
        return (8, 999) if level_id >= 21 else (6, 999)
    return (16, 999) if level_id == 40 else (12, 999) if level_id >= 21 else (10, 999)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('world', type=int)
    parser.add_argument('--attempts', type=int, default=200)
    parser.add_argument('--swaps', type=int, nargs='+', default=[2, 4, 6])
    parser.add_argument('--only', type=int, nargs='+')
    args = parser.parse_args()
    catalog = json.loads((WORK / 't35-levels.json').read_text())
    summary = []
    for level in catalog['levels']:
        level_id = level['id']
        if (level_id - 1) // 10 + 1 != args.world or (args.only and level_id not in args.only):
            continue
        low, high = band(level_id)
        if low <= level['par'] <= high:
            summary.append({'Id': level_id, 'Status': 'unchanged-in-band', 'Par': level['par']})
            continue
        species = sorted({token.split(':')[-1] if token.startswith('egg:') else token.split(':')[0]
                          for branch in level['branches'] for token in branch['birds']} - {'crow'})
        mechanics = ['gifts']
        tokens = [t for b in level['branches'] for t in b['birds']]
        for name, present in [('eggs', any(t.startswith('egg:') for t in tokens)),
                              ('sleep', any(':sleep' in t for t in tokens)),
                              ('mama', any(':mama' in t for t in tokens)),
                              ('crow', 'crow' in tokens),
                              ('swivel', any(b.get('kind') == 'swivel' for b in level['branches'])),
                              ('growing', any(b.get('kind') == 'growing' for b in level['branches'])),
                              ('daynight', level.get('dayLength', 0) > 0)]:
            if present:
                mechanics.append(name)
        selected = None
        best_par = level['par']
        runs = []
        for swaps in args.swaps:
            name = f'w{args.world}-l{level_id}-s{swaps}'
            spec = {'Id': level_id, 'K': len(species), 'Spare': len(level['branches']) - len(species),
                    'Species': species, 'Mechanics': mechanics, 'Role': ROLES[level_id - 1],
                    'BaseLevel': level, 'Swaps': swaps, 'Seed': 420000 + level_id * 100 + swaps,
                    'Attempts': args.attempts, 'Keep': 1,
                    'Limits': {'MaxStates': 2000000, 'Milliseconds': 10000, 'MemoryMiB': 2048}}
            spec_path = WORK / (name + '-spec.json')
            result_path = WORK / (name + '-result.json')
            spec_path.write_text(json.dumps(spec, indent=2) + '\n')
            command = [DOTNET, str(TOOL), 'generate', '--spec', str(spec_path), '--out', str(result_path)]
            print('RUN', name, flush=True)
            with (WORK / (name + '.log')).open('w') as output:
                completed = subprocess.run(command, cwd=ROOT, stdout=output, stderr=subprocess.STDOUT)
            if not result_path.exists():
                raise RuntimeError(f'{name} exited {completed.returncode} without evidence')
            result = json.loads(result_path.read_text())
            runs.append({'Name': name, 'Exit': completed.returncode,
                         **{k: v for k, v in result.items() if k != 'Candidates'}})
            best_par = max(best_par, result.get('BestPar') or 0)
            passing = [c for c in result['Candidates'] if c['Evidence']['Pass']]
            print('RESULT', name, completed.returncode, 'best', result.get('BestPar'),
                  'failures', [c['Evidence']['Failures'] for c in result['Candidates']], flush=True)
            if passing:
                selected = passing[0]
                (WORK / f'accepted-l{level_id}.json').write_text(json.dumps(selected, indent=2) + '\n')
                break
        summary.append({'Id': level_id, 'Status': 'accepted' if selected else 'not-found',
                        'OldPar': level['par'], 'BestPar': best_par,
                        'NewPar': selected['Level']['par'] if selected else level['par'], 'Runs': runs})
        (WORK / f'world{args.world}-search.json').write_text(json.dumps(summary, indent=2) + '\n')
    (WORK / f'world{args.world}-search.json').write_text(json.dumps(summary, indent=2) + '\n')
    print(json.dumps(summary, indent=2), flush=True)


if __name__ == '__main__':
    main()
