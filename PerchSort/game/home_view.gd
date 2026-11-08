class_name HomeView
extends Control
## The game's home page: one painted forest clearing with the logo, live birds on two painted branches, a big Play
## that continues at the next level, and the map, shop, aviary and help as painted picture buttons.

signal play_requested(level: int)
signal map_requested
signal shop_requested
signal aviary_requested
signal settings_requested
signal help_requested
signal checkin_requested
signal chest_requested

const LOGO := Rect2(150, 170, 780, 520)
## Perch lines measured on tools/home/home_raw.png (tools/scene/cut_branches.py cut_slot) and seat x positions.
const LEFT_PERCH := 1042.0
const RIGHT_PERCH := 1243.0
const LEFT_SEATS := [120.0, 250.0, 380.0, 510.0]
const RIGHT_SEATS := [470.0, 600.0, 730.0]
const BIRD_SCALE := 0.95
const PLAY_Y := 1520.0
const PLAY_W := 520.0
const PLAY_H := 152.0          # at most 1.25 x the 128 secondary buttons
const NAV_Y := 1724.0
const NAV_GAP := 200.0
const FLIT_EVERY := 6.0

var profile: Profile
var instant := false
var _birds: Array = []      # [BirdView, side ("L"/"R"), seat index]
var _logo: TextureRect
var _clock := 0.0
var _next_flit := FLIT_EVERY

func setup(p: Profile) -> void:
	profile = p
	for child in get_children():
		child.queue_free()
	_birds.clear()
	size = Vector2(1080, 1920)
	UI.image(self, "home_bg", Rect2(0, 0, 1080, 1920), false)
	_logo = UI.art(self, "home_logo", LOGO)
	_logo.pivot_offset = _logo.size * Vector2(0.5, 0.9)
	_build_birds()
	var coins := str(profile.coins)
	UI.counter(self, Vector2(48, 56), "coin", coins).name = "Coins"
	UI.button(self, "Settings", Vector2(968, 104), "cream", "gear", func(): settings_requested.emit())
	var level := _next_level()
	# Which level Play opens: a cream chip with the map picture and the number, right above the button.
	var num_w := UI.number_width(str(level), 44.0)
	var chip_w := 20.0 + 60.0 + 12.0 + num_w + 32.0
	var chip := UI.shape(self, Rect2(540.0 - chip_w * 0.5, PLAY_Y - 160.0, chip_w, 80.0), "cream", 40.0, 6.0)
	chip.name = "Level"
	UI.art(chip, "pic_map", Rect2(20, 8, 60, 60))
	UI.number(chip, str(level), Rect2(92, 16, num_w + 2.0, 44), UI.INK, -1)
	var play := UI.pill(self, "Play", Rect2(540.0 - PLAY_W * 0.5, PLAY_Y - PLAY_H * 0.5, PLAY_W, PLAY_H), "green",
		"action.play", func(): play_requested.emit(level), "", 64.0)
	play.name = "Play"
	# Rewards on the left (check-in and star chest, like the gift corner of casual games), places along the bottom.
	var side := [["Checkin", "calendar", checkin_requested, "home.checkin", Vector2(112, 1150)],
		["Chest", "chest", chest_requested, "home.chest", Vector2(112, 1330)]]
	for item in side:
		var sb := UI.button(self, item[0], item[4], "cream", item[1], (item[2] as Signal).emit)
		if item[0] == "Chest" and Progress.chests_ready(profile) > 0:
			UI.badge(sb, str(Progress.chests_ready(profile)))
		elif item[0] == "Checkin" and Progress.checkin_day(profile, Progress.today()) > 0:
			UI.badge(sb, "1")
		var sw := minf(190.0, UI.label_width(item[3], 32.0))
		UI.label(self, item[3], Rect2(item[4].x - sw * 0.5, item[4].y + 60.0, sw, 56.0), 32.0, UI.INK).name = "Caption"
	var buttons := [["Map", "map", map_requested, "home.map"], ["Shop", "shop", shop_requested, "home.shop"],
		["Aviary", "aviary", aviary_requested, "home.aviary"], ["Help", "info", help_requested, "home.help"]]
	for i in buttons.size():
		var sig: Signal = buttons[i][2]
		var x := 540.0 + (i - (buttons.size() - 1) * 0.5) * NAV_GAP
		UI.button(self, buttons[i][0], Vector2(x, NAV_Y), "cream", buttons[i][1], sig.emit)
		var lw := minf(180.0, UI.label_width(buttons[i][3], 34.0))
		UI.label(self, buttons[i][3], Rect2(x - lw * 0.5, NAV_Y + 62.0, lw, 60.0), 34.0, UI.INK).name = "Caption"
	if not instant:
		_entrance()

