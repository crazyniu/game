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
	var live := not _instant(host)
	var p = _open(host, Layout.r(s, "panel_frame"))
	var c: Control = p.content
	if live:
		_rays(p, Layout.r(s, "panel_frame").get_center())
	# The hero peeks over the ribbon; the clip hides its feet but leaves room above the screen for the fly-in.
	var hero := _bird_view(c, "sunny", Rect2(412, 616, 246, 309), Rect2(0, -400, 1080, 1215), not live, 2.4)
	var sides := [_bird_view(c, "sky", Rect2(95, 746, 130, 124), Rect2(), not live, 0.0),
		_bird_view(c, "berry", Rect2(855, 746, 130, 124), Rect2(), not live, 0.0)]
	if live:
		_fly_in(hero, Vector2(640, -1250), 0.45, 1.0)
		_fly_in(sides[0], Vector2(-420, -120), 0.75, 0.8)
		_fly_in(sides[1], Vector2(420, -120), 0.9, 0.8)
	var ribbon := _title(c, "label_title_well_done", Layout.r(s, "win_ribbon"))
	if live:
		_drop(ribbon, 0.25)
	_stars(c, [Layout.r(s, "win_star_1"), Layout.r(s, "win_star_2"), Layout.r(s, "win_star_3")], stars, live, 0.6, 0.18)
	var limits := [str(par), str(par + 3), "∞"]
	for i in 3:
		_pill(c, Layout.r(s, "threshold_pill_%d" % (i + 1)))
		UI.number(c, limits[i], Layout.r(s, "threshold_value_%d" % (i + 1)))
	_pill(c, Layout.r(s, "move_pill"))
	UI.art(c, "footprints", Layout.r(s, "move_footprint"))
	var moves_box := UI.number(c, str(moves), Layout.r(s, "move_value"))
	if live:
		_count(moves_box, moves, "%d", 0.5, 0.6)
	if reward > 0:
		var reward_box := _reward(c, Layout.r(s, "reward_pill"), Layout.r(s, "reward_coin"), Layout.r(s, "reward_value"), reward)
		if live:
			_count(reward_box, reward, "+%d", 1.0, 0.7)
			_coin_flight(p, Layout.r(s, "reward_coin").get_center(), Layout.r(s, "coin_hud_icon").get_center(), 1.8)
	_action(p, Layout.r(s, "action_retry"), DISC, "icon_replay", on_replay)
	UI.set_enabled(_action(p, Layout.r(s, "action_play"), PRIMARY, "icon_play", on_next, "btn_green"), next_enabled)
	_action(p, Layout.r(s, "action_home"), DISC, "icon_home", on_home)
	if live:
		_confetti(p, 0.3)
	return p

static func settings(host: Control, muted: bool, on_toggle_sound: Callable, on_restart: Callable, on_home: Callable,
		show_restart: bool) -> Control:
	var s := "settings"
	var p = _open(host, Layout.r(s, "panel"), Layout.r(s, "close"), true)
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


# ---------------------------------------------------------------- win motion

## Slowly turning light rays behind the panel; they fade in with it.
static func _rays(p: Control, center: Vector2) -> void:
	var rays := TextureRect.new()
	rays.name = "Rays"
	rays.texture = Art.tex("light_rays")
	rays.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	rays.size = Vector2.ONE * 1700.0
	rays.position = center - rays.size * 0.5
	rays.pivot_offset = rays.size * 0.5
	rays.mouse_filter = Control.MOUSE_FILTER_IGNORE
	rays.material = CanvasItemMaterial.new()
	rays.material.blend_mode = CanvasItemMaterial.BLEND_MODE_ADD
	rays.modulate.a = 0.0
	p.add_child(rays)
	p.move_child(rays, 1)
	rays.create_tween().tween_property(rays, "modulate:a", 0.38, 0.8)
	rays.create_tween().set_loops().tween_property(rays, "rotation", TAU, 28.0).from(0.0)

## Bird flies in along its arc from `offset` away and then keeps living (idle, breathing).
static func _fly_in(bird: BirdView, offset: Vector2, delay: float, duration: float) -> void:
	var target := bird.position
	bird.position = target + offset
	bird.fly_to(target, delay, duration)

## The node falls in from above with a bounce.
static func _drop(t: Control, delay: float) -> void:
	var rest := t.position
	t.position = rest + Vector2(0, -90)
	t.modulate.a = 0.0
	var tw := t.create_tween().set_parallel()
	tw.tween_property(t, "position", rest, 0.4).set_delay(delay).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	tw.tween_property(t, "modulate:a", 1.0, 0.15).set_delay(delay)

