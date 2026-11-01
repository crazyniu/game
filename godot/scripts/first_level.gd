extends Node2D
## Native Godot L01. Persistence confirms a candidate before logic or visual commit.
const Rules = preload("res://core/first_level_rules.gd")
const Store = preload("res://core/save_store.gd")
const Bird = preload("res://scripts/bird_actor.gd")
const ROOTS := [Vector2(337.8, 760), Vector2(742.2, 1020), Vector2(337.8, 1280)]
const FEET := [9.571823, 15.372928, 16.726519, 8.411603]
var session = Rules.Session.new()
var store
var record: Dictionary = {}
var pending: Dictionary = {}
var mode := "playing"
var selected := -1
var busy := false
var pending_pause := false
var focus_lost := false
var generation := 0
var actors: Dictionary = {}
var branches: Array[Sprite2D] = []
var motions: Array[Tween] = []
var content: Node2D
var ui: Control
var modal: Control
var modal_panel: Control
var moves_label: Label
var undo_button: Button
var restart_button: Button
var sound_button: Button
var music: AudioStreamPlayer
var audio: Dictionary = {}
var hud: Control
var font: Font = preload("res://assets/Fredoka-Medium.ttf")
var test_save_directory := ""
var settings_origin := "playing"
var audio_muted := false

func _ready() -> void:
	store = Store.new(test_save_directory) if not test_save_directory.is_empty() else Store.new()
	var loaded: Dictionary = store.load_record()
	record = loaded.record
	audio_muted = record.audio_muted
	session.restore(record.state, record.history)
	_build_world()
	_build_ui()
	_build_audio()
	_sync()
	get_viewport().size_changed.connect(_layout)
	_layout()
	if loaded.status == "readonly":
		mode = "readonly"
		_show_modal("Save unavailable", loaded.message, [["Home", _home]])
	elif Rules.is_won(session.snapshot()):
		_win()

func _sprite(path: String, parent: Node, at: Vector2, dimensions: Vector2, layer: int = 0) -> Sprite2D:
	var sprite := Sprite2D.new()
	sprite.texture = load(path)
	sprite.position = at
	sprite.scale = dimensions / sprite.texture.get_size()
	sprite.z_index = layer
	parent.add_child(sprite)
	return sprite

func _build_world() -> void:
	content = Node2D.new()
	add_child(content)
	_sprite("res://assets/art/forest_background.png", content, Vector2(540, 960), Vector2(1080, 1920), -20)
	for i in range(3):
		var right := i == 1
		var branch := _sprite("res://assets/art/perch.png", content, ROOTS[i] + Vector2(23.52 if right else -23.52, 17.5), Vector2(608.16, 140), 1)
		branch.flip_h = right
		branches.append(branch)
	for side in [-1, 1]:
		var rope := Line2D.new()
		rope.points = PackedVector2Array([Vector2(540 + side * 190, 10), Vector2(540 + side * 130, 320)])
		rope.width = 4.5
		rope.default_color = Color("50301a")
		content.add_child(rope)
	_sprite("res://assets/art/nest.png", content, Vector2(540, 320), Vector2(300, 300), 2)
	var rim := _sprite("res://assets/art/nest.png", content, Vector2(540, 320), Vector2(300, 300), 30)
	var shader := Shader.new()
	shader.code = "shader_type canvas_item; uniform sampler2D rim_mask; void fragment(){ vec4 c=texture(TEXTURE,UV); c.a*=step(0.5,texture(rim_mask,UV).a); COLOR=c; }"
	var material := ShaderMaterial.new()
	material.shader = shader
	material.set_shader_parameter("rim_mask", load("res://assets/art/nest_front_mask.png"))
	rim.material = material

func _label(parent: Node, text: String, rect: Rect2, size: int = 44) -> Label:
	var label := Label.new()
	label.text = text
	label.position = rect.position
	label.size = rect.size
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	label.add_theme_font_override("font", font)
	label.add_theme_font_size_override("font_size", size)
	label.add_theme_color_override("font_color", Color("fff1cf"))
	label.add_theme_color_override("font_outline_color", Color("5a3a22"))
	label.add_theme_constant_override("outline_size", 5)
	label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	parent.add_child(label)
	return label

