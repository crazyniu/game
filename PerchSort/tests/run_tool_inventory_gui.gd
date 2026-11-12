extends SceneTree
## All seven items through the canonical toolbar, including zero-stock purchase input.

var failures := 0
func check(ok: bool, message: String) -> void:
	print("PASS " if ok else "FAIL ", message)
	failures += 0 if ok else 1

func _initialize() -> void:
	_run.call_deferred()

func _run() -> void:
	var board = load("res://tests/tool_inventory_fixture.gd").new()
	board.instant = true
	root.add_child(board)
	var profile := Profile.new()
	profile.highest_unlocked = Levels.COUNT
	profile.inventory = PackedInt32Array([3, 0, 2, 1, 5, 0])
	board.start(Levels.get_level(95), profile, false)
	await create_timer(0.5).timeout
	var buttons: Array = board._tool_bar.get_children().filter(func(n): return n is Button and not n.is_queued_for_deletion())
	check(buttons.size() == 7, "all seven items retained")
	var previous := Rect2()
	for button: Button in buttons:
		var bounds := button.get_global_rect()
		check(bounds.size == Vector2(136, 184), "%s uniform full-item hit area" % button.name)
		check(bounds.position.x >= 40 and bounds.end.x <= 1040 and bounds.end.y <= 1888, "%s safe margins" % button.name)
		check(previous == Rect2() or previous.end.x + 8 <= bounds.position.x, "%s no overlap" % button.name)
		previous = bounds
		var stock := button.get_node_or_null("Stock")
		check(stock == null or stock is Label, "%s quantity is plain integrated text, no badge" % button.name)
	var buys: Array = []
	board.buy_requested.connect(func(kind): buys.append(kind))
	var target: Button = board._tool_bar.get_node("Tool 1")
	var point := target.get_global_rect().position + Vector2(68, 158)
	var motion := InputEventMouseMotion.new()
	motion.position = point
	Input.parse_input_event(motion)
	await process_frame
	for pressed in [true, false]:
		var click := InputEventMouseButton.new()
		click.position = point
		click.button_index = MOUSE_BUTTON_LEFT
		click.pressed = pressed
		Input.parse_input_event(click)
		await process_frame
	check(buys == [1], "zero-stock plus row hit opens the same purchase action")
	for resolution in [Vector2i(1080,1920), Vector2i(390,844), Vector2i(720,1280)]:
		root.size = resolution
		await create_timer(0.25).timeout
		await RenderingServer.frame_post_draw
		root.get_texture().get_image().save_png("res://../build/board-recovery/inventory_%dx%d.png" % [resolution.x,resolution.y])
	print("tool inventory GUI: %d failures" % failures)
	board.queue_free()
	await process_frame
	quit(1 if failures else 0)
