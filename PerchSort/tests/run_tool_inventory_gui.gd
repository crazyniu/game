extends SceneTree
## All seven items through the canonical toolbar and the zero-stock one-more offer.

var failures := 0
func check(ok: bool, message: String) -> void:
	print("PASS " if ok else "FAIL ", message)
	failures += 0 if ok else 1

func _initialize() -> void:
	root.content_scale_size = Vector2i(1080, 1920)
	root.content_scale_mode = Window.CONTENT_SCALE_MODE_VIEWPORT
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
	var first_size: Vector2 = buttons[0].get_global_rect().size
	for button: Button in buttons:
		var bounds := button.get_global_rect()
		check(bounds.size == first_size, "%s uniform full-item hit area" % button.name)
		check(bounds.position.x >= 40 and bounds.end.x <= 1040 and bounds.end.y <= 1888, "%s safe margins" % button.name)
		check(previous == Rect2() or previous.end.x + 8 <= bounds.position.x, "%s no overlap" % button.name)
		previous = bounds
		var stock := button.get_node_or_null("Stock")
		# Counts are the shared painted numerals under the slot (ButtonKit.number), no badge.
		check(stock != null and stock.has_node("Digits") and stock.get_child_count() == 1, "%s count is painted numerals, no badge" % button.name)
	var buys: Array = []
	board.buy_requested.connect(func(kind): buys.append(kind))
	var target: Button = board._tool_bar.get_node("Tool 1")
	var point := target.get_global_rect().get_center()
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
	check(buys == [Board.Booster.SAPLING], "zero-stock tool emits its one-more request")
	# Exercise Main's production handler for that same request, not a substitute shop callback. Preview never saves.
	var main: Control = load("res://game/main.tscn").instantiate()
	main.args["profile"] = JSON.stringify(profile.to_dict())
	main.args["preview"] = "coin_shop_view:normal"
	root.add_child(main)
	main.args["smoke"] = "1"
	main.args["capture"] = "1"
	await process_frame
	var before := JSON.stringify(main.profile.to_dict())
	main._offer_tool(Board.Booster.SAPLING, board)
	await process_frame
	var offer: Control = main.popups.get_node_or_null("Popup")
	check(offer != null, "one-more offer opens without navigating to the shop")
	if offer != null:
		check(offer.content.has_node("Buy") and offer.content.has_node("WatchAd"), "one-more offer retains buy and ad choices")
		check(offer.content.get_node("Buy/Content/FeatherPrice/Digits").get_meta("value") == str(Progress.booster_price(Board.Booster.SAPLING)), "one-more price uses the production price")
		offer.content.get_node("Close").pressed.emit()
		var deadline := Time.get_ticks_msec() + 2000
		while is_instance_valid(offer) and Time.get_ticks_msec() < deadline:
			await process_frame
		check(not is_instance_valid(offer), "cancel finishes without a hanging modal")
	check(JSON.stringify(main.profile.to_dict()) == before, "cancel does not spend currency or change inventory")
	check(main._overlay == null, "zero-stock request does not open a shop overlay")
	main.queue_free()
	await process_frame
	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path("res://../build/board-recovery"))
	for resolution in [Vector2i(1080,1920), Vector2i(390,844), Vector2i(720,1280)]:
		root.size = resolution
		await create_timer(0.25).timeout
		await RenderingServer.frame_post_draw
		root.get_texture().get_image().save_png("res://../build/board-recovery/inventory_%dx%d.png" % [resolution.x,resolution.y])
	print("tool inventory GUI: %d failures" % failures)
	board.queue_free()
	await process_frame
	quit(1 if failures else 0)
