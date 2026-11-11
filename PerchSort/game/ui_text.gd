class_name UiText
extends RefCounted
## Painted words selected by locale: every visible word is a hand-painted picture of one language pack, in one of four
## styles (gold titles, brown labels, caption lines, white words on green). No font is used at runtime.

const ROOT := "res://art/locale/"

static var _catalog: Dictionary = {}
static var _pack: Dictionary = {}
static var _number_textures: Dictionary = {}
static var _locale := ""
static var _folder := ""


class LabelSprite extends TextureRect:
	var label_id: String
	var styles: Array
	var bounds: Rect2
	var cap: float

	func _notification(what: int) -> void:
		if what == NOTIFICATION_TRANSLATION_CHANGED and not label_id.is_empty():
			UiText._fit_label(self)


## A painted word in its own colours (island lettering): fits the box keeping its ratio, centred or edge-aligned,
## and swaps to the other language's picture when the locale changes. No tint or outline is applied.
class PaintedSprite extends TextureRect:
	var painted_id: String
	var bounds: Rect2
	var align := 0   # -1 left, 0 centre, 1 right

	func _notification(what: int) -> void:
		if what == NOTIFICATION_TRANSLATION_CHANGED and not painted_id.is_empty():
			UiText._fit_painted(self)


const NUMBER_GAP := 14.0   # between the painted inks of neighbouring digits, in glyph pixels (cap 128)
static var _number_ink := {}   # style -> character -> [ink left, ink width] in glyph pixels


class NumberSprite extends Control:
	var value: String
	var style := "brown"
	var cap: float
	var align := 0

	func _draw() -> void:
		UiText._draw_number(self)

	func _notification(what: int) -> void:
		if what == NOTIFICATION_TRANSLATION_CHANGED and not value.is_empty():
			UiText.set_number(self, value)
		elif what == NOTIFICATION_RESIZED:
			queue_redraw()


static func _is_white(color: Color) -> bool:
	return color.r > 0.99 and color.g > 0.99 and color.b > 0.99


## The locale whose painted words are showing: the engine locale when a pack exists for it, else the default.
static func locale() -> String:
	_load_pack()
	return _locale

## Language codes that have a painted pack, in catalog order (the settings language row cycles through them).
static func languages() -> Array:
	_load_pack()
	return _catalog["locales"].keys()

## The word `id` painted in another language's pack (the language chooser names each language in its own words).
static func foreign_label(code: String, id: String, style := "brown") -> Texture2D:
	_load_pack()
	var path: String = ROOT + _catalog["locales"][code]
	var pack: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(path))
	return load(path.get_base_dir() + "/" + pack["words"][id][style]["file"])

## The code after `code` in catalog order, wrapping around.
static func next_language(code: String) -> String:
	var all := languages()
	var at := all.find(code)
	return all[(at + 1) % all.size()]

## The player's language choice; "" leaves the device language in charge.
static func set_language(code: String) -> void:
	if not code.is_empty():
		TranslationServer.set_locale(code)

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
	_number_ink.clear()
	var styles: Dictionary = _pack["numbers"]["styles"]
	for style in styles:
		_number_textures[style] = {}
		_number_ink[style] = {}
		var glyphs: Dictionary = styles[style]["glyphs"]
		for character in glyphs:
			var texture: Texture2D = load(_folder + glyphs[character])
			_number_textures[style][character] = texture
			var ink: Rect2i = texture.get_image().get_used_rect()
			_number_ink[style][character] = [float(ink.position.x), float(ink.size.x)]


## A painted picture with words in it (home tiles): the active locale's copy when it has one, else the shared picture.
static func picture(name: String) -> Texture2D:
	_load_pack()
	var path := _folder + name + ".png"
	return load(path) if ResourceLoader.exists(path) else Art.tex(name)


## Baked settings document page (terms, credits, contact) of the active locale.
static func document_texture(id: String) -> Texture2D:
	_load_pack()
	return load(_folder + "doc_%s.png" % id)


## The painted word `id`. The style follows the use: gold for headings, white on green fills, brown or caption for
## ink on cream. The picture keeps its own colours; `color` only chooses between the white and the ink variant.
static func label(parent: Node, id: String, bounds: Rect2, cap: float, color: Color, heading := false) -> TextureRect:
	var image := LabelSprite.new()
	image.name = id.replace(".", "_")
	image.label_id = id
	image.bounds = bounds
	image.cap = cap
	image.styles = ["gold"] if heading else (["white"] if _is_white(color) else ["brown", "caption"])
	image.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	image.stretch_mode = TextureRect.STRETCH_SCALE
	image.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_fit_label(image)
	parent.add_child(image)
	return image

static func painted(parent: Node, id: String, bounds: Rect2, align := 0) -> TextureRect:
	var image := PaintedSprite.new()
	image.name = "Word_" + id
	image.painted_id = id
	image.bounds = bounds
	image.align = align
	image.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	image.stretch_mode = TextureRect.STRETCH_SCALE
	image.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_fit_painted(image)
	parent.add_child(image)
	return image

