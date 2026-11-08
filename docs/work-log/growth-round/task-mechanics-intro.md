# Task mechanics-intro

Each mechanic gets a one-step animated lesson the first time a level with it is opened (profile tutorial flag, the
existing `has_tutorial` / `set_tutorial`; no new save fields), built like `game/tutorial.gd` steps: a small live scene
from the real art with a pointing hand, looping, one short painted line, a green "Got it" pill (`UI.pill`).
Mechanics and the first level that uses each (from `levels/levels.json`; find them in code, the coordinator may
reorder levels inside a world, so detect "first level with mechanic X" at runtime from the level data, not by number):
sleeping birds (`:sleep`; a matching bird landing next to it or a flock song wakes it), mama and chicks (`mama`,
`chick`; they travel together), eggs (if present; hatch / reveal), crow (if present), twig branch (2 seats), swivel
branch (tap the knot to turn it), growing branch (gains seats), day and night (night owls sleep by day; the counter
shows moves until the switch). Read `core/rules.gd` to describe each correctly. Show the lesson after the level's
entrance, before the first move, once per mechanic. Lines short (<= 6 words, English, added to en.json at the end).
Preview states `lesson_<mechanic>` in tutorial.gd, added to scripts/screenshots.sh.
Evidence: captures and a frame sequence per lesson; a run proving each lesson appears once on its first level.
