extends RefCounted
## Chest-only lettering: the accepted Home Play source, companion words, and unchanged Home digits.

const WORDS := {"action.play": "label_home_island_play", "chest.open": "label_chest_open", "action.ok": "label_chest_confirm"}
const INK := {
	"label_home_island_play_zh": Rect2(12, 12, 1049, 520),
	"label_home_island_play_en": Rect2(11, 11, 1291, 613),
	"label_chest_open_zh": Rect2(306, 101, 1267, 662),
	"label_chest_confirm_zh": Rect2(174, 105, 1409, 711),
	"label_chest_open_en": Rect2(283, 70, 1604, 614),
	"label_chest_confirm_en": Rect2(404, 117, 1088, 612),
}

class Word extends TextureRect:
	var prefix: String
	var bounds: Rect2

	func refresh() -> void:
		var key := prefix + "_" + UiText.locale()
		texture = Art.tex(key)
		var content: Rect2 = INK[key]
		var k := minf(bounds.size.x / content.size.x, bounds.size.y / content.size.y)
		size = texture.get_size() * k
		position = bounds.get_center() - content.get_center() * k
		set_meta("visible_ink", Rect2(bounds.get_center() - content.size * k * 0.5, content.size * k))

	func _notification(what: int) -> void:
		if what == NOTIFICATION_TRANSLATION_CHANGED and not prefix.is_empty():
			refresh()

static func word(parent: Node, id: String, bounds: Rect2) -> TextureRect:
	var image := Word.new()
	image.name = id.replace(".", "_")
	image.prefix = WORDS[id]
	image.bounds = bounds
	image.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	image.stretch_mode = TextureRect.STRETCH_SCALE
	image.mouse_filter = Control.MOUSE_FILTER_IGNORE
	if id == "action.play":
		var paint := ShaderMaterial.new()
		# The exact soft-white formula baked into the accepted complete Home button.
		paint.shader = preload("res://game/chest_play_word.gdshader")
		image.material = paint
	image.refresh()
	parent.add_child(image)
	return image

static func number(parent: Node, value: String, bounds: Rect2) -> Control:
	var group := Control.new()
	group.name = "Number"
	group.mouse_filter = Control.MOUSE_FILTER_IGNORE
	group.set_meta("value", value)
	UI.place(group, bounds)
	parent.add_child(group)
	var keys: Array[String] = []
	var inks: Array[Rect2] = []
	var width := 0.0
	var gap := bounds.size.y * 0.06
	for character in value:
		var key := "label_chest_slash" if character == "/" else Island.DIGIT + character
		var ink := Rect2(294, 294, 458, 912) if character == "/" else UI.used_rect(key)
		keys.append(key)
		inks.append(ink)
		width += ink.size.x / ink.size.y * bounds.size.y + gap
	width -= gap
	var k := minf(1.0, bounds.size.x / width)
	var x := (bounds.size.x - width * k) * 0.5
	for i in keys.size():
		var w := inks[i].size.x / inks[i].size.y * bounds.size.y * k
		UI.art(group, keys[i], Rect2(x, (bounds.size.y - bounds.size.y * k) * 0.5, w, bounds.size.y * k), inks[i])
		x += w + gap * k
	return group
