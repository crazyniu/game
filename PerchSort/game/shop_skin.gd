class_name ShopSkin
extends RefCounted
## One painted surface and fixed corner scale for every shop card and action.

const FACE := "btn_shop_tab"
const FACE_CONTENT := Rect2(53, 107, 1594, 717)
const CORNER_SCALE := 0.22
const MARGIN := 160
const GREEN := Color8(148, 218, 88)
const GOLD := Color8(255, 202, 66)
const PRICE := Color.WHITE
const COUNTER_INK := Color.WHITE
static var _face: AtlasTexture

## Approved forest shop surfaces are opt-in; existing non-shop consumers keep their own skin.
const FOREST_BACKGROUND := "background_shop_forest"
const PRODUCT_CONTENT := Rect2(37, 37, 1312, 1052)
const PRICE_CONTENT := Rect2(40, 33, 2111, 650)
const GREEN_CONTENT := Rect2(5, 5, 404, 109)
static var _product_textures: Dictionary = {}
static var _product_ink: ShaderMaterial

static func _product_surface(parent: Node, rect: Rect2, picture: String, content: Rect2,
		margins: Vector4, corner_scale: float) -> Control:
	if not _product_textures.has(picture):
		var texture := AtlasTexture.new()
		texture.atlas = Art.tex(picture)
		texture.region = content
		_product_textures[picture] = texture
	var holder := Control.new()
	holder.name = "Shape"
	holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(holder, rect)
	parent.add_child(holder)
	var skin := NinePatchRect.new()
	skin.name = "Skin"
	skin.mouse_filter = Control.MOUSE_FILTER_IGNORE
	skin.texture = _product_textures[picture]
	skin.patch_margin_left = int(margins.x)
	skin.patch_margin_top = int(margins.y)
	skin.patch_margin_right = int(margins.z)
	skin.patch_margin_bottom = int(margins.w)
	skin.size = rect.size / corner_scale
	skin.scale = Vector2.ONE * corner_scale
	holder.add_child(skin)
	return holder

static func product_card(parent: Node, rect: Rect2, selected := false) -> Control:
	var scale := 0.35 * minf(1.0, minf(rect.size.x / 480.0, rect.size.y / 392.0))
	var face := _product_surface(parent, rect, "panel_shop_forest", PRODUCT_CONTENT,
		Vector4(240, 300, 240, 260), scale)
	if selected:
		face.get_node("Skin").self_modulate = Color(1.15, 1.28, 1.06)
	return face

static func product_ink() -> ShaderMaterial:
	if _product_ink == null:
		_product_ink = ShaderMaterial.new()
		_product_ink.shader = preload("res://game/shop_label.gdshader")
	return _product_ink

static func product_word(parent: Node, label_id: String, bounds: Rect2) -> TextureRect:
	var label := UiText.painted_label(parent, label_id, bounds)
	label.material = product_ink()
	return label

static func product_heading(parent: Node, label_id: String, bounds: Rect2) -> TextureRect:
	return word(parent, label_id, bounds)

static func _product_action_face(parent: Node, rect: Rect2, _kind: String) -> Control:
	return UI.action_face(parent, rect)

static func product_price(parent: Node, rect: Rect2, value: int) -> Control:
	var face := _product_action_face(parent, rect, "gold")
	face.name = "Price"
	feather_amount(face, Rect2(Vector2(20, 10), rect.size - Vector2(40, 20)), value)
	return face

static func product_button(parent: Node, name: String, rect: Rect2, callback: Callable,
		kind := "gold") -> Button:
	var button := UI.hit_area(parent, rect, func():
		Sound.play("ui_tap")
		callback.call())
	button.name = name
	_product_action_face(button, Rect2(Vector2.ZERO, rect.size), kind)
	button.button_down.connect(func(): button.modulate = Island.PRESSED)
	button.button_up.connect(func(): button.modulate = Color.WHITE)
	return button

static func product_modal(host: Control, title_id: String, heights: Array, instant: bool,
		action_height := 0.0) -> Modal:
	var view := Modal.open(host, title_id, heights, true, action_height, 0.0, instant, false, "", false)
	for node_name in ["Panel", "TitleGroup"]:
		var old := view.content.get_node(node_name)
		view.content.remove_child(old)
		old.queue_free()
	var panel := product_card(view.content, view.panel_rect)
	panel.name = "Panel"
	view.content.move_child(panel, 0)
	var title := Control.new()
	title.name = "TitleGroup"
	title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(title, Rect2(200, view.panel_rect.position.y - 44, 680, 112))
	view.content.add_child(title)
	product_heading(title, title_id, Rect2(Vector2.ZERO, title.size))
	view._title_layer = title
	return view

