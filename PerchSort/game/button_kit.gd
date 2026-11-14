class_name ButtonKit
extends RefCounted
## One painted button family for every non-board screen, in the style of the home Play button.
## Faces are drawn at their native width / height ratio and only scaled uniformly; nothing is stretched.
##
## Size table (design pixels, 1080 x 1920):
##   WIDE       540 x 162   the purchase action at the bottom of a purchase card (feather and price only)
##   PRIMARY    430 x 162   the one main action of a page or popup (Play, OK); the home Play's shape, one step smaller
##   SECONDARY  288 x 108   repeated actions inside list and shop cards (price, Equip); never beside a Primary
##   KEY        130 x 130   square icon keys (minus, plus) beside a Primary or Wide: 162 / 130 = 1.25
## Buttons that sit beside a Primary are at most 1.25 times smaller (FamilyDialog.SECONDARY, Modal.SECONDARY_PILL).
##   TAB        196 x 192   square category tabs
##   FIELD      210 x 128   recessed value field (quantity); never pressable
## Text inside a face is cap = 0.48 x its height.

const WIDE := Vector2(540, 162)
const PRIMARY := Vector2(430, 162)
const SECONDARY := Vector2(288, 108)
const KEY := Vector2(130, 130)
const TAB := Vector2(196, 192)
const FIELD := Vector2(210, 128)
const CARD_ACTION_RATIO := 0.79
const CAP := 0.48
const PRESSED := Color(0.86, 0.86, 0.86)
const SINK := 0.95

const FACES := {
	"green": "btn_plate_green",
	"key_green": "btn_key_green",
	"key_cream": "btn_key_cream",
	"field": "field_quantity",
}

## Only standalone actions below a card use this width; paired actions and item tiles keep their sizes.
static func card_action_size(card_width: float, height := PRIMARY.y) -> Vector2:
	return Vector2(roundf(card_width * CARD_ACTION_RATIO), height)

## A painted face fitted uniformly into `rect` (centred); returns the TextureRect.
static func face(parent: Node, kind: String, rect: Rect2) -> Control:
	if kind == "wide":
		return _wide(parent, rect)
	var picture: String = FACES[kind]
	var tex := Art.tex(picture)
	var k := minf(rect.size.x / tex.get_width(), rect.size.y / tex.get_height())
	var shown := tex.get_size() * k
	var image := UI.image(parent, picture, Rect2(rect.get_center() - shown * 0.5, shown), false)
	image.name = "Face"
	image.pivot_offset = shown * 0.5
	return image

## The green plate made longer: its round ends keep their painted shape, only the plain middle extends.
static func _wide(parent: Node, rect: Rect2) -> Control:
	var tex := Art.tex(FACES["green"])
	var k := rect.size.y / tex.get_height()
	var holder := Control.new()
	holder.name = "Face"
	holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(holder, rect)
	holder.pivot_offset = rect.size * 0.5
	parent.add_child(holder)
	var skin := NinePatchRect.new()
	skin.name = "Skin"
	skin.mouse_filter = Control.MOUSE_FILTER_IGNORE
	skin.texture = tex
	skin.patch_margin_left = int(tex.get_height() * 0.6)
	skin.patch_margin_right = int(tex.get_height() * 0.6)
	skin.size = rect.size / k
	skin.scale = Vector2.ONE * k
	holder.add_child(skin)
	return holder

## The title banner: one light leaf-green glossy banner with leaf sprigs at both ends (banner_title_green), used for
## every page and popup title. Only its plain middle stretches; the leafy ends keep their painted shape. A title box
## keeps the old sign proportions: the banner fills its lower part (from BANNER_TOP) and the lettering sits on it.
const BANNER := "banner_title_green"
const BANNER_END := 400          # source pixels kept at each end (rounded end and leaves)
const BANNER_TOP := 0.30         # where the banner starts, as a share of the title box height
const SIGN_FACE := Rect2(0.10, 0.42, 0.80, 0.56)   # lettering area on the banner face, as fractions of the title box
static func sign(parent: Node, key: String, rect: Rect2) -> Control:
	var holder := Control.new()
	holder.name = "TitleGroup"
	holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(holder, rect)
	parent.add_child(holder)
	banner(holder, Rect2(0, rect.size.y * BANNER_TOP, rect.size.x, rect.size.y * (1.0 - BANNER_TOP)))
	lettering(holder, key, Rect2(rect.size * SIGN_FACE.position, rect.size * SIGN_FACE.size))
	return holder

