extends Control
## Open leafy perches keep the live birds prominent over the tropical sea. The standalone list
## opens bird details; the shop opens those same details directly and owns their Back destination.

signal closed
signal profile_changed

## Two equal columns float directly over the sea, below the compact header.
const CLIP_TOP := 256.0
const CLIP_X := 48.0
const CLIP_W := 984.0
const CLIP_HEIGHT := 1568.0
const GRID_TOP := 0.0
const GRID_BOTTOM := 16.0
const GAP := 24.0
const CARD_W := 480.0
const CARD_H := 432.0
const CARD_BIRD := Rect2(80, 24, 320, 288)
const CARD_NAME := Rect2(40, 370, 400, 54)
const CARD_PERCH_WIDTH := 360.0
const TRACK_X := 1044.0
const TRACK_W := 8.0
const THUMB_MIN := 96.0
const DRAG_START := 12.0
const FRICTION := 5.0             # fling velocity decay per second (exponential)
const CLOSE := Rect2(48, 48, 112, 112)
const TITLE_SIGN := Rect2(324, 48, 432, 112)
const DETAIL_TITLE_SIGN := Rect2(232, 48, 432, 112)
const TILE_GAP := 24.0
const TILE_COLS := 3
const SHOP_BIRD := Rect2(340, 232, 400, 400)
const HERO_PERCH_WIDTH := 592.0
const SHOP_TILE_SIZE := Vector2(312, 332)
const SHOP_TILE_BIRD := Rect2(40, 8, 232, 220)
const SHOP_TILE_PRICE := Rect2(52, 278, 208, 52)
const SHOP_TILES := Rect2(48, 936, 984, 688)
const TILE_PERCH_WIDTH := 272.0
const PERCH_CONTACT_Y := 46.0     # centre top of the wood in island_branch's source pixels
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
var _hero: BirdView
var _hero_display: Control
var _hero_box := Rect2()
var _tiles: Control
var _tile_row := Rect2()


func setup(p: Profile, species := "") -> void:
	profile = p
	_from_shop = not species.is_empty()
	size = Vector2(1080, 1920)
	UI.image(self, "background_shop_sea", Rect2(0, 0, 1080, 1920), false)
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
	return bird

## The complete production branch keeps its leaves and native ratio, with wood meeting the feet.
func _perch(parent: Node, contact: Vector2, width: float) -> TextureRect:
	var k := width / Art.tex("island_branch").get_width()
	return Island.piece(parent, "island_branch", contact - Vector2(width * 0.5, PERCH_CONTACT_Y * k), width)

## Dark breathing silhouette of a species fitted into the box (unmet birds); the shop's outfits tab shares it.
static func _silhouette(parent: Node, species: String, box: Rect2) -> TextureRect:
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
	var tw := sil.create_tween().set_loops()
	tw.tween_property(sil, "scale", Vector2(0.99, 1.025), 1.4).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
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
	Island.button(_page, "Close", "island_close_tile", CLOSE, func(): closed.emit())
	ShopSkin.plaque(_page, "aviary.title", TITLE_SIGN)
	var chip := ShopSkin.card(_page, Rect2(768, 168, 264, 72))
	chip.name = "MetCounter"
	UI.art(chip, "pic_feather", Rect2(20, 12, 48, 48))
	UI.number(chip, "%d/%d" % [met, count], Rect2(80, 14, 164, 44), UI.INK)

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
		ShopSkin.word(card, name_id, CARD_NAME)
	else:
		UI.art(card, "island_unknown_bird", Rect2(60, 40, 360, 322)).name = "Question"
		Island.piece_fit(card, "island_padlock", Vector2(240, 394), Vector2(48, 48)).name = "Lock"
	_cards.append([card, id])

func _build_track() -> void:
	var visible_h := _clip.size.y - GRID_BOTTOM
	_track = Rect2(TRACK_X, CLIP_TOP + GRID_TOP, TRACK_W, visible_h - GRID_TOP)
	var track := _pill(_track, Color(UI.RIM["cream"], 0.9))
	track.name = "Track"
	_thumb = _pill(Rect2(_track.position, Vector2(TRACK_W, THUMB_MIN)), UI.FILL["green"])
	_thumb.name = "Thumb"
	var show := _max_scroll > 0.0
	track.visible = show
	_thumb.visible = show

