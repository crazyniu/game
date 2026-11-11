extends RefCounted
## Modal popups opened on a host Control (main.popups). Every function returns the popup; callbacks are Callables.
## Layout: docs/design/island-style.md. One cream panel, a localized title, content groups stacked 32 apart, actions in a
## row under the panel. Words are complete locale textures, numbers are glyphs, and controls use the shared icon family.

const EFFECT := {"sunny": "wake", "sky": "reveal", "berry": "encore", "mint": "grow", "cocoa": "scare", "snow": "lightstep",
	"ember": "hatch", "ink": "nightcall", "hoot": "moonbeam"}
const BOOSTER_ID := ["firefly", "sapling", "owl_eye", "morning_bell", "scarecrow", "spring_rain"]
const BOOSTER_ILLUS := ["hint", "sapling", "owl_eye", "bell", "scarecrow", "rain"]
const WORLD_ID := {1: "green", 2: "bamboo", 3: "autumn", 4: "snow", 5: "blossom", 6: "mushroom", 7: "lakeside", 8: "pine",
	9: "rainforest", 10: "wisteria"}
const MECHANIC_ART := {"sleep": "hoot_sleep", "egg": "egg_idle", "crow": "crow_front", "daynight": "mechanic_day_night",
	"twig": "icon_sapling", "growing": "icon_sapling", "swivel": "swivel_knot", "mama": "mama_bow"}

## Content sizes (design pixels).
const STAR_SIDE := 190.0
const STAR_MID := 240.0
const STAR_ROW := 270.0            # height of the star row (the middle star is raised)
const STAR_SPREAD := 240.0         # centre to side star
const SMALL_STAR := 160.0
const LIMIT_CAP := 36.0            # move limits under the level card's stars
const LINE := 100.0                # one text line group (icon + number, or a caption)
const CAPTION_CAP := 54.0
const LINE_CAP := 64.0             # icon + number lines
const REWARD_H := 120.0
const REWARD_W := 380.0
const DROP_ROW := 200.0            # booster drops on the win panel
const DROP_SIDE := 180.0
const DROP_GAP := 48.0
const RING := 180.0
const RING_GAP := 24.0
const HERO := 240.0
const SIDE := 150.0
const FRIEND := 440.0
const ILLUS := 440.0
const GIFT_H := 360.0
const GATE_W := 720.0
const NAME_CAP := 96.0             # gold name heading under a hero picture
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


# ---------------------------------------------------------------- popups

static func win(host: Control, stars: int, moves: int, par: int, reward: int, boosters: Array, on_replay: Callable,
		on_next: Callable, on_home: Callable, next_enabled: bool) -> Control:
	var live := not _instant(host)
	var heights := [STAR_ROW, LINE]
	if reward > 0:
		heights.append(REWARD_H)
	if not boosters.is_empty():
		heights.append(DROP_ROW)
	var items := [["orange", "retry", on_replay], ["green", "play", on_next, "action.next"], ["orange", "home", on_home]]
	var p := Modal.open(host, "popup.win.title", heights, false, Modal.actions_height(items), HERO + 110.0, not live)
	var c := p.content
	var ys: Array = p.get_meta("ys")
	var panel := p.panel_rect
	if live:
		_rays(p, panel.get_center())
	# The hero stands on the title ribbon, its friends on the panel's top corners beside the ribbon tails.
	var ribbon_top := panel.position.y - 70.0
	var hero := _bird_view(c, "sunny", Rect2(540.0 - HERO * 0.5, ribbon_top + 18.0 - HERO, HERO, HERO), not live, 2.4)
	var sides := [_bird_view(c, "sky", Rect2(panel.position.x + 4.0, panel.position.y + 30.0 - SIDE, SIDE, SIDE), not live, 0.0),
		_bird_view(c, "berry", Rect2(panel.end.x - 4.0 - SIDE, panel.position.y + 30.0 - SIDE, SIDE, SIDE), not live, 0.0)]
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
			_coin_burst(p, parts[1], 1.0)
	if not boosters.is_empty():
		_drops(c, ys[-1], boosters, live, 1.5)
	var row := p.actions(items)
	UI.set_active(row[1], next_enabled)
	if live:
		_confetti(p, 0.3)
	return p

