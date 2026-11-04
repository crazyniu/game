#!/usr/bin/env bash
set -euo pipefail
repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
unity_editor="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity}"
mkdir -p "$repo_root/build/screens/game"
# Stable-state capture only; animated acceptance is a separate coordinator-owned GUI pass.
if ! "$unity_editor" -batchmode -projectPath "$repo_root/PerchSort" \
    -executeMethod PerchSort.Editor.GameCapture.Run -screen-width 1080 -screen-height 1920 \
    -logFile "$repo_root/build/screenshots.log"; then
    tail -n 80 "$repo_root/build/screenshots.log"
    exit 1
fi
python3 - "$repo_root/build/screens/game" <<'CHECK'
import pathlib, struct, sys
folder = pathlib.Path(sys.argv[1])
names = ['map', 'map_test_mode', 'shop_placeholder', 'aviary_placeholder', 'level_card',
         'level1_start', 'level1_after_move1', 'level1_win', 'win', 'level_k3', 'settings',
         'goal_info', 'unlock_card', 'popup_new_friend', 'popup_daily_gift',
         'popup_world_complete', 'popup_world_unlock', 'level_k7']
names += [f'map_gate_w{world}' for world in range(2, 5)]
names += [f'map_w{world}' for world in range(2, 5)]
names += [f'world_w{world}' for world in range(1, 5)]
names += ['mech_' + name for name in ('eggs', 'sleep', 'crow_twig', 'swivel', 'growing', 'night')]
for name in names:
    path = folder / (name + '.png')
    data = path.read_bytes()
    assert data[:8] == b'\x89PNG\r\n\x1a\n' and struct.unpack('>II', data[16:24]) == (1080, 1920), path
    print(path)
CHECK