func _pill(r: Rect2, color: Color) -> Panel:
	var p := Panel.new()
	var style := StyleBoxFlat.new()
	style.bg_color = color
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
	var h := maxf(THUMB_MIN, _track.size.y * _clip.size.y / maxf(_content.size.y, 1.0))
	_thumb.size.y = h
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
	ShopSkin.card(_page, Rect2(112, 748, 856, 80)).name = "AbilityStrip"
	UiText.painted_label(_page, "bird.%s.ability" % id, Rect2(140, 762, 800, 52))
	if not crow:
		_tile_row = SHOP_TILES
		_tiles = Control.new()
		_tiles.name = "Outfits"
		_tiles.mouse_filter = Control.MOUSE_FILTER_IGNORE
		_page.add_child(_tiles)
		_build_tiles()
		if not _from_shop:
			UiText.painted(_page, "tab_outfits", Rect2(340, 850, 400, 48))
	Island.button(_page, "Back", "island_back_tile", CLOSE, _back)
	ShopSkin.plaque(_page, "bird.%s.name" % id, DETAIL_TITLE_SIGN)
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
	_coins = ShopSkin.counter(_page, profile)

## Six open perch positions: plain bird, then outfits 1 to 5. Owned ones equip on tap; locked ones
## are dimmed with compact prices. A green foot marker and check identify the equipped outfit.
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
		if id == current:
			Island.piece_fit(tile, "island_pill_green", Vector2(tile_size.x * 0.5, 304), Vector2(144, 32)).name = "Selected"
		var outfit := "" if n == 0 else "%s_outfit_%d" % [_species, n]
		var bird := _live_bird(tile, _species, outfit, bird_box)
		if owned:
			var pick := id
			var hit := UI.hit_area(tile, Rect2(Vector2.ZERO, tile.size), func(): _equip(pick, n))
			hit.name = "Equip"
			hit.disabled = id == current
		else:
			bird.modulate.a = UI.DISABLED_ALPHA
			var lock_center := Vector2(tile_size.x - 40.0, 40.0)
			var lock_size := Vector2(56, 56)
			Island.piece_fit(tile, "island_padlock", lock_center, lock_size).name = "Lock"
			ShopSkin.price(tile, SHOP_TILE_PRICE, Shop.item(id)["price"])
			var pick_locked := id
			var hit := UI.hit_area(tile, Rect2(Vector2.ZERO, tile.size), func(): _confirm_outfit(pick_locked, n, tile))
			hit.name = "Locked"
		var tap_area: Button = tile.get_node("Equip" if owned else "Locked")
		tap_area.button_down.connect(func(): tile.modulate = Island.PRESSED)
		tap_area.button_up.connect(func(): tile.modulate = Color.WHITE)
		if id == current:
			Island.check_badge(tile, Vector2(tile_size.x - 40.0, 40.0)).name = "Equipped"

func _equip(id: String, index: int) -> void:
	Sound.play("ui_tap")
	if Shop.equip(profile, id) != "":
		return
	profile_changed.emit()
	_hero.queue_free()
	_hero = _live_bird(_hero_display, _species, _outfit_key(_species), _hero_box)
	_hero.showcase()
	_build_tiles()
	_bump(_tiles.get_child(index))

## One shared shop-style preview and numeric gold purchase action; insufficient funds dim it.
func _confirm_outfit(id: String, index: int, _tile: Control) -> void:
	Sound.play("ui_tap")
	var it := Shop.item(id)
	var quick := _is_instant()
	var pic_side := 320.0
	var modal := ShopSkin.modal(self, id + ".name", [pic_side], quick, 132.0)
	var ys: Array = modal.get_meta("ys")
	var pic := Rect2(540.0 - pic_side * 0.5, ys[0], pic_side, pic_side)
	_live_bird(modal.content, _species, "%s_outfit_%d" % [_species, it["index"]], pic.grow(-16.0))
	var affordable: bool = profile.coins >= it["price"]
	var accept := ShopSkin.button(modal.content, "Buy", Rect2(300, modal.get_meta("actions_y"), 480, 132),
		func(): modal.invoke(func(): _buy_outfit(id, index)), "gold")
	var line := ShopSkin.amount(accept, Rect2(24, 28, 432, 76), it["price"])
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
		p.met = {"sunny": true, "sky": true, "berry": true}
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
	view.instant = main.instant()
	view.setup(p)
	main.set_screen(view)
	view.closed.connect(func(): main.get_tree().quit())
	Sound.music("bgm_menu")
	match state:
		"list_scrolled": view._set_scroll(1e6)
		"detail_berry": view._show_detail("berry")
		"detail_crow": view._show_detail("crow")
		"detail_ember_skin":
			view._show_detail("ember")
			view._hero.showcase()   # what a tap on the bird does
		"detail_hoot", "detail_maximum": view._show_detail("hoot")
		"bought_outfit": view._show_detail("hoot")
		"buy_outfit", "buy_outfit_short":
			view._show_detail("hoot")
			view._confirm_outfit("outfit.hoot.1", 1, view._tiles.get_child(1))
