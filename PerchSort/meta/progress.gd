class_name Progress
extends RefCounted
## Unlocks, economy, world map and daily gift rules. Functions mutate the given profile and return payouts.

const WORLD_COMPLETE_COINS := 500
## Coins for a first clear by stars (1, 2, 3), for a replay, and per star improved on a replay.
const FIRST_CLEAR_COINS := [0, 50, 80, 120]
const REPLAY_COINS := 20
const IMPROVE_COINS := 30
## Star chest: one chest per this many stars; it holds coins and boosters.
const CHEST_STARS := 30
const CHEST_COINS := 300
const CHEST_BOOSTERS := 3
## Seven-day check-in: [coins, booster count] for day 1..7; the streak restarts after a missed day or day 7.
const CHECKIN := [[100, 1], [120, 1], [160, 1], [200, 1], [240, 2], [300, 2], [600, 3]]
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
## Booster drops on a win, so players always hold tools and learn to use them: a first clear drops 1 (2 with three
## stars), every DROP_MILESTONE-th level's first clear drops DROP_MILESTONE_COUNT, a replay drops 1 with
## DROP_REPLAY_CHANCE. Half the drops favour a booster that helps on `next_def` (the next level).
const DROP_MILESTONE := 5
const DROP_MILESTONE_COUNT := 3
const DROP_REPLAY_CHANCE := 0.3

## Applies a win. Returns {"stars", "first_clear", "replay", "improvement", "chorus", "world", "total", "starters",
## "boosters"}; "boosters" lists the dropped booster indices (already added to the inventory).
static func on_win(p: Profile, level: int, counted_moves: int, par: int, chorus_coins: int, next_def := {},
		rng := RandomNumberGenerator.new()) -> Dictionary:
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
		"first_clear": FIRST_CLEAR_COINS[stars] if old == 0 else 0,
		"replay": REPLAY_COINS if old > 0 else 0,
		"improvement": IMPROVE_COINS * maxi(0, stars - old) if old > 0 else 0,
		"chorus": chorus_coins,
		"world": WORLD_COMPLETE_COINS if world_bonus else 0,
	}
	payout["total"] = payout["first_clear"] + payout["replay"] + payout["improvement"] + payout["chorus"] + payout["world"]
	p.coins += payout["total"]
	payout["starters"] = grant_starter_gifts(p)
	payout["boosters"] = win_drops(p, level, stars, old == 0, next_def, rng)
	return payout

## Drops for one win (see DROP_*); adds them to the inventory and returns their indices.
static func win_drops(p: Profile, level: int, stars: int, first_clear: bool, next_def: Dictionary,
		rng: RandomNumberGenerator) -> Array:
	var count := 0
	if first_clear:
		count = DROP_MILESTONE_COUNT if level % DROP_MILESTONE == 0 else (2 if stars == 3 else 1)
	elif rng.randf() < DROP_REPLAY_CHANCE:
		count = 1
	var fitting := []
	for kind in fitting_boosters(next_def):
		if is_booster_unlocked(p, kind):
			fitting.append(kind)
	var pool := unlocked_boosters(p)
	var out := []
	for i in count:
		var kind: int
		if not fitting.is_empty() and (i == 0 or rng.randf() < 0.5):
			kind = fitting[rng.randi() % fitting.size()]
		else:
			kind = pool[rng.randi() % pool.size()]
		p.inventory[kind] += 1
		out.append(kind)
	return out

## ---------- boosters that teach themselves ----------

## Boosters that help with a level's mechanics: sleepers -> Morning Bell, eggs -> Owl Eye, crows -> Scarecrow,
## growing branches -> Spring Rain. Empty for a plain level or an empty definition.
static func fitting_boosters(def: Dictionary) -> Array:
	var text := JSON.stringify(def.get("branches", []))
	var out := []
	if text.contains(":sleep"):
		out.append(Board.Booster.MORNING_BELL)
	if text.contains("egg"):
		out.append(Board.Booster.OWL_EYE)
	if text.contains("crow"):
		out.append(Board.Booster.SCARECROW)
	if text.contains("growing"):
		out.append(Board.Booster.SPRING_RAIN)
	return out

