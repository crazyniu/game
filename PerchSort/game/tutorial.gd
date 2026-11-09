extends Control
## How to play: four illustrated steps in one modal. Each step is a small live scene built from the board's own art
## (the world 1 painting, its cut branch layers, the hanging house and live BirdView birds) with a pointing hand,
## looping by itself. Next (green) advances, the red close skips, the last step ends with Play.
## Shown once on the first open of level 1 (profile tutorial flag FLAG) and from the map's help button.
## Mechanic lessons reuse the same live scene: one looping step per mechanic (LESSONS), a short line and a green
## Got it pill, shown once on the first level that has the mechanic (profile flag lesson_flag(mechanic)).

const FLAG := "tutorial.how_to_play"
const STEPS := 4
## Painted sentence lines per step (tools/kit/locales/en.json); lines of one step share one cap height.
const LINES := [["tutorial.pick"], ["tutorial.move.a"], ["tutorial.home.a"], ["tutorial.win.a"]]
## Modal content groups: live scene, sentence (up to three lines), step dots.
const STAGE_SIZE := Vector2(704, 660)   # inside the wooden frame (Modal.INSET)
const TEXT_H := 72.0   # one short line per step
const TEXT_W := 720.0
const TEXT_CAP := 44.0
const LINE_H := TEXT_CAP * 208.0 / 128.0
const DOTS_H := 24.0
const DOT := 24.0
const DOT_GAP := 20.0
const STAGE_RADIUS := 40.0
## The scene is laid out in board pixels and scaled into the card.
const WORLD := 1
const K := 704.0 / 1080.0
const CROP_Y := 380.0               # painting rows shown: CROP_Y .. CROP_Y + STAGE_SIZE.y / K
const BIRD_SCALE := 1.10            # board bird scale (board_view.gd)
const SEAT_FIRST := 98.0
const SEAT_OUTWARD_SHIFT := 24.0
const END_CLEAR := 125.0
## Three painted branches: upper left, middle right, lower left (slot side and row, perch y in scene pixels).
const BRANCHES := [["L0", 470.0], ["R0", 690.0], ["L1", 910.0]]
const HOUSE := ["house_default", Rect2(122, 62, 778, 878), Rect2(313, 418, 378, 380), 70.0]
const HOUSE_RECT := Rect2(430, 24, 220, 248)
const HAND_TIP := Vector2(25, 9)    # fingertip in hand_pointer pixels
const HAND_SCALE := 0.8
const HAND_REST := Vector2(800, 900)
const STAR := 160.0
const STAR_GAP := 20.0
const STAR_CENTER := Vector2(540, 585)

## Mechanic lessons in the order a player meets them; level data decides where each one first appears.
const LESSONS: Array[String] = ["sleep", "mama", "egg", "crow", "twig", "swivel", "growing", "daynight"]
## Starting birds of each lesson per branch from the edge outwards (level tokens: "x:sleep", "egg:x", "x:mama").
const LESSON_BIRDS := {
	"sleep": [["berry", "berry", "berry:sleep"], ["sky", "berry"], ["sky", "sky"]],
	"mama": [["sunny:chick", "sky", "sky", "sunny:mama"], [], ["sky", "sky"]],
	"egg": [["egg:mint", "sky"], ["sky", "sky", "sky"], ["mint", "mint", "mint"]],
	"crow": [["sunny", "sunny", "sunny", "crow"], [], ["sunny"]],
	"twig": [["sky", "sky", "berry", "berry"], [], ["sky", "sky"]],
	"swivel": [["sky", "berry", "berry", "berry"], ["sky", "sky", "sky"], []],
	"growing": [["mint", "mint", "mint"], ["berry", "berry"], ["mint"]],
	"daynight": [["hoot", "hoot", "hoot", "sky"], ["sky", "sky"], ["hoot", "sky"]],
}
## Special branches of a lesson: branch -> [kind, seats].
const LESSON_BRANCHES := {
	"twig": {1: [Board.Kind.TWIG, 2]},
	"swivel": {0: [Board.Kind.SWIVEL, 4]},
	"growing": {1: [Board.Kind.GROWING, 2]},
}
const LESSON_DAY_LENGTH := 2
const GOT_IT := Vector2(360, 128)    # the same size as every Play pill
const NIGHT_TINT := Color(0.70, 0.76, 0.98)   # board_view.gd moonlight
const PHASE_RECT := Rect2(40, 36, 192, 104)    # board HUD day/night pill, in scene pixels

var modal: Modal
var step := 0
var instant := false
var lesson := ""              # mechanic of a lesson; empty for the how-to-play steps
var world := WORLD
var _art: Array = []          # per branch: the painted layer node, scaled for short branches
var _kinds: Array = []        # per branch: [kind, seats]
var _knot: Sprite2D
var _night_paint: TextureRect
var _branches: Node2D
var _phase_icons: Array = []  # sun, moon
var _phase_number: Control
var _night := false
var _layer: Control           # everything of the current step
var _world: Control           # scene in board pixels, scaled into the card
var _birds_layer: Node2D
var _house: Control
var _door_mask: TextureRect
var _resident: BirdView
var _hand: Sprite2D
var _tap_glow: Sprite2D
var _stars: Array = []
var _slots := {}
var _birds: Array = []        # per branch, bottom to top
var _buttons: Array = []
var _run := 0                 # bumps on every step change so old loops stop


