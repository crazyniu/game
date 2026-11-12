class_name HomeView
extends Control
## Forest homepage with one birdhouse-title focal group; navigation remains owned by main/meta.

signal play_requested(level: int)
signal map_requested
signal shop_requested
signal aviary_requested
signal settings_requested
signal help_requested
signal lucky_draw_requested
signal chest_requested
signal challenge_requested

const CANVAS_SIZE := Vector2(1080, 1920)
const LOGO := Rect2(128, 208, 832, 832)
const LOGO_CONTENT := Rect2(64, 50, 1120, 1121)
const ICON_ENTRY_SIZE := Vector2(160, 160)
const NAV_ICON_SIDE := 132.0
const EVENT_ICON_SIDE := 120.0
const SETTINGS_ICON_SIDE := 96.0
const NAV_BACKING_SIDE := 204.0
const EVENT_BACKING_SIDE := 184.0
const BACKING_OPACITY := 0.88
# Visible painted bodies, excluding transparent padding and faint cutout residue.
const ICON_CONTENT := {
	"pic_gear": Rect2(21, 28, 203, 202),
	"pic_daily": Rect2(43, 27, 161, 212),
	"icon_home_draw": Rect2(175, 75, 958, 972),
	"pic_map": Rect2(18, 36, 226, 177),
	"pic_shop": Rect2(18, 17, 196, 221),
	"pic_aviary": Rect2(21, 49, 200, 164),
	"pic_chest": Rect2(36, 50, 185, 170),
}
const ICON_OUTLINE := preload("res://game/home_icon_outline.gdshader")
const PLAY := Rect2(290, 1360, 500, 200)
const COIN_DISPLAY_MAX := FeatherBar.DISPLAY_MAX
const IDLE_FPS := 8.0
# Contact rows of the existing drawn claws, not the old padding-based registration row.
const BIRD_FOOT_Y := {
	"macaw": [320, 324, 323, 328, 324, 323, 323, 322],
	"green_parrot": [326, 328, 325, 331, 327, 326, 328, 327],
}

var profile: Profile
var instant := false
var _birds: Array[Dictionary] = []
var _clock := 0.0
var _last_idle := {"perch": -1.0}
var _play_face: TextureRect

func setup(p: Profile) -> void:
	profile = p
	for child in get_children():
		remove_child(child)
		child.queue_free()
	_birds.clear()
	_clock = 0.0
	_last_idle = {"perch": -1.0}
	size = CANVAS_SIZE
	UI.image(self, "background_home_jungle", Rect2(Vector2.ZERO, CANVAS_SIZE), false)
	var title := UI.art(self, "panel_home_perch_title", LOGO, LOGO_CONTENT)
	title.name = "Title"
	# The open lower branch holds the original live birds; no characters are baked into the title.
	var title_scale := title.size / title.texture.get_size()
	var seats := [
		["macaw", Vector2(325, 1084), 228.0, 4],
		["yellow_chick", Vector2(510, 1096), 176.0, 0],
		["blue_chick", Vector2(715, 1084), 178.0, 2],
		["green_parrot", Vector2(905, 1082), 184.0, 4],
	]
	for seat in seats:
		var contact: Vector2 = title.position + seat[1] * title_scale
		_add_bird(seat[0], contact, contact.y - seat[2], seat[3], "perch")
	# The same intact transparent branch covers tails beneath its top contact edge.
	move_child(title, get_child_count() - 1)

	FeatherBar.create(self, Vector2(48, 48), profile.coins)
	_icon_button("Settings", "pic_gear", Vector2(976, 104), func(): settings_requested.emit(), SETTINGS_ICON_SIDE)
	var level := _next_level()
	var play := _button("Play", "play_" + UiText.locale(), PLAY, func(): play_requested.emit(level))
	_play_face = play.get_node("Face") as TextureRect
	# Both locale textures share a canvas; swapping them preserves the complete painted composition.
	UI.place(_play_face, Rect2(Vector2.ZERO, PLAY.size))
	_play_face.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED

	var events := [["Daily", "pic_daily", challenge_requested], ["LuckyDraw", "icon_home_draw", lucky_draw_requested]]
	for i in events.size():
		var entry: Array = events[i]
		var button := _icon_button(entry[0], entry[1], Vector2(960, 920 + 220 * i), (entry[2] as Signal).emit, EVENT_ICON_SIDE, EVENT_BACKING_SIDE)
		if entry[0] == "Daily" and Progress.challenge_available(profile, Progress.today()):
			_badge(button, 1)
		elif entry[0] == "LuckyDraw" and Progress.draws_available(profile, Progress.today()):
			_badge(button, 1)

	var places := [["Map", "pic_map", map_requested], ["Shop", "pic_shop", shop_requested],
		["Aviary", "pic_aviary", aviary_requested], ["Chest", "pic_chest", chest_requested]]
	for i in places.size():
		var entry: Array = places[i]
		var button := _icon_button(entry[0], entry[1], Vector2(174 + 244 * i, 1760), (entry[2] as Signal).emit, NAV_ICON_SIDE, NAV_BACKING_SIDE)
		if entry[0] == "Chest" and Progress.chests_ready(profile) > 0:
			_badge(button, Progress.chests_ready(profile))

