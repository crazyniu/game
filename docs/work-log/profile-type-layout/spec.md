# Profile type file layout

Apply CONVENTIONS section 1.4 (one namespace-level type per matching file) to three confirmed non-UI declarations only. Baseline: 821242aa738bc16f369cd3eb55b6217248246795. Worktree/branch: profile-type-layout.

Move ProfileMigrator from Scripts/Meta/ProfileText.cs into ProfileMigrator.cs, retaining PerchSort.Meta and its existing assembly. Move Profiles and FakeClock from Tests/Meta/ProfileEconomyTests.cs into matching files in that same test namespace and assembly. Keep declaration text, visibility, names, storage literals and behavior unchanged. ProfileText.Data stays nested. Preserve the original two .meta files; create distinct new Unity-format metadata GUIDs. Transfer the seven-literal guard allowance to the actual migrator owner without broadening it, with a focused regression.

Owned paths are those five C# files, the three new metas, ci/check_current_game_names.py, ci/test_current_game_names.py, and this subject's spec/report. Everything else is read-only. No UI, presentation test, asset, scene, importer or resource changes are authorized.

The coordinator's latest instruction defers full Unity and screenshot execution until all authorized fixes are integrated. Perform static declaration/compatibility/GUID/reference checks and naming checks now; publish a draft PR with exact-head CI. Full integrated Unity compilation and EditMode/PlayMode acceptance remain pending with the coordinator. Do not merge, mark ready, deploy, force-push or spawn workers.

The coordinator additionally owns a narrow correction for issue #114 finding C06: update only the stale Shop/Aviary implementation and developer-flag passages in docs/CURRENT_GAME.md after source verification. Preserve reset-progress/persistence guidance. This is source-fact documentation only; UI/runtime/settings/resource changes remain forbidden.
