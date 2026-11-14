class_name JungleDialog
extends RefCounted
## Opt-in level-entry and win-result shell. Other modal families keep their accepted artwork.

const HEAD := 236.0
const FOOT := 168.0
const GAP := 24.0
const WIDTH := 960.0
const ENTRY_TITLE_WIDTH := 560.0
const PANEL := "panel_jungle_dialog"

static func open(host: Control, title: String, ordinal: String, heights: Array, instant: bool,
		action_height: float, show_close: bool, birds_above := false, warm := true, compact_entry := false) -> Modal:
	var inner := GAP * maxf(0.0, heights.size() - 1)
	for height in heights:
		inner += height
	var head := 208.0 if compact_entry else HEAD
	var foot := 72.0 if compact_entry else FOOT
	var panel_h := head + inner + foot
	var above := 180.0 if birds_above else 0.0
	var total := above + panel_h + Modal.ACTION_DROP + action_height
	var top := maxf(32.0, (1920.0 - total) * 0.5) + above
	var rect := Rect2((1080.0 - WIDTH) * 0.5, top, WIDTH, panel_h)
	var dialog := Modal.open(host, "", [], false, 0.0, 0.0, instant, false, "", false)
	for name in ["Panel", "TitleGroup"]:
		var old := dialog.content.get_node(name)
		dialog.content.remove_child(old)
		old.queue_free()
	dialog.panel_rect = rect
	dialog.content.pivot_offset = Vector2(540, top + panel_h * 0.5)
	dialog._dim.color = Modal.POPUP_SHADE
	if warm:
		var panel := warm_frame(dialog.content, rect)
		panel.name = "Panel"
	else:
		var panel := NinePatchRect.new()
		panel.name = "Panel"
		panel.mouse_filter = Control.MOUSE_FILTER_IGNORE
		panel.texture = Art.tex(PANEL)
		# Scale by width, then extend only the quiet middle: the title and footer never squash.
		var k := WIDTH / panel.texture.get_width()
		panel.position = rect.position
		panel.size = rect.size / k
		panel.scale = Vector2.ONE * k
		panel.patch_margin_top = 330
		panel.patch_margin_bottom = 210
		dialog.content.add_child(panel)
		decorate(dialog.content, rect)
	var sign: Control
	if title.is_empty() and ordinal.is_empty():
		sign = Control.new()
		sign.name = "TitleGroup"
		sign.mouse_filter = Control.MOUSE_FILTER_IGNORE
		UI.place(sign, Rect2(220, top - 14, 640, 223))
		dialog.content.add_child(sign)
		ButtonKit.banner(sign, Rect2(0, 223 * ButtonKit.BANNER_TOP, 640, 223 * (1.0 - ButtonKit.BANNER_TOP)))
	elif ordinal.is_empty():
		sign = ButtonKit.sign(dialog.content, title, Rect2(220, top - 14, 640, 223))
	else:
		sign = FamilyDialog.sign_title(dialog.content, "popup.level.title", ordinal, Rect2(60, top + 126, WIDTH, panel_h), ENTRY_TITLE_WIDTH if compact_entry else FamilyDialog.SIGN_RECT.size.x)
	if not ordinal.is_empty():
		var word: TextureRect = sign.get_child(1)
		var digits: Control = sign.get_node("TitleNumber")
		var ink := Rect2(word.texture.get_image().get_used_rect())
		var scale_ink := word.size / word.texture.get_size()
		var visible := Rect2(word.position + ink.position * scale_ink, ink.size * scale_ink)
		visible = visible.merge(Rect2(digits.position, digits.size))
		var shift := sign.size.x * 0.5 - visible.get_center().x
		word.position.x += shift
		word.bounds.position.x += shift
		digits.position.x += shift
	dialog._title_layer = sign
	dialog._title_id = title
	if show_close:
		MenuNavigation.close(dialog.content, "Close", Vector2(rect.end.x - MenuNavigation.SIZE.x - MenuNavigation.DIALOG_RIGHT_PAD, top + (48.0 if compact_entry else 156.0)), dialog.close)
	var ys := []
	var y := top + head
	for height in heights:
		ys.append(y)
		y += height + GAP
	dialog.set_meta("ys", ys)
	dialog.set_meta("card_rect", rect)
	dialog.set_meta("actions_y", rect.end.y + Modal.ACTION_DROP)
	return dialog

