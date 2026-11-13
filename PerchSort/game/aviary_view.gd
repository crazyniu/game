extends Control
## Open leafy perches keep the live birds prominent over the turquoise forest. The standalone list
## opens bird details; the shop opens those same details directly and owns their Back destination.

signal closed
signal profile_changed

## Two equal columns float directly over the forest, below the compact header.
const CLIP_TOP := 232.0
const CLIP_X := 48.0
const CLIP_W := 960.0
const CLIP_HEIGHT := 1592.0
const GRID_TOP := 0.0
const GRID_BOTTOM := 64.0 # Include the name/lock ink below CARD_H and breathing room.
const GAP := 32.0
const CARD_W := 464.0
const CARD_H := 468.0
const CARD_BIRD := Rect2(52, 8, 360, 356)
const CARD_NAME := Rect2(38, 400, 388, 80)
const CARD_PERCH_WIDTH := 416.0
const TRACK_X := 1036.0
const TRACK_W := 18.0
const THUMB_MIN := 96.0
const DRAG_START := 12.0
const FRICTION := 5.0             # fling velocity decay per second (exponential)
const HEADER_CENTER_Y := 119.0
const CLOSE := Rect2(48, HEADER_CENTER_Y - 56, 112, 112)
const TITLE_SIGN := Rect2(300, 32, 480, 174)
const DETAIL_TITLE_SIGN := Rect2(308, 32, 464, 174)
const TILE_GAP := 24.0
const TILE_COLS := 3
const SHOP_BIRD := Rect2(336, 212, 408, 432)
const HERO_PERCH_WIDTH := 640.0
const SHOP_TILE_SIZE := Vector2(328, 400)
const SHOP_TILE_BIRD := Rect2(28, 0, 272, 280)
const SHOP_TILE_ACTION := Rect2(32, 306, 264, 88)
const SHOP_TILES := Rect2(24, 940, 1032, 768)
const TILE_PERCH_WIDTH := 328.0
const PERCH_CONTACT_Y := 140.0     # centre wood contact in branch_aviary_perch's padded source pixels
const SILHOUETTE := """shader_type canvas_item;
void fragment() { COLOR = vec4(0.25, 0.19, 0.13, texture(TEXTURE, UV).a * 0.88); }"""

var profile: Profile
var instant := false        # set by the shop for captures; the aviary itself asks its parent
var _from_shop := false
var _coins: Control
var _page: Control
var _clip: Control
var _content: Control
var _thumb: Panel
var _track: Rect2
var _cards: Array = []      # [Control card, species id] in content coordinates
var _scroll := 0.0
var _max_scroll := 0.0
var _velocity := 0.0
var _press := Vector2.ZERO
var _pressed := false
var _dragging := false
var _list_scroll := 0.0
var _species := ""
var _backdrop: TextureRect
var _hero: BirdView
var _hero_display: Control
var _hero_box := Rect2()
var _tiles: Control
var _tile_row := Rect2()


func setup(p: Profile, species := "") -> void:
	profile = p
	_from_shop = not species.is_empty()
	size = Vector2(1080, 1920)
	_backdrop = UI.image(self, "background_aviary_forest", Rect2(0, 0, 1080, 1920))
	var calm := ShaderMaterial.new()   # calmed like the board painting so the birds read first
	calm.shader = preload("res://game/calm_backdrop.gdshader")
	_backdrop.material = calm
	_backdrop.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_COVERED
	if _from_shop:
		_show_detail(species)
	else:
		_show_list()

func _process(delta: float) -> void:
	if _content == null or not is_instance_valid(_content) or _pressed or absf(_velocity) < 4.0:
		return
	_set_scroll(_scroll + _velocity * delta)
	_velocity *= exp(-FRICTION * delta)
	if _scroll <= 0.0 or _scroll >= _max_scroll:
		_velocity = 0.0

func _notification(what: int) -> void:
	if what == NOTIFICATION_APPLICATION_FOCUS_OUT:
		_pressed = false
		_velocity = 0.0
	if what == NOTIFICATION_TRANSLATION_CHANGED and is_node_ready():
		if _species.is_empty():
			_show_list.call_deferred()
		else:
			_show_detail.call_deferred(_species)

