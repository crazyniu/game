# Pattern library

Every control in every screen uses one of these entries. Sizes are 1080x1920 design px. Same role = same size in every state; disabled is dimmed (alpha 0.45) and inert, never smaller.

## Buttons
| Pattern | Use | Size | States |
|---|---|---|---|
| primary-bar | the single main action of a panel (play, next, confirm, got it) | 520 x 120 (inside cream area, margin >= 28 px); at most 1.25x a sibling disc | normal; pressed (scale 0.92); disabled; icon only (check, arrow, play), 64 px white, dark-green outline |
| wood-disc | secondary and icon actions (settings, replay, home, close, tabs, tools) | 108 (HUD, map bar), 100 (inside panels) | normal; pressed (btn_wood_pressed + scale 0.92); disabled (alpha 0.45); selected (gold ring 190 px + green pill 70x28 for tabs) |
| tool-disc | HUD tools | 108, glyph coloured 54 percent | normal; pressed; disabled (alpha 0.45, same size); badge red 64 px count or plus at zero stock; hidden while the action does not exist (undo with nothing to undo) |
| close-x | closes a modal | wood-disc 100 at the panel's top-right corner, never covered | as wood-disc |
| card-button | shop and aviary cards | card art size; press scale 0.92 | normal; pressed; locked (grey card + padlock chip with unlock level); unaffordable (price pill red + shake) |

## Display
| Pattern | Size | Notes |
|---|---|---|
| coin-pill | 190 x 76, top-left (48, 150) | coin disc overlaps the left end; number centred in the right area; 99999 fits with margin; never touches ropes or the house |
| move-pill | 190 x 64 under the coin pill | footprint icon + counted moves + three stars 26 px with limits; star dims on loss; free-move state (green flash + feather) |
| goal-house | 240 x 240 centred, top 150, one rope | empty doorway; resident bird peeks after a delivery; glow on delivery |
| progress-badge | 44 px (38 beyond 7) under the house | face in gold ring; dimmed until home; green check; gift coin 22 px bottom-right |
| name-ribbon | baked image per species, up to 640 x 200 | text inside the green face; never live text |
| level-badge | baked leaf per level number | number baked |
| star-row | stars 120 (card) / 140 (win) | earned gold, unearned dim; move limit number under each |
| stat-pill | 440 x 72 | best moves and target, numbers only |
| gift-coin | 84 px coin with gift icon | plus two-word effect (max 18 characters) |
| count-chip | house icon + number, right area measured | |

## Containers
| Pattern | Rules |
|---|---|
| modal-panel | cream interior measured by tools/layout_metrics.py; children inside the inner rect with 24 px margin; drop shadow (offset +14 px, alpha 0.35); dim black alpha 0.72 over everything including birds and HUD; input blocked behind |
| page | full-screen with coin-pill and close-x; scroll viewport ends above/below bars |
| tabs | three wood discs at y 330, x 350/540/725; selected = gold ring + green pill |
| list-grid | 2 columns, cards from art metrics, scroll indicator right |
| tooltip/bubble | cream rounded bubble with tail, icon-only, shows 1.6 s |

## Animation standards
| Name | Used by | Duration | Easing | Overshoot | Skippable | Reduced motion |
|---|---|---|---|---|---|---|
| panel-open | modal, page | 0.18 s | ease-out | scale from 0.92 | no | instant |
| panel-close | modal | 0.12 s | ease-in | to 0.92 | no | instant |
| button-press / release | all buttons | 0.08 s / 0.18 s | linear / ease-out-back | 0.92 | n/a | none |
| counter | coins, moves | 0.5 s | ease-out | none | no | instant |
| badge-pop | delivery, purchase | 0.3 s | ease-out-back | 1.3 | no | none |
| star-pop | win | 0.35 s, 0.18 s apart | ease-out-back | 1.25 | tap | instant |
| level-entrance | board | 3.5 to 4.5 s | per element | birds land with 0.10 s squash | tap | instant |
| bird-flight | moves | drawn frames 14 fps | ease path | landing squash | n/a | none |

## Sound standards
| Name | Used by | Trigger | Group | Muted |
|---|---|---|---|---|
| ui-tap | all buttons | pointer down | UI | silent |
| booster-<kind> (six) + undo | tools | when the effect applies | SFX | silent |
| coin-spend / coin-gain | shop, win | counter start | SFX | silent |
| win-jingle, star-pop | win | panel open, each star | SFX | silent |
| bgm-menu, bgm-game | map, level | page change, 1 s crossfade | Music | silent |
| refuse | illegal move, locked card | on refusal | SFX | silent |

## Consistency
Back and close behave the same everywhere (close-x or Android back closes the top modal). Two buttons with the same intent share a pattern.
