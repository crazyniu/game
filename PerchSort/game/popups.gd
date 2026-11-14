extends RefCounted
## Modal popups opened on a host Control (main.popups). Every function returns the popup; callbacks are Callables.
## Layout: docs/design/island-style.md. One cream panel, a localized title, content groups stacked 32 apart, actions in a
## row under the panel. Words are complete locale textures, numbers are glyphs, and controls use the shared icon family.

const EFFECT := {"sunny": "wake", "sky": "reveal", "berry": "encore", "mint": "grow", "cocoa": "scare", "snow": "undo",
	"ember": "hatch", "ink": "encore", "hoot": "wake"}
const BOOSTER_ID := ["firefly", "sapling", "owl_eye", "morning_bell", "scarecrow", "spring_rain"]
const WORLD_ID := {1: "green", 2: "bamboo", 3: "autumn", 4: "snow", 5: "blossom", 6: "mushroom", 7: "lakeside", 8: "pine",
	9: "rainforest", 10: "wisteria"}
const MECHANIC_ART := {"sleep": "hoot_sleep", "egg": "egg_idle", "crow": "crow_front",
	"twig": "icon_sapling", "growing": "icon_sapling", "swivel": "btn_swivel_green", "mama": "mama_bow"}

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


# ---------------------------------------------------------------- popups

## Win: the approved green jungle frame and wooden title plaque, three live birds cheering above it, the stars,
## moves and feather reward in painted numerals, booster drops, then Next (green PRIMARY) with Replay and Home
## (equal wooden pills, existing white words) on a row below; Next alone is green and spans the row.
const WIN_STATS_H := 96.0
const WIN_STATS_CAP := 64.0
const WIN_ROW_GAP := 22.0       # between the Next row and the Replay / Home row
const WIN_PILL_GAP := 24.0      # between Replay and Home
const WIN_ACTIONS_H := 162.0 + WIN_ROW_GAP + 130.0   # ButtonKit.PRIMARY.y + gap + FamilyDialog.SECONDARY.y
const WIN_STAR_ARC := [Vector3(-1.0, 8.0, -12.0), Vector3(0.0, -14.0, 0.0), Vector3(1.0, 8.0, 12.0)]   # x side, drop, tilt
const WIN_STAR_ROOM := 56.0     # extra height of the star row for the arc and the larger middle star
const WIN_STAR_MIDDLE := 1.24   # the middle star is larger, the outer two tilt outwards on an arc
const WIN_PERCH := 0.38         # the top edge of the title banner, as a share of the title box height

static func win(host: Control, stars: int, moves: int, par: int, reward: int, boosters: Array, on_replay: Callable,
		on_next: Callable, on_home: Callable, next_enabled: bool) -> Control:
	var live := not _instant(host)
	var heights := [FamilyDialog.STAR_ROW + WIN_STAR_ROOM, WIN_STATS_H]
	if not boosters.is_empty():
		heights.append(DROP_ROW)
	var p := JungleDialog.open(host, "win_title", "", heights, not live, WIN_ACTIONS_H, false, true, true, false, 880.0, 72.0)
	var close := p.content.get_node_or_null("Close")
	if close:
		p.content.remove_child(close)
		close.queue_free()
	# The same full jungle backdrop as the other game dialogs (DialogCartoonSkin).
	var backdrop := UI.image(p, DialogCartoonSkin.BACKGROUND_ART, Rect2(0, 0, 1080, 1920))
	backdrop.name = "ForestBackground"
	backdrop.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_COVERED
	p.move_child(backdrop, 0)
	p._dim.color = Modal.POPUP_SHADE
	var c := p.content
	var ys: Array = p.get_meta("ys")
	var card: Rect2 = p.get_meta("card_rect")
	if live:
		_rays(p, card.get_center())
	_celebration_birds(c, card, live)
	_win_stars(c, ys[0], stars, live, 0.4)
	# Moves and the feather reward share one row of painted numerals.
	var cap := WIN_STATS_CAP
	var icon := cap * 1.15
	var moves_w := icon + 12.0 + ButtonKit.number_width(str(moves), cap)
	var reward_text := "+%d" % reward
	var reward_w := icon + 12.0 + ButtonKit.number_width(reward_text, cap) if reward > 0 else 0.0
	var gap := 72.0 if reward > 0 else 0.0
	var x := 540.0 - (moves_w + gap + reward_w) * 0.5
	var mid: float = ys[1] + (WIN_STATS_H) * 0.5
	UI.art(c, "icon_star_budget_steps", Rect2(x, mid - icon * 0.5, icon, icon), Rect2(317, 217, 652, 801)).name = "MovesIcon"
	var moves_box := _win_digits(c, "Moves", Rect2(x + icon + 12.0, mid - cap * 0.5, moves_w - icon - 12.0, cap))
	if live:
		_win_count(moves_box, moves, "%d", 0.5, 0.6)
	else:
		_win_set(moves_box, str(moves))
	if reward > 0:
		var group := Control.new()
		group.name = "Reward"
		group.mouse_filter = Control.MOUSE_FILTER_IGNORE
		UI.place(group, Rect2(x + moves_w + gap, mid - icon * 0.5, reward_w, icon))
		c.add_child(group)
		UI.image(group, "icon_purchase_feather", Rect2(0, 0, icon, icon), true).name = "Feather"
		var number := _win_digits(group, "Number", Rect2(icon + 12.0, (icon - cap) * 0.5, reward_w - icon - 12.0, cap))
		if live:
			_win_count(number, reward, "+%d", 1.35, 0.65)
			_reward_emphasis(group, 2.0)
			_coin_burst(p, group.position + Vector2(icon * 0.5, icon * 0.5), 1.35)
		else:
			_win_set(number, reward_text)
	if not boosters.is_empty():
		_drops(c, ys[-1], boosters, live, 1.8)
	# Next leads; Replay and Home are the two equal SECONDARY pills under it (PRIMARY / SECONDARY = 1.25).
	var y := p.panel_rect.end.y + Modal.ACTION_DROP
	# One block: Next spans exactly the Replay / Home row, so both rows share their left and right edges.
	var pill := FamilyDialog.SECONDARY   # 346 x 130: PRIMARY 162 / 130 = 1.246
	var block := pill.x * 2.0 + WIN_PILL_GAP
	var left := 540.0 - block * 0.5
	var next_size := Vector2(block, ButtonKit.PRIMARY.y)
	var next := ButtonKit.button(c, "action_next", Rect2(Vector2(left, y), next_size), "wide", func(): p.invoke(on_next))
	ButtonKit.word(next.get_node("Content"), "white_action_next", next_size).name = "NextWord"
	var row_y := y + ButtonKit.PRIMARY.y + WIN_ROW_GAP
	_win_pill(c, "Retry", Rect2(Vector2(left, row_y), pill), "white_action_replay", func(): p.invoke(on_replay))
	_win_pill(c, "Home", Rect2(Vector2(left + pill.x + WIN_PILL_GAP, row_y), pill), "white_settings_home",
		func(): p.invoke(on_home))
	UI.set_active(next, next_enabled)
	if live:
		_confetti(p, 0.3)
	return p

## Three rating stars on an arc: the middle one larger and higher, the outer two lower and tilted outwards. Earned
## stars pop in one after another with their chime.
static func _win_stars(parent: Control, y: float, filled: int, animate: bool, delay: float) -> void:
	var size := FamilyDialog.STAR_SIZE
	for i in 3:
		var arc: Vector3 = WIN_STAR_ARC[i]
		var side := size * (WIN_STAR_MIDDLE if i == 1 else 1.0)
		var center := Vector2(540.0 + arc.x * (size * 1.12 + FamilyDialog.STAR_GAP), y + (FamilyDialog.STAR_ROW + WIN_STAR_ROOM) * 0.5 + arc.y)
		var bounds := Rect2(center - Vector2.ONE * side * 0.5, Vector2.ONE * side)
		var star := UI.art(parent, "island_star_full" if i < filled else "island_star_empty", bounds)
		star.name = "RatingStar%d" % i
		star.rotation_degrees = arc.z
		star.set_meta("visible_box", bounds)
		if animate and i < filled:
			var rest := star.position
			star.position.y += 72.0
			star.scale = Vector2.ONE * 0.25
			star.modulate.a = 0.0
			star.rotation_degrees = arc.z - 22.0
			var tween := star.create_tween()
			tween.tween_interval(delay + 0.3 * i)
			tween.tween_callback(Sound.play.bind("star_%d" % (i + 1)))
			tween.tween_property(star, "scale", Vector2.ONE * 1.2, 0.26).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
			tween.parallel().tween_property(star, "position", rest, 0.26).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
			tween.parallel().tween_property(star, "rotation_degrees", arc.z, 0.26).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
			tween.parallel().tween_property(star, "modulate:a", 1.0, 0.1)
			tween.tween_property(star, "scale", Vector2.ONE, 0.18).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)

## A single reward emphasis after counting completes; never changes hit areas or payout.
static func _reward_emphasis(group: Control, delay: float) -> void:
	group.pivot_offset = group.size * 0.5
	var tween := group.create_tween()
	tween.tween_interval(delay)
	tween.tween_property(group, "scale", Vector2.ONE * 1.16, 0.12).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
	tween.tween_property(group, "scale", Vector2.ONE, 0.22).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)

## A wooden secondary pill with the existing painted white lettering.
static func _win_pill(parent: Control, n: String, rect: Rect2, word_id: String, action: Callable) -> Button:
	return JungleDialog.secondary(parent, n, rect, word_id, action)

