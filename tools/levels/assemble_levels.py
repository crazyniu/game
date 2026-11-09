"""Assemble the level catalog: the authored levels plus generated candidates, ordered along one difficulty curve.

python3 tools/levels/assemble_levels.py <existing metrics.json> <candidates dir> <redeal dir> <out dir>

Inputs come from tools/levels/level_lab.gd. Writes <out dir>/levels.json (catalog), renumber.json (authored level id ->
new position), curve.json (one record per level). The score is a single difficulty number: moves needed, colours,
how rarely random play wins, and how many mechanics are in play.
"""
import glob
import json
import os
import sys

UNLOCK = [{"egg"}, {"sleep"}, {"mama"}, {"crow", "twig"}, {"swivel"}, {"growing"}, {"daynight"}, set(), set(), set()]
TARGET = [8.0, 14.0, 17.0, 20.0, 23.0, 26.0, 29.0, 32.0, 35.0, 38.0]
RAMP = 2.5                    # a chapter runs from its target minus this to its target plus this
MAX_ROWS = [4, 5, 6, 6, 6, 6, 6, 6, 6, 6]
VARIANTS = [["morning", "noon", "golden"], ["mist", "noon", "drizzle"], ["morning", "noon", "windy"], ["morning", "noon", "bluehour"]]
SPECIALS = ["rain", "lanterns", "sunset", "aurora"]
FIRST_CHAPTER_GOAL = 12.0     # the one generated level of chapter 1 sits near its end
SPIKE = {9}                   # an authored level too hard for its old place; it moves to its own difficulty
TEACH = {1: "sleep", 2: "mama", 3: "crow", 4: "swivel", 5: "growing", 6: "daynight"}


def score(m):
    return round(m["counted"] * 0.6 + m["species"] + (1.0 - m["win_rate"]) * 10.0 + len(m["mechanics"]), 2)


def allowed(chapter):
    out = set()
    for c in range(chapter + 1):
        out |= UNLOCK[c]
    return out


def first_chapter(mechanics):
    for c in range(10):
        if set(mechanics) <= allowed(c):
            return c
    raise ValueError(mechanics)


def rows_of(level):
    return max(b["row"] for b in level["branches"]) + 1


def main():
    metrics_path, cand_dir, redeal_dir, out_dir = sys.argv[1:5]
    root = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
    authored = {int(l["id"]): l for l in json.load(open(os.path.join(root, "PerchSort/levels/levels.json")))["levels"]}
    existing = []
    for m in json.load(open(metrics_path)):
        existing.append({"level": authored[int(m["id"])], "metrics": m, "score": score(m), "origin": int(m["id"])})
    # Authored levels too easy for any place past their mechanic's debut are dealt again over the same branches.
    for path in sorted(glob.glob(os.path.join(redeal_dir, "*.json"))):
        options = json.load(open(path))
        for e in existing:
            if e["origin"] == options[0]["origin"]:
                goal = TARGET[first_chapter(e["metrics"]["mechanics"])]
                best = min(options, key=lambda c: abs(score(c["metrics"]) - goal))
                e.update(level=best["level"], metrics=best["metrics"], score=score(best["metrics"]), redealt=True)
    pool = []
    for path in sorted(glob.glob(os.path.join(cand_dir, "*.json"))):
        for c in json.load(open(path)):
            pool.append({"level": c["level"], "metrics": c["metrics"], "score": score(c["metrics"]), "origin": 0})

    chapters = [[] for _ in range(10)]
    placed = set()
    # Chapter 1 keeps the first ten authored levels; each new mechanic is taught by its easiest authored level.
    for e in existing:
        if e["origin"] <= 10 and e["origin"] not in SPIKE:
            chapters[0].append(e)
            placed.add(e["origin"])
    teach = {}
    for c, mech in TEACH.items():
        options = [e for e in existing if e["origin"] not in placed and mech in e["metrics"]["mechanics"]
                   and first_chapter(e["metrics"]["mechanics"]) <= c]
        best = min(options, key=lambda e: e["score"])
        e = dict(best)
        e["teach"] = mech
        chapters[c].append(e)
        placed.add(e["origin"])
        teach[c] = e["origin"]
    # Every other authored level goes to the allowed chapter whose target it is closest to.
    for e in sorted(existing, key=lambda e: e["score"]):
        if e["origin"] in placed:
            continue
        low = first_chapter(e["metrics"]["mechanics"])
        options = [c for c in range(low, 10) if len(chapters[c]) < 6 and rows_of(e["level"]) <= MAX_ROWS[c]]
        c = min(options, key=lambda c: abs(TARGET[c] - e["score"]))
        chapters[c].append(e)
        placed.add(e["origin"])
    # Generated levels fill each chapter towards evenly spaced scores.
    used = set()
    for c in range(10):
        gaps = 10 - len(chapters[c])
        goals = [FIRST_CHAPTER_GOAL] if c == 0 else [TARGET[c] - RAMP + 2 * RAMP * (i + 0.5) / gaps for i in range(gaps)]
        for goal in goals:
            fits = [i for i, p in enumerate(pool) if i not in used and set(p["metrics"]["mechanics"]) <= allowed(c)
                    and rows_of(p["level"]) <= MAX_ROWS[c]]
            pick = min(fits, key=lambda i: abs(pool[i]["score"] - goal))
            used.add(pick)
            chapters[c].append(dict(pool[pick]))

    catalog, curve, renumber = [], [], {}
    for c in range(10):
        taught = [e for e in chapters[c] if e.get("teach")]
        rest = sorted([e for e in chapters[c] if not e.get("teach")], key=lambda e: e["score"])
        ordered = taught + rest
        cycle = VARIANTS[c % 4]
        for i, e in enumerate(ordered):
            number = c * 10 + i + 1
            level = json.loads(json.dumps(e["level"]))
            level["id"] = number
            level["world"] = c + 1
            level["variant"] = SPECIALS[c % 4] if i == 9 else cycle[i % 3]
            if number == 1:
                level["variant"] = "noon"
            m = e["metrics"]
            slack = 1 if m.get("exact", True) and m["counted"] < 12 else 2
            level["par"] = int(e["level"]["par"]) if e["origin"] and not e.get("redealt") else int(m["counted"]) + slack
            catalog.append(level)
            if e["origin"]:
                renumber[e["origin"]] = number
            curve.append({"level": number, "chapter": c + 1, "score": e["score"], "counted": m["counted"], "par": level["par"],
                          "species": m["species"], "branches": m["branches"], "win": round(m["win_rate"], 2), "exact": m.get("exact", True),
                          "mechanics": m["mechanics"], "origin": e["origin"], "redealt": bool(e.get("redealt")), "teach": e.get("teach", "")})
    os.makedirs(out_dir, exist_ok=True)
    json.dump({"levels": catalog}, open(os.path.join(out_dir, "levels.json"), "w"), indent=1)
    json.dump(renumber, open(os.path.join(out_dir, "renumber.json"), "w"))
    json.dump(curve, open(os.path.join(out_dir, "curve.json"), "w"))
    for c in range(10):
        rows = [r for r in curve if r["chapter"] == c + 1]
        print("chapter", c + 1, " ".join("%.0f%s" % (r["score"], "t" if r["teach"] else ("a" if r["origin"] else "")) for r in rows))


if __name__ == "__main__":
    main()
