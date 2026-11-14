class_name Rules
extends RefCounted
## Move resolution. One instance per session or search; events go to `out` (null in search).
## Event dictionaries always carry "type"; see docs/work-log/godot-port/spec.md for fields.

enum Gift { WAKE_UP_SONG, GENTLE_BREEZE, ENCORE, GARDENER, GRUMPY_STARE, LIGHT_STEP, WARM_GLOW, NIGHT_CALL, MOONBEAM }
const GIFT_BY_SPECIES := {0: Gift.WAKE_UP_SONG, 1: Gift.GENTLE_BREEZE, 2: Gift.ENCORE, 3: Gift.GARDENER,
	4: Gift.GRUMPY_STARE, 5: Gift.LIGHT_STEP, 6: Gift.WARM_GLOW, 8: Gift.NIGHT_CALL, 9: Gift.MOONBEAM}

var track_economy := true
var _moonbeam_granted := false
var _homes: Array = []  # [branch, species] pairs of the current wave


static func can_transfer(s: Board, from: int, to: int) -> bool:
	var n := s.branch_count()
	if s.is_won() or from == to or from < 0 or to < 0 or from >= n or to >= n:
		return false
	if s.run_count(from) == 0 or s.free_slots(to) <= 0:
		return false
	var target := s.items[to]
	if target.is_empty():
		return true
	var tip := target[target.size() - 1]
	var source := s.items[from]
	return (tip & Board.EGG) == 0 and (s.light_step or Board.species_of(tip) == Board.species_of(source[source.size() - 1]))

static func can_swivel(s: Board, b: int) -> bool:
	return not s.is_won() and b >= 0 and b < s.branch_count() and s.kinds[b] == Board.Kind.SWIVEL

static func can_home(s: Board, b: int) -> bool:
	if s.kinds[b] == Board.Kind.TWIG or s.items[b].size() != s.capacity:
		return false
	return Board.species_of(s.items[b][0]) != Board.CROW and s.run_count(b) == s.capacity

static func has_legal_move(s: Board) -> bool:
	var n := s.branch_count()
	for from in n:
		if can_swivel(s, from):
			return true
		for to in n:
			if can_transfer(s, from, to):
				return true
	return false


func transfer(s: Board, from: int, to: int, out) -> void:
	_begin_move(s)
	var home_before := s.homed.size()
	var source := s.items[from]
	var target := s.items[to]
	var count := mini(s.run_count(from), s.free_slots(to))
	var start := source.size() - count
	var target_start := target.size()
	var species := Board.species_of(source[start])
	var landing_wake := -1
	if target_start > 0 and Board.species_of(target[target_start - 1]) == species:
		landing_wake = Board.id_of(target[target_start - 1])
	var mama := false
	var ids := PackedInt32Array()
	for i in count:
		var code := source[start + i]
		target.append(code)
		mama = mama or (code & Board.MAMA) != 0
		ids.append(Board.id_of(code))
	source.resize(start)
	s.light_step = false
	if out != null:
		out.append({"type": "transferred", "from": from, "to": to, "from_seat": start, "to_seat": target_start, "ids": ids})
	if mama:
		var followers := []
		var i := source.size() - 1
		while i >= 0 and target.size() < s.caps[to]:
			var code := source[i]
			if (code & Board.CHICK) and not (code & Board.EGG) and not s.is_asleep(code) and Board.species_of(code) == species:
				followers.append({"id": Board.id_of(code), "from_seat": i, "to_seat": target.size()})
				target.append(code)
				source.remove_at(i)
			i -= 1
		if out != null and not followers.is_empty():
			out.append({"type": "chicks_followed", "from": from, "to": to, "birds": followers})
	s.items[from] = source
	s.items[to] = target
	if out != null:
		for k in target_start:
			if (target[k] & Board.EGG) and Board.species_of(target[k]) == species:
				out.append({"type": "egg_wobbled", "id": Board.id_of(target[k]), "branch": to, "seat": k})
	_finish(s, home_before, true, to, landing_wake, out)

