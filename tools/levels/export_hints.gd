extends SceneTree
## Precomputes each level's hint route from its starting position, so the first hint of a level never waits.
## godot --headless --path PerchSort -s ../tools/levels/export_hints.gd -- [level id ...]
## Writes PerchSort/levels/hints.json: {"<level id>": [[search key ints...], [from, to]], ...}; the board merges
## it into its hint cache when the level starts. Raises each level's par to cover its exported route.
## With level ids, replaces only their routes; untouched entries and pars are preserved.
## Rerun whenever levels.json or the rules change.

func _init() -> void:
	var selected := OS.get_cmdline_user_args()
	var out: Dictionary = {} if selected.is_empty() else JSON.parse_string(FileAccess.get_file_as_string("res://levels/hints.json"))
	var missing := []
	for level in Levels.all():
		var id := int(level["id"])
		if not selected.is_empty() and not selected.has(str(id)):
			continue
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
		var session := GameSession.new(level)
		for move in found["moves"]:
			session.try_action(move)
		level["par"] = maxi(int(level["par"]), session.move_count())
	if not missing.is_empty():
		print("hints not written: unsolved %s" % [missing])
		quit(1)
		return
	var catalog := FileAccess.open(Levels.PATH, FileAccess.WRITE)
	catalog.store_string(JSON.stringify({"levels": Levels.all()}))
	catalog.close()
	var f := FileAccess.open("res://levels/hints.json", FileAccess.WRITE)
	f.store_string(JSON.stringify(out))
	f.close()
	print("hints: %d levels, unsolved %s" % [out.size(), missing])
	quit(0 if missing.is_empty() else 1)
