#!/usr/bin/env bash
# Create the WeChat Developer Tools package; never uploads or publishes it.
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$root"
source tools/cocos/creator.sh
need_node_modules
node tools/wechat/prepare.mjs
cocos_build "$root/build/wechat-build-config.json" build/wechat-build.log
node tools/wechat/finalize.mjs
