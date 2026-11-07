extends RefCounted
## Modal popups opened on a host Control (main.popups). Every function returns the popup; callbacks are Callables.
## Layout: docs/design/ui-system.md. One cream panel, a title word, content groups stacked 32 apart, actions in a
## row under the panel. Words are painted word sprites, numbers painted glyphs, icons the white icon family.

const EFFECT := {"sunny": "wake", "sky": "reveal", "berry": "encore", "mint": "grow", "cocoa": "scare", "snow": "lightstep",
	"ember": "hatch", "ink": "nightcall", "hoot": "moonbeam"}
const NAME := {"sunny": "Sunny", "sky": "Sky", "berry": "Berry", "mint": "Mint", "cocoa": "Cocoa", "snow": "Snow",
	"ember": "Ember", "ink": "Ink", "hoot": "Hoot", "crow": "Crow"}
const ABILITY := {"sunny": "Wakes sleepers", "sky": "Reveals eggs", "berry": "One free move", "mint": "Grows branches",
	"cocoa": "Sends crows away", "snow": "Land anywhere", "ember": "Hatches every egg", "ink": "Switches day night",
	"hoot": "Moonlit moves"}
const BOOSTER_NAME := ["Firefly", "Sapling", "Owl Eye", "Morning Bell", "Scarecrow", "Spring Rain"]
const BOOSTER_ILLUS := ["hint", "sapling", "owl_eye", "bell", "scarecrow", "rain"]
const BOOSTER_CAPTION := ["Find a helpful move", "Add one branch", "Reveals eggs", "Wake sleeping birds", "Send crows away",
	"Grow branches fully"]
const WORLD_NAME := {1: "Green Forest", 2: "Bamboo Forest", 3: "Autumn Forest", 4: "Snow Forest"}
const MECHANIC_ART := {"sleep": "hoot_sleep", "egg": "egg_idle", "crow": "crow_front", "daynight": "mechanic_day_night",
	"twig": "icon_sapling", "growing": "icon_sapling", "swivel": "swivel_knot"}

## Content sizes (design pixels).
const STAR_SIDE := 150.0
const STAR_MID := 190.0
const STAR_ROW := 200.0            # height of the star row (the middle star is raised)
const SMALL_STAR := 120.0
const LIMIT_CAP := 36.0            # move limits under the level card's stars
const LINE := 64.0                 # one text line group (icon + number, or a caption)
const CAPTION_CAP := 44.0
const REWARD_H := 96.0
const RING := 150.0
const RING_GAP := 28.0
const HERO := 240.0
const SIDE := 150.0
const FRIEND := 300.0
const ILLUS := 300.0
const GIFT_H := 360.0
const GATE_W := 520.0
## Shared art canvases (gift nest/box, world gate): visible content rect in their own pixels.
const GIFT_CONTENT := Rect2(70, 170, 628, 570)
const GIFT_COINS := [Rect2(102, 395, 80, 99), Rect2(531, 409, 82, 95), Rect2(137, 449, 145, 138), Rect2(456, 462, 134, 97),
	Rect2(271, 444, 187, 156)]
const GIFT_SPARKS := [Rect2(110, 220, 72, 72), Rect2(50, 300, 64, 64), Rect2(540, 180, 64, 64), Rect2(600, 300, 72, 72)]
const GATE_CONTENT := Rect2(82, 100, 855, 777)
const DOOR_SWING := 80.0
const ARCH_BIRDS := ["sunny", "sky", "berry"]
const ARCH_BIRD_X := [-0.38, 0.0, 0.38]
const ARCH_BIRD_H := 190.0
const ARCH_FLOOR := 44.0
## Where reward coins fly to: the coin counter of the screen underneath (map top right).
const HUD_COIN := Vector2(960, 104)


# ---------------------------------------------------------------- popups

