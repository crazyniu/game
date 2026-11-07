class_name UI
extends RefCounted
## UI builders (docs/design/ui-system.md). All rects are design pixels, origin top-left of the parent.



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

## Places a sprite so that its visible pixels (or `content`, in texture pixels) fit inside `bounds`, centred, keeping their aspect ratio.
static func art(parent: Node, name: String, bounds: Rect2, content := Rect2()) -> TextureRect:
	var t := TextureRect.new()
	t.name = name
	t.texture = Art.tex(name)
	t.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	t.stretch_mode = TextureRect.STRETCH_SCALE
	t.mouse_filter = Control.MOUSE_FILTER_IGNORE
	if t.texture:
		# Uniform scale: the visible pixels fit inside `bounds` (centred), never stretched.
		var c := content if content.size != Vector2.ZERO else used_rect(name)
		var k := minf(bounds.size.x / c.size.x, bounds.size.y / c.size.y)
		var fitted := c.size * k
		t.size = t.texture.get_size() * k
		t.position = bounds.position + (bounds.size - fitted) * 0.5 - c.position * k
	t.pivot_offset = bounds.position - t.position + bounds.size * 0.5
	parent.add_child(t)
	return t

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

## ---------- cartoon kit (docs/design/ui-system.md) ----------
## Shapes are flat rounded fills with a darker bottom edge and one soft shadow, drawn by code so every role has
## one exact size and look. Icons, words and numbers are painted white sprites tinted here.

const FILL := {"cream": Color8(255, 247, 232), "green": Color8(108, 194, 74), "orange": Color8(255, 165, 60),
	"blue": Color8(93, 185, 238), "red": Color8(239, 85, 72)}
const RIM := {"cream": Color8(233, 214, 176), "green": Color8(76, 150, 49), "orange": Color8(218, 122, 20),
	"blue": Color8(60, 147, 201), "red": Color8(194, 58, 47)}
const INK := Color8(90, 58, 30)
const GOLD := Color8(255, 200, 61)
const FADED := Color8(233, 214, 176)      # empty stars, locked marks on cream
const PRIMARY := 160.0
const SECONDARY := 128.0
const COUNTER_H := 96.0
const NUMBER_X := 108.0                  # a counter's number starts here, clear of its icon
const BADGE := 56.0
const DISABLED_ALPHA := 0.45
const WORD_CAP := 96.0                    # cap height the word sprites are built at (tools/kit/build_words.py)
const WORD_PAD := 29.0                    # their canvas padding around the outlined lettering
const EDGE := Color8(150, 82, 28)         # warm outline of lettering on cream and on the scenes
const LETTER_GAP := 0.1                   # gap between number glyphs, in caps

static var _letters: Dictionary = {}

## Ink on cream, white on every coloured fill.
static func ink_on(kind: String) -> Color:
	return INK if kind == "cream" else Color.WHITE

static func _box(fill: Color, rim: Color, radius: float, edge: float, shadow := true) -> StyleBoxFlat:
	var sb := StyleBoxFlat.new()
	sb.bg_color = fill
	sb.border_color = rim
	sb.border_width_bottom = int(edge)
	sb.set_corner_radius_all(int(radius))
	sb.anti_aliasing = true
	sb.anti_aliasing_size = 1.2
	if shadow:
		sb.shadow_color = Color(0, 0, 0, 0.18)
		sb.shadow_size = 12
		sb.shadow_offset = Vector2(0, 8)
	return sb

## Rounded flat shape: panel (radius 56, edge 12), card (40, 10), pill (height / 2, 8).
static func shape(parent: Node, r: Rect2, kind := "cream", radius := 56.0, edge := 12.0, shadow := true) -> Panel:
	var p := Panel.new()
	p.name = "Shape"
	p.mouse_filter = Control.MOUSE_FILTER_IGNORE
	p.add_theme_stylebox_override("panel", _box(FILL[kind], RIM[kind], radius, edge, shadow))
	place(p, r)
	parent.add_child(p)
	return p

## White icon sprite tinted and fitted into a square around `center`.
static func icon(parent: Node, name: String, center: Vector2, size: float, color := Color.WHITE) -> TextureRect:
	var t := art(parent, "icon_" + name, Rect2(center - Vector2.ONE * size * 0.5, Vector2.ONE * size))
	t.modulate = color
	return t

