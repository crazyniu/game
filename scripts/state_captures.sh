#!/usr/bin/env bash
set -euo pipefail
repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
unity_editor="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity}"
output="$repo_root/build/screens/states"

if [[ $# -ne 0 && ( $# -ne 2 || "$1" != "-states" || -z "$2" ) ]]; then
    echo 'Usage: scripts/state_captures.sh [-states "filter substring"]' >&2
    exit 1
fi

# Hold the worker lock through both rendering and contact-sheet generation.
exec 9>/tmp/unity.lock
flock 9
mkdir -p "$output"
python3 "$repo_root/ci/check_current_game_names.py" --self-test
python3 "$repo_root/ci/check_current_game_names.py"
uv run --with pillow python -c 'from PIL import Image, ImageDraw, ImageFont'

# Remove only this tool's previous outputs, so a filtered run cannot present stale states.
python3 - "$output" <<'CLEAN'
from pathlib import Path
import sys
folder = Path(sys.argv[1])
for pattern in ("*/*.png", "contact_*.jpg", "index.txt"):
    for path in folder.glob(pattern):
        path.unlink()
CLEAN

status=0
"$unity_editor" -batchmode -projectPath "$repo_root/PerchSort" \
    -executeMethod PerchSort.Editor.UiStateCapture.Run -screen-width 1080 -screen-height 1920 \
    -logFile "$output/unity.log" "$@" || status=1

uv run --with pillow python - "$output" <<'SHEETS' || status=1
from collections import defaultdict
from pathlib import Path
import sys
import textwrap
from PIL import Image, ImageDraw, ImageFont

folder = Path(sys.argv[1])
index = folder / "index.txt"
if not index.exists():
    index.write_text("ERROR\t-\tStartup\tUnity\tCapture entry point did not complete; see unity.log\n")
    raise SystemExit("Unity did not start the capture tool; see unity.log")

groups = defaultdict(list)
rows = index.read_text().splitlines()
for line in rows:
    fields = line.split("\t")
    if fields[0] == "FILE":
        groups[Path(fields[1]).parent.name].append((fields[1], fields[3]))

# These labels belong to the review sheet only, outside the unmodified game frame.
font = ImageFont.load_default(size=17)
for screen, entries in groups.items():
    columns = min(4, len(entries))
    sheet = Image.new("RGB", (columns * 360, ((len(entries) + columns - 1) // columns) * 704), "#eee9df")
    draw = ImageDraw.Draw(sheet)
    for position, (filename, state) in enumerate(entries):
        x, y = (position % columns) * 360, (position // columns) * 704
        with Image.open(folder / filename) as capture:
            if capture.size != (1080, 1920):
                raise ValueError(f"Wrong capture size: {filename}: {capture.size}")
            sheet.paste(capture.convert("RGB").resize((360, 640), Image.Resampling.LANCZOS), (x, y))
        label = "\n".join(textwrap.wrap(state, width=38))
        draw.multiline_text((x + 8, y + 648), label, font=font, fill="#25251f", spacing=2)
    filename = f"contact_{screen}.jpg"
    sheet.save(folder / filename, quality=90)
    with index.open("a") as stream:
        stream.write(f"CONTACT\t{filename}\t{screen}\t{len(entries)} states\n")
    print(folder / filename)

if not groups or not any(line.startswith("SUMMARY\t") for line in rows) or any(line.startswith("ERROR\t") for line in rows):
    raise SystemExit("Capture incomplete; inspect index.txt and unity.log")
SHEETS

if [[ $status -ne 0 ]]; then
    tail -n 60 "$output/unity.log"
fi
exit "$status"
