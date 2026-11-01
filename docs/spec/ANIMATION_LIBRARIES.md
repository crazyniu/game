> **T30 current-engine scope (2026-11-01):** [GODOT_MIGRATION](../GODOT_MIGRATION.md) owns active implementation, cloud-first testing and gaps. Unity/C#/LitMotion/PlayerPrefs/Canvas/asmdef commands, APIs, dimensions and test results retained below are **legacy evidence/reference, not active Godot instructions or pass claims**. Engine-independent rule/content/asset-approval requirements remain; current engine mappings are in the migration matrix. Other feature expansion is stopped during migration. Native save version1 is isolated from Unity save6 and design-only economy save7.

# Current primary animation: Godot built-in Tween

Godot native Tween is the sole current primary tween mechanism; LitMotion is legacy Unity reference, not an install prerequisite. Godot is open source under MIT; retain engine and reused-asset license notices. Create a tween through `Node.create_tween()` or `SceneTree.create_tween()`, not `Tween.new()`; bind its lifetime to the presentation owner and invalidate stale callbacks on restart/view-generation change. Native Sprite2D/AnimatedSprite2D frame sequencing uses the existing six accepted flight poses, with standing/selected states, not a translated single fly sprite. A small amount of event orchestration is appropriate; do not add a second framework.

Use Tween position/rotation/scale/modulate for travel/entry/fade, and native audio players for existing sounds. Finished flock retirement fades branch and supporting root as one visual unit; it does not alter Core completion. Pause/background/repeated taps/Undo/Restart must consume committed state exactly once. Headless tests cannot prove wingbeat readability, mask occlusion, input feel or sound. Read the official APIs and current evidence in GODOT_MIGRATION. All library-selection instructions below are preserved Unity-era evidence only.

## Retained legacy/reference details (not current engine workflow)

# Open-source animation options, licenses, and compatibility gates

2026-10-31 · **LitMotion 2.0.2 selected and pinned; isolated Mac UPM resolution and scoped Unity 6000.6.7f1 compile PASS. Motion/UI integration, accepted motion assets, runtime animation and mobile IL2CPP remain NOT_RUN.** See the [T21 execution record](../../tasks/T21_motion_dependency.md) for the exact source SHA, filtered test and limits. This page alone owns selection/licenses; INTERACTION owns animation durations and RULES owns gameplay. The user requires open-source libraries where suitable, one primary tween library, and no fixed images standing in for completed animation anywhere. These are USER constraints, not competitor rules.

## 1. Selected primary library and alternatives

