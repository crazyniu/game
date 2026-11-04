# T53A Board trees and branches (defects D1, D2)
Read `docs/work-log/interface-fidelity/spec.md` first. You own `BranchView.cs`, `BoardView.cs`, `SceneView.cs`, `GameLayout.cs`, `Ambient.cs`.
Goal: the board screens (`level1_start`, `level_k7`, plus a 5-row and a 3-row level) must match `ui_level_k3.png` and `ui_level_k7.png` in tree,
branch and crown placement. Use the existing world art in `Resources/World` (trunks, branches, crowns, vines). Branch rows are anchored to the trunk
they grow from (left rows from the left trunk, right rows from the right trunk), each row long enough for its birds and ending in a rounded tip.
Do not change bird slot positions relative to the branch top edge in a way that moves `BirdView` anchors out of the logical Place coordinates used by
tests; if the tests assert positions, keep the tested numbers and adapt only the drawn limb/trunk. Report which world art ids you used.