## Painted numerals (ButtonKit, white with a dark-green outline) in a holder whose "value" meta is the text.
static func _win_digits(parent: Control, n: String, rect: Rect2) -> Control:
	var holder := Control.new()
	holder.name = n
	holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(holder, rect)
	parent.add_child(holder)
	return holder

static func _win_set(holder: Control, text: String) -> void:
	if holder.get_meta("value", "") == text:
		return
	holder.set_meta("value", text)
	for child in holder.get_children():
		holder.remove_child(child)
		child.queue_free()
	ButtonKit.number(holder, text, Rect2(Vector2.ZERO, holder.size), -1)

static func _win_count(holder: Control, amount: int, pattern: String, delay: float, duration: float) -> void:
	_win_set(holder, pattern % 0)
	var tween := holder.create_tween()
	tween.tween_interval(delay)
	tween.tween_method(func(v: float): _win_set(holder, pattern % roundi(v)), 0.0, float(amount), duration)

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
	Simple.close_button(c, Vector2(box.end.x - MenuNavigation.DIALOG_RIGHT_PAD - Simple.CLOSE * 0.5, box.position.y + SET_PAD + Simple.CLOSE * 0.5), p.close)
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
	var column := JungleDialog.WIDTH - 144.0
	var view_h := minf(1080.0, page.get_height() * column / float(page.get_width()) + 8.0)
	var p := JungleDialog.open(host, "", "", [view_h], _instant(host), FamilyDialog.PRIMARY.y, false)
	UiText.painted_label(p._title_layer, "doc.%s.title" % id, Rect2(64, 94, 512, 112), true)
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
	var confirm: Button = FamilyDialog.actions(p, [["green", "check", func(): pass]])[0]
	confirm.name = "Confirm"
	return p

static func level_card(host: Control, level: int, best_stars: int, best_moves: int, par: int, species: Array[String],
		mechanics: Array[String], on_play: Callable, playable: bool, tools: Array = [], stock_of := Callable(),
		on_tool := Callable()) -> Control:
	# The approved jungle frame over the dimmed map, sized to its actual content,
	# with a wide painted Play action proportional to the card, without stretching its lettering.
	var per_row := 4 if species.size() <= 4 else 5
	var tile := LEVEL_TILE if per_row == 4 else LEVEL_TILE_SMALL
	var rows := ceili(species.size() / float(per_row))
	var heights := [FamilyDialog.STAR_ROW, LINE]
	if rows > 0:
		heights.append(rows * tile + (rows - 1) * LEVEL_TILE_GAP)
	if not mechanics.is_empty():
		heights.append(LEVEL_MECHANIC)
	tools = tools.slice(0, LEVEL_TOOLS_MAX)
	if not tools.is_empty():
		heights.append(LEVEL_TOOL_ROW)
	var p := JungleDialog.open(host, FamilyDialog.lettering_key("popup.level.title"), str(level), heights,
		_instant(host), LEVEL_PLAY.y, true, false, true, true)
	var card: Rect2 = p.get_meta("card_rect")
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
		var center := Vector2(540.0 + (i % per_row - (in_row - 1) * 0.5) * (tile + LEVEL_TILE_GAP),
			ys[g] + tile * 0.5 + (i / per_row) * (tile + LEVEL_TILE_GAP))
		_ring(c, species[i], center, _instant(host), tile, Levels.world_of(Levels.get_level(level)))
	if rows > 0:
		g += 1
	if not mechanics.is_empty():
		_mechanic_row(c, mechanics, ys[g])
		g += 1
	if not tools.is_empty():
		p.set_meta("refresh_tools", _tool_row(c, tools, stock_of, on_tool, ys[g]))
	var at := Vector2(540.0 - LEVEL_PLAY.x * 0.5, p.panel_rect.end.y + Modal.ACTION_DROP)
	var play := ButtonKit.button(c, "Play", Rect2(at, LEVEL_PLAY), "wide", func(): p.invoke(on_play))
	var content := play.get_node("Content")
	var group_width := 420.0
	var left := (LEVEL_PLAY.x - group_width) * 0.5
	UI.icon(content, "play", Vector2(left + 42.0, LEVEL_PLAY.y * 0.5), 84.0, Color.WHITE)
	UI.art(content, "label_home_island_play_" + UiText.locale(), Rect2(left + 108.0, (LEVEL_PLAY.y - 120.0) * 0.5, 312, 120))
	UI.set_active(play, playable)
	p.panel_rect = card
	return p

const LEVEL_TILE := 176.0
const LEVEL_TILE_SMALL := 152.0
const LEVEL_TILE_GAP := 20.0
const LEVEL_MECHANIC := 96.0
const LEVEL_PLAY := Vector2(760, 200)   # Wide standalone action under the 960-pixel card

const LEVEL_TOOLS_MAX := 4
const LEVEL_TOOL := 136.0                       # the board's tool slot (BoardView.TOOL_SIZE)
const LEVEL_TOOL_ICON := 100.0                  # BoardView.TOOL_ICON_SIZE
const LEVEL_TOOL_COUNT := 46.0                  # painted stock numerals under the slot (BoardView.TOOL_COUNT_H)
const LEVEL_TOOL_COUNT_GAP := 6.0
const LEVEL_TOOL_GAP := 44.0
const LEVEL_TOOL_ROW := LEVEL_TOOL + LEVEL_TOOL_COUNT_GAP + LEVEL_TOOL_COUNT
## Painted content of each tool picture, as the board's slots crop them (BoardView._restyle_tool).
const LEVEL_TOOL_CONTENT := {"bulb": Rect2(19, 11, 304, 371), "sapling": Rect2(16, 17, 333, 289), "eye": Rect2(17, 12, 366, 359),
	"bell": Rect2(16, 17, 368, 335), "scarecrow": Rect2(16, 16, 387, 395), "rain": Rect2(16, 17, 319, 386)}

## The tools that fit the level as the board's tool slots in one centred row, each with its painted stock ("+" when
## none). Tapping a slot calls `on_tool(kind, refresh)`; `refresh` re-reads every stock from `stock_of(kind)`.
## Returns that refresh callable.
static func _tool_row(c: Control, tools: Array, stock_of: Callable, on_tool: Callable, y: float) -> Callable:
	var row := Control.new()
	row.name = "Tools"
	row.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var n := tools.size()
	var width := n * LEVEL_TOOL + (n - 1) * LEVEL_TOOL_GAP
	UI.place(row, Rect2(540.0 - width * 0.5, y, width, LEVEL_TOOL_ROW))
	c.add_child(row)
	var counts := {}
	var refresh := func():
		for kind in counts:
			var holder: Control = counts[kind]
			if not is_instance_valid(holder):
				continue
			for child in holder.get_children():
				holder.remove_child(child)
				child.queue_free()
			var stock: int = stock_of.call(kind) if stock_of.is_valid() else 0
			ButtonKit.number(holder, str(stock) if stock > 0 else "+", Rect2(Vector2.ZERO, holder.size))
	for i in n:
		var kind: int = tools[i]
		var slot := Button.new()
		slot.name = "Tool_%d" % kind
		slot.flat = true
		slot.focus_mode = Control.FOCUS_NONE
		UI.place(slot, Rect2(i * (LEVEL_TOOL + LEVEL_TOOL_GAP), 0, LEVEL_TOOL, LEVEL_TOOL))
		row.add_child(slot)
		var frame := UI.image(slot, "hud_tool_slot", Rect2(0, 0, LEVEL_TOOL, LEVEL_TOOL), false)
		frame.name = "Slot"
		var icon_name: String = BoardView.TOOL_ICON_NAME[kind]
		var bounds := Rect2(Vector2.ONE * (LEVEL_TOOL - LEVEL_TOOL_ICON) * 0.5, Vector2.ONE * LEVEL_TOOL_ICON)
		var icon := UI.art(slot, UI.PICTURES[icon_name], bounds, LEVEL_TOOL_CONTENT[icon_name])
		icon.name = "Icon"
		slot.button_down.connect(func(): frame.modulate = ButtonKit.PRESSED; icon.modulate = ButtonKit.PRESSED)
		slot.button_up.connect(func(): frame.modulate = Color.WHITE; icon.modulate = Color.WHITE)
		var holder := Control.new()
		holder.name = "Stock"
		holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
		UI.place(holder, Rect2(0, LEVEL_TOOL + LEVEL_TOOL_COUNT_GAP, LEVEL_TOOL, LEVEL_TOOL_COUNT))
		slot.add_child(holder)
		counts[kind] = holder
		slot.pressed.connect(func(): if on_tool.is_valid(): on_tool.call(kind, refresh))
	refresh.call()
	return refresh

## The level's mechanics as plain pictures in one centred row (no pill behind them).
static func _mechanic_row(c: Control, mechanics: Array[String], y: float) -> void:
	var side := 88.0
	var gap := 44.0
	var x := 540.0 - (mechanics.size() * side + (mechanics.size() - 1) * gap) * 0.5
	for i in mechanics.size():
		var art: String = MECHANIC_ART[mechanics[i]]
		var t := _fit(c, art, Rect2(x + i * (side + gap), y + (LEVEL_MECHANIC - side) * 0.5, side, side), true)
		t.name = "Mechanic_" + mechanics[i]
		if art.begins_with("icon_"):
			t.modulate = UI.INK
		if mechanics[i] in ["sleep", "egg", "crow", "mama"]:
			_breathe(t, i * 0.7)

## A bird's card: the bird, then "four of them go home" as pictures (faces, arrow, birdhouse) when its flock's
## homecoming has an effect, then that effect as its badge and painted caption.
const TRIGGER_H := 140.0
const TRIGGER_FACE := 84.0
const TRIGGER_HOUSE := 104.0

