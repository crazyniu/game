#!/usr/bin/env bash
# Build web-mobile from an empty Creator cache (tools/cocos/creator.sh); prints the output folder.
set -euo pipefail
cd "$(dirname "$0")/.."
source tools/cocos/creator.sh
cocos_build "$PROJECT/build-config/web-mobile.json" build/cocos-build.log
echo "$PROJECT/build/web-mobile"
