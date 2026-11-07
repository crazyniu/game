extends Control
## Aviary (docs/design/ui-system.md): a top bar (close, title word, met counter) over a scrolling two-column grid
## of cream cards, one per species; tapping a met bird opens its detail page with a big live bird, its personality
## and ability lines and a row of outfit tiles (tap an owned outfit to equip it).

signal closed
signal profile_changed

const PopupsScript := preload("res://game/popups.gd")

## Top bar: the row y 48 to 160; buttons and counters share the board's positions.
const BAR_Y := 104.0
const BACK := Vector2(112, 104)
const COUNTER_TOP := 56.0
const TITLE_CAP := 64.0
const BODY_CAP := 44.0
const SMALL_CAP := 34.0
const MARGIN := 48.0
const RIGHT := 1032.0
## List grid: two columns of cards between the margins, scrolled under the top bar.
const CLIP_TOP := 176.0
const GRID_TOP := 16.0            # first card row inside the clip
const GRID_BOTTOM := 48.0         # under the last card row
const GAP := 32.0
const CARD_W := 476.0
const CARD_H := 456.0
const CARD_BIRD := Rect2(48, 40, 380, 300)    # bird box inside a card
const CARD_NAME_Y := 392.0                    # name word centre inside a card
const CARD_BADGE := 72.0
const QUESTION_CAP := 96.0
const TRACK_X := 1052.0
const TRACK_W := 8.0
const THUMB_MIN := 96.0
const DRAG_START := 12.0
const FRICTION := 5.0             # fling velocity decay per second (exponential)
## Detail page: one hero panel and the outfit row below it, centred in the free area under the bar.
const PANEL := 56.0
const PANEL_PAD := 48.0
const HERO_BIRD := 680.0
const CROW_BIRD := 840.0
const LINE := 64.0
const LINE_GAP := 24.0
const SECTION_GAP := 48.0
const TILE_W := 228.0
const TILE_H := 312.0
const TILE_GAP := 24.0
const TILE_BIRD := Rect2(24, 56, 180, 176)    # same bird size in every tile
const CORNER := Vector2(188, 40)              # centre of the lock or check mark
const CHECK := 56.0
const SILHOUETTE := """shader_type canvas_item;
void fragment() { COLOR = vec4(0.25, 0.19, 0.13, texture(TEXTURE, UV).a * 0.88); }"""

var profile: Profile
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


func setup(p: Profile) -> void:
	profile = p
	size = Vector2(1080, 1920)
	UI.image(self, "board_w1_bg", Rect2(0, 0, 1080, 1920), false)
	# Calm the painting so the cards read first.
	var wash := ColorRect.new()
	wash.name = "Wash"
	wash.color = Color(UI.FILL["cream"], 0.42)
	wash.size = size
	wash.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(wash)
	_show_list()

func _process(delta: float) -> void:
	if _content == null or not is_instance_valid(_content) or _pressed or absf(_velocity) < 4.0:
		return
	_set_scroll(_scroll + _velocity * delta)
	_velocity *= exp(-FRICTION * delta)
	if _scroll <= 0.0 or _scroll >= _max_scroll:
		_velocity = 0.0

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

## Neutral secondary button at the top left (close on the list, back on a detail page).
func _bar_button(icon_name: String, on_press: Callable) -> Button:
	return UI.button(_page, "Close" if icon_name == "close" else "Back", BACK, "cream", icon_name, on_press)

## Painted word centred on `center`, shrunk to fit `max_w` when it is wider.
func _word_fit(parent: Node, text: String, center: Vector2, cap: float, max_w: float, color := UI.INK) -> TextureRect:
	var w := UI.word_width(text, cap)
	if w > max_w:
		cap *= max_w / w
	return UI.word(parent, text, center, cap, color)

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
	if not outfit.is_empty() and Art.has(outfit + "_front"):
	return bird

## Dark breathing silhouette of a species fitted into the box (unmet birds).
func _silhouette(parent: Node, species: String, box: Rect2) -> TextureRect:
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