func _button(parent: Node, text: String, rect: Rect2, action: Callable, icon_name: String = "") -> Button:
	var button := Button.new()
	button.position = rect.position
	button.size = rect.size
	button.text = text
	button.add_theme_font_override("font", font)
	button.add_theme_font_size_override("font_size", 42)
	# Existing dark-brown palette remains readable on the accepted cream button skin.
	for color_name in ["font_color", "font_hover_color", "font_pressed_color", "font_disabled_color", "font_focus_color", "font_hover_pressed_color"]:
		button.add_theme_color_override(color_name, Color("5a3a22"))
	var skin := StyleBoxTexture.new()
	skin.texture = load("res://assets/ui/panel_wood.png")
	for state in ["normal", "hover", "pressed", "disabled", "focus"]:
		button.add_theme_stylebox_override(state, skin)
	if not icon_name.is_empty():
		button.text = ""
		button.tooltip_text = text
		var icon := TextureRect.new()
		# Ignore native texture dimensions before assigning a texture or geometry.
		icon.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
		icon.texture = load("res://assets/ui/icon_%s.png" % icon_name)
		icon.position = rect.size * 0.2
		icon.size = rect.size * 0.6
		icon.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
		icon.mouse_filter = Control.MOUSE_FILTER_IGNORE
		button.add_child(icon)
	button.pressed.connect(func():
		action.call()
		_sound("click"))
	parent.add_child(button)
	return button

func _build_ui() -> void:
	var canvas := CanvasLayer.new()
	add_child(canvas)
	ui = Control.new()
	ui.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	ui.mouse_filter = Control.MOUSE_FILTER_IGNORE
	canvas.add_child(ui)
	hud = Control.new()
	hud.mouse_filter = Control.MOUSE_FILTER_IGNORE
	ui.add_child(hud)
	_label(hud, "Level 1", Rect2(330, 50, 420, 90), 58)
	moves_label = _label(hud, "Moves: 0", Rect2(330, 140, 420, 70), 38)
	_button(hud, "Settings", Rect2(870, 65, 130, 130), request_pause, "settings")
	undo_button = _button(hud, "Undo", Rect2(213, 1695, 130, 130), undo, "undo")
	restart_button = _button(hud, "Restart", Rect2(737, 1695, 130, 130), restart, "restart")
	_label(hud, "Tap a bird, then a matching branch", Rect2(90, 1490, 900, 90), 34)
	modal = Control.new()
	modal.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	modal.mouse_filter = Control.MOUSE_FILTER_STOP
	ui.add_child(modal)
	var shade := ColorRect.new()
	shade.color = Color(0, 0, 0, 0.72)
	shade.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	modal.add_child(shade)
	modal_panel = Control.new()
	modal.add_child(modal_panel)
	modal.hide()

func _layout() -> void:
	var viewport := get_viewport_rect().size
	var fit := minf(viewport.x / 1080.0, viewport.y / 1920.0)
	var offset := (viewport - Vector2(1080, 1920) * fit) * 0.5
	content.scale = Vector2.ONE * fit
	content.position = offset
	# Godot already converts the logical viewport. Safe area is physical screen pixels.
	var safe := DisplayServer.get_display_safe_area()
	var window := DisplayServer.window_get_size()
	if safe.size.x > 0 and window.x > 0 and DisplayServer.get_name() != "headless":
		var ratio := viewport / Vector2(window)
		var safe_rect := Rect2(Vector2(safe.position) * ratio, Vector2(safe.size) * ratio).intersection(Rect2(Vector2.ZERO, viewport))
		if safe_rect.size.x > 0 and safe_rect.size.y > 0:
			fit = minf(safe_rect.size.x / 1080.0, safe_rect.size.y / 1920.0)
			offset = safe_rect.position + (safe_rect.size - Vector2(1080, 1920) * fit) * 0.5
	hud.scale = Vector2.ONE * fit
	hud.position = offset
	modal_panel.scale = Vector2.ONE * fit
	modal_panel.position = offset

func _build_audio() -> void:
	for bus_name in ["Music", "Effects"]:
		if AudioServer.get_bus_index(bus_name) < 0:
			AudioServer.add_bus()
			AudioServer.set_bus_name(AudioServer.bus_count - 1, bus_name)
	music = AudioStreamPlayer.new()
	music.bus = "Music"
	music.stream = load("res://assets/audio/bgm_game.wav")
	music.volume_db = -16
	add_child(music)
	music.finished.connect(func(): music.play())
	# Headless cloud checks verify routing/state, not audible playback.
	if DisplayServer.get_name() != "headless":
		music.play()
	for key in {"flutter":"flutter", "chirp":"chirp_01", "home":"nest_home", "win":"win_fanfare", "click":"ui_tap"}:
		var filenames := {"flutter":"flutter", "chirp":"chirp_01", "home":"nest_home", "win":"win_fanfare", "click":"ui_tap"}
		var player := AudioStreamPlayer.new()
		player.bus = "Effects"
		player.stream = load("res://assets/audio/%s.wav" % filenames[key])
		add_child(player)
		audio[key] = player
	_apply_mute()

