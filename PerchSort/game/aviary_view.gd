extends Control
## Aviary: a scrolling two-column list of species, and a detail page with outfits, ability and homecomings.

signal closed
signal profile_changed

const BirdViewScript := preload("res://game/bird_view.gd")
const LIST := "aviary_list"
const DETAIL := "aviary_detail"
## Layout constants (ui-system.md sections 1-5 and 8). Design space 1080x1920.
const MARGIN := 40.0
const HUD_PILL_H := 96.0
const COIN_ICON := 104.0
const CLOSE_DISC := 112.0          # same role and size as the board's gear disc
const TITLE_W := 440.0
const BOARD_TOP := 168.0           # 16 under the header row
const BOARD_BOTTOM := 1872.0       # 48 from the screen bottom
const CELL_W := 300.0
const RING_D := 260.0              # list portrait ring; the bird fits inside at 80 %
const NAME_W := 222.0              # every name plate in the list shares this size
const NAME_H := 64.0
const PILL_H := 80.0
const CHIP_W := 190.0
const GIFT_D := 80.0
const ROW_GAP := 40.0
const GRID_PAD := Vector2(28.0, 22.0)
const SCROLL_GUTTER := 44.0        # right side of the opening is kept for the scroll track
const DETAIL_CARD_X := 100.0
const DETAIL_CARD_W := 880.0
const CARD_PAD_IN := 56.0
const TILE_GAP := 24.0
const TILE_H := 200.0
const GIFT_DETAIL := 96.0
const CAPTION_H := 62.0
const BORDER_SCALE := 0.55
const CARD_PAD := Vector4(15, 48, 15, 4)   # transparent margins of card_wood (left, top, right, bottom)
const ABILITY_GIFT := {"sunny": "wake", "sky": "reveal", "berry": "encore", "mint": "grow", "cocoa": "scare",
	"snow": "lightstep", "ember": "hatch", "ink": "nightcall", "hoot": "moonbeam"}
const ABILITY_LABEL := {"sunny": "wakes_every_sleeping_bird", "sky": "shows_every_egg_s_secret", "berry": "free_move",
	"mint": "branches_grow_one_seat", "cocoa": "scares_nearby_crows_away", "snow": "next_landing_breaks_the_rules",
	"ember": "every_egg_hatches_now", "ink": "day_and_night_swap", "hoot": "three_moves_of_night"}
const PERSONALITY_LABEL := {"sunny": "cheerful_always_first_to_sing", "sky": "sleepy_eyed_never_in_a_hurry",
	"berry": "show_off_loves_an_encore", "mint": "gardener_makes_branches_grow", "cocoa": "grumpy_scares_crows_away",
	"snow": "light_on_her_feet_takes_a_shortcut", "ember": "hot_headed_cracks_eggs_open", "crow": "trouble_in_black_feathers",
	"ink": "night_owl_flips_the_sky", "hoot": "dreamy_calls_the_moon"}
const SILHOUETTE_SHADER := """shader_type canvas_item;
void fragment() { COLOR = vec4(0.45, 0.31, 0.25, texture(TEXTURE, UV).a); }"""

var profile: Profile
var _page: Control
var _coin_number: Control
var _clip: Control
var _content: Control
var _thumb: Control
var _track: Rect2
var _cells: Array = []      # [Rect2 in content coordinates, species id]
var _scroll := 0.0
var _max_scroll := 0.0
var _press := Vector2.ZERO
var _dragging := false
var _pressed := false
var _hero: BirdView
var _tiles: Control
var _species := ""
var _list_scroll := 0.0


func setup(p: Profile) -> void:
	profile = p
	size = Vector2(1080, 1920)
	UI.image(self, "bg_level_w1_noon", Rect2(0, 0, 1080, 1920), false)
	_show_list()

## ---------- shared pieces ----------

func _reset_page() -> void:
	if _page:
		_page.queue_free()
	_page = Control.new()
	_page.name = "Page"
	_page.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_page.size = size
	add_child(_page)
	_cells.clear()