const GOAL_CARD_W := 840.0
const GOAL_HERO := 330.0
const GOAL_HEAD := 100.0
const GOAL_GAP := 24.0
const GOAL_BOTTOM := 40.0
const GOAL_ACTION_GAP := 40.0

static func goal_info(host: Control, species: String, world := 1) -> Control:
	var effect: String = EFFECT.get(species, "")
	var heights := [GOAL_HERO, ABILITY_H] if effect.is_empty() else [GOAL_HERO, TRIGGER_H, ABILITY_H]
	# The title remains painted in the accepted skin; only this card's allocation is compacted.
	var body_h := GOAL_HEAD + GOAL_BOTTOM + GOAL_GAP * (heights.size() - 1)
	for height in heights:
		body_h += float(height)
	var total_h := PurchaseDialog.TITLE_ROOM + body_h + GOAL_ACTION_GAP + ButtonKit.PRIMARY.y
	var surface_top := roundf((1920.0 - total_h) * 0.5)
	var p := Modal.open(host, "bird.%s.name" % species, [], false, 0.0, 0.0, _instant(host))
	p.panel_rect = Rect2((1080.0 - GOAL_CARD_W) * 0.5, surface_top + PurchaseDialog.TITLE_ROOM, GOAL_CARD_W, body_h)
	p.content.pivot_offset = Vector2(540, 960)
	FamilyDialog.decorate(p, false)
	var c := p.content
	var y := p.panel_rect.position.y + GOAL_HEAD
	_bird_view(c, species, Rect2(540.0 - GOAL_HERO * 0.5, y, GOAL_HERO, GOAL_HERO), _instant(host), 0.0,
		WorldBirds.look(species, world))
	y += GOAL_HERO + GOAL_GAP
	if not effect.is_empty():
		_trigger_row(c, species, world, y, _instant(host))
		y += TRIGGER_H + GOAL_GAP
	FamilyDialog.ability(c, species, effect, y)
	var action_size := ButtonKit.card_action_size(GOAL_CARD_W)
	var button := ButtonKit.button(c, "Confirm", Rect2(Vector2(540.0 - action_size.x * 0.5,
		p.panel_rect.end.y + GOAL_ACTION_GAP), action_size), "wide", p.close)
	ButtonKit.word(button.get_node("Content"), "white_action_ok", action_size).name = "Word"
	return p

## The trigger row plays the skill as a loop, so the card needs no reading: four faces hop and fly into the house, the
## house bounces, the ability badge pops out and flies to the result cell, and the cell shows the effect (a sleeper
## wakes, an egg shows its bird or hatches, a crow flies off, a branch grows, the undo slot or the move count gets
## +1). Then everything comes back and it plays again. A capture (instant) shows the row at rest.
const DEMO_FACE := 84.0
const DEMO_GAP := 4.0
const DEMO_ARROW := 48.0
const DEMO_PAD := 14.0
const DEMO_CELL := 124.0

static func _trigger_row(c: Control, species: String, world: int, y: float, instant: bool) -> void:
	var effect: String = EFFECT.get(species, "")
	var width := 4 * DEMO_FACE + 3 * DEMO_GAP + 2 * (DEMO_PAD + DEMO_ARROW + DEMO_PAD) + TRIGGER_HOUSE + DEMO_CELL
	var x := 540.0 - width * 0.5
	var mid := y + TRIGGER_H * 0.5
	var row := Control.new()
	row.name = "Trigger"
	row.mouse_filter = Control.MOUSE_FILTER_IGNORE
	c.add_child(row)
	var faces: Array[TextureRect] = []
	for i in 4:
		var face := UI.art(row, WorldBirds.face(species, world), Rect2(x + i * (DEMO_FACE + DEMO_GAP), mid - DEMO_FACE * 0.5, DEMO_FACE, DEMO_FACE))
		face.name = "Face%d" % i
		face.pivot_offset = face.size * 0.5
		faces.append(face)
	var after := x + 4 * DEMO_FACE + 3 * DEMO_GAP + DEMO_PAD
	UI.icon(row, "right", Vector2(after + DEMO_ARROW * 0.5, mid), DEMO_ARROW, UI.FILL["orange"]).name = "Arrow"
	var house := UI.art(row, "house_default", Rect2(after + DEMO_ARROW + DEMO_PAD, mid - TRIGGER_HOUSE * 0.5, TRIGGER_HOUSE, TRIGGER_HOUSE))
	house.name = "House"
	house.pivot_offset = Vector2(house.size.x * 0.5, 0.0)
	var after2 := house.position.x + TRIGGER_HOUSE + DEMO_PAD
	UI.icon(row, "right", Vector2(after2 + DEMO_ARROW * 0.5, mid), DEMO_ARROW, UI.FILL["orange"]).name = "Arrow2"
	var cell := Rect2(after2 + DEMO_ARROW + DEMO_PAD, mid - DEMO_CELL * 0.5, DEMO_CELL, DEMO_CELL)
	var frame := Control.new()   # the result cell's box (layout only)
	frame.name = "Cell"
	frame.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(frame, cell)
	row.add_child(frame)
	var result := Control.new()
	result.name = "Result"
	result.mouse_filter = Control.MOUSE_FILTER_IGNORE
	result.set_meta("world", world)
	row.add_child(result)
	_demo_result(result, effect, species, world, cell, true)
	if instant:
		return
	var door := house.get_rect().get_center() + Vector2(0, 8)
	var badge := UI.art(row, "gift_coin_" + effect, Rect2(door - Vector2.ONE * 32.0, Vector2.ONE * 64.0))
	badge.name = "Badge"
	badge.pivot_offset = badge.size * 0.5
	badge.visible = false
	var loop := row.create_tween().set_loops()
	loop.tween_interval(0.5)
	for i in 4:   # hop in turn
		var face := faces[i]
		loop.tween_property(face, "scale", Vector2(1.15, 0.85), 0.07)
		loop.tween_property(face, "scale", Vector2(0.9, 1.15), 0.08)
		loop.tween_property(face, "scale", Vector2.ONE, 0.08)
	for i in 4:   # fly into the doorway, one after another
		var face := faces[i]
		var home := door - face.size * 0.5
		(loop if i == 0 else loop.parallel()).tween_property(face, "position", home, 0.45).set_delay(0.1 + i * 0.09).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN)
		loop.parallel().tween_property(face, "scale", Vector2.ONE * 0.35, 0.45).set_delay(0.1 + i * 0.09)
		loop.parallel().tween_property(face, "modulate:a", 0.0, 0.12).set_delay(0.45 + i * 0.09)
	loop.tween_interval(0.05)
	loop.tween_property(house, "scale", Vector2(1.15, 0.88), 0.08)   # the house bounces as they arrive
	loop.tween_property(house, "scale", Vector2(0.95, 1.08), 0.1)
	loop.tween_property(house, "scale", Vector2.ONE, 0.16).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	loop.tween_callback(func():   # the badge pops out of the door
		badge.position = door - badge.size * 0.5
		badge.scale = Vector2.ONE * 0.2
		badge.modulate.a = 1.0
		badge.visible = true)
	loop.tween_property(badge, "scale", Vector2.ONE * 1.3, 0.18).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	loop.tween_interval(0.15)
	var arc := func(t: float) -> void:
		var a := door
		var b := cell.get_center()
		var top := (a + b) * 0.5 + Vector2(0, -90)
		badge.position = a.lerp(top, t).lerp(top.lerp(b, t), t) - badge.size * 0.5
		badge.scale = Vector2.ONE * lerpf(1.3, 0.8, t)
	loop.tween_method(arc, 0.0, 1.0, 0.4).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	loop.tween_callback(func():
		badge.visible = false
		_demo_play(result, effect, cell))
	loop.tween_interval(1.4)
	loop.tween_callback(func():   # everything back for the next round
		for i in 4:
			faces[i].position = Vector2(x + i * (DEMO_FACE + DEMO_GAP), mid - DEMO_FACE * 0.5)
			faces[i].scale = Vector2.ONE
		_demo_result(result, effect, species, world, cell, false))
	for i in 4:
		loop.parallel().tween_property(faces[i], "modulate:a", 1.0, 0.3)

## The "before" picture of a skill's result cell. `first` builds it for the card; later rounds rebuild it.
static func _demo_result(holder: Control, effect: String, species: String, world: int, cell: Rect2, first: bool) -> void:
	for child in holder.get_children():
		holder.remove_child(child)
		child.queue_free()
	var other := "sky" if species != "sky" else "sunny"
	match effect:
		"wake":
			_demo_bird(holder, other, cell, Board.ASLEEP, true, world).name = "Sleeper"
		"reveal", "hatch":
			var egg := _demo_bird(holder, other, cell, Board.EGG, false, world)
			egg.name = "Egg"
		"scare":
			_demo_bird(holder, "crow", cell, 0, false, world).name = "Crow"
		"grow":
			var sprout := UI.art(holder, UI.PICTURES["sapling"], cell.grow(-18.0))
			sprout.name = "Sprout"
			sprout.pivot_offset = Vector2(sprout.size.x * 0.5, sprout.size.y)
			sprout.scale = Vector2.ONE * 0.55
		"undo":
			UI.art(holder, "island_icon_undo", cell.grow(-12.0), UI.used_rect("island_icon_undo")).name = "Undo"
		"encore":
			UI.art(holder, "icon_star_budget_steps", cell.grow(-12.0), Rect2(317, 217, 652, 801)).name = "Steps"