## Opens the tutorial in `host` (the popup layer); `done` runs once after it closes by Play or by skipping.
static func open(host: Control, done := Callable(), instant_motion := false) -> Control:
	var m := Modal.open(host, "tutorial.title", [STAGE_SIZE.y, TEXT_H, DOTS_H], true, UI.PRIMARY, 0.0, instant_motion)
	var t: Control = load("res://game/tutorial.gd").new()
	t.name = "Tutorial"
	t.modal = m
	t.instant = instant_motion
	t.size = m.size
	t.mouse_filter = Control.MOUSE_FILTER_IGNORE
	m.content.add_child(t)
	if done.is_valid():
		m.closed.connect(done, CONNECT_ONE_SHOT)
	t.show_step(0)
	return t

## Opens the lesson of one mechanic over the board of `world_index`; `done` runs once after Got it.
static func open_lesson(host: Control, mechanic: String, world_index: int, done := Callable(), instant_motion := false) -> Control:
	if OS.get_cmdline_user_args().has("--autoplay"):
		print("LESSON mechanic=%s" % mechanic)   # test driver log (main.gd --autoplay)
	var m := Modal.open(host, "lesson.%s.title" % mechanic, [STAGE_SIZE.y, TEXT_H], false, GOT_IT.y, 0.0, instant_motion)
	var t: Control = load("res://game/tutorial.gd").new()
	t.name = "Lesson"
	t.modal = m
	t.instant = instant_motion
	t.lesson = mechanic
	t.world = clampi(world_index, 1, 4)
	t.size = m.size
	t.mouse_filter = Control.MOUSE_FILTER_IGNORE
	m.content.add_child(t)
	if done.is_valid():
		m.closed.connect(done, CONNECT_ONE_SHOT)
	t.show_step(0)
	return t

static func lesson_flag(mechanic: String) -> String:
	return "lesson." + mechanic

## Lesson mechanics present in a level definition, in LESSONS order.
static func mechanics_in(level_def: Dictionary) -> Array[String]:
	var found := {}
	for b in level_def.get("branches", []):
		var kind := str(b.get("kind", ""))
		if kind in ["twig", "swivel", "growing"]:
			found[kind] = true
		for token in b.get("birds", []):
			var t := str(token)
			if t.begins_with("egg:"):
				found["egg"] = true
			elif t.ends_with(":sleep"):
				found["sleep"] = true
			elif t.ends_with(":mama") or t.ends_with(":chick"):
				found["mama"] = true
			elif t == "crow":
				found["crow"] = true
	if int(level_def.get("dayLength", 0)) > 0:
		found["daynight"] = true
	var out: Array[String] = []
	for m in LESSONS:
		if found.has(m):
			out.append(m)
	return out

## The lowest level id whose data has the mechanic (levels play in id order), or 0.
static func first_level_with(mechanic: String) -> int:
	var first := 0
	for def in Levels.all():
		var id := int(def["id"])
		if (first == 0 or id < first) and mechanics_in(def).has(mechanic):
			first = id
	return first

## Lessons to show when `level_def` opens: mechanics whose first level this is and that the player has not seen.
static func due_lessons(player: Profile, level_def: Dictionary) -> Array[String]:
	var out: Array[String] = []
	for m in mechanics_in(level_def):
		if first_level_with(m) == int(level_def["id"]) and not player.has_tutorial(lesson_flag(m)):
			out.append(m)
	return out

func show_step(index: int) -> void:
	if _slots.is_empty():
		for s in JSON.parse_string(FileAccess.get_file_as_string("res://world/scene/board_w%d_slots.json" % world))["slots"]:
			_slots[s["name"]] = s
	step = clampi(index, 0, STEPS - 1)
	_run += 1
	if _layer:
		_layer.queue_free()
	_layer = Control.new()
	_layer.name = "Step %d" % (step + 1)
	_layer.size = size
	_layer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(_layer)
	var ys: Array = modal.get_meta("ys")
	_build_stage(Rect2(Vector2(540.0 - STAGE_SIZE.x * 0.5, ys[0]), STAGE_SIZE))
	_build_text(ys[1])
	if lesson.is_empty():
		_build_dots(ys[2])
	_build_actions()
	if instant:
		_pose_still()
	else:
		_layer.modulate.a = 0.0
		_tw().tween_property(_layer, "modulate:a", 1.0, 0.2)
		_loop(_run)

## ---------- layout ----------

