# T21 — First-level motion dependency pin

Date: 2026-10-31. Scope: dependency preparation for the separately authorized first-level vertical slice. This note does not authorize a 40-level migration, UI integration, asset replacement, additional libraries, or broad audit cleanup.

## Requirements and owned changes

- Preserve every existing dependency in PerchSort/Packages/manifest.json.
- Add only com.annulusgames.lit-motion, pinned to the immutable commit below rather than a moving branch or tag.
- Keep the existing v2 animation implementation unchanged until the production asset/UI acceptance gate is met.
- Author this manifest and note on 251 only. Do not run installers or compilers on 251, manually edit packages-lock.json, or commit these changes from this agent.
- The coordinator will ask Unity Package Manager to resolve the package and run one compatibility compile in an isolated Mac test copy. Only that generated and reviewed lockfile may return to the 251 branch.

## Selected package and primary evidence

| Item | Fixed value / source |
| --- | --- |
| Package ID | com.annulusgames.lit-motion |
| Upstream package version | 2.0.2 |
| Immutable source commit | 0b4c588ee75a07198841d92aab653e6b39445089 |
| Manifest reference | https://github.com/annulusgames/LitMotion.git?path=src/LitMotion/Assets/LitMotion#0b4c588ee75a07198841d92aab653e6b39445089 |
| Package manifest at that commit | [Pinned package.json](https://github.com/annulusgames/LitMotion/blob/0b4c588ee75a07198841d92aab653e6b39445089/src/LitMotion/Assets/LitMotion/package.json) |
| License | MIT; [pinned LICENSE](https://github.com/annulusgames/LitMotion/blob/0b4c588ee75a07198841d92aab653e6b39445089/LICENSE) |
| Repository | [annulusgames/LitMotion](https://github.com/annulusgames/LitMotion) |
| Existing owning specification | [ANIMATION_LIBRARIES](../docs/spec/ANIMATION_LIBRARIES.md) |

The exact pinned upstream package manifest and license were read as first-party evidence. Its package ID and version match the selected dependency. MIT permits the planned integration while requiring retention of the copyright and license notice with copies/substantial portions. Keep the upstream notice with redistributed package code; this license does not grant rights to third-party artwork or audio.

The owning specification remains authoritative for single-primary-library selection, asset acceptance, and motion behavior. Its older research-only installation status describes the documentation stage; this task advances only manifest pinning, not installation or runtime acceptance.

## Dependency compatibility record

| Dependency | Pinned LitMotion package declaration | Existing project lock before this edit |
| --- | --- | --- |
| com.unity.burst | 1.6.0 | 2.0.0, built-in |
| com.unity.collections | 1.5.1 | 6.6.0, built-in |
| com.unity.mathematics | 1.0.1 | 1.4.0, built-in |

The project lock was inspected read-only on 251. Larger version numbers, especially across major releases, are not evidence of source/binary compatibility. Do not downgrade, add direct overrides, replace built-in packages, or edit their lock entries to force a result. If Unity cannot resolve or compile this pin, report the exact package conflict/compiler diagnostic to the coordinator before changing package policy.

## Acceptance and verification status

| Check | Status / evidence |
| --- | --- |
| Exact package ID/version/commit and MIT primary sources | VERIFIED by reading the pinned upstream package manifest and license |
| Preserve all existing manifest dependencies; add only the selected pin | Checked by comparing parsed manifest dictionaries before/after |
| Manifest JSON syntax and whitespace | Checked on 251; this is not a Unity package-resolution test |
| Lockfile integrity during 251 authoring | Read-only; before/after bytes must remain identical |
| Unity Package Manager resolution | NOT_RUN by this agent; isolated Mac validation pending |
| Unity 6000.6.7f1 compatibility compile | NOT_RUN by this agent; coordinator owns isolated Mac evidence |
| iOS/Android IL2CPP, AOT and stripping | NOT_RUN; desktop compile cannot satisfy these gates |
| First-level runtime tween integration and animation acceptance | NOT_IMPLEMENTED in this step |
| Existing v2 motion replacement | NOT_STARTED; asset/UI acceptance gate still applies |

A manifest URL is an authored dependency request. Do not report the package as installed, compatible, or integrated until the corresponding Mac evidence exists. The coordinator must review the generated lock diff, actual resolved commit, dependency changes and compiler result, then record the result separately. Unexpected transitive changes remain review items rather than being accepted merely because UPM generated them.

## Boundaries before future first-level integration

1. Use LitMotion as the sole primary tween library; do not add a second tween framework or hand-written scheduling infrastructure. Use verified APIs rather than DOTween-shaped guesses. Official path tooling can be evaluated separately only when required and authorized.
2. Position/rotation/scale tweening does not create real wingbeats. Accepted clips/frames must cycle wing shapes; nesting needs a visible approach/entry and occlusion, and celebration needs actual motion/particle lifetimes. A single fixed pose moving across the screen does not satisfy the animation requirement.
3. Bind handles to the scene/object lifetime and cancel them for each pool rental lifecycle. Destruction binding alone does not cancel a pooled object returned without destruction.
4. Verify pause/background/resume and clocks, repeated taps, Undo/Restart, scene destruction, bulk cancellation and late callbacks. Old view generations must not write to replacement objects or unlock new input.
5. Consume already committed Core events exactly once. Completion/cancellation callbacks must not duplicate Home, branch retirement, progress or rewards; motion failure must resynchronize from a stable snapshot without permanent input lock.
6. Later mobile acceptance must explicitly cover IL2CPP/AOT, stripping, device performance, package conflicts and true runtime recordings. Screenshots and successful desktop compilation are insufficient.
