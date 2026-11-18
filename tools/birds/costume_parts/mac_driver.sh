#!/bin/bash
# mac_driver.sh <workers> <species_slot>...: paint, align, pack and upload skins one after another (runs on the Mac, repository root = ~/skinwork).
cd "$(dirname "$0")/../../.." || exit 1
W=$1; shift
B=http://192.168.1.250/api/transfer
for key in "$@"; do
  /Users/apple/.local/bin/uv run --no-project --with pillow --with numpy --with scipy python tools/birds/costume_parts/run_skins.py --workers $W $key
  /Users/apple/.local/bin/uv run --no-project --with pillow python tools/birds/costume_parts/pack_skin.py $key && \
    curl -fsS -T tools/birds/costume_parts/out/pack/$key.zip "$B/file?path=perchsort%2Fskins%2F$key.zip" && echo "uploaded $key" >> tools/birds/costume_parts/out/mac_uploads.status
done