func _build_stage(r: Rect2) -> void:
	# The card clips the scene to its rectangle (the doorway needs the only clip_children mask); a panel-coloured
	# frame rounds the corners and a thin rim sits on top.
	var card := Control.new()
	card.name = "Stage"
	card.mouse_filter = Control.MOUSE_FILTER_IGNORE
	card.clip_contents = true
	UI.place(card, r)
	_layer.add_child(card)
	_world = Control.new()
	_world.name = "Scene"
	_world.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_world.size = STAGE_SIZE / K
	_world.scale = Vector2.ONE * K
	card.add_child(_world)
	var paint := AtlasTexture.new()
	paint.atlas = Art.tex("board_w%d_bg" % world)
	paint.region = Rect2(0, CROP_Y, 1080, _world.size.y)
	var bg := TextureRect.new()
	bg.texture = paint
	bg.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	bg.stretch_mode = TextureRect.STRETCH_SCALE
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	bg.size = _world.size
	_world.add_child(bg)
	_night_paint = null
	if lesson == "daynight":
		var night := AtlasTexture.new()
		night.atlas = Art.tex("board_w%d_night_bg" % world)
		night.region = paint.region
		_night_paint = bg.duplicate()
		_night_paint.texture = night
		_night_paint.modulate.a = 0.0
		_world.add_child(_night_paint)
	_build_house()
	_branches = Node2D.new()
	_branches.name = "Branches"
	_world.add_child(_branches)
	_build_branches()
	_birds_layer = Node2D.new()
	_birds_layer.name = "Birds"
	_world.add_child(_birds_layer)
	_tap_glow = Sprite2D.new()
	_tap_glow.texture = Art.tex("glow_soft")
	_tap_glow.modulate = Color(1, 0.95, 0.7, 0.0)
	_world.add_child(_tap_glow)
	_stars = []
	if lesson == "daynight":
		_build_phase_chip()
	if lesson.is_empty() and step == 3:
		# Three earned stars pop in over a warm glow once every kind is home.
		var halo := UI.art(_world, "glow_soft", Rect2(STAR_CENTER - Vector2(380, 190), Vector2(760, 380)))
		halo.modulate = Color(1, 0.92, 0.6, 0.0)
		_stars = [halo] + UI.rating_stars(_world, STAR_CENTER, STAR, STAR_GAP, 3)
		for s in _stars:
			s.modulate.a = 0.0
	_hand = Sprite2D.new()
	_hand.name = "Hand"
	_hand.texture = Art.tex("hand_pointer")
	_hand.centered = false
	_hand.offset = -HAND_TIP
	_hand.scale = Vector2.ONE * HAND_SCALE
	_hand.position = HAND_REST
	_world.add_child(_hand)
	var frame := Panel.new()
	frame.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var corners := StyleBoxFlat.new()
	corners.draw_center = false
	corners.border_color = UI.FILL["cream"]
	corners.set_border_width_all(24)
	corners.set_expand_margin_all(24)
	corners.set_corner_radius_all(int(STAGE_RADIUS) + 24)
	corners.anti_aliasing = true
	frame.add_theme_stylebox_override("panel", corners)
	UI.place(frame, r)
	_layer.add_child(frame)
	var rim := Panel.new()
	rim.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var line := StyleBoxFlat.new()
	line.draw_center = false
	line.border_color = UI.RIM["cream"]
	line.set_border_width_all(6)
	line.set_corner_radius_all(int(STAGE_RADIUS))
	line.anti_aliasing = true
	rim.add_theme_stylebox_override("panel", line)
	UI.place(rim, r)
	_layer.add_child(rim)
	_night = false
	_set_birds(_start_birds())

## Painted branch layers anchored at the screen edge (board_view.gd): a short branch (twig, growing) is the same
## painting narrowed from the edge, a twig is also thinner, a swivel branch carries the knot.
func _build_branches() -> void:
	_art = []
	_kinds = []
	_knot = null
	var special: Dictionary = LESSON_BRANCHES.get(lesson, {})
	for b in BRANCHES.size():
		var kind: Array = special.get(b, [Board.Kind.NORMAL, 4]).duplicate()
		_kinds.append(kind)
		var slot := _slot_of(b)
		var right := _right(b)
		var node := Node2D.new()
		node.position = Vector2(1080.0 if right else 0.0, _perch(b) + 20.0 * BIRD_SCALE)
		_branches.add_child(node)
		var art := Node2D.new()
		art.name = "Art"
		node.add_child(art)
		var spr := Sprite2D.new()
		spr.texture = Art.tex(slot["name"])
		spr.centered = false
		spr.position = Vector2(slot["at"][0], float(slot["at"][1]) + _perch(b) - float(slot["perch"])) - node.position
		art.add_child(spr)
		art.scale = Vector2(_branch_scale(b, kind[1]), 0.75 if kind[0] == Board.Kind.TWIG else 1.0)
		_art.append(art)
		if kind[0] == Board.Kind.SWIVEL:
			_knot = Sprite2D.new()
			_knot.name = "Knot"
			_knot.texture = Art.tex("swivel_knot")
			_knot.scale = Vector2.ONE * 0.5 * BIRD_SCALE
			_knot.position = Vector2(-150.0 if right else 150.0, 10.0 * BIRD_SCALE)
			node.add_child(_knot)

## Width share of a branch painting that holds `seats` seats (board_view.gd _branch_width).
func _branch_scale(b: int, seats: int) -> float:
	var full := _length(b)
	return minf(full, SEAT_FIRST * BIRD_SCALE + (maxi(seats, 2) - 1) * _pitch(b) + END_CLEAR) / full

