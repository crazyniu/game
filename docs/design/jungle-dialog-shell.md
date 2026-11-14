# Level-entry and win-result jungle shell

## Approved scope

The user approved `art/approved_ui/level_entry_jungle.png` and `win_result_jungle.png` as the direction for these two dialogs, then requested more depth on the wooden Replay / Home buttons. This is not a global replacement of the shop or other dialogs. Runtime acceptance remains pending the user's review.

## Layout and reusable artwork

- `panel_jungle_dialog.png`: independently generated transparent-exterior panel with opaque pale lime interior, emerald rounded frame, quiet tone-on-tone edge leaves and integrated honey-wood header/footer. No text, character or rewards baked into it. Width 960 design pixels; rigid top 330 source pixels and bottom 210 source pixels, extended only through the quiet middle. Content rows have 24 px separation, 236 px top room, 168 px footer room. The extra footer keeps item quantities and bird tiles clear of the wooden accent.
- Title: reuse current painted locale words and title digits. Center the combined level word and ordinal by visible ink; preserve large/multi-digit ordinals. Existing close button is unchanged and stays clear of the title.
- Stars, bird keys, mechanics, tool inventory, reward counters and drops remain the existing live components. Additional rows enlarge the panel, not shrink the text or remove functions.
- Actions are outside the panel, 48 px below it. Level Play remains the existing 500x200 artwork. Win Next spans 716x162; Replay and Home are 346x130 boxes on the following row. The approved full-width primary row is retained rather than applying the older sibling-width limit.
- `btn_dialog_wood.png`: independent blank wooden face. Existing white painted words remain optically centered; face has a 7 px dark lower edge and a short 14 px contact shadow, not an emission/glow effect. Pressing sinks face and word together by 4 px. The green Next remains dominant.
- Win birds perch on the upper green edge. Existing flight/idle, star reveal, count-up, confetti and callbacks are unchanged.

## Acceptance states

Both languages: first visit, locked entry, full roster, two/four tools; zero reward, one/two/three stars and repeated booster drop. Check all action callbacks, duplicate-press guard, disabled Next/Play, centered lettering, content clearance, rigid frame ends, and no off-screen actions. Capture actual Godot frames and compare the approved references; no screenshot is a gameplay substitute. Audio and physical-device acceptance are outside this local visual pass.

### Leaf accents

Approved follow-up adds two independent proportional leaf sprites on the rim, using
`JungleDialog.decorate(parent, rect)`. The panel bitmap is unchanged. Upper accent is
160×123, lower accent 106×81; both ignore mouse input. Atlas region excludes transparent
padding but does not distort or repaint the sprite. Purchase dialogs may reuse the same
asset/placement without adopting this shell's content layout.
