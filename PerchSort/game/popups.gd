extends RefCounted
## Modal popups opened on a host Control (main.popups). Every function returns the Popup; callbacks are Callables.
## Elements sit at the measured rects of res://layout/<screen>.json; words are painted sprites, numbers painted glyphs.

const EFFECT := {"sunny": "wake", "sky": "reveal", "berry": "encore", "mint": "grow", "cocoa": "scare", "snow": "lightstep",
	"ember": "hatch", "ink": "nightcall", "hoot": "moonbeam"}
const GOAL_CAPTION := {"sunny": "wakes_sleepers", "sky": "reveals_eggs", "berry": "one_free_move", "mint": "grows_branches",
	"cocoa": "sends_crows_away", "snow": "land_anywhere", "ember": "hatches_every_egg", "ink": "switches_day_night",
	"hoot": "moonlit_moves"}
const MECHANIC_ART := {"sleep": "hoot_sleep", "egg": "egg_idle", "crow": "crow_front", "daynight": "mechanic_day_night",
	"twig": "icon_branch_plus", "growing": "icon_branch_plus", "swivel": "swivel_knot"}
const BOOSTER_ID := ["firefly", "sapling", "owl_eye", "morning_bell", "scarecrow", "spring_rain"]
const BOOSTER_ILLUS := ["hint", "sapling", "owl_eye", "bell", "scarecrow", "rain"]
const BOOSTER_CAPTION := ["find_a_helpful_move", "add_one_branch", "reveals_eggs", "wake_sleeping_birds", "send_crows_away",
	"grow_branches_fully"]
const WORLD_FOREST := {2: "bamboo", 3: "autumn", 4: "snow"}
## The shared modal frame (popup.gd); referenced through one alias so the class name may change.
const FRAME := preload("res://game/popup.gd")
const CREAM := Color("f7e6b8")
## Disc sizes: same-role buttons share one size; the primary action is at most 1.25x its siblings.
const DISC := 92.0
const PRIMARY := 112.0
const SMALL_PRIMARY := 124.0


# ---------------------------------------------------------------- popups

static func win(host: Control, stars: int, moves: int, par: int, reward: int, on_replay: Callable, on_next: Callable,
		on_home: Callable, next_enabled: bool) -> Control:
	var s := "board_win_panel"
	var p = _open(host, Layout.r(s, "panel_frame"))
	var c: Control = p.content
	_bird_view(c, "sunny", Rect2(412, 616, 246, 309), Rect2(300, 560, 480, 255), _instant(host), 0.6)
	_title(c, "label_title_well_done", Layout.r(s, "win_ribbon"))
	_stars(c, [Layout.r(s, "win_star_1"), Layout.r(s, "win_star_2"), Layout.r(s, "win_star_3")], stars, not _instant(host), 0.5)
	var limits := [str(par), str(par + 3), "∞"]
	for i in 3:
		_pill(c, Layout.r(s, "threshold_pill_%d" % (i + 1)))
		UI.number(c, limits[i], Layout.r(s, "threshold_value_%d" % (i + 1)))
	_pill(c, Layout.r(s, "move_pill"))
	UI.art(c, "footprints", Layout.r(s, "move_footprint"))
	UI.number(c, str(moves), Layout.r(s, "move_value"))
	if reward > 0:
		_reward(c, Layout.r(s, "reward_pill"), Layout.r(s, "reward_coin"), Layout.r(s, "reward_value"), reward)
	_action(p, Layout.r(s, "action_retry"), DISC, "icon_replay", on_replay)
	UI.set_enabled(_action(p, Layout.r(s, "action_play"), PRIMARY, "icon_play", on_next, "btn_green"), next_enabled)
	_action(p, Layout.r(s, "action_home"), DISC, "icon_home", on_home)
	return p

