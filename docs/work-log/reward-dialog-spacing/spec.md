# Content-aware reward dialog spacing

Reduce unused side and bottom space without shrinking painted words, birds, stars or hit targets. Win results use an 880-pixel card with a 72-pixel footer. Reward grids use 840 pixels for one/two prizes and 880 for three or more; their row count still follows the real distinct prizes. New friends use 880, tool unlocks 900 to preserve long captions. World gates keep 960 for their door swing and use the same 72-pixel footer. Other modal families and dense level-entry cards retain their allocations.

Only JungleDialog and its reward callers change. Gameplay, callbacks, reward values and animation timing stay unchanged. Verify Chinese/English, zero reward, booster drops, multiple prize rows and door/bird/star motion. Runtime evidence stays in ignored build output.