## A live bird for the result cell (breathing like every bird), fitted into `box` by its visible body.
static func _demo_bird(holder: Control, species: String, box: Rect2, flags: int, asleep: bool, world: int) -> BirdView:
	var look := WorldBirds.look(species, world) if species != "crow" else ""
	var used := UI.used_rect((look if not look.is_empty() else species) + "_front")
	var k := minf(box.size.x / used.size.x, box.size.y / used.size.y) * 0.9
	var bird := BirdView.new()
	holder.add_child(bird)
	bird.position = Vector2(box.get_center().x - (used.get_center().x - BirdView.CANVAS * 0.5) * k,
		box.end.y - (used.end.y - BirdView.FEET_Y) * k)
	bird.setup(Board.bird(0, Board.species_index(species), flags), asleep, k * BirdView.CANVAS / BirdView.DISPLAY, look)
	bird.quiet = true
	return bird

## The effect landing in the result cell, with a sparkle flash; a "+1" for the undo slot and the move count.
static func _demo_play(holder: Control, effect: String, cell: Rect2) -> void:
	var flash := UI.image(holder, "glow_soft", cell.grow(cell.size.x * 0.35))
	flash.modulate = Color(1.0, 0.9, 0.45, 0.0)
	var f := flash.create_tween()
	f.tween_property(flash, "modulate:a", 0.95, 0.1)
	f.tween_property(flash, "modulate:a", 0.0, 0.5)
	match effect:
		"wake":
			var bird: BirdView = holder.get_node_or_null("Sleeper")
			if bird:
				bird.wake()
				bird.get_tree().create_timer(0.4).timeout.connect(func(): if is_instance_valid(bird): bird.cheer())
		"reveal":
			var egg: BirdView = holder.get_node_or_null("Egg")
			if egg:
				egg.reveal()
				var face := UI.art(holder, WorldBirds.face(Board.SPECIES[Board.species_of(egg.code)], holder.get_meta("world", 1)),
					Rect2(cell.get_center() + Vector2(-28, -cell.size.y * 0.5 - 50), Vector2(56, 56)))
				face.pivot_offset = face.size * 0.5
				face.scale = Vector2.ONE * 0.2
				face.create_tween().tween_property(face, "scale", Vector2.ONE, 0.25).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
		"hatch":
			var egg: BirdView = holder.get_node_or_null("Egg")
			if egg:
				egg.hatch()
		"scare":
			var crow: BirdView = holder.get_node_or_null("Crow")
			if crow:
				var tw := crow.create_tween()
				tw.tween_property(crow, "position", crow.position + Vector2(150, -140), 0.6).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN)
				tw.parallel().tween_property(crow, "modulate:a", 0.0, 0.6)
		"grow":
			var sprout: Control = holder.get_node_or_null("Sprout")
			if sprout:
				var tw := sprout.create_tween()
				tw.tween_property(sprout, "scale", Vector2.ONE * 1.15, 0.3).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
				tw.tween_property(sprout, "scale", Vector2.ONE, 0.2)
		"undo", "encore":
			var icon: Control = holder.get_child(0) as Control
			icon.pivot_offset = icon.size * 0.5
			var tw := icon.create_tween()
			tw.tween_property(icon, "scale", Vector2(1.3, 0.8), 0.08)
			tw.tween_property(icon, "scale", Vector2(0.9, 1.15), 0.1)
			tw.tween_property(icon, "scale", Vector2.ONE, 0.3).set_trans(Tween.TRANS_ELASTIC).set_ease(Tween.EASE_OUT)
			var plus := Control.new()
			plus.mouse_filter = Control.MOUSE_FILTER_IGNORE
			holder.add_child(plus)
			var h := 60.0
			plus.size = Vector2(ButtonKit.number_width("+1", h), h)
			plus.position = Vector2(cell.get_center().x - plus.size.x * 0.5, cell.position.y - h - 4.0)
			plus.pivot_offset = plus.size * 0.5
			ButtonKit.number(plus, "+1", Rect2(Vector2.ZERO, plus.size))
			plus.scale = Vector2.ONE * 0.3
			var up := plus.create_tween()
			up.tween_property(plus, "scale", Vector2.ONE * 1.2, 0.18).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
			up.tween_property(plus, "scale", Vector2.ONE, 0.12)
			up.tween_property(plus, "position:y", plus.position.y - 30.0, 0.6)
			up.parallel().tween_property(plus, "modulate:a", 0.0, 0.4).set_delay(0.3)

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

const PAGE_SIGN := Rect2(240, 96, 600, 209)   # the page title sign (as the map's world page)

## Re-dresses an existing action (hit area and callback unchanged) as the shop's green plate with a painted white word,
## always at ButtonKit.PRIMARY on screen (`coordinate_scale`: the scale of the button's parent canvas).
static func _kit_action(button: Control, word_id: String, coordinate_scale := 1.0) -> void:
	for child in button.get_children():
		if child is CanvasItem:
			(child as CanvasItem).visible = false
	var kit := Control.new()
	kit.name = "Kit"
	kit.mouse_filter = Control.MOUSE_FILTER_IGNORE
	kit.size = ButtonKit.PRIMARY
	kit.scale = Vector2.ONE / coordinate_scale
	kit.position = (button.size - ButtonKit.PRIMARY / coordinate_scale) * 0.5
	kit.pivot_offset = ButtonKit.PRIMARY * 0.5
	button.add_child(kit)
	var face := ButtonKit.face(kit, "green", Rect2(Vector2.ZERO, ButtonKit.PRIMARY))
	ButtonKit.word(kit, word_id, face.size).position += face.position
	if button is BaseButton:
		(button as BaseButton).button_down.connect(func(): kit.modulate = ButtonKit.PRESSED; kit.scale = Vector2.ONE * ButtonKit.SINK / coordinate_scale)
		(button as BaseButton).button_up.connect(func(): kit.modulate = Color.WHITE; kit.scale = Vector2.ONE / coordinate_scale)

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
	# The hanging title sign of the shop and map pages, at the map's page position.
	p._title_layer = ButtonKit.sign(p.content, "lucky_spin_title", PAGE_SIGN)
	var close := MenuNavigation.close(c, "Close", Vector2(23, 52), p.close, DrawPurchaseArt.SCALE)
	var stars := []
	for i in slots:
		var x := 458.0 + (i - (slots - 1) * 0.5) * 122.0
		# Both star states use the same visible box; only their accepted texture changes.
		# A used draw is the same gold star faded to warm tan (never a second colour family on the warm page).
		var key := "star_draw_available" if i < spins else "star_draw_spent"
		var star := DrawPurchaseArt.fit(c, key, Rect2(x - 54, 341, 108, 104))
		star.name = "Attempt%d" % i
		stars.append(star)
	var board := Control.new()
	board.name = "DrawBoard"
	board.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(board, DRAW_BOARD)
	c.add_child(board)
	# No backing sheet (docs/design/island-style.md): each prize is its own cream card floating on the forest.
	for i in 9:
		var cell := Rect2(DRAW_ORIGIN + Vector2(i % 3, i / 3) * DRAW_STEP, DRAW_CELL)
		ButtonKit.panel(board, cell.grow(-4.0)).name = "Slot%d" % i
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
		var prize_art := UI.image(card, DRAW_PRIZE_KEYS[i], DRAW_PICTURES[i])
		prize_art.name = "PrizeArt"
		if DRAW_PRIZE_KEYS[i] == "icon_draw_prize_four_hundred":
			prize_art.texture_filter = CanvasItem.TEXTURE_FILTER_LINEAR_WITH_MIPMAPS
		FamilyDialog.digits(card, "Number", Rect2(14, 159, 212, 51), str(prize[0]) if prize[0] > 0 else "×%d" % prize[1])
		cards.append(card)
	var chest := DrawPurchaseArt.fit(board, "icon_draw_center_chest", Rect2(311, 305, 216, 224))
	chest.name = "Chest"
	chest.pivot_offset = chest.size * 0.5
	if not instant:
		_breathe(chest, 0.0)
		p.appear_page(background)
	var state := {"left": spins, "busy": false, "at": 0}
	var go := DrawPurchaseArt.button(c, "Draw", "btn_draw_action", Rect2(182, 1366, 553, 202), func(): pass, true)
	_kit_action(go, "white_spin", DrawPurchaseArt.SCALE)
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
		if not Progress.unlimited:   # the launcher's test mode keeps every star lit
			state["left"] -= 1
			var used: Control = stars[state["left"]]
			(used as TextureRect).texture = Art.tex("star_draw_spent")
		var highlight := func(at: int):
			lights[state["at"]].modulate.a = 0.0
			for i in cards.size():
				cards[i].pivot_offset = cards[i].size * 0.5
				cards[i].scale = Vector2.ONE * (1.06 if i == at else 1.0)
				cards[i].modulate.a = 1.0 if i == at else 0.72
			lights[at].modulate.a = 1.0
			state["at"] = at
		var reveal := func():
			# The winning card jumps; the reward chime itself plays with the prize on the result page.
			Sound.play("star_3")
			if not instant:
				var won := cards[target] as Control
				won.pivot_offset = won.size * 0.5
				var rest := won.position
				var pop := won.create_tween()
				pop.tween_property(won, "scale", Vector2.ONE * 1.14, 0.16).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
				pop.parallel().tween_property(won, "position:y", rest.y - 22.0, 0.16).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
				pop.tween_property(won, "scale", Vector2.ONE, 0.24).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
				pop.parallel().tween_property(won, "position", rest, 0.24).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
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
				for card in cards:
					card.scale = Vector2.ONE
					card.modulate.a = 1.0
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
		var steps: int = 16 + posmod(target - int(state["at"]), 8)
		var start: int = state["at"]
		for n in steps:
			var at := posmod(start + n + 1, 8)
			roll.tween_callback(highlight.bind(at))
			roll.tween_callback(Sound.play.bind("ui_tap", 1.0 + 0.03 * n))
			var t := float(n) / float(steps - 1)
			roll.tween_interval(0.04 + 0.2 * t * t * t)
		roll.tween_callback(reveal)
		roll.tween_interval(0.55)
		roll.tween_callback(finish)
	(go as Button).pressed.connect(run)
	return p

