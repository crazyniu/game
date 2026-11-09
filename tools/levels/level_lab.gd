extends SceneTree
## Level laboratory on the game's own rules and solver.
## godot --headless --path PerchSort -s ../tools/levels/level_lab.gd -- --mode=measure --out=<file> [--in=<levels file>]
## godot --headless --path PerchSort -s ../tools/levels/level_lab.gd -- --mode=generate --spec=<file> --index=<i>
##     --seed=<s> --want=<n> --seconds=<budget> --out=<file>
## measure writes one metrics record per level; generate deals levels for one spec and keeps the solvable ones.

const PLAYOUTS := 120
const PLAYOUT_MOVES := 300
const SOLVE_STATES := 40000
const SOLVE_MS := 4000
const HINT_STATES := 200000
const HINT_MS := 15000
const SPECIES_ORDER := ["sunny", "sky", "berry", "mint", "cocoa", "snow", "ember", "ink", "hoot"]

var args := {}

func _init() -> void:
	for a in OS.get_cmdline_user_args():
		var parts: PackedStringArray = a.trim_prefix("--").split("=", true, 1)
		args[parts[0]] = parts[1] if parts.size() > 1 else "1"
	match args.get("mode", ""):
		"measure":
			_measure()
		"generate":
			_generate()
		"redeal":
			_redeal()
		_:
			print("unknown mode")
	quit(0)

func _write(path: String, value: Variant) -> void:
	var f := FileAccess.open(path, FileAccess.WRITE)
	f.store_string(JSON.stringify(value))
	f.close()

## ---------- metrics ----------

func _measure() -> void:
	var levels: Array = Levels.all()
	if args.has("in"):
		levels = JSON.parse_string(FileAccess.get_file_as_string(args["in"]))["levels"]
	var rng := RandomNumberGenerator.new()
	rng.seed = 11
	var out := []
	for level in levels:
		var m := metrics(level, rng)
		out.append(m)
		print(JSON.stringify(m))
	_write(args["out"], out)

## Mechanics present in a level definition.
static func mechanics_of(level: Dictionary) -> Array:
	var found := {}
	if int(level.get("dayLength", 0)) > 0:
		found["daynight"] = true
	for b in level["branches"]:
		var kind := str(b.get("kind", ""))
		if kind in ["twig", "swivel", "growing"]:
			found[kind] = true
		for token in b["birds"]:
			var t := str(token)
			if t.begins_with("egg:"):
				found["egg"] = true
			elif t.ends_with(":sleep"):
				found["sleep"] = true
			elif t.ends_with(":mama"):
				found["mama"] = true
			elif t == "crow":
				found["crow"] = true
	var out := found.keys()
	out.sort()
	return out

## Solver depth and counted moves, random-play win rate, sizes. {} when the solver fails within its budget.
func metrics(level: Dictionary, rng: RandomNumberGenerator) -> Dictionary:
	var board := Levels.create_board(level)
	if board == null:
		return {}
	var t := Time.get_ticks_msec()
	var result := Solver.solve(board, [], SOLVE_STATES, SOLVE_MS)
	var exact: bool = result["status"] == Solver.Status.SOLVED
	if result["status"] == Solver.Status.BUDGET_EXCEEDED:
		result = Solver.hint(board, [], HINT_STATES, HINT_MS)   # a route, not proven shortest
	var solve_ms := Time.get_ticks_msec() - t
	if result["status"] != Solver.Status.SOLVED:
		return {"id": level.get("id", 0), "failed": result["status"], "solve_ms": solve_ms}
	var session := GameSession.new(level)
	for move in result["moves"]:
		session.try_action(move)
	var wins := 0
	for i in PLAYOUTS:
		if _playout(level, rng):
			wins += 1
	var birds := 0
	var spare := 0
	for i in board.branch_count():
		birds += board.items[i].size()
		spare += board.free_slots(i)
	return {"id": level.get("id", 0), "depth": result["moves"].size(), "counted": session.move_count(), "par": level.get("par", 0),
		"species": board.species_count, "branches": board.branch_count(), "birds": birds, "spare": spare,
		"mechanics": mechanics_of(level), "exact": exact, "win_rate": float(wins) / PLAYOUTS, "solve_ms": solve_ms, "moves": result["moves"]}

