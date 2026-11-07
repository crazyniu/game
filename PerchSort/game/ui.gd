class_name UI
extends RefCounted
## UI builders (docs/design/ui-system.md). All rects are design pixels, origin top-left of the parent.

const TextArt := preload("res://game/ui_text.gd")



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
const PLAY_SIZE := Vector2(360.0, 128.0)
const COUNTER_H := 96.0
const NUMBER_X := 108.0                  # a counter's number starts here, clear of its icon
const BADGE := 56.0
const DISABLED_ALPHA := 0.45
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

## Painted full-colour pictures (tools/kit/pics, booster illustrations) that replace the white symbols wherever an
## icon sits on cream or on a scene. White symbols remain only on solid green and red fills (play, check, close).
const PICTURES := {"star": "pic_star", "gear": "pic_gear", "home": "pic_home", "retry": "pic_retry", "undo": "pic_undo",
	"back": "pic_back", "left": "pic_back", "info": "pic_help", "help": "pic_help", "lock": "pic_lock", "check": "pic_check",
	"hop": "pic_moves", "moves": "pic_moves", "footprint": "pic_moves", "sun": "pic_sun", "moon": "pic_moon",
	"feather": "pic_feather", "shop": "pic_shop", "aviary": "pic_aviary", "sound_on": "pic_sound_on",
	"sound_off": "pic_sound_off", "sparkle": "pic_sparkle", "bulb": "booster_hint_illus", "sapling": "booster_sapling_illus",
	"eye": "booster_owl_eye_illus", "bell": "booster_bell_illus", "scarecrow": "booster_scarecrow_illus",
	"rain": "booster_rain_illus"}

## The painted picture for an icon drawn in `color`, or "" when the white symbol stays (white on a solid fill).
## A faded star is the painted empty star.
static func picture_of(name: String, color: Color) -> String:
	if not PICTURES.has(name) or (color.r > 0.99 and color.g > 0.99 and color.b > 0.99):
		return ""
	if name == "star" and color.is_equal_approx(FADED):
		return "pic_star_empty"
	return PICTURES[name]

const CONTROL_ICONS := ["left", "right", "check", "retry", "undo", "home", "play", "shop", "aviary", "gear", "hanger", "lock", "sound_on", "sound_off", "bulb", "eye", "bell", "sun", "moon"]

static func icon_texture(name: String) -> Texture2D:
	if name == "back":
		return Art.tex("icon_left")
	if name == "close":
		return load("res://art/icons/icon_aviary_close.svg")
	if name == "feather":
		return load("res://art/icons/icon_aviary_feather.svg")
	if name in CONTROL_ICONS:
		return load("res://art/icons/icon_control_" + name + ".svg")
	return Art.tex("icon_" + name)

## Control symbols share a square canvas; illustrated effect symbols retain their alpha-fitted bounds.
static func icon(parent: Node, name: String, center: Vector2, size: float, color := Color.WHITE) -> TextureRect:
	var bounds := Rect2(center - Vector2.ONE * size * 0.5, Vector2.ONE * size)
	var t: TextureRect
	var pic := picture_of(name, color)
	if not pic.is_empty():
		t = art(parent, pic, bounds)
		t.name = "icon_" + name
		t.modulate = Color(1, 1, 1, color.a)
		return t
	if name == "back":
		t = art(parent, "icon_left", bounds)
	elif name in CONTROL_ICONS or name in ["close", "feather"]:
		t = TextureRect.new()
		t.name = "icon_" + name
		t.texture = icon_texture(name)
		t.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
		t.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
		t.mouse_filter = Control.MOUSE_FILTER_IGNORE
		place(t, bounds)
		parent.add_child(t)
	else:
		t = art(parent, "icon_" + name, bounds)
	t.modulate = color
	return t

