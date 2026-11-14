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
			# A legal route is not enough: the live hint cache indexes these exact state keys.
			check(session.state.search_key(false) == PackedInt32Array(entry[0]),
				"level %d hint key matches before action %d" % [id, session.state.moves + 1])
			var move: Array = [int(entry[1][0]), int(entry[1][1])]
			check(session.try_action(move), "level %d replay move %s" % [id, move])
		check(session.state.is_won(), "level %d replay wins" % id)
		var painted_rows: int = [4, 5, 6][mini(Levels.world_of(level) - 1, 2)]
		var used_rows := 1
		for r in board.rows:
			used_rows = maxi(used_rows, r + 1)
		check(used_rows <= painted_rows, "level %d fits its painting" % id)
		print("L%02d route=%d counted=%d par=%d" % [id, routes[str(id)].size(), session.move_count(), level["par"]])
	_check_meta()
	_check_dead_end()
	_check_resume()
	print("core checks: %d failures in %d ms" % [failures, Time.get_ticks_msec() - t0])
	quit(1 if failures else 0)

## Save migration and the gift economy.
func _check_meta() -> void:
	# A schema 1 save: stars on old levels 1 and 9, best moves on old 9; both follow the full migration chain.
	var nine: int = Profile.LEVEL_SPREAD[Profile.LEVEL_DIFFICULTY[Profile.LEVEL_EXPANSION[Profile.LEVEL_RENUMBER[9]]]]
	var stars := []
	stars.resize(41)
	stars.fill(0)
	stars[1] = 3
	stars[9] = 2
	var moves := stars.duplicate()
	moves[9] = 7
	var old := Profile.from_dict({"schemaVersion": 1, "bestStars": stars, "clearCounts": stars, "bestMoves": moves})
	check(old.best_stars(1) == 3 and old.best_stars(nine) == 2 and old.total_stars() == 5, "schema 1 stars renumbered")
	check(old.best_moves[nine] == 7, "schema 1 best moves renumbered")
	var again := Profile.from_dict(old.to_dict())
	check(again.best_stars(nine) == 2 and again.total_stars() == 5, "the current save keeps its numbering")
	# A schema 2 save: its forty levels move to their places in the hundred-level order.
	var forty := []
	forty.resize(41)
	forty.fill(0)
	forty[9] = 2
	var moved := Profile.from_dict({"schemaVersion": 2, "bestStars": forty, "clearCounts": forty, "bestMoves": forty})
	var moved_nine: int = Profile.LEVEL_SPREAD[Profile.LEVEL_DIFFICULTY[Profile.LEVEL_EXPANSION[9]]]
	check(moved.best_stars(moved_nine) == 2 and moved.total_stars() == 2, "schema 2 stars follow their level")
	var opened := Progress.is_level_unlocked(moved, moved_nine)
	# A schema 4 save (ten levels a world): level 12 was the second level of world 2 and stays the second old level there.
	var hundred := []
	hundred.resize(101)
	hundred.fill(0)
	hundred[12] = 3
	var spread := Profile.from_dict({"schemaVersion": 4, "bestStars": hundred, "clearCounts": hundred, "bestMoves": hundred, "highestUnlockedLevel": 13, "lastLevel": 12})
	check(spread.best_stars(24) == 3 and spread.total_stars() == 3 and spread.last_level == 24, "schema 4 stars move to the 21-level worlds")
	check(spread.highest_unlocked == 26, "schema 4 progress moves with its levels: %d" % spread.highest_unlocked)
	check(opened, "a migrated cleared level stays playable")
	# Win drops: boosters on every first clear (more for three stars and every fifth level), favouring the next
	# level's mechanic; a fitting held booster that was never used is pointed at.
	var p := Profile.new()
	p.highest_unlocked = Levels.COUNT
	var rng := RandomNumberGenerator.new()
	rng.seed = 7
	Progress.grant_starter_gifts(p)
	var bells := p.inventory[Board.Booster.MORNING_BELL]
	var won := Progress.on_win(p, 11, 3, 3, 0, Levels.get_level(22), rng)
	check(won["boosters"].size() == 2, "three-star first clear drops 2 boosters")
	check(Board.Booster.MORNING_BELL in won["boosters"], "drops favour the next level's mechanic")
	check(p.inventory[Board.Booster.MORNING_BELL] == bells + won["boosters"].count(Board.Booster.MORNING_BELL),
		"drops land in the inventory")
	check(Progress.on_win(p, 15, 99, 3, 0, {}, rng)["boosters"].size() == Progress.DROP_MILESTONE_COUNT,
		"every fifth level drops the milestone count")
	check(Progress.on_win(p, 9, 99, 3, 0, {}, rng)["boosters"].size() == 1, "one-star first clear drops 1 booster")
	check(Progress.booster_to_point_at(p, Levels.get_level(22)) == Board.Booster.MORNING_BELL, "fitting booster pointed at")
	p.set_tutorial(Progress.used_flag(Board.Booster.MORNING_BELL))
	check(Progress.booster_to_point_at(p, Levels.get_level(22)) == -1, "a used booster is not pointed at again")
	# Daily reward calendar, the login streak it keeps, and the draws that follow the streak.
	var cal := Profile.new()
	var cal_coins := 0
	var cal_boosters := 0
	for d in 7:
		var date := "2026-02-%02d" % (d + 1)
		check(Progress.daily_reward_available(cal, date) and Progress.daily_reward_day(cal, date) == d + 1,
			"calendar day %d is waiting" % (d + 1))
		var paid := Progress.claim_daily_reward(cal, date, rng)
		check(paid["day"] == d + 1 and paid["coins"] == Progress.DAILY_REWARDS[d][0]
			and paid["boosters"].size() == Progress.DAILY_REWARDS[d][1], "calendar day %d pays its prize" % (d + 1))
		check(Progress.claim_daily_reward(cal, date, rng).is_empty() and not Progress.daily_reward_available(cal, date)
			and Progress.daily_reward_day(cal, date) == d + 1, "calendar day %d pays once" % (d + 1))
		cal_coins += paid["coins"]
		cal_boosters += paid["boosters"].size()
	check(cal.coins == cal_coins and Array(cal.inventory).reduce(func(a, b): return a + b, 0) == cal_boosters,
		"calendar prizes land in coins and inventory")
	for d in 6:
		check(Progress.DAILY_REWARDS[d + 1][0] > Progress.DAILY_REWARDS[d][0], "each day in a row pays more coins")
	check(Progress.daily_reward_day(cal, "2026-02-08") == 1, "the day after day 7 starts a new week")
	check(Progress.daily_reward_day(cal, "2026-02-10") == 1, "a missed day restarts at day 1")
	check(Profile.from_dict(cal.to_dict()).checkin_streak == 7, "the calendar day survives a save")
	var spent := 0
	while not Progress.draw(p, "2026-01-01", rng).is_empty():
		spent += 1
	check(spent == 1 and p.draws_left == 0 and not Progress.draws_available(p, "2026-01-01"), "one draw on the first day")
	check(Progress.daily_reward_available(p, "2026-01-01"), "opening the draw does not claim the daily reward")
	Progress.claim_daily_reward(p, "2026-01-01", rng)
	Progress.claim_daily_reward(p, "2026-01-02", rng)
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
	for level in range(1, 11):
		p.stars[level] = 3
	check(Progress.chests_ready(p) == 1 and not Progress.open_chest(p).is_empty() and Progress.chests_ready(p) == 0,
		"star chest opens once per 30 stars")

