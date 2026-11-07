class_name BirdView
extends Node2D
## One bird (or egg) drawn from pose frames. Origin = feet on the perch line.
## Always alive: continuous breathing plus weighted idle actions; never a mirrored still.
## Every texture change goes through _show(): consecutive painted frames crossfade, actions blend into and out
## of the rest frame, and an equipped outfit is drawn as its painted overlay following measured anchors per frame
## (birds/outfit_anchors.json, tools/birds/measure_outfit_anchors.py), so it stays on through every action.
## Idle actions are coordinated: one bird per branch at a time, and starts are spread across the screen.

signal finished

const CANVAS := 320.0
const FEET_Y := 300.0
const DISPLAY := 170.0
## Idle actions: blink, turn, preen, flap, hop, then one crossfaded still pose each (POSES).
const POSES := ["fluff", "stretch", "yawn", "tilt_left", "tilt_right", "look_up", "look_down", "quirk1", "quirk2"]
const IDLE_WEIGHTS := [30, 14, 10, 8, 6, 4, 4, 4, 3, 3, 3, 3, 3, 3]
const ACTION_NAMES := ["blink", "turn", "preen", "flap", "hop"]
const POSE_FIRST := 5
## Fraction of each painted frame's time spent crossfading into the next one.
const FADE := 0.5
## Idle coordination: minimum time between any two idle starts, and the pause a branch keeps after an action.
const START_GAP := 0.22
const BRANCH_REST := 0.3
const ANCHORS_PATH := "res://birds/outfit_anchors.json"
const NO_ANCHOR := [0.0, 0.0, 1.0, 1.0, 1.0, 0.0]
## Family overlay placement on the 320 canvas: [bow x, bow y, fluff x, fluff y, bow clockwise degrees].
## Only sunny and sky carry mama/chick birds; other species use the legacy anchors.
const OVERLAY_SIZE := {"bow": 72.0, "fluff": 88.0}
const OVERLAY_POSES := {
	"sunny": {"idle": [104, 140, 165, 244, 0], "tilt_left": [94, 164, 165, 244, -20], "tilt_right": [121, 124, 165, 244, 20]},
	"sky": {"idle": [215, 123, 163, 243, 0], "tilt_left": [193, 112, 163, 243, -20], "tilt_right": [232, 142, 163, 243, 20],
		"quirk2": [224, 136, 163, 243, 15], "look_down": [215, 140, 163, 243, 0], "preen": [198, 132, 179, 255, 8]},
}
const LEGACY_POSE := [168, 51, 120, 165, 0]

static var _anchors: Dictionary = {}
static var _anchors_loaded := false
static var _clock_frame := -1
static var _now := 0.0
static var _last_start := -10.0
static var _branch_free := {}   # branch key -> shared clock time when its next bird may start an idle action
static var _log := false

var code := 0
var species := "sunny"
var density := 1.0
var asleep := false
var is_egg := false
var outfit := ""          # outfit key, e.g. "sunny_outfit_1"
var acting := false
var selected := false
var quiet := false         # true while instant captures run

var _body: Node2D
var _sprite: Sprite2D
var _blend: Sprite2D      # crossfade layer: the next frame fading in over _sprite
var _wear: Polygon2D      # equipped outfit overlay, drawn over both frames as two independently scaled halves
var _bow: Sprite2D
var _fluff: Sprite2D
var _zzz: Sprite2D
var _pose_names := {}     # Texture2D -> pose name, for family overlay calibration
var _frame_names := {}    # Texture2D -> painted frame name, for outfit anchors
var _wear_frames := {}    # frame name -> [tx, ty, sx_left, sx_right, sy, degrees, fit cost]
var _wear_pivot := Vector2.ZERO
var _blend_alpha := 0.0
var _nudge := -1.0        # 0..1 while a tapped sleeper reacts, -1 otherwise
var _rest: Texture2D
var _blink: Array[Texture2D] = []
var _seqs := {}
var _breath_period := 2.8
var _breath_phase := 0.0
var _next_idle := 0.0
var _idle_rise := 0.0
var _idle_landing := 0.0   # >0 squash (wider, shorter), <0 stretch
var _idle_tween: Tween
var _move_tween: Tween
var _selection_tween: Tween
var _hover_tween: Tween
var _settle_tween: Tween
var _last_action := -1
var _clock := 0.0
var _glance := 0