func _apply_mute() -> void:
	for bus_name in ["Music", "Effects"]:
		AudioServer.set_bus_mute(AudioServer.get_bus_index(bus_name), audio_muted)

func _sound(key: String) -> void:
	if audio.has(key) and not focus_lost and DisplayServer.get_name() != "headless":
		audio[key].play()

func slot(branch: int, index: int) -> Vector2:
	return ROOTS[branch] + Vector2((210 - index * 140) if branch == 1 else (-210 + index * 140), FEET[index])

func _sync() -> void:
	_cancel_motions()
	for actor in actors.values():
		actor.queue_free()
	actors.clear()
	var state: Dictionary = session.snapshot()
	for b in range(3):
		branches[b].visible = state.branches[b].home_species < 0
		branches[b].modulate = Color.WHITE
		for s in range(state.branches[b].tokens.size()):
			var token: Dictionary = state.branches[b].tokens[s]
			var actor = Bird.new()
			actor.configure(token)
			actor.position = slot(b, s)
			actor.z_index = 10 + s
			content.add_child(actor)
			actors[token.id] = actor
	moves_label.text = "Moves: %d" % state.move_count
	undo_button.visible = not session.history_snapshot().is_empty()
	selected = -1

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventKey and event.pressed and event.keycode == KEY_ESCAPE:
		if mode == "settings":
			close_settings()
		elif mode == "playing":
			request_pause()
		return
	var point: Vector2
	if event is InputEventMouseButton and event.device == -1:
		return # Touch is handled once below; GUI still receives emulated mouse clicks.
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		point = event.position
	elif event is InputEventScreenTouch and event.pressed:
		point = event.position
	else:
		return
	if busy or mode != "playing":
		return
	point = content.to_local(point)
	for b in range(3):
		if Rect2(ROOTS[b] + Vector2(-325, -190), Vector2(650, 250)).has_point(point):
			pick(b)
			return
	cancel_selection()

func pick(branch: int) -> void:
	if busy or mode != "playing":
		return
	var state: Dictionary = session.snapshot()
	if selected >= 0:
		var source := selected
		if branch == source:
			cancel_selection()
			return
		var tx: Dictionary = session.prepare_transfer(source, branch)
		if tx.ok:
			_submit(tx)
		else:
			cancel_selection()
		return
	var run: int = Rules.get_run(state, branch)
	if run == 0:
		return
	_cancel_motions()
	# Settle any previous cancellation before taking off a new group.
	for b in range(3):
		for s in range(state.branches[b].tokens.size()):
			var idle_actor = actors[state.branches[b].tokens[s].id]
			idle_actor.position = slot(b, s)
			idle_actor.set_flying(false)
	selected = branch
	_sound("chirp")
	_sound("flutter")
	for index in range(state.branches[branch].tokens.size() - run, state.branches[branch].tokens.size()):
		var actor = actors[state.branches[branch].tokens[index].id]
		actor.set_flying(true, true)
		var tween := _tween()
		tween.tween_property(actor, "position", slot(branch, index) + Vector2(0, -30), 0.15).set_trans(Tween.TRANS_SINE)

func cancel_selection() -> void:
	if selected < 0:
		return
	_cancel_motions()
	selected = -1
	var state: Dictionary = session.snapshot()
	for b in range(3):
		for s in range(state.branches[b].tokens.size()):
			var actor = actors[state.branches[b].tokens[s].id]
			if actor.flying:
				var tween := _tween()
				tween.tween_property(actor, "position", slot(b, s), 0.15)
				tween.tween_callback(actor.set_flying.bind(false))

func _submit(tx: Dictionary) -> void:
	if busy or mode != "playing" or store.is_read_only or not tx.get("ok", false):
		return
	pending = tx
	busy = true
	var candidate: Dictionary = store.make_record(tx.state, tx.history, record)
	var outcome: Dictionary = store.commit(candidate)
	if outcome.status == "committed":
		record = candidate
		_accept_pending()
	else:
		_cancel_motions()
		mode = "save_error"
		_show_modal("Could not save", "Your move is waiting safely. Retry before continuing.", [["Retry", retry_save], ["Home", _home]])

