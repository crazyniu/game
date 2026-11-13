extends SceneTree
## Timed production BirdView actions on the canonical branch, without screenshot-write stalls.

func _initialize() -> void:
	run.call_deferred()

func run() -> void:
	root.content_scale_size = Vector2i(1080, 1920)
	root.content_scale_mode = Window.CONTENT_SCALE_MODE_VIEWPORT
	var output := "res://../build/tree-canopy/action-motion"
	for arg in OS.get_cmdline_user_args():
		if arg.begins_with("--evidence="):
			output = arg.trim_prefix("--evidence=")
	DirAccess.make_dir_recursive_absolute(output)
	var main: Control = load("res://game/main.tscn").instantiate()
	main.args["preview"] = "map_view:world_page"
	root.add_child(main)
	main.args["smoke"] = "1"
	await create_timer(0.3).timeout
	var map: Control = main.screen
	var bird = map._bird
	var images: Array[Image] = []
	var seen := {}
	bird.showcase()
	var started := Time.get_ticks_msec()
	for frame in 32:
		await create_timer(0.2).timeout
		await RenderingServer.frame_post_draw
		images.append(root.get_texture().get_image().get_region(Rect2i(720, 610, 260, 340)))
		seen[bird._sprite.texture.resource_path] = true
		print("ACTION %.3f texture=%s scale=%s body=%s" % [(Time.get_ticks_msec() - started) / 1000.0,
			bird._sprite.texture.resource_path, bird._body.scale, bird._body.position])
	for index in images.size():
		images[index].save_png(output.path_join("action_%02d.png" % index))
	print("tree action capture: %d unique production frames" % seen.size())
	var success: bool = seen.size() >= 5
	main.queue_free()
	await process_frame
	quit.call_deferred(0 if success else 1)
