extends SceneTree
## Canonical GUI geometry, localization and actual captures of the map node correction.
var failures := 0
var checks := 0
var evidence := ""
var main: Control
var all_levels := false
var compact := false
var device := Vector2i.ZERO

func _initialize() -> void:
	run.call_deferred()

func check(ok: bool, label: String) -> void:
	checks += 1
	failures += 0 if ok else 1
	if not ok:
		push_error(label)

func visible_rect(image: TextureRect) -> Rect2:
	var used := Rect2(image.texture.get_image().get_used_rect())
	var scale := image.size / image.texture.get_size()
	return Rect2(image.global_position + used.position * scale, used.size * scale)

func run() -> void:
	root.content_scale_size = Vector2i(1080, 1920)
	root.content_scale_mode = Window.CONTENT_SCALE_MODE_VIEWPORT
	root.content_scale_aspect = Window.CONTENT_SCALE_ASPECT_KEEP
	for arg in OS.get_cmdline_user_args():
		if arg.begins_with("--evidence="):
			evidence = arg.trim_prefix("--evidence=")
		if arg == "--all-levels":
			all_levels = true
		if arg == "--compact":
			compact = true
		if arg.begins_with("--device="):
			var parts := arg.trim_prefix("--device=").split("x")
			device = Vector2i(int(parts[0]), int(parts[1]))
	if device != Vector2i.ZERO:
		root.min_size = device
		root.size = device
		root.content_scale_mode = Window.CONTENT_SCALE_MODE_CANVAS_ITEMS
	if not evidence.is_empty():
		DirAccess.make_dir_recursive_absolute(evidence)
	main = load("res://game/main.tscn").instantiate()
	main.args["preview"] = "map_view:cards"
	root.add_child(main)
	main.args["smoke"] = "1"
	main.args["capture"] = "1"
	await process_frame
	var map: Control = main.screen
	for language in ["zh", "en"]:
		UiText.set_language(language)
		await process_frame
		var cases: Array = range(1, 101) if all_levels else ([1, 8, 100] if compact else [1, 8, 10, 12, 25, 35, 45, 55, 65, 75, 85, 95, 100])
		for current in cases:
			var p := Profile.new()
			p.language = language
			p.coins = 1280
			p.highest_unlocked = current
			for level in range(1, current):
				p.stars[level] = 1 + level % 3
			var before := JSON.stringify(p.to_dict())
			map.setup(p)
			var world := Progress.world_of_level(current)
			map.open_world(world)
			await process_frame
			var tree: TextureRect = map._page.get_node("BranchTree")
			check(tree.position == map.TREE_ORIGIN and tree.size == map.TREE_SOURCE * map.TREE_SCALE, "Tree natural aspect and position")
			check(map._page.get_node_or_null("Painting") == null, "No road painting in tree page")
			var tree_ink := Rect2(map.TREE_ORIGIN + map.TREE_CONTENT.position * map.TREE_SCALE, map.TREE_CONTENT.size * map.TREE_SCALE)
			check(Rect2(0, 0, 1080, 1920).encloses(tree_ink), "Full tree roots and leaves inside screen")
			for level in range((world - 1) * Levels.PER_WORLD + 1, world * Levels.PER_WORLD + 1):
				var layer: Control = map._page.get_node("Level %d" % level)
				var node: Control = layer.get_node("Node")
				var ink := node.get_global_rect()
				check(is_equal_approx(ink.size.x, map.NODE), "Same branch landing width %d" % level)
				check(ink.end.y < 1920.0, "Complete landing inside screen %d" % level)
				check(layer.get_node("Tap").size.is_equal_approx(map.NODE_HIT_SIZE), "Same full marker hit area %d" % level)
				var number: Control = layer.get_node("Number")
				var number_ink := Rect2()
				for glyph in number.get_children():
					var used := visible_rect(glyph)
					number_ink = used if number_ink.size == Vector2.ZERO else number_ink.merge(used)
				check(number_ink.get_center().distance_to(map._node_pos(level)) < 0.01, "Actual numeral ink centred %d" % level)
				check(layer.get_node_or_null("glow_soft") == null, "No marker glow %d" % level)
				check(layer.z_index == 0, "Node does not escape modal layer %d" % level)
				if level < current:
					var previous := Rect2()
					var rating := Rect2()
					for index in 3:
						var picture: TextureRect = layer.get_node("Star_%d" % index)
						var star := visible_rect(picture)
						var source := Rect2(picture.texture.get_image().get_used_rect())
						check(is_equal_approx(star.size.x, map.STAR), "Star visible width %d/%d" % [level, index])
						check(is_equal_approx(star.size.x / star.size.y, source.size.x / source.size.y), "Star natural aspect %d/%d" % [level, index])
						check(absf(star.end.y - map._bird_feet(map._node_pos(level)).y) < 0.01, "Star lower ink meets branch %d/%d" % [level, index])
						check(layer.get_node("Tap").get_global_rect().grow(0.01).encloses(star), "Star remains inside input coverage %d/%d" % [level, index])
						if index > 0:
							check(absf(star.position.x - previous.end.x - map.STAR_GAP) < 0.01, "Actual star gap %d/%d" % [level, index])
						rating = star if index == 0 else rating.merge(star)
						previous = star
					check(absf(rating.get_center().x - map._node_pos(level).x) < 0.01, "Rating group centered on number %d" % level)
			var bird: Node2D = map._bird
			var used := Rect2(bird._sprite.texture.get_image().get_used_rect())
			var bird_center: Vector2 = bird.global_position + (used.get_center() + bird._sprite.offset) * bird._body.scale
			check(absf(bird_center.x - map._node_pos(current).x) < 0.5, "Actual bird ink is centred %d" % current)
			var feet: Vector2 = bird.global_position + Vector2(used.get_center().x + bird._sprite.offset.x, used.end.y + bird._sprite.offset.y) * bird._body.scale
			check(feet.distance_to(map._bird_feet(map._node_pos(current))) < 0.5, "Actual feet meet wood plane %d" % current)
			check(JSON.stringify(p.to_dict()) == before, "Layout does not mutate profile")
			await capture("trail_%d_%s" % [current, language])
			map.close_world()
			check(map._world == world and map._play_button.visible, "Back restores selected world")
			if current == 8:
				await capture("selector_%s" % language)
	main.args.erase("capture")
	var motion_profile := Profile.new()
	motion_profile.highest_unlocked = 8
	for level in range(1, 8):
		motion_profile.stars[level] = 1
	map.setup(motion_profile)
	map.open_world(1)
	var sound: Node = root.get_node("Sound")
	sound.set_music_muted(true)
	var started := Time.get_ticks_msec()
	for frame in 10:
		await create_timer(0.25).timeout
		print("MOTION %.2f texture=%s" % [(Time.get_ticks_msec() - started) / 1000.0, map._bird._sprite.texture.resource_path])
		await capture("motion_%02d" % frame)
	print("map node checks: %d checks, %d failures" % [checks, failures])
	main.queue_free()
	await process_frame
	await process_frame
	quit.call_deferred(1 if failures else 0)

func capture(label: String) -> void:
	if evidence.is_empty():
		return
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png(evidence.path_join(label + ".png"))