| Option | Verified first-party facts | Project decision/limits |
| --- | --- | --- |
| [LitMotion](https://github.com/annulusgames/LitMotion) | MIT; selected 2.0.2 commit 0b4c588ee75a07198841d92aab653e6b39445089; pinned package declares Burst 1.6.0, Collections 1.5.1 and Mathematics 1.0.1; supports Transform/UI/TMP, Punch/Shake and LSequence | **Sole selected primary tween library.** Exact pin resolved and scoped desktop compile/test passed in isolated Unity 6000.6.7f1; [T21 evidence](../../tasks/T21_motion_dependency.md). No runtime integration or mobile/AOT/performance acceptance. [Retained MIT notice](../licenses/LitMotion-MIT.txt) |
| [LeanTween](https://github.com/dentedpixel/LeanTween) | [MIT in source header](https://raw.githubusercontent.com/dentedpixel/LeanTween/master/Assets/LeanTween/Framework/LeanTween.cs), with separate BSD notice for easing equations; common tween/path capabilities; exact version/target platforms need future verification | Review as an alternative only if testing finds the preferred path solution insufficient; do not duplicate LitMotion's primary tween role |
| [DOTween](https://dotween.demigiant.com/license.php) | Custom license permitting use with restrictions on redistribution of modified versions; not MIT | Do not casually label it “MIT open source”; not the default preference. [Download notes](https://dotween.demigiant.com/download) record a Unity 6000.6 meta workaround; verify channel/version rather than substituting broad compatibility claims for tests |
| [Rive Unity](https://github.com/rive-app/rive-unity) | MIT code, technical preview status, and separate .riv asset pipeline | Do not add a complete new pipeline to this project now; the license does not cover arbitrary third-party .riv content |
| Unity Animator / Particle System / [Splines](https://docs.unity3d.com/Packages/com.unity.splines@2.8/manual/index.html) | Native Unity tools/official packages, not independent MIT tween libraries | Respectively provide character clips, real particles, and official path tools; do not claim all Unity engine code is MIT open source |

This table restates current first-party declarations and the parent task's independent checks; it is not legal advice or a completed dependency audit. Future selected versions must retain that version's license/NOTICE, source, tag/commit, dependency versions, and target-platform record. Library licensing does not replace asset licenses for characters/sounds/particle textures.

## 2. What can be reused directly and what cannot be assumed

[LitMotion motion control](https://annulusgames.github.io/LitMotion/articles/en/motion-control.html) provides MotionHandle, TryCancel, AddTo(GameObject), and CompositeMotionHandle for existence checks, destruction binding, and bulk cancellation. Repeated calls to finished handles must not assume perpetual validity. Use library Scheduler/PlaybackSpeed capabilities for pause/resume scheduling; UI that continues on the pause page must verify an IgnoreTimeScale-style clock instead of hand-writing a second timing framework.

The [official migration guide](https://annulusgames.github.io/LitMotion/articles/en/migrate-from-dotween.html) explicitly lacks full one-to-one DOTween equivalents for paths, AppendCallback, or SetSpeedBased; it recommends Unity Splines for paths. Do not invent calls based on similar API names or write a separate tween/spline/scheduling framework. If a small amount of game-event orchestration is necessary, consume already computed event lists and call library Sequence/cancellation APIs; do not move logic into animation callbacks.

## 3. Accept animation libraries and assets separately

| Action | Library/tool responsibility | Required evidence of real motion |
| --- | --- | --- |
| Takeoff/cross-branch/landing | Primary tween library controls position/rotation/scale and staggering; Animator or accepted frame sequences control wingbeats/folding | Wing shapes actually cycle, feet land, and wings fold; translating/rotating a single fly image is not real wingbeats |
| Nest entry | Library orchestrates approach/entrance shrinking/landing/occlusion/reception | Birds travel from in front of the nest into it, with front/back occlusion and individual reception, rather than static overlays/instant deletion |
| Completed branch exit/question-mark hatching | Library and approved frames/clips present committed events | Branch exit changes over time; shell→cracked shell→bird has stages; animation cannot define triggers |
| Results | Primary library sequences panel/character motion; Particle System handles emission/lifetimes | Runtime recording of particles actually spawning/moving/dying, panel entry, and character motion; a celebration poster fails acceptance |
| Map/buttons/dialogs | Same primary library handles fades/transitions/presses | Click feedback/locked states/world-opening order are genuinely interactive; no fake image buttons |

Legacy v2 implementation facts: 10 species×stand/blink/sleep/fly gives 40 static pose textures; UIAssetBuilder loads four poses, BirdView.SetPose switches Sprites, and BoardView.FlyRun combines a single fly pose with translation/rotation/scale. This proves asset integration, **not multiframe wingbeats/skeletal clips, and does not meet the latest real-wingbeat requirement**. Follow [UI_SPEC §4.1](../UI_SPEC.md#approved-forest-style): bird redesign in the approved forest direction is permitted, existing originals must remain preserved, and new production assets/clips require separate sliced/model and in-scene acceptance plus implementation authorization. Style approval does not accept motion assets or authorize replacing originals. Static backgrounds and raw frames may serve as assets, but final animation as a whole must change over time. Reduced motion still retains a short state-transition sequence rather than one image occupying the whole action.

## 4. Before future implementation and at delivery acceptance

- Confirm installation/code/asset authority and follow 251 development/Mac testing roles; this document does not authorize installation commands.
- Pin the primary library and dependency versions; verify Unity 6000.6.7f1, iOS/Android IL2CPP, AOT/stripping, device performance, and package conflicts. Minimum-version claims cannot replace testing.
- Check pause/background/IgnoreTimeScale, repeated taps, Undo/Restart/Next, scene destruction, pool return/reuse, and bulk cancellation; old viewGeneration handles cannot write to new objects. Returning to a pool is not GameObject destruction: AddTo destruction binding alone is insufficient, and each rental lifecycle needs cancellation.
- Completion/cancellation/callback races must still consume a committed event only once, without duplicate Home/Retire/unlocking/rewards. Animation failure must not roll back committed rules or permanently lock input; synchronize from the stable snapshot.
- Record playable actual runtime videos and timing samples for each action; screenshots supplement layout only. Check real wingbeats, nest-entry occlusion, and particle lifetimes; static images cannot count as completed animation.

Current status: LitMotion 2.0.2 at commit 0b4c588ee75a07198841d92aab653e6b39445089 is pinned; isolated Mac UPM resolution and one filtered desktop compile/Core witness PASS (1/1, exit 0), as scoped in [T21](../../tasks/T21_motion_dependency.md). Exact generated lock reconciliation is handled by the coordinator. Motion/UI integration, accepted new motion assets, cancellation/runtime recording, mobile IL2CPP/AOT/stripping and device performance remain NOT_RUN; existing v2 motion is unchanged.
