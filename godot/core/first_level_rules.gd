class_name FirstLevelRules
extends RefCounted
## Bird-only rules4 core. Eggs, assistance and economy are not ported here.

static func initial_state() -> Dictionary:
	var branches: Array = []
	var layout: Array = [[0, 0, 0], [1, 1, 1, 0], [1]]
	for i in range(layout.size()):
		var tokens: Array = []
		for slot in range(layout[i].size()):
			tokens.append({"id": "L01-S%d-%02d" % [i + 1, slot + 1], "species": layout[i][slot]})
		branches.append({"id": "S%d" % (i + 1), "tokens": tokens, "home_species": -1})
	return {"flock_size": 4, "species_count": 2, "branches": branches, "move_count": 0, "sequence": 0}

static func _integer(value: Variant) -> bool:
	return value is int or (value is float and is_finite(value) and value == floor(value) and abs(value) <= 9007199254740991.0)

static func validation_errors(state: Dictionary) -> Array[String]:
	var errors: Array[String] = []
	for key in ["flock_size", "species_count", "move_count", "sequence"]:
		if not state.has(key) or not _integer(state[key]):
			return ["Missing or non-integer " + key]
	var n: int = int(state.flock_size)
	var k: int = int(state.species_count)
	if n not in [4, 6, 8] or k < 1 or k > 8 or state.move_count < 0 or state.sequence < state.move_count:
		return ["Invalid configuration or counters"]
	if not state.get("branches") is Array or state.branches.size() < k or state.branches.size() > 10:
		return ["Invalid branch array"]
	var counts: Array = []
	counts.resize(k)
	counts.fill(0)
	var ids: Dictionary = {}
	var homes: Dictionary = {}
	var identifier := RegEx.new()
	identifier.compile("^[A-Za-z0-9][A-Za-z0-9._-]{0,63}\\z")
	for i in range(state.branches.size()):
		var branch: Variant = state.branches[i]
		if not branch is Dictionary or branch.get("id") != "S%d" % (i + 1) or not branch.get("tokens") is Array or not _integer(branch.get("home_species")):
			return ["Invalid branch identity or fields"]
		var home: int = int(branch.home_species)
		if home < -1 or home >= k or branch.tokens.size() > n:
			return ["Invalid capacity or home species"]
		if home >= 0:
			if not branch.tokens.is_empty() or homes.has(home):
				return ["Retired branches must be empty and home species unique"]
			homes[home] = true
			counts[home] += n
		var uniform: bool = not branch.tokens.is_empty()
		var first: int = -1
		for token in branch.tokens:
			if not token is Dictionary or not token.get("id") is String or identifier.search(token.id) == null or not _integer(token.get("species")):
				return ["Invalid stable token"]
			var species: int = int(token.species)
			if species < 0 or species >= k or ids.has(token.id):
				return ["Invalid species or duplicate token ID"]
			if token.get("form", "bird") != "bird":
				return ["Only bird tokens are supported"]
			ids[token.id] = true
			counts[species] += 1
			if first < 0:
				first = species
			uniform = uniform and species == first
		if branch.tokens.size() == n and uniform:
			return ["Stable full uniform branches must already be retired"]
	for count in counts:
		if count != n:
			errors.append("Each species must be conserved across birds and Home")
	return errors

static func validate_state(state: Dictionary) -> bool:
	return validation_errors(state).is_empty()

static func get_run(state: Dictionary, source: int) -> int:
	if source < 0 or source >= state.branches.size():
		return 0
	var b: Dictionary = state.branches[source]
	if b.home_species >= 0 or b.tokens.is_empty():
		return 0
	var count: int = 1
	while count < b.tokens.size() and b.tokens[-count - 1].species == b.tokens[-1].species:
		count += 1
	return count

static func legal_count(state: Dictionary, source: int, target: int) -> int:
	if source == target or source < 0 or target < 0 or source >= state.branches.size() or target >= state.branches.size():
		return 0
	var a: Dictionary = state.branches[source]
	var b: Dictionary = state.branches[target]
	if a.home_species >= 0 or b.home_species >= 0 or a.tokens.is_empty() or b.tokens.size() >= state.flock_size:
		return 0
	if not b.tokens.is_empty() and b.tokens[-1].species != a.tokens[-1].species:
		return 0
	return mini(get_run(state, source), int(state.flock_size) - b.tokens.size())

static func legal_moves(state: Dictionary) -> Array:
	var moves: Array = []
	if not validate_state(state):
		return moves
	for source in range(state.branches.size()):
		for target in range(state.branches.size()):
			var count: int = legal_count(state, source, target)
			if count > 0:
				moves.append({"from": source, "to": target, "count": count})
	return moves

static func _event(kind: String, source: int, target: int, branch: int, species: int, source_slot: int, target_slot: int, tokens: Array) -> Dictionary:
	var ids: Array = []
	for token in tokens:
		ids.append(token.id)
	return {"kind": kind, "from": source, "to": target, "branch": branch, "species": species, "source_start_slot": source_slot, "target_start_slot": target_slot, "tokens": tokens.duplicate(true), "token_ids": ids}