## The banner alone, fitted to `rect` by its height; named "Sign".
static func banner(parent: Node, rect: Rect2) -> NinePatchRect:
	var tex := Art.tex(BANNER)
	var k := rect.size.y / tex.get_height()
	var plate := NinePatchRect.new()
	plate.name = "Sign"
	plate.mouse_filter = Control.MOUSE_FILTER_IGNORE
	plate.texture = tex
	var end := mini(BANNER_END, int(rect.size.x / k * 0.5) - 1)
	plate.patch_margin_left = end
	plate.patch_margin_right = end
	plate.position = rect.position
	plate.size = Vector2(rect.size.x / k, tex.get_height())
	plate.scale = Vector2.ONE * k
	plate.visible = false   # titles are lettering alone: no plate behind them
	parent.add_child(plate)
	return plate

## AI-painted lettering, one style per role (titles and names: yellow with a dark-green outline; captions: deep
## green), shown exactly as painted. Painted id "lettering_<key>" per locale; a missing word fails loudly.
static func lettering(parent: Node, key: String, bounds: Rect2) -> TextureRect:
	var word := UiText.painted(parent, "lettering_" + key, bounds)
	word.material = null
	return word

## A pressable button of one kind. Content goes into its child "Content" (same size as the button).
static func button(parent: Node, name: String, rect: Rect2, kind: String, callback: Callable) -> Button:
	var hit := UI.hit_area(parent, rect, func():
		Sound.play("ui_tap")
		callback.call())
	hit.name = name
	var shown := face(hit, kind, Rect2(Vector2.ZERO, rect.size))
	var content := Control.new()
	content.name = "Content"
	content.mouse_filter = Control.MOUSE_FILTER_IGNORE
	content.size = rect.size
	content.pivot_offset = rect.size * 0.5
	hit.add_child(content)
	hit.button_down.connect(func():
		shown.modulate = PRESSED
		shown.scale = Vector2.ONE * SINK
		content.scale = Vector2.ONE * SINK)
	hit.button_up.connect(func():
		shown.modulate = Color.WHITE
		shown.scale = Vector2.ONE
		content.scale = Vector2.ONE)
	return hit

## A non-pressable face of a button kind (used for owned counts and equipped states).
static func plate(parent: Node, rect: Rect2, kind := "green") -> Control:
	var holder := Control.new()
	holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(holder, rect)
	parent.add_child(holder)
	face(holder, kind, Rect2(Vector2.ZERO, rect.size))
	return holder

## Painted white word (a painted id or a label id) on a face, sized by the face height.
static func word(parent: Node, label_id: String, size: Vector2) -> TextureRect:
	var cap := size.y * CAP
	var bounds := Rect2(size.x * 0.16, (size.y - cap * 1.25) * 0.5, size.x * 0.68, cap * 1.25)
	var label := UiText.painted(parent, label_id, bounds) if UiText.has_painted(label_id) else UiText.painted_label(parent, label_id, bounds)
	var paint := ShaderMaterial.new()
	paint.shader = preload("res://game/chest_play_word.gdshader")
	label.material = paint
	return label

## Painted numerals for every price, count and quantity: white with a dark-green outline (digit_ui_*).
const DIGIT := "digit_ui_"
const GLYPHS := {"/": "slash", "+": "plus", "x": "x", "×": "x"}
const TRACK := 0.02    # glyph spacing as a share of the height

static func glyph(character: String) -> String:
	return DIGIT + GLYPHS.get(character, character)

