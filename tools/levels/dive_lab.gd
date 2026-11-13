extends SceneTree
## Deals candidate lab levels for the Dive talent and keeps the first one of each shape that meets its acceptance:
## shortest route inside the target range, enough dives on it, and without the talent either unsolvable or clearly
## longer; no losing move beside the first three steps.
## godot --headless --path PerchSort -s ../tools/levels/dive_lab.gd -- <shape A|B|C> [first seed]
## Prints LAB,<shape>,<seed>,<moves>,<dives>,<moves without talent or -1>,<level json>.

const SHAPES := {
	"A": {"species": ["sky", "sunny", "berry"], "branches": 4, "min": 4, "max": 6, "dives": 1, "need": true, "sleep": 0, "egg": 0},
	"B": {"plant": 2, "species": ["sky", "sunny", "berry", "mint"], "branches": 5, "min": 9, "max": 14, "dives": 2, "need": true, "sleep": 0, "egg": 0},
	"C": {"plant": 3, "species": ["sky", "sunny", "berry", "mint", "cocoa"], "branches": 6, "min": 14, "max": 22, "dives": 2, "need": true, "sleep": 2, "egg": 1},
}

func _deal(shape: Dictionary, rng: RandomNumberGenerator, id: int) -> Dictionary:
	var birds := []
	for sp in shape["species"]:
		for i in 4:
			birds.append(sp)
	for i in range(birds.size() - 1, 0, -1):
		var j := rng.randi() % (i + 1)
		var t = birds[i]; birds[i] = birds[j]; birds[j] = t
	var n: int = shape["branches"]
	var lists := []
	for b in n:
		lists.append([])
	# Planted openings: on some branches a Dive bird sits directly under one other bird at the tip.
	var plant: int = shape.get("plant", 0)
	var order := range(n)
	for i in range(n - 1, 0, -1):
		var j := rng.randi() % (i + 1)
		var t = order[i]; order[i] = order[j]; order[j] = t
	for k in plant:
		var cover := ""
		for bird in birds:
			if bird != "sky":
				cover = bird
				break
		birds.erase("sky")
		birds.erase(cover)
		var filler := rng.randi() % 3
		for f in filler:
			var extra = birds[rng.randi() % birds.size()]
			birds.erase(extra)
			lists[order[k]].append(extra)
		lists[order[k]].append("sky")
		lists[order[k]].append(cover)
	for bird in birds:
		var b := rng.randi() % n
		while lists[b].size() >= 4:
			b = rng.randi() % n
		lists[b].insert(rng.randi() % (lists[b].size() + 1) if lists[b].size() < 2 else 0, bird)
	# Sleepers and the egg are never Dive birds and never the same seat; the egg is never a tip.
	var marked := {}
	for kind in ["sleep", "egg"]:
		var left: int = shape[kind]
		var guard := 0
		while left > 0 and guard < 200:
			guard += 1
			var b := rng.randi() % n
			if lists[b].size() < 2:
				continue
			var seat: int = rng.randi() % (lists[b].size() - (1 if kind == "egg" else 0))
			var key := "%d.%d" % [b, seat]
			if marked.has(key) or str(lists[b][seat]) == "sky":
				continue
			marked[key] = true
			lists[b][seat] = ("egg:%s" % lists[b][seat]) if kind == "egg" else ("%s:sleep" % lists[b][seat])
			left -= 1
	var branches := []
	for b in n:
		branches.append({"side": "L" if b % 2 == 0 else "R", "row": b / 2, "seats": 0, "birds": lists[b]})
	return {"id": id, "world": 2, "variant": "noon", "capacity": 4, "par": 1, "dayLength": 0,
		"talents": {"sky": "dive"}, "branches": branches}

func _complete_or_exposed(level: Dictionary) -> bool:
	for b in level["branches"]:
		var birds: Array = b["birds"]
		if birds.size() == 4 and birds[0] == birds[1] and birds[1] == birds[2] and birds[2] == birds[3]:
			return true
	return false

func _init() -> void:
	var args := OS.get_cmdline_user_args()
	var name: String = args[0]
	var shape: Dictionary = SHAPES[name]
	var seed := int(args[1]) if args.size() > 1 else 1
	var rules := Rules.new()
	rules.track_economy = false
	var rng := RandomNumberGenerator.new()
	var id: int = {"A": 1, "B": 2, "C": 3}[name]
	for attempt in 20000:
		rng.seed = seed + attempt
		var level := _deal(shape, rng, id)
		if _complete_or_exposed(level):
			continue
		var board: Board = Levels.create_board(level)
		if board == null:
			continue
		if Solver.hint(board, [], 30000, 600)["status"] != Solver.Status.SOLVED:
			continue
		var with := Solver.solve(board, [], 200000, 4000)
		if with["status"] != Solver.Status.SOLVED:
			continue
		var moves: Array = with["moves"]
		if moves.size() < shape["min"] or moves.size() > shape["max"]:
			continue
		# Dives on the route, and losing moves beside its first three steps.
		var cur := board.clone(false)
		var dives := 0
		var trap := false
		for step in moves.size():
			var m: Array = moves[step]
			if step < 3:
				for from in cur.branch_count():
					for to in cur.branch_count():
						if (from == m[0] and to == m[1]) or not Rules.can_transfer(cur, from, to):
							continue
						var nx := cur.clone(false)
						rules.transfer(nx, from, to, null)
						if Solver.hint(nx, [], 20000, 800)["status"] != Solver.Status.SOLVED:
							trap = true
			if m[1] >= 0 and not Rules._lands_on_top(cur, m[0], m[1]):
				dives += 1
			rules.transfer(cur, m[0], m[1], null)
		if trap or dives < shape["dives"]:
			continue
		var plain := board.clone(false)
		plain.dive_mask = 0
		var without := Solver.solve(plain, [], 200000, 4000)
		var without_moves := -1
		if without["status"] == Solver.Status.SOLVED:
			without_moves = without["moves"].size()
		elif without["status"] != Solver.Status.UNSOLVABLE:
			continue
		if shape["need"] and without_moves >= 0:
			continue
		if without_moves >= 0 and without_moves < moves.size() * 1.3:
			continue
		level["par"] = moves.size()
		print("LAB,%s,%d,%d,%d,%d,%s" % [name, seed + attempt, moves.size(), dives, without_moves, JSON.stringify(level)])
		quit(0)
		return
	print("NONE,%s" % name)
	quit(1)