static func prepare_move(state: Dictionary, source: int, target: int) -> Dictionary:
	if not validate_state(state):
		return {"ok": false, "error": "Invalid state"}
	var count: int = legal_count(state, source, target)
	if count == 0:
		return {"ok": false, "error": "Illegal transfer"}
	if state.sequence >= 9223372036854775807 or state.move_count >= 9223372036854775807:
		return {"ok": false, "error": "Counter overflow"}
	var next: Dictionary = state.duplicate(true)
	var a: Dictionary = next.branches[source]
	var b: Dictionary = next.branches[target]
	var start: int = a.tokens.size() - count
	var target_start: int = b.tokens.size()
	var moved: Array = a.tokens.slice(start)
	a.tokens.resize(start)
	b.tokens.append_array(moved)
	var species: int = int(moved[0].species)
	var events: Array = [_event("Moved", source, target, -1, species, start, target_start, moved)]
	var uniform: bool = true
	for token in b.tokens:
		uniform = uniform and token.species == species
	if b.tokens.size() == state.flock_size and uniform:
		events.append(_event("Home", target, -1, target, species, 0, -1, b.tokens))
		b.tokens = []
		b.home_species = species
		events.append(_event("Retired", -1, -1, target, species, -1, -1, []))
	next.move_count = int(state.move_count) + 1
	next.sequence = int(state.sequence) + 1
	if not validate_state(next):
		return {"ok": false, "error": "Invalid candidate"}
	return {"ok": true, "state": next, "before": state.duplicate(true), "events": events, "from": source, "to": target, "count": count, "run": get_run(state, source), "error": ""}

static func is_won(state: Dictionary) -> bool:
	if not validate_state(state):
		return false
	var homes: int = 0
	for branch in state.branches:
		if not branch.tokens.is_empty():
			return false
		if branch.home_species >= 0:
			homes += 1
	return homes == state.species_count

static func _logical(state: Dictionary) -> Dictionary:
	var result: Dictionary = state.duplicate(true)
	result.sequence = result.move_count
	return result

class Session extends RefCounted:
	var _state: Dictionary = FirstLevelRules.initial_state()
	var _history: Array = []
	var _pending: Dictionary = {}
	var _serial: int = 0

	func snapshot() -> Dictionary:
		return _state.duplicate(true)

	func history_snapshot() -> Array:
		return _history.duplicate(true)

	func _prepare(kind: String, next: Dictionary, history: Array, events: Array = []) -> Dictionary:
		_serial += 1
		var transaction: Dictionary = {"ok": true, "owner": get_instance_id(), "serial": _serial, "kind": kind, "before": snapshot(), "state": next.duplicate(true), "history": history.duplicate(true), "events": events.duplicate(true)}
		_pending[_serial] = transaction.duplicate(true)
		return transaction

	func prepare_transfer(source: int, target: int) -> Dictionary:
		var move: Dictionary = FirstLevelRules.prepare_move(_state, source, target)
		if not move.ok:
			return move
		var history: Array = history_snapshot()
		history.append({"before": snapshot(), "from": source, "to": target, "count": move.count})
		return _prepare("Transfer", move.state, history, move.events)

	func prepare_undo() -> Dictionary:
		if _history.is_empty() or _state.sequence >= 9223372036854775807:
			return {"ok": false, "error": "Nothing to undo or counter overflow"}
		var history: Array = history_snapshot()
		var next: Dictionary = history.pop_back().before
		next.sequence = int(_state.sequence) + 1
		return _prepare("Undo", next, history)

	func prepare_restart() -> Dictionary:
		if _state.sequence >= 9223372036854775807:
			return {"ok": false, "error": "Counter overflow"}
		var next: Dictionary = FirstLevelRules.initial_state()
		next.sequence = int(_state.sequence) + 1
		return _prepare("Restart", next, [])

	func commit(transaction: Dictionary) -> bool:
		var serial: Variant = transaction.get("serial")
		if not _pending.has(serial) or _pending[serial] != transaction or transaction.before != _state:
			return false
		_state = transaction.state.duplicate(true)
		_history = transaction.history.duplicate(true)
		_pending.clear()
		return true

	func restore(state: Dictionary, history: Array) -> bool:
		if not FirstLevelRules.validate_state(state):
			return false
		var expected: Dictionary = FirstLevelRules.initial_state()
		for i in range(history.size()):
			var item: Variant = history[i]
			if not item is Dictionary or not item.get("before") is Dictionary:
				return false
			for key in ["from", "to", "count"]:
				if not FirstLevelRules._integer(item.get(key)):
					return false
			var before: Dictionary = item.before
			if not FirstLevelRules.validate_state(before) or before.move_count != i or before.sequence < expected.sequence or FirstLevelRules._logical(before) != FirstLevelRules._logical(expected):
				return false
			var move: Dictionary = FirstLevelRules.prepare_move(before, int(item.from), int(item.to))
			if not move.ok or move.count != item.count:
				return false
			expected = move.state
		if state.move_count != history.size() or state.sequence < expected.sequence or FirstLevelRules._logical(state) != FirstLevelRules._logical(expected):
			return false
		_state = state.duplicate(true)
		_history = history.duplicate(true)
		_pending.clear()
		return true