static func _fit_painted(image: PaintedSprite) -> void:
	_load_pack()
	if not has_painted(image.painted_id):
		push_error("No painted word '%s' for locale %s" % [image.painted_id, _locale])
		image.texture = null
		return
	var base := _painted_ink(image.painted_id)
	var natural := base.get_size()
	var k := minf(image.bounds.size.x / natural.x, image.bounds.size.y / natural.y)
	var shown := natural * k
	var free := image.bounds.size - shown
	image.texture = base
	image.size = shown
	image.position = image.bounds.position + Vector2(free.x * (0.5 + 0.5 * image.align), free.y * 0.5)
	image.pivot_offset = shown * 0.5

## A painted word cut to its visible ink: the pictures carry uneven transparent margins, so words fit their boxes
## by what is actually drawn.
static var _ink_cache := {}

static func _painted_ink(id: String) -> Texture2D:
	var path: String = _folder + _pack["painted"][id]["file"]
	if not _ink_cache.has(path):
		var base: Texture2D = load(path)
		var used := base.get_image().get_used_rect()
		var cut := AtlasTexture.new()
		cut.atlas = base
		cut.region = Rect2(used.grow(2)).intersection(Rect2(Vector2.ZERO, base.get_size()))
		_ink_cache[path] = cut
	return _ink_cache[path]

## The width a painted word takes when its height is `height` (for laying out rows before placing them).
static func has_painted(id: String) -> bool:
	_load_pack()
	return _pack.has("painted") and _pack["painted"].has(id)

static func painted_width(id: String, height: float) -> float:
	_load_pack()
	var base := _painted_ink(id)
	return base.get_width() * height / base.get_height()


## The painted variant of word `id` for the first of `styles` it has. A word or style that was never painted is a
## loud error (the screenshots fail on it), never a silent stand-in.
static func _word(id: String, styles: Array) -> Dictionary:
	_load_pack()
	var variants: Dictionary = _pack["words"].get(id, {})
	for style in styles:
		if variants.has(style):
			return variants[style]
	push_error("No painted word '%s' in style %s for locale %s" % [id, styles, _locale])
	return {}

static func _fit_label(image: LabelSprite) -> void:
	var entry := _word(image.label_id, image.styles)
	if entry.is_empty():
		image.texture = null
		image.size = Vector2.ZERO
		return
	var base: Texture2D = load(_folder + entry["file"])
	var natural := base.get_size()
	var k := minf(image.cap / float(entry["cap"]), minf(image.bounds.size.x / natural.x, image.bounds.size.y / natural.y))
	var shown := natural * k
	image.texture = base
	image.size = shown
	image.position = image.bounds.position + (image.bounds.size - shown) * 0.5
	image.pivot_offset = shown * 0.5


static func has_heading(id: String) -> bool:
	_load_pack()
	return _pack["words"].has(id) and _pack["words"][id].has("gold")

static func label_width(id: String, cap: float, heading := false) -> float:
	var entry := _word(id, ["gold"] if heading else ["brown", "caption", "white"])
	if entry.is_empty():
		return 0.0
	var texture: Texture2D = load(_folder + entry["file"])
	return texture.get_width() * cap / float(entry["cap"])


## The painted digit set for a colour: cream on dark or green surfaces, gold for rewards, brown ink on cream.
static func number_style(color: Color) -> String:
	if _is_white(color):
		return "cream"
	if color.r > 0.9 and color.g > 0.65 and color.b < 0.4:
		return "gold"
	return "brown"

static func number_width(value: String, cap: float, style := "brown") -> float:
	_load_pack()
	var numbers: Dictionary = _pack["numbers"]
	var width := 0.0
	for character in value:
		width += _number_ink[style][character][1] + NUMBER_GAP
	return width * cap / float(numbers["cap"])


## Numeral cells share an ink-height canvas and fit together inside the supplied bounds.
static func number(parent: Node, value: String, bounds: Rect2, cap: float, color: Color, align := 0) -> Control:
	var box := NumberSprite.new()
	box.name = "Number"
	box.cap = cap
	box.align = align
	box.style = number_style(color)
	box.mouse_filter = Control.MOUSE_FILTER_IGNORE
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
	var advances: Dictionary = numbers["styles"][box.style]["advances"]
	var width := number_width(box.value, float(numbers["cap"]), box.style)
	var k := minf(box.cap / float(numbers["cap"]), minf(box.size.x / width, box.size.y / float(numbers["line_height"])))
	var height := float(numbers["line_height"]) * k
	var x := (box.size.x - width * k) * (0.5 + 0.5 * box.align)
	var y := (box.size.y - height) * 0.5
	for character in box.value:   # set by painted ink, not by the wide glyph cells, so digits sit close together
		var advance := float(advances[character]) * k
		var ink: Array = _number_ink[box.style][character]
		var texture: Texture2D = _number_textures[box.style][character]
		box.draw_texture_rect(texture, Rect2(x + (NUMBER_GAP * 0.5 - ink[0]) * k, y, advance, height), false)
		x += (ink[1] + NUMBER_GAP) * k
