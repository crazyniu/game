extends Control
## Aviary: a scrolling two-column list of species, and a detail page with outfits, ability and homecomings.

signal closed
signal profile_changed

const BirdViewScript := preload("res://game/bird_view.gd")
const LIST := "aviary_list"
const DETAIL := "aviary_detail"
const ROW_PITCH := 472.0
const FRAME := Rect2(5, 116, 1070, 1850)   # board art placed so its cream opening starts at the mockup's panel top
const CARD_PAD := Vector4(15, 48, 15, 4)   # transparent margins of card_wood (left, top, right, bottom)
const CARD_SCALE := 0.55
const CREAM := Color(0.97, 0.88, 0.69)
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
var _row_pad := 0.0   # gap above the first row so its ring starts where the mockup's does


func setup(p: Profile) -> void:
	profile = p
	size = Vector2(1080, 1920)
	UI.image(self, "bg_level_w1_noon", Rect2(0, 0, 1080, 1920), false)
	for right in [false, true]:
		var crown := UI.image(self, "tree_crown_w1", Rect2(-80 + (right as int) * 920, -150, 320, 420))
		crown.flip_h = right
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

func _header(close_rect: Rect2, on_close: Callable) -> void:
	UI.panel(_page, "stat_pill", Layout.r(LIST, "coin_pill"))
	UI.art(_page, "coin", Layout.r(LIST, "coin"))
	_coin_number = UI.number(_page, str(profile.coins), Layout.r(LIST, "coin_value"))
	UI.disc(_page, "Close", close_rect, "icon_close", on_close, "btn_wood", 0.48)

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
	UI.art(_page, "aviary_board", FRAME)
	_header(Layout.r(LIST, "close"), func(): closed.emit())
	_plate(_page, "label_title_aviary", Layout.r(LIST, "title_ribbon").grow_individual(0, 8, 0, 8))
	var opening := _opening(FRAME)
	opening.size.y = Layout.r(LIST, "cocoa_ability").end.y + 2.0 - opening.position.y   # exactly three rows, no peek of the fourth
	_row_pad = Layout.r(LIST, "sunny_ring").position.y - opening.position.y
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
	for i in Aviary.SPECIES.size():
		_cell(Aviary.SPECIES[i][0], i % 2, i / 2, opening.position)
	_content.size = Vector2(opening.size.x, _row_pad * 2 + ROW_PITCH * 5)
	_max_scroll = maxf(0.0, _content.size.y - opening.size.y)
	_edge(opening, false)
	_edge(opening, true)
	_build_track(opening)
	_set_scroll(_list_scroll)

## Cream opening of the board art, derived from its measured inner rect.
func _opening(frame: Rect2) -> Rect2:
	var m := Art.metrics("aviary_board")
	var o: Array = m["opaque"]
	var inner: Array = m["inner"]
	var k := Vector2(frame.size.x / o[2], frame.size.y / o[3])
	return Rect2(frame.position + Vector2(inner[0] - o[0], inner[1] - o[1]) * k, Vector2(inner[2] * k.x, inner[3] * k.y))

func _cell(id: String, col: int, row: int, origin: Vector2) -> void:
	var key := "sunny" if col == 0 else "skye"
	var met := profile.has_met(id)
	var ring := Layout.r(LIST, key + "_ring")
	var cell_top := _row_pad + row * ROW_PITCH
	var shift := Vector2(-origin.x, cell_top - ring.position.y)
	var at := func(suffix: String) -> Rect2:
		var r := Layout.r(LIST, key + "_" + suffix)
		return Rect2(r.position + shift, r.size)
	UI.art(_content, "badge_ring_gold", at.call("ring"))
	var bird_fit: Rect2 = (at.call("bird") as Rect2).grow_individual(8, 14, 8, 0)
	if met:
		_bird(_content, id, _outfit_key(id), bird_fit)
	else:
		_silhouette(_content, id, bird_fit)
	_plate(_content, "label_name_" + (id if met else "unknown"), at.call("ribbon"))
	var pill: Rect2 = at.call("count")
	UI.art(_content, "house_count_chip", pill)
	var num: Rect2 = at.call("number")
	UI.number(_content, str(profile.flocks_home(id)), Rect2(pill.position.x + 100, num.position.y, 80, num.size.y))
	if met and ABILITY_GIFT.has(id):
		UI.art(_content, "gift_coin_" + ABILITY_GIFT[id], at.call("ability"))
	var top: Rect2 = at.call("ring")
	_cells.append([Rect2(top.position, Vector2(top.size.x, (at.call("ability") as Rect2).end.y - top.position.y)), id])

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

