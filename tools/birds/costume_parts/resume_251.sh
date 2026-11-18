#!/bin/bash
# resume_251.sh <epoch>: wait until the Codex usage limit has reset, then start the three drivers on this machine over the skins left.
cd "$(dirname "$0")/../../.." || exit 1
while [ "$(date +%s)" -lt "$1" ]; do sleep 30; done
i=0
for L in 'berry_4 snow_2 snow_5 ink_1' 'berry_5 snow_3 sky_1' 'snow_1 snow_4 ink_3 ember_2'; do i=$((i+1)); (nohup uv run --no-project --with pillow --with numpy --with scipy python tools/birds/costume_parts/run_skins.py --workers 5 $L > /tmp/claude-1100/-home-developer-Projects-game/1c07b3b8-b2f6-4589-8830-752e25cf60c3/scratchpad/batch3_$i.log 2>&1 &); done