## Daily reward: the seven-day login calendar as a page in the Shop and Lucky Spin family. As on every page, content
## floats over the forest on its own cream cards (docs/design/island-style.md: no backing sheet, no card in a card):
## days 1-6 are two columns of shop-sized cards, day 7 is one full-width card under them, Claim sits below.
## Each card: the day number on a green disc at its corner, the prize pictures with their amounts. Claimed days are
## dimmed with a tick; today's card has the shop's green selected outline and bounces until it is claimed.
## `claim` pays today and returns {"day", "coins", "boosters"} like Progress.claim_daily_reward, or {} when already
## claimed. The tick stamps in, coins burst, then the rewards dialog names what was won; its OK closes the page.
const DAILY_AREA := Rect2(48, 352, 984, 1196)
const DAILY_GAP := 20.0
const DAILY_ACTION := Rect2(260, 1590, 560, 208)   # hit area around the ButtonKit.PRIMARY plate
const DAILY_DISC := 76.0                # the day disc, the shop's stock badge size
const DAILY_OUTLINE := Color8(63, 181, 43)   # the shop's selected green
## Coin pile pictures of the draw board, from the smallest amount they stand for.
const DAILY_COIN_ART := [[400, "icon_draw_prize_four_hundred"], [200, "icon_draw_prize_two_hundred"],
	[120, "icon_draw_prize_one_twenty"], [80, "icon_draw_prize_eighty"], [60, "icon_draw_prize_sixty"], [0, "icon_draw_prize_forty"]]

const DAILY_SETTLE := 0.3   # the counter's bump after the last coin

static func daily_reward(host: Control, today: int, available: bool, balance: int, claim: Callable, read_balance := Callable()) -> Control:
	var instant := _instant(host)
	var p := Modal.open(host, "", [], false, 0.0, 0.0, true)
	p.name = "DailyReward"
	p.panel_rect = Rect2(0, 0, 1080, 1920)
	p.content.pivot_offset = Vector2(540, 960)
	for node_name in ["Panel", "TitleGroup"]:
		var old := p.content.get_node(node_name)
		p.content.remove_child(old)
		old.queue_free()
	var background := DrawPurchaseArt.background(p)
	p.move_child(background, 1)
	p._title_layer = ButtonKit.sign(p.content, "daily_reward_title", PAGE_SIGN)
	MenuNavigation.close(p.content, "Close", Vector2(48, 48), p.close)
	var bar := FeatherBar.create_right(p.content, Vector2(1032, 48), balance, ShopSkin.COUNTER_INK)
	# A live page reads the current profile after a top-up; previews retain their supplied balance.
	p.set_meta("refresh_balance", func():
		if read_balance.is_valid():
			bar.set_amount(int(read_balance.call())))
	var days := Progress.DAILY_REWARDS.size()
	var rows := (days - 1) / 2 + 1
	var card_h := (DAILY_AREA.size.y - DAILY_GAP * (rows - 1)) / rows
	var card_w := (DAILY_AREA.size.x - DAILY_GAP) * 0.5
	var tiles := []
	for i in days:
		var day := i + 1
		var r := Rect2(DAILY_AREA.position + Vector2((i % 2) * (card_w + DAILY_GAP), (i / 2) * (card_h + DAILY_GAP)), Vector2(card_w, card_h))
		if day == days:
			r.size.x = DAILY_AREA.size.x
		var claimed := day < today or (day == today and not available)
		tiles.append(_daily_card(p.content, day, r, claimed, day == today and available, instant))
	var go := UI.hit_area(p.content, DAILY_ACTION, func(): pass)
	go.name = "Claim"
	_kit_action(go, "white_claim")
	UI.set_active(go, available)
	if not instant:
		p.appear_page(background)
	var state := {"busy": false}
	go.pressed.connect(func() -> void:
		if state["busy"] or p.content.modulate.a < 0.99:
			return
		var got: Dictionary = claim.call()
		if got.is_empty():
			return
		state["busy"] = true
		Sound.play("ui_tap")
		UI.set_active(go, false)
		var tile: Control = tiles[int(got["day"]) - 1]
		_daily_stamp(tile, instant)
		# The reward chime plays with the prizes in the rewards popup, not before it is on screen.
		var total: int = int(read_balance.call()) if read_balance.is_valid() else balance + int(got["coins"])
		var wait := 1.1   # the stamp alone
		if instant or int(got["coins"]) <= 0:
			bar.set_amount(total, true)
		else:
			# The rewards card opens only after the last coin has landed and the counter has settled.
			wait = coin_flight(p.content, tile.position + tile.size * 0.5, bar.position + Vector2(56, bar.size.y * 0.5), 0.2,
				func(): bar.set_amount(int(read_balance.call()) if read_balance.is_valid() else total, true)) + DAILY_SETTLE
		if not instant:
			await p.get_tree().create_timer(wait).timeout
		if not is_instance_valid(p) or p.is_queued_for_deletion() or p._closing:
			return
		rewards(host, "daily_reward.title", int(got["coins"]), got["boosters"], p.close, "pic_calendar"))
	return p

## One day: a cream card on the forest with its day disc, prize pictures and amounts; claimed, today or later.
static func _daily_card(parent: Control, day: int, r: Rect2, claimed: bool, today: bool, instant: bool) -> Control:
	var tile := Control.new()
	tile.name = "Day%d" % day
	tile.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(tile, r)
	tile.pivot_offset = r.size * 0.5
	parent.add_child(tile)
	var body := Control.new()
	body.name = "Body"
	body.mouse_filter = Control.MOUSE_FILTER_IGNORE
	body.size = r.size
	tile.add_child(body)
	ButtonKit.panel(body, Rect2(Vector2.ZERO, r.size)).name = "Surface"
	var prize: Array = Progress.DAILY_REWARDS[day - 1]
	var coins: int = prize[0]
	var gifts: int = prize[1]
	var big := day == Progress.DAILY_REWARDS.size()
	var coin_art := "icon_draw_prize_forty"
	for step in DAILY_COIN_ART:
		if coins >= int(step[0]):
			coin_art = step[1]
			break
	var items := [[coin_art, str(coins)]]
	if gifts > 0:
		items.append(["icon_draw_mystery_double" if gifts > 1 else "icon_draw_mystery_single", "×%d" % gifts])
	# Pictures in one row, centred in the card, each with its amount beneath; the big day gets wider columns.
	var column := 240.0 if big else (r.size.x - 72.0) / items.size()
	var left := (r.size.x - column * items.size()) * 0.5
	var art_h := r.size.y - 132.0
	if big:
		var glow := UI.image(body, "glow_soft", Rect2(r.size.x * 0.5 - 300.0, -10.0, 600.0, r.size.y + 20.0))
		glow.modulate = Color(1.0, 0.86, 0.42, 0.55)
		for k in 4:
			var at := Vector2([150.0, 790.0, 220.0, 720.0][k], [60.0, 70.0, 200.0, 190.0][k])
			var spark := UI.art(body, "sparkle", Rect2(at, Vector2(48, 48)))
			spark.pivot_offset = Vector2(24, 24)
			if not instant:
				_twinkle(spark, k * 0.35)
	for k in items.size():
		var art := _fit(body, items[k][0], Rect2(left + k * column + 10.0, 36.0, column - 20.0, art_h), true)
		art.name = "Prize%d" % k
		if items[k][0] == "icon_draw_prize_four_hundred":
			art.texture_filter = CanvasItem.TEXTURE_FILTER_LINEAR_WITH_MIPMAPS
		if today and not instant:
			_breathe(art, k * 0.4)
		FamilyDialog.digits(body, "Amount%d" % k, Rect2(left + k * column, 36.0 + art_h + 10.0, column, 58), items[k][1])
	# The day number on the shop's green disc at the card's top-left corner.
	var disc := Control.new()
	disc.name = "DayDisc"
	disc.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(disc, Rect2(Vector2(14, 14), Vector2.ONE * DAILY_DISC))
	body.add_child(disc)
	UI.image(disc, "island_badge_green", Rect2(Vector2.ZERO, disc.size), false)
	ButtonKit.number(disc, str(day), Rect2(DAILY_DISC * 0.14, DAILY_DISC * 0.2, DAILY_DISC * 0.72, DAILY_DISC * 0.56)).name = "DayNumber"
	if claimed:
		_daily_claimed(tile)
	elif today:
		var lit := Panel.new()
		lit.name = "Today"
		lit.mouse_filter = Control.MOUSE_FILTER_IGNORE
		var outline := StyleBoxFlat.new()
		outline.draw_center = false
		outline.border_color = DAILY_OUTLINE
		outline.set_border_width_all(8)
		outline.set_corner_radius_all(48)
		outline.shadow_color = Color(0.25, 0.71, 0.17, 0.45)
		outline.shadow_size = 10
		lit.add_theme_stylebox_override("panel", outline)
		UI.place(lit, Rect2(Vector2(-4, -4), r.size + Vector2(8, 8)))
		tile.add_child(lit)
		if not instant:
			var bounce := tile.create_tween().set_loops()
			bounce.tween_property(tile, "scale", Vector2.ONE * 1.03, 0.6).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
			bounce.tween_property(tile, "scale", Vector2.ONE, 0.6).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
			tile.set_meta("bounce", bounce)
	return tile