func swivel(s: Board, b: int, out) -> void:
	_begin_move(s)
	var home_before := s.homed.size()
	var list := s.items[b]
	if out != null:
		var travel := []
		for i in list.size():
			travel.append({"id": Board.id_of(list[i]), "from_seat": i, "to_seat": list.size() - 1 - i})
		out.append({"type": "swivelled", "branch": b, "birds": travel})
	list.reverse()
	s.items[b] = list
	_finish(s, home_before, true, -1, -1, out)

func _begin_move(s: Board) -> void:
	_moonbeam_granted = false
	s.moves += 1
	if not s.encore:
		s.counted_moves += 1
	s.encore = false
	if s.day_length > 0:
		s.phase_counter += 1

func _finish(s: Board, home_before: int, move: bool, landing_branch: int, landing_bird: int, out) -> void:
	_resolve(s, landing_branch, landing_bird, out)
	_auto_home(s, out)
	if move and s.day_length > 0 and s.phase_counter >= s.day_length:
		_toggle_phase(s, "clock", out)
		_resolve(s, -1, -1, out)
	if move and s.moonbeam_left > 0 and not _moonbeam_granted:
		s.moonbeam_left -= 1
	if move:
		_rescue_deadlock(s, out)
	if track_economy:
		var homes := s.homed.size() - home_before
		if homes == 0 and move:
			s.chain = 0
		for i in homes:
			s.chain += 1
			if s.chain > 1:
				s.chorus_coins += 5
	if s.is_won() and out != null:
		out.append({"type": "won"})

## A position where every remaining bird is asleep has no move and no way to wake anyone: the flock wakes up.
func _rescue_deadlock(s: Board, out) -> void:
	if s.is_won() or has_legal_move(s):
		return
	var slept := false
	for b in s.branch_count():
		for code in s.items[b]:
			slept = slept or (code & Board.ASLEEP) != 0
	if slept:
		_wake_rows(s, 0, false, out)
		_resolve(s, -1, -1, out)
		_auto_home(s, out)
	if not s.is_won() and not has_legal_move(s) and s.nocturnal_day_sleep():
		_toggle_phase(s, "stuck", out)
		_resolve(s, -1, -1, out)

## When the only birds left are one full flock of a single kind (all awake, no eggs, no crows), they go home on
## their own: nothing is left to decide, so the player is not asked to tap. It costs no move.
func _auto_home(s: Board, out) -> bool:
	if s.homed.size() != s.species_count - 1:
		return false
	var species := -1
	var count := 0
	for b in s.branch_count():
		for code in s.items[b]:
			var kind := Board.species_of(code)
			if kind == Board.CROW or (code & Board.EGG) or s.is_asleep(code) or (species >= 0 and kind != species):
				return false
			species = kind
			count += 1
	if species < 0 or count != s.capacity:
		return false
	var ids := PackedInt32Array()
	var first := -1
	for b in s.branch_count():
		for code in s.items[b]:
			ids.append(Board.id_of(code))
		if first < 0 and not s.items[b].is_empty():
			first = b
		s.items[b] = PackedInt32Array()
		s.kinds[b] = Board.Kind.NORMAL if s.kinds[b] != Board.Kind.TWIG else s.kinds[b]
	if out != null:
		out.append({"type": "flock_home", "branch": first, "species": Board.SPECIES[species], "ids": ids})
	s.homed.append(species)
	return true

