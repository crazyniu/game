#!/usr/bin/env bash
set -euo pipefail
repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
unity_editor="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity}"
mkdir -p "$repo_root/build"
# Build first so PlayMode tests exercise the same serialized scene as the player.
if ! "$unity_editor" -batchmode -nographics -quit -projectPath "$repo_root/PerchSort" \
    -executeMethod PerchSort.Editor.GameSceneBuilder.Build -logFile "$repo_root/build/scene-build.log"; then
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
# Retained source inventory: 237 game + 53 Meta + 14 Audio; 17 PlayMode.
# These are discovery safeguards, not a historical passing result. Added tests may increase them.
minimum = 304 if sys.argv[2] == 'EditMode' else 17
cases = list(root.iter('test-case'))
if (root.attrib['result'] != 'Passed' or failed or passed != total or passed < minimum
        or len(cases) != total or any(case.attrib.get('result') != 'Passed' for case in cases)):
    sys.exit('Incomplete, skipped, failed, or unexpectedly reduced test discovery.')
if sys.argv[2] == 'EditMode':
    expected = {
        'PerchSort.EditModeTests.': 237,
        'PerchSort.Meta.EditModeTests.': 53,
        'PerchSort.Tests.Audio.': 14,
        'PerchSort.EditModeTests.LevelsDataTests.': 81,
        'PerchSort.EditModeTests.LayoutTests.': 41,
    }
else:
    expected = {'PerchSort.PlayModeTests.': 17}
for prefix, count in expected.items():
    found = sum(case.attrib.get('fullname', '').startswith(prefix) for case in cases)
    if found < count:
        sys.exit(f'{prefix}: expected at least {count} retained cases, discovered {found}')
PY
done
