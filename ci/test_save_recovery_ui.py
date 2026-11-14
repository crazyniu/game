#!/usr/bin/env python3
"""Run the real startup/retry flow with disposable user data, never a player profile."""
import os
from pathlib import Path
import subprocess
import tempfile
import sys

if sys.platform != "linux":
    print("save recovery UI checks: skipped (Linux XDG isolation only)")
    sys.exit(0)

root = Path(__file__).resolve().parents[1]
with tempfile.TemporaryDirectory(prefix="perch-save-recovery-") as temporary:
    base = Path(temporary)
    env = os.environ.copy()
    for variable, folder in (
        ("XDG_DATA_HOME", "data"),
        ("XDG_CONFIG_HOME", "config"),
        ("XDG_CACHE_HOME", "cache"),
    ):
        location = base / folder
        location.mkdir()
        env[variable] = str(location)
    env["PERCH_RECOVERY_TEST"] = env["XDG_DATA_HOME"]
    result = subprocess.run(
        [env.get("GODOT", "godot"), "--headless", "--path", str(root / "PerchSort"),
         "--audio-driver", "Dummy", "-s", "res://tests/run_save_recovery_ui.gd"],
        env=env, text=True, capture_output=True, timeout=90,
    )
    print(result.stdout, end="")
    print(result.stderr, end="")
    assert result.returncode == 0, "Save recovery UI regression failed"
    assert "save recovery UI checks: 0 failures" in result.stdout
    assert "SCRIPT ERROR" not in result.stderr, "GDScript runtime error"

    env["PERCH_RECOVERY_EXIT"] = "1"
    result = subprocess.run(
        [env.get("GODOT", "godot"), "--headless", "--path", str(root / "PerchSort"),
         "--audio-driver", "Dummy", "-s", "res://tests/run_save_recovery_ui.gd"],
        env=env, text=True, capture_output=True, timeout=30,
    )
    assert result.returncode == 0 and "save recovery exit requested" in result.stdout
    assert "SCRIPT ERROR" not in result.stderr
    primary = list((base / "data").rglob("profile.json"))
    assert len(primary) == 1
    assert primary[0].read_text() == "{broken"
    assert Path(str(primary[0]) + ".bak").read_text() == "not-json"
    print("PASS Exit terminates the game and preserves both damaged files")