# ---------------------------------------------------------------- shared pieces

func _reset_page() -> void:
	if _page:
		remove_child(_page)
		_page.queue_free()
	_page = Control.new()
	_page.name = "Page"
	_page.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_page.size = size
	add_child(_page)
	_cards.clear()
	_content = null
	_velocity = 0.0
	_pressed = false

## Fit the front frame above the BirdView foot origin so costumes keep the same perch contact.
func _live_bird(parent: Node, species: String, outfit: String, box: Rect2) -> BirdView:
	var front := outfit + "_front" if not outfit.is_empty() and Art.has(outfit + "_front") else species + "_front"
	var used := UI.used_rect(front)
	var k := minf(box.size.x / used.size.x, box.size.y / (BirdView.FEET_Y - used.position.y))
	var bird: BirdView = BirdView.new()
	bird.position = Vector2(box.get_center().x - (used.get_center().x - BirdView.CANVAS * 0.5) * k,
		box.end.y)
	parent.add_child(bird)
	bird.setup(Board.bird(0, Board.species_index(species), 0), false, k * BirdView.CANVAS / BirdView.DISPLAY, outfit)
	if _is_instant():
		bird.freeze()
	return bird

## The complete production branch keeps its leaves and native ratio, with wood meeting the feet.
func _perch(parent: Node, contact: Vector2, width: float) -> TextureRect:
	var k := width / Art.tex("branch_aviary_perch").get_width()
	return Island.piece(parent, "branch_aviary_perch", contact - Vector2(width * 0.5, PERCH_CONTACT_Y * k), width)

## Dark breathing silhouette of a species fitted into the box (unmet birds); the shop's outfits tab shares it.
static func _silhouette(parent: Node, species: String, box: Rect2, quiet := false) -> TextureRect:
	var front := species + "_front"
	var used := UI.used_rect(front)
	var k := minf(box.size.x / used.size.x, box.size.y / used.size.y)
	var fitted := used.size * k
	var sil := UI.art(parent, front, Rect2(Vector2(box.get_center().x - fitted.x * 0.5, box.end.y - fitted.y), fitted))
	var shader := Shader.new()
	shader.code = SILHOUETTE
	var mat := ShaderMaterial.new()
	mat.shader = shader
	sil.material = mat
	sil.pivot_offset = Vector2(box.get_center().x, box.end.y) - sil.position
	if quiet:
		return sil
	var tw := sil.create_tween().set_loops()
	tw.tween_property(sil, "scale", Vector2(0.992, 1.02), 1.4).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	tw.tween_property(sil, "scale", Vector2.ONE, 1.4).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	return sil

func _outfit_key(species: String) -> String:
	var id: String = profile.equipped.get(species, "")
	return "" if id.is_empty() else "%s_outfit_%s" % [species, id.get_slice(".", 2)]

## Feedback changes tint only, keeping every card and its bird at the approved size.
func _bump(c: Control) -> void:
	if _is_instant():
		return
	c.modulate = Island.PRESSED
	var tw := c.create_tween()
	tw.tween_property(c, "modulate", Color.WHITE, 0.2)

# ---------------------------------------------------------------- list

func _show_list() -> void:
	_species = ""
	_reset_page()
	_clip = Control.new()
	_clip.name = "Scroll"
	_clip.clip_contents = true
	UI.place(_clip, Rect2(CLIP_X, CLIP_TOP, CLIP_W, CLIP_HEIGHT))
	_clip.mouse_filter = Control.MOUSE_FILTER_STOP
	_clip.gui_input.connect(_on_scroll_input)
	_page.add_child(_clip)
	_content = Control.new()
	_content.name = "Grid"
	_content.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_clip.add_child(_content)
	var count := Aviary.SPECIES.size()
	var met := 0
	for i in count:
		var id: String = Aviary.SPECIES[i][0]
		if profile.has_met(id):
			met += 1
		_card(id, Vector2((i % 2) * (CARD_W + GAP), GRID_TOP + (i / 2) * (CARD_H + GAP)))
	var rows := ceili(count / 2.0)
	_content.size = Vector2(CLIP_W, GRID_TOP + rows * (CARD_H + GAP) - GAP + GRID_BOTTOM)
	_max_scroll = maxf(0.0, _content.size.y - _clip.size.y)
	_build_track()
	_list_header(met, count)
	_set_scroll(_list_scroll)

