class_name UI
extends RefCounted
## Builders for the plush UI kit. All rects are design pixels [x, y, w, h], origin top-left of the parent.

const GLYPH_NAMES := {"+": "plus", "-": "minus", "x": "times", "/": "slash", "∞": "infinity"}
const PRESS_SCALE := 0.92


static func rect(r: Array) -> Rect2:
	return Rect2(r[0], r[1], r[2], r[3])

static func place(node: Control, r: Rect2) -> Control:
	node.position = r.position
	node.size = r.size
	node.pivot_offset = r.size * 0.5
	return node

## Plain picture stretched to the rect, keeping aspect centered.
static func image(parent: Node, name: String, r: Rect2, keep_aspect := true) -> TextureRect:
	var t := TextureRect.new()
	t.texture = Art.tex(name)
	t.name = name
	t.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	t.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED if keep_aspect else TextureRect.STRETCH_SCALE
	t.mouse_filter = Control.MOUSE_FILTER_IGNORE
	place(t, r)
	parent.add_child(t)
	return t

static var _used: Dictionary = {}

## Visible (alpha) bounds of a texture in its own pixels, cached.
static func used_rect(name: String) -> Rect2:
	if not _used.has(name):
		var t := Art.tex(name)
		_used[name] = Rect2(Vector2.ZERO, t.get_size()) if t == null else Rect2(t.get_image().get_used_rect())
	return _used[name]

## Places a sprite so that its visible pixels (or `content`, in texture pixels) fill the measured rect exactly.
static func art(parent: Node, name: String, bounds: Rect2, content := Rect2()) -> TextureRect:
	var t := TextureRect.new()
	t.name = name
	t.texture = Art.tex(name)
	t.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	t.stretch_mode = TextureRect.STRETCH_SCALE
	t.mouse_filter = Control.MOUSE_FILTER_IGNORE
	if t.texture:
		var c := content if content.size != Vector2.ZERO else used_rect(name)
		var s := bounds.size / c.size
		t.size = t.texture.get_size() * s
		t.position = bounds.position - c.position * s
	t.pivot_offset = bounds.position - t.position + bounds.size * 0.5
	parent.add_child(t)
	return t

## Nine-sliced surface; margins come from the sprite's measured inner rect (scaled by `border_scale`).
static func panel(parent: Node, name: String, r: Rect2, border_scale := 1.0) -> NinePatchRect:
	var p := NinePatchRect.new()
	p.texture = Art.tex(name)
	p.name = name
	var m := Art.metrics(name)
	if not m.is_empty():
		var size: Array = m["size"]
		var inner: Array = m["inner"]
		p.patch_margin_left = int(inner[0]); p.patch_margin_top = int(inner[1])
		p.patch_margin_right = int(size[0] - inner[0] - inner[2]); p.patch_margin_bottom = int(size[1] - inner[1] - inner[3])
	p.mouse_filter = Control.MOUSE_FILTER_IGNORE
	place(p, r)
	if border_scale != 1.0:
		# Draw at a scaled-down resolution so the borders shrink with the panel.
		p.size = r.size / border_scale
		p.scale = Vector2.ONE * border_scale
		p.pivot_offset = Vector2.ZERO
	parent.add_child(p)
	return p

## Painted number built from glyph sprites, centered in the rect. Height is the glyph canvas height.
static func number(parent: Node, text: String, r: Rect2, color := "brown", align := 0) -> Control:
	var box := Control.new()
	box.name = "Number " + text
	box.mouse_filter = Control.MOUSE_FILTER_IGNORE
	place(box, r)
	parent.add_child(box)
	set_number(box, text, color, align)
	return box

## align: -1 left, 0 center, 1 right.
static func set_number(box: Control, text: String, color := "brown", align := 0) -> void:
	for child in box.get_children():
		child.queue_free()
	var metrics := Art.glyph_metrics(color)
	var keys: Array[String] = []
	var width := 0.0
	for ch in text:
		var key: String = GLYPH_NAMES.get(ch, ch)
		if not metrics.has(key):
			continue
		keys.append(key)
		width += float(metrics[key]["advance"])
	if keys.is_empty():
		return
	# The rect height is the visible cap height (glyph pixels 26..126 of the 150 canvas).
	var scale := minf(box.size.y / 100.0, box.size.x / maxf(width, 1.0))
	var x := (box.size.x - width * scale) * (0.5 + 0.5 * align)
	var top := (box.size.y - 100.0 * scale) * 0.5 - 26.0 * scale
	for key in keys:
		var t := TextureRect.new()
		t.texture = Art.tex("glyph_%s_%s" % [color, key])
		t.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
		t.stretch_mode = TextureRect.STRETCH_SCALE
		t.mouse_filter = Control.MOUSE_FILTER_IGNORE
		var tw := float(t.texture.get_width()) * scale
		var adv := float(metrics[key]["advance"]) * scale
		t.position = Vector2(x + (adv - tw) * 0.5, top)
		t.size = Vector2(tw, 150.0 * scale)
		box.add_child(t)
		x += adv

## Round disc button with an icon; press scales to 0.92 with easing. Disabled = dimmed, same size.
static func disc(parent: Node, name: String, r: Rect2, icon: String, on_press: Callable, base := "btn_wood", icon_ratio := 0.54) -> Button:
	var b := Button.new()
	b.name = name
	b.flat = true
	b.focus_mode = Control.FOCUS_NONE
	var empty := StyleBoxEmpty.new()
	for state in ["normal", "hover", "pressed", "disabled", "focus", "hover_pressed"]:
		b.add_theme_stylebox_override(state, empty)
	place(b, r)
	var bg := TextureRect.new()
	bg.name = "Base"
	bg.texture = Art.tex(base)
	bg.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	bg.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	bg.size = r.size
	b.add_child(bg)
	if not icon.is_empty():
		var s := r.size * icon_ratio
		var ic := TextureRect.new()
		ic.name = "Icon"
		ic.texture = Art.tex(icon)
		ic.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
		ic.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
		ic.mouse_filter = Control.MOUSE_FILTER_IGNORE
		ic.position = (r.size - s) * 0.5
		ic.size = s
		b.add_child(ic)
	make_pressable(b, on_press)
	parent.add_child(b)
	return b

static func make_pressable(b: BaseButton, on_press: Callable) -> void:
	b.button_down.connect(func():
		var tw := b.create_tween()
		tw.tween_property(b, "scale", Vector2.ONE * PRESS_SCALE, 0.08).set_ease(Tween.EASE_OUT).set_trans(Tween.TRANS_QUAD))
	b.button_up.connect(func():
		var tw := b.create_tween()
		tw.tween_property(b, "scale", Vector2.ONE, 0.18).set_ease(Tween.EASE_OUT).set_trans(Tween.TRANS_BACK))
	b.pressed.connect(func():
		Sound.play("ui_tap")
		on_press.call())

static func set_enabled(b: BaseButton, enabled: bool) -> void:
	b.disabled = not enabled
	b.modulate = Color.WHITE if enabled else Color(1, 1, 1, 0.45)

## Transparent full-rect click catcher.
static func hit_area(parent: Node, r: Rect2, on_press: Callable) -> Button:
	var b := Button.new()
	b.flat = true
	b.focus_mode = Control.FOCUS_NONE
	var empty := StyleBoxEmpty.new()
	for state in ["normal", "hover", "pressed", "disabled", "focus", "hover_pressed"]:
		b.add_theme_stylebox_override(state, empty)
	place(b, r)
	b.pressed.connect(on_press)
	parent.add_child(b)
	return b
