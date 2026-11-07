# Board bird spacing

The user first requested a small outward adjustment to perched birds, then reported that the birds still looked too small and the snow board left too much empty sky below the goal row.

- Increase board bird scale from 0.88 to 1.10, a 25 percent increase in width and height. Keep the same scale across forests.
- Spread occupied branch slots between perch heights 660 and 1640 on the 1080x1920 canvas. Preserve the authored row order and left/right staggering; use the actual last branch slot to set the vertical interval.
- Keep the 24-pixel outward seat offset. Use a 98-pixel first-seat distance at density 1 so the larger birds retain room at the screen edges and centre.
- Translate the existing painted branches with their perch lines. Keep their original artwork, the HUD and bottom tool positions.
- Apply shared perch coordinates to birds, movement destinations and branch hit areas. When an added branch changes the layout, existing branches and birds move to the new rows together.

This is a presentation adjustment only. Preserve authored levels, game rules and save data.

The user subsequently authorized testing. Run bounded project and native layout/interaction checks with a silent, unsaved test profile. Visual approval remains with the user.
