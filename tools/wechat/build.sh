#!/usr/bin/env bash
# Create the WeChat Developer Tools package; never uploads or publishes it.
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
creator="${COCOS_CREATOR:-/Applications/Cocos/Creator/3.8.8/CocosCreator.app/Contents/MacOS/CocosCreator}"
cd "$root"
[[ -d PerchSort/node_modules ]] || npm --prefix PerchSort ci --no-audit --no-fund
python3 tools/cocos-meta.py
node tools/wechat/prepare.mjs
code=0
"$creator" --project "$root/PerchSort" --build "configPath=$root/build/wechat-build-config.json" > build/wechat-build.log 2>&1 || code=$?
if [[ "$code" != 36 && "$code" != 0 ]]; then tail -60 build/wechat-build.log; exit "$code"; fi
node tools/wechat/finalize.mjs