static func win(host: Control, stars: int, moves: int, par: int, reward: int, on_replay: Callable, on_next: Callable,
		on_home: Callable, next_enabled: bool) -> Control:
	var live := not _instant(host)
	var heights := [STAR_ROW, LINE]
	if reward > 0:
		heights.append(REWARD_H)
	var p := Modal.open(host, "Well done!", heights, false, UI.PRIMARY, HERO - 40.0, not live)
	var c := p.content
	var ys: Array = p.get_meta("ys")
	var panel := p.panel_rect
	if live:
		_rays(p, panel.get_center())
	# Birds stand on the panel's top edge: the hero in the middle, friends at the sides.
	var hero := _bird_view(c, "sunny", Rect2(540.0 - HERO * 0.5, panel.position.y + 18.0 - HERO, HERO, HERO), not live, 2.4)
	var sides := [_bird_view(c, "sky", Rect2(panel.position.x + 40.0, panel.position.y + 18.0 - SIDE, SIDE, SIDE), not live, 0.0),
		_bird_view(c, "berry", Rect2(panel.end.x - 40.0 - SIDE, panel.position.y + 18.0 - SIDE, SIDE, SIDE), not live, 0.0)]
	if live:
		_fly_in(hero, Vector2(640, -1250), 0.45, 1.0)
		_fly_in(sides[0], Vector2(-420, -120), 0.75, 0.8)
		_fly_in(sides[1], Vector2(420, -120), 0.9, 0.8)
	_big_stars(c, ys[0], stars, live, 0.6)
	var moves_box := _icon_line(c, ys[1], "hop", str(moves))
	if live:
		_count(moves_box, moves, "%d", 0.5, 0.6)
	if reward > 0:
		var parts := _reward(c, ys[2], reward)
		if live:
			_count(parts[0], reward, "+%d", 1.0, 0.7)
			_coin_flight(p, parts[1], HUD_COIN, 1.8)
	var row := p.actions([["orange", "retry", on_replay], ["green", "play", on_next], ["orange", "home", on_home]])
	UI.set_active(row[1], next_enabled)
	if live:
		_confetti(p, 0.3)
	return p

static func settings(host: Control, muted: bool, on_toggle_sound: Callable, on_restart: Callable, on_home: Callable,
		show_restart: bool) -> Control:
	var p := Modal.open(host, "Settings", [UI.SECONDARY], true, 0.0, 0.0, _instant(host), true)
	var ys: Array = p.get_meta("ys")
	var ids := ["sound", "restart", "home"] if show_restart else ["sound", "home"]
	var state := {"muted": muted}
	var gap := 72.0
	var x := 540.0 - (ids.size() * UI.SECONDARY + (ids.size() - 1) * gap) * 0.5 + UI.SECONDARY * 0.5
	for id in ids:
		var cb: Callable = {"sound": on_toggle_sound, "restart": on_restart, "home": on_home}[id]
		var icon: String = {"sound": _sound_icon(muted), "restart": "retry", "home": "home"}[id]
		var b := UI.button(p.content, id.capitalize(), Vector2(x, ys[0] + UI.SECONDARY * 0.5), "orange", icon, func():
			if id == "sound":
				cb.call()
				state["muted"] = not state["muted"]
				var ic: TextureRect = state["button"].get_node("Icon")
				ic.texture = Art.tex("icon_" + _sound_icon(state["muted"]))
			else:
				p.close()
				cb.call())
		if id == "sound":
			state["button"] = b
		x += UI.SECONDARY + gap
	return p

static func level_card(host: Control, level: int, best_stars: int, best_moves: int, par: int, species: Array[String],
		mechanics: Array[String], on_play: Callable, playable: bool) -> Control:
	var per_row := 4
	var rows := ceili(species.size() / float(per_row))
	var heights := [SMALL_STAR + 16.0 + LIMIT_CAP]
	if best_moves > 0:
		heights.append(LINE)
	if rows > 0:
		heights.append(rows * RING + (rows - 1) * RING_GAP)
	if not mechanics.is_empty():
		heights.append(UI.COUNTER_H)
	var p := Modal.open(host, "", heights, true, UI.PRIMARY, 0.0, _instant(host))
	var c := p.content
	var ys: Array = p.get_meta("ys")
	_level_title(c, level, p.panel_rect)
	var g := 0
	# Stars earned so far, each with its move limit under it.
	var limits := [str(par), str(par + 3), "∞"]
	var stars := UI.stars(c, Vector2(540, ys[g] + SMALL_STAR * 0.5), SMALL_STAR, 60.0, best_stars)
	for i in 3:
		var at: Vector2 = stars[i].position + stars[i].size * 0.5
		UI.number(c, limits[i], Rect2(at.x - 80.0, ys[g] + SMALL_STAR + 16.0, 160.0, LIMIT_CAP))
	g += 1
	if best_moves > 0:
		var w := UI.word_width("Best", CAPTION_CAP) + 16.0 + UI.number_width(str(best_moves), 52.0)
		var x := 540.0 - w * 0.5
		UI.word(c, "Best", Vector2(x, ys[g] + LINE * 0.5), CAPTION_CAP, UI.INK, -1)
		UI.number(c, str(best_moves), Rect2(x + w - UI.number_width(str(best_moves), 52.0), ys[g] + (LINE - 52.0) * 0.5,
			UI.number_width(str(best_moves), 52.0) + 2.0, 52.0), UI.INK, 1)
		g += 1
	for i in species.size():
		var in_row := mini(per_row, species.size() - (i / per_row) * per_row)
		var center := Vector2(540.0 + (i % per_row - (in_row - 1) * 0.5) * (RING + RING_GAP),
			ys[g] + RING * 0.5 + (i / per_row) * (RING + RING_GAP))
		_ring(c, species[i], center, _instant(host))
	if rows > 0:
		g += 1
	if not mechanics.is_empty():
		_mechanic_strip(c, mechanics, ys[g])
	UI.set_active(p.actions([["green", "play", on_play]])[0], playable)
	return p

