"""Offline level tooling for Perch Sort rules v4 (documentation only, not runtime).

Model: docs/spec/RULES.md v4. Per-branch capacity, short twigs and open/lean/tight layouts are
PROPOSALS from docs/GAMEPLAY_REVIEW.md, not effective rules; with all capacities = N, two empty
branches and no stacked eggs this is exactly RULES v4. Every branch has its own capacity (standard = N, short < N);
a move takes the exposed run of same-species hatched birds, Count = min(run, free);
the exposed egg on the source hatches in place; a branch holding exactly N hatched birds
of one species flies home and retires. Short branches never complete.

Subcommands:
  analyze <levels.json>                 print metrics for every level
  generate <plan.json> <levels.json>    search fixed candidates that meet each plan row
  render <levels.json> <docs/levels>    write FIXED_*.md appendices from the JSON
"""
import json, random, sys
from collections import deque

# ---------- rules ----------

def start_state(level):
    sh = []
    for branch in level["branches"]:
        toks = []
        for t in branch["tokens"]:
            toks.append((t[-1], 1) if t.startswith("?") else (t, 0))
        sh.append(tuple(toks))
    caps = tuple(b["capacity"] for b in level["branches"])
    return (tuple(sh), tuple([False] * len(sh)), frozenset()), caps

def legal_moves(state, caps):
    sh, ret, _ = state
    for i, s in enumerate(sh):
        if ret[i] or not s or s[-1][1]:
            continue
        top = s[-1]; run = 0
        for t in reversed(s):
            if t != top: break
            run += 1
        for j, d in enumerate(sh):
            if i == j or ret[j] or len(d) >= caps[j]:
                continue
            if d and d[-1] != top:
                continue
            yield i, j, min(run, caps[j] - len(d)), run

def apply_move(state, move, caps, N):
    sh, ret, home = state; i, j, count = move[:3]
    sh = list(sh); ret = list(ret); home = set(home); events = []
    src, dst = list(sh[i]), list(sh[j])
    dst += src[-count:]; del src[-count:]
    if src and src[-1][1]:
        src[-1] = (src[-1][0], 0); events.append(("Hatch", i, src[-1][0]))
    sh[i], sh[j] = tuple(src), tuple(dst)
    for k in sorted((i, j)):
        b = sh[k]
        if caps[k] >= N and len(b) == N and all(t == (b[0][0], 0) for t in b):
            home.add(b[0][0]); sh[k] = (); ret[k] = True
            events.append(("Home", k, b[0][0]))
    return (tuple(sh), tuple(ret), frozenset(home)), events

def canon(state, caps):
    sh, ret, home = state
    return (tuple(sorted(zip(caps, sh, ret))), home)

# ---------- analysis ----------

def explore(level, cap=1_500_000):
    N, K = level["N"], level["K"]
    s0, caps = start_state(level)
    k0 = canon(s0, caps); seen = {k0: s0}; edges = {}; q = deque([k0])
    while q:
        k = q.popleft(); outs = []
        for mv in legal_moves(seen[k], caps):
            ns, _ = apply_move(seen[k], mv, caps, N); nk = canon(ns, caps); outs.append(nk)
            if nk not in seen:
                seen[nk] = ns; q.append(nk)
                if len(seen) > cap: return None
        edges[k] = outs
    rev = {}
    for k, outs in edges.items():
        for o in outs: rev.setdefault(o, []).append(k)
    togo = {k: 0 for k, s in seen.items() if len(s[2]) == K}; q = deque(togo)
    while q:
        k = q.popleft()
        for p in rev.get(k, []):
            if p not in togo: togo[p] = togo[k] + 1; q.append(p)
    return {"states": len(seen), "dead": len(seen) - len(togo), "opt": togo.get(k0),
            "first_moves": len(edges[k0]), "first_dead": sum(o not in togo for o in edges[k0]),
            "togo": togo, "caps": caps, "s0": s0}

def random_win_rate(level, trials=300, limit=400, seed=1):
    rnd = random.Random(seed); N, K = level["N"], level["K"]; s0, caps = start_state(level); wins = 0
    for _ in range(trials):
        s = s0
        for _ in range(limit):
            mvs = list(legal_moves(s, caps))
            if not mvs: break
            s, _ = apply_move(s, rnd.choice(mvs), caps, N)
            if len(s[2]) == K: wins += 1; break
    return wins / trials

def witness(level, ex):
    """Optimal path with real branch ids, by descending the proven distance-to-win."""
    N = level["N"]; caps = ex["caps"]; s = ex["s0"]; togo = ex["togo"]; path = []
    while togo[canon(s, caps)] > 0:
        need = togo[canon(s, caps)] - 1
        for mv in legal_moves(s, caps):
            ns, ev = apply_move(s, mv, caps, N)
            if togo.get(canon(ns, caps)) == need:
                path.append((mv, ev, ns)); s = ns; break
        else:
            raise RuntimeError("witness descent failed")
    return path

def structure(level):
    segs = eggs = stacked = 0
    for b in level["branches"]:
        prev = None
        for t in b["tokens"]:
            if t != prev: segs += 1
            if t.startswith("?"):
                eggs += 1
                if prev and prev.startswith("?"): stacked += 1
            prev = t
    return {"segments": segs, "eggs": eggs, "stacked_egg_pairs": stacked,
            "short": sum(b["capacity"] < level["N"] for b in level["branches"]),
            "empty": sum(not b["tokens"] for b in level["branches"])}

