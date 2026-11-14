class_name Levels
extends RefCounted
## Level catalog loading and board construction (validation mirrors the original rules).

const PATH := "res://levels/levels.json"
const COUNT := 210
const WORLDS := 10
const PER_WORLD := 21

static var _catalog: Array = []


static func all() -> Array:
	if _catalog.is_empty():
		var data = JSON.parse_string(FileAccess.get_file_as_string(PATH))
		_catalog = data["levels"]
	return _catalog

static func get_level(id: int) -> Dictionary:
	for level in all():
		if int(level["id"]) == id:
			return level
	push_error("Unknown level %d" % id)
	return {}

static func world_of(level: Dictionary) -> int:
	var w := int(level.get("world", 0))
	return 1 if w == 0 else w

## Returns null and logs an error when the definition is invalid.
static func create_board(level: Dictionary, inventory := PackedInt32Array()) -> Board:
	var capacity := int(level.get("capacity", 0))
	var branches: Array = level.get("branches", [])
	if int(level.get("id", 0)) < 1 or capacity < 1 or int(level.get("par", 0)) < 0 or branches.is_empty():
		push_error("Invalid level header"); return null
	var board := Board.new()
	board.level_id = int(level["id"])
	board.world = world_of(level)
	var variant: String = str(level.get("variant", ""))
	board.variant = "noon" if variant.is_empty() else variant
	board.capacity = capacity
	var counts := {}
	var slots := {}
	var next_id := 0
	for def in branches:
		var side: String = def.get("side", "")
		var row := int(def.get("row", -1))
		if (side != "L" and side != "R") or row < 0 or slots.has(side + str(row)):
			push_error("Invalid branch definition"); return null
		slots[side + str(row)] = true
		var kind := _kind(str(def.get("kind", "")))
		var authored_seats := int(def.get("seats", 0))
		var seats := 2 if kind == Board.Kind.TWIG else (authored_seats if kind == Board.Kind.GROWING else capacity)
		var tokens: Array = def.get("birds", [])
		if kind < 0 or seats < 1 or (kind == Board.Kind.GROWING and (seats < 2 or seats > capacity)) \
				or (kind != Board.Kind.GROWING and authored_seats != 0) or tokens.size() > seats:
			push_error("Invalid branch seats"); return null
		var birds := PackedInt32Array()
		for token in tokens:
			var code := _parse_bird(next_id, str(token))
			if code < 0:
				return null
			next_id += 1
			var sp := Board.species_of(code)
			if sp != Board.CROW:
				counts[sp] = counts.get(sp, 0) + 1
			birds.append(code)
		board.add_branch(seats, side == "R", row, kind, birds)
	if counts.is_empty():
		push_error("A level needs a real species"); return null
	for c in counts.values():
		if c != capacity:
			push_error("Each real species must have exactly capacity birds"); return null
	board.species_count = counts.size()
	if inventory.size() == Board.BOOSTER_COUNT:
		board.inventory = inventory.duplicate()
	return board

static func _kind(kind: String) -> int:
	match kind:
		"", "normal": return Board.Kind.NORMAL
		"twig": return Board.Kind.TWIG
		"growing": return Board.Kind.GROWING
		"swivel": return Board.Kind.SWIVEL
	push_error("Unknown branch kind: " + kind)
	return -1

static func _parse_bird(id: int, token: String) -> int:
	if token.strip_edges().is_empty():
		push_error("Empty bird token"); return -1
	var species := token
	var flags := 0
	var colon := token.find(":")
	if colon >= 0:
		var prefix := token.substr(0, colon)
		var suffix := token.substr(colon + 1)
		if suffix.contains(":"):
			push_error("Invalid bird token: " + token); return -1
		if prefix == "egg":
			species = suffix; flags = Board.EGG
		else:
			species = prefix
			match suffix:
				"sleep": flags = Board.ASLEEP
				"mama": flags = Board.MAMA
				"chick": flags = Board.CHICK
				_:
					push_error("Invalid bird token: " + token); return -1
	if species == "egg" or (species == "crow" and flags != 0):
		push_error("Invalid special bird token: " + token); return -1
	var index := Board.species_index(species)
	if index < 0:
		push_error("Unknown species: " + species); return -1
	return Board.bird(id, index, flags)

## Species ids present on the board in board order; crows only when include_crow.
static func species_ids(board: Board, include_crow := false) -> PackedStringArray:
	var out := PackedStringArray()
	for list in board.items:
		for code in list:
			var name: String = Board.SPECIES[Board.species_of(code)]
			if (include_crow or name != "crow") and not out.has(name):
				out.append(name)
	return out
