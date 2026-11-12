# Final feedback visual review

## Current status

All 92 canonical states now have current bilingual visual evidence: 184 frames, comprising 60 fresh final-integrated captures and 124 exact-image, source-scoped reuse frames. All affected Family/Purchase/Map frames have been reviewed, including the final Map star-detail source b9a4683e4b8926af1bbac4d8a942d0092027dd68460a22f5fa49b33c9ddb285f. The five final processes exited 0 and reported only unsupported V-Sync warnings. The independent world_complete_1 English repeat is complete and has no XServer or script error. Its earlier warning PNG/log remain intact and explicitly diagnostic.

The original 184-frame manifest and raw bytes remain preserved through feedback-checkpoint.json and per-record checkpoint paths/hashes. The current capture-manifest.json and feedback-capture-manifest.json contain the same 184 verified records. Every record includes locale, original resolution 1080×1920, exact path/hash, capture epoch and source scope. No old raw was silently overwritten. Reuse is now verified: all 124 raw hashes match, 66 of 69 existing game scripts/shaders are byte-identical to the updated N1 checkpoint, and only the intended Family/Purchase/Map files changed. All 2948 original art/bird/world assets are byte-identical. Final Map preserves 19 selector function bodies, 20 selector constants and the clipping shader; the updated route/star helpers are not used by those reused selector frames.

## Visual findings

The 34 Family and 22 purchase frames pass the scoped visual review. Title groups, visible words, complete icon/amount unions and action groups were judged separately against their own panels and the screen; the earlier label-box-only metric was not used as proof. New bright green/gold panels have continuous edges without a visible nine-patch seam. Existing accepted titles retain their outlines; no extra surround or new clipping is visible. Gold Level numerals match the title's visible height at 1, 12 and 100 in both languages. Close is fully above the panel, while primary/secondary actions and Buy/OK stay fully below where present. Disabled controls retain their dimensions. This is an engineering visual review, not a replacement for user aesthetic acceptance.

Sunny detail remains frozen at Aviary SHA f5d3ee9a35c2cb07a94ad219304a76694f0aa2c026b0469543ad106ae6fa01d6. Only PurchaseDialog's opt-in surface changes the Aviary confirmation. Home, Settings, unframed Shop/CoinShop/Aviary, Draw, Chest, documents and instructional overlays were not modified by this work. No gameplay, production code, accepted art, upload or protected UI change was made in this review.

The four final Map renders pass: connected trunk/branch faces, visible numeral centers, centered three-star unions, bird contact with the branch, root clearance and bilingual plaques are consistent. The full renders were inspected separately from numerical box checks. [Final Map contact](evidence/feedback/map-final.jpg).

## Motion samples and diagnostics

Three separate processes captured 16 actual frames each, and all completed with FAMILY CAPTURE COMPLETE and exit 0. Win samples show leaves parting, stars/coins counting up and birds entering/changing poses. Berry samples show looking, spreading wings, closing eyes and bowing. World-unlock samples show closed doors opening fully. Sampled material/title/action positions remain stable.

| Motion | First/last elapsed milliseconds | Sample span |
| --- | --- | --- |
| win_three_stars | 1317 / 18613 | 17296 ms |
| new_friend_berry | 1170 / 18531 | 17361 ms |
| world_unlock_2 | 1175 / 19499 | 18324 ms |

These are sample/readback timestamps on software-rendered X11 OpenGL 4.5 Mesa 25.0.7 llvmpipe, not FPS or four-second real-time playback claims. All 64 process logs (61 still captures including the repeat, plus three motion processes) include the unsupported V-Sync warning. The first world_complete_1 EN process also records BadAccess and BadShmSeg XServer diagnostics; that original is not a clean-log pass. Its independent repeat completed with no XServer/script error and supplies the current canonical frame. No script error was found in this capture batch. Earlier Home headless shader compilation and Draw/Nav shutdown diagnostics remain documented in the checkpoint review and are not erased by these captures. No physical-device or human listening/audio coverage is claimed.

## Per-state review

“Pass (reused)” retains the prior actual-image review with verified source/art scope, not a new rendering. Every affected state uses fresh final integrated evidence. Raw paths, hashes, locale, dimensions, capture source and warning log are in feedback-capture-manifest.json. Contacts below are render-derived inspection sheets, never replacement artwork.

