# Task feel-layout

1. Entrance: the full 3.5-4.5 s entrance only the first time a level is opened in this run of the game; replays and
   "next level" right after a win use a short entrance of about 1.5 s (houses, branches and birds the same, faster).
   Tap still speeds it up. Log both with --autoplay.
2. Flock home: when a full branch flies home make it the high point: the house bounces and glows, a ring of small
   painted sparkles (`pic_sparkle`) bursts from the door, and each consecutive homing in one move chain plays the
   home sound a little higher (pitch steps). Keep it under 1 s and never block input longer than now.
3. Landing: the receiving branch dips a few pixels and springs back when birds land (ease, 0.25 s); a refused tap
   keeps its wobble.
4. Haptics: a short vibration on pick-up, landing and flock home (`Input.vibrate_handheld`, 15-40 ms, mobile only),
   behind a profile setting. Add the save field `vibration` (bool, default true) in `meta/profile.gd` (this one new field
   is allowed) and a Vibration switch row in the settings popup using the existing toggle helper.
5. Board tool row: tools slightly larger (secondary 128 -> 140 is not allowed; keep 128) but grouped on a painted dark
   HUD bar (`UI.shape(..., "hud", ...)`) behind them like the top bar, centred at the bottom with even spacing, so the
   bottom balances the top.
6. Map: at either end of the scroll no node may hide under the bottom buttons (clamp or extra room). When the view
   enters a new world, show that world's name on a title ribbon at the top with its star progress "N/30" (painted
   star + numbers) for 2 s, fading in and out.
Evidence: autoplay logs (entrance first and replay), frame sequences of a flock home and a landing, captures of the
board bottom, the map ends and the world ribbon.
