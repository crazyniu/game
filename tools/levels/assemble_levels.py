"""Assemble or reorder the fixed catalog along a route-independent difficulty curve.

python3 tools/levels/assemble_levels.py <metrics.json> <candidates dir> <redeal dir> <out dir>
python3 tools/levels/assemble_levels.py --reorder <levels.json> <hints.json> <curve.json> <out dir> <report dir>

The reorder command preserves puzzles and cached solutions, changes their positions, and raises pars to cover
those solutions. Its renumber.json is the migration table to add to Profile before delivering the catalog.
"""
import glob
import json
import os
import sys

MAX_ROWS = [4, 5, 6, 6, 6, 6, 6, 6, 6, 6]
VARIANTS = [["morning", "noon", "golden"], ["mist", "noon", "drizzle"], ["morning", "noon", "windy"], ["morning", "noon", "bluehour"]]
SPECIALS = ["rain", "lanterns", "sunset", "aurora"]
LEVEL_COUNT = 100
PER_WORLD = 10
ENCORE_FLAG = 1 << 13


def fragmentation(level):
    """Extra separate flocks in the initial board; independent of any solver route."""
    runs = 0
    species = set()
    for branch in level["branches"]:
        previous = None
        for token in branch["birds"]:
            parts = token.split(":")
            bird = parts[1] if parts[0] == "egg" else parts[0]
            if bird != "crow":
                species.add(bird)
                if bird != previous:
                    runs += 1
            previous = bird
    return runs - len(species)


def score(level, metrics):
    structural = fragmentation(level) * 0.6 + metrics["species"] \
        + (1.0 - metrics["win_rate"]) * 10.0 + len(metrics["mechanics"])
    # A known short win proves a chain-heavy board is easy. Longer search routes cannot raise the structural score.
    easy_bound = 2.0 + 2.0 * metrics.get("known_counted", metrics["counted"])
    return round(min(structural, easy_bound), 2)


def rows_of(level):
    return max(b["row"] for b in level["branches"]) + 1


def ordered(entries):
    """Order globally; only the painted row capacity constrains a puzzle's earliest position."""
    remaining = sorted(entries, key=lambda e: (e["score"], e["origin"]))
    result = []
    for position in range(LEVEL_COUNT):
        rows = MAX_ROWS[position // PER_WORLD]
        index = next(i for i, e in enumerate(remaining) if rows_of(e["level"]) <= rows)
        result.append(remaining.pop(index))
    return result


def write_json(path, data):
    with open(path, "w", encoding="utf-8") as target:
        json.dump(data, target, separators=(",", ":"))
        target.write("\n")


def build_catalog(entries, slots=None):
    catalog, curve, renumber = [], [], {}
    for position, entry in enumerate(ordered(entries)):
        number = position + 1
        chapter = position // PER_WORLD
        level = dict(entry["level"])
        level["id"] = number
        level["world"] = chapter + 1
        if slots is not None:
            level["variant"] = slots[position]["variant"]
        else:
            level["variant"] = SPECIALS[chapter % 4] if number % PER_WORLD == 0 else VARIANTS[chapter % 4][position % PER_WORLD % 3]
            if number == 1:
                level["variant"] = "noon"
        metrics = entry["metrics"]
        level["par"] = max(int(level["par"]), int(metrics["counted"]))
        catalog.append(level)
        if entry["origin"]:
            renumber[entry["origin"]] = number
        curve.append({"level": number, "chapter": chapter + 1, "source": entry["origin"], "score": entry["score"],
                      "fragmentation": fragmentation(level), "counted": metrics["counted"],
                      "known_counted": metrics.get("known_counted", metrics["counted"]), "par": level["par"],
                      "species": metrics["species"], "branches": len(level["branches"]),
                      "win": metrics["win_rate"], "exact": metrics.get("exact", False), "mechanics": metrics["mechanics"]})
    return catalog, curve, renumber


def reorder(levels_path, hints_path, curve_path, out_dir, report_dir):
    with open(levels_path, encoding="utf-8") as source:
        levels = json.load(source)["levels"]
    with open(hints_path, encoding="utf-8") as source:
        hints = json.load(source)
    with open(curve_path, encoding="utf-8") as source:
        curve = {int(row["level"]): row for row in json.load(source)}
    entries = []
    for level in levels:
        source_id = int(level["id"])
        row = curve[source_id]
        route = hints[str(source_id)]
        counted = sum(1 for key, _move in route if not int(key[0]) & ENCORE_FLAG)
        metrics = {"species": row["species"], "win_rate": row["win"], "mechanics": row["mechanics"],
                   "counted": counted, "known_counted": min(row.get("known_counted", row["counted"]), counted)}
        entries.append({"level": level, "metrics": metrics, "score": score(level, metrics), "origin": source_id})
    catalog, curve, renumber = build_catalog(entries, levels)
    remapped_hints = {str(renumber[int(old)]): route for old, route in hints.items()}
    os.makedirs(out_dir, exist_ok=True)
    os.makedirs(report_dir, exist_ok=True)
    write_json(os.path.join(out_dir, "levels.json"), {"levels": catalog})
    write_json(os.path.join(out_dir, "hints.json"), dict(sorted(remapped_hints.items(), key=lambda item: int(item[0]))))
    write_json(os.path.join(report_dir, "curve.json"), curve)
    write_json(os.path.join(report_dir, "renumber.json"), renumber)
    print("Reordered catalog and hints; wrote the migration table and difficulty curve.")


def assemble(metrics_path, cand_dir, redeal_dir, out_dir):
    root = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
    with open(os.path.join(root, "PerchSort/levels/levels.json"), encoding="utf-8") as source:
        authored = {int(level["id"]): level for level in json.load(source)["levels"]}
    with open(metrics_path, encoding="utf-8") as source:
        measurements = json.load(source)
    entries = [{"level": authored[int(m["id"])], "metrics": m, "score": score(authored[int(m["id"])], m),
                "origin": int(m["id"])} for m in measurements]
    for path in sorted(glob.glob(os.path.join(redeal_dir, "*.json"))):
        with open(path, encoding="utf-8") as source:
            options = json.load(source)
        for entry in entries:
            if entry["origin"] == options[0]["origin"]:
                best = min(options, key=lambda candidate: abs(score(candidate["level"], candidate["metrics"]) - entry["score"]))
                entry.update(level=best["level"], metrics=best["metrics"], score=score(best["level"], best["metrics"]))
    pool = []
    for path in sorted(glob.glob(os.path.join(cand_dir, "*.json"))):
        with open(path, encoding="utf-8") as source:
            for candidate in json.load(source):
                pool.append({"level": candidate["level"], "metrics": candidate["metrics"],
                             "score": score(candidate["level"], candidate["metrics"]), "origin": 0})
    gaps = LEVEL_COUNT - len(entries)
    if gaps:
        low = min(e["score"] for e in entries + pool)
        high = max(e["score"] for e in entries + pool)
        for i in range(gaps):
            goal = low + (high - low) * (i + 0.5) / gaps
            pick = min(range(len(pool)), key=lambda index: abs(pool[index]["score"] - goal))
            entries.append(pool.pop(pick))
    catalog, curve, renumber = build_catalog(entries)
    os.makedirs(out_dir, exist_ok=True)
    write_json(os.path.join(out_dir, "levels.json"), {"levels": catalog})
    write_json(os.path.join(out_dir, "curve.json"), curve)
    write_json(os.path.join(out_dir, "renumber.json"), renumber)


if __name__ == "__main__":
    if sys.argv[1] == "--reorder":
        reorder(*sys.argv[2:])
    else:
        assemble(*sys.argv[1:])