## Round button: kind "green" (primary, 160), "orange" or "cream" (secondary, 128). A press sinks it by its edge.
static func button(parent: Node, name: String, center: Vector2, kind: String, icon_name: String, on_press: Callable,
		size := -1.0) -> Button:
	if size < 0.0:
		size = PRIMARY if kind == "green" else SECONDARY
	var b := Button.new()
	b.name = name
	b.focus_mode = Control.FOCUS_NONE
	var up := _box(FILL[kind], RIM[kind], size * 0.5, 10.0)
	var down := _box(FILL[kind], RIM[kind], size * 0.5, 4.0)
	down.expand_margin_top = -6.0
	for state in ["normal", "hover", "disabled", "focus"]:
		b.add_theme_stylebox_override(state, up)
	b.add_theme_stylebox_override("pressed", down)
	b.add_theme_stylebox_override("hover_pressed", down)
	place(b, Rect2(center - Vector2.ONE * size * 0.5, Vector2.ONE * size))
	if not icon_name.is_empty():
		var ic := icon(b, icon_name, Vector2(size * 0.5, size * 0.5 - 5.0), size * 0.56, ink_on(kind))
		ic.name = "Icon"
		b.button_down.connect(func(): ic.position.y += 6.0)
		b.button_up.connect(func(): ic.position.y -= 6.0)
	b.pressed.connect(func():
		Sound.play("ui_tap")
		on_press.call())
	parent.add_child(b)
	return b

## Red count badge on a button's rim at 45 degrees (upper right).
static func badge(b: Control, count_text: String) -> Panel:
	var c := Vector2.ONE * b.size.x * 0.5 + Vector2(1, -1) * b.size.x * 0.5 * 0.7071
	var p := shape(b, Rect2(c - Vector2.ONE * BADGE * 0.5, Vector2.ONE * BADGE), "red", BADGE * 0.5, 5.0, false)
	p.name = "Badge"
	number(p, count_text, Rect2(8, BADGE * 0.5 - 18.0, BADGE - 16, 30), Color.WHITE)
	return p

## Pill counter: icon (texture name, tinted when `icon_color` is set) then a number. Width fits the number plus
## `extra` room on the right. Returns the pill; its child "Value" is the number box (see set_number).
static func counter(parent: Node, at: Vector2, icon_name: String, value: String, icon_color := Color(0, 0, 0, 0),
		extra := 0.0, kind := "cream") -> Panel:
	var w := NUMBER_X + number_width(value, 44.0) + 32.0 + extra
	var p := shape(parent, Rect2(at, Vector2(w, COUNTER_H)), kind, COUNTER_H * 0.5, 8.0)
	p.name = "Counter"
	var t := art(p, icon_name, Rect2(16, 8, 72, 72))
	if icon_color.a > 0.0:
		t.modulate = icon_color
	var n := number(p, value, Rect2(NUMBER_X, (COUNTER_H - 8.0 - 44.0) * 0.5, w - NUMBER_X - 24.0 - extra, 44), ink_on(kind), -1)
	n.name = "Value"
	return p

static func _letter_meta() -> Dictionary:
	if _letters.is_empty():
		_letters = JSON.parse_string(FileAccess.get_file_as_string("res://art/letters/letters.json"))
	return _letters

## Width of a painted number at a cap height.
static func number_width(text: String, cap: float) -> float:
	var m := _letter_meta()
	var k: float = cap / float(m["cap"])
	var gap := cap * LETTER_GAP
	var w := 0.0
	for ch in text:
		var g: Dictionary = m["glyphs"].get(str(ch.unicode_at(0)), {})
		if g.is_empty():
			continue
		w += float(g["w"]) * k + gap
	return maxf(0.0, w - gap)

## Painted number (digits and + - x / % and infinity) from the lettering glyphs. The rect height is the cap
## height; align -1 left, 0 centre, 1 right. Numbers are plain glyphs tinted `color`: dark ink on cream (best
## contrast at small sizes), white on coloured fills. Only words carry the warm outline.
static func number(parent: Node, text: String, r: Rect2, color := INK, align := 0) -> Control:
	var box := Control.new()
	box.name = "Number"
	box.mouse_filter = Control.MOUSE_FILTER_IGNORE
	place(box, r)
	box.set_meta("align", align)
	box.modulate = color
	parent.add_child(box)
	set_number(box, text)
	return box

