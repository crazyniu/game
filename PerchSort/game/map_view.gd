extends Control
## The level map: all 40 levels on a scrolling trail across the four painted world maps, with a fixed top bar
## (world title, coins) and the shop, aviary, settings and help buttons at the bottom.

signal level_tapped(level: int)
signal shop_requested
signal aviary_requested
signal settings_requested
signal help_requested
signal home_requested
signal chapters_requested
signal gate_tapped(world: int)

const SEGMENT := 3000.0
const STRIDE := 2840.0
const CONTENT_H := STRIDE * 3.0 + SEGMENT
const OVERLAP := SEGMENT - STRIDE   # each painting fades in over its top 160 px, over the world before it
const FOCUS_RATIO := 0.58
const TAP_SLOP := 14.0
const WORLD_LABELS := ["world.green.name", "world.bamboo.name", "world.autumn.name", "world.snow.name"]
## Node centers per world (x, y within the world segment), levels 1..10.
const NODES := [
	[[514, 2700], [310, 2440], [744, 2180], [660, 1920], [252, 1660], [672, 1400], [640, 1140], [305, 880], [754, 620], [613, 360]],
	[[512, 2700], [304, 2440], [729, 2180], [656, 1920], [251, 1660], [667, 1400], [634, 1140], [302, 880], [752, 620], [614, 360]],
	[[508, 2700], [306, 2440], [752, 2180], [640, 1920], [240, 1660], [699, 1400], [620, 1140], [298, 880], [750, 620], [598, 360]],
	[[512, 2700], [312, 2440], [738, 2180], [650, 1920], [250, 1660], [668, 1400], [618, 1140], [299, 880], [750, 620], [604, 360]],
]
## Layout (docs/design/ui-system.md, approved map style). All values in the 1080x1920 design space.
const MARGIN := 48.0
const NODE := 164.0              # cleared and locked nodes (painted gold disc, stars inside)
const CURRENT := 210.0           # current node (painted glowing disc)
const NODE_EDGE := 10.0
const NODE_CAP := 60.0
const CURRENT_CAP := 76.0
const LOCK := 56.0
const LOCK_COLOR := Color8(200, 180, 150)
const LOCKED_DIM := Color(0.92, 0.92, 0.92, 0.82)
const STAR := 48.0
const STAR_GAP := -6.0
const STAR_DY := 34.0            # star row centre below the node centre, inside the disc
const BIRD_FEET := 82.0          # the current bird stands this far above the node centre
const BIRD_SCALE := 1.0          # BirdView scale (1.0 = 170 px canvas)
const BOUNCE := 10.0
const GATE_SIZE := Vector2(320, 280)
const GATE_CENTER_X := 870.0     # right of the trail so the current bird of the last node never touches the gate
const GATE_CENTER_Y := 100.0     # between the last node of a world and the first of the next
const GATE_PADLOCK := Vector2(0.5, 0.68)   # painted padlock on the closed gate, fraction of its visible box
const GATE_LOCK := 112.0
const TOP_Y := 56.0
const TRAIL_VIEW := Rect2(0, 200, 1080, 1720)
## A soft fade in the painting's own colour sits behind the fixed header.
## WORLD_SHADE: each painting's mean grass colour beside the trail, darkened to 45 percent.
const TOP_FADE := Rect2(0, 0, 1080, 200)
const TOP_FADE_ALPHA := 0.62
const WORLD_SHADE := [Color8(59, 70, 16), Color8(53, 71, 33), Color8(80, 55, 13), Color8(65, 72, 63)]
## At either scroll end the bird and stars rest clear of the fixed controls.
const TOP_CLEAR := 340.0
const BOTTOM_CLEAR := 1644.0
const CHIP := Vector2(300.0, 96.0)
const CURRENT_CHIP_Y := 1620.0
const NAV_Y := 1818.0
const NAV_TILE := Vector2(214.0, 176.0)   # the painted home tiles, a little smaller than on the home page
const NAV_GAP := 236.0
## Extra scroll room above the last world and below the first (painted, see _overscroll_*), from TOP_CLEAR and
## BOTTOM_CLEAR: level 40 at y 360 with a bird up to 240 above its centre, level 1 at y 2700 + 3 strides with its
## star row extending below the node centre.
const TOP_PAD := TOP_CLEAR - (360.0 - 240.0)
const BOTTOM_PAD := (2700.0 + STRIDE * 3.0 + STAR_DY + STAR * 0.5) - BOTTOM_CLEAR - (CONTENT_H - 1920.0)
## At either scroll end no node may sit under the bottom buttons: the end moves outward (extra painted room) to
## the offset where nodes, their stars and the buttons overlap least (ideally not at all), at most END_SEARCH px.
const NAV_COUNT := 3
const NAV_CLEAR := 12.0          # free ring around each bottom button
const END_SEARCH := 900.0
## World sign: the current world's name on a hanging plank under the top bar, swapped when the view changes world.
const SIGN := Rect2(270, 112, 540, 212)   # hanging world sign (ropes rise behind the top bar)

