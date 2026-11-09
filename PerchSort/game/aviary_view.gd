extends Control
## Aviary (docs/design/ui-system.md): a top bar (close, title word, met counter) over a scrolling two-column grid
## of cream cards, one per species; tapping a met bird opens its detail page with a live bird, its ability
## and a row of outfit tiles (tap an owned outfit to equip it). The shop opens the same detail page on its own
## (`setup(profile, species)`): then Back closes the page and a coin counter shows what the player can spend.

signal closed
signal profile_changed

const PopupsScript := preload("res://game/popups.gd")
const CONFIRM_PIC := 320.0    # outfit picture of the buy popup
const CONFIRM_LINE := 72.0    # its price line
const TextArt := preload("res://game/ui_text.gd")
const CLOSE_ICON := preload("res://art/icons/icon_aviary_close.svg")
const FEATHER_ICON := preload("res://art/icons/icon_aviary_feather.svg")
const LIST_INK := Color("493c2e")
const LIST_GREEN := Color("34563f")
const LIST_CREAM := Color("fff9ee")

const BODY_CAP := 44.0
const MARGIN := 48.0
const RIGHT := 1032.0
## List grid: two columns of cards between the margins, scrolled under the top bar.
const FRAME := Rect2(36, 300, 1008, 1590)   # the wooden frame around the grid (user sketch)
const CLIP_TOP := 392.0
const CLIP_X := 84.0
const CLIP_W := 912.0
const GRID_X := 24.0
const CLIP_HEIGHT := 1440.0
const GRID_TOP := 16.0            # first card row inside the clip
const GRID_BOTTOM := 16.0         # under the last card row
const GAP := 32.0
const CARD_W := 416.0
const CARD_H := 420.0
const CARD_BIRD := Rect2(56, 35, 304, 270)
const CARD_NAME := Rect2(35, 316, 346, 78)
const CARD_BADGE := 64.0
const QUESTION_CAP := 96.0
const TRACK_X := 990.0
const TRACK_W := 12.0
const THUMB_MIN := 96.0
const DRAG_START := 12.0
const FRICTION := 5.0             # fling velocity decay per second (exponential)
## Detail page: one hero panel and the outfit row below it, centred in the free area under the bar.
const PANEL_PAD := 48.0
const HERO_BIRD := 480.0
const CROW_BIRD := 560.0
const LINE := 96.0
const SECTION_GAP := 48.0
const TILE_W := 228.0
const TILE_H := 312.0
const TILE_GAP := 24.0
const TILE_COLS := 3                          # six tiles (plain + five outfits) in two rows
const TILE_ROWS := 2
const TILE_BIRD := Rect2(24, 56, 180, 176)    # same bird size in every tile
const CORNER := Vector2(188, 40)              # centre of the lock or check mark
const CHECK := 56.0
const SILHOUETTE := """shader_type canvas_item;
void fragment() { COLOR = vec4(0.25, 0.19, 0.13, texture(TEXTURE, UV).a * 0.88); }"""

var profile: Profile
var instant := false        # set by the shop for captures; the aviary itself asks its parent
var _from_shop := false
var _coins: Panel
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
var _hero_box := Rect2()
var _tiles: Control
var _tile_row := Rect2()


func setup(p: Profile, species := "") -> void:
	profile = p
	_from_shop = not species.is_empty()
	size = Vector2(1080, 1920)
	UI.image(self, "board_w1_bg", Rect2(0, 0, 1080, 1920), false)
	# Calm the painting so the cards read first.
	var wash := ColorRect.new()
	wash.name = "Wash"
	wash.color = Color(Color("fff7e8"), 0.42)
	wash.size = size
	wash.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(wash)
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
			_show_list()
		else:
			_show_detail(_species)

# ---------------------------------------------------------------- shared pieces

func _reset_page() -> void:
	if _page:
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