func _name(species: String) -> String:
	return Aviary.display_name(species)

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
	_clip = Control.new()
	_clip.name = "Scroll"
	_clip.clip_contents = true
	UI.place(_clip, Rect2(0, CLIP_TOP, 1080, 1920 - CLIP_TOP))
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
		_card(id, Vector2(MARGIN + (i % 2) * (CARD_W + GAP), GRID_TOP + (i / 2) * (CARD_H + GAP)))
	var rows := ceili(count / 2.0)
	_content.size = Vector2(1080, GRID_TOP + rows * (CARD_H + GAP) - GAP + GRID_BOTTOM)
	_max_scroll = maxf(0.0, _content.size.y - _clip.size.y)
	_build_track()
	# Top bar above the clipped grid.
	_bar_button("close", func(): closed.emit())
	UI.word(_page, "Aviary", Vector2(540, BAR_Y), TITLE_CAP)
	var value := "%d/%d" % [met, count]
	var w := 100.0 + UI.number_width(value, 44.0) + 32.0
	UI.counter(_page, Vector2(RIGHT - w, COUNTER_TOP), "icon_feather", value, UI.INK).name = "MetCounter"
	_set_scroll(_list_scroll)

## One cream card: a live bird and its name, or a dark silhouette with a question mark.
func _card(id: String, at: Vector2) -> void:
	var met := profile.has_met(id)
	var card := UI.shape(_content, Rect2(at, Vector2(CARD_W, CARD_H)), "cream", 40.0, 10.0)
	card.name = "Card_" + id
	var bird_box := Rect2(CARD_BIRD.position, CARD_BIRD.size)
	if met:
		_live_bird(card, id, _outfit_key(id), bird_box)
		_word_fit(card, _name(id), Vector2(CARD_W * 0.5, CARD_NAME_Y), BODY_CAP, CARD_W - 64.0)
		if PopupsScript.EFFECT.has(id):
			UI.art(card, "gift_coin_" + PopupsScript.EFFECT[id],
				Rect2(CARD_W - 24.0 - CARD_BADGE, 24.0, CARD_BADGE, CARD_BADGE))
	else:
		_silhouette(card, id, bird_box)
		var q := UI.number(card, "?", Rect2(bird_box.get_center().x - 60.0, bird_box.get_center().y - QUESTION_CAP * 0.5 + 24.0,
			120.0, QUESTION_CAP), Color.WHITE)
		q.name = "Question"
		_word_fit(card, "Unknown", Vector2(CARD_W * 0.5, CARD_NAME_Y), BODY_CAP, CARD_W - 64.0, Color(UI.INK, 0.45))
	_cards.append([card, id])

func _build_track() -> void:
	var visible_h := _clip.size.y - GRID_BOTTOM
	_track = Rect2(TRACK_X, CLIP_TOP + GRID_TOP, TRACK_W, visible_h - GRID_TOP)
	var track := _pill(_track, Color(UI.INK, 0.12))
	track.name = "Track"
	_thumb = _pill(Rect2(_track.position, Vector2(TRACK_W, THUMB_MIN)), Color(UI.INK, 0.45))
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
	var panel_h := PANEL_PAD + bird_h + 32.0 + LINE + LINE_GAP + LINE + PANEL_PAD + 12.0
	var block := panel_h + (0.0 if crow else SECTION_GAP + TILE_H)
	var top := roundf((160.0 + (1872.0 - 160.0 - block) * 0.5) / 8.0) * 8.0
	var panel := Rect2(MARGIN, top, RIGHT - MARGIN, panel_h)
	UI.shape(_page, panel, "cream", PANEL, 12.0).name = "Panel"
	_hero_box = Rect2(540.0 - bird_h * 0.5, panel.position.y + PANEL_PAD, bird_h, bird_h)
	_hero = _live_bird(_page, id, _outfit_key(id), _hero_box)
	var timer := Timer.new()
	timer.wait_time = 5.0
	timer.autostart = true
	timer.timeout.connect(func(): if is_instance_valid(_hero) and not _hero.acting: _hero.cheer())
	_page.add_child(timer)
	var y := _hero_box.end.y + 32.0
	var inner_w := panel.size.x - PANEL_PAD * 2.0
	_word_fit(_page, _personality(id), Vector2(540, y + LINE * 0.5), BODY_CAP, inner_w, Color(UI.INK, 0.8))
	y += LINE + LINE_GAP
	_ability(id, y, inner_w)
	if not crow:
		_tile_row = Rect2(MARGIN, panel.end.y + SECTION_GAP, RIGHT - MARGIN, TILE_H)
		_tiles = Control.new()
		_tiles.name = "Outfits"
		_tiles.mouse_filter = Control.MOUSE_FILTER_IGNORE
		_page.add_child(_tiles)
		_build_tiles()
	_bar_button("left", _back)
	_word_fit(_page, _name(id), Vector2(540, BAR_Y), TITLE_CAP, 600.0)

