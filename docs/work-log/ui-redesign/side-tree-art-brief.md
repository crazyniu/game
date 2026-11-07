# Task: paint side trees and branch collars for the Perch Sort board (art only, no code)

Repository: this worktree (Godot project in PerchSort/). Work ONLY in this worktree. English only. Do not edit any .gd file.

## Why
On the level board, birds sit on horizontal branches that grow from the left and right screen edges. Today the trunks are a cropped bark strip and a straight pole; the user says the trees on both sides look wrong and the branches look stuck on. We need real side trees and a collar that makes each branch grow out of its trunk.

## Look first (style references, do not modify)
- PerchSort/world/branch_left_w1.png .. w4 (branch art per world; branches stay as they are)
- PerchSort/world/trunk_curved_w1.png, trunk_left_w1.png (current trunks, to be replaced)
- PerchSort/world/bg_level_w1_noon.png, bg_level_w2_noon.png, bg_level_w3_noon.png, bg_level_w4_noon.png (backgrounds the trees stand on; top corners already have foliage)
- art/approved_ui/board_base.png, board_many_branches.png, tree_worlds.png (approved direction: soft plush/felt, fluffy, warm; same family as the birds)
Worlds: w1 green forest (warm brown bark), w2 bamboo forest (green bamboo culm with nodes), w3 autumn forest (red-brown bark, a few orange leaves allowed at the top), w4 snow forest (grey-brown bark with soft snow on top surfaces).

## Deliverables (new files; never overwrite existing ones)
For each world N in 1..4, in PerchSort/world/:
1. `side_tree_left_wN.png` 360x1920 RGBA, transparent background. A full-height tree trunk standing at the LEFT screen edge: roots/flare in the bottom ~260 px, a gentle natural S-curve, visible trunk thickness 120-150 px through the middle, bark silhouette and texture on its inner (right) edge, its outer edge may leave the image on the left. The top ~320 px rises into the corner and may end in a few leaves; it must look complete, never cut flat. No branches painted on it (the game adds branches anywhere between y=600 and y=1700), no birds, no text.
2. `side_tree_right_wN.png` 360x1920 RGBA: the same idea for the RIGHT edge, a different tree (different curve and knots, NOT a mirror of the left one).
3. `branch_collar_wN.png` 240x220 RGBA: the bark collar where a horizontal branch leaves a trunk standing to its LEFT: a soft flared bulge that blends into trunk bark on its left side (left ~70 px is trunk-coloured bark that will overlap the trunk) and narrows to the branch's base thickness on its right edge: the right edge must be a vertical band 76-90 px tall centred at y=110, texture and colour continuous with branch_left_wN.png's thick end so the branch sprite can start there seamlessly. The game mirrors it for right-side branches (scenery may be mirrored).
Bamboo (w2): side trees are thick bamboo culms (one or two stalks, nodes every ~240 px, a few thin leaves at the top); the collar is a bamboo node joint ring with a short side shoot that matches branch_left_w2.png.

Style: painted soft plush/felt texture with gentle volume shading, matching the branch art exactly in palette and line softness. No hard outlines, no photographic bark, no text, no watermark.

## Process
1. Generate with your image tool, one image per call, at the stated sizes (resize/crop with PIL only to fit the canvas; keep alpha clean, no halos, no background remnants: check alpha at the borders).
2. Build review sheets in build/side-tree-review/ (not committed): for each world, the left and right trees placed on bg_level_wN_noon.png at 1080x1920 with three branch_left_wN.png branches (scaled 1.5x height) attached through the collar on each side at y=800, 1100 and 1400. LOOK at every sheet at full size and fix anything that looks pasted on, cut, mismatched in colour, or sideways.
3. Commit only the 12 PNGs (no .import files) on branch `side-tree-art` with message "Paint side trees and branch collars for every world" and push to origin. Do not open a PR, do not merge, do not touch main.
4. Report: the file list, and the paths of the review sheets.
