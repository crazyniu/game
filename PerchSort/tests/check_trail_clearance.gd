extends SceneTree
## All 100 current-level states use real animation alpha unions, including live idle movement.

func _initialize() -> void:
	run.call_deferred()

func run() -> void:
	var map: Control = load("res://game/map_view.gd").new()
	var bird_script: GDScript = load("res://game/bird_view.gd")
	var species_ids: Array = JSON.parse_string(FileAccess.get_file_as_string(map.BIRD_GEOMETRY_PATH))["species"].keys()
	var cache := {}
	var failures := 0
	for trial in range(Levels.COUNT * species_ids.size()):
		var level: int = trial / species_ids.size() + 1
		map.current_level = level
		var center: Vector2 = map._node_pos(level)
		var feet: Vector2 = map._bird_feet(center)
		var species: String = species_ids[trial % species_ids.size()]
		if not cache.has(species):
			var bird: Node2D = bird_script.new()
			root.add_child(bird)
			bird.setup(Board.bird(0, Board.species_index(species), 0), false, 1.0)
			var ink := Rect2()
			for texture in bird._frame_names:
				var used := Rect2(texture.get_image().get_used_rect())
				ink = used if ink.size == Vector2.ZERO else ink.merge(used)
			var stored: Array = map._bird_spec(species)["animation"]
			if ink != Rect2(stored[0], stored[1], stored[2], stored[3]):
				push_error("Animation metadata is stale: " + species)
				failures += 1
			cache[species] = ink
			bird.free()
		var scale: float = map._bird_fit(species)
		var density: float = scale * bird_script.CANVAS / bird_script.DISPLAY
		var origin: Vector2 = map._bird_origin(feet, species)
		var local: Rect2 = cache[species]
		local.position -= Vector2(bird_script.CANVAS * 0.5, bird_script.FEET_Y)
		var animated_scale := Vector2(scale * 1.10, scale * 1.08)
		var bird := Rect2(origin + local.position * animated_scale, local.size * animated_scale)
		bird = bird.grow_individual(2, 16.0 * density + 2, 2, 2)
		if not Rect2(0, 300, 1080, 1620).encloses(bird):
			push_error("Bird leaves content bounds at level %d: %s" % [level, bird])
			failures += 1
		if bird.intersects(Rect2(350, 300, 380, 80)):
			push_error("Bird intersects centred progress at level %d" % level)
			failures += 1
		if not map._hit_bounds(center).encloses(bird):
			push_error("Live bird leaves its hit region at level %d: %s" % [level, bird])
			failures += 1
		var world: int = Progress.world_of_level(level)
		for other in range((world - 1) * Levels.PER_WORLD + 1, world * Levels.PER_WORLD + 1):
			if other == level:
				continue
			var at: Vector2 = map._node_pos(other)
			var marker: Rect2 = map._landing_bounds(at)
			var top: Vector2 = map._bird_feet(at)
			var status: Rect2 = map._star_bounds(top) if other < level else Rect2(top - Vector2(30, 56), Vector2(60, 60))
			if bird.intersects(marker) or bird.intersects(status):
				push_error("Current bird %d intersects marker/status %d" % [level, other])
				failures += 1
	print("tree trail clearance: 100 current levels x %d species, %d failures" % [cache.size(), failures])
	map.free()
	quit.call_deferred(1 if failures else 0)
