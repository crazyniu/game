class_name BoardView
extends Control
## The level screen: forest, branches, birds, HUD and tool bar. Plays rule events as animation.

signal won(result: Dictionary)          # {"session": GameSession}
signal settings_requested
signal buy_requested(kind: int)          # booster with zero stock was tapped
signal undo_more_requested               # undo was tapped with no undo left in this attempt
signal bird_info_requested(species: String)   # a bird was held: its card explains what its flock's homecoming does
signal rescue_requested                  # first proven dead end of the attempt: the free rewind is offered
signal profile_changed

const GIFT_COIN := {"sunny": "wake", "sky": "reveal", "berry": "encore", "mint": "grow", "cocoa": "scare",
	"snow": "lightstep", "ember": "hatch", "ink": "encore", "hoot": "wake"}
const BOOSTER_SOUND := {Board.Booster.FIREFLY: "hint", Board.Booster.SAPLING: "sapling", Board.Booster.OWL_EYE: "owl_eye",
	Board.Booster.MORNING_BELL: "morning_bell", Board.Booster.SCARECROW: "scarecrow", Board.Booster.SPRING_RAIN: "spring_rain"}
## Entrance beats (seconds): houses drop in, branches grow, then birds fly in; ends about 3.9 s after the first frame.
const ENTRANCE_HOUSE := 0.1
const ENTRANCE_WING_DB := -15.0
const ENTRANCE_BRANCHES := 0.95
const ENTRANCE_BIRDS := 1.6
const ENTRANCE_BIRD_SPREAD := 0.85
const ENTRANCE_FLIGHT := 0.8
## Replays and the next level right after a win use the same beats, faster: about 1.5 s in all.
const ENTRANCE_QUICK := 0.36
const SKIP_TIME_SCALE := 12.0
const BUBBLE_SECONDS := 1.6
const STUCK_DELAY := 0.8
## Seconds without a tap or move before the hint tool is pointed at (once per level, only with hints in stock).
const IDLE_NUDGE := 12.0
const MOVE_FLIGHT_SECONDS := 0.28
const MOVE_STAGGER_SECONDS := 0.04
const MOVE_LANDING_SECONDS := 0.10

var session: GameSession
var profile: Profile
var level: Dictionary
var busy := false
var _buffering_move := false
var _queued_taps: Array = []   # at most the next source and destination, [branch, swivel]
var _playback_id := 0
var _playback_tweens: Array[Tween] = []
var _homing_birds := {}        # departing birds are no longer in _birds
var _home_effects: Node2D
var _idle_time := 0.0
var _idle_nudged := false
var instant := false   # captures: no motion

var _scene: Node2D
var _branch_layer: Node2D
var _bird_layer: Node2D
var _hud: Control
var _air: Ambient           # living air of the world painting: leaves, snow, mist, motes, sunlight
var _birds := {}        # bird id -> BirdView
var _branch_nodes: Array = []
## Trunk widths hidden off and visible at each screen edge, and how far a branch reaches under the trunk.
const END_CLEAR := 125.0      # last seat centre to the branch tip (painted px): clear of the leaf tuft
const SEAT_FIRST := 98.0      # seat 0 distance from the screen edge at density 1
const SEAT_OUTWARD_SHIFT := 38.0  # screen pixels toward each branch's outer edge (keeps seat 3 off the tuft)
const BIRD_SCALE := 1.10
const CALM_BACKDROP := preload("res://game/calm_backdrop.gdshader")
const SWIVEL_ART := "btn_swivel_green"
const SWIVEL_SIDE := 100.0    # turn-around button diameter at density 1
const SWIVEL_TIP_IN := 20.0   # its centre this far inside the branch tip
const SWIVEL_DROP := -8.0     # its centre relative to the branch line
const PERCH_TOP := 600.0   # the play area starts under the hanging house and its row of bird keys
const PERCH_BOTTOM := 1640.0
const PERCH_STEP_MAX := 150.0   # the largest gap between neighbouring perch lines (the fullest boards use about 140)
const ROW_REFLOW_SECONDS := 0.45
var _branch_widths := {}      # slot key -> drawn width share, so growth can animate
var _slots: Array = []       # painted branch slots of this world (world/scene/board_wN_slots.json)
var _row_offset := 0
var _rows := 2
var _perch_step := 0.0
var _perch_top := PERCH_TOP   # first perch line; the block of perches is centred in the play area
var _house: Control
var _status: Control          # stars and move count
var _status_stars: Array = []  # TextureRect per star
var _moves_number: Control
var _moves_icon: Control
var _stars_centred := false
var _stars_tween: Tween
var _shown_stars := -1
var _shown_moves := -1
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
var _tool_slots: Array = []  # [kind, centre] per visible tool, for the stuck pointer
var _bubble: Node2D
var _bubble_tween: Tween
var _stuck: Control
var _entrance_running := false
var _hint_button: Button
var _hint_thread: Thread    # dedicated, so it never queues behind engine pool tasks
var _hint_result := {}
var _hint_cancel: Array = [false]
var _hint_task_key := PackedInt32Array()
var _hint_moves := {}       # search_key -> move: winning routes and one-move fallbacks found in the background
var _hint_failures := {}    # search_key -> status; cancelled work is never cached
var _winnable := {}         # search_key -> true for positions on a found winning route
var _hint_waiting := false  # the firefly was pressed before the background search finished
var _hint_request_key := PackedInt32Array()
var _hint_press_usec := 0
var _hint_task_usec := 0
var _hint_level := -1
var _shown_hint_key := PackedInt32Array()
var _hint_pointer: Sprite2D
var _hint_motion: Tween
var _hint_mask: ColorRect
var _hint_trail: Node2D
var _hint_move: Array = []
var _home_chain := 0        # homings in the current chain already played


static var _start_hints: Dictionary = {}
## This run of the game: levels already opened, the last level started and the last level won.
static var _opened_levels: Dictionary = {}
static var _last_started := -1
static var _last_won := -1

## Hint routes from each level's start, precomputed by tools/levels/export_hints.gd, so the first hint is instant.
func _load_start_hints(id: int) -> void:
	if _start_hints.is_empty() and FileAccess.file_exists("res://levels/hints.json"):
		_start_hints = JSON.parse_string(FileAccess.get_file_as_string("res://levels/hints.json"))
	for entry in _start_hints.get(str(id), []):
		var key := PackedInt32Array()
		for v in entry[0]:
			key.append(int(v))
		_hint_moves[key] = [int(entry[1][0]), int(entry[1][1])]

func start(level_def: Dictionary, player: Profile, play_entrance := true) -> void:
	_rescue_declined = false
	_cancel_playback()
	_release_hint_task()
	_hide_hint()
	if int(level_def["id"]) != _hint_level:
		_hint_moves.clear()
		_hint_failures.clear()
		_winnable.clear()
		_load_start_hints(int(level_def["id"]))
	_hint_level = int(level_def["id"])
	_hint_waiting = false
	_shown_hint_key = PackedInt32Array()
	busy = false
	_entrance_running = false
	modulate.a = 1.0
	_idle_time = 0.0
	_idle_nudged = false
	level = level_def
	profile = player
	session = GameSession.new(level, profile.inventory)
	for child in get_children():
		child.queue_free()
	_birds.clear()
	_motes.clear()
	_shown_moves = -1
	_resident_species = -1
	Engine.time_scale = 1.0
	size = Vector2(1080, 1920)
	_build_scene()
	_build_hud()
	_build_skill_row()
	_layout_branches()
	_spawn_birds()
	refresh_hud()
	_prefetch_hint()
	var id := int(level_def["id"])
	var quick := _opened_levels.has(id) or (_last_won >= 0 and _last_won == _last_started and id == _last_won + 1)
	_opened_levels[id] = true
	_last_started = id
	if play_entrance and not instant:
		_entrance(quick)

func _exit_tree() -> void:
	_cancel_playback()
	_release_hint_task()
	_hide_hint()
	Engine.time_scale = 1.0

## Restored inventory and purchases share one profile balance.
func sync_inventory() -> void:
	profile.inventory = session.state.inventory.duplicate()
	profile_changed.emit()

func restart() -> void:
	session.restart()
	sync_inventory()
	start(level, profile, false)

## Modals cancel deferred input and unpaid hint requests, not the move already playing.
func suspend_input() -> void:
	_queued_taps.clear()
	_cancel_hint_request()
	_hide_hint()
	_build_tools()

func _playback_tween() -> Tween:
	var tw := create_tween()
	_playback_tweens.append(tw)
	tw.finished.connect(func(): _playback_tweens.erase(tw), CONNECT_ONE_SHOT)
	return tw

## Undo/restart invalidates suspended callbacks as well as the visible homecoming.
func _cancel_playback() -> void:
	_playback_id += 1
	_buffering_move = false
	_queued_taps.clear()
	_cancel_hint_request()
	_hide_bubble()
	for tw in _playback_tweens:
		tw.kill()
	_playback_tweens.clear()
	for bird in _homing_birds.values():
		if is_instance_valid(bird):
			bird.hide()
			bird.queue_free()
	_homing_birds.clear()
	if is_instance_valid(_home_effects):
		for child in _home_effects.get_children():
			child.queue_free()
	if is_instance_valid(_house):
		_house.position = _house_rect().position
		_house.scale = Vector2.ONE
		_glow.modulate.a = 0.0
		_glow.scale = Vector2.ONE
		if _door_open:
			_door_open.modulate.a = 0.0

func _cancel_hint_request() -> void:
	_hint_waiting = false
	_hint_request_key = PackedInt32Array()

## ---------- scene ----------

func _world() -> int:
	return session.state.world

func _build_scene() -> void:
	# One painting per world (docs/design/island-style.md): the branch-free scene, then each branch layer cut from the
	# same painting (tools/scene/cut_branches.py), so every branch keeps the painting's light and softness.
	var scene_name := "board_w%d_open" % _world() if _world() in [1, 10] else "board_w%d" % _world()
	# The painting is calmed behind play (calm_backdrop.gdshader) so the birds read first; the branches stay sharp.
	var calm := ShaderMaterial.new()
	calm.shader = CALM_BACKDROP
	UI.image(self, scene_name + "_bg", Rect2(0, 0, 1080, 1920), false).material = calm
	# The painting breathes behind play: falling leaves, petals or snow, mist, motes and light chosen by the
	# world and the level's weather variant (Ambient.for_level), below the branches and the birds.
	_air = Ambient.new()
	_air.look = Ambient.for_level(_world(), session.state.variant)
	_air.still = instant
	add_child(_air)
	_slots = JSON.parse_string(FileAccess.get_file_as_string("res://world/scene/board_w%d_slots.json" % _world()))["slots"]
	_build_ambient()
	_scene = Node2D.new()
	_scene.name = "Scene"
	add_child(_scene)
	_branch_layer = Node2D.new()
	_branch_layer.name = "Branches"
	_scene.add_child(_branch_layer)
	_bird_layer = Node2D.new()
	_bird_layer.name = "Birds"
	_scene.add_child(_bird_layer)

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
	_tick_idle(delta)

