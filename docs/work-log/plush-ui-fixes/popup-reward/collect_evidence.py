"""Save the current silent capture batch before the next filtered run clears it."""
import json
import re
from pathlib import Path

from PIL import Image

REPO = Path(__file__).resolve().parents[4]
HERE = Path(__file__).resolve().parent
CAPTURES = REPO / "build/screens/states"
MOCKUPS = {
    "Daily gift": "daily_gift", "New friend": "new_friend", "Unlock": "unlock_card",
    "World complete": "world_complete", "World unlock": "world_unlock",
}

entries = []
for line in (CAPTURES / "index.txt").read_text().splitlines():
    fields = line.split("\t")
    if fields[0] != "FILE" or fields[3].split(" / ")[0] not in MOCKUPS:
        continue
    state = fields[3]
    mockup = MOCKUPS[state.split(" / ")[0]]
    filename = re.sub(r"[^a-z0-9]+", "-", state.lower()).strip("-") + ".jpg"
    sheet = Image.new("RGB", (1440, 1280))
    for column, path in enumerate((Path("/tmp") / (mockup + ".png"), CAPTURES / fields[1])):
        with Image.open(path) as image:
            sheet.paste(image.convert("RGB").resize((720, 1280), Image.Resampling.LANCZOS), (column * 720, 0))
    sheet.save(HERE / filename, quality=92)
    entries.append({"state": state, "mockup": mockup, "side_by_side": filename, "capture": fields[1]})

states = {entry["state"] for entry in entries}
manifest_path = HERE / "evidence-index.json"
previous = json.loads(manifest_path.read_text()) if manifest_path.exists() else []
manifest_path.write_text(json.dumps([row for row in previous if row["state"] not in states] + entries, indent=2) + "\n")
dump_path = HERE / "rendered-rects.jsonl"
previous = [json.loads(line) for line in dump_path.read_text().splitlines()] if dump_path.exists() else []
current = [json.loads(line.split("POPUP_RECT ", 1)[1]) for line in (CAPTURES / "unity.log").read_text().splitlines() if "POPUP_RECT {" in line]
dump_path.write_text("".join(json.dumps(row, separators=(",", ":")) + "\n" for row in previous if row["state"] not in states) +
                     "".join(json.dumps(row, separators=(",", ":")) + "\n" for row in current))
print(f"Saved {len(entries)} comparisons and {len(current)} runtime rectangles.")
