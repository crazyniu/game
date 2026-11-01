# T22 coordinator integration checkpoint

The coordinator submitted the reviewed first-level save snapshot in commit `30625eba835dd3f924862a47590bfed5e623e7cc`, based on `62ea0e404b86aa4248f7350f3cd2078ce9025ef4`. The coding worker did not retry its rejected git commit. Two independent read-only reviews confirmed the scoped hardening fixes before submission.

This follow-up integrates the exact five pending consumer files from the original T21 worktree: FirstLevelEntry, FirstLevelFlow, FirstLevelHud, FirstLevelFlowTests and the FirstLevel PlayMode test asmdef. Originals were read-only copied and remain untouched; no working-tree reset or forced push is permitted.

## Evidence

- Complete reviewed project input snapshot: `86ae745b348e60d24cae4b5905ab35bf7ec07e634a039ed546e4f600dc2bdb31`. Actual production asmdefs compiled; three selected persistence cases passed, zero failed/skipped, in Unity 6000.6.7f1.
- Complete project plus this exact five-file consumer overlay: `3885d1e255c433400b678eb7641c1fbefa5597b90655c208212053ed7a169978`. Actual production asmdefs compiled; two selected Flow cases passed, zero failed/skipped. These use controlled/no-op presenters and are not rendered UI evidence.
- The store's earlier 85-case isolated verification remains separately scoped. It was not rerun in full for this integration.
- Independent read-only consumer review found no confirmed P1/P2: durable commit precedes Core/presentation, failed candidates stay immutable, SaveError locks input, audio/game retry purposes remain separate, Home retains pending state, and generation guards protect replacement scenes. The Presenting pointer-Pause fix is retained.

## Limits and next gate

No operation on the Perch Sort Player window was performed for these checks. Its computer-use authorization remains separate and pending. No microphone, recording permission, payment, PR metadata edit, merge or deployment was performed. Actual window/touch behavior, rendered persistence error/recovery, audible output, mobile/AOT and power-loss behavior remain unverified. Same-process normalized-path serialization does not guarantee cross-process or physical-alias writer safety.

This is a code/compile/targeted-test integration checkpoint, not completion of the first-level product or the full v4 campaign. Continue first-level validation in priority order without rewriting unrelated levels or implying that the legacy v2 runtime has migrated.
