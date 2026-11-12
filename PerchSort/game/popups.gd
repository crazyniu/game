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
	var stats_h := REWARD_H if reward > 0 else LINE
	var heights := [FamilyDialog.STAR_ROW, stats_h]
	if not boosters.is_empty():
		heights.append(DROP_ROW)
	var items := [["orange", "retry", on_replay], ["green", "play", on_next, "action.next"], ["orange", "home", on_home]]
	var p := Modal.open(host, "popup.win.title", heights, false, FamilyDialog.actions_height(items), HERO + 86.0, not live)
	FamilyDialog.decorate(p)
	var c := p.content
	var ys: Array = p.get_meta("ys")
	var panel := p.panel_rect
	if live:
		_rays(p, panel.get_center())
	# Equal footline, balanced spacing and a clear gap above the title, using the same live birds.
	var footline := panel.position.y - 92.0
	var hero_box := Rect2(432, footline - 216.0, 216, 216)
	var side_boxes := [Rect2(208, footline - 184.0, 184, 184), Rect2(688, footline - 184.0, 184, 184)]
	var hero := _bird_view(c, "sunny", hero_box, not live, 2.4)
	var sides := [_bird_view(c, "sky", side_boxes[0], not live, 0.0),
		_bird_view(c, "berry", side_boxes[1], not live, 0.0)]
	hero.set_meta("visible_box", hero_box)
	for i in sides.size():
		sides[i].set_meta("visible_box", side_boxes[i])
	if live:
		_fly_in(hero, Vector2(640, -1250), 0.45, 1.0)
		_fly_in(sides[0], Vector2(-420, -120), 0.75, 0.8)
		_fly_in(sides[1], Vector2(420, -120), 0.9, 0.8)
	FamilyDialog.stars(c, ys[0], stars, live, 0.6)
	# Moves and feather rewards share one unboxed reading row.
	var number_w := FamilyDialog.number_width(str(moves), LINE_CAP)
	var moves_w := 100.0 + number_w
	var stats_w := moves_w + (64.0 + REWARD_W if reward > 0 else 0.0)
	var stats_x := 540.0 - stats_w * 0.5
	var stats_mid: float = ys[1] + stats_h * 0.5
	UI.icon(c, "hop", Vector2(stats_x + 40.0, stats_mid), 80.0, Color.WHITE)
	var moves_box := FamilyDialog.number(c, str(moves), Rect2(stats_x + 100.0, stats_mid - LINE_CAP * 0.5, number_w, LINE_CAP))
	if live:
		FamilyDialog.count(moves_box, moves, "%d", 0.5, 0.6)
	if reward > 0:
		var bar := FamilyDialog.reward(c, Rect2(stats_x + moves_w + 64.0, ys[1], REWARD_W, REWARD_H), reward)
		if live:
			FamilyDialog.count(bar.get_node("Number"), reward, "+%d", 1.0, 0.7)
			_coin_burst(p, bar.get_meta("coin_center"), 1.0)
	if not boosters.is_empty():
		_drops(c, ys[-1], boosters, live, 1.5)
	var row := FamilyDialog.actions(p, items)
	UI.set_active(row[1], next_enabled)
	if live:
		_confetti(p, 0.3)
	return p

## Settings: a simple cream card. Four identical rows (icon, word, control): Music, Sound and Vibration switches and
## the Language choice, then one row of equal buttons (Help; Restart and Home only while playing). Every surface is drawn
## from the same few shapes in Simple, every word is a baked label (tools/island/bake_labels.py) shown at one cap
## height, and nothing is stretched. Changing the language swaps the words in place: the popup stays open.
## `on` holds the callbacks: music, sound, vibration, language, restart, home, help.
const SET_W := 840.0
const SET_PAD := 48.0
const SET_TITLE_H := 128.0
const SET_ROW_H := 132.0
const SET_ICON := 72.0
const SET_WORD_CAP := 54.0
const SET_TITLE_CAP := 76.0
const SET_BUTTON := Vector2(340, 116)
const SET_BUTTON_GAP := 24.0
const SET_BUTTON_CAP := 52.0

