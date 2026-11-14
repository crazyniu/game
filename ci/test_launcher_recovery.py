"""Exercise the Unix launcher without a real user profile or launching a game window."""
import json
import os
from pathlib import Path
import subprocess
import tempfile
import sys

if sys.platform != "linux":
    print("launcher recovery: Linux mock-launcher check skipped on this platform")
    raise SystemExit(0)

root = Path(__file__).resolve().parents[1]
with tempfile.TemporaryDirectory(prefix="perch-launcher-check-") as temp:
    base = Path(temp)
    stub = base / "godot"
    stub.write_text('#!/bin/sh\nif [ "$1" = "--version" ]; then echo 4.7.2; fi\nprintf "%s\\n" "$*" >> "$LAUNCH_LOG"\n')
    stub.chmod(0o755)
    env = dict(os.environ, HOME=str(base), GODOT=str(stub), XDG_DATA_HOME=str(base / "data"), LAUNCH_LOG=str(base / "launch.log"))
    directory = base / "data/godot/app_userdata/Perch Sort"
    directory.mkdir(parents=True)
    primary = directory / "profile.json"
    backup = directory / "profile.json.bak"
    good = json.dumps({"schemaVersion": 5, "coins": 1234, "highestUnlockedLevel": 40})

    def launch(*args):
        (base / "launch.log").write_text("")
        subprocess.run(["bash", str(root / "run.sh"), *args], env=env, check=True, capture_output=True, text=True)
        return (base / "launch.log").read_text()

    primary.write_text("broken")
    backup.write_text(good)
    assert "--unlimited" not in launch()
    assert primary.read_text() == "broken" and backup.read_text() == good
    primary.unlink()
    assert "--unlimited" not in launch()
    assert not primary.exists() and backup.read_text() == good
    backup.unlink()
    primary.write_text("broken")
    assert "--unlimited" not in launch()
    assert primary.read_text() == "broken"
    backup.write_text(good)
    for suffix in (".tmp", ".bak.tmp"):
        Path(str(primary) + suffix).write_text("unfinished")
    launch("--reset", "--preview=home_view")
    assert all(not Path(str(primary) + s).exists() for s in ("", ".bak", ".tmp", ".bak.tmp"))
    assert "--unlimited" in launch()
    assert json.loads(primary.read_text())["coins"] == 50000
print("launcher recovery: five scenarios passed (mock engine, isolated temporary profiles)")
