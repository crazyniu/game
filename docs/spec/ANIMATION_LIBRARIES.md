# Open-source animation options, licenses, and compatibility gates

2026-10-31 · Research/design only; **no libraries installed, no runtime code written, and no verification on this project's Unity 6000.6.7f1 or mobile IL2CPP**. This page alone owns selection/licenses; INTERACTION owns animation durations and RULES owns gameplay. The user requires open-source libraries where suitable, one primary tween library, and no fixed images standing in for completed animation anywhere. These are USER constraints, not competitor rules.

## 1. Recommendation and comparison

| Option | Verified first-party facts | Project decision/limits |
| --- | --- | --- |
| [LitMotion](https://github.com/annulusgames/LitMotion) | MIT; official requirements Unity≥2021.3, Burst≥1.6, Collections≥1.5.1, Mathematics≥1.0; supports Transform/UI/TMP tweening, Punch/Shake, LSequence | **Preferred candidate for the sole primary tween library**, pending authorization to pin a version and test compatibility; not installed/integrated. Readable source does not prove allocation-free frames in this project or acceptance on every platform |
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

Current read-only project facts: 10 species×stand/blink/sleep/fly gives 40 static pose textures; UIAssetBuilder loads four poses, BirdView.SetPose switches Sprites, and BoardView.FlyRun combines a single fly pose with translation/rotation/scale. This proves asset integration, **not multiframe wingbeats/skeletal clips, and does not meet the latest real-wingbeat requirement**. Do not redraw/replace existing characters; after future authorization, supply missing motion assets/clips for separate acceptance. Static backgrounds and raw frames may serve as assets, but final animation as a whole must change over time. Reduced motion still retains a short state-transition sequence rather than one image occupying the whole action.

## 4. Before future implementation and at delivery acceptance

- Confirm installation/code/asset authority and follow 251 development/Mac testing roles; this document does not authorize installation commands.
- Pin the primary library and dependency versions; verify Unity 6000.6.7f1, iOS/Android IL2CPP, AOT/stripping, device performance, and package conflicts. Minimum-version claims cannot replace testing.
- Check pause/background/IgnoreTimeScale, repeated taps, Undo/Restart/Next, scene destruction, pool return/reuse, and bulk cancellation; old viewGeneration handles cannot write to new objects. Returning to a pool is not GameObject destruction: AddTo destruction binding alone is insufficient, and each rental lifecycle needs cancellation.
- Completion/cancellation/callback races must still consume a committed event only once, without duplicate Home/Retire/unlocking/rewards. Animation failure must not roll back committed rules or permanently lock input; synchronize from the stable snapshot.
- Record playable actual runtime videos and timing samples for each action; screenshots supplement layout only. Check real wingbeats, nest-entry occlusion, and particle lifetimes; static images cannot count as completed animation.

Current status: research complete; final version pinning, installation, code/mobile compatibility, asset clips, and runtime acceptance are all NOT_RUN.