var profile: Profile
var current_level := 1
var offset := 0.0

var _paintings: Control
var _clip: Control
var _content: Control
var _hud: Control
var _coins: Panel
var _title_world := 0
var _sign: Control
var _top_pad := TOP_PAD
var _bottom_pad := BOTTOM_PAD
var _top_fade: TextureRect
var _hit_rects: Array = []   # [Rect2 in content space, kind, id]
var _dragging := false
var _drag_travel := 0.0
var _velocity := 0.0
var _bird: BirdView


func setup(player: Profile) -> void:
	profile = player
	for child in get_children():
		child.queue_free()
	_hit_rects.clear()
	_bird = null
	_dragging = false
	_velocity = 0.0
	size = Vector2(1080, 1920)
	clip_contents = true
	mouse_filter = Control.MOUSE_FILTER_STOP
	var next := Progress.next_level(profile)
	current_level = next if next > 0 else 40
	_fit_scroll_ends()
	_build_content()
	_build_hud()
	scroll_to(_node_y(current_level) - size.y * FOCUS_RATIO)

func _notification(what: int) -> void:
	if what == NOTIFICATION_TRANSLATION_CHANGED and is_node_ready() and profile != null:
		var trail_offset := offset
		setup(profile)
		scroll_to(trail_offset)

func _node_y(level: int) -> float:
	return _node_pos(level).y

func _node_pos(level: int) -> Vector2:
	var world := Progress.world_of_level(level)
	var p: Array = NODES[world - 1][(level - 1) % 10]
	return Vector2(p[0], p[1] + (4 - world) * STRIDE)

func _gate_center(world: int) -> Vector2:
	return Vector2(GATE_CENTER_X, GATE_CENTER_Y + (4 - world) * STRIDE)

## Scrolls so the world's gate sits near the middle of the screen.
func show_gate(world: int) -> void:
	scroll_to(_gate_center(world).y - size.y * 0.35)

func scroll_to(value: float) -> void:
	offset = clampf(value, -_top_pad, CONTENT_H - size.y + _bottom_pad)
	_paintings.position.y = -offset
	_content.position.y = -offset - TRAIL_VIEW.position.y
	_update_title()
	_update_fades()

## True while silent captures run: no tweens, birds hold still.
func _instant() -> bool:
	var n := get_parent()
	while n != null:
		if n.has_method("instant"):
			return n.instant()
		n = n.get_parent()
	return false

func _build_content() -> void:
	_paintings = Control.new()
	_paintings.name = "WorldPaintings"
	_paintings.size = Vector2(1080, CONTENT_H)
	_paintings.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(_paintings)
	# Overscroll rooms continue the painting: its edge strip reflected, so no dark band shows behind the bar.
	_overscroll_top()
	_overscroll_bottom()
	for world in range(1, 5):
		var bg := UI.image(_paintings, "map_bg_continuous_w%d" % world, Rect2(0, (4 - world) * STRIDE, 1080, SEGMENT), false)
		bg.name = "World %d" % world
	_clip = Control.new()
	_clip.name = "TrailViewport"
	_clip.clip_contents = true
	_clip.mouse_filter = Control.MOUSE_FILTER_STOP
	UI.place(_clip, TRAIL_VIEW)
	_clip.gui_input.connect(_on_trail_input)
	add_child(_clip)
	_content = Control.new()
	_content.name = "Trail"
	_content.size = Vector2(1080, CONTENT_H)
	_content.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_clip.add_child(_content)
	for world in range(1, 4):
		_build_gate(world)
	for level in range(1, 41):
		_build_node(level)

