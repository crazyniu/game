# T54D Uniform buttons (problem P4)
Read tasks/T54_contract.md. Owned: `PerchSort/Assets/Scripts/Game/GameButton.cs`, `HudView.cs`, new `Tests/**/T54D*.cs`.
Implement the Buttons feel target. `GameLayout.cs` is read-only (it owns `DisabledScale`; stop using it for scale inside your files). Also check settings,
map bottom bar and popup buttons that use `GameButton`. Add an EditMode or PlayMode test that asserts equal rect size and equal scale for all tool buttons
in rest and disabled state on levels with 3 and 4 tools. Provide a bottom-bar crop with undo disabled and enabled.
