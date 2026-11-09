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
	_check_meta()
	print("core checks: %d failures in %d ms" % [failures, Time.get_ticks_msec() - t0])
	quit(1 if failures else 0)

## Save migration and the gift economy.
func _check_meta() -> void:
	# A schema 1 save: stars on old levels 1 and 9 (old 9 is level 4 now), best moves on old 9.
	var stars := []
	stars.resize(41)
	stars.fill(0)
	stars[1] = 3
	stars[9] = 2
	var moves := stars.duplicate()
	moves[9] = 7
	var old := Profile.from_dict({"schemaVersion": 1, "bestStars": stars, "clearCounts": stars, "bestMoves": moves})
	check(old.best_stars(1) == 3 and old.best_stars(4) == 2 and old.best_stars(9) == 0, "schema 1 stars renumbered")
	check(old.best_moves[4] == 7, "schema 1 best moves renumbered")
	var again := Profile.from_dict(old.to_dict())
	check(again.best_stars(4) == 2 and again.best_stars(9) == 0, "schema 2 save keeps its numbering")
	# Level gift: once per level, into the inventory.
	var p := Profile.new()
	p.highest_unlocked = 40
	var gift := Progress.level_gift(p, 12, Levels.get_level(12))
	check(gift >= 0 and p.inventory[gift] == 1, "level gift adds a booster")
	check(Progress.level_gift(p, 12, Levels.get_level(12)) == -1, "level gift only once")
	# Check-in streak and star chest.
	var c := Progress.claim_checkin(p, "2026-01-01")
	check(c["day"] == 1 and Progress.claim_checkin(p, "2026-01-01").is_empty(), "check-in once a day")
	check(Progress.claim_checkin(p, "2026-01-02")["day"] == 2, "check-in streak continues")
	check(Progress.claim_checkin(p, "2026-01-05")["day"] == 1, "check-in streak restarts after a gap")
	for level in range(1, 11):
		p.stars[level] = 3
	check(Progress.chests_ready(p) == 1 and not Progress.open_chest(p).is_empty() and Progress.chests_ready(p) == 0,
		"star chest opens once per 30 stars")
	# Daily challenge: the same authored level all day, solvable, paid once.
	var day_a := Levels.daily_challenge(Progress.challenge_seed("2026-03-04"), 40)
	var day_b := Levels.daily_challenge(Progress.challenge_seed("2026-03-04"), 40)
	check(not day_a.is_empty() and JSON.stringify(day_a) == JSON.stringify(day_b), "daily challenge is fixed per day")
	var solved := Solver.solve(Levels.create_board(day_a), [], 200000, 20000)
	check(solved["status"] == Solver.Status.SOLVED, "daily challenge solvable")
	check(not Progress.complete_challenge(p, "2026-03-04").is_empty() and Progress.complete_challenge(p, "2026-03-04").is_empty(),
		"daily challenge pays once a day")
