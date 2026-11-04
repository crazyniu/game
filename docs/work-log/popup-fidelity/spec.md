# T53C Popups: win, settings, level card (defects D6-D9)
Read `docs/work-log/interface-fidelity/spec.md` first. You own `WinPanel.cs`, `SettingsPanel.cs`, `PanelsView.cs`, `PopupFlow.cs`, `PopupMotion.cs`.
Goal: win, settings, level card (also shop/aviary/world-complete/world-unlock/new-friend/daily-gift panels where they share these classes) match
`ui_win.png`, `ui_settings.png`, `ui_level_card.png`, `ui_shop_*.png`, `ui_aviary_*.png`, `ui_popup_*.png`. Add a proper dim layer, correct panel sizes,
filled/unfilled stars, big leaf number, ribbon, side birds hugging the panel edge, leaf sprigs, rays/confetti for win. Keep existing popup motion timing and
the transaction/notification ordering (GameRoot and Tests depend on it). Capture and compare every popup you touch, including shop and aviary, which are
not in the default screenshot set: extend your own check via the existing fixtures only if possible without editing Editor/Test files; otherwise list them as
"not captured" in your report and tell the coordinator.