static func settings(host: Control, music_on: bool, sound_on: bool, on: Dictionary, show_restart: bool) -> Control:
	var buttons_h := SET_BUTTON.y * (2.0 if show_restart else 1.0) + (SET_BUTTON_GAP if show_restart else 0.0)
	var panel_h := SET_PAD * 0.5 + SET_TITLE_H + SET_ROW_H * 4.0 + 40.0 + buttons_h + SET_PAD
	var p := Modal.open(host, "", [], false, 0.0, 0.0, _instant(host), true)
	var c := p.content
	c.get_node("Panel").visible = false
	var box := Rect2(540.0 - SET_W * 0.5, roundf(960.0 - panel_h * 0.5), SET_W, panel_h)
	c.pivot_offset = box.get_center()
	Simple.card(c, box)
	var words := Simple.Words.new()
	words.place(c, "settings", "ink", Vector2(box.get_center().x, box.position.y + SET_PAD * 0.5 + SET_TITLE_H * 0.5), SET_TITLE_CAP, 0)
	Simple.close_button(c, Vector2(box.end.x - 20.0, box.position.y + 20.0), p.close)
	var rows := [["pic_music", "music", music_on, func(v: bool): on["music"].call(v)],
		["pic_sound_on", "sound", sound_on, func(v: bool): on["sound"].call(v)],
		# Vibration: the state lives in Sound.vibration (profile.vibration).
		["pic_vibration", "vibration", Sound.vibration, func(v: bool):
			Sound.vibration = v
			if on.has("vibration"):
				on["vibration"].call(v)
			Sound.vibrate(Sound.PULSE_LAND)],
		["pic_globe", "language", false, Callable()]]
	var left := box.position.x + SET_PAD
	var right := box.end.x - SET_PAD
	var y := box.position.y + SET_PAD * 0.5 + SET_TITLE_H
	for i in rows.size():
		var mid := y + SET_ROW_H * (i + 0.5)
		if i > 0:
			Simple.divider(c, Rect2(left, y + SET_ROW_H * i - 1.5, right - left, 3.0))
		_fit(c, rows[i][0], Rect2(Vector2(left, mid - SET_ICON * 0.5), Vector2.ONE * SET_ICON))   # by visible pixels: one size
		words.place(c, rows[i][1], "ink", Vector2(left + SET_ICON + 28.0, mid), SET_WORD_CAP, -1)
		if rows[i][1] == "language":
			Simple.language_choice(c, words, Vector2(right, mid), func(code: String):
				on["language"].call(code)
				words.refresh())
		else:
			Simple.switch(c, Vector2(right, mid), rows[i][2], rows[i][3])
	y += SET_ROW_H * 4.0 + 40.0
	var actions := []
	if show_restart:
		actions = [[["restart", "cream", on["restart"]], ["home", "green", on["home"]]], [["help", "green", on["help"]]]]
	else:
		actions = [[["help", "green", on["help"]]]]
	for line in actions:
		var count: int = (line as Array).size()
		var width := SET_BUTTON.x * count + SET_BUTTON_GAP * (count - 1)
		for k in count:
			var item: Array = line[k]
			var at := Vector2(540.0 - width * 0.5 + k * (SET_BUTTON.x + SET_BUTTON_GAP), y)
			var callback: Callable = item[2]
			Simple.button(c, words, item[0], item[1], Rect2(at, SET_BUTTON), SET_BUTTON_CAP, func(): p.invoke(callback))
		y += SET_BUTTON.y + SET_BUTTON_GAP
	return p

## A document page (terms of service, credits, contact): the baked text in a scrolling column.
static func document(host: Control, id: String) -> Control:
	var page: Texture2D = UiText.document_texture(id)
	var column := Modal.WIDTH - 144.0
	var view_h := minf(1180.0, page.get_height() * column / float(page.get_width()) + 8.0)
	var p := Modal.open(host, "doc.%s.title" % id, [view_h], true, 0.0, 0.0, _instant(host), true)
	MenuNavigation.replace_close(p)
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
	var heights := [FamilyDialog.STAR_ROW, LINE]
	if rows > 0:
		heights.append(rows * RING + (rows - 1) * RING_GAP)
	if not mechanics.is_empty():
		heights.append(UI.COUNTER_H)
	var p := Modal.open(host, "popup.level.title", heights, true, FamilyDialog.PRIMARY.y, 0.0, _instant(host), false, str(level))
	FamilyDialog.decorate(p)
	var c := p.content
	var ys: Array = p.get_meta("ys")
	var g := 0
	# Stars earned so far, then the move target for three stars (moves picture and number, no words).
	FamilyDialog.stars(c, ys[g], best_stars)
	g += 1
	FamilyDialog.stat(c, ys[g], "hop", str(par))
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
	UI.set_active(FamilyDialog.actions(p, [["green", "play", on_play]])[0], playable)
	return p

static func goal_info(host: Control, species: String) -> Control:
	var p := Modal.open(host, "bird.%s.name" % species, [FRIEND, ABILITY_H], true, 0.0, 0.0, _instant(host))
	FamilyDialog.decorate(p, false)
	var c := p.content
	var ys: Array = p.get_meta("ys")
	_bird_view(c, species, Rect2(540.0 - FRIEND * 0.5 + 40.0, ys[0] + 30.0, FRIEND - 80.0, FRIEND - 60.0), _instant(host), 0.0)
	FamilyDialog.ability(c, species, EFFECT.get(species, ""), ys[1])
	return p

## Daily lucky draw: a 3x3 board whose eight outer cells hold prizes and whose centre is the gold chest. A light runs
## round the ring, slows down and stops on the prize (coins, or a booster with its count). The star row shows today's
## draws: filled = left, hollow = unlocked by playing more days in a row. `draw` performs one draw (grants the prize)
## and returns {"cell", "coins", "booster", "count"} like Progress.draw, or {} when none is left. When the light stops,
## a rewards popup over the board names what was won; the next draw is possible once it is dismissed.
const DRAW_BOARD := Rect2(39, 486, 838, 846)
const DRAW_CELL := Vector2(240, 232)
const DRAW_ORIGIN := Vector2(46, 58)
const DRAW_STEP := Vector2(251, 247)
const DRAW_ART := Rect2(20, 10, 200, 156)
const DRAW_BOOSTER_ART := ["firefly", "sprout", "owl_eye", "bell", "scarecrow", "rain"]
const DRAW_SPOTS := [Vector2i(0, 0), Vector2i(1, 0), Vector2i(2, 0), Vector2i(2, 1), Vector2i(2, 2), Vector2i(1, 2), Vector2i(0, 2), Vector2i(0, 1)]
const DRAW_PRIZE_KEYS := ["icon_draw_prize_forty", "icon_draw_prize_eighty", "icon_draw_mystery_single", "icon_draw_prize_one_twenty", "icon_draw_prize_two_hundred", "icon_draw_mystery_double", "icon_draw_prize_sixty", "icon_draw_prize_four_hundred"]
# The original source crops relative to their measured prize wells. No source pixels are changed.
const DRAW_PICTURES := [Rect2(41, 21, 176, 137), Rect2(32, 15, 203, 142), Rect2(29, 15, 195, 149), Rect2(7, 3, 234, 161), Rect2(4, -2, 240, 165), Rect2(18, -3, 221, 166), Rect2(7, 2, 226, 160), Rect2(9, 0, 222, 161)]

