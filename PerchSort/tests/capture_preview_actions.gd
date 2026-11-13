extends SceneTree
## Actual six-pack preview renders and pointer confirmation on the canonical purchase page.

func _initialize() -> void:
	_run.call_deferred()

func _run() -> void:
	root.content_scale_size = Vector2i(1080, 1920)
	root.content_scale_mode = Window.CONTENT_SCALE_MODE_VIEWPORT
	root.content_scale_aspect = Window.CONTENT_SCALE_ASPECT_KEEP
	var main: Control = load("res://game/main.tscn").instantiate()
	var fake := Profile.new()
	fake.coins = 1280
	fake.muted = true
	fake.music_muted = true
	main.args["profile"] = JSON.stringify(fake.to_dict())
	main.args["preview"] = "coin_shop_view:normal"
	root.add_child(main)
	main.args["smoke"] = "1"
	main.args["capture"] = "1"
	await process_frame
	var before := JSON.stringify(main.profile.to_dict())
	var page: Control = load("res://game/coin_shop_view.gd").new()
	page.instant = true
	main.set_screen(page)
	page.setup(1280)
	var folder := ProjectSettings.globalize_path("res://../build/dialog-layout/purchases")
	DirAccess.make_dir_recursive_absolute(folder)
	for language in ["en", "zh"]:
		UiText.set_language(language)
		for pack in page.PACKS:
			page.show_purchase(pack, func(_bought): pass)
			var modal: Control = page.get_node("Popup")
			await RenderingServer.frame_post_draw
			var image := root.get_texture().get_image()
			assert(image.get_size() == Vector2i(1080, 1920))
			assert(image.save_png(folder + "/preview_%s_%s.png" % [pack.id, language]) == OK)
			modal.content.get_node("Confirm").pressed.emit()
			await create_timer(0.25).timeout
			assert(not page.has_node("Popup") and JSON.stringify(main.profile.to_dict()) == before)
			print("PREVIEW CAPTURE PASS ", pack.id, " ", language)
	UiText.set_language("en")
	page.show_purchase(page.PACKS[2], func(_bought): pass)
	var preview: Control = page.get_node("Popup")
	print("PREVIEW POINTER READY: click outside-frame OK")
	await preview.closed
	await process_frame
	assert(not page.has_node("Popup") and JSON.stringify(main.profile.to_dict()) == before)
	print("PREVIEW POINTER PASS: dismissed without payment, coin grant or save mutation")
	main.queue_free()
	await process_frame
	quit()