func _list_header(met: int, count: int) -> void:
	var close := MenuNavigation.close(_page, "Close", CLOSE.position, func(): closed.emit())
	_align_navigation(close)
	_title("aviary.title", TITLE_SIGN)
	var chip := Control.new()
	chip.name = "MetCounter"
	chip.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(chip, Rect2(784, HEADER_CENTER_Y - 40, 248, 80))
	_page.add_child(chip)
	UI.kit_piece(chip, "hud_bar", Rect2(0, 0, 248, 80))
	UI.art(chip, "fall_leaf_green", Rect2(0, 0, 80, 80))
	ButtonKit.number(chip, "%d/%d" % [met, count], Rect2(80, 12, 152, 56)).name = "Count"

func _title(label_id: String, bounds: Rect2) -> void:
	# The shared light-green title banner (ButtonKit), its lettering on the banner face.
	# The banner is centred on the header line, level with Back / Close and the coin counter.
	var h := bounds.size.y * (1.0 - ButtonKit.BANNER_TOP)
	var group := Control.new()
	group.name = "TitleGroup"
	group.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(group, Rect2(bounds.position.x, HEADER_CENTER_Y - h * 0.5, bounds.size.x, h))
	_page.add_child(group)
	ButtonKit.banner(group, Rect2(Vector2.ZERO, group.size))
	var face := Rect2(group.position + group.size * Vector2(0.12, 0.17), group.size * Vector2(0.76, 0.76))
	var key := "aviary" if label_id == "aviary.title" else label_id.replace(".name", "").replace(".", "_")
	ButtonKit.lettering(_page, key, face).name = "TitleWord"

## Transparent hit regions retain list dragging while the birds sit on independent leafy perches.
func _card(id: String, at: Vector2) -> void:
	var met := profile.has_met(id)
	var card := Control.new()
	card.name = "Card_" + id
	card.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(card, Rect2(at, Vector2(CARD_W, CARD_H)))
	_content.add_child(card)
	var bird_box := Rect2(CARD_BIRD.position, CARD_BIRD.size)
	if met:
		_perch(card, Vector2(bird_box.get_center().x, bird_box.end.y), CARD_PERCH_WIDTH)
		_live_bird(card, id, _outfit_key(id), bird_box)
		var name_id := "bird.%s.name" % id
		ButtonKit.lettering(card, "bird_" + id, CARD_NAME).name = "Name"
	else:
		_perch(card, Vector2(bird_box.get_center().x, bird_box.end.y), CARD_PERCH_WIDTH)
		var silhouette := _silhouette(card, id, bird_box, _is_instant())
		var shader := Shader.new()
		shader.code = """shader_type canvas_item;
void fragment() {
	float a = texture(TEXTURE, UV).a;
	vec2 step_size = TEXTURE_PIXEL_SIZE * 3.0;
	float rim = max(max(texture(TEXTURE, UV + vec2(step_size.x, 0)).a,
		texture(TEXTURE, UV - vec2(step_size.x, 0)).a),
		max(texture(TEXTURE, UV + vec2(0, step_size.y)).a,
		texture(TEXTURE, UV - vec2(0, step_size.y)).a));
	COLOR = vec4(mix(vec3(0.60, 1.0, 0.91), vec3(0.02, 0.34, 0.31), a), max(a, rim * 0.85));
}"""
		var material := ShaderMaterial.new()
		material.shader = shader
		silhouette.material = material
		UI.art(card, "icon_aviary_question", Rect2(bird_box.get_center() - Vector2(52, 32), Vector2(104, 116))).name = "Question"
		Island.piece_fit(card, "island_padlock", Vector2(CARD_W * 0.5, 450), Vector2(72, 80)).name = "Lock"
	_cards.append([card, id])