static func goal_info(host: Control, species: String) -> Control:
	var p := Modal.open(host, NAME.get(species, "Crow"), [FRIEND, LINE], true, 0.0, 0.0, _instant(host))
	var c := p.content
	var ys: Array = p.get_meta("ys")
	_bird_view(c, species, Rect2(540.0 - FRIEND * 0.5, ys[0], FRIEND, FRIEND), _instant(host), 0.0)
	_ability_line(c, species, ys[1])
	return p

static func daily_gift(host: Control, reward: int, on_claim: Callable) -> Control:
	var p := Modal.open(host, "Daily Gift", [GIFT_H, REWARD_H], false, UI.PRIMARY, 0.0, _instant(host))
	var c := p.content
	var ys: Array = p.get_meta("ys")
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
	_reward(c, ys[1], reward)
	p.actions([["green", "check", on_claim]])
	return p

static func new_friend(host: Control, species: String, reward: int, on_ok: Callable) -> Control:
	var p := Modal.open(host, "New Friend", [FRIEND, LINE, REWARD_H], false, UI.PRIMARY, 0.0, _instant(host))
	var c := p.content
	var ys: Array = p.get_meta("ys")
	_bird_view(c, species, Rect2(540.0 - FRIEND * 0.5, ys[0], FRIEND, FRIEND), _instant(host), 1.2)
	UI.word(c, NAME.get(species, "Crow"), Vector2(540, ys[1] + LINE * 0.5), 56.0)
	_reward(c, ys[2], reward)
	p.actions([["green", "check", on_ok]])
	return p

static func unlock(host: Control, booster: int, on_ok: Callable) -> Control:
	var p := Modal.open(host, BOOSTER_NAME[booster], [ILLUS, LINE], false, UI.PRIMARY, 0.0, _instant(host))
	var c := p.content
	var ys: Array = p.get_meta("ys")
	var box := Rect2(540.0 - ILLUS * 0.5, ys[0], ILLUS, ILLUS)
	var sparks := [Vector2(-0.78, -0.07), Vector2(0.78, -0.13), Vector2(-0.66, 0.57), Vector2(0.7, 0.55)]
	for i in sparks.size():
		var at: Vector2 = box.get_center() + sparks[i] * ILLUS * 0.5
		_twinkle(UI.art(c, "sparkle", Rect2(at - Vector2(32, 32), Vector2(64, 64))), i * 0.35)
	_breathe(_fit(c, "booster_%s_illus" % BOOSTER_ILLUS[booster], box, true), 0.0)
	UI.word(c, BOOSTER_CAPTION[booster], Vector2(540, ys[1] + LINE * 0.5), CAPTION_CAP)
	p.actions([["green", "check", on_ok]])
	return p

static func world_complete(host: Control, world: int, reward: int, on_ok: Callable) -> Control:
	var arch_h := GATE_W * GATE_CONTENT.size.y / GATE_CONTENT.size.x
	var p := Modal.open(host, "World Complete", [STAR_ROW, arch_h, REWARD_H], false, UI.PRIMARY, 0.0, _instant(host))
	var c := p.content
	var ys: Array = p.get_meta("ys")
	var live := not _instant(host)
	_big_stars(c, ys[0], 3, live, 0.5)
	var arch := Rect2(540.0 - GATE_W * 0.5, ys[1], GATE_W, arch_h)
	UI.art(c, "gate_arch", arch, GATE_CONTENT)
	for i in 3:
		var cx: float = 540.0 + ARCH_BIRD_X[i] * GATE_W
		var box := Rect2(cx - ARCH_BIRD_H * 0.5, arch.end.y - ARCH_FLOOR - ARCH_BIRD_H, ARCH_BIRD_H, ARCH_BIRD_H)
		var bird := _bird_view(c, ARCH_BIRDS[i], box, not live, 0.0)
		if live:
			var tw := bird.create_tween().set_loops()
			tw.tween_interval(1.6 + i * 0.9)
			tw.tween_callback(bird.cheer)
			tw.tween_interval(2.6)
	_reward(c, ys[2], reward)
	p.actions([["green", "check", on_ok]])
	return p