## Settings: music and sound switches, Restart and Home while playing, then How to Play and the documents
## (terms of service, credits and licences, contact). `on` holds the callbacks: music, sound,
## restart, home, help; music and sound receive the new "on" state.
static func settings(host: Control, music_on: bool, sound_on: bool, on: Dictionary, show_restart: bool) -> Control:
	var languages := UiText.languages()
	var lang_rows := ceili(languages.size() / 2.0)
	var heights := [ROW_H, ROW_H, ROW_H, ROW_CAP * 2.0, lang_rows * LANG_H + (lang_rows - 1) * 20.0]
	if show_restart:
		heights.append(PILL_H)
	heights.append(UI.PLAY_SIZE.y)
	var p := Modal.open(host, "popup.settings.title", heights, true, 0.0, 0.0, _instant(host), true)
	var c := p.content
	var ys: Array = p.get_meta("ys")
	var left := 132.0
	var right := 948.0
	var rows := [["settings.music", "pic_music", music_on, func(v: bool): on["music"].call(v)],
		["settings.sound", "pic_sound_on", sound_on, func(v: bool): on["sound"].call(v)],
		# Vibration: the state lives in Sound.vibration (profile.vibration).
		["settings.vibration", "pic_vibration", Sound.vibration, func(v: bool):
			Sound.vibration = v
			if on.has("vibration"):
				on["vibration"].call(v)
			Sound.vibrate(Sound.PULSE_LAND)]]
	for g in rows.size():
		var item: Array = rows[g]
		var y: float = ys[g]
		Island.nine(c, "island_row_card", Rect2(left, y, right - left, ROW_H), Vector4(40, 40, 40, 40))
		UI.art(c, item[1], Rect2(left + 28.0, y + (ROW_H - 96.0) * 0.5, 96, 96))
		_row_label(c, item[0], Vector2(left + 148.0, y + ROW_H * 0.5))
		_toggle(c, Rect2(right - 28.0 - TOGGLE.x, y + (ROW_H - TOGGLE.y) * 0.5, TOGGLE.x, TOGGLE.y), item[2], item[3])
	# Language: a centred heading, then one chip per language (each named in its own words); the current one is green.
	var g := rows.size()
	var lw := UI.label_width("settings.language", ROW_CAP)
	UI.label(c, "settings.language", Rect2(540.0 - lw * 0.5, ys[g], lw, ROW_CAP * 2.0), ROW_CAP)
	for side in [-1.0, 1.0]:
		var leaf := UI.art(c, "pic_sparkle", Rect2(540.0 + side * (lw * 0.5 + 56.0) - 24.0, ys[g] + ROW_CAP - 24.0, 48, 48))
		leaf.modulate = Color(1, 1, 1, 0.9)
	g += 1
	var chip_w := (right - left - 20.0) * 0.5
	for i in languages.size():
		var code: String = languages[i]
		var r := Rect2(left + (i % 2) * (chip_w + 20.0), ys[g] + (i / 2) * (LANG_H + 20.0), chip_w, LANG_H)
		var current := code == UiText.locale()
		var chip := UI.pill(c, "Language_" + code, r, "green" if current else "orange", "", func(): on["language"].call(code))
		var word := TextureRect.new()
		word.texture = UiText.foreign_label(code, "settings.language_name")
		word.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
		word.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
		word.mouse_filter = Control.MOUSE_FILTER_IGNORE
		word.modulate = Color.WHITE if current else UI.INK
		UI.place(word, Rect2(r.size.x * 0.15, r.size.y * 0.24, r.size.x * 0.7, r.size.y * 0.46))
		chip.add_child(word)
	g += 1
	var wide := (right - left - 24.0) * 0.5
	if show_restart:
		UI.pill(c, "Restart", Rect2(left, ys[g], wide, PILL_H), "orange", "settings.restart",
			func(): p.invoke(on["restart"]), "", 48.0)
		UI.pill(c, "Home", Rect2(left + wide + 24.0, ys[g], wide, PILL_H), "green", "settings.home",
			func(): p.invoke(on["home"]), "", 48.0)
		g += 1
	UI.pill(c, "Help", Rect2(Vector2(540.0 - UI.PLAY_SIZE.x * 0.5, ys[g]), UI.PLAY_SIZE), "green", "settings.help",
		func(): p.invoke(on["help"]), "help", 60.0)
	return p

const ROW_H := 140.0
const LANG_H := 110.0
const TOGGLE := Vector2(220.0, 94.0)
const PILL_H := 120.0
const ROW_CAP := 56.0

## A settings row word, left-aligned at `at` (its left edge, vertical centre), like the sketch.
static func _row_label(parent: Control, id: String, at: Vector2) -> void:
	var w := UI.label_width(id, ROW_CAP)
	UI.label(parent, id, Rect2(at.x, at.y - ROW_CAP, w, ROW_CAP * 2.0), ROW_CAP)

## Painted on/off switch; tapping flips it and reports the new state.
static func _toggle(parent: Control, r: Rect2, value: bool, on_change: Callable) -> Button:
	var b := Button.new()
	b.name = "Toggle"
	b.focus_mode = Control.FOCUS_NONE
	var empty := StyleBoxEmpty.new()
	for state in ["normal", "hover", "pressed", "disabled", "focus", "hover_pressed"]:
		b.add_theme_stylebox_override(state, empty)
	UI.place(b, r)
	parent.add_child(b)
	var face := TextureRect.new()
	face.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	face.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	face.mouse_filter = Control.MOUSE_FILTER_IGNORE
	face.size = r.size
	b.add_child(face)
	var state := {"on": value}
	face.texture = Art.tex("island_toggle_on" if value else "island_toggle_off")
	b.pressed.connect(func():
		state["on"] = not state["on"]
		face.texture = Art.tex("island_toggle_on" if state["on"] else "island_toggle_off")
		Sound.play("ui_tap")
		on_change.call(state["on"]))
	return b

## A document page (terms of service, credits, contact): the baked text in a scrolling column.
static func document(host: Control, id: String) -> Control:
	var page: Texture2D = UiText.document_texture(id)
	var column := Modal.WIDTH - 144.0
	var view_h := minf(1180.0, page.get_height() * column / float(page.get_width()) + 8.0)
	var p := Modal.open(host, "doc.%s.title" % id, [view_h], true, 0.0, 0.0, _instant(host), true)
	var ys: Array = p.get_meta("ys")
	var clip := Control.new()
	clip.name = "Scroll"
	clip.clip_contents = true
	UI.place(clip, Rect2(p.panel_rect.position.x + 72.0, ys[0], p.panel_rect.size.x - 144.0, view_h))
	p.content.add_child(clip)
	var text := TextureRect.new()
	text.texture = UiText.document_texture(id)
	text.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	text.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var k := clip.size.x / float(text.texture.get_width())
	text.size = Vector2(text.texture.get_size()) * k
	text.modulate = UI.INK
	clip.add_child(text)
	var span := maxf(0.0, text.size.y - view_h)
	clip.gui_input.connect(func(e: InputEvent):
		var dy := 0.0
		if e is InputEventScreenDrag or (e is InputEventMouseMotion and e.button_mask & MOUSE_BUTTON_MASK_LEFT):
			dy = e.relative.y
		elif e is InputEventMouseButton and e.pressed and e.button_index in [MOUSE_BUTTON_WHEEL_UP, MOUSE_BUTTON_WHEEL_DOWN]:
			dy = 80.0 if e.button_index == MOUSE_BUTTON_WHEEL_UP else -80.0
		text.position.y = clampf(text.position.y + dy, -span, 0.0))
	return p

