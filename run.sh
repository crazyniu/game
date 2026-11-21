#!/usr/bin/env bash
# Single entry point for the Cocos project. Existing saves are never topped up or reset.
set -euo pipefail
cd "$(dirname "$0")"
ORIG_ARGS=("$@")
source tools/cocos/creator.sh
port="${WEB_PORT:-8765}"
lan_port="${LAN_PORT:-8770}"
cdp_port="${CDP_PORT:-9333}"

usage() {
  cat <<'HELP'
Play (Mac with Cocos Creator 3.8.8):
  ./run.sh [play] [options]       Build web-mobile, serve on localhost, open the portrait player window
  ./run.sh serve [options]        Same without rebuilding
  ./run.sh lan [serve] [options]  Build (lan serve: reuse the build) and serve on the local network for phones and
                                  other computers; prints http://<LAN IP>:8770/ and keeps running until Ctrl+C
  Options (combinable):
    level <N> | --level=N         Open level N on an in-memory copy of the save; nothing is written back
    auto [from] [to]              Autoplay real moves from level <from> (default: furthest unlocked) to <to>
                                  (default: 210) on an in-memory copy of the save
    page <name> [world]           Open map [world], shop, aviary, or the settings, daily, draw, chest popup
    --trial=N                     Open partial-goal trial N (1-3)
  ./run.sh build | editor         Build web-mobile only, or open Creator
  ./run.sh cdp logs [s] | eval '<expr>' | shot <file> [x,y,w,h] | burst <dir> <count> <ms> '<JS>' | watch [log]
                                  Console, evaluation, capture, timed frames, stall stacks in the player window
  ./run.sh clean                  Remove Creator caches (library, temp, build, local)
  ./package.sh                    Choose a platform package
  ./run.sh native-apply | native-icons   Prepare native platform settings or artwork

Develop (Linux or CI, Node 22+):
  ./run.sh check                  Same as CI: script syntax, metas, PNG compression, source guards, tests, types
  ./run.sh test                   Core tests only
  ./run.sh meta                   Write missing .meta files (commit them)
  ./run.sh art-compress           Lossless PNG recompression (pixels unchanged), then metas

play, serve, lan, build and editor first fast-forward the current branch (NO_PULL=1 skips); local changes are kept.
Environment: WEB_PORT (8765), LAN_PORT (8770), CDP_PORT (9333), COCOS_CREATOR, NO_OPEN=1, NO_PULL=1,
PERCH_PLAYER_BROWSER, PERCH_PLAYER_PROFILE.
HELP
}

# Fast-forward the current branch. Local changes are never discarded: a checkout may hold the user's own work.
PULLED_NEW=0
pull_latest() {
  [ "${NO_PULL:-}" = 1 ] && return 0
  local branch before
  branch="$(git symbolic-ref --short -q HEAD)" || { echo "(detached HEAD, not pulling)"; return 0; }
  git rev-parse -q --verify '@{u}' >/dev/null 2>&1 || { echo "(${branch} has no upstream, not pulling)"; return 0; }
  before="$(git rev-parse HEAD)"
  echo "Pulling ${branch}"
  if ! git pull --ff-only -q; then
    echo "Pull failed (offline, local commits, or conflicting local changes); continuing with the current code." >&2
    return 0
  fi
  if [ "$before" != "$(git rev-parse HEAD)" ]; then
    git log --oneline "$before..HEAD" | head -10 | sed 's/^/  /'
    PULLED_NEW=1
  fi
}

# Launch options -> query string read by Boot.ts.
query=''
add() { query="${query}${query:+&}$1"; }
parse_options() {
  while [ $# -gt 0 ]; do
    case "$1" in
      --level=*) add "level=${1#--level=}" ;;
      --trial=*) add "trial=${1#--trial=}" ;;
      level)
        [[ "${2:-}" =~ ^[0-9]+$ ]] || die "Usage: ./run.sh level <N>"
        add "level=$2"; shift ;;
      auto)
        add autoplay=1
        if [[ "${2:-}" =~ ^[0-9]+$ ]]; then add "level=$2"; shift; fi
        if [[ "${2:-}" =~ ^[0-9]+$ ]]; then add "autoTo=$2"; shift; fi ;;
      page)
        case "${2:-}" in
          map) add page=map; shift; if [[ "${2:-}" =~ ^[0-9]+$ ]]; then add "world=$2"; shift; fi; shift; continue ;;
          shop|aviary) add "page=$2" ;;
          settings|daily|draw|chest) add "popup=$2" ;;
          *) die "Usage: ./run.sh page map [world] | shop | aviary | settings | daily | draw | chest" ;;
        esac
        shift ;;
      *) die "Unknown option: $1. See ./run.sh help." ;;
    esac
    shift
  done
}

build_web() { bash scripts/build_web.sh; }

lan_ip() {
  local ip=''
  if command -v ipconfig >/dev/null 2>&1; then ip="$(ipconfig getifaddr en0 2>/dev/null || ipconfig getifaddr en1 2>/dev/null || true)"; fi
  if [ -z "$ip" ]; then ip="$(hostname -I 2>/dev/null | awk '{print $1}' || true)"; fi
  echo "${ip:-<LAN IP>}"
}

serve_lan() {
  local dir="$PROJECT/build/web-mobile"
  [ -f "$dir/index.html" ] || die "No web build yet: run ./run.sh lan"
  echo "http://$(lan_ip):${lan_port}/${query:+?}${query}  (same network; Ctrl+C stops)"
  exec node tools/web/serve.mjs "$dir" "$lan_port" 0.0.0.0
}

