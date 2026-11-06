# Popup frame correction

Use the approved 1080 x 1920 layouts in `docs/design/layout` for Win, Settings, Goal info and Level card. Their measured 860 px frames override the approximate 780 px description in the brief. `Body(Rect panel, string title, Rect titleRect, bool close, Rect closeRect)` accepts absolute top-left design coordinates; content uses `GameUI.Local` to convert them to the panel's anchor space. The height-based overload remains for the other popup owner.

Use existing art and painted lettering only. Compensate for transparent margins with runtime sprite crops and retained alpha measurements. Keep live bird frame animation, disabled button sizing, authored level data, save data and state names unchanged. Treat the daily-gift Edges cases as shared-frame work; the gift composition belongs to the other popup worker.

Verification is limited to the requested silent `state_captures.sh` filters, inspection of every owned state's mockup/capture JPEG, actual RectTransform corner dumps, and the naming guard. Do not run EditMode or PlayMode suites. Background/HUD changes and missing/replacement artwork must be reported for their owners.
