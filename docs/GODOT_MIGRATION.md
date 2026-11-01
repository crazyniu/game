# Godot migration — current state, workflow and gaps

**Status: IN PROGRESS.** This is the unique current-engine status owner. Baseline `f60114e572c08fcbafd3bdf2979fc52e1640c38f`, branch `t30-godot-migration`. The user explicitly requested native Godot instead of Unity, stopping other feature expansion during migration, cloud-first testing with local fallback, updating related documentation, and submitting/merging the completed migration. This is not merely a skin replacement. A directory or a standalone passing suite does not establish migration completion.

## 1. Authority and execution

Active project: [godot/project.godot](../godot/project.godot), native GDScript, main scene [first_level.tscn](../godot/scenes/first_level.tscn). Unity `PerchSort/` and old `scripts/legacy_unity/` are retained legacy/reference, not the active entry/test runner. Existing Unity saves and original assets are untouched.

Cloud Godot **4.6.3** is primary. Local Godot **4.7.2** is fallback only when a needed cloud capability is actually unavailable; document the blocker and tested version rather than silently defaulting local. A headless/dummy-renderer image is not visual evidence. The cloud's initial engine bootstrap succeeded, but aggregate game tests, rendering, audio and exports have their own status below. Independent window/OS permissions remain in force. **No251 connection, read, write, inspection, migration or transfer.** Old data stays there, with no recovery workaround.

Parallel module owners edit disjoint core/save/view/assets/docs/root-CI paths. The integrator owns combined verification and submission. The user's merge authorization covers the completed framework migration after necessary review/CI; no self-approval on their behalf, branch-protection bypass, unrelated merge, deployment or store release. Other feature expansion remains stopped by explicit user instruction. Preserve three approved shop/skill/settings visual originals; no new shop code, features or generated replacement artwork is implied.

## 2. Native implementation and gap matrix

All source-presence rows describe the current working tree, not a claim of committed/pushed/accepted output. The coordinator records the tested final revision before changing the overall status.

| Area | Current native implementation / mapping | Acceptance or remaining gap |
| --- | --- | --- |
| Entry | `godot/project.godot` -> `res://scenes/first_level.tscn`, GL Compatibility renderer, portrait1080×1920 reference | Cloud native import/headless integration ran; real cloud graphics could not start. Local Mac fallback is therefore needed for final visual validation, subject to existing permissions. No active Unity BuildSettings/SceneBuilder dependency. |
| Rules/session | [first_level_rules.gd](../godot/core/first_level_rules.gd), `FirstLevelRules` and nested `Session`, both RefCounted-based | Fixed L01 N4/K2/P3, initial occupied counts3/4/1, exact AAA/BBBA/B; Transfer/Home/Retire, candidate commit, Undo/Restart. Independent core suite and final integration are separate gates. |
| Persistence | [save_store.gd](../godot/core/save_store.gd), native binary version1; [save tests](../godot/tests/test_save.gd) | Save owner reported41 checks/0 failures, exit0 on cloud4.6.3 at05:11UTC; scoped standalone run, not final aggregate or power-loss/mobile acceptance. |
| View | [first_level.gd](../godot/scripts/first_level.gd), [bird_actor.gd](../godot/scripts/bird_actor.gd) | Native six-frame flight, idle/selection, actual nest-entry presentation and retired branch/root fade are migration requirements; source presence/headless smoke does not prove visual quality or timing. |
| UI/audio | English/Fredoka source font, existing images/WAV; native Control, Theme/FontFile, AudioStreamPlayer/buses | Current first-level global `audio_muted` parity is distinct from the approved future two-switch Music/Sound page. No claim new settings page, shop or skill economy is implemented. Listening/touch/device evidence pending. |
| Tests/CI | [run_tests.sh](../scripts/run_tests.sh) imports then runs core/save/scene native suites; Godot version/checksum CI owned by integrator | Coordinator reran the integrated cloud entrypoint after the scene fixes: core61 + save41 + scene50 passed (152 checks), exit0 with clean script logs on Godot4.6.3. Graphical/device acceptance and remote CI remain separate pending gates. Historical Unity18+2/85/etc counts are not Godot test counts. |
| Android export | Real native Godot preset/template/toolchain/package required | Exports are not configured/accepted; matching templates/SDK/signing/artifact/device prerequisites remain outstanding. No APK/AAB or Android-device pass claimed. |
| iOS export | Real native Godot preset/templates and supported export/build/signing toolchain required | Exports are not configured/accepted; no iOS project/archive/IPA or device pass claimed. A cloud host limitation may require authorized local fallback, not an invented cloud success. |
| Later content | Existing40-level design, forest progression and immutable source references retained | Only current bird-only L01 is the first native runtime milestone. Later39, eggs, full campaign migration and new help/economy systems are not silently implemented. |
| Approved UI designs | Skin shop, help-skills and settings page reference IDs remain in UI_SPEC | Keep approved originals, do not redraw. Actual new page slicing/runtime integration is not expanded during framework migration. Other layout/nest decisions retain their documented pending scope. |

