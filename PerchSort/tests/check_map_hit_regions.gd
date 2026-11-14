extends SceneTree
## Canonical hit regions and viewport dispatch across every current-level state.
var checks := 0
var failures := 0
var tapped: Array[int] = []

func _initialize() -> void:
	run.call_deferred()

func check(ok: bool, label: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		push_error(label)

func run() -> void:
	root.content_scale_size = Vector2i(1080, 1920)
	root.content_scale_mode = Window.CONTENT_SCALE_MODE_VIEWPORT
	var main: Control = load("res://game/main.tscn").instantiate()
	main.args["preview"] = "map_view:cards"
	root.add_child(main)
	main.args["smoke"] = "1"
	main.args["capture"] = "1"
	var map: Control = main.screen
	map.level_tapped.connect(func(level: int): tapped.append(level))
	for current in range(1, Levels.COUNT + 1):
		var p := Profile.new()
		p.highest_unlocked = current
		for level in range(1, current):
			p.stars[level] = 2
		map.setup(p)
		var world := Progress.world_of_level(current)
		map.open_world(world)
		await process_frame
		var levels := range((world - 1) * Levels.PER_WORLD + 1, world * Levels.PER_WORLD + 1)
		var regions := {}
		for level in levels:
			var hit: Button = map._page.get_node("Level %d/Tap" % level)
			var at: Vector2 = map._node_pos(level)
			var rect := hit.get_global_rect()
			regions[level] = rect
			check(rect == map._hit_bounds(at), "Actual hit rectangle %d/%d" % [current, level])
			check(Rect2(0, 0, 1080, 1920).encloses(rect), "Hit region inside canvas %d/%d" % [current, level])
			check(rect.size == map._hit_bounds(Vector2.ZERO).size, "Uniform hit size %d/%d" % [current, level])
			check(rect.encloses(map._landing_bounds(at)), "Complete landing coverage %d/%d" % [current, level])
			var top: Vector2 = map._bird_feet(at)
			check(rect.encloses(map._star_bounds(top)), "Star row coverage %d/%d" % [current, level])
		for a in levels:
			for b in levels:
				if b > a:
					check(not regions[a].intersects(regions[b]), "No hit overlap %d/%d/%d" % [current, a, b])
		for level in levels:
			await click_and_check(map._node_pos(level), current, regions)
			var rect: Rect2 = regions[level]
			# Exact upper/lower boundaries and each side verify Godot's half-open hit dispatch.
			for y in [rect.position.y - 0.25, rect.position.y, rect.position.y + 0.25, rect.end.y - 0.25, rect.end.y, rect.end.y + 0.25]:
				await click_and_check(Vector2(rect.get_center().x, y), current, regions)
		# Probe the small gap at each same-column tier boundary, independent of sibling order.
		for level in levels:
			var rect: Rect2 = regions[level]
			await click_and_check(Vector2(rect.get_center().x, rect.end.y + 1.0), current, regions)
	print("map hit regions: %d checks, %d failures; all%d current levels" % [checks, failures, Levels.COUNT])
	main.queue_free()
	await process_frame
	quit.call_deferred(1 if failures else 0)

func click_and_check(point: Vector2, current: int, regions: Dictionary) -> void:
	var expected: Array[int] = []
	for level in regions:
		if regions[level].has_point(point) and level <= current:
			expected.append(level)
	tapped.clear()
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
	check(tapped == expected, "Point dispatch current%d at%s expected%s got%s" % [current, point, expected, tapped])
