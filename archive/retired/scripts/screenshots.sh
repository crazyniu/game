#!/usr/bin/env bash
set -euo pipefail
repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
unity_editor="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity}"
mkdir -p "$repo_root/build/screens/flow"
rm -f "$repo_root/build/screens/flow"/flow_*.png "$repo_root/build/screens/flow"/anim_*.png "$repo_root/build/screens/flow"/anim_*_times.txt
if ! "$unity_editor" -batchmode -projectPath "$repo_root/PerchSort" \
    -executeMethod Screenshots.Capture -screen-width 1080 -screen-height 1920 \
    -logFile "$repo_root/build/screenshots.log"; then
    tail -n 80 "$repo_root/build/screenshots.log"
    exit 1
fi
python3 - "$repo_root/build/screens/flow" <<'CHECK'
import pathlib, struct, sys
files = sorted(pathlib.Path(sys.argv[1]).glob('flow_*.png'))
assert len(files) >= 30, len(files)
for path in files:
    data = path.read_bytes()
    assert data[:8] == b'\x89PNG\r\n\x1a\n' and struct.unpack('>II', data[16:24]) == (1080,1920), path
    print(path)
assert any('saved_resume' in f.name for f in files)
for name in ('menu_intro','popup_open','level_start','select_and_fly','homing','win_stars','blink'):
    assert len(list(pathlib.Path(sys.argv[1]).glob(f'anim_{name}_??.png'))) == 6, name
CHECK