## 3. Run/test commands and isolation

From repository root:

```bash
# Primary aggregate validation; GODOT_BIN may select a verified binary.
GODOT_BIN=godot bash scripts/run_tests.sh

# Native graphical run only where display/window access is available/authorized.
GODOT_BIN=godot bash scripts/run_game.sh
```

The test script imports with `--headless --path godot --editor --import --quit`, then executes `res://tests/test_core.gd`, `test_save.gd`, and `test_scene.gd`. It isolates XDG_DATA_HOME/XDG_CONFIG_HOME/XDG_CACHE_HOME into a fresh temporary test home and writes logs under `build/godot-tests`; it must not touch the user's real save. Parse/script errors must fail even if the engine process accidentally exits zero. Inspect the actual script for its latest isolation details. Do not run legacy Unity commands as current gates.

A direct focused command is `godot --headless --path godot --script res://tests/test_core.gd` after import, with explicit isolated XDG directories. A focused run is not an aggregate pass. `--headless` is for non-window execution; it does not certify screenshots, motion, input feel or audio. Capture tooling is owned by the scene/integration agents; do not invent a screenshot flag or claim a dummy renderer produced accepted visuals.

## 4. Native save version1, not Unity save6

Default directory `user://first-level-godot-v1`; filename `first-level.godot-save-v1.bin`. Tests may pass an explicit isolated directory. Envelope fields are exactly `format`, `version`, `campaign_id`, `content_hash`, `record`, `record_hash`:

- `format="perch-sort-godot"`, `version=1`, `campaign_id="bird-home-peer-v4"`.
- Current native content hash `4e912c846f652e130ff92430e71d2b6d4f053aa3264ca2c031ddd962945b7d79`; this is the bounded native identity, not proof all legacy payload/hash formats are identical.
- `record={state,history,completed:bool,best:int,audio_muted:bool}`; best=-1 means incomplete. `record_hash` is SHA256 of the native encoded record.
- Data-only `var_to_bytes` and `bytes_to_var` preserve int64 and do not encode/decode objects. No object-enabled API, arbitrary script/resource load, JSON-number coercion or trusted-deserialization assumption.
- Maximum4MiB, exact keys/types and whole-record preflight; restore verifies Session history replay and state. Unknown versions/fields/unreadable structure stay preserved/read-only. Only recognized semantic/hash corruption can recover a validated compatible backup; preserve corrupt evidence.
- Candidate bytes stay immutable through temporary-write/readback/backup/rename/readback. Outcomes distinguish committed/not_committed/uncertain. Observed stale bytes reject rather than overwrite. Single cooperating writer per physical path; no cross-process lock, fsync/power-loss durability or mobile filesystem guarantee is claimed.

Never open, overwrite or automatically import Unity save6/PlayerPrefs. Future economy save7 remains design-only and is not the new native format. No claim of legacy user progress compatibility, full campaign migration or verified historical entitlement transfer.

## 5. Engine-specific mapping and preserved constraints

