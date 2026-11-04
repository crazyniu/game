# T53B Top birdhouses, HUD and boosters (defects D3, D4)
Read `tasks/T53_ui_fidelity_contract.md` first. You own `BirdhouseView.cs`, `HudView.cs`, `GameButton.cs`, `GameUI.cs`, `ViewModel.cs`.
Goal: match the top area and bottom booster row of `ui7_level_k3_v2.png` (few houses: one large rope-hung house) and `ui7_level_k7.png` (many houses:
beam with ropes). Switch layout by house count. Booster buttons: purple discs with red number badges, correct count; crow (4th) only when mechanic active.
Gear is a normal enabled disc except under modals. Keep all existing click handlers/events and accessibility names; icon-only controls.
The flock-home door open/close motion and the check badge must still work (existing PlayMode tests cover it).
