extends SceneTree
## Exercise the canonical carousel at real render sizes and test the actual shader alpha.
var failures := 0
var checks := 0
var evidence := ""
var device := Vector2i(1080, 1920)

func _initialize() -> void:
	run.call_deferred()

func check(ok: bool, label: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		push_error(label)

func run() -> void:
	for arg in OS.get_cmdline_user_args():
		if arg.begins_with("--evidence="):
			evidence = arg.trim_prefix("--evidence=")
		if arg.begins_with("--device="):
			var parts := arg.trim_prefix("--device=").split("x")
			device = Vector2i(int(parts[0]), int(parts[1]))
	if not evidence.is_empty():
		DirAccess.make_dir_recursive_absolute(evidence)
	# Prevent the desktop window manager from shrinking the requested render target.
	root.min_size = device
	root.size = device
	root.content_scale_size = Vector2i(1080, 1920)
	root.content_scale_mode = Window.CONTENT_SCALE_MODE_CANVAS_ITEMS
	root.content_scale_aspect = Window.CONTENT_SCALE_ASPECT_KEEP
	var main: Control = load("res://game/main.tscn").instantiate()
	main.args["preview"] = "map_view:cards"
	root.add_child(main)
	main.args["smoke"] = "1"
	main.args["capture"] = "1"
	await process_frame
	var map: Control = main.screen
	var fake := Profile.new()
	fake.coins = 1280
	fake.language = "en"
	fake.highest_unlocked = Levels.COUNT
	UiText.set_language("en")
	await process_frame
	map.setup(fake)
	var before := JSON.stringify(fake.to_dict())
	for world in range(1, Progress.world_of_level(Levels.COUNT) + 1):
		map.show_world(world, true)
		await process_frame
		var card: Control = map._cards[world - 1]
		var window: Control = card.get_node("Window")
		var painting: TextureRect = window.get_node("Painting")
		check(window.get_rect() == map.WINDOW, "Existing image window unchanged")
		check(painting.texture == Art.tex("card_scene_w%d" % world), "Retained world source %d" % world)
		check(is_equal_approx(painting.size.x / painting.size.y, 2.0 / 3.0), "Image remains uniformly scaled")
		check(painting.material.shader == map.CARD_CLIP, "All worlds share one clip shader")
		check(painting.material.get_shader_parameter("window_size") == map.WINDOW.size, "Mask uses window coordinates")
		check(painting.material.get_shader_parameter("image_offset") == painting.position, "Crop offset stays registered")
		check(painting.material.get_shader_parameter("radius") == map.WINDOW_RADIUS, "One corner radius in every state")
		check(card.get_node("Frame").get_index() < window.get_index() and card.get_node("Chosen").get_index() < window.get_index(), "Filled frame stays behind painting")
		check(card.get_node("Chosen").visible and map._world == world, "Selection remains functional")
		check(map._play_button.visible and not map._play_button.disabled, "One enabled shared Play")
		if not evidence.is_empty():
			await verify_alpha(painting, map.WINDOW.size)
			await capture("world_%02d" % world)
	check(JSON.stringify(fake.to_dict()) == before, "Presentation never mutates profile")
	fake.highest_unlocked = 1
	map.setup(fake)
	map.show_world(5, true)
	check(map._play_button.disabled, "Locked selected world keeps disabled Play")
	check(map._cards[4].get_node("Window").modulate == Color(0.62, 0.62, 0.66), "Locked shading unchanged")
	await capture("world_locked")
	if not evidence.is_empty():
		main.args.erase("capture")
		map.show_world(4, true)
		map.show_world(5, false)
		var frames: Array[Image] = []
		var start := Time.get_ticks_msec()
		for index in 6:
			await RenderingServer.frame_post_draw
			frames.append(root.get_texture().get_image())
			print("SWIPE %.3f scroll=%.2f" % [(Time.get_ticks_msec() - start) / 1000.0, map._scroll_x])
			await create_timer(0.06).timeout
		for index in frames.size():
			frames[index].save_png(evidence.path_join("swipe_%02d.png" % index))
	print("map card clipping: %d checks, %d failures; window=%s final_transform=%s" % [checks, failures, root.size, root.get_final_transform()])
	var sound: Node = root.get_node("Sound")
	sound.set_music_muted(true)
	await create_timer(0.25).timeout
	sound._music.stream = null
	sound._old_music.stream = null
	main.queue_free()
	await process_frame
	await process_frame
	quit.call_deferred(1 if failures else 0)

func verify_alpha(painting: TextureRect, size: Vector2) -> void:
	# Render the canonical painting node, unchanged, in a transparent GPU test target.
	var viewport := SubViewport.new()
	viewport.size = Vector2i(size)
	viewport.transparent_bg = true
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	root.add_child(viewport)
	var sample := painting.duplicate()
	viewport.add_child(sample)
	await RenderingServer.frame_post_draw
	var pixels := viewport.get_texture().get_image()
	for point in [Vector2i(2, 2), Vector2i(size.x - 3, 2), Vector2i(2, size.y - 3), Vector2i(size.x - 3, size.y - 3)]:
		check(pixels.get_pixelv(point).a < 0.01, "Painting corner is transparent")
	for point in [Vector2i(size.x / 2, 4), Vector2i(4, size.y / 2), Vector2i(size.x - 5, size.y / 2), Vector2i(size.x / 2, size.y - 5), Vector2i(size / 2)]:
		check(pixels.get_pixelv(point).a > 0.99, "Interior painting remains opaque")
	viewport.queue_free()
	await process_frame

func capture(name: String) -> void:
	if evidence.is_empty():
		return
	await RenderingServer.frame_post_draw
	var frame := root.get_texture().get_image()
	print("CAPTURE %s size=%s" % [name, frame.get_size()])
	frame.save_png(evidence.path_join(name + ".png"))
