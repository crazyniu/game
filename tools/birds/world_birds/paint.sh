#!/usr/bin/env bash
# paint.sh <bird> <frame>...: Codex repaints plain animation frames of a species as a world bird of another kind.
# <bird> is <species>_<family>, e.g. sunny_budgie: the world-2 bird in the sunny slot. Inputs
# tools/birds/world_birds/src/<species>_<frame>.png (plain frames upscaled to 1024) and src/<bird>_reference.png;
# outputs out/<bird>_<frame>.png (1024, transparent).
bird=$1; shift; sp=${bird%%_*}; D=tools/birds/world_birds
list=""
for f in "$@"; do list="$list $D/src/${sp}_${f}.png -> $D/out/${bird}_${f}.png;"; done
tag=$(echo "$1" | tr -c 'a-z0-9_' '_')
timeout 5400 codex exec --skip-git-repo-check --dangerously-bypass-approvals-and-sandbox -C "$(pwd)" "You repaint animation frames of a cartoon plush bird as a different kind of bird. Reference: $D/src/${bird}_reference.png shows the new bird from the front (its species, colours, markings, beak, tail and plush felt style). For EACH pair below, use your image generation tool once, with the source frame as the input image and the reference as a second input, and paint the NEW bird in exactly the same pose, position, size, head angle, body tilt, wing and feet positions and lighting as the source frame, with the new bird's own markings, beak shape and tail seen from this frame's angle. The eyes and eyelids follow the source frame (closed eyes stay closed, half-closed stay half-closed); a beak seen from the side stays from the side; only one pair of eyes ever. Keep the body round and plush like the source. Exactly one bird per picture; no ground shadow, no motion blur, no speed lines, no extra copies, no props. Keep the whole bird inside the frame with margin, fully transparent background, nothing else added. Pairs (input -> output path):$list Save each output exactly at its path as a 1024x1024 RGBA PNG with transparent background. Do not edit any other file. Reply with DONE." < /dev/null > $D/out/${bird}_${tag}.log 2>&1
echo "${bird} $1.. exit $?"
