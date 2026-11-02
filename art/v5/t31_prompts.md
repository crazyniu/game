# T31 top limb image generation

Tool: built-in `image_gen.imagegen`; `transparent_background: true`.

References (style/material only, originals remain unchanged):

- `PerchSort/Assets/Resources/V5/Art/bg_forest_day.png`
- `PerchSort/Assets/Resources/V5/Art/branch_left.png`
- `PerchSort/Assets/Resources/V5/Art/birdhouse.png`

## Generation 1

```text
Use case: stylized-concept. Asset type: original transparent RGBA production game sprite, wide 1080 x 320 canvas for a portrait forest game. The three supplied images are STYLE AND MATERIAL REFERENCES ONLY: image 1 is the existing forest background with leafy canopies in both upper corners, image 2 is the matching smooth warm honey-brown wooden branch, image 3 is the matching green-roof birdhouse with a natural rope loop. Create only a NEW overhead supporting tree limb with its rope hanger, no background and NO birdhouse. Match exactly the warm polished plush 3D-rendered cartoon finish, rounded leaf shapes, saturated yellow-green leaves, softly bevelled warm wood grain and upper-left light of these references. Composition: one thick continuous natural wooden tree limb crosses the whole width from the LEFT canvas edge to the RIGHT canvas edge, both ends visibly growing into leafy canopy clusters at the top corners, never floating and no detached logs or severed ends. The limb gently sags at the horizontal center: endpoints near y=70, central limb around y=130, thickness about 60 pixels. Both ends and their leafy clusters are cropped by the top and side canvas boundaries, matching the existing background's upper-corner canopies. A few overlapping leaves hang over the limb for depth; keep every leaf and every wood pixel above y=220. At EXACT horizontal center x=540, a short natural tan twisted rope wraps ONCE around the limb, then hangs a short distance down to a small rounded hook-shaped knot for the existing birdhouse loop to hang from. The LOWEST POINT of this central hook must be (540,235), with NO long dangling tails, no extra rope, and no ornament. The center hanger is clearly one connected structural part of the limb. The bottom 60 pixels from y=260 to319 must be fully transparent, with nothing there at all. Single sprite, natural slight asymmetry in branch grain and foliage but centered rope. Keep the full 1080:320 aspect ratio and compositional coordinates. Truly transparent alpha, no baked checkerboard, no white or black background, no shadow plane, no disconnected glow. No birds, text, lettering, logos, UI, house or other objects.
```
