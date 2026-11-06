class_name BirdView
extends Node2D
## One bird (or egg) drawn from pose frames. Origin = feet on the perch line.
## Always alive: continuous breathing plus weighted idle actions every 1-3 s; never a mirrored still.

signal finished

const CANVAS := 320.0
const FEET_Y := 300.0
const DISPLAY := 170.0
## Idle actions: blink, turn, preen, flap, hop. Weights follow the approved cadence.
const IDLE_WEIGHTS := [30, 14, 10, 8, 6]

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
var _zzz: Sprite2D
var _rest: Texture2D
var _blink: Array[Texture2D] = []
var _seqs := {}
var _breath_period := 2.8
var _breath_phase := 0.0
var _next_idle := 0.0
var _idle_rise := 0.0
var _idle_landing := 0.0
var _idle_tween: Tween
var _move_tween: Tween
var _last_action := -1
var _clock := 0.0


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
		_zzz = Sprite2D.new()
		_zzz.texture = Art.tex("zzz")
		_zzz.position = Vector2(90, -230)
		_zzz.scale = Vector2.ONE * 0.6
		_body.add_child(_zzz)
	_body.scale = Vector2.ONE * DISPLAY / CANVAS * density
	_load_frames()
	_breath_period = randf_range(2.4, 3.2)
	_breath_phase = randf()
	_next_idle = randf_range(1.0, 3.0)
	_show_rest()

func _load_frames() -> void:
	var prefix := species
	_seqs.clear()
	if is_egg:
		_rest = Art.tex("egg_idle")
		if code & Board.REVEALED:
			_rest = Art.tex("egg_idle")
		return
	var front := outfit + "_front" if not outfit.is_empty() and Art.has(outfit + "_front") else prefix + "_front"
	_rest = Art.tex(front)
	var b1 := Art.tex(outfit + "_front_blink_01") if not outfit.is_empty() else null
	var b2 := Art.tex(outfit + "_front_blink_02") if not outfit.is_empty() else null
	_blink = [_rest, b1 if b1 else Art.tex(prefix + "_front_blink_01"), b2 if b2 else Art.tex(prefix + "_front_blink_02")]
	for name in ["turn_left", "turn_right", "preen", "flap", "hop", "cheer", "flight_takeoff", "flight_flap", "flight_land",
			"flight_takeoff_l", "flight_flap_l", "flight_land_l"]:
		_seqs[name] = Art.frames(prefix + "_" + name)

func _show_rest() -> void:
	if is_egg:
		_sprite.texture = _rest
	elif asleep:
		_sprite.texture = _blink[2] if _blink.size() > 2 and _blink[2] else _rest
	else:
		_sprite.texture = _rest
	_zzz.visible = asleep

func _process(delta: float) -> void:
	_clock += delta
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
	if not acting:
		_body.scale = Vector2(sx, sy) * base
		_body.position = Vector2(0, -_idle_rise * density)
	if asleep:
		var drift := fmod(_clock, 2.4) / 2.4
		_zzz.position = Vector2(90, -230 - 40 * drift)
		_zzz.modulate.a = 1.0 - drift
	if acting or selected or asleep or is_egg or _idle_tween:
		return
	_next_idle -= delta
	if _next_idle <= 0.0:
		_start_idle(_pick_idle())

func _has_action(a: int) -> bool:
	match a:
		0: return _blink.size() >= 3 and _blink[1] != null
		1: return _seqs.get("turn_left", []).size() >= 4 and _seqs.get("turn_right", []).size() >= 4
		2: return _seqs.get("preen", []).size() >= 5
		3: return _seqs.get("flap", []).size() >= 6
		4: return _seqs.get("hop", []).size() >= 6
	return false

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