static func level_card(host: Control, level: int, best_stars: int, best_moves: int, par: int, species: Array[String],
		mechanics: Array[String], on_play: Callable, playable: bool) -> Control:
	var per_row := 4
	var rows := ceili(species.size() / float(per_row))
	var heights := [SMALL_STAR, LINE]
	if rows > 0:
		heights.append(rows * RING + (rows - 1) * RING_GAP)
	if not mechanics.is_empty():
		heights.append(UI.COUNTER_H)
	var p := Modal.open(host, "popup.level.title", heights, true, UI.PLAY_SIZE.y, 0.0, _instant(host), false, str(level))
	var c := p.content
	var ys: Array = p.get_meta("ys")
	var g := 0
	# Stars earned so far, then the move target for three stars (moves picture and number, no words).
	UI.stars(c, Vector2(540, ys[g] + SMALL_STAR * 0.5), SMALL_STAR, 40.0, best_stars)
	g += 1
	_icon_line(c, ys[g], "moves", str(par))
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
	var p := Modal.open(host, "bird.%s.name" % species, [FRIEND, ABILITY_H], true, 0.0, 0.0, _instant(host))
	var c := p.content
	var ys: Array = p.get_meta("ys")
	_halo(c, Rect2(540.0 - FRIEND * 0.5, ys[0], FRIEND, FRIEND))
	_bird_view(c, species, Rect2(540.0 - FRIEND * 0.5 + 40.0, ys[0] + 30.0, FRIEND - 80.0, FRIEND - 60.0), _instant(host), 0.0)
	_ability_line(c, species, ys[1])
	return p

## Daily lucky draw: a 3x3 board whose eight outer cells hold prizes and whose centre is the gold chest. A light runs
## round the ring, slows down and stops on the prize (coins, or a booster with its count). The star row shows today's
## draws: filled = left, hollow = unlocked by playing more days in a row. `draw` performs one draw (grants the prize)
## and returns {"cell", "coins", "booster", "count"} like Progress.draw, or {} when none is left. When the light stops,
## a rewards popup over the board names what was won; the next draw is possible once it is dismissed.
const DRAW_CELL := 246.0
const DRAW_GAP := 18.0
const DRAW_SPOTS := [Vector2i(0, 0), Vector2i(1, 0), Vector2i(2, 0), Vector2i(2, 1), Vector2i(2, 2), Vector2i(1, 2), Vector2i(0, 2), Vector2i(0, 1)]

