#!/usr/bin/env bash
# paint.sh <name> <prompt-file> [input.png]: one local Codex image job; the picture is copied to out/<name>.png.
# Run from the repository root. The Codex session id in the log locates the generated image.
name=$1; prompt=$2; input=$3; D=tools/birds/costume_parts
args=(--skip-git-repo-check --dangerously-bypass-approvals-and-sandbox -C "$(pwd)")
[ -n "$input" ] && args+=(-i "$input")
timeout 1500 codex exec "${args[@]}" -- "$(cat "$prompt")" < /dev/null > $D/out/$name.log 2>&1
sid=$(grep -m1 'session id:' $D/out/$name.log | awk '{print $3}')
img=$(ls -t ~/.codex/generated_images/$sid/*.png 2>/dev/null | head -1)
[ -n "$img" ] && cp "$img" $D/out/$name.png
echo "$name exit $? session=$sid image=${img:-none}"