static func card(parent: Node, rect: Rect2, selected := false) -> Control:
	if _face == null:
		_face = AtlasTexture.new()
		_face.atlas = Art.tex(FACE)
		_face.region = FACE_CONTENT
	var holder := Control.new()
	holder.name = "Shape"
	holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(holder, rect)
	parent.add_child(holder)
	var skin := NinePatchRect.new()
	skin.name = "Skin"
	skin.texture = _face
	skin.patch_margin_left = MARGIN
	skin.patch_margin_top = MARGIN
	skin.patch_margin_right = MARGIN
	skin.patch_margin_bottom = MARGIN
	skin.mouse_filter = Control.MOUSE_FILTER_IGNORE
	skin.size = rect.size / CORNER_SCALE
	skin.scale = Vector2.ONE * CORNER_SCALE
	skin.self_modulate = Color(0.87, 1.0, 0.72) if selected else Color.WHITE
	holder.add_child(skin)
	return holder

static func amount(parent: Node, rect: Rect2, value: int) -> Control:
	var digits := Island.digits(parent, str(value), rect, PRICE)
	var paint := ShaderMaterial.new()
	paint.shader = preload("res://game/chest_play_word.gdshader")
	for glyph in digits.get_children():
		if glyph is CanvasItem:
			glyph.material = paint
	return digits

## Currency icon and painted digits are centered by visible ink, not transparent padding.
static func feather_amount(parent: Node, rect: Rect2, value: int) -> Control:
	var group := Control.new()
	group.name = "FeatherPrice"
	group.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(group, rect)
	parent.add_child(group)
	var height := minf(rect.size.y, 80.0)
	var gap := height * 0.18
	var digits := amount(group, Rect2(0, 0, rect.size.x - height - gap, height), value)
	var visible := Rect2()
	var cursor := 0.0
	var tracking := height * 0.055
	for glyph in digits.get_children():
		var sprite := glyph as TextureRect
		var used := Rect2(sprite.texture.get_image().get_used_rect())
		var scale := sprite.size / sprite.texture.get_size()
		sprite.position.x = cursor - used.position.x * scale.x
		var ink := Rect2(sprite.position + used.position * scale, used.size * scale)
		cursor += ink.size.x + tracking
		visible = ink if visible.size == Vector2.ZERO else visible.merge(ink)
	var feather_width := height * 0.9
	var start := (rect.size.x - feather_width - gap - visible.size.x) * 0.5
	UI.art(group, "icon_purchase_feather", Rect2(start, (rect.size.y - height) * 0.5, feather_width, height)).name = "Feather"
	digits.position = Vector2(start + feather_width + gap - visible.position.x,
		(rect.size.y - visible.size.y) * 0.5 - visible.position.y)
	return group

static func price(parent: Node, rect: Rect2, value: int) -> Control:
	var holder := card(parent, rect)
	holder.name = "Price"
	holder.get_node("Skin").self_modulate = GOLD
	amount(holder, Rect2(Vector2(16, 12), rect.size - Vector2(32, 24)), value)
	return holder

static func button(parent: Node, name: String, rect: Rect2, callback: Callable, kind := "green") -> Button:
	var hit := UI.hit_area(parent, rect, func():
		Sound.play("ui_tap")
		callback.call())
	hit.name = name
	var face := card(hit, Rect2(Vector2.ZERO, rect.size))
	face.get_node("Skin").self_modulate = GREEN if kind == "green" else (GOLD if kind == "gold" else Color.WHITE)
	hit.button_down.connect(func(): face.modulate = Island.PRESSED)
	hit.button_up.connect(func(): face.modulate = Color.WHITE)
	return hit

static func plaque(parent: Node, title_id: String, bounds: Rect2) -> Control:
	var holder := Control.new()
	holder.name = "TitleGroup"
	holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(holder, bounds)
	parent.add_child(holder)
	UI.art(holder, "panel_shop_sign", Rect2(Vector2.ZERO, bounds.size), Rect2(18, 76, 2080, 610))
	word(holder, title_id, Rect2(54, 20, bounds.size.x - 108, bounds.size.y - 40))
	return holder

static func word(parent: Node, title_id: String, bounds: Rect2) -> TextureRect:
	var word := UiText.painted_label(parent, title_id, bounds, true)
	if not UiText.has_heading(title_id):
		var ink := ShaderMaterial.new()
		ink.shader = load("res://game/shop_heading.gdshader")
		ink.set_shader_parameter("dark_letters", true)
		word.material = ink
	return word

static func modal(host: Control, title_id: String, heights: Array, instant := false, action_height := 0.0) -> Modal:
	var view := Modal.open(host, title_id, heights, true, action_height, 0.0, instant, false, "", false)
	for name in ["Panel", "TitleGroup"]:
		var old := view.content.get_node(name)
		view.content.remove_child(old)
		old.queue_free()
	var panel := card(view.content, view.panel_rect)
	view.content.move_child(panel, 0)
	view._title_layer = plaque(view.content, title_id, Rect2(280, view.panel_rect.position.y - 56, 520, 132))
	return view

static func counter(parent: Node, profile: Profile, right := 1032.0, top := 48.0) -> FeatherBar:
	return FeatherBar.create_right(parent, Vector2(right, top), profile.coins, COUNTER_INK)
