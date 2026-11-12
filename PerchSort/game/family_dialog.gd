class_name FamilyDialog
extends RefCounted
## Opt-in presentation for non-gameplay celebrations and the level card.

const PRIMARY := Vector2(520, 176)
const SECONDARY := PRIMARY / 1.25
const STAR_SIZE := 180.0
const STAR_ROW := 224.0
const STAR_GAP := 36.0
const ChestWords := preload("res://game/chest_lettering.gd")
const ACTION_CLEARANCE := 48.0
const REPLAY_INK := {"en": Rect2(207, 69, 1760, 590), "zh": Rect2(274, 180, 1161, 571)}

## Only the missing companion word is new. The accepted Home/Next words stay unchanged.
class ReplayWord extends TextureRect:
	var bounds: Rect2

	func refresh() -> void:
		var locale := UiText.locale()
		var ink: Rect2 = FamilyDialog.REPLAY_INK[locale]
		var cut := AtlasTexture.new()
		cut.atlas = Art.tex("label_dialog_replay_" + locale)
		cut.region = ink
		texture = cut
		var k := minf(bounds.size.x / ink.size.x, bounds.size.y / ink.size.y)
		size = ink.size * k
		position = bounds.get_center() - size * 0.5
		set_meta("visible_ink", Rect2(position, size))

	func _notification(what: int) -> void:
		if what == NOTIFICATION_TRANSLATION_CHANGED and bounds.has_area():
			refresh()

class Heading extends Control:
	var label_id: String
	var ordinal: String

	func refresh() -> void:
		for child in get_children():
			remove_child(child)
			child.queue_free()
		if ordinal.is_empty():
			ShopSkin.product_heading(self, label_id, Rect2(Vector2.ZERO, size))
			return
		var word_width := minf(440.0, UI.heading_width(label_id, size.y))
		var word := ShopSkin.product_heading(self, label_id, Rect2(0, 0, word_width, size.y))
		var ink := word.texture.get_image().get_used_rect()
		var ink_scale := word.size / word.texture.get_size()
		var word_ink := Rect2(word.position + Vector2(ink.position) * ink_scale, Vector2(ink.size) * ink_scale)
		# Ordinals belong to the title, not the cream reward-counter family.
		var number := Control.new()
		number.name = "TitleNumber"
		number.mouse_filter = Control.MOUSE_FILTER_IGNORE
		add_child(number)
		var cap := word_ink.size.y
		var x := 0.0
		for character in ordinal:
			var source := UiText.picture("number_gold_" + character)
			var region := source.get_image().get_used_rect()
			var glyph := TextureRect.new()
			var cut := AtlasTexture.new()
			cut.atlas = source
			cut.region = region
			glyph.texture = cut
			glyph.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
			glyph.stretch_mode = TextureRect.STRETCH_SCALE
			glyph.mouse_filter = Control.MOUSE_FILTER_IGNORE
			glyph.size = Vector2(region.size) * cap / region.size.y
			glyph.position.x = x
			number.add_child(glyph)
			x += glyph.size.x + cap * 0.06
		number.size = Vector2(x - cap * 0.06, cap)
		var left := (size.x - word_ink.size.x - 24.0 - number.size.x) * 0.5
		var offset := left - word_ink.position.x
		word.position.x += offset
		word.bounds.position.x += offset
		number.position = Vector2(left + word_ink.size.x + 24.0, word_ink.position.y)

	func _notification(what: int) -> void:
		if what == NOTIFICATION_TRANSLATION_CHANGED and not label_id.is_empty():
			refresh.call_deferred()

class Number extends Control:
	var value := ""

	func set_value(text: String) -> void:
		value = text
		queue_redraw()

	func _draw() -> void:
		var natural := FamilyDialog.number_width(value, size.y)
		var k := minf(1.0, size.x / maxf(1, natural))
		var x := (size.x - natural * k) * 0.5
		for character in value:
			var key := "icon_plus" if character == "+" else Island.DIGIT + character
			var ink := UI.used_rect(key)
			var h := size.y * (0.7 if character == "+" else 1.0)
			var w := ink.size.x / ink.size.y * h
			draw_texture_rect_region(Art.tex(key), Rect2(x, (size.y - h * k) * 0.5, w * k, h * k), ink)
			x += (w + size.y * 0.06) * k

static func number_width(value: String, cap: float) -> float:
	var width := 0.0
	for character in value:
		var key := "icon_plus" if character == "+" else Island.DIGIT + character
		var ink := UI.used_rect(key)
		width += ink.size.x / ink.size.y * cap * (0.7 if character == "+" else 1.0) + cap * 0.06
	return maxf(0.0, width - cap * 0.06)