func setup(bird_code: int, effective_sleep: bool, scale_value: float, outfit_key := "") -> void:
	code = bird_code
	species = Board.SPECIES[Board.species_of(bird_code)]
	is_egg = (bird_code & Board.EGG) != 0
	asleep = effective_sleep and not is_egg
	density = scale_value
	outfit = outfit_key
	if _body == null:
		_body = Node2D.new()
		add_child(_body)
		_sprite = Sprite2D.new()
		_sprite.centered = false
		_sprite.offset = Vector2(-CANVAS * 0.5, -FEET_Y)
		_body.add_child(_sprite)
		_blend = Sprite2D.new()
		_blend.centered = false
		_blend.offset = _sprite.offset
		_sprite.add_child(_blend)
		_wear = Polygon2D.new()
		_wear.visible = false
		_sprite.add_child(_wear)
		_bow = _overlay("mama_bow")
		_fluff = _overlay("chick_fluff")
		_zzz = Sprite2D.new()
		_zzz.texture = Art.tex("zzz")
		_zzz.position = Vector2(90, -230)
		_zzz.scale = Vector2.ONE * 0.6
		_body.add_child(_zzz)
		_log = OS.get_cmdline_user_args().has("--bird-log")
	_body.scale = Vector2.ONE * DISPLAY / CANVAS * density
	_load_frames()
	_breath_period = randf_range(2.4, 3.2)
	_breath_phase = randf()
	_next_idle = randf_range(0.6, 3.0)
	_show_rest()

func _overlay(art_name: String) -> Sprite2D:
	var s := Sprite2D.new()
	s.texture = Art.tex(art_name)
	s.scale = Vector2.ONE * OVERLAY_SIZE["bow" if art_name == "mama_bow" else "fluff"] / 128.0
	s.visible = false
	_sprite.add_child(s)
	return s

static func _outfit_anchors(key: String) -> Dictionary:
	if not _anchors_loaded:
		_anchors_loaded = true
		var text := FileAccess.get_file_as_string(ANCHORS_PATH)
		var parsed: Variant = JSON.parse_string(text) if not text.is_empty() else null
		if parsed is Dictionary:
			_anchors = parsed
	return _anchors.get(key, {})

func _load_frames() -> void:
	var prefix := species
	_seqs.clear()
	_pose_names.clear()
	_frame_names.clear()
	_wear_frames = {}
	_wear.texture = null
	_blend.texture = null
	if is_egg:
		_rest = Art.tex("egg_idle")
		return
	# Every frame is the plain painted species frame; an outfit is its overlay on top (the painted outfit front
	# and blinks are exactly these composites). Only the cheer has its own painted outfit frames.
	_rest = Art.tex(prefix + "_front")
	_blink = [_rest, Art.tex(prefix + "_front_blink_01"), Art.tex(prefix + "_front_blink_02")]
	_frame_names[_rest] = "front"
	for i in range(1, 3):
		if _blink[i]:
			_frame_names[_blink[i]] = "front_blink_%02d" % i
	for name in ["turn_left", "turn_right", "preen", "flap", "hop", "cheer", "flight_takeoff", "flight_flap", "flight_land",
			"flight_takeoff_l", "flight_flap_l", "flight_land_l"]:
		var frames := Art.frames(prefix + "_" + name)
		_seqs[name] = frames
		for i in frames.size():
			_frame_names[frames[i]] = "%s_%02d" % [name, i + 1]
	for pose in POSES + ["preen"]:
		var t := Art.tex(prefix + "_" + pose)
		if t:
			_pose_names[t] = pose
			_frame_names[t] = pose
	var overlay := Art.tex(outfit + "_front_overlay") if not outfit.is_empty() else null
	if overlay:
		var table := _outfit_anchors(outfit)
		_wear.texture = overlay
		_wear_frames = table.get("frames", {})
		var pivot: Array = table.get("pivot", [CANVAS * 0.5, FEET_Y * 0.5])
		_wear_pivot = Vector2(pivot[0], pivot[1])
		# Two quads split at the pivot column, textured with the overlay canvas 1:1.
		_wear.uv = PackedVector2Array([Vector2(0, 0), Vector2(_wear_pivot.x, 0), Vector2(CANVAS, 0),
			Vector2(0, CANVAS), Vector2(_wear_pivot.x, CANVAS), Vector2(CANVAS, CANVAS)])
		_wear.polygons = [PackedInt32Array([0, 1, 4, 3]), PackedInt32Array([1, 2, 5, 4])]
		_wear.polygon = _wear.uv
		var cheer := Art.frames(outfit + "_cheer")
		if not cheer.is_empty():
			for t in _seqs["cheer"]:
				_frame_names.erase(t)
			_seqs["cheer"] = cheer   # painted with the outfit: drawn without the overlay