static func number_width(text: String, height: float) -> float:
	var w := 0.0
	for c in text:
		var t := Art.tex(glyph(c))
		w += t.get_width() * height / t.get_height() + height * TRACK
	return maxf(0.0, w - height * TRACK)

## Numerals fitted into `rect` (height first, shrunk to fit the width), centred; align -1 left, 0 centre, 1 right.
static func number(parent: Node, text: String, rect: Rect2, align := 0) -> Control:
	var box := Control.new()
	box.name = "Digits"
	box.mouse_filter = Control.MOUSE_FILTER_IGNORE
	box.set_meta("value", text)
	UI.place(box, rect)
	parent.add_child(box)
	var h := minf(rect.size.y, rect.size.y * rect.size.x / maxf(1.0, number_width(text, rect.size.y)))
	var x := (rect.size.x - number_width(text, h)) * (0.5 + 0.5 * align)
	var y := (rect.size.y - h) * 0.5
	for c in text:
		var t := Art.tex(glyph(c))
		var w := t.get_width() * h / t.get_height()
		UI.image(box, glyph(c), Rect2(x, y, w, h), false)
		x += w + h * TRACK
	return box

static func set_number(box: Control, text: String) -> void:
	var parent := box.get_parent()
	var rect := Rect2(box.position, box.size)
	var align: int = box.get_meta("align", 0)
	var at := box.get_index()
	parent.remove_child(box)
	box.queue_free()
	var fresh := number(parent, text, rect, align)
	fresh.set_meta("align", align)
	parent.move_child(fresh, at)

## Feather icon and painted price, centred on a face.
static func price(parent: Node, size: Vector2, value: int) -> Control:
	var h := size.y * (0.58 if size.x > PRIMARY.x else 0.52)
	var group := Control.new()
	group.name = "FeatherPrice"
	group.mouse_filter = Control.MOUSE_FILTER_IGNORE
	group.size = size
	parent.add_child(group)
	var text := str(value)
	var feather := h * 0.95
	var gap := h * 0.16
	var digits_w := minf(number_width(text, h), size.x * 0.72 - feather - gap)
	var start := (size.x - feather - gap - digits_w) * 0.5
	var mid := size.y * 0.47
	UI.image(group, "icon_purchase_feather", Rect2(start, mid - feather * 0.5, feather, feather), true).name = "Feather"
	var digits := number(group, text, Rect2(start + feather + gap, mid - h * 0.5, digits_w, h), -1)
	var ink := _ink(group.get_node("Feather"))
	for g in digits.get_children():
		ink = ink.merge(_ink(g, digits.position))
	var shift := size * 0.5 - ink.get_center()
	for child in group.get_children():
		child.position += shift
	return group

## Visible pixels of a fitted picture, in its parent's coordinates (plus `offset`).
static func _ink(sprite: TextureRect, offset := Vector2.ZERO) -> Rect2:
	var tex := sprite.texture
	var used := UI.used_rect(tex.resource_path.get_file().get_basename())
	var k := minf(sprite.size.x / tex.get_width(), sprite.size.y / tex.get_height())
	var shown := tex.get_size() * k
	var origin := sprite.position + (sprite.size - shown) * 0.5 + offset
	return Rect2(origin + used.position * k, used.size * k)

## The cream card surface: painted panel with fixed corners.
const PANEL := "panel_card_cream"
const PANEL_MARGIN := 120
const PANEL_SCALE := 0.42
static func panel(parent: Node, rect: Rect2) -> Control:
	var holder := Control.new()
	holder.name = "Panel"
	holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(holder, rect)
	parent.add_child(holder)
	var skin := NinePatchRect.new()
	skin.name = "Skin"
	skin.mouse_filter = Control.MOUSE_FILTER_IGNORE
	skin.texture = Art.tex(PANEL)
	for side in [SIDE_LEFT, SIDE_TOP, SIDE_RIGHT, SIDE_BOTTOM]:
		skin.set_patch_margin(side, PANEL_MARGIN)
	skin.size = rect.size / PANEL_SCALE
	skin.scale = Vector2.ONE * PANEL_SCALE
	holder.add_child(skin)
	return holder