static func lucky_draw(host: Control, spins: int, slots: int, draw: Callable) -> Control:
	var instant := _instant(host)
	var p := Modal.open(host, "", [], false, 0.0, 0.0, true)
	p.name = "DailyDraw"
	p.panel_rect = Rect2(0, 0, 1080, 1920)
	p.content.pivot_offset = Vector2(540, 960)
	for node_name in ["Panel", "TitleGroup"]:
		var old := p.content.get_node(node_name)
		p.content.remove_child(old)
		old.queue_free()
	var background := DrawPurchaseArt.background(p)
	p.move_child(background, 1)
	var c := DrawPurchaseArt.canvas(p.content)
	p._title_layer = DrawPurchaseArt.draw_heading(p.content)
	var close := MenuNavigation.close(c, "Close", Vector2(23, 52), p.close, DrawPurchaseArt.SCALE)
	var stars := []
	for i in slots:
		var x := 458.0 + (i - (slots - 1) * 0.5) * 122.0
		# Both star states use the same visible box; only their accepted texture changes.
		var key := "star_draw_available" if i < spins else "star_draw_used"
		var star := DrawPurchaseArt.fit(c, key, Rect2(x - 54, 341, 108, 104))
		star.name = "Attempt%d" % i
		stars.append(star)
	var board := Control.new()
	board.name = "DrawBoard"
	board.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(board, DRAW_BOARD)
	c.add_child(board)
	DrawPurchaseArt.fit(board, "panel_draw_empty_board", Rect2(Vector2.ZERO, DRAW_BOARD.size))
	var cursor := StyleBoxFlat.new()
	cursor.bg_color = Color(1.0, 0.81, 0.22, 0.08)
	cursor.border_color = Color8(255, 231, 117)
	cursor.set_border_width_all(5)
	cursor.set_corner_radius_all(43)
	cursor.shadow_color = Color(1.0, 0.69, 0.14, 0.85)
	cursor.shadow_size = 12
	var cards := []
	var lights := []
	for i in 8:
		var card := Control.new()
		card.name = "Prize%d" % i
		card.mouse_filter = Control.MOUSE_FILTER_IGNORE
		UI.place(card, Rect2(DRAW_ORIGIN + Vector2(DRAW_SPOTS[i]) * DRAW_STEP, DRAW_CELL))
		board.add_child(card)
		var lit := Panel.new()
		lit.name = "Highlight"
		lit.mouse_filter = Control.MOUSE_FILTER_IGNORE
		lit.add_theme_stylebox_override("panel", cursor)
		UI.place(lit, Rect2(Vector2(2, 2), DRAW_CELL - Vector2(4, 4)))
		lit.modulate.a = 0.0
		card.add_child(lit)
		lights.append(lit)
		var prize: Array = Progress.DRAW_PRIZES[i]
		UI.image(card, DRAW_PRIZE_KEYS[i], DRAW_PICTURES[i]).name = "PrizeArt"
		DrawPurchaseArt.number(card, str(prize[0]) if prize[0] > 0 else "×%d" % prize[1], Rect2(14, 159, 212, 51))
		cards.append(card)
	var chest := DrawPurchaseArt.fit(board, "icon_draw_center_chest", Rect2(311, 305, 216, 224))
	chest.name = "Chest"
	chest.pivot_offset = chest.size * 0.5
	if not instant:
		_breathe(chest, 0.0)
		Sound.play("ui_tap")
		background.modulate.a = 0.0
		p.content.modulate.a = 0.0
		p._appearance = p.create_tween().set_parallel()
		p._appearance.tween_property(background, "modulate:a", 1.0, 0.18)
		p._appearance.tween_property(p.content, "modulate:a", 1.0, 0.18)
	var state := {"left": spins, "busy": false, "at": 0}
	var go := DrawPurchaseArt.button(c, "Draw", "btn_draw_action", Rect2(182, 1366, 553, 202), func(): pass, true)
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
		UI.set_active(close, false)
		state["left"] -= 1
		var used: Control = stars[state["left"]]
		(used as TextureRect).texture = Art.tex("star_draw_used")
		var highlight := func(at: int):
			lights[state["at"]].modulate.a = 0.0
			lights[at].modulate.a = 1.0
			state["at"] = at
		var reveal := func():
			Sound.play("reward")
			if got["booster"] >= 0:
				var holder := cards[target] as Control
				var old := holder.get_node("PrizeArt")
				holder.remove_child(old)
				old.queue_free()
				var art := _fit(holder, "island_icon_" + DRAW_BOOSTER_ART[got["booster"]], DRAW_ART)
				art.name = "PrizeArt"
		var finish := func():
			var kinds := []
			if got["booster"] >= 0:
				for k in int(got["count"]):
					kinds.append(got["booster"])
			p.content.hide()
			var result := _draw_rewards(host, got["coins"], kinds, func():
				p.content.show()
				lights[target].modulate.a = 0.0
				if got["booster"] >= 0:
					var holder := cards[target] as Control
					var old := holder.get_node("PrizeArt")
					holder.remove_child(old)
					old.queue_free()
					UI.image(holder, DRAW_PRIZE_KEYS[target], DRAW_PICTURES[target]).name = "PrizeArt"
				state["busy"] = false
				UI.set_active(close, true)
				UI.set_active(go, state["left"] > 0))
			# The draw's dim backdrop already covers the home screen.
			result._dim.color.a = 0.0
			p.tree_exiting.connect(result.queue_free)
		if instant:
			highlight.call(target)
			reveal.call()
			finish.call()
			return
		# Binding the complete roll to the modal cancels it when the modal is removed.
		var roll := p.create_tween()
		var steps: int = 24 + posmod(target - int(state["at"]), 8)
		var start: int = state["at"]
		for n in steps:
			var at := posmod(start + n + 1, 8)
			roll.tween_callback(highlight.bind(at))
			roll.tween_callback(Sound.play.bind("ui_tap", 1.0 + 0.03 * n if n < 24 else 1.7))
			var t := float(n) / float(steps - 1)
			roll.tween_interval(0.045 + 0.3 * t * t * t)
		roll.tween_callback(reveal)
		roll.tween_interval(0.7)
		roll.tween_callback(finish)
	(go as Button).pressed.connect(run)
	return p

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
	MenuNavigation.replace_close(m)
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
	# Existing painted pagination words share a surface; neither page turn changes saved progress.
	var turn := func(to: int):
		var parent := m.get_parent()
		m.queue_free()
		chapters(parent, p, on_pick, to)
	var row_y: float = m.get_meta("actions_y")
	var back := ShopSkin.product_button(c, "PagePrevious",
		Rect2(540.0 - Modal.SECONDARY_PILL.x - 28.0, row_y, Modal.SECONDARY_PILL.x, Modal.SECONDARY_PILL.y),
		func(): turn.call(page - 1), "gold")
	var forward := ShopSkin.product_button(c, "PageNext",
		Rect2(540.0 + 28.0, row_y, Modal.SECONDARY_PILL.x, Modal.SECONDARY_PILL.y),
		func(): turn.call(page + 1), "gold")
	var word_box := Rect2(Vector2(32, 24), Modal.SECONDARY_PILL - Vector2(64, 48))
	UiText.painted_label(back, "action.back", word_box)
	UiText.painted_label(forward, "action.next", word_box)
	UI.set_active(back, page > 0)
	UI.set_active(forward, page < last_page)
	return m