## A player who sits still for IDLE_NUDGE seconds gets the hand on the hint tool, so tools become a habit.
func _tick_idle(delta: float) -> void:
	if instant or _idle_nudged or busy or session == null or session.state.is_won():
		return
	_idle_time += delta
	if _idle_time < IDLE_NUDGE or is_instance_valid(_stuck):
		return
	if session.state.inventory[Board.Booster.FIREFLY] <= 0 or not get_tree().root.find_children("Popup", "", true, false).is_empty():
		_idle_time = IDLE_NUDGE * 0.5   # look again a little later
		return
	_idle_nudged = true
	point_at_tool(Board.Booster.FIREFLY)

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
	# Few branches stay close together (never further apart than the fullest board's spacing) and sit centred.
	_perch_step = minf((PERCH_BOTTOM - PERCH_TOP) / float(maxi(1, last_perch)), PERCH_STEP_MAX)
	_perch_top = PERCH_TOP + ((PERCH_BOTTOM - PERCH_TOP) - _perch_step * last_perch) * 0.5
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
			# The turn-around button sits on the branch tip, clear of every seat, as a round green button that breathes.
			var knot := Sprite2D.new()
			knot.name = "SwivelButton"
			knot.texture = Art.tex(SWIVEL_ART)
			var side := SWIVEL_SIDE * BIRD_SCALE
			knot.scale = Vector2.ONE * side / knot.texture.get_width()
			var reach := _branch_width(b, s.capacity) - SWIVEL_TIP_IN * BIRD_SCALE
			knot.position = Vector2(-reach if right else reach, SWIVEL_DROP * BIRD_SCALE)
			node.add_child(knot)
			var rest := knot.scale
			var pulse := knot.create_tween().set_loops()
			pulse.tween_property(knot, "scale", rest * 1.07, 0.9).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
			pulse.tween_property(knot, "scale", rest, 0.9).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
			var hit := side + 24.0
			UI.hit_area(node, Rect2(knot.position - Vector2.ONE * hit * 0.5, Vector2.ONE * hit), func(): _tap_swivel(b))
		_branch_layer.add_child(node)
		_branch_nodes.append(node)
		if b >= before and before > 0 and not instant:
			# A branch added by the sapling grows in from the edge.
			node.scale = Vector2(0.05, 0.6)
			node.create_tween().tween_property(node, "scale", Vector2.ONE, 0.6).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
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
		art.create_tween().tween_property(art, "scale:x", width, 0.6).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)

## Painted wood length of a slot from its screen edge to the end of its tuft.
func _painted_length(slot: Dictionary) -> float:
	var tip := float(slot["tip"])
	return tip if slot["side"] == "L" else 1080.0 - tip

## Seat pitch of a branch: four seats spread over its painted length, clear of the tuft.
func _seat_step(b: int) -> float:
	var s := session.state
	var slot := _slot(s.rows[b], s.rights[b] == 1)
	var free := _painted_length(slot) - SEAT_FIRST * BIRD_SCALE - END_CLEAR
	return clampf(free / 3.0, 94.0 * BIRD_SCALE, 150.0 * BIRD_SCALE)   # never pitch past the painted tip

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
	return _perch_top + (s.rows[b] * 2 + s.rights[b]) * _perch_step

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
	bird.sounds = true
	var species: String = Board.SPECIES[Board.species_of(code)]
	# A world with its own birds shows them (WorldBirds); only the original birds wear outfits.
	var look := WorldBirds.look(species, _world())
	if look.is_empty() and profile.equipped.has(species):
		look = profile.equipped[species].replace("outfit.", "").replace(".", "_outfit_")
	bird.setup(code, session.state.is_asleep(code), BIRD_SCALE, look)
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
					glide = _bird_layer.create_tween().set_parallel()
				glide.tween_property(bird, "position", seat_point(b, i), ROW_REFLOW_SECONDS).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
			bird.set_selected(false)
	for id in _birds.keys():
		if not seen.has(id):
			if is_instance_valid(_birds[id]):
				_birds[id].queue_free()
			_birds.erase(id)
	if glide:
		await glide.finished

## Houses drop in, branches grow out of the trunks, then the birds fly to their seats. A tap speeds it up.
## `quick` (a replay, or the next level right after a win) plays the same beats at ENTRANCE_QUICK of the time.
func _entrance(quick := false) -> void:
	var playback := _playback_id
	busy = true
	_entrance_running = true
	var started := Time.get_ticks_msec()
	var k := ENTRANCE_QUICK if quick else 1.0
	modulate.a = 0.0
	_playback_tween().tween_property(self, "modulate:a", 1.0, 0.4 * k)
	var rise := _playback_tween().set_parallel()
	var drop := [_rope, _house]
	for node in drop:
		var rest_y: float = node.position.y
		node.position.y -= 420.0
		rise.tween_property(node, "position:y", rest_y, 0.7 * k).set_delay(ENTRANCE_HOUSE * k).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	var grow := _playback_tween().set_parallel()
	for i in _branch_nodes.size():
		var node: Node2D = _branch_nodes[i]
		node.scale = Vector2(0.02, 0.6)
		grow.tween_property(node, "scale", Vector2.ONE, 0.5 * k).set_delay((ENTRANCE_BRANCHES + 0.1 * i) * k).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
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
		# A whole flock arrives at once: only every third bird is heard, softly.
		bird.fly_to(target, (ENTRANCE_BIRDS + ENTRANCE_BIRD_SPREAD * j / maxf(1.0, n - 1.0)) * k, ENTRANCE_FLIGHT * k, false, true, null,
			ENTRANCE_WING_DB if j % 3 == 0 else BirdView.WING_SILENT)
		last = bird
	if last:
		await last.finished
	await get_tree().create_timer(0.1 * k).timeout
	if playback != _playback_id:
		return
	var skipped := Engine.time_scale > 1.0
	Engine.time_scale = 1.0
	_entrance_running = false
	busy = false
	if OS.is_debug_build():
		print("ENTRANCE level=%d kind=%s skipped=%s ms=%d" % [int(level["id"]), "quick" if quick else "full", skipped,
			Time.get_ticks_msec() - started])
	_watch_stuck()

## ---------- HUD ----------

## Exposed, aligned HUD: stars, footprint and remaining star-budget share one row.
## The budget hides on the last star.
const STATUS_AT := Vector2(28, 48)
const STATUS_H := 92.0
const STATUS_HOUSE_CLEAR := 48.0  # the capsule always ends at least this far left of the house
const STATUS_GAP := 16.0         # between the two cards
const STATUS_INSET := 30.0       # inside the capsule's round ends
const STATUS_CAP := 44.0
const STATUS_STAR := 46.0
## Painted HUD pieces (Codex): the wooden plank (goal shelf and status plates), the tool slot and the gear button.
const CAPSULE := "hud_status_cream"   # the cream status capsule, the bird keys and cards family
const CAPSULE_END := 80          # capsule source pixels kept at each end (round end); the middle extends
const CAPSULE_BODY := Vector2(0.0, 1.0)   # the capsule body fills the whole height
const STATUS_STAR_GAP := 2.0
const STATUS_SHADOW := Color(0.18, 0.12, 0.05, 0.38)   # warm dark, under the cream status capsule
const STATUS_SHADOW_OFFSET := Vector2(0, 6)
const STATUS_ICON := 46.0
## Baked label metrics (tools/kit/locales/en.json): cap height, baseline and line height in texture pixels.
const LABEL_CAP := 128.0
const LABEL_BASELINE := 160.0
const LABEL_LINE := 208.0
const NUMBER_BASELINE := 126.0 / 128.0
const GEAR := Vector2(992, 100)
const GEAR_ICON := 112.0         # the painted round green gear button
## Tool buttons stand in one centred row at the bottom (no tray), 48 px from the screen edge like the status pill at the top.
const TOOL_Y := 1736.0
const TOOL_GAP := 24.0
const TOOL_SIZE := 136.0
const TOOL_ICON_SIZE := 100.0
const TOOL_COUNT_H := 46.0       # painted stock numerals under each tool slot (no badge)
const TOOL_COUNT_GAP := 6.0
const TOOL_ROW_WIDTH := 1000.0
## Hanging house: sprite, visible content and doorway in sprite pixels, and where the rope meets it.
const HOUSES := {
	"default": ["house_default", Rect2(122, 62, 778, 878), Rect2(313, 418, 378, 380), 70.0],
	"acorn": ["house_style_acorn_body", Rect2(56, 51, 397, 403), Rect2(148, 205, 204, 199), 51.0],
	"teapot": ["house_style_teapot_body", Rect2(29, 55, 458, 398), Rect2(148, 205, 204, 199), 55.0],
	"mushroom": ["house_style_mushroom_body", Rect2(41, 51, 426, 401), Rect2(148, 204, 204, 200), 51.0],
	"lantern": ["house_style_lantern_body", Rect2(50, 61, 408, 393), Rect2(148, 204, 204, 200), 61.0],
	"pumpkin": ["house_style_pumpkin_body", Rect2(49, 37, 413, 423), Rect2(148, 205, 204, 199), 37.0],
	"beehive": ["house_style_beehive_body", Rect2(54, 53, 401, 403), Rect2(148, 205, 204, 199), 53.0],
	"teacup": ["house_style_teacup_body", Rect2(83, 136, 389, 324), Rect2(165, 221, 170, 169), 136.0],
	"pinecone": ["house_style_pinecone_body", Rect2(59, 14, 390, 441), Rect2(148, 205, 204, 199), 14.0],
	"cage": ["house_style_cage_body", Rect2(58, 28, 390, 430), Rect2(150, 205, 204, 200), 28.0]}
const HOUSE_WIDTH := 220.0
const HOUSE_TOP := 40.0
const ROPE_WIDTH := 16.0
const GOAL := 100.0
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
	if profile.equipped_house == "cage":
		# The birdcage never hangs still: it sways on its rope about the ring on top, easing at both ends.
		_house.pivot_offset = Vector2(hr.size.x * 0.5, 0.0)
		var sway := _house.create_tween().set_loops()
		sway.tween_property(_house, "rotation", deg_to_rad(2.6), 1.5).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
		sway.tween_property(_house, "rotation", deg_to_rad(-2.6), 1.5).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
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
	_build_status()
	var gear := UI.art(_hud, "hud_gear_button", Rect2(GEAR - Vector2.ONE * GEAR_ICON * 0.5, Vector2.ONE * GEAR_ICON))
	gear.name = "Gear"
	var gear_hit := UI.hit_area(_hud, Rect2(GEAR - Vector2.ONE * 64.0, Vector2.ONE * 128.0), func():
		Sound.play("ui_tap")
		settings_requested.emit())
	gear_hit.name = "Settings"
	gear_hit.button_down.connect(func(): gear.self_modulate = UI.PRESSED_TINT)
	gear_hit.button_up.connect(func(): gear.self_modulate = Color.WHITE)
	_tool_bar = Control.new()
	_tool_bar.name = "Tools"
	_tool_bar.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_hud.add_child(_tool_bar)
	_home_effects = Node2D.new()
	_home_effects.name = "HomeEffects"
	_hud.add_child(_home_effects)

func _status_rect() -> Rect2:
	return Rect2(STATUS_AT, Vector2(_status_width(), STATUS_H))

## The capsule hugs its content: three stars, the footprint and room for a two-digit count.
func _status_width() -> float:
	return STATUS_INSET * 2.0 + 3.0 * STATUS_STAR + 2.0 * STATUS_STAR_GAP + 14.0 + STATUS_ICON + 6.0 \
		+ ButtonKit.number_width("88", STATUS_CAP)

