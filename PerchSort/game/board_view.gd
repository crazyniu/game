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
const BOOSTER_SOUND := {Board.Booster.FIREFLY: "hint", Board.Booster.SAPLING: "sapling", Board.Booster.OWL_EYE: "owl_eye",
	Board.Booster.MORNING_BELL: "morning_bell", Board.Booster.SCARECROW: "scarecrow", Board.Booster.SPRING_RAIN: "spring_rain"}
## Entrance beats (seconds): houses drop in, branches grow, then birds fly in; ends about 3.9 s after the first frame.
const ENTRANCE_HOUSE := 0.1
const ENTRANCE_BRANCHES := 0.95
const ENTRANCE_BIRDS := 1.6
const ENTRANCE_BIRD_SPREAD := 0.85
const ENTRANCE_FLIGHT := 0.8
const SKIP_TIME_SCALE := 12.0
const BUBBLE_SECONDS := 1.6
const STUCK_DELAY := 0.8

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
const END_CLEAR := 125.0      # last seat centre to the branch tip (painted px): clear of the leaf tuft
const SEAT_FIRST := 98.0      # seat 0 distance from the screen edge at density 1
const SEAT_OUTWARD_SHIFT := 24.0  # screen pixels toward each branch's outer edge
const BIRD_SCALE := 1.10
const PERCH_TOP := 660.0
const PERCH_BOTTOM := 1640.0
const ROW_REFLOW_SECONDS := 0.45
var _branch_widths := {}      # slot key -> drawn width share, so growth can animate
var _slots: Array = []       # painted branch slots of this world (world/scene/board_wN_slots.json)
var _row_offset := 0
var _rows := 2
var _perch_step := 0.0
var _house: Control
var _goal_nodes := {}   # species -> Control
var _sun_pill: Control
var _sun_number: Control
var _tool_bar: Control
var _sky: Node2D
var _motes: Array = []  # ambient drifters: {node, kind, origin, period, offset, alpha, drift}
var _clock := 0.0
var _rope: Control
var _glow: Control
var _door_open: Control
var _door_mask: Control
var _resident: BirdView
var _resident_species := -1
var _goal_gift := {}    # species -> gift badge shown until delivered
var _goal_check := {}   # species -> check mark shown after delivery
var _tool_slots: Array = []  # [kind, centre] per visible tool, for the stuck pointer
var _bubble: Node2D
var _bubble_tween: Tween
var _stuck: Control
var _entrance_running := false
var _hint_button: Button
var _hint_task := -1
var _hint_result := {}
var _hint_moves := {}
var _shown_hint_key := PackedInt32Array()
var _hint_pointer: Sprite2D
var _hint_motion: Tween
var _hint_move: Array = []


func start(level_def: Dictionary, player: Profile, play_entrance := true) -> void:
	_release_hint_task()
	_hide_hint()
	_hint_moves.clear()
	_shown_hint_key = PackedInt32Array()
	busy = false
	level = level_def
	profile = player
	session = GameSession.new(level, profile.inventory)
	for child in get_children():
		child.queue_free()
	_birds.clear()
	_goal_nodes.clear()
	_motes.clear()
	_resident_species = -1
	Engine.time_scale = 1.0
	size = Vector2(1080, 1920)
	_build_scene()
	_build_hud()
	_layout_branches()
	_spawn_birds()
	refresh_hud()
	if play_entrance and not instant:
		_entrance()

func _exit_tree() -> void:
	_release_hint_task()
	_hide_hint()
	Engine.time_scale = 1.0

## ---------- scene ----------

func _world() -> int:
	return session.state.world

func _build_scene() -> void:
	# One painting per world (docs/design/ui-system.md): the branch-free scene, then each branch layer cut from the
	# same painting (tools/scene/cut_branches.py), so every branch keeps the painting's light and softness.
	UI.image(self, "board_w%d_bg" % _world(), Rect2(0, 0, 1080, 1920), false)
	_slots = JSON.parse_string(FileAccess.get_file_as_string("res://world/scene/board_w%d_slots.json" % _world()))["slots"]
	_build_ambient()
	_scene = Node2D.new()
	_scene.name = "Scene"
	add_child(_scene)
	_branch_layer = Node2D.new()
	_scene.add_child(_branch_layer)
	_bird_layer = Node2D.new()
	_scene.add_child(_bird_layer)
	_night = UI.image(self, "night_overlay", Rect2(0, 0, 1080, 1920), false)
	_night.modulate.a = 0.0

## ---------- ambient ----------

## Rain and drizzle levels add falling drops over the painting.
func _build_ambient() -> void:
	_sky = Node2D.new()
	_sky.name = "Ambient"
	add_child(_sky)
	if session.state.variant in ["rain", "drizzle"]:
		for i in 16:
			_add_mote("raindrop", "rain", Vector2((i * 139 + 47) % 1080, 0), Vector2(9, 26), 1.6, i * 0.113, 0.3, 8)
	_tick_ambient()

func _add_mote(sprite: String, kind: String, origin: Vector2, size_px: Vector2, period: float, offset: float, alpha: float, drift: float) -> void:
	var node := Sprite2D.new()
	node.texture = Art.tex(sprite)
	node.scale = size_px / UI.used_rect(sprite).size
	_sky.add_child(node)
	_motes.append({"node": node, "kind": kind, "origin": origin, "period": period, "offset": offset, "alpha": alpha, "drift": drift})

func _process(delta: float) -> void:
	_clock += delta
	_tick_ambient()
	_poll_hint()

