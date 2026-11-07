extends RefCounted
## Modal popups opened on a host Control (main.popups). Every function returns the Popup; callbacks are Callables.
## Layout follows docs/design/ui-system.md section 5: the panel height follows its content, groups are stacked
## top to bottom 32 px apart and centred, actions sit 40 px below the panel. Words are painted sprites, numbers painted glyphs.

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

# ---------------------------------------------------------------- layout (ui-system.md, 1080x1920 design pixels)

const SCREEN := Vector2(1080, 1920)
## Panel widths: content popups and small popups (settings, confirmations).
const WIDE := 880.0
const NARROW := 720.0
## Felt rim of panel_wood inside the visible frame (left, top, right, bottom); content keeps 32 px off the cream.
const RIM := Vector4(50, 60, 50, 70)
const INSET := 32.0
## Spacing: between content groups, between parts of one group.
const GAP := 32.0
const TIGHT := 16.0
## Title ribbons: every label_title_* shares one painted template, so one content rect gives them one scale.
const RIBBON_W := 640.0
const SMALL_RIBBON_W := 540.0
const RIBBON_CONTENT := Rect2(18, 20, 2004, 320)
const RIBBON_BODY := 274.0          # painted ribbon body height inside the template (canvas px)
const NAME_RIBBON_H := 110.0        # label_name_* ribbons used as titles or name plates
const CAPTION_H := 62.0             # label_caption_* plates
## Buttons.
const DISC := 128.0
const PRIMARY := 160.0
const CLOSE := 96.0
const CLOSE_INSET := 24.0
const ACTION_GAP := 64.0
const ACTION_DROP := 40.0
## Pills and numbers.
const PILL_H := 80.0
const REWARD_PILL_H := 96.0
const PILL_SIDE := 32.0
const ICON_H := 60.0
const COIN := 104.0
const CAP_HERO := 72.0
const CAP_MAIN := 52.0
const CAP_SMALL := 30.0
## Stars share one content rect so empty and earned stars line up exactly.
const STAR := 132.0
const STAR_PITCH := 200.0
const STAR_CONTENT := Rect2(0, 11, 252, 245)
const THRESHOLD_W := 140.0
## Portrait rings in popups.
const RING := 150.0
const RING_GAP := 32.0
const RING_ROW_GAP := 24.0
const RING_BIRD := 0.8
const RING_BADGE := 0.44
## Word sprite "Best": painted like a glyph (cap from canvas 26 to 126).
const WORD_CONTENT := Rect2(6, 26, 268, 100)
## Daily gift: nest, box and lid share one 768 canvas; coins and sparkles in the same canvas pixels.
const GIFT_CONTENT := Rect2(70, 170, 628, 570)
const GIFT_H := 400.0
const GIFT_COINS := [Rect2(102, 395, 80, 99), Rect2(531, 409, 82, 95), Rect2(137, 449, 145, 138), Rect2(456, 462, 134, 97),
	Rect2(271, 444, 187, 156)]
const GIFT_SPARKS := [Rect2(110, 220, 72, 72), Rect2(50, 300, 64, 64), Rect2(540, 180, 64, 64), Rect2(600, 300, 72, 72)]
## World gate: arch, backdrop, light and doors share one 1024 canvas.
const GATE_CONTENT := Rect2(82, 100, 855, 777)
const GATE_W := 560.0
const ARCH_W := 500.0
## Open doors swing this far beyond the arch on each side.
const DOOR_SWING := 80.0
## Birds standing in front of the world-complete arch: centre x as a share of the arch width, box height.
const ARCH_BIRDS := ["sunny", "sky", "berry"]
const ARCH_BIRD_X := [-0.38, 0.0, 0.38]
const ARCH_BIRD_H := 200.0
const ARCH_FLOOR := 44.0
## Hero and side birds of the win panel.
const HERO_H := 240.0
const SIDE_H := 150.0
const SIDE_X := [168.0, 912.0]
const FRIEND_H := 300.0
const GOAL_BIRD_H := 280.0
const BADGE := 96.0
const BOOSTER_H := 300.0
## HUD coin on the board (ui-system.md section 6): target of the reward coin burst.
const HUD_COIN := Vector2(92, 88)


# ---------------------------------------------------------------- popups