static func lucky_draw(host: Control, spins: int, slots: int, draw: Callable) -> Control:
	var board_w := DRAW_CELL * 3.0 + DRAW_GAP * 2.0
	var star := 84.0
	var p := Modal.open(host, "draw.title", [star, board_w, UI.PLAY_SIZE.y], true, 0.0, 0.0, _instant(host))
	var c := p.content
	var ys: Array = p.get_meta("ys")
	var instant := _instant(host)
	# Draws left today.
	var stars := []
	for i in slots:
		var x := 540.0 + (i - (slots - 1) * 0.5) * (star + 16.0)
		var s := UI.art(c, "island_star_full" if i < spins else "island_star_empty", Rect2(x - star * 0.5, ys[0], star, star))
		s.pivot_offset = s.size * 0.5
		stars.append(s)
	# The board.
	var left := 540.0 - board_w * 0.5
	var cards := []
	var lights := []
	for i in 8:
		var r := Rect2(left + DRAW_SPOTS[i].x * (DRAW_CELL + DRAW_GAP), ys[1] + DRAW_SPOTS[i].y * (DRAW_CELL + DRAW_GAP), DRAW_CELL, DRAW_CELL)
		var card := Control.new()
		card.mouse_filter = Control.MOUSE_FILTER_IGNORE
		UI.place(card, r)
		c.add_child(card)
		Island.nine(card, "island_grid_card", Rect2(Vector2.ZERO, r.size), Vector4(48, 48, 48, 48))
		card.name = "Prize%d" % i
		card.pivot_offset = r.size * 0.5
		var lit := UI.shape(card, Rect2(Vector2.ZERO, r.size), "gold", 36.0, 8.0)
		lit.modulate.a = 0.0
		lights.append(lit)
		var prize: Array = Progress.DRAW_PRIZES[i]
		if prize[0] > 0:
			UI.art(card, "coin", Rect2(r.size.x * 0.5 - 62.0, 26, 124, 124))
			UI.number(card, str(prize[0]), Rect2(14, 160, r.size.x - 28.0, 52), UI.INK)
		else:
			# A wrapped gift until the light stops on it, then the booster it really holds.
			var gift := Control.new()
			gift.name = "Gift"
			gift.mouse_filter = Control.MOUSE_FILTER_IGNORE
			card.add_child(gift)
			UI.art(gift, "gift_box_base", Rect2(r.size.x * 0.5 - 70.0, 20, 140, 140))
			UI.art(gift, "gift_box_lid", Rect2(r.size.x * 0.5 - 70.0, -2, 140, 140))
			var count := UI.number(card, "x%d" % prize[1], Rect2(14, 160, r.size.x - 28.0, 52), UI.INK)
			count.name = "GiftCount"
		cards.append(card)
	var mid := Rect2(left + DRAW_CELL + DRAW_GAP, ys[1] + DRAW_CELL + DRAW_GAP, DRAW_CELL, DRAW_CELL)
	var centre := UI.shape(c, mid, "gold", 36.0, 8.0)
	centre.name = "Chest"
	centre.pivot_offset = mid.size * 0.5
	var chest := UI.art(centre, "pic_chest", Rect2(14, 14, DRAW_CELL - 28.0, DRAW_CELL - 28.0))
	if not instant:
		_breathe(chest, 0.0)
	var state := {"left": spins, "busy": false, "at": 0}
	var go := UI.pill(c, "Draw", Rect2(Vector2(540.0 - UI.PLAY_SIZE.x * 0.5, ys[2]), UI.PLAY_SIZE), "green", "draw.go",
		func(): pass, "", 52.0)
	UI.set_active(go, spins > 0)
	var run := func() -> void:
		if state["busy"] or state["left"] <= 0:
			return
		var got: Dictionary = draw.call()
		if got.is_empty():
			return
		var target: int = got["cell"]
		state["busy"] = true
		UI.set_active(go, false)
		state["left"] -= 1
		var used: Control = stars[state["left"]]
		var tw := used.create_tween()
		tw.tween_property(used, "scale", Vector2.ONE * 1.4, 0.12)
		tw.tween_callback(func(): (used as TextureRect).texture = Art.tex("island_star_empty"))
		tw.tween_property(used, "scale", Vector2.ONE, 0.2)
		var steps: int = 24 + posmod(target - int(state["at"]), 8)
		var at: int = state["at"]
		for n in steps:
			var prev: int = at
			at = posmod(at + 1, 8)
			lights[prev].modulate.a = 0.0
			lights[at].modulate.a = 1.0
			cards[at].scale = Vector2.ONE * 1.06
			create_step_tween(cards[at], not instant)
			Sound.play("ui_tap", 1.0 + 0.03 * n if n < 24 else 1.7)
			if not instant:
				var t := float(n) / float(steps - 1)
				await c.get_tree().create_timer(0.045 + 0.3 * t * t * t).timeout
		state["at"] = at
		Sound.play("reward")
		if int(got.get("booster", -1)) >= 0:
			var holder := cards[at] as Control
			holder.get_node("Gift").queue_free()
			_fit(holder, "booster_%s_illus" % BOOSTER_ILLUS[got["booster"]], Rect2((holder.size.x - 112.0) * 0.5, 12.0, 112.0, 112.0), true)
		if not instant:
			var pop: Tween = (cards[at] as Control).create_tween()
			pop.tween_property(cards[at], "scale", Vector2.ONE * 1.22, 0.16).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
			pop.tween_property(cards[at], "scale", Vector2.ONE * 1.1, 0.25)
			await c.get_tree().create_timer(0.7).timeout
		if not is_instance_valid(c):
			return
		# Name the win in the rewards popup; the board steps aside until it is dismissed, then the light clears.
		var kinds := []
		if int(got.get("booster", -1)) >= 0:
			for k in int(got.get("count", 0)):
				kinds.append(got["booster"])
		p.content.visible = false
		rewards(host, "draw.title", int(got.get("coins", 0)), kinds, func():
			if not is_instance_valid(c):
				return
			p.content.visible = true
			cards[at].scale = Vector2.ONE
			lights[at].modulate.a = 0.0
			state["busy"] = false
			UI.set_active(go, state["left"] > 0), "coin")
	(go as Button).pressed.connect(run)
	return p

static func create_step_tween(card: Control, animate: bool) -> void:
	if not animate:
		return
	var tw := card.create_tween()
	tw.tween_property(card, "scale", Vector2.ONE, 0.18).set_ease(Tween.EASE_OUT)

const CHAPTERS_PER_PAGE := 4
const CHAPTER_TAG := Vector2(216.0, 76.0)

