# Popup reward layout repair

Match the measured content rectangles in `docs/design/layout/{daily_gift,new_friend,unlock_card,world_complete,world_unlock}.json` on the 1080 x 1920 canvas. Use retained sprites with alpha-padding compensation; preserve state names, game behavior, persistence, and existing bird animation.

Scope: only the assigned methods in PanelsView, PopupFakeData, and the five screen entries in GameLayout/layout_metrics. Shared frame, title, backdrop board and close controls remain coordinator integration dependencies. Pass the measured panel height and center through the existing Body API.

Evidence: silent state captures, one 1280-high mockup/capture JPEG per assigned state, a rendered-rect dump and comparison script, and the naming guard. No EditMode or PlayMode suites. Missing painted art is reported, never generated.