static func win(host: Control, stars: int, moves: int, par: int, reward: int, on_replay: Callable, on_next: Callable,
		on_home: Callable, next_enabled: bool) -> Control:
	var live := not _instant(host)
	var heights := [STAR + TIGHT + PILL_H, PILL_H]
	if reward > 0:
		heights.append(REWARD_PILL_H)
	var f := _frame(host, WIDE, "label_title_well_done", heights, HERO_H, false, PRIMARY)
	var p: Control = f["p"]
	var c: Control = f["c"]
	var panel: Rect2 = f["panel"]
	var ys: Array = f["ys"]
	var ribbon: TextureRect = f["ribbon"]
	if live:
		_rays(p, panel.get_center())
		_drop(ribbon, 0.25)
	# Side birds stand on the panel's top edge beside the ribbon; the hero stands on the ribbon, in front of it.
	var ribbon_top: float = f["ribbon_top"]
	var sides := [_bird_view(c, "sky", Rect2(SIDE_X[0] - SIDE_H * 0.5, panel.position.y + 14.0 - SIDE_H, SIDE_H, SIDE_H), Rect2(), not live, 0.0),
		_bird_view(c, "berry", Rect2(SIDE_X[1] - SIDE_H * 0.5, panel.position.y + 14.0 - SIDE_H, SIDE_H, SIDE_H), Rect2(), not live, 0.0)]
	var hero := _bird_view(c, "sunny", Rect2(540.0 - HERO_H * 0.5, ribbon_top + 10.0 - HERO_H, HERO_H, HERO_H), Rect2(), not live, 2.4)
	if live:
		_fly_in(hero, Vector2(640, -1250), 0.45, 1.0)
		_fly_in(sides[0], Vector2(-420, -120), 0.75, 0.8)
		_fly_in(sides[1], Vector2(420, -120), 0.9, 0.8)
	_star_row(c, ys[0], stars, live, 0.6, 0.18, par)
	var moves_box := _icon_pill(c, ys[1], "footprints", str(moves))
	if live:
		_count(moves_box, moves, "%d", 0.5, 0.6)
	if reward > 0:
		var reward_parts := _coin_pill(c, ys[2], "+%d" % reward)
		if live:
			_count(reward_parts[0], reward, "+%d", 1.0, 0.7)
			_coin_flight(p, reward_parts[1], HUD_COIN, 1.8)
	var row := _action_row(p, f["actions_y"], [
		["icon_replay", on_replay, "btn_wood"], ["icon_play", on_next, "btn_green"], ["icon_home", on_home, "btn_wood"]])
	UI.set_enabled(row[1], next_enabled)
	if live:
		_confetti(p, 0.3)
	return p

static func settings(host: Control, muted: bool, on_toggle_sound: Callable, on_restart: Callable, on_home: Callable,
		show_restart: bool) -> Control:
	# Icon-only discs: the caption plates have different painted widths, and the icons are clear on their own.
	var f := _frame(host, NARROW, "label_title_settings", [DISC], 0.0, true, 0.0, true)
	var p: Control = f["p"]
	var ys: Array = f["ys"]
	var ids := ["sound", "restart", "home"] if show_restart else ["sound", "home"]
	var state := {"muted": muted}
	var x := 540.0 - (ids.size() * DISC + (ids.size() - 1) * ACTION_GAP) * 0.5
	for id in ids:
		var cb: Callable = {"sound": on_toggle_sound, "restart": on_restart, "home": on_home}[id]
		var icon: String = {"sound": _sound_icon(muted), "restart": "icon_replay", "home": "icon_home"}[id]
		var b: Button = p.action(Rect2(x, ys[0], DISC, DISC), icon, func():
			cb.call()
			if id == "sound":
				state["muted"] = not state["muted"]
				state["button"].get_node("Icon").texture = Art.tex(_sound_icon(state["muted"])), "btn_wood", id != "sound")
		b.name = id.capitalize()
		if id == "sound":
			state["button"] = b
		x += DISC + ACTION_GAP
	return p

