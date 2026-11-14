"""Regression tests for mixed GDScript summary formats, without launching Godot."""
import unittest

from sweep_gui import failure_lines


class SweepDiagnosticsTest(unittest.TestCase):
    def test_failure_counts_do_not_confuse_check_counts(self):
        cases = [
            ("AUX_DIALOGS checks=10 failures=0", False),
            ("checks=103 failures=0", False),
            ("42 checks, 0 failures", False),
            ("3 failures", True),
            ("checks=10 failures=2", True),
            ("FAIL bad", True),
            ("SCRIPT ERROR: bad", True),
        ]
        for text, expected in cases:
            with self.subTest(summary=text):
                self.assertEqual(bool(failure_lines(text)), expected)


if __name__ == "__main__":
    unittest.main()
