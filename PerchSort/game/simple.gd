class_name Simple
extends RefCounted
## The simple UI set: a few shapes in a few colours (cream card, warm-brown rim, one green, one cream), words from the
## baked labels (tools/island/bake_labels.py) at one cap height, and controls that keep one size in every state.
## Shapes are drawn, not stretched pictures, so nothing is ever distorted.

const CREAM := Color8(255, 247, 232)
const CREAM_DEEP := Color8(241, 226, 200)   # recessed tracks
const RIM := Color8(214, 168, 112)          # card and cream-button rim
const RIM_DARK := Color8(186, 136, 82)      # lip under cream shapes
const LINE := Color8(234, 214, 182)         # row dividers
const SOFT_INK := Color8(150, 104, 62)      # captions (tools/island/bake_labels.py SOFT)
const GREEN := Color8(92, 184, 58)
const GREEN_LIP := Color8(60, 136, 34)
const OFF := Color8(214, 202, 184)
const OFF_LIP := Color8(184, 168, 146)
const KNOB := Color8(255, 253, 247)
const SHADOW := Color(0.25, 0.14, 0.05, 0.22)
const LIP := 8.0                            # the darker bottom edge that gives buttons and tracks their depth
const SWITCH := Vector2(156, 84)
const CHOICE := Vector2(340, 84)
const CHOICE_CAP := 40.0
const CLOSE := 92.0
const PRESS_DROP := 4.0
const LABELS := "res://art/labels/"


static func _box(fill: Color, radius: float, lip_color := Color.TRANSPARENT, lip := 0.0, border := 0.0,
		border_color := Color.TRANSPARENT, shadow := 0.0) -> StyleBoxFlat:
	var b := StyleBoxFlat.new()
	b.bg_color = fill
	b.set_corner_radius_all(int(radius))
	b.anti_aliasing = true
	if border > 0.0:
		b.set_border_width_all(int(border))
		b.border_color = border_color
	if lip > 0.0:
		b.border_width_bottom = int(lip + border)
		b.border_color = lip_color if border == 0.0 else border_color
	if shadow > 0.0:
		b.shadow_color = SHADOW
		b.shadow_size = int(shadow)
		b.shadow_offset = Vector2(0, shadow * 0.4)
	return b

static func _panel(parent: Node, r: Rect2, style: StyleBox, n := "Shape") -> Panel:
	var p := Panel.new()
	p.name = n
	p.mouse_filter = Control.MOUSE_FILTER_IGNORE
	p.add_theme_stylebox_override("panel", style)
	UI.place(p, r)
	parent.add_child(p)
	return p

## The cream card every simple popup sits on: warm rim, soft shadow.
static func card(parent: Node, r: Rect2) -> Panel:
	return _panel(parent, r, _box(CREAM, 48.0, Color.TRANSPARENT, 0.0, 8.0, RIM, 28.0), "Card")

static func divider(parent: Node, r: Rect2) -> Panel:
	return _panel(parent, r, _box(LINE, 1.5), "Divider")


## Baked words: placed by their optical middle at a cap height, re-textured in place when the language changes.
class Words extends RefCounted:
	var _items := []   # [TextureRect, id, style, anchor, cap, align]

	func place(parent: Node, id: String, style: String, anchor: Vector2, cap: float, align: int) -> TextureRect:
		var t := TextureRect.new()
		t.name = "Word_" + id
		t.mouse_filter = Control.MOUSE_FILTER_IGNORE
		t.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
		t.stretch_mode = TextureRect.STRETCH_SCALE
		parent.add_child(t)
		_items.append([t, id, style, anchor, cap, align])
		_fit(_items[-1])
		return t

	## Changes a placed word's id or style (the language choice moves its green half).
	func restyle(t: TextureRect, style: String) -> void:
		for item in _items:
			if item[0] == t:
				item[2] = style
				_fit(item)

	func refresh() -> void:
		for item in _items:
			if is_instance_valid(item[0]):
				_fit(item)

	func _fit(item: Array) -> void:
		var t: TextureRect = item[0]
		var name := "label_%s_%s_%s" % [item[1], item[2], UiText.locale()]
		var tex: Texture2D = load(Simple.LABELS + name + ".png")
		var meta := Simple._meta()
		var k: float = float(item[4]) / float(meta["cap"])
		var size := tex.get_size() * k   # uniform scale: never distorted
		var anchor: Vector2 = item[3]
		var x: float = anchor.x - size.x * (0.5 + 0.5 * float(item[5]))
		t.texture = tex
		t.size = size
		t.position = Vector2(x, anchor.y - float(meta[name]) * k)

	func width(id: String, style: String, cap: float) -> float:
		var tex: Texture2D = load(Simple.LABELS + "label_%s_%s_%s.png" % [id, style, UiText.locale()])
		return tex.get_width() * cap / float(Simple._meta()["cap"])


static var _label_meta: Dictionary = {}

static func _meta() -> Dictionary:
	if _label_meta.is_empty():
		_label_meta = JSON.parse_string(FileAccess.get_file_as_string(LABELS + "labels.json"))
	return _label_meta