static func level_card(host: Control, level: int, best_stars: int, best_moves: int, par: int, species: Array[String],
		mechanics: Array[String], on_play: Callable, playable: bool) -> Control:
	var rows := ceili(species.size() / 4.0)
	var heights := [STAR + TIGHT + PILL_H]
	if best_moves > 0:
		heights.append(PILL_H)
	if rows > 0:
		heights.append(rows * RING + (rows - 1) * RING_ROW_GAP)
	if not mechanics.is_empty():
		heights.append(PILL_H)
	var f := _frame(host, WIDE, "label_title_level_%d" % level, heights, 0.0, true, PRIMARY)
	var p: Control = f["p"]
	var c: Control = f["c"]
	var ys: Array = f["ys"]
	var g := 0
	_star_row(c, ys[g], best_stars, false, 0.0, 0.0, par)
	g += 1
	if best_moves > 0:
		_best_pill(c, ys[g], best_moves)
		g += 1
	for i in species.size():
		var in_row := mini(4, species.size() - (i / 4) * 4)
		var center := Vector2(540.0 + (i % 4 - (in_row - 1) * 0.5) * (RING + RING_GAP),
			ys[g] + RING * 0.5 + (i / 4) * (RING + RING_ROW_GAP))
		_ring(c, species[i], center, _instant(host))
	if rows > 0:
		g += 1
	if not mechanics.is_empty():
		_mechanic_strip(c, mechanics, ys[g])
	UI.set_enabled(_action_row(p, f["actions_y"], [["icon_play", on_play, "btn_green"]])[0], playable)
	return p

static func goal_info(host: Control, species: String) -> Control:
	var f := _frame(host, WIDE, "label_name_" + species, [GOAL_BIRD_H, BADGE], 0.0, true)
	var p: Control = f["p"]
	var c: Control = f["c"]
	var ys: Array = f["ys"]
	_bird_view(c, species, Rect2(540.0 - GOAL_BIRD_H * 0.5, ys[0], GOAL_BIRD_H, GOAL_BIRD_H), Rect2(), _instant(host), 0.0)
	# Ability: badge disc then its caption plate, centred together.
	var caption: String = "label_caption_" + GOAL_CAPTION.get(species, "wakes_sleepers")
	var cap_w := _width_at(caption, CAPTION_H)
	var has_badge := EFFECT.has(species)
	var total := cap_w + ((BADGE + TIGHT) if has_badge else 0.0)
	var x := 540.0 - total * 0.5
	if has_badge:
		UI.art(c, "gift_coin_" + EFFECT[species], Rect2(x, ys[1], BADGE, BADGE))
		x += BADGE + TIGHT
	UI.art(c, caption, Rect2(x, ys[1] + (BADGE - CAPTION_H) * 0.5, cap_w, CAPTION_H))
	return p

static func daily_gift(host: Control, reward: int, on_claim: Callable) -> Control:
	var f := _frame(host, WIDE, "label_title_daily_gift", [GIFT_H, REWARD_PILL_H], 0.0, false, PRIMARY)
	var p: Control = f["p"]
	var c: Control = f["c"]
	var ys: Array = f["ys"]
	# The stage maps the shared 768 canvas so its visible pixels fill the gift row, centred.
	var k := GIFT_H / GIFT_CONTENT.size.y
	var stage := Control.new()
	stage.name = "Gift"
	stage.mouse_filter = Control.MOUSE_FILTER_IGNORE
	stage.size = Vector2(768, 768)
	stage.scale = Vector2.ONE * k
	stage.position = Vector2(540.0 - GIFT_CONTENT.get_center().x * k, ys[0] - GIFT_CONTENT.position.y * k)
	c.add_child(stage)
	_canvas(stage, "gift_nest")
	for r in GIFT_COINS:
		UI.art(stage, "coin", r)
	_canvas(stage, "gift_box_base")
	var lid := _canvas(stage, "gift_box_lid")
	_canvas(stage, "gift_nest_front")
	var bob := lid.create_tween().set_loops()
	bob.tween_property(lid, "position:y", -8.0, 1.1).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	bob.tween_property(lid, "position:y", 0.0, 1.1).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	for i in GIFT_SPARKS.size():
		_twinkle(UI.art(stage, "sparkle", GIFT_SPARKS[i]), i * 0.4)
	_coin_pill(c, ys[1], "+%d" % reward)
	_action_row(p, f["actions_y"], [["icon_check", on_claim, "btn_green"]])
	return p

