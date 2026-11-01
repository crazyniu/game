extends SceneTree
const Rules = preload("res://core/first_level_rules.gd")
var checks: int = 0
var failures: int = 0

func check(value: bool, label: String) -> void:
	checks += 1
	if not value:
		failures += 1
		push_error(label)

func _initialize() -> void:
	var initial: Dictionary = Rules.initial_state()
	check(Rules.validate_state(initial), "initial valid")
	check(initial == Rules.initial_state(), "deterministic fixed state")
	check(initial.branches[1].tokens[3].id == "L01-S2-04", "fixed token IDs")
	check(Rules.legal_moves(initial) == [{"from": 1, "to": 0, "count": 1}], "unique opening")
	check(not Rules.is_won(initial), "not prematurely won")
	for endpoints in [[0, 0], [-1, 0], [0, 8], [0, 1], [2, 0]]:
		check(not Rules.prepare_move(initial, endpoints[0], endpoints[1]).ok, "illegal endpoints")
	var first: Dictionary = Rules.prepare_move(initial, 1, 0)
	check(first.ok and first.count == 1 and first.run == 1, "first transfer")
	check(initial == Rules.initial_state(), "input not mutated")
	check(first.state.move_count == 1 and first.state.sequence == 1, "transfer counters")
	check(first.events.size() == 3 and first.events[0].kind == "Moved" and first.events[1].kind == "Home" and first.events[2].kind == "Retired", "event order")
	check(first.events[1].token_ids == ["L01-S1-01", "L01-S1-02", "L01-S1-03", "L01-S2-04"], "home order preserved")
	check(first.state.branches[0].home_species == 0 and first.state.branches[0].tokens.is_empty(), "home retirement")
	check(not Rules.prepare_move(first.state, 1, 0).ok and not Rules.prepare_move(first.state, 0, 1).ok, "retired endpoints reject")
	for endpoints in [[1, 2], [2, 1]]:
		var won: Dictionary = Rules.prepare_move(first.state, endpoints[0], endpoints[1])
		check(won.ok and Rules.is_won(won.state) and won.state.move_count == 2, "two-transfer win witness")
		check(won.state.branches[endpoints[0]].home_species == -1, "ordinary emptied branch remains available")
		check(Rules.legal_moves(won.state).is_empty(), "no moves after win")
	var partial: Dictionary = Rules.initial_state()
	partial.branches[0].tokens = [{"id":"b0","species":1},{"id":"a0","species":0},{"id":"a1","species":0}]
	partial.branches[1].tokens = [{"id":"b1","species":1},{"id":"a2","species":0},{"id":"a3","species":0}]
	partial.branches[2].tokens = [{"id":"b2","species":1},{"id":"b3","species":1}]
	var limited: Dictionary = Rules.prepare_move(partial, 0, 1)
	check(limited.ok and limited.count == 1 and limited.run == 2, "capacity-limited partial transfer")
	check(limited.state.branches[1].tokens[3].id == "a1" and limited.state.branches[1].home_species == -1, "mixed full target not home")
	check(not Rules.prepare_move(limited.state, 0, 1).ok, "full target rejects")
	var empty: Dictionary = Rules.prepare_move(partial, 2, 0)
	check(not empty.ok, "mismatched endpoint rejects")
	# A reusable empty branch receives an entire run in original stable order.
	partial.branches.append({"id":"S4","tokens":[],"home_species":-1})
	var group: Dictionary = Rules.prepare_move(partial, 0, 3)
	check(group.ok and group.count == 2 and group.state.branches[3].tokens[0].id == "a0" and group.state.branches[3].tokens[1].id == "a1", "whole-run order")
	for corruption in ["loss", "duplicate", "species", "retired", "order", "counter", "form", "id", "newline"]:
		var bad: Dictionary = initial.duplicate(true)
		match corruption:
			"loss": bad.branches[0].tokens.pop_back()
			"duplicate": bad.branches[0].tokens[0].id = bad.branches[0].tokens[1].id
			"species": bad.branches[0].tokens[0].species = -1
			"retired": bad.branches[0].home_species = 0
			"order": bad.branches[0].id = "S2"
			"counter": bad.sequence = -1
			"form": bad.branches[0].tokens[0].form = "egg"
			"id": bad.branches[0].tokens[0].id = "bad ID"
			"newline": bad.branches[0].tokens[0].id = "bad\n"
		check(not Rules.validate_state(bad), "reject corruption " + corruption)
	var reusable: Dictionary = initial.duplicate(true)
	reusable.branches.append({"id":"S4","tokens":[],"home_species":-1})
	var emptied: Dictionary = Rules.prepare_move(reusable, 0, 3)
	check(emptied.ok and emptied.state.branches[0].tokens.is_empty() and emptied.state.branches[0].home_species == -1, "empty is not retired")
	check(Rules.prepare_move(emptied.state, 2, 0).ok, "emptied source reusable")
	var overflow: Dictionary = initial.duplicate(true)
	overflow.sequence = 9223372036854775807
	check(not Rules.prepare_move(overflow, 1, 0).ok, "sequence overflow rejected")
	var session = Rules.Session.new()
	var tx: Dictionary = session.prepare_transfer(1, 0)
	check(session.snapshot() == initial and session.history_snapshot().is_empty(), "prepare does not commit")
	var forged: Dictionary = tx.duplicate(true)
	forged.state.move_count = 99
	check(not session.commit(forged), "tampered transaction rejected")
	var foreign = Rules.Session.new()
	check(not foreign.commit(tx), "foreign transaction rejected")
	check(session.commit(tx), "prepared transaction committed")
	check(not session.commit(tx), "stale transaction rejected")
	var copy: Dictionary = session.snapshot()
	copy.branches.clear()
	check(session.snapshot().branches.size() == 3, "snapshots isolated")
	var restored = Rules.Session.new()
	check(restored.restore(session.snapshot(), session.history_snapshot()), "history restores")
	check(restored.snapshot() == session.snapshot(), "restored equality")
	var bad_history: Array = session.history_snapshot()
	bad_history[0].count = 2
	check(not restored.restore(session.snapshot(), bad_history), "illegal history rejected")
	var undo: Dictionary = session.prepare_undo()
	check(undo.ok and session.commit(undo), "undo commits")
	check(session.snapshot().move_count == 0 and session.snapshot().sequence == 2 and session.snapshot().branches == initial.branches, "undo restores home birds and branches, monotonic sequence")
	check(not session.prepare_undo().ok, "empty undo rejects")
	check(restored.restore(session.snapshot(), session.history_snapshot()), "undo state restores")
	check(session.commit(session.prepare_transfer(1, 0)), "move after undo")
	check(restored.restore(session.snapshot(), session.history_snapshot()), "history with sequence gap restores")
	check(session.commit(session.prepare_transfer(1, 2)), "win session")
	check(session.commit(session.prepare_undo()) and not Rules.is_won(session.snapshot()), "undo win")
	var restart: Dictionary = session.prepare_restart()
	check(session.commit(restart), "restart commits")
	check(session.snapshot().branches == initial.branches and session.snapshot().move_count == 0 and session.snapshot().sequence == 6 and session.history_snapshot().is_empty(), "restart fixed board and sequence")
	var fake: Dictionary = initial.duplicate(true)
	fake.branches[0].tokens[0].id = "forged"
	check(not restored.restore(fake, []), "foreign identity rejected")
	check(restored.restore(session.snapshot(), []), "restart state restores")
	print("CORE_TESTS checks=%d failures=%d" % [checks, failures])
	quit(0 if failures == 0 else 1)
