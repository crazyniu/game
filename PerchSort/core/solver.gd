class_name Solver
extends RefCounted
## Shortest-path analysis and best-first interactive hints, without boosters.
## A move is [from, to]; a swivel is [b, -1].

enum Status { SOLVED, UNSOLVABLE, BUDGET_EXCEEDED }


## Returns {"status": Status, "moves": Array of [from, to]}.
## previous_move ([from, to] or []): the first step never undoes it unless no other first step reaches a win.
static func solve(start: Board, previous_move: Array = [], max_states := 100000, max_ms := 3000) -> Dictionary:
	if previous_move.size() == 2 and previous_move[1] >= 0:
		var result := _search(start, previous_move, max_states, max_ms)
		if result["status"] == Status.SOLVED:
			return result
	return _search(start, [], max_states, max_ms)

## Interactive hints need a winning route, not an exhaustive shortest-path proof.
## Prefer consolidated flocks and fewer blockers; keep reversal in the same search with a small penalty.
## `cancel` ([false]) lets a background caller stop the search early by setting cancel[0] = true.
static func hint(start: Board, previous_move: Array = [], max_states := 24000, max_ms := 1500, cancel: Array = []) -> Dictionary:
	if start.is_won():
		return {"status": Status.SOLVED, "moves": [], "continuation": {}}
	var cancellable := not cancel.is_empty()
	var deadline := Time.get_ticks_msec() + max_ms
	var rules := Rules.new()
	rules.track_economy = false
	var boards: Array[Board] = [start.clone(false)]
	var parents := PackedInt32Array([-1])
	var move_from := PackedInt32Array([0])
	var move_to := PackedInt32Array([0])
	var depths := PackedInt32Array([0])
	var penalties := PackedInt32Array([0])
	var visited := {start.search_key(false): true}
	var frontier: Array[Vector2i] = [Vector2i(_hint_score(start), 0)]
	while not frontier.is_empty():
		if cancellable and cancel[0]:
			return {"status": Status.BUDGET_EXCEEDED, "moves": []}
		var cursor := _hint_pop(frontier)
		var board := boards[cursor]
		boards[cursor] = null
		var n := board.branch_count()
		for from in n:
			for to in n + 1:
				if Time.get_ticks_msec() >= deadline or visited.size() >= max_states:
					return {"status": Status.BUDGET_EXCEEDED, "moves": []}
				var swivel := to == n
				var target := -1 if swivel else to
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
				var index := boards.size()
				boards.append(next)
				parents.append(cursor)
				move_from.append(from)
				move_to.append(target)
				var depth := depths[cursor] + 1
				depths.append(depth)
				var penalty := penalties[cursor]
				if cursor == 0 and previous_move.size() == 2 and previous_move[1] >= 0 \
						and from == previous_move[1] and target == previous_move[0]:
					penalty = 24
				penalties.append(penalty)
				if next.is_won():
					var moves := _path(parents, move_from, move_to, index)
					var continuation := {}
					var replay := start.clone(false)
					for move in moves:
						continuation[replay.search_key(false)] = move
						if move[1] < 0:
							rules.swivel(replay, move[0], null)
						else:
							rules.transfer(replay, move[0], move[1], null)
					return {"status": Status.SOLVED, "moves": moves, "continuation": continuation}
				_hint_push(frontier, Vector2i(depth + penalty + _hint_score(next), index))
	return {"status": Status.UNSOLVABLE, "moves": []}

## One-move suggestion when no full route was found in budget: the legal move whose result has the best
## (lowest) hint score; a winning move first, and undoing the previous move only as a last resort.
## Returns [from, to] ([b, -1] for a swivel), or [] when no legal move exists.
static func best_move(start: Board, previous_move: Array = []) -> Array:
	var rules := Rules.new()
	rules.track_economy = false
	var best: Array = []
	var best_score := 0
	var n := start.branch_count()
	for from in n:
		for to in n + 1:
			var swivel := to == n
			var target := -1 if swivel else to
			if swivel:
				if not Rules.can_swivel(start, from):
					continue
			elif not Rules.can_transfer(start, from, to):
				continue
			var next := start.clone(false)
			if swivel:
				rules.swivel(next, from, null)
			else:
				rules.transfer(next, from, to, null)
			var score := -1000000 if next.is_won() else _hint_score(next)
			if previous_move.size() == 2 and previous_move[1] >= 0 and from == previous_move[1] and target == previous_move[0]:
				score += 24
			if best.is_empty() or score < best_score:
				best = [from, target]
				best_score = score
	return best

static func _hint_score(board: Board) -> int:
	var groups := 0
	var blockers := 0
	for branch in board.items:
		var previous := -1
		for code in branch:
			var species := Board.species_of(code)
			if species != Board.CROW and species != previous:
				groups += 1
			if code & (Board.EGG | Board.ASLEEP):
				blockers += 1
			previous = species
	return groups * 12 + blockers * 2 + (board.species_count - board.homed.size()) * 8

## Min-heap entries are (priority, node index); never sort the entire frontier after a move.
static func _hint_push(heap: Array[Vector2i], entry: Vector2i) -> void:
	var i := heap.size()
	heap.append(entry)
	while i > 0:
		var parent := (i - 1) >> 1
		if heap[parent].x <= entry.x:
			break
		heap[i] = heap[parent]
		i = parent
	heap[i] = entry

static func _hint_pop(heap: Array[Vector2i]) -> int:
	var index := heap[0].y
	var last: Vector2i = heap.pop_back()
	if heap.is_empty():
		return index
	var i := 0
	while i * 2 + 1 < heap.size():
		var child := i * 2 + 1
		if child + 1 < heap.size() and heap[child + 1].x < heap[child].x:
			child += 1
		if heap[child].x >= last.x:
			break
		heap[i] = heap[child]
		i = child
	heap[i] = last
	return index

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