func _tick_ambient() -> void:
	for m in _motes:
		var u := fposmod(_clock / m["period"] + m["offset"], 1.0)
		var at: Vector2 = m["origin"]
		var node: Sprite2D = m["node"]
		var fade := minf(1.0, minf(u, 1.0 - u) * 12.0)
		at += Vector2(sin(u * TAU) * m["drift"], -40.0 + 2000.0 * u)
		node.position = at
		node.modulate.a = m["alpha"] * fade

func _row_count() -> int:
	var rows := 1
	for r in session.state.rows:
		rows = maxi(rows, r + 1)
	return rows

## The painted slot for a board row and side. A level uses a block of the world's rows centred in the painting,
## chosen at the start so the sapling's extra row still has a painted slot below it.
func _slot(row: int, right: bool) -> Dictionary:
	var side := "R" if right else "L"
	var r := clampi(row + _row_offset, 0, _slot_rows() - 1)
	for s in _slots:
		if s["side"] == side and int(s["row"]) == r:
			return s
	return {}

func _slot_rows() -> int:
	var n := 0
	for s in _slots:
		n = maxi(n, int(s["row"]) + 1)
	return n

func _layout_branches() -> void:
	_rows = _row_count()
	var painted := _slot_rows()
	if _branch_nodes.is_empty():
		_row_offset = clampi(mini((painted - _rows) / 2, painted - _rows - 1), 0, maxi(0, painted - _rows))
	# Use the available play area instead of the painting's original vertical placement.
	var s := session.state
	var last_perch := 0
	for b in s.branch_count():
		last_perch = maxi(last_perch, s.rows[b] * 2 + s.rights[b])
	_perch_step = (PERCH_BOTTOM - PERCH_TOP) / float(last_perch)
	var before := _branch_nodes.size()
	var previous_positions: Array[Vector2] = []
	for node in _branch_nodes:
		previous_positions.append(node.position)
		node.queue_free()
	_branch_nodes.clear()
	for b in s.branch_count():
		var right := s.rights[b] == 1
		var slot := _slot(s.rows[b], right)
		var twig := s.kinds[b] == Board.Kind.TWIG
		var y := perch_y(b)
		# The node origin is where the branch leaves the screen edge, so wobble and growth pivot there.
		var node := Node2D.new()
		var edge := 1080.0 if right else 0.0
		node.position = Vector2(edge, y + 20.0 * BIRD_SCALE)
		_add_branch_art(node, slot, right, b, twig)
		if s.kinds[b] == Board.Kind.SWIVEL:
			var knot := Sprite2D.new()
			knot.texture = Art.tex("swivel_knot")
			knot.scale = Vector2.ONE * 0.5 * BIRD_SCALE
			knot.position = Vector2((930.0 if right else 150.0) - edge, 10.0 * BIRD_SCALE)
			node.add_child(knot)
			UI.hit_area(node, Rect2(knot.position - Vector2.ONE * 48.0, Vector2.ONE * 96.0), func(): _tap_swivel(b))
		_branch_layer.add_child(node)
		_branch_nodes.append(node)
		if b >= before and before > 0 and not instant:
			# A branch added by the sapling grows in from the edge.
			node.scale = Vector2(0.05, 0.6)
			create_tween().tween_property(node, "scale", Vector2.ONE, 0.6).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
		elif b < before and not instant and node.position != previous_positions[b]:
			var target := node.position
			node.position = previous_positions[b]
			node.create_tween().tween_property(node, "position", target, ROW_REFLOW_SECONDS).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)

## The painted branch layer in a child node anchored at the screen edge. A branch with fewer than four seats
## (twig, growing branch) is the same painting narrowed from the edge, so its tuft stays whole; twigs are also
## thinner. Growth tweens the width.
func _add_branch_art(node: Node2D, slot: Dictionary, right: bool, b: int, twig: bool) -> void:
	var tex := Art.tex(slot["name"])
	var at := Vector2(slot["at"][0], slot["at"][1])
	at.y += perch_y(b) - float(slot["perch"])
	var art := Node2D.new()
	art.name = "Art"
	node.add_child(art)
	var spr := Sprite2D.new()
	spr.texture = tex
	spr.centered = false
	spr.position = at - node.position
	art.add_child(spr)
	var seats := maxi(session.state.caps[b], 2)
	var width := _branch_width(b, seats) / _painted_length(slot)
	art.scale = Vector2(width, 0.75 if twig else 1.0)
	var key := "%s%d" % ["R" if right else "L", session.state.rows[b]]
	var before: float = _branch_widths.get(key, width)
	_branch_widths[key] = width
	if not instant and absf(before - width) > 0.01:
		art.scale.x = before
		create_tween().tween_property(art, "scale:x", width, 0.6).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)

## Painted wood length of a slot from its screen edge to the end of its tuft.
func _painted_length(slot: Dictionary) -> float:
	var tip := float(slot["tip"])
	return tip if slot["side"] == "L" else 1080.0 - tip

## Seat pitch of a branch: four seats spread over its painted length, clear of the tuft.
func _seat_step(b: int) -> float:
	var s := session.state
	var slot := _slot(s.rows[b], s.rights[b] == 1)
	var free := _painted_length(slot) - SEAT_FIRST * BIRD_SCALE - END_CLEAR
	return clampf(free / 3.0, 100.0 * BIRD_SCALE, 150.0 * BIRD_SCALE)

## Length a branch needs for `seats` seats, from the screen edge to the end of its tuft.
func _branch_width(b: int, seats: int) -> float:
	var s := session.state
	var full := _painted_length(_slot(s.rows[b], s.rights[b] == 1))
	return minf(full, SEAT_FIRST * BIRD_SCALE + (seats - 1) * _seat_step(b) + END_CLEAR)

