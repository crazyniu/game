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
const BOOSTER_SOUND := {Board.Booster.FIREFLY: "hint", Board.Booster.SAPLING: "sapling", Board.Booster.OWL_EYE: "owl_eye",
	Board.Booster.MORNING_BELL: "morning_bell", Board.Booster.SCARECROW: "scarecrow", Board.Booster.SPRING_RAIN: "spring_rain"}
## Entrance beats (seconds): houses drop in, branches grow, then birds fly in; ends about 3.9 s after the first frame.
const ENTRANCE_HOUSE := 0.1
const ENTRANCE_BRANCHES := 0.95
const ENTRANCE_BIRDS := 1.6
const ENTRANCE_BIRD_SPREAD := 1.1
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


func start(level_def: Dictionary, player: Profile, play_entrance := true) -> void:
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
	Engine.time_scale = 1.0

## ---------- scene ----------

func _world() -> int:
	return session.state.world

func _build_scene() -> void:
	var variant := session.state.variant
	var bg := "bg_level_w%d_%s" % [_world(), variant]
	if not Art.has(bg):
		bg = "bg_level_w%d_noon" % _world()
	UI.image(self, bg, Rect2(0, 0, 1080, 1920), false)
	_build_ambient()
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

## ---------- ambient ----------

## Soft clouds drift across the sky behind the branches; rain and drizzle levels add falling drops.
func _build_ambient() -> void:
	_sky = Node2D.new()
	_sky.name = "Ambient"
	add_child(_sky)
	for i in 3:
		_add_mote("cloud_soft_%d" % (i + 1), "cloud", Vector2(0, 280 + i * 145), Vector2(420 + i * 65, 190), 48 + i * 12, i * 0.29, 0.13, 0)
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

func _tick_ambient() -> void:
	for m in _motes:
		var u := fposmod(_clock / m["period"] + m["offset"], 1.0)
		var at: Vector2 = m["origin"]
		var node: Sprite2D = m["node"]
		var fade := minf(1.0, minf(u, 1.0 - u) * (10.0 if m["kind"] == "cloud" else 12.0))
		if m["kind"] == "cloud":
			at.x = -500.0 + 2080.0 * u
		else:
			at += Vector2(sin(u * TAU) * m["drift"], -40.0 + 2000.0 * u)
		node.position = at
		node.modulate.a = m["alpha"] * fade

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
	var before := _branch_nodes.size()
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
		if b >= before and before > 0 and not instant:
			# A branch added by the sapling grows out of the trunk.
			node.scale = Vector2(0.05, 0.6)
			create_tween().tween_property(node, "scale", Vector2.ONE, 0.6).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)

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
	var glide: Tween = null
	for b in s.branch_count():
		for i in s.items[b].size():
			var code := s.items[b][i]
			var id := Board.id_of(code)
			seen[id] = true
			var bird: BirdView = _birds.get(id)
			if bird == null or not is_instance_valid(bird):
				bird = _make_bird(code, seat_point(b, i))
			elif bird.code != code or bird.asleep != s.is_asleep(code) or bird.density != _density:
				bird.setup(code, s.is_asleep(code), _density, bird.outfit)
			if instant or bird.position.distance_to(seat_point(b, i)) < 2.0:
				bird.position = seat_point(b, i)
			else:
				# Rows can shift when a branch is added; birds glide to their new seats.
				if glide == null:
					glide = create_tween().set_parallel()
				glide.tween_property(bird, "position", seat_point(b, i), 0.45).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
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

