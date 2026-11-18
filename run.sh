#!/usr/bin/env bash
# Build and play the single Cocos project. Existing saves are never topped up or reset.
set -euo pipefail
cd "$(dirname "$0")"
creator="${COCOS_CREATOR:-/Applications/Cocos/Creator/3.8.8/CocosCreator.app/Contents/MacOS/CocosCreator}"
port="${WEB_PORT:-8765}"
command="${1:-play}"
case "$command" in
  --help|help)
    cat <<'HELP'
./run.sh [play|serve] [--level=N]       Build/play, or serve an existing web build
./run.sh build | editor | test          Build web, open Creator, or run core tests
./run.sh wechat-build                   Create a WeChat Developer Tools project
./run.sh ios-build [sim|dev|ipa]        Export Xcode, simulator, device or signed IPA
./run.sh android-build [project|apk|aab] Export Gradle, debug APK or release bundle
./run.sh native-apply | native-icons    Prepare native platform settings or artwork
HELP
    exit 0 ;;
  ios-build) shift; exec bash tools/native/build.sh ios "$@" ;;
  android-build) shift; exec bash tools/native/build.sh android "$@" ;;
  wechat-build) shift; exec bash tools/wechat/build.sh "$@" ;;
  native-apply) shift; exec node tools/native/apply.mjs "$@" ;;
  native-icons) shift; exec node tools/native/icons.mjs "$@" ;;
  editor) exec "$creator" --project "$PWD/PerchSort" ;;
  test) npm --prefix PerchSort test; exit ;;
  play|serve|build|--level=*) ;;
  *) echo "Unknown command: $command. See ./run.sh help." >&2; exit 2 ;;
esac
if [[ "$command" != serve ]]; then
  [[ -d PerchSort/node_modules ]] || npm --prefix PerchSort install --no-audit --no-fund
  python3 tools/cocos-meta.py
  mkdir -p build
  code=0
  "$creator" --project "$PWD/PerchSort" --build "configPath=$PWD/PerchSort/build-config/web-mobile.json" > build/cocos-build.log 2>&1 || code=$?
  if [[ "$code" != 36 && "$code" != 0 ]]; then tail -60 build/cocos-build.log; exit "$code"; fi
fi
[[ "$command" != build ]] || exit 0
query=''
for arg in "$@"; do case "$arg" in --level=*) query="?level=${arg#--level=}";; esac; done
url="http://127.0.0.1:$port/$query"
if curl --silent --fail "http://127.0.0.1:$port/__profile" >/dev/null; then
  echo "$url"
else
  python3 scripts/serve.py --port "$port" > build/cocos-server.log 2>&1 &
  echo "$!" > build/cocos-server.pid
  echo "$url"
fi
if [[ "${NO_OPEN:-0}" != 1 ]]; then
  case "$(uname -s)" in Darwin) open "$url";; Linux) xdg-open "$url";; esac
fi
