#!/usr/bin/env bash
# One-click launcher for Perch Sort.
#   ./run.sh                 start the game
#   ./run.sh --level=12      start directly on level 12
#   ./run.sh --reset         delete the saved progress, then start
#   ./run.sh --update        pull the latest main first (only if the checkout has no local changes)
# Godot is found via $GODOT, `godot` on PATH, or /Applications/Godot.app. Extra engine flags: GODOT_FLAGS="...".
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
project="$root/PerchSort"

find_godot() {
  if [[ -n "${GODOT:-}" ]]; then echo "$GODOT"; return; fi
  if command -v godot >/dev/null 2>&1; then command -v godot; return; fi
  for app in /Applications/Godot.app "$HOME/Applications/Godot.app"; do
    if [[ -x "$app/Contents/MacOS/Godot" ]]; then echo "$app/Contents/MacOS/Godot"; return; fi
  done
  echo "Godot 4.7.2 not found. Install it to /Applications/Godot.app or set GODOT=/path/to/godot." >&2
  exit 1
}

save_dir() {
  case "$(uname -s)" in
    Darwin) echo "$HOME/Library/Application Support/Godot/app_userdata/Perch Sort" ;;
    *) echo "${XDG_DATA_HOME:-$HOME/.local/share}/godot/app_userdata/Perch Sort" ;;
  esac
}

game_args=()
for arg in "$@"; do
  case "$arg" in
    --reset)
      rm -f "$(save_dir)/profile.json"
      echo "Saved progress deleted." ;;
    --update)
      if [[ -n "$(git -C "$root" status --porcelain)" ]]; then
        echo "Local changes present; skipping update." >&2
      else
        git -C "$root" pull --ff-only origin main
      fi ;;
    -h|--help)
      sed -n '2,7p' "${BASH_SOURCE[0]}" | sed 's/^# \{0,1\}//'
      exit 0 ;;
    *) game_args+=("$arg") ;;
  esac
done

godot="$(find_godot)"
version="$("$godot" --version 2>/dev/null | head -1)"
[[ "$version" == 4.7.* ]] || echo "Warning: expected Godot 4.7.x, found ${version:-unknown}." >&2

# A plain game launch does not import new or changed art, so import whenever the project changed
# since the last import (new commit or local edits under PerchSort/). Without Git, always import.
stamp_file="$project/.godot/run_import_stamp"
stamp="$( { git -C "$root" rev-parse HEAD; git -C "$root" status --porcelain -- PerchSort; } 2>/dev/null | cksum || true)"
if [[ -z "$stamp" || ! -f "$stamp_file" || "$(cat "$stamp_file")" != "$stamp" ]]; then
  echo "Importing assets (the first time takes about a minute)..."
  "$godot" --headless --path "$project" --import >/dev/null 2>&1 || true
  [[ -n "$stamp" ]] && echo "$stamp" > "$stamp_file"
fi

# shellcheck disable=SC2086
exec "$godot" ${GODOT_FLAGS:-} --path "$project" -- ${game_args[@]+"${game_args[@]}"}
