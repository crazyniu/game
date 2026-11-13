class_name DialogCartoonSkin
extends RefCounted
## Explicit unfinished-dialog skin. Accepted page artwork and shared Modal stay unchanged.

const BACKGROUND_ART := "background_home_jungle"   # the home and shop clearing
const PANEL_ART := "panel_dialog_cartoon"
const PANEL_CONTENT := Rect2(18, 52, 1218, 1152)
const CORNERS := Vector4(220, 220, 220, 220)
const CORNER_SCALE := 0.55
static var _panel_texture: AtlasTexture

static func panel(parent: Node, bounds: Rect2) -> Control:
	if _panel_texture == null:
		_panel_texture = AtlasTexture.new()
		_panel_texture.atlas = Art.tex(PANEL_ART)
		_panel_texture.region = PANEL_CONTENT
	var holder := Control.new()
	holder.name = "Panel"
	holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(holder, bounds)
	parent.add_child(holder)
	var face := NinePatchRect.new()
	face.name = "Skin"
	face.mouse_filter = Control.MOUSE_FILTER_IGNORE
	face.texture = _panel_texture
	face.patch_margin_left = int(CORNERS.x)
	face.patch_margin_top = int(CORNERS.y)
	face.patch_margin_right = int(CORNERS.z)
	face.patch_margin_bottom = int(CORNERS.w)
	face.size = bounds.size / CORNER_SCALE
	face.scale = Vector2.ONE * CORNER_SCALE
	holder.add_child(face)
	return holder

## The shop's cream card (ButtonKit.panel: panel_card_cream, 120 px corners at 0.42), drawn here without ButtonKit so
## this skin stays loadable before the Sound autoload exists.
const CARD_ART := "panel_card_cream"
const CARD_MARGIN := 120
const CARD_SCALE := 0.42

static func cream_card(parent: Node, bounds: Rect2) -> Control:
	var holder := Control.new()
	holder.name = "Panel"
	holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(holder, bounds)
	parent.add_child(holder)
	var skin := NinePatchRect.new()
	skin.name = "Skin"
	skin.mouse_filter = Control.MOUSE_FILTER_IGNORE
	skin.texture = Art.tex(CARD_ART)
	for side in [SIDE_LEFT, SIDE_TOP, SIDE_RIGHT, SIDE_BOTTOM]:
		skin.set_patch_margin(side, CARD_MARGIN)
	skin.size = bounds.size / CARD_SCALE
	skin.scale = Vector2.ONE * CARD_SCALE
	holder.add_child(skin)
	return holder

## Replaces only the panel and background. Title, controls, geometry and callbacks belong to the caller.
static func apply(dialog: Modal) -> Control:
	var previous := dialog.content.get_node("Panel")
	dialog.content.remove_child(previous)
	previous.queue_free()
	var surface := cream_card(dialog.content, dialog.panel_rect)
	dialog.content.move_child(surface, 0)
	var previous_background := dialog.get_node_or_null("ForestBackground")
	if previous_background != null:
		dialog.remove_child(previous_background)
		previous_background.queue_free()
	var background := UI.image(dialog, BACKGROUND_ART, Rect2(0, 0, 1080, 1920))
	background.name = "ForestBackground"
	background.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_COVERED
	dialog.move_child(background, 0)
	dialog._dim.color = Color(0, 0, 0, 0)
	return surface
