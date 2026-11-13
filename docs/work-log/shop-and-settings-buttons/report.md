# Shop and settings buttons: report

Checks on 251: naming guard (0 violations), `ci/check_painted_words.py` (0 problems), smoke test (0 failures).
Captures: `shop_view:{boosters,outfits,houses}`, `popups:{settings_muted,settings_sound_on}`, English and `--lang=zh`.
Evidence: `evidence/design_vs_runtime.jpg` (design then runtime, per screen), `evidence/chinese.jpg`.

Known differences: the settings panel keeps the shared popup width (960) used by every popup; nest pictures are the
game's own house skins, not the design's paintings. Not done: core test, motion frame sequences, user test on the Mac.

Second round (calm palette, scrolling): naming guard 0 violations, painted-words check 0 problems, smoke 0 failures.
Scrolling was checked with a temporary probe pushing real input events through the viewport onto the Nests tab:
a 400 px drag scrolled 400 px, a wheel step 166 px more, and no purchase popup opened. The probe is not kept.