## Show `base` with `next` crossfading over it by `mix` (0..1); the base fades out in the second half so parts
## that exist only in the old frame never pop at the swap. Overlays follow the shown frames at once.
func _show(base: Texture2D, next: Texture2D = null, mix := 0.0) -> void:
	_sprite.texture = base
	_blend.texture = next
	_blend_alpha = clampf(mix, 0.0, 1.0) if next else 0.0
	_blend.modulate.a = _blend_alpha
	_sprite.self_modulate.a = 1.0 - smoothstep(0.5, 1.0, _blend_alpha)
	_sync_overlays()

## Continuous playback position `pos` (0 .. size-1) through `frames`, crossfading each frame into the next.
func _play(frames: Array, pos: float) -> void:
	var last := frames.size() - 1
	pos = clampf(pos, 0.0, float(last))
	var i := mini(int(pos), last)
	if i >= last:
		_show(frames[last])
		return
	_show(frames[i], frames[i + 1], smoothstep(1.0 - FADE, 1.0, pos - i))

func _rest_frame() -> Texture2D:
	if not is_egg and asleep and _blink.size() > 2 and _blink[2]:
		return _blink[2]
	return _rest

func _show_rest() -> void:
	_show(_rest_frame())
	_zzz.visible = asleep

## Crossfade from whatever is shown back to the rest frame.
func _settle(duration := 0.12) -> void:
	if _settle_tween:
		_settle_tween.kill()
		_settle_tween = null
	var from := _blend.texture if _blend.texture and _blend_alpha >= 0.5 else _sprite.texture
	var to := _rest_frame()
	if from == null or from == to or quiet:
		_show_rest()
		return
	_settle_tween = create_tween()
	_settle_tween.tween_method(func(t: float): _show(from, to, t), 0.0, 1.0, duration)
	_settle_tween.tween_callback(func():
		_settle_tween = null
		_show_rest())

func _pose_of(tex: Texture2D) -> Array:
	var table: Dictionary = OVERLAY_POSES.get(species, {})
	if table.is_empty():
		return LEGACY_POSE
	return table.get(_pose_names.get(tex, "idle"), table["idle"])

func _anchor_of(tex: Texture2D) -> Variant:
	if tex == null or not _frame_names.has(tex):
		return null
	return _wear_frames.get(_frame_names[tex], NO_ANCHOR)

## Keep the outfit, mama bow and chick fluff attached to whatever frame (or crossfade) is shown.
func _sync_overlays() -> void:
	_sync_wear()
	var bow_on := (code & Board.MAMA) != 0 and not is_egg
	var fluff_on := (code & Board.CHICK) != 0 and not is_egg
	_bow.visible = bow_on
	_fluff.visible = fluff_on
	if not (bow_on or fluff_on):
		return
	var pose := _pose_of(_sprite.texture)
	if _blend_alpha > 0.0 and _blend.texture:
		var to := _pose_of(_blend.texture)
		var mixed := []
		for i in 5:
			mixed.append(lerpf(pose[i], to[i], _blend_alpha))
		pose = mixed
	_bow.position = Vector2(pose[0] - CANVAS * 0.5, pose[1] - FEET_Y)
	_bow.rotation = deg_to_rad(pose[4])
	_fluff.position = Vector2(pose[2] - CANVAS * 0.5, pose[3] - FEET_Y)