static func settings(host: Control, muted: bool, on_toggle_sound: Callable, on_restart: Callable, on_home: Callable,
		show_restart: bool) -> Control:
	var s := "settings"
	var p = _open(host, Layout.r(s, "panel"), Layout.r(s, "close"))
	var c: Control = p.content
	_title(c, "label_title_settings", Layout.r(s, "leaves_left").merge(Layout.r(s, "ribbon")).merge(Layout.r(s, "leaves_right")))
	var spread := Layout.r(s, "restart").position.x - Layout.r(s, "sound").position.x
	var shift := {"sound": 0.5, "restart": 0.0, "home": -0.5}
	var state := {"muted": muted}
	for id in ["sound", "restart", "home"]:
		if id == "restart" and not show_restart:
			continue
		var d := Layout.r(s, id)
		var plate := Layout.r(s, id + "_plate")
		if not show_restart:
			d.position.x += shift[id] * spread
			plate.position.x += shift[id] * spread
		_fit(c, "label_caption_" + id, plate)
		var cb: Callable = {"sound": on_toggle_sound, "restart": on_restart, "home": on_home}[id]
		var icon: String = {"sound": _sound_icon(muted), "restart": "icon_replay", "home": "icon_home"}[id]
		var b := _action(p, d, DISC + 8.0, icon, func():
			cb.call()
			if id == "sound":
				state["muted"] = not state["muted"]
				state["button"].get_node("Icon").texture = Art.tex(_sound_icon(state["muted"])), "btn_wood", id != "sound")
		state["button"] = b if id == "sound" else state.get("button")
	return p

static func level_card(host: Control, level: int, best_stars: int, best_moves: int, par: int, species: Array[String],
		mechanics: Array[String], on_play: Callable, playable: bool) -> Control:
	var s := "level_card"
	var p = _open(host, Layout.r(s, "panel_frame"), Layout.r(s, "close"))
	var c: Control = p.content
	_title(c, "label_title_level_%d" % level, Layout.r(s, "level_ribbon"))
	_stars(c, [Layout.r(s, "star_1"), Layout.r(s, "star_2"), Layout.r(s, "star_3")], best_stars, false)
	var pills := ["threshold_12", "threshold_16", "threshold_inf"]
	var limits := [str(par), str(par + 3), "∞"]
	for i in 3:
		_pill(c, Layout.r(s, pills[i]))
		UI.number(c, limits[i], Layout.r(s, pills[i] + "_text"))
	if best_moves > 0:
		var text := Layout.r(s, "best_text")
		_pill(c, Layout.r(s, "best_pill"))
		UI.art(c, "footprints", Layout.r(s, "best_tracks"))
		var word := _fit(c, "label_word_best", Rect2(text.position, Vector2(120, text.size.y)))
		UI.number(c, str(best_moves), Rect2(text.position + Vector2(140, 0), Vector2(text.size.x - 140, text.size.y)), "brown", -1)
		word.name = "Best"
	for i in species.size():
		var in_row := mini(4, species.size() - (i / 4) * 4)
		var center := Vector2(540.0 + (i % 4 - (in_row - 1) * 0.5) * 187.0, 870.0 + (i / 4) * 196.0)
		_ring(c, species[i], center, 178.0, 66.0)
		var effect: String = EFFECT.get(species[i], "")
		if not effect.is_empty():
			UI.art(c, "gift_coin_" + effect, Rect2(center + Vector2(5, 15), Vector2(79, 78)))
	if not mechanics.is_empty():
		_mechanic_strip(c, mechanics, Layout.r(s, "lower_card"))
	UI.set_enabled(p.action(_below(Layout.r(s, "play"), SMALL_PRIMARY), "icon_play", on_play, "btn_green"), playable)
	return p

static func goal_info(host: Control, species: String) -> Control:
	var s := "goal_info"
	var p = _open(host, Layout.r(s, "modal_frame"), Layout.r(s, "close"))
	var c: Control = p.content
	var ring := Layout.r(s, "sunny_ring")
	_fill(c, Layout.r(s, "portrait_well"), CREAM)
	UI.art(c, "badge_ring_gold", ring)
	_bird_view(c, species, Layout.r(s, "sunny_hero"), Rect2(), _instant(host), 0.0)
	_title(c, "label_name_" + species, Layout.r(s, "leaves_left").merge(Layout.r(s, "ribbon")).merge(Layout.r(s, "leaves_right")))
	var plate := Layout.r(s, "ability_plate")
	_fit(c, "label_caption_" + GOAL_CAPTION.get(species, "wakes_sleepers"), Rect2(plate.position + Vector2(50, 0), plate.size - Vector2(50, 0)))
	if EFFECT.has(species):
		UI.art(c, "gift_coin_" + EFFECT[species], Layout.r(s, "ability_disc"))
	return p

