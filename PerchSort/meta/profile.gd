class_name Profile
extends RefCounted
## Player progression and economy state. JSON field names match the original profile schema.

const SCHEMA_VERSION := 1
const SAVE_PATH := "user://profile.json"
const BOOSTER_IDS: PackedStringArray = ["firefly", "sapling", "owlEye", "morningBell", "scarecrow", "springRain"]

var coins := 0
var highest_unlocked := 1
var inventory := PackedInt32Array([0, 0, 0, 0, 0, 0])
var stars := PackedInt32Array()     # index 1..40
var clears := PackedInt32Array()    # index 1..40
var best_moves := PackedInt32Array() # index 1..40, fewest counted moves of a win (0 = none)
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


func _init() -> void:
	stars.resize(41)
	clears.resize(41)
	best_moves.resize(41)

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
	for i in range(1, 41):
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
	for w in range(1, 5):
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
	}

## Lenient load: unknown or invalid entries are dropped, like the original deserializer.
static func from_dict(data: Dictionary) -> Profile:
	var p := Profile.new()
	p.coins = maxi(0, int(data.get("coins", 0)))
	p.highest_unlocked = clampi(int(data.get("highestUnlockedLevel", 1)), 1, 40)
	p.muted = bool(data.get("muted", false))
	p.music_muted = bool(data.get("musicMuted", p.muted))   # saves from before the split mute both
	var boosters: Dictionary = data.get("boosters", {})
	for i in BOOSTER_IDS.size():
		p.inventory[i] = maxi(0, int(boosters.get(BOOSTER_IDS[i], 0)))
	var star_list: Array = data.get("bestStars", [])
	var clear_list: Array = data.get("clearCounts", [])
	for level in range(1, 41):
		var s := clampi(int(star_list[level]) if level < star_list.size() else 0, 0, 3)
		p.stars[level] = s
		var c := int(clear_list[level]) if level < clear_list.size() else 0
		p.clears[level] = 0 if s == 0 else maxi(1, c)
		var moves_list: Array = data.get("bestMoves", [])
		p.best_moves[level] = maxi(0, int(moves_list[level])) if s > 0 and level < moves_list.size() else 0
		if s > 0:
			p.highest_unlocked = maxi(p.highest_unlocked, mini(40, level + 1))
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
	for w in data.get("worldRewardsGranted", []):
		if int(w) >= 1 and int(w) <= 4:
			p.rewarded_worlds[int(w)] = true
	return p

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