## Chapter browser: one row per world (painting, "Chapter N", level range and stars, then a worded pill). Tapping an unlocked row
## calls `on_pick(world)`; a locked row is dimmed and does nothing.
static func chapters(host: Control, p: Profile, on_pick: Callable, page := -1) -> Control:
	var row_h := 236.0
	var gap := 22.0
	if page < 0:   # opens on the page holding the player's current chapter
		var next := Progress.next_level(p)
		page = (Progress.world_of_level(next if next > 0 else Levels.COUNT) - 1) / CHAPTERS_PER_PAGE
	var last_page := (Levels.WORLDS - 1) / CHAPTERS_PER_PAGE
	var m := Modal.open(host, "map.chapters", [row_h * CHAPTERS_PER_PAGE + gap * (CHAPTERS_PER_PAGE - 1.0)], true,
		Modal.SECONDARY_PILL.y, 0.0, _instant(host), true)
	var c := m.content
	var ys: Array = m.get_meta("ys")
	var left: float = m.panel_rect.position.x + Modal.INSET
	var width: float = Modal.WIDTH - Modal.INSET * 2.0
	for slot in CHAPTERS_PER_PAGE:
		var i := page * CHAPTERS_PER_PAGE + slot
		if i >= Levels.WORLDS:
			break
		var world := i + 1
		var r := Rect2(left, ys[0] + slot * (row_h + gap), width, row_h)
		var open := Progress.is_world_unlocked(p, world)
		var done := Progress.is_world_complete(p, world)
		var card := UI.shape(c, r, "cream", 40.0, 8.0)
		card.name = "Chapter%d" % world
		var hit := UI.hit_area(card, Rect2(Vector2.ZERO, r.size), func(): m.invoke(on_pick.bind(world)))
		hit.disabled = not open
		var body := Control.new()
		body.name = "Body"
		body.size = r.size
		body.mouse_filter = Control.MOUSE_FILTER_IGNORE
		card.add_child(body)
		var thumb := TextureRect.new()
		var at := AtlasTexture.new()
		at.atlas = Art.tex("map_world_w%d" % world)
		at.region = Rect2(60, 180, 960, 604)
		thumb.texture = at
		thumb.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
		thumb.stretch_mode = TextureRect.STRETCH_SCALE
		thumb.mouse_filter = Control.MOUSE_FILTER_IGNORE
		thumb.position = Vector2(30, 28)
		thumb.size = Vector2(246, 174)
		var clip := Control.new()
		clip.clip_contents = true
		clip.position = Vector2.ZERO
		clip.size = r.size - Vector2(0, 8)
		clip.mouse_filter = Control.MOUSE_FILTER_IGNORE
		body.add_child(clip)
		clip.add_child(thumb)
		var x := 300.0
		var word_w := UI.label_width("chapter.word", 48.0)
		UI.label(body, "chapter.word", Rect2(x, 24, word_w, 66), 48.0)
		UI.number(body, str(world), Rect2(x + word_w + 4.0, 34, UI.number_width(str(world), 48.0) + 4.0, 48), UI.INK, -1)
		# "Levels 1-10" on the second line; under it a worded pill (no round buttons in popups) with the stars beside
		# it: Replay on a finished chapter, Play on the open one, a lock on a closed one.
		var brown := Color8(140, 98, 58)
		var lw := UI.label_width("chapter.levels", 30.0)
		UI.label(body, "chapter.levels", Rect2(x, 90, lw, 44), 30.0, brown)
		var levels := "%d-%d" % [i * 10 + 1, i * 10 + 10]
		var levels_w := UI.number_width(levels, 28.0) + 4.0
		UI.number(body, levels, Rect2(x + lw + 10.0, 98, levels_w, 28), brown, -1)
		var earned := 0
		for level in range(i * 10 + 1, i * 10 + 11):
			earned += p.best_stars(level)
		var tag := Rect2(Vector2(x, 140.0), CHAPTER_TAG)
		var sx := tag.end.x + 20.0
		var mid := tag.get_center().y
		UI.art(body, "pic_star", Rect2(sx, mid - 22.0, 40, 40))
		var text := "%d/30" % earned
		UI.number(body, text, Rect2(sx + 44.0, mid - 16.0, UI.number_width(text, 28.0) + 4.0, 28), UI.INK, -1)
		if not open:
			UI.icon(body, "lock", Vector2(x + 32.0, tag.get_center().y), 56.0, UI.INK)
			body.modulate.a = UI.DISABLED_ALPHA
		elif done:
			UI.pill(body, "ReplayTag", tag, "orange", "action.replay", func(): m.invoke(on_pick.bind(world)), "", 30.0)
		else:
			UI.pill(body, "PlayTag", tag, "green", "action.play", func(): m.invoke(on_pick.bind(world)), "", 30.0)
	# Worded page pills under the panel (no round buttons in popups); a closed end is dimmed.
	var turn := func(to: int):
		var parent := m.get_parent()
		m.queue_free()
		chapters(parent, p, on_pick, to)
	var row_y: float = m.get_meta("actions_y")
	var back := UI.pill(c, "PagePrevious", Rect2(540.0 - Modal.SECONDARY_PILL.x - 28.0, row_y, Modal.SECONDARY_PILL.x, Modal.SECONDARY_PILL.y), "orange",
		"action.back", func(): turn.call(page - 1), "", 44.0)
	var forward := UI.pill(c, "PageNext", Rect2(540.0 + 28.0, row_y, Modal.SECONDARY_PILL.x, Modal.SECONDARY_PILL.y), "orange",
		"action.next", func(): turn.call(page + 1), "", 44.0)
	UI.set_active(back, page > 0)
	UI.set_active(forward, page < last_page)
	return m

## Star chest: the chest, stars collected towards it on a painted bar, and Open when one is ready.
## With `on_play` a second green Play sits under Open (user sketch), so the player can go on playing for stars.
static func chest(host: Control, progress: int, ready: int, on_open: Callable, on_play := Callable()) -> Control:
	var actions_h := UI.PLAY_SIZE.y * (2.0 if on_play.is_valid() else 1.0) + (ACTION_STACK if on_play.is_valid() else 0.0)
	var p := Modal.open(host, "chest.title", [ILLUS, LINE, 110.0], true, actions_h, 0.0, _instant(host))
	var c := p.content
	var ys: Array = p.get_meta("ys")
	var box := Rect2(540.0 - ILLUS * 0.5, ys[0], ILLUS, ILLUS)
	# Sketch: the chest sits in a warm glow with sparkles around it.
	var glow := UI.image(c, "glow_soft", box.grow(90.0))
	glow.modulate = Color(1.0, 0.86, 0.4, 0.9)
	var sparks := [Vector2(-0.86, -0.3), Vector2(0.86, -0.42), Vector2(-0.7, 0.4), Vector2(0.78, 0.3)]
	for i in sparks.size():
		var at: Vector2 = box.get_center() + sparks[i] * ILLUS * 0.5
		var spark := UI.art(c, "sparkle", Rect2(at - Vector2(30, 30), Vector2(60, 60)))
		spark.pivot_offset = Vector2(30, 30)
		if not _instant(host):
			_twinkle(spark, i * 0.35)
	var art := _fit(c, "pic_chest_open" if ready > 0 else "pic_chest", box, true)
	_breathe(art, 0.0)
	UI.label(c, "chest.line", Rect2(132.0, ys[1], 816.0, LINE), CAPTION_CAP * 1.15)
	var fill := clampf(float(progress % Progress.CHEST_STARS if ready == 0 else Progress.CHEST_STARS) / Progress.CHEST_STARS, 0.0, 1.0)
	var text := "%d/%d" % [mini(progress, Progress.CHEST_STARS) if ready == 0 else Progress.CHEST_STARS, Progress.CHEST_STARS]
	Island.progress_bar(c, Rect2(160.0, ys[2], 760.0, 110.0), fill, text)
	var open: Button = p.actions([["green", "play", on_open, "chest.open"]])[0]
	UI.set_active(open, ready > 0)
	if on_play.is_valid():
		var at := Vector2(540.0 - UI.PLAY_SIZE.x * 0.5, open.position.y + UI.PLAY_SIZE.y + ACTION_STACK)
		UI.pill(c, "Play", Rect2(at, UI.PLAY_SIZE), "green", "action.play", func(): p.invoke(on_play), "", 52.0)
	return p

