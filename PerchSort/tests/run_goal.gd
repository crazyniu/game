extends SceneTree
## Partial goal (level field "goal"): win rule, validator, undo across the win, the three trial levels at their par,
## and the bird keys under the house (only the named kinds, each with a ring that the check fills).
var checks := 0
var failures := 0

func _initialize() -> void:
	_run.call_deferred()

func check(ok: bool, message: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		print("FAIL ", message)

func level(goal: Array, branches: Array) -> Dictionary:
	var def := {"id": 9100, "world": 1, "variant": "noon", "capacity": 4, "par": 1, "branches": []}
	if not goal.is_empty():
		def["goal"] = goal
	var sides := ["L", "R"]
	for i in branches.size():
		def["branches"].append({"side": sides[i % 2], "row": i / 2, "birds": branches[i]})
	return def

func _run() -> void:
	_validator()
	_win_and_undo()
	_trial_levels()
	await _key_row()
	print("run_goal: %d checks, %d failures" % [checks, failures])
	quit(1 if failures > 0 else 0)

func _validator() -> void:
	var mint4 := ["mint", "mint", "mint", "mint"]
	check(Levels.create_board(level(["mint"], [mint4, ["sunny", "sunny", "cocoa"], []])) != null, "leftovers of one and two birds are accepted")
	check(Levels.create_board(level([], [mint4, ["sunny", "sunny", "cocoa"], []])) == null, "without a goal every kind must be a full flock")
	check(Levels.create_board(level(["mint"], [["mint", "mint", "mint"], ["sunny"], []])) == null, "a named kind needs exactly capacity birds")
	check(Levels.create_board(level(["mint"], [mint4, ["sunny", "sunny", "sunny", "sunny"], ["sunny"]])) == null, "no kind has more than capacity birds")
	check(Levels.create_board(level(["mint"], [mint4, ["sunny", "sunny", "sunny", "sunny"], []])) != null, "an unnamed full flock is allowed")
	check(Levels.create_board(level(["cocoa"], [mint4, ["sunny"], []])) == null, "a named kind must be on the board")
	check(Levels.create_board(level(["parrot"], [mint4, ["sunny"], []])) == null, "an unknown named kind is refused")
	check(Levels.create_board(level(["crow"], [mint4, ["crow"], []])) == null, "crows cannot be named")
	check(Levels.create_board(level(["mint", "mint"], [mint4, ["sunny"], []])) == null, "a kind is named once")
	var board := Levels.create_board(level(["mint"], [mint4, ["sunny", "sunny", "cocoa"], []]))
	check(board.goal_mask == 1 << Board.species_index("mint"), "the goal mask holds the named kind")
	check(board.clone().goal_mask == board.goal_mask, "clone keeps the goal")
	var keyless := board.clone()
	keyless.goal_mask = 0
	check(board.search_key(false) == keyless.search_key(false), "the goal is not part of the search key")
	# Every catalog level still builds without a goal.
	var plain := 0
	for def in Levels.all():
		var b := Levels.create_board(def)
		if b != null and b.goal_mask == 0:
			plain += 1
	check(plain == Levels.COUNT, "all %d catalog levels build without a goal (%d)" % [Levels.COUNT, plain])

func _win_and_undo() -> void:
	# Mint is named; one move sends the mint flock home and wins with sunny and cocoa still on the board.
	var def := level(["mint"], [["mint", "mint", "mint"], ["sunny", "mint"], ["cocoa", "sunny"], []])
	var session := GameSession.new(def)
	check(not session.state.is_won(), "not won at the start")
	check(session.try_move(1, 0), "mint joins the mint branch")
	check(session.state.is_won(), "won when the named flock is home")
	var birds_left := 0
	for list in session.state.items:
		birds_left += list.size()
	check(birds_left == 3, "the leftovers are still on the board at the win (%d)" % birds_left)
	check(session.events.filter(func(e): return e["type"] == "won").size() == 1, "one won event")
	check(not Rules.has_legal_move(session.state) and not session.select(2), "no move after the win")
	check(session.undo(), "undo after the win")
	check(not session.state.is_won() and session.state.homed.is_empty(), "undo crosses back over the win")
	check(session.try_move(1, 0) and session.state.is_won(), "the same move wins again")
	session.restart()
	check(not session.state.is_won(), "restart clears the win")
	# Without a goal the same board can never be won (the leftovers cannot go home), so the goal decides the win.
	var plain := Levels.create_board(def)
	plain.goal_mask = 0
	check(Solver.solve(plain)["status"] == Solver.Status.UNSOLVABLE, "without the goal the board has no win")

func _trial_levels() -> void:
	var count := Levels.trial_count()
	check(count == 3, "three trial levels (%d)" % count)
	check(Levels.trial(0).is_empty() and Levels.trial(count + 1).is_empty(), "no trial level out of range")
	var rules := Rules.new()
	for n in range(1, count + 1):
		var def := Levels.trial(n)
		var board := Levels.create_board(def)
		check(board != null and board.goal_mask != 0, "trial %d builds with a goal" % n)
		if board == null:
			continue
		check(int(def["id"]) > Levels.COUNT, "trial %d id is outside the catalog" % n)
		var counts := {}
		for list in board.items:
			for code in list:
				var sp := Board.species_of(code)
				counts[sp] = counts.get(sp, 0) + 1
		for sp in counts:
			check(board.in_goal(sp) or counts[sp] <= 2, "trial %d: leftover %s has at most two birds" % [n, Board.SPECIES[sp]])
		var plan := Solver.solve(board, [], 400000, 60000)
		check(plan["status"] == Solver.Status.SOLVED, "trial %d is solvable without tools" % n)
		if plan["status"] != Solver.Status.SOLVED:
			continue
		check(plan["moves"].size() == int(def["par"]), "trial %d solved at its par %d (%d)" % [n, int(def["par"]), plan["moves"].size()])
		var session := GameSession.new(def)
		for move in plan["moves"]:
			session.try_action(move)
		check(session.state.is_won() and session.move_count() == int(def["par"]), "trial %d replays to a win at par" % n)
	var goals := [Levels.trial(1).get("goal", []), Levels.trial(2).get("goal", []), Levels.trial(3).get("goal", [])]
	check(goals[0].size() == 1 and goals[1].size() == 2 and goals[2].size() == 1, "one, two and one named kinds")
	var special := false
	for branch in Levels.trial(3).get("branches", []):
		for token in branch["birds"]:
			special = special or str(token).contains("sleep") or str(token).begins_with("egg:")
	check(special, "trial 3 has a sleeper or an egg")

func _key_row() -> void:
	var main: Control = load("res://game/main.tscn").instantiate()
	var fake := Profile.new()
	fake.muted = true
	fake.music_muted = true
	main.args["profile"] = JSON.stringify(fake.to_dict())
	main.args["preview"] = "coin_shop_view:normal"
	root.add_child(main)
	main.args["smoke"] = "1"
	main.args["capture"] = "1"
	await process_frame
	var board: Control = load("res://game/board_view.gd").new()
	board.instant = true
	main.set_screen(board)
	# Trial 2: two named kinds, so the row holds exactly those two keys with empty rings.
	var def := Levels.trial(2)
	board.start(def, main.profile, false)
	var row: Control = board._skill_row
	var names: Array = []
	for key in row.get_children():
		names.append(str(key.name).trim_prefix("Key_"))
	var expected: Array = def["goal"].duplicate()
	expected.sort_custom(func(a, b): return Board.SPECIES.find(a) < Board.SPECIES.find(b))
	check(names == expected, "only the named kinds have keys: %s" % [names])
	for key in row.get_children():
		check(key.get_node_or_null("Ring") != null and key.get_node("Ring").visible, "%s shows an empty ring" % key.name)
		check(not key.get_node("Home").visible, "%s ring is empty at the start" % key.name)
		check(key.get_child_count() > 0 and key.get_node("Home").get_index() > key.get_node("Ring").get_index(), "the check sits over the ring")
	# Play the route: each ring fills when its flock is home.
	var plan := Solver.solve(board.session.state, [], 400000, 60000)
	var filled := {}
	for move in plan["moves"]:
		board.session.try_action(move)
		board.refresh_hud()
		for key in row.get_children():
			var sp := Board.species_index(str(key.name).trim_prefix("Key_"))
			check(key.get_node("Home").visible == board.session.state.homed.has(sp), "%s ring follows its flock" % key.name)
			if key.get_node("Home").visible:
				filled[key.name] = true
	check(filled.size() == 2 and board.session.state.is_won(), "both rings filled at the win")
	board.session.undo()
	board.refresh_hud()
	var open := 0
	for key in row.get_children():
		open += 0 if key.get_node("Home").visible else 1
	check(open == 1, "undo across the win empties the last ring again")
	# A catalog level keeps one key per kind and no rings.
	board.start(Levels.get_level(12), main.profile, false)
	var fresh := Levels.create_board(Levels.get_level(12))
	check(board._skill_row.get_child_count() == Levels.species_ids(fresh, true).size(), "a catalog level keeps a key per kind")
	var rings := 0
	for key in board._skill_row.get_children():
		rings += 1 if key.get_node_or_null("Ring") != null else 0
	check(rings == 0, "a catalog level has no rings")
