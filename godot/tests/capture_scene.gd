extends SceneTree
## Actual viewport capture only. Headless dummy renderer is not visual evidence.
func _initialize() -> void:
	call_deferred("run")

func capture(path: String) -> void:
	await RenderingServer.frame_post_draw
	var image := root.get_texture().get_image()
	if image.is_empty():
		push_error("Renderer produced no image")
		quit(1)
		return
	var result := image.save_png(path)
	print("Capture %s: %s" % [path, error_string(result)])

func run() -> void:
	if DisplayServer.get_name() == "headless":
		push_error("Visual capture requires actual X11/Wayland/display renderer; headless is not evidence")
		quit(2)
		return
	var output := "/tmp/perch-godot.png"
	for arg in OS.get_cmdline_user_args():
		if arg.begins_with("--output="): output = arg.trim_prefix("--output=")
	var scene = load("res://scenes/first_level.tscn").instantiate()
	scene.test_save_directory = "/tmp/perch-capture-%d" % Time.get_ticks_usec()
	root.add_child(scene)
	await create_timer(0.5).timeout
	await capture(output)
	scene.pick(1)
	await create_timer(0.2).timeout
	await capture(output.replace(".png", "-selected.png"))
	scene.pick(0)
	await create_timer(0.95).timeout
	await capture(output.replace(".png", "-homing.png"))
	await create_timer(1.5).timeout
	scene.pick(1)
	scene.pick(2)
	await create_timer(2.7).timeout
	await capture(output.replace(".png", "-won.png"))
	scene.queue_free()
	await process_frame
	quit()
