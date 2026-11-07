#!/usr/bin/env bash
# One-click launcher for Perch Sort.
#   ./run.sh                 open all 40 levels and top up to at least 50,000 coins
#   ./run.sh --level=12      start directly on level 12
#   ./run.sh --reset         delete saved progress, then apply the launcher defaults
#   ./run.sh --update        pull the latest main first (only if the checkout has no local changes)
# The original save is backed up once as profile.before-launcher-access.json. Requires Python 3.
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
boost_profile=true
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
      sed -n '2,8p' "${BASH_SOURCE[0]}" | sed 's/^# \{0,1\}//'
      exit 0 ;;
    --capture=*|--frames=*|--preview=*|--smoke|--autoplay|--profile=*)
      boost_profile=false
      game_args+=("$arg") ;;
    *) game_args+=("$arg") ;;
  esac
done

godot="$(find_godot)"
version="$("$godot" --version 2>/dev/null | head -1)"
[[ "$version" == 4.7.* ]] || echo "Warning: expected Godot 4.7.x, found ${version:-unknown}." >&2

# A plain game launch does not import new or changed art, so import whenever the project changed
# since the last import (new commit, local status, or file modification time). Without Git, always import.
stamp_file="$project/.godot/run_import_stamp"
stamp="$( { git -C "$root" rev-parse HEAD; git -C "$root" status --porcelain -- PerchSort; } 2>/dev/null | cksum || true)"
if [[ -z "$stamp" || ! -f "$stamp_file" || "$(cat "$stamp_file")" != "$stamp" ]] \
    || [[ -n "$(find "$project" -path "$project/.godot" -prune -o -type f -newer "$stamp_file" -print -quit)" ]]; then
  echo "Importing assets (the first time takes about a minute)..."
  "$godot" --headless --path "$project" --import >/dev/null 2>&1 || true
  [[ -n "$stamp" ]] && echo "$stamp" > "$stamp_file"
fi

if [[ "$boost_profile" == true ]]; then
  python3 - "$(save_dir)" <<'PY'
import json
from pathlib import Path
import sys
import tempfile

save_directory = Path(sys.argv[1])
save_directory.mkdir(parents=True, exist_ok=True)
profile_path = save_directory / "profile.json"
backup_path = save_directory / "profile.before-launcher-access.json"
if profile_path.exists():
    original = profile_path.read_bytes()
    profile = json.loads(original)
    if not backup_path.exists():
        with backup_path.open("xb") as backup:
            backup.write(original)
else:
    profile = {}

profile["coins"] = max(profile.get("coins", 0), 50000)
profile["highestUnlockedLevel"] = 40
# The game's existing unlock rule requires a star on each preceding level.
for field in ("bestStars", "clearCounts"):
    entries = profile.setdefault(field, [])
    entries.extend([0] * (41 - len(entries)))
    for level in range(1, 40):
        entries[level] = max(entries[level], 1)

with tempfile.NamedTemporaryFile(mode="w", encoding="utf-8", dir=save_directory,
                                 prefix=".profile-", suffix=".tmp", delete=False) as temporary:
    json.dump(profile, temporary, ensure_ascii=False, separators=(",", ":"))
Path(temporary.name).replace(profile_path)
PY
fi

# shellcheck disable=SC2086
exec "$godot" ${GODOT_FLAGS:-} --path "$project" -- ${game_args[@]+"${game_args[@]}"}
