extends SceneTree
func _init() -> void:
	for id in [23, 25, 26, 27]:
		var t := Time.get_ticks_msec()
		var r := Solver.solve(Levels.create_board(Levels.get_level(id)), [], 400000, 60000)
		print("L%d depth=%d %dms" % [id, r["moves"].size(), Time.get_ticks_msec() - t])
	quit()