## A claimed day: the card dims like a disabled item and the shop's tick sits on its top-right corner.
static func _daily_claimed(tile: Control) -> TextureRect:
	(tile.get_node("Body") as Control).modulate.a = UI.DISABLED_ALPHA
	var tick := Island.check_badge(tile, Vector2(tile.size.x - 44.0, 44.0))
	tick.name = "Tick"
	tick.pivot_offset = tick.size * 0.5
	return tick

## Today's card is claimed: the bounce and outline stop, the card pops and the tick stamps in.
static func _daily_stamp(tile: Control, instant: bool) -> void:
	if tile.has_meta("bounce"):
		(tile.get_meta("bounce") as Tween).kill()
	if tile.has_node("Today"):
		tile.get_node("Today").queue_free()
	var tick := _daily_claimed(tile)
	if instant:
		tile.scale = Vector2.ONE
		return
	var pop := tile.create_tween()
	pop.tween_property(tile, "scale", Vector2.ONE * 1.08, 0.14).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	pop.tween_property(tile, "scale", Vector2.ONE, 0.22).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)
	tick.scale = Vector2.ONE * 2.2
	tick.modulate.a = 0.0
	var stamp := tick.create_tween().set_parallel()
	stamp.tween_property(tick, "scale", Vector2.ONE, 0.3).set_delay(0.12).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	stamp.tween_property(tick, "modulate:a", 1.0, 0.15).set_delay(0.12)

const CHAPTERS_PER_PAGE := 4
const CHAPTER_TAG := Vector2(216.0, 76.0)

## Chapter browser: one row per world (painting, "Chapter N", level range and stars, then a worded pill). Tapping an unlocked row
## calls `on_pick(world)`; a locked row is dimmed and does nothing.
static func chapters(host: Control, p: Profile, on_pick: Callable, page := -1) -> Control:
	var row_h := 236.0
	var gap := 22.0
	if page < 0:   # opens on the page holding the player's current chapter
		page = (Progress.world_of_level(Progress.resume_level(p)) - 1) / CHAPTERS_PER_PAGE
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
const CHEST_ACTION := Rect2(260, 1440, 560, 208)   # hit area around the ButtonKit.PRIMARY plate
const CHEST_RESULT_HERO := Rect2(270, 792, 540, 540)
const CHEST_RESULT_SUPPORT := Rect2(145, 1242, 790, 214)
# Ignore invisible generation padding when positioning the original, unchanged RGBA pictures.
const CHEST_CLOSED_INK := Rect2(33, 172, 1189, 933)
const CHEST_OPEN_INK := Rect2(101, 90, 1031, 1012)
const CHEST_SUPPORT_INK := Rect2(43, 112, 2082, 559)
const ChestLettering := preload("res://game/chest_lettering.gd")
const CHEST_BOOSTER_ART := ["firefly", "sprout", "owl_eye", "bell", "scarecrow", "rain"]
## Source-alpha bounds, verified against source PNGs in the chest regression (no GPU readback at reward time).
const CHEST_REWARD_INK := {
	"coin": Rect2(3, 2, 90, 92),
	"island_icon_firefly": Rect2(17, 11, 307, 371),
	"island_icon_sprout": Rect2(16, 17, 333, 289),
	"island_icon_owl_eye": Rect2(17, 12, 367, 359),
	"island_icon_bell": Rect2(16, 17, 368, 335),
	"island_icon_scarecrow": Rect2(16, 16, 387, 395),
	"island_icon_rain": Rect2(16, 17, 319, 386)}

static func chest(host: Control, progress: int, ready: int, on_open: Callable, on_play := Callable(), balance := -1) -> Control:
	var p := _chest_page(host, balance, true)
	_chest_hero(p, false, ready > 0)
	ChestOpening.prepare()
	# Random reward art must not decode in the opening-complete callback.
	Art.tex("chest_star_open")
	for reward_art in CHEST_BOOSTER_ART:
		Art.tex("island_icon_" + reward_art)
	var count := ChestLettering.number(p.content, str(maxi(0, ready)), Rect2(400, 552, 280, 104))
	count.name = "ReadyCount"
	var collected := clampi(progress, 0, Progress.CHEST_STARS)
	_chest_progress(p.content, float(collected) / Progress.CHEST_STARS,
		"%d/%d" % [collected, Progress.CHEST_STARS])
	var open := _chest_action(p, "OpenChest", "chest.open", CHEST_ACTION, func():
		if ready <= 0 or p.has_meta("opening"):
			return
		p.set_meta("opening", true)
		for action_name in ["OpenChest", "Play"]:
			var action := p.content.get_node_or_null(action_name) as Button
			if action != null:
				action.disabled = true
		count.hide()
		if _instant(host):
			p.invoke(on_open)
		else:
			ChestOpening.play(p, func(): p.invoke(on_open)), false)
	UI.set_active(open, ready > 0)
	if ready <= 0:
		var disabled_ink := ShaderMaterial.new()
		disabled_ink.shader = preload("res://game/chest_action_disabled.gdshader")
		open.get_node("Face").material = disabled_ink
		open.modulate = Color(0.76, 0.79, 0.75, 1.0)
	if on_play.is_valid():
		_chest_action(p, "Play", "action.play", Rect2(260, 1672, 560, 208), on_play)
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
static func _chest_page(host: Control, balance: int, close_button: bool, animate := true) -> Modal:
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
	p._title_layer = ButtonKit.sign(p.content, "star_chest_title", PAGE_SIGN)
	if close_button:
		MenuNavigation.close(p.content, "Close", Vector2(48, 48), p.close)
	if balance >= 0:
		FeatherBar.create_right(p.content, Vector2(1032, 48), balance, ShopSkin.COUNTER_INK)
	if animate and not _instant(host):
		p.appear_page(background)
	return p

static func _chest_hero(p: Modal, opened: bool, sparkling: bool, result := false) -> void:
	var live := not _instant(p.get_parent())
	var hero := CHEST_RESULT_HERO if result else (CHEST_READY_HERO if opened else CHEST_HERO)
	var rays := UI.image(p.content, "light_rays", Rect2(hero.get_center() - Vector2(480, 480), Vector2(960, 960)))
	rays.modulate.a = 0.52 if sparkling else 0.3
	var glow := UI.image(p.content, "glow_soft", hero.grow(80.0))
	glow.modulate = Color(1.0, 0.86, 0.4, 0.55)
	var support := UI.art(p.content, "chest_islet_support", CHEST_RESULT_SUPPORT if result else CHEST_SUPPORT, CHEST_SUPPORT_INK)
	support.name = "ChestSupport"
	var art := ChestLid.new()
	art.name = "ChestHero"
	p.content.add_child(art)
	art.setup(hero.end.y, hero.size.x)
	art.angle = ChestLid.OPEN_ANGLE if opened else ChestLid.CLOSED_ANGLE
	if live:
		rays.create_tween().set_loops().tween_property(rays, "rotation", TAU, 28.0).from(0.0)
		var rest := art.scale
		var breath := art.create_tween().set_loops()
		breath.tween_property(art, "scale", rest * Vector2(0.99, 1.025), 1.3).set_trans(Tween.TRANS_SINE)
		breath.tween_property(art, "scale", rest, 1.3).set_trans(Tween.TRANS_SINE)
		art.set_meta("breath", breath)
	if sparkling:
		var sparks := [Vector2(-320, -180), Vector2(300, -140), Vector2(280, 140), Vector2(-300, 160)]
		for i in sparks.size():
			var spark := UI.art(p.content, "sparkle", Rect2(hero.get_center() + sparks[i], Vector2(56, 56)))
			if live:
				_twinkle(spark, i * 0.35)

static func _chest_action(p: Modal, name: String, word: String, bounds: Rect2, callback: Callable, closes := true) -> Button:
	var button := UI.hit_area(p.content, bounds, func():
		# An Open double-tap must not click through to the newly appearing reward confirmation.
		if p.content.modulate.a < 0.99 or Time.get_ticks_msec() < int(p.get_meta("actions_ready_at", 0)):
			return
		Sound.play("ui_tap")
		p.invoke(callback, closes))
	button.name = name
	# A hidden "Face" keeps the disabled-ink hook; the visible face is the shop's green plate.
	var face := Control.new()
	face.name = "Face"
	face.mouse_filter = Control.MOUSE_FILTER_IGNORE
	button.add_child(face)
	_kit_action(button, {"chest.open": "white_open", "action.play": "white_play", "action.ok": "white_action_ok"}.get(word, word))
	return button

