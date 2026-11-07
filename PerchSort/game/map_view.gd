extends Control
## The level map: all 40 levels on a scrolling trail across the four painted world maps, with a fixed top bar
## (world title, coins) and the shop, aviary and settings buttons at the bottom.

signal level_tapped(level: int)
signal shop_requested
signal aviary_requested
signal settings_requested
signal gate_tapped(world: int)

const SEGMENT := 3000.0
const STRIDE := 2840.0
const CONTENT_H := STRIDE * 3.0 + SEGMENT
const OVERLAP := SEGMENT - STRIDE   # each painting fades in over its top 160 px, over the world before it
const FOCUS_RATIO := 0.58
const TAP_SLOP := 14.0
const WORLD_NAMES := ["Green Forest", "Bamboo Forest", "Autumn Forest", "Snow Forest"]
## Node centers per world (x, y within the world segment), levels 1..10.
const NODES := [
	[[514, 2700], [310, 2440], [744, 2180], [660, 1920], [252, 1660], [672, 1400], [640, 1140], [305, 880], [754, 620], [613, 360]],
	[[512, 2700], [304, 2440], [729, 2180], [656, 1920], [251, 1660], [667, 1400], [634, 1140], [302, 880], [752, 620], [614, 360]],
	[[508, 2700], [306, 2440], [752, 2180], [640, 1920], [240, 1660], [699, 1400], [620, 1140], [298, 880], [750, 620], [598, 360]],
	[[512, 2700], [312, 2440], [738, 2180], [650, 1920], [250, 1660], [668, 1400], [618, 1140], [299, 880], [750, 620], [604, 360]],
]
## Layout (docs/design/ui-system.md, approved map style). All values in the 1080x1920 design space.
const MARGIN := 48.0
const NODE := 132.0              # cleared and locked nodes (cream)
const CURRENT := 168.0           # current node (green)
const NODE_EDGE := 10.0
const NODE_CAP := 52.0
const CURRENT_CAP := 64.0
const LOCK := 56.0
const LOCK_COLOR := Color8(200, 180, 150)
const LOCKED_DIM := Color(0.92, 0.92, 0.92, 0.82)
const STAR := 46.0
const STAR_GAP := 2.0
const STAR_DY := 88.0            # star row centre below the node centre
const BIRD_FEET := 70.0          # the current bird stands this far above the node centre
const BIRD_SCALE := 1.0          # BirdView scale (1.0 = 170 px canvas)
const BOUNCE := 10.0
const GATE_SIZE := Vector2(320, 280)
const GATE_CENTER_X := 870.0     # right of the trail so the current bird of the last node never touches the gate
const GATE_CENTER_Y := 100.0     # between the last node of a world and the first of the next
const GATE_PADLOCK := Vector2(0.5, 0.68)   # painted padlock on the closed gate, fraction of its visible box
const GATE_LOCK := 112.0
const TITLE_CAP := 40.0
const TITLE_PAD := 32.0
const TOP_Y := 56.0              # top bar row 48..160, controls centred on 104
## Soft fades in the painting's own colour behind the top bar and the bottom button row, so nodes passing under
## them read as behind. WORLD_SHADE: each painting's mean grass colour beside the trail, darkened to 45 percent.
const TOP_FADE := Rect2(0, 0, 1080, 200)
const BOTTOM_FADE := Rect2(0, 1660, 1080, 260)
const TOP_FADE_ALPHA := 0.62
const BOTTOM_FADE_ALPHA := 0.78
const WORLD_SHADE := [Color8(59, 70, 16), Color8(53, 71, 33), Color8(80, 55, 13), Color8(65, 72, 63)]
## At either end of the scroll the last / first node rests fully clear of both fades (16 px air).
const TOP_CLEAR := 216.0
const BOTTOM_CLEAR := 1644.0
const NAV_Y := 1792.0
const NAV_GAP := 200.0
## Extra scroll room above the last world and below the first (painted, see _overscroll_*), from TOP_CLEAR and
## BOTTOM_CLEAR: level 40 at y 360 with a bird up to 240 above its centre, level 1 at y 2700 + 3 strides with its
## stars down to 111 below.
const TOP_PAD := TOP_CLEAR - (360.0 - 240.0)
const BOTTOM_PAD := (2700.0 + STRIDE * 3.0 + 111.0) - BOTTOM_CLEAR - (CONTENT_H - 1920.0)

