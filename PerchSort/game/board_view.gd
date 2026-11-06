class_name BoardView
extends Control
## The level screen: forest, branches, birds, HUD and tool bar. Plays rule events as animation.

signal won(result: Dictionary)          # {"session": GameSession}
signal settings_requested
signal buy_requested(kind: int)          # booster with zero stock was tapped
signal goal_info_requested(species: String)
signal profile_changed

const GIFT_COIN := {"sunny": "wake", "sky": "reveal", "berry": "encore", "mint": "grow", "cocoa": "scare",
	"snow": "lightstep", "ember": "hatch", "ink": "nightcall", "hoot": "moonbeam"}
const TOOL_ICON := {Board.Booster.FIREFLY: "icon_bulb", Board.Booster.SAPLING: "icon_branch_plus",
	Board.Booster.OWL_EYE: "tool_owl_eye", Board.Booster.MORNING_BELL: "tool_bell",
	Board.Booster.SCARECROW: "tool_scarecrow", Board.Booster.SPRING_RAIN: "tool_rain"}
const ENTRANCE_SECONDS := 4.0

var session: GameSession
var profile: Profile
var level: Dictionary
var busy := false
var instant := false   # captures: no motion

var _scene: Node2D
var _branch_layer: Node2D
var _bird_layer: Node2D
var _hud: Control
var _night: TextureRect
var _birds := {}        # bird id -> BirdView
var _branch_nodes: Array = []
## Trunk widths hidden off and visible at each screen edge, and how far a branch reaches under the trunk.
const TRUNK_HIDE_LEFT := 110.0
const TRUNK_SHOW_LEFT := 56.0
const TRUNK_HIDE_RIGHT := 96.0
const TRUNK_SHOW_RIGHT := 60.0
const JOINT_OVERLAP := 30.0
var _rows := 2
var _density := 1.0
var _top := 815.0
var _pitch := 330.0
var _house: Control
var _goal_nodes := {}   # species -> Control
var _coin_number: Control
var _move_number: Control
var _sun_pill: Control
var _sun_number: Control
var _tool_bar: Control
var _hint_job := -1


func start(level_def: Dictionary, player: Profile, play_entrance := true) -> void:
	level = level_def
	profile = player
	session = GameSession.new(level, profile.inventory)
	for child in get_children():
		child.queue_free()
	_birds.clear()
	size = Vector2(1080, 1920)
	_build_scene()
	_build_hud()
	_layout_branches()
	_spawn_birds()
	refresh_hud()
	if play_entrance and not instant:
		_entrance()

## ---------- scene ----------

func _world() -> int:
	return session.state.world

func _build_scene() -> void:
	var variant := session.state.variant
	var bg := "bg_level_w%d_%s" % [_world(), variant]
	if not Art.has(bg):
		bg = "bg_level_w%d_noon" % _world()
	UI.image(self, bg, Rect2(0, 0, 1080, 1920), false)
	_scene = Node2D.new()
	_scene.name = "Scene"
	add_child(_scene)
	var w := _world()
	_branch_layer = Node2D.new()
	_scene.add_child(_branch_layer)
	# Slim trunks at both screen edges, drawn over the thick branch ends so each branch grows out of the trunk.
	_scene.add_child(_sprite("trunk_curved_w%d" % w, Vector2(-TRUNK_HIDE_LEFT, 0), false))
	_scene.add_child(_sprite("trunk_left_w%d" % w, Vector2(1080 - 160 + TRUNK_HIDE_RIGHT, 0), true))
	_bird_layer = Node2D.new()
	_scene.add_child(_bird_layer)
	_night = UI.image(self, "night_overlay", Rect2(0, 0, 1080, 1920), false)
	_night.modulate.a = 0.0

func _row_count() -> int:
	var rows := 1
	for r in session.state.rows:
		rows = maxi(rows, r + 1)
	return rows