## The next level to play: the first unlocked level without stars, else the highest unlocked one.
func _next_level() -> int:
	for level in range(1, profile.highest_unlocked + 1):
		if profile.best_stars(level) == 0:
			return level
	return profile.highest_unlocked

## Met birds (the first five met, else the starters) on the two branches, alive.
func _build_birds() -> void:
	var kinds: Array = []
	for card in Aviary.SPECIES:
		if card[0] != "crow" and profile.met.has(card[0]):
			kinds.append(card[0])
	for fallback in ["sunny", "sky", "berry", "mint", "cocoa"]:
		if kinds.size() >= 5:
			break
		if not kinds.has(fallback):
			kinds.append(fallback)
	var places := [["L", 0], ["L", 1], ["L", 2], ["R", 1], ["R", 2]]
	for i in places.size():
		var outfit: String = profile.equipped.get(kinds[i], "")
		var key := outfit.replace("outfit.", "").replace(".", "_outfit_") if not outfit.is_empty() and not outfit.ends_with("default") else ""
		var bird := BirdView.new()
		add_child(bird)
		bird.setup(Board.bird(0, Board.species_index(kinds[i]), 0), false, BIRD_SCALE, key)
		bird.quiet = instant
		bird.position = _seat(places[i][0], places[i][1])
		_birds.append([bird, places[i][0], places[i][1]])
	# Keep the HUD and buttons above the birds.
	for b in _birds:
		move_child(b[0], 2)

func _seat(side: String, i: int) -> Vector2:
	return Vector2(LEFT_SEATS[i], LEFT_PERCH) if side == "L" else Vector2(RIGHT_SEATS[i], RIGHT_PERCH)

func _free_seat() -> Array:
	for side in ["L", "R"]:
		var seats: Array = LEFT_SEATS if side == "L" else RIGHT_SEATS
		for i in seats.size():
			if not _birds.any(func(b): return b[1] == side and b[2] == i):
				return [side, i]
	return []

## Logo drops in with a bounce, birds fly in one after another.
func _entrance() -> void:
	var rest := _logo.position
	_logo.position.y -= 600.0
	var tw := create_tween()
	tw.tween_property(_logo, "position", rest, 0.7).set_delay(0.15).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	for k in _birds.size():
		var bird: BirdView = _birds[k][0]
		var target := bird.position
		bird.position = Vector2(-160.0 if _birds[k][1] == "L" else 1240.0, target.y - 500.0)
		bird.fly_to(target, 0.5 + 0.18 * k, 0.8, false, true)

func _process(delta: float) -> void:
	if instant or _logo == null:
		return
	_clock += delta
	# The logo breathes gently; every few seconds one bird flits to a free seat on the other branch.
	_logo.scale = Vector2.ONE * (1.0 + 0.015 * sin(_clock * 1.6))
	_logo.rotation = 0.012 * sin(_clock * 0.9)
	if _clock >= _next_flit:
		_next_flit = _clock + FLIT_EVERY
		var free := _free_seat()
		if free.is_empty():
			return
		var pick: Array = _birds[randi() % _birds.size()]
		var bird: BirdView = pick[0]
		if bird.acting:
			return
		pick[1] = free[0]
		pick[2] = free[1]
		bird.fly_to(_seat(free[0], free[1]), 0.0, 0.9)

## Capture states: "home" (a player at level 12) and "home_new" (a first launch).
static func preview(main: Control, state: String) -> void:
	var p := Profile.new()
	p.coins = 1240 if state == "home" else 0
	if state == "home":
		p.highest_unlocked = 12
		for level in range(1, 12):
			p.stars[level] = 3 if level % 3 else 2
		for kind in ["sunny", "sky", "berry", "mint", "cocoa"]:
			p.met[kind] = true
		p.outfits = {"outfit.berry.1": true}
		p.equipped = {"berry": "outfit.berry.1"}
	var view := HomeView.new()
	view.instant = main.instant()
	main.set_screen(view)
	view.setup(p)
