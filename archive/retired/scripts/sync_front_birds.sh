#!/usr/bin/env bash
set -euo pipefail
repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
source_dir="$repo_root/art/raw/birds_front"
target_dir="$repo_root/PerchSort/Assets/Art/birds_front"
mkdir -p "$target_dir"
for number in {1..10}; do
    printf -v id "%02d" "$number"
    for pose in '' _blink _sleep _fly; do
        cp "$source_dir/bird_${id}${pose}.png" "$target_dir/bird_${id}${pose}.png"
    done
done
echo "Synced 40 front bird sprites to $target_dir"