func retry_save() -> void:
	if not store.has_pending():
		return
	var outcome: Dictionary = store.retry()
	if outcome.status == "committed":
		if pending.is_empty():
			record = store.confirmed_record()
			audio_muted = record.audio_muted
			_apply_mute()
			busy = false
			mode = "playing"
			request_pause()
		else:
			record = store.confirmed_record()
			_accept_pending()

func _accept_pending() -> void:
	var tx := pending
	pending = {}
	if not session.commit(tx):
		# The durable candidate is authoritative even if an external lifecycle invalidated the session.
		session.restore(record.state, record.history)
	modal.hide()
	mode = "playing"
	selected = -1
	_cancel_motions()
	if tx.kind != "Transfer":
		_sync()
		_finish_presentation()
		return
	_play_events(tx.events, 0, generation)

func _tween() -> Tween:
	var tween := create_tween()
	motions.append(tween)
	return tween

func _cancel_motions() -> void:
	generation += 1
	for tween in motions:
		if tween.is_valid():
			tween.kill()
	motions.clear()

func _play_events(events: Array, index: int, version: int) -> void:
	if version != generation or not is_inside_tree():
		return
	if index >= events.size():
		_sync()
		_finish_presentation()
		return
	var event: Dictionary = events[index]
	var tween := _tween()
	if event.kind == "Moved":
		_sound("flutter")
		tween.set_parallel(true)
		for i in range(event.token_ids.size()):
			var actor = actors[event.token_ids[i]]
			actor.set_flying(true)
			var start: Vector2 = actor.position
			var end := slot(event.to, event.target_start_slot + i)
			tween.tween_method(func(t: float): actor.position = start.lerp(end, t) + Vector2(0, -sin(t * PI) * 100), 0.0, 1.0, 0.58).set_delay(i * 0.045)
		tween.chain().tween_callback(func():
			for id in event.token_ids:
				actors[id].set_flying(false))
	elif event.kind == "Home":
		_sound("home")
		tween.set_parallel(true)
		for i in range(event.token_ids.size()):
			var actor = actors[event.token_ids[i]]
			actor.set_flying(true)
			actor.z_index = 40
			var start: Vector2 = actor.position
			var mouth := Vector2(540, 320 + (310.0 - 256.0) * 300.0 / 512.0)
			var inside := Vector2(540, 320 + (435.0 - 256.0) * 300.0 / 512.0)
			tween.tween_method(_home_pose.bind(actor, start, mouth, inside), 0.0, 1.0, 0.8).set_delay(i * 0.06)
		tween.chain().tween_callback(func():
			for id in event.token_ids:
				actors[id].hide())
	elif event.kind == "Retired":
		tween.tween_property(branches[event.branch], "modulate:a", 0.0, 0.3)
	else:
		tween.tween_interval(0.01)
	tween.tween_callback(_play_events.bind(events, index + 1, version))

func _home_pose(t: float, actor: Node2D, start: Vector2, mouth: Vector2, inside: Vector2) -> void:
	var elapsed := t * 0.8
	if elapsed <= 0.55:
		var p := clampf(elapsed / 0.55, 0.0, 1.0)
		actor.position = start.lerp(mouth, smoothstep(0.0, 1.0, p)) + Vector2(0, -sin(p * PI) * 70.0)
		actor.scale = Vector2.ONE
		actor.z_index = 40 # Approach in front of the rim; do not occlude early.
		if p >= 0.9:
			actor.set_flying(false)
	else:
		var u := clampf((elapsed - 0.55) / 0.25, 0.0, 1.0)
		actor.z_index = 20 # Mask only after reaching the accepted mouth anchor.
		actor.set_flying(false)
		actor.position = mouth.lerp(inside, u)
		actor.scale = Vector2.ONE * lerpf(1.0, 0.35, clampf((u - 0.32) / 0.68, 0.0, 1.0))

func _finish_presentation() -> void:
	busy = false
	if Rules.is_won(session.snapshot()):
		pending_pause = false
		_win()
	elif pending_pause:
		pending_pause = false
		request_pause()

func undo() -> void:
	if busy or mode != "playing":
		return
	var tx: Dictionary = session.prepare_undo()
	if tx.ok:
		_submit(tx)