func _build_gate(world: int) -> void:
	var open := Progress.is_world_unlocked(profile, world + 1)
	var c := _gate_center(world)
	var rect := Rect2(c - GATE_SIZE * 0.5, GATE_SIZE)
	var holder := Control.new()
	holder.name = "Gate %d" % world
	holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_content.add_child(holder)
	if open:
		var box := _fitted("gate_arch_frame_w%d" % world, rect)
		UI.art(holder, "gate_light_w%d" % world, Rect2(box.position + box.size * Vector2(0.2, 0.22), box.size * Vector2(0.6, 0.78)))
		UI.art(holder, "gate_arch_frame_w%d" % world, rect)
		var door := Vector2(box.size.x * 0.17, box.size.y * 0.5)
		var left := UI.art(holder, "gate_door_left_w%d" % world, Rect2(box.position + Vector2(box.size.x * 0.155, box.size.y * 0.42), door))
		var right := UI.art(holder, "gate_door_right_w%d" % world, Rect2(box.position + Vector2(box.size.x * 0.675, box.size.y * 0.42), door))
		left.scale.x = 0.35
		right.scale.x = 0.35
		left.pivot_offset = Vector2(0.0, left.size.y * 0.5)
		right.pivot_offset = Vector2(right.size.x, right.size.y * 0.5)
	else:
		UI.art(holder, "gate_arch_closed_w%d" % world, rect)
		var box := _fitted("gate_arch_closed_w%d" % world, rect)
		# The lock reads in the UI style: an ink lock on a cream circle over the painted padlock.
		var at := box.position + box.size * GATE_PADLOCK
		_circle(holder, at, GATE_LOCK, "cream")
		UI.icon(holder, "lock", at + Vector2(0, -NODE_EDGE * 0.5), GATE_LOCK * 0.5, UI.INK)
	_hit_rects.append([rect, "gate", world])

## Where UI.art puts the visible pixels of a sprite fitted into `bounds`.
func _fitted(name: String, bounds: Rect2) -> Rect2:
	var used := UI.used_rect(name)
	var k := minf(bounds.size.x / used.size.x, bounds.size.y / used.size.y)
	return Rect2(bounds.position + (bounds.size - used.size * k) * 0.5, used.size * k)

## Painted level disc centred on `at` (its glow may reach outside `diameter`).
func _disc(parent: Control, picture: String, at: Vector2, diameter: float) -> void:
	UI.art(parent, picture, Rect2(at - Vector2.ONE * diameter * 0.5, Vector2.ONE * diameter)).name = "Node"

## Round cream or coloured disc (same shape as the kit's buttons) centred on `at`.
func _circle(parent: Control, at: Vector2, diameter: float, kind: String) -> Panel:
	var p := UI.shape(parent, Rect2(at - Vector2.ONE * diameter * 0.5, Vector2.ONE * diameter), kind, diameter * 0.5, NODE_EDGE)
	p.name = "Node"
	return p