## Painted number counting from 0 to `amount`; `pattern` is "%d" or "+%d".
static func _count(box: Control, amount: int, pattern: String, delay: float, duration: float) -> void:
	UI.set_number(box, pattern % 0)
	var tw := box.create_tween()
	tw.tween_interval(delay)
	tw.tween_method(func(v: float): UI.set_number(box, pattern % roundi(v)), 0.0, float(amount), duration)

## About 3 s of confetti falling from above the screen across the whole popup.
static func _confetti(p: Control, delay: float) -> void:
	var rng := RandomNumberGenerator.new()
	rng.seed = 11
	var layer := Control.new()
	layer.name = "Confetti"
	layer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	p.add_child(layer)
	for i in 44:
		var piece := TextureRect.new()
		piece.texture = Art.tex("confetti_%d" % (1 + i % 4))
		piece.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
		piece.size = Vector2.ONE * rng.randf_range(34.0, 52.0)
		piece.pivot_offset = piece.size * 0.5
		piece.mouse_filter = Control.MOUSE_FILTER_IGNORE
		piece.modulate.a = 0.0
		layer.add_child(piece)
		var x := rng.randf_range(40.0, 1040.0)
		var sway := rng.randf_range(25.0, 70.0)
		var spin := rng.randf_range(360.0, 720.0) * (1.0 if i % 2 == 0 else -1.0)
		var fall := rng.randf_range(2.6, 3.2)
		var tw := piece.create_tween()
		tw.tween_interval(delay + rng.randf_range(0.0, 0.5))
		tw.tween_method(func(t: float):
			piece.position = Vector2(x + sin(t * 9.0 + i) * sway, lerpf(-80.0, 1500.0, t * t * 0.4 + t * 0.6))
			piece.rotation_degrees = spin * t
			piece.modulate.a = minf(1.0, t * 12.0) * minf(1.0, (1.0 - t) * 5.0), 0.0, 1.0, fall)
		tw.tween_callback(piece.queue_free)

## A burst of coins flying from the reward to the HUD coin counter.
static func _coin_flight(p: Control, from: Vector2, to: Vector2, delay: float) -> void:
	for i in 7:
		var coin := UI.image(p, "coin", Rect2(Vector2.ZERO, Vector2(60, 60)))
		coin.modulate.a = 0.0
		var tw := coin.create_tween()
		tw.tween_interval(delay + i * 0.07)
		tw.tween_method(func(t: float):
			var at := from.lerp(to, t) + Vector2((i - 3) * 26.0 * sin(t * PI), -160.0 * sin(t * PI))
			coin.position = at - coin.size * 0.5
			coin.scale = Vector2.ONE * lerpf(1.0, 0.55, t)
			coin.modulate.a = minf(1.0, t * 10.0) * minf(1.0, (1.0 - t) * 6.0), 0.0, 1.0, 0.65)
		tw.tween_callback(coin.queue_free)


# ---------------------------------------------------------------- helpers

static func _sound_icon(muted: bool) -> String:
	return "icon_sound_off" if muted else "icon_sound_on"

static func _instant(host: Node) -> bool:
	var main := host.get_parent()
	return main != null and main.has_method("instant") and main.instant()

## The measured rect is the visible frame; popup.gd compensates for panel_wood's transparent margins.
static func _open(host: Control, panel: Rect2, close := Rect2(), quick := false) -> Control:
	return FRAME.open(host, panel, "", Rect2(), close, _instant(host), quick)

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
static func _title(c: Control, label: String, bounds: Rect2) -> TextureRect:
	return _fit(c, label, Rect2(bounds.get_center() - bounds.size * 0.575, bounds.size * 1.15))

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

static func _reward(c: Control, pill: Rect2, coin: Rect2, value: Rect2, amount: int) -> Control:
	_pill(c, pill)
	UI.art(c, "coin", coin)
	return UI.number(c, "+%d" % amount, value)

## Empty stars first, then the earned ones on top; earned stars pop in one after another with a sound.
static func _stars(c: Control, rects: Array, filled: int, animate: bool, delay := 0.0, spacing := 0.4) -> void:
	for r in rects:
		UI.art(c, "star_empty", r)
	for i in filled:
		var star := UI.art(c, "star_full", rects[i])
		if not animate:
			continue
		star.scale = Vector2.ZERO
		var tw := star.create_tween()
		tw.tween_interval(delay + spacing * i)
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
		"leaf_transition":
			main.open_map()
			await main.get_tree().create_timer(0.6).timeout
			main.open_board(1, false)
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