## The vertical centre of the capsule body (below the leaf sprig) in a plate of height h.
static func _capsule_mid(h: float) -> float:
	return h * (CAPSULE_BODY.x + CAPSULE_BODY.y) * 0.5

## A painted label whose baseline sits at `baseline`, at exactly `cap` cap height.
func _place_label(parent: Node, id: String, x: float, baseline: float, cap: float) -> TextureRect:
	var k := cap / LABEL_CAP
	return UI.label(parent, id, Rect2(x, baseline - LABEL_BASELINE * k, UI.label_width(id, cap) + 1.0, LABEL_LINE * k + 1.0), cap)

## The single place that picks the HUD star art (painted stars; an unreachable star is the empty one).
static func _star_texture(lit: bool) -> Texture2D:
	return Art.tex("island_star_full" if lit else "island_star_empty")

## Stars as one row of `count` sprites in `bounds` (left aligned); see _set_stars.
func _make_stars(parent: Node, at: Vector2, size: float, gap: float, count := 3) -> Array:
	var out := []
	for i in count:
		var star := TextureRect.new()
		star.name = "Star %d" % i
		star.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
		star.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
		star.mouse_filter = Control.MOUSE_FILTER_IGNORE
		UI.place(star, Rect2(at + Vector2(i * (size + gap), 0), Vector2.ONE * size))
		parent.add_child(star)
		out.append(star)
	return out

## The first `lit` stars are full; the others are empty (no longer reachable).
func _set_stars(stars: Array, lit: int) -> void:
	for i in stars.size():
		stars[i].texture = _star_texture(i < lit)

func _build_status() -> void:
	var face := _capsule_mid(STATUS_H)   # content centres on the capsule body, not on the leaf above it
	var width := _status_width()
	_status = Control.new()
	_status.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(_status, Rect2(STATUS_AT, Vector2(width, STATUS_H)))
	_hud.add_child(_status)
	_status.name = "Status"
	_hud_pill(_status, Rect2(Vector2.ZERO, _status.size))
	_status_stars = _make_stars(_status, Vector2(STATUS_INSET, face - STATUS_STAR * 0.5), STATUS_STAR, STATUS_STAR_GAP)
	var moves_x := STATUS_INSET + 3.0 * STATUS_STAR + 2.0 * STATUS_STAR_GAP + 14.0
	_moves_icon = UI.art(_status, "icon_star_budget_steps", Rect2(moves_x, face - STATUS_ICON * 0.5, STATUS_ICON, STATUS_ICON), Rect2(317, 217, 652, 801))
	_stars_centred = false
	_shown_stars = -1
	var number_x := moves_x + STATUS_ICON + 6.0
	_moves_number = _digits(_status, Rect2(number_x, face - STATUS_CAP * 0.5, width - STATUS_INSET - number_x, STATUS_CAP))

func _refresh_status() -> void:
	var moves := session.move_count()
	var stars := GameSession.stars_for(moves, session.par)
	var keep := GameSession.moves_left_for_star(moves, session.par)
	_set_digits(_moves_number, str(maxi(keep, 0)))
	_moves_number.visible = keep >= 0
	_moves_icon.visible = keep >= 0
	_set_stars(_status_stars, stars)
	_centre_status_stars(keep < 0, _shown_moves >= 0)
	if _shown_moves >= 0 and moves != _shown_moves:
		_pulse(_moves_number)
		if stars < _shown_stars:
			_pulse(_status_stars[stars], _status_stars[stars].size.x * 0.5)
	_shown_moves = moves
	_shown_stars = stars

## The wooden plank of the goal shelf behind the parent's content (one wood family with the shelf and the signs);
## its leafy ends keep their shape, only the plain middle extends.
func _hud_pill(parent: Control, rect: Rect2) -> NinePatchRect:
	var tex := Art.tex(CAPSULE)
	var k := rect.size.y / tex.get_height()
	var plate := NinePatchRect.new()
	plate.name = "Plate"
	plate.mouse_filter = Control.MOUSE_FILTER_IGNORE
	plate.texture = tex
	var end := mini(CAPSULE_END, int(rect.size.x / k * 0.5) - 1)
	plate.patch_margin_left = end
	plate.patch_margin_right = end
	plate.position = rect.position
	plate.size = rect.size / k
	plate.scale = Vector2.ONE * k
	parent.add_child(plate)
	parent.move_child(plate, 0)
	# A soft shadow under the cream capsule keeps its edge clear against a pale sky.
	var shadow := plate.duplicate() as NinePatchRect
	shadow.name = "Shadow"
	shadow.position += STATUS_SHADOW_OFFSET
	shadow.modulate = STATUS_SHADOW
	parent.add_child(shadow)
	parent.move_child(shadow, 0)
	return plate

## A holder for the shop's painted numerals (white, dark-green outline), left aligned; _set_digits fills it.
func _digits(parent: Control, rect: Rect2) -> Control:
	var holder := Control.new()
	holder.name = "Number"
	holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(holder, rect)
	parent.add_child(holder)
	return holder

func _set_digits(holder: Control, text: String) -> void:
	if holder.get_meta("value", "") == text:
		return
	holder.set_meta("value", text)
	for child in holder.get_children():
		holder.remove_child(child)
		child.queue_free()
	ButtonKit.number(holder, text, Rect2(Vector2.ZERO, holder.size), -1)

## With the counter gone the stars glide to the middle of the pill; undo brings them back.
func _centre_status_stars(centred: bool, animate: bool) -> void:
	if centred == _stars_centred:
		return
	_stars_centred = centred
	if _stars_tween:
		_stars_tween.kill()
	var x := STATUS_INSET  # Keep the exposed left edge stable when the budget hides.
	if instant or not animate:
		for i in _status_stars.size():
			_status_stars[i].position.x = x + i * (STATUS_STAR + STATUS_STAR_GAP)
		return
	_stars_tween = _status.create_tween().set_parallel()
	for i in _status_stars.size():
		_stars_tween.tween_property(_status_stars[i], "position:x", x + i * (STATUS_STAR + STATUS_STAR_GAP), 0.3) \
			.set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	if not centred:   # the counter returns only once the stars have left its place
		for node: Control in [_moves_icon, _moves_number]:
			node.modulate.a = 0.0
			_stars_tween.tween_property(node, "modulate:a", 1.0, 0.12).set_delay(0.22)

## A changed value grows briefly so the player sees what moved.
func _pulse(node: Control, pivot_x := 0.0) -> void:
	if instant:
		return
	node.pivot_offset = Vector2(pivot_x, node.size.y * 0.5)
	var tw := node.create_tween()
	tw.tween_property(node, "scale", Vector2.ONE * 1.25, 0.1).set_ease(Tween.EASE_OUT)
	tw.tween_property(node, "scale", Vector2.ONE, 0.25).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)

## ---------- bird keys under the house ----------

## One key per kind of bird in the level, in a row under the hanging house: the bird's face, its homecoming
## badge at the corner, and a check once its flock is home. Tapping a key opens that bird's card.
const KEY := 88.0
const KEY_SMALL := 74.0      # nine or more kinds
const KEY_GAP := 14.0
const KEY_DROP := 14.0       # under the house
var _skill_row: Control
var _skill_keys := {}        # species name -> key

func _build_skill_row() -> void:
	_skill_keys.clear()
	var fresh := Levels.create_board(level)
	var names: Array = Array(Levels.species_ids(fresh, true)) if fresh != null else []
	names.sort_custom(func(a, b): return Board.SPECIES.find(a) < Board.SPECIES.find(b))
	var side := KEY if names.size() <= 8 else KEY_SMALL
	var width := names.size() * side + maxi(0, names.size() - 1) * KEY_GAP
	_skill_row = Control.new()
	_skill_row.name = "SkillRow"
	_skill_row.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(_skill_row, Rect2(540.0 - width * 0.5, _house_rect().end.y + KEY_DROP, width, side))
	_hud.add_child(_skill_row)
	for i in names.size():
		var species: String = names[i]
		var r := Rect2(i * (side + KEY_GAP), 0, side, side)
		var key := Control.new()
		key.name = "Key_" + species
		key.mouse_filter = Control.MOUSE_FILTER_IGNORE
		UI.place(key, r)
		_skill_row.add_child(key)
		UI.art(key, "hud_tool_slot", Rect2(Vector2.ZERO, r.size))
		var inner := side * 0.78
		UI.art(key, WorldBirds.face(species, _world()), Rect2((side - inner) * 0.5, (side - inner) * 0.5 - 2.0, inner, inner)).name = "Face"
		if GIFT_COIN.has(species):
			var b := side * 0.44
			UI.art(key, "gift_coin_" + GIFT_COIN[species], Rect2(side - b * 0.8, side - b * 0.8, b, b)).name = "Badge"
		var done := UI.art(key, UI.PICTURES["check"], Rect2(-side * 0.12, -side * 0.12, side * 0.42, side * 0.42))
		done.name = "Home"
		done.visible = false
		var tap := UI.hit_area(key, Rect2(Vector2.ZERO, r.size), func():
			if busy:
				return
			if session.selected >= 0:
				_lift(session.selected, false)
				session.cancel_selection()
				_refresh_hint_selection()
			suspend_input()
			Sound.play("select")
			bird_info_requested.emit(species))
		tap.name = "Tap"
		_skill_keys[species] = key

func _refresh_skill_row() -> void:
	for species in _skill_keys:
		var key: Control = _skill_keys[species]
		var home: bool = session.state.homed.has(Board.SPECIES.find(species))
		var mark: Control = key.get_node("Home")
		if home and not mark.visible:
			_pulse(key, key.size.x * 0.5)
		mark.visible = home

func refresh_hud() -> void:
	var s := session.state
	_refresh_status()
	_refresh_skill_row()
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
	for kind in [Board.Booster.OWL_EYE, Board.Booster.MORNING_BELL, Board.Booster.SCARECROW, Board.Booster.SPRING_RAIN]:
		if Progress.is_booster_unlocked(profile, kind) and session.booster_has_effect(kind):
			tools.append(kind)
	return tools

const TOOL_ICON_NAME := {Board.Booster.FIREFLY: "bulb", Board.Booster.SAPLING: "sapling", Board.Booster.OWL_EYE: "eye",
	Board.Booster.MORNING_BELL: "bell", Board.Booster.SCARECROW: "scarecrow", Board.Booster.SPRING_RAIN: "rain"}

## Tools as a centred row of painted tool slots (hud_tool_slot); the stock stands under each slot in painted numerals
## (white, dark-green outline; no badge), "+" when empty (pressing buys more). The whole slot is clickable.
func _tool_count(button: Button, value: String) -> void:
	var count := Control.new()
	count.name = "Stock"
	count.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(count, Rect2(0, TOOL_SIZE + TOOL_COUNT_GAP, TOOL_SIZE, TOOL_COUNT_H))
	button.add_child(count)
	ButtonKit.number(count, value, Rect2(Vector2.ZERO, count.size))