func _build_node(level: int) -> void:
	var state := Progress.node_state(profile, level)
	var c := _node_pos(level)
	var layer := Control.new()
	layer.name = "Level %d" % level
	layer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_content.add_child(layer)
	var face := Vector2(c.x, c.y - NODE_EDGE * 0.5)   # optical centre of the face above the bottom edge
	match state:
		"locked":
			_disc(layer, "map_node", c, NODE)
			UI.icon(layer, "lock", face, LOCK, LOCK_COLOR)
			layer.modulate = LOCKED_DIM
			_hit_rects.append([Rect2(c - Vector2.ONE * NODE * 0.5, Vector2.ONE * NODE).grow(16), "level", level])
		"cleared":
			_disc(layer, "map_node", c, NODE)
			_number(layer, level, face - Vector2(0, 24), NODE_CAP, UI.INK)
			UI.rating_stars(layer, Vector2(c.x, c.y + STAR_DY), STAR, STAR_GAP, profile.best_stars(level))
			_hit_rects.append([Rect2(c - Vector2.ONE * NODE * 0.5, Vector2.ONE * NODE).grow(16), "level", level])
		"current":
			_disc(layer, "map_node_current", c, CURRENT)
			_number(layer, level, face - Vector2(0, 10), CURRENT_CAP, UI.INK)
			var top := c.y - BIRD_FEET - BirdView.DISPLAY * BIRD_SCALE
			_hit_rects.append([Rect2(c.x - CURRENT * 0.5, top, CURRENT, c.y + CURRENT * 0.5 - top).grow(16), "level", level])
			if level == current_level:
				_add_bird(layer, Vector2(c.x, c.y - BIRD_FEET), _level_species(level))
			if not _instant():
				_bounce(layer)

func _number(layer: Control, level: int, center: Vector2, cap: float, color: Color) -> void:
	var w := UI.number_width(str(level), cap) + 8.0
	UI.number(layer, str(level), Rect2(center.x - w * 0.5, center.y - cap * 0.5, w, cap), color)

## A gentle hop of the current node (and the bird on it): up and back, then a rest.
func _bounce(layer: Control) -> void:
	var tw := layer.create_tween().set_loops()
	tw.tween_property(layer, "position:y", -BOUNCE, 0.45).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)
	tw.tween_property(layer, "position:y", 0.0, 0.55).set_trans(Tween.TRANS_BOUNCE).set_ease(Tween.EASE_OUT)
	tw.tween_interval(1.2)

## The first bird species of the level, so the node shows who the player is about to meet.
func _level_species(level: int) -> String:
	var ids := Levels.species_ids(Levels.create_board(Levels.get_level(level)))
	return ids[0] if not ids.is_empty() else "sunny"

func _add_bird(layer: Control, feet: Vector2, species: String) -> void:
	_bird = BirdView.new()
	_bird.name = "Bird"
	_bird.position = feet
	layer.add_child(_bird)
	_bird.setup(Board.bird(0, Board.species_index(species), 0), false, BIRD_SCALE)
	_bird.quiet = _instant()

func _build_hud() -> void:
	_hud = Control.new()
	_hud.name = "HUD"
	_hud.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_hud.size = size
	add_child(_hud)
	_top_fade = _fade(TOP_FADE, TOP_FADE_ALPHA, false)
	var coins := str(profile.coins)
	_coins = UI.counter(_hud, Vector2(MARGIN + UI.SECONDARY + 24.0, TOP_Y), "coin", coins)
	_coins.name = "Coins"
	_title_world = 0
	UI.button(_hud, "Home", Vector2(MARGIN + UI.SECONDARY * 0.5, TOP_Y + UI.COUNTER_H * 0.5), "cream", "back",
		func(): home_requested.emit())
	_chip("Chapters", "pic_map", "map.chapters", Rect2(1080.0 - MARGIN - CHIP.x, TOP_Y + UI.COUNTER_H * 0.5 - CHIP.y * 0.5, CHIP.x, CHIP.y),
		func(): chapters_requested.emit())
	_chip("Current", "pic_moves", "map.current", Rect2(1080.0 - MARGIN - CHIP.x, CURRENT_CHIP_Y, CHIP.x, CHIP.y),
		func(): scroll_to(_node_y(current_level) - size.y * FOCUS_RATIO))
	# Four secondary buttons of one size, centred on the screen; help (how to play) is the cream one at the end.
	var buttons := [["Shop", "home_button_shop", shop_requested], ["Aviary", "home_button_aviary", aviary_requested],
		["Help", "map_button_help", help_requested]]
	for i in buttons.size():
		var sig: Signal = buttons[i][2]
		UI.art_button(_hud, buttons[i][0], buttons[i][1], Rect2(Vector2(_nav_x(i), NAV_Y) - NAV_TILE * 0.5, NAV_TILE), sig.emit)
	_sign = Control.new()
	_sign.name = "WorldSign"
	_sign.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_sign.size = size
	_hud.add_child(_sign)