## Live BirdView (breathing and idle actions) whose front frame fits the box, feet on the box bottom.
func _live_bird(parent: Node, species: String, outfit: String, box: Rect2) -> BirdView:
	var front := outfit + "_front" if not outfit.is_empty() and Art.has(outfit + "_front") else species + "_front"
	var used := UI.used_rect(front)
	var k := minf(box.size.x / used.size.x, box.size.y / used.size.y)
	var bird: BirdView = BirdView.new()
	bird.position = Vector2(box.get_center().x - (used.get_center().x - BirdView.CANVAS * 0.5) * k,
		box.end.y - (used.end.y - BirdView.FEET_Y) * k)
	parent.add_child(bird)
	bird.setup(Board.bird(0, Board.species_index(species), 0), false, k * BirdView.CANVAS / BirdView.DISPLAY, outfit)
	return bird

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

## A quick squash on a tapped card or tile.
func _bump(c: Control) -> void:
	c.pivot_offset = c.size * 0.5
	var tw := c.create_tween()
	tw.tween_property(c, "scale", Vector2.ONE * 0.95, 0.06).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
	tw.tween_property(c, "scale", Vector2.ONE, 0.2).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)

# ---------------------------------------------------------------- list

func _show_list() -> void:
	_species = ""
	_reset_page()
	var wash := ColorRect.new()
	wash.color = Color(LIST_CREAM, 0.18)
	wash.size = size
	wash.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_page.add_child(wash)
	UI.kit_piece(_page, "panel", FRAME, UI.PANEL_SCALE).name = "Frame"
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
		_card(id, Vector2(GRID_X + (i % 2) * (CARD_W + GAP), GRID_TOP + (i / 2) * (CARD_H + GAP)))
	var rows := ceili(count / 2.0)
	_content.size = Vector2(CLIP_W, GRID_TOP + rows * (CARD_H + GAP) - GAP + GRID_BOTTOM)
	_max_scroll = maxf(0.0, _content.size.y - _clip.size.y)
	_build_track()
	_list_header(met, count)
	_set_scroll(_list_scroll)

func _list_panel(parent: Node, rect: Rect2, color: Color, radius: float) -> Panel:
	# The painted kit card, like the shop's cards.
	var panel := Panel.new()
	panel.add_theme_stylebox_override("panel", StyleBoxEmpty.new())
	panel.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(panel, rect)
	var round_ends := radius * 2.0 >= rect.size.y - 2.0
	UI.kit_piece(panel, "capsule_plain" if round_ends else "card", Rect2(Vector2.ZERO, rect.size),
		0.0 if round_ends else UI.CARD_SCALE).name = "Skin"
	parent.add_child(panel)
	return panel

func _list_icon(parent: Node, texture: Texture2D, rect: Rect2, color: Color) -> TextureRect:
	var icon := TextureRect.new()
	icon.texture = texture
	icon.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	icon.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	icon.mouse_filter = Control.MOUSE_FILTER_IGNORE
	icon.modulate = color
	UI.place(icon, rect)
	parent.add_child(icon)
	return icon

func _list_header(met: int, count: int) -> void:
	var close := UI.button(_page, "Close", Vector2(112, 104), "red", "", func(): closed.emit())
	var icon := _list_icon(close, CLOSE_ICON, Rect2(28, 23, 72, 72), Color.WHITE)
	close.button_down.connect(func(): icon.position.y += 6.0)
	close.button_up.connect(func(): icon.position.y -= 6.0)
	UI.title_ribbon(_page, "aviary.title", Vector2(540, FRAME.position.y + 8.0), 56.0, 520.0)   # on the frame top
	var counter := _list_panel(_page, Rect2(712, 56, 320, 96), Color("e3edd9"), 48)
	counter.name = "MetCounter"
	_list_icon(counter, FEATHER_ICON, Rect2(16, 20, 56, 56), LIST_GREEN)
	TextArt.number(counter, "%d/%d" % [met, count], Rect2(80, 4, 224, 88), 44.0, LIST_GREEN)