def metrics(level, trials=300):
    ex = explore(level)
    if ex is None or ex["opt"] is None: return None, ex
    path = witness(level, ex)
    m = {"opt": ex["opt"], "states": ex["states"],
         "dead_pct": round(100 * ex["dead"] / ex["states"], 1),
         "first_dead": ex["first_dead"], "first_moves": ex["first_moves"],
         "random_win_pct": round(100 * random_win_rate(level, trials), 1),
         "witness_truncated": sum(mv[2] < mv[3] for mv, _, _ in path),
         "start_truncating_moves": sum(mv[2] < mv[3] for mv in legal_moves(ex["s0"], ex["caps"])),
         "witness_hatches": sum(e[0] == "Hatch" for _, ev, _ in path for e in ev),
         "witness_short_use": sum(level["branches"][mv[1]]["capacity"] < level["N"] for mv, _, _ in path)}
    m.update(structure(level))
    return m, ex

# ---------- generation ----------

LETTERS = "ABCDEFGHIJ"

def deal(row, rnd):
    N, K, layout = row["N"], row["K"], row["layout"]
    shorts = row.get("short", [])
    toks = [LETTERS[s] for s in range(K) for _ in range(N)]; rnd.shuffle(toks)
    if layout == "open":   sizes = [N] * K + [0, 0]
    elif layout == "lean": sizes = [N] * K + [0]
    elif layout == "tight":
        P = K + 2
        while True:
            sizes = [N] * P; free = 2 * N
            while free:
                i = rnd.randrange(P)
                if sizes[i] > 1: sizes[i] -= 1; free -= 1
            if 0 not in sizes and sum(s < N for s in sizes) >= 3: break
    else: raise ValueError(layout)
    branches = []; pos = 0
    for size in sizes:
        branches.append({"capacity": N, "tokens": toks[pos:pos + size]}); pos += size
    branches += [{"capacity": c, "tokens": []} for c in shorts]
    rnd.shuffle(branches)
    for b in branches:
        t = b["tokens"]
        if len(t) == N and len(set(t)) == 1: return None   # no complete group at start
    slots = [(bi, ti) for bi, b in enumerate(branches) for ti in range(len(b["tokens"]) - 1)]
    rnd.shuffle(slots)
    want, stacked = row.get("eggs", 0), row.get("stacked", 0)
    chosen = set()
    if stacked:
        pairs = [(bi, ti) for bi, ti in slots if ti + 1 < len(branches[bi]["tokens"]) - 1]
        for bi, ti in pairs[:stacked]: chosen |= {(bi, ti), (bi, ti + 1)}
    for s in slots:
        if len(chosen) >= want: break
        chosen.add(s)
    if len(chosen) != want: return None
    for bi, ti in chosen:
        branches[bi]["tokens"][ti] = "?" + branches[bi]["tokens"][ti]
    return {"N": N, "K": K, "branches": branches}

def in_band(m, row):
    lo, hi = row["random_win"]
    if not (lo <= m["random_win_pct"] <= hi): return False
    if m["opt"] > row.get("max_opt", 99) or m["opt"] < row.get("min_opt", 0): return False
    if m["dead_pct"] < row.get("min_dead", 0): return False
    if m["stacked_egg_pairs"] < row.get("stacked", 0): return False
    for key in row.get("require", []):        # e.g. witness_truncated, witness_short_use
        if m[key] < 1: return False
    return True

def search(row, attempts=2500):
    """Return (level, metrics, attempt, exact) for one plan row; best near-miss if no exact hit."""
    rnd = random.Random(row["id"] * 7919); lo, hi = row["random_win"]; near = None
    for attempt in range(attempts):
        lv = deal(row, rnd)
        if lv is None: continue
        quick = 100 * random_win_rate(lv, trials=60, seed=attempt)
        if not (lo - 20 <= quick <= hi + 20): continue
        m, _ = metrics(lv)
        if not m: continue
        if in_band(m, row): return lv, m, attempt, True
        gap = max(lo - m["random_win_pct"], m["random_win_pct"] - hi, 0)
        if all(m[k] >= 1 for k in row.get("require", [])) and (near is None or gap < near[0]):
            near = (gap, lv, m, attempt)
    if near is None: raise RuntimeError(f"level {row['id']}: no candidate")
    return near[1], near[2], near[3], False

def _job(row):
    lv, m, attempt, exact = search(row)
    lv.update({k: row[k] for k in ("id", "world", "role", "title", "layout", "teach")})
    lv["source"] = f"offline search seed {row['id'] * 7919}, attempt {attempt}; frozen" + ("" if exact else "; NEAR-MISS")
    lv["metrics"] = m
    return lv

def generate(plan_path, out_path):
    from multiprocessing import Pool
    plan = json.load(open(plan_path, encoding="utf-8"))
    with Pool() as pool:
        out = []
        for lv in pool.imap(_job, plan["levels"]):
            m = lv["metrics"]; out.append(lv)
            print(f"{lv['id']:02d} {lv['world']} N{lv['N']} K{lv['K']} P{len(lv['branches'])} {lv['layout']:5s} "
                  f"opt={m['opt']:2d} dead={m['dead_pct']:5.1f}% rand={m['random_win_pct']:5.1f}% eggs={m['eggs']} "
                  f"stack={m['stacked_egg_pairs']} short={m['short']} states={m['states']}"
                  f"{'  NEAR-MISS' if 'NEAR' in lv['source'] else ''}", flush=True)
    json.dump({"rules": 4, "levels": out}, open(out_path, "w", encoding="utf-8"), ensure_ascii=False, indent=1)

if __name__ == "__main__":
    cmd = sys.argv[1]
    if cmd == "generate": generate(sys.argv[2], sys.argv[3])
    elif cmd == "analyze":
        for lv in json.load(open(sys.argv[2], encoding="utf-8"))["levels"]:
            m, _ = metrics(lv); print(lv["id"], m)
    elif cmd == "render":
        import render_levels; render_levels.render(sys.argv[2], sys.argv[3])
    else: raise SystemExit(__doc__)