static func number(parent: Node, value: String, bounds: Rect2) -> Number:
	var box := Number.new()
	box.name = "Number"
	box.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(box, bounds)
	box.value = value
	parent.add_child(box)
	return box

static func count(box: Number, amount: int, pattern: String, delay: float, duration: float) -> void:
	box.set_value(pattern % 0)
	var tween := box.create_tween()
	tween.tween_interval(delay)
	tween.tween_method(func(value: float): box.set_value(pattern % roundi(value)), 0.0, float(amount), duration)

static func decorate(dialog: Modal) -> void:
	# Keep Modal's content and total allocation, but end this opt-in frame before its actions.
	var previous_actions_y: float = dialog.get_meta("actions_y")
	dialog.panel_rect.size.y = previous_actions_y - Modal.ACTION_DROP + Modal.BOTTOM - dialog.panel_rect.position.y
	dialog.set_meta("actions_y", dialog.panel_rect.end.y + ACTION_CLEARANCE)
	DialogCartoonSkin.apply(dialog)
	var old_title := dialog.content.get_node("TitleGroup")
	dialog.content.remove_child(old_title)
	old_title.queue_free()
	var title := Heading.new()
	title.name = "TitleGroup"
	title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	title.label_id = dialog._title_id
	title.ordinal = dialog._title_number
	# This opt-in header owns locale layout; Modal must not restore its old wooden plank.
	dialog._title_number = ""
	UI.place(title, Rect2(200, dialog.panel_rect.position.y - 44, 680, 112))
	dialog.content.add_child(title)
	title.refresh()
	dialog._title_layer = title
	if dialog.content.has_node("Close"):
		var close := MenuNavigation.replace_close(dialog)
		close.position = Vector2(dialog.panel_rect.end.x - close.size.x,
			dialog.panel_rect.position.y - ACTION_CLEARANCE - close.size.y)

static func stars(parent: Control, y: float, filled: int, animate := false, delay := 0.0) -> void:
	for i in 3:
		var center := Vector2(540.0 + (i - 1) * (STAR_SIZE + STAR_GAP), y + STAR_ROW * 0.5)
		var bounds := Rect2(center - Vector2.ONE * STAR_SIZE * 0.5, Vector2.ONE * STAR_SIZE)
		var star := UI.art(parent, "island_star_full" if i < filled else "island_star_empty", bounds)
		star.name = "RatingStar%d" % i
		star.set_meta("visible_box", bounds)
		if animate and i < filled:
			star.scale = Vector2.ZERO
			var tween := star.create_tween()
			tween.tween_interval(delay + 0.18 * i)
			tween.tween_callback(Sound.play.bind("star_%d" % (i + 1)))
			tween.tween_property(star, "scale", Vector2.ONE * 1.25, 0.16).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
			tween.tween_property(star, "scale", Vector2.ONE, 0.2).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)

static func name_heading(parent: Node, id: String, y: float, height: float) -> void:
	ShopSkin.product_heading(parent, id, Rect2(164, y + (height - 76) * 0.5, 752, 76))

## Feather and painted amount have no bar, pill or backing image.
static func reward(parent: Control, bounds: Rect2, amount: int) -> Control:
	var group := Control.new()
	group.name = "Reward"
	group.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(group, bounds)
	parent.add_child(group)
	var cap := bounds.size.y * 0.54
	var icon := bounds.size.y * 0.76
	var gap := 20.0
	var width := minf(bounds.size.x - icon - gap, number_width("+%d" % amount, cap))
	var start := (bounds.size.x - icon - gap - width) * 0.5
	UI.art(group, "coin", Rect2(start, (bounds.size.y - icon) * 0.5, icon, icon)).name = "Feather"
	number(group, "+%d" % amount, Rect2(start + icon + gap, (bounds.size.y - cap) * 0.5, width, cap)).name = "Number"
	group.set_meta("coin_center", bounds.position + Vector2(start + icon * 0.5, bounds.size.y * 0.5))
	return group

## Keep the established primary/secondary sizes, ordering and callback lockout.
static func actions_height(items: Array) -> float:
	var primary := false
	var secondary := false
	for item in items:
		primary = primary or item[0] == "green"
		secondary = secondary or item[0] != "green"
	return (PRIMARY.y if primary else 0.0) + (SECONDARY.y if secondary else 0.0) + (Modal.ROW_STACK if primary and secondary else 0.0)

