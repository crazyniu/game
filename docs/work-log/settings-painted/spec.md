# Settings painted as one picture

The settings popup is painted whole by local Codex from `art/approved_ui/settings.png`: one panel picture per language
with frame, title sign, rows, icons, row words and the Language heading in it (`art/island/panel_settings_<lang>.png`,
1080 x 1920, ropes repeated to the top). Switches, the language switch and the Help / Restart / Home buttons are
painted in the same session and placed on it by `game/popups.gd` (`SETTINGS_*` constants). Installed with
`python3 tools/island/install_settings_art.py <job dir>`. No code-drawn surfaces or separately coloured words remain.