func _build_track() -> void:
	var visible_h := _clip.size.y - GRID_BOTTOM
	_track = Rect2(TRACK_X, CLIP_TOP + GRID_TOP, TRACK_W, visible_h - GRID_TOP)
	var track := _pill(_track, Color(0.13, 0.65, 0.55, 0.5))
	track.name = "Track"
	_thumb = _pill(Rect2(_track.position, Vector2(TRACK_W + 14.0, THUMB_MIN)), Color8(255, 195, 34))
	_thumb.name = "Thumb"
	var show := _max_scroll > 0.0
	track.visible = show
	_thumb.visible = show

func _pill(r: Rect2, color: Color) -> Panel:
	var p := Panel.new()
	var style := StyleBoxFlat.new()
	style.bg_color = color
	style.border_color = Color(1.0, 0.97, 0.78, 0.9)
	style.set_border_width_all(3)
	style.set_corner_radius_all(int(r.size.x * 0.5))
	style.anti_aliasing = true
	p.add_theme_stylebox_override("panel", style)
	p.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(p, r)
	_page.add_child(p)
	return p

func _set_scroll(value: float) -> void:
	_scroll = clampf(value, 0.0, _max_scroll)
	_list_scroll = _scroll
	_content.position.y = -_scroll
	var h := THUMB_MIN
	_thumb.size.y = h
	_thumb.position.x = _track.position.x - 7.0
	_thumb.position.y = _track.position.y + (_track.size.y - h) * (_scroll / maxf(_max_scroll, 1.0))

func _on_scroll_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		var e := event as InputEventMouseButton
		if e.pressed and e.button_index in [MOUSE_BUTTON_WHEEL_UP, MOUSE_BUTTON_WHEEL_DOWN]:
			_velocity += (-1.0 if e.button_index == MOUSE_BUTTON_WHEEL_UP else 1.0) * 900.0
		elif e.button_index == MOUSE_BUTTON_LEFT:
			if e.pressed:
				_pressed = true
				_dragging = false
				_velocity = 0.0
				_press = e.position
			elif _pressed:
				_pressed = false
				if not _dragging:
					_tap(e.position + Vector2(0, _scroll))
	elif event is InputEventMouseMotion and _pressed:
		var m := event as InputEventMouseMotion
		if _dragging or absf(m.position.y - _press.y) > DRAG_START:
			_dragging = true
			_set_scroll(_scroll - m.relative.y)
			# Smoothed release speed for the fling.
			_velocity = lerpf(_velocity, -m.velocity.y, 0.5)

func _tap(point: Vector2) -> void:
	for entry in _cards:
		var card: Control = entry[0]
		if not Rect2(card.position, card.size).has_point(point):
			continue
		if profile.has_met(entry[1]):
			Sound.play("ui_tap")
			_show_detail(entry[1])
		return

# ---------------------------------------------------------------- detail

func _show_detail(id: String) -> void:
	_species = id
	_reset_page()
	var crow := id == "crow"
	_hero_display = Control.new()
	_hero_display.name = "BirdDisplay"
	_hero_display.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_hero_display.size = size
	_page.add_child(_hero_display)
	var glow := UI.art(_hero_display, "glow_soft", Rect2(232, 204, 616, 472))
	glow.modulate.a = 0.48
	_hero_box = SHOP_BIRD
	_perch(_hero_display, Vector2(_hero_box.get_center().x, _hero_box.end.y), HERO_PERCH_WIDTH)
	_hero = _live_bird(_hero_display, id, _outfit_key(id), _hero_box)
	var timer := Timer.new()
	timer.wait_time = 5.0
	timer.autostart = true
	timer.timeout.connect(func(): if is_instance_valid(_hero) and not _hero.acting and not _hero.showcasing: _hero.cheer())
	_page.add_child(timer)
	UI.hit_area(_page, _hero_box, func(): if is_instance_valid(_hero): _hero.showcase())
	UI.art(_page, "panel_aviary_ability", Rect2(232, 714, 616, 104)).name = "AbilityStrip"
	var ability := UiText.painted_label(_page, "bird.%s.ability" % id, Rect2(304, 740, 472, 52))
	ability.name = "AbilityWord"
	_center_sprite(ability, Rect2(304, 740, 472, 52))
	var ability_ink := UiText.warm_ink().duplicate() as ShaderMaterial
	ability_ink.set_shader_parameter("top_color", Color8(137, 71, 20))
	ability_ink.set_shader_parameter("bottom_color", Color8(101, 47, 9))
	ability.material = ability_ink
	if not crow:
		_tile_row = SHOP_TILES
		_tiles = Control.new()
		_tiles.name = "Outfits"
		_tiles.mouse_filter = Control.MOUSE_FILTER_IGNORE
		_page.add_child(_tiles)
		_build_tiles()
		if not _from_shop:
			ButtonKit.lettering(_page, "tab_outfits", Rect2(340, 832, 400, 88)).name = "OutfitsHeading"
	var back := MenuNavigation.back(_page, "Back", CLOSE.position, _back)
	_align_navigation(back)
	_title("bird.%s.name" % id, DETAIL_TITLE_SIGN)
	_build_coins()