## One random game: legal moves only, never straight back. True on a win.
func _playout(level: Dictionary, rng: RandomNumberGenerator) -> bool:
	var board := Levels.create_board(level)
	var rules := Rules.new()
	rules.track_economy = false
	var last := Vector2i(-1, -1)
	for step in PLAYOUT_MOVES:
		var options: Array[Vector2i] = []
		var n := board.branch_count()
		for from in n:
			if Rules.can_swivel(board, from):
				options.append(Vector2i(from, -1))
			for to in n:
				if Rules.can_transfer(board, from, to) and not (from == last.y and to == last.x):
					if board.items[to].is_empty() and board.run_count(from) == board.items[from].size():
						continue   # moving a whole branch to an empty one changes nothing
					options.append(Vector2i(from, to))
		if options.is_empty():
			return false
		var pick := options[rng.randi() % options.size()]
		if pick.y < 0:
			rules.swivel(board, pick.x, null)
		else:
			rules.transfer(board, pick.x, pick.y, null)
		last = pick
		if board.is_won():
			return true
	return false

## ---------- dealing ----------

## Spec fields: colors, empty (empty normal branches), slack (seats left free on loaded branches, spread at random),
## twigs, swivels, growing, crows, sleep, eggs, mamas, day_length, min_depth, max_depth, min_win, max_win.
func _generate() -> void:
	var specs: Array = JSON.parse_string(FileAccess.get_file_as_string(args["spec"]))
	var index := int(args["index"])
	var spec: Dictionary = specs[index]
	var rng := RandomNumberGenerator.new()
	rng.seed = int(args.get("seed", "1")) * 1000 + index
	var deadline := Time.get_ticks_msec() + int(args.get("seconds", "120")) * 1000
	var want := int(args.get("want", "3"))
	var kept := []
	var tried := 0
	while kept.size() < want and Time.get_ticks_msec() < deadline:
		tried += 1
		var level := deal(spec, rng)
		if level.is_empty():
			continue
		var m := metrics(level, rng)
		if m.is_empty() or m.has("failed"):
			continue
		if m["depth"] < int(spec.get("min_depth", 1)) or m["depth"] > int(spec.get("max_depth", 99)):
			continue
		if m["win_rate"] < float(spec.get("min_win", 0.0)) or m["win_rate"] > float(spec.get("max_win", 1.0)):
			continue
		if not _has_all_mechanics(m, spec):
			continue
		kept.append({"spec": index, "level": level, "metrics": m})
	print("spec %d tried %d kept %d" % [index, tried, kept.size()])
	_write(args["out"], kept)

## godot ... -- --mode=redeal --level=<id> --seed=<s> --want=<n> --seconds=<budget> --out=<file>
## Keeps an authored level's branches and birds but deals the birds again over the same branch sizes, harder.
func _redeal() -> void:
	var source := Levels.get_level(int(args["level"]))
	var rng := RandomNumberGenerator.new()
	rng.seed = int(args.get("seed", "1")) * 1000 + int(args["level"])
	var deadline := Time.get_ticks_msec() + int(args.get("seconds", "100")) * 1000
	var want := int(args.get("want", "6"))
	var kept := []
	while kept.size() < want and Time.get_ticks_msec() < deadline:
		var level: Dictionary = source.duplicate(true)
		var tokens := []
		for b in level["branches"]:
			tokens.append_array(b["birds"])
		_shuffle(tokens, rng)
		var at := 0
		var valid := true
		for b in level["branches"]:
			var n: int = b["birds"].size()
			b["birds"] = tokens.slice(at, at + n)
			at += n
			if n > 0 and (str(b["birds"][n - 1]).begins_with("egg:") or (n == 4 and _one_species(b["birds"]))):
				valid = false
		if not valid:
			continue
		var m := metrics(level, rng)
		if not m.is_empty() and not m.has("failed") and m["depth"] >= int(args.get("min_depth", "6")):
			kept.append({"origin": int(args["level"]), "level": level, "metrics": m})
	print("redeal %s kept %d" % [args["level"], kept.size()])
	_write(args["out"], kept)

