# Outside dialog actions and win composition

The user supplied a marked actual win screenshot. The upper three-bird/title composition and lower icon-only Replay/Home pair are identified. Every action, including the primary Next, must sit outside the container. Map marks in the second image belong to the map worker.

Use the current integrated snapshot, not repository HEAD alone. Scope is the seven unapproved FamilyDialog consumers. Keep Shop, Home, Chest, gameplay, reward rules, quantities, save fields and shared navigation unchanged. A later explicit clarification permits moving the existing Coin Shop Purchase Preview Confirm and its hit area outside the frame, without changing its dimensions, material, container or close-only behavior. Other purchase action positions have a separate owner.

Material gate before UI work:
- The user later rejected the dark, textured Family container and explicitly protected the Sunny Aviary detail page. Reuse background_aviary_forest pixels unchanged and opt in to the independent, self-reviewed panel_dialog_cartoon through DialogCartoonSkin. Its thick orange-gold/cream edge and clean saturated grass-green field replace only unfinished dialog surfaces. Preserve the old Shop assets and every protected page.
- Retain home_play_plate and the established Play/OK/Next sprites.
- Reuse white_settings_home in each locale for the Home action. Its real alpha bounds and soft-white treatment must match the Next family.
- A matching Replay / 重玩 white word is absent. Generate only these independent transparent word images in the accepted rounded soft-white family, modest dark outline and no halo. Inspect spelling/strokes, alpha, edge and weight before implementation; never overwrite accepted source words.

Geometry:
- Maintain primary 520x176 and secondary 416x140.8. All action rows move wholly below the visible container with 48 px clearance. Keep content positions and reduce the panel to its content plus 64 px lower padding, preserving available total extent.
- Keep the accepted Close image and exact role size, wholly above the panel with 48 px clearance and its right edge aligned to the panel. Do not let it straddle the border.
- Win birds use existing animated assets, symmetric centers and the same foot baseline at least 36 px above the title ink. Reduce the excessive size difference; no overlap at idle/breathing/cheer envelopes. No sprite mirroring.
- Center title and action words by visible ink, not PNG canvas. Level ordinals use the retained localized gold glyphs, the same visible height as the title, a 24 px visible gap, and one centered group.

Verification: exact action/container separation in all states and both locales, visible word semantics, disabled size, callback lockout, live language changes, profile preservation, scene bounds; aggregate suite; actual bilingual full-resolution screenshots and timed motion; actual pointer flow. User visual acceptance remains pending.

Purchase Preview: keep its current panel and content geometry. Place its existing 336×112 Confirm instance at x=372, y=panel.end.y+Modal.ACTION_DROP (48 px), leaving art, word, callback and preview-only semantics intact. Capture all six packs in both languages and verify real pointer dismissal.
