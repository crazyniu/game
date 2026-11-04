# T54F Popup and birdhouse birds alive, win-panel proportions (new user feedback 2026-11-04, after testing)
Read tasks/T54_contract.md first. Owned: `PerchSort/Assets/Scripts/Game/WinPanel.cs`, `PopupMotion.cs`, `PopupFlow.cs`, `BirdhouseView.cs`, NEW `PerchSort/Assets/Scripts/Game/UiBirdLife.cs`,
new `Tests/**/T54F*.cs`. Everything else read-only (BirdView/BirdIdleGroup belong to T54C; do not edit them).

User problems (screenshot of the win panel):
- F1 The two side birds (blue left, pink right) and the hero bird on the ribbon are fixed pictures. Nothing moves. They must be alive.
- F2 The green Next button is too big next to the two purple buttons; the trio does not match. Make Next about 1.25x the purple discs (e.g. purple 124 px, Next 156 px), same gold
  rim thickness ratio, optical centre aligned, equal gaps. Same proportion logic for every popup that shows a primary green button with purple siblings.
- F3 Birds inside the birdhouses (goal houses at the top, in every level) are static. They must move: gentle breathing, peeking up and down in the doorway, occasional blink and
  look-left/right by pose frames, tiny hop when a matching bird is delivered. Never mirror-flip a sprite to fake a turn.
- F4 Popup entrance/idle choreography must read as animation, not stills: panel scales in with overshoot, ribbon unrolls/drops, stars pop one after another (0.18 s apart) with
  a small spin and sparkle, coin/chain counters count up, rays rotate slowly, confetti falls continuously for about 3 s, side birds fly in from outside and land on the panel edge
  with a squash, then keep living (below).

How birds must move (UiBirdLife, reusable for win-panel birds and house birds):
- Continuous: breathing scale (1.5-2.5 %, period 2-3 s, random phase) and a soft vertical bob; wing-flutter/hop bursts on celebration (win side birds hop 14-22 px every 1.2-2.4 s,
  alternate sides, never in sync, plus 6-10 degree tilt that eases in and out around the FEET pivot, not the sprite centre).
- Use the species pose frames in `Resources/Birds` (blink, look_side, preen, fluff, stretch, quirk, happy/cheer frames if present) via cross-fade or quick scale-squash, not hard pops.
- Mirroring a sprite (scale.x = -1) is forbidden as an animation. Facing change = a 0.18 s squash of scale.x to 0.15, swap to the pose frame facing the other way (only if such a frame exists), expand back; if no such frame exists, do not turn at all.
- Everything honours `GameMotion.Instant` (no motion) and cancels cleanly on close; no coroutines leaking after the panel closes.

Evidence (contract rule 3): a contact sheet of the win panel every 0.1 s for the first 4 s and then every 0.5 s for 6 s; a contact sheet of the top houses every 0.25 s for 15 s on
level 1; a crop of the three bottom buttons of the win panel showing the new proportions. Tests: PlayMode assertions that side birds' rect position or scale actually changes over 2 s of
real time on the win panel, and that a house bird's anchored position/scale changes over 4 s; EditMode or PlayMode assertion for the Next/purple size ratio.