## The board's day/night pill: painted sun or moon and the moves until the switch.
func _build_phase_chip() -> void:
	var chip := UI.shape(_world, PHASE_RECT, "hud", PHASE_RECT.size.y * 0.5, 6.0)
	chip.name = "Phase"
	var face := (PHASE_RECT.size.y - 4.0) * 0.5
	_phase_icons = []
	for icon_name in ["sun", "moon"]:
		_phase_icons.append(UI.icon(chip, icon_name, Vector2(34.0 + 28.0, face), 56.0, UI.INK))
	var x := 34.0 + 56.0 + 16.0
	_phase_number = UI.number(chip, str(LESSON_DAY_LENGTH), Rect2(x, face + 26.0 - 52.0 * 126.0 / 128.0,
		PHASE_RECT.size.x - 34.0 - x, 52.0), Color.WHITE, -1)
	_show_phase(false, LESSON_DAY_LENGTH)

func _show_phase(night: bool, left: int) -> void:
	_phase_icons[0].visible = not night
	_phase_icons[1].visible = night
	UI.set_number(_phase_number, str(left))

## The board's hanging house on its rope, with a round doorway mask for flights home and the resident.
func _build_house() -> void:
	var content: Rect2 = HOUSE[1]
	var k := HOUSE_RECT.size.x / content.size.x
	var rope := Control.new()
	rope.clip_contents = true
	rope.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var rope_bottom := HOUSE_RECT.position.y + (float(HOUSE[3]) - content.position.y) * k
	UI.place(rope, Rect2(532, -40, 16, rope_bottom + 40))
	_world.add_child(rope)
	UI.art(rope, "goal_rope_single", Rect2(0, 0, 16, 1260.0 * 16.0 / 108.0), Rect2(6, 0, 108, 1260))
	_house = Control.new()
	_house.name = "House"
	_house.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(_house, HOUSE_RECT)
	_world.add_child(_house)
	UI.art(_house, HOUSE[0], Rect2(Vector2.ZERO, HOUSE_RECT.size), content)
	var door: Rect2 = HOUSE[2]
	_door_mask = TextureRect.new()
	_door_mask.texture = _disc_texture()
	_door_mask.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	_door_mask.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_door_mask.clip_children = CanvasItem.CLIP_CHILDREN_ONLY
	UI.place(_door_mask, Rect2((door.position - content.position) * k, door.size * k))
	_house.add_child(_door_mask)
	_resident = null

static var _disc: ImageTexture
static func _disc_texture() -> ImageTexture:
	if _disc == null:
		var img := Image.create(128, 128, false, Image.FORMAT_RGBA8)
		for y in 128:
			for x in 128:
				var edge := 64.0 - Vector2(x + 0.5 - 64.0, y + 0.5 - 64.0).length()
				img.set_pixel(x, y, Color(1, 1, 1, clampf(edge, 0.0, 1.0)))
		_disc = ImageTexture.create_from_image(img)
	return _disc

func _build_text(top: float) -> void:
	var ids: Array = LINES[step] if lesson.is_empty() else ["lesson.%s.line" % lesson]
	var cap := TEXT_CAP
	for id in ids:
		cap = minf(cap, TEXT_CAP * TEXT_W / UI.label_width(id, TEXT_CAP))
	var line_h := LINE_H * cap / TEXT_CAP
	var y := top + (TEXT_H - line_h * ids.size()) * 0.5
	for id in ids:
		UI.label(_layer, id, Rect2(540.0 - TEXT_W * 0.5, y, TEXT_W, line_h), cap, UI.INK)
		y += line_h

func _build_dots(top: float) -> void:
	var total := STEPS * DOT + (STEPS - 1) * DOT_GAP
	for i in STEPS:
		var dot := Panel.new()
		dot.name = "Dot"
		dot.mouse_filter = Control.MOUSE_FILTER_IGNORE
		var sb := StyleBoxFlat.new()
		sb.bg_color = UI.FILL["green"] if i == step else UI.FADED
		sb.set_corner_radius_all(int(DOT * 0.5))
		sb.anti_aliasing = true
		dot.add_theme_stylebox_override("panel", sb)
		UI.place(dot, Rect2(540.0 - total * 0.5 + i * (DOT + DOT_GAP), top, DOT, DOT))
		_layer.add_child(dot)

## Next (green round) on steps 1-3, Play (green pill) on the last; the panel's red close skips.
func _build_actions() -> void:
	for b in _buttons:
		if is_instance_valid(b):
			b.queue_free()
	if not lesson.is_empty():
		var at := Vector2(540.0 - GOT_IT.x * 0.5, modal.get_meta("actions_y"))
		_buttons = [UI.pill(modal.content, "GotIt", Rect2(at, GOT_IT), "green", "action.got_it",
			func(): modal.invoke(func(): pass))]
		return
	if step < STEPS - 1:
		_buttons = modal.actions([["green", "right", func(): show_step(step + 1)]], false)
	else:
		_buttons = modal.actions([["green", "play", func(): pass]], true)
	for b in _buttons:
		b.name = "Next" if step < STEPS - 1 else "Play"

## ---------- scene helpers ----------

func _perch(b: int) -> float:
	return BRANCHES[b][1]

