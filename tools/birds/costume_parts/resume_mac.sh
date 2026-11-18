#!/bin/bash
# resume_mac.sh <epoch>: wait until the Codex usage limit has reset, then start the five Mac drivers over the skins left (repository root = ~/skinwork).
cd "$(dirname "$0")/../../.." || exit 1
while [ "$(date +%s)" -lt "$1" ]; do sleep 30; done
D=tools/birds/costume_parts; i=0
for L in 'ember_4 sky_3' 'ember_5 sky_4' 'ember_3 sky_5' 'hoot_2 ember_1 sky_2' 'ink_2 hoot_1'; do i=$((i+1)); (nohup nice -n 10 bash $D/mac_driver.sh 5 $L > m$i.log 2>&1 &); done