func _restyle_tool(button: Button, icon_name: String) -> void:
	var old_face := button.get_node_or_null("Face")
	if old_face:
		old_face.hide()
	var frame := UI.image(button, "hud_tool_slot", Rect2(0, 0, TOOL_SIZE, TOOL_SIZE), false)
	frame.name = "Slot"
	button.move_child(frame, 0)
	var old_icon := button.get_node_or_null("Icon")
	if old_icon:
		old_icon.name = "UnusedIcon"
		old_icon.hide()
	# Undo is painted like the boosters (island_icon_undo), so all four slots share one style.
	var picture: String = "island_icon_undo" if icon_name == "undo" else UI.PICTURES[icon_name]
	var content: Rect2 = {
		"undo": UI.used_rect("island_icon_undo"), "bulb": Rect2(19, 11, 304, 371),
		"sapling": Rect2(16, 17, 333, 289), "eye": Rect2(17, 12, 366, 359),
		"bell": Rect2(16, 17, 368, 335), "scarecrow": Rect2(16, 16, 387, 395),
		"rain": Rect2(16, 17, 319, 386)
	}[icon_name]
	var bounds := Rect2(Vector2.ONE * (TOOL_SIZE - TOOL_ICON_SIZE) * 0.5, Vector2.ONE * TOOL_ICON_SIZE)
	var icon := UI.art(button, picture, bounds, Rect2(content))
	icon.name = "Icon"
	button.button_down.connect(func(): frame.modulate = ButtonKit.PRESSED; icon.modulate = ButtonKit.PRESSED)
	button.button_up.connect(func(): frame.modulate = Color.WHITE; icon.modulate = Color.WHITE)

func _build_tools() -> void:
	_hint_button = null
	for child in _tool_bar.get_children():
		child.queue_free()
	var tools := _visible_tools()
	_tool_slots.clear()
	var n := tools.size()
	var gap := minf(TOOL_GAP, (TOOL_ROW_WIDTH - n * TOOL_SIZE) / maxf(1.0, n - 1))
	var row_w := n * TOOL_SIZE + (n - 1) * gap
	# The tool buttons stand on their own (user: no tray around them).
	var x := 540.0 - row_w * 0.5 + TOOL_SIZE * 0.5
	for kind in tools:
		var centre := Vector2(x, TOOL_Y)
		x += TOOL_SIZE + gap
		_tool_slots.append([kind, centre, kind < 0 or session.state.inventory[kind] == 0 or session.booster_has_effect(kind)])
		if kind < 0:
			var undo := UI.button(_tool_bar, "Undo", centre, "cream", "undo", _on_undo, TOOL_SIZE)
			_restyle_tool(undo, "undo")
			# Undos left in this attempt, "+" when none is left (pressing offers more, like an empty booster).
			_tool_count(undo, str(session.undos_left) if session.undos_left > 0 else "+")
			continue
		var b := UI.button(_tool_bar, "Tool %d" % kind, centre, "orange", TOOL_ICON_NAME[kind], func(): _on_tool(kind), TOOL_SIZE)
		_restyle_tool(b, TOOL_ICON_NAME[kind])
		if kind == Board.Booster.FIREFLY:
			_hint_button = b
		var stock: int = session.state.inventory[kind]
		_tool_count(b, str(stock) if stock > 0 else "+")
		UI.set_active(b, stock == 0 or session.booster_has_effect(kind))
	if _hint_waiting:
		_show_hint_loading()

## ---------- input ----------

func _gui_input(event: InputEvent) -> void:
	var pressed: bool = event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT
	if pressed and _entrance_running:
		Engine.time_scale = SKIP_TIME_SCALE
		accept_event()
		return
	if pressed:
		_idle_time = 0.0
		_hide_bubble()
		_hide_stuck()
	if session.state.is_won():
		return
	if pressed:
		var b := branch_at(event.position)
		if b >= 0:
			tap_branch(b)
			accept_event()
		_begin_hold(event.position)
	elif (event is InputEventMouseButton and not event.pressed) \
			or (event is InputEventMouseMotion and event.position.distance_to(_hold_at) > HOLD_SLOP):
		_hold_id += 1

## Holding a finger on a bird for HOLD_SECONDS opens its card. The press itself still selects as usual; the hold
## then drops that selection. A move started by the press, an egg or a moved finger cancels it.
const HOLD_SECONDS := 0.45
const HOLD_SLOP := 24.0
var _hold_id := 0
var _hold_at := Vector2.ZERO

func bird_at(p: Vector2) -> BirdView:
	var best: BirdView = null
	var best_d := 80.0 * BIRD_SCALE
	for bird in _birds.values():
		if not is_instance_valid(bird):
			continue
		var at: Vector2 = bird.global_position - global_position
		var dx := absf(p.x - at.x)
		if dx < best_d and p.y < at.y + 10.0 and p.y > at.y - 175.0 * BIRD_SCALE:
			best = bird
			best_d = dx
	return best

func _begin_hold(p: Vector2) -> void:
	_hold_id += 1
	_hold_at = p
	var id := _hold_id
	var bird := bird_at(p)
	if bird == null or (bird.code & Board.EGG) != 0:
		return
	await get_tree().create_timer(HOLD_SECONDS).timeout
	if id != _hold_id or busy or not is_inside_tree() or not is_instance_valid(bird):
		return
	_hold_id += 1
	if session.selected >= 0:
		_lift(session.selected, false)
		session.cancel_selection()
		_refresh_hint_selection()
	suspend_input()
	Sound.play("select")
	bird_info_requested.emit(Board.SPECIES[Board.species_of(bird.code)])

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
	if session.state.is_won():
		return
	if busy:
		_queue_tap(b, false)
		return
	var s := session.state
	if session.selected < 0:
		if Rules.can_swivel(s, b) and s.run_count(b) == 0:
			_do(func(): return session.swivel(b))
			return
		if session.select(b):
			Sound.play("select")
			Sound.vibrate(Sound.PULSE_PICK)
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
			Sound.vibrate(Sound.PULSE_PICK)
			_lift(b, true)
		else:
			_refuse(b)
		_refresh_hint_selection()

func _tap_swivel(b: int) -> void:
	if session.state.is_won():
		return
	if busy:
		_queue_tap(b, true)
		return
	_hide_bubble()
	if session.selected >= 0:
		_lift(session.selected, false)
	_do(func(): return session.swivel(b))

func _queue_tap(branch: int, swivel: bool) -> void:
	if _buffering_move and _queued_taps.size() < 2:
		_queued_taps.append([branch, swivel])

func _drain_taps() -> void:
	while not busy and not _queued_taps.is_empty():
		var tap: Array = _queued_taps.pop_front()
		if tap[1]:
			_tap_swivel(tap[0])
		else:
			tap_branch(tap[0])

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
	var tw := node.create_tween()
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

## Sleep cue: a matching bird landing or a nearby flock's song wakes the sleeper.
func _show_bubble(bird: BirdView) -> void:
	_hide_bubble()
	var icons: Array[String] = []
	if (bird.code & Board.ASLEEP) != 0:
		icons.append(WorldBirds.face(bird.species, _world()))
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

## With no legal move left, or only moves that can never win (the background search exhausted every position
## reachable without boosters), a gold glow and a bobbing pointer mark the cheapest tool that rescues the position.
func _watch_stuck() -> void:
	_hide_stuck()
	var s := session.state
	if instant or s.is_won() or not session.can_undo() or not _dead_end(s):
		return
	var moves := session.move_count()
	await get_tree().create_timer(STUCK_DELAY).timeout
	if is_inside_tree() and not busy and session.state == s and session.move_count() == moves:
		if session.free_rescue_used or _rescue_declined:
			show_stuck_hint()
		else:
			_rescue_declined = true   # closing the offer leaves the pointer for the rest of the attempt
			suspend_input()
			show_stuck_hint(false)   # the card is the offer; later dead ends search for the rescuing tool
			rescue_requested.emit()

var _rescue_declined := false

## The free rescue: back to the latest position of this attempt that can still be won. Positions on a route the
## background search already found are known; the others are searched on a worker thread, newest first. A position
## over the search budget is skipped; the level's opening position always qualifies.
const RESCUE_STATES := 60000
const RESCUE_MS := 1500

func rescue() -> void:
	if busy or session.history_size() == 0:
		return
	busy = true
	_hide_stuck()
	_cancel_hint_request()
	var snapshots: Array[Board] = []
	for back in range(1, session.history_size() + 1):
		snapshots.append(session.history_state(back).clone(false))
	var known := _winnable.duplicate()
	var thread := Thread.new()
	thread.start(func() -> int:
		for i in snapshots.size() - 1:
			if known.has(snapshots[i].search_key(false)):
				return i + 1
			if Solver.hint(snapshots[i], [], RESCUE_STATES, RESCUE_MS)["status"] == Solver.Status.SOLVED:
				return i + 1
		return snapshots.size())
	while thread.is_alive():
		await get_tree().process_frame
	var steps: int = thread.wait_to_finish()
	if not is_inside_tree():
		return
	session.rewind(steps)
	session.free_rescue_used = true
	# Proven by the search (or the level's opening position): a later undo back here rescues the attempt.
	_winnable[session.state.search_key(false)] = true
	if OS.is_debug_build():
		print("RESCUE level=%d steps=%d of=%d" % [session.state.level_id, steps, snapshots.size()])
	await _after_rewind()

func _dead_end(s: Board) -> bool:
	return not Rules.has_legal_move(s) or _hint_failures.get(s.search_key(false), -1) == Solver.Status.UNSOLVABLE

## The rescuing tool: every visible tool (undo and unlocked boosters, the Firefly excepted) is tried cheapest first on a
## worker thread. A booster rescues when the board with its effect applied is proven winnable; undo rescues when an
## earlier position of the attempt is known winnable or the position one step back is proven winnable. The pointer
## goes to the first rescue; tapping that slot uses it, or offers one more when out of stock (the slot's own action).
## With no rescue the pointer stays on undo. `search` false points at undo at once (behind the free-rescue card).
const RESCUE_TOOL_STATES := 20000
const RESCUE_TOOL_MS := 600
const NO_RESCUE := -2
var rescue_tool := NO_RESCUE      # last search result: -1 undo, a booster kind, or NO_RESCUE
var rescue_tool_main_ms := 0.0    # longest main-thread slice of the last search (evidence: never a frame)
var _rescue_tool_search := 0      # bumped by _hide_stuck: a search the player has moved past is dropped
var _rescue_tool_cancel: Array = [false]

func show_stuck_hint(search := true) -> void:
	_hide_stuck()
	var pick := -1
	for i in _tool_slots.size():
		if _tool_slots[i][2] and (pick < 0 or _tool_slots[i][0] < 0):
			pick = i
	if not search:
		_point_stuck(pick)
		return
	var id := _rescue_tool_search
	var found: int = await _find_rescue_tool()
	if id != _rescue_tool_search or not is_inside_tree():
		return
	rescue_tool = found
	for i in _tool_slots.size():
		if found != NO_RESCUE and _tool_slots[i][0] == found:
			pick = i
	_point_stuck(pick)

func _rescue_tool_candidates() -> Array:
	var tools := []
	for slot in _tool_slots:
		var kind: int = slot[0]
		if kind == Board.Booster.FIREFLY:
			continue
		var price: int = (0 if session.undos_left > 0 else Progress.UNDO_PRICE) if kind < 0 else Progress.booster_price(kind)
		tools.append([price, tools.size(), kind])
	tools.sort()   # price, then slot order
	return tools.map(func(t: Array) -> int: return t[2])

