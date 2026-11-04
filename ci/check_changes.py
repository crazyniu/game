"""Check retained current source/configuration; retired archives never select or run checks."""
import ast
import json
import os
from pathlib import Path
import subprocess
import xml.etree.ElementTree as ET

if os.environ["EVENT_NAME"] == "workflow_dispatch":
    command = ["git", "ls-files", "-z"]
else:
    # checkout fetches the PR merge commit and its parents; parent 1 is main.
    command = ["git", "diff", "--name-only", "-z", "HEAD^1", "HEAD"]
current_roots = (
    "PerchSort/Assets/Scripts/", "PerchSort/Assets/Tests/", "PerchSort/Assets/Resources/",
    "PerchSort/Packages/", "scripts/", "ci/", ".github/workflows/",
)
paths = [Path(p) for p in subprocess.check_output(command).decode().split("\0")
         if p and p.startswith(current_roots)]
core = any(
    p.as_posix().startswith("PerchSort/Assets/Scripts/Core/")
    or (p.as_posix().startswith("PerchSort/Assets/Tests/EditMode/") and "Audio" not in p.parts)
    or p.as_posix().startswith("ci/")
    or p.as_posix() == ".github/workflows/ci.yml"
    for p in paths
)
workflows = any(p.as_posix().startswith(".github/workflows/") for p in paths)
checked = 0
for path in paths:
    # Deleted files still select relevant checks, but have no contents to parse.
    if not path.is_file():
        continue
    if path.suffix in (".json", ".asmdef"):
        json.loads(path.read_text())
    elif path.suffix == ".csproj":
        ET.parse(path)
    elif path.suffix == ".sh":
        subprocess.run(["bash", "-n", str(path)], check=True)
    elif path.suffix == ".py":
        ast.parse(path.read_text(), filename=str(path))
    else:
        continue
    checked += 1
print(f"Checked {checked} current JSON/assembly/project/script files. Core tests: {core}; workflows: {workflows}.")
with open(os.environ["GITHUB_OUTPUT"], "a") as output:
    output.write(f"core={str(core).lower()}\nworkflows={str(workflows).lower()}\n")