func _sync_wear() -> void:
	if _wear.texture == null or is_egg:
		_wear.visible = false
		return
	var from: Variant = _anchor_of(_sprite.texture)
	var to: Variant = _anchor_of(_blend.texture) if _blend.texture and _blend_alpha > 0.0 else from
	var p: Array
	var alpha := 1.0
	if from != null and to != null:
		p = []
		for i in 6:
			p.append(lerpf(from[i], to[i], _blend_alpha))
	elif from != null:
		p = from           # fading into a frame painted with the outfit
		alpha = 1.0 - _blend_alpha
	elif to != null:
		p = to
		alpha = _blend_alpha
	else:
		_wear.visible = false
		return
	_wear.visible = alpha > 0.0
	_wear.modulate.a = alpha
	# Same mapping as tools/birds/measure_outfit_anchors.py: pivot + t + R * (half scale * dx, sy * dy).
	var origin := _wear_pivot + Vector2(p[0], p[1]) + _sprite.offset
	var turn := deg_to_rad(p[5])
	var points := PackedVector2Array()
	for v in _wear.uv:
		var d := v - _wear_pivot
		points.append(origin + Vector2(d.x * (p[2] if d.x < 0.0 else p[3]), d.y * p[4]).rotated(turn))
	_wear.polygon = points

## Shared clock for idle coordination, advanced once per frame by whichever bird processes first.
static func _tick(delta: float) -> float:
	var frame := Engine.get_process_frames()
	if frame != _clock_frame:
		_clock_frame = frame
		_now += delta
	return _now

func _branch_key() -> String:
	var parent := get_parent()
	return "%d:%d" % [parent.get_instance_id() if parent else 0, roundi(position.y)]

func _process(delta: float) -> void:
	_clock += delta
	var now := _tick(delta)
	_sync_overlays()
	if quiet:
		return
	# Breathing never stops: a gentle squash at the feet pivot, slower and smaller when asleep.
	var base := DISPLAY / CANVAS * density
	var breath := 0.5 * (1.0 - cos((_clock / _breath_period + _breath_phase) * TAU))
	var sx := 1.0 - 0.008 * breath
	var sy := 1.0 + 0.02 * breath
	if not asleep and not is_egg:
		sx *= 1.0 + 0.05 * _idle_landing
		sy *= 1.0 - 0.05 * _idle_landing
	var pulse := sin(maxf(_nudge, 0.0) * PI)
	if not acting:
		_body.scale = Vector2(sx * (1.0 + 0.05 * pulse), sy * (1.0 - 0.04 * pulse)) * base
		_body.position = Vector2(0, -(_idle_rise + 14.0 * pulse) * density)
	if asleep:
		var drift := fmod(_clock, 2.4) / 2.4
		_zzz.position = Vector2(90, -230 - 40 * drift)
		_zzz.modulate.a = 1.0 - drift
		_zzz.scale = Vector2.ONE * 0.6 * (1.0 + 0.7 * pulse)
		if _nudge >= 0.0:
			_zzz.modulate.a = 1.0
	if acting or selected or asleep or is_egg or _idle_tween or _hover_tween or _settle_tween:
		return
	_next_idle -= delta
	if _next_idle > 0.0:
		return
	# One bird per branch at a time, and never two starts together anywhere: wait a little and try again.
	var key := _branch_key()
	if now - _last_start < START_GAP or float(_branch_free.get(key, -1.0)) > now:
		_next_idle = randf_range(0.08, 0.3)
		return
	var action := _pick_idle()
	var duration := _start_idle(action)
	if action >= 0:
		_last_start = now
		_branch_free[key] = now + duration + BRANCH_REST

func _has_action(a: int) -> bool:
	match a:
		0: return _blink.size() >= 3 and _blink[1] != null
		1: return _seqs.get("turn_left", []).size() >= 4 and _seqs.get("turn_right", []).size() >= 4
		2: return _seqs.get("preen", []).size() >= 5
		3: return _seqs.get("flap", []).size() >= 6
		4: return _seqs.get("hop", []).size() >= 6
	return a >= POSE_FIRST and _pose_names.has(_pose_tex(a))

func _pose_tex(action: int) -> Texture2D:
	return Art.tex(species + "_" + POSES[action - POSE_FIRST])

