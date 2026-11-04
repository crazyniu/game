"""Reject numbered game identities in every tracked path and authored identifier.

Run directly for the repository check, or with --self-test for isolated fixtures.
Markdown prose is not an identifier: inline code, fenced code and link targets are.
History exceptions cover dated handoffs and completed task reports, not task specs.
"""

from pathlib import Path, PurePosixPath
import re
import subprocess
import sys


ASSETS = "PerchSort/Assets/"
# Recognize standalone labels and embedded PascalCase suffixes, not words such as
# 'preview', SHA hashes, world IDs, level IDs or third-party module names.
NUMBERED = re.compile(r"(?:(?<![a-z0-9])(?:v|ui|final)|V|UI|Final)[1-9][0-9]*(?![a-z0-9])")
NUMBERED_PATH = re.compile(r"(?:v|ui|final)[0-9]+", re.IGNORECASE)
QUOTED = re.compile(r'''(["'`])((?:\\.|(?!\1)[^\\\r\n])*)\1''')
INLINE_CODE = re.compile(r"`+([^`\r\n]+)`+")
LINK_TARGET = re.compile(r"\]\(([^\s)]+)(?:\s+[^)]*)?\)")
TEXT_SUFFIXES = {
    ".cs", ".asmdef", ".asmref", ".unity", ".prefab", ".asset", ".scenetemplate",
    ".inputactions", ".json", ".yaml", ".yml", ".meta", ".txt", ".xml", ".uxml",
    ".uss", ".shader", ".cginc", ".hlsl", ".py", ".sh", ".md", ".csproj",
    ".props", ".targets", ".toml", ".ini", ".cfg", ".gitignore",
}
# Exact persisted literals remain owner-scoped in code. Documentation may quote
# these exact literals, but never a wildcard that licenses new keys.
STORAGE_LITERALS = {
    ASSETS + "Scripts/Game/ProgressStore.cs": {
        "v5.coins", "v5.muted", "v5.highestUnlocked", "v7.notifications",
        "v5.bestStars.", "v5.tutorial.", "v5.booster.", "v5.starter.",
        "v5.bestMoves.", "v6.unlockSeen.",
    },
    ASSETS + "Scripts/Meta/ProfileText.cs": {
        "v5.coins", "v5.muted", "v5.highestUnlocked", "v5.bestStars.",
        "v5.tutorial.", "v5.booster.", "v5.starter.",
    },
    ASSETS + "Tests/Meta/CollectionProgressionTests.cs": {
        "v5.highestUnlocked", "v5.starter.0", "v5.booster.0",
    },
    ASSETS + "Tests/Meta/ProfileEconomyTests.cs": {
        "v5.coins", "v5.muted", "v5.highestUnlocked", "v5.bestStars.1",
        "v5.bestStars.10", "v5.bestStars.9", "v5.bestStars.", "v5.tutorial.7",
        "v5.booster.0", "v5.booster.1", "v5.booster.5", "v5.starter.0",
        "v5.songNotes", "v5.songPayouts",
    },
    # Historical schema examples are persisted campaign IDs, not game identities.
    "docs/spec/DATA_SAVE.md": {"bird-home-peer-v4", "perch-sort-v2", "bird-home-v3"},
    "docs/spec/VALIDATION.md": {"bird-home-peer-v4", "perch-sort-v2", "bird-home-v3"},
}
DOCUMENTED_STORAGE = set().union(*STORAGE_LITERALS.values())


def git(root, *args):
    return subprocess.check_output(["git", "-C", str(root), *args], stderr=subprocess.PIPE)


def exempt(path):
    parts = PurePosixPath(path).parts
    name = PurePosixPath(path).name
    if ".git" in parts:
        return True
    # Unity package manifests and package-managed sources belong to dependencies.
    if path.startswith(("PerchSort/Packages/", "PerchSort/ProjectSettings/Packages/")) or any(
            part in {"node_modules", "PackageCache"} for part in parts):
        return True
    if path.startswith("PerchSort/Assets/Fonts/TextMesh Pro/"):
        return True
    if (name in {"package-lock.json", "yarn.lock", "pnpm-lock.yaml", "Cargo.lock",
                 "packages-lock.json", "poetry.lock", "uv.lock", "Pipfile.lock"}
            or name.endswith(".lock")):
        return True
    if path.startswith(("tasks/", "docs/team/")):
        return bool(re.search(r"(?:HANDOFF_\d{4}-\d{2}-\d{2}|T\d+[A-Za-z0-9_]*_report)(?:[_.]|$)",
                              name, re.IGNORECASE))
    return False


