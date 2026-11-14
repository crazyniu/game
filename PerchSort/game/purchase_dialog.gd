class_name PurchaseDialog
extends RefCounted
## Purchase-only cartoon container: one compact cream card with the gold title sign over its top edge, the close
## button on its corner and the action pill inside the card. Shared modal and finalized page skins stay unchanged.

const CARD_W := 900.0
const HEAD := 120.0
const FOOT := 56.0
const GAP := 24.0
const ACTION_LIFT := 36.0
const SHOP_ACTION_GAP := 32.0
## The painted card reaches this far above the content's top so the title lettering sits inside the card, not
## across its edge; the whole card moves down by half of it to stay centred.
const TITLE_ROOM := 96.0

## `heights` are the content groups; `ys` metadata holds their tops. The caller puts its action pill at
## `panel_rect.end.y + Modal.ACTION_DROP` (inside the visible card); `action_height` reserves its room.
static func open(host: Control, title_key: String, heights: Array, instant: bool,
		action_height := 0.0, width := CARD_W) -> Modal:
	var inner := 0.0
	for h in heights:
		inner += h
	inner += GAP * maxf(0.0, heights.size() - 1)
	var below := (ACTION_LIFT + action_height) if action_height > 0.0 else 0.0
	var total := HEAD + inner + below + FOOT
	var card := Rect2(Vector2((1080.0 - width) * 0.5, roundf((1920.0 - total) * 0.5 + 60.0 + TITLE_ROOM * 0.5)), Vector2(width, total))
	var dialog := Modal.open(host, "", [], false, 0.0, 0.0, instant, false, "", false)
	dialog.panel_rect = card
	dialog.content.pivot_offset = card.get_center()
	dialog._dim.color = Modal.POPUP_SHADE
	var old := dialog.content.get_node("Panel")
	dialog.content.remove_child(old)
	old.queue_free()
	var surface := ButtonKit.panel(dialog.content, card.grow_side(SIDE_TOP, TITLE_ROOM))
	dialog.content.move_child(surface, 0)
	dialog._title_id = title_key
	var layer := dialog.content.get_node("TitleGroup")
	dialog.content.remove_child(layer)
	layer.queue_free()
	var sign := ButtonKit.sign(dialog.content, title_key, Rect2(220, card.position.y - 150, 640, 244))
	dialog._title_layer = sign
	MenuNavigation.close(dialog.content, "Close", Vector2(card.end.x - MenuNavigation.SIZE.x - MenuNavigation.DIALOG_RIGHT_PAD, card.position.y + 30 - TITLE_ROOM), dialog.close)
	var ys := []
	var y := card.position.y + HEAD
	for h in heights:
		ys.append(y)
		y += h + GAP
	dialog.set_meta("ys", ys)
	# The action row sits ACTION_DROP below panel_rect's end, so end the logical rect that far above it.
	var action_y := card.position.y + HEAD + inner + ACTION_LIFT
	dialog.panel_rect = Rect2(card.position, Vector2(card.size.x, action_y - Modal.ACTION_DROP - card.position.y))
	dialog.set_meta("card_rect", card)
	return dialog

## Approved shop-only shell: rigid painted header/footer and an exterior price action.
## Other PurchaseDialog callers retain their established presentation.
static func open_shop(host: Control, title_key: String, heights: Array, instant: bool,
		action_height: float) -> Modal:
	var width := 880.0 if heights.size() >= 3 else 840.0
	const HEADER := 220.0
	var footer := 88.0 if heights.size() >= 3 else 72.0
	var inner := GAP * maxf(0.0, heights.size() - 1)
	for height in heights:
		inner += height
	var panel_h := HEADER + inner + footer
	var top := (1920.0 - panel_h - SHOP_ACTION_GAP - action_height) * 0.5
	var rect := Rect2((1080.0 - width) * 0.5, top, width, panel_h)
	var dialog := Modal.open(host, "", [], false, 0.0, 0.0, instant, false, "", false)
	for name in ["Panel", "TitleGroup"]:
		var old := dialog.content.get_node(name)
		dialog.content.remove_child(old)
		old.queue_free()
	dialog.panel_rect = rect
	dialog.content.pivot_offset = Vector2(540, top + panel_h * 0.5)
	dialog._dim.color = Modal.POPUP_SHADE
	toy_frame(dialog.content, rect)
	dialog._title_layer = ButtonKit.sign(dialog.content, title_key, Rect2(270, top - 14, 540, 223))
	dialog._title_id = title_key
	MenuNavigation.close(dialog.content, "Close", Vector2(rect.end.x - MenuNavigation.SIZE.x - MenuNavigation.DIALOG_RIGHT_PAD, top + 48), dialog.close)
	var ys := []
	var y := top + HEADER
	for height in heights:
		ys.append(y)
		y += height + GAP
	dialog.set_meta("ys", ys)
	dialog.set_meta("card_rect", rect)
	dialog.set_meta("actions_y", rect.end.y + SHOP_ACTION_GAP)
	return dialog

static func _leaves(parent: Control, rect: Rect2) -> void:
	# Independent sprites stay proportional when the panel's quiet centre stretches.
	var leaf := AtlasTexture.new()
	leaf.atlas = Art.tex("decor_jungle_leaves")
	leaf.region = Rect2(275, 110, 985, 755)
	for item in [
		["LeavesUpper", Rect2(rect.position + Vector2(24, 80), Vector2(160, 123)), false],
		["LeavesLower", Rect2(rect.end - Vector2(146, 108), Vector2(106, 81)), true],
	]:
		var sprig := TextureRect.new()
		sprig.name = item[0]
		sprig.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
		sprig.texture = leaf
		sprig.position = item[1].position
		sprig.size = item[1].size
		sprig.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
		sprig.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
		sprig.flip_h = item[2]
		sprig.mouse_filter = Control.MOUSE_FILTER_IGNORE
		parent.add_child(sprig)

static func glow(parent: Control, picture: Rect2) -> void:
	var glow := UI.image(parent, "glow_soft", picture.grow(picture.size.x * 0.22))
	glow.name = "ProductGlow"
	glow.modulate = Color(0.55, 0.95, 0.30, 0.95)

## Independent coral/turquoise toy rim; the opaque face is never taken from a mockup.
static func toy_frame(parent: Control, rect: Rect2) -> Control:
	var frame := Control.new()
	frame.name = "Panel"
	frame.mouse_filter = Control.MOUSE_FILTER_IGNORE
	frame.position = rect.position
	frame.size = rect.size
	parent.add_child(frame)
	var k := rect.size.x / 1136.0
	var face := Panel.new()
	face.name = "OpaqueFace"
	face.mouse_filter = Control.MOUSE_FILTER_IGNORE
	face.position = Vector2.ONE * 24.0 * k
	face.size = rect.size - face.position * 2.0
	var style := StyleBoxFlat.new()
	style.bg_color = Color("fff0c2")
	style.set_corner_radius_all(roundi(90.0 * k))
	style.corner_detail = 16
	face.add_theme_stylebox_override("panel", style)
	frame.add_child(face)
	var rim := NinePatchRect.new()
	rim.name = "Skin"
	rim.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var texture := AtlasTexture.new()
	texture.atlas = Art.tex("panel_shop_toy_rim")
	texture.region = Rect2(60, 80, 1136, 1100)
	rim.texture = texture
	rim.size = rect.size / k
	rim.scale = Vector2.ONE * k
	rim.patch_margin_left = 150
	rim.patch_margin_right = 150
	rim.patch_margin_top = 150
	rim.patch_margin_bottom = 150
	rim.draw_center = false
	frame.add_child(rim)
	return frame