func _pick_idle() -> int:
	var total := 0
	for i in IDLE_WEIGHTS.size():
		if i != _last_action and _has_action(i):
			total += IDLE_WEIGHTS[i]
	if total == 0:
		return -1
	var draw := randi() % total
	for i in IDLE_WEIGHTS.size():
		if i == _last_action or not _has_action(i):
			continue
		draw -= IDLE_WEIGHTS[i]
		if draw < 0:
			return i
	return -1

func _action_name(action: int) -> String:
	return ACTION_NAMES[action] if action < POSE_FIRST else POSES[action - POSE_FIRST]

## Starts idle `action`; returns its duration in seconds.
func _start_idle(action: int, direction := -1) -> float:
	if action < 0:
		_next_idle = randf_range(1.0, 3.0)
		return 0.0
	_last_action = action
	var twice := action == 0 and randf() < 0.2
	if direction < 0:
		direction = 0 if randf() < 0.5 else 1
	var hold := randf_range(0.6, 1.2) if action == 1 else (randf_range(0.8, 1.4) if action >= POSE_FIRST else 0.0)
	var duration: float = 0.0
	if action >= POSE_FIRST:
		duration = hold + 0.5
	else:
		duration = [0.32 * (2.0 if twice else 1.0) + (0.2 if twice else 0.0), 0.84 + hold, 1.05, 1.0, 0.82][action]
	if _log:
		print("BIRDLOG t=%.2f bird=%s@%d,%d branch=%s action=%s dur=%.2f" % [_now, species, roundi(global_position.x),
			roundi(global_position.y), _branch_key(), _action_name(action), duration])
	_idle_tween = create_tween()
	_idle_tween.tween_method(func(t: float): _idle_frame(action, direction, t * duration, duration, hold, twice), 0.0, 1.0, duration)
	_idle_tween.tween_callback(_end_idle)
	return duration

## Eased 0..1 progress that starts and ends gently but keeps moving in the middle.
static func _ease(u: float) -> float:
	u = clampf(u, 0.0, 1.0)
	return lerpf(u, smoothstep(0.0, 1.0, u), 0.6)

func _idle_frame(action: int, direction: int, elapsed: float, duration: float, hold: float, twice: bool) -> void:
	_idle_rise = 0.0
	_idle_landing = 0.0
	if action >= POSE_FIRST:
		# Still pose: smooth crossfade in, hold, crossfade out over the rest frame.
		var mix := smoothstep(0.0, 1.0, minf(elapsed, duration - elapsed) / 0.25)
		_show(_rest, _pose_tex(action), mix)
		_idle_rise = 2.0 * mix
		return
	match action:
		0:
			var blink_len := 0.32
			if twice and elapsed >= blink_len + 0.2:
				elapsed -= blink_len + 0.2
			elif elapsed >= blink_len:
				_show(_rest)
				return
			_play([_rest, _blink[1], _blink[2], _blink[1], _rest], 4.0 * elapsed / blink_len)
		1:
			var turn: Array = [_rest] + _seqs["turn_left" if direction == 0 else "turn_right"]
			var span := float(turn.size() - 1)
			if elapsed < 0.42:
				_play(turn, span * _ease(elapsed / 0.42))
			elif elapsed < 0.42 + hold:
				_show(turn[turn.size() - 1])
			else:
				_play(turn, span * (1.0 - _ease((elapsed - 0.42 - hold) / 0.42)))
		2:
			var preen: Array = [_rest] + _seqs["preen"] + [_rest]
			_play(preen, (preen.size() - 1) * _ease(elapsed / duration))
		3:
			var flap: Array = [_rest] + _seqs["flap"] + _seqs["flap"] + [_rest]
			var u := elapsed / duration
			_play(flap, (flap.size() - 1) * _ease(u))
			_idle_rise = 5.0 * pow(sin(PI * u), 2.0)
		4:
			_hop_pose(elapsed)