static func daily_gift(host: Control, reward: int, on_claim: Callable) -> Control:
	var s := "daily_gift"
	var p = _open(host, Layout.r(s, "modal_panel"))
	var c: Control = p.content
	_title(c, "label_title_daily_gift", Layout.r(s, "ribbon"))
	# The nest, box and lid share one 768 px canvas; the stage maps it onto the hero rect.
	var stage := Control.new()
	stage.name = "Gift"
	stage.mouse_filter = Control.MOUSE_FILTER_IGNORE
	stage.size = Vector2(768, 768)
	stage.scale = Vector2.ONE * 0.93
	stage.position = Vector2(204, 545)
	c.add_child(stage)
	_canvas(stage, "gift_nest")
	for r in ["gift_coin_back_left", "gift_coin_back_right", "gift_coin_left", "gift_coin_right", "gift_coin_middle"]:
		UI.art(c, "coin", Layout.r(s, r))
	_canvas(stage, "gift_box_base")
	var lid := _canvas(stage, "gift_box_lid")
	_canvas(stage, "gift_nest_front")
	var bob := lid.create_tween().set_loops()
	bob.tween_property(lid, "position:y", -6.0, 1.1).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	bob.tween_property(lid, "position:y", 0.0, 1.1).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	for i in 4:
		_twinkle(UI.art(c, "sparkle", Layout.r(s, "spark_%d" % (i + 1))), i * 0.4)
	_reward(c, Layout.r(s, "reward_pill"), Layout.r(s, "reward_coin"), Layout.r(s, "reward_50"), reward)
	_action(p, Layout.r(s, "accept"), SMALL_PRIMARY, "icon_check", on_claim, "btn_green")
	return p

static func new_friend(host: Control, species: String, reward: int, on_ok: Callable) -> Control:
	var s := "new_friend"
	var p = _open(host, Layout.r(s, "panel"))
	var c: Control = p.content
	_title(c, "label_title_new_friend", Layout.r(s, "title_leaf_l").merge(Layout.r(s, "title_rib")).merge(Layout.r(s, "title_leaf_r")))
	_bird_view(c, species, Layout.r(s, "minty"), Rect2(), _instant(host), 0.0)
	_title(c, "label_name_" + species, Layout.r(s, "name_rib"))
	_reward(c, Layout.r(s, "reward"), Layout.r(s, "reward_coin"), Layout.r(s, "reward_num"), reward)
	_action(p, Layout.r(s, "accept"), SMALL_PRIMARY, "icon_check", on_ok, "btn_green")
	return p

static func unlock(host: Control, booster: int, on_ok: Callable) -> Control:
	var s := "unlock_card"
	var p = _open(host, Layout.r(s, "panel"))
	var c: Control = p.content
	_title(c, "label_title_booster_" + BOOSTER_ID[booster], Layout.r(s, "leaf_l").merge(Layout.r(s, "ribbon")).merge(Layout.r(s, "leaf_r")))
	for i in 4:
		var r := Layout.r(s, ["spark_l1", "spark_r1", "spark_l2", "spark_r2"][i])
		_twinkle(UI.art(c, "sparkle", r), i * 0.35)
	var illus := _fit(c, "booster_%s_illus" % BOOSTER_ILLUS[booster], Layout.r(s, "halo").grow_individual(-60, -20, -60, -20), true)
	_breathe(illus, 0.0)
	_fit(c, "label_caption_" + BOOSTER_CAPTION[booster], Layout.r(s, "caption_plate"))
	_action(p, Layout.r(s, "accept"), SMALL_PRIMARY, "icon_check", on_ok, "btn_green")
	return p

