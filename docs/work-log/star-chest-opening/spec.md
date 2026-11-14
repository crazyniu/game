# Star chest opening

Owner request (2026-11-14): the chest had a different size before and after opening (the opening ended with a
"land" step that shrank it from 610 to 540 px wide and moved it down for the reward page). Redo the animation.

- The chest keeps one place and one size from the closed page through the reward page
  (`CHEST_RESULT_HERO := CHEST_HERO`, `CHEST_RESULT_SUPPORT := CHEST_SUPPORT`).
- Ready chest: jiggles every 1.6 s.
- Open (`game/chest_opening.gd`): progress, count and actions fade; four widening shakes around the feet; a squash;
  the lid pops open (back ease) while the box springs up, with a flash of light and a fountain of stars and sparkles
  from the mouth (`CHEST_MOUTH`); the lid settles; a short hold; the reward page replaces the page under the same chest.
- Reward page (`Popups._chest_prize_burst`): the prizes leave the mouth one by one, arc high above the open lid and drop
  onto a row in front of the chest (`CHEST_PRIZE_Y`, where the progress rail was), land with a squash and sparkles,
  and their amounts count up; confetti as before. OK stays where it was.
- Capture state `popups:chest_opening` presses Open after 1.5 s.