func _notification(what: int) -> void:
	if what == NOTIFICATION_TRANSLATION_CHANGED and is_instance_valid(_play_face):
		_play_face.texture = Art.tex("btn_home_island_play_" + UiText.locale())

func _button(n: String, picture: String, bounds: Rect2, action: Callable) -> Button:
	return UI.art_button(self, n, "btn_home_island_" + picture, bounds, action)

func _icon_button(n: String, picture: String, center: Vector2, action: Callable, side: float, backing_side := 0.0) -> Button:
	var button := Button.new()
	button.name = n
	button.focus_mode = Control.FOCUS_NONE
	var empty := StyleBoxEmpty.new()
	for state in ["normal", "hover", "pressed", "disabled", "focus", "hover_pressed"]:
		button.add_theme_stylebox_override(state, empty)
	UI.place(button, Rect2(center - ICON_ENTRY_SIZE * 0.5, ICON_ENTRY_SIZE))
	if backing_side > 0.0:
		# Keep the independent frame's transparent padding; the accepted icon and hit area stay intact.
		var backing_size := Vector2.ONE * backing_side
		var backing := UI.image(button, "btn_home_icon_frame", Rect2((ICON_ENTRY_SIZE - backing_size) * 0.5, backing_size))
		backing.name = "Backing"
		backing.modulate.a = BACKING_OPACITY
		button.button_down.connect(func(): backing.self_modulate = UI.PRESSED_TINT)
		button.button_up.connect(func(): backing.self_modulate = Color.WHITE)
	# Fit the painted body uniformly; a common slot must never stretch the source art.
	var content: Rect2 = ICON_CONTENT[picture]
	var pixel_scale := side / maxf(content.size.x, content.size.y)
	var face_size := Art.tex(picture).get_size() * pixel_scale
	var face_at := ICON_ENTRY_SIZE * 0.5 - content.get_center() * pixel_scale
	var face := UI.image(button, picture, Rect2(face_at, face_size), false)
	face.name = "Face"
	var outline := ShaderMaterial.new()
	outline.shader = ICON_OUTLINE
	outline.set_shader_parameter("draw_size", face.size)
	face.material = outline
	button.button_down.connect(func(): face.self_modulate = UI.PRESSED_TINT)
	button.button_up.connect(func(): face.self_modulate = Color.WHITE)
	button.pressed.connect(func():
		Sound.play("ui_tap")
		action.call())
	add_child(button)
	return button

func _badge(button: Control, value: int) -> void:
	var badge := Island.badge(button, Vector2(button.size.x - 16, 20), value, true)
	badge.pivot_offset = badge.size * 0.5
	badge.scale = Vector2.ONE * (48.0 / Island.BADGE_SIZE)

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
	pivot.position = at
	add_child(pivot)
	var bird := AnimatedSprite2D.new()
	bird.name = "Pose"
	bird.centered = false
	bird.position = -feet * pixel_scale
	bird.scale = Vector2.ONE * pixel_scale
	var frames := SpriteFrames.new()
	frames.set_animation_loop("default", false)
	frames.set_animation_speed("default", IDLE_FPS)
	for texture in textures:
		frames.add_frame("default", texture)
	bird.sprite_frames = frames
	bird.frame = rest_frame
	var contact_rows: Array = BIRD_FOOT_Y.get(kind, [])
	if not contact_rows.is_empty():
		var align_feet := func(): bird.position.y = -float(contact_rows[bird.frame]) * pixel_scale
		bird.frame_changed.connect(align_feet)
		align_feet.call()
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
	Sound.music("bgm_menu")