| Legacy Unity mechanism | Active Godot migration mapping / boundary |
| --- | --- |
| MonoBehaviour, pure C# assembly, asmdef/UnityEngine | Pure RefCounted GDScript rules/session; Node scripts only for scene/input/audio. No active .NET/Unity package dependency. |
| Game.unity/FirstLevel.unity, SceneBuilder, BuildSettings | project.godot/main .tscn/native script initialization. Legacy scenes retained for reference, not the default launch. |
| CanvasScaler, RectTransform, SpriteRenderer, masks | Control/CanvasLayer/Node2D/TextureRect/Sprite2D, explicit anchors/stretch/z-order/clipping. Convert old center-origin/Y-up reference coordinates; do not paste them unchanged into native top-left/Y-down UI. |
| TextMeshPro SDF/materials | Original OFL Fredoka font as FontFile/Theme. Do not port Unity Companion License package shaders/TMP internals by assumption. |
| LitMotion/Animator/coroutines | Built-in Godot Tween and accepted frame sequencing; actual wings and occluded nest entry remain required. Cancel/kill obsolete owner tweens and invalidate view generation; logic/save never depend on completion callbacks. |
| AudioSource/AudioMixer | Existing original WAV through AudioStreamPlayer/bus mute; physical listening evidence still required. |
| PlayerPrefs/Unity save6 JSON | Separate native binary version1 namespace and strict validation; no automatic old-save conversion. |
| EditMode/PlayMode/Unity Player/IL2CPP | Native headless GDScript suites, actual graphical scene/runtime, then actual platform export/device gates. No inherited pass claim. |
| Unity Android/iOS build script | Godot export presets and version-matched export templates plus actual SDK/toolchain/signing; blocked prerequisites are not completed exports. |

Rules/content, user-approved art identity, English UI, fixed layout, transaction semantics, input lock, actual animation, safe areas and anti-double-commit requirements survive the engine change. Implementation APIs, serialized bytes and certificates do not transfer automatically. Preserve current colors/i18n; no new feature or asset redesign during migration.

## 6. Official API references (readable summary, not links-only)

- [Godot4.6 Tween](https://docs.godotengine.org/en/4.6/classes/class_tween.html): create through Node/SceneTree, not direct Tween.new; interpolate properties with sequential/parallel Tweeners. Owner lifecycle/cancellation must be handled before stale callbacks touch a new view. Adopt native Tween instead of the old LitMotion package.
- [Godot4.6 CLI](https://docs.godotengine.org/en/4.6/tutorials/editor/command_line_tutorial.html): `--path` selects the project; `--headless` supports non-window workflows, `--editor --import --quit` imports then exits, `--script` runs a script. Check the actual binary version and script exit/log results; a command example is not a test pass.
- [Godot4.6 Variant conversion](https://docs.godotengine.org/en/4.6/classes/class_@globalscope.html): ordinary bytes_to_var/var_to_bytes exclude objects; object-enabled variants can deserialize executable content. This project deliberately uses data-only conversion with strict own validation.
- [Godot4.6 exporting](https://docs.godotengine.org/en/4.6/tutorials/export/exporting_projects.html): export presets and export templates are required; platform-specific tooling remains necessary. A Linux headless run does not create or validate mobile packages.
- [Godot engine license](https://godotengine.org/license/): engine is MIT, with copyright/license notice obligations when redistributed. Original font/image/audio licenses remain separate; preserve notices and do not treat Unity package resources as freely portable.

These official references were read during migration documentation. They support API choices, not measured performance, app-store compliance or platform build success.

## 7. Evidence handoff / stopping condition

Remain IN PROGRESS until the coordinator records final tested commit, aggregate native tests, independent review/required CI, actual graphical behavior evidence or exact outstanding blockers, and honest export status. Merge only under the user's migration authority and repository protections after the accepted scope is met. Deployment/store release is not authorized. The coordinator final working-tree aggregate passed core61/save41/scene50 (152 checks); the preceding135-check run is historical. Scene follow-up includes nest occlusion/click/idle-foot/failure/focus states. Independent review has closed two P2 findings per coordinator; that does not bypass final CI/visual gates. The coordinator independently verified all35 migration-manifest entries byte-identical to original repository sources, including the font license; the earlier34 count excluded a manifest entry. Actual local fallback engine version must be recorded, not assumed equal to cloud4.6.3. Any unmet mobile/runtime requirement must remain explicit rather than hidden behind 'migration complete'.