func _has_all_mechanics(m: Dictionary, spec: Dictionary) -> bool:
	var need := {"sleep": "sleep", "eggs": "egg", "mamas": "mama", "crows": "crow", "twigs": "twig", "swivels": "swivel",
		"growing": "growing"}
	for field in need:
		if int(spec.get(field, 0)) > 0 and not (need[field] in m["mechanics"]):
			return false
	return true

## One random deal, or {} when it breaks an authoring rule (exposed egg, complete flock at the start).
func deal(spec: Dictionary, rng: RandomNumberGenerator) -> Dictionary:
	var colors := int(spec["colors"])
	var palette: Array = (spec["palette"] as Array).duplicate()
	_shuffle(palette, rng)
	var chosen: Array = palette.slice(0, colors)
	# Tokens: four of each colour, some of them specialised.
	var plain := []
	var special := []
	var mamas := int(spec.get("mamas", 0))
	var eggs := int(spec.get("eggs", 0))
	var sleepers := int(spec.get("sleep", 0))
	for c in colors:
		var left := 4
		var species: String = chosen[c]
		if mamas > 0:
			mamas -= 1
			special.append(species + ":mama")
			special.append(species + ":chick")
			left -= 2
		if eggs > 0 and left > 0:
			eggs -= 1
			special.append("egg:" + species)
			left -= 1
		if sleepers > 0 and left > 0:
			sleepers -= 1
			special.append(species + ":sleep")
			left -= 1
		for i in left:
			plain.append(species)
	var tokens: Array = plain + special
	for i in int(spec.get("crows", 0)):
		tokens.append("crow")
	_shuffle(tokens, rng)
	# Branches: enough normal branches for the tokens with `slack` free seats, then the extras.
	var seats := tokens.size() + int(spec.get("slack", 0))
	var loaded := ceili(seats / 4.0)
	var shapes := []
	for i in loaded:
		shapes.append({"kind": "", "seats": 0, "cap": 4})
	var swivels := mini(int(spec.get("swivels", 0)), loaded)
	for i in swivels:
		shapes[i]["kind"] = "swivel"
	var fills := []
	fills.resize(loaded)
	fills.fill(4)
	for i in loaded * 4 - tokens.size():
		var open := []
		for b in loaded:
			if fills[b] > 0:
				open.append(b)
		fills[open[rng.randi() % open.size()]] -= 1
	var at := 0
	var lists := []
	for b in loaded:
		lists.append(tokens.slice(at, at + fills[b]))
		at += fills[b]
	# Authoring rules: no exposed egg, no complete flock, crows never buried under a flock of four.
	for list in lists:
		if list.is_empty():
			continue
		if str(list[list.size() - 1]).begins_with("egg:"):
			return {}
		if list.size() == 4 and _one_species(list):
			return {}
	for i in int(spec.get("empty", 0)):
		shapes.append({"kind": "", "seats": 0, "cap": 4})
		lists.append([])
	for i in int(spec.get("twigs", 0)):
		shapes.append({"kind": "twig", "seats": 0, "cap": 2})
		lists.append([])
	for i in int(spec.get("growing", 0)):
		shapes.append({"kind": "growing", "seats": 2, "cap": 2})
		lists.append([])
	# Seats on the screen: alternate sides from the top; shuffled so the same kinds do not stack.
	var order := range(shapes.size())
	_shuffle(order, rng)
	var branches := []
	for i in order.size():
		var b: int = order[i]
		var def := {"side": "L" if i % 2 == 0 else "R", "row": i / 2, "seats": shapes[b]["seats"], "birds": lists[b]}
		if not str(shapes[b]["kind"]).is_empty():
			def["kind"] = shapes[b]["kind"]
		branches.append(def)
	var level := {"id": 1000 + int(args.get("index", "0")), "world": 1, "variant": "noon", "capacity": 4, "par": 1,
		"dayLength": int(spec.get("day_length", 0)), "branches": branches}
	return level

static func _one_species(list: Array) -> bool:
	var first := str(list[0]).trim_prefix("egg:").get_slice(":", 0)
	for t in list:
		if str(t).trim_prefix("egg:").get_slice(":", 0) != first:
			return false
	return first != "crow"

func _shuffle(a: Array, rng: RandomNumberGenerator) -> void:
	for i in range(a.size() - 1, 0, -1):
		var j := rng.randi_range(0, i)
		var t = a[i]
		a[i] = a[j]
		a[j] = t