## HUD layout (docs/design/ui-system.md section 6, 1080x1920 design pixels).
## The spec's left column (x 40-380) cannot hold footprints + moves + three star columns with two-digit
## thresholds at the specified sizes, so both pills end at x 404 and the goal house narrows to 240 (16 px gap).
const HUD_LEFT := 40.0
const PILL_ART_HEIGHT := 96.0            # stat_pill is drawn at its own height and scaled, so every pill has round ends
const COIN_PILL := Rect2(92, 40, 312, 96)
const COIN_ICON := Rect2(40, 36, 104, 104)   # overlaps the pill's left end by half its width
const COIN_VALUE := Rect2(168, 62, 212, 52)  # left-aligned 24 px after the coin
const LEVEL_PILL := Rect2(40, 152, 364, 112)
const FOOTPRINTS := Rect2(54, 182, 52, 52)
const MOVE_VALUE := Rect2(114, 182, 84, 52)
const MOVE_DIVIDER := Rect2(204, 176, 4, 64)
const STAR_COLUMN_X := 216.0             # first star column; columns are 52 wide, 6 apart
const STAR_COLUMN_W := 52.0
const STAR_COLUMN_PITCH := 58.0
const STAR_Y := 170.0
const STAR_SIZE := 44.0
const THRESHOLD_Y := 216.0
const THRESHOLD_CAP := 30.0
const SUN_PILL := Rect2(40, 280, 176, 88)
const SUN_ICON := Rect2(60, 294, 60, 60)
const SUN_VALUE := Rect2(132, 298, 64, 52)
const GEAR := Rect2(928, 40, 112, 112)
const GLYPH_RATIO := 0.5                 # glyph icon inside a disc
## Goal house: hangs from its rope, centred at x 540; badge row 24 px below it.
const HOUSE_WIDTH := 240.0
const HOUSE_TOP := 140.0
const HOUSE_CONTENT := Rect2(45, 110, 624, 580)      # default house body in sprite pixels (without the hanger loop)
const HOUSE_DOOR := Rect2(245.75, 325, 240, 247.5)   # its round doorway in sprite pixels
const HOUSE_LOOP_Y := 50.0                           # hanger loop centre in sprite pixels
const ROPE_WIDTH := 16.0
const BADGE_GAP_Y := 24.0
const RING := 104.0
const RING_GAP := 24.0
const RING_MIN_GAP := 16.0
const RING_ROW_WIDTH := 1000.0           # screen width minus both side margins
const FACE_RATIO := 0.8                  # face fits inside 80 % of the ring
const RING_HOLE_RATIO := 0.8             # cream backing disc behind the face
const ABILITY_RATIO := 0.44
## Bottom tool bar: cream bar 160 tall, 48 px from the bottom, discs 128 spaced 64 apart.
const TOOL_BAR_HEIGHT := 160.0
const TOOL_BAR_BOTTOM := 1872.0
const TOOL_DISC := 128.0
const TOOL_GAP := 64.0
const TOOL_BAR_PAD := 40.0
const STOCK_BADGE := 52.0
const STOCK_CAP := 30.0
## Sleep bubble and stuck pointer use the same icon size as the pills.
const HUD_ICON := 64.0
const BUBBLE_SCALE := 0.35               # sleep_bubble art drawn at 35 %: body about 110 px tall
const BUBBLE_MARGINS := [200, 140, 170, 140]   # nine-slice left, top, right, bottom in sprite pixels (tail inside the left)
const BUBBLE_TAIL := Vector2(124, 400)   # tail tip in sprite pixels
const BUBBLE_BODY_CENTRE_Y := 165.0      # body centre in sprite pixels
const BUBBLE_PAD := 48.0
const BUBBLE_ICON_GAP := 16.0

var _door_rect := Rect2()

func _house_rect() -> Rect2:
	var k := HOUSE_WIDTH / HOUSE_CONTENT.size.x
	return Rect2(540.0 - HOUSE_WIDTH * 0.5, HOUSE_TOP, HOUSE_WIDTH, HOUSE_CONTENT.size.y * k)

## A cream stat pill of any height with fully round ends: the art is sliced at its own height and scaled.
func _pill(parent: Control, r: Rect2) -> NinePatchRect:
	return UI.panel(parent, "stat_pill", r, r.size.y / PILL_ART_HEIGHT)

