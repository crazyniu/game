# Chest materials delivery

## Implemented

- Edited only chest-owned constants/functions and generic `rewards` in `PerchSort/game/popups.gd`.
  Acquired the documented `popups.lock`, reread the current source, applied the narrow patch and
  released the lock in a finally block. Preserved adjacent ongoing popup work.
- Removed the large cream hero/progress panels. The chest now sits on a sandy honey-wood islet,
  with warm rotating rays, existing glow/sparkles and a separate star-progress track over the sea.
- Replaced chest and generic reward cards with shallow arcs of reward objects and painted amounts.
  Repeated booster kinds share an object with the actual total. Generic framing remains delegated
  to `Modal`; draw rewards and win are outside this change.
- Retained chest action geometry (560 x 132), `Modal.invoke` callbacks, readiness, badge counts,
  progress threshold, balance, preview paths and existing sound/music ownership. No awarding,
  economy, profile, locale or shared-helper changes.
- Added instant-mode guards to the owned reward prop effects. Live rays rotate in 28 seconds;
  chest and reward props use existing breathing, ready-state sparkles use existing twinkle.

## Asset and documentation

Generated one independent production sprite with built-in Codex `image_gen`, saved unchanged to
`PerchSort/art/island/chest_islet_support.png` (2172 x 724 RGBA). Source:
`/Users/apple/.codex/generated_images/01a1cae2-0852-78d0-96a8-8c910658999a/exec-40875651-2158-4128-a4c3-22cea6a045d6.png`.
The original generated file and all existing game assets are retained. No design-image pixels
were copied or sliced. `docs/design/star-chest.md` contains the layout and exact generation prompt.
No shared-helper change is requested.

## Inspection and acceptance

Read-only source inspection used `rg`, `sed` and `cat`; raw assets were viewed directly.
`file PerchSort/art/island/chest_islet_support.png` reported the PNG dimensions and RGBA format.
No tests, naming checks, lint, builds, exports, game launches, UI automation, runtime screenshots,
motion capture or temporary validation programs were written or run.

The canonical preview states remain `chest_empty`, `chest_half`, `chest_ready`, `chest_multiple`,
`rewards_chest` and `rewards_chest_distinct`. Generic challenge rewards still use the same live
`rewards` entry, including the no-prize completion picture.

Runtime loading, visual placement, localized fit, interaction, exactly-once behavior, motion,
sound and device performance are **NOT ASSESSED**. The user owns testing and visual acceptance.
