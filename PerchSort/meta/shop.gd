class_name Shop
extends RefCounted
## Shop catalog, purchases and equipping.

enum { BOOSTER, OUTFIT, HOUSE }
const HOUSE_STYLES: PackedStringArray = ["mushroom", "teapot", "lantern", "acorn"]
const OUTFIT_PRICES := [300, 500, 800]

static var _catalog: Array = []
static var _by_id: Dictionary = {}


## Items: {"id", "kind", "price", "quantity", "booster", "species", "style", "index"}.
static func catalog() -> Array:
	if _catalog.is_empty():
		for info in Progress.BOOSTERS:
			_add({"id": "booster." + info[1], "kind": BOOSTER, "price": info[3], "quantity": 1, "booster": info[0]})
			_add({"id": "booster." + info[1] + ".x3", "kind": BOOSTER, "price": (info[3] * 5 + 1) / 2, "quantity": 3, "booster": info[0]})
		for card in Aviary.SPECIES:
			if card[0] == "crow":
				continue
			for index in range(1, 4):
				_add({"id": "outfit.%s.%d" % [card[0], index], "kind": OUTFIT, "price": OUTFIT_PRICES[index - 1],
					"quantity": 1, "species": card[0], "index": index})
		for style in HOUSE_STYLES:
			_add({"id": "house." + style, "kind": HOUSE, "price": 600, "quantity": 1, "style": style})
	return _catalog

static func _add(item: Dictionary) -> void:
	_catalog.append(item)
	_by_id[item["id"]] = item

static func item(id: String) -> Dictionary:
	catalog()
	return _by_id.get(id, {})

## Buy a quantity in one transaction. Returns "" or unknown, locked, owned, quantity, coins.
static func purchase(p: Profile, id: String, quantity := 1) -> String:
	var it := item(id)
	if it.is_empty():
		return "unknown"
	if it["kind"] == BOOSTER and not Progress.is_booster_unlocked(p, it["booster"]):
		return "locked"
	if (it["kind"] == OUTFIT and p.owns_outfit(id)) or (it["kind"] == HOUSE and p.owns_house(it["style"])):
		return "owned"
	if quantity < 1 or (it["kind"] != BOOSTER and quantity != 1):
		return "quantity"
	var total: int = it["price"] * quantity
	if p.coins < total:
		return "coins"
	p.coins -= total
	match it["kind"]:
		BOOSTER: p.inventory[it["booster"]] += it["quantity"] * quantity
		OUTFIT: p.outfits[id] = true
		HOUSE: p.houses[it["style"]] = true
	return ""

## Equip an owned outfit/house; "house.default" and "outfit.<species>.default" reset. Returns "" or an error id.
static func equip(p: Profile, id: String) -> String:
	if id == "house.default":
		p.equipped_house = "default"
		return ""
	if id.begins_with("outfit.") and id.ends_with(".default"):
		var species := id.substr(7, id.length() - 15)
		if not Aviary.has_species(species) or species == "crow":
			return "unknown"
		p.equipped.erase(species)
		return ""
	var it := item(id)
	if it.is_empty():
		return "unknown"
	if it["kind"] == BOOSTER:
		return "not_equippable"
	if it["kind"] == OUTFIT:
		if not p.owns_outfit(id):
			return "not_owned"
		p.equipped[it["species"]] = id
	else:
		if not p.owns_house(it["style"]):
			return "not_owned"
		p.equipped_house = it["style"]
	return ""
