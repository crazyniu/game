#!/usr/bin/env bash
set -euo pipefail
repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
exec "${GODOT_BIN:-godot}" --path "$repo_root/godot" "$@"
