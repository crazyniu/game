class_name Profile
extends RefCounted
## Player progression and economy state. JSON field names match the original profile schema.

const SCHEMA_VERSION := 5
## Schema 2 put each world's levels in order of difficulty (opening level first). A schema 1 save's per-level
## records move from the old level number (key) to the new one (value) on load.
const LEVEL_RENUMBER := {1: 1, 2: 2, 3: 3, 4: 6, 5: 7, 6: 5, 7: 10, 8: 8, 9: 4, 10: 9, 11: 11, 12: 17, 13: 16, 14: 13, 15: 12, 16: 14, 17: 18, 18: 15, 19: 20, 20: 19, 21: 21, 22: 25, 23: 24, 24: 22, 25: 27, 26: 26, 27: 28, 28: 23, 29: 29, 30: 30, 31: 31, 32: 35, 33: 32, 34: 36, 35: 33, 36: 38, 37: 37, 38: 40, 39: 34, 40: 39}
## Schema 2 to 3: the forty levels took new places in the hundred-level order.
const LEVEL_EXPANSION := {1: 1, 2: 2, 3: 3, 4: 4, 5: 5, 6: 6, 7: 7, 8: 10, 9: 35, 10: 9, 11: 11, 12: 21, 13: 28, 14: 22, 15: 24, 16: 15, 17: 12, 18: 37, 19: 39, 20: 46, 21: 31, 22: 41, 23: 48, 24: 34, 25: 33, 26: 44, 27: 53, 28: 67, 29: 58, 30: 72, 31: 51, 32: 61, 33: 71, 34: 66, 35: 60, 36: 43, 37: 42, 38: 62, 39: 64, 40: 63}
## The continuous difficulty order keeps every puzzle and moves its records with it.
const LEVEL_DIFFICULTY := {1: 1, 2: 2, 3: 4, 4: 5, 5: 6, 6: 12, 7: 13, 8: 19, 9: 26, 10: 27, 11: 7, 12: 14, 13: 18, 14: 17, 15: 16, 16: 25, 17: 22, 18: 23, 19: 28, 20: 24, 21: 3, 22: 15, 23: 32, 24: 33, 25: 29, 26: 35, 27: 34, 28: 43, 29: 39, 30: 36, 31: 8, 32: 37, 33: 30, 34: 20, 35: 41, 36: 45, 37: 42, 38: 40, 39: 46, 40: 50, 41: 11, 42: 21, 43: 38, 44: 31, 45: 44, 46: 47, 47: 55, 48: 49, 49: 54, 50: 56, 51: 9, 52: 52, 53: 51, 54: 59, 55: 61, 56: 57, 57: 65, 58: 62, 59: 66, 60: 67, 61: 10, 62: 53, 63: 60, 64: 48, 65: 58, 66: 73, 67: 64, 68: 70, 69: 68, 70: 76, 71: 63, 72: 69, 73: 71, 74: 72, 75: 74, 76: 75, 77: 81, 78: 78, 79: 84, 80: 86, 81: 80, 82: 77, 83: 88, 84: 85, 85: 83, 86: 87, 87: 94, 88: 82, 89: 91, 90: 93, 91: 97, 92: 79, 93: 95, 94: 89, 95: 90, 96: 92, 97: 98, 98: 99, 99: 96, 100: 100}
const SAVE_PATH := "user://profile.json"
const BOOSTER_IDS: PackedStringArray = ["firefly", "sapling", "owlEye", "morningBell", "scarecrow", "springRain"]
## Schema 5: every world grew from 10 to 21 levels; an old level keeps its place among its world's odd numbers.
const LEVEL_SPREAD := {1: 1, 2: 3, 3: 5, 4: 7, 5: 9, 6: 11, 7: 13, 8: 15, 9: 17, 10: 19, 11: 22, 12: 24, 13: 26, 14: 28, 15: 30, 16: 32, 17: 34, 18: 36, 19: 38, 20: 40, 21: 43, 22: 45, 23: 47, 24: 49, 25: 51, 26: 53, 27: 55, 28: 57, 29: 59, 30: 61, 31: 64, 32: 66, 33: 68, 34: 70, 35: 72, 36: 74, 37: 76, 38: 78, 39: 80, 40: 82, 41: 85, 42: 87, 43: 89, 44: 91, 45: 93, 46: 95, 47: 97, 48: 99, 49: 101, 50: 103, 51: 106, 52: 108, 53: 110, 54: 112, 55: 114, 56: 116, 57: 118, 58: 120, 59: 122, 60: 124, 61: 127, 62: 129, 63: 131, 64: 133, 65: 135, 66: 137, 67: 139, 68: 141, 69: 143, 70: 145, 71: 148, 72: 150, 73: 152, 74: 154, 75: 156, 76: 158, 77: 160, 78: 162, 79: 164, 80: 166, 81: 169, 82: 171, 83: 173, 84: 175, 85: 177, 86: 179, 87: 181, 88: 183, 89: 185, 90: 187, 91: 190, 92: 192, 93: 194, 94: 196, 95: 198, 96: 200, 97: 202, 98: 204, 99: 206, 100: 208}

## A damaged save without a valid backup must never be overwritten by a fresh fallback profile.
var _blocked_save_path := ""

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
var checkin_streak := 0             # daily reward calendar day claimed on last_daily_gift (1..7)
var challenge_date := ""            # old daily-challenge record, no longer used, kept in saves
var vibration := true               # short haptic pulses on mobile
var language := ""                  # "" follows the device; a language code is the player's choice
var last_level := 0                 # last level opened (0 = none): Play continues from it


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
		"vibration": vibration, "language": language, "lastLevel": last_level,
	}