func _find_rescue_tool() -> int:
	var t0 := Time.get_ticks_usec()
	var kinds := _rescue_tool_candidates()
	var board := session.state.clone(false)
	var earlier: Array[PackedInt32Array] = []
	for back in range(1, session.history_size() + 1):
		earlier.append(session.history_state(back).search_key(false))
	var previous: Board = session.history_state(1).clone(false) if session.history_size() > 0 else null
	var known := _winnable.duplicate()
	var cancel: Array = [false]
	_rescue_tool_cancel = cancel
	var result := {}
	var thread := Thread.new()
	thread.start(func():
		var start := Time.get_ticks_usec()
		result["kind"] = NO_RESCUE
		for kind in kinds:
			if cancel[0]:
				break
			var ok := false
			if kind < 0:
				ok = previous != null and (earlier.any(func(key): return known.has(key)) \
					or Solver.hint(previous, [], RESCUE_TOOL_STATES, RESCUE_TOOL_MS, cancel)["status"] == Solver.Status.SOLVED)
			else:
				ok = Solver.rescue_booster(board, [kind], RESCUE_TOOL_STATES, RESCUE_TOOL_MS, cancel) == kind
			if ok and not cancel[0]:
				result["kind"] = kind
				break
		result["ms"] = (Time.get_ticks_usec() - start) / 1000.0)
	var main_ms := (Time.get_ticks_usec() - t0) / 1000.0
	while thread.is_alive():
		if not is_inside_tree():
			cancel[0] = true
			break
		await get_tree().process_frame
	var t1 := Time.get_ticks_usec()
	thread.wait_to_finish()
	rescue_tool_main_ms = maxf(main_ms, (Time.get_ticks_usec() - t1) / 1000.0)
	if cancel[0]:
		return NO_RESCUE
	var found: int = result["kind"]
	if OS.is_debug_build():
		var tool_name: String = "none" if found == NO_RESCUE else ("undo" if found < 0 else Progress.BOOSTERS[found][1])
		print("RESCUE_TOOL level=%d tool=%s ms=%.1f main_ms=%.2f tried=%d" % [session.state.level_id, tool_name,
			result["ms"], rescue_tool_main_ms, kinds.size()])
	return found

## Gold glow and a bobbing pointer over the tool slot `pick` (index into _tool_slots).
func _point_stuck(pick: int) -> void:
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
	var holder := _stuck   # _hide_stuck clears _stuck before the queued node (and its tween) is gone
	var tw := holder.create_tween().set_loops()
	tw.tween_method(func(t: float):
		var wave := (1.0 - cos(t * TAU)) * 0.5
		holder.modulate.a = 0.7 + 0.3 * wave
		arrow.position.y = base_y - wave * 12.0, 0.0, 1.0, 1.2)

## A glow and a tapping hand on one tool (a fitting booster at level start, or the hint after a long pause),
## until the player acts.
func point_at_tool(kind: int) -> void:
	_hide_stuck()
	var pick := -1
	for i in _tool_slots.size():
		if _tool_slots[i][0] == kind:
			pick = i
	if pick < 0:
		return
	var centre: Vector2 = _tool_slots[pick][1]
	_stuck = Control.new()
	_stuck.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_hud.add_child(_stuck)
	var glow_size := Vector2.ONE * UI.SECONDARY * 1.5
	var glow := UI.image(_stuck, "glow_soft", Rect2(centre - glow_size * 0.5, glow_size))
	glow.modulate = Color(1, 0.8, 0.3, 0.45)
	var hand := TextureRect.new()
	hand.texture = Art.tex("hand_pointer")
	hand.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	hand.mouse_filter = Control.MOUSE_FILTER_IGNORE
	hand.size = Vector2(110, 110)
	var rest := centre + Vector2(6, -6)
	hand.position = rest
	_stuck.add_child(hand)
	if instant:
		return
	var tw := _stuck.create_tween().set_loops()
	tw.tween_method(func(t: float):
		var wave := (1.0 - cos(t * TAU)) * 0.5
		glow.modulate.a = 0.25 + 0.35 * wave
		glow.scale = Vector2.ONE * (0.9 + 0.2 * wave)
		hand.position = rest + Vector2(14, 14) * wave, 0.0, 1.0, 1.0)

func _hide_stuck() -> void:
	_rescue_tool_search += 1
	_rescue_tool_cancel[0] = true
	if _stuck:
		_stuck.queue_free()
		_stuck = null

func _do(action: Callable) -> void:
	_cancel_hint_request()
	_hide_hint()
	var playback := _playback_id
	busy = true
	_buffering_move = true
	var ok: bool = action.call()
	if ok:
		_prefetch_hint()   # searches while the move plays and the player thinks
		_refresh_status()   # counts change with the move, not after its animation
		await play_events(session.events.duplicate())
	if playback != _playback_id:
		return
	await _finish_action(playback)

## Moves and boosters both settle through the same completion path.
func _finish_action(playback: int) -> void:
	await sync_birds()
	if playback != _playback_id:
		return
	refresh_hud()
	busy = false
	_buffering_move = false
	if session.state.is_won():
		_queued_taps.clear()
		_celebrate()
	else:
		_watch_stuck()
		_drain_taps()

func _on_undo() -> void:
	if busy:
		return
	if session.can_undo() and session.undos_left <= 0:
		suspend_input()
		undo_more_requested.emit()
		return
	if not session.undo():
		return
	await _after_rewind()

## The board follows the session back to an earlier position (undo, rescue).
func _after_rewind() -> void:
	_cancel_playback()
	sync_inventory()
	_hide_hint()
	_prefetch_hint()
	Sound.play("undo")
	busy = true
	_layout_branches()
	await _finish_action(_playback_id)

func _on_tool(kind: int) -> void:
	if busy:
		return
	if kind == Board.Booster.FIREFLY:
		_hint()
		return
	if session.state.inventory[kind] == 0:
		suspend_input()
		buy_requested.emit(kind)
		return
	_cancel_hint_request()
	_queued_taps.clear()
	_hide_hint()
	var playback := _playback_id
	busy = true
	var result := session.use_booster(kind)
	if result.get("applied", false):
		profile.set_tutorial(Progress.used_flag(kind))
		sync_inventory()
		_prefetch_hint()
		Sound.play(BOOSTER_SOUND[kind])
		await play_events(session.events.duplicate())
	else:
		Sound.play("error")
	if playback != _playback_id:
		return
	_layout_branches()
	await _finish_action(playback)

## Hints are searched in the background (worker thread) as soon as a level starts and after every move,
## undo or booster, and cached by search key, so a press shows the hand at once. The background search has
## a large budget; when it still finds no full route it caches a one-move suggestion (Solver.best_move).
const HINT_STATES := 400000
const HINT_MS := 8000

func _hint() -> void:
	var key := session.state.search_key(false)
	if _shown_hint_key == key and _hint_moves.has(key):
		show_hint(_hint_moves[key])
		return
	if session.state.inventory[Board.Booster.FIREFLY] == 0:
		suspend_input()
		buy_requested.emit(Board.Booster.FIREFLY)
		return
	_hint_press_usec = Time.get_ticks_usec()
	if _hint_moves.has(key):
		_log_hint("cached")
		_apply_hint(_hint_moves[key])
		return
	if _hint_failures.has(key) and _hint_failures[key] == Solver.Status.BUDGET_EXCEEDED:
		_hint_failures.erase(key)   # the retry icon promises another search
	if _hint_failures.has(key):
		_show_hint_failure(_hint_failures[key])
		Sound.play("error")
		return
	# Only Hint waits. A move cancels this request without consuming a Firefly.
	_hide_hint()
	_hint_waiting = true
	_hint_request_key = key
	_prefetch_hint()
	_build_tools()

## Starts a background search for the current board unless its hint is cached or already being searched.
## A search for an older board is cancelled; _poll_hint starts the current one when it has stopped.
func _prefetch_hint() -> void:
	if session == null:
		return
	var key := session.state.search_key(false)
	if _hint_thread:
		if _hint_task_key != key or session.state.is_won():
			_hint_cancel[0] = true
		return
	if session.state.is_won() or _hint_moves.has(key) or _hint_failures.has(key):
		return
	var snapshot := session.state.clone()
	var last := session.last_move.duplicate()
	var cancel: Array = [false]
	_hint_cancel = cancel
	_hint_task_key = key
	_hint_result = {}
	var result := _hint_result
	_hint_task_usec = Time.get_ticks_usec()
	_hint_thread = Thread.new()
	_hint_thread.start(func():
		var found := Solver.hint(snapshot, last, HINT_STATES, HINT_MS, cancel)
		# A proven dead end gets no suggestion: it would only shuffle birds back and forth.
		if found["status"] == Solver.Status.BUDGET_EXCEEDED and not cancel[0]:
			found["fallback"] = Solver.best_move(snapshot, last)
		result.merge(found))

func _poll_hint() -> void:
	if _hint_thread == null or _hint_thread.is_alive():
		return
	_hint_thread.wait_to_finish()
	_hint_thread = null
	var found := _hint_result
	_hint_result = {}
	var source := "search"
	if _hint_cancel[0]:
		source = "cancelled"
	elif found.get("status", -1) == Solver.Status.SOLVED and not found["moves"].is_empty():
		_hint_moves.merge(found["continuation"], true)
		for route_key in found["continuation"]:
			_winnable[route_key] = true
	elif not found.get("fallback", []).is_empty():
		_hint_moves[_hint_task_key] = found["fallback"]
		source = "fallback"
	else:
		_hint_failures[_hint_task_key] = found["status"]
		source = "failed"
	if OS.is_debug_build():
		print("HINT_SEARCH level=%d result=%s search_ms=%.1f" % [session.state.level_id, source, (Time.get_ticks_usec() - _hint_task_usec) / 1000.0])
	var key := session.state.search_key(false)
	if _hint_waiting and key == _hint_request_key and not busy:
		if _hint_moves.has(key):
			_cancel_hint_request()
			_log_hint(source)
			_apply_hint(_hint_moves[key])
		elif _hint_failures.has(key):
			_cancel_hint_request()
			_build_tools()
			_show_hint_failure(_hint_failures[key])
			Sound.play("error")
	# A dead end proven after the move settled still gets the stuck pointer.
	if source == "failed" and key == _hint_task_key and not busy and not is_instance_valid(_stuck):
		_watch_stuck()
	_prefetch_hint()

## Debug timing evidence: press to hand shown.
func _log_hint(source: String) -> void:
	if OS.is_debug_build():
		print("HINT level=%d moves=%d source=%s press_to_hand_ms=%.1f" % [session.state.level_id, session.move_count(),
			source, (Time.get_ticks_usec() - _hint_press_usec) / 1000.0])

func _release_hint_task() -> void:
	if _hint_thread:
		_hint_cancel[0] = true
		_hint_thread.wait_to_finish()
		_hint_thread = null
	_hint_result = {}

func _show_hint_loading() -> void:
	UI.set_active(_hint_button, false)
	_hint_button.modulate.a = 1.0
	_hint_button.get_node("Icon").hide()
	var spinner := Node2D.new()
	spinner.position = Vector2.ONE * TOOL_SIZE * 0.5
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
	profile.set_tutorial(Progress.used_flag(Board.Booster.FIREFLY))
	sync_inventory()
	Sound.play("hint")
	refresh_hud()
	show_hint(move)

