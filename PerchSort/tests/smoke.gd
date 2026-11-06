extends RefCounted
## Smoke test run inside the real main scene (autoloads present): every script loads, every level builds.
## godot --headless --path PerchSort -- --smoke

static func run(main: Control) -> void:
	var failures := 0
	for dir in ["res://core", "res://meta", "res://game"]:
		for file in DirAccess.get_files_at(dir):
			if file.ends_with(".gd") and load("%s/%s" % [dir, file]) == null:
				failures += 1
				print("FAIL load %s/%s" % [dir, file])
	for level in Levels.all():
		main.open_board(int(level["id"]), false)
		await main.get_tree().process_frame
		if not main.screen is BoardView or main.screen.session.state.branch_count() == 0:
			failures += 1
			print("FAIL board %d" % level["id"])
	main.open_map()
	await main.get_tree().process_frame
	if main.screen == null or not main.screen.has_signal("level_tapped"):
		failures += 1
		print("FAIL map")
	print("smoke checks: %d failures" % failures)
	main.get_tree().quit(1 if failures else 0)