func _start_idle(action: int, direction := -1) -> void:
	if action < 0:
		_next_idle = randf_range(1.0, 3.0)
		return
	_last_action = action
	var twice := action == 0 and randf() < 0.2
	if direction < 0:
		direction = 0 if randf() < 0.5 else 1
	var hold := randf_range(0.6, 1.2) if action == 1 else 0.0
	var duration: float = [4.0 / 12.0 * (2.0 if twice else 1.0) + (0.2 if twice else 0.0), 0.8 + hold, 5.0 / 8.0 + 0.4, 12.0 / 14.0, 0.6][action]
	_idle_tween = create_tween()
	_idle_tween.tween_method(func(t: float): _idle_frame(action, direction, t * duration, hold, twice), 0.0, 1.0, duration)
	_idle_tween.tween_callback(_end_idle)

func _idle_frame(action: int, direction: int, elapsed: float, hold: float, twice: bool) -> void:
	_idle_rise = 0.0
	_idle_landing = 0.0
	match action:
		0:
			var blink_len := 4.0 / 12.0
			if twice and elapsed >= blink_len + 0.2:
				elapsed -= blink_len + 0.2
			elif elapsed >= blink_len:
				_sprite.texture = _rest
				return
			var i := mini(4, int(elapsed * 12.0))
			i = i if i <= 2 else 4 - i
			_sprite.texture = _blink[i]
		1:
			var turn: Array = _seqs["turn_left" if direction == 0 else "turn_right"]
			if elapsed < 0.4:
				_sprite.texture = turn[mini(3, int(elapsed * 10.0))]
			elif elapsed < 0.4 + hold:
				_sprite.texture = turn[3]
			else:
				_sprite.texture = turn[maxi(0, 3 - int((elapsed - 0.4 - hold) * 10.0))]
		2:
			var preen: Array = _seqs["preen"]
			_sprite.texture = preen[mini(preen.size() - 1, int(elapsed * 8.0))]
		3:
			var flap: Array = _seqs["flap"]
			_sprite.texture = flap[mini(11, int(elapsed * 14.0)) % 6]
			var pulse := sin(fmod(elapsed * 14.0, 6.0) / 6.0 * PI)
			_idle_rise = 3.0 * pulse * pulse
		4:
			var hop: Array = _seqs["hop"]
			_sprite.texture = hop[mini(5, int(elapsed * 12.0))]
			if elapsed < 0.25:
				var u := elapsed / 0.25
				_idle_rise = 14.0 * (1.0 - (1.0 - u) * (1.0 - u))
			elif elapsed < 0.5:
				var u := (elapsed - 0.25) / 0.25
				_idle_rise = 14.0 * (1.0 - u * u)
			else:
				_idle_landing = 1.0 - smoothstep(0.0, 1.0, (elapsed - 0.5) / 0.1)

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
	_idle_rise = 0.0
	_idle_landing = 0.0
	_show_rest()

## Glance toward an x position (used when a neighbour is selected).
func look_at_x(x: float) -> void:
	if acting or asleep or is_egg or selected or not _has_action(1):
		return
	_stop_idle()
	_start_idle(1, 0 if x < global_position.x else 1)


func set_selected(on: bool) -> void:
	if selected == on:
		return
	selected = on
	_stop_idle()
	var tw := create_tween().set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	tw.tween_property(_sprite, "position:y", -34.0 / (DISPLAY / CANVAS) if on else 0.0, 0.18)

