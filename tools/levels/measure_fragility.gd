extends SceneTree
## Measures how punishing each level is: trap moves beside the stored route, and random-play outcomes.
func _init() -> void:
	var args := OS.get_cmdline_user_args()
	var lo := int(args[0]); var hi := int(args[1])
	var routes: Dictionary = JSON.parse_string(FileAccess.get_file_as_string("res://levels/hints.json"))
	var rules := Rules.new()
	rules.track_economy = false
	var rng := RandomNumberGenerator.new()
	for level in Levels.all():
		var id := int(level["id"])
		if id < lo or id > hi:
			continue
		var board: Board = Levels.create_board(level)
		rng.seed = id
		var alts := 0; var traps := 0; var unknown := 0; var first_trap := -1; var step := 0
		var cur := board.clone(false)
		for entry in routes[str(id)]:
			var rf := int(entry[1][0]); var rt := int(entry[1][1])
			var n := cur.branch_count()
			for from in n:
				for to in n + 1:
					var sw := to == n
					if sw:
						if not Rules.can_swivel(cur, from): continue
					elif not Rules.can_transfer(cur, from, to): continue
					if from == rf and ((sw and rt == -1) or (not sw and to == rt)): continue
					var nx := cur.clone(false)
					if sw: rules.swivel(nx, from, null)
					else: rules.transfer(nx, from, to, null)
					alts += 1
					var st: int = Solver.hint(nx, [], 6000, 250)["status"]
					if st == Solver.Status.UNSOLVABLE:
						traps += 1
						if first_trap < 0: first_trap = step
					elif st == Solver.Status.BUDGET_EXCEEDED:
						unknown += 1
			if rt == -1: rules.swivel(cur, rf, null)
			else: rules.transfer(cur, rf, rt, null)
			step += 1
		# random play
		var wins := 0; var dead := 0; var lost := 0; var plays := 40
		for p in plays:
			var b := board.clone(false)
			var last := [-1, -1]
			var cap := int(level["par"]) * 3 + 10
			var done := false
			for m in cap:
				var moves := []
				var n2 := b.branch_count()
				for from in n2:
					if Rules.can_swivel(b, from): moves.append([from, -1])
					for to in n2:
						if Rules.can_transfer(b, from, to) and not (from == last[1] and to == last[0]):
							moves.append([from, to])
				if moves.is_empty():
					dead += 1; done = true; break
				var mv: Array = moves[rng.randi() % moves.size()]
				if mv[1] == -1: rules.swivel(b, mv[0], null)
				else: rules.transfer(b, mv[0], mv[1], null)
				last = mv
				if b.is_won():
					wins += 1; done = true; break
			if not done:
				var st2: int = Solver.hint(b, [], 6000, 250)["status"]
				if st2 == Solver.Status.UNSOLVABLE: lost += 1
		print("ROW,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d" % [id, int(level["world"]), int(level["par"]), routes[str(id)].size(), alts, traps, unknown, first_trap, plays, wins, dead, lost])
	quit(0)