static func world_unlock(host: Control, world: int, on_play: Callable) -> Control:
	var gate_h := GATE_W * GATE_CONTENT.size.y / GATE_CONTENT.size.x
	var p := Modal.open(host, "World Unlocked", [gate_h, LINE], false, UI.PRIMARY, 0.0, _instant(host))
	var c := p.content
	var ys: Array = p.get_meta("ys")
	# The light glow is taller than the arch, so the stage sits in a clip the size of the gate's row.
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
	UI.word(c, WORLD_NAME.get(world, "Bamboo Forest"), Vector2(540, ys[1] + LINE * 0.5), 52.0)
	p.actions([["green", "play", on_play]])
	return p


# ---------------------------------------------------------------- content pieces

## "Level" and the level number as the popup title.
static func _level_title(c: Control, level: int, panel: Rect2) -> void:
	var cap := Modal.TITLE_CAP
	var num_w := UI.number_width(str(level), cap)
	var w := UI.word_width("Level", cap) + cap * 0.35 + num_w
	var x := 540.0 - w * 0.5
	var cy := panel.position.y + Modal.TITLE_TOP + cap * 0.5
	UI.word(c, "Level", Vector2(x, cy), cap, UI.INK, -1)
	UI.number(c, str(level), Rect2(x + w - num_w - 1.0, cy - cap * 0.5, num_w + 2.0, cap), UI.INK, 1)

## Three big stars, the middle one larger and raised; earned ones pop in one after another with a sound.
static func _big_stars(c: Control, y: float, filled: int, animate: bool, delay: float) -> void:
	var specs := [[-200.0, STAR_SIDE, 40.0], [0.0, STAR_MID, 0.0], [200.0, STAR_SIDE, 40.0]]
	for i in 3:
		var s: Array = specs[i]
		var center := Vector2(540.0 + s[0], y + s[2] + s[1] * 0.5)
		UI.icon(c, "star", center, s[1], UI.FADED)
		if i >= filled:
			continue
		var star := UI.icon(c, "star", center, s[1], UI.GOLD)
		if not animate:
			continue
		star.scale = Vector2.ZERO
		var tw := star.create_tween()
		tw.tween_interval(delay + 0.18 * i)
		tw.tween_callback(Sound.play.bind("star_%d" % (i + 1)))
		tw.tween_property(star, "scale", Vector2.ONE * 1.25, 0.16).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
		tw.tween_property(star, "scale", Vector2.ONE, 0.2).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)

## Ink icon and number centred on one line; returns the number box.
static func _icon_line(c: Control, y: float, icon: String, text: String) -> Control:
	var cap := 52.0
	var num_w := UI.number_width(text, cap)
	var w := 56.0 + 16.0 + num_w
	var x := 540.0 - w * 0.5
	UI.icon(c, icon, Vector2(x + 28.0, y + LINE * 0.5), 56.0, UI.INK)
	return UI.number(c, text, Rect2(x + 72.0, y + (LINE - cap) * 0.5, num_w + 120.0, cap), UI.INK, -1)

## Blue reward pill with a coin and "+N" in white; returns [number box, coin centre].
static func _reward(c: Control, y: float, amount: int) -> Array:
	var text := "+%d" % amount
	var cap := 52.0
	var num_w := UI.number_width(text, cap)
	var w := 16.0 + 72.0 + 16.0 + num_w + 40.0
	var pill := UI.shape(c, Rect2(540.0 - w * 0.5, y, w, REWARD_H), "blue", REWARD_H * 0.5, 8.0)
	pill.name = "Reward"
	var coin := Rect2(pill.position.x + 16.0, y + 8.0, 72.0, 72.0)
	UI.art(c, "coin", coin)
	var box := UI.number(c, text, Rect2(coin.end.x + 16.0, y + (REWARD_H - 8.0 - cap) * 0.5, num_w + 120.0, cap), Color.WHITE, -1)
	return [box, coin.get_center()]

## Ability badge and caption of a species, centred on one line.
static func _ability_line(c: Control, species: String, y: float) -> void:
	var caption: String = ABILITY.get(species, "Wakes sleepers")
	var badge := 72.0
	var has_badge := EFFECT.has(species)
	var w := UI.word_width(caption, CAPTION_CAP) + ((badge + 16.0) if has_badge else 0.0)
	var x := 540.0 - w * 0.5
	if has_badge:
		UI.art(c, "gift_coin_" + EFFECT[species], Rect2(x, y + (LINE - badge) * 0.5, badge, badge))
		x += badge + 16.0
	UI.word(c, caption, Vector2(x, y + LINE * 0.5), CAPTION_CAP, UI.INK, -1)