static func new_friend(host: Control, species: String, reward: int, on_ok: Callable) -> Control:
	var f := _frame(host, WIDE, "label_title_new_friend", [FRIEND_H + TIGHT + NAME_RIBBON_H, REWARD_PILL_H], 0.0, false, PRIMARY)
	var p: Control = f["p"]
	var c: Control = f["c"]
	var ys: Array = f["ys"]
	_bird_view(c, species, Rect2(540.0 - FRIEND_H * 0.5, ys[0], FRIEND_H, FRIEND_H), Rect2(), _instant(host), 0.0)
	var name_w := _width_at("label_name_" + species, NAME_RIBBON_H)
	UI.art(c, "label_name_" + species, Rect2(540.0 - name_w * 0.5, ys[0] + FRIEND_H + TIGHT, name_w, NAME_RIBBON_H))
	_coin_pill(c, ys[1], "+%d" % reward)
	_action_row(p, f["actions_y"], [["icon_check", on_ok, "btn_green"]])
	return p

static func unlock(host: Control, booster: int, on_ok: Callable) -> Control:
	var f := _frame(host, WIDE, "label_title_booster_" + BOOSTER_ID[booster], [BOOSTER_H, CAPTION_H], 0.0, false, PRIMARY)
	var p: Control = f["p"]
	var c: Control = f["c"]
	var ys: Array = f["ys"]
	var box := Rect2(540.0 - BOOSTER_H * 0.5, ys[0], BOOSTER_H, BOOSTER_H)
	var sparks := [Vector2(-0.78, 0.18), Vector2(0.78, 0.12), Vector2(-0.66, 0.82), Vector2(0.7, 0.8)]
	for i in sparks.size():
		var at: Vector2 = box.get_center() + sparks[i] * BOOSTER_H * 0.5 * Vector2(1.0, 1.0) - Vector2(0, BOOSTER_H * 0.25)
		_twinkle(UI.art(c, "sparkle", Rect2(at - Vector2(32, 32), Vector2(64, 64))), i * 0.35)
	var illus := _fit(c, "booster_%s_illus" % BOOSTER_ILLUS[booster], box, true)
	_breathe(illus, 0.0)
	var caption: String = "label_caption_" + BOOSTER_CAPTION[booster]
	var cap_w := _width_at(caption, CAPTION_H)
	UI.art(c, caption, Rect2(540.0 - cap_w * 0.5, ys[1], cap_w, CAPTION_H))
	_action_row(p, f["actions_y"], [["icon_check", on_ok, "btn_green"]])
	return p

static func world_complete(host: Control, world: int, reward: int, on_ok: Callable) -> Control:
	var arch_h := ARCH_W * GATE_CONTENT.size.y / GATE_CONTENT.size.x
	var f := _frame(host, WIDE, "label_title_world_complete", [STAR, arch_h, REWARD_PILL_H], 0.0, false, PRIMARY)
	var p: Control = f["p"]
	var c: Control = f["c"]
	var ys: Array = f["ys"]
	var live := not _instant(host)
	var star_rects := []
	for i in 3:
		star_rects.append(Rect2(540.0 + (i - 1) * STAR_PITCH - STAR * 0.5, ys[0], STAR, STAR))
	_stars(c, star_rects, 3, live, 0.5)
	var arch := Rect2(540.0 - ARCH_W * 0.5, ys[1], ARCH_W, arch_h)
	UI.art(c, "gate_arch", arch, GATE_CONTENT)
	for i in 3:
		var cx: float = 540.0 + ARCH_BIRD_X[i] * ARCH_W
		var box := Rect2(cx - ARCH_BIRD_H * 0.5, arch.end.y - ARCH_FLOOR - ARCH_BIRD_H, ARCH_BIRD_H, ARCH_BIRD_H)
		var bird := _bird_view(c, ARCH_BIRDS[i], box, Rect2(), not live, 0.0)
		if live:
			var tw := bird.create_tween().set_loops()
			tw.tween_interval(1.6 + i * 0.9)
			tw.tween_callback(bird.cheer)
			tw.tween_interval(2.6)
	_coin_pill(c, ys[2], "+%d" % reward)
	_action_row(p, f["actions_y"], [["icon_check", on_ok, "btn_green"]])
	return p