func _build_hud() -> void:
	_hud = Control.new()
	_hud.name = "HUD"
	_hud.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_hud.size = Vector2(1080, 1920)
	add_child(_hud)
	# Rope and goal house.
	var hr := _house_rect()
	var k := HOUSE_WIDTH / HOUSE_CONTENT.size.x
	_glow = UI.art(_hud, "house_glow", Rect2(hr.get_center() - hr.size * 0.9, hr.size * 1.8))
	_glow.modulate = Color(1, 0.8, 0.35, 0.0)
	_rope = Control.new()
	_rope.name = "Rope"
	_rope.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_rope.clip_contents = true
	var rope_bottom := hr.position.y - (HOUSE_CONTENT.position.y - HOUSE_LOOP_Y) * k
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
	var style := profile.equipped_house
	var body_name := "goal_house_single" if style == "default" else "house_style_%s_body" % style
	var body_content := HOUSE_CONTENT if style == "default" else Rect2()
	UI.art(_house, body_name, Rect2(Vector2.ZERO, hr.size), body_content)
	# The doorway is empty and dark until a species is delivered; the open-door art fades in while birds enter.
	var opening := Rect2((HOUSE_DOOR.position - HOUSE_CONTENT.position) * k, HOUSE_DOOR.size * k)
	_door_rect = Rect2(opening.position + hr.position, opening.size)
	_door_mask = TextureRect.new()
	_door_mask.texture = _disc_texture()
	_door_mask.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	_door_mask.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_door_mask.clip_children = CanvasItem.CLIP_CHILDREN_ONLY
	UI.place(_door_mask, opening)
	_house.add_child(_door_mask)
	_door_open = null
	if style == "default":
		_door_open = UI.art(_house, "goal_house_door_open", Rect2(Vector2.ZERO, hr.size), body_content)
		_door_open.modulate.a = 0.0
	# Coin pill.
	_pill(_hud, COIN_PILL)
	UI.art(_hud, "coin", COIN_ICON)
	_coin_number = UI.number(_hud, "0", COIN_VALUE, "brown", -1)
	# Level pill: footprints and moves, a divider, then three stars with their move thresholds.
	_pill(_hud, LEVEL_PILL)
	UI.art(_hud, "footprints", FOOTPRINTS)
	_move_number = UI.number(_hud, "0", MOVE_VALUE)
	var div := ColorRect.new()
	div.color = Color(0.62, 0.45, 0.3, 0.55)
	div.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(div, MOVE_DIVIDER)
	_hud.add_child(div)
	var thresholds := [str(session.par), str(session.par + 3), "∞"]
	for i in 3:
		var x := STAR_COLUMN_X + i * STAR_COLUMN_PITCH
		UI.art(_hud, "star_full_small", Rect2(x + (STAR_COLUMN_W - STAR_SIZE) * 0.5, STAR_Y, STAR_SIZE, STAR_SIZE))
		UI.number(_hud, thresholds[i], Rect2(x, THRESHOLD_Y, STAR_COLUMN_W, THRESHOLD_CAP))
	# Day/night counter.
	_sun_pill = Control.new()
	_sun_pill.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_hud.add_child(_sun_pill)
	_pill(_sun_pill, SUN_PILL)
	UI.art(_sun_pill, "dial_sun", SUN_ICON)
	_sun_number = UI.number(_sun_pill, "0", SUN_VALUE)
	_sun_pill.visible = session.state.day_length > 0
	# Settings.
	var gear := UI.disc(_hud, "Settings", GEAR, "", func(): settings_requested.emit())
	_disc_glyph(gear, "icon_gear")
	_build_goals()
	_tool_bar = Control.new()
	_tool_bar.name = "Tools"
	_tool_bar.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_hud.add_child(_tool_bar)

## Glyph icon centred in a disc at half its diameter, fitted by its visible pixels.
func _disc_glyph(b: Control, icon: String, ratio := GLYPH_RATIO) -> void:
	var s := b.size * ratio
	UI.art(b, icon, Rect2((b.size - s) * 0.5, s)).name = "Icon"

