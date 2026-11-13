extends Control
## Map (docs/design/map.md): ten equal world cards in a horizontal strip, and, when a card is opened, that
## world's page: one connected tree with ten numbered branch landings. Level numbers restart at 1 in every
## world; the global level id and the saves are unchanged. Words are painted labels, numbers painted glyphs.

signal level_tapped(level: int)
signal shop_requested
signal aviary_requested
signal settings_requested
signal help_requested
signal home_requested
signal gate_tapped(world: int)

const WORLD_NAMES := ["world.green.name", "world.bamboo.name", "world.autumn.name", "world.snow.name", "world.blossom.name",
	"world.mushroom.name", "world.lakeside.name", "world.pine.name", "world.rainforest.name", "world.wisteria.name"]
## World page: ten painted level tiles in a 2 x 5 grid, in reading order (1 2 / 3 4 / ...), tile centres in design px.
const GRID_X := [370.0, 710.0]
const GRID_TOP := 486.0
const GRID_PITCH := 306.0
const TILE := 184.0
const STAR_DROP := 8.0   # gap between a tile and its row of stars

## Equal-sized forest cards; docs/design/map.md owns the horizontal layout.
const BACK := Rect2(48, 48, 112, 112)
const STRIP := Rect2(0, 300, 1080, 1096)
const CARD := Vector2(626, 900)
const CARD_TOP := 140.0                 # within the strip; the paintings start at screen y=440
const CARD_GAP := 40.0
const SIDE_MARGIN := (1080.0 - CARD.x) * 0.5
const STRIDE := CARD.x + CARD_GAP
const WINDOW := Rect2(24, 24, 578, 852)  # the painting fills the card inside its rim
const WINDOW_RADIUS := 30.0
const CARD_CLIP := preload("res://game/map_card_clip.gdshader")
const PLAQUE := Vector2(520, 124)        # the light-green name banner over the card's lower part
const PLAQUE_Y := 810.0                  # plank centre from the card top
const PLAQUE_WORD := Rect2(0.16, 0.1, 0.68, 0.74)   # word area on the plank, shares of the plank box
const BADGE := 140.0                     # the world number disc on the card's top edge
const BADGE_Y := 12.0                    # badge centre below the card top
const STAR_PROGRESS := Rect2(93, 620, 440, 96) # inside each painting, above its wooden name
const PLAY := Rect2(220, 1424, 640, 216)  # one fixed action for the selected forest
const TAP_SLOP := 18.0
const SCROLL_STEP := 112.0
const TREE_ORIGIN := Vector2(-12.96, 308.0)
const TREE_SCALE := 1.08
const TREE_SOURCE := Vector2(1024, 1536)
const TREE_CONTENT := Rect2(33, 63, 958, 1420)
const NODE := 380.0
const NODE_CAP := 64.0
const NODE_HIT_SIZE := Vector2(396, 298)
const NODE_HIT_OFFSET := Vector2(198, 244)
const PERCH_OFFSET := Vector2(0, -55.0 * TREE_SCALE)
const STAR := 70.0
const STAR_GAP := 6.0
const BIRD_LIMIT := Vector2(250, 250)
const BIRD_GEOMETRY_PATH := "res://layout/map_bird_geometry.json"
static var _bird_geometry: Dictionary = {}

var profile: Profile
var current_level := 1
var _world := 1                          # requested or opened world (1..10)
var _scroll_x := 0.0
var _max_scroll := 0.0
var _cards: Array = []
var _strip: Control
var _content: Control
var _page: Control
var _bar: Control
var _coins: FeatherBar
var _scroll_tween: Tween
var _pressed := false
var _travel := 0.0
var _bird: BirdView
var _play_button: Button


