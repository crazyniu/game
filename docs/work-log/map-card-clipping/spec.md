# World card rounded clipping

The user-marked screenshot shows world 5's selected carousel card. The painting currently
has a rectangular clip and covers the frame's rounded inner corners. This is the selector,
not the playable trail or level modal.

Retain every accepted world image, green/gold frame and existing carousel measurement.
Only mask the painting to its existing 578x852 window using a 30px rounded rectangle with
screen-derivative antialiasing. Its crop and aspect ratio stay unchanged. The filled frame
continues behind the painting; it must not be moved above and obscure the content.
Selection, locking, progress, badges, plaque, single Play and navigation stay unchanged.

Run all ten worlds at 1080x1920, 390x844, 360x640 and 768x1024 render targets using the
production canvas-items/keep scaling, plus selected/locked and live swipe states. Compare
actual corner pixels and enlarged runtime corners in both green and gold frames. Check
all source art remains byte-identical; run existing navigation/map/core tests. Upload only
actual runtime evidence requested by the user, never code or a replacement approved baseline.