## Coin pill top-left (coin overlaps its left end), close disc top-right; optional centred title ribbon.
func _header(on_close: Callable, title := "", title_w := TITLE_W) -> void:
	var number_w := 36.0 * maxi(str(profile.coins).length(), 3)
	var coin := Rect2(MARGIN, MARGIN + (HUD_PILL_H - COIN_ICON) * 0.5, COIN_ICON, COIN_ICON)
	UI.panel(_page, "stat_pill", Rect2(coin.get_center().x, MARGIN, COIN_ICON * 0.5 + 24.0 + number_w + 28.0, HUD_PILL_H))
	UI.art(_page, "coin", coin)
	_coin_number = UI.number(_page, str(profile.coins), Rect2(coin.end.x + 24.0, MARGIN + 22.0, number_w, 52.0), "brown", -1)
	UI.disc(_page, "Close", Rect2(1080.0 - MARGIN - CLOSE_DISC, MARGIN, CLOSE_DISC, CLOSE_DISC), "icon_close", on_close, "btn_wood", 0.5)
	if not title.is_empty():
		var used := UI.used_rect(title)
		var h := title_w * used.size.y / used.size.x
		_plate(_page, title, Rect2(540.0 - title_w * 0.5, MARGIN + (HUD_PILL_H - h) * 0.5, title_w, h))

## Count chip (house icon, then the number) of height PILL_H at `at`.
func _count_chip(parent: Node, at: Vector2, count: int) -> void:
	var chip := Rect2(at, Vector2(CHIP_W, PILL_H))
	UI.art(parent, "house_count_chip", chip)
	UI.number(parent, str(count), Rect2(chip.position.x + CHIP_W * 0.42, chip.position.y + (PILL_H - 40.0) * 0.5, CHIP_W * 0.46, 40.0))

## Painted plate (ribbon or caption sprite, text included) fitted inside `r`, centered, aspect kept.
func _plate(parent: Node, label: String, r: Rect2) -> void:
	var used := UI.used_rect(label)
	var k := minf(r.size.x / used.size.x, r.size.y / used.size.y)
	UI.art(parent, label, Rect2(r.get_center() - used.size * k * 0.5, used.size * k))

## A living bird whose visible pixels fit `fit` (bottom-centered), never a still.
func _bird(parent: Node, species: String, outfit: String, fit: Rect2) -> BirdView:
	var front := outfit + "_front" if not outfit.is_empty() and Art.has(outfit + "_front") else species + "_front"
	var used := UI.used_rect(front)
	var s := minf(fit.size.x / used.size.x, fit.size.y / used.size.y)
	var bird: BirdView = BirdViewScript.new()
	bird.position = Vector2(fit.get_center().x, fit.end.y + (BirdView.FEET_Y - used.end.y) * s)
	parent.add_child(bird)
	bird.setup(Board.bird(0, Board.species_index(species), 0), false, s * BirdView.CANVAS / BirdView.DISPLAY, outfit)
	return bird

func _outfit_key(species: String) -> String:
	var id: String = profile.equipped.get(species, "")
	return "" if id.is_empty() else "%s_outfit_%s" % [species, id.get_slice(".", 2)]

## ---------- list ----------

