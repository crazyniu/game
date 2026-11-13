class_name ButtonKit
extends RefCounted
## One painted button family for every non-board screen, in the style of the home Play button.
## Faces are drawn at their native width / height ratio and only scaled uniformly; nothing is stretched.
##
## Size table (design pixels, 1080 x 1920):
##   WIDE       520 x 132   the purchase action at the bottom of a purchase card (feather and price only)
##   PRIMARY    360 x 136   the one main action of a page or popup (Play, OK)
##   SECONDARY  288 x 108   repeated actions in lists and cards (price, Equip, owned count); PRIMARY / 1.25
##   KEY        108 x 108   square icon keys (minus, plus)
##   TAB        196 x 192   square category tabs
##   FIELD      184 x 106   recessed value field (quantity); never pressable
## Text inside a face is cap = 0.48 x its height.

const WIDE := Vector2(520, 132)
const PRIMARY := Vector2(360, 136)
const SECONDARY := Vector2(288, 108)
const KEY := Vector2(108, 108)
const TAB := Vector2(196, 192)
const FIELD := Vector2(184, 106)
const CAP := 0.48
const PRESSED := Color(0.86, 0.86, 0.86)
const SINK := 0.95

const FACES := {
	"green": "btn_plate_green",
	"key_green": "btn_key_green",
	"key_cream": "btn_key_cream",
	"field": "field_quantity",
}

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
	var tex := Art.tex("btn_plate_green")
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

## The hanging wooden title sign (rope loops on a leafy branch) with a gold painted heading on its plank.
const SIGN := "sign_hanging"
const SIGN_FACE := Rect2(0.10, 0.37, 0.80, 0.60)   # plank face, as fractions of the sign
static func sign(parent: Node, key: String, rect: Rect2) -> Control:
	var holder := Control.new()
	holder.name = "TitleGroup"
	holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(holder, rect)
	parent.add_child(holder)
	UI.image(holder, SIGN, Rect2(Vector2.ZERO, rect.size), false).name = "Sign"
	lettering(holder, key, Rect2(rect.size * SIGN_FACE.position, rect.size * SIGN_FACE.size))
	return holder

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

## Feather icon and painted price, centred on a face.
static func price(parent: Node, size: Vector2, value: int) -> Control:
	var cap := size.y * (0.58 if size.x > PRIMARY.x else CAP)
	return ShopSkin.feather_amount(parent, Rect2(size.x * 0.14, (size.y - cap * 1.1) * 0.5, size.x * 0.72, cap * 1.1), value)

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