## On/off switch: a track that turns green and a knob that slides; one size in both states.
static func switch(parent: Node, right_middle: Vector2, value: bool, on_change: Callable) -> Button:
	var r := Rect2(right_middle - Vector2(SWITCH.x, SWITCH.y * 0.5), SWITCH)
	var b := _bare_button(parent, "Switch", r)
	var track := _panel(b, Rect2(Vector2.ZERO, SWITCH), StyleBoxFlat.new(), "Track")
	var knob_d := SWITCH.y - LIP - 12.0
	var knob := _panel(b, Rect2(0, 6.0, knob_d, knob_d), _box(KNOB, knob_d * 0.5, Color.TRANSPARENT, 0.0, 0.0,
		Color.TRANSPARENT, 6.0), "Knob")
	var state := {"on": value}
	var paint := func(animate: bool):
		track.add_theme_stylebox_override("panel", _box(GREEN if state["on"] else OFF, SWITCH.y * 0.5,
			GREEN_LIP if state["on"] else OFF_LIP, LIP))
		var x: float = SWITCH.x - knob_d - 6.0 if state["on"] else 6.0
		if animate:
			knob.create_tween().tween_property(knob, "position:x", x, 0.14).set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)
		else:
			knob.position.x = x
	paint.call(false)
	b.pressed.connect(func():
		state["on"] = not state["on"]
		Sound.play("ui_tap")
		paint.call(true)
		on_change.call(state["on"]))
	return b

## Two-way language choice: a recessed cream track, the current language on a green half. Each language is named in
## its own words. `on_pick` receives the language code.
static func language_choice(parent: Node, words: Words, right_middle: Vector2, on_pick: Callable) -> Control:
	var r := Rect2(right_middle - Vector2(CHOICE.x, CHOICE.y * 0.5), CHOICE)
	var holder := Control.new()
	holder.name = "Language"
	holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(holder, r)
	parent.add_child(holder)
	_panel(holder, Rect2(Vector2.ZERO, CHOICE), _box(CREAM_DEEP, CHOICE.y * 0.5, RIM, 0.0, 3.0, RIM), "Track")
	var half := CHOICE.x * 0.5
	var pill := _panel(holder, Rect2(4, 4, half - 4, CHOICE.y - 8), _box(GREEN, (CHOICE.y - 8) * 0.5, GREEN_LIP, 6.0), "Chosen")
	var codes := UiText.languages()
	var labels := []
	for i in codes.size():
		var anchor := r.position + Vector2(half * (i + 0.5), CHOICE.y * 0.5 - 2.0)
		labels.append(words.place(parent, "lang_" + codes[i], "ink", anchor, CHOICE_CAP, 0))
	var show := func(animate: bool):
		var at := codes.find(UiText.locale())
		var x := 4.0 if at == 0 else half
		if animate:
			pill.create_tween().tween_property(pill, "position:x", x, 0.16).set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)
		else:
			pill.position.x = x
		for i in labels.size():
			words.restyle(labels[i], "white" if i == at else "ink")
	show.call(false)
	for i in codes.size():
		var code: String = codes[i]
		var hit := _bare_button(parent, "Language_" + code, Rect2(r.position + Vector2(half * i, 0), Vector2(half, CHOICE.y)))
		hit.pressed.connect(func():
			if code == UiText.locale():
				return
			Sound.play("ui_tap")
			on_pick.call(code)
			show.call(true))
	return holder

## A worded button: green (main) or cream, one size per role, the word on the face's optical middle; pressing drops
## the face onto its lip.
static func button(parent: Node, words: Words, id: String, kind: String, r: Rect2, cap: float, on_press: Callable) -> Button:
	var b := _bare_button(parent, id.capitalize(), r)
	var green := kind == "green"
	var face := Control.new()
	face.name = "Face"
	face.mouse_filter = Control.MOUSE_FILTER_IGNORE
	face.size = r.size
	b.add_child(face)
	var radius := r.size.y * 0.5
	var style := _box(GREEN, radius, GREEN_LIP, LIP) if green else _box(CREAM, radius, RIM_DARK, LIP, 3.0, RIM)
	_panel(face, Rect2(Vector2.ZERO, r.size), style)
	words.place(face, id, "white" if green else "ink", Vector2(r.size.x * 0.5, (r.size.y - LIP) * 0.5), cap, 0)
	b.button_down.connect(func(): face.position.y = PRESS_DROP)
	b.button_up.connect(func(): face.position.y = 0.0)
	b.pressed.connect(func():
		Sound.play("ui_tap")
		on_press.call())
	return b

## Round cream close button with a drawn cross, centred on `center`.
static func close_button(parent: Node, center: Vector2, on_press: Callable) -> Button:
	var r := Rect2(center - Vector2.ONE * CLOSE * 0.5, Vector2.ONE * CLOSE)
	var b := _bare_button(parent, "Close", r)
	var face := Control.new()
	face.name = "Face"
	face.mouse_filter = Control.MOUSE_FILTER_IGNORE
	face.size = r.size
	b.add_child(face)
	_panel(face, Rect2(Vector2.ZERO, r.size), _box(CREAM, CLOSE * 0.5, RIM_DARK, LIP * 0.75, 3.0, RIM, 10.0))
	for angle in [PI * 0.25, -PI * 0.25]:
		var bar := _panel(face, Rect2(Vector2(CLOSE * 0.5 - 26.0, (CLOSE - LIP * 0.75) * 0.5 - 5.0), Vector2(52, 10)),
			_box(Color8(150, 92, 44), 5.0), "Cross")
		bar.pivot_offset = bar.size * 0.5
		bar.rotation = angle
	b.button_down.connect(func(): face.position.y = PRESS_DROP * 0.75)
	b.button_up.connect(func(): face.position.y = 0.0)
	b.pressed.connect(on_press)
	return b

static func _bare_button(parent: Node, n: String, r: Rect2) -> Button:
	var b := Button.new()
	b.name = n
	b.focus_mode = Control.FOCUS_NONE
	var empty := StyleBoxEmpty.new()
	for state in ["normal", "hover", "pressed", "disabled", "focus", "hover_pressed"]:
		b.add_theme_stylebox_override(state, empty)
	UI.place(b, r)
	parent.add_child(b)
	return b
