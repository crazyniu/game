#!/usr/bin/env python3
"""Find cheaper-to-prove authored boards without changing the Core/test budgets."""
import argparse
import json
import subprocess
from pathlib import Path

WORK = Path(__file__).resolve().parent
ROOT = WORK.parents[2]
DOTNET = '/tmp/perch-t32-sdk/dotnet'
TOOL = WORK / 'LiveLevelTool/bin/Release/net8.0/LevelTool.dll'


def variants(level):
    empty = [i for i, b in enumerate(level['branches']) if not b['birds'] and b.get('kind', 'normal') == 'normal']
    if 21 <= level['id'] <= 30:
        for count in [4, 3, 2, 1]:
            for index in empty:
                changed = json.loads(json.dumps(level))
                changed['branches'][index]['birds'] = ['crow'] * count
                yield f'crow{count}b{index}', changed
    else:
        for index in empty:
            changed = json.loads(json.dumps(level))
            del changed['branches'][index]
            for i, branch in enumerate(changed['branches']):
                branch['row'] = i // 2
                branch['side'] = 'L' if i % 2 == 0 else 'R'
            yield f'remove{index}', changed
    yield 'original', level


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('ids', type=int, nargs='+')
    args = parser.parse_args()
    summary = []
    for level_id in args.ids:
        original = json.loads((WORK / f'accepted-l{level_id}.json').read_text())
        base = original['Level']
        mechanics = original['Evidence']['Mechanics']
        k = original['Evidence']['K']
        accepted = None
        runs = []
        for label, level in variants(base):
            for swaps in [0, 1, 2, 4]:
                name = f'fast-l{level_id}-{label}-s{swaps}'
                spec = {'Id': level_id, 'K': k, 'Spare': len(level['branches']) - k,
                        'Mechanics': mechanics, 'BaseLevel': level, 'Swaps': swaps,
                        'Seed': 425000 + level_id * 100 + swaps,
                        'Attempts': 1 if swaps == 0 else 120, 'Keep': 1,
                        'SolverMilliseconds': 1000,
                        'Limits': {'MaxStates': 2000000, 'Milliseconds': 10000, 'MemoryMiB': 2048}}
                spec_path = WORK / (name + '-spec.json')
                result_path = WORK / (name + '-result.json')
                spec_path.write_text(json.dumps(spec, indent=2) + '\n')
                print('RUN', name, flush=True)
                with (WORK / (name + '.log')).open('w') as log:
                    result = subprocess.run([DOTNET, str(TOOL), 'generate', '--spec', str(spec_path), '--out', str(result_path)], cwd=ROOT, stdout=log, stderr=subprocess.STDOUT)
                if not result_path.exists():
                    raise RuntimeError(f'{name} exited {result.returncode} without evidence')
                generated = json.loads(result_path.read_text())
                runs.append({'Name': name, 'Exit': result.returncode,
                             **{key: value for key, value in generated.items() if key != 'Candidates'}})
                passing = [c for c in generated['Candidates'] if c['Evidence']['Pass']]
                print('RESULT', name, 'best', generated.get('BestPar'),
                      'failures', [c['Evidence']['Failures'] for c in generated['Candidates']], flush=True)
                if passing:
                    accepted = passing[0]
                    (WORK / f'pre-fast-accepted-l{level_id}.json').write_text(json.dumps(original, indent=2) + '\n')
                    (WORK / f'accepted-l{level_id}.json').write_text(json.dumps(accepted, indent=2) + '\n')
                    break
            if accepted:
                break
        summary.append({'Id': level_id, 'Status': 'accepted' if accepted else 'not-found',
                        'OldDeepPar': base['par'], 'NewPar': accepted['Level']['par'] if accepted else None,
                        'Runs': runs})
        (WORK / 'fast-repair-summary.json').write_text(json.dumps(summary, indent=2) + '\n')
        if not accepted:
            raise RuntimeError(f'L{level_id} needs a deliberate below-floor fallback decision')
    print(json.dumps(summary, indent=2), flush=True)


if __name__ == '__main__':
    main()