func _layout_branches() -> void:
	_rows = _row_count()
	_density = [1.0, 1.0, 1.0, 1.0, 0.88, 0.76, 0.66][mini(_rows, 6)]
	_top = 815.0 if _rows <= 3 else 760.0
	_pitch = minf(330.0, (1660.0 - _top) / (_rows - 0.5))
	for node in _branch_nodes:
		node.queue_free()
	_branch_nodes.clear()
	var s := session.state
	for b in s.branch_count():
		var node := Node2D.new()
		var right := s.rights[b] == 1
		var y := perch_y(b)
		var twig := s.kinds[b] == Board.Kind.TWIG
		# The node origin is where the branch leaves the trunk, so a refusal wobble pivots there.
		var edge := 1080.0 - TRUNK_SHOW_RIGHT if right else TRUNK_SHOW_LEFT
		node.position = Vector2(edge, y)
		var dir := -1.0 if right else 1.0
		var d := _density
		var length := seat_x(maxi(s.caps[b], 2) - 1, right, true) + 90.0 * d
		var arm := _sprite(("twig_left_w%d" if twig else "branch_left_w%d") % _world(), Vector2.ZERO, right)
		var tex_w := float(arm.texture.get_width())
		var reach := length - (TRUNK_SHOW_RIGHT if right else TRUNK_SHOW_LEFT) + JOINT_OVERLAP
		arm.scale = Vector2(reach / tex_w, (1.1 if twig else 1.5) * d)
		arm.position = Vector2(JOINT_OVERLAP - reach if right else -JOINT_OVERLAP, -26.0 * d)
		node.add_child(arm)
		var tip := _leaf_tuft(right, d)
		tip.position = Vector2(dir * (reach - JOINT_OVERLAP - 14.0 * d), (4.0 if twig else 10.0) * d)
		node.add_child(tip)
		if s.kinds[b] == Board.Kind.SWIVEL:
			var knot := Sprite2D.new()
			knot.texture = Art.tex("swivel_knot")
			knot.scale = Vector2.ONE * 0.5 * _density
			knot.position = Vector2((930.0 if right else 150.0) - edge, 30.0 * _density)
			node.add_child(knot)
		_branch_layer.add_child(node)
		_branch_nodes.append(node)

func _sprite(name: String, pos: Vector2, flip: bool) -> Sprite2D:
	var spr := Sprite2D.new()
	spr.texture = Art.tex(name)
	spr.centered = false
	spr.flip_h = flip
	spr.position = pos
	return spr

## Leaves on a branch tip: green sprigs in the green and bamboo worlds, seasonal leaves after.
func _leaf_tuft(right: bool, d: float) -> Node2D:
	var tuft := Node2D.new()
	var dir := -1.0 if right else 1.0
	if _world() <= 2:
		var sprig := Sprite2D.new()
		sprig.texture = Art.tex("leaf_sprig_b")
		sprig.flip_h = right
		sprig.scale = Vector2.ONE * 0.62 * d
		sprig.position = Vector2(dir * 40.0 * d, -22.0 * d)
		tuft.add_child(sprig)
	else:
		for k in 3:
			var leaf := Sprite2D.new()
			leaf.texture = Art.tex("leaf_fall_w%d" % _world())
			leaf.scale = Vector2.ONE * 0.9 * d
			leaf.rotation = dir * (-0.7 + 0.7 * k)
			leaf.position = Vector2(dir * (22.0 + 16.0 * k) * d, (-18.0 + 16.0 * k) * d)
			tuft.add_child(leaf)
	return tuft

func perch_y(b: int) -> float:
	var s := session.state
	return _top + (s.rows[b] + (0.5 if s.rights[b] == 1 else 0.0)) * _pitch

## Seat x on a branch; seat 0 is at the trunk. `local` returns distance from the edge.
func seat_x(i: int, right: bool, local := false) -> float:
	var d := 124.0 + 126.0 * i * _density
	if local:
		return d
	return 1080.0 - d if right else d

func seat_point(b: int, i: int) -> Vector2:
	return Vector2(seat_x(i, session.state.rights[b] == 1), perch_y(b))

func _spawn_birds() -> void:
	for bird in _birds.values():
		bird.queue_free()
	_birds.clear()
	var s := session.state
	for b in s.branch_count():
		for i in s.items[b].size():
			_make_bird(s.items[b][i], seat_point(b, i))

func _make_bird(code: int, at: Vector2) -> BirdView:
	var bird := BirdView.new()
	var species: String = Board.SPECIES[Board.species_of(code)]
	bird.setup(code, session.state.is_asleep(code), _density, profile.equipped.get(species, "").replace("outfit.", "").replace(".", "_outfit_") if profile.equipped.has(species) else "")
	bird.position = at
	_bird_layer.add_child(bird)
	if instant:
		bird.freeze()
	_birds[Board.id_of(code)] = bird
	return bird