const ACTION_STACK := 28.0
const REWARD_TILE := 250.0

## What a check-in or chest gave: coins and boosters, in a row; OK closes.
## `picture` stands in when no booster is won (coins only).
static func rewards(host: Control, title_id: String, coins: int, boosters: Array, on_ok: Callable, picture := "pic_chest_open") -> Control:
	var p := Modal.open(host, title_id, [ILLUS, REWARD_TILE] if coins > 0 or not boosters.is_empty() else [ILLUS], false,
		UI.PLAY_SIZE.y, 0.0, _instant(host))
	var c := p.content
	var ys: Array = p.get_meta("ys")
	# The opened chest (or the given picture) in its glow, then one tile per prize: coins first, then each booster.
	var hero := Rect2(540.0 - ILLUS * 0.5, ys[0], ILLUS, ILLUS)
	_halo(c, hero)
	_breathe(_fit(c, picture if picture != "coin" else "pic_chest_open", hero.grow(-30.0), true), 0.0)
	var tiles := []
	if coins > 0:
		tiles.append(["coins", coins])
	for kind in boosters:
		tiles.append(["booster", kind])
	var n := tiles.size()
	var tw := minf(250.0, (816.0 - (n - 1) * 20.0) / maxf(1.0, n))
	for i in n:
		var x := 540.0 + (i - (n - 1) * 0.5) * (tw + 20.0)
		var r := Rect2(x - tw * 0.5, ys[1], tw, REWARD_TILE)
		Island.nine(c, "island_grid_card", r, Vector4(48, 48, 48, 48))
		if tiles[i][0] == "coins":
			_fit(c, "coin", Rect2(r.position.x + tw * 0.2, r.position.y + 24.0, tw * 0.6, REWARD_TILE * 0.5), true)
			UI.number(c, "+%d" % tiles[i][1], Rect2(r.position.x + 12.0, r.end.y - 84.0, tw - 24.0, 56.0), UI.INK)
		else:
			_breathe(_fit(c, "booster_%s_illus" % BOOSTER_ILLUS[tiles[i][1]], Rect2(r.position.x + 20.0, r.position.y + 20.0,
				tw - 40.0, REWARD_TILE - 50.0), true), i * 0.4)
			Island.plus_badge(c, r.end - Vector2(40, 40), 1)
	p.actions([["green", "play", on_ok, "action.ok"]])
	return p

static func new_friend(host: Control, species: String, reward: int, on_ok: Callable) -> Control:
	var p := Modal.open(host, "popup.new_friend.title", [FRIEND, NAME_CAP * 1.3, ABILITY_H, REWARD_H], false,
		UI.PLAY_SIZE.y, 0.0, _instant(host))
	var c := p.content
	var ys: Array = p.get_meta("ys")
	_halo(c, Rect2(540.0 - FRIEND * 0.5, ys[0], FRIEND, FRIEND))
	_bird_view(c, species, Rect2(540.0 - FRIEND * 0.5 + 30.0, ys[0] + 20.0, FRIEND - 60.0, FRIEND - 40.0), _instant(host), 1.2)
	_name_heading(c, "bird.%s.name" % species, ys[1])
	_ability_line(c, species, ys[2])
	_reward(c, ys[3], reward)
	p.actions([["green", "check", on_ok]])
	return p

static func unlock(host: Control, booster: int, on_ok: Callable) -> Control:
	var p := Modal.open(host, "booster.%s.name" % BOOSTER_ID[booster], [ILLUS, LINE], false, UI.PLAY_SIZE.y, 0.0, _instant(host))
	var c := p.content
	var ys: Array = p.get_meta("ys")
	var box := Rect2(540.0 - ILLUS * 0.5, ys[0], ILLUS, ILLUS)
	var sparks := [Vector2(-0.78, -0.07), Vector2(0.78, -0.13), Vector2(-0.66, 0.57), Vector2(0.7, 0.55)]
	for i in sparks.size():
		var at: Vector2 = box.get_center() + sparks[i] * ILLUS * 0.5
		_twinkle(UI.art(c, "sparkle", Rect2(at - Vector2(32, 32), Vector2(64, 64))), i * 0.35)
	_halo(c, box)
	_breathe(_fit(c, "booster_%s_illus" % BOOSTER_ILLUS[booster], box.grow(-40.0), true), 0.0)
	UI.label(c, "booster.%s.description" % BOOSTER_ID[booster], Rect2(132, ys[1], 816, LINE), CAPTION_CAP)
	p.actions([["green", "check", on_ok]])
	return p

static func world_complete(host: Control, world: int, reward: int, on_ok: Callable) -> Control:
	var arch_h := GATE_W * GATE_CONTENT.size.y / GATE_CONTENT.size.x
	var p := Modal.open(host, "popup.world_complete.title", [arch_h, STAR_ROW, REWARD_H], false, UI.PLAY_SIZE.y, 0.0, _instant(host))
	var c := p.content
	var ys: Array = p.get_meta("ys")
	var live := not _instant(host)
	_big_stars(c, ys[1], 3, live, 0.5)
	var arch := Rect2(540.0 - GATE_W * 0.5, ys[0], GATE_W, arch_h)
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
	var p := Modal.open(host, "popup.world_unlock.title", [gate_h, NAME_CAP * 1.3], false, UI.PLAY_SIZE.y, 0.0, _instant(host))
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
	_name_heading(c, "world.%s.name" % WORLD_ID[world], ys[1])
	p.actions([["green", "play", on_play]])
	return p