func setup(player: Profile) -> void:
	if _scroll_tween != null:
		_scroll_tween.kill()
	profile = player
	for child in get_children():
		remove_child(child)
		child.queue_free()
	_cards.clear()
	_page = null
	_bird = null
	_pressed = false
	_travel = 0.0
	size = Vector2(1080, 1920)
	clip_contents = true
	mouse_filter = Control.MOUSE_FILTER_STOP
	var backdrop := UI.image(self, "background_map_forest", Rect2(0, 0, 1080, 1920))
	backdrop.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_COVERED
	var next := Progress.next_level(profile)
	current_level = next if next > 0 else Levels.COUNT
	_world = Progress.world_of_level(current_level)
	_build_strip()
	_build_play()
	_build_bar()
	show_world(_world, true)

func _notification(what: int) -> void:
	if what == NOTIFICATION_APPLICATION_FOCUS_OUT:
		_pressed = false
		_travel = 0.0
	if what == NOTIFICATION_TRANSLATION_CHANGED and is_node_ready() and profile != null:
		_refresh_language.call_deferred()

func _refresh_language() -> void:
	if not is_inside_tree():
		return
	var keep := _world
	var scroll := _scroll_x
	var was_page := _page != null
	setup(profile)
	_world = keep
	_refresh_selection()
	_set_scroll(scroll)
	if was_page:
		open_world(keep)

## True while silent captures run: no tweens, birds hold still.
func _instant() -> bool:
	var n := get_parent()
	while n != null:
		if n.has_method("instant"):
			return n.instant()
		n = n.get_parent()
	return false


# ---------------------------------------------------------------- bar (shared by carousel and world page)

func _build_bar() -> void:
	_bar = Control.new()
	_bar.name = "Bar"
	_bar.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_bar.size = size
	add_child(_bar)
	_coins = ShopSkin.counter(_bar, profile)
	MenuNavigation.back(_bar, "Home", BACK.position, _back)

## Back arrow: from a world page to the cards, from the cards to the home page.
func _back() -> void:
	if _page != null:
		close_world()
	else:
		home_requested.emit()


# ---------------------------------------------------------------- horizontal forest strip

func _world_stars(world: int) -> int:
	var total := 0
	for level in range((world - 1) * Levels.PER_WORLD + 1, world * Levels.PER_WORLD + 1):
		total += profile.best_stars(level)
	return total

func _world_cleared(world: int) -> bool:
	for level in range((world - 1) * Levels.PER_WORLD + 1, world * Levels.PER_WORLD + 1):
		if profile.best_stars(level) == 0:
			return false
	return true

func _build_strip() -> void:
	_strip = Control.new()
	_strip.name = "WorldStrip"
	_strip.clip_contents = true
	_strip.mouse_filter = Control.MOUSE_FILTER_STOP
	UI.place(_strip, STRIP)
	_strip.gui_input.connect(_on_scroll_input)
	add_child(_strip)
	_content = Control.new()
	_content.name = "Cards"
	_content.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_content.size = Vector2(SIDE_MARGIN * 2.0 + Levels.WORLDS * STRIDE - CARD_GAP, STRIP.size.y)
	_strip.add_child(_content)
	_max_scroll = _content.size.x - STRIP.size.x
	for world in range(1, Levels.WORLDS + 1):
		_cards.append(_build_card(world))