## Snap every bird to its seat (after undo/restart or as a safety net).
func sync_birds() -> void:
	var s := session.state
	var seen := {}
	for b in s.branch_count():
		for i in s.items[b].size():
			var code := s.items[b][i]
			var id := Board.id_of(code)
			seen[id] = true
			var bird: BirdView = _birds.get(id)
			if bird == null or not is_instance_valid(bird):
				bird = _make_bird(code, seat_point(b, i))
			elif bird.code != code or bird.asleep != s.is_asleep(code):
				bird.setup(code, s.is_asleep(code), _density, bird.outfit)
			bird.position = seat_point(b, i)
			bird.set_selected(false)
	for id in _birds.keys():
		if not seen.has(id):
			if is_instance_valid(_birds[id]):
				_birds[id].queue_free()
			_birds.erase(id)
	_night.modulate.a = 0.45 if s.phase == Board.Phase.NIGHT else 0.0

func _entrance() -> void:
	busy = true
	modulate.a = 0.0
	var fade := create_tween()
	fade.tween_property(self, "modulate:a", 1.0, 0.5)
	var s := session.state
	var birds: Array = []
	for b in s.branch_count():
		for i in s.items[b].size():
			birds.append([s.items[b][i], b, i])
	var n := birds.size()
	var last: BirdView = null
	for k in n:
		var entry: Array = birds[k]
		var bird: BirdView = _birds[Board.id_of(entry[0])]
		var target := bird.position
		var right: bool = s.rights[entry[1]] == 1
		bird.position = Vector2(-160.0 if not right else 1240.0, target.y - 420.0 - randf() * 200.0)
		# Spread take-offs over 0.6..2.9 s with 1.0 s flights: whole entrance lasts about 4 s.
		var delay := 0.6 + (2.3 * k / maxf(1.0, n - 1.0))
		bird.fly_to(target, delay, 1.0, false, true)
		last = bird
	if last:
		await last.finished
	await get_tree().create_timer(0.1).timeout
	busy = false

## ---------- HUD ----------

func _build_hud() -> void:
	_hud = Control.new()
	_hud.name = "HUD"
	_hud.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_hud.size = Vector2(1080, 1920)
	add_child(_hud)
	var L := "board_base"
	# Rope and goal house.
	UI.art(_hud, "goal_rope_single", Layout.r(L, "goal_rope"), Rect2(6, 0, 108, 1260))
	_house = Control.new()
	_house.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var hr := Layout.r(L, "goal_house")
	UI.place(_house, hr)
	_hud.add_child(_house)
	var style := profile.equipped_house
	UI.art(_house, "goal_house_single" if style == "default" else "house_style_%s_body" % style,
		Rect2(Vector2.ZERO, hr.size), Rect2(45, 110, 624, 580) if style == "default" else Rect2())
	# Coin pill.
	UI.panel(_hud, "stat_pill", Layout.r(L, "coin_pill"))
	UI.art(_hud, "coin", Layout.r(L, "coin_disc"))
	_coin_number = UI.number(_hud, "0", Layout.r(L, "coin_value"), "brown", -1)
	# Moves and star thresholds.
	UI.panel(_hud, "stat_pill", Layout.r(L, "move_pill"))
	UI.art(_hud, "footprints", Layout.r(L, "footprints"))
	_move_number = UI.number(_hud, "0", Layout.r(L, "move_value"))
	var div := ColorRect.new()
	div.color = Color(0.62, 0.45, 0.3, 0.6)
	UI.place(div, Layout.r(L, "move_divider"))
	_hud.add_child(div)
	var thresholds := [str(session.par), str(session.par + 3), "∞"]
	for i in 3:
		UI.art(_hud, "star_full_small", Layout.r(L, "move_star_%d" % (i + 1)))
		UI.number(_hud, thresholds[i], Layout.r(L, "threshold_%d" % (i + 1)))
	# Day/night counter.
	_sun_pill = Control.new()
	_sun_pill.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_hud.add_child(_sun_pill)
	UI.panel(_sun_pill, "stat_pill", Layout.r(L, "sun_pill"))
	UI.art(_sun_pill, "dial_sun", Layout.r(L, "sun_icon"))
	_sun_number = UI.number(_sun_pill, "0", Layout.r(L, "sun_value"))
	_sun_pill.visible = session.state.day_length > 0
	# Settings.
	UI.disc(_hud, "Settings", Layout.r(L, "settings"), "icon_gear", func(): settings_requested.emit(), "btn_wood", 0.6)
	_build_goals()
	_tool_bar = Control.new()
	_tool_bar.name = "Tools"
	_tool_bar.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_hud.add_child(_tool_bar)