func _sprite(name: String, pos: Vector2, flip: bool) -> Sprite2D:
	var spr := Sprite2D.new()
	spr.texture = Art.tex(name)
	spr.centered = false
	spr.flip_h = flip
	spr.position = pos
	return spr

## Perches fill the play area, alternating left and right in the authored row order.
func perch_y(b: int) -> float:
	var s := session.state
	return PERCH_TOP + (s.rows[b] * 2 + s.rights[b]) * _perch_step

## Seat x on branch b; seat 0 is nearest the screen edge.
func seat_x(b: int, i: int) -> float:
	var d := SEAT_FIRST * BIRD_SCALE + i * _seat_step(b) - SEAT_OUTWARD_SHIFT
	return 1080.0 - d if session.state.rights[b] == 1 else d

func seat_point(b: int, i: int) -> Vector2:
	return Vector2(seat_x(b, i), perch_y(b))

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
	bird.setup(code, session.state.is_asleep(code), BIRD_SCALE, profile.equipped.get(species, "").replace("outfit.", "").replace(".", "_outfit_") if profile.equipped.has(species) else "")
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
	var glide: Tween = null
	for b in s.branch_count():
		for i in s.items[b].size():
			var code := s.items[b][i]
			var id := Board.id_of(code)
			seen[id] = true
			var bird: BirdView = _birds.get(id)
			if bird == null or not is_instance_valid(bird):
				bird = _make_bird(code, seat_point(b, i))
			elif bird.code != code or bird.asleep != s.is_asleep(code) or bird.density != BIRD_SCALE:
				bird.setup(code, s.is_asleep(code), BIRD_SCALE, bird.outfit)
			if instant or bird.position.distance_to(seat_point(b, i)) < 2.0:
				bird.position = seat_point(b, i)
			else:
				# Rows can shift when a branch is added; birds glide to their new seats.
				if glide == null:
					glide = create_tween().set_parallel()
				glide.tween_property(bird, "position", seat_point(b, i), ROW_REFLOW_SECONDS).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
			bird.set_selected(false)
	for id in _birds.keys():
		if not seen.has(id):
			if is_instance_valid(_birds[id]):
				_birds[id].queue_free()
			_birds.erase(id)
	_night.modulate.a = 0.45 if s.phase == Board.Phase.NIGHT else 0.0
	if glide:
		await glide.finished

## Houses drop in, branches grow out of the trunks, then the birds fly to their seats. A tap speeds it up.
func _entrance() -> void:
	busy = true
	_entrance_running = true
	modulate.a = 0.0
	create_tween().tween_property(self, "modulate:a", 1.0, 0.4)
	var rise := create_tween().set_parallel()
	var drop := [_rope, _house]
	for node in drop:
		var rest_y: float = node.position.y
		node.position.y -= 420.0
		rise.tween_property(node, "position:y", rest_y, 0.7).set_delay(ENTRANCE_HOUSE).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	var k := 0
	for goal in _goal_nodes.values():
		goal.scale = Vector2.ZERO
		rise.tween_property(goal, "scale", Vector2.ONE, 0.35).set_delay(ENTRANCE_HOUSE + 0.5 + 0.08 * k).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
		k += 1
	var grow := create_tween().set_parallel()
	for i in _branch_nodes.size():
		var node: Node2D = _branch_nodes[i]
		node.scale = Vector2(0.02, 0.6)
		grow.tween_property(node, "scale", Vector2.ONE, 0.5).set_delay(ENTRANCE_BRANCHES + 0.1 * i).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	var s := session.state
	var birds: Array = []
	for b in s.branch_count():
		for i in s.items[b].size():
			birds.append([s.items[b][i], b])
	var n := birds.size()
	var last: BirdView = null
	for j in n:
		var bird: BirdView = _birds[Board.id_of(birds[j][0])]
		var target := bird.position
		var right: bool = s.rights[birds[j][1]] == 1
		bird.position = Vector2(1240.0 if right else -160.0, target.y - 420.0 - randf() * 200.0)
		bird.fly_to(target, ENTRANCE_BIRDS + ENTRANCE_BIRD_SPREAD * j / maxf(1.0, n - 1.0), ENTRANCE_FLIGHT, false, true)
		last = bird
	if last:
		await last.finished
	await get_tree().create_timer(0.1).timeout
	Engine.time_scale = 1.0
	_entrance_running = false
	busy = false
	_watch_stuck()

## ---------- HUD ----------

## Level and phase share a compact row, clear of the central house.
const LEVEL_RECT := Rect2(48, 64, 200, 80)
const PHASE_RECT := Rect2(264, 64, 136, 80)
const GEAR := Vector2(968, 104)
const TOOL_Y := 1792.0
const TOOL_GAP := 48.0
## Hanging house: sprite, visible content and doorway in sprite pixels, and where the rope meets it.
const HOUSES := {
	"default": ["house_default", Rect2(122, 62, 778, 878), Rect2(313, 418, 378, 380), 70.0],
	"acorn": ["house_style_acorn_body", Rect2(56, 51, 397, 403), Rect2(148, 205, 204, 199), 51.0],
	"teapot": ["house_style_teapot_body", Rect2(29, 55, 458, 398), Rect2(148, 205, 204, 199), 55.0],
	"mushroom": ["house_style_mushroom_body", Rect2(41, 51, 426, 401), Rect2(148, 204, 204, 200), 51.0],
	"lantern": ["house_style_lantern_body", Rect2(50, 61, 408, 393), Rect2(148, 204, 204, 200), 61.0]}
