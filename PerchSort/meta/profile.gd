class_name Profile
extends RefCounted
## Player progression and economy state. JSON field names match the original profile schema.

const SCHEMA_VERSION := 3
## Schema 2 put each world's levels in order of difficulty (opening level first). A schema 1 save's per-level
## records move from the old level number (key) to the new one (value) on load.
const LEVEL_RENUMBER := {1: 1, 2: 2, 3: 3, 4: 6, 5: 7, 6: 5, 7: 10, 8: 8, 9: 4, 10: 9, 11: 11, 12: 17, 13: 16, 14: 13, 15: 12, 16: 14, 17: 18, 18: 15, 19: 20, 20: 19, 21: 21, 22: 25, 23: 24, 24: 22, 25: 27, 26: 26, 27: 28, 28: 23, 29: 29, 30: 30, 31: 31, 32: 35, 33: 32, 34: 36, 35: 33, 36: 38, 37: 37, 38: 40, 39: 34, 40: 39}
## Schema 2 to 3: the forty levels took new places in the hundred-level order.
const LEVEL_EXPANSION := {1: 1, 2: 2, 3: 3, 4: 4, 5: 5, 6: 6, 7: 7, 8: 10, 9: 35, 10: 9, 11: 11, 12: 21, 13: 28, 14: 22, 15: 24, 16: 15, 17: 12, 18: 37, 19: 39, 20: 46, 21: 31, 22: 41, 23: 48, 24: 34, 25: 33, 26: 44, 27: 53, 28: 67, 29: 58, 30: 72, 31: 51, 32: 61, 33: 71, 34: 66, 35: 60, 36: 43, 37: 42, 38: 62, 39: 64, 40: 63}
const SAVE_PATH := "user://profile.json"
const BOOSTER_IDS: PackedStringArray = ["firefly", "sapling", "owlEye", "morningBell", "scarecrow", "springRain"]

var coins := 0
var highest_unlocked := 1
var inventory := PackedInt32Array([0, 0, 0, 0, 0, 0])
var stars := PackedInt32Array()     # index 1..Levels.COUNT
var clears := PackedInt32Array()    # index 1..Levels.COUNT
var best_moves := PackedInt32Array() # index 1..Levels.COUNT, fewest counted moves of a win (0 = none)
var outfits := {}                   # item id -> true
var equipped := {}                  # species id -> outfit item id
var houses := {}                    # style id -> true
var equipped_house := "default"
var met := {}                       # species id -> true
var homes := {}                     # species id -> count
var last_daily_gift := ""
var tutorials := {}
var starters := {}                  # booster index -> true
var rewarded_worlds := {}           # world -> true
var muted := false                  # sound effects off
var music_muted := false            # background music off
var level_gifts := {}               # level -> true: old start-gift record, no longer granted, kept in saves
var chests_opened := 0              # star chests opened (one per CHEST_STARS stars earned)
var draws_date := ""                # day the lucky draw chances were handed out
var draws_left := 0                 # draws still unused on draws_date
var checkin_streak := 0             # day of the 7-day check-in reached on last_daily_gift (1..7)
var challenge_date := ""            # last day the daily challenge was completed
var vibration := true               # short haptic pulses on mobile


func _init() -> void:
	stars.resize(Levels.COUNT + 1)
	clears.resize(Levels.COUNT + 1)
	best_moves.resize(Levels.COUNT + 1)

func best_stars(level: int) -> int:
	return stars[level]

func owns_outfit(item_id: String) -> bool:
	return outfits.has(item_id)

func owns_house(style: String) -> bool:
	return houses.has(style)

func has_met(species: String) -> bool:
	return met.has(species)

func flocks_home(species: String) -> int:
	return homes.get(species, 0)

func has_tutorial(flag: String) -> bool:
	return tutorials.has(flag)

func set_tutorial(flag: String) -> void:
	tutorials[flag] = true

## Records a win's counted moves; keeps the fewest.
func record_moves(level: int, moves: int) -> void:
	if best_moves[level] == 0 or moves < best_moves[level]:
		best_moves[level] = moves

func clears_count() -> int:
	var total := 0
	for i in range(1, Levels.COUNT + 1):
		total += clears[i]
	return total

func duplicate_profile() -> Profile:
	return Profile.from_dict(to_dict())


func to_dict() -> Dictionary:
	var boosters := {}
	var starter_ids := []
	for i in BOOSTER_IDS.size():
		boosters[BOOSTER_IDS[i]] = inventory[i]
		if starters.has(i):
			starter_ids.append(BOOSTER_IDS[i])
	var worlds := []
	for w in range(1, Levels.WORLDS + 1):
		if rewarded_worlds.has(w):
			worlds.append(w)
	return {
		"schemaVersion": SCHEMA_VERSION, "coins": coins, "highestUnlockedLevel": highest_unlocked,
		"boosters": boosters, "bestStars": Array(stars), "clearCounts": Array(clears), "bestMoves": Array(best_moves),
		"ownedOutfits": _sorted(outfits), "equippedOutfits": equipped.duplicate(),
		"ownedBirdhouseStyles": _sorted(houses), "equippedBirdhouseStyle": equipped_house,
		"speciesMet": _sorted(met), "flocksHome": homes.duplicate(), "lastDailyGiftDate": last_daily_gift,
		"tutorialFlags": _sorted(tutorials), "starterGiftsGranted": starter_ids,
		"worldRewardsGranted": worlds, "muted": muted, "musicMuted": music_muted,
		"levelGifts": _sorted_levels(level_gifts), "chestsOpened": chests_opened, "checkinStreak": checkin_streak, "drawDate": draws_date, "drawsLeft": draws_left, "challengeDate": challenge_date,
		"vibration": vibration,
	}