func _build_goals() -> void:
	var ids := Levels.species_ids(session.state)
	var n := ids.size()
	var ring := Layout.r("board_base", "goal_ring_blue")
	var pitch := minf(139.0, 900.0 / maxf(1.0, n))
	var size := ring.size * (1.0 if n <= 6 else 0.8)
	for i in n:
		var sp: String = ids[i]
		var cx := 540.0 + (i - (n - 1) * 0.5) * pitch
		var r := Rect2(Vector2(cx - size.x * 0.5, ring.position.y), size)
		var node := Control.new()
		UI.place(node, r)
		_hud.add_child(node)
		UI.art(node, sp + "_face", Rect2(size * 0.12, size * 0.76))
		UI.art(node, "badge_ring_gold", Rect2(Vector2.ZERO, size))
		UI.art(node, "gift_coin_" + GIFT_COIN[sp], Rect2(size * Vector2(0.58, 0.56), size * 0.55))
		UI.hit_area(node, Rect2(Vector2.ZERO, size), func(): goal_info_requested.emit(sp))
		_goal_nodes[sp] = node

func refresh_hud() -> void:
	UI.set_number(_coin_number, str(profile.coins), "brown", -1)
	UI.set_number(_move_number, str(session.move_count()))
	var s := session.state
	if s.day_length > 0:
		UI.set_number(_sun_number, str(s.day_length - s.phase_counter))
		var icon: TextureRect = _sun_pill.get_child(1)
		icon.texture = Art.tex("dial_moon" if s.phase == Board.Phase.NIGHT else "dial_sun")
	for sp in _goal_nodes:
		_goal_nodes[sp].modulate.a = 0.45 if s.homed.has(Board.species_index(sp)) else 1.0
	_build_tools()

func _visible_tools() -> Array:
	var tools := []
	if session.can_undo():
		tools.append(-1)
	for kind in [Board.Booster.FIREFLY, Board.Booster.SAPLING]:
		if Progress.is_booster_unlocked(profile, kind):
			tools.append(kind)
	var context := Progress.world_booster_for(int(level["id"]))
	if Progress.is_booster_unlocked(profile, context) and session.booster_has_effect(context):
		tools.append(context)
	return tools

func _build_tools() -> void:
	for child in _tool_bar.get_children():
		child.queue_free()
	var tools := _visible_tools()
	if tools.is_empty():
		return
	var disc := 110.0
	var gap := 130.0
	var width := tools.size() * disc + (tools.size() - 1) * gap + 104.0
	var bar := Rect2(540.0 - width * 0.5, 1727, width, 148)
	UI.panel(_tool_bar, "stat_pill", bar)
	for i in tools.size():
		var kind: int = tools[i]
		var r := Rect2(bar.position.x + 52.0 + i * (disc + gap), 1746, disc, 108)
		if kind < 0:
			UI.disc(_tool_bar, "Undo", r, "icon_undo", _on_undo, "btn_wood", 0.55)
			continue
		var b := UI.disc(_tool_bar, "Tool %d" % kind, r, TOOL_ICON[kind], func(): _on_tool(kind), "btn_wood",
			0.62 if kind <= Board.Booster.SAPLING else 0.8)
		var stock := session.state.inventory[kind]
		var badge := Rect2(r.size.x - 44, -8, 52, 52)
		UI.art(b, "badge_red", badge)
		UI.number(b, str(stock) if stock > 0 else "+", badge.grow(-12), "white")
		UI.set_enabled(b, stock == 0 or session.booster_has_effect(kind))

## ---------- input ----------

func _gui_input(event: InputEvent) -> void:
	if busy or session.state.is_won():
		return
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		var b := branch_at(event.position)
		if b >= 0:
			tap_branch(b)
			accept_event()

func branch_at(p: Vector2) -> int:
	var s := session.state
	var best := -1
	var best_d := 1e9
	for b in s.branch_count():
		var right := s.rights[b] == 1
		var y := perch_y(b)
		var x_ok := p.x >= 560.0 if right else p.x <= 520.0
		var dy := y - 70.0 * _density - p.y
		if x_ok and absf(dy) < maxf(110.0, _pitch * 0.45) and absf(dy) < best_d:
			best = b
			best_d = absf(dy)
	return best