func _back() -> void:
	if _from_shop:
		closed.emit()
	else:
		_show_list()

func _is_instant() -> bool:
	return instant or (get_parent() != null and get_parent().has_method("instant") and get_parent().instant())

## Detail pages show the same compact balance counter, rebuilt after a purchase.
func _build_coins() -> void:
	if _coins and is_instance_valid(_coins):
		_coins.get_parent().remove_child(_coins)
		_coins.queue_free()
	_coins = null
	_coins = ShopSkin.counter(_page, profile, 1056.0)
	var panel := _coins.get_child(0) as TextureRect
	var visible := _sprite_ink(panel)
	_coins.position.y = HEADER_CENTER_Y - visible.get_center().y
	_center_digits(_coins.get_node("Digits").get_child(0))

## Six open perch positions: plain bird, then outfits 1 to 5. Owned ones equip on tap; locked ones
## retain full-color art with locks and prices. Shared action faces label Equip/Equipped explicitly.
func _build_tiles() -> void:
	for c in _tiles.get_children():
		_tiles.remove_child(c)   # rebuilt tiles keep their names
		c.queue_free()
	var tile_size := SHOP_TILE_SIZE
	var bird_box := SHOP_TILE_BIRD
	var x := _tile_row.position.x + (_tile_row.size.x - (tile_size.x * TILE_COLS + TILE_GAP * (TILE_COLS - 1))) * 0.5
	var current: String = profile.equipped.get(_species, "outfit.%s.default" % _species)
	for n in Shop.OUTFIT_COUNT + 1:
		var id := "outfit.%s.default" % _species if n == 0 else "outfit.%s.%d" % [_species, n]
		var owned := n == 0 or profile.owns_outfit(id)
		var tile := Control.new()
		tile.name = "Tile_%d" % n
		tile.mouse_filter = Control.MOUSE_FILTER_IGNORE
		UI.place(tile, Rect2(Vector2(x + (n % TILE_COLS) * (tile_size.x + TILE_GAP), _tile_row.position.y + (n / TILE_COLS) * (tile_size.y + TILE_GAP)), tile_size))
		_tiles.add_child(tile)
		_perch(tile, Vector2(bird_box.get_center().x, bird_box.end.y), TILE_PERCH_WIDTH)
		var outfit := "" if n == 0 else "%s_outfit_%d" % [_species, n]
		_live_bird(tile, _species, outfit, bird_box)
		_tile_action(tile, owned, id == current, 0 if owned else Shop.item(id)["price"])
		var hit_bounds := Rect2(Vector2.ZERO, Vector2(tile.size.x, maxf(tile.size.y, SHOP_TILE_ACTION.end.y)))
		if owned:
			var pick := id
			var hit := UI.hit_area(tile, hit_bounds, func(): _equip(pick, n))
			hit.name = "Equip"
			hit.disabled = id == current
		else:
			var lock_center := Vector2(tile_size.x - 40.0, 40.0)
			var lock_size := Vector2(56, 56)
			Island.piece_fit(tile, "island_padlock", lock_center, lock_size).name = "Lock"
			var pick_locked := id
			var hit := UI.hit_area(tile, hit_bounds, func(): _confirm_outfit(pick_locked, n, tile))
			hit.name = "Locked"
		var tap_area: Button = tile.get_node("Equip" if owned else "Locked")
		tap_area.button_down.connect(func(): tile.modulate = Island.PRESSED)
		tap_area.button_up.connect(func(): tile.modulate = Color.WHITE)
		if id == current:
			Island.check_badge(tile, Vector2(tile_size.x - 40.0, 40.0)).name = "Equipped"