## A hint dims the board around the move's two branches (a soft spotlight with a pulsing rim) and a hand
## shows the move: it taps the source birds, glides along a dotted arc to the landing seat and taps there,
## then starts again. A swivel hint lights its branch and taps the turn-around button. Taps pass through.
const HINT_SPOTLIGHT := preload("res://game/hint_spotlight.gdshader")
const HINT_HAND_SCALE := 0.75
const HINT_HOLE_PAD := 34.0
const HINT_HOLE_HEAD := 190.0     # space above the perch for the standing birds
const HINT_HOLE_FOOT := 70.0
const HINT_DOTS := 9

func show_hint(move: Array) -> void:
	if _hint_move == move and is_instance_valid(_hint_mask):
		return   # the same hint stays up (re-selecting its source does not restart the fade)
	_hide_hint()
	_hint_move = move.duplicate()
	var s := session.state
	var swivel: bool = move[1] < 0
	var source := seat_point(move[0], s.items[move[0]].size() - 1) + Vector2(0, -60)
	var target: Vector2
	if swivel:
		var reach := _branch_width(move[0], s.capacity) - SWIVEL_TIP_IN * BIRD_SCALE
		var right := s.rights[move[0]] == 1
		target = Vector2(1080.0 - reach if right else reach, perch_y(move[0]) + 20.0 * BIRD_SCALE + SWIVEL_DROP * BIRD_SCALE)
	else:
		target = seat_point(move[1], s.items[move[1]].size()) + Vector2(0, -60)
	# Spotlight mask above the board and HUD; it never takes input.
	var mask := ColorRect.new()
	mask.name = "HintSpotlight"
	mask.mouse_filter = Control.MOUSE_FILTER_IGNORE
	mask.size = Vector2(1080, 1920)
	var mat := ShaderMaterial.new()
	mat.shader = HINT_SPOTLIGHT
	mat.set_shader_parameter("hole_a", _hint_hole(move[0]))
	mat.set_shader_parameter("hole_b", Vector4.ZERO if swivel else _hint_hole(move[1]))
	mask.material = mat
	add_child(mask)
	_hint_mask = mask
	# Dotted arc from source to target, revealed as the hand travels.
	var control := (source + target) * 0.5 + Vector2(0, -minf(260.0, 90.0 + source.distance_to(target) * 0.35))
	var trail := Node2D.new()
	trail.name = "HintTrail"
	trail.set_meta("shown", 0.0 if not instant else 1.0)
	trail.set_meta("ripple", -1.0)   # 0..1 while a tap ring spreads from the fingertip, else -1
	trail.set_meta("ripple_at", source)
	trail.draw.connect(func():
		var ripple: float = trail.get_meta("ripple")
		if ripple >= 0.0:
			var ring: Vector2 = trail.get_meta("ripple_at")
			trail.draw_arc(ring, 26.0 + 58.0 * ripple, 0.0, TAU, 48, Color(1.0, 0.9, 0.5, 1.0 - ripple), 9.0 * (1.0 - ripple) + 2.0, true)
		if swivel:
			return
		var shown: float = trail.get_meta("shown")
		for i in HINT_DOTS:
			var t := (i + 1.0) / (HINT_DOTS + 1.0)
			if t > shown:
				break
			var at := source.lerp(control, t).lerp(control.lerp(target, t), t)
			trail.draw_circle(at, 13.0, Color(0.36, 0.22, 0.05, 0.35))
			trail.draw_circle(at, 9.0, Color(1.0, 0.93, 0.62)))
	add_child(trail)
	_hint_trail = trail
	var hand := Sprite2D.new()
	_hint_pointer = hand
	hand.texture = Art.tex("hand_pointer")
	hand.scale = Vector2.ONE * HINT_HAND_SCALE
	hand.offset = Vector2(40, 60)
	add_child(hand)
	hand.position = target if swivel else source
	if instant:
		trail.queue_redraw()
		return
	mat.set_shader_parameter("fade", 0.0)
	var fade_in := mask.create_tween()
	fade_in.tween_method(func(v: float): mat.set_shader_parameter("fade", v), 0.0, 1.0, 0.3)
	var tw := hand.create_tween().set_loops()
	_hint_motion = tw
	var rest := Vector2.ONE * HINT_HAND_SCALE
	var press := rest * 0.82
	if swivel:
		_hint_tap(tw, hand, trail, target, rest, press)
		tw.tween_interval(0.4)
		return
	tw.tween_callback(func():
		hand.position = source
		hand.modulate.a = 1.0
		trail.set_meta("shown", 0.0)
		trail.queue_redraw())
	_hint_tap(tw, hand, trail, source, rest, press)
	tw.tween_method(func(t: float):
		hand.position = source.lerp(control, t).lerp(control.lerp(target, t), t)
		trail.set_meta("shown", t)
		trail.queue_redraw(), 0.0, 1.0, 0.9).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	_hint_tap(tw, hand, trail, target, rest, press)
	tw.tween_interval(0.1)
	tw.tween_property(hand, "modulate:a", 0.0, 0.2)
	tw.tween_interval(0.15)

## One tap of the hint hand: it presses in and springs back while a ring spreads from the fingertip.
func _hint_tap(tw: Tween, hand: Sprite2D, trail: Node2D, at: Vector2, rest: Vector2, press: Vector2) -> void:
	tw.tween_property(hand, "scale", press, 0.16).set_trans(Tween.TRANS_SINE)
	tw.tween_callback(func(): trail.set_meta("ripple_at", at))
	tw.tween_property(hand, "scale", rest, 0.2).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	tw.parallel().tween_method(func(t: float):
		trail.set_meta("ripple", t if t < 1.0 else -1.0)
		trail.queue_redraw(), 0.0, 1.0, 0.45)
	tw.tween_interval(0.25)

## The spotlight hole of one branch: its seats and standing birds, from the screen edge to past the last seat.
func _hint_hole(b: int) -> Vector4:
	var s := session.state
	var width := _branch_width(b, s.capacity) + HINT_HOLE_PAD
	var y := perch_y(b)
	var x := 1080.0 - width if s.rights[b] == 1 else -HINT_HOLE_PAD
	return Vector4(x, y - HINT_HOLE_HEAD, width + HINT_HOLE_PAD, HINT_HOLE_HEAD + HINT_HOLE_FOOT)

func _hide_hint() -> void:
	_hint_move = []
	if _hint_motion:
		_hint_motion.kill()
		_hint_motion = null
	for node in [_hint_pointer, _hint_trail, _hint_mask]:
		if is_instance_valid(node):
			node.queue_free()
	_hint_pointer = null
	_hint_trail = null
	_hint_mask = null

## ---------- event playback ----------

func play_events(events: Array) -> void:
	if instant:
		return
	var playback := _playback_id
	# Homings continue the board's chain of consecutive homing moves (the chorus chain), so a second flock in a
	# row, in this move or the one before, sounds higher.
	var homes := events.filter(func(e): return e["type"] == "flock_home").size()
	_home_chain = maxi(0, session.state.chain - homes)
	for e in events:
		if playback != _playback_id:
			return
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
					Sound.play("egg_hatch")
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
				# The house celebration never holds the board: the next tap works while the flock still flies.
				# Only the winning move waits, so the win popup follows the last homing.
				if session.state.is_won():
					await _flock_home(e)
				else:
					_flock_home(e)
			"gift":
				# The badge is shown reaching what it changes before those changes play.
				var reach := _gift_targets(events, e)
				if session.state.is_won() or not reach.is_empty():
					await _gift(e, reach)
				else:
					_gift(e, reach)
			"branch_grew", "twig_upgraded":
				_layout_branches()
			"crows_left":
				for item in e["birds"]:
					var crow: BirdView = _birds.get(item["id"])
					if crow:
						_birds.erase(item["id"])
						crow.fly_to(Vector2(crow.position.x + (600.0 if crow.position.x > 540 else -600.0), -200), 0.0, 0.8, true)
				await get_tree().create_timer(0.8).timeout
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
		bird.fly_to(seat_point(to, flights[k][1]), k * MOVE_STAGGER_SECONDS, MOVE_FLIGHT_SECONDS,
			false, false, null, BirdView.WING_SILENT, MOVE_LANDING_SECONDS)
		last = bird
	Sound.play("move")
	if last:
		# The branch takes the first bird's weight at touchdown.
		var touch := _playback_tween()
		touch.tween_interval(BirdView.TAKEOFF_SECONDS + MOVE_FLIGHT_SECONDS)
		touch.tween_callback(func():
			Sound.vibrate(Sound.PULSE_LAND)
			_dip_branch(to))
		await last.finished

## Landing: the branch dips a few pixels under the weight and springs back (LAND_DIP_SECONDS). It bends from the
## screen edge, so the tip moves most; the birds on it move with their seat. A refused tap keeps its wobble.
const LAND_DIP := 7.0            # tip travel in px
const LAND_DIP_ROOT := 0.4       # share of the travel at the screen edge
const LAND_DIP_SECONDS := 0.25

func _dip_branch(b: int) -> void:
	if instant or b < 0 or b >= _branch_nodes.size() or not is_instance_valid(_branch_nodes[b]):
		return
	var node: Node2D = _branch_nodes[b]
	var art: Node2D = node.get_node_or_null("Art")
	if art == null:
		return
	var s := session.state
	var right := s.rights[b] == 1
	var length := maxf(1.0, _painted_length(_slot(s.rows[b], right)))
	var riders: Array = []
	for code in s.items[b]:
		var bird: BirdView = _birds.get(Board.id_of(code))
		if bird:
			riders.append([bird, clampf(absf(bird.position.x - node.position.x) / length, 0.0, 1.0)])
	var bend := LAND_DIP * (1.0 - LAND_DIP_ROOT) / length
	var pose := func(f: float):
		if not is_instance_valid(art):
			return
		art.position.y = LAND_DIP * LAND_DIP_ROOT * f
		art.rotation = bend * f * (-1.0 if right else 1.0)
		for r in riders:
			if is_instance_valid(r[0]):
				r[0].sag = LAND_DIP * (LAND_DIP_ROOT + (1.0 - LAND_DIP_ROOT) * r[1]) * f
	# Owned by the branch art, so the tween dies with it: a lambda whose captured node was freed (restart,
	# leaving the level mid-landing) errors when called, before its own validity guard can run.
	var tw := art.create_tween()
	tw.tween_method(func(u: float): pose.call(_dip_curve(u)), 0.0, 1.0, LAND_DIP_SECONDS)
	tw.tween_callback(func(): pose.call(0.0))