func _show_list() -> void:
	_species = ""
	_reset_page()
	# Board art fitted uniformly between the header row and the bottom margin.
	var m := Art.metrics("aviary_board")
	var opaque: Array = m["opaque"]
	var inner: Array = m["inner"]
	var k := (BOARD_BOTTOM - BOARD_TOP) / float(opaque[3])
	var visible := Rect2((1080.0 - opaque[2] * k) * 0.5, BOARD_TOP, opaque[2] * k, opaque[3] * k)
	UI.art(_page, "aviary_board", visible, Rect2(opaque[0], opaque[1], opaque[2], opaque[3]))
	var origin := visible.position - Vector2(opaque[0], opaque[1]) * k
	var opening := Rect2(origin + Vector2(inner[0], inner[1]) * k, Vector2(inner[2], inner[3]) * k)
	_header(func(): closed.emit(), "label_title_aviary")
	_clip = Control.new()
	_clip.name = "Scroll"
	_clip.clip_contents = true
	UI.place(_clip, opening)
	_clip.mouse_filter = Control.MOUSE_FILTER_STOP
	_clip.gui_input.connect(_on_scroll_input)
	_page.add_child(_clip)
	_content = Control.new()
	_content.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_clip.add_child(_content)
	var grid_w := CELL_W * 2.0 + (opening.size.x - GRID_PAD.x - SCROLL_GUTTER - CELL_W * 2.0)
	var x0 := GRID_PAD.x
	var col_gap := grid_w - CELL_W * 2.0
	for i in Aviary.SPECIES.size():
		_cell(Aviary.SPECIES[i][0], x0 + (i % 2) * (CELL_W + col_gap), GRID_PAD.y + (i / 2) * _row_pitch())
	var rows := ceili(Aviary.SPECIES.size() / 2.0)
	_content.size = Vector2(opening.size.x, GRID_PAD.y * 2.0 + rows * _row_pitch() - ROW_GAP)
	_max_scroll = maxf(0.0, _content.size.y - opening.size.y)
	_build_track(opening)
	_set_scroll(_list_scroll)

func _row_h() -> float:
	return RING_D + 8.0 + NAME_H + 16.0 + PILL_H

func _row_pitch() -> float:
	return _row_h() + ROW_GAP

## One species cell: portrait ring with the bird at 80 %, name plate, count chip and ability coin.
func _cell(id: String, x: float, y: float) -> void:
	var met := profile.has_met(id)
	var ring := Rect2(x + (CELL_W - RING_D) * 0.5, y, RING_D, RING_D)
	UI.art(_content, "badge_ring_gold", ring)
	var fit := ring.grow(-RING_D * 0.1)
	if met:
		_bird(_content, id, _outfit_key(id), fit)
	else:
		_silhouette(_content, id, fit)
	var name_r := Rect2(x + (CELL_W - NAME_W) * 0.5, ring.end.y + 8.0, NAME_W, NAME_H)
	_plate(_content, "label_name_" + (id if met else "unknown"), name_r)
	var row_y := name_r.end.y + 16.0
	var row_x := x + (CELL_W - (CHIP_W + 10.0 + GIFT_D)) * 0.5
	_count_chip(_content, Vector2(row_x, row_y), profile.flocks_home(id))
	if met and ABILITY_GIFT.has(id):
		UI.art(_content, "gift_coin_" + ABILITY_GIFT[id], Rect2(row_x + CHIP_W + 10.0, row_y, GIFT_D, GIFT_D))
	_cells.append([Rect2(x, y, CELL_W, _row_h()), id])

func _silhouette(parent: Node, species: String, fit: Rect2) -> void:
	var front := species + "_front"
	var used := UI.used_rect(front)
	var s := minf(fit.size.x / used.size.x, fit.size.y / used.size.y)
	var box := Rect2(Vector2(fit.get_center().x - used.size.x * s * 0.5, fit.end.y - used.size.y * s), used.size * s)
	var sil := UI.art(parent, front, box)
	var shader := Shader.new()
	shader.code = SILHOUETTE_SHADER
	var mat := ShaderMaterial.new()
	mat.shader = shader
	sil.material = mat
	sil.pivot_offset = Vector2(sil.size.x * 0.5, sil.size.y * 0.94)
	var tw := sil.create_tween().set_loops()
	tw.tween_property(sil, "scale", Vector2(0.992, 1.02), 1.4).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	tw.tween_property(sil, "scale", Vector2.ONE, 1.4).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)

func _build_track(opening: Rect2) -> void:
	_track = Rect2(opening.end.x - 28, opening.position.y + 50, 16, opening.size.y - 100)
	_pill(_track, Color(0.8, 0.64, 0.4, 0.7))
	_thumb = _pill(Rect2(_track.position.x - 3, _track.position.y, 22, 100), Color(1.0, 0.72, 0.16))