## Round icon buttons; Play uses a green labelled pill. A press sinks the face by its edge.
static func button(parent: Node, name: String, center: Vector2, kind: String, icon_name: String, on_press: Callable,
		size := -1.0) -> Button:
	if size < 0.0:
		size = PRIMARY if kind == "green" else SECONDARY
	# Painted pictures sit on cream faces, so every secondary picture button reads as one family.
	if kind == "orange" and PICTURES.has(icon_name):
		kind = "cream"
	var dimensions := PLAY_SIZE if icon_name == "play" else Vector2.ONE * size
	var b := Button.new()
	b.name = name
	b.focus_mode = Control.FOCUS_NONE
	var up := _box(FILL[kind], RIM[kind], dimensions.y * 0.5, 10.0)
	var down := _box(FILL[kind], RIM[kind], dimensions.y * 0.5, 4.0)
	down.expand_margin_top = -6.0
	for state in ["normal", "hover", "disabled", "focus"]:
		b.add_theme_stylebox_override(state, up)
	b.add_theme_stylebox_override("pressed", down)
	b.add_theme_stylebox_override("hover_pressed", down)
	place(b, Rect2(center - dimensions * 0.5, dimensions))
	if not icon_name.is_empty():
		var ic: TextureRect
		if icon_name == "play":
			ic = label(b, "action.play", Rect2(32, 12, dimensions.x - 64.0, dimensions.y - 34.0), 48.0, Color.WHITE)
			ic.name = "Label"
		else:
			var scale := 0.66 if not picture_of(icon_name, ink_on(kind)).is_empty() else 0.56
			ic = icon(b, icon_name, Vector2(size * 0.5, size * 0.5 - 5.0), size * scale, ink_on(kind))
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
	var t: TextureRect
	if icon_name.begins_with("icon_"):
		t = icon(p, icon_name.trim_prefix("icon_"), Vector2(52, 44), 64.0)
	else:
		t = art(p, icon_name, Rect2(16, 8, 72, 72))
	if icon_color.a > 0.0:
		t.modulate = icon_color
	var n := number(p, value, Rect2(NUMBER_X, (COUNTER_H - 8.0 - 44.0) * 0.5, w - NUMBER_X - 24.0 - extra, 44), ink_on(kind), -1)
	n.name = "Value"
	return p

## Whole locale label fitted to its dedicated layout box.
static func label(parent: Node, id: String, bounds: Rect2, cap: float, color := INK) -> TextureRect:
	return TextArt.label(parent, id, bounds, cap, color)

static func label_width(id: String, cap: float) -> float:
	return TextArt.label_width(id, cap)

## Original white-and-brown heading treatment, baked as one shaped phrase.
static func heading(parent: Node, id: String, bounds: Rect2, cap: float) -> TextureRect:
	return TextArt.label(parent, id, bounds, cap, Color.WHITE, true)

static func heading_width(id: String, cap: float) -> float:
	return TextArt.label_width(id, cap, true)

## Dynamic numerals fit their complete painted cells; alignment is -1 left, 0 centre, 1 right.
static func number_width(text: String, cap: float) -> float:
	return TextArt.number_width(text, cap)

static func number(parent: Node, text: String, r: Rect2, color := INK, align := 0) -> Control:
	return TextArt.number(parent, text, r, r.size.y, color, align)

static func set_number(box: Control, text: String) -> void:
	TextArt.set_number(box as TextArt.NumberSprite, text)

## Fills `filled` of `count` stars in a row centred on `center`.
static func stars(parent: Node, center: Vector2, size: float, gap: float, filled: int, count := 3) -> Array:
	var out := []
	for i in count:
		var c := center + Vector2((i - (count - 1) * 0.5) * (size + gap), 0)
		out.append(icon(parent, "star", c, size, GOLD if i < filled else FADED))
	return out

## Outlined map ratings remain legible on both snow and the golden trail. Empty slots are hollow.
static func rating_stars(parent: Node, center: Vector2, size: float, gap: float, filled: int, count := 3) -> Array:
	var out := []
	for i in count:
		var star := TextureRect.new()
		star.texture = Art.tex("pic_star" if i < filled else "pic_star_empty")
		star.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
		star.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
		star.mouse_filter = Control.MOUSE_FILTER_IGNORE
		var c := center + Vector2((i - (count - 1) * 0.5) * (size + gap), 0)
		place(star, Rect2(c - Vector2.ONE * size * 0.5, Vector2.ONE * size))
		parent.add_child(star)
		out.append(star)
	return out

## Dims a control to show it is disabled; it keeps its size and stops taking input.
static func set_active(c: Control, active: bool) -> void:
	c.modulate.a = 1.0 if active else DISABLED_ALPHA
	if c is BaseButton:
		c.disabled = not active