static func world_complete(host: Control, world: int, reward: int, on_ok: Callable) -> Control:
	var s := "world_complete"
	var p = _open(host, Layout.r(s, "panel"))
	var c: Control = p.content
	_title(c, "label_title_world_complete", Layout.r(s, "ribbon"))
	_stars(c, [Layout.r(s, "star1"), Layout.r(s, "star2"), Layout.r(s, "star3")], 3, not _instant(host), 0.5)
	_fit(c, "gate_arch", Rect2(310, 685, 460, 420))
	var birds := ["sunny", "sky", "berry"]
	for i in 3:
		_live(c, birds[i], Layout.r(s, ["yellow", "blue", "pink"][i]), i * 0.3)
	_reward(c, Layout.r(s, "reward"), Layout.r(s, "reward_coin"), Layout.r(s, "reward_value"), reward)
	_action(p, Layout.r(s, "accept"), SMALL_PRIMARY, "icon_check", on_ok, "btn_green")
	return p

static func world_unlock(host: Control, world: int, on_play: Callable) -> Control:
	var s := "world_unlock"
	var p = _open(host, Layout.r(s, "modal"))
	var c: Control = p.content
	_title(c, "label_title_world_unlocked", Layout.r(s, "leaves_left").merge(Layout.r(s, "ribbon")).merge(Layout.r(s, "leaves_right")))
	# Arch, backdrop and both doors share one 1024 px canvas; the doors swing open outwards.
	var stage := Control.new()
	stage.name = "Gate"
	stage.mouse_filter = Control.MOUSE_FILTER_IGNORE
	stage.size = Vector2(1024, 1024)
	stage.scale = Vector2.ONE * 0.7
	stage.position = Vector2(183, 496)
	c.add_child(stage)
	_canvas(stage, "gate_backdrop_w%d" % world)
	var light := _canvas(stage, "gate_light")
	_canvas(stage, "gate_arch")
	var left := _canvas(stage, "gate_door_left")
	var right := _canvas(stage, "gate_door_right")
	left.pivot_offset.x = 234.0
	right.pivot_offset.x = 790.0
	var glow := light.create_tween().set_loops()
	glow.tween_property(light, "modulate:a", 0.35, 1.4).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	glow.tween_property(light, "modulate:a", 0.9, 1.4).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	var open := func(t: float):
		left.scale.x = lerpf(1.0, 0.85, t)
		left.position.x = lerpf(0.0, -247.0, t)
		right.scale.x = lerpf(1.0, 0.85, t)
		right.position.x = lerpf(0.0, 234.0, t)
	if _instant(host):
		open.call(1.0)
	else:
		open.call(0.0)
		var tw := p.create_tween()
		tw.tween_interval(0.5)
		tw.tween_method(open, 0.0, 1.0, 1.1).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
	_fit(c, "label_caption_%s_forest" % WORLD_FOREST.get(world, "bamboo"), Layout.r(s, "caption_plate"))
	_action(p, Layout.r(s, "play"), SMALL_PRIMARY, "icon_play", on_play, "btn_green")
	return p


# ---------------------------------------------------------------- helpers

static func _sound_icon(muted: bool) -> String:
	return "icon_sound_off" if muted else "icon_sound_on"

static func _instant(host: Node) -> bool:
	var main := host.get_parent()
	return main != null and main.has_method("instant") and main.instant()

## The measured rect is the visible frame; panel_wood has transparent margins (left, top, right, bottom) around it.
const PANEL_PAD := Vector4(4, 36, 4, 21)

static func _open(host: Control, panel: Rect2, close := Rect2()) -> Control:
	var full := Rect2(panel.position - Vector2(PANEL_PAD.x, PANEL_PAD.y), panel.size + Vector2(PANEL_PAD.x + PANEL_PAD.z, PANEL_PAD.y + PANEL_PAD.w))
	return FRAME.open(host, full, "", Rect2(), close, _instant(host))

## Disc centered on the rect of a measured button, drawn at a uniform role size.
static func _action(p: Control, measured: Rect2, size: float, icon: String, on_press: Callable, base := "btn_wood", closes := true) -> Button:
	var r := Rect2(measured.get_center() - Vector2(size, size) * 0.5, Vector2(size, size))
	return p.action(r, icon, on_press, base, closes)

static func _below(measured: Rect2, size: float) -> Rect2:
	return Rect2(measured.get_center() - Vector2(size, size) * 0.5, Vector2(size, size))

