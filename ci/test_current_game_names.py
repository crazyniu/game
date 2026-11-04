"""Consumer-visible boundary fixtures for the repository naming guard."""

import contextlib
import io
from pathlib import Path
import subprocess
import tempfile
import unittest

import check_current_game_names as guard


class NamingPolicyTests(unittest.TestCase):
    def test_task_names_and_dates(self):
        for name in ("T" + "54AExample.cs", "T" + "7_report.md",
                     "ui" + "7_x.png", "V" + "5Tool.cs"):
            with self.subTest(name=name):
                self.assertTrue(list(guard.path_violations("docs/" + name)))
        self.assertFalse(list(guard.path_violations("docs/2026-11-04_handoff.md")))

    def test_rejects_numbered_names_in_every_authored_root(self):
        for prefix in ("art/", "tools/levels/", "docs/", "scripts/", "ci/",
                       "PerchSort/Assets/Scripts/", "PerchSort/ProjectSettings/"):
            for token in ("v" + "1", "V" + "99", "ui" + "7", "final" + "2",
                          "BirdV" + "5", "v" + "7sprites", "uI" + "7"):
                with self.subTest(prefix=prefix, token=token):
                    self.assertTrue(list(guard.path_violations(prefix + token + ".txt")))

    def test_rejects_namespaces_resource_keys_comments_and_schema_aliases(self):
        for text in ("namespace PerchSort.V" + "5.Core {}",
                     'Resources.Load("ui' + '7/Art/bird")',
                     "// Keep BirdV" + "6 here",
                     '{"schemaVersion":"v' + '2"}'):
            with self.subTest(text=text):
                self.assertTrue(list(guard.content_violations("tools/author.cs", text)))
        self.assertTrue(list(guard.content_violations(
            "ci/data.json", '{"prompt":"GameV' + '5"}')))

    def test_storage_exception_is_exact_and_owner_scoped(self):
        owner = guard.ASSETS + "Scripts/Game/ProgressStore.cs"
        key = next(iter(guard.STORAGE_LITERALS[owner]))
        self.assertFalse(list(guard.content_violations(owner, 'const string key = "' + key + '";')))
        self.assertTrue(list(guard.content_violations(owner, 'const string key = "' + key + 'new";')))
        self.assertTrue(list(guard.content_violations("tools/save.cs", '"' + key + '"')))

    def test_history_exception_does_not_cover_current_specs_or_team_files(self):
        token = "V" + "5"
        for path in ("tasks/T30_report.md", "docs/team/HANDOFF_2026-11-04.md"):
            self.assertTrue(guard.exempt(path))
            self.assertFalse(list(guard.content_violations(path, "`" + token + "`")))
        for path in ("tasks/T54E_cleanup.md", "docs/team/TEAM.md", "docs/CURRENT_GAME.md"):
            self.assertFalse(guard.exempt(path))
            self.assertTrue(list(guard.content_violations(path, "`" + token + "`")))

    def test_dependency_and_lockfile_exceptions_are_narrow(self):
        token = "V" + "5"
        for path in ("PerchSort/Packages/vendor/" + token + ".cs", "package-lock.json",
                     "deps.lock", ".git/refs/" + token):
            self.assertFalse(list(guard.path_violations(path)))
        text = 'uses: actions/checkout@v' + '4\nname: BirdV' + '5'
        errors = list(guard.content_violations(".github/workflows/ci.yml", text))
        self.assertEqual(1, len(errors))
        self.assertIn(":2:", errors[0])

    def test_markdown_paths_and_fences_checked_without_rewriting_history_prose(self):
        token = "v" + "5"
        self.assertFalse(list(guard.content_violations("docs/design.md", "Historical " + token + " decision.")))
        for text in ("`art/" + token + "/screen.png`", "[art](../art/" + token + "/screen.png)",
                     "```cs\nclass BirdV" + "5 {}\n```", "~~~sh\nrun ui" + "7\n~~~"):
            self.assertTrue(list(guard.content_violations("docs/design.md", text)))

    def test_level_world_task_numbers_and_dependency_module_are_not_game_versions(self):
        for path in ("tasks/T54E_report.md", "art/world/w4/l40.png", "tools/vision_adapter.py"):
            self.assertFalse(list(guard.path_violations(path)))
        self.assertFalse(list(guard.content_violations("tools/check.py", "import cv2\nlevel = 40")))

    def test_index_fallback_and_unstaged_changes_cannot_bypass_check(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            subprocess.run(["git", "init", "--quiet", str(root)], check=True)
            source = root / "author.cs"
            source.write_text("class BirdV" + "5 {}\n")
            subprocess.run(["git", "-C", str(root), "add", "author.cs"], check=True)
            source.unlink()  # Model an indexed file absent from a sparse checkout.
            with contextlib.redirect_stdout(io.StringIO()), contextlib.redirect_stderr(io.StringIO()):
                self.assertEqual(1, guard.check(root))
                source.write_text("class Bird {}\n")
                self.assertEqual(0, guard.check(root))
                subprocess.run(["git", "-C", str(root), "add", "author.cs"], check=True)
                source.write_text("class BirdV" + "7 {}\n")
                self.assertEqual(1, guard.check(root))


if __name__ == "__main__":
    unittest.main()