## Soft cream fade at the top or bottom of the scroll window.
func _edge(opening: Rect2, bottom: bool) -> void:
	var g := Gradient.new()
	g.colors = PackedColorArray([Color(CREAM, 1.0), Color(CREAM, 0.0)] if not bottom else [Color(CREAM, 0.0), Color(CREAM, 1.0)])
	var tex := GradientTexture2D.new()
	tex.gradient = g
	tex.fill_from = Vector2(0, 0)
	tex.fill_to = Vector2(0, 1)
	var t := TextureRect.new()
	t.texture = tex
	t.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	t.stretch_mode = TextureRect.STRETCH_SCALE
	t.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var h := 8.0 if bottom else 26.0
	UI.place(t, Rect2(opening.position.x, opening.end.y - h if bottom else opening.position.y, opening.size.x, h))
	_page.add_child(t)

func _build_track(opening: Rect2) -> void:
	_track = Rect2(opening.end.x - 32, opening.position.y + 50, 20, opening.size.y - 100)
	_pill(_track, Color(0.8, 0.64, 0.4, 0.7))
	_thumb = _pill(Rect2(_track.position.x - 3, _track.position.y, 26, 100), Color(1.0, 0.72, 0.16))

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
	var has_extras := id != "crow"
	var body := Rect2(104, 1035, 872, 827 if has_extras else 430)
	var k := CARD_SCALE
	UI.panel(_page, "card_wood", body.grow_individual(CARD_PAD.x * k, CARD_PAD.y * k, CARD_PAD.z * k, CARD_PAD.w * k), k)
	_hero = _bird(_page, id, _outfit_key(id), Rect2(150, 425, 780, 625))
	_header(Rect2(922, 77, 115, 115), func(): _show_list())
	_plate(_page, "label_name_" + id, Layout.r(DETAIL, "berry_ribbon"))
	_plate(_page, "label_caption_" + PERSONALITY_LABEL[id], Layout.r(DETAIL, "description"))
	var timer := Timer.new()
	timer.wait_time = 5.0
	timer.autostart = true
	timer.timeout.connect(func(): if not _hero.acting: _hero.cheer())
	_page.add_child(timer)
	if has_extras:
		_tiles = Control.new()
		_tiles.mouse_filter = Control.MOUSE_FILTER_IGNORE
		_page.add_child(_tiles)
		_build_tiles()
		UI.art(_page, "gift_coin_" + ABILITY_GIFT[id], Layout.r(DETAIL, "ability_disc"))
		_plate(_page, "label_caption_" + ABILITY_LABEL[id], Layout.r(DETAIL, "ability_caption"))
	var pill := Layout.r(DETAIL, "house_pill")
	if not has_extras:
		pill.position.y = 1230
	pill = Rect2(pill.get_center() - Vector2(165, 69), Vector2(330, 138))
	UI.art(_page, "house_count_chip", pill)
	UI.number(_page, str(profile.flocks_home(id)), Rect2(pill.position.x + 215, pill.position.y + 32, 90, 71))

func _build_tiles() -> void:
	for c in _tiles.get_children():
		c.queue_free()
	var tile := Vector2(184, 230)
	var gap := 12.0
	var x0 := 141.0 + (796.0 - (tile.x * 4 + gap * 3)) * 0.5
	for n in 4:
		var id := "outfit.%s.default" % _species if n == 0 else "outfit.%s.%d" % [_species, n]
		var owned := n == 0 or profile.owns_outfit(id)
		var equipped: bool = profile.equipped.get(_species, "outfit.%s.default" % _species) == id
		var r := Rect2(x0 + n * (tile.x + gap), 1140, tile.x, tile.y)
		UI.panel(_tiles, "card_wood", r, 0.22)
		var fit := Rect2(r.position + Vector2(14, 16), r.size - Vector2(28, 30))
		var outfit := "" if n == 0 else "%s_outfit_%d" % [_species, n]
		if owned:
			_bird(_tiles, _species, outfit, fit)
		else:
			_silhouette(_tiles, _species, fit)
			UI.art(_tiles, "padlock", Rect2(r.get_center() + Vector2(-26, -10), Vector2(52, 60)))
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
			UI.art(_tiles, "badge_check", Rect2(r.end - Vector2(66, 56), Vector2(70, 64)))
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