func tap_branch(b: int) -> void:
	var s := session.state
	if session.selected < 0:
		if Rules.can_swivel(s, b) and s.run_count(b) == 0:
			_do(func(): return session.swivel(b))
			return
		if session.select(b):
			Sound.play("select")
			_lift(b, true)
		else:
			_refuse(b)
		return
	var from := session.selected
	if from == b:
		_lift(from, false)
		session.cancel_selection()
		if Rules.can_swivel(s, b):
			_do(func(): return session.swivel(b))
		return
	if Rules.can_transfer(s, from, b):
		_lift(from, false)
		_do(func(): return session.try_move(from, b))
	else:
		_lift(from, false)
		session.cancel_selection()
		if session.select(b):
			Sound.play("select")
			_lift(b, true)
		else:
			_refuse(b)

func _lift(b: int, on: bool) -> void:
	var s := session.state
	var list := s.items[b]
	var run := s.run_count(b)
	for i in range(list.size() - run, list.size()):
		var bird: BirdView = _birds.get(Board.id_of(list[i]))
		if bird:
			bird.set_selected(on)
	if on:
		for other in _birds.values():
			if is_instance_valid(other) and not other.selected and randf() < 0.3:
				other.look_at_x(seat_x(0, s.rights[b] == 1))

func _refuse(b: int) -> void:
	Sound.play("error")
	if b < 0 or b >= _branch_nodes.size():
		return
	var node: Node2D = _branch_nodes[b]
	var tw := create_tween()
	for k in 4:
		tw.tween_property(node, "rotation", 0.025 * (1 if k % 2 == 0 else -1), 0.05)
	tw.tween_property(node, "rotation", 0.0, 0.05)

func _do(action: Callable) -> void:
	busy = true
	var ok: bool = action.call()
	if ok:
		await play_events(session.events.duplicate())
	sync_birds()
	refresh_hud()
	busy = false
	if session.state.is_won():
		_celebrate()

func _on_undo() -> void:
	if busy or not session.undo():
		return
	Sound.play("undo")
	_layout_branches()
	sync_birds()
	refresh_hud()

func _on_tool(kind: int) -> void:
	if busy:
		return
	if session.state.inventory[kind] == 0:
		buy_requested.emit(kind)
		return
	if kind == Board.Booster.FIREFLY:
		_hint()
		return
	busy = true
	var result := session.use_booster(kind)
	if result.get("applied", false):
		profile.inventory[kind] = session.state.inventory[kind]
		profile_changed.emit()
		Sound.play("hint")
		await play_events(session.events.duplicate())
	else:
		Sound.play("error")
	_layout_branches()
	sync_birds()
	refresh_hud()
	busy = false

## Hint: solve on a worker thread so the screen never freezes; commit only if the state is unchanged.
func _hint() -> void:
	busy = true
	var snapshot := session.state.clone()
	var last := session.last_move.duplicate()
	var result := {}
	var task := WorkerThreadPool.add_task(func(): result.merge(Solver.solve(snapshot, last, 200000, 12000)))
	while not WorkerThreadPool.is_task_completed(task):
		await get_tree().process_frame
	WorkerThreadPool.wait_for_task_completion(task)
	busy = false
	if result.get("status", -1) != Solver.Status.SOLVED or result["moves"].is_empty():
		Sound.play("error")
		return
	var move: Array = result["moves"][0]
	if not session.try_use_firefly(move):
		return
	profile.inventory[Board.Booster.FIREFLY] = session.state.inventory[Board.Booster.FIREFLY]
	profile_changed.emit()
	Sound.play("hint")
	refresh_hud()
	show_hint(move)

## Pointing hand travels from the source branch to the target (or taps a swivel).
func show_hint(move: Array) -> void:
	var hand := Sprite2D.new()
	hand.texture = Art.tex("hand_pointer")
	hand.scale = Vector2.ONE * 0.45
	hand.offset = Vector2(40, 60)
	add_child(hand)
	var from := seat_point(move[0], maxi(0, session.state.items[move[0]].size() - 1)) + Vector2(0, -60)
	var to := from if move[1] < 0 else seat_point(move[1], session.state.items[move[1]].size()) + Vector2(0, -60)
	hand.position = from
	var tw := create_tween().set_loops(3)
	tw.tween_property(hand, "position", to, 0.7).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	tw.tween_interval(0.3)
	tw.tween_callback(func(): hand.position = from)
	await tw.finished
	hand.queue_free()

## ---------- event playback ----------

