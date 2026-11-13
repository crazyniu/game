#!/usr/bin/env bash
# one.sh <world> <key> <description>: paint one bird in the locked style
S=$(dirname "$0"); w=$1; key=$2; desc=$3
[ -f $S/out/w${w}_$key.png ] && exit 0
timeout 900 codex exec --skip-git-repo-check -C $S -i $S/style_ref.png -- "Use the image generation tool, do not write code. The input image shows the exact art style to match. Paint ONE single bird: a ${desc}. LOCKED STYLE (identical for every bird of the game): glossy bright cartoon rendering with soft smooth shading and a thin soft darker outline; a cute friendly face with large round glossy eyes with a white highlight, small cheerful open beak; real species body proportions and silhouette (not a round ball); the bird stands on a short horizontal branch, body turned three-quarters to the viewer's left, head turned slightly to the viewer; the whole bird including its tail and feet fully inside the image with margin; the bird fills about 70 percent of the image height; no cast shadow, no ground, no leaves, no text. Fully transparent background (real alpha), nothing else in the picture." < /dev/null > $S/out/log_w${w}_$key.txt 2>&1
id=$(grep -m1 'session id' $S/out/log_w${w}_$key.txt | awk '{print $3}')
f=$(ls ~/.codex/generated_images/$id/*.png 2>/dev/null | head -1)
[ -n "$f" ] && cp "$f" $S/out/w${w}_$key.png && echo "ok w$w $key" || echo "FAIL w$w $key"
