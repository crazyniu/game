# Unity templates

Starting code for stage 4 and the checks in stages 4 to 6. Compiled against Unity 6000.6 (UGUI, TextMeshPro, Unity Test Framework) with `tools/compile_unity_templates.sh`; Unity versions before 6000.4 need the `FindObjectsByType` overloads that take a `FindObjectsSortMode`.

Copy what the project needs into its own folders and rename the `Game.*` namespaces to the project's. Keep the project's existing equivalents when it already has them (screen manager, motion switch, capture tool); adapt them to the same contract instead of adding a second copy.

| File | Purpose |
| --- | --- |
| `Runtime/UiScreen.cs` | Base class for screens and popups: one CanvasGroup for visibility and input, modal flag, music, close request, enter and exit motion hooks. |
| `Runtime/ScreenStack.cs` | Push, pop, replace, clear to; back always pops; `TopChanged` for music and analytics. |
| `Runtime/IScreenStates.cs` | Contract for fake data sources: the screen spec's state names and `Apply(state)`. |
| `Runtime/ShopExample.cs` | Worked example: view contract interface, view that only shows data and raises events, fake data source with every state. |
| `Runtime/UiMotion.cs` | Global instant and reduced motion switches; `Duration()` for every tween. |
| `Runtime/UiTiming.cs` | JSON-lines timing log with `Mark(name, phase)`; compiled out of release builds. |
| `Runtime/TimedCapture.cs` | Saves every frame at a fixed `Time.captureFramerate` with the timing log. |
| `Runtime/UiRole.cs` | Tags controls with their pattern library role for size checks. |
| `Editor/UiStateSwitcherWindow.cs` | `Tools > UI States`: every screen's states as buttons in Play mode, plus the instant motion toggle. |
| `Tests/UiConsistencyTests.cs` | Play Mode: every state applies without errors; same-role controls keep identical on-screen size in every state. |

Assembly definitions: put Runtime in the game's UI assembly, Editor in an Editor-only assembly, Tests in a Play Mode test assembly that references the UI assembly.

Timing check: `python3 scripts/check_timing.py <timing.jsonl> <expected.json>` compares the log with the screen spec durations.
