extends Control
## The level map: all 40 levels on a scrolling serpentine trail across four worlds, with a fixed HUD and nav bar.

signal level_tapped(level: int)
signal shop_requested
signal aviary_requested
signal settings_requested
signal gate_tapped(world: int)

const BirdViewScript := preload("res://game/bird_view.gd")
const SEGMENT := 3000.0
const STRIDE := 2840.0
const CONTENT_H := STRIDE * 3.0 + SEGMENT
const FOCUS_RATIO := 0.6
const TAP_SLOP := 14.0
const WORLD_TITLES := ["label_title_green_forest", "label_title_bamboo_forest", "label_title_autumn_forest", "label_title_snow_forest"]
## Node centers per world (x, y within the world segment), levels 1..10.
const NODES := [
	[[514, 2700], [310, 2440], [744, 2180], [660, 1920], [252, 1660], [672, 1400], [640, 1140], [305, 880], [754, 620], [613, 360]],
	[[512, 2700], [304, 2440], [729, 2180], [656, 1920], [251, 1660], [667, 1400], [634, 1140], [302, 880], [752, 620], [614, 360]],
	[[508, 2700], [306, 2440], [752, 2180], [640, 1920], [240, 1660], [699, 1400], [620, 1140], [298, 880], [750, 620], [598, 360]],
	[[512, 2700], [312, 2440], [738, 2180], [650, 1920], [250, 1660], [668, 1400], [618, 1140], [299, 880], [750, 620], [604, 360]],
]
const CURRENT_SIZE := Vector2(326, 224)
const NODE_SIZE := Vector2(306, 226)
const LOCKED_SIZE := Vector2(250, 176)
const GATE_RECT_SIZE := Vector2(402, 300)
const GATE_CENTER_X := 700.0   # mockup gate spans x 455..945
const GATE_CENTER_Y := 110.0   # between the last node of a world and the first of the next

var profile: Profile
var current_level := 1
var offset := 0.0

var _content: Control
var _hud: Control
var _coin_number: Control
var _title: TextureRect
var _title_world := 0
var _hit_rects: Array = []   # [Rect2 in content space, kind, id]
var _gates := {}
var _dragging := false
var _drag_travel := 0.0
var _velocity := 0.0
var _press_pos := Vector2.ZERO
var _bird: Node2D


func setup(player: Profile) -> void:
	profile = player
	for child in get_children():
		child.queue_free()
	_hit_rects.clear()
	_gates.clear()
	size = Vector2(1080, 1920)
	clip_contents = true
	mouse_filter = Control.MOUSE_FILTER_STOP
	var next := Progress.next_level(profile)
	current_level = next if next > 0 else 40
	_build_content()
	_build_hud()
	scroll_to(_node_y(current_level) - size.y * FOCUS_RATIO)

func _node_y(level: int) -> float:
	return _node_pos(level).y

func _node_pos(level: int) -> Vector2:
	var world := Progress.world_of_level(level)
	var p: Array = NODES[world - 1][(level - 1) % 10]
	return Vector2(p[0], p[1] + (4 - world) * STRIDE)

func _gate_center(world: int) -> Vector2:
	return Vector2(GATE_CENTER_X, GATE_CENTER_Y + (4 - world) * STRIDE)

## Scrolls so the world's gate sits near the middle of the screen.
func show_gate(world: int) -> void:
	scroll_to(_gate_center(world).y - size.y * 0.35)

func scroll_to(value: float) -> void:
	offset = clampf(value, 0.0, CONTENT_H - size.y)
	_content.position.y = -offset
	_update_title()

func _build_content() -> void:
	_content = Control.new()
	_content.name = "Trail"
	_content.size = Vector2(1080, CONTENT_H)
	_content.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(_content)
	for world in range(1, 5):
		var bg := UI.image(_content, "map_bg_continuous_w%d" % world, Rect2(0, (4 - world) * STRIDE, 1080, SEGMENT), false)
		bg.name = "World %d" % world
	for world in range(1, 4):
		_build_gate(world)
	for level in range(1, 41):
		_build_node(level)

