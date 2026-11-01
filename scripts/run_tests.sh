#!/usr/bin/env bash
# Active Godot validation. Legacy Unity checks live in legacy_unity/.
set -euo pipefail
repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
godot_bin="${GODOT_BIN:-godot}"
test_home="$(mktemp -d "${TMPDIR:-/tmp}/perch-godot-tests.XXXXXX")"
trap 'rm -rf "$test_home"' EXIT
export XDG_DATA_HOME="$test_home/data" XDG_CONFIG_HOME="$test_home/config" XDG_CACHE_HOME="$test_home/cache"
mkdir -p "$XDG_DATA_HOME" "$XDG_CONFIG_HOME" "$XDG_CACHE_HOME" "$repo_root/build/godot-tests"
"$godot_bin" --version
"$godot_bin" --headless --path "$repo_root/godot" --editor --import --quit 2>&1 | tee "$repo_root/build/godot-tests/import.log"
if grep -Eq 'SCRIPT ERROR:|Parse Error:|Failed to load script' "$repo_root/build/godot-tests/import.log"; then exit 1; fi
for suite in core save scene; do
  log="$repo_root/build/godot-tests/$suite.log"
  "$godot_bin" --headless --path "$repo_root/godot" --script "res://tests/test_$suite.gd" 2>&1 | tee "$log"
  # The engine can exit zero after script parse errors; that is not a pass.
  if grep -Eq 'SCRIPT ERROR:|Parse Error:|Failed to load script' "$log"; then exit 1; fi
done