static func set_number(box: Control, text: String) -> void:
	for child in box.get_children():
		child.queue_free()
	var m := _letter_meta()
	var cap := box.size.y
	var k: float = cap / float(m["cap"])
	var width := number_width(text, cap)
	if width > box.size.x and width > 0.0:   # shrink to fit
		cap *= box.size.x / width
		k = cap / float(m["cap"])
		width = box.size.x
	var align: int = box.get_meta("align", 0)
	var x := (box.size.x - width) * (0.5 + 0.5 * align)
	var baseline := (box.size.y + cap) * 0.5
	var glyphs := []
	for ch in text:
		var code := ch.unicode_at(0)
		var g: Dictionary = m["glyphs"].get(str(code), {})
		if g.is_empty():
			continue
		var at := Vector2(x, baseline + float(g["dy"]) * cap)
		var size := Vector2(float(g["w"]), float(g["h"])) * k
		glyphs.append([code, at, size])
		x += size.x + cap * LETTER_GAP
	for item in glyphs:
		_glyph(box, "letter_%04x" % item[0], item[1], item[2])

static func _glyph(box: Control, name: String, at: Vector2, size: Vector2) -> TextureRect:
	var t := TextureRect.new()
	t.texture = Art.tex(name)
	t.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	t.stretch_mode = TextureRect.STRETCH_SCALE
	t.mouse_filter = Control.MOUSE_FILTER_IGNORE
	t.size = size
	t.position = at
	box.add_child(t)
	return t

## Slug of a phrase: the word sprite is art/words/word_<slug>.png (tools/kit/build_words.py builds every phrase
## passed to UI.word as a literal, plus tools/kit/words.txt).
static func slug(text: String) -> String:
	var out := ""
	for ch in text.to_lower():
		var c := ch.unicode_at(0)
		out += ch if (c >= 97 and c <= 122) or (c >= 48 and c <= 57) else "_"
	while out.contains("__"):
		out = out.replace("__", "_")
	return out.strip_edges().trim_prefix("_").trim_suffix("_")

## Width of a painted phrase at a cap height.
static func word_width(text: String, cap: float) -> float:
	var t := Art.tex("word_" + slug(text))
	if t == null:
		return 0.0
	return (float(t.get_width()) - 2.0 * WORD_PAD) * cap / WORD_CAP

## Painted phrase whose cap height is `cap`, centred vertically on `at.y`; align -1 starts at at.x, 0 centres on
## it, 1 ends at it. Lettering is white with the warm outline baked in; `color` is kept for callers and ignored.
## The sprite canvas is padded by WORD_PAD around 1.5 caps, with the baseline at WORD_PAD + 1.15 caps.
static func word(parent: Node, text: String, at: Vector2, cap: float, color := INK, align := 0) -> TextureRect:
	var name := "word_" + slug(text)
	var t := TextureRect.new()
	t.name = name
	t.texture = Art.tex(name)
	t.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	t.stretch_mode = TextureRect.STRETCH_SCALE
	t.mouse_filter = Control.MOUSE_FILTER_IGNORE
	if t.texture == null:
		push_error("missing word sprite %s (run tools/kit/build_words.py)" % name)
		return t
	var k := cap / WORD_CAP
	t.size = Vector2(t.texture.get_size()) * k
	var top := at.y + cap * 0.5 - (WORD_PAD + WORD_CAP * 1.15) * k
	var ink_w := t.size.x - 2.0 * WORD_PAD * k
	t.position = Vector2(at.x - ink_w * (0.5 + 0.5 * align) - WORD_PAD * k, top)
	t.pivot_offset = t.size * 0.5
	parent.add_child(t)
	return t

## Fills `filled` of `count` stars in a row centred on `center`.
static func stars(parent: Node, center: Vector2, size: float, gap: float, filled: int, count := 3) -> Array:
	var out := []
	for i in count:
		var c := center + Vector2((i - (count - 1) * 0.5) * (size + gap), 0)
		out.append(icon(parent, "star", c, size, GOLD if i < filled else FADED))
	return out

## Dims a control to show it is disabled; it keeps its size and stops taking input.
static func set_active(c: Control, active: bool) -> void:
	c.modulate.a = 1.0 if active else DISABLED_ALPHA
	if c is BaseButton:
		c.disabled = not active