const HOUSE_WIDTH := 220.0
const HOUSE_TOP := 40.0
const ROPE_WIDTH := 16.0
const GOAL := 112.0
const GOAL_GAP := 24.0
const GOAL_TOP_GAP := 20.0
const HUD_ICON := 64.0
const BUBBLE_H := 112.0
const BUBBLE_PAD := 32.0
const BUBBLE_ICON_GAP := 16.0

var _door_rect := Rect2()

func _house_spec() -> Array:
	return HOUSES.get(profile.equipped_house, HOUSES["default"])

func _house_rect() -> Rect2:
	var content: Rect2 = _house_spec()[1]
	var k := HOUSE_WIDTH / content.size.x
	return Rect2(540.0 - HOUSE_WIDTH * 0.5, HOUSE_TOP, HOUSE_WIDTH, content.size.y * k)

func _build_hud() -> void:
	_hud = Control.new()
	_hud.name = "HUD"
	_hud.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_hud.size = Vector2(1080, 1920)
	add_child(_hud)
	# Rope and hanging house.
	var spec := _house_spec()
	var content: Rect2 = spec[1]
	var hr := _house_rect()
	var k := HOUSE_WIDTH / content.size.x
	_glow = UI.art(_hud, "house_glow", Rect2(hr.get_center() - hr.size * 0.9, hr.size * 1.8))
	_glow.modulate = Color(1, 0.8, 0.35, 0.0)
	_rope = Control.new()
	_rope.name = "Rope"
	_rope.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_rope.clip_contents = true
	var rope_bottom := hr.position.y + (float(spec[3]) - content.position.y) * k
	# It starts above the screen so the entrance overshoot never shows its end.
	UI.place(_rope, Rect2(540.0 - ROPE_WIDTH * 0.5, -60.0, ROPE_WIDTH, rope_bottom + 60.0))
	_hud.add_child(_rope)
	var rope_k := ROPE_WIDTH / 108.0
	UI.art(_rope, "goal_rope_single", Rect2(0, 0, ROPE_WIDTH, 1260.0 * rope_k), Rect2(6, 0, 108, 1260))
	_house = Control.new()
	_house.name = "House"
	_house.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(_house, hr)
	_hud.add_child(_house)
	UI.art(_house, spec[0], Rect2(Vector2.ZERO, hr.size), content)
	# The doorway is dark until a species is delivered; the resident then looks out of it.
	var door: Rect2 = spec[2]
	var opening := Rect2((door.position - content.position) * k, door.size * k)
	_door_rect = Rect2(opening.position + hr.position, opening.size)
	_door_mask = TextureRect.new()
	_door_mask.texture = _disc_texture()
	_door_mask.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	_door_mask.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_door_mask.clip_children = CanvasItem.CLIP_CHILDREN_ONLY
	UI.place(_door_mask, opening)
	_house.add_child(_door_mask)
	_door_open = null
	var level_pill := UI.shape(_hud, LEVEL_RECT, "cream", LEVEL_RECT.size.y * 0.5, 6.0)
	level_pill.name = "Level"
	var status_cap := 32.0 * minf(1.0, (LEVEL_RECT.size.x - 44.0) / (UI.label_width("hud.level", 32.0) + UI.number_width("00", 32.0)))
	var label_w := UI.label_width("hud.level", status_cap)
	# Align the label's painted cap band, accounting for its baked line padding.
	UI.label(level_pill, "hud.level", Rect2(18, status_cap / 16.0, label_w, 74), status_cap)
	var number_x := 18.0 + label_w + 8.0
	UI.number(level_pill, str(session.state.level_id), Rect2(number_x, 37.0 - status_cap * 0.5, LEVEL_RECT.size.x - number_x - 18.0, status_cap))
	# Day and night: suns (or moons) left in the current phase.
	_sun_pill = UI.shape(_hud, PHASE_RECT, "cream", PHASE_RECT.size.y * 0.5, 6.0)
	_sun_pill.name = "Phase"
	UI.icon(_sun_pill, "sun", Vector2(38, 37), 44.0, Color8(255, 176, 40))
	_sun_number = UI.number(_sun_pill, "0", Rect2(70, 37.0 - status_cap * 0.5, 48, status_cap))
	_sun_pill.visible = session.state.day_length > 0
	UI.button(_hud, "Settings", GEAR, "cream", "gear", func(): settings_requested.emit())
	_build_goals()
	_tool_bar = Control.new()
	_tool_bar.name = "Tools"
	_tool_bar.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_hud.add_child(_tool_bar)

## Goal birds under the house: a cream circle with the face, the ability badge until delivered, then a check.
func _build_goals() -> void:
	var ids := Levels.species_ids(session.state)
	var n := ids.size()
	var size := GOAL
	var gap := GOAL_GAP
	if n * size + (n - 1) * gap > 984.0:
		gap = 12.0
		size = minf(GOAL, (984.0 - (n - 1) * gap) / maxf(1.0, n))
	var top := _house_rect().end.y + GOAL_TOP_GAP
	for i in n:
		var sp: String = ids[i]
		var cx := 540.0 + (i - (n - 1) * 0.5) * (size + gap)
		var node := Control.new()
		node.name = "Goal " + sp
		node.mouse_filter = Control.MOUSE_FILTER_IGNORE
		UI.place(node, Rect2(Vector2(cx - size * 0.5, top), Vector2.ONE * size))
		_hud.add_child(node)
		UI.shape(node, Rect2(Vector2.ZERO, Vector2.ONE * size), "cream", size * 0.5, 6.0)
		var face := size * 0.78
		UI.art(node, sp + "_face", Rect2(Vector2.ONE * (size - face) * 0.5 + Vector2(0, -4), Vector2.ONE * face))
		var badge := Rect2(Vector2.ONE * size * 0.6, Vector2.ONE * size * 0.46)
		_goal_gift[sp] = UI.art(node, "gift_coin_" + GIFT_COIN[sp], badge)
		var check := UI.shape(node, badge, "green", badge.size.x * 0.5, 4.0, false)
		UI.icon(check, "check", badge.size * 0.5, badge.size.x * 0.6)
		_goal_check[sp] = check
		check.visible = false
		UI.hit_area(node, Rect2(Vector2.ZERO, Vector2.ONE * size), func(): goal_info_requested.emit(sp))
		_goal_nodes[sp] = node