var profile: Profile
var current_level := 1
var offset := 0.0

var _content: Control
var _hud: Control
var _coins: Panel
var _title: Panel
var _title_world := 0
var _top_fade: TextureRect
var _bottom_fade: TextureRect
var _hit_rects: Array = []   # [Rect2 in content space, kind, id]
var _dragging := false
var _drag_travel := 0.0
var _velocity := 0.0
var _press_pos := Vector2.ZERO
var _bird: BirdView


func setup(player: Profile) -> void:
	profile = player
	for child in get_children():
		child.queue_free()
	_hit_rects.clear()
	_bird = null
	size = Vector2(1080, 1920)
	clip_contents = true
	mouse_filter = Control.MOUSE_FILTER_STOP
	var next := Progress.next_level(profile)
	current_level = next if next > 0 else 40
	_build_content()
	_build_hud()
	scroll_to(_node_y(current_level) - size.y * FOCUS_RATIO)

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
	offset = clampf(value, -TOP_PAD, CONTENT_H - size.y + BOTTOM_PAD)
	_content.position.y = -offset
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
	_content = Control.new()
	_content.name = "Trail"
	_content.size = Vector2(1080, CONTENT_H)
	_content.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(_content)
	# Overscroll rooms continue the painting: its edge strip reflected, so no dark band shows behind the bar.
	_overscroll_top()
	_overscroll_bottom()
	for world in range(1, 5):
		var bg := UI.image(_content, "map_bg_continuous_w%d" % world, Rect2(0, (4 - world) * STRIDE, 1080, SEGMENT), false)
		bg.name = "World %d" % world
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
			_circle(layer, c, NODE, "cream")
			UI.icon(layer, "lock", face, LOCK, LOCK_COLOR)
			layer.modulate = LOCKED_DIM
			_hit_rects.append([Rect2(c - Vector2.ONE * NODE * 0.5, Vector2.ONE * NODE).grow(16), "level", level])
		"cleared":
			_circle(layer, c, NODE, "cream")
			_number(layer, level, face, NODE_CAP, UI.INK)
			UI.stars(layer, Vector2(c.x, c.y + STAR_DY), STAR, STAR_GAP, profile.best_stars(level))
			_hit_rects.append([Rect2(c - Vector2.ONE * NODE * 0.5, Vector2(NODE, NODE * 0.5 + STAR_DY + STAR * 0.5)).grow(16), "level", level])
		"current":
			_circle(layer, c, CURRENT, "green")
			_number(layer, level, face, CURRENT_CAP, Color.WHITE)
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
	_bottom_fade = _fade(BOTTOM_FADE, BOTTOM_FADE_ALPHA, true)
	var coins := str(profile.coins)
	var coin_w := 100.0 + UI.number_width(coins, 44.0) + 32.0
	_coins = UI.counter(_hud, Vector2(1080.0 - MARGIN - coin_w, TOP_Y), "coin", coins)
	_coins.name = "Coins"
	_title_world = 0
	var buttons := [["Shop", "orange", "shop", shop_requested], ["Aviary", "orange", "aviary", aviary_requested],
		["Settings", "cream", "gear", settings_requested]]
	for i in buttons.size():
		var sig: Signal = buttons[i][3]
		UI.button(_hud, buttons[i][0], Vector2(540.0 + (i - 1) * NAV_GAP, NAV_Y), buttons[i][1], buttons[i][2], sig.emit)