## Visible pixels contained in the box, uniform scale, centered (or bottom-aligned with a feet pivot).
static func _fit(parent: Node, name: String, box: Rect2, bottom := false) -> TextureRect:
	var used := UI.used_rect(name)
	var k := minf(box.size.x / used.size.x, box.size.y / used.size.y)
	var size := used.size * k
	var at := box.position + Vector2((box.size.x - size.x) * 0.5, (box.size.y - size.y) if bottom else (box.size.y - size.y) * 0.5)
	var t := UI.art(parent, name, Rect2(at, size))
	if bottom:
		t.pivot_offset = at + Vector2(size.x * 0.5, size.y) - t.position
	return t

## Title ribbons are flatter than the mockup ribbons, so they are drawn a little larger than the measured width.
static func _title(c: Control, label: String, bounds: Rect2) -> void:
	_fit(c, label, Rect2(bounds.get_center() - bounds.size * 0.575, bounds.size * 1.15))

static func _canvas(stage: Control, name: String) -> TextureRect:
	var t := TextureRect.new()
	t.name = name
	t.texture = Art.tex(name)
	t.size = Vector2(768, 768) if stage.size.x < 1000.0 else Vector2(1024, 1024)
	t.mouse_filter = Control.MOUSE_FILTER_IGNORE
	stage.add_child(t)
	return t

static func _fill(parent: Node, r: Rect2, color: Color) -> Panel:
	var panel := Panel.new()
	var box := StyleBoxFlat.new()
	box.bg_color = color
	box.set_corner_radius_all(int(maxf(r.size.x, r.size.y)))
	panel.add_theme_stylebox_override("panel", box)
	panel.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(panel, r)
	parent.add_child(panel)
	return panel

## Cream pill; the nine-slice borders shrink with the pill height.
static func _pill(c: Control, r: Rect2) -> NinePatchRect:
	return UI.panel(c, "stat_pill", r, minf(1.0, r.size.y / 96.0))

static func _reward(c: Control, pill: Rect2, coin: Rect2, value: Rect2, amount: int) -> void:
	_pill(c, pill)
	UI.art(c, "coin", coin)
	UI.number(c, "+%d" % amount, value)

## Empty stars first, then the earned ones on top; earned stars pop in one after another with a sound.
static func _stars(c: Control, rects: Array, filled: int, animate: bool, delay := 0.0) -> void:
	for r in rects:
		UI.art(c, "star_empty", r)
	for i in filled:
		var star := UI.art(c, "star_full", rects[i])
		if not animate:
			continue
		star.scale = Vector2.ZERO
		var tw := star.create_tween()
		tw.tween_interval(delay + 0.4 * i)
		tw.tween_callback(Sound.play.bind("star_%d" % (i + 1)))
		tw.tween_property(star, "scale", Vector2.ONE * 1.28, 0.16).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
		tw.tween_property(star, "scale", Vector2.ONE, 0.2).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)

## Gentle squash at the feet, forever.
static func _breathe(t: Control, phase: float) -> void:
	var tw := t.create_tween().set_loops()
	tw.tween_property(t, "scale", Vector2(0.99, 1.025), 1.3 + phase * 0.1).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT).set_delay(phase * 0.2)
	tw.tween_property(t, "scale", Vector2.ONE, 1.3 + phase * 0.1).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)

static func _twinkle(t: Control, phase: float) -> void:
	var tw := t.create_tween().set_loops()
	tw.tween_interval(phase)
	tw.tween_property(t, "scale", Vector2.ONE * 1.25, 0.6).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	tw.parallel().tween_property(t, "modulate:a", 1.0, 0.6)
	tw.tween_property(t, "scale", Vector2.ONE * 0.6, 0.8).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	tw.parallel().tween_property(t, "modulate:a", 0.45, 0.8)

## A small bird picture that breathes and blinks without stopping.
static func _live(parent: Node, species: String, box: Rect2, phase: float) -> TextureRect:
	var t := _fit(parent, species + "_front", box, true)
	_breathe(t, phase)
	var rest := t.texture
	var blink: Array[Texture2D] = [Art.tex(species + "_front_blink_01"), Art.tex(species + "_front_blink_02")]
	if blink[0] != null and blink[1] != null:
		var tw := t.create_tween().set_loops()
		tw.tween_interval(2.0 + phase * 1.3)
		for frame in [blink[0], blink[1], blink[0], rest]:
			tw.tween_callback(func(): t.texture = frame)
			tw.tween_interval(0.07)
	return t

