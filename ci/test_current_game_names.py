"""Boundary fixtures for the repository naming guard."""

import contextlib
import io
from pathlib import Path
import subprocess
import tempfile
import unittest

import check_current_game_names as guard


class NamingPolicyTests(unittest.TestCase):
    def test_rejects_numbered_identities_in_source(self):
        for token in ("v" + "1bird", "ui" + "2panel", "final" + "3candidate", "bird_v" + "2copy",
                      "BirdUi" + "2", "BirdV" + "2copy", "FINAL" + "3candidate", "T" + "54A"):
            with self.subTest(token=token):
                self.assertTrue(list(guard.content_violations("PerchSort/game/bird.gd", "class_name " + token)))

    def test_accepts_ordinary_words_and_metadata(self):
        for text in ("var preview := true", "import cv2", "uses: actions/checkout@v" + "7.0.1",
                     "stamp = 2026-11-06T10:00:00", "[ext_resource uid=\"uid://bv" + "2abc\"]",
                     "url = \"https://example.com/v" + "2/x\""):
            with self.subTest(text=text):
                self.assertFalse(list(guard.content_violations("PerchSort/game/main.gd", text)))

    def test_only_source_files_are_read(self):
        self.assertTrue(guard.is_source("PerchSort/game/main.gd"))
        self.assertTrue(guard.is_source("PerchSort/main.tscn"))
        self.assertTrue(guard.is_source("PerchSort/levels/levels.json"))
        self.assertFalse(guard.is_source("docs/work-log/topic/report.md"))
        self.assertFalse(guard.is_source("PerchSort/art/layout_metrics.json"))

    def test_numbered_paths_are_rejected(self):
        self.assertTrue(list(guard.path_violations("PerchSort/game/board_v" + "2.gd")))
        self.assertTrue(list(guard.path_violations("docs/T" + "54/report.md")))
        self.assertFalse(list(guard.path_violations("PerchSort/world/bg_level_w1_noon.png")))

    def test_repository_check_reads_unstaged_changes(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            subprocess.run(["git", "init", "-q", str(root)], check=True)
            source = root / "PerchSort/game/main.gd"
            source.parent.mkdir(parents=True)
            source.write_text("extends Node\n")
            subprocess.run(["git", "-C", str(root), "add", "."], check=True)
            with contextlib.redirect_stdout(io.StringIO()), contextlib.redirect_stderr(io.StringIO()):
                self.assertEqual(0, guard.check(root))
                source.write_text("class_name GameV" + "2\n")
                self.assertEqual(1, guard.check(root))


if __name__ == "__main__":
    unittest.main()