## One cream card: a live bird and its name, or a dark silhouette with a question mark.
func _card(id: String, at: Vector2) -> void:
	var met := profile.has_met(id)
	var card := _list_panel(_content, Rect2(at, Vector2(CARD_W, CARD_H)), LIST_CREAM, 44)
	card.name = "Card_" + id
	var bird_box := Rect2(CARD_BIRD.position, CARD_BIRD.size)
	if met:
		_live_bird(card, id, _outfit_key(id), bird_box)
		TextArt.label(card, "bird.%s.name" % id, CARD_NAME, 44.0, LIST_INK)
		if PopupsScript.EFFECT.has(id):
			UI.shape(card, Rect2(CARD_W - 100, 20, 80, 80), "cream", 40.0, 0.0, false)
			UI.art(card, "gift_coin_" + PopupsScript.EFFECT[id],
				Rect2(CARD_W - 92, 28, CARD_BADGE, CARD_BADGE))
	else:
		_silhouette(card, id, bird_box)
		var q := TextArt.number(card, "?", Rect2(bird_box.get_center().x - 60.0, bird_box.get_center().y - 72.0,
			120.0, 160.0), QUESTION_CAP, Color.WHITE)
		q.name = "Question"
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
		_bump(card)
		if profile.has_met(entry[1]):
			Sound.play("ui_tap")
			var id: String = entry[1]
			get_tree().create_timer(0.12).timeout.connect(func(): if _species.is_empty(): _show_detail(id))
		return

# ---------------------------------------------------------------- detail

func _show_detail(id: String) -> void:
	_species = id
	_reset_page()
	var crow := id == "crow"
	var bird_h := CROW_BIRD if crow else HERO_BIRD
	var panel_h := PANEL_PAD + bird_h + 32.0 + LINE + PANEL_PAD + 12.0
	var block := panel_h + (0.0 if crow else SECTION_GAP + TILE_H * TILE_ROWS + TILE_GAP * (TILE_ROWS - 1))
	var top := roundf((160.0 + (1872.0 - 160.0 - block) * 0.5) / 8.0) * 8.0
	var panel := Rect2(108, top, 864, panel_h)
	UI.shape(_page, panel, "cream", 56.0, 12.0).name = "Panel"
	_hero_box = Rect2(540.0 - bird_h * 0.5, panel.position.y + PANEL_PAD, bird_h, bird_h)
	_hero = _live_bird(_page, id, _outfit_key(id), _hero_box)
	var timer := Timer.new()
	timer.wait_time = 5.0
	timer.autostart = true
	timer.timeout.connect(func(): if is_instance_valid(_hero) and not _hero.acting: _hero.cheer())
	_page.add_child(timer)
	var y := _hero_box.end.y + 32.0
	var inner_w := panel.size.x - PANEL_PAD * 2.0
	_ability(id, y, inner_w)
	if not crow:
		_tile_row = Rect2(MARGIN, panel.end.y + SECTION_GAP, RIGHT - MARGIN, TILE_H * TILE_ROWS + TILE_GAP * (TILE_ROWS - 1))
		_tiles = Control.new()
		_tiles.name = "Outfits"
		_tiles.mouse_filter = Control.MOUSE_FILTER_IGNORE
		_page.add_child(_tiles)
		_build_tiles()
	UI.button(_page, "Back", Vector2(112, 104), "cream", "back", _back)
	UI.title_ribbon(_page, "bird.%s.name" % id, Vector2(540, 104), 52.0, 600.0)
	_build_coins()

func _back() -> void:
	if _from_shop:
		closed.emit()
	else:
		_show_list()

func _is_instant() -> bool:
	return instant or (get_parent() != null and get_parent().has_method("instant") and get_parent().instant())

