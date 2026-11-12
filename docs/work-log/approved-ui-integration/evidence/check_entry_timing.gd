extends SceneTree

var failures := 0
var main: Control

func _initialize() -> void:
	_run.call_deferred()

func _run() -> void:
	main = load("res://game/main.tscn").instantiate()
	var fake := Profile.new()
	fake.highest_unlocked = 32
	fake.language = "zh"
	fake.muted = true
	fake.music_muted = true
	for i in range(1, 32):
		fake.stars[i] = 3
	for species in Aviary.SPECIES:
		fake.met[species[0]] = true
	var tutorial = load("res://game/tutorial.gd")
	fake.set_tutorial(tutorial.FLAG)
	for mechanic in tutorial.LESSONS:
		fake.set_tutorial(tutorial.lesson_flag(mechanic))
	for booster in Progress.BOOSTERS:
		fake.set_tutorial("unlock.%s" % booster[1])
	main.args["profile"] = JSON.stringify(fake.to_dict())
	root.add_child(main)
	main.args["smoke"] = "1"
	await create_timer(0.6).timeout
	for route in ["home_play", "daily", "chest_play"]:
		main.open_home()
		await create_timer(0.6).timeout
		var board_script = load("res://game/board_view.gd")
		board_script._opened_levels.clear()
		board_script._last_started = -1
		board_script._last_won = -1
		var control: Control
		if route == "chest_play":
			main.show_chest()
			await create_timer(0.35).timeout
			control = main.popups.get_child(-1).get_node("Content/Play")
		else:
			control = main.screen.get_node("Daily" if route == "daily" else "Play")
		var started := Time.get_ticks_msec()
		tap(control)
		var playable := false
		var elapsed := 0
		while Time.get_ticks_msec() - started < 6500:
			await process_frame
			if main.screen.get_script().resource_path.ends_with("board_view.gd") and not main.screen.busy and not main._wiping and main.popups.get_child_count() == 0:
				elapsed = Time.get_ticks_msec() - started
				playable = true
				break
		var passed := playable and elapsed >= 3500 and elapsed <= 4500
		if not passed:
			failures += 1
		print("TAP_TO_PLAYABLE route=%s elapsed_ms=%d playable=%s pass=%s" % [route, elapsed, playable, passed])
	main.queue_free()
	await process_frame
	await process_frame
	print("entry timing checks: %d failures" % failures)
	quit(1 if failures else 0)

func tap(control: Control) -> void:
	var point := control.get_global_rect().get_center()
	var move := InputEventMouseMotion.new()
	move.position = point
	root.push_input(move, true)
	var press := InputEventMouseButton.new()
	press.position = point
	press.button_index = MOUSE_BUTTON_LEFT
	press.pressed = true
	root.push_input(press, true)
	var release := press.duplicate()
	release.pressed = false
	root.push_input(release, true)