func refresh_hud() -> void:
	var s := session.state
	if s.day_length > 0:
		UI.set_number(_sun_number, str(s.day_length - s.phase_counter))
		var night := s.phase == Board.Phase.NIGHT
		for child in _sun_pill.get_children():
			if child is TextureRect and String(child.name).begins_with("icon_"):
				child.texture = UI.icon_texture("moon" if night else "sun")
				child.modulate = Color8(160, 170, 230) if night else Color8(255, 176, 40)
	for sp in _goal_nodes:
		var home: bool = s.homed.has(Board.species_index(sp))
		_goal_gift[sp].visible = not home
		_goal_check[sp].visible = home
	_set_resident(s.homed[s.homed.size() - 1] if not s.homed.is_empty() else -1)
	_hide_stuck()
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

const TOOL_ICON_NAME := {Board.Booster.FIREFLY: "bulb", Board.Booster.SAPLING: "sapling", Board.Booster.OWL_EYE: "eye",
	Board.Booster.MORNING_BELL: "bell", Board.Booster.SCARECROW: "scarecrow", Board.Booster.SPRING_RAIN: "rain"}

## Tools as a centred row of buttons: undo (cream) then boosters (orange) with a red stock badge ("+" buys more).
func _build_tools() -> void:
	_hint_button = null
	for child in _tool_bar.get_children():
		child.queue_free()
	var tools := _visible_tools()
	_tool_slots.clear()
	var n := tools.size()
	var x := 540.0 - (n * UI.SECONDARY + (n - 1) * TOOL_GAP) * 0.5 + UI.SECONDARY * 0.5
	for kind in tools:
		var centre := Vector2(x, TOOL_Y)
		x += UI.SECONDARY + TOOL_GAP
		_tool_slots.append([kind, centre, kind < 0 or session.state.inventory[kind] == 0 or session.booster_has_effect(kind)])
		if kind < 0:
			UI.button(_tool_bar, "Undo", centre, "cream", "undo", _on_undo)
			continue
		var b := UI.button(_tool_bar, "Tool %d" % kind, centre, "orange", TOOL_ICON_NAME[kind], func(): _on_tool(kind))
		if kind == Board.Booster.FIREFLY:
			_hint_button = b
		var stock: int = session.state.inventory[kind]
		UI.badge(b, str(stock) if stock > 0 else "+")
		UI.set_active(b, stock == 0 or session.booster_has_effect(kind))
	if _hint_task >= 0:
		_show_hint_loading()

## ---------- input ----------

func _gui_input(event: InputEvent) -> void:
	var pressed: bool = event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT
	if pressed and _entrance_running:
		Engine.time_scale = SKIP_TIME_SCALE
		accept_event()
		return
	if pressed:
		_hide_bubble()
		_hide_stuck()
	if busy or session.state.is_won():
		return
	if pressed:
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
		var dy := y - 70.0 * BIRD_SCALE - p.y
		if x_ok and absf(dy) < 110.0 and absf(dy) < best_d:
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
		_refresh_hint_selection()
		return
	var from := session.selected
	if from == b:
		_lift(from, false)
		session.cancel_selection()
		if Rules.can_swivel(s, b):
			_do(func(): return session.swivel(b))
		else:
			_refresh_hint_selection()
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
		_refresh_hint_selection()

func _tap_swivel(b: int) -> void:
	if busy or session.state.is_won():
		return
	_hide_bubble()
	if session.selected >= 0:
		_lift(session.selected, false)
	_do(func(): return session.swivel(b))

func _refresh_hint_selection() -> void:
	if _hint_move.is_empty():
		return
	if session.selected < 0 or session.selected == _hint_move[0]:
		show_hint(_hint_move)
	else:
		_hide_hint()

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
				other.look_at_x(seat_x(b, 0))

## A refused tap: a sleeping bird gets a visible reaction and an explanation, anything else a branch shake.
func _refuse(b: int) -> void:
	var sleeper := _sleeper_on(b)
	if sleeper != null:
		Sound.play("sleep")
		if sleeper.has_method("nudge_asleep"):
			sleeper.nudge_asleep()
		_show_bubble(sleeper)
	else:
		Sound.play("error")
	if b < 0 or b >= _branch_nodes.size():
		return
	var node: Node2D = _branch_nodes[b]
	var tw := create_tween()
	for k in 4:
		tw.tween_property(node, "rotation", 0.025 * (1 if k % 2 == 0 else -1), 0.05)
	tw.tween_property(node, "rotation", 0.0, 0.05)

## The topmost sleeping bird of a branch, or null.
func _sleeper_on(b: int) -> BirdView:
	if b < 0 or b >= session.state.branch_count():
		return null
	var list := session.state.items[b]
	for i in range(list.size() - 1, -1, -1):
		if (list[i] & Board.EGG) == 0 and session.state.is_asleep(list[i]):
			return _birds.get(Board.id_of(list[i]))
	return null

