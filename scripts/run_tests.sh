#!/usr/bin/env bash
# Run the requested Cocos source/data checks; no browser or build is implied.
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
if [[ ! -d "$root/PerchSort/node_modules" ]]; then
  echo 'Install dependencies first: npm --prefix PerchSort ci' >&2
  exit 2
fi
python3 "$root/ci/check_current_game_names.py" --self-test
python3 "$root/ci/check_current_game_names.py"
python3 "$root/ci/check_screen_text.py"
npm --prefix "$root/PerchSort" test
if [[ -f "$root/PerchSort/temp/declarations/cc.d.ts" ]]; then
  npm --prefix "$root/PerchSort" run typecheck
else
  "$root/PerchSort/node_modules/.bin/tsc" -p "$root/PerchSort/tsconfig.core.json"
  echo 'Pure core/meta typecheck passed. Full Cocos typecheck was not run: import the project in Creator to generate temp declarations.'
fi
