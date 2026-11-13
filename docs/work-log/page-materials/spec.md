# Page presentation repair coordination

Status: the user stopped this multi-page dispatch. Do not resume any old assignments. Only later, explicit page-specific approvals authorize work. Settings has been restored and is frozen. The current Shop request is design-preview-only until user confirmation.

The user explicitly requested separate Codex chats to repair each page after approving the earlier
UI critique. The original direction is bright tropical scenery, detailed birds, painted lettering,
warm wood and dimensional controls. Large repeated cream rectangles are the defect; do not merely
replace every cream card with another uniform card. Keep useful small labels, prices and controls.
Do not add promotional copy, explanations, new gameplay or redundant UI.

## Current scope and file ownership

All chats use the current local `island-ui-redesign` working tree, including its existing uncommitted
work. Do not pull, reset, stash, commit, switch branches, merge, deploy or release. Do not create
alternate runtime implementations. No chat may spawn additional workers for this dispatch.

| Chat | Owned runtime surface | Design documentation |
| --- | --- | --- |
| Home title repair | `game/home_view.gd` | Home section of `docs/design/home-and-aviary.md` |
| Shop | `game/shop_view.gd` | `docs/design/shop.md` |
| Aviary | `game/aviary_view.gd` | Aviary section of `docs/design/home-and-aviary.md` |
| Coin packages | `game/coin_shop_view.gd` | `docs/design/coin-shop.md` |
| Daily draw | Only `lucky_draw`, `_draw_rewards` and DRAW-specific constants in `game/popups.gd` | `docs/design/daily-draw.md` |
| Star chest and rewards | Only chest-specific functions/constants and generic `rewards` in `game/popups.gd` | `docs/design/star-chest.md` |
| Map | `game/map_view.gd` | `docs/design/map.md` |
| Win and settings | Only `win`, `settings` and their private, exclusive helpers in `game/popups.gd`; `game/popup.gd` | `docs/design/modal-materials.md` |

Runtime paths above are relative to `PerchSort/`. Shared helpers (`shop_skin.gd`, `ui.gd`, `island.gd`,
`ui_text.gd`), locale manifests, gameplay, profile, audio and the global `island-style.md` are read-only
to page chats. Existing styles may be reused, and necessary page-local presentation helpers may be
added in the owned script. Newly generated assets require unique semantic names; retain originals.
Do not overwrite a shared asset or word catalog. Use current painted words and digits.

## Shared-file write ownership

Different popup pages share one source file. Their ownership is sequential during edits, never
simultaneous: atomically acquire `build/ui-page-locks/popups.lock` with `mkdir` immediately before
editing `popups.gd`. On success record the chat title in `owner`, reread the current file, apply only
the assigned sections, remove `owner`, and release with `rmdir` in a finally block. Never hold the
lock during research, asset generation, user waits or unrelated work. When the lock exists, prepare
independent work and retry later; never delete a lock owned by another chat. Release promptly.

The Home/Aviary document uses the same protocol with `build/ui-page-locks/home-and-aviary.lock`.
Only its assigned section may be changed. Each chat owns its separate subject-named work-log
spec/report, and records any requested shared-helper change there instead of editing it.

## Behavioral and acceptance contract

Preserve signals, purchase/equip behavior, availability, zero/maximum balances, locale switching,
preview interfaces, save fields, levels and music/mute behavior. Existing card-shaped world previews,
draw cells and compact settings rows are legitimate structure; improve their purpose and hierarchy.
Avoid copying a design image into runtime or extracting its pixels. Follow UI-first development.

The user's current instruction overrides older verification requirements: no tests, naming guards,
lint, builds, exports, application launches, UI automation, screenshots, motion captures or temporary
validation programs. Inspect source and supplied/existing images only. Deliver edited canonical
screens for user review, and distinguish implemented changes from unassessed runtime behavior.

The screenshot `/var/folders/qp/0_msc8wx3pv1c6jghk9fj2nh0000gn/T/codex-clipboard-a591f1f2-4c71-4c4b-bcb3-fdb2838b7328.png`
is the latest Home defect: both title mascots float outside the wordmark. It must be corrected by
registering feet to the actual title artwork, without removing the birds or changing main-scene birds.