## Sleep cues distinguish waiting for night from a matching bird landing or a nearby flock's song.
func _show_bubble(bird: BirdView) -> void:
	_hide_bubble()
	var icons: Array[String] = []
	if Board.is_nocturnal(Board.species_of(bird.code)) and session.state.nocturnal_day_sleep():
		icons.append("moon")
	if (bird.code & Board.ASLEEP) != 0:
		icons.append(bird.species + "_face")
		icons.append("gift_coin_wake")
	if icons.is_empty():
		return
	var n := icons.size()
	var body_w := 2.0 * BUBBLE_PAD + n * HUD_ICON + (n - 1) * BUBBLE_ICON_GAP
	_bubble = Node2D.new()
	_bubble.name = "SleepBubble"
	# A cream speech pill with a small round tail dot; the node origin is just above the bird's head.
	var holder := Control.new()
	holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_bubble.add_child(holder)
	var body := Rect2(Vector2(-BUBBLE_PAD - 8.0, -BUBBLE_H - 36.0), Vector2(body_w, BUBBLE_H))
	UI.shape(holder, body, "cream", BUBBLE_H * 0.5, 8.0)
	UI.shape(holder, Rect2(Vector2(-14, -34), Vector2(28, 28)), "cream", 14.0, 4.0)
	for i in n:
		var c := Vector2(body.position.x + BUBBLE_PAD + HUD_ICON * 0.5 + i * (HUD_ICON + BUBBLE_ICON_GAP), body.position.y + (BUBBLE_H - 8.0) * 0.5)
		if icons[i] == "moon":
			UI.icon(holder, "moon", c, HUD_ICON, Color8(120, 130, 200))
		else:
			UI.art(holder, icons[i], Rect2(c - Vector2.ONE * HUD_ICON * 0.5, Vector2.ONE * HUD_ICON))
	var tip := bird.position + Vector2(-10.0, -135.0 * bird.density)
	tip.x = clampf(tip.x, 48.0 + BUBBLE_PAD + 8.0, 1032.0 - body_w + BUBBLE_PAD + 8.0)
	_bubble.position = tip
	_scene.add_child(_bubble)
	var node := _bubble
	if instant:
		return
	node.scale = Vector2.ZERO
	_bubble_tween = create_tween()
	_bubble_tween.tween_property(node, "scale", Vector2.ONE, 0.18).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	_bubble_tween.tween_method(func(t: float): node.position.y = tip.y - 6.0 * sin(t * TAU), 0.0, 1.0, BUBBLE_SECONDS)
	_bubble_tween.tween_property(node, "modulate:a", 0.0, 0.2)
	_bubble_tween.tween_callback(_hide_bubble)

func _hide_bubble() -> void:
	if _bubble_tween:
		_bubble_tween.kill()
		_bubble_tween = null
	if _bubble:
		_bubble.queue_free()
		_bubble = null

## ---------- stuck hint ----------

## With no legal move left, a gold glow and a bobbing pointer mark the tool that helps (undo first).
func _watch_stuck() -> void:
	_hide_stuck()
	var s := session.state
	if instant or s.is_won() or not session.can_undo() or Rules.has_legal_move(s):
		return
	var moves := session.move_count()
	await get_tree().create_timer(STUCK_DELAY).timeout
	if is_inside_tree() and not busy and session.state == s and session.move_count() == moves:
		show_stuck_hint()

func show_stuck_hint() -> void:
	_hide_stuck()
	var pick := -1
	for i in _tool_slots.size():
		if _tool_slots[i][2] and (pick < 0 or _tool_slots[i][0] < 0):
			pick = i
	if pick < 0:
		return
	var centre: Vector2 = _tool_slots[pick][1]
	_stuck = Control.new()
	_stuck.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_hud.add_child(_stuck)
	var glow_size := Vector2.ONE * UI.SECONDARY * 1.375
	var glow := UI.image(_stuck, "glow_soft", Rect2(centre - glow_size * 0.5, glow_size))
	glow.modulate = Color(1, 0.72, 0.15, 0.3)   # translucent gold so the disc stays readable
	# Pointer 16 px above the disc (and its stock badge), pointing down; it bobs upwards so it never touches.
	var arrow_rect := Rect2(centre + Vector2(-HUD_ICON * 0.5, -UI.SECONDARY * 0.5 - 24.0 - HUD_ICON), Vector2.ONE * HUD_ICON)
	var arrow := UI.icon(_stuck, "right", arrow_rect.get_center(), HUD_ICON, UI.FILL["orange"])
	var base_y := arrow.position.y
	arrow.rotation = PI * 0.5   # points down at the tool
	if instant:
		return
	var tw := _stuck.create_tween().set_loops()
	tw.tween_method(func(t: float):
		var wave := (1.0 - cos(t * TAU)) * 0.5
		_stuck.modulate.a = 0.7 + 0.3 * wave
		arrow.position.y = base_y - wave * 12.0, 0.0, 1.0, 1.2)

func _hide_stuck() -> void:
	if _stuck:
		_stuck.queue_free()
		_stuck = null

func _do(action: Callable) -> void:
	_hide_hint()
	busy = true
	var ok: bool = action.call()
	if ok:
		await play_events(session.events.duplicate())
	await sync_birds()
	refresh_hud()
	busy = false
	if session.state.is_won():
		_celebrate()
	else:
		_watch_stuck()

func _on_undo() -> void:
	if busy or not session.undo():
		return
	_hide_hint()
	Sound.play("undo")
	busy = true
	_hide_bubble()
	_layout_branches()
	await sync_birds()
	refresh_hud()
	busy = false
	_watch_stuck()