## A position whose only moves shuffle two part-flocks back and forth: legal moves exist, but the hint search
## exhausts every reachable position and proves it unsolvable, which is what the board's stuck pointer uses.
func _check_dead_end() -> void:
	var board := Levels.create_board({"id": 1, "capacity": 4, "par": 10, "world": 1, "variant": "noon", "branches": [
		{"side": "L", "row": 0, "birds": ["berry", "cocoa"]},
		{"side": "R", "row": 0, "birds": ["sky", "berry", "sunny"]},
		{"side": "L", "row": 1, "birds": ["sky", "sunny", "sunny", "sunny"]},
		{"side": "R", "row": 1, "birds": ["sky"]},
		{"side": "L", "row": 2, "birds": ["sky", "cocoa", "cocoa", "cocoa"]},
		{"side": "R", "row": 2, "birds": ["berry", "berry:sleep"]}]})
	check(Rules.has_legal_move(board), "dead end still has shuffle moves")
	check(Solver.hint(board, [], 24000, 5000)["status"] == Solver.Status.UNSOLVABLE, "dead end proven unsolvable")

## Play continues from the last level opened; a beaten last level continues with the next one.
func _check_resume() -> void:
	var p := Profile.new()
	check(Progress.resume_level(p) == 1, "fresh profile resumes at level 1")
	for level in range(1, 21):
		p.stars[level] = 3
	p.highest_unlocked = 21
	p.last_level = 7
	check(Progress.resume_level(p) == 8, "a beaten last level resumes at the next one")
	p.stars[15] = 0
	p.last_level = 15
	check(Progress.resume_level(p) == 15, "an unbeaten last level resumes itself")
	check(Profile.from_dict(p.to_dict()).last_level == 15, "the last level is saved")
	check(Profile.from_dict({}).last_level == 0, "old saves have no last level")
