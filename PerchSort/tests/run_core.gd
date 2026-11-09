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
	check(levels.size() == Levels.COUNT, "%d levels" % Levels.COUNT)
	# Every level is replayed through a session along its stored solution (levels/hints.json), which proves it is
	# solvable. The largest boards are too big for a shortest-path search on every test run.
	var routes: Dictionary = JSON.parse_string(FileAccess.get_file_as_string("res://levels/hints.json"))
	for level in levels:
		var id := int(level["id"])
		var board := Levels.create_board(level)
		check(board != null, "level %d builds" % id)
		check(routes.has(str(id)), "level %d has a stored solution" % id)
		if board == null or not routes.has(str(id)):
			continue
		var session := GameSession.new(level)
		for entry in routes[str(id)]:
			var move: Array = [int(entry[1][0]), int(entry[1][1])]
			check(session.try_action(move), "level %d replay move %s" % [id, move])
		check(session.state.is_won(), "level %d replay wins" % id)
		var painted_rows: int = [4, 5, 6][mini((id - 1) / 10, 2)]
		var used_rows := 1
		for r in board.rows:
			used_rows = maxi(used_rows, r + 1)
		check(used_rows <= painted_rows, "level %d fits its painting" % id)
		print("L%02d route=%d counted=%d par=%d" % [id, routes[str(id)].size(), session.move_count(), level["par"]])
	_check_meta()
	print("core checks: %d failures in %d ms" % [failures, Time.get_ticks_msec() - t0])
	quit(1 if failures else 0)

## Save migration and the gift economy.
func _check_meta() -> void:
	# A schema 1 save: stars on old levels 1 and 9, best moves on old 9; both follow the full migration chain.
	var nine: int = Profile.LEVEL_DIFFICULTY[Profile.LEVEL_EXPANSION[Profile.LEVEL_RENUMBER[9]]]
	var stars := []
	stars.resize(41)
	stars.fill(0)
	stars[1] = 3
	stars[9] = 2
	var moves := stars.duplicate()
	moves[9] = 7
	var old := Profile.from_dict({"schemaVersion": 1, "bestStars": stars, "clearCounts": stars, "bestMoves": moves})
	check(old.best_stars(1) == 3 and old.best_stars(nine) == 2 and old.best_stars(9) == 0, "schema 1 stars renumbered")
	check(old.best_moves[nine] == 7, "schema 1 best moves renumbered")
	var again := Profile.from_dict(old.to_dict())
	check(again.best_stars(nine) == 2 and again.best_stars(9) == 0, "the current save keeps its numbering")
	# A schema 2 save: its forty levels move to their places in the hundred-level order.
	var forty := []
	forty.resize(41)
	forty.fill(0)
	forty[9] = 2
	var moved := Profile.from_dict({"schemaVersion": 2, "bestStars": forty, "clearCounts": forty, "bestMoves": forty})
	check(moved.best_stars(Profile.LEVEL_DIFFICULTY[Profile.LEVEL_EXPANSION[9]]) == 2 and moved.total_stars() == 2, "schema 2 stars follow their level")
	var opened := Progress.is_level_unlocked(moved, Profile.LEVEL_DIFFICULTY[Profile.LEVEL_EXPANSION[9]])
	check(opened, "a migrated cleared level stays playable")
	# Win drops: boosters on every first clear (more for three stars and every fifth level), favouring the next
	# level's mechanic; a fitting held booster that was never used is pointed at.
	var p := Profile.new()
	p.highest_unlocked = Levels.COUNT
	var rng := RandomNumberGenerator.new()
	rng.seed = 7
	Progress.grant_starter_gifts(p)
	var bells := p.inventory[Board.Booster.MORNING_BELL]
	var won := Progress.on_win(p, 11, 3, 3, 0, Levels.get_level(11), rng)
	check(won["boosters"].size() == 2, "three-star first clear drops 2 boosters")
	check(Board.Booster.MORNING_BELL in won["boosters"], "drops favour the next level's mechanic")
	check(p.inventory[Board.Booster.MORNING_BELL] == bells + won["boosters"].count(Board.Booster.MORNING_BELL),
		"drops land in the inventory")
	check(Progress.on_win(p, 15, 99, 3, 0, {}, rng)["boosters"].size() == Progress.DROP_MILESTONE_COUNT,
		"every fifth level drops the milestone count")
	check(Progress.on_win(p, 9, 99, 3, 0, {}, rng)["boosters"].size() == 1, "one-star first clear drops 1 booster")
	check(Progress.booster_to_point_at(p, Levels.get_level(11)) == Board.Booster.MORNING_BELL, "fitting booster pointed at")
	p.set_tutorial(Progress.used_flag(Board.Booster.MORNING_BELL))
	check(Progress.booster_to_point_at(p, Levels.get_level(11)) == -1, "a used booster is not pointed at again")
	# Check-in streak and star chest.
	var spent := 0
	while not Progress.draw(p, "2026-01-01", rng).is_empty():
		spent += 1
	check(spent == 1 and p.draws_left == 0 and not Progress.draws_available(p, "2026-01-01"), "one draw on the first day")
	spent = 0
	while not Progress.draw(p, "2026-01-02", rng).is_empty():
		spent += 1
	check(spent == 2, "the streak adds a draw each day in a row")
	spent = 0
	while not Progress.draw(p, "2026-01-05", rng).is_empty():
		spent += 1
	check(spent == 1, "the draws restart after a gap")
	var day := Profile.from_dict(p.to_dict())
	check(day.draws_date == "2026-01-05" and day.draws_left == 0, "draw state survives a save")
	var daily_cleared := Profile.new()
	daily_cleared.stars[2] = 3
	check(int(Levels.daily_challenge(Progress.challenge_seed("2026-03-04"), Progress.highest_cleared(daily_cleared))["id"]) <= 2,
		"the daily level is never past the last cleared level")
	for level in range(1, 11):
		p.stars[level] = 3
	check(Progress.chests_ready(p) == 1 and not Progress.open_chest(p).is_empty() and Progress.chests_ready(p) == 0,
		"star chest opens once per 30 stars")
	# Daily challenge: the same authored level all day, solvable, paid once.
	var day_a := Levels.daily_challenge(Progress.challenge_seed("2026-03-04"), Levels.COUNT)
	var day_b := Levels.daily_challenge(Progress.challenge_seed("2026-03-04"), Levels.COUNT)
	check(not day_a.is_empty() and JSON.stringify(day_a) == JSON.stringify(day_b), "daily challenge is fixed per day")
	var solved := Solver.solve(Levels.create_board(day_a), [], 200000, 20000)
	check(solved["status"] == Solver.Status.SOLVED, "daily challenge solvable")
	check(not Progress.complete_challenge(p, "2026-03-04").is_empty() and Progress.complete_challenge(p, "2026-03-04").is_empty(),
		"daily challenge pays once a day")