func _right(b: int) -> bool:
	return str(BRANCHES[b][0]).begins_with("R")

func _slot_of(b: int) -> Dictionary:
	return _slots["board_w%d_branch_%s" % [world, BRANCHES[b][0]]]

## Painted wood length from the screen edge to the end of the tuft.
func _length(b: int) -> float:
	var tip := float(_slot_of(b)["tip"])
	return 1080.0 - tip if _right(b) else tip

func _pitch(b: int) -> float:
	return clampf((_length(b) - SEAT_FIRST * BIRD_SCALE - END_CLEAR) / 3.0, 100.0 * BIRD_SCALE, 150.0 * BIRD_SCALE)

## Seat centre on branch b, seat 0 nearest the screen edge (board_view.gd seat_x).
func _seat(b: int, i: int) -> Vector2:
	var d := SEAT_FIRST * BIRD_SCALE + i * _pitch(b) - SEAT_OUTWARD_SHIFT
	return Vector2(1080.0 - d if _right(b) else d, _perch(b))

## Starting birds of each step, per branch from the edge outwards.
func _start_birds() -> Array:
	if not lesson.is_empty():
		return LESSON_BIRDS[lesson]
	match step:
		0: return [["sky", "sunny", "sunny"], ["berry", "berry", "sky"], ["sky", "berry"]]
		1: return [["sky", "sunny", "sunny"], ["berry", "sunny"], []]
		2: return [["sky", "sunny"], ["sunny", "sunny", "sunny"], ["berry", "sky"]]
		_: return [["sunny", "sunny", "sunny"], ["sky", "sky", "sky"], ["sky", "sunny"]]

func _set_birds(layout: Array) -> void:
	for child in _birds_layer.get_children():
		child.queue_free()
	_birds = []
	for b in layout.size():
		var row := []
		for i in layout[b].size():
			var bird := BirdView.new()
			var code := _code(b * 8 + i, str(layout[b][i]))
			var asleep := (code & Board.ASLEEP) != 0 or (lesson == "daynight" and not _night
				and Board.is_nocturnal(Board.species_of(code)))
			bird.setup(code, asleep, BIRD_SCALE)
			bird.position = _seat(b, i)
			_birds_layer.add_child(bird)
			if instant:
				bird.freeze()
			row.append(bird)
		_birds.append(row)

## A bird code from a level token: "berry", "berry:sleep", "egg:mint", "sunny:mama", "sunny:chick".
static func _code(id: int, token: String) -> int:
	var parts := token.split(":")
	if parts[0] == "egg":
		return Board.bird(id, Board.species_index(parts[1]), Board.EGG)
	var flags := 0
	if parts.size() > 1:
		flags = {"sleep": Board.ASLEEP, "mama": Board.MAMA, "chick": Board.CHICK}.get(parts[1], 0)
	return Board.bird(id, Board.species_index(parts[0]), flags)

## The top run of one kind on branch b (awake birds; an egg or a sleeper ends it).
func _top_run(b: int) -> Array:
	var row: Array = _birds[b]
	var out := []
	if row.is_empty():
		return out
	var kind: String = row[-1].species
	for i in range(row.size() - 1, -1, -1):
		if row[i].species != kind or row[i].asleep or row[i].is_egg:
			break
		out.push_front(row[i])
	return out

## Where the fingertip points: the top bird of a branch, or the first free seat of an empty one.
func _point_at(b: int) -> Vector2:
	var row: Array = _birds[b]
	if row.is_empty():
		return _seat(b, 0) + Vector2(0, -24)
	return _seat(b, row.size() - 1) + Vector2(18, -78)

func _set_resident(species: String) -> void:
	if _resident and is_instance_valid(_resident):
		_resident.queue_free()
	_resident = null
	if species.is_empty():
		return
	_resident = BirdView.new()
	_resident.setup(Board.bird(0, Board.species_index(species), 0), false, 0.58)
	_door_mask.add_child(_resident)
	var feet := Vector2(_door_mask.size.x * 0.5, _door_mask.size.y * 0.97)
	_resident.position = feet
	if instant:
		_resident.freeze()
	else:
		_resident.position.y = feet.y + 90.0
		_tw().tween_property(_resident, "position:y", feet.y, 0.3).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)

## ---------- motion ----------

## Every step tween is bound to the step's layer: a new step or a closed tutorial frees the layer, its tweens die
## with it and the step's loop simply never resumes (no scene-tree timers).
func _tw() -> Tween:
	return _layer.create_tween()

func _wait(seconds: float) -> void:
	var tw := _tw()
	tw.tween_interval(seconds)
	await tw.finished

func _alive(run: int) -> bool:
	return run == _run and is_inside_tree()

func _hand_to(target: Vector2, seconds := 0.55) -> void:
	var tw := _tw().set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	tw.tween_property(_hand, "position", target, seconds)
	await tw.finished

