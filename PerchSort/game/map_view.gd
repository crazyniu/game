extends Control
## Map (docs/design/map.md): ten equal world cards in a horizontal strip, and, when a card is opened, that
## world's page: its painting on one screen with ten level nodes along the path. Level numbers restart at 1 in every
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
## Node centres per trail shape (x, y within the 1080 x 1620 painting), levels 1..10 in walking order from the bottom.
const NODES := [
	[[770, 1315], [290, 1321], [290, 1084], [770, 1099], [770, 881], [290, 866], [290, 663], [770, 650], [770, 450], [290, 448]],
	[[250, 1129], [540, 1130], [830, 1115], [770, 882], [290, 875], [250, 637], [540, 638], [830, 620], [770, 407], [290, 402]],
	[[250, 1127], [540, 1121], [830, 1120], [770, 896], [290, 895], [250, 689], [540, 666], [830, 652], [770, 456], [290, 447]],
	[[250, 1133], [540, 1119], [830, 1101], [770, 910], [290, 895], [250, 697], [540, 680], [830, 658], [770, 483], [290, 462]],
]
const TRAIL_OF_WORLD := [0, 1, 2, 3, 0, 1, 2, 3, 0, 1]

## Equal-sized forest cards; docs/design/map.md owns the horizontal layout.
const BACK := Rect2(48, 48, 112, 112)
const STRIP := Rect2(0, 300, 1080, 1256)
const CARD := Vector2(626, 844)
const CARD_TOP := 128.0                 # within the strip; the paintings start at screen y=428
const CARD_GAP := 40.0
const SIDE_MARGIN := 64.0
const STRIDE := CARD.x + CARD_GAP
const WINDOW := Rect2(26, 26, 574, 792)  # the painting fills the card inside its rim
const PLAQUE := Vector2(600, 200)        # the wooden name plank over the card's lower part
const PLAQUE_Y := 741.0                  # plank centre from the card top
const PLAQUE_WORD := Rect2(0.16, 0.1, 0.68, 0.74)   # word area on the plank, shares of the plank box
const BADGE := 140.0                     # the world number disc on the card's top edge
const BADGE_Y := 12.0                    # badge centre below the card top
const STAR_PROGRESS := Rect2(113, 550, 400, 96) # inside each painting, above its wooden name
const PLAY := Rect2(103, 880, 420, 160)  # one painted Play per card, at the same size in every state
const TAP_SLOP := 18.0
const SCROLL_STEP := 112.0
const PAGE_TOP := 250.0                  # world page: the painting starts under the bar
const NODE := 150.0
const CURRENT := 196.0
const NODE_CAP := 54.0
const CURRENT_CAP := 70.0
const STAR := 44.0
const STAR_GAP := -6.0
const STAR_DY := 31.0
const BIRD_FEET := 84.0
const BIRD_SCALE := 1.0
const BOUNCE := 10.0
const LOCK_COLOR := Color8(150, 110, 70)

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


func setup(player: Profile) -> void:
	if _scroll_tween != null:
		_scroll_tween.kill()
	profile = player
	for child in get_children():
		child.queue_free()
	_cards.clear()
	_page = null
	_bird = null
	_pressed = false
	_travel = 0.0
	size = Vector2(1080, 1920)
	clip_contents = true
	mouse_filter = Control.MOUSE_FILTER_STOP
	UI.image(self, "island_map_backdrop", Rect2(0, 0, 1080, 1920), false)
	var next := Progress.next_level(profile)
	current_level = next if next > 0 else Levels.COUNT
	_world = Progress.world_of_level(current_level)
	_build_strip()
	_build_bar()
	show_world(_world, true)

func _notification(what: int) -> void:
	if what == NOTIFICATION_TRANSLATION_CHANGED and is_node_ready() and profile != null:
		var keep := _world
		var scroll := _scroll_x
		var was_page := _page != null
		setup(profile)
		_world = keep
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
	Island.button(_bar, "Home", "island_back_tile", BACK, _back)

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
	card.size = Vector2(CARD.x, PLAY.end.y)
	card.position = Vector2(SIDE_MARGIN + (world - 1) * STRIDE, CARD_TOP)
	_content.add_child(card)
	Island.nine(card, "island_grid_card_soft", Rect2(Vector2.ZERO, CARD), Vector4(60, 60, 60, 60)).name = "Frame"
	var chosen := Island.nine(card, "island_grid_card_selected", Rect2(Vector2.ZERO, CARD), Vector4(60, 60, 60, 60))
	chosen.name = "Chosen"
	var current := world == Progress.world_of_level(current_level)
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
	_star_progress(card, world, STAR_PROGRESS)
	var plank := Island.piece_fit(card, "island_name_plank", Vector2(CARD.x * 0.5, PLAQUE_Y), PLAQUE)
	plank.name = "Plaque"
	var word := Rect2(plank.position + plank.size * PLAQUE_WORD.position, plank.size * PLAQUE_WORD.size)
	UI.heading(card, WORLD_NAMES[world - 1], word, 120.0)
	# Green marks the next playable forest; browsing never changes a card's size or status.
	var at := Rect2(CARD.x * 0.5 - BADGE * 0.5, BADGE_Y - BADGE * 0.5, BADGE, BADGE)
	var badge := UI.image(card, "island_badge_green" if current else "island_badge_gold", at, false)
	badge.name = "WorldNumber"
	Island.digits(badge, str(world), Rect2(BADGE * 0.24, BADGE * 0.22, BADGE * 0.52, BADGE * 0.52))
	if not open:
		window.modulate = Color(0.62, 0.62, 0.66)
		Island.piece_fit(card, "island_padlock", Vector2(CARD.x * 0.5, WINDOW.get_center().y - 60.0), Vector2(220, 220)).name = "Lock"
	elif _world_cleared(world):
		Island.check_badge(card, Vector2(CARD.x - 50.0, 50.0)).name = "Done"
	var play := UI.art_button(card, "Play", "btn_home_island_play_" + UiText.locale(), PLAY, func(): _play(world))
	play.mouse_filter = Control.MOUSE_FILTER_PASS
	UI.set_active(play, open)
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
	progress.scale = Vector2.ONE * fit

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

