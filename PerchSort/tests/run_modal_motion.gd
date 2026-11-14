extends Node
## Timed checks use canonical dialogs, with animation enabled and an isolated fake profile.
var failures := 0
var checks := 0
var main: Control

func check(ok: bool, message: String) -> void:
	checks += 1
	if not ok:
		failures += 1
	print("PASS " if ok else "FAIL ", message)

func _ready() -> void:
	_run.call_deferred()

func _run() -> void:
	main = load("res://game/main.tscn").instantiate()
	var profile := Profile.new()
	profile.muted = true
	profile.music_muted = true
	main.args["profile"] = JSON.stringify(profile.to_dict())
	main.args["preview"] = "popups:settings_muted"
	add_child(main)
	main.args["smoke"] = "1"
	await get_tree().process_frame
	main._clear_popups()
	# Check interruptions at the first frame and midway through entry.
	for delay in [0.0, 0.06, 0.3]:
		var p := Modal.open(main.popups, "", [240.0], false)
		await get_tree().create_timer(maxf(delay, 0.001)).timeout
		var calls := [0, 0]
		p.closed.connect(func(): calls[0] += 1)
		var initial: Vector2 = p.content.scale
		p.invoke(func(): calls[1] += 1)
		p.invoke(func(): calls[1] += 1)
		p.close()
		await get_tree().create_timer(0.04).timeout
		check(not is_instance_valid(p) or p.content.scale.x <= initial.x + 0.0001, "closing recedes from interrupted pose %.2f" % delay)
		await get_tree().create_timer(0.18).timeout
		check(not is_instance_valid(p) and calls == [1, 1], "rapid close/action dispatch exactly once %.2f" % delay)
	var source := FileAccess.get_file_as_string("res://game/popups.gd")
	var regex := RegEx.new()
	regex.compile('^\\t\\t"([a-z_0-9]+)":')
	var states: Array[String] = []
	for line in source.split("\n"):
		var found := regex.search(line)
		if found and found.get_string(1) not in ["leaf_transition", "lucky_draw_spin", "lucky_draw_coins", "daily_reward_claim"]:
			states.append(found.get_string(1))
	for state in states:
		if state.begins_with("lucky_draw"):
			main.open_home()
			await get_tree().create_timer(0.7).timeout
		if state.begins_with("lucky_draw"):
			main._clear_popups()
			var script := load("res://game/popups.gd")
			# Home navigation is deferred; open the canonical dialog only after it settles.
			if state.begins_with("lucky_draw_reward"):
				var coins := 400 if state.ends_with("coins") else (120 if state.ends_with("mixed") else 0)
				var boosters := [] if state.ends_with("coins") else ([3] if state.ends_with("single") else [1, 1])
				script.rewards(main.popups, "draw.title", coins, boosters, func(): pass)
			else:
				script.lucky_draw(main.popups, 0 if state.ends_with("none") else 3, 5, func(): return {})
		else:
			load("res://game/popups.gd").preview(main, state)
		await get_tree().create_timer(0.36).timeout
		var dialogs: Array[Modal] = []
		for child in main.popups.get_children():
			if child is Modal:
				dialogs.append(child)
		check(not dialogs.is_empty(), state + " creates its canonical dialog")
		for p in dialogs:
			check(p.content.scale.is_equal_approx(Vector2.ONE) and p.content.modulate.a > 0.99, state + " entrance settles")
			var before := JSON.stringify(main.profile.to_dict())
			var calls := [0]
			p.closed.connect(func(): calls[0] += 1)
			p.close()
			p.close()
			await get_tree().create_timer(0.04).timeout
			if is_instance_valid(p) and p._page_motion:
				check(p.content.scale.is_equal_approx(Vector2.ONE), state + " full-screen exit does not shrink")
			await get_tree().create_timer(0.18).timeout
			check(not is_instance_valid(p) and calls[0] == 1, state + " exit frees exactly once")
			check(JSON.stringify(main.profile.to_dict()) == before, state + " close preserves profile")
	for title in ["item_firefly", "house_teapot"]:
		var purchase := PurchaseDialog.open_shop(main.popups, title, [300.0], false, 162.0)
		await _check_extra(purchase, "purchase " + title)
	var tutorial_script := load("res://game/tutorial.gd")
	for step in range(4):
		var tutorial: Control = tutorial_script.open(main.popups, Callable(), false)
		tutorial.show_step(step)
		await _check_extra(tutorial.modal, "tutorial step %d" % step)
	for mechanic in ["sleep", "egg"]:
		var lesson: Control = tutorial_script.open_lesson(main.popups, mechanic, 1, Callable(), false)
		await _check_extra(lesson.modal, "lesson " + mechanic)
	var recovery: Control = load("res://game/save_recovery.gd").open(main)
	await _check_extra(recovery, "save recovery")
	main.queue_free()
	await get_tree().process_frame
	print("MODAL MOTION: %d states, %d checks, %d failures" % [states.size(), checks, failures])
	get_tree().quit(1 if failures else 0)

func _check_extra(p: Modal, label: String) -> void:
	await get_tree().create_timer(0.36).timeout
	check(p.content.scale.is_equal_approx(Vector2.ONE) and p.content.modulate.a > 0.99, label + " settles")
	var calls := [0]
	p.closed.connect(func(): calls[0] += 1)
	p.close()
	p.close()
	await get_tree().create_timer(0.25).timeout
	check(not is_instance_valid(p) and calls[0] == 1, label + " closes once during content motion")
