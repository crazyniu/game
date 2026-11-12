# Outside dialog actions and balanced win header

## Scope and result

The user's marked win screenshot identified the top three birds/title and the two icon-only secondary controls. The seven unapproved FamilyDialog types now end their container below the content, with every action row wholly outside and a 48-pixel clear gap. Next, Play and OK keep their existing accepted words. The win secondary actions now read Replay/Home and 重玩/主页. The existing birds have symmetric centers at 300/540/780 and a common visible footline, 48 pixels above the title box. Their original continuous animation, pose assets and callbacks remain intact.

The frame and inner background remain the exact Shop/PurchaseDialog panel_shop_forest component with unchanged source pixels, corner patches and corner scale. The exterior remains the bright sunlit background_shop_forest. Reducing the frame to the content removes the large dark action area. No accepted container or background image is redrawn or substituted. The material identity is verified; user approval of this new composition is still pending.

The coordinator subsequently clarified that the existing Purchase Preview OK must also be outside. This is a separate one-line y-position change to its existing Confirm instance and hit area, at panel.end.y + 48. Its dimensions, pictures, word, container and close-only behavior are unchanged. It remains a simulation without a payment or currency grant.

## Material gate

Only two missing standalone words were generated: label_dialog_replay_en.png and label_dialog_replay_zh.png. Existing Home and Next words are reused byte for byte. The built-in image generation tool used the existing localized Home words as style references, not edit targets. The generated words were checked letter by letter and for transparent backgrounds before code integration. The manifest records exact file hashes, dimensions and visible-ink regions. Low-alpha generated noise outside the glyph is excluded by a documented AtlasTexture region; source pixels remain unchanged. A local existing soft-white shader gives the same treatment as Next/Home, without adding an outline or glow.

English prompt: Create one independent game UI word sprite, exact English text “Replay” (capital R followed by e p l a y), no other text. Reference Home is style guidance only. Match its plump rounded hand-painted casual puzzle-game lettering, ivory soft-white face, subtle warm cream bevel/highlight, thin restrained deep forest-green edge. Friendly compact front-facing horizontal word on a genuinely transparent canvas with clear margins. No plaque, button, icons, floor shadow, halo, outer glow, sparkling effects, extra border or backdrop. This is the missing companion word, not a redesign of the accepted reference.

Chinese prompt: Create one independent game UI word sprite, exact simplified Chinese text “重玩”, precisely those two characters. Reference 主页 is style guidance only. Match its plump rounded hand-painted lettering, ivory soft-white face, subtle warm cream bevel/highlight, thin restrained deep forest-green edge. Correct clear strokes, front-facing horizontal word, genuinely transparent canvas with clear margins. No plaque, button, icon, floor shadow, halo, outer glow, sparkling effect or background. This is a missing companion word, not a redesign of the accepted reference.

## Preservation

- Primary 520×176 and secondary 416×140.8 remain identical across enabled/disabled states and within the 1.25 sibling ratio.
- Modal's original content positions and total layout allocation are retained. Only this opt-in family shortens the panel and relocates its action metadata. Shared Modal, UI, UiText, typography, navigation, Chest and DrawResult are not modified.
- All save/economy/rule files and all 5,894 existing tracked assets/rule files are unchanged. Two new words are the only new production assets.
- FamilyDialog owns its new word's locale refresh and visible-ink centering. No font-rendered player text is introduced.
- Production function changes are bounded to FamilyDialog decoration/actions plus a new local word class, popups.win, and coin_shop_view.show_preview_purchase. Independent purchase/aviary work remains with its assigned owner.

## Verification

Final results and exact commands are in evidence/test-results.txt. All captures come from canonical scenes with fake profiles and the real GL Compatibility renderer; no screenshot is a mockup or edited game render.

- Family checks include all seven types, 1–3 stars, zero/multiple/all rewards, level 100/full roster, locked/enabled Play, all six boosters, three world indices, both languages, live locale changes, callback lockout, interrupted exit, actual front-frame footline and all loaded pose-frame title clearances.
- Purchase checks cover six preview packs, exact amounts, single open, outside Confirm geometry, unchanged size, viewport fit and close without charge/grant. Existing booster/outfit transaction checks also remain in the suite.
- Forty-two bilingual family stills and four reference pages, plus 48 timestamped motion frames, are saved under build/dialog-layout. Six purchase packs in both languages are captured separately after the position-only change.
- Real pointer input checked disabled Play, Close, Home, Replay, Next and world Play. Callback totals were exactly 0/100/101/111/1111, and fake balance remained 0. A separate pointer check covers Purchase Preview OK.
- Tests initially exposed a test-only early autoload dependency from directly naming UI in a SceneTree test. Deferred loading, consistent with the existing suite, fixed it. No production workaround was introduced.

## Independent review

The integration reviewer inspected both 21-state contact sheets, both full-resolution win screenshots and the container comparison. It found no new clipping/overlap blocker, confirmed outside actions and balanced birds, and judged the retained panel part of the Shop material family. The typography reviewer independently checked visible-ink centering and outline layers; no local fix was requested. The new Chinese word has a finer dark contour than retained Home/Next, which is documented rather than modifying accepted labels.

## Limits

This is local implementation and engineering/visual self-review, not the user's visual acceptance. No remote code upload, push, PR, merge or deploy occurred. No approved-runtime baseline was replaced. Raw images are local pending coordinator delivery, so this worker creates no new Library identities and does not replace existing screenshots.

No physical mobile touch, audio-listening, accessibility or frame-rate budget acceptance is claimed. Motion frames demonstrate continuous live changes and include timestamps; synchronous PNG saving makes their intervals longer than the requested 0.25-second wait, so they do not establish device timing compliance.