static func _chest_rewards(host: Control, coins: int, boosters: Array, on_ok: Callable, balance: int) -> Control:
	var previous: Modal = null
	for child in host.get_children():
		if child is Modal and child.get_meta("chest_landed", false):
			previous = child
	var p := _chest_page(host, balance, false, previous == null)
	# A new confirmation cannot receive the tap burst that opened the chest.
	p.set_meta("actions_ready_at", Time.get_ticks_msec() + 300)
	if previous != null:
		# The transaction already ran. Replace the finished page without two forests/rays fading over each other.
		previous.hide()
	_chest_hero(p, true, true, true)
	var prizes: Array = []
	if coins > 0:
		prizes.append(["coin", coins])
	var counts := {}
	for kind in boosters:
		counts[kind] = int(counts.get(kind, 0)) + 1
	for kind in counts:
		prizes.append(["island_icon_" + CHEST_BOOSTER_ART[kind], counts[kind]])
	var shown := []
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
		var art := _fit(prize, prizes[i][0], Rect2(8, 0, spacing - 16, 190), true, CHEST_REWARD_INK[prizes[i][0]])
		if not _instant(host):
			_breathe(art, i * 0.4)
		FamilyDialog.digits(prize, "Number", Rect2(4, 204, spacing - 8, 56), "+%d" % prizes[i][1])
		shown.append(prize)
	if not _instant(host):
		_prize_entrance(p, shown, 0.25)
	_chest_action(p, "ConfirmRewards", "action.ok", Rect2(260, 1520, 560, 208), on_ok)
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
		prizes.append(["icon_purchase_feather", coins])
	var counts := {}
	for kind in boosters:
		counts[kind] = int(counts.get(kind, 0)) + 1
	for kind in counts:
		prizes.append(["island_icon_%s" % load("res://game/shop_view.gd").BOOSTER_ILLUS[kind], counts[kind]])
	var rows := ceili(prizes.size() / 3.0)
	var hero_h := 320.0 if rows > 1 else ILLUS
	var reward_h := REWARD_TILE * rows + 20.0 * maxf(0.0, rows - 1)
	var p := JungleDialog.open(host, FamilyDialog.lettering_key(title_id), "",
		[hero_h, reward_h] if not prizes.is_empty() else [hero_h], not live, FamilyDialog.PRIMARY.y, false, false, true, false,
		880.0 if prizes.size() >= 3 else 840.0, 72.0)
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
	var shown := []
	for i in prizes.size():
		var row: int = i / 3
		var columns := mini(3, prizes.size() - row * 3)
		var spacing := 250.0
		var x := 540.0 + (i % 3 - (columns - 1) * 0.5) * spacing
		var y: float = ys[1] + row * (REWARD_TILE + 20.0)
		var prize := Control.new()
		prize.name = "Reward%d" % i
		prize.mouse_filter = Control.MOUSE_FILTER_IGNORE
		prize.set_meta("amount", prizes[i][1])
		UI.place(prize, Rect2(x - spacing * 0.5, y, spacing, REWARD_TILE))
		c.add_child(prize)
		var item := _fit(prize, prizes[i][0], Rect2(20, 0, spacing - 40, 170), true)
		if live:
			_breathe(item, i * 0.4)
		FamilyDialog.digits(prize, "Number", Rect2(8, 190, spacing - 16, 56), "+%d" % prizes[i][1])
		shown.append(prize)
	if live:
		_prize_entrance(p, shown, 0.3)
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
	# The hanging title sign of the shop and map pages, at the map's page position.
	p._title_layer = ButtonKit.sign(p.content, "lucky_spin_title", PAGE_SIGN)
	# Completion is acknowledged by the reachable bottom action, not a duplicate corner exit.
	var columns := maxi(1, mini(prizes.size(), 3))
	var rows := maxi(1, ceili(prizes.size() / 3.0))
	var tile := Vector2(260, 360) if columns > 1 else Vector2(500, 540)
	var gap := 24.0
	var height := tile.y * rows + gap * (rows - 1)
	var top := 870.0 - height * 0.5
	var shown := []
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
		FamilyDialog.digits(reward, "Number", Rect2(20, tile.y - 108, tile.x - 40, 80), "+%d" % prizes[i][1])
		shown.append(reward)
	if not instant:
		_prize_entrance(p, shown, 0.2)
	# The hit area holds the on-screen ButtonKit.PRIMARY plate with room to spare.
	var action_box := Rect2(Vector2(208, 1340) * DrawPurchaseArt.SCALE + canvas.position,
		Vector2(500, 186) * DrawPurchaseArt.SCALE)
	var action := ShopSkin.product_button(p.content, "Confirm", action_box, p.close, "green")
	_kit_action(action, "white_action_ok")
	if not instant:
		p.appear_page(null, false)
	# The bottom action restores the board once, after the shared modal exit finishes.
	p.closed.connect(on_ok)
	return p

static func new_friend(host: Control, species: String, reward: int, on_ok: Callable) -> Control:
	var p := JungleDialog.open(host, "popup_new_friend_title", "", [FRIEND, NAME_CAP * 1.3, ABILITY_H, REWARD_H],
		_instant(host), FamilyDialog.PRIMARY.y, false, false, true, false, 880.0, 72.0)
	var c := p.content
	var ys: Array = p.get_meta("ys")
	_bird_view(c, species, Rect2(540.0 - FRIEND * 0.5 + 30.0, ys[0] + 20.0, FRIEND - 60.0, FRIEND - 40.0), _instant(host), 1.2)
	FamilyDialog.name_heading(c, "bird.%s.name" % species, ys[1], NAME_CAP * 1.3)
	FamilyDialog.ability(c, species, EFFECT.get(species, ""), ys[2])
	FamilyDialog.reward(c, Rect2(540.0 - REWARD_W * 0.5, ys[3], REWARD_W, REWARD_H), reward)
	FamilyDialog.actions(p, [["green", "check", on_ok]])
	return p

static func unlock(host: Control, booster: int, on_ok: Callable) -> Control:
	var p := JungleDialog.open(host, FamilyDialog.lettering_key("booster.%s.name" % BOOSTER_ID[booster]), "", [ILLUS, LINE], _instant(host), FamilyDialog.PRIMARY.y, false, false, true, false, 900.0, 72.0)
	var c := p.content
	var ys: Array = p.get_meta("ys")
	var box := Rect2(540.0 - ILLUS * 0.5, ys[0], ILLUS, ILLUS)
	var shop_art: Array = load("res://game/shop_view.gd").BOOSTER_ILLUS
	var picture := _fit(c, "island_icon_%s" % shop_art[booster], box.grow(-40.0), true)
	picture.name = "UnlockPicture"
	if not _instant(host):
		_breathe(picture, 0.0)
	# The shop's deep green painted caption of the booster.
	ButtonKit.lettering(c, "cap_" + FamilyDialog.BOOSTER_LETTERING[BOOSTER_ID[booster]], Rect2(132, ys[1] + (LINE - 64) * 0.5, 816, 64))
	FamilyDialog.actions(p, [["green", "check", on_ok]])
	return p

## Shared perched celebration: feet meet the frame, never empty doorway space.
static func _celebration_birds(c: Control, card: Rect2, live: bool) -> void:
	# Feet sit on the upper green rim, clear of the painted title.
	var footline := card.position.y + 28.0
	var hero_box := Rect2(432, footline - 216.0, 216, 216)
	var side_boxes := [Rect2(208, footline - 184.0, 184, 184), Rect2(688, footline - 184.0, 184, 184)]
	var hero := _bird_view(c, "sunny", hero_box, not live, 1.6)
	var sides := [_bird_view(c, "sky", side_boxes[0], not live, 2.0),
		_bird_view(c, "berry", side_boxes[1], not live, 2.4)]
	hero.set_meta("visible_box", hero_box)
	for i in sides.size():
		sides[i].set_meta("visible_box", side_boxes[i])
	if live:
		_fly_in(hero, Vector2(640, -1250), 0.45, 1.0)
		_fly_in(sides[0], Vector2(-420, -120), 0.75, 0.8)
		_fly_in(sides[1], Vector2(420, -120), 0.9, 0.8)

static func world_complete(host: Control, world: int, reward: int, on_ok: Callable) -> Control:
	var live := not _instant(host)
	var p := JungleDialog.open(host, "popup_world_complete_title", "", [FamilyDialog.STAR_ROW + WIN_STAR_ROOM, REWARD_H],
		not live, FamilyDialog.PRIMARY.y, false, true, true, false, 880.0, 72.0)
	var c := p.content
	var ys: Array = p.get_meta("ys")
	_celebration_birds(c, p.panel_rect, live)
	_win_stars(c, ys[0], 3, live, 0.4)
	FamilyDialog.reward(c, Rect2(540.0 - REWARD_W * 0.5, ys[1], REWARD_W, REWARD_H), reward)
	FamilyDialog.actions(p, [["green", "check", on_ok]])
	return p

static func world_unlock(host: Control, world: int, on_play: Callable) -> Control:
	var gate_h := GATE_W * GATE_CONTENT.size.y / GATE_CONTENT.size.x
	var p := JungleDialog.open(host, "popup_world_unlock_title", "", [gate_h, NAME_CAP * 1.3],
		_instant(host), FamilyDialog.PRIMARY.y, false, false, true, false, 960.0, 72.0)
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
		# Perspective projection around fixed outer hinges, rather than sliding the doors apart.
		var projection := cos(lerpf(0.0, deg_to_rad(82.0), t))
		left.scale.x = projection
		right.scale.x = projection
		p.set_meta("gate_open_fraction", t)
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
		_fit(holder, "island_icon_%s" % load("res://game/shop_view.gd").BOOSTER_ILLUS[kinds[i]], Rect2(Vector2.ZERO, holder.size), true)
		# Unboxed painted numerals (ButtonKit: white, dark-green outline) on the item's lower right.
		var quantity := _win_digits(holder, "Quantity", Rect2(DROP_SIDE - 64.0, DROP_SIDE - 60.0, 100, 56))
		_win_set(quantity, "+%d" % counts[kinds[i]])
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
static func _ring(c: Control, species: String, center: Vector2, instant: bool, side := RING, world := 1) -> void:
	var r := Rect2(center - Vector2.ONE * side * 0.5, Vector2.ONE * side)
	UI.art(c, "hud_tool_slot", r)   # the game page's painted slot
	var inner := side * 0.82
	_bird_view(c, species, Rect2(center - Vector2.ONE * inner * 0.5 + Vector2(0, -6), Vector2.ONE * inner), instant, 0.0,
		WorldBirds.look(species, world))
	var effect: String = EFFECT.get(species, "")
	if not effect.is_empty():
		var b := side * 0.42
		UI.art(c, "gift_coin_" + effect, Rect2(r.end - Vector2(b, b) * 0.85, Vector2(b, b)))
	# Tapping a ring opens that bird's card over the level card.
	var tap := UI.hit_area(c, r, func(): goal_info(c.get_parent().get_parent(), species, world))
	tap.name = "Bird_" + species


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