## 0 -> 1 eased down in the first 30 percent, then a spring back past rest (slightly up) and settle at 0.
static func _dip_curve(u: float) -> float:
	if u < 0.3:
		return sin(u / 0.3 * PI * 0.5)
	var v := (u - 0.3) / 0.7
	var c := 1.70158
	var back := 1.0 + (c + 1.0) * pow(v - 1.0, 3.0) + c * pow(v - 1.0, 2.0)   # ease-out back, overshoots past 1
	return 1.0 - back

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
	var playback := _playback_id
	var ids: PackedInt32Array = e["ids"]
	_door(1.0, 0.2)
	var last: BirdView = null
	for k in ids.size():
		var bird: BirdView = _birds.get(ids[ids.size() - 1 - k])
		if bird == null:
			continue
		_birds.erase(ids[ids.size() - 1 - k])
		_homing_birds[ids[ids.size() - 1 - k]] = bird
		# Approach in front of the house; clipping starts only once the bird fits inside its doorway.
		bird.reparent(_hud)
		bird.fly_to(_door_point(), k * 0.12, FLOCK_FLIGHT_SECONDS, true, false, _door_mask)
		last = bird
	if last:
		await last.finished
	if playback != _playback_id:
		return
	for id in ids:
		_homing_birds.erase(id)
	# The high point: each further homing in one move chain sounds a whole tone higher.
	_home_chain += 1
	var pitch := pow(2.0, HOME_PITCH_STEP * mini(_home_chain - 1, HOME_PITCH_MAX) / 12.0)
	Sound.play("nest_home", pitch)
	if OS.is_debug_build():
		print("FLOCK_HOME species=%s chain=%d pitch=%.3f" % [e["species"], _home_chain, pitch])
	Sound.vibrate(Sound.PULSE_HOME)
	_set_resident(Board.species_index(e["species"]), true)
	_sparkle_burst(_door_point())
	# The house bounces on its rope (pivot where the rope meets it) in the same 0.35 s as before.
	_house.pivot_offset = Vector2(_house.size.x * 0.5, 0.0)
	var rest_y := _house_rect().position.y
	var tw := _playback_tween()
	tw.tween_property(_house, "scale", Vector2(1.12, 0.88), 0.08).set_ease(Tween.EASE_OUT)
	tw.parallel().tween_property(_house, "position:y", rest_y - 14.0, 0.08).set_ease(Tween.EASE_OUT)
	tw.tween_property(_house, "scale", Vector2(0.94, 1.07), 0.1).set_trans(Tween.TRANS_SINE)
	tw.parallel().tween_property(_house, "position:y", rest_y, 0.1).set_trans(Tween.TRANS_SINE)
	tw.tween_property(_house, "scale", Vector2.ONE, 0.17).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	_glow.pivot_offset = _glow.size * 0.5
	var glow := _playback_tween()
	glow.tween_property(_glow, "modulate:a", 0.95, 0.12)
	glow.parallel().tween_property(_glow, "scale", Vector2.ONE * 1.2, 0.3).from(Vector2.ONE * 0.9).set_ease(Tween.EASE_OUT)
	glow.tween_property(_glow, "modulate:a", 0.0, 0.5).set_ease(Tween.EASE_IN)
	glow.parallel().tween_property(_glow, "scale", Vector2.ONE, 0.5)
	await tw.finished
	if playback != _playback_id:
		return
	await _door(0.0, 0.25)

## Every flock takes the same time to reach the house on every level, whatever the distance.
const FLOCK_FLIGHT_SECONDS := 0.9
const HOME_PITCH_STEP := 2.0     # semitones per further homing in one chain
const HOME_PITCH_MAX := 5
const SPARKLES := 12
const SPARKLE_SECONDS := 0.6

## A ring of painted sparkles bursts from the doorway, spins out and fades (fire and forget, never awaited).
func _sparkle_burst(at: Vector2) -> void:
	if instant:
		return
	var tex := Art.tex("pic_sparkle")
	if tex == null:
		return
	for i in SPARKLES:
		var spark := Sprite2D.new()
		spark.texture = tex
		spark.position = at
		var size := (46.0 if i % 2 == 0 else 32.0) / float(tex.get_width())
		spark.scale = Vector2.ONE * size * 0.3
		_home_effects.add_child(spark)
		var dir := Vector2.from_angle(TAU * i / SPARKLES + 0.2)
		var reach := 150.0 if i % 2 == 0 else 112.0
		var tw := spark.create_tween().set_parallel()
		tw.tween_property(spark, "position", at + dir * reach, SPARKLE_SECONDS).set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)
		tw.tween_property(spark, "rotation", (1.0 if i % 2 == 0 else -1.0) * PI, SPARKLE_SECONDS)
		tw.tween_property(spark, "scale", Vector2.ONE * size, SPARKLE_SECONDS * 0.35).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
		tw.tween_property(spark, "modulate:a", 0.0, SPARKLE_SECONDS * 0.45).set_delay(SPARKLE_SECONDS * 0.55)
		tw.chain().tween_callback(spark.queue_free)

## Fades the open-door art in or out.
func _door(open: float, seconds: float) -> void:
	if _door_open == null:
		return
	var tw := _playback_tween()
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
	_resident.setup(Board.bird(0, species_index, 0), false, 0.58, WorldBirds.look(Board.SPECIES[species_index], _world()))
	_door_mask.add_child(_resident)
	var feet := Vector2(_door_mask.size.x * 0.5, _door_mask.size.y * 0.97)
	_resident.position = feet
	if instant:
		_resident.freeze()
	elif rise:
		_resident.position.y = feet.y + 90.0
		_resident.create_tween().tween_property(_resident, "position:y", feet.y, 0.3).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)

## What a homecoming gift is about to change, as points on the board: the birds it wakes, the eggs it reveals or
## hatches, the crows it sends away, the branch it grows, the move counter it spares.
const GIFT_REACH_MAX := 8

func _gift_targets(events: Array, gift: Dictionary) -> Array[Vector2]:
	var points: Array[Vector2] = []
	var at := events.find(gift)
	for i in range(at + 1, events.size()):
		var e: Dictionary = events[i]
		match e["type"]:
			"gift", "flock_home", "transferred", "swivelled", "wave":
				break
			"woke", "hatched", "egg_revealed":
				var bird: BirdView = _birds.get(e["id"])
				if bird:
					points.append(bird.global_position - global_position + Vector2(0, -80.0 * BIRD_SCALE))
			"crows_left":
				for item in e["birds"]:
					var crow: BirdView = _birds.get(item["id"])
					if crow:
						points.append(crow.global_position - global_position + Vector2(0, -80.0 * BIRD_SCALE))
			"branch_grew", "twig_upgraded":
				points.append(seat_point(e["branch"], maxi(0, int(e["after"]) - 1)) + Vector2(0, -60.0))
	if gift["species"] == "berry" and is_instance_valid(_moves_icon):   # the free step: the move counter
		points.append(_moves_icon.get_global_rect().get_center() - global_position)
	if points.size() > GIFT_REACH_MAX:
		points.resize(GIFT_REACH_MAX)
	return points

## The ability badge of a delivered flock pops out of the doorway. With something to change it sends a small badge
## to each thing it changes (returning when they arrive, so the change plays after its cause); otherwise it rises
## and fades.
func _gift(e: Dictionary, reach: Array[Vector2] = []) -> void:
	var coin := Sprite2D.new()
	coin.texture = Art.tex("gift_coin_" + GIFT_COIN.get(e["species"], "wake"))
	coin.position = _door_point()
	coin.scale = Vector2.ONE * 0.2
	_home_effects.add_child(coin)
	Sound.play("collect")
	var small := 0.46 * GOAL / float(coin.texture.get_width())
	var big := 0.8 * GOAL / float(coin.texture.get_width())
	var tw := coin.create_tween()
	tw.tween_property(coin, "scale", Vector2.ONE * big, 0.2).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	tw.tween_interval(0.15)
	if reach.is_empty():
		var target := coin.position + Vector2(0, -90)
		tw.tween_property(coin, "position", target, 0.3).set_ease(Tween.EASE_IN_OUT)
		tw.parallel().tween_property(coin, "scale", Vector2.ONE * small, 0.3)
		tw.tween_property(coin, "modulate:a", 0.0, 0.15)
		await tw.finished
		coin.queue_free()
		return
	await tw.finished
	if not is_instance_valid(coin):
		return
	var origin := _home_effects.global_position - global_position
	for k in reach.size():
		var copy := Sprite2D.new()
		copy.texture = coin.texture
		copy.position = coin.position
		copy.scale = Vector2.ONE * small
		_home_effects.add_child(copy)
		var fly := copy.create_tween()
		fly.tween_interval(k * GIFT_STAGGER)
		fly.tween_property(copy, "position", reach[k] - origin, GIFT_FLIGHT).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
		fly.tween_property(copy, "scale", Vector2.ONE * small * 1.5, 0.12).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
		fly.tween_interval(0.25)
		fly.tween_property(copy, "modulate:a", 0.0, 0.2)
		fly.tween_callback(copy.queue_free)
	var fade := coin.create_tween()
	fade.tween_property(coin, "modulate:a", 0.0, 0.25)
	fade.tween_callback(coin.queue_free)
	await get_tree().create_timer(GIFT_FLIGHT + (reach.size() - 1) * GIFT_STAGGER + 0.1).timeout

const GIFT_FLIGHT := 0.4
const GIFT_STAGGER := 0.05

## The board cheers before the win panel opens: the house bounces and glows, the goal keys hop one after another,
## confetti bursts from the doorway and the resident cheers in it (CELEBRATE seconds).
const CELEBRATE := 1.3

func _celebrate() -> void:
	var playback := _playback_id
	busy = true
	_last_won = int(level["id"])
	Sound.play("win_fanfare")
	if not instant:
		_play_celebration()
	await get_tree().create_timer(CELEBRATE if not instant else 0.0).timeout
	if playback != _playback_id:
		return
	won.emit({"session": session})

func _play_celebration() -> void:
	_house.pivot_offset = Vector2(_house.size.x * 0.5, _house.size.y)
	var hop := _house.create_tween()
	hop.tween_property(_house, "scale", Vector2(1.12, 0.9), 0.1).set_ease(Tween.EASE_OUT)
	hop.tween_property(_house, "scale", Vector2(0.94, 1.12), 0.14).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)
	hop.tween_property(_house, "scale", Vector2.ONE, 0.4).set_trans(Tween.TRANS_ELASTIC).set_ease(Tween.EASE_OUT)
	var shine := _glow.create_tween()
	shine.tween_property(_glow, "modulate:a", 0.95, 0.2)
	shine.tween_property(_glow, "modulate:a", 0.0, 0.9).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN)
	if _resident and is_instance_valid(_resident):
		_resident.cheer()
	_confetti_burst(_door_rect.get_center())

## Confetti pieces fly out of `origin` in a fan, then flutter down and fade.
func _confetti_burst(origin: Vector2) -> void:
	var rng := RandomNumberGenerator.new()
	rng.seed = int(level["id"])
	var layer := Control.new()
	layer.name = "Celebration"
	layer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_hud.add_child(layer)
	for i in 56:
		var piece := TextureRect.new()
		piece.texture = Art.tex("confetti_%d" % (1 + i % 4))
		piece.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
		piece.size = Vector2.ONE * rng.randf_range(40.0, 62.0)
		piece.pivot_offset = piece.size * 0.5
		piece.mouse_filter = Control.MOUSE_FILTER_IGNORE
		layer.add_child(piece)
		var angle := deg_to_rad(rng.randf_range(-170.0, -10.0))
		var speed := rng.randf_range(620.0, 1240.0)
		var velocity := Vector2(cos(angle), sin(angle)) * speed
		var spin := rng.randf_range(300.0, 720.0) * (1.0 if i % 2 == 0 else -1.0)
		var life := rng.randf_range(1.3, 1.8)
		var sway := rng.randf_range(10.0, 30.0)
		piece.position = origin - piece.pivot_offset
		var tw := piece.create_tween()
		tw.tween_method(func(t: float):
			var drag := 1.0 - exp(-3.0 * t)
			var at := origin + velocity * drag / 3.0 + Vector2(sin(t * 10.0 + i) * sway, 520.0 * t * t)
			piece.position = at - piece.pivot_offset
			piece.rotation_degrees = spin * t
			piece.modulate.a = minf(1.0, (life - t) * 3.0), 0.0, life, life)
		tw.tween_callback(piece.queue_free)

