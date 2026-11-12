extends SceneTree
## Unboxed reward amounts reuse painted cream glyphs, with their original dark edges.
var checks := 0
var failures := 0
var main: Control
var evidence := ""

func check(ok: bool, label: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		push_error(label)

func _initialize() -> void:
	run.call_deferred()

func run() -> void:
	root.content_scale_size = Vector2i(1080, 1920)
	root.content_scale_mode = Window.CONTENT_SCALE_MODE_VIEWPORT
	root.content_scale_aspect = Window.CONTENT_SCALE_ASPECT_KEEP
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--evidence="):
			evidence = argument.trim_prefix("--evidence=")
	if not evidence.is_empty():
		DirAccess.make_dir_recursive_absolute(evidence)
	main = load("res://game/main.tscn").instantiate()
	main.args["preview"] = "map_view:cards"
	root.add_child(main)
	main.args["smoke"] = "1"
	main.args["capture"] = "1"
	await process_frame
	var before := JSON.stringify(main.profile.to_dict())
	var popups: GDScript = load("res://game/popups.gd")
	for language in ["en", "zh"]:
		UiText.set_language(language)
		for count in [1, 2, 9, 99]:
			var boosters: Array = []
			for i in count:
				boosters.append(0)
			boosters.append(3)
			var modal: Control = popups.win(main.popups, 3, 12, 12, 120, boosters, func(): pass, func(): pass, func(): pass, true)
			await process_frame
			for kind in [0, 3]:
				var drop: Control = modal.content.get_node("Drop%d" % kind)
				var quantity: UiText.NumberSprite = drop.get_node("Quantity")
				check(quantity.value == "+%d" % (count if kind == 0 else 1), "quantity preserves grouped grant")
				check(quantity.style == "cream", "accepted cream numeral images")
				check(quantity.material == null, "no glow shader or additive material")
				check(quantity.size == Vector2(100, 56), "one fixed quantity box")
				check(quantity.cap == 56.0, "legible unboxed numeral height")
				check(quantity.mouse_filter == Control.MOUSE_FILTER_IGNORE, "quantity never intercepts controls")
				check(drop.get_child_count() == 2, "only retained item and painted quantity; no badge or glow")
				check(not drop.has_node("PlusBadge"), "old glossy reward badge absent")
				check(quantity.get_rect().end.y <= popups.DROP_SIDE, "number clears next action row")
				check(quantity.get_rect().end.x < popups.DROP_SIDE + popups.DROP_GAP, "number clears adjacent item")
			check(JSON.stringify(main.profile.to_dict()) == before, "preview does not grant currency or items")
			if not evidence.is_empty() and count in [1, 99]:
				await RenderingServer.frame_post_draw
				var image := root.get_texture().get_image()
				check(image.get_size() == Vector2i(1080, 1920), "native render dimensions")
				check(image.save_png(evidence + "/reward_%d_%s.png" % [count, language]) == OK, "actual screenshot saved")
			modal.close()
			var reference: WeakRef = weakref(modal)
			var deadline := Time.get_ticks_msec() + 2000
			while reference.get_ref() != null and Time.get_ticks_msec() < deadline:
				await process_frame
			check(reference.get_ref() == null, "modal cleanup")
	# Release the test fixture's music streams before quitting the graphical renderer.
	var sound: Node = root.get_node("Sound")
	sound.set_music_muted(true)
	await create_timer(0.25).timeout
	sound._music.stream = null
	sound._old_music.stream = null
	main.queue_free()
	await process_frame
	await process_frame
	print("reward quantity checks: %d checks, %d failures" % [checks, failures])
	quit.call_deferred(0 if failures == 0 else 1)