static func world_unlock(host: Control, world: int, on_play: Callable) -> Control:
	var gate_h := GATE_W * GATE_CONTENT.size.y / GATE_CONTENT.size.x
	var f := _frame(host, WIDE, "label_title_world_unlocked", [gate_h, CAPTION_H], 0.0, false, PRIMARY)
	var p: Control = f["p"]
	var c: Control = f["c"]
	var ys: Array = f["ys"]
	# The light glow is taller than the arch, so the stage sits in a clip the size of the arch's row.
	var clip := Control.new()
	clip.name = "GateClip"
	clip.clip_contents = true
	clip.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(clip, Rect2(540.0 - GATE_W * 0.5 - DOOR_SWING, ys[0], GATE_W + DOOR_SWING * 2.0, gate_h))
	c.add_child(clip)
	var k := GATE_W / GATE_CONTENT.size.x
	var stage := Control.new()
	stage.name = "Gate"
	stage.mouse_filter = Control.MOUSE_FILTER_IGNORE
	stage.size = Vector2(1024, 1024)
	stage.scale = Vector2.ONE * k
	stage.position = Vector2(clip.size.x * 0.5 - GATE_CONTENT.get_center().x * k, -GATE_CONTENT.position.y * k)
	clip.add_child(stage)
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
	var caption: String = "label_caption_%s_forest" % WORLD_FOREST.get(world, "bamboo")
	var cap_w := _width_at(caption, CAPTION_H)
	UI.art(c, caption, Rect2(540.0 - cap_w * 0.5, ys[1], cap_w, CAPTION_H))
	_action_row(p, f["actions_y"], [["icon_play", on_play, "btn_green"]])
	return p


# ---------------------------------------------------------------- frame and rows

## Opens the panel sized to its content and returns {"p", "c", "panel", "ys" (top y of each group), "ribbon",
## "ribbon_top", "actions_y"}. The whole composition (`above` the ribbon, the panel, the action row of height
## `action_h`) is centred on the screen.
static func _frame(host: Control, width: float, title: String, heights: Array, above: float, close: bool,
		action_h := 0.0, quick := false) -> Dictionary:
	var is_name := title.begins_with("label_name_")
	var ribbon_w := SMALL_RIBBON_W if width < WIDE else RIBBON_W
	var ribbon_box_h := ribbon_w * RIBBON_CONTENT.size.y / RIBBON_CONTENT.size.x
	var body_h := NAME_RIBBON_H if is_name else ribbon_w * RIBBON_BODY / RIBBON_CONTENT.size.x
	var first := maxf(body_h * 0.5 + GAP, RIM.y + INSET)
	var inner := 0.0
	for h in heights:
		inner += h
	inner += GAP * maxf(0.0, heights.size() - 1)
	var panel_h := roundf(first + inner + RIM.w + INSET)
	var below := (ACTION_DROP + action_h) if action_h > 0.0 else 0.0
	var top_extra := body_h * 0.5 + above
	var top := roundf((SCREEN.y - (top_extra + panel_h + below)) * 0.5 + top_extra)
	var panel := Rect2((SCREEN.x - width) * 0.5, top, width, panel_h)
	var close_rect := Rect2()
	if close:
		close_rect = Rect2(panel.end.x - CLOSE_INSET - CLOSE * 0.5, panel.position.y + CLOSE_INSET - CLOSE * 0.5, CLOSE, CLOSE)
	var p = FRAME.open(host, panel, "", Rect2(), close_rect, _instant(host), quick)
	var c: Control = p.content
	var ribbon: TextureRect
	if is_name:
		var w := _width_at(title, NAME_RIBBON_H)
		ribbon = UI.art(c, title, Rect2(540.0 - w * 0.5, top - NAME_RIBBON_H * 0.5, w, NAME_RIBBON_H))
	else:
		ribbon = UI.art(c, title, Rect2(540.0 - ribbon_w * 0.5, top - ribbon_box_h * 0.5, ribbon_w, ribbon_box_h), RIBBON_CONTENT)
	if close:
		# Keep the close disc above the ribbon and content.
		var disc := c.get_node_or_null("Close")
		if disc:
			c.move_child(disc, c.get_child_count() - 1)
	var ys := []
	var y := top + first
	for h in heights:
		ys.append(y)
		y += h + GAP
	return {"p": p, "c": c, "panel": panel, "ys": ys, "ribbon": ribbon, "ribbon_top": top - body_h * 0.5,
		"actions_y": panel.end.y + ACTION_DROP}