## A press: the hand dips and squeezes, a soft glow rings the fingertip.
func _tap() -> void:
	_tap_glow.position = _hand.position
	_tap_glow.scale = Vector2.ONE * 0.5
	_tap_glow.modulate.a = 0.85
	var glow := _tw().set_parallel()
	glow.tween_property(_tap_glow, "scale", Vector2.ONE * 1.1, 0.35).set_ease(Tween.EASE_OUT)
	glow.tween_property(_tap_glow, "modulate:a", 0.0, 0.35)
	var tw := _tw()
	tw.tween_property(_hand, "scale", Vector2(0.92, 0.88) * HAND_SCALE, 0.08)
	tw.parallel().tween_property(_hand, "position:y", _hand.position.y + 10.0, 0.08)
	tw.tween_property(_hand, "scale", Vector2.ONE * HAND_SCALE, 0.14).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	tw.parallel().tween_property(_hand, "position:y", _hand.position.y, 0.14)
	pass   # the demo is silent; menus play only the music
	await tw.finished

## Taps branch `from` (its top run lifts with flapping frames), then branch `to` (the run flies there).
func _move(run: int, from: int, to: int) -> void:
	await _hand_to(_point_at(from))
	if not _alive(run): return
	await _tap()
	var group := _top_run(from)
	for bird in group:
		bird.set_selected(true)
	await _wait(0.7)
	if not _alive(run): return
	await _hand_to(_point_at(to) + (Vector2(0, -24) if _birds[to].is_empty() else Vector2.ZERO))
	if not _alive(run): return
	await _tap()
	var row_from: Array = _birds[from]
	var row_to: Array = _birds[to]
	var last: BirdView = null
	for k in group.size():
		var bird: BirdView = group[k]
		row_from.erase(bird)
		bird.fly_to(_seat(to, row_to.size()), 0.0, 0.45)
		row_to.append(bird)
		last = bird
		if k == 0:
			pass   # the demo is silent; menus play only the music
		await _wait(0.07)
		if not _alive(run): return
	if last and is_instance_valid(last):
		await last.finished

## A full branch of one kind flies into the doorway, the house bounces and the resident looks out.
func _fly_home(run: int, b: int) -> void:
	var group: Array = _birds[b].duplicate()
	_birds[b] = []
	var target := HOUSE_RECT.position + _door_mask.position + _door_mask.size * 0.5
	var last: BirdView = null
	for k in range(group.size() - 1, -1, -1):
		var bird: BirdView = group[k]
		bird.fly_to(target, 0.0, 0.6, true, false, _door_mask)
		last = bird
		await _wait(0.12)
		if not _alive(run): return
	if last and is_instance_valid(last):
		await last.finished
	if not _alive(run): return
	pass   # the demo is silent; menus play only the music
	_set_resident(group[0].species if not group.is_empty() else "")
	var tw := _tw()
	tw.tween_property(_house, "scale", Vector2(1.08, 0.94), 0.1)
	tw.tween_property(_house, "scale", Vector2.ONE, 0.25).set_trans(Tween.TRANS_BACK)
	await tw.finished

## Fades the birds out, puts the step's starting birds back and fades them in.
func _reset(run: int) -> void:
	var out := _tw().set_parallel()
	out.tween_property(_birds_layer, "modulate:a", 0.0, 0.25)
	if _resident and is_instance_valid(_resident):
		out.tween_property(_resident, "modulate:a", 0.0, 0.25)
	for s in _stars:
		out.tween_property(s, "modulate:a", 0.0, 0.25)
	out.tween_property(_hand, "position", HAND_REST, 0.4).set_trans(Tween.TRANS_SINE)
	# Lessons return to their first scene: day again, a grown branch back to its seats.
	if _night_paint:
		out.tween_property(_night_paint, "modulate:a", 0.0, 0.4)
		out.tween_property(_branches, "modulate", Color.WHITE, 0.4)
	var special: Dictionary = LESSON_BRANCHES.get(lesson, {})
	for b in special:
		if _kinds[b][1] != special[b][1]:
			_kinds[b][1] = special[b][1]
			out.tween_property(_art[b], "scale:x", _branch_scale(b, _kinds[b][1]), 0.4).set_trans(Tween.TRANS_SINE)
	await out.finished
	if not _alive(run): return
	if _night_paint:
		_night = false
		_birds_layer.modulate = Color(1, 1, 1, 0)
		_show_phase(false, LESSON_DAY_LENGTH)
	_set_resident("")
	_set_birds(_start_birds())
	var back := _tw()
	back.tween_property(_birds_layer, "modulate:a", 1.0, 0.3)
	await back.finished