func _back() -> void:
	_show_list()

func _personality(id: String) -> String:
	return {"sunny": "Cheerful always first to sing", "sky": "Sleepy-eyed never in a hurry",
		"berry": "Show-off loves an encore", "mint": "Gardener makes branches grow", "cocoa": "Grumpy scares crows away",
		"snow": "Light on her feet takes a shortcut", "ember": "Hot-headed cracks eggs open",
		"crow": "Trouble in black feathers", "ink": "Night owl flips the sky", "hoot": "Dreamy calls the moon"}.get(id, "")

## Ability badge and caption on one centred line; the crow has none and shows that it never goes home.
func _ability(id: String, y: float, max_w: float) -> void:
	var has_badge := PopupsScript.EFFECT.has(id)
	var caption: String = PopupsScript.ABILITY.get(id, "Never goes home")
	var badge := LINE + 8.0
	var lead := badge + 16.0 if has_badge else 56.0 + 16.0
	var cap := BODY_CAP
	var tw := UI.word_width(caption, cap)
	if lead + tw > max_w:
		cap *= (max_w - lead) / tw
		tw = max_w - lead
	var x := 540.0 - (lead + tw) * 0.5
	if has_badge:
		UI.art(_page, "gift_coin_" + PopupsScript.EFFECT[id], Rect2(x, y + (LINE - badge) * 0.5, badge, badge))
	else:
		UI.icon(_page, "home", Vector2(x + 28.0, y + LINE * 0.5), 56.0, Color(UI.INK, 0.8))
	UI.word(_page, caption, Vector2(x + lead, y + LINE * 0.5), cap, UI.INK, -1)

## Four outfit tiles: the plain bird, then outfits 1 to 3. Owned ones equip on tap; locked ones are dimmed with a
## lock and their price; the equipped one wears a green check badge.
func _build_tiles() -> void:
	for c in _tiles.get_children():
		c.queue_free()
	var x := _tile_row.position.x + (_tile_row.size.x - (TILE_W * 4.0 + TILE_GAP * 3.0)) * 0.5
	var current: String = profile.equipped.get(_species, "outfit.%s.default" % _species)
	for n in 4:
		var id := "outfit.%s.default" % _species if n == 0 else "outfit.%s.%d" % [_species, n]
		var owned := n == 0 or profile.owns_outfit(id)
		var tile := UI.shape(_tiles, Rect2(x + n * (TILE_W + TILE_GAP), _tile_row.position.y, TILE_W, TILE_H), "cream", 40.0, 10.0)
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
			var hit := UI.hit_area(tile, Rect2(Vector2.ZERO, tile.size), func(): _nudge(tile))
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
	_bump(_tiles.get_child(_tiles.get_child_count() - 4 + index))

## A locked tile shakes sideways and stays put.
func _nudge(tile: Control) -> void:
	Sound.play("ui_tap")
	var x := tile.position.x
	var tw := tile.create_tween()
	for dx in [-10.0, 10.0, -6.0, 6.0, 0.0]:
		tw.tween_property(tile, "position:x", x + dx, 0.05).set_trans(Tween.TRANS_SINE)

# ---------------------------------------------------------------- capture preview

static func preview(main: Control, state: String) -> void:
	var p := Profile.new()
	p.coins = 240
	for card in Aviary.SPECIES:
		p.met[card[0]] = true
	p.homes = {"sunny": 12, "sky": 9, "berry": 7, "mint": 5, "cocoa": 3, "snow": 1}
	if state == "list_some_unmet":
		p.met = {"sunny": true, "sky": true, "berry": true}
	if state == "detail_berry":
		p.outfits = {"outfit.berry.1": true, "outfit.berry.2": true}
		p.equipped = {"berry": "outfit.berry.1"}
	var view: Control = load("res://game/aviary_view.gd").new()
	view.setup(p)
	main.set_screen(view)
	match state:
		"list_scrolled": view._set_scroll(1e6)
		"detail_berry": view._show_detail("berry")
		"detail_crow": view._show_detail("crow")
		"detail_hoot": view._show_detail("hoot")
