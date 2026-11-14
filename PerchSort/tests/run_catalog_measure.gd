extends SceneTree
## Catalog repair contract. No profile, scene, inventory or production data is written.

const REPAIRED := [2, 6, 14, 16, 18, 20, 21, 23, 31, 44, 62, 63, 65, 100, 111]
var failures := 0
var checks := 0

func check(ok: bool, label: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		print("FAIL ", label)

func _init() -> void:
	var routes: Dictionary = JSON.parse_string(FileAccess.get_file_as_string("res://levels/hints.json"))
	check(Levels.all().size() == Levels.COUNT, "catalog size")
	var rules := Rules.new()
	rules.track_economy = false
	for world in Levels.WORLDS:
		var base := world * Levels.PER_WORLD
		for local in range(2, Levels.PER_WORLD + 1):
			if local % 2 != 0 and local != Levels.PER_WORLD:
				continue
			var original := base + (local - 2 if local == Levels.PER_WORLD else local - 1)
			var sibling := base + local
			var origin_length: int = routes[str(original)].size()
			var length: int = routes[str(sibling)].size()
			check(length * 100 >= origin_length * 67 and length * 2 <= origin_length * 3,
				"sibling %d route %d stays within original %d route %d band" % [sibling, length, original, origin_length])
	for id in REPAIRED:
		var level: Dictionary = Levels.get_level(id)
		var session := GameSession.new(level)
		var route: Array = routes[str(id)]
		for step in route.size():
			var entry: Array = route[step]
			check(session.state.search_key(false) == PackedInt32Array(entry[0]), "repaired %d hint key %d" % [id, step])
			var move := [int(entry[1][0]), int(entry[1][1])]
			if step < 3:
				_check_alternatives(id, step, session.state, move, rules)
			check(session.try_action(move), "repaired %d legal step %d" % [id, step])
		check(session.state.is_won(), "repaired %d wins without boosters" % id)
	print("catalog measure: %d checks, %d failures" % [checks, failures])
	quit(1 if failures else 0)

func _check_alternatives(id: int, step: int, board: Board, route_move: Array, rules: Rules) -> void:
	var count := board.branch_count()
	for from in count:
		for target_index in count + 1:
			var target := -1 if target_index == count else target_index
			if [from, target] == route_move:
				continue
			if target == -1:
				if not Rules.can_swivel(board, from): continue
			elif not Rules.can_transfer(board, from, target):
				continue
			var after := board.clone(false)
			if target == -1:
				rules.swivel(after, from, null)
			else:
				rules.transfer(after, from, target, null)
			var result: Dictionary = Solver.hint(after, [], 60000, 2000)
			check(result["status"] == Solver.Status.SOLVED,
				"repaired %d step %d alternative %d>%d proven recoverable (status %d)" % [id, step, from, target, result["status"]])