## Cream circle with a live bird and its ability badge at the lower right.
static func _ring(c: Control, species: String, center: Vector2, instant: bool) -> void:
	var r := Rect2(center - Vector2.ONE * RING * 0.5, Vector2.ONE * RING)
	var disc := UI.shape(c, r, "cream", RING * 0.5, 8.0)
	disc.add_theme_stylebox_override("panel", UI._box(Color8(250, 236, 208), UI.RIM["cream"], RING * 0.5, 8.0, false))
	var inner := RING * 0.82
	_bird_view(c, species, Rect2(center - Vector2.ONE * inner * 0.5 + Vector2(0, -6), Vector2.ONE * inner), instant, 0.0)
	var effect: String = EFFECT.get(species, "")
	if not effect.is_empty():
		var b := RING * 0.42
		UI.art(c, "gift_coin_" + effect, Rect2(r.end - Vector2(b, b) * 0.85, Vector2(b, b)))

## Mechanics of the level as pictures in one cream pill, centred.
static func _mechanic_strip(c: Control, mechanics: Array[String], y: float) -> void:
	var size := 64.0
	var gap := 40.0
	var w := 40.0 * 2.0 + mechanics.size() * size + (mechanics.size() - 1) * gap
	var pill := Rect2(540.0 - w * 0.5, y, w, UI.COUNTER_H)
	UI.shape(c, pill, "cream", UI.COUNTER_H * 0.5, 8.0).add_theme_stylebox_override("panel",
		UI._box(Color8(250, 236, 208), UI.RIM["cream"], UI.COUNTER_H * 0.5, 8.0, false))
	for i in mechanics.size():
		var box := Rect2(pill.position.x + 40.0 + i * (size + gap), y + (UI.COUNTER_H - 8.0 - size) * 0.5, size, size)
		var art: String = MECHANIC_ART[mechanics[i]]
		var t := _fit(c, art, box, true)
		if art.begins_with("icon_"):
			t.modulate = UI.INK
		if mechanics[i] in ["sleep", "egg", "crow"]:
			_breathe(t, i * 0.7)


# ---------------------------------------------------------------- motion

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
	rays.create_tween().tween_property(rays, "modulate:a", 0.3, 0.8)
	rays.create_tween().set_loops().tween_property(rays, "rotation", TAU, 28.0).from(0.0)

## Bird flies in along its arc from `offset` away and then keeps living (idle, breathing).
static func _fly_in(bird: BirdView, offset: Vector2, delay: float, duration: float) -> void:
	var target := bird.position
	bird.position = target + offset
	bird.fly_to(target, delay, duration)

## Painted number counting from 0 to `amount`; `pattern` is "%d" or "+%d".
static func _count(box: Control, amount: int, pattern: String, delay: float, duration: float) -> void:
	UI.set_number(box, pattern % 0)
	var tw := box.create_tween()
	tw.tween_interval(delay)
	tw.tween_method(func(v: float): UI.set_number(box, pattern % roundi(v)), 0.0, float(amount), duration)

## About 3 s of confetti falling across the whole popup.
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

## A burst of coins flying from the reward towards the coin counter.
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


# ---------------------------------------------------------------- helpers

static func _sound_icon(muted: bool) -> String:
	return "sound_off" if muted else "sound_on"

static func _instant(host: Node) -> bool:
	var main := host.get_parent()
	return main != null and main.has_method("instant") and main.instant()

## Visible pixels contained in the box, uniform scale, centred (or bottom-aligned with a feet pivot).
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

## Full BirdView (breathing plus idle actions) whose front frame fits the box, feet on the box bottom.
static func _bird_view(c: Control, species: String, box: Rect2, instant: bool, cheer_delay: float) -> BirdView:
	var used := UI.used_rect(species + "_front")
	var k := minf(box.size.x / used.size.x, box.size.y / used.size.y)
	var feet := Vector2(box.get_center().x - (used.get_center().x - BirdView.CANVAS * 0.5) * k,
		box.end.y - (used.end.y - BirdView.FEET_Y) * k)
	var bird := BirdView.new()
	c.add_child(bird)
	bird.position = feet
	bird.setup(Board.bird(0, Board.species_index(species), 0), false, k * BirdView.CANVAS / BirdView.DISPLAY)
	bird.quiet = instant
	if cheer_delay > 0.0 and not instant:
		var tw := bird.create_tween().set_loops()
		tw.tween_interval(cheer_delay)
		tw.tween_callback(bird.cheer)
		tw.tween_interval(3.0)
	return bird


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