## Fly along an arc to `target` (parent space). home=true shrinks into a door and frees the bird.
func fly_to(target: Vector2, delay := 0.0, duration := 0.45, home := false, leap := false) -> void:
	_stop_idle()
	acting = true
	selected = false
	_sprite.position = Vector2.ZERO
	_zzz.visible = false
	if delay > 0.0:
		await get_tree().create_timer(delay).timeout
	var start := position
	var left := target.x < start.x
	var base := DISPLAY / CANVAS * density
	var crouch := create_tween()
	crouch.tween_property(_body, "scale", Vector2(1.08, 0.92) * base, 0.06)
	await crouch.finished
	Sound.play("flutter", randf_range(0.95, 1.08))
	var height := 215.0 if leap else 160.0
	var takeoff: Array = _seqs.get("flight_takeoff_l" if left else "flight_takeoff", [])
	var flap: Array = _seqs.get("flight_flap_l" if left else "flight_flap", [])
	var land: Array = _seqs.get("flight_land_l" if left else "flight_land", [])
	_move_tween = create_tween()
	_move_tween.tween_method(func(t: float):
		position = start.lerp(target, t) + Vector2.UP * (4.0 * t * (1.0 - t) * height)
		var elapsed := t * duration
		if not flap.is_empty():
			if elapsed < 0.08 and not takeoff.is_empty():
				_sprite.texture = takeoff[mini(takeoff.size() - 1, int(elapsed / 0.08 * takeoff.size()))]
			elif t > 0.82 and not land.is_empty():
				_sprite.texture = land[mini(land.size() - 1, int((t - 0.82) / 0.18 * land.size()))]
			else:
				_sprite.texture = flap[int(elapsed * 18.0) % flap.size()]
		var vertical := (target.y - start.y) - 4.0 * (1.0 - 2.0 * t) * height
		_body.rotation = clampf(atan2(vertical, absf(target.x - start.x) + 1.0), -0.21, 0.21) * (1.0 if target.x >= start.x else -1.0)
		var shrink := lerpf(1.0, 0.4, t) if home else 1.0
		_body.scale = Vector2.ONE * base * shrink, 0.0, 1.0, duration)
	await _move_tween.finished
	_body.rotation = 0.0
	if home:
		acting = false
		finished.emit()
		queue_free()
		return
	var squash := create_tween()
	squash.tween_method(func(t: float): _body.scale = Vector2(lerpf(1.15, 1.0, t), lerpf(0.85, 1.0, t)) * base, 0.0, 1.0, 0.10)
	await squash.finished
	acting = false
	_show_rest()
	finished.emit()

func hop_to(target: Vector2, duration := 0.35) -> void:
	_stop_idle()
	acting = true
	var start := position
	var hop: Array = _seqs.get("hop", [])
	if not hop.is_empty():
		duration = maxf(duration, hop.size() / 10.0)
	var tw := create_tween()
	tw.tween_method(func(t: float):
		position = start.lerp(target, smoothstep(0.0, 1.0, t)) + Vector2.UP * (60.0 * sin(t * PI) if not hop.is_empty() else 0.0)
		if not hop.is_empty():
			_sprite.texture = hop[mini(hop.size() - 1, int(t * hop.size()))], 0.0, 1.0, duration)
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
	var tw := create_tween()
	tw.tween_method(func(t: float): _sprite.texture = frames[mini(frames.size() - 1, int(t * frames.size()))], 0.0, 1.0, frames.size() / 10.0)
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
		_sprite.texture = crack if t < 0.3 else (shell if t < 0.5 else _rest)
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
	tw.tween_method(func(t: float): _sprite.texture = l if t < 0.35 else (r if t < 0.75 else _rest), 0.0, 1.0, 0.3)
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
	code = Board.with_flags(code, Board.flags_of(code) & ~Board.ASLEEP)
	_show_rest()
	var base := DISPLAY / CANVAS * density
	acting = true
	var tw := create_tween()
	tw.tween_method(func(t: float):
		var w := sin(t * PI)
		_body.scale = Vector2(1.0 - 0.06 * w, 1.0 + 0.1 * w) * base, 0.0, 1.0, 0.3)
	await tw.finished
	acting = false
	finished.emit()

func set_sleep(value: bool) -> void:
	asleep = value and not is_egg
	_stop_idle()

## Static pose for instant captures (no tweens running).
func freeze() -> void:
	quiet = true
	_stop_idle()
	_body.scale = Vector2.ONE * DISPLAY / CANVAS * density
	_body.position = Vector2.ZERO