## External navigation reveals a forest; ordinary dragging never snaps to one.
func show_world(world: int, instant := false) -> void:
	_world = clampi(world, 1, Levels.WORLDS)
	if _scroll_tween != null:
		_scroll_tween.kill()
	var target := clampf((_world - 1) * STRIDE, 0.0, _max_scroll)
	if instant or _instant():
		_set_scroll(target)
		return
	_scroll_tween = create_tween()
	_scroll_tween.tween_method(_set_scroll, _scroll_x, target, 0.3).set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)

## Reset before child buttons handle the press, so a drag starting on Play cannot activate it.
func _input(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.pressed:
		_pressed = false
		_travel = 0.0
		if _scroll_tween != null:
			_scroll_tween.kill()

func _on_scroll_input(event: InputEvent) -> void:
	if _page != null:
		return
	if event is InputEventMouseButton:
		var e := event as InputEventMouseButton
		if e.pressed and e.button_index in [MOUSE_BUTTON_WHEEL_UP, MOUSE_BUTTON_WHEEL_LEFT]:
			_set_scroll(_scroll_x - SCROLL_STEP * e.factor)
		elif e.pressed and e.button_index in [MOUSE_BUTTON_WHEEL_DOWN, MOUSE_BUTTON_WHEEL_RIGHT]:
			_set_scroll(_scroll_x + SCROLL_STEP * e.factor)
		elif e.button_index == MOUSE_BUTTON_LEFT:
			if e.pressed:
				_pressed = true
			elif _pressed:
				_pressed = false
				if _travel < TAP_SLOP:
					_tap(e.position + Vector2(_scroll_x, 0))
	elif event is InputEventMouseMotion and event.button_mask & MOUSE_BUTTON_MASK_LEFT:
		var m := event as InputEventMouseMotion
		_travel += absf(m.relative.x)
		if _travel >= TAP_SLOP:
			_set_scroll(_scroll_x - m.relative.x)
	elif event is InputEventPanGesture:
		var delta: Vector2 = event.delta
		_set_scroll(_scroll_x + (delta.x if absf(delta.x) >= absf(delta.y) else delta.y) * SCROLL_STEP)

func _tap(at: Vector2) -> void:
	for i in _cards.size():
		var card: Control = _cards[i]
		var box := Rect2(card.position, CARD)
		if not box.has_point(at):
			continue
		var world := i + 1
		if Progress.is_world_unlocked(profile, world):
			Sound.play("ui_tap")
			open_world(world)
		else:
			Sound.play("error")
			_shake(card)
			gate_tapped.emit(world)
		return

func _shake(card: Control) -> void:
	if _instant():
		return
	var x := card.position.x
	var tw := card.create_tween()
	for dx in [14.0, -14.0, 8.0, -8.0, 0.0]:
		tw.tween_property(card, "position:x", x + dx, 0.05)


# ---------------------------------------------------------------- world page

## The world's painting on one screen with its ten level nodes; Back returns to the cards on the same world.
func open_world(world: int) -> void:
	if _page != null:
		_page.queue_free()
	_world = world
	_strip.visible = false
	_page = Control.new()
	_page.name = "WorldPage"
	_page.mouse_filter = Control.MOUSE_FILTER_STOP
	_page.size = size
	add_child(_page)
	move_child(_page, _bar.get_index())
	var painting := UI.image(_page, "map_world_w%d" % world, Rect2(0, PAGE_TOP, 1080, 1620), false)
	painting.name = "Painting"
	var plank := Island.hanging_sign(_page, 540.0, 200.0, 680.0)
	UI.heading(_page, WORLD_NAMES[world - 1], plank.grow_individual(50.0, 34.0, 50.0, 20.0), 76.0)
	var text := "%d/%d" % [_world_stars(world), Levels.PER_WORLD * 3]
	var w := UI.number_width(text, 44.0)
	_star_progress(_page, world, Rect2(540.0 - (w + 130.0) * 0.5, 300.0, w + 130.0, 80.0))
	for n in Levels.PER_WORLD:
		_build_node((world - 1) * Levels.PER_WORLD + n + 1)
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

func _node_pos(level: int) -> Vector2:
	var world := Progress.world_of_level(level)
	var p: Array = NODES[TRAIL_OF_WORLD[world - 1]][(level - 1) % Levels.PER_WORLD]
	return Vector2(p[0], p[1] + PAGE_TOP)

func _disc(parent: Control, picture: String, at: Vector2, diameter: float) -> void:
	UI.art(parent, picture, Rect2(at - Vector2.ONE * diameter * 0.5, Vector2.ONE * diameter)).name = "Node"

func _build_node(level: int) -> void:
	var state := Progress.node_state(profile, level)
	var c := _node_pos(level)
	var shown := (level - 1) % Levels.PER_WORLD + 1   # numbers restart at 1 in every world
	var layer := Control.new()
	layer.name = "Level %d" % level
	layer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_page.add_child(layer)
	var face := Vector2(c.x, c.y - 5.0)
	match state:
		"locked":
			_disc(layer, "island_round_plate", c, NODE * 0.92)
			Island.piece_fit(layer, "island_padlock", face + Vector2(0, 4), Vector2(64, 64))
			_hit(layer, c, NODE, level)
		"cleared":
			_disc(layer, "island_badge_green", c, NODE)
			_white_number(layer, shown, face - Vector2(0, 12), NODE_CAP)
			UI.rating_stars(layer, Vector2(c.x, c.y + STAR_DY + 6.0), STAR, STAR_GAP, profile.best_stars(level))
			_hit(layer, c, NODE, level)
		"current":
			var glow := UI.image(layer, "glow_soft", Rect2(c - Vector2.ONE * CURRENT * 0.9, Vector2.ONE * CURRENT * 1.8))
			glow.modulate = Color(1.0, 0.85, 0.3, 0.9)
			_disc(layer, "island_badge_green", c, CURRENT)
			_white_number(layer, shown, face, CURRENT_CAP)
			_hit(layer, c, CURRENT, level)
			if level == current_level:
				_add_bird(layer, Vector2(c.x, c.y - BIRD_FEET), _level_species(level))
			if not _instant():
				_bounce(layer)

## Level number in the painted white digits (the homepage digits), centred.
func _white_number(layer: Control, value: int, center: Vector2, cap: float) -> void:
	var text := str(value)
	var h := cap * 1.25
	var w := h * 0.8 * text.length()
	Island.digits(layer, text, Rect2(center - Vector2(w, h) * 0.5, Vector2(w, h)))

func _hit(layer: Control, c: Vector2, diameter: float, level: int) -> void:
	var b := UI.hit_area(layer, Rect2(c - Vector2.ONE * (diameter + 32.0) * 0.5, Vector2.ONE * (diameter + 32.0)),
		func(): level_tapped.emit(level))
	b.name = "Tap"
	b.disabled = Progress.node_state(profile, level) == "locked"

func _number(layer: Control, value: int, center: Vector2, cap: float) -> void:
	var w := UI.number_width(str(value), cap) + 8.0
	UI.number(layer, str(value), Rect2(center.x - w * 0.5, center.y - cap * 0.5, w, cap), UI.INK)

func _bounce(layer: Control) -> void:
	var tw := layer.create_tween().set_loops()
	tw.tween_property(layer, "position:y", -BOUNCE, 0.45).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)
	tw.tween_property(layer, "position:y", 0.0, 0.55).set_trans(Tween.TRANS_BOUNCE).set_ease(Tween.EASE_OUT)
	tw.tween_interval(1.2)

## The first bird species of the level, so the node shows who the player is about to meet.
func _level_species(level: int) -> String:
	var ids := Levels.species_ids(Levels.create_board(Levels.get_level(level)))
	return ids[0] if not ids.is_empty() else "sunny"

func _add_bird(layer: Control, feet: Vector2, species: String) -> void:
	_bird = BirdView.new()
	_bird.name = "Bird"
	_bird.position = feet
	layer.add_child(_bird)
	_bird.setup(Board.bird(0, Board.species_index(species), 0), false, BIRD_SCALE)
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
		"world_page": cleared = 7
		"world_page_late": cleared = 34
	for level in range(1, cleared + 1):
		p.stars[level] = [3, 3, 2, 3, 1][(level - 1) % 5]
	p.highest_unlocked = mini(Levels.COUNT, cleared + 1)
	var view: Control = load("res://game/map_view.gd").new()
	main.set_screen(view)
	view.setup(p)
	if state.begins_with("world_page"):
		view.open_world(Progress.world_of_level(view.current_level))
