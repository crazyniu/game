#!/usr/bin/env bash
set -u
root=/workspace/scratch/2bd695cff14d/game-approved-ui-integration
qa="$root/build/non-gameplay-integration/gui"
export XDG_DATA_HOME="$qa/data" XDG_CONFIG_HOME="$qa/config" XDG_CACHE_HOME="$qa/cache"
godot=/workspace/scratch/2bd695cff14d/godot472/godot
"$godot" --path "$root/PerchSort" --audio-driver Dummy --rendering-driver opengl3 --resolution 456x812 -s res://tests/run_menu_navigation.gd > "$qa/navigation-verified.log" 2>&1
printf 'verified navigation exit=%s\n' "$?" >> "$qa/status.log"
for language in zh en; do
 "$godot" --path "$root/PerchSort" --audio-driver Dummy --rendering-driver opengl3 --resolution 456x812 -s "$qa/render-target.gd" -- --preview=popups:chapters --lang="$language" --capture="$qa/render-target/chapters_$language.png" > "$qa/render-target/chapters_$language.log" 2>&1
 printf 'chapters %s exit=%s\n' "$language" "$?" >> "$qa/status.log"
done
"$godot" --path "$root/PerchSort" --audio-driver Dummy --rendering-driver opengl3 --resolution 456x812 -s "$root/docs/work-log/approved-ui-integration/evidence/check_entry_timing.gd" > "$qa/entry-timing.log" 2>&1
printf 'entry timing exit=%s\n' "$?" >> "$qa/status.log"
printf '\nSUPPLEMENT FINISHED\n'
