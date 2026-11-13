#!/usr/bin/env bash
# reference.sh <species> <family> "<colour description>": Codex paints the front design of a world bird from the
# plain species front frame (pose, size, eyes) and the family lineup (kind and style). Output src/<species>_<family>_reference.png.
sp=$1; fam=$2; colour=$3; D=tools/birds/world_birds
timeout 900 codex exec --skip-git-repo-check -C "$(pwd)" -i PerchSort/birds/${sp}_front.png -i $D/src/sunny_${fam}_reference.png -i $D/src/${fam}_family_lineup.png -- "Use the image generation tool, do not write code. Input 1 is a cartoon plush bird (front view, transparent background); input 2 is the yellow ${fam} of the same family; input 3 is the family lineup. Paint the bird of input 1 as a ${fam} exactly like input 2 in kind, plush felt style, markings, beak, cere and tail, keeping input 1's pose, position, size, framing, eyes and eyelids and lighting, coloured ${colour}. The same orange feet. One bird only, fully transparent background, nothing else." < /dev/null > $D/out/reference_${sp}.log 2>&1
id=$(grep -m1 'session id' $D/out/reference_${sp}.log | awk '{print $3}')
f=$(ls ~/.codex/generated_images/$id/*.png 2>/dev/null | head -1)
[ -n "$f" ] && python3 -c "from PIL import Image; Image.open('$f').convert('RGBA').resize((1024,1024),Image.LANCZOS).save('$D/src/${sp}_${fam}_reference.png')" && echo "ok $sp" || echo "FAIL $sp"