## Approved jungle chest page: freestanding gold heading, wooden support and card-free prizes.
const CHEST_SUPPORT := Rect2(145, 1070, 790, 214)
const CHEST_HERO := Rect2(235, 680, 610, 480)
const CHEST_READY_HERO := Rect2(235, 544, 610, 616)
const CHEST_PROGRESS := Rect2(168, 1300, 744, 112)
const CHEST_PROGRESS_INSET := 9.0
const CHEST_ACTION := Rect2(280, 1456, 520, 176)
const CHEST_RESULT_HERO := Rect2(270, 792, 540, 540)
const CHEST_RESULT_SUPPORT := Rect2(145, 1242, 790, 214)
# Ignore invisible generation padding when positioning the original, unchanged RGBA pictures.
const CHEST_CLOSED_INK := Rect2(33, 172, 1189, 933)
const CHEST_OPEN_INK := Rect2(101, 90, 1031, 1012)
const CHEST_SUPPORT_INK := Rect2(43, 112, 2082, 559)
const ChestLettering := preload("res://game/chest_lettering.gd")
const CHEST_BOOSTER_ART := ["firefly", "sprout", "owl_eye", "bell", "scarecrow", "rain"]

static func chest(host: Control, progress: int, ready: int, on_open: Callable, on_play := Callable(), balance := -1) -> Control:
	var p := _chest_page(host, balance, true)
	_chest_hero(p, ready > 0, ready > 0)
	if ready > 1:
		Island.badge(p.content, Vector2(842, 704), ready)
	var collected := clampi(progress, 0, Progress.CHEST_STARS)
	_chest_progress(p.content, float(collected) / Progress.CHEST_STARS,
		"%d/%d" % [collected, Progress.CHEST_STARS])
	var open := _chest_action(p, "OpenChest", "chest.open", CHEST_ACTION, on_open)
	UI.set_active(open, ready > 0)
	if ready <= 0:
		var disabled_ink := ShaderMaterial.new()
		disabled_ink.shader = preload("res://game/chest_action_disabled.gdshader")
		open.get_node("Face").material = disabled_ink
		open.modulate = Color(0.76, 0.79, 0.75, 1.0)
	if on_play.is_valid():
		_chest_action(p, "Play", "action.play", Rect2(280, 1648, 520, 176), on_play)
	return p

## The approved green rail uses its own slim rim; numbers reuse the homepage glyph family.
static func _chest_progress(parent: Control, fill: float, text: String) -> Control:
	var progress := Control.new()
	progress.name = "Progress"
	progress.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(progress, CHEST_PROGRESS)
	parent.add_child(progress)
	var star := CHEST_PROGRESS.size.y * 1.25
	var track := Rect2(star * 0.45, CHEST_PROGRESS.size.y * 0.12,
		CHEST_PROGRESS.size.x - star * 0.45, CHEST_PROGRESS.size.y * 0.76)
	var rail := _chest_capsule(progress, "panel_chest_progress", Rect2(112, 285, 1946, 157), track)
	rail.name = "Track"
	var inner := track.grow(-CHEST_PROGRESS_INSET)
	if fill > 0.0:
		var clip := Control.new()
		clip.name = "Fill"
		clip.mouse_filter = Control.MOUSE_FILTER_IGNORE
		clip.clip_contents = true
		UI.place(clip, Rect2(inner.position, Vector2(inner.size.x * clampf(fill, 0.0, 1.0), inner.size.y)))
		progress.add_child(clip)
		# A fixed end-cap radius preserves the rounded tip at partial and full progress.
		var fill_rect := Rect2(Vector2.ZERO, Vector2(maxf(inner.size.y, clip.size.x), inner.size.y))
		_chest_capsule(clip, "fill_chest_progress", Rect2(160, 273, 1846, 177), fill_rect)
	UI.art(progress, "island_star_full", Rect2(-star * 0.1, (CHEST_PROGRESS.size.y - star) * 0.5, star, star))
	ChestLettering.number(progress, text,
		Rect2(0, 24, CHEST_PROGRESS.size.x, 62))
	return progress

