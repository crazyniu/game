# Aviary text and header centering

## Scope

The user annotated the current Aviary detail screenshots: center the ability text, reduce excessive text edging on ownership and price labels, center the name sign on the page, and vertically align the header counter. Equivalent non-gameplay label roles are audited. Home, Settings, gameplay, original raster assets, shared helpers, economy, callbacks and persistence remain unchanged.

## Local presentation contract

- The visible name-sign center is x540, y119 on the 1080x1920 canvas. Back and balance artwork share y119. Move the balance right 24px to leave clearance after centering the unchanged 464px name sign.
- The ability label's visible ink is centered at x540,y766 within the existing cream plaque.
- Keep accepted green and gold action skins and all original word/number textures. Aviary-only shading removes their existing outer dark surround; do not stack a new outline or replace source pixels.
- Center the resulting visible ownership ink on the fixed 240x68 action face. Center the coin and price together, using the actual first/last glyph ink rather than equal-cell margins.
- Source and material bounds must be tested in both languages, all 10 species, and representative prices of every length through 99999. Crow keeps its no-outfits state.
- Actual OpenGL screenshots and functional regression accompany the source checks. They do not imply user visual acceptance, physical-device testing or audio listening.