| Canonical state | EN | ZH | Current evidence |
| --- | --- | --- | --- |
| `home_view:home` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `home_view:home_new` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `home_view:home_max` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `map_view:cards` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `map_view:cards_late` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `map_view:cards_all` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `map_view:cards_locked` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `map_view:world_page` | Pass | Pass | [EN/ZH](evidence/feedback/map-final.jpg); final integrated Map source |
| `map_view:world_page_late` | Pass | Pass | [EN/ZH](evidence/feedback/map-final.jpg); final integrated Map source |
| `shop_view:boosters` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `shop_view:boosters_empty` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `shop_view:boosters_maximum` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `shop_view:outfits` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `shop_view:outfits_some_unmet` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `shop_view:bird_outfits` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `shop_view:houses` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `shop_view:confirm_booster` | Pass | Pass | [EN](evidence/feedback/purchases-en-1.jpg) / [ZH](evidence/feedback/purchases-zh-1.jpg) |
| `shop_view:confirm_outfit` | Pass | Pass | [EN](evidence/feedback/purchases-en-1.jpg) / [ZH](evidence/feedback/purchases-zh-1.jpg) |
| `shop_view:confirm_outfit_short` | Pass | Pass | [EN](evidence/feedback/purchases-en-1.jpg) / [ZH](evidence/feedback/purchases-zh-1.jpg) |
| `shop_view:confirm_multiple` | Pass | Pass | [EN](evidence/feedback/purchases-en-1.jpg) / [ZH](evidence/feedback/purchases-zh-1.jpg) |
| `shop_view:confirm_maximum` | Pass | Pass | [EN](evidence/feedback/purchases-en-1.jpg) / [ZH](evidence/feedback/purchases-zh-1.jpg) |
| `shop_view:confirm_nest` | Pass | Pass | [EN](evidence/feedback/purchases-en-1.jpg) / [ZH](evidence/feedback/purchases-zh-1.jpg) |
| `shop_view:confirm_nest_short` | Pass | Pass | [EN](evidence/feedback/purchases-en-2.jpg) / [ZH](evidence/feedback/purchases-zh-2.jpg) |
| `shop_view:locked_tools` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `shop_view:insufficient` | Pass | Pass | [EN](evidence/feedback/purchases-en-2.jpg) / [ZH](evidence/feedback/purchases-zh-2.jpg) |
| `coin_shop_view:packages` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `coin_shop_view:empty_balance` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `coin_shop_view:max_balance` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `coin_shop_view:purchase_preview` | Pass | Pass | [EN](evidence/feedback/purchases-en-2.jpg) / [ZH](evidence/feedback/purchases-zh-2.jpg) |
| `aviary_view:list` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `aviary_view:list_none_met` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `aviary_view:list_some_unmet` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `aviary_view:list_scrolled` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `aviary_view:detail_sunny` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `aviary_view:detail_berry` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `aviary_view:detail_crow` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `aviary_view:detail_hoot` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `aviary_view:detail_ember_skin` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `aviary_view:detail_maximum` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `aviary_view:buy_outfit` | Pass | Pass | [EN](evidence/feedback/purchases-en-2.jpg) / [ZH](evidence/feedback/purchases-zh-2.jpg) |
| `aviary_view:buy_outfit_short` | Pass | Pass | [EN](evidence/feedback/purchases-en-2.jpg) / [ZH](evidence/feedback/purchases-zh-2.jpg) |
| `aviary_view:bought_outfit` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `popups:win_three_stars` | Pass | Pass | [EN](evidence/feedback/family-en-1.jpg) / [ZH](evidence/feedback/family-zh-1.jpg) |
| `popups:win_two_stars` | Pass | Pass | [EN](evidence/feedback/family-en-1.jpg) / [ZH](evidence/feedback/family-zh-1.jpg) |
| `popups:win_one_star` | Pass | Pass | [EN](evidence/feedback/family-en-2.jpg) / [ZH](evidence/feedback/family-zh-2.jpg) |
| `popups:win_zero_reward` | Pass | Pass | [EN](evidence/feedback/family-en-2.jpg) / [ZH](evidence/feedback/family-zh-2.jpg) |
| `popups:settings_sound_on` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `popups:settings_muted` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `popups:document_credits` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `popups:goal_info_berry` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `popups:goal_info_hoot` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `popups:win_milestone` | Pass | Pass | [EN](evidence/feedback/family-en-2.jpg) / [ZH](evidence/feedback/family-zh-2.jpg) |
| `popups:lucky_draw` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `popups:lucky_draw_spin` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `popups:lucky_draw_coins` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `popups:lucky_draw_none` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `popups:lucky_draw_full` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `popups:lucky_draw_reward_coins` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `popups:lucky_draw_reward_boosters` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `popups:lucky_draw_reward_single` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `popups:lucky_draw_reward_mixed` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `popups:chapters` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `popups:chest_empty` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `popups:chest_half` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `popups:chest_ready` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `popups:chest_multiple` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `popups:rewards_challenge` | Pass | Pass | [EN](evidence/feedback/family-en-1.jpg) / [ZH](evidence/feedback/family-zh-1.jpg) |
| `popups:rewards_challenge_empty` | Pass | Pass | [EN](evidence/feedback/family-en-1.jpg) / [ZH](evidence/feedback/family-zh-1.jpg) |
| `popups:rewards_challenge_all` | Pass | Pass | [EN](evidence/feedback/family-en-1.jpg) / [ZH](evidence/feedback/family-zh-1.jpg) |
| `popups:rewards_chest` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `popups:rewards_chest_distinct` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `popups:new_friend_berry` | Pass | Pass | [EN](evidence/feedback/family-en-2.jpg) / [ZH](evidence/feedback/family-zh-2.jpg) |
| `popups:unlock_owl_eye` | Pass | Pass | [EN](evidence/feedback/family-en-3.jpg) / [ZH](evidence/feedback/family-zh-3.jpg) |
| `popups:unlock_sapling` | Pass | Pass | [EN](evidence/feedback/family-en-3.jpg) / [ZH](evidence/feedback/family-zh-3.jpg) |
| `popups:world_complete_1` | Pass | Pass | [EN](evidence/feedback/family-en-3.jpg) / [ZH](evidence/feedback/family-zh-3.jpg); current EN is clean independent repeat, original diagnostic retained |
| `popups:world_unlock_2` | Pass | Pass | [EN](evidence/feedback/family-en-3.jpg) / [ZH](evidence/feedback/family-zh-3.jpg) |
| `popups:world_unlock_4` | Pass | Pass | [EN](evidence/feedback/family-en-3.jpg) / [ZH](evidence/feedback/family-zh-3.jpg) |
| `tutorial:tutorial_1` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `tutorial:tutorial_2` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `tutorial:tutorial_3` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `tutorial:tutorial_4` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `tutorial:lesson_sleep` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `tutorial:lesson_mama` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `tutorial:lesson_egg` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `tutorial:lesson_crow` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `tutorial:lesson_twig` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `tutorial:lesson_swivel` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `tutorial:lesson_growing` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `tutorial:lesson_daynight` | Pass (reused) | Pass (reused) | Exact checkpoint path/hash and verified unchanged scope in manifest |
| `popups:level_card_first_visit` | Pass | Pass | [EN](evidence/feedback/family-en-2.jpg) / [ZH](evidence/feedback/family-zh-2.jpg) |
| `popups:level_card_full_roster` | Pass | Pass | [EN](evidence/feedback/family-en-1.jpg) / [ZH](evidence/feedback/family-zh-1.jpg) |
| `popups:level_card_locked` | Pass | Pass | [EN](evidence/feedback/family-en-2.jpg) / [ZH](evidence/feedback/family-zh-2.jpg) |