## Shop mode only: the player's coins at the right margin of the bar, rebuilt after a purchase.
func _build_coins() -> void:
	if _coins and is_instance_valid(_coins):
		_coins.queue_free()
	_coins = null
	if not _from_shop:
		return
	var value := str(profile.coins)
	var w := UI.NUMBER_X + UI.number_width(value, 44.0) + 32.0
	_coins = UI.counter(_page, Vector2(RIGHT - w, 104.0 - UI.COUNTER_H * 0.5), "coin", value)
	_coins.name = "Coins"

## Ability badge and caption on one centred line; the crow has none and shows that it never goes home.
func _ability(id: String, y: float, max_w: float) -> void:
	var has_badge := PopupsScript.EFFECT.has(id)
	var label_id := "bird.%s.ability" % id
	var badge := 72.0
	var lead := badge + 16.0 if has_badge else 56.0 + 16.0
	var tw := minf(UI.label_width(label_id, BODY_CAP), max_w - lead)
	var x := 540.0 - (lead + tw) * 0.5
	if has_badge:
		UI.art(_page, "gift_coin_" + PopupsScript.EFFECT[id], Rect2(x, y + (LINE - badge) * 0.5, badge, badge))
	else:
		UI.icon(_page, "home", Vector2(x + 28.0, y + LINE * 0.5), 56.0, Color(UI.INK, 0.8))
	UI.label(_page, label_id, Rect2(x + lead, y, tw, LINE), BODY_CAP, UI.INK)

## Outfit tiles in two rows of three: the plain bird, then outfits 1 to 5. Owned ones equip on tap; locked ones are dimmed with a
## lock and their price; the equipped one wears a green check badge.
func _build_tiles() -> void:
	for c in _tiles.get_children():
		_tiles.remove_child(c)   # rebuilt tiles keep their names
		c.queue_free()
	var x := _tile_row.position.x + (_tile_row.size.x - (TILE_W * TILE_COLS + TILE_GAP * (TILE_COLS - 1))) * 0.5
	var current: String = profile.equipped.get(_species, "outfit.%s.default" % _species)
	for n in Shop.OUTFIT_COUNT + 1:
		var id := "outfit.%s.default" % _species if n == 0 else "outfit.%s.%d" % [_species, n]
		var owned := n == 0 or profile.owns_outfit(id)
		var tile := UI.shape(_tiles, Rect2(x + (n % TILE_COLS) * (TILE_W + TILE_GAP), _tile_row.position.y + (n / TILE_COLS) * (TILE_H + TILE_GAP), TILE_W, TILE_H), "cream", 40.0, 10.0)
		tile.name = "Tile_%d" % n
		var outfit := "" if n == 0 else "%s_outfit_%d" % [_species, n]
		var bird := _live_bird(tile, _species, outfit, TILE_BIRD)
		if owned:
			var pick := id
			var hit := UI.hit_area(tile, Rect2(Vector2.ZERO, tile.size), func(): _equip(pick, n))
			hit.name = "Equip"
			hit.disabled = id == current
		else:
			bird.modulate.a = UI.DISABLED_ALPHA
			UI.icon(tile, "lock", CORNER, 44.0, Color(UI.INK, 0.7))
			_price(tile, Shop.item(id).get("price", 0))
			var pick_locked := id
			var hit := UI.hit_area(tile, Rect2(Vector2.ZERO, tile.size), func(): _confirm_outfit(pick_locked, n, tile))
			hit.name = "Locked"
		if id == current:
			var check := UI.shape(tile, Rect2(CORNER - Vector2.ONE * CHECK * 0.5, Vector2.ONE * CHECK), "green", CHECK * 0.5, 5.0, false)
			check.name = "Equipped"
			UI.icon(check, "check", Vector2(CHECK * 0.5, CHECK * 0.5 - 2.0), CHECK * 0.6)

