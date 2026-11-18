#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
creator="${COCOS_CREATOR:-/Applications/Cocos/Creator/3.8.8/CocosCreator.app/Contents/MacOS/CocosCreator}"
[[ -d PerchSort/node_modules ]] || npm --prefix PerchSort install --no-audit --no-fund
python3 tools/cocos-meta.py
mkdir -p build
code=0
"$creator" --project "$PWD/PerchSort" --build "configPath=$PWD/PerchSort/build-config/web-mobile.json" > build/cocos-build.log 2>&1 || code=$?
if [[ "$code" != 36 && "$code" != 0 ]]; then tail -60 build/cocos-build.log; exit "$code"; fi
echo "$PWD/PerchSort/build/web-mobile"
