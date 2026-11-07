extends Control
## How to play: four illustrated steps in one modal. Each step is a small live scene built from the board's own art
## (the world 1 painting, its cut branch layers, the hanging house and live BirdView birds) with a pointing hand,
## looping by itself. Next (green) advances, the red close skips, the last step ends with Play.
## Shown once on the first open of level 1 (profile tutorial flag FLAG) and from the map's help button.

const FLAG := "tutorial.how_to_play"
const STEPS := 4
## Painted sentence lines per step (tools/kit/locales/en.json); lines of one step share one cap height.
const LINES := [["tutorial.pick"], ["tutorial.move.a"], ["tutorial.home.a"], ["tutorial.win.a"]]
## Modal content groups: live scene, sentence (up to three lines), step dots.
const STAGE_SIZE := Vector2(768, 720)
const TEXT_H := 72.0   # one short line per step
const TEXT_W := 784.0
const TEXT_CAP := 44.0
const LINE_H := TEXT_CAP * 208.0 / 128.0
const DOTS_H := 24.0
const DOT := 24.0
const DOT_GAP := 20.0
const STAGE_RADIUS := 40.0
## The scene is laid out in board pixels and scaled into the card.
const WORLD := 1
const K := 768.0 / 1080.0
const CROP_Y := 380.0               # painting rows shown: CROP_Y .. CROP_Y + STAGE_SIZE.y / K
const BIRD_SCALE := 1.10            # board bird scale (board_view.gd)
const SEAT_FIRST := 98.0
const SEAT_OUTWARD_SHIFT := 24.0
const END_CLEAR := 125.0
## Three painted branches: upper left, middle right, lower left (slot name, perch y in scene pixels).
const BRANCHES := [["board_w1_branch_L0", 470.0], ["board_w1_branch_R0", 690.0], ["board_w1_branch_L1", 910.0]]
const HOUSE := ["house_default", Rect2(122, 62, 778, 878), Rect2(313, 418, 378, 380), 70.0]
const HOUSE_RECT := Rect2(430, 24, 220, 248)
const HAND_TIP := Vector2(25, 9)    # fingertip in hand_pointer pixels
const HAND_SCALE := 0.8
const HAND_REST := Vector2(800, 900)
const STAR := 160.0
const STAR_GAP := 20.0
const STAR_CENTER := Vector2(540, 585)

var modal: Modal
var step := 0
var instant := false
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

func show_step(index: int) -> void:
	if _slots.is_empty():
		for s in JSON.parse_string(FileAccess.get_file_as_string("res://world/scene/board_w%d_slots.json" % WORLD))["slots"]:
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
	paint.atlas = Art.tex("board_w%d_bg" % WORLD)
	paint.region = Rect2(0, CROP_Y, 1080, _world.size.y)
	var bg := TextureRect.new()
	bg.texture = paint
	bg.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	bg.stretch_mode = TextureRect.STRETCH_SCALE
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	bg.size = _world.size
	_world.add_child(bg)
	_build_house()
	var branches := Node2D.new()
	branches.name = "Branches"
	_world.add_child(branches)
	for b in BRANCHES.size():
		var slot: Dictionary = _slots[BRANCHES[b][0]]
		var spr := Sprite2D.new()
		spr.texture = Art.tex(slot["name"])
		spr.centered = false
		spr.position = Vector2(slot["at"][0], float(slot["at"][1]) + _perch(b) - float(slot["perch"]))
		branches.add_child(spr)
	_birds_layer = Node2D.new()
	_birds_layer.name = "Birds"
	_world.add_child(_birds_layer)
	_tap_glow = Sprite2D.new()
	_tap_glow.texture = Art.tex("glow_soft")
	_tap_glow.modulate = Color(1, 0.95, 0.7, 0.0)
	_world.add_child(_tap_glow)
	_stars = []
	if step == 3:
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
	_set_birds(_start_birds())

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
	var ids: Array = LINES[step]
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
	return str(BRANCHES[b][0]).contains("_R")

## Seat centre on branch b, seat 0 nearest the screen edge (board_view.gd seat_x).
func _seat(b: int, i: int) -> Vector2:
	var slot: Dictionary = _slots[BRANCHES[b][0]]
	var tip := float(slot["tip"])
	var length := 1080.0 - tip if _right(b) else tip
	var pitch := clampf((length - SEAT_FIRST * BIRD_SCALE - END_CLEAR) / 3.0, 100.0 * BIRD_SCALE, 150.0 * BIRD_SCALE)
	var d := SEAT_FIRST * BIRD_SCALE + i * pitch - SEAT_OUTWARD_SHIFT
	return Vector2(1080.0 - d if _right(b) else d, _perch(b))

## Starting birds of each step, per branch from the edge outwards.
func _start_birds() -> Array:
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
			bird.setup(Board.bird(b * 8 + i, Board.species_index(layout[b][i]), 0), false, BIRD_SCALE)
			bird.position = _seat(b, i)
			_birds_layer.add_child(bird)
			if instant:
				bird.freeze()
			row.append(bird)
		_birds.append(row)

## The top run of one kind on branch b.
func _top_run(b: int) -> Array:
	var row: Array = _birds[b]
	var out := []
	if row.is_empty():
		return out
	var kind: String = row[-1].species
	for i in range(row.size() - 1, -1, -1):
		if row[i].species != kind:
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
	Sound.play("select")
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
			Sound.play("move")
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
	Sound.play("nest_home")
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
	await out.finished
	if not _alive(run): return
	_set_resident("")
	_set_birds(_start_birds())
	var back := _tw()
	back.tween_property(_birds_layer, "modulate:a", 1.0, 0.3)
	await back.finished

func _loop(run: int) -> void:
	await _wait(0.5)
	var cycle := 0
	while _alive(run):
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

func _stars_pop(run: int) -> void:
	_tw().tween_property(_stars[0], "modulate:a", 0.9, 0.3)
	for i in _stars.size() - 1:
		var s: Control = _stars[i + 1]
		s.modulate.a = 1.0
		s.scale = Vector2.ONE * 0.2
		var tw := _tw()
		tw.tween_property(s, "scale", Vector2.ONE, 0.32).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
		Sound.play("star_%d" % (i + 1))
		await _wait(0.28)
		if not _alive(run): return

## Silent captures: one telling moment of each step, without motion.
func _pose_still() -> void:
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

## Fake data for captures. States: tutorial_1 .. tutorial_4 (over the map, as from the help button).
static func preview(main: Control, state: String) -> void:
	main.open_map()
	var t := open(main.popups, Callable(), main.instant())
	t.show_step(clampi(int(state.trim_prefix("tutorial_")) - 1, 0, STEPS - 1))
