"""Reject numbered implementation identities in tracked, current authored Unity assets.

Run with python3 ci/check_current_game_names.py. Historical documents/original art
are outside the authored roots. Storage exceptions are exact literals per owner,
not file exemptions. See tasks/T52D_naming_policy.md for the policy boundary.
"""

from pathlib import Path, PurePosixPath
import re
import subprocess
import sys


ASSETS = "PerchSort/Assets/"
ROOTS = tuple(ASSETS + name for name in ("Scripts", "Editor", "Tests", "Scenes", "Resources"))
TEXT_SUFFIXES = {".cs", ".asmdef", ".asmref", ".unity", ".prefab", ".asset",
                 ".json", ".yaml", ".yml", ".meta", ".txt", ".xml", ".uxml", ".uss"}
NUMBERED = re.compile(r"v[0-9]+", re.IGNORECASE)
QUOTED = re.compile(r'"(?:\\.|[^"\\\r\n])*"')
# Numeric schema/dependency/release values are formats, not implementation names.
FORMAT_VERSION = re.compile(r"v[0-9]+(?:\.[0-9]+)*(?:[-+][A-Za-z0-9.-]+)?", re.IGNORECASE)
VERSION_FIELD = re.compile(
    r'(?:"(?:schema|schemaVersion|formatVersion|saveVersion|releaseVersion|dependencyVersion|version)"\s*:\s*'
    r'|\b(?:schemaVersion|formatVersion|saveVersion|releaseVersion|dependencyVersion|version)\s*=\s*)$',
    re.IGNORECASE,
)
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
        # Historical ignored Song data is retained only by this migration regression.
        "v5.songNotes", "v5.songPayouts",
    },
}


def git(root, *args):
    return subprocess.check_output(["git", "-C", str(root), *args], stderr=subprocess.PIPE)


def authored(path):
    return any(path.startswith(root + "/") or path == root + ".meta" for root in ROOTS)


def content_violations(path, text):
    allowed = STORAGE_LITERALS.get(path, set())
    for number, line in enumerate(text.splitlines(), 1):
        def mask_literal(match):
            literal = match.group()[1:-1]
            if literal in allowed or (FORMAT_VERSION.fullmatch(literal)
                                      and VERSION_FIELD.search(line[:match.start()])):
                return " " * len(match.group())
            return match.group()

        checked = QUOTED.sub(mask_literal, line)
        for match in NUMBERED.finditer(checked):
            yield (f"{path}:{number}:{match.start() + 1}: numbered implementation identity "
                   f"{match.group()!r}; use the canonical game name, not a versioned fork")


def check(root):
    errors = []
    inspected = 0
    # The index enumerates absent sparse-checkout paths and supplies their blob IDs.
    entries = git(root, "ls-files", "--stage", "-z", "--", ASSETS)
    for entry in entries.split(b"\0"):
        if not entry:
            continue
        metadata, raw_path = entry.split(b"\t", 1)
        mode, oid, stage = metadata.decode("ascii").split()
        path = raw_path.decode("utf-8")
        if stage != "0":
            errors.append(f"{path}:1: unresolved Git index conflict; resolve before checking names")
            continue
        for part in PurePosixPath(path).parts:
            if NUMBERED.search(part):
                errors.append(f"{path}:1: numbered implementation path component {part!r}; "
                              "rename it and its callers to canonical names")
        if not authored(path) or PurePosixPath(path).suffix.lower() not in TEXT_SUFFIXES:
            continue  # All Assets paths are checked; only authored text payloads are read.
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
        base = Path(__file__).resolve().parent.parent
        root = Path(git(base, "rev-parse", "--show-toplevel").decode().strip())
        return check(root)
    except (OSError, UnicodeError, ValueError, subprocess.CalledProcessError) as error:
        print(f"Current game naming: inspection failed: {error}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    sys.exit(main())