## Shared shop skins keep state and price legible without an empty marker or overhanging coin.
func _tile_action(tile: Control, owned: bool, selected: bool, price: int) -> void:
	var face := ButtonKit.plate(tile, SHOP_TILE_ACTION)
	face.name = "Status" if owned else "Price"
	if owned:
		ButtonKit.word(face, "white_equipped" if selected else "white_equip", SHOP_TILE_ACTION.size)
	else:
		ButtonKit.price(face, SHOP_TILE_ACTION.size, price)

## Keep this correction local: other accepted screens retain their original materials and glyph layout.
static var _action_material: ShaderMaterial
static var _visible_ink: Dictionary = {}

static func _action_ink() -> ShaderMaterial:
	if _action_material == null:
		var shader := Shader.new()
		shader.code = """shader_type canvas_item;
varying vec4 item_tint;
void vertex() { item_tint = COLOR; }
void fragment() {
	vec4 paint = texture(TEXTURE, UV);
	float light = dot(paint.rgb, vec3(0.299, 0.587, 0.114));
	float face = smoothstep(0.48, 0.64, light);
	COLOR = vec4(paint.rgb, paint.a * face) * item_tint;
}"""
		_action_material = ShaderMaterial.new()
		_action_material.shader = shader
	return _action_material

## Measure the visible result, including the local material's removed dark surround.
static func _texture_ink(texture: Texture2D, soften := false) -> Rect2:
	var key := str(texture.get_instance_id()) + ("soft" if soften else "painted")
	if not _visible_ink.has(key):
		var image := texture.get_image().duplicate() as Image
		if soften:
			for y in image.get_height():
				for x in image.get_width():
					var pixel := image.get_pixel(x, y)
					var light := pixel.r * 0.299 + pixel.g * 0.587 + pixel.b * 0.114
					pixel.a *= smoothstep(0.48, 0.64, light)
					image.set_pixel(x, y, pixel)
		_visible_ink[key] = Rect2(image.get_used_rect())
	return _visible_ink[key]

static func _sprite_ink(sprite: TextureRect, soften := false) -> Rect2:
	var ink := _texture_ink(sprite.texture, soften)
	var scale := sprite.size / sprite.texture.get_size()
	return Rect2(sprite.position + ink.position * scale, ink.size * scale)

static func _center_sprite(sprite: TextureRect, bounds: Rect2, soften := false) -> void:
	sprite.position += bounds.get_center() - _sprite_ink(sprite, soften).get_center()
	sprite.pivot_offset = bounds.get_center() - sprite.position

static func _align_navigation(button: Button) -> void:
	var face := button.get_node("Face") as TextureRect
	button.position.y = HEADER_CENTER_Y - _sprite_ink(face).get_center().y

static func _digits_ink(digits: Control, soften := false) -> Rect2:
	var visible := Rect2()
	for child in digits.get_children():
		var ink := _sprite_ink(child, soften)
		visible = ink if not visible.has_area() else visible.merge(ink)
	return visible

static func _center_digits(digits: Control, soften := false) -> void:
	var shift := digits.size * 0.5 - _digits_ink(digits, soften).get_center()
	for child in digits.get_children():
		if soften:
			child.material = _action_ink()
		child.position += shift

func _equip(id: String, index: int) -> void:
	Sound.play("ui_tap")
	if Shop.equip(profile, id) != "":
		return
	Sound.play("equip")
	profile_changed.emit()
	_hero.queue_free()
	_hero = _live_bird(_hero_display, _species, _outfit_key(_species), _hero_box)
	_hero.showcase()
	_build_tiles()
	_bump(_tiles.get_child(index))

