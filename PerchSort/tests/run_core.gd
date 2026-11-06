extends SceneTree
## Headless core checks: godot --headless --path PerchSort -s res://tests/run_core.gd

var failures := 0

func check(cond: bool, label: String) -> void:
	if not cond:
		failures += 1
		print("FAIL ", label)

func _init() -> void:
	var t0 := Time.get_ticks_msec()
	var levels := Levels.all()
	check(levels.size() == 40, "40 levels")
	var total_states := 0
	for level in levels:
		var board := Levels.create_board(level)
		check(board != null, "level %d builds" % level["id"])
		if board == null:
			continue
		var t := Time.get_ticks_msec()
		var result := Solver.solve(board, [], 400000, 60000)
		var ms := Time.get_ticks_msec() - t
		var solved: bool = result["status"] == Solver.Status.SOLVED
		check(solved, "level %d solvable" % level["id"])
		# Replay the solution through a session and compare counted moves with par.
		var session := GameSession.new(level)
		for move in result["moves"]:
			check(session.try_action(move), "level %d replay move %s" % [level["id"], move])
		check(session.state.is_won(), "level %d replay wins" % level["id"])
		print("L%02d depth=%d counted=%d par=%d %dms" % [level["id"], result["moves"].size(), session.move_count(), level["par"], ms])
	print("core checks: %d failures in %d ms" % [failures, Time.get_ticks_msec() - t0])
	quit(1 if failures else 0)
