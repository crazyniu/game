class_name Solver
extends RefCounted
## Breadth-first search over transfers and swivels (no boosters). A move is [from, to]; a swivel is [b, -1].

enum Status { SOLVED, UNSOLVABLE, BUDGET_EXCEEDED }


## Returns {"status": Status, "moves": Array of [from, to]}.
## previous_move ([from, to] or []): the first step never undoes it unless no other first step reaches a win.
static func solve(start: Board, previous_move: Array = [], max_states := 100000, max_ms := 3000) -> Dictionary:
	if previous_move.size() == 2 and previous_move[1] >= 0:
		var result := _search(start, previous_move, max_states, max_ms)
		if result["status"] == Status.SOLVED:
			return result
	return _search(start, [], max_states, max_ms)

## Breadth-first search; `banned` is a first move that is skipped.
static func _search(start: Board, banned: Array, max_states: int, max_ms: int) -> Dictionary:
	if start.is_won():
		return {"status": Status.SOLVED, "moves": []}
	var deadline := Time.get_ticks_msec() + max_ms
	var rules := Rules.new()
	rules.track_economy = false
	var boards: Array[Board] = [start.clone(false)]
	var parents := PackedInt32Array([-1])
	var move_from := PackedInt32Array([0])
	var move_to := PackedInt32Array([0])
	var visited := {start.search_key(false): true}
	var cursor := 0
	while cursor < boards.size():
		var board := boards[cursor]
		var n := board.branch_count()
		for from in n:
			for to in n + 1:
				if Time.get_ticks_msec() >= deadline:
					return {"status": Status.BUDGET_EXCEEDED, "moves": []}
				var swivel := to == n
				var target := -1 if swivel else to
				# Reversing means moving back from the branch the previous move landed on.
				if cursor == 0 and not banned.is_empty() and from == banned[1] and target == banned[0]:
					continue
				if swivel:
					if not Rules.can_swivel(board, from):
						continue
				elif not Rules.can_transfer(board, from, to):
					continue
				var next := board.clone(false)
				if swivel:
					rules.swivel(next, from, null)
				else:
					rules.transfer(next, from, to, null)
				var key := next.search_key(false)
				if visited.has(key):
					continue
				visited[key] = true
				if visited.size() > max_states:
					return {"status": Status.BUDGET_EXCEEDED, "moves": []}
				boards.append(next)
				parents.append(cursor)
				move_from.append(from)
				move_to.append(target)
				if next.is_won():
					return {"status": Status.SOLVED, "moves": _path(parents, move_from, move_to, boards.size() - 1)}
		boards[cursor] = null  # expanded nodes are no longer needed
		cursor += 1
	return {"status": Status.UNSOLVABLE, "moves": []}

static func _path(parents: PackedInt32Array, from: PackedInt32Array, to: PackedInt32Array, end: int) -> Array:
	var moves := []
	var i := end
	while parents[i] >= 0:
		moves.push_front([from[i], to[i]])
		i = parents[i]
	return moves
