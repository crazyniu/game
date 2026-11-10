class_name HomeView
extends Control
## The approved island homepage; existing navigation and progression remain owned by main/meta.

signal play_requested(level: int)
signal map_requested
signal shop_requested
signal aviary_requested
signal settings_requested
signal help_requested
signal lucky_draw_requested
signal chest_requested
signal challenge_requested

const REFERENCE_SIZE := Vector2(940, 1674)
const CANVAS_SIZE := Vector2(1080, 1920)
const REFERENCE_SCALE := CANVAS_SIZE / REFERENCE_SIZE
const NAV_SIZE := Vector2(208, 191)
const EVENT_SIZE := Vector2(142, 135)
const IDLE_FPS := 8.0

var profile: Profile
var instant := false
var _birds: Array[Dictionary] = []
var _clock := 0.0
var _last_idle := {"house": -1.0, "logo": -1.0}

static func _rect(x: float, y: float, w: float, h: float) -> Rect2:
	return Rect2(Vector2(x, y) * REFERENCE_SCALE, Vector2(w, h) * REFERENCE_SCALE)

func setup(p: Profile) -> void:
	profile = p
	for child in get_children():
		remove_child(child)
		child.queue_free()
	_birds.clear()
	_clock = 0.0
	_last_idle = {"house": -1.0, "logo": -1.0}
	size = CANVAS_SIZE
	UI.image(self, "panel_home_island_background", Rect2(Vector2.ZERO, CANVAS_SIZE), false)
	_add_bird("macaw", Vector2(206, 849), 574, 4, "house")
	_add_bird("yellow_chick", Vector2(419, 834), 650, 0, "house")
	_add_bird("blue_chick", Vector2(529, 835), 649, 2, "house")
	_add_bird("green_parrot", Vector2(671, 898), 675, 4, "house")
	# Only the branch foreground hides the parrot's tail; its feet stay above the perch.
	UI.image(self, "house_home_island_foreground", Rect2(Vector2.ZERO, CANVAS_SIZE), false)
	UI.image(self, "panel_home_island_logo", _rect(123, 209, 691, 171), false)
	_add_bird("logo_yellow", Vector2(265, 243), 134, 0, "logo")
	_add_bird("logo_blue", Vector2(653, 251), 146, 0, "logo")

	UI.image(self, "panel_home_island_coin", _rect(22, 31, 191, 93), false)
	UI.number(self, str(profile.coins), _rect(107, 55, 91, 43), UI.INK).name = "Coins"
	_button("Settings", "button_settings", _rect(796, 25, 119, 111), func(): settings_requested.emit())
	var level := _next_level()
	var play := _button("Play", "button_play_blank", _rect(153, 1096, 626, 242), func(): play_requested.emit(level))
	# Localized painted words retain the existing language preference; no fixed English text is baked in.
	UI.label(play, "action.play", _rect(231, 52, 321, 127), 114.0, Color.WHITE)

	var events := [["Daily", "button_daily", challenge_requested], ["LuckyDraw", "button_draw", lucky_draw_requested]]
	for i in events.size():
		var entry: Array = events[i]
		var button := _button(entry[0], entry[1], _rect(765, 534 + 161 * i, EVENT_SIZE.x, EVENT_SIZE.y), (entry[2] as Signal).emit)
		if entry[0] == "Daily" and Progress.challenge_available(profile, Progress.today()):
			_badge(button, "1")
		elif entry[0] == "LuckyDraw" and Progress.draws_available(profile, Progress.today()):
			_badge(button, "1")
	var places := [["Map", "button_map", map_requested], ["Shop", "button_shop", shop_requested],
		["Aviary", "button_aviary", aviary_requested], ["Chest", "button_chest", chest_requested]]
	for i in places.size():
		var entry: Array = places[i]
		var button := _button(entry[0], entry[1], _rect(33 + 223 * i, 1413, NAV_SIZE.x, NAV_SIZE.y), (entry[2] as Signal).emit)
		if entry[0] == "Chest" and Progress.chests_ready(profile) > 0:
			_badge(button, str(Progress.chests_ready(profile)))