func _loop(run: int) -> void:
	await _wait(0.5)
	var cycle := 0
	while _alive(run):
		if not lesson.is_empty():
			await _lesson_cycle(run)
			if not _alive(run): return
			await _wait(0.4)
			continue
		match step:
			0:
				await _hand_to(_point_at(0))
				if not _alive(run): return
				await _tap()
				for bird in _top_run(0):
					bird.set_selected(true)
				await _wait(1.6)
				if not _alive(run): return
				for bird in _top_run(0):
					bird.set_selected(false)
				await _hand_to(HAND_REST, 0.6)
				await _wait(0.6)
			1:
				# Alternate: onto the same bird on top, then onto an empty branch.
				await _move(run, 0, 1 if cycle % 2 == 0 else 2)
				if not _alive(run): return
				await _wait(1.0)
				if not _alive(run): return
				await _reset(run)
			2:
				await _move(run, 0, 1)
				if not _alive(run): return
				await _wait(0.35)
				if not _alive(run): return
				await _fly_home(run, 1)
				if not _alive(run): return
				await _wait(1.4)
				if not _alive(run): return
				await _reset(run)
			_:
				await _move(run, 2, 0)
				if not _alive(run): return
				await _fly_home(run, 0)
				if not _alive(run): return
				await _move(run, 2, 1)
				if not _alive(run): return
				await _fly_home(run, 1)
				if not _alive(run): return
				await _hand_to(HAND_REST, 0.5)
				await _stars_pop(run)
				if not _alive(run): return
				for bird in _birds_layer.get_children():
					if bird is BirdView:
						bird.cheer()
				if _resident and is_instance_valid(_resident):
					_resident.cheer()
				await _wait(1.8)
				if not _alive(run): return
				await _reset(run)
		cycle += 1
		await _wait(0.4)

## One pass of a mechanic lesson, then the scene resets. The rules shown follow core/rules.gd.
func _lesson_cycle(run: int) -> void:
	match lesson:
		"sleep":
			# A berry lands on the sleeping berry: it wakes, and the full branch flies home.
			await _move(run, 1, 0)
			if not _alive(run): return
			var sleeper: BirdView = _birds[0][2]
			sleeper.wake()
			await _wait(0.6)
			if not _alive(run): return
			await _fly_home(run, 0)
		"mama":
			# The mama flies to the empty branch and her chick leaps after her over the other birds.
			await _move(run, 0, 1)
			if not _alive(run): return
			await _chicks_follow(run, 0, 1)
		"egg":
			# The bird covering the egg moves away: the egg hatches; the full branch flies home.
			await _move(run, 0, 1)
			if not _alive(run): return
			var egg: BirdView = _birds[0][0]
			egg.hatch()
			await _wait(0.6)
			if not _alive(run): return
			await _fly_home(run, 1)
		"crow", "twig":
			# The crow (or the pair of berries) is parked on the free branch, the sunnies (skies) then fly home.
			await _move(run, 0, 1)
			if not _alive(run): return
			await _wait(0.4)
			if not _alive(run): return
			await _move(run, 2, 0)
			if not _alive(run): return
			await _fly_home(run, 0)
		"swivel":
			# Tapping the knot turns the branch end for end; the sky now on top joins its flock.
			await _swivel(run, 0)
			if not _alive(run): return
			await _wait(0.4)
			if not _alive(run): return
			await _move(run, 0, 1)
			if not _alive(run): return
			await _fly_home(run, 1)
		"growing":
			# A flock flies home and the short branch grows one seat.
			await _move(run, 2, 0)
			if not _alive(run): return
			await _fly_home(run, 0)
			if not _alive(run): return
			await _grow(run, 1)
		"daynight":
			# The pill counts the moves to night; owls sleep by day and wake when night falls.
			await _move(run, 0, 1)
			if not _alive(run): return
			_count_down(LESSON_DAY_LENGTH - 1)
			await _wait(0.4)
			if not _alive(run): return
			await _move(run, 2, 1)
			if not _alive(run): return
			await _fly_home(run, 1)
			if not _alive(run): return
			await _nightfall(run)
			if not _alive(run): return
			await _move(run, 2, 0)
			if not _alive(run): return
			await _fly_home(run, 0)
	if not _alive(run): return
	await _hand_to(HAND_REST, 0.5)
	if not _alive(run): return
	await _wait(1.4)
	if not _alive(run): return
	await _reset(run)

## Chicks of the mama's kind leave branch `from` from the tip side and leap to the seats after her.
func _chicks_follow(run: int, from: int, to: int) -> void:
	var row_from: Array = _birds[from]
	var row_to: Array = _birds[to]
	var kind: String = row_to[-1].species
	for i in range(row_from.size() - 1, -1, -1):
		var bird: BirdView = row_from[i]
		if (bird.code & Board.CHICK) == 0 or bird.species != kind:
			continue
		row_from.remove_at(i)
		bird.fly_to(_seat(to, row_to.size()), 0.0, 0.55, false, true)
		row_to.append(bird)
		await _wait(0.75)
		if not _alive(run): return

## The hand taps the knot; the birds hop to the mirrored seats (seat order reversed, never a mirrored sprite).
func _swivel(run: int, b: int) -> void:
	await _hand_to(_knot.get_parent().position + _knot.position + Vector2(0, 6))
	if not _alive(run): return
	await _tap()
	var base := _knot.scale
	var pulse := _tw()
	pulse.tween_property(_knot, "scale", base * 1.25, 0.1)
	pulse.tween_property(_knot, "scale", base, 0.25).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	var row: Array = _birds[b]
	row.reverse()
	for i in row.size():
		row[i].hop_to(_seat(b, i), 0.4)
	await _wait(0.6)

