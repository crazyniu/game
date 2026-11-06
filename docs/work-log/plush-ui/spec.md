# Plush UI build: shared contract

Status: the user APPROVED (2026-11-06) the plush direction mockups and the runtime sprite kit. This stage builds every screen as a clickable Unity UI shell in the canonical `Game.unity` views, fed by fake states, matching the approved mockups. Logic stays as it is (real Session/profile drive the same views); no Core/Meta/rules/economy/save changes.

## Inputs
- Approved mockups (direction, 1080x1920): `git show origin/ui-design-review:art/ui_design/<name>.png`. Names: board_base, board_many_branches, board_stuck_hint, board_sleeping_bubble, board_win_panel, board_win_panel_two_stars, settings, goal_info, level_card, daily_gift, new_friend, unlock_card, world_complete, world_unlock, map, shop_boosters, shop_houses, shop_outfits, shop_confirm, aviary_list, aviary_detail, component_sheet, tree_worlds.
- Component rules and all numbers (modal skeleton, action row, disc sizes, HUD, map, shop): `docs/work-log/plush-ui/components.md`. Where it and a mockup disagree, components.md wins for sizes; the mockup wins for look.
- Runtime sprites (already on main, same file names as before, replaced in place): `PerchSort/Assets/Resources/Art/*`.
- Painted number glyphs: `Art/glyphs/glyph_<set>_<char>.png` with sets brown (dark-brown, on cream pills), cream (cream with green outline, on green ribbons and map discs), white (cream-white with brown outline, on orange and red discs); chars 0-9, plus, minus, times, infinity, slash; metrics in `Art/glyphs/glyph_metrics.json` (canvas height 150, cap 100, baseline 126).
- Painted text sprites: `Art/labels/label_*.png`. The mapping string -> file -> where shown is `docs/work-log/plush-ui/text-inventory.md`. Level badges `label_level_<N>`, bird names `label_name_<species>`.

## Hard rules
1. NO font-rendered player-visible text anywhere (no TextMeshPro/Text for anything the player reads). Numbers use PaintedNumber; fixed words use PaintedLabels sprites. Accessibility/GameObject names may stay strings. If a needed word has no sprite, do NOT render it with a font: list it in your report under "missing label sprites" and leave a same-size empty placeholder.
2. Every screen matches its mockup: positions, sizes, colours of the sprites; the modal skeleton (dim 0.35 + desaturate everything behind, panel 860 wide centred, title ribbon on the panel top edge, close X 24 inside the top-right corner only on goal_info/level_card/settings/shop_confirm, action row BELOW the panel, primary green 124 / secondary orange 100, 48 gaps).
3. Same-role buttons identical size in every state; disabled = dimmed, not smaller. Primary at most 1.25x siblings.
4. Motion uses the existing GameMotion/PopupMotion: panel open 0.18 s, close 0.12 s, press 0.92 scale 0.08 s, release 0.18 s. Birds never static.
5. Each screen provides fake states through `IScreenStates` (see Foundation API) covering every state of its mockups plus edge values (0 and 99999 coins, 999 moves, long names, locked/unlocked, owned/equipped, disabled buttons).
6. No new scene, no copied views, no version or task numbers in any name. Edit canonical files. No changes to Core, Meta, levels, economy, saves.
7. The user's Unity Editor has the PRIMARY checkout open (a different project path); it never blocks you. The user STOPPED automated test runs: do NOT run EditMode/PlayMode suites or screenshot scripts. Run exactly ONE Unity batch compile check at the end under `flock /tmp/unity.lock`. If old tests no longer compile because a view API changed, update those test call sites minimally (do not run them).

## Foundation API (owned by the painted-text task; everyone codes against it)
```csharp
namespace PerchSort.Game {
  public enum GlyphSet { Brown, Cream, White }
  // uGUI component; children are Image glyphs laid out by glyph_metrics.json advances.
  public sealed class PaintedNumber : MonoBehaviour {
    public static PaintedNumber Create(Transform parent, string name, GlyphSet set, float capHeight, TextAnchor alignment);
    public void SetText(string text);   // allowed chars: 0-9 + - x (times) / and '∞'
    public void SetValue(int value);    // uses BoardNumber.Format for overflow (k/M via glyph-less fallback: clamp to 99999 and show 99999)
    public float PreferredWidth { get; }
    public RectTransform Rect { get; }
  }
  public static class PaintedLabels {
    public static Sprite Get(string key);                     // key = file name without .png under Art/labels; logs an error and returns null if missing
    public static Image Create(Transform parent, string name, string key, Vector2 point, float height); // preserves aspect ratio
    public static string KeyForText(string text);             // inverse lookup from text-inventory (e.g. "Daily Gift" -> "label_title_daily_gift"); null if none
  }
}
```
State switcher: `Tools > UI States` lists every `IScreenStates` implementation found by reflection (public sealed class with a constructor `(GameRoot root)`), one foldout per `ScreenName`, a button per state, an Instant motion toggle and "Return to real game". Screen tasks add their own `<Screen>FakeData : IScreenStates` file; they never edit the switcher.

## Tasks and file ownership (parallel; one OMP worker each)
| Task | Branch | Owns | Mockups |
|---|---|---|---|
| painted-text | mac-painted-text/work | new PaintedNumber.cs, PaintedLabels.cs, GameUI.cs (Text helper becomes painted where callers are generic), TextFit.cs (retire), Editor/Game/UiStateSwitcherWindow.cs, IScreenStates.cs, GameArtImporter import settings for Art/glyphs and Art/labels (single sprite, full rect, centred pivot, no compression artefacts) | component_sheet |
| board | mac-board-plush/work | HudView, BoardView, BranchView, BirdView (zzz only), BirdhouseView, SceneView, StuckHint, SleepBubble, TutorialHand, LevelEntrance, BoardFakeData, Board*Presentation files | board_base, board_many_branches, board_stuck_hint, board_sleeping_bubble, tree_worlds |
| popups | mac-popups-plush/work | PanelsView, WinPanel, PopupMotion, new PopupFakeData | board_win_panel, board_win_panel_two_stars, settings, goal_info, level_card, daily_gift, new_friend, unlock_card, world_complete, world_unlock |
| map | mac-map-plush/work | MapView, new MapFakeData | map |
| shop | mac-shop-plush/work | ShopView, new ShopFakeData | shop_boosters, shop_houses, shop_outfits, shop_confirm |
| aviary | mac-aviary-plush/work | AviaryView, new AviaryFakeData | aviary_list, aviary_detail |
GameRoot.cs: only the painted-text task may edit it; screen tasks needing a GameRoot hook describe it in their report instead. Screen tasks start from main; when the painted-text branch lands on main the coordinator tells you, then `git merge origin/main` and switch any temporary code to the real API.

## Report
`docs/work-log/<task-topic>/report.md`: changed files, state list with click path `Tools > UI States > <Screen> > <state>`, the one compile check command and result, missing label sprites, anything not done. Push your branch, no PR; last message: final SHA. English only, no emoji.