func _button(n: String, picture: String, bounds: Rect2, action: Callable) -> Button:
	var button := UI.art_button(self, n, "btn_home_island_" + picture.trim_prefix("button_"), bounds, action)
	# Export footprints match the approved composition; all siblings use identical bounds.
	var face := button.get_node("Face") as TextureRect
	UI.place(face, Rect2(Vector2.ZERO, bounds.size))
	return button

func _badge(button: Control, value: String) -> void:
	var d := 54.0 * REFERENCE_SCALE.y
	var origin := Vector2(button.size.x - d * 0.84, -d * 0.33)
	var badge := UI.image(button, "badge_home_island_notification", Rect2(origin, Vector2.ONE * d), false)
	badge.name = "Badge"
	UI.number(button, value, Rect2(origin + Vector2(9, 8), Vector2(d - 18, d - 18)), Color.WHITE)

## The next level to play: first unlocked level without stars, else the highest unlocked one.
func _next_level() -> int:
	for level in range(1, profile.highest_unlocked + 1):
		if profile.best_stars(level) == 0:
			return level
	return profile.highest_unlocked

func _add_bird(kind: String, at: Vector2, top: float, rest_frame: int, group: String) -> void:
	var textures := Art.frames("home_island_%s_idle" % kind)
	assert(textures.size() == 8, "Island homepage bird needs its complete drawn idle sequence: " + kind)
	var feet := Vector2(256, 330 if kind in ["macaw", "green_parrot"] else 470)
	var used := textures[rest_frame].get_image().get_used_rect()
	var pixel_scale := (at.y - top) / (feet.y - used.position.y)
	var pivot := Node2D.new()
	pivot.name = "Bird_" + kind
	pivot.position = at * REFERENCE_SCALE
	add_child(pivot)
	var bird := AnimatedSprite2D.new()
	bird.name = "Pose"
	bird.centered = false
	bird.position = -feet * pixel_scale * REFERENCE_SCALE
	bird.scale = Vector2.ONE * pixel_scale * REFERENCE_SCALE
	var frames := SpriteFrames.new()
	frames.set_animation_loop("default", false)
	frames.set_animation_speed("default", IDLE_FPS)
	for texture in textures:
		frames.add_frame("default", texture)
	bird.sprite_frames = frames
	bird.frame = rest_frame
	pivot.add_child(bird)
	bird.animation_finished.connect(func(): bird.frame = rest_frame)
	_birds.append({"pivot": pivot, "view": bird, "group": group, "phase": _birds.size() * 0.91,
		"next": 0.45 + (_birds.size() % 4) * 0.65})

func _process(delta: float) -> void:
	if instant:
		return
	_clock += delta
	for entry in _birds:
		var pivot: Node2D = entry["pivot"]
		var bird: AnimatedSprite2D = entry["view"]
		var breath := (sin(_clock * TAU / 2.8 + entry["phase"]) + 1.0) * 0.5
		pivot.scale = Vector2(1.0 - 0.008 * breath, 1.0 + 0.02 * breath)
		var group: String = entry["group"]
		if not bird.is_playing() and _clock >= entry["next"] and _clock - _last_idle[group] >= 0.62:
			bird.frame = 0
			bird.frame_progress = 0.0
			bird.play("default")
			_last_idle[group] = _clock
			entry["next"] = _clock + randf_range(1.8, 2.8)

## Fake-data review states use the same production view and signals.
static func preview(main: Control, state: String) -> void:
	var p := Profile.new()
	p.coins = 50270 if state != "home_new" else 0
	if state != "home_new":
		p.highest_unlocked = 12
		for level in range(1, 12):
			p.stars[level] = 3 if level % 3 else 2
		for kind in ["sunny", "sky", "berry", "mint", "cocoa"]:
			p.met[kind] = true
		p.outfits = {"outfit.berry.1": true}
		p.equipped = {"berry": "outfit.berry.1"}
	if state == "home_max":
		p.coins = 99999
	var view := HomeView.new()
	view.instant = main.instant()
	main.set_screen(view)
	view.setup(p)