func _pill(r: Rect2, color: Color) -> Panel:
	var p := Panel.new()
	var style := StyleBoxFlat.new()
	style.bg_color = color
	style.border_color = Color(0.72, 0.42, 0.08)
	style.set_border_width_all(3)
	style.set_corner_radius_all(14)
	p.add_theme_stylebox_override("panel", style)
	p.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(p, r)
	_page.add_child(p)
	return p

func _set_scroll(value: float) -> void:
	_scroll = clampf(value, 0.0, _max_scroll)
	_list_scroll = _scroll
	_content.position.y = -_scroll
	var h := maxf(60.0, _track.size.y * _clip.size.y / _content.size.y)
	_thumb.size.y = h
	_thumb.position.y = _track.position.y + (_track.size.y - h) * (_scroll / maxf(_max_scroll, 1.0))

func _on_scroll_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		var e := event as InputEventMouseButton
		if e.button_index == MOUSE_BUTTON_WHEEL_UP and e.pressed:
			_set_scroll(_scroll - 120.0)
		elif e.button_index == MOUSE_BUTTON_WHEEL_DOWN and e.pressed:
			_set_scroll(_scroll + 120.0)
		elif e.button_index == MOUSE_BUTTON_LEFT:
			if e.pressed:
				_pressed = true
				_dragging = false
				_press = e.position
			elif _pressed:
				_pressed = false
				if not _dragging:
					_tap(e.position + Vector2(0, _scroll))
	elif event is InputEventMouseMotion and _pressed:
		var m := event as InputEventMouseMotion
		if _dragging or absf(m.position.y - _press.y) > 14.0:
			_dragging = true
			_set_scroll(_scroll - m.relative.y)

func _tap(point: Vector2) -> void:
	for cell in _cells:
		if (cell[0] as Rect2).has_point(point) and profile.has_met(cell[1]):
			Sound.play("ui_tap")
			_show_detail(cell[1])
			return

## ---------- detail ----------

func _show_detail(id: String) -> void:
	_species = id
	_reset_page()
	_header(func(): _show_list(), "label_name_" + id, 340.0)
	var has_extras := id != "crow"
	# Card content, top to bottom, 32 px between groups: tiles, ability row, house chip.
	var inner_w := DETAIL_CARD_W - CARD_PAD_IN * 2.0
	var card_h := CARD_PAD_IN * 2.0 + PILL_H
	if has_extras:
		card_h += TILE_H + 32.0 + GIFT_DETAIL + 32.0
	# Vertical block: caption, hero bird (standing on the card's top edge), card; centred in the free area.
	var top := MARGIN + HUD_PILL_H + 24.0
	var avail := BOARD_BOTTOM - top
	var bird_h := minf(avail - CAPTION_H - 24.0 - card_h - 48.0, 780.0)
	var block := CAPTION_H + 24.0 + bird_h + card_h
	var y := top + (avail - block) * 0.5
	_plate(_page, "label_caption_" + PERSONALITY_LABEL[id], Rect2(MARGIN, y, 1000.0, CAPTION_H))
	var bird_box := Rect2(DETAIL_CARD_X, y + CAPTION_H + 24.0, DETAIL_CARD_W, bird_h)
	var body := Rect2(DETAIL_CARD_X, bird_box.end.y, DETAIL_CARD_W, card_h)
	var k := BORDER_SCALE
	UI.panel(_page, "card_wood", body.grow_individual(CARD_PAD.x * k, CARD_PAD.y * k, CARD_PAD.z * k, CARD_PAD.w * k), k)
	_hero = _bird(_page, id, _outfit_key(id), bird_box)
	var timer := Timer.new()
	timer.wait_time = 5.0
	timer.autostart = true
	timer.timeout.connect(func(): if not _hero.acting: _hero.cheer())
	_page.add_child(timer)
	var cy := body.position.y + CARD_PAD_IN
	if has_extras:
		_tiles = Control.new()
		_tiles.mouse_filter = Control.MOUSE_FILTER_IGNORE
		_page.add_child(_tiles)
		_tile_origin = Vector2(body.position.x + CARD_PAD_IN, cy)
		_tile_w = (inner_w - TILE_GAP * 3.0) / 4.0
		_build_tiles()
		cy += TILE_H + 32.0
		# Ability row: gift coin and its caption plate, centred together.
		var label: String = "label_caption_" + ABILITY_LABEL[id]
		var used := UI.used_rect(label)
		var cap_h := minf(CAPTION_H, (inner_w - GIFT_DETAIL - 16.0) * used.size.y / used.size.x)
		var cap_w := cap_h * used.size.x / used.size.y
		var row_x := 540.0 - (GIFT_DETAIL + 16.0 + cap_w) * 0.5
		UI.art(_page, "gift_coin_" + ABILITY_GIFT[id], Rect2(row_x, cy, GIFT_DETAIL, GIFT_DETAIL))
		_plate(_page, label, Rect2(row_x + GIFT_DETAIL + 16.0, cy + (GIFT_DETAIL - cap_h) * 0.5, cap_w, cap_h))
		cy += GIFT_DETAIL + 32.0
	_count_chip(_page, Vector2(540.0 - CHIP_W * 0.5, cy), profile.flocks_home(id))