func play_events(events: Array) -> void:
	if instant:
		return
	for e in events:
		match e["type"]:
			"transferred", "chicks_followed":
				await _fly_group(e)
			"swivelled":
				await _swivel(e)
			"egg_wobbled":
				var bird: BirdView = _birds.get(e["id"])
				if bird:
					bird.wobble()
			"hatched":
				var bird: BirdView = _birds.get(e["id"])
				if bird:
					Sound.play("crate_open")
					bird.hatch()
					await bird.finished
			"egg_revealed":
				var bird: BirdView = _birds.get(e["id"])
				if bird:
					bird.reveal()
			"woke":
				var bird: BirdView = _birds.get(e["id"])
				if bird and bird.asleep:
					bird.wake()
					Sound.play("chirp_0%d" % (1 + randi() % 5))
			"flock_home":
				await _flock_home(e)
			"gift":
				await _gift(e)
			"branch_grew", "twig_upgraded":
				_layout_branches()
			"crows_left":
				for item in e["birds"]:
					var crow: BirdView = _birds.get(item["id"])
					if crow:
						_birds.erase(item["id"])
						crow.fly_to(Vector2(crow.position.x + (600.0 if crow.position.x > 540 else -600.0), -200), 0.0, 0.8, true)
				await get_tree().create_timer(0.8).timeout
			"phase_changed":
				var tw := create_tween()
				tw.tween_property(_night, "modulate:a", 0.45 if e["phase"] == Board.Phase.NIGHT else 0.0, 0.6)
				for bird in _birds.values():
					if is_instance_valid(bird):
						bird.set_sleep(session.state.is_asleep(bird.code))
				await tw.finished
			"won":
				pass

func _fly_group(e: Dictionary) -> void:
	var to: int = e["to"]
	var flights: Array = []
	if e["type"] == "transferred":
		var ids: PackedInt32Array = e["ids"]
		for k in ids.size():
			flights.append([ids[k], e["to_seat"] + k])
	else:
		for item in e["birds"]:
			flights.append([item["id"], item["to_seat"]])
	var last: BirdView = null
	for k in flights.size():
		var bird: BirdView = _birds.get(flights[k][0])
		if bird == null:
			continue
		bird.fly_to(seat_point(to, flights[k][1]), k * 0.07, 0.45)
		last = bird
	Sound.play("move")
	if last:
		await last.finished

func _swivel(e: Dictionary) -> void:
	var b: int = e["branch"]
	var last: BirdView = null
	for item in e["birds"]:
		var bird: BirdView = _birds.get(item["id"])
		if bird:
			bird.hop_to(seat_point(b, item["to_seat"]), 0.4)
			last = bird
	if last:
		await last.finished

func _door_point() -> Vector2:
	return Layout.r("board_base", "house_opening").get_center()

func _flock_home(e: Dictionary) -> void:
	var ids: PackedInt32Array = e["ids"]
	var last: BirdView = null
	for k in ids.size():
		var bird: BirdView = _birds.get(ids[ids.size() - 1 - k])
		if bird == null:
			continue
		_birds.erase(ids[ids.size() - 1 - k])
		bird.fly_to(_door_point() + Vector2(0, 40), k * 0.12, 0.6, true)
		last = bird
	if last:
		await last.finished
	Sound.play("nest_home")
	var tw := create_tween()
	tw.tween_property(_house, "scale", Vector2(1.08, 0.94), 0.1)
	tw.tween_property(_house, "scale", Vector2.ONE, 0.25).set_trans(Tween.TRANS_BACK)
	var goal: Control = _goal_nodes.get(e["species"])
	if goal:
		var pop := create_tween()
		pop.tween_property(goal, "scale", Vector2.ONE * 1.25, 0.12)
		pop.tween_property(goal, "scale", Vector2.ONE, 0.2)
		pop.tween_property(goal, "modulate:a", 0.45, 0.2)
	await tw.finished

func _gift(e: Dictionary) -> void:
	var coin := Sprite2D.new()
	coin.texture = Art.tex("gift_coin_" + GIFT_COIN.get(e["species"], "wake"))
	coin.position = _door_point() + Vector2(0, 120)
	coin.scale = Vector2.ONE * 0.2
	add_child(coin)
	Sound.play("collect")
	var tw := create_tween()
	tw.tween_property(coin, "scale", Vector2.ONE * 1.0, 0.25).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	tw.tween_interval(0.35)
	tw.tween_property(coin, "modulate:a", 0.0, 0.2)
	await tw.finished
	coin.queue_free()

func _celebrate() -> void:
	busy = true
	Sound.play("win_fanfare")
	await get_tree().create_timer(0.4 if not instant else 0.0).timeout
	won.emit({"session": session})

## Called after a booster purchase.
func granted(kind: int, count: int) -> void:
	session.grant_booster(kind, count)
	refresh_hud()
