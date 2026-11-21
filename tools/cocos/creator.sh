# Shared Cocos Creator command-line build. Source from the repository root (run.sh, package.sh, tools/*/build.sh).
COCOS_VERSION=3.8.8
find_cocos() {
  if [ -n "${COCOS_CREATOR:-}" ]; then echo "$COCOS_CREATOR"; return; fi
  local bin
  for bin in "/Applications/Cocos/Creator/$COCOS_VERSION/CocosCreator.app/Contents/MacOS/CocosCreator" \
             "$HOME/Applications/Cocos/Creator/$COCOS_VERSION/CocosCreator.app/Contents/MacOS/CocosCreator"; do
    if [ -x "$bin" ]; then echo "$bin"; return; fi
  done
  echo CocosCreator
}
COCOS="$(find_cocos)"
PROJECT="$PWD/PerchSort"

die() { echo "Error: $*" >&2; exit 1; }

need_cocos() {
  [ -x "$COCOS" ] || command -v "$COCOS" >/dev/null || die "Cocos Creator ${COCOS_VERSION} not found. Install it under /Applications/Cocos/Creator/${COCOS_VERSION}/ or set COCOS_CREATOR."
}

need_node_modules() {
  command -v node >/dev/null || die "node not found (Node 22 or later is required)"
  [ -d PerchSort/node_modules ] || npm --prefix PerchSort ci --no-audit --no-fund
}

# Metas whose import was broken: Creator occasionally rewrites them to importer "*" (a plugin reload racing the asset database).
broken_metas() {
  grep -rl '"importer": "\*"' "$PROJECT/assets" --include='*.meta' 2>/dev/null || true
}

# One build from an empty cache. Creator 3.8.8 incremental imports on an existing cache can mark every asset
# importer "*" and the game renders black, so library/ and temp/ are cleared first. COCOS_KEEP_CACHE=1 skips that.
# Creator exits 36 on success (32 = bad arguments, 34 = build error).
cocos_build_once() {
  local config="$1" log="$2" code=0
  [ "${COCOS_KEEP_CACHE:-}" = "1" ] || rm -rf "$PROJECT/library" "$PROJECT/temp"
  "$COCOS" --project "$PROJECT" --build "configPath=$config" >"$log" 2>&1 || code=$?
  return "$code"
}

# Build with up to three attempts. A broken import restores only the affected tracked metas, never other local work.
# Usage: cocos_build <config path> <log path>
cocos_build() {
  local config="$1" log="$2" code=0 try broken
  need_cocos
  need_node_modules
  node tools/cocos/meta.mjs
  mkdir -p "$(dirname "$log")"
  for try in 1 2 3; do
    code=0
    cocos_build_once "$config" "$log" || code=$?
    broken="$(broken_metas)"
    if [ -z "$broken" ]; then break; fi
    echo "Attempt ${try}: Creator broke the asset import (importer \"*\"); restoring those metas and rebuilding from an empty cache." >&2
    local meta
    while IFS= read -r meta; do
      if git ls-files --error-unmatch -- "$meta" >/dev/null 2>&1; then git checkout -- "$meta"; else rm -f "$meta"; fi
    done <<< "$broken"
    node tools/cocos/meta.mjs
    code=1
  done
  if [ "$code" != 36 ] && [ "$code" != 0 ]; then
    tail -60 "$log" >&2
    die "Cocos build failed (exit ${code}); full log: ${log}"
  fi
}
