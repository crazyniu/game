class_name Board
extends RefCounted
## Pure board state. A bird is one int: (id << 10) | (species << 5) | flags.

const EGG := 1
const ASLEEP := 2
const MAMA := 4
const CHICK := 8
const REVEALED := 16
const FLAG_MASK := 31

const SPECIES: PackedStringArray = ["sunny", "sky", "berry", "mint", "cocoa", "snow", "ember", "crow", "ink", "hoot"]
const CROW := 7
const INK := 8
const HOOT := 9

enum Kind { NORMAL, TWIG, GROWING, SWIVEL }
## Retained only so stored enum numbers keep their meaning; levels no longer have a day or a night.
enum Phase { DAY, NIGHT }
enum Booster { FIREFLY, SAPLING, OWL_EYE, MORNING_BELL, SCARECROW, SPRING_RAIN }
const BOOSTER_COUNT := 6

var level_id := 0
var world := 1
var variant := "noon"
var capacity := 4
var species_count := 0

# Parallel branch arrays; items[b] is a PackedInt32Array of bird codes, bottom first.
var items: Array[PackedInt32Array] = []
var caps := PackedInt32Array()
var kinds := PackedInt32Array()
var rows := PackedInt32Array()
var rights := PackedByteArray()

var homed := PackedInt32Array()  # species indices in homecoming order
var moves := 0
var counted_moves := 0
var encore := false
var added_branch := false
var light_step := false
var chain := 0
var chorus_coins := 0
var inventory := PackedInt32Array([0, 0, 0, 0, 0, 0])


static func bird(id: int, species: int, flags: int) -> int:
	return (id << 10) | (species << 5) | flags

static func species_of(code: int) -> int:
	return (code >> 5) & 31

static func flags_of(code: int) -> int:
	return code & FLAG_MASK

static func id_of(code: int) -> int:
	return code >> 10

static func with_flags(code: int, flags: int) -> int:
	return (code & ~FLAG_MASK) | flags

static func species_index(name: String) -> int:
	return SPECIES.find(name)

func branch_count() -> int:
	return items.size()

func is_won() -> bool:
	return homed.size() == species_count

## Effective sleep: a bird the level put to sleep.
func is_asleep(code: int) -> bool:
	if code & EGG:
		return false
	return (code & ASLEEP) != 0

func free_slots(b: int) -> int:
	return caps[b] - items[b].size()

func run_count(b: int) -> int:
	var list := items[b]
	var count := list.size()
	if count == 0:
		return 0
	var tip := list[count - 1]
	if (tip & EGG) or is_asleep(tip):
		return 0
	var sp := species_of(tip)
	var start := count - 1
	while start > 0:
		var other := list[start - 1]
		if (other & EGG) or is_asleep(other) or species_of(other) != sp:
			break
		start -= 1
	return count - start

func set_bird(b: int, seat: int, code: int) -> void:
	var list := items[b]
	list[seat] = code
	items[b] = list

func add_branch(capacity_value: int, right: bool, row: int, kind: int, birds: PackedInt32Array) -> void:
	items.append(birds)
	caps.append(capacity_value)
	kinds.append(kind)
	rows.append(row)
	rights.append(1 if right else 0)

func clone(copy_inventory := true) -> Board:
	var c := Board.new()
	c.level_id = level_id; c.world = world; c.variant = variant; c.capacity = capacity
	c.species_count = species_count
	for list in items:
		c.items.append(list.duplicate())
	c.caps = caps.duplicate(); c.kinds = kinds.duplicate(); c.rows = rows.duplicate(); c.rights = rights.duplicate()
	c.homed = homed.duplicate()
	c.moves = moves; c.counted_moves = counted_moves; c.encore = encore; c.added_branch = added_branch
	c.light_step = light_step
	c.chain = chain; c.chorus_coins = chorus_coins
	c.inventory = inventory.duplicate() if copy_inventory else inventory
	return c

## Search key without bird ids, economy or move totals. preserve_order keeps homecoming order.
func search_key(preserve_order: bool) -> PackedInt32Array:
	var key := PackedInt32Array()
	# Bits 0-11 once held the day/night phase; stored hints depend on this layout, so the flags keep their bits.
	key.append((int(light_step) << 12) | (int(encore) << 13))
	if preserve_order:
		key.append(homed.size())
		key.append_array(homed)
	else:
		var mask := 0
		for s in homed:
			mask |= 1 << s
		key.append(mask)
	for b in items.size():
		var list := items[b]
		key.append(rights[b] | (rows[b] << 1) | (kinds[b] << 8) | (caps[b] << 12) | (list.size() << 18))
		for code in list:
			key.append(code & 1023)
	return key
