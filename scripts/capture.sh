#!/usr/bin/env bash
# Silent screenshot of one game state: scripts/capture.sh <out.png> [game args...]
# e.g. scripts/capture.sh /tmp/a.png --level=3      scripts/capture.sh /tmp/b.png --preview=popups:win_three_stars
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
out="$1"; shift
timeout 120 xvfb-run -a -s "-screen 0 1280x2000x24" "${GODOT:-godot}" --path "$root/PerchSort" \
  --audio-driver Dummy --rendering-driver opengl3 --resolution 1080x1920 -- --capture="$out" "$@" 2>&1 \
  | grep -E 'SCRIPT ERROR|ERROR:|Parse Error' | grep -v vsync || true
test -f "$out"