## The room above the last world: the painting's top rows made opaque (they fade in over the world before, and
## nothing is above world 4), reflected upwards at the painting's top edge.
func _overscroll_top() -> void:
	var tex := Art.tex("map_bg_continuous_w4")
	var k := float(tex.get_height()) / SEGMENT
	var h := ceilf(maxf(TOP_PAD, OVERLAP) * k)
	var img := tex.get_image().get_region(Rect2i(0, 0, tex.get_width(), int(h)))
	img.convert(Image.FORMAT_RGB8)
	var opaque := ImageTexture.create_from_image(img)
	_strip(opaque, Rect2(0, 0, tex.get_width(), TOP_PAD * k), Rect2(0, -TOP_PAD, 1080, TOP_PAD), true)
	_strip(opaque, Rect2(0, 0, tex.get_width(), OVERLAP * k), Rect2(0, 0, 1080, OVERLAP), false)

## The room below the first world: its bottom rows reflected downwards.
func _overscroll_bottom() -> void:
	var tex := Art.tex("map_bg_continuous_w1")
	var h := BOTTOM_PAD * float(tex.get_height()) / SEGMENT
	_strip(tex, Rect2(0, tex.get_height() - h, tex.get_width(), h), Rect2(0, CONTENT_H, 1080, BOTTOM_PAD), true)

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
	_content.add_child(t)

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
	_bottom_fade.modulate = _paint_color(offset + BOTTOM_FADE.get_center().y)

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

## The world title follows the trail node nearest to the upper third of the screen.
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
	if _title != null:
		_title.queue_free()
	var text: String = WORLD_NAMES[world - 1]
	var w := UI.word_width(text, TITLE_CAP) + TITLE_PAD * 2.0
	_title = UI.shape(_hud, Rect2(MARGIN, TOP_Y, w, UI.COUNTER_H), "cream", UI.COUNTER_H * 0.5, 8.0)
	_title.name = "Title"
	UI.word(_title, text, Vector2(w * 0.5, (UI.COUNTER_H - 8.0) * 0.5), TITLE_CAP, UI.INK)

func _process(delta: float) -> void:
	if not _dragging and absf(_velocity) > 8.0:
		scroll_to(offset + _velocity * delta)
		_velocity *= pow(0.135, delta)
		if offset <= -TOP_PAD or offset >= CONTENT_H - size.y + BOTTOM_PAD:
			_velocity = 0.0

func _gui_input(event: InputEvent) -> void:
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
				_press_pos = mb.position
			elif _dragging:
				_dragging = false
				if _drag_travel < TAP_SLOP:
					_tap(mb.position)
	elif event is InputEventMouseMotion and _dragging:
		var mm := event as InputEventMouseMotion
		_drag_travel += absf(mm.relative.y) + absf(mm.relative.x)
		if _drag_travel >= TAP_SLOP:
			scroll_to(offset - mm.relative.y)
			_velocity = lerpf(_velocity, -mm.velocity.y, 0.5)

func _tap(screen_pos: Vector2) -> void:
	var p := screen_pos + Vector2(0, offset)
	for hit in _hit_rects:
		if (hit[0] as Rect2).has_point(p):
			if hit[1] == "level" and Progress.node_state(profile, hit[2]) != "locked":
				level_tapped.emit(hit[2])
			elif hit[1] == "gate":
				gate_tapped.emit(hit[2])
			return

## Fake data for captures. States: forest_trail, first_level, autumn_world, all_cleared, gate_locked.
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
	for level in range(1, cleared + 1):
		p.stars[level] = [3, 3, 2, 3, 1][(level - 1) % 5]
	p.highest_unlocked = mini(40, cleared + 1)
	var view: Control = load("res://game/map_view.gd").new()
	main.set_screen(view)
	view.setup(p)
	if state == "gate_locked":
		view.show_gate(1)