# ---------------------------------------------------------------- content pieces

## Three big stars, the middle one larger and raised; earned ones pop in one after another with a sound.
static func _big_stars(c: Control, y: float, filled: int, animate: bool, delay: float) -> void:
	var specs := [[-STAR_SPREAD, STAR_SIDE, 50.0], [0.0, STAR_MID, 0.0], [STAR_SPREAD, STAR_SIDE, 50.0]]
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
	var cap := LINE_CAP
	var num_w := UI.number_width(text, cap)
	var w := 80.0 + 20.0 + num_w
	var x := 540.0 - w * 0.5
	UI.icon(c, icon, Vector2(x + 40.0, y + LINE * 0.5), 80.0, UI.INK)
	return UI.number(c, text, Rect2(x + 100.0, y + (LINE - cap) * 0.5, num_w, cap), UI.INK, -1)

## Gold reward bar (the feather coin on a gold face, like the shop prices) with "+N" in dark ink; returns
## [number box, coin centre].
static func _reward(c: Control, y: float, amount: int) -> Array:
	var bar := Island.reward_bar(c, Rect2(540.0 - REWARD_W * 0.5, y, REWARD_W, REWARD_H), "+%d" % amount)
	return [bar.get_node("Number"), bar.position + Vector2(bar.size.y * 0.5, bar.size.y * 0.5)]

## The win's booster drops, one picture per kind with a green "+N" badge; live, each pops in after `delay`.
static func _drops(c: Control, y: float, boosters: Array, live: bool, delay: float) -> void:
	var kinds := []
	var counts := {}
	for kind in boosters:
		if not counts.has(kind):
			kinds.append(kind)
			counts[kind] = 0
		counts[kind] += 1
	var n := kinds.size()
	for i in n:
		var x := 540.0 + (i - (n - 1) * 0.5) * (DROP_SIDE + DROP_GAP)
		var holder := Control.new()
		holder.name = "Drop%d" % kinds[i]
		holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
		UI.place(holder, Rect2(x - DROP_SIDE * 0.5, y + (DROP_ROW - DROP_SIDE) * 0.5, DROP_SIDE, DROP_SIDE))
		holder.pivot_offset = holder.size * 0.5
		c.add_child(holder)
		_fit(holder, "booster_%s_illus" % BOOSTER_ILLUS[kinds[i]], Rect2(Vector2.ZERO, holder.size), true)
		Island.plus_badge(holder, Vector2(DROP_SIDE - 26.0, DROP_SIDE - 26.0), counts[kinds[i]])
		if live:
			holder.scale = Vector2.ZERO
			var tw := holder.create_tween()
			tw.tween_interval(delay + i * 0.18)
			tw.tween_callback(func(): Sound.play("reward"))
			tw.tween_property(holder, "scale", Vector2.ONE, 0.35).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
			tw.tween_callback(func(): _breathe(holder, i * 0.4))

## Ability badge and caption of a species, centred on one line.
static func _ability_line(c: Control, species: String, y: float) -> void:
	var label_id := "bird.%s.ability" % species
	var row := Rect2(540.0 - ROW_W * 0.5, y, ROW_W, ABILITY_H)
	Island.nine(c, "island_row_card", row, Vector4(40, 40, 40, 40))
	var badge := 84.0
	var has_badge := EFFECT.has(species)
	var lead := (badge + 20.0) if has_badge else 0.0
	var label_w := minf(UI.label_width(label_id, CAPTION_CAP), ROW_W - 80.0 - lead)
	var w := label_w + lead
	var x := 540.0 - w * 0.5
	if has_badge:
		UI.art(c, "gift_coin_" + EFFECT[species], Rect2(x, y + (ABILITY_H - badge) * 0.5, badge, badge))
	UI.label(c, label_id, Rect2(x + lead, y, label_w, ABILITY_H), CAPTION_CAP)

const ROW_W := 816.0
const ABILITY_H := 128.0

## Cream circle with a live bird and its ability badge at the lower right.
static func _ring(c: Control, species: String, center: Vector2, instant: bool) -> void:
	var r := Rect2(center - Vector2.ONE * RING * 0.5, Vector2.ONE * RING)
	UI.shape(c, r, "cream", RING * 0.5, 8.0)
	var inner := RING * 0.82
	_bird_view(c, species, Rect2(center - Vector2.ONE * inner * 0.5 + Vector2(0, -6), Vector2.ONE * inner), instant, 0.0)
	var effect: String = EFFECT.get(species, "")
	if not effect.is_empty():
		var b := RING * 0.42
		UI.art(c, "gift_coin_" + effect, Rect2(r.end - Vector2(b, b) * 0.85, Vector2(b, b)))

## Mechanics of the level as pictures in one cream pill, centred.
static func _mechanic_strip(c: Control, mechanics: Array[String], y: float) -> void:
	var size := 64.0
	var gap := 32.0
	var w := 40.0 * 2.0 + mechanics.size() * size + (mechanics.size() - 1) * gap
	var pill := Rect2(540.0 - w * 0.5, y, w, UI.COUNTER_H)
	UI.shape(c, pill, "cream", UI.COUNTER_H * 0.5, 8.0)
	for i in mechanics.size():
		var box := Rect2(pill.position.x + 40.0 + i * (size + gap), y + (UI.COUNTER_H - 8.0 - size) * 0.5, size, size)
		var art: String = MECHANIC_ART[mechanics[i]]
		var t := _fit(c, art, box, true)
		if art.begins_with("icon_"):
			t.modulate = UI.INK
		if mechanics[i] in ["sleep", "egg", "crow", "mama"]:
			_breathe(t, i * 0.7)


