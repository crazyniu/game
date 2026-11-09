#!/usr/bin/env bash
# paint.sh <species> <index> "<costume>": Codex paints one bird wearing a large costume.
# Input tools/birds/costumes/src/<species>_front.png; output tools/birds/costumes/out/<species>_outfit_<n>_raw.png
sp=$1; n=$2; d=$3; D=tools/birds/costumes
timeout 1500 codex exec --skip-git-repo-check --dangerously-bypass-approvals-and-sandbox -C "$(pwd)" -i $D/src/${sp}_front.png -- "Edit the input image (a cartoon plush bird, front view, transparent background). Use your image generation tool to paint the SAME bird in exactly the same pose, position, size, framing, colours, eyes, beak, feet and lighting, now wearing ${d}. The costume must be big, bold and clearly different from the plain bird, in the same soft plush felt style, with the costume colours harmonising with the bird's own colour. The bird's face, eyes and beak must stay fully visible and unchanged. Keep the bird the same size and the costume inside the picture with margin. Fully transparent background, nothing else added. Save nothing else." < /dev/null > $D/out/${sp}_outfit_${n}.log 2>&1
echo "${sp}_${n} exit $?"
