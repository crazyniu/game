#!/usr/bin/env bash
# lineup.sh <family> "<kind description>": Codex paints the nine-colour family lineup of a world's birds from the
# nine plain species fronts (style, colours, eyes). Output src/<family>_family_lineup.png.
fam=$1; kind=$2; D=tools/birds/world_birds; B=PerchSort/birds
timeout 900 codex exec --skip-git-repo-check -C "$(pwd)" -i $B/sunny_front.png -i $B/sky_front.png -i $B/berry_front.png -i $B/mint_front.png -i $B/cocoa_front.png -i $B/snow_front.png -i $B/ember_front.png -i $B/ink_front.png -i $B/hoot_front.png -- "Use the image generation tool, do not write code. The nine input images are the nine birds of a sorting game (round plush felt cartoon style, front view, big friendly eyes). Paint ONE image: a 3x3 lineup of nine cute round ${kind} in exactly that plush felt style and roundness, each in the same main colour as the matching input bird, in input order left-to-right, top-to-bottom: sunny yellow, sky blue with white belly, raspberry pink, mint green, DARK chocolate brown, snow white with lilac tips, flame orange, deep navy with a small white star on the chest, pale golden sand with a cream face. Real features of the kind on each, big friendly eyes like the inputs, same orange feet. Every bird clearly distinguishable by colour at a glance. Plain light background, no text." < /dev/null > $D/out/lineup_${fam}.log 2>&1
id=$(grep -m1 'session id' $D/out/lineup_${fam}.log | awk '{print $3}')
f=$(ls ~/.codex/generated_images/$id/*.png 2>/dev/null | head -1)
[ -n "$f" ] && cp "$f" $D/src/${fam}_family_lineup.png && echo "ok $fam" || echo "FAIL $fam"
