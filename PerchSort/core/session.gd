class_name GameSession
extends RefCounted
## One level attempt: selection, moves, undo, restart, boosters. Events of the last action are in `events`.

var state: Board
var par := 0
var level: Dictionary
var events: Array = []
var selected := -1
var last_move: Array = []
var _initial: Board
var _history: Array = []  # [Board, last_move]
var _rules := Rules.new()
## Undos still allowed in this attempt: FREE_UNDOS at the start, more bought with coins or earned by an ad.
## Restart keeps the count, so restarting never refills it.
var undos_left := FREE_UNDOS

const FREE_UNDOS := 3

## Moves over par that still earn two stars.
const TWO_STAR_SLACK := 3


func _init(level_def: Dictionary, inventory := PackedInt32Array()) -> void:
	level = level_def
	_initial = Levels.create_board(level_def, inventory)
	state = _initial.clone()
	par = int(level_def.get("par", 0))

static func stars_for(moves: int, par_value: int) -> int:
	return 3 if moves <= par_value else (2 if moves <= par_value + TWO_STAR_SLACK else 1)

## Moves that can still be made while keeping the current rating; -1 on the last star, which cannot be lost.
static func moves_left_for_star(moves: int, par_value: int) -> int:
	if moves <= par_value:
		return par_value - moves
	return par_value + TWO_STAR_SLACK - moves if moves <= par_value + TWO_STAR_SLACK else -1

func move_count() -> int:
	return state.counted_moves

func stars() -> int:
	return stars_for(move_count(), par)

func can_undo() -> bool:
	return not _history.is_empty()

func select(b: int) -> bool:
	events.clear()
	if state.is_won() or b == selected or b < 0 or b >= state.branch_count() or state.run_count(b) == 0:
		selected = -1
		return false
	selected = b
	return true

func cancel_selection() -> void:
	selected = -1

func try_move(from: int, to: int) -> bool:
	_clear()
	if not Rules.can_transfer(state, from, to):
		return false
	_push()
	_rules.transfer(state, from, to, events)
	last_move = [from, to]
	return true

func swivel(b: int) -> bool:
	_clear()
	if not Rules.can_swivel(state, b):
		return false
	_push()
	_rules.swivel(state, b, events)
	last_move = [b, -1]
	return true

func try_action(move: Array) -> bool:
	return swivel(move[0]) if move[1] < 0 else try_move(move[0], move[1])

## Firefly returns {"applied", "hint"}; the hint is the solver result for the current state.
func use_booster(kind: int, max_states := 100000, max_ms := 3000) -> Dictionary:
	_clear()
	if state.is_won() or state.inventory[kind] == 0:
		return {"applied": false}
	var hint := {}
	if kind == Board.Booster.FIREFLY:
		hint = Solver.solve(state, last_move, max_states, max_ms)
		if hint["status"] != Solver.Status.SOLVED or hint["moves"].is_empty():
			return {"applied": false, "hint": hint}
	elif not Rules.booster_has_effect(state, kind):
		return {"applied": false}
	_push()
	_rules.booster(state, kind, true, events)
	return {"applied": true, "hint": hint}

## Commits a Firefly whose hint was computed off-thread on a snapshot.
func try_use_firefly(hint_move: Array) -> bool:
	_clear()
	if state.is_won() or state.inventory[Board.Booster.FIREFLY] == 0:
		return false
	var legal := Rules.can_swivel(state, hint_move[0]) if hint_move[1] < 0 else Rules.can_transfer(state, hint_move[0], hint_move[1])
	if not legal:
		return false
	_push()
	_rules.booster(state, Board.Booster.FIREFLY, true, events)
	return true

## Extra undos (bought or earned); they are not part of any snapshot.
func grant_undos(count: int) -> void:
	undos_left += count

## Purchases are economy transactions, not undoable actions: apply to every snapshot.
func grant_booster(kind: int, count: int) -> void:
	state.inventory[kind] += count
	_initial.inventory[kind] += count
	for snapshot in _history:
		snapshot[0].inventory[kind] += count

func booster_has_effect(kind: int) -> bool:
	if kind == Board.Booster.FIREFLY:
		return not state.is_won() and Rules.has_legal_move(state)
	return Rules.booster_has_effect(state, kind)

## Free once-per-level extra branch (shares the Sapling allowance).
func add_branch() -> bool:
	_clear()
	if not Rules.booster_has_effect(state, Board.Booster.SAPLING):
		return false
	_push()
	_rules.booster(state, Board.Booster.SAPLING, false, events)
	return true

func undo() -> bool:
	_clear()
	if _history.is_empty() or undos_left <= 0:
		return false
	undos_left -= 1
	var snapshot: Array = _history.pop_back()
	state = snapshot[0]
	last_move = snapshot[1]
	return true

func restart() -> void:
	state = _initial.clone()
	last_move = []
	_history.clear()
	_clear()

func _push() -> void:
	_history.append([state.clone(), last_move])

func _clear() -> void:
	events.clear()
	selected = -1