## Evidence and completion boundary

- Current 60 fresh raw originals: build/text-centering/all-pages/feedback-screens/, except world_complete_1 EN in feedback-repeat/. The earlier warning original remains in feedback-screens/ as diagnostic only.
- Process logs/status: feedback-logs/, feedback-status.log and feedback-motion-status.log in that directory.
- Source snapshots: feedback-capture-source-start.json, feedback-capture-source-end.json and feedback-final-five-sources.json. Non-Map sources remained identical throughout both batches. The final source check and all 184-frame manifest are copied beside this report.
- Motion raw/times: feedback-motion/motion/; manifest: feedback-motion-manifest.json. Contacts: [win](evidence/feedback/motion-win_three_stars.jpg), [Berry](evidence/feedback/motion-new_friend_berry.jpg), [gate](evidence/feedback/motion-world_unlock_2.jpg).
- No visual/provenance gate remains in this assigned review. The integrator owns the separate final combined graphical/action regression. Screenshots do not replace callback/hit-region tests, user aesthetic acceptance, real-device checks or audio listening.

## Independently reviewed noncanonical supplements

The final-source-equivalent component frames cover four additional unlocks (Firefly, Wake Bell, Scarecrow and Spring Rain) and all six coin-package previews, each in EN/ZH. Eight unlock frames and twelve package frames were independently visually reviewed. The pouch pair overlaps the canonical preview, so there are eighteen additional frames for nine unique supplemental fixtures; they do not inflate the 184-frame canonical count. Titles, captions, reward sums 600–33000, and outside actions fit without new outline/clipping defects. Component source and relevant art hashes match final integration.

- Unlock source/hash/paths: build/text-centering/all-pages/feedback-unlock-supplement.json. [EN](evidence/feedback/supplement-unlock-en.jpg) / [ZH](evidence/feedback/supplement-unlock-zh.jpg).
- Coin-package source/hash/paths: build/text-centering/all-pages/feedback-purchase-supplement.json. [EN](evidence/feedback/preview-en.jpg) / [ZH](evidence/feedback/preview-zh.jpg).