def path_violations(path):
    if exempt(path):
        return
    for part in PurePosixPath(path).parts:
        if NUMBERED_PATH.search(part):
            yield f"{path}:1: numbered game path component {part!r}; use a canonical name"


def identifier_lines(path, text):
    if PurePosixPath(path).suffix.lower() == ".json":
        for number, line in enumerate(text.splitlines(), 1):
            # Image-generation prompt bodies are prose, not identifier fields.
            # Their exact historical wording remains provenance evidence.
            if re.match(r'\s*"(?:prompt|prompt_prefix)"\s*:', line):
                continue
            yield number, line
        return
    if PurePosixPath(path).suffix.lower() != ".md":
        yield from enumerate(text.splitlines(), 1)
        return
    fence = None
    for number, line in enumerate(text.splitlines(), 1):
        marker = re.match(r"\s*(`{3,}|~{3,})", line)
        if marker:
            if fence is None:
                fence = marker.group(1)[0]
            elif marker.group(1)[0] == fence:
                fence = None
            continue
        if fence:
            yield number, line
        else:
            # Keep columns stable for diagnostics and ignore natural-language
            # references to historical revisions rather than rewriting evidence.
            checked = [" "] * len(line)
            for pattern in (INLINE_CODE, LINK_TARGET):
                for match in pattern.finditer(line):
                    checked[match.start():match.end()] = line[match.start():match.end()]
            yield number, "".join(checked)


def content_violations(path, text):
    if exempt(path):
        return
    allowed = STORAGE_LITERALS.get(path, set())
    if PurePosixPath(path).suffix.lower() == ".md" or path == "ci/check_current_game_names.py":
        allowed = allowed | DOCUMENTED_STORAGE
    for number, line in identifier_lines(path, text):
        def mask_literal(match):
            return " " * len(match.group()) if match.group(2) in allowed else match.group()

        checked = QUOTED.sub(mask_literal, line)
        # A workflow's pinned action ref is third-party package metadata, not an
        # authored game identity. Do not exempt the rest of the workflow file.
        checked = re.sub(r"(?<=uses: )[A-Za-z0-9_.-]+/[A-Za-z0-9_./-]+@[^\s]+",
                         lambda match: " " * len(match.group()), checked)
        checked = re.sub(r"https?://[^\s\"'<>]+", lambda match: " " * len(match.group()), checked)
        checked = re.sub(r"(?<=gh release download )v[0-9.]+(?= .*--repo [A-Za-z0-9_.-]+/)",
                         lambda match: " " * len(match.group()), checked)
        for match in NUMBERED.finditer(checked):
            yield (f"{path}:{number}:{match.start() + 1}: numbered authored identifier "
                   f"{match.group()!r}; use the canonical game name")


def check(root):
    errors = []
    inspected = 0
    # Index blob IDs also cover sparse-checkout files. Present working files
    # are read instead so unstaged authored changes cannot evade the check.
    entries = git(root, "ls-files", "--stage", "-z")
    for entry in entries.split(b"\0"):
        if not entry:
            continue
        metadata, raw_path = entry.split(b"\t", 1)
        mode, oid, stage = metadata.decode("ascii").split()
        path = raw_path.decode("utf-8")
        if stage != "0":
            errors.append(f"{path}:1: unresolved Git index conflict")
            continue
        errors.extend(path_violations(path))
        suffix = PurePosixPath(path).suffix.lower()
        if exempt(path) or (suffix not in TEXT_SUFFIXES and not path.endswith(".gitignore")):
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
    print(f"Current game naming: {inspected} tracked text files inspected; {len(errors)} violation(s).")
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