## Called after a booster purchase.
func granted(kind: int, count: int) -> void:
	session.grant_booster(kind, count)
	refresh_hud()

## Extra undos bought with coins or earned by an ad.
func granted_undos(count: int) -> void:
	session.grant_undos(count)
	_build_tools()

## Capture states: "celebrate" (the win celebration on the board), "sleeping_bubble" (a tapped sleeping owl explains itself),
## "stuck_hint" (no move left) and "dead_end" (a shuffle inside a position that can never win; the pointer appears by itself);
## "air_N" (level N as it opens, to watch the living
## air of its world and weather variant), "hud_progress" (two stars already lost), "hud_over_A_B..." (level 12 at par+A moves,
## then par+B after a pause, and so on; values may be negative),
## "hint_timing_N" / "hint_think_N" (press the
## firefly at once or after three seconds on level N and log the time to the hand) and "hint_all" (all 40
## levels: background search time and first press, then quit); "replay_N" / "next_N" (level N with the quick
## entrance), "flock_home" (two flocks home in a row) and "landing" (two plain landings); "_slow" runs them at a
## fifth of the speed for frame sequences.
static func preview(main: Control, state: String) -> void:
	if state == "gift_reach":   # a sunny flock goes home: its wake badge flies to both sleepers, then they wake
		var board := BoardView.new()
		main.set_screen(board)
		board.start({"id": 1, "capacity": 4, "par": 3, "world": 1, "variant": "noon", "branches": [
			{"side": "L", "row": 0, "birds": ["sunny", "sunny", "sunny"]},
			{"side": "R", "row": 0, "birds": ["berry:sleep", "sunny"]},
			{"side": "L", "row": 1, "birds": ["berry", "berry:sleep"]},
			{"side": "R", "row": 1, "birds": ["berry"]}]}, main.profile, false)
		await main.get_tree().create_timer(1.0).timeout
		board.tap_branch(1)
		await main.get_tree().create_timer(0.6).timeout
		board.tap_branch(0)
		return
	if state == "hold_bird":   # a finger held on a bird opens its card
		var board := BoardView.new()
		main.set_screen(board)
		board.start(Levels.get_level(12), main.profile, false)
		board.bird_info_requested.connect(func(species: String): load("res://game/popups.gd").goal_info(main.popups, species))
		await main.get_tree().create_timer(1.0).timeout
		var bird: BirdView = board._birds.values()[0]
		board._begin_hold(bird.global_position - board.global_position + Vector2(0, -60))
		return
	if state == "celebrate":   # level 12 at rest, then the win celebration on the board (frame sequences)
		var board := BoardView.new()
		main.set_screen(board)
		board.start(Levels.get_level(12), main.profile, false)
		await main.get_tree().create_timer(1.0).timeout
		board._play_celebration()
		return
	if state == "lifted_both_sides":   # one left and one right branch lifted: each flock looks across the board
		var board := BoardView.new()
		main.set_screen(board)
		board.start(Levels.get_level(12), main.profile, false)
		await main.get_tree().create_timer(5.0).timeout
		var sides := {}
		for b in board.session.state.branch_count():
			if board.session.state.items[b].size() > 0 and not sides.has(board.session.state.rights[b]):
				sides[board.session.state.rights[b]] = b
		for b in sides.values():
			board._lift(b, true)
		return
	if state.begins_with("air_"):
		var board := BoardView.new()
		board.instant = main.instant()
		main.set_screen(board)
		board.start(Levels.get_level(int(state.trim_prefix("air_"))), main.profile, false)
		return
	if state.begins_with("hud_over_"):
		var board := BoardView.new()
		board.instant = main.instant()
		main.set_screen(board)
		board.start(Levels.get_level(12), main.profile, false)
		for step in state.trim_prefix("hud_over_").split("_"):
			board.session.state.counted_moves = maxi(0, board.session.par + step.to_int())
			board.refresh_hud()
			if not board.instant:
				await board.get_tree().create_timer(1.0).timeout
		return
	if state == "hud_progress":
		var board := BoardView.new()
		board.instant = main.instant()
		main.set_screen(board)
		board.start(Levels.get_level(36), main.profile, false)
		board.session.state.counted_moves = 13
		board.refresh_hud()
		return
	if state.begins_with("hint_timing_") or state.begins_with("hint_think_") or state == "hint_all":
		var player: Profile = main.profile.duplicate_profile()
		player.highest_unlocked = Levels.COUNT
		var ids: Array = range(1, 41) if state == "hint_all" else [int(state.get_slice("_", 2))]
		var think := 3.0 if state.begins_with("hint_think_") else 0.0
		for id in ids:
			player.inventory[Board.Booster.FIREFLY] = 5
			var board := BoardView.new()
			board.instant = state == "hint_all"
			main.set_screen(board)
			var started := Time.get_ticks_usec()
			board.start(Levels.get_level(id), player, false)
			if state == "hint_all":
				while not board._hint_moves.has(board.session.state.search_key(false)):
					await main.get_tree().process_frame
				print("HINT_READY level=%d ready_after_start_ms=%.1f" % [id, (Time.get_ticks_usec() - started) / 1000.0])
			else:
				await main.get_tree().create_timer(think + 0.05).timeout
			# Three presses: the first hint, then the next one after following it.
			for press in (1 if state == "hint_all" else 3):
				board._on_tool(Board.Booster.FIREFLY)
				while board._hint_waiting:
					await main.get_tree().process_frame
				if state == "hint_all" or board._hint_move.is_empty() or board.session.state.is_won():
					break
				var move: Array = board._hint_move
				await main.get_tree().create_timer(0.3).timeout
				board.tap_branch(move[0])
				if move[1] >= 0:
					board.tap_branch(move[1])
				elif board.session.selected >= 0:
					board.tap_branch(move[0])
				await main.get_tree().process_frame
				while board.busy and not board.session.state.is_won():
					await main.get_tree().process_frame
				await main.get_tree().create_timer(0.2 if state == "hint_all" else 0.6).timeout
		main.get_tree().quit()
		return
	if state in ["hint_searching", "hint_timeout", "hint_unavailable", "hint_transfer"]:
		var player: Profile = main.profile.duplicate_profile()
		player.highest_unlocked = Levels.COUNT
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
	if state.begins_with("replay_") or state.begins_with("next_"):
		# The quick entrance: level N opened before in this run, or N right after winning N - 1.
		var id := int(state.get_slice("_", 1))
		if state.begins_with("replay_"):
			_opened_levels[id] = true
		else:
			_last_won = id - 1
			_last_started = id - 1
		var board := BoardView.new()
		board.instant = main.instant()
		main.set_screen(board)
		board.start(Levels.get_level(id), main.profile, true)
		return
	if state.begins_with("tool_bar"):
		# Every tool unlocked, stock on some, one move made so undo shows: "tool_bar" (level 25), "tool_bar_N".
		var player: Profile = main.profile.duplicate_profile()
		player.highest_unlocked = Levels.COUNT
		for kind in Board.BOOSTER_COUNT:
			player.inventory[kind] = [3, 0, 2, 1, 5, 0][kind % 6]
		var board := BoardView.new()
		board.instant = main.instant()
		main.set_screen(board)
		board.start(Levels.get_level(int(state.trim_prefix("tool_bar_")) if state != "tool_bar" else 25), player, false)
		var move: Array = Solver.best_move(board.session.state)
		if not move.is_empty() and move[1] >= 0:
			board.session.try_move(move[0], move[1])
			board.sync_birds()
			board.refresh_hud()
		return
	if state in ["flock_home", "landing", "flock_home_slow", "landing_slow"]:
		# Two plain moves: "landing" lands a bird on a matching one twice; "flock_home" completes two flocks in a
		# row (the second home sound is a whole tone higher).
		var feel := {"id": 98, "world": 1, "variant": "morning", "capacity": 4, "par": 12, "branches": [
			{"side": "L", "row": 0, "kind": "normal", "seats": 0, "birds": ["sunny", "sunny", "sunny"]},
			{"side": "R", "row": 0, "kind": "normal", "seats": 0, "birds": ["berry", "sunny"]},
			{"side": "L", "row": 1, "kind": "normal", "seats": 0, "birds": ["sky", "sky", "sky"]},
			{"side": "R", "row": 1, "kind": "normal", "seats": 0, "birds": ["berry", "sky"]},
			{"side": "L", "row": 2, "kind": "normal", "seats": 0, "birds": ["berry", "berry"]},
			{"side": "R", "row": 2, "kind": "normal", "seats": 0, "birds": []}]}
		if state.begins_with("landing"):
			for b in 6:
				feel["branches"][b]["birds"] = [["berry", "sunny"], ["berry", "sunny"], ["berry", "sky"], ["berry", "sky"],
					["sunny", "sunny"], ["sky", "sky"]][b]
		var board := BoardView.new()
		main.set_screen(board)
		board.start(feel, main.profile, false)
		if state.ends_with("_slow"):
			Engine.time_scale = 0.2   # frame evidence: game time runs at a fifth, so each saved frame is a short step
		for move in [[1, 0], [3, 2]]:
			await main.get_tree().create_timer(0.8).timeout
			board.tap_branch(move[0])
			await main.get_tree().create_timer(0.4).timeout
			board.tap_branch(move[1])
			await main.get_tree().process_frame
			while board.busy:
				await main.get_tree().process_frame
		return
	if state == "dead_end":   # one real shuffle inside a position that can never win: the pointer appears by itself
		var board := BoardView.new()
		board.instant = main.instant()
		main.set_screen(board)
		board.start({"id": 10, "world": 1, "variant": "noon", "capacity": 4, "par": 10, "branches": [
			{"side": "L", "row": 0, "birds": ["berry", "cocoa"]},
			{"side": "R", "row": 0, "birds": ["sky", "berry", "sunny"]},
			{"side": "L", "row": 1, "birds": ["sky", "sunny", "sunny", "sunny"]},
			{"side": "R", "row": 1, "birds": ["sky"]},
			{"side": "L", "row": 2, "birds": ["sky", "cocoa", "cocoa", "cocoa"]},
			{"side": "R", "row": 2, "birds": ["berry", "berry:sleep"]}]}, main.profile, false)
		await main.get_tree().create_timer(0.5).timeout
		board.tap_branch(4)
		board.tap_branch(0)
		var t0 := Time.get_ticks_msec()
		while not is_instance_valid(board._stuck) and Time.get_ticks_msec() - t0 < 5000:
			await main.get_tree().process_frame
		var key := board.session.state.search_key(false)
		print("DEAD_END moves=%d stuck=%s after_ms=%d hint_failure=%d hint_move=%s" % [board.session.move_count(),
			is_instance_valid(board._stuck), Time.get_ticks_msec() - t0, board._hint_failures.get(key, -1), board._hint_moves.has(key)])
		return
	if state.begins_with("level_"):   # the real level on the board, without the first-visit popups
		var board := BoardView.new()
		board.instant = true
		main.set_screen(board)
		board.start(Levels.get_level(int(state.trim_prefix("level_"))), main.profile, false)
		return
	var def := {"id": 99, "world": 1, "variant": "morning", "capacity": 4, "par": 12, "branches": [
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
