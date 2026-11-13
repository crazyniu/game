#!/usr/bin/env bash
# queue.sh family...: paint world families one after another (family.sh), after any running run_all.py finishes.
D=tools/birds/world_birds
while pgrep -f "world_birds/run_all.py" >/dev/null; do sleep 60; done
for fam in "$@"; do echo "=== $fam $(date +%H:%M)"; bash $D/family.sh $fam; done
echo "=== QUEUE DONE $(date +%H:%M)"