## Lenient load: unknown or invalid entries are dropped, like the original deserializer.
static func from_dict(data: Dictionary) -> Profile:
	var p := Profile.new()
	p.coins = maxi(0, int(data.get("coins", 0)))
	p.highest_unlocked = clampi(int(data.get("highestUnlockedLevel", 1)), 1, Levels.COUNT)
	p.muted = bool(data.get("muted", false))
	p.music_muted = bool(data.get("musicMuted", p.muted))   # saves from before the split mute both
	p.vibration = bool(data.get("vibration", true))
	var boosters: Dictionary = data.get("boosters", {})
	for i in BOOSTER_IDS.size():
		p.inventory[i] = maxi(0, int(boosters.get(BOOSTER_IDS[i], 0)))
	var star_list: Array = data.get("bestStars", [])
	var clear_list: Array = data.get("clearCounts", [])
	var moves_source: Array = data.get("bestMoves", [])
	var schema := int(data.get("schemaVersion", 1))
	for step in [[2, LEVEL_RENUMBER], [3, LEVEL_EXPANSION]]:
		if schema < step[0]:
			star_list = _renumbered(star_list, step[1])
			clear_list = _renumbered(clear_list, step[1])
			moves_source = _renumbered(moves_source, step[1])
	for level in range(1, Levels.COUNT + 1):
		var s := clampi(int(star_list[level]) if level < star_list.size() else 0, 0, 3)
		p.stars[level] = s
		var c := int(clear_list[level]) if level < clear_list.size() else 0
		p.clears[level] = 0 if s == 0 else maxi(1, c)
		var moves_list: Array = moves_source
		p.best_moves[level] = maxi(0, int(moves_list[level])) if s > 0 and level < moves_list.size() else 0
		if s > 0:
			p.highest_unlocked = maxi(p.highest_unlocked, mini(Levels.COUNT, level + 1))
	for id in data.get("ownedOutfits", []):
		var item := Shop.item(str(id))
		if not item.is_empty() and item["kind"] == Shop.OUTFIT:
			p.outfits[str(id)] = true
	var eq: Dictionary = data.get("equippedOutfits", {})
	for species in eq:
		var item := Shop.item(str(eq[species]))
		if p.outfits.has(str(eq[species])) and not item.is_empty() and item["species"] == species:
			p.equipped[species] = str(eq[species])
	for id in data.get("ownedBirdhouseStyles", []):
		if Shop.HOUSE_STYLES.has(str(id)):
			p.houses[str(id)] = true
	var house := str(data.get("equippedBirdhouseStyle", "default"))
	if p.houses.has(house):
		p.equipped_house = house
	for id in data.get("speciesMet", []):
		if Aviary.has_species(str(id)):
			p.met[str(id)] = true
	var home_counts: Dictionary = data.get("flocksHome", {})
	for id in home_counts:
		if Aviary.has_species(id) and id != "crow" and int(home_counts[id]) > 0:
			p.homes[id] = int(home_counts[id])
	var daily := str(data.get("lastDailyGiftDate", ""))
	if daily.length() == 10 and daily[4] == "-" and daily[7] == "-":
		p.last_daily_gift = daily
	for flag in data.get("tutorialFlags", []):
		if not str(flag).is_empty():
			p.tutorials[str(flag)] = true
	for id in data.get("starterGiftsGranted", []):
		var index := BOOSTER_IDS.find(str(id))
		if index >= 0:
			p.starters[index] = true
	for level in data.get("levelGifts", []):
		if int(level) >= 1 and int(level) <= Levels.COUNT:
			p.level_gifts[int(level)] = true
	p.chests_opened = maxi(0, int(data.get("chestsOpened", 0)))
	p.checkin_streak = clampi(int(data.get("checkinStreak", 0)), 0, 7)
	p.draws_date = str(data.get("drawDate", ""))
	p.draws_left = clampi(int(data.get("drawsLeft", 0)), 0, 99)
	p.challenge_date = str(data.get("challengeDate", ""))
	for w in data.get("worldRewardsGranted", []):
		if int(w) >= 1 and int(w) <= Levels.WORLDS:
			p.rewarded_worlds[int(w)] = true
	return p

## A per-level list (index = level) from the schema 1 level order moved to the current order.
static func _renumbered(list: Array, table: Dictionary) -> Array:
	var out := []
	out.resize(Levels.COUNT + 1)
	out.fill(0)
	for old in table:
		if old < list.size():
			out[table[old]] = list[old]
	return out

static func _sorted_levels(d: Dictionary) -> Array:
	var out := d.keys()
	out.sort()
	return out

func total_stars() -> int:
	var n := 0
	for level in range(1, Levels.COUNT + 1):
		n += stars[level]
	return n

func save(path := SAVE_PATH) -> void:
	var file := FileAccess.open(path, FileAccess.WRITE)
	if file:
		file.store_string(JSON.stringify(to_dict()))

static func load_saved(path := SAVE_PATH) -> Profile:
	if not FileAccess.file_exists(path):
		return Profile.new()
	var data = JSON.parse_string(FileAccess.get_file_as_string(path))
	return Profile.from_dict(data) if data is Dictionary else Profile.new()

static func _sorted(set: Dictionary) -> Array:
	var keys := set.keys()
	keys.sort()
	return keys
