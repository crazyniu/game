class_name UiText
extends RefCounted
## Whole-phrase texture labels selected by locale. Fonts are used only by the offline asset tool.

const ROOT := "res://art/locale/"

static var _catalog: Dictionary = {}
static var _pack: Dictionary = {}
static var _number_textures: Dictionary = {}
static var _locale := ""
static var _folder := ""


class LabelSprite extends TextureRect:
	var label_id: String
	var bounds: Rect2
	var cap: float
	var heading := false
	var outlined := false

	func _notification(what: int) -> void:
		if what == NOTIFICATION_TRANSLATION_CHANGED and not label_id.is_empty():
			UiText._fit_label(self)


const NUMBER_GAP := 14.0   # between the painted inks of neighbouring digits, in glyph pixels (cap 128)
static var _number_ink := {}   # character -> [ink left, ink width] in glyph pixels


class NumberSprite extends Control:
	var value: String
	var cap: float
	var align := 0

	func _draw() -> void:
		UiText._draw_number(self)

	func _notification(what: int) -> void:
		if what == NOTIFICATION_TRANSLATION_CHANGED and not value.is_empty():
			UiText.set_number(self, value)
		elif what == NOTIFICATION_RESIZED:
			queue_redraw()


const OUTLINE_SHADER := preload("res://game/outline.gdshader")
const OUTLINE_PAD := 12
static var _outline_material: ShaderMaterial
static var _padded: Dictionary = {}

## White words and numbers on coloured surfaces get a dark outline (shader), as in the candy-button style.
static func _is_white(color: Color) -> bool:
	return color.r > 0.99 and color.g > 0.99 and color.b > 0.99

static func outline_material(width: float) -> ShaderMaterial:
	var m := ShaderMaterial.new()
	m.shader = OUTLINE_SHADER
	m.set_shader_parameter("width", width)
	return m

static func _padded_texture(path: String, source: Texture2D) -> Texture2D:
	if not _padded.has(path):
		var img := source.get_image()
		var big := Image.create(img.get_width() + OUTLINE_PAD * 2, img.get_height() + OUTLINE_PAD * 2, false, Image.FORMAT_RGBA8)
		big.blit_rect(img, Rect2i(Vector2i.ZERO, img.get_size()), Vector2i(OUTLINE_PAD, OUTLINE_PAD))
		_padded[path] = ImageTexture.create_from_image(big)
	return _padded[path]


static func _load_pack() -> void:
	if _catalog.is_empty():
		_catalog = JSON.parse_string(FileAccess.get_file_as_string(ROOT + "catalog.json"))
	var locale := TranslationServer.get_locale()
	var available: Dictionary = _catalog["locales"]
	if not available.has(locale):
		locale = locale.get_slice("_", 0)
	if not available.has(locale):
		locale = _catalog["default_locale"]
	if locale == _locale:
		return
	_locale = locale
	var path: String = ROOT + available[locale]
	_folder = path.get_base_dir() + "/"
	_pack = JSON.parse_string(FileAccess.get_file_as_string(path))
	# Draw commands use texture RIDs; retain the resources until the locale changes.
	_number_textures.clear()
	var glyphs: Dictionary = _pack["numbers"]["glyphs"]
	for character in glyphs:
		_number_textures[character] = load(_folder + glyphs[character])
		var ink: Rect2i = _number_textures[character].get_image().get_used_rect()
		_number_ink[character] = [float(ink.position.x), float(ink.size.x)]


static func label(parent: Node, id: String, bounds: Rect2, cap: float, color: Color, heading := false) -> TextureRect:
	var image := LabelSprite.new()
	image.name = id.replace(".", "_")
	image.label_id = id
	image.bounds = bounds
	image.cap = cap
	image.heading = heading
	image.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	image.stretch_mode = TextureRect.STRETCH_SCALE
	image.mouse_filter = Control.MOUSE_FILTER_IGNORE
	image.modulate = color
	image.outlined = not heading and _is_white(color)
	_fit_label(image)
	parent.add_child(image)
	return image


static func _fit_label(image: LabelSprite) -> void:
	_load_pack()
	var entry: Dictionary = _pack["labels"][image.label_id]
	var path: String = _folder + entry["heading_file" if image.heading else "file"]
	var base: Texture2D = load(path)
	var natural := base.get_size()
	var k := minf(image.cap / float(entry["cap"]), minf(image.bounds.size.x / natural.x, image.bounds.size.y / natural.y))
	var shown := natural * k
	var origin := image.bounds.position + (image.bounds.size - shown) * 0.5
	if image.outlined:
		image.texture = _padded_texture(path, base)
		image.material = outline_material(maxf(3.0, float(entry["cap"]) * 0.16))
		image.size = (natural + Vector2.ONE * OUTLINE_PAD * 2.0) * k
		image.position = origin - Vector2.ONE * OUTLINE_PAD * k
	else:
		image.texture = base
		image.size = shown
		image.position = origin
	image.pivot_offset = image.size * 0.5


static func label_width(id: String, cap: float, heading := false) -> float:
	_load_pack()
	var entry: Dictionary = _pack["labels"][id]
	var texture: Texture2D = load(_folder + entry["heading_file" if heading else "file"])
	return texture.get_width() * cap / float(entry["cap"])


static func number_width(value: String, cap: float) -> float:
	_load_pack()
	var numbers: Dictionary = _pack["numbers"]
	var width := 0.0
	for character in value:
		width += _number_ink[character][1] + NUMBER_GAP
	return width * cap / float(numbers["cap"])


## Numeral cells share an ink-height canvas and fit together inside the supplied bounds.
static func number(parent: Node, value: String, bounds: Rect2, cap: float, color: Color, align := 0) -> Control:
	var box := NumberSprite.new()
	box.name = "Number"
	box.cap = cap
	box.align = align
	box.mouse_filter = Control.MOUSE_FILTER_IGNORE
	box.modulate = color
	if _is_white(color):
		box.material = outline_material(5.0)
	box.position = bounds.position
	box.size = bounds.size
	box.pivot_offset = bounds.size * 0.5
	set_number(box, value)
	parent.add_child(box)
	return box


static func set_number(box: NumberSprite, value: String) -> void:
	_load_pack()
	box.value = value
	box.queue_redraw()


## Draw after layout, using one scale for the full string and no child Control sizing.
static func _draw_number(box: NumberSprite) -> void:
	var numbers: Dictionary = _pack["numbers"]
	var width := number_width(box.value, float(numbers["cap"]))
	var k := minf(box.cap / float(numbers["cap"]), minf(box.size.x / width, box.size.y / float(numbers["line_height"])))
	var height := float(numbers["line_height"]) * k
	var x := (box.size.x - width * k) * (0.5 + 0.5 * box.align)
	var y := (box.size.y - height) * 0.5
	for character in box.value:   # set by painted ink, not by the wide glyph cells, so digits sit close together
		var advance := float(numbers["advances"][character]) * k
		var ink: Array = _number_ink[character]
		var texture: Texture2D = _number_textures[character]
		box.draw_texture_rect(texture, Rect2(x + (NUMBER_GAP * 0.5 - ink[0]) * k, y, advance, height), false)
		x += (ink[1] + NUMBER_GAP) * k
