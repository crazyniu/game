#!/usr/bin/env bash
set -euo pipefail
repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
unity_editor="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity}"
mkdir -p "$repo_root/build"
"$unity_editor" -batchmode -nographics -quit -projectPath "$repo_root/PerchSort" \
    -executeMethod ReleaseSettings.Apply -logFile "$repo_root/build/release-settings.log"
