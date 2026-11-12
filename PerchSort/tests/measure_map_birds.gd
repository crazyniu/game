extends SceneTree
## Derive layout metadata from the exact production frame sets used on level markers.

func _initialize() -> void:
	run.call_deferred()

func run() -> void:
	var bird_script: GDScript = load("res://game/bird_view.gd")
	var species_data := {}
	for level in range(1, Levels.COUNT + 1):
		var ids := Levels.species_ids(Levels.create_board(Levels.get_level(level)))
		var species: String = ids[0]
		if species_data.has(species):
			continue
		var bird: Node2D = bird_script.new()
		root.add_child(bird)
		bird.setup(Board.bird(0, Board.species_index(species), 0), false, 1.0)
		var bounds := Rect2i()
		for texture in bird._frame_names:
			var ink: Rect2i = texture.get_image().get_used_rect()
			bounds = ink if bounds.size == Vector2i.ZERO else bounds.merge(ink)
		var front := Art.tex(species + "_front").get_image().get_used_rect()
		species_data[species] = {"animation": [bounds.position.x, bounds.position.y, bounds.size.x, bounds.size.y],
			"front": [front.position.x, front.position.y, front.size.x, front.size.y], "frames": bird._frame_names.size()}
		bird.free()
	var data := {"canvas": bird_script.CANVAS, "feet_y": bird_script.FEET_Y, "species": species_data}
	var file := FileAccess.open("res://layout/map_bird_geometry.json", FileAccess.WRITE)
	file.store_string(JSON.stringify(data, "\t") + "\n")
	print("Measured %d map species from exact production animation sets" % species_data.size())
	quit.call_deferred()