## Cream capsule with a picture and a word (Chapters, Current).
func _chip(n: String, picture: String, label_id: String, r: Rect2, on_press: Callable) -> Button:
	var b := Button.new()
	b.name = n
	b.focus_mode = Control.FOCUS_NONE
	UI.place(b, r)
	UI._skin_button(b, "capsule_plain", Rect2(Vector2.ZERO, r.size))
	var content := Control.new()
	content.name = "Content"
	content.size = r.size
	content.mouse_filter = Control.MOUSE_FILTER_IGNORE
	b.add_child(content)
	var pic := r.size.y * 0.6
	var cap := 36.0
	var lw := minf(r.size.x - pic - 72.0, UI.label_width(label_id, cap))
	var x := (r.size.x - (pic + 12.0 + lw)) * 0.5
	UI.art(content, picture, Rect2(x, (r.size.y - 8.0 - pic) * 0.5, pic, pic))
	UI.label(content, label_id, Rect2(x + pic + 12.0, (r.size.y - 8.0 - 48.0) * 0.5, lw, 48.0), cap)
	b.button_down.connect(func(): content.position.y = 4.0)
	b.button_up.connect(func(): content.position.y = 0.0)
	b.pressed.connect(func():
		Sound.play("ui_tap")
		on_press.call())
	_hud.add_child(b)
	return b

## Scrolls so the chapter's first level sits at the focus line.
func show_world(world: int) -> void:
	scroll_to(_node_y((world - 1) * 10 + 1) - size.y * FOCUS_RATIO)

## The room above the last world: the painting's top rows made opaque (they fade in over the world before, and
## nothing is above world 4), reflected upwards at the painting's top edge.
func _overscroll_top() -> void:
	var tex := Art.tex("map_bg_continuous_w4")
	var k := float(tex.get_height()) / SEGMENT
	var h := ceilf(maxf(_top_pad, OVERLAP) * k)
	var img := tex.get_image().get_region(Rect2i(0, 0, tex.get_width(), int(h)))
	img.convert(Image.FORMAT_RGB8)
	var opaque := ImageTexture.create_from_image(img)
	_strip(opaque, Rect2(0, 0, tex.get_width(), _top_pad * k), Rect2(0, -_top_pad, 1080, _top_pad), true)
	_strip(opaque, Rect2(0, 0, tex.get_width(), OVERLAP * k), Rect2(0, 0, 1080, OVERLAP), false)

## The room below the first world: its bottom rows reflected downwards.
func _overscroll_bottom() -> void:
	var tex := Art.tex("map_bg_continuous_w1")
	var h := _bottom_pad * float(tex.get_height()) / SEGMENT
	_strip(tex, Rect2(0, tex.get_height() - h, tex.get_width(), h), Rect2(0, CONTENT_H, 1080, _bottom_pad), true)

func _strip(tex: Texture2D, region: Rect2, r: Rect2, flip: bool) -> void:
	var atlas := AtlasTexture.new()
	atlas.atlas = tex
	atlas.region = region
	var t := TextureRect.new()
	t.name = "Overscroll"
	t.texture = atlas
	t.flip_v = flip
	t.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	t.stretch_mode = TextureRect.STRETCH_SCALE
	t.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(t, r)
	_paintings.add_child(t)

## A white fade (eased, no hard edge) tinted per frame with the colour of the painting under it.
func _fade(r: Rect2, alpha: float, from_bottom: bool) -> TextureRect:
	var g := Gradient.new()
	var ramp := [0.0, 0.3, 0.6, 0.85, 1.0]   # eased alpha from the clear edge to the screen edge
	g.offsets = PackedFloat32Array([0.0, 0.25, 0.5, 0.75, 1.0])
	var colors := PackedColorArray()
	for i in ramp.size():
		colors.append(Color(1, 1, 1, alpha * ramp[i if from_bottom else ramp.size() - 1 - i]))
	g.colors = colors
	var t := _gradient_rect(g, r)
	t.name = "Fade"
	_hud.add_child(t)
	return t