## Soft warm glow behind a hero picture, slowly pulsing.
static func _halo(c: Control, box: Rect2) -> void:
	var glow := UI.image(c, "glow_soft", box.grow(box.size.x * 0.12))
	glow.modulate = Color(1.0, 0.86, 0.42, 0.85)
	glow.pivot_offset = glow.size * 0.5
	var tw := glow.create_tween().set_loops()
	tw.tween_property(glow, "modulate:a", 0.55, 1.6).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	tw.tween_property(glow, "modulate:a", 0.9, 1.6).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)

## A gold painted name (bird, world) centred on its own line; falls back to the ink word when no heading exists.
static func _name_heading(c: Control, id: String, y: float) -> void:
	var h := NAME_CAP * 1.3
	var w := minf(816.0, UI.heading_width(id, NAME_CAP))
	UI.heading(c, id, Rect2(540.0 - w * 0.5, y + h * 0.5 - NAME_CAP, w, NAME_CAP * 2.0), NAME_CAP)


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

## Coins rise above the reward and settle back into its visible coin, without crossing unrelated HUD controls.
static func _coin_burst(p: Control, center: Vector2, delay: float) -> void:
	for i in 7:
		var coin := UI.image(p, "coin", Rect2(Vector2.ZERO, Vector2(60, 60)))
		coin.modulate.a = 0.0
		var tw := coin.create_tween()
		tw.tween_interval(delay + i * 0.07)
		tw.tween_method(func(t: float):
			var at := center + Vector2((i - 3) * 30.0 * sin(t * PI), -(140.0 + i * 12.0) * sin(t * PI))
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

static func _chapter_profile() -> Profile:
	var p := Profile.new()
	for level in range(1, 22):
		p.stars[level] = [3, 3, 2, 3, 1][(level - 1) % 5]
	p.highest_unlocked = 22
	return p

static func preview(main: Control, state: String) -> void:
	main.open_board(1, false)
	main._clear_popups()
	var host: Control = main.popups
	var noop := func(): pass
	var noop1 := func(_v): pass
	var roster: Array[String] = ["sunny", "sky", "berry", "mint", "cocoa", "snow", "ember"]
	var few: Array[String] = ["sunny", "sky", "berry"]
	var mechanics: Array[String] = ["sleep", "egg", "daynight"]
	var none: Array[String] = []
	match state:
		"leaf_transition":
			main.open_map()
			await main.get_tree().create_timer(0.6).timeout
			main.open_board(1, false)
		"win_three_stars": win(host, 3, 12, 12, 120, [Board.Booster.MORNING_BELL, Board.Booster.FIREFLY], noop, noop, noop, true)
		"win_two_stars": win(host, 2, 14, 12, 80, [Board.Booster.SAPLING], noop, noop, noop, true)
		"win_one_star": win(host, 1, 20, 12, 50, [Board.Booster.OWL_EYE], noop, noop, noop, false)
		"win_zero_reward": win(host, 3, 12, 12, 0, [], noop, noop, noop, true)
		"settings_sound_on": settings(host, true, true, {"music": noop1, "sound": noop1, "restart": noop, "home": noop, "help": noop}, true)
		"settings_muted": settings(host, false, false, {"music": noop1, "sound": noop1, "restart": noop, "home": noop, "help": noop}, false)
		"document_credits": document(host, "credits")
		"level_card_first_visit": level_card(host, 1, 0, 0, 6, few, none, noop, true)
		"level_card_full_roster": level_card(host, Levels.COUNT, 3, 28, 12, roster, mechanics, noop, true)
		"level_card_locked": level_card(host, 12, 0, 0, 9, few, mechanics, noop, false)
		"goal_info_berry": goal_info(host, "berry")
		"goal_info_hoot": goal_info(host, "hoot")
		"win_milestone": win(host, 3, 9, 9, 120, [Board.Booster.FIREFLY, Board.Booster.SCARECROW, Board.Booster.FIREFLY], noop, noop, noop, true)
		"lucky_draw": lucky_draw(host, 3, 5, func():
			var cell := randi() % 8
			var prize: Array = Progress.DRAW_PRIZES[cell]
			return {"cell": cell, "coins": prize[0], "booster": 0 if prize[1] > 0 else -1, "count": prize[1]})
		"lucky_draw_spin":
			var drawn := lucky_draw(host, 3, 5, func(): return {"cell": 2, "coins": 0, "booster": Board.Booster.SAPLING, "count": 1})
			host.get_tree().create_timer(0.3).timeout.connect(func(): (drawn.find_child("Draw", true, false) as Button).pressed.emit())
		"lucky_draw_coins":
			var won := lucky_draw(host, 3, 5, func(): return {"cell": 4, "coins": 200, "booster": -1, "count": 0})
			host.get_tree().create_timer(0.3).timeout.connect(func(): (won.find_child("Draw", true, false) as Button).pressed.emit())
		"lucky_draw_none": lucky_draw(host, 0, 5, func(): return {})
		"chapters": chapters(host, _chapter_profile(), noop1)
		"chest_half": chest(host, 17, 0, noop, noop)
		"chest_ready": chest(host, 31, 1, noop)
		"rewards_chest": rewards(host, "chest.title", 150, [0, 3], noop)
		"new_friend_berry": new_friend(host, "berry", 25, noop)
		"unlock_owl_eye": unlock(host, 2, noop)
		"unlock_sapling": unlock(host, 1, noop)
		"world_complete_1": world_complete(host, 1, 200, noop)
		"world_unlock_2": world_unlock(host, 2, noop)
		"world_unlock_4": world_unlock(host, 4, noop)