static func actions(dialog: Modal, items: Array) -> Array:
	var rows := [[], []]
	for i in items.size():
		rows[0 if items[i][0] == "green" else 1].append(i)
	var buttons := []
	buttons.resize(items.size())
	var y: float = dialog.get_meta("actions_y")
	for row in rows:
		if row.is_empty():
			continue
		var primary: bool = items[row[0]][0] == "green"
		var size: Vector2 = PRIMARY if primary else SECONDARY
		var width: float = size.x * row.size() + Modal.ACTION_GAP * (row.size() - 1)
		var x: float = 540.0 - width * 0.5
		for i in row:
			var item: Array = items[i]
			var callback: Callable = item[2]
			var id: String = item[3] if item.size() > 3 else Modal.ACTION_WORDS.get(item[1], "action.ok")
			var name: String = item[3] if item.size() > 3 else str(item[1]).capitalize()
			var button := UI.hit_area(dialog.content, Rect2(Vector2(x, y), size), func():
				Sound.play("ui_tap")
				dialog.invoke(callback))
			button.name = name
			var face := Island.nine(button, "home_play_plate", Rect2(Vector2.ZERO, size), Vector4(220, 100, 220, 100))
			face.name = "Face"
			var scale := size.y / Art.tex("home_play_plate").get_height()
			face.size = size / scale
			face.pivot_offset = Vector2.ZERO
			face.scale = Vector2.ONE * scale
			var word_box := Rect2(48, (size.y - 100) * 0.5, size.x - 96, 100)
			if id in ["action.play", "action.ok"]:
				ChestWords.word(button, id, word_box)
			else:
				var word: TextureRect
				if id == "action.replay":
					var replay := ReplayWord.new()
					replay.name = "ReplayWord"
					replay.bounds = word_box
					replay.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
					replay.stretch_mode = TextureRect.STRETCH_SCALE
					replay.mouse_filter = Control.MOUSE_FILTER_IGNORE
					replay.refresh()
					button.add_child(replay)
					word = replay
				else:
					word = UiText.painted(button, "white_action_next" if id == "action.next" else "white_settings_home", word_box)
				var paint := ShaderMaterial.new()
				paint.shader = preload("res://game/chest_play_word.gdshader")
				word.material = paint
			button.button_down.connect(func(): button.modulate = Island.PRESSED)
			button.button_up.connect(func(): button.modulate = Color.WHITE)
			buttons[i] = button
			x += size.x + Modal.ACTION_GAP
		y += size.y + Modal.ROW_STACK
	return buttons

## An unboxed ability line; size and center by the painted ink, including after locale changes.
class Ability extends Control:
	var label_id: String
	var effect: String

	func refresh() -> void:
		for child in get_children():
			remove_child(child)
			child.queue_free()
		var lead := 104.0 if not effect.is_empty() else 0.0
		var word := ShopSkin.product_word(self, label_id, Rect2(0, 37, size.x - lead, 54))
		var left := (size.x - word.size.x - lead) * 0.5
		word.position.x = left + lead
		word.bounds = Rect2(word.position, word.size)
		if not effect.is_empty():
			UI.art(self, "gift_coin_" + effect, Rect2(left, 22, 84, 84))

	func _notification(what: int) -> void:
		if what == NOTIFICATION_TRANSLATION_CHANGED and not label_id.is_empty():
			refresh.call_deferred()

static func ability(parent: Control, species: String, effect: String, y: float) -> void:
	var row := Ability.new()
	row.name = "Ability"
	row.label_id = "bird.%s.ability" % species
	row.effect = effect
	row.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(row, Rect2(172, y, 736, 128))
	parent.add_child(row)
	row.refresh()

static func stat(parent: Control, y: float, icon: String, value: String) -> Control:
	var cap := 64.0
	var width := number_width(value, cap)
	var x := 540.0 - (100.0 + width) * 0.5
	UI.icon(parent, icon, Vector2(x + 40.0, y + 50.0), 80.0, Color.WHITE)
	return number(parent, value, Rect2(x + 100.0, y + 18.0, width, cap))

static func halo(parent: Control, bounds: Rect2, instant: bool) -> TextureRect:
	var glow := UI.image(parent, "glow_soft", bounds.grow(bounds.size.x * 0.12))
	glow.modulate = Color(1.0, 0.86, 0.42, 0.85)
	glow.pivot_offset = glow.size * 0.5
	if not instant:
		var tween := glow.create_tween().set_loops()
		tween.tween_property(glow, "modulate:a", 0.55, 1.6).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
		tween.tween_property(glow, "modulate:a", 0.9, 1.6).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	return glow