func _on_tool(kind: int) -> void:
	if busy:
		return
	if kind == Board.Booster.FIREFLY:
		_hint()
		return
	if session.state.inventory[kind] == 0:
		buy_requested.emit(kind)
		return
	_hide_hint()
	busy = true
	var result := session.use_booster(kind)
	if result.get("applied", false):
		profile.inventory[kind] = session.state.inventory[kind]
		profile_changed.emit()
		Sound.play(BOOSTER_SOUND[kind])
		await play_events(session.events.duplicate())
	else:
		Sound.play("error")
	_layout_branches()
	await sync_birds()
	refresh_hud()
	busy = false
	_watch_stuck()

## Hint search has one short worker budget; later steps reuse its complete winning route.
func _hint() -> void:
	var key := session.state.search_key(false)
	if _shown_hint_key == key and _hint_moves.has(key):
		show_hint(_hint_moves[key])
		return
	if session.state.inventory[Board.Booster.FIREFLY] == 0:
		buy_requested.emit(Board.Booster.FIREFLY)
		return
	if _hint_moves.has(key):
		_apply_hint(_hint_moves[key])
		return
	_hide_hint()
	busy = true
	var snapshot := session.state.clone()
	var last := session.last_move.duplicate()
	_hint_result = {}
	var result := _hint_result
	_hint_task = WorkerThreadPool.add_task(func(): result.merge(Solver.hint(snapshot, last)))
	_build_tools()

func _poll_hint() -> void:
	if _hint_task < 0 or not WorkerThreadPool.is_task_completed(_hint_task):
		return
	WorkerThreadPool.wait_for_task_completion(_hint_task)
	_hint_task = -1
	busy = false
	if _hint_result["status"] != Solver.Status.SOLVED or _hint_result["moves"].is_empty():
		refresh_hud()
		_show_hint_failure(_hint_result["status"])
		Sound.play("error")
		return
	_hint_moves.merge(_hint_result["continuation"], true)
	_apply_hint(_hint_result["moves"][0])

func _release_hint_task() -> void:
	if _hint_task >= 0:
		WorkerThreadPool.wait_for_task_completion(_hint_task)
		_hint_task = -1
	_hint_result = {}

func _show_hint_loading() -> void:
	for tool in _tool_bar.get_children():
		if tool is Button:
			UI.set_active(tool, false)
	_hint_button.modulate.a = 1.0
	_hint_button.get_node("Icon").hide()
	var spinner := Node2D.new()
	spinner.position = Vector2(UI.SECONDARY * 0.5, UI.SECONDARY * 0.5 - 5.0)
	_hint_button.add_child(spinner)
	var arc := Line2D.new()
	arc.width = 6.0
	arc.default_color = Color.WHITE
	arc.antialiased = true
	for i in 25:
		arc.add_point(Vector2.from_angle(float(i) / 24.0 * PI * 1.5) * 25.0)
	spinner.add_child(arc)
	if not instant:
		spinner.create_tween().set_loops().tween_property(spinner, "rotation", TAU, 0.8).from(0.0)

func _show_hint_failure(status: int) -> void:
	var icon: TextureRect = _hint_button.get_node("Icon")
	icon.texture = UI.icon_texture("retry" if status == Solver.Status.BUDGET_EXCEEDED else "close")
	for state in ["normal", "hover", "focus"]:
		_hint_button.add_theme_stylebox_override(state, UI._box(UI.FILL["red"], UI.RIM["red"], UI.SECONDARY * 0.5, 10.0))

func _apply_hint(move: Array) -> void:
	if session.selected >= 0:
		_lift(session.selected, false)
	if not session.try_use_firefly(move):
		return
	_shown_hint_key = session.state.search_key(false)
	profile.inventory[Board.Booster.FIREFLY] = session.state.inventory[Board.Booster.FIREFLY]
	profile_changed.emit()
	Sound.play("hint")
	refresh_hud()
	show_hint(move)

## Point to the next click: awake source first, then its landing seat; swivels target the knot.
func show_hint(move: Array) -> void:
	_hide_hint()
	_hint_move = move.duplicate()
	var hand := Sprite2D.new()
	_hint_pointer = hand
	hand.texture = Art.tex("hand_pointer")
	hand.scale = Vector2.ONE * 0.45
	hand.offset = Vector2(40, 60)
	add_child(hand)
	var target: Vector2
	if move[1] < 0:
		target = Vector2(930.0 if session.state.rights[move[0]] == 1 else 150.0, perch_y(move[0]) + 30.0 * BIRD_SCALE)
	elif session.selected == move[0]:
		target = seat_point(move[1], session.state.items[move[1]].size()) + Vector2(0, -60)
	else:
		target = seat_point(move[0], session.state.items[move[0]].size() - 1) + Vector2(0, -60)
	hand.position = target
	if instant:
		return
	var tw := hand.create_tween().set_loops()
	_hint_motion = tw
	tw.set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	tw.tween_property(hand, "position:y", target.y - 18.0, 0.4)
	tw.tween_property(hand, "position:y", target.y, 0.4)

func _hide_hint() -> void:
	_hint_move = []
	if _hint_motion:
		_hint_motion.kill()
		_hint_motion = null
	if _hint_pointer:
		_hint_pointer.queue_free()
		_hint_pointer = null

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
	return _door_rect.get_center()

