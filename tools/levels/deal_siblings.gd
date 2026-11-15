extends SceneTree
## Deals a sibling for a catalog level: the same branches and the same kinds of birds, but mirrored left to right,
## with the colours handed round among the level's own species and every bird dealt again. A sibling is kept when
## it is solvable without tools, has no losing move beside its first three solution steps, its route is within a
## band set by its neighbours (never easier than its original, not beyond the next original where achievable, and
## within 0.67 to 1.5 times its original's route when that fits inside the band).
## godot --headless --path PerchSort -s ../tools/levels/deal_siblings.gd -- <level id>[@<take>[@light|full[@<salt>]]] ...
## The sibling of origin 2k-1 is level 2k (take 1); level 21 is take 2 of level 19 and must not be easier than level 20.
## Prints SIBLING,<origin id>,<take>,<route>,<origin route>,<level json>,<tier> (tier TIGHT: inside the band and the
## ratio; BAND: inside the band only) or NONE,<origin id>,<take>: a candidate outside the band is never returned.

const STEPS := 3

func _species_of(token: String) -> String:
	return token.trim_prefix("egg:").get_slice(":", 0)

func _recolour(token: String, map: Dictionary) -> String:
	var sp := _species_of(token)
	if not map.has(sp):
		return token
	return ("egg:" + map[sp]) if token.begins_with("egg:") else token.replace(sp, map[sp])

func _tokens_ok(level: Dictionary) -> bool:
	for b in level["branches"]:
		var birds: Array = b["birds"]
		if birds.is_empty():
			continue
		if str(birds[birds.size() - 1]).begins_with("egg:"):
			return false
		if birds.size() == int(level["capacity"]):
			var same := true
			for t in birds:
				same = same and _species_of(str(t)) == _species_of(str(birds[0]))
			if same and _species_of(str(birds[0])) != "crow":
				return false
	return true

## Route length, or -1 when unsolved or a move beside the first three steps can never be recovered from.
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
				if Solver.hint(nx, [], 6000, 250)["status"] == Solver.Status.UNSOLVABLE:
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
		var light := str(arg).get_slice("@", 2) == "light"
		var salt := int(str(arg).get_slice("@", 3)) if str(arg).get_slice_count("@") > 3 else 0   # another run of the same level
		var origin: Dictionary = Levels.get_level(id)
		var old_len: int = routes[str(id)].size()
		var target := id + take
		var base_low := old_len
		var base_high: int
		if take == 2:
			base_low = maxi(old_len, routes[str(target - 1)].size())
			base_high = maxi(base_low + 2, int(ceil(old_len * 1.5)))
		elif target % 21 == 20:
			base_high = old_len + 2
		else:
			base_high = maxi(routes[str(id + 2)].size(), old_len + 2)
		var tight_low := maxi(base_low, int(ceil(old_len * 0.67)))
		var tight_high := mini(base_high, int(floor(old_len * 1.5)))
		var tight_ok := tight_high >= tight_low
		var low := base_low
		var high := base_high
		rng.seed = id * 104729 + take * 7919 + salt * 31337
		var species := []
		for b in origin["branches"]:
			for t in b["birds"]:
				var sp := _species_of(str(t))
				if sp != "crow" and not species.has(sp):
					species.append(sp)
		var best := {}
		var best_len := 0
		var tier := "BAND"
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
			if length < low or length > high:
				continue
			if tight_ok and length >= tight_low and length <= tight_high:
				best = cand; best_len = length; tier = "TIGHT"
				break
			if best.is_empty():
				best = cand; best_len = length
		if best.is_empty():
			print("NONE,%d,%d" % [id, take])
		else:
			print("SIBLING,%d,%d,%d,%d,%s,%s" % [id, take, best_len, old_len, JSON.stringify(best["branches"]), tier])
	quit(0)