static func decorate(parent: Control, rect: Rect2) -> void:
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
		sprig.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
		sprig.flip_h = item[2]
		sprig.mouse_filter = Control.MOUSE_FILTER_IGNORE
		parent.add_child(sprig)

static func secondary(parent: Control, name: String, rect: Rect2, word: String, action: Callable) -> Button:
	var button := UI.art_button(parent, name, "btn_dialog_wood", rect, action)
	# A short contact shadow and solid lower rim give the wooden key depth without glow.
	var shadow := UI.art(button, "btn_dialog_wood", Rect2(0, 14, rect.size.x, rect.size.y))
	shadow.name = "ContactShadow"
	shadow.modulate = Color(0.12, 0.07, 0.025, 0.40)
	button.move_child(shadow, 0)
	var edge := UI.art(button, "btn_dialog_wood", Rect2(0, 7, rect.size.x, rect.size.y))
	edge.name = "LowerRim"
	edge.modulate = Color(0.50, 0.27, 0.08, 1.0)
	button.move_child(edge, 1)
	var label := ButtonKit.word(button, word, Vector2(rect.size.x, rect.size.y * 0.78))
	label.position.y += rect.size.y * 0.11
	label.bounds.position.y += rect.size.y * 0.11
	label.name = "Word"
	var face: Control = button.get_node("Face")
	var face_y := face.position.y
	var label_y := label.position.y
	button.button_down.connect(func():
		face.position.y = face_y + 4.0
		label.position.y = label_y + 4.0)
	button.button_up.connect(func():
		face.position.y = face_y
		label.position.y = label_y)
	return button

## Warm wood rim is independent artwork. Only its clean border is sampled; the face is opaque.
static func warm_frame(parent: Control, rect: Rect2) -> Control:
	var frame := Control.new()
	frame.name = "WarmFrame"
	frame.mouse_filter = Control.MOUSE_FILTER_IGNORE
	frame.position = rect.position
	frame.size = rect.size
	parent.add_child(frame)
	var k := rect.size.x / 1222.0
	var backing := Panel.new()
	backing.name = "OpaqueFace"
	backing.mouse_filter = Control.MOUSE_FILTER_IGNORE
	backing.position = Vector2.ONE * 14.0 * rect.size.x / WIDTH
	backing.size = rect.size - backing.position * 2.0
	var style := StyleBoxFlat.new()
	style.bg_color = Color("fff0c2")
	style.set_corner_radius_all(roundi(70.0 * rect.size.x / WIDTH))
	style.corner_detail = 16
	backing.add_theme_stylebox_override("panel", style)
	frame.add_child(backing)
	var rim := NinePatchRect.new()
	rim.name = "Rim"
	rim.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var tex := AtlasTexture.new()
	tex.atlas = Art.tex("panel_warm_wood_rim")
	tex.region = Rect2(28, 28, 1222, 1174)
	rim.texture = tex
	rim.size = rect.size / k
	rim.scale = Vector2.ONE * k
	rim.patch_margin_left = 100
	rim.patch_margin_right = 100
	rim.patch_margin_top = 100
	rim.patch_margin_bottom = 100
	rim.draw_center = false
	frame.add_child(rim)
	var face := backing.duplicate()
	face.name = "CleanFace"
	face.position = Vector2.ONE * 26.0 * rect.size.x / WIDTH
	face.size = rect.size - face.position * 2.0
	frame.add_child(face)
	return frame
