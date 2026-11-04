#!/usr/bin/env python3
"""Render the delivery report only from completed validation and install evidence."""
import json
import xml.etree.ElementTree as ET
from pathlib import Path

WORK = Path(__file__).resolve().parent
ROOT = WORK.parents[2]


def percent(value):
    return f'{value * 100:.6f}'.rstrip('0').rstrip('.') + '%'


def main():
    frozen = json.loads((WORK / 'staged-deep-evidence.json').read_text())
    live = json.loads((WORK / 'live-deep-evidence.json').read_text())
    installed = json.loads((WORK / 'install-result.json').read_text())
    fast = json.loads((WORK / 'final-fast-screen.json').read_text())
    if fast['Milliseconds'] != 1000 or any(not r['Pass'] for r in fast['Levels']):
        raise RuntimeError('Replacement solver screening failed')
    fast_rows = {r['Id']: r for r in fast['Levels']}
    if set(fast_rows) != set(installed['ChangedIds']):
        raise RuntimeError('Screening did not cover every replacement')
    tests = ET.parse(WORK / 'installed-data-tests.xml').getroot()
    if tests.attrib['result'] != 'Passed' or tests.attrib['passed'] != '81':
        raise RuntimeError('Installed data tests did not pass 81/81')
    for report in (frozen, live):
        if report['CatalogFailures'] or len(report['Levels']) != 40 or not all(r['Pass'] for r in report['Levels']):
            raise RuntimeError('Full catalog evidence is not a clean pass')
    old = {l['id']: l for l in json.loads((WORK / 't35-levels.json').read_text())['levels']}
    rows = live['Levels']
    for a, b in zip(frozen['Levels'], rows):
        for key in ('Id', 'Par', 'RandomWins', 'K', 'Branches', 'Mechanics', 'Role'):
            if a[key] != b[key]:
                raise RuntimeError(f'Frozen/live gameplay difference: L{a["Id"]} {key}')
    intents = {
        12: 'Rebuild the sleeping-flock practice around Cocoa before Berry at the proved decision; Bell still saves two real moves.',
        13: 'Uncover the eggs while prioritizing Mint over Sky; extend the wake/gift lesson to seven actions with a two-flock cascade.',
        16: 'Use Mama/chick regrouping and egg reveals before the Cocoa-first decision rather than collapsing the board in four actions.',
        17: 'Require ten-action sleeper sorting while preserving the immediate 2>4 losing temptation and the Sky-before-Berry advantage.',
        18: 'Keep the Mama lesson forgiving, but spread it over six actions before the three-flock rest payoff.',
        19: 'Stage the sleeping groups before choosing Cocoa rather than Mint; keep a two-action losing opening in a twelve-action challenge.',
        20: 'Make the lantern finale a ten-action setup into a three-flock cascade, with an immediate bad 1>4 opening.',
        22: 'Force eight-action crow parking and release before a three-flock payoff; one Scarecrow still saves three actions.',
        23: 'Combine crow obstruction, hidden eggs and sleepers in eight actions; Sky-first beats Snow-first at the proved decision.',
        25: 'Retain the two-action twig-space trap while stretching the Sky-before-Snow decision into a twelve-action challenge.',
        26: 'Use Inky in eight-action egg/wake practice with extra crow occupancy; Sky-first beats Berry-first by three actions without a large search frontier.',
        27: 'Use the swivel and constrained crow parking over fourteen actions; preserve the immediate 5>1 trap and Berry-before-Mint choice.',
        29: 'Require sixteen actions around the crowded parking branch; preserve the immediate 4>1 trap and Cocoa-before-Ember decision.',
        30: 'Turn crowded crow parking into a nineteen-action sunset challenge with an immediate 8>1 losing temptation and Cocoa-before-Ember ordering.',
        34: 'Mix older crow/egg/swivel geometry into nine-action practice; Sky-before-Mint remains the cheaper proved decision.',
        36: 'Exercise eggs, growing seats and the day/night clock over twelve actions on eight branches; retain a two-action dead branch and a three-flock payoff.',
        37: 'Build a nine-action setup for the five-flock payoff rather than a two-action auto-clear; Sky-first beats Berry-first by two actions.',
        38: 'Exercise night timing and newly grown seats across thirteen actions on eight branches; Snow-first beats Hoot-first and a two-action opening is proven dead.',
        39: 'Keep the day/night Mama rest at six actions with a two-flock payoff and a clear Sunny-before-Sky advantage.',
        40: 'Replace the three-action auto-finale with twelve-action seven-species planning on nine branches; Cocoa-first beats Hoot-first by four actions and the optimal line uses growing seats before a three-flock cascade.'
    }
    lines = [
        '# T42 delivery report', '', '## Result', '',
        '- Atomically installed **20 replacement boards**; all other boards are unchanged, including every T level, levels 1–6, L7 (par 13) and L32 (par 8).',
        '- Full **DEEP validation: 40/40 PASS, exit 0** on both the frozen Core and the final live Core, with the explicitly authorized exceptions below; every other T35 gate remains active.',
        '- Existing live-Core `LevelsDataTests`: **81 passed, 0 failed, 0 skipped** on both the staged catalog and the installed resource, using NUnitLite 3.13.3, a separate Core assembly and a Linux file-backed Resources/JSON adapter.',
        '- No Core, shared level-tool, scene, artwork or existing test source was edited by T42; no commit or push.',
        '- Authority: [T42 task](T42_level_deepening.md), [game design §§11–12](../docs/GAME_DESIGN.md#11-level-plan-40-levels), coordinator clarifications `msg_06bfe96858d7`, `msg_36c158aa8784` and Mac-budget decision `msg_56134b267b23`.',
        '', '## Named gates and explicit exceptions', '',
        '- P/C enforce **lower bounds only**: P ≥6 from L7 / ≥8 from L21; C ≥10 from L8 / ≥12 from L21. Par >24 is a report warning, not rejection; no installed level triggers it. R from L7 remains 3–6; no re-role was used. T and levels 1–6 retain their original gates.',
        '- **DEADEND_WINDOW_6 (D6):** C levels with par ≥12 may use a proven losing state reachable within six legal actions. Only final L40 actually needs more than the old three-action window (six actions); the repaired L30/L36/L38 witnesses fit the original window.',
        '- **FINALE_PAR10 (F10):** L40 may use par ≥10 instead of the original ≥16 target. Its installed par is 12; the six-action dead witness uses D6 as well.',
        '- Existing **EARLY_WARMUP_L4_L5 (EW)** and **SAPLING_INTRO_L4 (S4)** remain scoped to their unchanged levels.',
        '- These are disclosed acceptance changes, not claims that the unmodified original T35 three-action gate or the original L40 ≥16 target passed.',
        '', '## Full final live-Core validation table', '',
        '1,000 seeded uniform-legal-action random trials per level, 200-action cap, seed 350053; no undo or booster. Search limits: 60,000 ms / 20,000,000 states / 4096 MiB per search. `bound` is an interval **over explored states only**, not over the unenumerated whole graph. `O` = proved ordering decision, `D<n>` = proved dead branch in n actions, `H<n>` = maximum simultaneous homecomings on the optimal public-session replay; `—` = ordering not required. Applicable named exceptions are listed even when their relaxed limit is not needed.',
        '', '| ID | K | Branches | Mechanics | Role | Par old → new | Random wins | Dead-state share | Witnesses | Core ms (1000 cap) | Exceptions | Gate |',
        '|---:|---:|---:|---|---|---:|---:|---|---|---:|---|---|'
    ]
    short = {'DEADEND_WINDOW_6': 'D6', 'FINALE_PAR10': 'F10', 'EARLY_WARMUP_L4_L5': 'EW', 'SAPLING_INTRO_L4': 'S4'}
    for row in rows:
        dead = percent(row['DeadLower']) if row['Closed'] else 'bound [' + percent(row['DeadLower']) + ', ' + percent(row['DeadUpper']) + ']'
        witnesses = ['O' if row['Id'] >= 3 else '—']
        if row['Role'] == 'C':
            witnesses.append('D' + str(len(row['TemptingDeadLine'])))
        witnesses.append('H' + str(row['MaximumHomesInOneMove']))
        exceptions = ', '.join(short[x] for x in row['AppliedExceptions']) or '—'
        lines.append(f'| {row["Id"]} | {row["K"]} | {row["Branches"]} | {", ".join(row["Mechanics"]) or "core"} | {row["Role"]} | {old[row["Id"]]["par"]} → {row["Par"]} | {row["RandomWins"]}/1000 | {dead} | {", ".join(witnesses)} | {fast_rows[row["Id"]]["Milliseconds"] if row["Id"] in fast_rows else "—"} | {exceptions} | PASS |')
    lines += ['', '## One-line intent for every changed level', '']
    lines += [f'- **L{level_id}:** {intents[level_id]}' for level_id in installed['ChangedIds']]
    lines += ['', '## Search limits and unmet original target', '',
              '- No level remains below its **final authorized** floor; no shallow T35 fallback was installed for L30/L36/L38/L40.',
              '- The original L40 ≥16 target was **not reached**: its original 640 proposals (160 at each of 2/4/6/8 swaps) found a best solver-proven in-rate par of 10; deeper proposals were Unknown under the 200,000-state / 3,000 ms Core-solver screening budget, not proven impossible. Re-evaluating the same stream with FINALE_PAR10 initially accepted proposal 135 (par 10, 122/1000 wins); the final constrained-space search improved this to par 12, 164/1000 wins, at proposal 62.',
              '- L30/L36/L38 each originally exhausted 640 proposals under the three-action dead-window gate; best proven in-rate pars were 15/17/12. Deterministic recovery accepted L30 six-swap proposal 74, L36 four-swap proposal 106 and L38 six-swap proposal 75 after the authorized window expansion and full proof. L38 proposal 38 hit a time-budget Unknown during recovery; proposal 75 passed instead.',
              '- Initial search analysis used 10,000 ms / 2,000,000 states / 2048 MiB, then all installed boards received the complete 60,000 ms proof settings above. Rate-rejected proposals and Unknown searches were never installed as accepted boards.',
              '- Initial 250-proposal L7 upper-band search was superseded by the floor-only clarification; L7 remains exactly unchanged at par 13.',
              '', '## Post-install failure and correction', '',
              '- The initial ten-branch par-10 finale passed both full validators and the staged 81-case fixture, but the first installed-resource fixture run failed only L40 (80/81 passed): its Core solver exceeded the unchanged 3,000 ms limit. This failure is preserved in `initial-installed-data-tests.xml`; no budget or test was loosened.',
              '- Removed one empty normal branch from that authored finale, kept both growing branches, restored alternating row geometry, and searched token arrangements with a stricter **1,000 ms generation screen**. Proposal 62 is par 12 on nine branches and exercises all five of its listed mechanics on the optimal line. The changed fixture case took 0.058473 s in the staged full run; final installed results are linked below. This is observed Linux timing, not a Mac performance guarantee.',
              '- The coordinator then reported actual Mac Unity BudgetExceeded failures for the earlier L30/L36 boards. These observations were accepted without re-running the Mac check. A new 1,000 ms live-Core screen over every replacement rejected L26/L27/L29/L30/L36/L37/L38 on 251 as well; all seven were replaced, while all other replacements stayed unchanged.',
              '- The final **20/20 replacements pass 1,000 ms / 200,000 states** on 251; measured times appear in the full table (maximum 787 ms). L36 traded its earlier 17-action proposal for bounded proof rather than increasing any test limit; no final authorized floor shortfall was necessary. These Linux measurements do not claim a new Mac test pass.',
              '- The repaired World-3 boards use four additional existing crow tokens to occupy formerly empty space; ordinary flock counts remain exactly four, the required twig and all species/mechanic debuts remain unchanged. L36/L38 remove an unused normal branch, retain both growing branches, and restore alternating geometry; L37 keeps its original branch count. These are authored-board configurations, not new mechanics or solver pruning.',
              '- Strict-screen repair effort, exact seeds and all rejected/Unknown proposals are recorded in `fast-repair-summary.json` and the `fast-l*-spec.json` / result / log files; `repair_fast.py 30 36 26 27 29 37 38` reproduces the order. Every installed board still passes the random role band, exact par, ordering and applicable dead-window requirements.',
              '', '## Booster value and gifts', '',
              '| Level | Booster | Proven value |', '|---:|---|---|']
    for row in rows:
        booster = row.get('Booster')
        if booster:
            value = ('covered-egg reveal-count proxy ' + str(booster['RevealCountProxy'])) if booster['Kind'] == 'OwlEye' else f'{row["Par"]} → {booster["MinimumMoves"]} real moves; saves {booster["MovesSaved"]}'
            lines.append(f'| {row["Id"]} | {booster["Kind"]} | {value} |')
    lines += ['', '- All L3–40 ordering witnesses are proven and publicly replayed; every changed board exercises the Berry gift event on its optimal replay, and all except L17/L19 include a multi-flock optimal cascade. Berry is a counted-move gift in final live Core, not the removed Song meter. Owl Eye remains the approved reveal-count proxy, not a blind-play fairness proof.',
              '', '## Frozen/live comparison and verification boundaries', '',
              '- The captured Core predates the completed T41 cutover: six source files differ. The frozen Core has Song state and no Encore board field; live Core removes Song and adds Encore/CountedMoves. The isolated codec preserves the live Encore bit under `LIVE_CORE`; the frozen build reserves that byte as zero. No gameplay rules are reimplemented.',
              '- All 40 frozen/live rows agree on exact par, random wins, species count, branch count, mechanic set and role; all proof/data gates pass. Graph sizes/dead-state intervals may differ because live Core retains Encore state and bounded graph searches stop at different explored frontiers; these intervals are not claimed identical.',
              '- Par and proof depths count `State.Moves` (real transfers/swivels), not the Encore-discounted `GameSession.MoveCount`; the updated existing data test asserts the same contract.',
              '- Linux tests execute real live Core and the unchanged fixture source through an adapter. Unity Editor import/JsonUtility behavior, Mac compilation, rendering, input and animation were **not run** here.',
              '', '## Exact commands', '', 'Run from the repository root; SDK is the existing local 8.0.419 installation.', '', '```sh',
              'mkdir -p tools/levels/work/core_snapshot',
              'cp -a PerchSort/Assets/Scripts/Core/. tools/levels/work/core_snapshot/',
              '/tmp/perch-t32-sdk/dotnet build tools/levels/work/DeepLevelTool -c Release',
              'python3 tools/levels/work/search_world.py 2 --attempts 160 --swaps 2 4 6 8',
              'python3 tools/levels/work/search_world.py 3 --attempts 160 --swaps 2 4 6 8',
              'python3 tools/levels/work/search_world.py 4 --attempts 160 --swaps 2 4 6 8',
              '/tmp/perch-t32-sdk/dotnet tools/levels/work/DeepLevelTool/bin/Release/net8.0/LevelTool.dll recover tools/levels/work/w3-l30-s6-spec.json 74 tools/levels/work/recovered-l30.json',
              '/tmp/perch-t32-sdk/dotnet tools/levels/work/DeepLevelTool/bin/Release/net8.0/LevelTool.dll recover tools/levels/work/w4-l36-s4-spec.json 106 tools/levels/work/recovered-l36.json',
              '/tmp/perch-t32-sdk/dotnet tools/levels/work/DeepLevelTool/bin/Release/net8.0/LevelTool.dll recover tools/levels/work/w4-l38-s6-spec.json 75 tools/levels/work/recovered-l38.json',
              '/tmp/perch-t32-sdk/dotnet tools/levels/work/DeepLevelTool/bin/Release/net8.0/LevelTool.dll generate --spec tools/levels/work/finale-par10-spec.json --out tools/levels/work/finale-par10-result.json',
              '/tmp/perch-t32-sdk/dotnet build tools/levels/work/LiveLevelTool -c Release -p:CoreSource=/home/developer/Projects/game/PerchSort/Assets/Scripts/Core -p:DefineConstants=LIVE_CORE',
              '/tmp/perch-t32-sdk/dotnet tools/levels/work/LiveLevelTool/bin/Release/net8.0/LevelTool.dll generate --spec tools/levels/work/finale-fast-spec.json --out tools/levels/work/finale-fast-result.json',
              'python3 tools/levels/work/repair_fast.py 30 36 26 27 29 37 38',
              'python3 tools/levels/work/stage_catalog.py',
              '/tmp/perch-t32-sdk/dotnet tools/levels/work/LiveLevelTool/bin/Release/net8.0/LevelTool.dll screen tools/levels/work/staged-levels.json 12,13,16,17,18,19,20,22,23,25,26,27,29,30,34,36,37,38,39,40 tools/levels/work/final-fast-screen.json',
              '/tmp/perch-t32-sdk/dotnet build tools/levels/work/DataTests -c Release',
              'PERCH_LEVELS_FILE=tools/levels/work/staged-levels.json /tmp/perch-t32-sdk/dotnet tools/levels/work/DataTests/bin/Release/net8.0/DataTests.dll --workers=0 --result=tools/levels/work/staged-data-tests.xml',
              '/tmp/perch-t32-sdk/dotnet tools/levels/work/DeepLevelTool/bin/Release/net8.0/LevelTool.dll validate tools/levels/work/staged-levels.json --complete --json-out tools/levels/work/staged-deep-evidence.json',
              '/tmp/perch-t32-sdk/dotnet tools/levels/work/LiveLevelTool/bin/Release/net8.0/LevelTool.dll validate tools/levels/work/staged-levels.json --complete --json-out tools/levels/work/live-deep-evidence.json',
              'python3 tools/levels/work/install_catalog.py',
              '/tmp/perch-t32-sdk/dotnet tools/levels/work/DataTests/bin/Release/net8.0/DataTests.dll --workers=0 --result=tools/levels/work/installed-data-tests.xml',
              'python3 tools/levels/work/build_report.py', '```', '',
              'Do not repeat the snapshot-copy command now: it records the historical start-of-task action and would overwrite the original frozen evidence. Generation logs/specs/results, public replay lines, exact ordering alternatives and dead witnesses remain under `tools/levels/work/`; the final JSON evidence is authoritative for each row.',
              '', '## Artifacts', '',
              '- Installed resource SHA-256: `' + installed['Sha256'] + '`.',
              '- [Final live evidence](../tools/levels/work/live-deep-evidence.json), [frozen evidence](../tools/levels/work/staged-deep-evidence.json), [installed data-test results](../tools/levels/work/installed-data-tests.xml), [atomic installation record](../tools/levels/work/install-result.json).']
    lines.append('- [Strict 1000 ms screen](../tools/levels/work/final-fast-screen.json) and [strict-screen search effort](../tools/levels/work/fast-repair-summary.json).')
    path = ROOT / 'docs/work-log/level-deepening/report.md'
    path.write_text('\n'.join(lines) + '\n')
    print('Wrote', path, 'with', len(rows), 'validated rows and', len(installed['ChangedIds']), 'changed-level intents')


if __name__ == '__main__':
    main()