static func _chest_capsule(parent: Node, key: String, ink: Rect2, bounds: Rect2) -> NinePatchRect:
	var texture := AtlasTexture.new()
	texture.atlas = Art.tex(key)
	texture.region = ink
	var capsule := NinePatchRect.new()
	capsule.texture = texture
	capsule.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var k := bounds.size.y / ink.size.y
	capsule.position = bounds.position
	capsule.size = bounds.size / k
	capsule.scale = Vector2.ONE * k
	var end := int(ceilf(ink.size.y * 0.55))
	capsule.patch_margin_left = end
	capsule.patch_margin_right = end
	parent.add_child(capsule)
	return capsule

## Keep the background fixed while the controls fade; modal ownership still blocks the page underneath.
static func _chest_page(host: Control, balance: int, close_button: bool) -> Modal:
	var p := Modal.open(host, "", [], false, 0.0, 0.0, true)
	p.name = "StarChest"
	p.panel_rect = Rect2(0, 0, 1080, 1920)
	p.content.pivot_offset = Vector2(540, 960)
	for name in ["Panel", "TitleGroup"]:
		var old := p.content.get_node(name)
		p.content.remove_child(old)
		old.queue_free()
	var background := UI.image(p, "background_home_jungle", Rect2(0, 0, 1080, 1920), false)
	p.move_child(background, 1)
	p._title_layer = ShopSkin.word(p.content, "chest.title", Rect2(230, 230, 620, 170))
	p._title_layer.name = "TitleGroup"
	if close_button:
		MenuNavigation.close(p.content, "Close", Vector2(48, 48), p.close)
	if balance >= 0:
		FeatherBar.create_right(p.content, Vector2(1032, 48), balance, ShopSkin.COUNTER_INK)
	if not _instant(host):
		Sound.play("ui_tap")
		background.modulate.a = 0.0
		p.content.modulate.a = 0.0
		p._appearance = p.create_tween().set_parallel()
		p._appearance.tween_property(background, "modulate:a", 1.0, 0.18)
		p._appearance.tween_property(p.content, "modulate:a", 1.0, 0.18)
	return p

static func _chest_hero(p: Modal, opened: bool, sparkling: bool, result := false) -> void:
	var live := not _instant(p.get_parent())
	var hero := CHEST_RESULT_HERO if result else (CHEST_READY_HERO if opened else CHEST_HERO)
	var rays := UI.image(p.content, "light_rays", Rect2(hero.get_center() - Vector2(480, 480), Vector2(960, 960)))
	rays.modulate.a = 0.52 if sparkling else 0.3
	var glow := UI.image(p.content, "glow_soft", hero.grow(80.0))
	glow.modulate = Color(1.0, 0.86, 0.4, 0.55)
	UI.art(p.content, "chest_islet_support", CHEST_RESULT_SUPPORT if result else CHEST_SUPPORT, CHEST_SUPPORT_INK)
	var content := CHEST_OPEN_INK if opened else CHEST_CLOSED_INK
	var scale := minf(hero.size.x / content.size.x, hero.size.y / content.size.y)
	var shown := content.size * scale
	var bounds := Rect2(Vector2(hero.get_center().x - shown.x * 0.5, hero.end.y - shown.y), shown)
	var art := UI.art(p.content, "chest_star_open" if opened else "chest_star_closed", bounds, content)
	art.name = "ChestHero"
	art.pivot_offset = Vector2(bounds.get_center().x, bounds.end.y) - art.position
	if live:
		rays.create_tween().set_loops().tween_property(rays, "rotation", TAU, 28.0).from(0.0)
		_breathe(art, 0.0)
	if sparkling:
		var sparks := [Vector2(-320, -180), Vector2(300, -140), Vector2(280, 140), Vector2(-300, 160)]
		for i in sparks.size():
			var spark := UI.art(p.content, "sparkle", Rect2(hero.get_center() + sparks[i], Vector2(56, 56)))
			if live:
				_twinkle(spark, i * 0.35)

static func _chest_action(p: Modal, name: String, word: String, bounds: Rect2, callback: Callable) -> Button:
	var button := UI.hit_area(p.content, bounds, func():
		# An Open double-tap must not click through to the newly appearing reward confirmation.
		if p.content.modulate.a < 0.99:
			return
		Sound.play("ui_tap")
		p.invoke(callback))
	button.name = name
	var face := Island.nine(button, "home_play_plate", Rect2(Vector2.ZERO, bounds.size), Vector4(220, 100, 220, 100))
	face.name = "Face"
	var skin_scale := bounds.size.y / Art.tex("home_play_plate").get_height()
	face.size = bounds.size / skin_scale
	face.pivot_offset = Vector2.ZERO
	face.scale = Vector2.ONE * skin_scale
	ChestLettering.word(button, word, Rect2(48, 36, bounds.size.x - 96, 100))
	button.button_down.connect(func(): button.modulate = Island.PRESSED)
	button.button_up.connect(func(): button.modulate = Color.WHITE)
	return button