var _tile_origin := Vector2.ZERO
var _tile_w := 174.0

func _build_tiles() -> void:
	for c in _tiles.get_children():
		c.queue_free()
	for n in 4:
		var id := "outfit.%s.default" % _species if n == 0 else "outfit.%s.%d" % [_species, n]
		var owned := n == 0 or profile.owns_outfit(id)
		var equipped: bool = profile.equipped.get(_species, "outfit.%s.default" % _species) == id
		var r := Rect2(_tile_origin + Vector2(n * (_tile_w + TILE_GAP), 0), Vector2(_tile_w, TILE_H))
		UI.panel(_tiles, "card_wood", r, 0.22)
		var fit := Rect2(r.position + Vector2(14, 16), r.size - Vector2(28, 30))
		var outfit := "" if n == 0 else "%s_outfit_%d" % [_species, n]
		if owned:
			_bird(_tiles, _species, outfit, fit)
		else:
			_silhouette(_tiles, _species, fit)
			UI.art(_tiles, "padlock", Rect2(r.get_center() - Vector2(26, 26), Vector2(52, 52)))
		if equipped:
			var edge := Panel.new()
			var style := StyleBoxFlat.new()
			style.draw_center = false
			style.set_border_width_all(8)
			style.border_color = Color(0.45, 0.72, 0.16)
			style.set_corner_radius_all(30)
			edge.add_theme_stylebox_override("panel", style)
			edge.mouse_filter = Control.MOUSE_FILTER_IGNORE
			UI.place(edge, r.grow(2))
			_tiles.add_child(edge)
			UI.art(_tiles, "badge_check", Rect2(r.end - Vector2(52, 52), Vector2(56, 56)))
		elif owned:
			var pick := id
			UI.hit_area(_tiles, r, func(): _equip(pick))

func _equip(id: String) -> void:
	Shop.equip(profile, id)
	profile_changed.emit()
	_hero.setup(Board.bird(0, Board.species_index(_species), 0), false, _hero.density, _outfit_key(_species))
	_hero.cheer()
	_build_tiles()

## ---------- capture preview ----------

static func preview(main: Control, state: String) -> void:
	var p := Profile.new()
	p.coins = 240
	for card in Aviary.SPECIES:
		p.met[card[0]] = true
	p.homes = {"sunny": 12, "sky": 9, "berry": 7, "mint": 5, "cocoa": 3, "snow": 1}
	if state == "list_some_unmet":
		p.met = {"sunny": true, "sky": true, "berry": true}
	if state == "detail_berry":
		p.outfits = {"outfit.berry.1": true}
		p.equipped = {"berry": "outfit.berry.1"}
	var view: Control = load("res://game/aviary_view.gd").new()
	view.setup(p)
	main.set_screen(view)
	match state:
		"list_scrolled": view._set_scroll(1e6)
		"detail_berry": view._show_detail("berry")
		"detail_crow": view._show_detail("crow")
		"detail_hoot": view._show_detail("hoot")
