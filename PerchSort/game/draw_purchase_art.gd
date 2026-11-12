class_name DrawPurchaseArt
extends RefCounted
## Page-local approved artwork. Source pixels stay unchanged; every fit is uniform.

const DESIGN := Vector2(916, 1717)
const SCALE := 1920.0 / 1717.0
const GLYPHS := {"0": "zero", "1": "one", "2": "two", "3": "three", "4": "four", "5": "five", "6": "six", "7": "seven", "8": "eight", "9": "nine", "+": "plus", "×": "multiply", "¥": "yuan", "/": "slash", "-": "minus"}
static var _inks: Dictionary = {}

class LocalizedSprite extends TextureRect:
	var prefix: String
	var bounds: Rect2

	func refresh() -> void:
		var source := Art.tex(prefix + "_zh")
		var source_ink := DrawPurchaseArt.ink(prefix + "_zh")
		var source_scale := minf(bounds.size.x / source.get_width(), bounds.size.y / source.get_height())
		var target := Rect2(bounds.position + (bounds.size - source.get_size() * source_scale) * 0.5 + source_ink.position * source_scale, source_ink.size * source_scale)
		var key := prefix + "_" + UiText.locale()
		texture = Art.tex(key)
		var visible := DrawPurchaseArt.ink(key)
		var k := minf(target.size.x / visible.size.x, target.size.y / visible.size.y)
		size = texture.get_size() * k
		position = target.get_center() - visible.get_center() * k

	func _notification(what: int) -> void:
		if what == NOTIFICATION_TRANSLATION_CHANGED and not prefix.is_empty():
			refresh()

static func ink(key: String) -> Rect2:
	if _inks.is_empty():
		_inks = JSON.parse_string(FileAccess.get_file_as_string("res://art/island/draw_purchase_ink.json"))
	var box: Array = _inks[key]
	return Rect2(box[0], box[1], box[2], box[3])

static func canvas(parent: Node) -> Control:
	var page := Control.new()
	page.name = "ApprovedLayout"
	page.mouse_filter = Control.MOUSE_FILTER_IGNORE
	page.size = DESIGN
	page.scale = Vector2.ONE * SCALE
	page.position.x = (1080.0 - DESIGN.x * SCALE) * 0.5
	parent.add_child(page)
	return page

static func background(parent: Node) -> TextureRect:
	var image := UI.image(parent, ShopSkin.FOREST_BACKGROUND, Rect2(0, 0, 1080, 1920))
	image.name = "ForestBackground"
	image.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_COVERED
	return image

static func fit(parent: Node, key: String, bounds: Rect2) -> TextureRect:
	return UI.art(parent, key, bounds, ink(key))

static func localized(parent: Node, prefix: String, bounds: Rect2) -> LocalizedSprite:
	var image := LocalizedSprite.new()
	image.name = prefix
	image.prefix = prefix
	image.bounds = bounds
	image.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	image.stretch_mode = TextureRect.STRETCH_SCALE
	image.mouse_filter = Control.MOUSE_FILTER_IGNORE
	image.refresh()
	parent.add_child(image)
	return image

static func button(parent: Node, node_name: String, key: String, bounds: Rect2, callback: Callable, translated := false) -> Button:
	var button := UI.hit_area(parent, bounds, func():
		Sound.play("ui_tap")
		callback.call())
	button.name = node_name
	var face: TextureRect
	if translated:
		face = localized(button, key, Rect2(Vector2.ZERO, bounds.size))
	else:
		face = UI.image(button, key, Rect2(Vector2.ZERO, bounds.size))
	face.name = "Face"
	button.button_down.connect(func(): face.self_modulate = Island.PRESSED)
	button.button_up.connect(func(): face.self_modulate = Color.WHITE)
	return button

static func number(parent: Node, value: String, bounds: Rect2) -> Control:
	var group := Control.new()
	group.name = "Number"
	group.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(group, bounds)
	parent.add_child(group)
	group.set_meta("value", value)
	var width := 0.0
	var cap := bounds.size.y
	var gap := cap * 0.04
	for character in value:
		var glyph := ink("label_draw_purchase_" + GLYPHS[character])
		width += glyph.size.x / glyph.size.y * cap + gap
	width -= gap
	var k := minf(1.0, bounds.size.x / width)
	var left := (bounds.size.x - width * k) * 0.5
	for character in value:
		var key: String = "label_draw_purchase_" + GLYPHS[character]
		var glyph := ink(key)
		var w := glyph.size.x / glyph.size.y * cap * k
		fit(group, key, Rect2(left, (bounds.size.y - cap * k) * 0.5, w, cap * k))
		left += w + gap * k
	return group
