class_name Aviary
extends RefCounted
## Species cards, first meetings and flocks-home counts.

## [id, display name, debut level]
const SPECIES := [
	["sunny", "Sunny", 1], ["sky", "Skye", 1], ["berry", "Berry", 3], ["mint", "Minty", 6],
	["cocoa", "Cocoa", 9], ["snow", "Flurry", 14], ["ember", "Ember", 19], ["crow", "Rook", 21],
	["ink", "Inky", 26], ["hoot", "Hoot", 33],
]
const NEW_SPECIES_COINS := 20


static func has_species(id: String) -> bool:
	for card in SPECIES:
		if card[0] == id:
			return true
	return false

static func display_name(id: String) -> String:
	for card in SPECIES:
		if card[0] == id:
			return card[1]
	return id

## Marks species present in a starting level as met (canonical order). Returns newly met ids; pays 20 each.
static func on_level_started(p: Profile, species_ids: PackedStringArray) -> Array:
	var met := []
	for card in SPECIES:
		if species_ids.has(card[0]) and not p.has_met(card[0]):
			p.met[card[0]] = true
			met.append(card[0])
	p.coins += met.size() * NEW_SPECIES_COINS
	return met

static func add_flock_home(p: Profile, species: String, count := 1) -> void:
	if species != "crow" and has_species(species):
		p.homes[species] = p.flocks_home(species) + count
