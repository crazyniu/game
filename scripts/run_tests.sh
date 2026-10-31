#!/usr/bin/env bash
set -euo pipefail
repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
unity_editor="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity}"
mkdir -p "$repo_root/build"
# Build first so PlayMode tests exercise the same serialized scene as the player.
if ! "$unity_editor" -batchmode -nographics -quit -projectPath "$repo_root/PerchSort" \
    -executeMethod SceneBuilder.Build -logFile "$repo_root/build/scene-build.log"; then
    tail -n 80 "$repo_root/build/scene-build.log"
    exit 1
fi
for platform in EditMode PlayMode; do
    if [ "$platform" = EditMode ]; then
        results="$repo_root/build/test-results.xml"
        log_file="$repo_root/build/editmode-tests.log"
    else
        results="$repo_root/build/playmode-results.xml"
        log_file="$repo_root/build/playmode-tests.log"
    fi
    rm -f "$results"
    if ! "$unity_editor" -batchmode -nographics -projectPath "$repo_root/PerchSort" \
        -runTests -testPlatform "$platform" -testResults "$results" -logFile "$log_file"; then
        tail -n 80 "$log_file"
        exit 1
    fi
    python3 - "$results" "$platform" <<'PY'
import sys
import xml.etree.ElementTree as ET
root = ET.parse(sys.argv[1]).getroot()
total, failed, passed = (int(root.attrib[k]) for k in ('total', 'failed', 'passed'))
print(f'{sys.argv[2]}: {passed}/{total} passed, {failed} failed')
print(f'XML: {sys.argv[1]}')
minimum = 101 if sys.argv[2] == 'EditMode' else 11
if root.attrib['result'] != 'Passed' or failed or passed + int(root.attrib.get('skipped', 0)) != total or passed < minimum:
    sys.exit(1)
PY
done