static func _chest_rewards(host: Control, coins: int, boosters: Array, on_ok: Callable, balance: int) -> Control:
	var p := _chest_page(host, balance, false)
	_chest_hero(p, true, true, true)
	var prizes: Array = []
	if coins > 0:
		prizes.append(["coin", coins])
	var counts := {}
	for kind in boosters:
		counts[kind] = int(counts.get(kind, 0)) + 1
	for kind in counts:
		prizes.append(["island_icon_" + CHEST_BOOSTER_ART[kind], counts[kind]])
	for i in prizes.size():
		var spacing := minf(228.0, 912.0 / prizes.size())
		var offset := i - (prizes.size() - 1) * 0.5
		var x := 540.0 + offset * spacing
		var y := 486.0 + minf(1.0, absf(offset)) * 68.0
		var prize := Control.new()
		prize.name = "ChestPrize%d" % i
		prize.mouse_filter = Control.MOUSE_FILTER_IGNORE
		prize.set_meta("picture", prizes[i][0])
		prize.set_meta("amount", prizes[i][1])
		UI.place(prize, Rect2(x - spacing * 0.5, y, spacing, 250))
		p.content.add_child(prize)
		var art := _fit(prize, prizes[i][0], Rect2(8, 0, spacing - 16, 190), true)
		if not _instant(host):
			_breathe(art, i * 0.4)
		UI.number(prize, "+%d" % prizes[i][1], Rect2(4, 204, spacing - 8, 56), Color.WHITE)
	_chest_action(p, "ConfirmRewards", "action.ok", Rect2(280, 1536, 520, 176), on_ok)
	return p

const REWARD_TILE := 250.0

## Coins and boosters form one celebration group with painted amounts; OK closes.
## `picture` stands in when no booster is won (coins only).
static func rewards(host: Control, title_id: String, coins: int, boosters: Array, on_ok: Callable, picture := "pic_chest_open", balance := -1) -> Control:
	if title_id == "draw.title":
		return _draw_rewards(host, coins, boosters, on_ok)
	if title_id == "chest.title":
		return _chest_rewards(host, coins, boosters, on_ok, balance)
	var live := not _instant(host)
	var prizes := []
	if coins > 0:
		prizes.append(["coin", coins])
	var counts := {}
	for kind in boosters:
		counts[kind] = int(counts.get(kind, 0)) + 1
	for kind in counts:
		prizes.append(["booster_%s_illus" % BOOSTER_ILLUS[kind], counts[kind]])
	var rows := ceili(prizes.size() / 3.0)
	var hero_h := 320.0 if rows > 1 else ILLUS
	var reward_h := REWARD_TILE * rows + 20.0 * maxf(0.0, rows - 1)
	var p := Modal.open(host, title_id, [hero_h, reward_h] if not prizes.is_empty() else [hero_h], false,
		FamilyDialog.PRIMARY.y, 0.0, not live)
	FamilyDialog.decorate(p)
	var c := p.content
	var ys: Array = p.get_meta("ys")
	var hero := Rect2(540.0 - hero_h * 0.5, ys[0], hero_h, hero_h)
	var rays := UI.image(c, "light_rays", hero.grow(76.0))
	rays.pivot_offset = rays.size * 0.5
	rays.modulate.a = 0.4
	var glow := UI.image(c, "glow_soft", hero.grow(52.0))
	glow.modulate = Color(1.0, 0.86, 0.42, 0.7)
	var art := _fit(c, picture if picture != "coin" else "pic_chest_open", hero.grow(-30.0), true)
	if live:
		rays.create_tween().set_loops().tween_property(rays, "rotation", TAU, 28.0).from(0.0)
		_breathe(art, 0.0)
	for i in prizes.size():
		var row: int = i / 3
		var columns := mini(3, prizes.size() - row * 3)
		var spacing := 250.0
		var x := 540.0 + (i % 3 - (columns - 1) * 0.5) * spacing
		var y: float = ys[1] + row * (REWARD_TILE + 20.0)
		var prize := Control.new()
		prize.name = "Reward%d" % i
		prize.mouse_filter = Control.MOUSE_FILTER_IGNORE
		UI.place(prize, Rect2(x - spacing * 0.5, y, spacing, REWARD_TILE))
		c.add_child(prize)
		var item := _fit(prize, prizes[i][0], Rect2(20, 0, spacing - 40, 170), true)
		if live:
			_breathe(item, i * 0.4)
		FamilyDialog.number(prize, "+%d" % prizes[i][1], Rect2(8, 190, spacing - 16, 56))
	FamilyDialog.actions(p, [["green", "play", on_ok, "action.ok"]])
	return p