## Coin and price centred at the bottom of a locked tile.
func _price(tile: Control, price: int) -> void:
	var cap := 36.0
	var text := str(price)
	var coin := 44.0
	var w := coin + 8.0 + UI.number_width(text, cap)
	var x := (TILE_W - w) * 0.5
	var cy := TILE_BIRD.end.y + (TILE_H - 10.0 - TILE_BIRD.end.y) * 0.5
	UI.art(tile, "coin", Rect2(x, cy - coin * 0.5, coin, coin))
	UI.number(tile, text, Rect2(x + coin + 8.0, cy - cap * 0.5, w - coin - 8.0 + 2.0, cap), UI.INK, -1)

func _equip(id: String, index: int) -> void:
	Sound.play("ui_tap")
	if Shop.equip(profile, id) != "":
		return
	profile_changed.emit()
	_hero.queue_free()
	_hero = _live_bird(_page, _species, _outfit_key(_species), _hero_box)
	_page.move_child(_hero, 1)
	_hero.cheer()
	_build_tiles()
	_bump(_tiles.get_child(index))

## Buying a locked outfit: the shop's confirm popup (name, the bird wearing it, price) over this page. With enough
## coins the green Buy pill buys and puts it on; without, Buy is dimmed and the price shakes.
func _confirm_outfit(id: String, index: int, tile: Control) -> void:
	Sound.play("ui_tap")
	var it := Shop.item(id)
	var title_id := id + ".name"
	var quick := _is_instant()
	var modal := Modal.open(self, title_id, [CONFIRM_PIC, CONFIRM_LINE], true, UI.PRIMARY, 0.0, quick)
	var ys: Array = modal.get_meta("ys")
	var pic := Rect2(540.0 - CONFIRM_PIC * 0.5, ys[0], CONFIRM_PIC, CONFIRM_PIC)
	_live_bird(modal.content, _species, "%s_outfit_%d" % [_species, it["index"]], pic.grow(-16.0))
	var affordable: bool = profile.coins >= it["price"]
	var line := _price_line(modal.content, ys[1], it["price"], UI.INK if affordable else UI.FILL["red"])
	var accept: Button = modal.actions([["green", "buy", func(): _buy_outfit(id, index)]])[0]
	UI.set_active(accept, affordable)
	if not affordable:
		var tw := line.create_tween()
		tw.tween_interval(0.0 if quick else 0.35)
		tw.tween_callback(func(): Sound.play("error"))
		for dx in [16.0, -16.0, 10.0, -10.0, 4.0, 0.0]:
			tw.tween_property(line, "position:x", dx, 0.05)

## Coin and price centred on one line; returns the holder that shakes.
func _price_line(c: Control, y: float, price: int, color: Color) -> Control:
	var holder := Control.new()
	holder.name = "PriceLine"
	holder.size = Vector2(1080, 1920)
	holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	c.add_child(holder)
	var text := str(price)
	var cap := 52.0
	var num_w := UI.number_width(text, cap)
	var x := 540.0 - (CONFIRM_LINE + 16.0 + num_w) * 0.5
	UI.art(holder, "coin", Rect2(x, y, CONFIRM_LINE, CONFIRM_LINE))
	UI.number(holder, text, Rect2(x + CONFIRM_LINE + 16.0, y + (CONFIRM_LINE - cap) * 0.5, num_w + 2.0, cap), color, -1)
	return holder

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
	var view: Control = load("res://game/aviary_view.gd").new()
	view.setup(p)
	main.set_screen(view)
	view.closed.connect(func(): main.get_tree().quit())
	Sound.music("bgm_menu")
	match state:
		"list_scrolled": view._set_scroll(1e6)
		"detail_berry": view._show_detail("berry")
		"detail_crow": view._show_detail("crow")
		"detail_hoot": view._show_detail("hoot")
		"bought_outfit": view._show_detail("hoot")
		"buy_outfit", "buy_outfit_short":
			view._show_detail("hoot")
			view._confirm_outfit("outfit.hoot.1", 1, view._tiles.get_child(1))
