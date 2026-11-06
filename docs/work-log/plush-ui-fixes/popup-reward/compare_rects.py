"""Compare captured RectTransforms, corrected for sprite alpha, with measured layouts."""
import fnmatch
import json
import math
from collections import Counter
from pathlib import Path

from PIL import Image, ImageChops, ImageDraw

REPO = Path(__file__).resolve().parents[4]
HERE = Path(__file__).resolve().parent
mapping = json.loads((HERE / "layout-map.json").read_text())
manifest = json.loads((HERE / "evidence-index.json").read_text())
records = [json.loads(line) for line in (HERE / "rendered-rects.jsonl").read_text().splitlines()]
sprites = {path.stem: path for path in (REPO / "PerchSort/Assets/Resources").rglob("*.png")}
alpha_bounds = {}


def visible(row):
    """The dump is in design-canvas space; preserveAspect and alpha add image insets."""
    x, y, w, h = row["rect"]
    sprite = row["sprite"]
    if sprite not in alpha_bounds:
        with Image.open(sprites[sprite]) as image:
            alpha = image.convert("RGBA").getchannel("A").point(lambda a: 255 if a >= 8 else 0)
            alpha_bounds[sprite] = alpha
    alpha = alpha_bounds[sprite]
    sw, sh = alpha.size
    if row["preserveAspect"]:
        scale = min(w / sw, h / sh)
        x += (w - sw * scale) / 2
        y += (h - sh * scale) / 2
        w, h = sw * scale, sh * scale
    clip = row.get("clip")
    if row.get("maskSprite") == "door-mask":
        cx, cy, cw, ch = clip
        mask = Image.new("L", alpha.size)
        ImageDraw.Draw(mask).ellipse(((cx - x + cw / 192) * sw / w, (cy - y + ch / 192) * sh / h,
                                      (cx + cw - x - cw / 192) * sw / w, (cy + ch - y - ch / 192) * sh / h), fill=255)
        alpha = ImageChops.multiply(alpha, mask)
    elif row.get("maskSprite"):
        return visible({**row, "rect": clip, "sprite": row["maskSprite"], "maskSprite": "", "clip": None})
    if clip:
        cx, cy, cw, ch = clip
        crop = [max(0, math.ceil((cx - x) * sw / w)), max(0, math.ceil((cy - y) * sh / h)),
                min(sw, math.floor((cx + cw - x) * sw / w)), min(sh, math.floor((cy + ch - y) * sh / h))]
        left, top, right, bottom = alpha.crop(crop).getbbox()
        left, right = left + crop[0], right + crop[0]
        top, bottom = top + crop[1], bottom + crop[1]
    else:
        left, top, right, bottom = alpha.getbbox()
    return [x + left * w / sw, y + top * h / sh, (right - left) * w / sw, (bottom - top) * h / sh]


def union(rects):
    x, y = min(r[0] for r in rects), min(r[1] for r in rects)
    return [x, y, max(r[0] + r[2] for r in rects) - x, max(r[1] + r[3] for r in rects) - y]


summaries = []
for entry in manifest:
    reference = json.loads((REPO / "docs/design/layout" / (entry["mockup"] + ".json")).read_text())
    expected = {element["id"]: element["rect"] for element in reference["elements"]}
    rows = [row for row in records if row["state"] == entry["state"] and row["graphic"] and row["alpha"] > 0 and row["sprite"]]
    print("\n" + entry["state"])
    maximum = 0
    measured = 0
    skipped = Counter()
    for element in mapping[entry["mockup"]]:
        if element.get("state") and element["state"] != entry["state"]:
            skipped["different booster artwork"] += 1
            continue
        if element["status"] != "measure":
            skipped[element["status"] + ": " + element["owner"]] += 1
            continue
        selectors = element["objects"]
        selectors = [selectors] if isinstance(selectors, str) else selectors
        selected = []
        for row in rows:
            path = row["path"]
            for selector in selectors:
                if selector.startswith("@action"):
                    match = path == ("Acknowledge unlock" if entry["mockup"] == "unlock_card" else "Close popup") + selector[len("@action"):]
                else:
                    match = fnmatch.fnmatchcase(path, "Panel/" + selector)
                if match:
                    selected.append(visible(row))
                    break
        if not selected:
            print(f"  {element['id']}: MISSING runtime image")
            continue
        actual = union(selected)
        delta = max(abs(a - b) for a, b in zip(actual, expected[element["id"]]))
        maximum = max(maximum, delta)
        measured += 1
        print(f"  {element['id']}: {delta:.2f} px; rendered=" + ",".join(f"{n:.2f}" for n in actual) + (" OUTSIDE 20 px" if delta > 20 else ""))
    print(f"  MAX: {maximum:.2f} px across {measured} measured elements")
    print("  NOT MEASURED: " + "; ".join(f"{reason}={count}" for reason, count in skipped.items()))
    summaries.append({**entry, "max_deviation": round(maximum, 2), "measured_elements": measured})
(HERE / "rect-summary.json").write_text(json.dumps(summaries, indent=2) + "\n")
