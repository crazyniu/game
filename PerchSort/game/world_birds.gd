class_name WorldBirds
extends RefCounted
## Every world has its own kind of bird. A species is a colour slot with its own rules (gift, sleep, nocturnal) in
## every world; a world with a FAMILY shows each slot as that family's bird of the same colour, painted as a whole
## bird under the prefix "<species>_<family>" (tools/birds/world_birds). World 1 keeps the original birds. Outfits
## are worn only by the original birds. The crow is the same troublemaker everywhere.

const FAMILY := {2: "budgie"}

## The painted prefix of `species` in `world` ("" = the original bird). A slot of a family world whose bird is not
## painted yet shows the original bird and says so in the log.
static func look(species: String, world: int) -> String:
	var family: String = FAMILY.get(world, "")
	if family.is_empty() or species == "crow" or species.is_empty():
		return ""
	var key := species + "_" + family
	if Art.has(key + "_hop_01"):
		return key
	push_warning("World %d has no painted %s yet; showing the original %s" % [world, key, species])
	return ""

## The head icon of `species` in `world`.
static func face(species: String, world: int) -> String:
	var key := look(species, world)
	return (key if not key.is_empty() else species) + "_face"