func _update_fades() -> void:
	if _top_fade == null:
		return
	_top_fade.modulate = _paint_color(offset + TOP_FADE.get_center().y)

## Shade colour of the painting at a content y, blended smoothly across each seam between worlds.
func _paint_color(y: float) -> Color:
	var c: Color = WORLD_SHADE[3]
	for k in range(1, 4):
		var seam := k * STRIDE + OVERLAP * 0.5
		c = c.lerp(WORLD_SHADE[3 - k], smoothstep(seam - 160.0, seam + 160.0, y))
	return c

func _gradient_rect(g: Gradient, r: Rect2) -> TextureRect:
	var tex := GradientTexture2D.new()
	tex.gradient = g
	tex.fill_from = Vector2(0, 0)
	tex.fill_to = Vector2(0, 1)
	var t := TextureRect.new()
	t.texture = tex
	t.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	t.stretch_mode = TextureRect.STRETCH_SCALE
	t.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(t, r)
	return t

## The world the view is in follows the trail node nearest to the upper third of the screen. Entering a world
## (also the first map view of a run) shows its ribbon; silent captures show it only when a preview asks.
func _update_title() -> void:
	if _hud == null:
		return
	var probe := offset + size.y * 0.36
	var best := 1
	var best_d := INF
	for level in range(1, 41):
		var d := absf(_node_y(level) - probe)
		if d < best_d:
			best_d = d
			best = level
	var world := Progress.world_of_level(best)
	if world == _title_world:
		return
	_title_world = world
	_build_sign(world)

## The world's name on the painted hanging sign under the top bar; it stays while the view is in that world.
func _build_sign(world: int) -> void:
	if _sign == null:
		return
	for child in _sign.get_children():
		child.queue_free()
	var r := SIGN
	UI.art(_sign, "map_sign", r)
	var plank := Rect2(r.position.x + r.size.x * 0.12, r.position.y + r.size.y * 0.44, r.size.x * 0.76, r.size.y * 0.42)
	var cap := 46.0
	var w := minf(plank.size.x, UI.label_width(WORLD_LABELS[world - 1], cap))
	UI.outline_to(UI.label(_sign, WORLD_LABELS[world - 1], Rect2(plank.get_center().x - w * 0.5, plank.position.y, w, plank.size.y), cap,
		Color.WHITE), "orange")
	if not _instant():
		_sign.pivot_offset = Vector2(540.0, r.position.y)
		_sign.rotation = 0.05
		var tw := _sign.create_tween()
		tw.tween_property(_sign, "rotation", 0.0, 0.6).set_trans(Tween.TRANS_ELASTIC).set_ease(Tween.EASE_OUT)

## Shows the world's hanging sign at once (captures and callers that announce a world).
func show_world_ribbon(world: int) -> void:
	_title_world = world
	_build_sign(world)

## ---------- scroll ends ----------

func _nav_x(i: int) -> float:
	return 540.0 + (i - (NAV_COUNT - 1) * 0.5) * NAV_GAP

## How far (px) any node, its stars or the current bird reach into a bottom button's clear ring at scroll offset
## `at`; 0 or less means every node is clear.
func _nav_overlap(at: float) -> float:
	var worst := -INF
	for level in range(1, 41):
		var c := _node_pos(level) - Vector2(0.0, at)
		if c.y < 1600.0 or c.y > 2100.0:
			continue
		var state := Progress.node_state(profile, level)
		var radius := (CURRENT if state == "current" else NODE) * 0.5
		var stars := Rect2()
		if state == "cleared":
			var row_w := STAR * 3.0 + STAR_GAP * 2.0
			stars = Rect2(c.x - row_w * 0.5, c.y + STAR_DY - STAR * 0.5, row_w, STAR)
		for i in NAV_COUNT:
			var b := Vector2(_nav_x(i), NAV_Y)
			var ring := NAV_TILE.y * 0.5 + NAV_CLEAR
			worst = maxf(worst, radius + ring - c.distance_to(b))
			if stars.has_area():
				var near := Vector2(clampf(b.x, stars.position.x, stars.end.x), clampf(b.y, stars.position.y, stars.end.y))
				worst = maxf(worst, ring - near.distance_to(b))
	return worst

