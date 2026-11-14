extends SceneTree
## Canonical controls driven by actual desktop pointer input; fake data never saves.
var calls := 0
var failures := 0

func _initialize() -> void:
	_run.call_deferred()

func check(ok: bool, message: String) -> void:
	print("PASS " if ok else "FAIL ", message)
	failures += 0 if ok else 1

func _tap(button: Control) -> void:
	var point := button.get_global_rect().get_center()
	var motion := InputEventMouseMotion.new()
	motion.position = point
	root.push_input(motion, true)
	for pressed in [true, false]:
		var event := InputEventMouseButton.new()
		event.position = point
		event.button_index = MOUSE_BUTTON_LEFT
		event.pressed = pressed
		root.push_input(event, true)

func _close_with(dialog: Control, action: String) -> void:
	await process_frame
	var button: Control = dialog.content.get_node(action)
	_tap(button)
	var deadline := Time.get_ticks_msec() + 2000
	while is_instance_valid(dialog) and Time.get_ticks_msec() < deadline:
		await process_frame
	if is_instance_valid(dialog):
		failures += 1
		push_error("Dialog did not close after pointer action: " + action)
		dialog.queue_free()
		await process_frame

func _run() -> void:
	root.content_scale_size = Vector2i(1080, 1920)
	root.content_scale_mode = Window.CONTENT_SCALE_MODE_VIEWPORT
	var main: Control = load("res://game/main.tscn").instantiate()
	var fake := Profile.new()
	fake.muted = true
	fake.music_muted = true
	main.args["profile"] = JSON.stringify(fake.to_dict())
	main.args["preview"] = "map_view:cards"
	root.add_child(main)
	main.args["smoke"] = "1"
	main.args["capture"] = "1"
	UiText.set_language("en")
	var popups: GDScript = load("res://game/popups.gd")
	var species: Array[String] = ["sunny", "sky", "berry"]
	var mechanics: Array[String] = ["sleep", "egg"]
	var dialog: Control = popups.level_card(main.popups, 12, 0, 0, 9, species, mechanics, func(): calls += 100, false)
	await process_frame
	_tap(dialog.content.get_node("Play"))
	await process_frame
	check(is_instance_valid(dialog) and calls == 0, "disabled Play neither closes nor dispatches")
	await _close_with(dialog, "Close")
	check(calls == 0, "Close preserves callback count")
	print("POINTER PASS: disabled Play and Close preserve callback count")
	await process_frame
	for action in ["Home", "Retry", "action_next"]:
		dialog = popups.win(main.popups, 3, 12, 12, 120, [0, 3],
			func(): calls += 1, func(): calls += 10, func(): calls += 100, true)
		await _close_with(dialog, action)
		await process_frame
		print("POINTER PASS: ", action, " callbacks=", calls)
	check(calls == 111, "each win action dispatches exactly once")
	dialog = popups.world_unlock(main.popups, 2, func(): calls += 1000)
	await _close_with(dialog, "Play")
	check(calls == 1111, "world Play dispatches exactly once")
	print("POINTER PASS: world Play; all callbacks exact; preserved balance=", main.profile.coins)
	main.queue_free()
	await process_frame
	quit(1 if failures else 0)
