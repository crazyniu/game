#!/usr/bin/env bash
set -euo pipefail
repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
unity_editor="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity}"
mkdir -p "$repo_root/build/screens/aspect"
if ! "$unity_editor" -batchmode -projectPath "$repo_root/PerchSort" \
    -executeMethod Screenshots.CaptureAspect -screen-width 1080 -screen-height 1920 \
    -logFile "$repo_root/build/screenshots-aspect.log"; then
    tail -n 80 "$repo_root/build/screenshots-aspect.log"
    exit 1
fi
python3 - "$repo_root/build/screens/aspect" <<'PY'
import pathlib, struct, sys
for width, height in ((360, 640), (1080, 1920), (1080, 2340), (1440, 1920)):
    for page in ('menu', 'game', 'settings', 'levels', 'tutorial'):
        path = pathlib.Path(sys.argv[1]) / f'{width}x{height}-{page}.png'
        data = path.read_bytes()
        assert data[:8] == b'\x89PNG\r\n\x1a\n' and struct.unpack('>II', data[16:24]) == (width, height), path
        print(path)
PY
