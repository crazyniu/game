extends SceneTree
## Precomputes each level's hint route from its starting position, so the first hint of a level never waits.
## godot --headless --path PerchSort -s ../tools/levels/export_hints.gd
## Writes PerchSort/levels/hints.json: {"<level id>": [[search key ints...], [from, to]], ...}; the board merges
## it into its hint cache when the level starts. Rerun whenever levels.json or the rules change.

func _init() -> void:
	var out := {}
	var missing := []
	for level in Levels.all():
		var id := int(level["id"])
		var board := Levels.create_board(level)
		var found := Solver.hint(board, [], 400000, 60000)
		if found.get("status") != Solver.Status.SOLVED:
			missing.append(id)
			continue
		var route := []
		var cont: Dictionary = found.get("continuation", {})
		for key in cont:
			route.append([Array(key), cont[key]])
		out[str(id)] = route
	var f := FileAccess.open("res://levels/hints.json", FileAccess.WRITE)
	f.store_string(JSON.stringify(out))
	f.close()
	print("hints: %d levels, unsolved %s" % [out.size(), missing])
	quit(0 if missing.is_empty() else 1)
