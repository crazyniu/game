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
./run.sh [play|serve] [--level=N]       Open a dedicated portrait game window; serve reuses the web build
./run.sh [play|serve] --trial=1          Open partial-goal trial 1 (trials 1–3)
./run.sh build | editor | test          Build web, open Creator, or run core tests
./package.sh                            交互式选择打包平台
./run.sh native-apply | native-icons    Prepare native platform settings or artwork
HELP
    exit 0 ;;
  native-apply) shift; exec node tools/native/apply.mjs "$@" ;;
  native-icons) shift; exec node tools/native/icons.mjs "$@" ;;
  editor) exec "$creator" --project "$PWD/PerchSort" ;;
  test) npm --prefix PerchSort test; exit ;;
  play|serve|build|--level=*|--trial=*) ;;
  *) echo "Unknown command: $command. See ./run.sh help." >&2; exit 2 ;;
esac
if [[ "$command" != serve ]]; then
  bash scripts/build_web.sh
fi
[[ "$command" != build ]] || exit 0
query=''
for arg in "$@"; do
  case "$arg" in
    --level=*) query="${query}${query:+&}level=${arg#--level=}" ;;
    --trial=*) query="${query}${query:+&}trial=${arg#--trial=}" ;;
  esac
done
url="http://127.0.0.1:$port/${query:+?}$query"
server_url="http://127.0.0.1:$port"
mkdir -p build
if ! curl --silent --fail "$server_url/__profile" >/dev/null; then
  nohup python3 scripts/serve.py --port "$port" > build/cocos-server.log 2>&1 </dev/null &
  echo "$!" > build/cocos-server.pid
  for ((attempt=0; attempt<30; attempt++)); do
    if curl --silent --fail "$server_url/__profile" >/dev/null; then break; fi
    sleep 0.1
  done
  if [[ "$attempt" == 30 ]]; then
    echo "Local server did not start. See build/cocos-server.log." >&2
    exit 1
  fi
fi
echo "$url"
[[ "${NO_OPEN:-0}" != 1 ]] || exit 0

# App mode shares the same Cocos build as the web export. Keep its profile between launches.
case "$(uname -s)" in
  Darwin)
    profile="$HOME/Library/Application Support/Perch Sort/Player"
    candidates=(
      "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome"
      "$HOME/Applications/Google Chrome.app/Contents/MacOS/Google Chrome"
      "/Applications/Chromium.app/Contents/MacOS/Chromium"
      "/Applications/Microsoft Edge.app/Contents/MacOS/Microsoft Edge"
    )
    ;;
  *)
    profile="${XDG_DATA_HOME:-$HOME/.local/share}/perch-sort/player"
    candidates=(google-chrome chromium chromium-browser microsoft-edge)
    ;;
esac
if [[ -n "${PERCH_PLAYER_BROWSER:-}" ]]; then candidates=("$PERCH_PLAYER_BROWSER"); fi
for candidate in "${candidates[@]}"; do
  if player="$(command -v "$candidate")"; then
    nohup "$player" --app="$url" --window-size=450,832 \
      --user-data-dir="${PERCH_PLAYER_PROFILE:-$profile}" \
      --no-first-run --no-default-browser-check \
      > build/cocos-player.log 2>&1 </dev/null &
    exit 0
  fi
done
echo "Install Chrome, Chromium or Edge, or set PERCH_PLAYER_BROWSER to its executable." >&2
exit 1
