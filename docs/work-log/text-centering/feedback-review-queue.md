# Affected-state visual review queue

Status: the authorized 56 non-Map captures and three isolated motion runs are complete. See [current review](feedback-review.md). All four Map captures and the independent warning-frame repeat are now complete. The planning inventory below is preserved; no production edit was made.

The existing 92-state, 184-frame review is preserved as a diagnostic checkpoint, not a pass for the pending tree and cartoon dialog materials. The checkpoint manifest is build/text-centering/all-pages/feedback-checkpoint.json. Its exact frame paths, SHA256s, dimensions, locale and earlier source epoch are preserved.

## Fresh captures

30 existing canonical states require both English and Chinese captures: 60 frames. No new preview state is added. Capture from final integration only, with the existing canonical renderer, separate process per state, isolated HOME/XDG paths and a native 1080x1920 viewport. New outputs target feedback-screens so the checkpoint frame bytes are not overwritten.

| Group | States | Frames | Reason |
| --- | --- | --- | --- |
| Map route | world_page, world_page_late | 4 | Connected tree/branches replace the winding route. |
| Family win | win_three_stars, win_two_stars, win_one_star, win_zero_reward, win_milestone | 10 | New opt-in panel/background across win compositions. |
| Family Level | level_card_first_visit, level_card_full_roster, level_card_locked | 6 | New material; gold 1/12/100 ordinal at matching visible title height; fully outside Close. |
| Family rewards | rewards_challenge, rewards_challenge_empty, rewards_challenge_all | 6 | New material at zero, sparse and full-grid content. Draw/Chest rewards are separate. |
| Family friend/tools | new_friend_berry, unlock_owl_eye, unlock_sapling | 6 | New material under names, ability captions and quantities. |
| Family worlds | world_complete_1, world_unlock_2, world_unlock_4 | 6 | New material and tall gate/bird compositions. |
| Shop confirmations | confirm_booster, confirm_multiple, confirm_maximum, insufficient, confirm_nest, confirm_nest_short, confirm_outfit, confirm_outfit_short | 16 | Framed purchase material and fully outside Close; preserve quantity/caret, amount ink and disabled sizing. |
| Aviary confirmations | buy_outfit, buy_outfit_short | 4 | Only popup material/Close changes; underlying Aviary detail source is frozen. |
| CoinShop simulation | purchase_preview | 2 | New skin confirmed for coins.preview; fully outside Close and preserved title scale. |

Exact run queue: build/text-centering/all-pages/feedback-capture-queue.tsv. Metadata and review criteria: feedback-capture-queue.json in the same directory. This is a queue, not an executed runner.

## Reuse provenance

The remaining 124 frames are candidate reuse, not automatically approved after merge. feedback-reuse-plan.json retains every exact source frame path/hash/size/locale and original capture epoch, plus the scope which must remain unchanged. feedback-source-checkpoint.json records the small current source snapshot. After final merge, compare these scopes and retained original art against the existing baseline. If a scope changed, add its frames to the fresh queue instead of claiming equivalence.

- Map owner confirms no new preview names and only world_page/world_page_late pixels change. cards/cards_late/cards_all/cards_locked retain selector functions/constants, assets and clip shader. Final frozen preservation manifest is still required.
- Family owner confirms DialogCartoonSkin.apply is opt-in through FamilyDialog.decorate only. Default modal, ShopSkin, unframed Aviary, Draw and Chest remain unchanged. Purchase owner confirms its PurchaseDialog.open opt-in covers every title_id, including coins.preview, after the existing title-scale and outside-Close handling.
- Sunny and all unframed Aviary states remain frozen at aviary_view.gd SHA f5d3ee9a35c2cb07a94ad219304a76694f0aa2c026b0469543ad106ae6fa01d6. Do not modify or redraw them. Their reuse depends on unchanged source/assets and no shared-style mutation.
- Home, Settings, instructional/goal overlays, Draw, Chest, documents, chapter list, open CoinShop packages and unframed Shop pages keep their current source-scoped evidence. No gameplay frame is introduced into this review.

## Acceptance checks after capture

Inspect actual full renders and labeled contact sheets. Separately judge (1) plaque/group relative to page or parent control, (2) visible alpha ink within its face, and (3) complete word+ordinal or icon+amount union. A label-box-only metric is insufficient.

Check material coherence and contrast, transparent cutout edges, nine-patch seams, no unexpected recoloring or duplicate outlines, title/ordinal scale and gold style, outside Close/actions, no clipping at tall/short dialogs, disabled size retention, route/tree continuity and bottom root margins. Compare frozen Sunny and protected surfaces only as preservation references. Use the owners' focused runtime/callback/hit-region tests separately; screenshots do not prove animation, timing, physical-device behavior or listening/audio.

Supplemental direct fixtures can reuse owner captures only after final-source equivalence is verified: four additional tool unlocks in both languages, all six CoinShop package previews (stack, cluster, pouch, basket, chest, hoard) in both languages; pouch is already canonical, so only five package pairs are additional, and Map early/last-world boundary states when available. These do not inflate the 184 canonical-frame count. New artwork asset review does not replace runtime visual review.

Current gate: complete. See the current review and manifest for the 60 final integrated frames, 124 verified reuse frames and retained diagnostics.
