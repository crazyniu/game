class_name HomeView
extends Control
## The game's home page (user's home sketch): a painted forest clearing with the birdhouse, the logo, live birds in
## and on the house, a wide Play that continues at the next level, Daily and Lucky Draw tiles, and Map, Shop, Aviary
## and Chest along the bottom. Button faces are painted pictures with their words painted in.

signal play_requested(level: int)
signal map_requested
signal shop_requested
signal aviary_requested
signal settings_requested
signal help_requested
signal lucky_draw_requested
signal chest_requested
signal challenge_requested

## Layout follows the user's home sketch (2:3 sketch fitted to 9:16 by height, buttons anchored to the bottom).
const LOGO := Rect2(100, 60, 880, 560)    # a big title alone in the sky; the birdhouse stands on the branch below
const SETTINGS := Rect2(904, 44, 124, 124)
## Seats in the painted birdhouse (home_clearing_soft: house centred on the branch): two on the shelf, two on the roof slopes.
const SEATS := [Vector2(492, 934), Vector2(590, 934), Vector2(742, 706), Vector2(338, 710)]
const BIRD_SCALE := 1.3
const BIRD_COUNT := 3
const PLAY := Rect2(200, 1150, 680, 184)
const TILE := Vector2(206, 206)       # icon tiles, all one size
const TOP_ROW_Y := 1372.0
const NAV_Y := 1648.0
const NAV_GAP := 30.0
const FLIT_EVERY := 6.0

var profile: Profile
var instant := false
var _birds: Array = []      # [BirdView, seat index]
var _logo: TextureRect
var _clock := 0.0
var _next_flit := FLIT_EVERY

func setup(p: Profile) -> void:
	profile = p
	for child in get_children():
		child.queue_free()
	_birds.clear()
	size = Vector2(1080, 1920)
	UI.image(self, "home_clearing_soft", Rect2(0, 0, 1080, 1920), false)
	_logo = UI.art(self, "home_logo", LOGO)
	_logo.pivot_offset = _logo.size * Vector2(0.5, 0.9)
	_build_birds()
	var coins := str(profile.coins)
	UI.counter(self, Vector2(48, 56), "coin", coins).name = "Coins"
	UI.art_button(self, "Settings", "home_button_settings", SETTINGS, func(): settings_requested.emit())
	var level := _next_level()
	var play := UI.art_button(self, "Play", "home_play_plate", PLAY, func(): play_requested.emit(level))
	UI.label(play, "action.play", Rect2(PLAY.size.x * 0.2, 14.0, PLAY.size.x * 0.6, PLAY.size.y - 40.0), 92.0, Color.WHITE)
	var left_edge := (1080.0 - TILE.x * 4.0 - NAV_GAP * 3.0) * 0.5
	var top := [["Daily", "daily", challenge_requested], ["LuckyDraw", "lucky_draw", lucky_draw_requested]]
	for i in top.size():
		var r := Rect2(left_edge + (1 + i) * (TILE.x + NAV_GAP), TOP_ROW_Y, TILE.x, TILE.y)   # columns 2 and 3 of the bottom row
		var tb := UI.icon_tile(self, top[i][0], "home_icon_" + top[i][1], r, (top[i][2] as Signal).emit)
		if top[i][0] == "Daily" and Progress.challenge_available(profile, Progress.today()):
			_badge(tb, "1")
		elif top[i][0] == "LuckyDraw" and Progress.draws_available(profile, Progress.today()):
			_badge(tb, "1")
	var places := [["Map", "map", map_requested], ["Shop", "shop", shop_requested],
		["Aviary", "aviary", aviary_requested], ["Chest", "chest", chest_requested]]
	for i in places.size():
		var nb := UI.icon_tile(self, places[i][0], "home_icon_" + places[i][1],
			Rect2(left_edge + i * (TILE.x + NAV_GAP), NAV_Y, TILE.x, TILE.y), (places[i][2] as Signal).emit)
		if places[i][0] == "Chest" and Progress.chests_ready(profile) > 0:
			_badge(nb, str(Progress.chests_ready(profile)))
	if not instant:
		_entrance()

## The next level to play: the first unlocked level without stars, else the highest unlocked one.
func _next_level() -> int:
	for level in range(1, profile.highest_unlocked + 1):
		if profile.best_stars(level) == 0:
			return level
	return profile.highest_unlocked

## Met birds (the first three met, else the starters) in and on the birdhouse, alive.
func _build_birds() -> void:
	var kinds: Array = []
	for card in Aviary.SPECIES:
		if card[0] != "crow" and profile.met.has(card[0]):
			kinds.append(card[0])
	for fallback in ["sunny", "sky", "berry"]:
		if kinds.size() >= BIRD_COUNT:
			break
		if not kinds.has(fallback):
			kinds.append(fallback)
	for i in BIRD_COUNT:
		var outfit: String = profile.equipped.get(kinds[i], "")
		var key := outfit.replace("outfit.", "").replace(".", "_outfit_") if not outfit.is_empty() and not outfit.ends_with("default") else ""
		var bird := BirdView.new()
		add_child(bird)
		bird.setup(Board.bird(0, Board.species_index(kinds[i]), 0), false, BIRD_SCALE, key)
		bird.quiet = instant
		bird.position = SEATS[i]
		_birds.append([bird, i])
	# Keep the HUD and buttons above the birds.
	for b in _birds:
		move_child(b[0], 2)

func _free_seat() -> int:
	for i in SEATS.size():
		if not _birds.any(func(b): return b[1] == i):
			return i
	return -1

## Red count badge on a tile's top-right corner.
func _badge(b: Control, text: String) -> void:
	var d := 64.0
	var p := Control.new()
	p.name = "Badge"
	p.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(p, Rect2(Vector2(b.size.x - d * 0.75, -d * 0.3), Vector2.ONE * d))
	b.add_child(p)
	UI.kit_piece(p, "badge", Rect2(Vector2.ONE * -4.0, Vector2.ONE * (d + 8.0)))
	UI.number(p, text, Rect2(10, d * 0.5 - 20.0, d - 20.0, 34), Color.WHITE)

## Logo drops in with a bounce, birds fly in one after another.
func _entrance() -> void:
	var rest := _logo.position
	_logo.position.y -= 600.0
	var tw := create_tween()
	tw.tween_property(_logo, "position", rest, 0.7).set_delay(0.15).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	for k in _birds.size():
		var bird: BirdView = _birds[k][0]
		var target := bird.position
		bird.position = Vector2(-160.0 if k % 2 == 0 else 1240.0, target.y - 500.0)
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
		if free < 0:
			return
		var pick: Array = _birds[randi() % _birds.size()]
		var bird: BirdView = pick[0]
		if bird.acting:
			return
		pick[1] = free
		bird.fly_to(SEATS[free], 0.0, 0.9)

## Capture states: "home" (a player at level 12) and "home_new" (a first launch).
static func preview(main: Control, state: String) -> void:
	var p := Profile.new()
	p.coins = 50270 if state == "home" else 0
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
