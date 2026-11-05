# Economy type file layout

Baseline: 0805f87f50f29f06929d7f281a31a9a9423fe7b7. Branch/worktree: economy-type-layout.

Apply CONVENTIONS section 1.4 to the non-UI namespace-level declarations in Scripts/Meta/Economy.cs. Move only IClock, SystemClock, WinPayout and PayoutResult to matching files in that same directory and PerchSort.Meta assembly. Preserve declaration text, namespace, visibility, members, arithmetic, comments and constructor signatures. Economy remains unchanged in its original file. Preserve Economy.cs.meta byte-for-byte; add four unique Unity-format metadata GUIDs using the existing repository template.

Owned files: Economy.cs, the four extracted .cs files and their .meta files, and this topic's spec/report. Do not edit callers, tests, assembly definitions, storage/schema data, rules, levels, scoring or economy behavior. All UI-related code, assets, layout, scenes and presentation tests are read-only. Repository design proposals are not implementation authorization.

Perform declaration equivalence, GUID preservation/uniqueness, reference and assembly-inclusion checks, naming self-tests/guard and applicable static checks after implementation. Do not start Unity tests or screenshots for this batch: full integrated compilation and suites remain pending with the coordinator, under the user's unified-verification timing. Publish a draft PR and verify exact-head CI; never merge, mark ready, deploy, force-push, kill the user's Editor or spawn workers.