func _build_gate(world: int) -> void:
	var open := Progress.is_world_unlocked(profile, world + 1)
	var c := _gate_center(world)
	var rect := Rect2(c - GATE_RECT_SIZE * 0.5, GATE_RECT_SIZE)
	var holder := Control.new()
	holder.name = "Gate %d" % world
	holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_content.add_child(holder)
	if open:
		UI.art(holder, "gate_light_w%d" % world, Rect2(rect.position + Vector2(120, 140), Vector2(250, 225)))
		UI.art(holder, "gate_arch_frame_w%d" % world, rect)
		var door := Vector2(rect.size.x * 0.17, rect.size.y * 0.5)
		var left := UI.art(holder, "gate_door_left_w%d" % world, Rect2(rect.position + Vector2(rect.size.x * 0.155, rect.size.y * 0.42), door))
		var right := UI.art(holder, "gate_door_right_w%d" % world, Rect2(rect.position + Vector2(rect.size.x * 0.675, rect.size.y * 0.42), door))
		left.scale.x = 0.35
		right.scale.x = 0.35
		left.pivot_offset = Vector2(left.size.x * 0.0, left.size.y * 0.5)
		right.pivot_offset = Vector2(right.size.x, right.size.y * 0.5)
	else:
		UI.art(holder, "gate_arch_closed_w%d" % world, rect)
	_hit_rects.append([rect, "gate", world])

func _build_node(level: int) -> void:
	var state := Progress.node_state(profile, level)
	var c := _node_pos(level)
	var big := state == "current"
	var box := CURRENT_SIZE if big else (LOCKED_SIZE if state == "locked" else NODE_SIZE)
	var rect := Rect2(c - box * 0.5, box)
	var layer := Control.new()
	layer.name = "Level %d" % level
	layer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_content.add_child(layer)
	_hit_rects.append([rect, "level", level])
	match state:
		"locked":
			UI.art(layer, "node_stone_locked", rect)
			UI.number(layer, str(level), Rect2(c.x - 40, rect.position.y + box.y * 0.05, 80, box.y * 0.36), "cream")
		"cleared":
			UI.art(layer, "node_wood", rect)
			UI.number(layer, str(level), Rect2(c.x - 45, rect.position.y + box.y * 0.17, 90, box.y * 0.34), "cream")
			_add_stars(layer, c, rect, profile.best_stars(level), 0.0)
		"current":
			var glow := UI.art(layer, "node_wood_current_glow", rect.grow(14))
			var tw := glow.create_tween().set_loops()
			tw.tween_property(glow, "modulate:a", 0.55, 1.1).set_trans(Tween.TRANS_SINE)
			tw.tween_property(glow, "modulate:a", 1.0, 1.1).set_trans(Tween.TRANS_SINE)
			UI.art(layer, "node_wood", rect)
			UI.number(layer, str(level), Rect2(c.x - 50, rect.position.y + box.y * 0.31, 100, box.y * 0.44), "cream")
			if level == current_level:
				# The last node of a world sits right under the gate, so its bird stands smaller beside the node.
				var beside := level % 10 == 0 and level < 40
				_add_bird(layer, Vector2(c.x - 270, c.y + 10) if beside else Vector2(c.x, rect.position.y + 30), 2.2 if beside else 2.9)

func _add_stars(layer: Control, c: Vector2, rect: Rect2, earned: int, _unused: float) -> void:
	for i in 3:
		var name := "star_full_small" if i < earned else "star_empty_small"
		var r := Rect2(c.x + (i - 1) * 67 - 34, rect.position.y + rect.size.y * 0.53, 68, 62)
		UI.art(layer, name, r)

func _add_bird(layer: Control, feet: Vector2, density: float) -> void:
	_bird = BirdViewScript.new()
	_bird.position = feet
	layer.add_child(_bird)
	_bird.setup(Board.bird(0, 0, 0), false, density)
	for x in [-175.0, 175.0]:
		var s := UI.art(layer, "sparkle", Rect2(feet.x + x - 24, feet.y - 30, 48, 50))
		var tw := s.create_tween().set_loops()
		tw.tween_property(s, "modulate:a", 0.3, 0.9 + absf(x) * 0.001).set_trans(Tween.TRANS_SINE)
		tw.tween_property(s, "modulate:a", 1.0, 0.9).set_trans(Tween.TRANS_SINE)