## The growing branch gains a seat: its painting widens and a soft glow marks the new seat.
func _grow(run: int, b: int) -> void:
	_kinds[b][1] += 1
	var tw := _tw()
	tw.tween_property(_art[b], "scale:x", _branch_scale(b, _kinds[b][1]), 0.6).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	_tap_glow.position = _seat(b, _kinds[b][1] - 1) + Vector2(0, -40)
	_tap_glow.scale = Vector2.ONE * 0.6
	_tap_glow.modulate.a = 0.0
	var glow := _tw()
	glow.tween_property(_tap_glow, "modulate:a", 0.9, 0.3).set_delay(0.2)
	glow.parallel().tween_property(_tap_glow, "scale", Vector2.ONE * 1.4, 0.6).set_delay(0.2)
	glow.tween_property(_tap_glow, "modulate:a", 0.0, 0.5)
	await glow.finished

func _count_down(left: int) -> void:
	UI.set_number(_phase_number, str(left))
	_phase_number.pivot_offset = _phase_number.size * 0.5
	var tw := _tw()
	tw.tween_property(_phase_number, "scale", Vector2.ONE * 1.3, 0.12)
	tw.tween_property(_phase_number, "scale", Vector2.ONE, 0.2)

## Night falls: the world's night painting cross-fades in under moonlight, the pill turns to the moon and the
## owls wake.
func _nightfall(run: int) -> void:
	_night = true
	var tw := _tw().set_parallel()
	tw.tween_property(_night_paint, "modulate:a", 1.0, 0.8).set_trans(Tween.TRANS_SINE)
	tw.tween_property(_branches, "modulate", NIGHT_TINT, 0.8).set_trans(Tween.TRANS_SINE)
	tw.tween_property(_birds_layer, "modulate", NIGHT_TINT, 0.8).set_trans(Tween.TRANS_SINE)
	_show_phase(true, LESSON_DAY_LENGTH)
	_count_down(LESSON_DAY_LENGTH)
	await tw.finished
	if not _alive(run): return
	for row in _birds:
		for bird in row:
			if bird.asleep and Board.is_nocturnal(Board.species_of(bird.code)):
				bird.wake()
	await _wait(0.7)

func _stars_pop(run: int) -> void:
	_tw().tween_property(_stars[0], "modulate:a", 0.9, 0.3)
	for i in _stars.size() - 1:
		var s: Control = _stars[i + 1]
		s.modulate.a = 1.0
		s.scale = Vector2.ONE * 0.2
		var tw := _tw()
		tw.tween_property(s, "scale", Vector2.ONE, 0.32).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
		pass   # the demo is silent; menus play only the music
		await _wait(0.28)
		if not _alive(run): return

## Silent captures: one telling moment of each step, without motion.
func _pose_still() -> void:
	if not lesson.is_empty():
		_pose_lesson()
		return
	match step:
		0:
			for bird in _top_run(0):
				bird.set_selected(true)
			_hand.position = _point_at(0)
		1:
			for bird in _top_run(0):
				bird.set_selected(true)
			_hand.position = _point_at(1)
		2:
			_set_birds([["sky"], ["sunny", "sunny", "sunny", "sunny"], ["berry", "sky"]])
			_hand.position = _point_at(1)
		_:
			_set_birds([[], [], []])
			_set_resident("sky")
			for s in _stars:
				s.modulate.a = 1.0
			_stars[0].modulate.a = 0.9
			_hand.position = HAND_REST

func _select(b: int) -> void:
	for bird in _top_run(b):
		bird.set_selected(true)

func _pose_lesson() -> void:
	match lesson:
		"sleep", "egg", "daynight":
			_select(1 if lesson == "sleep" else 0)
			_hand.position = _point_at(0 if lesson == "sleep" else 1)
		"mama":
			_set_birds([["sky", "sky"], ["sunny:mama", "sunny:chick"], ["sky", "sky"]])
			_hand.position = _point_at(1)
		"crow":
			_set_birds([["sunny", "sunny", "sunny"], ["crow"], ["sunny"]])
			_select(2)
			_hand.position = _point_at(0)
		"twig":
			_set_birds([["sky", "sky"], ["berry", "berry"], ["sky", "sky"]])
			_hand.position = _point_at(1)
		"swivel":
			_hand.position = _knot.get_parent().position + _knot.position + Vector2(0, 6)
		"growing":
			_set_birds([[], ["berry", "berry"], []])
			_kinds[1][1] = 3
			_art[1].scale.x = _branch_scale(1, 3)
			_set_resident("mint")
			_hand.position = HAND_REST

## Fake data for captures. States: tutorial_1 .. tutorial_4 (over the map, as from the help button) and
## lesson_<mechanic> (over the board of the mechanic's first level).
static func preview(main: Control, state: String) -> void:
	if state.begins_with("lesson_"):
		var mechanic := state.trim_prefix("lesson_")
		var def := Levels.get_level(maxi(1, first_level_with(mechanic)))
		var board := BoardView.new()
		board.instant = main.instant()
		main.set_screen(board)
		board.start(def, main.profile, false)
		open_lesson(main.popups, mechanic, Levels.world_of(def), Callable(), main.instant())
		return
	main.open_map()
	var t := open(main.popups, Callable(), main.instant())
	t.show_step(clampi(int(state.trim_prefix("tutorial_")) - 1, 0, STEPS - 1))