func _resolve(s: Board, landing_branch: int, landing_bird: int, out) -> void:
	var changed := true
	while changed:
		if out != null:
			out.append({"type": "wave"})
		changed = false
		for b in s.branch_count():
			var tip := s.items[b].size() - 1
			if tip >= 0 and (s.items[b][tip] & Board.EGG):
				_hatch(s, b, tip, out)
				changed = true
		_homes.clear()
		for b in s.branch_count():
			if not can_home(s, b):
				continue
			var species := Board.species_of(s.items[b][0])
			if out != null:
				var ids := PackedInt32Array()
				for code in s.items[b]:
					ids.append(Board.id_of(code))
				out.append({"type": "flock_home", "branch": b, "species": Board.SPECIES[species], "ids": ids})
			s.items[b] = PackedInt32Array()
			s.kinds[b] = Board.Kind.NORMAL
			s.homed.append(species)
			_homes.append([b, species])
			changed = true
		if landing_bird >= 0:
			var target := s.items[landing_branch]
			for i in target.size():
				if Board.id_of(target[i]) == landing_bird:
					changed = _wake(s, landing_branch, i, out) or changed
					break
			landing_bird = -1
		for home in _homes.duplicate():
			_apply_gift(s, home[0], home[1], out)
			_wake_rows(s, s.rows[home[0]], true, out)
			_grow(s, false, out)

func _apply_gift(s: Board, b: int, species: int, out) -> void:
	if not GIFT_BY_SPECIES.has(species):
		return
	var effect: int = GIFT_BY_SPECIES[species]
	if out != null:
		out.append({"type": "gift", "branch": b, "species": Board.SPECIES[species], "effect": effect})
	match effect:
		Gift.WAKE_UP_SONG: _wake_rows(s, 0, false, out)
		Gift.GENTLE_BREEZE: _reveal_eggs(s, out)
		Gift.ENCORE: s.encore = true
		Gift.GARDENER:
			_grow(s, false, out)
			for t in s.branch_count():
				if s.kinds[t] != Board.Kind.TWIG:
					continue
				var before := s.caps[t]
				s.kinds[t] = Board.Kind.NORMAL
				s.caps[t] = s.capacity
				if out != null:
					out.append({"type": "twig_upgraded", "branch": t, "before": before, "after": s.capacity})
				break
		Gift.GRUMPY_STARE: _remove_crows(s, s.rows[b], true, out)
		Gift.LIGHT_STEP: s.light_step = true
		Gift.WARM_GLOW:
			for t in s.branch_count():
				for i in s.items[t].size():
					if s.items[t][i] & Board.EGG:
						_hatch(s, t, i, out)
		Gift.NIGHT_CALL:
			if s.day_length > 0:
				_toggle_phase(s, "ink", out)
		Gift.MOONBEAM:
			if s.day_length > 0:
				var was_sleeping := s.nocturnal_day_sleep()
				s.moonbeam_left = 3
				_moonbeam_granted = true
				if was_sleeping:
					_emit_nocturnal_wakes(s, out)

func _hatch(s: Board, b: int, seat: int, out) -> void:
	var code := s.items[b][seat]
	s.set_bird(b, seat, Board.with_flags(code, Board.flags_of(code) & ~(Board.EGG | Board.REVEALED)))
	if out != null:
		out.append({"type": "hatched", "id": Board.id_of(code), "branch": b, "seat": seat})

func _wake(s: Board, b: int, seat: int, out) -> bool:
	var code := s.items[b][seat]
	if (code & Board.ASLEEP) == 0:
		return false
	code = Board.with_flags(code, Board.flags_of(code) & ~Board.ASLEEP)
	s.set_bird(b, seat, code)
	if out != null and not s.is_asleep(code):
		out.append({"type": "woke", "id": Board.id_of(code), "branch": b, "seat": seat})
	return true

func _wake_rows(s: Board, row: int, limited: bool, out) -> void:
	for b in s.branch_count():
		if limited and absi(s.rows[b] - row) > 1:
			continue
		for i in s.items[b].size():
			_wake(s, b, i, out)

func _reveal_eggs(s: Board, out) -> void:
	for b in s.branch_count():
		for i in s.items[b].size():
			var code := s.items[b][i]
			if not (code & Board.EGG) or (code & Board.REVEALED):
				continue
			s.set_bird(b, i, code | Board.REVEALED)
			if out != null:
				out.append({"type": "egg_revealed", "id": Board.id_of(code), "branch": b, "seat": i})

