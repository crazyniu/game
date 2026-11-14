extends SceneTree
## Deals a sibling for a catalog level: the same branches and the same kinds of birds, but mirrored left to right,
## with the colours handed round among the level's own species and every bird dealt again. A sibling is kept when
## it is solvable without tools, has no losing move beside its first three solution steps, its route is within a
## quarter of the original's (never shorter than the original for short levels) and it does not start like the
## original.
## godot --headless --path PerchSort -s ../tools/levels/deal_siblings.gd -- <level id>[@<take>] ...
## Prints SIBLING,<origin id>,<take>,<route>,<origin route>,<level json> or NONE,<origin id>,<take>.

const STEPS := 3

func _species_of(token: String) -> String:
	return token.trim_prefix("egg:").get_slice(":", 0)

func _recolour(token: String, map: Dictionary) -> String:
	var sp := _species_of(token)
	if not map.has(sp):
		return token
	return ("egg:" + map[sp]) if token.begins_with("egg:") else token.replace(sp, map[sp])

func _tokens_ok(level: Dictionary) -> bool:
	var board := Levels.create_board(level)
	if board == null:
		return false
	for branch in board.branch_count():
		var birds: PackedInt32Array = board.items[branch]
		if not birds.is_empty() and (birds[birds.size() - 1] & Board.EGG):
			return false
		# Sleeping full flocks are not home-ready; keep the teaching structures that rely on them.
		if Rules.can_home(board, branch):
			return false
	return true

## Route length, or -1 unless a win is proven for every alternative beside the first three steps.
func _judge(level: Dictionary, rules: Rules) -> int:
	var board: Board = Levels.create_board(level)
	if board == null:
		return -1
	var found := Solver.hint(board, [], 60000, 2000)
	if found["status"] != Solver.Status.SOLVED:
		return -1
	var cur := board.clone(false)
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
				if Solver.hint(nx, [], 60000, 2000)["status"] != Solver.Status.SOLVED:
					return -1
		if rt == -1: rules.swivel(cur, rf, null)
		else: rules.transfer(cur, rf, rt, null)
	return moves.size()

func _init() -> void:
	var rules := Rules.new()
	rules.track_economy = false
	var routes: Dictionary = JSON.parse_string(FileAccess.get_file_as_string("res://levels/hints.json"))
	var rng := RandomNumberGenerator.new()
	for arg in OS.get_cmdline_user_args():
		var id := int(str(arg).get_slice("@", 0))
		var take := int(str(arg).get_slice("@", 1)) if "@" in str(arg) else 1   # a second sibling: <id>@2
		# <id>@<take>@light: nearly sorted teaching levels keep their order and only trade a few pairs of birds.
		var light := str(arg).get_slice_count("@") > 2
		var origin: Dictionary = Levels.get_level(id)
		var old_len: int = routes[str(id)].size()
		var low := old_len if old_len <= 4 else int(ceil(old_len * 0.8))
		var high := maxi(old_len, int(floor(old_len * 1.25)))
		rng.seed = id * 104729 + take * 7919
		var species := []
		for b in origin["branches"]:
			for t in b["birds"]:
				var sp := _species_of(str(t))
				if sp != "crow" and not species.has(sp):
					species.append(sp)
		var best := {}
		var best_gap := 1000
		var best_len := 0
		for attempt in 400:
			var cand: Dictionary = origin.duplicate(true)
			# Colours handed round (a rotation, so no species keeps its role when there are two or more).
			var map := {}
			var shift := 1 + rng.randi() % maxi(1, species.size() - 1)
			for i in species.size():
				map[species[i]] = species[(i + shift) % species.size()]
			var tokens := []
			for b in cand["branches"]:
				b["side"] = "R" if b["side"] == "L" else "L"
				for t in b["birds"]:
					tokens.append(_recolour(str(t), map))
			if light:
				for swap in 1 + attempt / 80:
					var i := rng.randi_range(0, tokens.size() - 1)
					var j := rng.randi_range(0, tokens.size() - 1)
					var t = tokens[i]; tokens[i] = tokens[j]; tokens[j] = t
			else:
				for i in range(tokens.size() - 1, 0, -1):
					var j := rng.randi_range(0, i)
					var t = tokens[i]; tokens[i] = tokens[j]; tokens[j] = t
			var at := 0
			for b in cand["branches"]:
				var n: int = b["birds"].size()
				b["birds"] = tokens.slice(at, at + n)
				at += n
			if not _tokens_ok(cand):
				continue
			var length := _judge(cand, rules)
			if length < 0:
				continue
			var gap := 0 if length >= low and length <= high else mini(absi(length - low), absi(length - high))
			if gap < best_gap:
				best_gap = gap
				best = cand
				best_len = length
			if gap == 0:
				break
		if best.is_empty() or best_gap != 0:
			print("NONE,%d,%d" % [id, take])
		else:
			print("SIBLING,%d,%d,%d,%d,%s" % [id, take, best_len, old_len, JSON.stringify(best)])
	quit(0)
