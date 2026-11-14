class_name JungleDialog
extends RefCounted
## Opt-in level-entry and win-result shell. Other modal families keep their accepted artwork.

const HEAD := 236.0
const FOOT := 168.0
const GAP := 24.0
const WIDTH := 960.0
const PANEL := "panel_jungle_dialog"

static func open(host: Control, title: String, ordinal: String, heights: Array, instant: bool,
		action_height: float, show_close: bool, birds_above := false) -> Modal:
	var inner := GAP * maxf(0.0, heights.size() - 1)
	for height in heights:
		inner += height
	var panel_h := HEAD + inner + FOOT
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
	var sign: Control
	if ordinal.is_empty():
		sign = ButtonKit.sign(dialog.content, title, Rect2(220, top - 14, 640, 223))
	else:
		sign = FamilyDialog.sign_title(dialog.content, "popup.level.title", ordinal, Rect2(60, top + 126, WIDTH, panel_h))
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
		MenuNavigation.close(dialog.content, "Close", Vector2(rect.end.x - 132, top + 156), dialog.close)
	var ys := []
	var y := top + HEAD
	for height in heights:
		ys.append(y)
		y += height + GAP
	dialog.set_meta("ys", ys)
	dialog.set_meta("card_rect", rect)
	dialog.set_meta("actions_y", rect.end.y + Modal.ACTION_DROP)
	return dialog

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
