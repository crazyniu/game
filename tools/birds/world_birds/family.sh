#!/usr/bin/env bash
# family.sh <family>: paint a whole world family: the yellow front design from the family lineup, the other eight
# colours from it (reference.sh), then every animation frame of all nine birds (run_all.py). Resumable.
fam=$1; D=tools/birds/world_birds; B=PerchSort/birds
if [ ! -f $D/src/sunny_${fam}_reference.png ]; then
  timeout 900 codex exec --skip-git-repo-check -C "$(pwd)" -i $B/sunny_front.png -i $D/src/${fam}_family_lineup.png -- "Use the image generation tool, do not write code. Input 1 is a cartoon plush bird (front view, transparent background). Input 2 is a lineup of one bird family. Paint the bird of input 1 as the YELLOW bird of input 2 (top-left): its kind, beak, markings, tail and plush felt style, keeping exactly input 1's pose, position, size, framing, eyes and lighting. The same orange feet. One bird only, fully transparent background, nothing else." < /dev/null > $D/out/reference_sunny_${fam}.log 2>&1
  id=$(grep -m1 'session id' $D/out/reference_sunny_${fam}.log | awk '{print $3}')
  f=$(ls ~/.codex/generated_images/$id/*.png 2>/dev/null | head -1)
  [ -n "$f" ] && python3 -c "from PIL import Image; Image.open('$f').convert('RGBA').resize((1024,1024),Image.LANCZOS).save('$D/src/sunny_${fam}_reference.png')"
fi
[ -f $D/src/sunny_${fam}_reference.png ] || { echo "FAIL sunny_${fam} reference"; exit 1; }
ref() { [ -f $D/src/$1_${fam}_reference.png ] || bash $D/reference.sh $1 $fam "$2"; }
ref sky "sky blue with a white face and white belly" & ref berry "raspberry pink all over" &
ref mint "mint green with a pale mint face" & ref cocoa "DARK chocolate brown with a cream face (clearly darker than a sandy bird)" & wait
ref snow "snow white with soft lilac wing tips" & ref ember "bright flame orange" &
ref ink "deep navy blue with a small white star on the chest" & ref hoot "pale golden sand with a cream face (clearly lighter than chocolate brown)" & wait
birds=""; for sp in sunny sky berry mint cocoa snow ember ink hoot; do birds="$birds ${sp}_${fam}"; done
python3 -u $D/run_all.py --workers 12 --tries 3 $birds