## A centred row of action discs 40 px under the panel: green ones are primaries (160), wood ones secondaries (128),
## 64 px apart and centred on one line. Each item: [icon, callback, base].
static func _action_row(p: Control, y: float, items: Array) -> Array:
	var sizes := []
	var total := 0.0
	for it in items:
		var s := PRIMARY if it[2] == "btn_green" else DISC
		sizes.append(s)
		total += s
	total += ACTION_GAP * (items.size() - 1)
	var x := 540.0 - total * 0.5
	var out := []
	for i in items.size():
		var s: float = sizes[i]
		out.append(p.action(Rect2(x, y + (PRIMARY - s) * 0.5, s, s), items[i][0], items[i][1], items[i][2]))
		x += s + ACTION_GAP
	return out

## Three stars with their move thresholds in small pills right under them.
static func _star_row(c: Control, y: float, filled: int, animate: bool, delay: float, spacing: float, par: int) -> void:
	var rects := []
	var limits := [str(par), str(par + 3), "∞"]
	for i in 3:
		var cx := 540.0 + (i - 1) * STAR_PITCH
		rects.append(Rect2(cx - STAR * 0.5, y, STAR, STAR))
		var pill := Rect2(cx - THRESHOLD_W * 0.5, y + STAR + TIGHT, THRESHOLD_W, PILL_H)
		_pill(c, pill)
		UI.number(c, limits[i], Rect2(pill.position.x + PILL_SIDE * 0.5, pill.position.y + (PILL_H - CAP_SMALL) * 0.5,
			pill.size.x - PILL_SIDE, CAP_SMALL))
	_stars(c, rects, filled, animate, delay, spacing if spacing > 0.0 else 0.4)

## Cream pill with an icon and a main number, centred; returns the number box.
static func _icon_pill(c: Control, y: float, icon: String, text: String) -> Control:
	var num_w := _number_width(text, CAP_MAIN)
	var icon_w := ICON_H
	var w := PILL_SIDE * 2.0 + icon_w + TIGHT + num_w
	var pill := Rect2(540.0 - w * 0.5, y, w, PILL_H)
	_pill(c, pill)
	UI.art(c, icon, Rect2(pill.position.x + PILL_SIDE, y + (PILL_H - ICON_H) * 0.5, icon_w, ICON_H))
	return UI.number(c, text, Rect2(pill.position.x + PILL_SIDE + icon_w + TIGHT, y + (PILL_H - CAP_MAIN) * 0.5, num_w, CAP_MAIN), "brown", -1)

## Best-moves pill: footprints, the painted word "Best" and the number, all at the main cap height.
static func _best_pill(c: Control, y: float, best: int) -> void:
	var text := str(best)
	var num_w := _number_width(text, CAP_MAIN)
	var word_w := WORD_CONTENT.size.x * CAP_MAIN / WORD_CONTENT.size.y
	var w := PILL_SIDE * 2.0 + ICON_H + TIGHT + word_w + TIGHT + num_w
	var pill := Rect2(540.0 - w * 0.5, y, w, PILL_H)
	_pill(c, pill)
	var x := pill.position.x + PILL_SIDE
	UI.art(c, "footprints", Rect2(x, y + (PILL_H - ICON_H) * 0.5, ICON_H, ICON_H))
	x += ICON_H + TIGHT
	var word := UI.art(c, "label_word_best", Rect2(x, y + (PILL_H - CAP_MAIN) * 0.5, word_w, CAP_MAIN), WORD_CONTENT)
	word.name = "Best"
	x += word_w + TIGHT
	UI.number(c, text, Rect2(x, y + (PILL_H - CAP_MAIN) * 0.5, num_w, CAP_MAIN), "brown", -1)

## Reward pill: coin 104 overlapping the pill's left end by half its width, hero number 24 px after the coin.
## Returns [number box, coin centre]. The coin and pill are centred together.
static func _coin_pill(c: Control, y: float, text: String) -> Array:
	var num_w := _number_width(text, CAP_HERO)
	var pill_w := COIN * 0.5 + 24.0 + num_w + PILL_SIDE
	var left := 540.0 - (COIN * 0.5 + pill_w) * 0.5
	var pill := Rect2(left + COIN * 0.5, y, pill_w, REWARD_PILL_H)
	_pill(c, pill)
	var coin := Rect2(left, y + (REWARD_PILL_H - COIN) * 0.5, COIN, COIN)
	UI.art(c, "coin", coin)
	var box := UI.number(c, text, Rect2(coin.end.x + 24.0, y + (REWARD_PILL_H - CAP_HERO) * 0.5, num_w, CAP_HERO), "brown", -1)
	return [box, coin.get_center()]


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

