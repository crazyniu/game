#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../../.." && pwd)"
export GODOT="${GODOT:-godot}"
for family in new_friend world_unlock; do
  bash "$root/docs/work-log/dialog-consistency/evidence/capture-runtime.sh" --family-filter="$family"
  cp "$root/build/dialog-consistency/gui.log" "$root/build/dialog-consistency/$family.log"
done