## Idle hop on the spot, 0.82 s: crouch, eased rise and fall through the painted hop frames, landing squash.
func _hop_pose(t: float) -> void:
	var hop: Array = [_rest] + _seqs["hop"] + [_rest]
	if t < 0.14:
		var u := t / 0.14
		_play(hop, u)
		_idle_landing = 0.7 * smoothstep(0.0, 1.0, u)
	elif t < 0.56:
		var v := (t - 0.14) / 0.42
		_play(hop, 1.0 + 4.0 * v)
		_idle_rise = 16.0 * sin(PI * v)
		# Out of the crouch into a slight stretch, relaxing at the top and stretching again before touchdown.
		_idle_landing = lerpf(0.7, -0.45, smoothstep(0.0, 0.3, v)) * (1.0 - smoothstep(0.3, 0.6, v)) \
			- 0.3 * smoothstep(0.75, 1.0, v)
	else:
		var w := clampf((t - 0.56) / 0.26, 0.0, 1.0)
		_play(hop, 5.0 + 2.0 * smoothstep(0.0, 1.0, w))
		_idle_landing = lerpf(-0.3, 0.8, smoothstep(0.0, 0.15, w)) * (1.0 - smoothstep(0.15, 1.0, w))

func _end_idle() -> void:
	_idle_tween = null
	_idle_rise = 0.0
	_idle_landing = 0.0
	_show_rest()
	_next_idle = randf_range(1.0, 3.0)

func _stop_idle() -> void:
	if _idle_tween:
		_idle_tween.kill()
		_idle_tween = null
	if _settle_tween:
		_settle_tween.kill()
		_settle_tween = null
	_idle_rise = 0.0
	_idle_landing = 0.0
	_show_rest()

## Glance toward an x position (used when a neighbour is selected). Neighbours start a moment apart.
func look_at_x(x: float) -> void:
	if acting or asleep or is_egg or selected or _hover_tween or not _has_action(1):
		return
	_glance += 1
	var ticket := _glance
	await get_tree().create_timer(randf_range(0.0, 0.22)).timeout
	if ticket != _glance or acting or asleep or selected or _hover_tween or not is_inside_tree():
		return
	_stop_idle()
	_start_idle(1, 0 if x < global_position.x else 1)


func set_selected(on: bool) -> void:
	if selected == on:
		return
	selected = on
	_glance += 1
	_stop_idle()
	if _selection_tween:
		_selection_tween.kill()
		_selection_tween = null
	var height := -34.0 / (DISPLAY / CANVAS) if on else 0.0
	if quiet:
		_sprite.position.y = height
		return
	if on and _hover_tween == null:
		var frames: Array = _seqs["flight_flap"]
		var lift: Array = [_rest_frame()] + _seqs.get("flight_takeoff", [])
		var since := _clock
		# Blend up through the takeoff frames, then keep flapping until released.
		_hover_tween = create_tween().set_loops()
		_hover_tween.tween_method(func(_t: float):
			var e := _clock - since
			if e < 0.12:
				_play(lift, e / 0.12 * (lift.size() - 1))
			else:
				_show(frames[int((e - 0.12) * 18.0) % frames.size()]),
			0.0, 1.0, 1.0)
	_selection_tween = create_tween().set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	_selection_tween.tween_property(_sprite, "position:y", height, 0.18)
	if not on:
		_selection_tween.tween_callback(func():
			_stop_hover()
			_settle(0.14))

func _stop_hover() -> void:
	if _hover_tween:
		_hover_tween.kill()
		_hover_tween = null