## Won prizes arrive instead of just standing there: one after another each pops out of nothing with the reward
## chime and a ring of sparkles, and its "+N" counts up from zero. `prizes` are the prize holders; each has a
## "Number" child (FamilyDialog.digits) and the meta "amount". Ends with confetti over the whole popup.
const PRIZE_STAGGER := 0.16

static func _prize_entrance(p: Control, prizes: Array, delay: float) -> void:
	for i in prizes.size():
		var prize: Control = prizes[i]
		var number: Control = prize.get_node("Number")
		var amount := int(prize.get_meta("amount"))
		FamilyDialog.set_digits(number, "+0")
		prize.pivot_offset = prize.size * 0.5
		prize.scale = Vector2.ZERO
		var rest := prize.position
		prize.position.y += 44.0
		var tw := prize.create_tween()
		tw.tween_interval(delay + i * PRIZE_STAGGER)
		tw.tween_callback(Sound.play.bind("reward", 1.0 + 0.08 * i))
		tw.tween_callback(_sparkle_ring.bind(prize))
		tw.tween_property(prize, "scale", Vector2.ONE * 1.18, 0.26).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
		tw.parallel().tween_property(prize, "position", rest, 0.26).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
		tw.tween_property(prize, "scale", Vector2.ONE, 0.2).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
		tw.parallel().tween_method(func(v: float): FamilyDialog.set_digits(number, "+%d" % roundi(v)), 0.0, float(amount), 0.5)
	_confetti(p, delay + 0.1)

## Eight sparkles fly out of a prize's middle and fade.
static func _sparkle_ring(prize: Control) -> void:
	var middle := Vector2(prize.size.x * 0.5, prize.size.y * 0.4)
	var reach := prize.size.x * 0.62
	for i in 8:
		var spark := UI.image(prize, "sparkle", Rect2(middle - Vector2(28, 28), Vector2(56, 56)))
		spark.pivot_offset = spark.size * 0.5
		var way := Vector2.from_angle(TAU * i / 8.0 + 0.3)
		var tw := spark.create_tween()
		tw.tween_method(func(t: float):
			spark.position = middle + way * reach * t - spark.size * 0.5
			spark.scale = Vector2.ONE * lerpf(0.5, 1.2, sin(t * PI))
			spark.rotation = t * 1.5
			spark.modulate.a = minf(1.0, (1.0 - t) * 3.0), 0.0, 1.0, 0.6).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
		tw.tween_callback(spark.queue_free)

## Coins leave `from` in a fan and fly into `to` (a feather counter), each landing with a tick; positions are in
## `layer`'s own coordinates. `on_land` runs as the first coin lands. Returns the seconds until the last coin has
## landed, so a caller can wait for the whole flight before showing anything else.
const COIN_FLIGHT_COINS := 9
const COIN_FLIGHT_STAGGER := 0.05
const COIN_FLIGHT_BURST := 0.26
const COIN_FLIGHT_TRAVEL := 0.42

static func coin_flight(layer: Control, from: Vector2, to: Vector2, delay := 0.0, on_land := Callable()) -> float:
	for i in COIN_FLIGHT_COINS:
		var coin := UI.image(layer, "coin", Rect2(from - Vector2(48, 48), Vector2(96, 96)))
		coin.pivot_offset = coin.size * 0.5
		coin.modulate.a = 0.0
		var spread := from + Vector2.from_angle(TAU * i / 9.0) * (120.0 + 18.0 * (i % 3))
		var tw := coin.create_tween()
		tw.tween_interval(delay + i * COIN_FLIGHT_STAGGER)
		tw.tween_property(coin, "modulate:a", 1.0, 0.06)
		tw.parallel().tween_property(coin, "position", spread - coin.size * 0.5, COIN_FLIGHT_BURST).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
		tw.tween_property(coin, "position", to - coin.size * 0.5, COIN_FLIGHT_TRAVEL).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_IN)
		tw.parallel().tween_property(coin, "scale", Vector2.ONE * 0.6, COIN_FLIGHT_TRAVEL)
		tw.tween_callback(Sound.play.bind("ui_tap", 1.5 + 0.05 * i))
		if i == 0 and on_land.is_valid():
			tw.tween_callback(on_land)
		tw.tween_callback(coin.queue_free)
	return delay + (COIN_FLIGHT_COINS - 1) * COIN_FLIGHT_STAGGER + COIN_FLIGHT_BURST + COIN_FLIGHT_TRAVEL

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
static func _fit(parent: Node, name: String, box: Rect2, bottom := false, source_ink := Rect2()) -> TextureRect:
	var used := source_ink if source_ink.size != Vector2.ZERO else UI.used_rect(name)
	var k := minf(box.size.x / used.size.x, box.size.y / used.size.y)
	var size := used.size * k
	var at := box.position + Vector2((box.size.x - size.x) * 0.5, (box.size.y - size.y) if bottom else (box.size.y - size.y) * 0.5)
	var t := UI.art(parent, name, Rect2(at, size), used)
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
static func _bird_view(c: Control, species: String, box: Rect2, instant: bool, cheer_delay: float, look := "") -> BirdView:
	var used := UI.used_rect((look if not look.is_empty() else species) + "_front")
	var k := minf(box.size.x / used.size.x, box.size.y / used.size.y)
	var feet := Vector2(box.get_center().x - (used.get_center().x - BirdView.CANVAS * 0.5) * k,
		box.end.y - (used.end.y - BirdView.FEET_Y) * k)
	var bird := BirdView.new()
	c.add_child(bird)
	bird.position = feet
	bird.setup(Board.bird(0, Board.species_index(species), 0), false, k * BirdView.CANVAS / BirdView.DISPLAY, look)
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
	var mechanics: Array[String] = ["sleep", "egg"]
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
		"level_card_tools_two": level_card(host, 12, 0, 0, 9, few, none, noop, true, [Board.Booster.OWL_EYE, Board.Booster.FIREFLY],
			func(kind): return [0, 0, 3, 0, 0, 0][kind], func(_k, _r): pass)
		"level_card_tools_four": level_card(host, 40, 0, 0, 9, few, mechanics, noop, true,
			[Board.Booster.MORNING_BELL, Board.Booster.OWL_EYE, Board.Booster.SCARECROW, Board.Booster.SPRING_RAIN],
			func(kind): return [0, 0, 2, 0, 5, 1][kind], func(_k, _r): pass)
		"goal_info_berry": goal_info(host, "berry")
		"goal_info_mint": goal_info(host, "mint")
		"goal_info_crow": goal_info(host, "crow")
		"goal_info_hoot": goal_info(host, "hoot")
		"goal_info_sunny": goal_info(host, "sunny")
		"goal_info_sky": goal_info(host, "sky")
		"goal_info_cocoa": goal_info(host, "cocoa")
		"goal_info_snow": goal_info(host, "snow")
		"goal_info_ember": goal_info(host, "ember")
		"goal_info_ink": goal_info(host, "ink")
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
		"rewards_daily": rewards(host, "daily_reward.title", 250, [0, 1], noop, "pic_calendar")
		"rewards_daily_coins": rewards(host, "daily_reward.title", 120, [], noop, "pic_calendar")
		"rewards_daily_all": rewards(host, "daily_reward.title", 500, [0, 1, 2, 3, 4, 5], noop, "pic_calendar")
		"daily_reward": daily_reward(host, 3, true, 1280, func(): return {"day": 3, "coins": 120, "boosters": []})
		"daily_reward_first": daily_reward(host, 1, true, 0, func(): return {"day": 1, "coins": 50, "boosters": []})
		"daily_reward_week": daily_reward(host, 7, true, 2400, func(): return {"day": 7, "coins": 500, "boosters": [0, 1, 1]})
		"daily_reward_claimed": daily_reward(host, 4, false, 1430, func(): return {})
		"daily_reward_claim":
			var page := daily_reward(host, 6, true, 1800, func(): return {"day": 6, "coins": 250, "boosters": [0, 2]})
			host.get_tree().create_timer(0.5).timeout.connect(func(): (page.find_child("Claim", true, false) as Button).pressed.emit())
		"rewards_chest": rewards(host, "chest.title", Progress.CHEST_COINS, [0, 3, 0], noop, "pic_chest_open", 1580)
		"rewards_chest_distinct": rewards(host, "chest.title", Progress.CHEST_COINS, [0, 2, 3], noop, "pic_chest_open", 1580)
		"new_friend_berry": new_friend(host, "berry", 25, noop)
		"unlock_firefly": unlock(host, 0, noop)
		"unlock_morning_bell": unlock(host, 3, noop)
		"unlock_scarecrow": unlock(host, 4, noop)
		"unlock_spring_rain": unlock(host, 5, noop)
		"unlock_owl_eye": unlock(host, 2, noop)
		"unlock_sapling": unlock(host, 1, noop)
		"world_complete_1": world_complete(host, 1, 500, noop)
		"world_complete_4": world_complete(host, 4, 500, noop)
		"world_complete_10": world_complete(host, 10, 500, noop)
		"world_unlock_2": world_unlock(host, 2, noop)
		"world_unlock_4": world_unlock(host, 4, noop)
		"world_unlock_10": world_unlock(host, 10, noop)