## Moves each scroll end outward to the least-overlapping offset (the first fully clear one when there is one).
func _fit_scroll_ends() -> void:
	_top_pad = TOP_PAD
	_bottom_pad = BOTTOM_PAD
	var low := -TOP_PAD
	var high := CONTENT_H - size.y + BOTTOM_PAD
	_top_pad = TOP_PAD + _clear_distance(low, -1.0)
	_bottom_pad = BOTTOM_PAD + _clear_distance(high, 1.0)

func _clear_distance(from: float, direction: float) -> float:
	var best := 0.0
	var best_overlap := _nav_overlap(from)
	var d := 2.0
	while best_overlap > 0.0 and d <= END_SEARCH:
		var overlap := _nav_overlap(from + direction * d)
		if overlap < best_overlap - 0.5:
			best_overlap = overlap
			best = d
		d += 2.0
	return best

func _process(delta: float) -> void:
	if not _dragging and absf(_velocity) > 8.0:
		scroll_to(offset + _velocity * delta)
		_velocity *= pow(0.135, delta)
		if offset <= -_top_pad or offset >= CONTENT_H - size.y + _bottom_pad:
			_velocity = 0.0

func _on_trail_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		var mb := event as InputEventMouseButton
		if mb.button_index == MOUSE_BUTTON_WHEEL_UP and mb.pressed:
			_velocity = 0.0
			scroll_to(offset - 220.0)
		elif mb.button_index == MOUSE_BUTTON_WHEEL_DOWN and mb.pressed:
			_velocity = 0.0
			scroll_to(offset + 220.0)
		elif mb.button_index == MOUSE_BUTTON_LEFT:
			if mb.pressed:
				_dragging = true
				_drag_travel = 0.0
				_velocity = 0.0
			elif _dragging:
				_dragging = false
				if _drag_travel < TAP_SLOP:
					_tap(mb.position + TRAIL_VIEW.position)
	elif event is InputEventMouseMotion and _dragging:
		var mm := event as InputEventMouseMotion
		_drag_travel += absf(mm.relative.y) + absf(mm.relative.x)
		if _drag_travel >= TAP_SLOP:
			scroll_to(offset - mm.relative.y)
			_velocity = lerpf(_velocity, -mm.velocity.y, 0.5)

func _tap(screen_pos: Vector2) -> void:
	if not TRAIL_VIEW.has_point(screen_pos):
		return
	var p := screen_pos + Vector2(0, offset)
	for hit in _hit_rects:
		if (hit[0] as Rect2).has_point(p):
			if hit[1] == "level" and Progress.node_state(profile, hit[2]) != "locked":
				level_tapped.emit(hit[2])
			elif hit[1] == "gate":
				gate_tapped.emit(hit[2])
			return

## Fake data for captures. States: forest_trail, first_level, autumn_world, all_cleared, gate_locked,
## top_end / bottom_end (both scroll ends, all cleared / first level) and world_ribbon (the autumn sign).
static func preview(main: Control, state: String) -> void:
	var p := Profile.new()
	var cleared := 0
	match state:
		"forest_trail":
			cleared = 7
			p.coins = 1240
		"autumn_world":
			cleared = 20
			p.coins = 1240
		"all_cleared":
			cleared = 40
			p.coins = 1240
		"gate_locked":
			cleared = 9
			p.coins = 1240
		"top_end", "world_ribbon":
			cleared = 40 if state == "top_end" else 24
			p.coins = 1240
	for level in range(1, cleared + 1):
		p.stars[level] = [3, 3, 2, 3, 1][(level - 1) % 5]
	p.highest_unlocked = mini(40, cleared + 1)
	var view: Control = load("res://game/map_view.gd").new()
	main.set_screen(view)
	view.setup(p)
	if state == "gate_locked":
		view.show_gate(1)
	elif state == "top_end":
		view.scroll_to(-INF)
	elif state == "bottom_end":
		view.scroll_to(INF)
	elif state == "world_ribbon":
		view.show_world_ribbon(3)
