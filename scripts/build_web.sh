#!/usr/bin/env bash
# Build web-mobile from an empty Creator cache (tools/cocos/creator.sh); prints the output folder.
set -euo pipefail
cd "$(dirname "$0")/.."
source tools/cocos/creator.sh
out="$PROJECT/build/web-mobile"
rm -rf "$out"
cocos_build "$PROJECT/build-config/web-mobile.json" build/cocos-build.log
[ -f "$out/index.html" ] || die "Creator did not generate $out/index.html. See build/cocos-build.log."
echo "$out"