## Painted number counting from 0 to `amount`; `pattern` is "%d" or "+%d". Left-aligned like its final value.
static func _count(box: Control, amount: int, pattern: String, delay: float, duration: float) -> void:
	UI.set_number(box, pattern % 0, "brown", -1)
	var tw := box.create_tween()
	tw.tween_interval(delay)
	tw.tween_method(func(v: float): UI.set_number(box, pattern % roundi(v), "brown", -1), 0.0, float(amount), duration)

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

## Painted width of a number at a cap height.
static func _number_width(text: String, cap: float, color := "brown") -> float:
	var metrics := Art.glyph_metrics(color)
	var w := 0.0
	for ch in text:
		var key: String = UI.GLYPH_NAMES.get(ch, ch)
		if metrics.has(key):
			w += float(metrics[key]["advance"])
	return w * cap / 100.0

## Width of a sprite's visible pixels when drawn at `height`.
static func _width_at(name: String, height: float) -> float:
	var used := UI.used_rect(name)
	return used.size.x * height / maxf(used.size.y, 1.0)

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

## Empty stars first, then the earned ones on top; earned stars pop in one after another with a sound.
static func _stars(c: Control, rects: Array, filled: int, animate: bool, delay := 0.0, spacing := 0.4) -> void:
	for r in rects:
		UI.art(c, "star_empty", r, STAR_CONTENT)
	for i in filled:
		var star := UI.art(c, "star_full", rects[i], STAR_CONTENT)
		if not animate:
			continue
		star.scale = Vector2.ZERO
		var tw := star.create_tween()
		tw.tween_interval(delay + spacing * i)
		tw.tween_callback(Sound.play.bind("star_%d" % (i + 1)))
		tw.tween_property(star, "scale", Vector2.ONE * 1.28, 0.16).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
		tw.tween_property(star, "scale", Vector2.ONE, 0.2).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)

## Gentle squash at the feet, forever (props only; birds are BirdViews).
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

## Full BirdView (breathing plus idle actions) whose front frame fits the box, feet on the box bottom, never stretched;
## `clip` (non-empty) crops it.
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

## Gold portrait ring 150 with a cream fill, a live bird at 80 % of the ring and the ability badge at its bottom-right.
static func _ring(c: Control, species: String, center: Vector2, instant: bool) -> void:
	var ring := Rect2(center - Vector2(RING, RING) * 0.5, Vector2(RING, RING))
	_fill(c, ring.grow(-14), CREAM)
	UI.art(c, "badge_ring_gold", ring)
	var inner := RING * RING_BIRD
	_bird_view(c, species, Rect2(center - Vector2(inner, inner) * 0.5, Vector2(inner, inner)), Rect2(), instant, 0.0)
	var effect: String = EFFECT.get(species, "")
	if not effect.is_empty():
		var b := RING * RING_BADGE
		UI.art(c, "gift_coin_" + effect, Rect2(ring.end - Vector2(b, b) * 0.8, Vector2(b, b)))

## Mechanics of the level as icons in one cream pill, centred.
static func _mechanic_strip(c: Control, mechanics: Array[String], y: float) -> void:
	var icon := 64.0
	var gap := 40.0
	var w := PILL_SIDE * 2.0 + mechanics.size() * icon + (mechanics.size() - 1) * gap
	var pill := Rect2(540.0 - w * 0.5, y, w, PILL_H)
	_pill(c, pill)
	for i in mechanics.size():
		var box := Rect2(pill.position.x + PILL_SIDE + i * (icon + gap), y + (PILL_H - icon) * 0.5, icon, icon)
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
		"level_card_locked": level_card(host, 12, 0, 0, 9, few, mechanics, noop, false)
		"goal_info_berry": goal_info(host, "berry")
		"goal_info_hoot": goal_info(host, "hoot")
		"daily_gift": daily_gift(host, 50, noop)
		"new_friend_berry": new_friend(host, "berry", 25, noop)
		"unlock_owl_eye": unlock(host, 2, noop)
		"unlock_sapling": unlock(host, 1, noop)
		"world_complete_1": world_complete(host, 1, 200, noop)
		"world_unlock_2": world_unlock(host, 2, noop)
		"world_unlock_4": world_unlock(host, 4, noop)
