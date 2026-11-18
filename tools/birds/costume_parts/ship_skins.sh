#!/bin/bash
# ship_skins.sh: on the Mac, pack and upload every finished skin once (loops forever; repository root = ~/skinwork).
cd "$(dirname "$0")/../../.." || exit 1
B=http://192.168.1.250/api/transfer
S=tools/birds/costume_parts/out/run_skins.status
while true; do
  for key in $(grep ' done ' $S 2>/dev/null | awk '{print $4}' | sort -u); do
    grep -qx "$key" shipped.txt 2>/dev/null && continue
    /Users/apple/.local/bin/uv run --no-project --with pillow python tools/birds/costume_parts/pack_skin.py $key || continue
    code=$(curl -sS -o /dev/null -w '%{http_code}' -T tools/birds/costume_parts/out/pack/$key.zip "$B/file?path=perchsort%2Fskins%2F$key.zip")
    case $code in 200|201|409) echo $key >> shipped.txt; echo "shipped $key $code";; *) echo "upload $key failed $code";; esac
  done
  sleep 60
done