func _build_card(world: int) -> Control:
	var open := Progress.is_world_unlocked(profile, world)
	var card := Control.new()
	card.name = "World_%d" % world
	card.mouse_filter = Control.MOUSE_FILTER_IGNORE
	card.size = CARD
	card.position = Vector2(SIDE_MARGIN + (world - 1) * STRIDE, CARD_TOP)
	_content.add_child(card)
	Island.nine(card, "island_grid_card", Rect2(Vector2.ZERO, CARD), Vector4(60, 60, 60, 60)).name = "Frame"
	var chosen := Island.nine(card, "island_grid_card_selected", Rect2(Vector2.ZERO, CARD), Vector4(60, 60, 60, 60))
	chosen.name = "Chosen"
	var current := world == _world
	chosen.visible = current
	var window := Control.new()
	window.name = "Window"
	window.clip_contents = true
	window.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(window, WINDOW)
	card.add_child(window)
	var scene := Art.tex("card_scene_w%d" % world)
	var tall := WINDOW.size.x * scene.get_height() / scene.get_width()
	var painting := UI.image(window, "card_scene_w%d" % world, Rect2(0, -(tall - WINDOW.size.y) * 0.5, WINDOW.size.x, tall), false)
	painting.name = "Painting"
	var mask := ShaderMaterial.new()
	mask.shader = CARD_CLIP
	mask.set_shader_parameter("window_size", WINDOW.size)
	mask.set_shader_parameter("image_size", painting.size)
	mask.set_shader_parameter("image_offset", painting.position)
	mask.set_shader_parameter("radius", WINDOW_RADIUS)
	painting.material = mask
	_star_progress(card, world, STAR_PROGRESS)
	var plaque := Rect2(Vector2(CARD.x * 0.5, PLAQUE_Y) - PLAQUE * 0.5, PLAQUE)
	ButtonKit.banner(card, plaque).name = "Plaque"
	var word := Rect2(plaque.position + plaque.size * PLAQUE_WORD.position, plaque.size * PLAQUE_WORD.size)
	ButtonKit.lettering(card, WORLD_NAMES[world - 1].replace(".name", "").replace(".", "_"), word.grow_individual(10.0, 6.0, 10.0, 6.0)).name = "WorldName"
	# Green marks the next playable forest; browsing never changes a card's size or status.
	var at := Rect2(CARD.x * 0.5 - BADGE * 0.5, BADGE_Y - BADGE * 0.5, BADGE, BADGE)
	var badge := UI.image(card, "world_medallion", at.grow(26.0), false)
	badge.name = "WorldNumber"
	var size := BADGE + 52.0
	ButtonKit.number(badge, str(world), Rect2(size * 0.27, size * 0.28, size * 0.46, size * 0.42)).name = "Digits"
	if not open:
		window.modulate = Color(0.62, 0.62, 0.66)
		Island.piece_fit(card, "island_padlock", Vector2(CARD.x * 0.5, WINDOW.get_center().y - 60.0), Vector2(220, 220)).name = "Lock"
	elif _world_cleared(world):
		Island.check_badge(card, Vector2(CARD.x - 50.0, 50.0)).name = "Done"

	return card

## Fit the shared painted track and its overhanging star into the existing progress slot.
func _star_progress(parent: Control, world: int, bounds: Rect2) -> void:
	var stars := _world_stars(world)
	var total := Levels.PER_WORLD * 3
	# Build at the shared component's reference height so its nine-patch corners do not set the final size.
	var height := 112.0
	var star := height * 1.25
	var fit := bounds.size.y / star
	var inset := Vector2(star * 0.1, (star - height) * 0.5)
	var track := Rect2(bounds.position + inset * fit, Vector2(bounds.size.x / fit - inset.x, height))
	var progress := Island.progress_bar(parent, track, float(stars) / total, "%d/%d" % [stars, total])
	progress.name = "Stars"
	progress.pivot_offset = Vector2.ZERO
	progress.scale = Vector2.ONE * fit

## A single fixed action belongs to the selection, never to a carousel card.
func _build_play() -> void:
	_play_button = UI.art_button(self, "Play", "btn_map_play", PLAY, func(): _play(_world))
	UI.art(_play_button, "label_home_island_play_" + UiText.locale(), Rect2(164, 40, 312, 120))

func _refresh_selection() -> void:
	for i in _cards.size():
		var selected := i + 1 == _world
		_cards[i].get_node("Chosen").visible = selected
		_cards[i].get_node("WorldNumber").modulate = Color.WHITE if selected else Color(0.85, 0.85, 0.85)
	if is_instance_valid(_play_button):
		UI.set_active(_play_button, Progress.is_world_unlocked(profile, _world))

func _play(world: int) -> void:
	if _travel >= TAP_SLOP:
		return
	if Progress.is_world_unlocked(profile, world):
		open_world(world)
	else:
		Sound.play("error")
		gate_tapped.emit(world)