func _flock_home(e: Dictionary) -> void:
	var ids: PackedInt32Array = e["ids"]
	_door(1.0, 0.2)
	var last: BirdView = null
	for k in ids.size():
		var bird: BirdView = _birds.get(ids[ids.size() - 1 - k])
		if bird == null:
			continue
		_birds.erase(ids[ids.size() - 1 - k])
		# Approach in front of the house; clipping starts only once the bird fits inside its doorway.
		bird.reparent(_hud)
		bird.fly_to(_door_point(), k * 0.12, 0.6, true, false, _door_mask)
		last = bird
	if last:
		await last.finished
	Sound.play("nest_home")
	_set_resident(Board.species_index(e["species"]), true)
	_goal_gift[e["species"]].visible = false
	_goal_check[e["species"]].visible = true
	var tw := create_tween()
	tw.tween_property(_house, "scale", Vector2(1.08, 0.94), 0.1)
	tw.tween_property(_house, "scale", Vector2.ONE, 0.25).set_trans(Tween.TRANS_BACK)
	var glow := create_tween()
	glow.tween_property(_glow, "modulate:a", 0.6, 0.15)
	glow.tween_property(_glow, "modulate:a", 0.0, 0.35)
	var goal: Control = _goal_nodes.get(e["species"])
	if goal:
		var pop := create_tween()
		pop.tween_property(goal, "scale", Vector2.ONE * 1.25, 0.12)
		pop.tween_property(goal, "scale", Vector2.ONE, 0.2)
	await tw.finished
	await _door(0.0, 0.25)

## Fades the open-door art in or out.
func _door(open: float, seconds: float) -> void:
	if _door_open == null:
		return
	var tw := create_tween()
	tw.tween_property(_door_open, "modulate:a", open, seconds)
	await tw.finished

## Soft white disc that masks the resident bird to the round doorway.
static var _disc: ImageTexture
func _disc_texture() -> ImageTexture:
	if _disc == null:
		var img := Image.create(128, 128, false, Image.FORMAT_RGBA8)
		for y in 128:
			for x in 128:
				var edge := 64.0 - Vector2(x + 0.5 - 64.0, y + 0.5 - 64.0).length()
				img.set_pixel(x, y, Color(1, 1, 1, clampf(edge, 0.0, 1.0)))
		_disc = ImageTexture.create_from_image(img)
	return _disc

## The delivered species looks out of the doorway; -1 leaves it empty and dark (undo, restart).
func _set_resident(species_index: int, rise := false) -> void:
	if species_index == _resident_species:
		return
	_resident_species = species_index
	if _resident and is_instance_valid(_resident):
		_resident.queue_free()
	_resident = null
	if species_index < 0:
		return
	_resident = BirdView.new()
	_resident.setup(Board.bird(0, species_index, 0), false, 0.58)
	_door_mask.add_child(_resident)
	var feet := Vector2(_door_mask.size.x * 0.5, _door_mask.size.y * 0.97)
	_resident.position = feet
	if instant:
		_resident.freeze()
	elif rise:
		_resident.position.y = feet.y + 90.0
		create_tween().tween_property(_resident, "position:y", feet.y, 0.3).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)

func _gift(e: Dictionary) -> void:
	var coin := Sprite2D.new()
	coin.texture = Art.tex("gift_coin_" + GIFT_COIN.get(e["species"], "wake"))
	coin.position = _door_point() + Vector2(0, 120)
	coin.scale = Vector2.ONE * 0.2
	add_child(coin)
	Sound.play("collect")
	var tw := create_tween()
	tw.tween_property(coin, "scale", Vector2.ONE * GOAL / float(coin.texture.get_width()), 0.25).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
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

## Capture states: "sleeping_bubble" (a tapped sleeping owl explains itself) and "stuck_hint" (no move left).
static func preview(main: Control, state: String) -> void:
	if state in ["hint_searching", "hint_timeout", "hint_unavailable", "hint_transfer"]:
		var player: Profile = main.profile.duplicate_profile()
		player.highest_unlocked = 40
		player.inventory[Board.Booster.FIREFLY] = 5
		var board := BoardView.new()
		board.instant = main.instant()
		main.set_screen(board)
		board.start(Levels.get_level(2), player, false)
		match state:
			"hint_searching":
				board.busy = true
				board._show_hint_loading()
			"hint_timeout": board._show_hint_failure(Solver.Status.BUDGET_EXCEEDED)
			"hint_unavailable": board._show_hint_failure(Solver.Status.UNSOLVABLE)
			"hint_transfer": board.show_hint([0, 2])
		return
	if state.begins_with("level_"):   # the real level on the board, without the first-visit popups
		var board := BoardView.new()
		board.instant = true
		main.set_screen(board)
		board.start(Levels.get_level(int(state.trim_prefix("level_"))), main.profile, false)
		return
	var def := {"id": 99, "world": 1, "variant": "morning", "capacity": 4, "par": 12, "dayLength": 4, "branches": [
		{"side": "L", "row": 0, "kind": "normal", "seats": 0, "birds": ["sunny", "sky", "hoot:sleep"]},
		{"side": "R", "row": 0, "kind": "normal", "seats": 0, "birds": ["sunny", "sky", "sunny"]},
		{"side": "L", "row": 1, "kind": "normal", "seats": 0, "birds": ["sunny", "sky", "hoot"]},
		{"side": "R", "row": 1, "kind": "normal", "seats": 0, "birds": ["hoot", "sky", "hoot"]}]}
	if state == "stuck_hint":
		def = JSON.parse_string(JSON.stringify(def).replace("hoot:sleep", "berry").replace("hoot", "berry"))
	var board := BoardView.new()
	board.instant = true
	main.set_screen(board)
	board.start(def, main.profile, false)
	if state == "sleeping_bubble":
		board.tap_branch(0)
	elif state == "stuck_hint":
		board.session.try_move(2, 3)   # gives undo a place in the tool bar
		board.sync_birds()
		board.refresh_hud()
		board.show_stuck_hint()