## Full BirdView (breathing plus idle actions) fitted into the box; `clip` (non-empty) crops it, e.g. behind a ribbon.
static func _bird_view(c: Control, species: String, box: Rect2, clip: Rect2, instant: bool, cheer_delay: float) -> BirdView:
	var used := UI.used_rect(species + "_front")
	var k := minf(box.size.x / used.size.x, box.size.y / used.size.y)
	var feet := Vector2(box.get_center().x - (used.get_center().x - BirdView.CANVAS * 0.5) * k,
		box.end.y - (used.end.y - BirdView.FEET_Y) * k)
	var parent: Control = c
	if clip.size != Vector2.ZERO:
		parent = Control.new()
		parent.clip_contents = true
		parent.mouse_filter = Control.MOUSE_FILTER_IGNORE
		UI.place(parent, clip)
		c.add_child(parent)
		feet -= clip.position
	var bird := BirdView.new()
	parent.add_child(bird)
	bird.position = feet
	bird.setup(Board.bird(0, Board.species_index(species), 0), false, k * BirdView.CANVAS / BirdView.DISPLAY)
	bird.quiet = instant
	if cheer_delay > 0.0 and not instant:
		var tw := bird.create_tween().set_loops()
		tw.tween_interval(cheer_delay)
		tw.tween_callback(bird.cheer)
		tw.tween_interval(3.0)
	return bird

## Gold portrait ring with a cream fill and a live bird.
static func _ring(c: Control, species: String, center: Vector2, size: float, lift: float) -> void:
	var ring := Rect2(center - Vector2(size, size * 0.92) * 0.5, Vector2(size, size * 0.92))
	_fill(c, ring.grow(-10), CREAM)
	UI.art(c, "badge_ring_gold", ring)
	_live(c, species, Rect2(center.x - 70.0, center.y - 104.0, 140.0, 104.0 + lift), center.x * 0.01)

static func _mechanic_strip(c: Control, mechanics: Array[String], card: Rect2) -> void:
	_pill(c, card)
	var slot := card.size.x / mechanics.size()
	for i in mechanics.size():
		var box := Rect2(card.position.x + slot * i + 12.0, card.position.y + 14.0, slot - 24.0, card.size.y - 28.0)
		var t := _fit(c, MECHANIC_ART[mechanics[i]], box, true)
		if mechanics[i] in ["sleep", "egg", "crow"]:
			_breathe(t, i * 0.7)


# ---------------------------------------------------------------- previews

static func preview(main: Control, state: String) -> void:
	main.open_board(1, false)
	main._clear_popups()
	var host: Control = main.popups
	var noop := func(): pass
	var roster: Array[String] = ["sunny", "sky", "berry", "mint", "cocoa", "snow", "ember"]
	var few: Array[String] = ["sunny", "sky", "berry"]
	var mechanics: Array[String] = ["sleep", "egg", "daynight"]
	var none: Array[String] = []
	match state:
		"win_three_stars": win(host, 3, 12, 12, 30, noop, noop, noop, true)
		"win_two_stars": win(host, 2, 14, 12, 20, noop, noop, noop, true)
		"win_one_star": win(host, 1, 20, 12, 10, noop, noop, noop, false)
		"win_zero_reward": win(host, 3, 12, 12, 0, noop, noop, noop, true)
		"settings_sound_on": settings(host, false, noop, noop, noop, true)
		"settings_muted": settings(host, true, noop, noop, noop, false)
		"level_card_first_visit": level_card(host, 1, 0, 0, 6, few, none, noop, true)
		"level_card_full_roster": level_card(host, 40, 3, 28, 12, roster, mechanics, noop, true)
		"goal_info_berry": goal_info(host, "berry")
		"goal_info_hoot": goal_info(host, "hoot")
		"daily_gift": daily_gift(host, 50, noop)
		"new_friend_berry": new_friend(host, "berry", 25, noop)
		"unlock_owl_eye": unlock(host, 2, noop)
		"unlock_sapling": unlock(host, 1, noop)
		"world_complete_1": world_complete(host, 1, 200, noop)
		"world_unlock_2": world_unlock(host, 2, noop)
		"world_unlock_4": world_unlock(host, 4, noop)
