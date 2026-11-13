extends SceneTree
## Dive talent: godot --headless --path PerchSort -s res://tests/run_dive.gd
## A run of Dive birds slips in under one bird of another species that sits on its own kind; eggs refuse it; undo
## restores the position; the lab levels are solvable at their par and the first one needs the talent.
var failures := 0

func check(cond: bool, label: String) -> void:
	if not cond:
		failures += 1
		print("FAIL ", label)

func _level(talent: bool) -> Dictionary:
	var def := {"id": 1, "capacity": 4, "par": 3, "world": 2, "variant": "noon", "branches": [
		{"side": "L", "row": 0, "birds": ["sky", "sky", "sunny"]},
		{"side": "R", "row": 0, "birds": ["sunny", "sky"]},
		{"side": "L", "row": 1, "birds": ["sunny", "sunny", "sky"]},
		{"side": "R", "row": 1, "birds": []}]}
	if talent:
		def["talents"] = {"sky": "dive"}
	return def

func _init() -> void:
	var plain := Levels.create_board(_level(false))
	check(not Rules.can_transfer(plain, 1, 0), "no dive without the talent")
	var session := GameSession.new(_level(true))
	var s := session.state
	check(Rules.can_transfer(s, 1, 0) and not Rules._lands_on_top(s, 1, 0), "a sky dives under the sunny on its own kind")
	check(not Rules.can_transfer(s, 0, 1), "an ordinary bird does not dive")
	check(Rules.can_transfer(s, 1, 2) and Rules._lands_on_top(s, 1, 2), "landing beside its own kind stays an ordinary move")
	var before := s.search_key(false)
	check(session.try_move(1, 0), "the dive is a move")
	s = session.state
	var names := []
	for code in s.items[0]:
		names.append(Board.SPECIES[Board.species_of(code)])
	check(names == ["sky", "sky", "sky", "sunny"], "the diver sits under the passed bird: %s" % [names])
	var passed := false
	for e in session.events:
		passed = passed or (e["type"] == "transferred" and e.has("passed") and e["to_seat"] == 2)
	check(passed, "the transfer event names the passed bird and the landing seat")
	check(session.move_count() == 1 and not s.is_won(), "one move, no homecoming under another bird")
	check(not Rules.can_transfer(s, 2, 0), "a full branch takes no dive")
	check(session.undo() and session.state.search_key(false) == before, "undo restores the position")
	# Eggs refuse a dive, as the tip and as the bird under it.
	var egg_tip := Levels.create_board(_level(true))
	egg_tip.set_bird(0, 2, egg_tip.items[0][2] | Board.EGG)
	check(not Rules.can_transfer(egg_tip, 1, 0), "no dive under an egg")
	var egg_under := Levels.create_board(_level(true))
	egg_under.set_bird(0, 1, egg_under.items[0][1] | Board.EGG)
	check(not Rules.can_transfer(egg_under, 1, 0), "no dive onto an egg")
	# Lab levels.
	var number := 1
	while not Levels.lab(number).is_empty():
		var def := Levels.lab(number)
		var board := Levels.create_board(def)
		check(board != null and board.dive_mask != 0, "lab %d builds with the talent" % number)
		var found := Solver.solve(board, [], 300000, 20000)
		check(found["status"] == Solver.Status.SOLVED and found["moves"].size() == int(def["par"]), "lab %d is solved at its par" % number)
		if number == 1:
			var off := board.clone(false)
			off.dive_mask = 0
			check(Solver.solve(off, [], 300000, 20000)["status"] == Solver.Status.UNSOLVABLE, "lab 1 cannot be won without a dive")
		number += 1
	check(number == 4, "three lab levels")
	print("dive checks: %d failures" % failures)
	quit(1 if failures else 0)