func _build_goals() -> void:
	var ids := Levels.species_ids(session.state)
	var n := ids.size()
	var ring := RING
	var gap := RING_GAP
	if n * ring + (n - 1) * gap > RING_ROW_WIDTH:
		gap = RING_MIN_GAP
		ring = minf(RING, (RING_ROW_WIDTH - (n - 1) * gap) / maxf(1.0, n))
	var size := Vector2.ONE * ring
	var top := _house_rect().end.y + BADGE_GAP_Y
	for i in n:
		var sp: String = ids[i]
		var cx := 540.0 + (i - (n - 1) * 0.5) * (ring + gap)
		var node := Control.new()
		node.name = "Goal " + sp
		node.mouse_filter = Control.MOUSE_FILTER_IGNORE
		UI.place(node, Rect2(Vector2(cx - ring * 0.5, top), size))
		_hud.add_child(node)
		# Cream backing disc that also clips the face to the ring's hole.
		var hole := TextureRect.new()
		hole.texture = _disc_texture()
		hole.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
		hole.mouse_filter = Control.MOUSE_FILTER_IGNORE
		hole.self_modulate = Color(1.0, 0.95, 0.84)
		hole.clip_children = CanvasItem.CLIP_CHILDREN_AND_DRAW
		var hole_size := size * RING_HOLE_RATIO
		UI.place(hole, Rect2((size - hole_size) * 0.5, hole_size))
		node.add_child(hole)
		var face_size := size * FACE_RATIO
		UI.art(hole, sp + "_face", Rect2((hole_size - face_size) * 0.5 + Vector2(0, hole_size.y * 0.04), face_size))
		UI.art(node, "badge_ring_gold", Rect2(Vector2.ZERO, size))
		var badge := Rect2(size * (1.0 - ABILITY_RATIO) + Vector2(4, 4), size * ABILITY_RATIO)
		_goal_gift[sp] = UI.art(node, "gift_coin_" + GIFT_COIN[sp], badge)
		_goal_check[sp] = UI.art(node, "badge_check", badge)
		_goal_check[sp].visible = false
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

func _build_tools() -> void:
	for child in _tool_bar.get_children():
		child.queue_free()
	var tools := _visible_tools()
	_tool_slots.clear()
	if tools.is_empty():
		return
	var n := tools.size()
	var width := n * TOOL_DISC + (n - 1) * TOOL_GAP + 2.0 * TOOL_BAR_PAD
	var bar := Rect2(540.0 - width * 0.5, TOOL_BAR_BOTTOM - TOOL_BAR_HEIGHT, width, TOOL_BAR_HEIGHT)
	_pill(_tool_bar, bar)
	for i in n:
		var kind: int = tools[i]
		var r := Rect2(bar.position.x + TOOL_BAR_PAD + i * (TOOL_DISC + TOOL_GAP),
			bar.position.y + (TOOL_BAR_HEIGHT - TOOL_DISC) * 0.5, TOOL_DISC, TOOL_DISC)
		_tool_slots.append([kind, r.get_center(), kind < 0 or session.state.inventory[kind] == 0 or session.booster_has_effect(kind)])
		if kind < 0:
			_disc_glyph(UI.disc(_tool_bar, "Undo", r, "", _on_undo), "icon_undo")
			continue
		var b := UI.disc(_tool_bar, "Tool %d" % kind, r, "", func(): _on_tool(kind))
		# Firefly and sapling are flat glyphs; the world tools are small illustrations and read better a little larger.
		_disc_glyph(b, TOOL_ICON[kind], GLYPH_RATIO if kind <= Board.Booster.SAPLING else 0.62)
		var stock := session.state.inventory[kind]
		# Red badge centred on the disc rim at its top-right (45 degrees).
		var rim := Vector2.ONE * TOOL_DISC * 0.5 + Vector2(1, -1) * TOOL_DISC * 0.5 * 0.7071
		var badge := Rect2(rim - Vector2.ONE * STOCK_BADGE * 0.5, Vector2.ONE * STOCK_BADGE)
		UI.art(b, "badge_red", badge)
		UI.number(b, str(stock) if stock > 0 else "+", Rect2(badge.position.x + 6, badge.get_center().y - STOCK_CAP * 0.5, STOCK_BADGE - 12, STOCK_CAP), "white")
		UI.set_enabled(b, stock == 0 or session.booster_has_effect(kind))

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
	var list: Array = session.state.items[b]
	for i in range(list.size() - 1, -1, -1):
		if (list[i] & Board.EGG) == 0 and session.state.is_asleep(list[i]):
			return _birds.get(Board.id_of(list[i]))
	return null

