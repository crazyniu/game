class_name Progress
extends RefCounted
## Unlocks, economy, world map and daily gift rules. Functions mutate the given profile and return payouts.

const WORLD_COMPLETE_COINS := 200
## [booster index, id, unlock level, price, starter count]
const BOOSTERS := [
	[0, "firefly", 2, 30, 3], [1, "sapling", 4, 60, 2], [2, "owlEye", 7, 80, 2],
	[3, "morningBell", 12, 80, 2], [4, "scarecrow", 22, 100, 2], [5, "springRain", 32, 100, 2],
]
const WORLD_NAMES: PackedStringArray = ["", "Green Forest", "Bamboo Forest", "Autumn Forest", "Snow Forest"]


static func booster_unlock_level(kind: int) -> int:
	return BOOSTERS[kind][2]

static func booster_price(kind: int) -> int:
	return BOOSTERS[kind][3]

static func world_of_level(level: int) -> int:
	return (level - 1) / 10 + 1

static func is_world_unlocked(p: Profile, world: int) -> bool:
	return world == 1 or p.best_stars((world - 1) * 10) > 0

static func is_level_unlocked(p: Profile, level: int) -> bool:
	return level == 1 or (is_world_unlocked(p, world_of_level(level)) and p.best_stars(level - 1) > 0)

static func is_world_complete(p: Profile, world: int) -> bool:
	for level in range(world * 10 - 9, world * 10 + 1):
		if p.best_stars(level) == 0:
			return false
	return true

static func is_booster_unlocked(p: Profile, kind: int) -> bool:
	return p.highest_unlocked >= booster_unlock_level(kind)

## Grants each unlocked booster's starter stock once. Returns the granted booster indices.
static func grant_starter_gifts(p: Profile) -> Array:
	var granted := []
	for info in BOOSTERS:
		var kind: int = info[0]
		if is_booster_unlocked(p, kind) and not p.starters.has(kind):
			p.inventory[kind] += info[4]
			p.starters[kind] = true
			granted.append(kind)
	return granted

## World default booster for the level card and tool bar context slot.
static func world_booster_for(level: int) -> int:
	if level <= 10:
		return Board.Booster.OWL_EYE
	if level <= 20:
		return Board.Booster.MORNING_BELL
	return Board.Booster.SCARECROW if level <= 30 else Board.Booster.SPRING_RAIN

static func stars_for_moves(counted: int, par: int) -> int:
	return GameSession.stars_for(counted, par)

## Applies a win. Returns {"stars", "first_clear", "replay", "improvement", "chorus", "world", "total", "starters"}.
static func on_win(p: Profile, level: int, counted_moves: int, par: int, chorus_coins: int) -> Dictionary:
	var stars := stars_for_moves(counted_moves, par)
	var old := p.best_stars(level)
	p.stars[level] = maxi(old, stars)
	p.clears[level] += 1
	p.highest_unlocked = maxi(p.highest_unlocked, mini(40, level + 1))
	var world := world_of_level(level)
	var world_bonus := not p.rewarded_worlds.has(world) and is_world_complete(p, world)
	if world_bonus:
		p.rewarded_worlds[world] = true
	var payout := {
		"stars": stars,
		"first_clear": 10 + 5 * stars if old == 0 else 0,
		"replay": 5 if old > 0 else 0,
		"improvement": 5 * maxi(0, stars - old) if old > 0 else 0,
		"chorus": chorus_coins,
		"world": WORLD_COMPLETE_COINS if world_bonus else 0,
	}
	payout["total"] = payout["first_clear"] + payout["replay"] + payout["improvement"] + payout["chorus"] + payout["world"]
	p.coins += payout["total"]
	payout["starters"] = grant_starter_gifts(p)
	return payout

## Pays the daily gift once per calendar day (never when the date moves backwards). Returns coins paid.
static func claim_daily_gift(p: Profile, today: String) -> int:
	if today <= p.last_daily_gift:
		return 0
	p.last_daily_gift = today
	p.coins += 50
	return 50

static func today() -> String:
	return Time.get_date_string_from_system()

## Next playable uncleared level, or 0 when every unlocked level is cleared.
static func next_level(p: Profile) -> int:
	for level in range(1, 41):
		if p.best_stars(level) == 0 and is_level_unlocked(p, level):
			return level
	return 0

## "locked", "current" or "cleared".
static func node_state(p: Profile, level: int) -> String:
	if p.best_stars(level) > 0:
		return "cleared"
	return "current" if is_level_unlocked(p, level) else "locked"