func restart() -> void:
	if busy or mode not in ["playing", "won"]:
		return
	mode = "playing"
	_submit(session.prepare_restart())

func request_pause() -> void:
	if mode != "playing":
		return
	if busy:
		pending_pause = true
		return
	cancel_selection()
	settings_origin = "playing"
	mode = "settings"
	_show_settings()

func _show_settings() -> void:
	_show_modal("Settings", "", [["Resume", close_settings], ["Sound: Off" if audio_muted else "Sound: On", toggle_sound], ["Home", _home]])

func close_settings() -> void:
	if mode != "settings":
		return
	mode = settings_origin
	modal.hide()
	if mode == "home":
		_home()

func toggle_sound() -> void:
	if mode != "settings" or store.has_pending() or store.is_read_only:
		return
	var candidate := record.duplicate(true)
	candidate.audio_muted = not audio_muted
	audio_muted = candidate.audio_muted
	_apply_mute()
	var outcome: Dictionary = store.commit(candidate)
	if outcome.status == "committed":
		record = candidate
		_apply_mute()
		_show_settings()
	else:
		mode = "save_error"
		_show_modal("Could not save", "Retry to save your sound preference.", [["Retry", retry_save], ["Home", _home]])

func _home() -> void:
	if busy and mode == "playing":
		return
	cancel_selection()
	mode = "home"
	var actions: Array = [["Continue", _continue]]
	if not store.has_pending() and not store.is_read_only:
		actions.append(["Settings", func():
			settings_origin = "home"
			mode = "settings"
			_show_settings()])
	_show_modal("Perch Sort", "First-level migration • Level 1", actions)

func _continue() -> void:
	if store.has_pending():
		mode = "save_error"
		_show_modal("Move awaiting save", "Retry the same save before continuing.", [["Retry", retry_save], ["Home", _home]])
	elif store.is_read_only:
		_show_modal("Save unavailable", "Your existing save is protected. Close and reopen after resolving storage access.", [["Home", _home]])
		return
	elif Rules.is_won(session.snapshot()):
		_win()
	else:
		busy = false
		mode = "playing"
		modal.hide()

func _win() -> void:
	mode = "won"
	_sound("win")
	_show_modal("Welcome home!", "Level 1 complete • %d moves" % session.snapshot().move_count, [["Replay", restart], ["Home", _home]])
	var tween := _tween()
	modal_panel.pivot_offset = Vector2(540, 960)
	var final_scale := modal_panel.scale
	modal_panel.scale = final_scale * 0.92
	tween.tween_property(modal_panel, "scale", final_scale, 0.3).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)

func _show_modal(title: String, body: String, actions: Array) -> void:
	for child in modal_panel.get_children():
		child.queue_free()
	var panel := TextureRect.new()
	panel.texture = load("res://assets/ui/panel_wood.png")
	panel.position = Vector2(140, 500)
	panel.size = Vector2(800, 850)
	panel.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	panel.mouse_filter = Control.MOUSE_FILTER_STOP
	modal_panel.add_child(panel)
	_label(modal_panel, title, Rect2(190, 570, 700, 110), 60)
	var label := _label(modal_panel, body, Rect2(210, 690, 660, 145), 36)
	label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	for i in range(actions.size()):
		_button(modal_panel, actions[i][0], Rect2(320, 850 + i * 140, 440, 110), actions[i][1])
	modal.show()

func _notification(what: int) -> void:
	if what in [NOTIFICATION_APPLICATION_FOCUS_OUT, NOTIFICATION_APPLICATION_PAUSED] and is_node_ready():
		focus_lost = true
		request_pause()
		for actor in actors.values():
			actor.set_process(false)
		for tween in motions:
			if tween.is_valid(): tween.pause()
		music.stream_paused = true
		for player in audio.values(): player.stream_paused = true
	elif what in [NOTIFICATION_APPLICATION_FOCUS_IN, NOTIFICATION_APPLICATION_RESUMED] and is_node_ready():
		focus_lost = false
		for actor in actors.values(): actor.set_process(true)
		for tween in motions:
			if tween.is_valid(): tween.play()
		music.stream_paused = false
		for player in audio.values(): player.stream_paused = false

func _exit_tree() -> void:
	_cancel_motions()
	if is_instance_valid(music):
		music.stop()
		music.stream = null
	for player in audio.values():
		player.stop()
		player.stream = null
	audio.clear()
