class_name FamilyDialog
extends RefCounted
## Opt-in presentation for non-gameplay celebrations and the level card.

## Actions use the shop's button family (ButtonKit): green plates, painted white words.
const PRIMARY := Vector2(430, 162)     # ButtonKit.PRIMARY (literal: a const reference would cycle the scripts)
const SECONDARY := Vector2(346, 130)   # Primary 162 / 130 = 1.246: at most 1.25 times its siblings
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

static func decorate(dialog: Modal, _external_actions := true) -> void:
	# Keep Modal's content and total allocation, but end this opt-in frame before its actions.
	# Actions stay inside the card, as in the shop's purchase popups and the win panel.
	DialogCartoonSkin.apply(dialog)
	var old_title := dialog.content.get_node("TitleGroup")
	dialog.content.remove_child(old_title)
	old_title.queue_free()
	# The hanging wooden sign of the shop and purchase popups, its lettering painted in the shop style.
	var title := sign_title(dialog.content, dialog._title_id, dialog._title_number, dialog.panel_rect)
	dialog._title_number = ""
	dialog._title_layer = title
	if dialog.content.has_node("Close"):
		var old := dialog.content.get_node("Close")
		dialog.content.remove_child(old)
		old.queue_free()
		var at := Vector2(dialog.panel_rect.end.x - CLOSE_INSET.x - MenuNavigation.SIZE.x, dialog.panel_rect.position.y + CLOSE_INSET.y - PurchaseDialog.TITLE_ROOM)
		MenuNavigation.close(dialog.content, "Close", at, dialog.close)

## The painted lettering id of a title label id: bird and booster names and world names reuse the shop's paintings.
const BOOSTER_LETTERING := {"firefly": "firefly", "sapling": "sprout", "owl_eye": "owl_eye", "morning_bell": "wake_bell",
	"scarecrow": "scarecrow", "spring_rain": "spring_rain"}

static func lettering_key(label_id: String) -> String:
	var parts := label_id.split(".")
	if parts.size() == 3 and parts[0] == "bird" and parts[2] == "name":
		return "bird_" + parts[1]
	if parts.size() == 3 and parts[0] == "booster" and parts[2] == "name":
		return "item_" + BOOSTER_LETTERING.get(parts[1], parts[1])
	if parts.size() == 3 and parts[0] == "world" and parts[2] == "name":
		return "world_" + parts[1]
	return label_id.replace(".", "_")

## The sign's box relative to the card top (640 wide so the corner Close never touches it).
const SIGN_RECT := Rect2(220, -140, 640, 223)
const CLOSE_INSET := Vector2(14, 22)   # Close from the card's top-right corner, inside the card

## The sign straddles the card's top edge; an ordinal (the level number) follows the word in painted numerals.
static func sign_title(parent: Control, label_id: String, ordinal: String, card: Rect2) -> Control:
	var rect := SIGN_RECT
	rect.position.y += card.position.y
	if ordinal.is_empty():
		return ButtonKit.sign(parent, lettering_key(label_id), rect)
	var holder := ButtonKit.sign(parent, lettering_key(label_id), rect)
	var face := Rect2(rect.size * ButtonKit.SIGN_FACE.position, rect.size * ButtonKit.SIGN_FACE.size)
	var word: TextureRect = holder.get_child(1)
	var digit_h := face.size.y * TITLE_DIGIT_HEIGHT
	var digits_w := title_number_width(ordinal, digit_h)
	var word_box := Rect2(face.position, Vector2(face.size.x - digits_w - 16.0, face.size.y))
	word.bounds = word_box
	word.align = 1
	UiText._fit_painted(word)
	title_number(holder, ordinal, Rect2(face.end.x - digits_w, face.get_center().y - digit_h * 0.5, digits_w, digit_h)).name = "TitleNumber"
	return holder

## Title numerals: the gold, green-outlined digits painted in the title lettering's style (digit_title_0..9), so a
## number beside a title word reads as part of it.
const TITLE_DIGIT := "digit_title_"
const TITLE_DIGIT_HEIGHT := 0.92   # share of the title face height, level with the word's capitals
const TITLE_DIGIT_OVERLAP := 0.06  # neighbouring digits tuck their outlines together, as the letters do