## Fly to `target` (parent space); home=true shrinks and frees the bird.
## With a doorway, target is its centre: approach in front, then recede inside its mask.
func fly_to(target: Vector2, delay := 0.0, duration := 0.45, home := false, leap := false, doorway: Control = null) -> void:
	_stop_idle()
	_glance += 1
	acting = true
	selected = false
	if _selection_tween:
		_selection_tween.kill()
		_selection_tween = null
	# Carry the lifted sprite offset into the flight origin instead of snapping to the perch.
	position += _sprite.position * _body.scale
	_sprite.position = Vector2.ZERO
	_zzz.visible = false
	if doorway:
		position += _body.position
		_body.position = Vector2.ZERO
	if delay > 0.0:
		await get_tree().create_timer(delay).timeout
	var start := position
	var left := target.x < start.x
	var base := DISPLAY / CANVAS * density
	var centre_offset := Vector2.DOWN * (FEET_Y - CANVAS * 0.5) * base
	var entry_scale := 0.4
	var approach_duration := duration
	if doorway:
		# Fit the full square frame inside the circular opening before changing parents.
		entry_scale = minf(doorway.size.x, doorway.size.y) * 0.66 / (DISPLAY * density)
		target += centre_offset * entry_scale
		approach_duration = duration * 0.75
	var takeoff: Array = _seqs.get("flight_takeoff_l" if left else "flight_takeoff", [])
	var flap: Array = _seqs.get("flight_flap_l" if left else "flight_flap", [])
	var land: Array = _seqs.get("flight_land_l" if left else "flight_land", [])
	var hovering := _hover_tween != null
	var from := _sprite.texture
	var crouch := create_tween()
	crouch.tween_method(func(t: float):
		_body.scale = Vector2(lerpf(1.0, 1.08, t), lerpf(1.0, 0.92, t)) * base
		if not hovering and not takeoff.is_empty():
			_show(from, takeoff[0], smoothstep(0.0, 1.0, t)), 0.0, 1.0, 0.06)
	await crouch.finished
	_stop_hover()
	Sound.play("flutter", randf_range(0.95, 1.08))
	var height := 215.0 if leap else 160.0
	if doorway:
		height = minf(height, (start.y - target.y) * 0.25)
	_move_tween = create_tween()
	_move_tween.tween_method(func(t: float):
		position = start.lerp(target, t) + Vector2.UP * (4.0 * t * (1.0 - t) * height)
		var elapsed := t * approach_duration
		if not flap.is_empty():
			if elapsed < 0.08 and not takeoff.is_empty():
				_play(takeoff + [flap[0]], elapsed / 0.08 * takeoff.size())
			elif t > 0.82 and doorway == null and not land.is_empty():
				_play(land, (t - 0.82) / 0.18 * (land.size() - 1))
			else:
				_show(flap[int(elapsed * 18.0) % flap.size()])
		else:
			_show(Art.tex(species + ("_fly_up" if int(elapsed / 0.07) % 2 == 0 else "_fly_down")))
		var vertical := (target.y - start.y) - 4.0 * (1.0 - 2.0 * t) * height
		_body.rotation = clampf(atan2(vertical, absf(target.x - start.x) + 1.0), -0.21, 0.21) * (1.0 if target.x >= start.x else -1.0)
		if doorway:
			_body.rotation *= 1.0 - smoothstep(0.7, 1.0, t)
		var shrink := 1.0
		if home:
			var progress := smoothstep(0.45, 1.0, t) if doorway else t
			shrink = lerpf(1.0, entry_scale, progress)
		_body.scale = Vector2.ONE * base * shrink, 0.0, 1.0, approach_duration)
	await _move_tween.finished
	_body.rotation = 0.0
	if doorway:
		reparent(doorway)
		var entry_duration := duration - approach_duration
		_move_tween = create_tween()
		_move_tween.tween_method(func(t: float):
			var shrink := entry_scale * lerpf(1.0, 0.15, smoothstep(0.0, 1.0, t))
			_body.scale = Vector2.ONE * base * shrink
			# Keep the body centred while its feet pivot moves inward.
			position = doorway.size * 0.5 + centre_offset * shrink
			_show(flap[int((approach_duration + t * entry_duration) * 18.0) % flap.size()])
			modulate.a = 1.0 - smoothstep(0.6, 1.0, t), 0.0, 1.0, entry_duration)
		await _move_tween.finished
	if home:
		acting = false
		finished.emit()
		queue_free()
		return
	# Landing squash eases out while the last landing frame crossfades into the rest pose.
	var last := _sprite.texture
	var rest := _rest_frame()
	var squash := create_tween()
	squash.tween_method(func(t: float):
		var k := 1.0 - smoothstep(0.0, 1.0, t)
		_body.scale = Vector2(1.0 + 0.15 * k, 1.0 - 0.15 * k) * base
		_show(last, rest, smoothstep(0.1, 0.9, t)), 0.0, 1.0, 0.16)
	await squash.finished
	acting = false
	_show_rest()
	finished.emit()

