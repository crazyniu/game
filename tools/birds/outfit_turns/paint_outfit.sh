#!/usr/bin/env bash
# paint_outfit.sh <species> <outfit index>: Codex paints the 8 turn frames of one bird wearing one outfit.
# Inputs tools/birds/outfit_turns/src/<species>_turn_<left|right>_<nn>.png and <species>_outfit_<n>_front.png;
# outputs tools/birds/outfit_turns/out/<species>_outfit_<n>_turn_<side>_<nn>.png (1024, transparent).
sp=$1; o=$2; D=tools/birds/outfit_turns
list=""
for side in left right; do for i in 01 02 03 04; do list="$list $D/src/${sp}_turn_${side}_${i}.png -> $D/out/${sp}_outfit_${o}_turn_${side}_${i}.png;"; done; done
timeout 3600 codex exec --skip-git-repo-check --dangerously-bypass-approvals-and-sandbox -C "$(pwd)" "You add an outfit to existing painted animation frames of a cartoon bird. Reference for the outfit: $D/src/${sp}_outfit_${o}_front.png (the bird seen from the front WEARING the outfit). For EACH pair below, use your image generation tool once, with the source frame as the input image (1024x1024), and paint the SAME bird in exactly the same pose, position, size, head angle and lighting, now wearing that outfit, seen from this frame's angle (turned heads show the outfit turned with the head; glasses follow the eyes; hats sit on the head). Do not change the bird's body, colours, eyes, feet or silhouette; do not move it; keep the background fully transparent; nothing else added. Pairs (input -> output path):$list Save each output exactly at its path as a 1024x1024 transparent PNG. Do not edit any other file. Reply with DONE." < /dev/null > $D/out/${sp}_outfit_${o}.log 2>&1
echo "${sp}_outfit_${o} exit $?"
