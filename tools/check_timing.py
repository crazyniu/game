#!/usr/bin/env python3
"""Compare a UiTiming log with the durations of a screen spec.

Usage: check_timing.py timing.jsonl expected.json

expected.json maps an animation name to [min_seconds, max_seconds], for example
{"shop_open": [0.22, 0.28], "level_entrance": [3.5, 4.5]}.
Every expected animation must appear with a start and an end; each occurrence must
fall inside its range. Exit code 0 when everything passes, 1 otherwise.
"""
import json
import sys


def main(log_path, expected_path):
    with open(expected_path) as f:
        expected = json.load(f)

    starts, durations = {}, {}
    with open(log_path) as f:
        for line in f:
            line = line.strip()
            if not line:
                continue
            entry = json.loads(line)
            name, phase, t = entry["name"], entry["phase"], entry["t"]
            if phase == "start":
                starts[name] = t
            elif phase == "end" and name in starts:
                durations.setdefault(name, []).append(t - starts.pop(name))

    failed = False
    for name, (low, high) in sorted(expected.items()):
        measured = durations.get(name)
        if not measured:
            print(f"MISSING  {name}: no start/end pair in the log")
            failed = True
            continue
        for value in measured:
            ok = low <= value <= high
            failed |= not ok
            print(f"{'PASS' if ok else 'FAIL'}     {name}: {value:.3f} s (expected {low}-{high} s)")
    for name in sorted(set(starts)):
        print(f"OPEN     {name}: started but never ended")
        failed = True
    return 1 if failed else 0


if __name__ == "__main__":
    if len(sys.argv) != 3:
        print(__doc__)
        sys.exit(2)
    sys.exit(main(sys.argv[1], sys.argv[2]))