## Lenient load: unknown or invalid entries are dropped, like the original deserializer.
static func from_dict(data: Dictionary) -> Profile:
	var p := Profile.new()
	p.coins = maxi(0, int(data.get("coins", 0)))
	p.highest_unlocked = clampi(int(data.get("highestUnlockedLevel", 1)), 1, Levels.COUNT)
	p.muted = bool(data.get("muted", false))
	p.music_muted = bool(data.get("musicMuted", p.muted))   # saves from before the split mute both
	p.vibration = bool(data.get("vibration", true))
	var language := str(data.get("language", ""))
	p.language = language if language.length() in range(2, 9) and language.to_lower() == language else ""
	var boosters: Dictionary = data.get("boosters", {})
	for i in BOOSTER_IDS.size():
		p.inventory[i] = maxi(0, int(boosters.get(BOOSTER_IDS[i], 0)))
	var star_list: Array = data.get("bestStars", [])
	var clear_list: Array = data.get("clearCounts", [])
	var moves_source: Array = data.get("bestMoves", [])
	var gift_list: Array = data.get("levelGifts", [])
	var schema := int(data.get("schemaVersion", 1))
	for step in [[2, LEVEL_RENUMBER], [3, LEVEL_EXPANSION], [4, LEVEL_DIFFICULTY], [5, LEVEL_SPREAD]]:
		if schema < step[0]:
			star_list = _renumbered(star_list, step[1])
			clear_list = _renumbered(clear_list, step[1])
			moves_source = _renumbered(moves_source, step[1])
			var unlocked := 1
			for old in step[1]:
				if old <= p.highest_unlocked:
					unlocked = maxi(unlocked, step[1][old])
			p.highest_unlocked = unlocked
			var gifts := []
			for old in gift_list:
				if step[1].has(int(old)):
					gifts.append(step[1][int(old)])
			gift_list = gifts
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
	for level in gift_list:
		if int(level) >= 1 and int(level) <= Levels.COUNT:
			p.level_gifts[int(level)] = true
	p.chests_opened = maxi(0, int(data.get("chestsOpened", 0)))
	p.checkin_streak = clampi(int(data.get("checkinStreak", 0)), 0, 7)
	p.draws_date = str(data.get("drawDate", ""))
	p.draws_left = clampi(int(data.get("drawsLeft", 0)), 0, 99)
	p.challenge_date = str(data.get("challengeDate", ""))
	var last := int(data.get("lastLevel", 0))
	p.last_level = clampi(LEVEL_SPREAD.get(last, 0) if schema < 5 else last, 0, Levels.COUNT)
	for w in data.get("worldRewardsGranted", []):
		if int(w) >= 1 and int(w) <= Levels.WORLDS:
			p.rewarded_worlds[int(w)] = true
	return p

## Move a per-level list (index = level) through one catalog migration.
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

## Write a complete temporary file before replacing the primary. Keep the previous valid primary as backup.
func save(path := SAVE_PATH) -> void:
	if path == _blocked_save_path:
		push_error("Profile save blocked: preserve damaged save at " + path)
		return
	var text := JSON.stringify(to_dict())
	if not _write_complete(path + ".tmp", text):
		return
	if FileAccess.file_exists(path):
		var previous := FileAccess.get_file_as_string(path)
		if _parse_save(previous) is Dictionary:
			if not _write_complete(path + ".bak.tmp", previous):
				return
			if DirAccess.rename_absolute(path + ".bak.tmp", path + ".bak") != OK:
				push_error("Could not replace profile backup: " + path)
				return
	if DirAccess.rename_absolute(path + ".tmp", path) != OK:
		push_error("Could not replace profile: " + path)

static func _write_complete(path: String, text: String) -> bool:
	var file := FileAccess.open(path, FileAccess.WRITE)
	if file == null:
		push_error("Could not open profile temporary file: " + path)
		return false
	file.store_string(text)
	file.flush()
	var error := file.get_error()
	file.close()
	if error != OK or FileAccess.get_file_as_string(path) != text:
		push_error("Incomplete profile write: " + path)
		return false
	return true

## JSON.parse_string logs invalid JSON as an engine error; inspect parse status explicitly for recovery instead.
static func _parse_save(text: String) -> Variant:
	var json := JSON.new()
	return json.data if json.parse(text) == OK and json.data is Dictionary else null

static func load_saved(path := SAVE_PATH) -> Profile:
	if FileAccess.file_exists(path):
		var data = _parse_save(FileAccess.get_file_as_string(path))
		if data is Dictionary:
			return Profile.from_dict(data)
	if FileAccess.file_exists(path + ".bak"):
		var backup = _parse_save(FileAccess.get_file_as_string(path + ".bak"))
		if backup is Dictionary:
			push_warning("Recovered previous profile from backup: " + path)
			return Profile.from_dict(backup)
	var p := Profile.new()
	if FileAccess.file_exists(path) or FileAccess.file_exists(path + ".bak"):
		p._blocked_save_path = path
		push_error("Profile is damaged; originals preserved, automatic saving blocked: " + path)
	return p

static func _sorted(set: Dictionary) -> Array:
	var keys := set.keys()
	keys.sort()
	return keys