## Completion keeps the accepted draw header, forest and painted prize family.
## The actual draw grants one distinct item; the grid also keeps mixed preview data readable.
static func _draw_rewards(host: Control, coins: int, boosters: Array, on_ok: Callable) -> Modal:
	var instant := _instant(host)
	var prizes := []
	if coins > 0:
		var coin_picture := "icon_draw_prize_four_hundred"
		for i in Progress.DRAW_PRIZES.size():
			if Progress.DRAW_PRIZES[i][0] == coins:
				coin_picture = DRAW_PRIZE_KEYS[i]
				break
		prizes.append([coin_picture, coins])
	var counts := {}
	for kind in boosters:
		counts[kind] = int(counts.get(kind, 0)) + 1
	for kind in counts:
		prizes.append(["island_icon_" + DRAW_BOOSTER_ART[kind], counts[kind]])
	var p := Modal.open(host, "", [], false, 0.0, 0.0, true)
	p.name = "DrawResult"
	p.panel_rect = Rect2(0, 0, 1080, 1920)
	p.content.pivot_offset = Vector2(540, 960)
	for node_name in ["Panel", "TitleGroup"]:
		var old := p.content.get_node(node_name)
		p.content.remove_child(old)
		old.queue_free()
	var background := DrawPurchaseArt.background(p)
	p.move_child(background, 1)
	var canvas := DrawPurchaseArt.canvas(p.content)
	p._title_layer = DrawPurchaseArt.draw_heading(p.content)
	# Keep the existing direct Content/Close and Content/Confirm contracts for callers.
	var close_at := Vector2(23, 50) * DrawPurchaseArt.SCALE + canvas.position
	MenuNavigation.close(p.content, "Close", close_at, p.close)
	var columns := maxi(1, mini(prizes.size(), 3))
	var rows := maxi(1, ceili(prizes.size() / 3.0))
	var tile := Vector2(260, 360) if columns > 1 else Vector2(500, 540)
	var gap := 24.0
	var height := tile.y * rows + gap * (rows - 1)
	var top := 870.0 - height * 0.5
	for i in prizes.size():
		var row := i / 3
		var in_row := mini(3, prizes.size() - row * 3)
		var x := 458.0 + (i % 3 - (in_row - 1) * 0.5) * (tile.x + gap)
		var reward := Control.new()
		reward.name = "Reward%d" % i
		reward.mouse_filter = Control.MOUSE_FILTER_IGNORE
		reward.set_meta("picture", prizes[i][0])
		reward.set_meta("amount", prizes[i][1])
		UI.place(reward, Rect2(Vector2(x - tile.x * 0.5, top + row * (tile.y + gap)), tile))
		canvas.add_child(reward)
		var icon_box := Rect2(44, 32, tile.x - 88, tile.y - 184)
		var ray_size := Vector2.ONE * (tile.x + 112)
		var rays := UI.image(reward, "light_rays", Rect2(icon_box.get_center() - ray_size * 0.5, ray_size))
		rays.name = "Rays"
		rays.modulate.a = 0.68
		rays.pivot_offset = rays.size * 0.5
		var glow := UI.image(reward, "glow_soft", Rect2(icon_box.get_center() - ray_size * 0.44, ray_size * 0.88))
		glow.modulate = Color(1.0, 0.87, 0.42, 0.55)
		var art: TextureRect
		if str(prizes[i][0]).begins_with("icon_draw_"):
			art = DrawPurchaseArt.fit(reward, prizes[i][0], icon_box)
		else:
			art = _fit(reward, prizes[i][0], icon_box)
		art.name = "PrizeArt"
		art.pivot_offset = art.size * 0.5
		if not instant:
			rays.create_tween().set_loops().tween_property(rays, "rotation", TAU, 28.0).from(0.0)
			_breathe(art, i * 0.4)
		DrawPurchaseArt.number(reward, "+%d" % prizes[i][1], Rect2(20, tile.y - 108, tile.x - 40, 80))
	var action_box := Rect2(Vector2(218, 1366) * DrawPurchaseArt.SCALE + canvas.position,
		Vector2(480, 150) * DrawPurchaseArt.SCALE)
	var action := ShopSkin.product_button(p.content, "Confirm", action_box, p.close, "green")
	UiText.painted_label(action, "action.ok", Rect2(Vector2(48, 26) * DrawPurchaseArt.SCALE, Vector2(384, 98) * DrawPurchaseArt.SCALE))
	if not instant:
		p.content.modulate.a = 0.0
		p._appearance = p.create_tween()
		p._appearance.tween_property(p.content, "modulate:a", 1.0, 0.18)
	# Both controls restore the board once, after the shared modal exit finishes.
	p.closed.connect(on_ok)
	return p

static func new_friend(host: Control, species: String, reward: int, on_ok: Callable) -> Control:
	var p := Modal.open(host, "popup.new_friend.title", [FRIEND, NAME_CAP * 1.3, ABILITY_H, REWARD_H], false,
		FamilyDialog.PRIMARY.y, 0.0, _instant(host))
	FamilyDialog.decorate(p)
	var c := p.content
	var ys: Array = p.get_meta("ys")
	FamilyDialog.halo(c, Rect2(540.0 - FRIEND * 0.5, ys[0], FRIEND, FRIEND), _instant(host))
	_bird_view(c, species, Rect2(540.0 - FRIEND * 0.5 + 30.0, ys[0] + 20.0, FRIEND - 60.0, FRIEND - 40.0), _instant(host), 1.2)
	FamilyDialog.name_heading(c, "bird.%s.name" % species, ys[1], NAME_CAP * 1.3)
	FamilyDialog.ability(c, species, EFFECT.get(species, ""), ys[2])
	FamilyDialog.reward(c, Rect2(540.0 - REWARD_W * 0.5, ys[3], REWARD_W, REWARD_H), reward)
	FamilyDialog.actions(p, [["green", "check", on_ok]])
	return p

static func unlock(host: Control, booster: int, on_ok: Callable) -> Control:
	var p := Modal.open(host, "booster.%s.name" % BOOSTER_ID[booster], [ILLUS, LINE], false, FamilyDialog.PRIMARY.y, 0.0, _instant(host))
	FamilyDialog.decorate(p)
	var c := p.content
	var ys: Array = p.get_meta("ys")
	var box := Rect2(540.0 - ILLUS * 0.5, ys[0], ILLUS, ILLUS)
	var sparks := [Vector2(-0.78, -0.07), Vector2(0.78, -0.13), Vector2(-0.66, 0.57), Vector2(0.7, 0.55)]
	for i in sparks.size():
		var at: Vector2 = box.get_center() + sparks[i] * ILLUS * 0.5
		var sparkle := UI.art(c, "sparkle", Rect2(at - Vector2(32, 32), Vector2(64, 64)))
		if not _instant(host):
			_twinkle(sparkle, i * 0.35)
	FamilyDialog.halo(c, box, _instant(host))
	var picture := _fit(c, "booster_%s_illus" % BOOSTER_ILLUS[booster], box.grow(-40.0), true)
	if not _instant(host):
		_breathe(picture, 0.0)
	ShopSkin.product_word(c, "booster.%s.description" % BOOSTER_ID[booster], Rect2(132, ys[1] + (LINE - 54) * 0.5, 816, 54))
	FamilyDialog.actions(p, [["green", "check", on_ok]])
	return p