func _grow(s: Board, full: bool, out) -> void:
	for b in s.branch_count():
		if s.kinds[b] != Board.Kind.GROWING or s.caps[b] >= s.capacity:
			continue
		var before := s.caps[b]
		s.caps[b] = s.capacity if full else before + 1
		if out != null:
			out.append({"type": "branch_grew", "branch": b, "before": before, "after": s.caps[b]})

func _remove_crows(s: Board, row: int, limited: bool, out) -> void:
	for b in s.branch_count():
		if limited and absi(s.rows[b] - row) > 1:
			continue
		var list := s.items[b]
		var departed := []
		var i := list.size() - 1
		while i >= 0:
			if Board.species_of(list[i]) == Board.CROW:
				departed.push_front({"id": Board.id_of(list[i]), "from_seat": i})
				list.remove_at(i)
			i -= 1
		s.items[b] = list
		if out != null and not departed.is_empty():
			out.append({"type": "crows_left", "branch": b, "birds": departed})

func _toggle_phase(s: Board, cause: String, out) -> void:
	var was_sleeping := s.nocturnal_day_sleep()
	s.phase = Board.Phase.NIGHT if s.phase == Board.Phase.DAY else Board.Phase.DAY
	s.phase_counter = 0
	if out != null:
		out.append({"type": "phase_changed", "phase": s.phase, "cause": cause})
	if was_sleeping and not s.nocturnal_day_sleep():
		_emit_nocturnal_wakes(s, out)

func _emit_nocturnal_wakes(s: Board, out) -> void:
	if out == null:
		return
	for b in s.branch_count():
		for i in s.items[b].size():
			var code := s.items[b][i]
			if not (code & Board.EGG) and not (code & Board.ASLEEP) and Board.is_nocturnal(Board.species_of(code)):
				out.append({"type": "woke", "id": Board.id_of(code), "branch": b, "seat": i})


static func booster_has_effect(s: Board, kind: int) -> bool:
	if s.is_won():
		return false
	if kind == Board.Booster.SAPLING:
		return not s.added_branch
	for b in s.branch_count():
		if kind == Board.Booster.SPRING_RAIN and s.kinds[b] == Board.Kind.GROWING and s.caps[b] < s.capacity:
			return true
		for code in s.items[b]:
			if (kind == Board.Booster.OWL_EYE and (code & Board.EGG) and not (code & Board.REVEALED)) \
					or (kind == Board.Booster.MORNING_BELL and (code & Board.ASLEEP)) \
					or (kind == Board.Booster.SCARECROW and Board.species_of(code) == Board.CROW):
				return true
	return false

func booster(s: Board, kind: int, consume: bool, out) -> void:
	_moonbeam_granted = false
	var home_before := s.homed.size()
	if consume:
		s.inventory[kind] -= 1
	if out != null:
		out.append({"type": "booster_used", "kind": kind, "consumed": consume, "remaining": s.inventory[kind]})
	match kind:
		Board.Booster.FIREFLY:
			return
		Board.Booster.SAPLING:
			var slot := 0
			while true:
				var occupied := false
				for b in s.branch_count():
					if s.rows[b] == slot / 2 and s.rights[b] == slot % 2:
						occupied = true
						break
				if not occupied:
					break
				slot += 1
			s.add_branch(s.capacity, slot % 2 == 1, slot / 2, Board.Kind.NORMAL, PackedInt32Array())
			s.added_branch = true
		Board.Booster.OWL_EYE: _reveal_eggs(s, out)
		Board.Booster.MORNING_BELL: _wake_rows(s, 0, false, out)
		Board.Booster.SCARECROW: _remove_crows(s, 0, false, out)
		Board.Booster.SPRING_RAIN: _grow(s, true, out)
	_finish(s, home_before, false, -1, -1, out)
