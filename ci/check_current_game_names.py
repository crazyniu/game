"""Reject numbered game identities in tracked paths and authored source names.

Run directly for the repository check, or with --self-test for isolated fixtures.
Prose, records and generated data are history, not names; only source files are read.
"""

from pathlib import Path, PurePosixPath
import re
import subprocess
import sys


# Standalone labels and embedded PascalCase suffixes, not words such as 'preview',
# hashes, world IDs, level IDs or third-party module names.
NUMBERED = re.compile(r"(?<![a-z0-9])(?i:v|ui|final)[0-9]+|(?:V|UI|Ui|Final)[0-9]+|(?<![A-Z])T[0-9]+[A-Za-z]?")
NUMBERED_PATH = re.compile(r"(?:v|ui|final)[0-9]+|T[0-9]+[A-Za-z]?", re.IGNORECASE)
SOURCE_SUFFIXES = {".gd", ".tscn", ".tres", ".godot", ".cfg", ".py", ".sh", ".yml", ".yaml"}
# Data roots whose JSON is authored game content.
JSON_ROOTS = ("PerchSort/levels/", "PerchSort/layout/", "ci/")
LOCK_NAMES = {"package-lock.json", "yarn.lock", "pnpm-lock.yaml", "poetry.lock", "uv.lock"}


def git(root, *args):
    return subprocess.check_output(["git", "-C", str(root), *args], stderr=subprocess.PIPE)


def is_source(path):
    suffix = PurePosixPath(path).suffix.lower()
    if suffix == ".json":
        return path.startswith(JSON_ROOTS)
    return suffix in SOURCE_SUFFIXES


def exempt(path):
    parts = PurePosixPath(path).parts
    name = PurePosixPath(path).name
    return ".git" in parts or ".godot" in parts or "node_modules" in parts or name in LOCK_NAMES or name.endswith(".lock")


def path_violations(path):
    if exempt(path):
        return
    for part in PurePosixPath(path).parts:
        if NUMBERED_PATH.search(part):
            yield f"{path}:1: numbered game path component {part!r}; use a canonical name"


def _blank(match):
    return " " * len(match.group())


def content_violations(path, text):
    # The guard's own self-test must contain deliberately bad example names.
    if exempt(path) or path == "ci/test_current_game_names.py":
        return
    for number, line in enumerate(text.splitlines(), 1):
        checked = re.sub(r"(?<=\d{4}-\d{2}-\d{2})T(?=\d{2}:)", " ", line)  # ISO date-times
        checked = re.sub(r"(?<=uses: )[A-Za-z0-9_.-]+/[A-Za-z0-9_./-]+@[^\s]+", _blank, checked)  # pinned actions
        checked = re.sub(r"https?://[^\s\"'<>]+", _blank, checked)
        checked = re.sub(r"(?<=gh release download )v[0-9.]+(?= .*--repo [A-Za-z0-9_.-]+/)", _blank, checked)
        checked = re.sub(r'uid://[a-z0-9]+', _blank, checked)  # Godot resource ids
        for match in NUMBERED.finditer(checked):
            yield (f"{path}:{number}:{match.start() + 1}: numbered authored identifier "
                   f"{match.group()!r}; use the canonical game name")


def check(root):
    errors = []
    inspected = 0
    # Index blob IDs also cover sparse-checkout files; present working files are
    # read instead so unstaged authored changes cannot evade the check.
    for entry in git(root, "ls-files", "--stage", "-z").split(b"\0"):
        if not entry:
            continue
        metadata, raw_path = entry.split(b"\t", 1)
        mode, oid, stage = metadata.decode("ascii").split()
        path = raw_path.decode("utf-8")
        if stage != "0":
            errors.append(f"{path}:1: unresolved Git index conflict")
            continue
        errors.extend(path_violations(path))
        if exempt(path) or not is_source(path):
            continue
        if mode not in {"100644", "100755"}:
            errors.append(f"{path}:1: authored text must be a regular tracked file")
            continue
        source = root / path
        data = source.read_bytes() if source.is_file() else git(root, "cat-file", "blob", oid)
        try:
            text = data.decode("utf-8-sig")
        except UnicodeDecodeError:
            errors.append(f"{path}:1: cannot inspect authored text as UTF-8")
            continue
        inspected += 1
        errors.extend(content_violations(path, text))
    for error in errors:
        print(error, file=sys.stderr)
    print(f"Current game naming: {inspected} tracked source files inspected; {len(errors)} violation(s).")
    return 1 if errors else 0


def main():
    try:
        if sys.argv[1:] == ["--self-test"]:
            import unittest
            suite = unittest.defaultTestLoader.discover(str(Path(__file__).parent),
                                                       pattern="test_current_game_names.py")
            return 0 if unittest.TextTestRunner(verbosity=2).run(suite).wasSuccessful() else 1
        base = Path(__file__).resolve().parent.parent
        root = Path(git(base, "rev-parse", "--show-toplevel").decode().strip())
        return check(root)
    except (OSError, UnicodeError, ValueError, subprocess.CalledProcessError) as error:
        print(f"Current game naming: inspection failed: {error}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    sys.exit(main())