static func world_complete(host: Control, world: int, reward: int, on_ok: Callable) -> Control:
	var arch_h := GATE_W * GATE_CONTENT.size.y / GATE_CONTENT.size.x
	var p := Modal.open(host, "popup.world_complete.title", [arch_h, FamilyDialog.STAR_ROW, REWARD_H], false, FamilyDialog.PRIMARY.y, 0.0, _instant(host))
	FamilyDialog.decorate(p)
	var c := p.content
	var ys: Array = p.get_meta("ys")
	var live := not _instant(host)
	FamilyDialog.stars(c, ys[1], 3, live, 0.5)
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
	FamilyDialog.reward(c, Rect2(540.0 - REWARD_W * 0.5, ys[2], REWARD_W, REWARD_H), reward)
	FamilyDialog.actions(p, [["green", "check", on_ok]])
	return p

static func world_unlock(host: Control, world: int, on_play: Callable) -> Control:
	var gate_h := GATE_W * GATE_CONTENT.size.y / GATE_CONTENT.size.x
	var p := Modal.open(host, "popup.world_unlock.title", [gate_h, NAME_CAP * 1.3], false, FamilyDialog.PRIMARY.y, 0.0, _instant(host))
	FamilyDialog.decorate(p)
	var c := p.content
	var ys: Array = p.get_meta("ys")
	# Keep the door swing and scene inside the gate row, clear of the title and actions.
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
	# Reuse the soft halo wholly inside the doorway; the old tall light was cut into a rectangle by this clip.
	var light := UI.art(stage, "glow_soft", Rect2(230, 240, 560, 610))
	light.modulate = Color(1.0, 0.86, 0.42, 0.75)
	_canvas(stage, "gate_arch")
	var left := _canvas(stage, "gate_door_left")
	var right := _canvas(stage, "gate_door_right")
	left.pivot_offset.x = 234.0
	right.pivot_offset.x = 790.0
	if not _instant(host):
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
	FamilyDialog.name_heading(c, "world.%s.name" % WORLD_ID[world], ys[1], NAME_CAP * 1.3)
	FamilyDialog.actions(p, [["green", "play", on_play]])
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

## Booster drops keep plain painted "+N" quantities beside each picture, without a glossy badge or halo.
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
		var quantity := UI.number(holder, "+%d" % counts[kinds[i]], Rect2(DROP_SIDE - 64.0, DROP_SIDE - 60.0, 100, 56), Color.WHITE)
		quantity.name = "Quantity"
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
	Island.nine(c, "island_row_card_soft", row, Vector4(40, 40, 40, 40))
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
	UI.art(c, "btn_home_icon_frame", r)
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
	if state.begins_with("lucky_draw"):
		main.open_home()
	else:
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
		"lucky_draw_full": lucky_draw(host, Progress.DRAW_MAX_SPINS, Progress.DRAW_MAX_SPINS, func():
			return {"cell": 5, "coins": 0, "booster": Board.Booster.FIREFLY, "count": 2})
		"lucky_draw_reward_coins": rewards(host, "draw.title", 400, [], noop)
		"lucky_draw_reward_boosters": rewards(host, "draw.title", 0, [Board.Booster.SAPLING, Board.Booster.SAPLING], noop)
		"lucky_draw_reward_single": rewards(host, "draw.title", 0, [Board.Booster.MORNING_BELL], noop)
		"lucky_draw_reward_mixed": rewards(host, "draw.title", 120, [Board.Booster.SAPLING, Board.Booster.SAPLING, Board.Booster.FIREFLY], noop)
		"chapters": chapters(host, _chapter_profile(), noop1)
		"chest_empty": chest(host, 0, 0, noop, noop, 0)
		"chest_half": chest(host, 17, 0, noop, noop, 1280)
		"chest_ready": chest(host, Progress.CHEST_STARS, 1, func():
			rewards(host, "chest.title", Progress.CHEST_COINS, [0, 3, 0], noop, "pic_chest_open", 1280 + Progress.CHEST_COINS),
			noop, 1280)
		"chest_multiple": chest(host, Progress.CHEST_STARS * 3 + 2, 3, func():
			rewards(host, "chest.title", Progress.CHEST_COINS, [0, 2, 3], noop, "pic_chest_open", 99999), noop, 99999)
		"rewards_challenge": rewards(host, "challenge.title", 300, [0, 0, 1], noop)
		"rewards_challenge_empty": rewards(host, "challenge.title", 0, [], noop, "pic_check")
		"rewards_challenge_all": rewards(host, "challenge.title", 300, [0, 1, 2, 3, 4, 5], noop)
		"rewards_chest": rewards(host, "chest.title", Progress.CHEST_COINS, [0, 3, 0], noop, "pic_chest_open", 1580)
		"rewards_chest_distinct": rewards(host, "chest.title", Progress.CHEST_COINS, [0, 2, 3], noop, "pic_chest_open", 1580)
		"new_friend_berry": new_friend(host, "berry", 25, noop)
		"unlock_owl_eye": unlock(host, 2, noop)
		"unlock_sapling": unlock(host, 1, noop)
		"world_complete_1": world_complete(host, 1, 200, noop)
		"world_unlock_2": world_unlock(host, 2, noop)
		"world_unlock_4": world_unlock(host, 4, noop)
