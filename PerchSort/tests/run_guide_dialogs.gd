extends SceneTree
## Guide-family shell geometry and unchanged close/action routing, in both painted locales.
var checks := 0
var failures := 0
var calls := 0
var background_clicks := 0

func check(ok: bool, label: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		print("FAIL ", label)

func _initialize() -> void:
	_run.call_deferred()

func inspect(dialog: Control, buttons: Array, minimum_action_gap := 40.0) -> void:
	check(dialog.content.get_node("Panel/Rim") is NinePatchRect, "jungle frame")
	check(dialog.panel_rect.position.y >= 20, "frame on screen")
	check(not dialog.content.has_node("ProductGlow"), "no decorative product glow")
	check(dialog.content.get_node("TitleGroup").get_child_count() >= 2, "painted title")
	for name in buttons:
		var button: Control = name if name is Control else dialog.content.get_node(name)
		var label := str(button.name)
		check(button.position.y >= dialog.panel_rect.end.y + minimum_action_gap, label + " outside frame")
		check(button.get_rect().end.y <= 1900, label + " stays on screen")
		check(absf(button.get_rect().get_center().x - 540) < 470, label + " horizontal bounds")

func _run() -> void:
	root.content_scale_size = Vector2i(1080, 1920)
	root.content_scale_mode = Window.CONTENT_SCALE_MODE_VIEWPORT
	root.content_scale_aspect = Window.CONTENT_SCALE_ASPECT_KEEP
	var main: Control = load("res://game/main.tscn").instantiate()
	var fake := Profile.new()
	fake.muted = true
	fake.music_muted = true
	main.args["profile"] = JSON.stringify(fake.to_dict())
	main.args["preview"] = "coin_shop_view:normal"
	root.add_child(main)
	main.args["smoke"] = "1"
	main.args["capture"] = "1"
	await process_frame
	var popups: GDScript = load("res://game/popups.gd")
	var tutorial: GDScript = load("res://game/tutorial.gd")
	for locale in ["en", "zh"]:
		TranslationServer.set_locale(locale)
		for kind in 6:
			var dialog: Control = popups.unlock(main.popups, kind, func(): calls += 1)
			inspect(dialog, ["Check"])
			var shop_art: Array = load("res://game/shop_view.gd").BOOSTER_ILLUS
			check(dialog.content.get_node("UnlockPicture").texture == Art.tex("island_icon_%s" % shop_art[kind]), "unlock matches shop tool artwork")
			dialog.content.get_node("Check").pressed.emit()
			dialog.content.get_node("Check").pressed.emit()
			await create_timer(0.2).timeout
		for ad in [false, true]:
			var dialog: Control = load("res://game/undo_offer.gd").open(main.popups, 120, func(): calls += 1, func(): calls += 1, true)
			inspect(dialog, ["Buy", "WatchAd"])
			var button: Button = dialog.content.get_node("WatchAd" if ad else "Buy")
			button.pressed.emit()
			button.pressed.emit()
			await create_timer(0.2).timeout
		var stuck: Control = load("res://game/stuck_offer.gd").open(main.popups, func(): calls += 1, true)
		# The compact rescue design explicitly keeps 32 px below its frame.
		inspect(stuck, ["Rescue"], 32.0)
		check(is_equal_approx(stuck.content.get_node("Rescue").position.y - stuck.panel_rect.end.y, 32.0), "compact rescue keeps its approved gap")
		stuck.content.get_node("Rescue").pressed.emit()
		stuck.content.get_node("Rescue").pressed.emit()
		await create_timer(0.2).timeout
		var guide: Control = tutorial.open(main.popups, func(): calls += 1, true)
		check(guide.modal._dim.color.a >= 0.8, "tutorial visually isolates underlying Play")
		check(guide.modal._dim.mouse_filter == Control.MOUSE_FILTER_STOP, "tutorial shade blocks underlying input")
		check(guide.modal._dim.size == Vector2(1080, 1920), "tutorial shade covers entire game canvas")
		for step in 4:
			inspect(guide.modal, guide._buttons)
			check(guide.step == step, "tutorial retains sequential steps")
			var word: Control = guide._buttons[0].get_node("Content/Word")
			check(word.painted_id == ("white_play" if step == 3 else "white_tutorial_next"), "painted action matches current step")
			guide._buttons[0].pressed.emit()
			await process_frame
		await create_timer(0.2).timeout
		for mechanic in tutorial.LESSONS:
			var lesson: Control = tutorial.open_lesson(main.popups, mechanic, 1, func(): calls += 1, true)
			inspect(lesson.modal, lesson._buttons)
			lesson._buttons[0].pressed.emit()
			lesson._buttons[0].pressed.emit()
			await create_timer(0.2).timeout
	# A real pointer event outside the panel must stop at the modal shade, not reach the page.
	var host := Control.new()
	host.size = Vector2(1080, 1920)
	host.mouse_filter = Control.MOUSE_FILTER_IGNORE
	root.add_child(host)
	var underlying := Button.new()
	underlying.position = Vector2(20, 1700)
	underlying.size = Vector2(120, 120)
	underlying.pressed.connect(func(): background_clicks += 1)
	host.add_child(underlying)
	await process_frame
	await click_point(Vector2(80, 1760))
	check(background_clicks == 1, "background test button receives unblocked click")
	var overlay: Control = tutorial.open(host, Callable(), true)
	await process_frame
	await click_point(Vector2(80, 1760))
	check(background_clicks == 1, "tutorial blocks click through outside panel")
	overlay.modal.close()
	await create_timer(0.25).timeout
	await click_point(Vector2(80, 1760))
	check(background_clicks == 2, "background interaction restored after closing tutorial")
	host.queue_free()
	check(calls == 34, "all callbacks once; Next does not close tutorial")
	print("GUIDE_DIALOGS checks=%d failures=%d" % [checks, failures])
	main.queue_free()
	await process_frame
	quit(1 if failures else 0)

func click_point(point: Vector2) -> void:
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
	await create_timer(0.1).timeout