func hop_to(target: Vector2, duration := 0.35) -> void:
	_stop_idle()
	acting = true
	var start := position
	var hop: Array = [_rest] + _seqs.get("hop", []) + [_rest]
	var has_hop := hop.size() > 2
	if has_hop:
		duration = maxf(duration, (hop.size() - 2) / 10.0)
	var tw := create_tween()
	tw.tween_method(func(t: float):
		position = start.lerp(target, smoothstep(0.0, 1.0, t)) + Vector2.UP * (60.0 * sin(t * PI) if has_hop else 0.0)
		if has_hop:
			_play(hop, t * (hop.size() - 1)), 0.0, 1.0, duration)
	await tw.finished
	acting = false
	_show_rest()
	finished.emit()

func cheer() -> void:
	_stop_idle()
	var frames: Array = _seqs.get("cheer", [])
	if frames.is_empty():
		finished.emit()
		return
	acting = true
	var seq: Array = [_rest_frame()] + frames + [_rest_frame()]
	var tw := create_tween()
	tw.tween_method(func(t: float): _play(seq, (seq.size() - 1) * _ease(t)), 0.0, 1.0, (frames.size() + 1) / 10.0)
	await tw.finished
	acting = false
	_show_rest()
	finished.emit()

func hatch() -> void:
	_stop_idle()
	acting = true
	var start := position
	var crack := Art.tex("egg_crack")
	var shell := Art.tex("egg_shell")
	var base := DISPLAY / CANVAS * density
	is_egg = false
	code = Board.with_flags(code, Board.flags_of(code) & ~(Board.EGG | Board.REVEALED))
	_load_frames()
	var tw := create_tween()
	tw.tween_method(func(t: float):
		_show(crack if t < 0.3 else (shell if t < 0.5 else _rest))
		position = start + Vector2.UP * 45.0 * sin(t * PI)
		var w := sin(t * TAU)
		_body.scale = Vector2(1.0 + 0.12 * w, 1.0 - 0.12 * w) * base, 0.0, 1.0, 0.35)
	await tw.finished
	acting = false
	_show_rest()
	finished.emit()

func wobble() -> void:
	_stop_idle()
	acting = true
	var l := Art.tex("egg_wobble_l")
	var r := Art.tex("egg_wobble_r")
	var tw := create_tween()
	tw.tween_method(func(t: float): _show(l if t < 0.35 else (r if t < 0.75 else _rest)), 0.0, 1.0, 0.3)
	await tw.finished
	acting = false
	_show_rest()
	finished.emit()

func reveal() -> void:
	code |= Board.REVEALED
	var tw := create_tween()
	tw.tween_property(_sprite, "modulate", Color(1.15, 1.1, 0.9), 0.25)
	await tw.finished
	finished.emit()

func wake() -> void:
	asleep = false
	_nudge = -1.0
	code = Board.with_flags(code, Board.flags_of(code) & ~Board.ASLEEP)
	_zzz.visible = false
	var base := DISPLAY / CANVAS * density
	var closed := _sprite.texture
	acting = true
	var tw := create_tween()
	tw.tween_method(func(t: float):
		var w := sin(t * PI)
		_body.scale = Vector2(1.0 - 0.06 * w, 1.0 + 0.1 * w) * base
		_show(closed, _rest, smoothstep(0.2, 0.8, t)), 0.0, 1.0, 0.3)
	await tw.finished
	acting = false
	_show_rest()
	finished.emit()

## A tapped sleeper stirs: small bob, eyes half open, and a bigger Zzz pulse; then it dozes off again.
func nudge_asleep() -> void:
	if not asleep or is_egg or acting or _nudge >= 0.0:
		return
	var tw := create_tween()
	tw.tween_method(func(t: float):
		_nudge = t
		if _blink.size() > 2 and _blink[1]:
			_show(_blink[2], _blink[1], smoothstep(0.05, 0.2, t) * (1.0 - smoothstep(0.65, 0.85, t)))
		else:
			_show_rest(), 0.0, 1.0, 0.7)
	await tw.finished
	_nudge = -1.0
	_show_rest()

func set_sleep(value: bool) -> void:
	asleep = value and not is_egg
	_stop_idle()

## Static pose for instant captures (no tweens running).
func freeze() -> void:
	quiet = true
	if _selection_tween:
		_selection_tween.kill()
		_selection_tween = null
	_stop_hover()
	_stop_idle()
	_body.scale = Vector2.ONE * DISPLAY / CANVAS * density
	_body.position = Vector2.ZERO
