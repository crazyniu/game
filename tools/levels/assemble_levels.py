"""Propose a complete reorder of the current fixed catalog.

python3 tools/levels/assemble_levels.py --reorder <levels.json> <hints.json> <curve.json> <out dir> <report dir>

This historical heuristic produces a proposal, not accepted difficulty ordering. It preserves puzzle bodies,
pars and cached solutions while changing positions. Approve the difficulty policy and implement the matching
Profile migration before shipping any output. The former expansion/candidate assembly mode is retired.
"""
import argparse
import json
import os

MAX_ROWS = [4, 5, 6, 6, 6, 6, 6, 6, 6, 6]
LEVEL_COUNT = 210
PER_WORLD = 21


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
    """Retained proposal heuristic; not a shortest-action or human-difficulty proof."""
    structural = fragmentation(level) * 0.6 + metrics["species"] \
        + (1.0 - metrics["win_rate"]) * 10.0 + len(metrics["mechanics"])
    easy_bound = 2.0 + 2.0 * metrics.get("known_counted", metrics["counted"])
    return round(min(structural, easy_bound), 2)


def rows_of(level):
    return max(b["row"] for b in level["branches"]) + 1


def require_catalog_ids(label, ids):
    """A complete fixed-catalog input must cover each current level exactly once."""
    expected = set(range(1, LEVEL_COUNT + 1))
    if len(ids) != LEVEL_COUNT or set(ids) != expected:
        raise ValueError(f"{label} must contain each level ID 1..{LEVEL_COUNT} exactly once")


def ordered(entries):
    """Propose a global order constrained only by the painted row capacity."""
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


def build_catalog(entries, slots):
    catalog, curve, renumber = [], [], {}
    for position, entry in enumerate(ordered(entries)):
        number = position + 1
        chapter = position // PER_WORLD
        level = dict(entry["level"])
        level["id"] = number
        level["world"] = chapter + 1
        level["variant"] = slots[position]["variant"]
        metrics = entry["metrics"]
        catalog.append(level)
        renumber[entry["origin"]] = number
        curve.append({"level": number, "chapter": chapter + 1, "source": entry["origin"], "score": entry["score"],
                      "fragmentation": fragmentation(level), "actions": metrics["actions"], "counted": metrics["counted"],
                      "known_counted": metrics.get("known_counted", metrics["counted"]), "par": level["par"],
                      "species": metrics["species"], "branches": len(level["branches"]),
                      "win": metrics["win_rate"], "exact": metrics["exact"], "mechanics": metrics["mechanics"]})
    return catalog, curve, renumber


def reorder(levels_path, hints_path, curve_path, out_dir, report_dir):
    with open(levels_path, encoding="utf-8") as source:
        levels = sorted(json.load(source)["levels"], key=lambda level: level["id"])
    with open(hints_path, encoding="utf-8") as source:
        hints = json.load(source)
    with open(curve_path, encoding="utf-8") as source:
        measurements = json.load(source)
    require_catalog_ids("Catalog", [int(level["id"]) for level in levels])
    require_catalog_ids("Hints", [int(key) for key in hints])
    require_catalog_ids("Measurements", [int(row["level"]) for row in measurements])
    curve = {int(row["level"]): row for row in measurements}
    entries = []
    for level in levels:
        source_id = int(level["id"])
        row = curve[source_id]
        # Hint key[0] is a homed-species mask. It cannot reconstruct counted moves after gift refunds.
        metrics = {"species": row["species"], "win_rate": row["win"], "mechanics": row["mechanics"],
                   "actions": len(hints[str(source_id)]), "counted": row["counted"],
                   "known_counted": row.get("known_counted", row["counted"]), "exact": row.get("exact", False)}
        entries.append({"level": level, "metrics": metrics, "score": score(level, metrics), "origin": source_id})
    catalog, curve, renumber = build_catalog(entries, levels)
    remapped_hints = {str(renumber[int(old)]): route for old, route in hints.items()}
    os.makedirs(out_dir, exist_ok=True)
    os.makedirs(report_dir, exist_ok=True)
    write_json(os.path.join(out_dir, "levels.json"), {"levels": catalog})
    write_json(os.path.join(out_dir, "hints.json"), dict(sorted(remapped_hints.items(), key=lambda item: int(item[0]))))
    write_json(os.path.join(report_dir, "curve.json"), curve)
    write_json(os.path.join(report_dir, "renumber.json"), renumber)
    print("Wrote a 210-level reorder proposal with unchanged pars, remapped hints and a migration table. "
          "Difficulty approval and save migration are still required before delivery.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--reorder", action="store_true", required=True,
                        help="propose a complete reorder; the former expansion mode is retired")
    for name in ("levels", "hints", "curve", "output", "report"):
        parser.add_argument(name)
    args = parser.parse_args()
    reorder(args.levels, args.hints, args.curve, args.output, args.report)