## One shared shop-style preview and numeric gold purchase action; insufficient funds dim it.
func _confirm_outfit(id: String, index: int, _tile: Control) -> void:
	if get_node_or_null("Popup") != null:
		return
	Sound.play("ui_tap")
	var it := Shop.item(id)
	var quick := _is_instant()
	var pic_side := 500.0
	var modal := PurchaseDialog.open(self, id.replace(".", "_"), [pic_side], quick, ButtonKit.WIDE.y)
	var ys: Array = modal.get_meta("ys")
	var pic := Rect2(540.0 - pic_side * 0.5, ys[0], pic_side, pic_side)
	PurchaseDialog.glow(modal.content, pic)
	_live_bird(modal.content, _species, "%s_outfit_%d" % [_species, it["index"]], pic.grow(-16.0))
	var affordable: bool = profile.coins >= it["price"]
	var accept := ButtonKit.button(modal.content, "Buy", Rect2(Vector2(540.0 - ButtonKit.WIDE.x * 0.5,
		modal.panel_rect.end.y + Modal.ACTION_DROP), ButtonKit.WIDE), "wide", func(): modal.invoke(func(): _buy_outfit(id, index)))
	var line := ButtonKit.price(accept.get_node("Content"), ButtonKit.WIDE, it["price"])
	UI.set_active(accept, affordable)
	if not affordable:
		var origin_x := line.position.x
		var tw := line.create_tween()
		tw.tween_interval(0.0 if quick else 0.35)
		tw.tween_callback(func(): Sound.play("error"))
		for dx in [16.0, -16.0, 10.0, -10.0, 4.0, 0.0]:
			tw.tween_property(line, "position:x", origin_x + dx, 0.05)

func _buy_outfit(id: String, index: int) -> void:
	if Shop.purchase(profile, id) != "":
		Sound.play("error")
		return
	Sound.play("collect")
	_build_coins()
	_equip(id, index)

# ---------------------------------------------------------------- capture preview

static func preview(main: Control, state: String) -> void:
	var p := Profile.new()
	p.coins = 240
	for card in Aviary.SPECIES:
		p.met[card[0]] = true
	p.homes = {"sunny": 12, "sky": 9, "berry": 7, "mint": 5, "cocoa": 3, "snow": 1}
	if state == "list_none_met":
		p.met.clear()
	if state == "detail_maximum":
		p.coins = FeatherBar.DISPLAY_MAX
	if state == "list_some_unmet":
		p.met = {"sunny": true, "sky": true, "berry": true, "mint": true}
	if state == "buy_outfit":   # coins for the first outfit but not the others
		p.coins = 400
	if state == "bought_outfit":   # the cheapest outfit already bought and worn
		p.outfits = {"outfit.hoot.1": true}
		p.equipped = {"hoot": "outfit.hoot.1"}
	if state == "buy_outfit_short":
		p.coins = 120
	if state == "detail_berry":
		p.outfits = {"outfit.berry.1": true, "outfit.berry.2": true}
		p.equipped = {"berry": "outfit.berry.1"}
	if state == "detail_ember_skin":   # a skin painted as a whole bird, worn
		p.outfits = {"outfit.ember.5": true}
		p.equipped = {"ember": "outfit.ember.5"}
	var view: Control = load("res://game/aviary_view.gd").new()
	if state == "detail_berry_flower":   # the flower costume, animated on itself
		p.outfits = {"outfit.berry.5": true}
		p.equipped = {"berry": "outfit.berry.5"}
	view.instant = main.instant()
	view.setup(p)
	main.set_screen(view)
	view.closed.connect(func(): main.get_tree().quit())
	Sound.music("bgm_menu")
	match state:
		"list_scrolled": view._set_scroll(1e6)
		"detail_sunny": view._show_detail("sunny")
		"detail_berry", "detail_berry_flower": view._show_detail("berry")
		"detail_crow": view._show_detail("crow")
		"detail_ember_skin":
			view._show_detail("ember")
			view._hero.showcase()   # what a tap on the bird does
		"detail_hoot", "detail_maximum": view._show_detail("hoot")
		"bought_outfit": view._show_detail("hoot")
		"buy_outfit", "buy_outfit_short":
			view._show_detail("hoot")
			view._confirm_outfit("outfit.hoot.1", 1, view._tiles.get_child(1))
