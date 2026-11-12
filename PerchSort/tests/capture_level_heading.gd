extends SceneTree
## Real GL captures of canonical level cards and retained Close references.
var main: Control
var folder: String
var measurements: Array = []

func _initialize() -> void:
	_run.call_deferred()

func capture(label: String) -> void:
	await create_timer(0.25).timeout
	await RenderingServer.frame_post_draw
	var image := root.get_texture().get_image()
	assert(image.get_size() == Vector2i(1080, 1920))
	assert(image.save_png(folder + "/screens/" + label + ".png") == OK)
	print("CAPTURE ", label, " 1080x1920")

func _run() -> void:
	root.content_scale_size = Vector2i(1080, 1920)
	root.content_scale_mode = Window.CONTENT_SCALE_MODE_VIEWPORT
	root.content_scale_aspect = Window.CONTENT_SCALE_ASPECT_KEEP
	folder = ProjectSettings.globalize_path("res://../build/level-heading")
	DirAccess.make_dir_recursive_absolute(folder + "/screens")
	main = load("res://game/main.tscn").instantiate()
	var fake := Profile.new()
	fake.coins = 1280
	fake.muted = true
	fake.music_muted = true
	main.args["profile"] = JSON.stringify(fake.to_dict())
	main.args["preview"] = "coin_shop_view:normal"
	root.add_child(main)
	main.args["capture"] = "1"
	main.args["smoke"] = "1"
	await process_frame
	var popups: GDScript = load("res://game/popups.gd")
	for language in ["en", "zh"]:
		UiText.set_language(language)
		for state in ["level_card_first_visit", "level_card_full_roster", "level_card_locked", "level_card_twelve"]:
			if state == "level_card_twelve":
				main._clear_popups()
				var species: Array[String] = ["sunny", "sky", "berry"]
				var mechanics: Array[String] = []
				popups.level_card(main.popups, 12, 2, 15, 12, species, mechanics, func(): pass, true)
			else:
				popups.preview(main, state)
			await capture(state + "_" + language)
			var dialog: Control = main.popups.get_child(0)
			var close: Control = dialog.content.get_node("Close")
			measurements.append({"state": state, "language": language, "panel": str(dialog.panel_rect), "close": str(close.get_rect()), "close_size": str(close.size)})
		for state in ["normal", "purchase_preview"]:
			main._clear_popups()
			load("res://game/coin_shop_view.gd").preview(main, state)
			await capture("coin_shop_" + state + "_" + language)
	var file := FileAccess.open(folder + "/measurements.json", FileAccess.WRITE)
	file.store_string(JSON.stringify(measurements, "\t"))
	UiText.set_language("en")
	popups.preview(main, "level_card_full_roster")
	var dialog: Control = main.popups.get_child(0)
	await process_frame
	# Canonical previews populate their fake display profile; compare only the Close interaction.
	var saved := JSON.stringify(main.profile.to_dict())
	print("POINTER READY: click full-roster Level 100 Close above the frame")
	await dialog.closed
	await process_frame
	assert(main.popups.get_child_count() == 0)
	assert(JSON.stringify(main.profile.to_dict()) == saved)
	print("POINTER PASS: outside-frame Close dismissed without Play or profile mutation")
	main.queue_free()
	await process_frame
	quit()
