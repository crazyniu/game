# Actual desktop pointer observation

The canonical test window was 1180 x 812, with a 456 x 812 game viewport centered at x=362.
The existing original game window was not used. CUA screenshots grounded these clicks:

1. Locked level 9: (716, 238), no dialog.
2. Current level 8: (716, 366), Level 8 dialog appeared; callback log recorded exactly once.
3. Close: (765, 117), route restored.
4. The middle enlarged star on cleared level 7: (462, 328), Level 7 dialog appeared.
5. Close: (765, 160), route restored.
6. Back: (408, 47), harness verified selector and unlock progress, then exited normally.

The coordinate evidence is the worker's direct CUA observation, not a field in pointer.log.
The terminal visibly returned TREE_DETAILS_GUI_COMPLETE and TREE_DETAILS_ALL_COMPLETE.
