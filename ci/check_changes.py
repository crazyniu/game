"""Run fast checks only for changed source/configuration files."""
import ast
import json
import os
from pathlib import Path
import subprocess

if os.environ["EVENT_NAME"] == "workflow_dispatch":
    command = ["git", "ls-files", "-z"]
else:
    # checkout fetches the PR merge commit and its parents; parent 1 is main.
    command = ["git", "diff", "--name-only", "-z", "HEAD^1", "HEAD"]
paths = [Path(p) for p in subprocess.check_output(command).decode().split("\0") if p]
core = any(
    p.as_posix().startswith("PerchSort/Assets/Scripts/Core/")
    or p.as_posix() == "PerchSort/Assets/Tests/EditMode/CoreTests.cs"
    or p.as_posix().startswith("ci/")
    or p.as_posix() == ".github/workflows/ci.yml"
    for p in paths
)
godot = any(p.as_posix().startswith(("godot/", "scripts/")) or p.as_posix() in ("ci/check_changes.py", ".github/workflows/ci.yml") for p in paths)
workflows = any(p.as_posix().startswith(".github/workflows/") for p in paths)
checked = 0
for path in paths:
    # Deleted files still select relevant checks, but have no contents to parse.
    if not path.is_file():
        continue
    if path.suffix in (".json", ".asmdef"):
        json.loads(path.read_text())
    elif path.suffix == ".sh":
        subprocess.run(["bash", "-n", str(path)], check=True)
    elif path.suffix == ".py":
        ast.parse(path.read_text(), filename=str(path))
    else:
        continue
    checked += 1
print(f"Checked {checked} changed JSON/assembly/script files. Core tests: {core}; workflows: {workflows}.")
with open(os.environ["GITHUB_OUTPUT"], "a") as output:
    output.write(f"godot={str(godot).lower()}\n")
    output.write(f"core={str(core).lower()}\nworkflows={str(workflows).lower()}\n")