func _set_scroll(value: float) -> void:
	_scroll_x = clampf(value, 0.0, _max_scroll)
	_content.position.x = -_scroll_x
	if _pressed:
		_world = clampi(roundi(_scroll_x / STRIDE) + 1, 1, Levels.WORLDS)
		_refresh_selection()

## Selection recentres equal cards while the sole Play action remains fixed.
func show_world(world: int, instant := false) -> void:
	_world = clampi(world, 1, Levels.WORLDS)
	_refresh_selection()
	if _scroll_tween != null:
		_scroll_tween.kill()
	var target := clampf((_world - 1) * STRIDE, 0.0, _max_scroll)
	if instant or _instant():
		_set_scroll(target)
		return
	_scroll_tween = create_tween()
	_scroll_tween.tween_method(_set_scroll, _scroll_x, target, 0.3).set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)

## Reset before child buttons handle a press; release outside the strip cannot leave dragging stuck.
func _input(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.pressed:
		_pressed = false
		_travel = 0.0
		if _scroll_tween != null:
			_scroll_tween.kill()
	elif event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and not event.pressed:
		if _pressed and not _strip.get_global_rect().has_point(event.position):
			_pressed = false
			show_world(_world)

func _on_scroll_input(event: InputEvent) -> void:
	if _page != null:
		return
	if event is InputEventMouseButton:
		var e := event as InputEventMouseButton
		if e.pressed and e.button_index in [MOUSE_BUTTON_WHEEL_UP, MOUSE_BUTTON_WHEEL_LEFT]:
			show_world(_world - 1)
		elif e.pressed and e.button_index in [MOUSE_BUTTON_WHEEL_DOWN, MOUSE_BUTTON_WHEEL_RIGHT]:
			show_world(_world + 1)
		elif e.button_index == MOUSE_BUTTON_LEFT:
			if e.pressed:
				_pressed = true
			elif _pressed:
				_pressed = false
				if _travel < TAP_SLOP:
					_tap(e.position + Vector2(_scroll_x, 0))
				else:
					show_world(_world)
	elif event is InputEventMouseMotion and event.button_mask & MOUSE_BUTTON_MASK_LEFT:
		var m := event as InputEventMouseMotion
		_travel += absf(m.relative.x)
		if _travel >= TAP_SLOP:
			_set_scroll(_scroll_x - m.relative.x)
	elif event is InputEventPanGesture:
		var delta: Vector2 = event.delta
		_set_scroll(_scroll_x + (delta.x if absf(delta.x) >= absf(delta.y) else delta.y) * SCROLL_STEP)
		_world = clampi(roundi(_scroll_x / STRIDE) + 1, 1, Levels.WORLDS)
		_refresh_selection()

func _tap(at: Vector2) -> void:
	for i in _cards.size():
		var card: Control = _cards[i]
		var box := Rect2(card.position, CARD)
		if not box.has_point(at):
			continue
		Sound.play("ui_tap")
		show_world(i + 1)
		return



# ---------------------------------------------------------------- world page

## A connected tree replaces the rejected road; Back restores the same approved world card.
func open_world(world: int) -> void:
	if _page != null:
		_page.queue_free()
	_world = world
	_strip.visible = false
	_play_button.visible = false
	_page = Control.new()
	_page.name = "WorldPage"
	_page.mouse_filter = Control.MOUSE_FILTER_STOP
	_page.size = size
	add_child(_page)
	move_child(_page, _bar.get_index())
	ButtonKit.sign(_page, WORLD_NAMES[world - 1].replace(".name", "").replace(".", "_"), Rect2(240, 96, 600, 209)).name = "WorldName"
	var text := "%d/%d" % [_world_stars(world), Levels.PER_WORLD * 3]
	var w := UI.number_width(text, 44.0)
	_star_progress(_page, world, Rect2(540.0 - (w + 140.0) * 0.5, 300.0, w + 140.0, 72.0))
	for n in Levels.PER_WORLD:
		_build_node((world - 1) * Levels.PER_WORLD + n + 1)
	# Local sibling order keeps the current marker above locks without crossing modal layers.
	var current := _page.get_node_or_null("Level %d" % current_level)
	if current != null:
		_page.move_child(current, _page.get_child_count() - 1)
	if not _instant():
		_page.modulate.a = 0.0
		_page.create_tween().tween_property(_page, "modulate:a", 1.0, 0.2)

func close_world() -> void:
	if _page == null:
		return
	_page.queue_free()
	_page = null
	_bird = null
	_strip.visible = true
	_play_button.visible = true
	show_world(_world, true)

func _base_node_pos(level: int) -> Vector2:
	var n := (level - 1) % Levels.PER_WORLD
	return Vector2(GRID_X[n % 2], GRID_TOP + (n / 2) * GRID_PITCH)

func _node_pos(level: int) -> Vector2:
	return _base_node_pos(level)

## The painted level tile.
func _landing_bounds(center: Vector2) -> Rect2:
	return Rect2(center - Vector2.ONE * TILE * 0.5, Vector2.ONE * TILE)

func _build_node(level: int) -> void:
	var state := Progress.node_state(profile, level)
	var c := _node_pos(level)
	var shown := (level - 1) % Levels.PER_WORLD + 1
	var layer := Control.new()
	layer.name = "Level %d" % level
	layer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_page.add_child(layer)
	if state == "current" and level == current_level:
		var glow := UI.image(layer, "glow_soft", _landing_bounds(c).grow(TILE * 0.32))
		glow.name = "Glow"
		glow.modulate = Color(1.0, 0.92, 0.45, 0.9)
	var landing := UI.image(layer, "level_tile_locked" if state == "locked" else "level_tile_open", _landing_bounds(c), false)
	landing.name = "Node"
	if state == "locked":
		ButtonKit.number(layer, str(shown), Rect2(c - Vector2(TILE * 0.32, TILE * 0.2), Vector2(TILE * 0.64, TILE * 0.4))).modulate = Color(1, 1, 1, 0.55)
		UI.art(layer, "island_padlock", Rect2(c + Vector2(TILE * 0.22, -TILE * 0.62), Vector2(64, 64))).name = "Lock"
	else:
		ButtonKit.number(layer, str(shown), Rect2(c - Vector2(TILE * 0.34, TILE * 0.25), Vector2(TILE * 0.68, TILE * 0.46))).name = "Number"
	var perch := _bird_feet(c)
	if state != "locked":
		_node_stars(layer, perch, profile.best_stars(level) if state == "cleared" else 0)
	if state == "current" and level == current_level:
		var side := -1.0 if c.x < 540.0 else 1.0
		_add_bird(layer, c + Vector2(side * (TILE * 0.5 + 100.0), TILE * 0.46), _level_species(level))
	_hit(layer, c, level)

## Feet, locks and rating stars share the measured branch plane.
func _bird_feet(center: Vector2) -> Vector2:
	return center + Vector2(0, TILE * 0.5 + STAR_DROP + STAR)

## Uniform visible width, natural aspect and a shared lower ink edge on the branch plane.
func _star_bounds(perch: Vector2) -> Rect2:
	var width := STAR * 3.0 + STAR_GAP * 2.0
	return Rect2(perch - Vector2(width * 0.5, STAR), Vector2(width, STAR))

func _node_stars(layer: Control, perch: Vector2, filled: int) -> void:
	var bounds := _star_bounds(perch)
	for index in 3:
		var picture := "island_star_full" if index < filled else "island_star_empty"
		var ink := UI.used_rect(picture)
		var height := STAR * ink.size.y / ink.size.x
		var at := Vector2(bounds.position.x + STAR * 0.5 + index * (STAR + STAR_GAP), bounds.end.y)
		UI.art(layer, picture, Rect2(at - Vector2(STAR * 0.5, height), Vector2(STAR, height)), ink).name = "Star_%d" % index

## Tight painted-ink widths centre both single and two-digit local level numbers.
func _white_number(layer: Control, value: int, center: Vector2, cap: float) -> void:
	var text := str(value)
	var gap := cap * 0.08
	var width := gap * (text.length() - 1)
	for digit in text:
		var ink := UI.used_rect(Island.DIGIT + digit)
		width += ink.size.x / ink.size.y * cap
	var group := Control.new()
	group.name = "Number"
	group.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(group, Rect2(center - Vector2(width, cap) * 0.5, Vector2(width, cap)))
	layer.add_child(group)
	var left := 0.0
	for digit in text:
		var key := Island.DIGIT + digit
		var ink := UI.used_rect(key)
		var w := ink.size.x / ink.size.y * cap
		UI.art(group, key, Rect2(left, 0, w, cap), ink)
		left += w + gap

## Includes the full animated bird, landing and rating; adjacent tiers remain disjoint.
func _hit_bounds(center: Vector2) -> Rect2:
	return Rect2(center - Vector2(TILE * 0.5 + 20.0, TILE * 0.5 + 20.0), Vector2(TILE + 40.0, TILE + STAR_DROP + STAR + 30.0))

func _hit(layer: Control, center: Vector2, level: int) -> void:
	var button := UI.hit_area(layer, _hit_bounds(center),
		func(): level_tapped.emit(level))
	button.name = "Tap"
	button.disabled = Progress.node_state(profile, level) == "locked"

## The first bird species of the level, so the node shows who the player is about to meet.
func _level_species(level: int) -> String:
	var ids := Levels.species_ids(Levels.create_board(Levels.get_level(level)))
	return ids[0] if not ids.is_empty() else "sunny"

func _bird_spec(species: String) -> Dictionary:
	if _bird_geometry.is_empty():
		_bird_geometry = JSON.parse_string(FileAccess.get_file_as_string(BIRD_GEOMETRY_PATH))["species"]
	return _bird_geometry[species]

func _bird_fit(species: String) -> float:
	var bounds: Array = _bird_spec(species)["animation"]
	return minf(BIRD_LIMIT.x / float(bounds[2]), BIRD_LIMIT.y / float(bounds[3]))

func _bird_origin(feet: Vector2, species: String) -> Vector2:
	var data: Array = _bird_spec(species)["front"]
	var front := Rect2(data[0], data[1], data[2], data[3])
	return feet - Vector2(front.get_center().x - BirdView.CANVAS * 0.5,
		front.end.y - BirdView.FEET_Y) * _bird_fit(species)

func _add_bird(layer: Control, feet: Vector2, species: String) -> void:
	_bird = BirdView.new()
	_bird.name = "Bird"
	_bird.position = _bird_origin(feet, species)
	layer.add_child(_bird)
	_bird.setup(Board.bird(0, Board.species_index(species), 0), false,
		_bird_fit(species) * BirdView.CANVAS / BirdView.DISPLAY)
	_bird.quiet = _instant()


# ---------------------------------------------------------------- capture preview

static func preview(main: Control, state: String) -> void:
	var p := Profile.new()
	var cleared := 0
	p.coins = 1240
	match state:
		"cards": cleared = 7
		"cards_late": cleared = 34
		"cards_all": cleared = Levels.COUNT
		"cards_locked": cleared = 7
		"world_page": cleared = 7
		"world_page_late": cleared = 34
	for level in range(1, cleared + 1):
		p.stars[level] = [3, 3, 2, 3, 1][(level - 1) % 5]
	p.highest_unlocked = mini(Levels.COUNT, cleared + 1)
	var view: Control = load("res://game/map_view.gd").new()
	main.set_screen(view)
	view.setup(p)
	if state == "cards_locked":
		view.show_world(2, true)
	if state.begins_with("world_page"):
		view.open_world(Progress.world_of_level(view.current_level))