func _build_hud() -> void:
	_hud = Control.new()
	_hud.name = "HUD"
	_hud.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_hud.size = size
	add_child(_hud)
	UI.panel(_hud, "stat_pill", Layout.r("map", "coin_pill"))
	UI.art(_hud, "coin", Layout.r("map", "coin"))
	_coin_number = UI.number(_hud, str(profile.coins), Layout.r("map", "coin_count"), "brown")
	_title = TextureRect.new()
	_title.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	_title.stretch_mode = TextureRect.STRETCH_SCALE
	_title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_hud.add_child(_title)
	_title_world = 0
	var bar := Layout.r("map", "nav_bar")   # orange felt rim around a cream bar: panel_wood with thin borders
	var k := 0.22
	UI.panel(_hud, "panel_wood", bar.grow_individual(4 * k, 21 * k, 4 * k, 21 * k), k)
	UI.disc(_hud, "Shop", Layout.r("map", "nav_shop"), "icon_basket", shop_requested.emit, "btn_wood", 0.52)
	UI.disc(_hud, "Aviary", Layout.r("map", "nav_aviary"), "icon_aviary", aviary_requested.emit, "btn_wood", 0.62)
	UI.disc(_hud, "Settings", Layout.r("map", "nav_settings"), "icon_gear", settings_requested.emit, "btn_wood", 0.54)

## The world title follows the trail node nearest to the upper third of the screen.
func _update_title() -> void:
	if _title == null:
		return
	var probe := offset + size.y * 0.36
	var best := 1
	var best_d := INF
	for level in range(1, 41):
		var d := absf(_node_y(level) - probe)
		if d < best_d:
			best_d = d
			best = level
	var world := Progress.world_of_level(best)
	if world == _title_world:
		return
	_title_world = world
	var tex := Art.tex(WORLD_TITLES[world - 1])
	_title.texture = tex
	var used := UI.used_rect(WORLD_TITLES[world - 1])
	var scale := 464.0 / used.size.x
	_title.size = tex.get_size() * scale
	_title.position = Vector2(547, 137) - (used.position + used.size * 0.5) * scale

func _process(delta: float) -> void:
	if not _dragging and absf(_velocity) > 8.0:
		scroll_to(offset + _velocity * delta)
		_velocity *= pow(0.135, delta)
		if offset <= 0.0 or offset >= CONTENT_H - size.y:
			_velocity = 0.0

func _gui_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		var mb := event as InputEventMouseButton
		if mb.button_index == MOUSE_BUTTON_WHEEL_UP and mb.pressed:
			_velocity = 0.0
			scroll_to(offset - 220.0)
		elif mb.button_index == MOUSE_BUTTON_WHEEL_DOWN and mb.pressed:
			_velocity = 0.0
			scroll_to(offset + 220.0)
		elif mb.button_index == MOUSE_BUTTON_LEFT:
			if mb.pressed:
				_dragging = true
				_drag_travel = 0.0
				_velocity = 0.0
				_press_pos = mb.position
			elif _dragging:
				_dragging = false
				if _drag_travel < TAP_SLOP:
					_tap(mb.position)
	elif event is InputEventMouseMotion and _dragging:
		var mm := event as InputEventMouseMotion
		_drag_travel += absf(mm.relative.y) + absf(mm.relative.x)
		if _drag_travel >= TAP_SLOP:
			scroll_to(offset - mm.relative.y)
			_velocity = lerpf(_velocity, -mm.velocity.y, 0.5)

func _tap(screen_pos: Vector2) -> void:
	var p := screen_pos + Vector2(0, offset)
	for hit in _hit_rects:
		if (hit[0] as Rect2).has_point(p):
			if hit[1] == "level" and Progress.node_state(profile, hit[2]) != "locked":
				level_tapped.emit(hit[2])
			elif hit[1] == "gate":
				gate_tapped.emit(hit[2])
			return

## Fake data for captures. States: forest_trail, first_level, autumn_world, all_cleared, gate_locked.
static func preview(main: Control, state: String) -> void:
	var p := Profile.new()
	var cleared := 0
	match state:
		"forest_trail":
			cleared = 7
			p.coins = 1240
		"autumn_world":
			cleared = 20
			p.coins = 1240
		"all_cleared":
			cleared = 40
			p.coins = 1240
		"gate_locked":
			cleared = 9
			p.coins = 1240
	for level in range(1, cleared + 1):
		p.stars[level] = 3
	p.highest_unlocked = mini(40, cleared + 1)
	var view: Control = load("res://game/map_view.gd").new()
	main.set_screen(view)
	view.setup(p)
	if state == "gate_locked":
		view.show_gate(1)