## The booster a level start points at: one the player holds, that helps here, and that was never used yet.
## -1 when none qualifies.
static func booster_to_point_at(p: Profile, def: Dictionary) -> int:
	for kind in fitting_boosters(def):
		if p.inventory[kind] > 0 and not p.has_tutorial(used_flag(kind)):
			return kind
	return -1

## Tutorial flag set the first time a booster is used.
static func used_flag(kind: int) -> String:
	return "used.%s" % BOOSTERS[kind][1]

## Unlocked boosters, for random rewards.
static func unlocked_boosters(p: Profile) -> Array:
	var out := []
	for info in BOOSTERS:
		if is_booster_unlocked(p, info[0]):
			out.append(info[0])
	return out if not out.is_empty() else [Board.Booster.FIREFLY]

static func _random_boosters(p: Profile, count: int, rng: RandomNumberGenerator) -> Array:
	var pool := unlocked_boosters(p)
	var out := []
	for i in count:
		var kind: int = pool[rng.randi() % pool.size()]
		p.inventory[kind] += 1
		out.append(kind)
	return out

## Stars collected towards the next chest and chests ready to open.
static func chest_progress(p: Profile) -> int:
	return p.total_stars() - p.chests_opened * CHEST_STARS

static func chests_ready(p: Profile) -> int:
	return chest_progress(p) / CHEST_STARS

## Opens one ready chest: {"coins", "boosters"} or {} when none is ready.
static func open_chest(p: Profile, rng := RandomNumberGenerator.new()) -> Dictionary:
	if chests_ready(p) <= 0:
		return {}
	p.chests_opened += 1
	p.coins += CHEST_COINS
	return {"coins": CHEST_COINS, "boosters": _random_boosters(p, CHEST_BOOSTERS, rng)}

## The check-in day (1..7) today's claim would be, or 0 when today is already claimed.
static func checkin_day(p: Profile, today: String) -> int:
	if today <= p.last_daily_gift:
		return 0
	var yesterday := Time.get_date_string_from_unix_time(Time.get_unix_time_from_datetime_string(today) - 86400)
	if p.last_daily_gift == yesterday and p.checkin_streak < 7:
		return p.checkin_streak + 1
	return 1

## Claims today's check-in: {"day", "coins", "boosters"} or {} when already claimed today.
static func claim_checkin(p: Profile, today: String, rng := RandomNumberGenerator.new()) -> Dictionary:
	var day := checkin_day(p, today)
	if day == 0:
		return {}
	p.last_daily_gift = today
	p.checkin_streak = day
	var reward: Array = CHECKIN[day - 1]
	p.coins += reward[0]
	return {"day": day, "coins": reward[0], "boosters": _random_boosters(p, reward[1], rng)}

## ---------- daily challenge ----------
const CHALLENGE_COINS := 150

static func challenge_available(p: Profile, today: String) -> bool:
	return today > p.challenge_date

## The day's seed: the same puzzle for the whole day.
static func challenge_seed(today: String) -> int:
	return hash("perch-sort-daily-" + today)

## Pays the daily challenge once a day: {"coins", "boosters"} or {} when already done today.
static func complete_challenge(p: Profile, today: String, rng := RandomNumberGenerator.new()) -> Dictionary:
	if not challenge_available(p, today):
		return {}
	p.challenge_date = today
	p.coins += CHALLENGE_COINS
	return {"coins": CHALLENGE_COINS, "boosters": _random_boosters(p, 1, rng)}

## Older name kept for the check-in: pays today's check-in coins. Returns coins paid.
static func claim_daily_gift(p: Profile, today: String) -> int:
	return claim_checkin(p, today).get("coins", 0)

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