## Cream thought bubble above a sleeper; the pictures say what wakes it (the moon for night owls, the sun note otherwise).
func _show_bubble(bird: BirdView) -> void:
	_hide_bubble()
	var icons: Array[String] = []
	if Board.is_nocturnal(Board.species_of(bird.code)) and session.state.nocturnal_day_sleep():
		icons.append("dial_moon")
	if (bird.code & Board.ASLEEP) != 0:
		icons.append("gift_sun_note")
	if icons.is_empty():
		return
	var n := icons.size()
	var body_w := 2.0 * BUBBLE_PAD + n * HUD_ICON + (n - 1) * BUBBLE_ICON_GAP
	_bubble = Node2D.new()
	_bubble.name = "SleepBubble"
	# Nine-sliced at a fixed art scale so the tail and round ends never stretch.
	var art := NinePatchRect.new()
	art.texture = Art.tex("sleep_bubble")
	art.patch_margin_left = BUBBLE_MARGINS[0]
	art.patch_margin_top = BUBBLE_MARGINS[1]
	art.patch_margin_right = BUBBLE_MARGINS[2]
	art.patch_margin_bottom = BUBBLE_MARGINS[3]
	art.mouse_filter = Control.MOUSE_FILTER_IGNORE
	art.size = Vector2(body_w / BUBBLE_SCALE, float(art.texture.get_height()))
	art.scale = Vector2.ONE * BUBBLE_SCALE
	art.position = -BUBBLE_TAIL * BUBBLE_SCALE   # the tail tip sits at the node origin
	_bubble.add_child(art)
	var centre_y := art.position.y + BUBBLE_BODY_CENTRE_Y * BUBBLE_SCALE
	for i in n:
		var x := art.position.x + BUBBLE_PAD + i * (HUD_ICON + BUBBLE_ICON_GAP)
		UI.art(_bubble, icons[i], Rect2(x, centre_y - HUD_ICON * 0.5, HUD_ICON, HUD_ICON))
	# Tail tip just above the bird's head; keep the bubble body on screen.
	var tip := bird.position + Vector2(-10.0, -135.0 * bird.density)
	tip.x = clampf(tip.x, HUD_LEFT + BUBBLE_TAIL.x * BUBBLE_SCALE, 1040.0 - body_w + BUBBLE_TAIL.x * BUBBLE_SCALE)
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
	var glow_size := Vector2.ONE * TOOL_DISC * 1.375
	var glow := UI.image(_stuck, "glow_soft", Rect2(centre - glow_size * 0.5, glow_size))
	glow.modulate = Color(1, 0.72, 0.15, 0.3)   # translucent gold so the disc stays readable
	# Pointer 16 px above the disc (and its stock badge), pointing down; it bobs upwards so it never touches.
	var arrow_rect := Rect2(centre + Vector2(-HUD_ICON * 0.5, -TOOL_DISC * 0.5 - 16.0 - HUD_ICON), Vector2.ONE * HUD_ICON)
	var arrow := UI.art(_stuck, "icon_next_arrow", arrow_rect)
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
		Sound.play(BOOSTER_SOUND[kind])
		await play_events(session.events.duplicate())
	else:
		Sound.play("error")
	_layout_branches()
	await sync_birds()
	refresh_hud()
	busy = false
	_watch_stuck()

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
		bird.fly_to(_door_point() + Vector2(0, 40), k * 0.12, 0.6, true)
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
	tw.tween_property(coin, "scale", Vector2.ONE * RING / float(coin.texture.get_width()), 0.25).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
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
