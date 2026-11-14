extends SceneTree
## Guided first use of every tool: at its unlock level the board dims except the tool, board taps do nothing until it
## is pressed, pressing uses one unit and ends the guide, and a second visit does not guide again.
var checks := 0
var failures := 0

func _initialize() -> void:
	_run.call_deferred()

func check(ok: bool, message: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		print("FAIL ", message)

func _fresh(level: int) -> Control:
	var main: Control = load("res://game/main.tscn").instantiate()
	var fake := Profile.new()
	fake.muted = true
	fake.music_muted = true
	fake.highest_unlocked = level
	for card in Aviary.SPECIES:
		fake.met[card[0]] = true
	# Every card, lesson and the how-to-play are already seen, so the board is clear for the guide.
	var tutorial: GDScript = load("res://game/tutorial.gd")
	fake.set_tutorial(tutorial.FLAG)
	for mechanic in tutorial.LESSONS:
		fake.set_tutorial(tutorial.lesson_flag(mechanic))
	for info in Progress.BOOSTERS:
		fake.set_tutorial("unlock.%s" % info[1])
	main.args["profile"] = JSON.stringify(fake.to_dict())
	main.args["preview"] = "coin_shop_view:normal"
	main.args["motion"] = "1"
	root.add_child(main)
	await create_timer(0.3).timeout
	main.args["smoke"] = "1"
	main.args["capture"] = "1"
	return main

func _run() -> void:
	for info in Progress.BOOSTERS:
		var kind: int = info[0]
		var level := Progress.booster_unlock_level(kind)
		var tag := "%s (level %d)" % [info[1], level]
		var main := await _fresh(level)
		main._open_board(level, false)
		await create_timer(0.8).timeout
		var board: Control = main.screen
		check(board.guiding() and board._guide_kind == kind, tag + ": guided state at the unlock level")
		check(is_instance_valid(board._guide_mask) and board._stuck != null, tag + ": spotlight and hand shown")
		check(main.profile.inventory[kind] >= 1 and board.session.state.inventory[kind] >= 1, tag + ": a free unit is in hand")
		var stock: int = board.session.state.inventory[kind]
		board.tap_branch(0)
		board.tap_branch(1)
		check(board.session.move_count() == 0 and board.session.selected < 0, tag + ": board taps do nothing")
		for other in Progress.BOOSTERS:
			if other[0] != kind:
				board._on_tool(other[0])
		board._on_undo()
		check(board.guiding() and board.session.move_count() == 0, tag + ": other tools and undo wait")
		board._on_tool(kind)
		var t0 := Time.get_ticks_msec()
		while board.guiding() and Time.get_ticks_msec() - t0 < 10000:
			await process_frame
		await create_timer(0.5).timeout
		check(not board.guiding() and board._guide_mask == null, tag + ": pressing ends the guide")
		check(board.session.state.inventory[kind] == stock - 1, tag + ": one unit used")
		check(main.profile.has_tutorial(Progress.guided_flag(kind)), tag + ": guided flag set")
		main._open_board(level, false)
		await create_timer(0.8).timeout
		check(not main.screen.guiding(), tag + ": a second visit does not guide")
		main.queue_free()
		await process_frame
	# A tool with no effect yet waits; the guide starts once a move or undo gives it one.
	var main := await _fresh(4)
	main._open_board(4, false)
	await create_timer(0.8).timeout
	var board: Control = main.screen
	board._end_guide(Board.Booster.SAPLING)   # the unlock-level guide of the first pass is not under test here
	board.profile.tutorials.erase(Progress.guided_flag(Board.Booster.SAPLING))
	board.session.state.added_branch = true
	board.guide_tool(Board.Booster.SAPLING)
	check(not board.guiding(), "no effect, no guide")
	board.session.state.added_branch = false
	board._try_guide()
	check(board.guiding(), "the guide starts when the tool has an effect")
	main.queue_free()
	print("tool guide checks: %d checks, %d failures" % [checks, failures])
	quit.call_deferred(1 if failures else 0)
