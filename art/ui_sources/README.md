# UI source assets
`generate_ui.py` draws original rounded 9-slice panels and icons with Pillow at 4× resolution, then downsamples. Run from any directory; output is `PerchSort/Assets/Art/UI`. Existing bird/environment art is only referenced in Unity and never changed.

Fredoka Medium was instantiated at wght=500, wdth=100 from Google Fonts' `ofl/fredoka/Fredoka[wdth,wght].ttf` with fontTools.varLib.instancer; the resulting static TTF and OFL are in Assets/Fonts. Runtime and scene rebuild use only checked-in assets.
