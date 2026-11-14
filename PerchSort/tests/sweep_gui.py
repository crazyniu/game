#!/usr/bin/env python3
"""Run the current standalone Godot checks on an existing graphical display.

Example: python3 PerchSort/tests/sweep_gui.py --output build/gui-sweep
Godot must be on PATH (or supplied with --godot); run under Xvfb in CI.
Artifacts and isolated per-test user data stay below the ignored output directory.
"""
import argparse
import csv
import json
import os
from pathlib import Path
import re
import subprocess
import time


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--godot', default=os.environ.get('GODOT', 'godot'))
    parser.add_argument('--output', type=Path, default=Path('build/gui-sweep'))
    parser.add_argument('--timeout', type=float, default=300)
    parser.add_argument('--repeat', type=int, default=1)
    parser.add_argument('tests', nargs='*', help='Optional test basenames; otherwise every run_*.gd and check_*.gd')
    args = parser.parse_args()
    if args.timeout <= 0 or args.repeat < 1:
        parser.error("Timeout and repeat count must be positive")
    project = Path(__file__).resolve().parents[1]
    output = args.output.resolve()
    output.mkdir(parents=True, exist_ok=True)
    files = sorted(p for p in (project / 'tests').glob('*.gd') if p.name.startswith(('run_', 'check_')))
    if args.tests:
        requested = {Path(name).stem for name in args.tests}
        files = [p for p in files if p.stem in requested]
        missing = requested - {p.stem for p in files}
        if missing:
            parser.error('Unknown tests: ' + ', '.join(sorted(missing)))
    version = subprocess.check_output([args.godot, '--version'], text=True).strip()
    rows = []
    for run in range(1, args.repeat + 1):
        for path in files:
            label = path.stem + (f'-run-{run}' if args.repeat > 1 else '')
            log_path = output / (label + '.log')
            cmd = [args.godot, '--path', str(project), '--audio-driver', 'Dummy', '--rendering-driver', 'opengl3', '--resolution', '1080x1920']
            if path.read_text().startswith('extends Node'):
                scene = path.with_suffix('.tscn')
                if not scene.exists():
                    parser.error('Node script has no standalone scene: ' + path.name)
                cmd.append('res://tests/' + scene.name)
            else:
                cmd.extend(['-s', 'res://tests/' + path.name])
            env = os.environ.copy()
            env['XDG_DATA_HOME'] = str(output / 'userdata' / label)
            env['XDG_CONFIG_HOME'] = str(output / 'config' / label)
            start = time.monotonic()
            with log_path.open('w') as log:
                try:
                    code = subprocess.run(cmd, env=env, stdout=log, stderr=subprocess.STDOUT, timeout=args.timeout).returncode
                except subprocess.TimeoutExpired:
                    code = 124
            contents = log_path.read_text()
            errors = re.findall(r'^.*(?:SCRIPT ERROR:|^FAIL |[1-9][0-9]* failures).*$' , contents, re.MULTILINE)
            row = {'test': path.name, 'run': run, 'seconds': round(time.monotonic() - start, 2), 'exit_code': code,
                   'result': 'PASS' if code == 0 and not errors else 'FAIL', 'detail': errors[0] if errors else ('timeout' if code == 124 else '')}
            rows.append(row)
            print(json.dumps(row), flush=True)
            (output / 'results.json').write_text(json.dumps({'godot': version, 'results': rows}, indent=2) + '\n')
            with (output / 'results.csv').open('w', newline='') as f:
                writer = csv.DictWriter(f, fieldnames=list(row))
                writer.writeheader()
                writer.writerows(rows)
    return int(any(row['result'] != 'PASS' for row in rows))


if __name__ == '__main__':
    raise SystemExit(main())
