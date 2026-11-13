extends SceneTree
## Repairs a level whose first three solution steps sit beside a move that can never be recovered from: it swaps one
## or two pairs of birds and keeps the first variant that is solvable, has no such move in its first three steps and
## whose route length stays within two moves of the old one and whose random-play win rate stays within a fifth.
## godot --headless --path PerchSort -s ../tools/levels/repair_early_traps.gd -- <level id> [<level id> ...]
## Prints one line per level: REPAIR,<id>,<json of the new branches> (or KEEP,<id> when nothing better was found).
## Merge the lines into levels.json, then rerun export_hints.gd and measure_fragility.gd.

const STEPS := 3

func _tokens_ok(level: Dictionary) -> bool:
	var cap := int(level["capacity"])
	for b in level["branches"]:
		var birds: Array = b["birds"]
		if birds.is_empty():
			continue
		if str(birds[birds.size() - 1]).begins_with("egg:"):
			return false
		if birds.size() == cap:
			var same := true
			for t in birds:
				same = same and t == birds[0] and not ":" in str(t)
			if same:
				return false
	return true

## [route length, unknown count] or [] when the level is unsolved or has an early losing move.
func _judge(level: Dictionary, rules: Rules) -> Array:
	var board: Board = Levels.create_board(level)
	if board == null:
		return []
	var found := Solver.hint(board, [], 60000, 2000)
	if found["status"] != Solver.Status.SOLVED:
		return []
	var cur := board.clone(false)
	var unknown := 0
	var moves: Array = found["moves"]
	for step in mini(STEPS, moves.size()):
		var rf: int = moves[step][0]
		var rt: int = moves[step][1]
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
				var st: int = Solver.hint(nx, [], 6000, 250)["status"]
				if st == Solver.Status.UNSOLVABLE:
					return []
				if st == Solver.Status.BUDGET_EXCEEDED:
					unknown += 1
		if rt == -1: rules.swivel(cur, rf, null)
		else: rules.transfer(cur, rf, rt, null)
	return [moves.size(), unknown]

## Wins out of GAMES random games (uniform legal move, never the direct reversal): the difficulty yardstick.
const GAMES := 40

func _random_wins(level: Dictionary, rules: Rules, rng: RandomNumberGenerator) -> int:
	var board: Board = Levels.create_board(level)
	var wins := 0
	for game in GAMES:
		var b := board.clone(false)
		var last := [-1, -1]
		for m in int(level["par"]) * 3 + 10:
			var moves := []
			var n := b.branch_count()
			for from in n:
				if Rules.can_swivel(b, from): moves.append([from, -1])
				for to in n:
					if Rules.can_transfer(b, from, to) and not (from == last[1] and to == last[0]):
						moves.append([from, to])
			if moves.is_empty():
				break
			var mv: Array = moves[rng.randi() % moves.size()]
			if mv[1] == -1: rules.swivel(b, mv[0], null)
			else: rules.transfer(b, mv[0], mv[1], null)
			last = mv
			if b.is_won():
				wins += 1
				break
	return wins

func _init() -> void:
	var rules := Rules.new()
	rules.track_economy = false
	var routes: Dictionary = JSON.parse_string(FileAccess.get_file_as_string("res://levels/hints.json"))
	var rng := RandomNumberGenerator.new()
	for arg in OS.get_cmdline_user_args():
		var id := int(arg)
		var level: Dictionary = Levels.get_level(id)
		var old_len: int = routes[str(id)].size()
		rng.seed = id * 7919
		var old_wins := _random_wins(level, rules, rng)
		var best := {}
		var best_score := 1000000
		for attempt in 600:
			var cand: Dictionary = level.duplicate(true)
			var swaps := 1 if attempt < 300 else 2
			var ok := true
			for s in swaps:
				var seats := []
				for bi in cand["branches"].size():
					for si in cand["branches"][bi]["birds"].size():
						seats.append([bi, si])
				var a: Array = seats[rng.randi() % seats.size()]
				var b: Array = seats[rng.randi() % seats.size()]
				var ta = cand["branches"][a[0]]["birds"][a[1]]
				var tb = cand["branches"][b[0]]["birds"][b[1]]
				if a[0] == b[0] or ta == tb:
					ok = false
					break
				cand["branches"][a[0]]["birds"][a[1]] = tb
				cand["branches"][b[0]]["birds"][b[1]] = ta
			if not ok or not _tokens_ok(cand):
				continue
			var verdict := _judge(cand, rules)
			if verdict.is_empty():
				continue
			var drift: int = absi(_random_wins(cand, rules, rng) - old_wins)
			var score: int = absi(verdict[0] - old_len) + 3 * verdict[1] + drift
			if score < best_score:
				best_score = score
				best = cand
			if absi(verdict[0] - old_len) <= 2 and verdict[1] == 0 and drift <= GAMES / 5:
				break
		if best.is_empty():
			print("KEEP,%d" % id)
		else:
			print("REPAIR,%d,%d,%s" % [id, best_score, JSON.stringify(best["branches"])])
	quit(0)