serve_local() {
  local dir="$PROJECT/build/web-mobile" server_url="http://127.0.0.1:$port" attempt
  [ -f "$dir/index.html" ] || die "No web build yet: run ./run.sh build"
  mkdir -p build
  if ! curl --silent --fail "$server_url/__profile" >/dev/null; then
    nohup node tools/web/serve.mjs "$dir" "$port" 127.0.0.1 > build/cocos-server.log 2>&1 </dev/null &
    echo "$!" > build/cocos-server.pid
    for ((attempt=0; attempt<30; attempt++)); do
      if curl --silent --fail "$server_url/__profile" >/dev/null; then break; fi
      sleep 0.1
    done
    [ "$attempt" != 30 ] || die "Local server did not start. See build/cocos-server.log."
  fi
}

open_player() {
  local url="$1" profile candidate player
  # App mode shares the web build. Keep its profile between launches. The debugging port serves ./run.sh cdp;
  # the last three flags keep the game running while the window is covered, so remote captures stay live.
  case "$(uname -s)" in
    Darwin)
      profile="$HOME/Library/Application Support/Perch Sort/Player"
      candidates=(
        "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome"
        "$HOME/Applications/Google Chrome.app/Contents/MacOS/Google Chrome"
        "/Applications/Chromium.app/Contents/MacOS/Chromium"
        "/Applications/Microsoft Edge.app/Contents/MacOS/Microsoft Edge"
      ) ;;
    *)
      profile="${XDG_DATA_HOME:-$HOME/.local/share}/perch-sort/player"
      candidates=(google-chrome chromium chromium-browser microsoft-edge) ;;
  esac
  if [ -n "${PERCH_PLAYER_BROWSER:-}" ]; then candidates=("$PERCH_PLAYER_BROWSER"); fi
  for candidate in "${candidates[@]}"; do
    if player="$(command -v "$candidate")"; then
      nohup "$player" --app="$url" --window-size=450,832 \
        --user-data-dir="${PERCH_PLAYER_PROFILE:-$profile}" --remote-debugging-port="$cdp_port" \
        --no-first-run --no-default-browser-check \
        --disable-backgrounding-occluded-windows --disable-renderer-backgrounding --disable-background-timer-throttling \
        > build/cocos-player.log 2>&1 </dev/null &
      return 0
    fi
  done
  die "Install Chrome, Chromium or Edge, or set PERCH_PLAYER_BROWSER to its executable."
}

lan=0
if [ "${1:-}" = lan ]; then lan=1; shift; fi
command="${1:-play}"
case "$command" in
  level|auto|page|--level=*|--trial=*) command=play ;;
  *) if [ $# -gt 0 ]; then shift; fi ;;
esac

case "$command" in
  play|serve|build|editor)
    if [ "${RUN_SH_PULLED:-}" != 1 ]; then
      pull_latest
      # run.sh itself may have changed: rerun the pulled version.
      if [ "$PULLED_NEW" = 1 ]; then RUN_SH_PULLED=1 exec ./run.sh ${ORIG_ARGS[@]+"${ORIG_ARGS[@]}"}; fi
    fi ;;
esac

case "$command" in
  play|serve)
    parse_options "$@"
    [ "$command" = serve ] || build_web
    if [ "$lan" = 1 ]; then serve_lan; fi
    serve_local
    url="http://127.0.0.1:$port/${query:+?}$query"
    echo "$url"
    [ "${NO_OPEN:-0}" = 1 ] || open_player "$url" ;;
  build) [ "$lan" = 0 ] || die "Use ./run.sh lan or ./run.sh lan serve"; build_web ;;
  editor) need_cocos; exec "$COCOS" --project "$PROJECT" ;;
  clean)
    rm -rf PerchSort/library PerchSort/temp PerchSort/build PerchSort/local
    echo "Removed PerchSort/library, temp, build and local" ;;
  cdp) exec node tools/cocos/cdp.mjs "$@" ;;
  check)
    need_node_modules
    git ls-files -z '*.sh' | xargs -0 -n1 bash -n
    node tools/cocos/meta.mjs --check
    node tools/art/compress.mjs --check
    # Creator's Babel can fail on a cast to a union (as X | Y); tsc accepts it.
    if grep -rnE '^[^/]*[^/]\bas [^;,)=]*\|' PerchSort/assets/scripts --include='*.ts'; then
      die "Casts to a union type above: name the union with a type alias"
    fi
    # Creator's loose-mode Babel compiles [...iterator] to [].concat(iterator), which is wrong for Map, Set and iterators.
    if grep -rnE '\[\.\.\.(new (Set|Map)|[A-Za-z_.]*\.(values|keys|entries)\(\))' PerchSort/assets/scripts --include='*.ts'; then
      die "Spread of a Map, Set or iterator above: use Array.from(...)"
    fi
    # macOS bash 3.2 reads non-ASCII text directly after $name as part of the name (unbound variable); write ${name}.
    if git ls-files -z '*.sh' | xargs -0 env LC_ALL=C grep -nE '[$][A-Za-z_][A-Za-z0-9_]*[^ -~[:space:]]'; then
      die "Non-ASCII text directly after \$name above: write \${name}"
    fi
    bash scripts/run_tests.sh ;;
  test) need_node_modules; npm --prefix PerchSort test ;;
  meta) exec node tools/cocos/meta.mjs "$@" ;;
  art-compress) need_node_modules; node tools/art/compress.mjs; node tools/cocos/meta.mjs ;;
  native-apply) exec node tools/native/apply.mjs "$@" ;;
  native-icons) exec node tools/native/icons.mjs "$@" ;;
  -h|--help|help) usage ;;
  *) echo "Unknown command: $command. See ./run.sh help." >&2; exit 2 ;;
esac