static func title_number_width(text: String, height: float) -> float:
	var w := 0.0
	for c in text:
		var t := Art.tex(TITLE_DIGIT + c)
		w += t.get_width() * height / t.get_height() - height * TITLE_DIGIT_OVERLAP
	return maxf(0.0, w + height * TITLE_DIGIT_OVERLAP)

static func title_number(parent: Node, text: String, rect: Rect2) -> Control:
	var box := Control.new()
	box.mouse_filter = Control.MOUSE_FILTER_IGNORE
	box.set_meta("value", text)
	UI.place(box, rect)
	parent.add_child(box)
	var x := 0.0
	for c in text:
		var t := Art.tex(TITLE_DIGIT + c)
		var w := t.get_width() * rect.size.y / t.get_height()
		UI.image(box, TITLE_DIGIT + c, Rect2(x, 0, w, rect.size.y), false)
		x += w - rect.size.y * TITLE_DIGIT_OVERLAP
	return box

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
	ButtonKit.lettering(parent, lettering_key(id), Rect2(164, y + (height - 96) * 0.5, 752, 96))

## Feather and painted amount (ButtonKit numerals) have no bar, pill or backing image. "Number" carries "value".
static func reward(parent: Control, bounds: Rect2, amount: int) -> Control:
	var group := Control.new()
	group.name = "Reward"
	group.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(group, bounds)
	parent.add_child(group)
	var cap := bounds.size.y * 0.56
	var icon := bounds.size.y * 0.7
	var gap := 18.0
	var text := "+%d" % amount
	var width := minf(bounds.size.x - icon - gap, ButtonKit.number_width(text, cap))
	var start := (bounds.size.x - icon - gap - width) * 0.5
	UI.image(group, "icon_purchase_feather", Rect2(start, (bounds.size.y - icon) * 0.5, icon, icon), true).name = "Feather"
	digits(group, "Number", Rect2(start + icon + gap, (bounds.size.y - cap) * 0.5, width, cap), text)
	group.set_meta("coin_center", bounds.position + Vector2(start + icon * 0.5, bounds.size.y * 0.5))
	return group

## A holder of ButtonKit numerals whose "value" meta is the shown text; set_digits replaces them.
static func digits(parent: Control, n: String, rect: Rect2, text: String) -> Control:
	var holder := Control.new()
	holder.name = n
	holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(holder, rect)
	parent.add_child(holder)
	set_digits(holder, text)
	return holder

static func set_digits(holder: Control, text: String) -> void:
	if holder.get_meta("value", "") == text:
		return
	holder.set_meta("value", text)
	for child in holder.get_children():
		holder.remove_child(child)
		child.queue_free()
	ButtonKit.number(holder, text, Rect2(Vector2.ZERO, holder.size), 0)

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
			var button := ButtonKit.button(dialog.content, name, Rect2(Vector2(x, y), size), "green", func(): dialog.invoke(callback))
			var word_id: String = {"action.play": "white_play", "action.ok": "white_action_ok", "action.next": "white_action_next",
				"action.home": "white_settings_home"}.get(id, id)
			ButtonKit.word(button.get_node("Content"), word_id, size).name = "Word"
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
		# The shop's warm brown painted caption ("caption_bird_<id>_ability"), shown as painted.
		var species := label_id.split(".")[1]
		var word := UiText.painted(self, "caption_bird_%s_ability" % species, Rect2(0, 37, size.x - lead, 54))
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

static func stat(parent: Control, y: float, _icon: String, value: String) -> Control:
	var cap := 64.0
	var width := ButtonKit.number_width(value, cap)
	var x := 540.0 - (88.0 + width) * 0.5
	UI.art(parent, "icon_star_budget_steps", Rect2(x, y + 14.0, 72.0, 72.0), Rect2(317, 217, 652, 801)).name = "MovesIcon"
	return digits(parent, "Number", Rect2(x + 88.0, y + 18.0, width, cap), value)

static func halo(parent: Control, bounds: Rect2, instant: bool) -> TextureRect:
	var glow := UI.image(parent, "glow_soft", bounds.grow(bounds.size.x * 0.12))
	glow.modulate = Color(1.0, 0.86, 0.42, 0.85)
	glow.pivot_offset = glow.size * 0.5
	if not instant:
		var tween := glow.create_tween().set_loops()
		tween.tween_property(glow, "modulate:a", 0.55, 1.6).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
		tween.tween_property(glow, "modulate:a", 0.9, 1.6).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	return glow
