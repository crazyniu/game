class_name Ambient
extends Control
## Living light and air over a forest painting. The home page uses the default look (sunbeams from the top-left
## corner, leaves and petals drifting down) above the screen and below the popups. A level uses `for_level()`:
## the same layer sits between the world painting and the branches, so the scene breathes behind play, with
## falling things, mist, floating motes and light chosen by the world (spring, bamboo, autumn, snow, blossom,
## mushrooms, willow lake, misty pines, waterfall jungle, wisteria) and the
## level's weather variant. Never takes input.

const SPRITES := ["fall_leaf_lime", "fall_leaf_green", "fall_leaf_olive", "fall_petal_cream", "fall_petal_pink", "fall_leaf_gold"]
const SUN_CORNER := Vector2(-40.0, -40.0)
const AUTUMN_ORANGE := Color(1.0, 0.66, 0.42)
const AUTUMN_RED := Color(1.0, 0.46, 0.36)
const WISTERIA := Color(0.82, 0.7, 1.0)

var still := false               # captures: everything stays where it was seeded
var night := 0.0:                # 0 day .. 1 night: the sun fades out and falling things take moonlight
	set(value):
		night = value
		_apply(0.0)
var _clock := 0.0
var _beams: Array[TextureRect] = []
var _glow: TextureRect
var _leaves: Array[Dictionary] = []   # {node, x, y, speed, sway, freq, phase, spin, tumble, drift, scale}
var _mists: Array[Dictionary] = []    # {node, x, y, speed, alpha, phase}
var _motes: Array[Dictionary] = []    # {node, at, reach, speed, rise, phase, size}
var _aurora: Array[TextureRect] = []
var _count := 0
var _sun_strength := 1.0

## The look: what falls (sprite names, each optionally [name, tint]), how many, how big, how fast, how windy,
## whether pieces tumble like leaves or float like snow, the sun, mist bands, floating motes and an aurora.
var look := {}
const DEFAULT_LOOK := {"fall": SPRITES, "count": 10, "scale": [0.34, 0.62], "speed": 1.0, "wind": 0.0,
	"sway": 1.0, "tumble": true, "sun": 1.0, "sun_tint": Color.WHITE, "mist": 0, "mist_tint": Color.WHITE,
	"motes": 0, "mote_tint": Color(1.0, 0.95, 0.8), "aurora": false}

## The look of a level: its world sets the scene, its variant the weather and the light.
static func for_level(world: int, variant: String) -> Dictionary:
	var l := DEFAULT_LOOK.duplicate()
	l["sun"] = 0.8
	match world:
		1:   # spring forest: fresh leaves and blossom petals
			l["fall"] = ["fall_leaf_lime", "fall_leaf_green", "fall_petal_pink", "fall_petal_cream", "fall_petal_pink"]
			l["count"] = 7
			l["scale"] = [0.28, 0.5]
		2:   # bamboo grove: slim leaves through drifting mist
			l["fall"] = ["fall_leaf_olive", "fall_leaf_olive", "fall_leaf_lime"]
			l["count"] = 6
			l["scale"] = [0.3, 0.5]
			l["mist"] = 3
			l["mist_tint"] = Color(0.9, 1.0, 0.96)
			l["sun"] = 0.6
		3:   # autumn maples: red, orange and gold leaves
			l["fall"] = ["fall_leaf_gold", ["fall_leaf_gold", AUTUMN_ORANGE], ["fall_leaf_gold", AUTUMN_RED],
				["fall_leaf_gold", AUTUMN_RED], ["fall_leaf_olive", AUTUMN_ORANGE]]
			l["count"] = 9
			l["scale"] = [0.3, 0.52]
			l["sun_tint"] = Color(1.0, 0.92, 0.8)
		4:   # snow forest: soft flakes float down, no tumbling
			l["fall"] = [["glow_soft", Color(1.0, 1.0, 1.0)]]
			l["count"] = 44
			l["scale"] = [0.06, 0.12]
			l["speed"] = 0.45
			l["sway"] = 0.6
			l["tumble"] = false
			l["sun"] = 0.6
			l["sun_tint"] = Color(0.92, 0.94, 1.0)
		5:   # cherry blossom: a steady fall of pink and cream petals
			l["fall"] = ["fall_petal_pink", "fall_petal_pink", "fall_petal_cream"]
			l["count"] = 12
			l["scale"] = [0.24, 0.42]
			l["speed"] = 0.8
			l["sun_tint"] = Color(1.0, 0.94, 0.96)
		6:   # mushroom wood: a few green leaves, glowing spores rising from the floor
			l["fall"] = ["fall_leaf_green", "fall_leaf_lime"]
			l["count"] = 5
			l["motes"] = 10
			l["mote_tint"] = Color(0.85, 1.0, 0.7)
		7:   # willow lake: slim willow leaves, glints over the water
			l["fall"] = ["fall_leaf_lime", "fall_leaf_olive"]
			l["count"] = 6
			l["scale"] = [0.26, 0.44]
			l["motes"] = 8
			l["mote_tint"] = Color(0.9, 0.97, 1.0)
		8:   # misty pines: mist drifting through the valley, hardly anything falls
			l["fall"] = ["fall_leaf_olive"]
			l["count"] = 3
			l["scale"] = [0.24, 0.38]
			l["mist"] = 4
			l["mist_tint"] = Color(0.92, 1.0, 1.0)
			l["sun"] = 0.6
		9:   # waterfall jungle: big green leaves, spray mist low down
			l["fall"] = ["fall_leaf_green", "fall_leaf_green", "fall_leaf_lime"]
			l["count"] = 6
			l["scale"] = [0.36, 0.6]
			l["mist"] = 2
			l["sun"] = 0.9
		10:  # wisteria: lilac petals drift through violet light
			l["fall"] = [["fall_petal_pink", WISTERIA], ["fall_petal_cream", WISTERIA], "fall_petal_cream"]
			l["count"] = 10
			l["scale"] = [0.22, 0.4]
			l["speed"] = 0.8
			l["sun_tint"] = Color(0.96, 0.9, 1.0)
	match variant:
		"morning":   # fresh light with pollen in the air
			l["sun"] *= 1.2
			l["motes"] = 8
		"golden":
			l["sun"] *= 1.3
			l["sun_tint"] = Color(1.0, 0.85, 0.55)
			l["motes"] = 10
			l["mote_tint"] = Color(1.0, 0.85, 0.5)
		"sunset":
			l["sun"] *= 1.3
			l["sun_tint"] = Color(1.0, 0.66, 0.46)
			l["motes"] = 10
			l["mote_tint"] = Color(1.0, 0.72, 0.5)
		"drizzle":
			l["sun"] *= 0.3
			l["count"] = int(l["count"] * 0.6)
			l["mist"] += 1
		"rain":
			l["sun"] = 0.0
			l["count"] = int(l["count"] * 0.5)
			l["mist"] += 1
		"mist":
			l["sun"] *= 0.5
			l["mist"] += 4
		"windy":
			l["wind"] = 150.0
			l["speed"] *= 1.4
			l["sway"] *= 1.5
			l["count"] = int(l["count"] * 1.6)
		"bluehour":   # dusk: no sun, cool glints in the air
			l["sun"] = 0.0
			l["motes"] = 10
			l["mote_tint"] = Color(0.75, 0.86, 1.0)
		"lanterns":   # warm lantern lights rising through the grove
			l["sun"] *= 0.3
			l["motes"] = 14
			l["mote_tint"] = Color(1.0, 0.72, 0.4)
		"aurora":
			l["sun"] = 0.0
			l["aurora"] = true
			l["motes"] = 8
			l["mote_tint"] = Color(0.7, 1.0, 0.9)
	return l

func _ready() -> void:
	name = "Ambient"
	mouse_filter = Control.MOUSE_FILTER_IGNORE
	size = Vector2(1080, 1920)
	for key in DEFAULT_LOOK:
		if not look.has(key):
			look[key] = DEFAULT_LOOK[key]
	var add := CanvasItemMaterial.new()
	add.blend_mode = CanvasItemMaterial.BLEND_MODE_ADD
	if look["aurora"]:
		_build_aurora()
	for i in look["mist"]:
		_add_mist(i)
	for i in 2:
		var beam := _rect("light_rays", add)
		beam.size = Vector2.ONE * (1500.0 if i == 0 else 1050.0)
		beam.pivot_offset = beam.size * 0.5
		beam.position = SUN_CORNER - beam.size * 0.5
		beam.self_modulate = look["sun_tint"]
		_beams.append(beam)
	_glow = _rect("glow_soft", add)
	_glow.size = Vector2.ONE * 520.0
	_glow.position = SUN_CORNER - _glow.size * 0.5
	_glow.self_modulate = look["sun_tint"]
	for i in look["motes"]:
		_add_mote(i, add)
	set_density(look["count"], look["sun"])
	_apply(0.0)

func _rect(texture: String, material: Material = null) -> TextureRect:
	var r := TextureRect.new()
	r.texture = Art.tex(texture)
	r.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	r.mouse_filter = Control.MOUSE_FILTER_IGNORE
	r.material = material
	add_child(r)
	return r

## How many pieces are in the air and how strong the sunbeams are.
func set_density(count: int, sun: float) -> void:
	_sun_strength = sun
	_count = count
	while _leaves.size() > count:
		var gone: Dictionary = _leaves.pop_back()
		(gone["node"] as Node).queue_free()
	while _leaves.size() < count:
		_add_leaf()

func _add_leaf() -> void:
	var pick = look["fall"][randi() % look["fall"].size()]
	var node := _rect(pick[0] if pick is Array else pick)
	if pick is Array:
		node.self_modulate = pick[1]
	if node.texture:
		node.size = node.texture.get_size()
	node.pivot_offset = node.size * 0.5
	var leaf := {"node": node}
	_reseed(leaf, true)
	_leaves.append(leaf)

func _reseed(leaf: Dictionary, anywhere: bool) -> void:
	var node: TextureRect = leaf["node"]
	var k := randf()
	var s := lerpf(look["scale"][0], look["scale"][1], k)
	node.scale = Vector2.ONE * s
	leaf["x"] = randf_range(-40.0, 1120.0)
	leaf["y"] = randf_range(-80.0, 1900.0) if anywhere else randf_range(-200.0, -60.0)
	leaf["speed"] = randf_range(70.0, 125.0) * (1.09 + 0.28 * k) * look["speed"]   # bigger pieces fall faster
	leaf["sway"] = randf_range(26.0, 70.0) * look["sway"]
	leaf["freq"] = randf_range(0.7, 1.4)
	leaf["phase"] = randf_range(0.0, TAU)
	leaf["spin"] = randf_range(-0.9, 0.9) if look["tumble"] else 0.0
	leaf["tumble"] = randf_range(1.2, 2.6)
	leaf["drift"] = randf_range(-10.0, 22.0) + look["wind"] * randf_range(0.7, 1.2)   # a breeze to the right
	leaf["scale"] = s
	node.rotation = randf_range(-PI, PI) if look["tumble"] else 0.0

## Wide soft bands of mist that slide sideways across the lower forest at their own pace.
func _add_mist(i: int) -> void:
	var node := _rect("glow_soft")
	node.size = Vector2(randf_range(1300.0, 1700.0), randf_range(300.0, 420.0))
	node.self_modulate = look["mist_tint"]
	var band := {"node": node, "x": randf_range(-900.0, 600.0), "y": 700.0 + fposmod(i * 389.0, 1050.0),
		"speed": randf_range(14.0, 30.0) * (1.0 if i % 2 == 0 else -1.0), "alpha": randf_range(0.32, 0.5),
		"phase": randf_range(0.0, TAU)}
	_mists.append(band)

## Small glowing motes (pollen, glints, lantern lights) that wander on slow loops, rise a little and blink.
func _add_mote(i: int, add: Material) -> void:
	var node := _rect("glow_soft", add)
	var px := randf_range(26.0, 48.0)
	node.size = Vector2.ONE * px
	node.pivot_offset = node.size * 0.5
	node.self_modulate = look["mote_tint"]
	_motes.append({"node": node, "at": Vector2(randf_range(60.0, 1020.0), randf_range(420.0, 1800.0)),
		"reach": Vector2(randf_range(40.0, 90.0), randf_range(30.0, 60.0)), "speed": randf_range(0.18, 0.34),
		"rise": randf_range(10.0, 26.0), "phase": randf_range(0.0, TAU), "size": px})

## Aurora: two broad coloured veils high in the sky that wave and breathe.
func _build_aurora() -> void:
	for i in 2:
		var veil := _rect("glow_soft")
		veil.size = Vector2(1500.0, 460.0)
		veil.pivot_offset = veil.size * 0.5
		veil.position = Vector2(540.0, 260.0 + 140.0 * i) - veil.pivot_offset
		veil.self_modulate = Color(0.35, 0.95, 0.7) if i == 0 else Color(0.7, 0.45, 1.0)
		_aurora.append(veil)

func _process(delta: float) -> void:
	if still:
		return
	_clock += delta
	_apply(delta)

func _apply(delta: float) -> void:
	if _glow == null:
		return
	var sun := _sun_strength * (1.0 - night)
	# Sunbeams: two layers turn slowly in opposite directions and breathe out of step, so the light shimmers.
	_beams[0].rotation = _clock * 0.05
	_beams[1].rotation = -_clock * 0.08 + 0.4
	_beams[0].modulate.a = (0.055 + 0.03 * sin(_clock * 0.7)) * sun
	_beams[1].modulate.a = (0.04 + 0.03 * sin(_clock * 1.1 + 1.6)) * sun
	_glow.modulate.a = (0.09 + 0.05 * sin(_clock * 0.9 + 0.8)) * sun
	_glow.scale = Vector2.ONE * (1.0 + 0.05 * sin(_clock * 0.9 + 0.8))
	_glow.pivot_offset = _glow.size * 0.5
	var moon := Color.WHITE.lerp(Color(0.7, 0.76, 0.98), night)
	for leaf in _leaves:
		var node: TextureRect = leaf["node"]
		leaf["y"] += leaf["speed"] * delta
		leaf["x"] += leaf["drift"] * delta
		if leaf["y"] > 1980.0 or leaf["x"] > 1180.0 or leaf["x"] < -140.0:
			_reseed(leaf, false)
			if look["wind"] > 0.0:   # a windy breeze carries pieces in from the left edge too
				leaf["x"] = randf_range(-500.0, 900.0)
		var t: float = _clock * leaf["freq"] + leaf["phase"]
		node.position = Vector2(leaf["x"] + leaf["sway"] * sin(t), leaf["y"]) - node.pivot_offset
		var squash := 1.0
		if look["tumble"]:
			node.rotation += leaf["spin"] * delta + 0.012 * cos(t)
			# Tumbling: the leaf turns edge-on and back, a real change of pose rather than a flip of the picture.
			squash = 0.45 + 0.55 * absf(cos(_clock * leaf["tumble"] + leaf["phase"]))
		node.scale = Vector2(leaf["scale"], leaf["scale"] * squash)
		node.modulate = moon
		node.modulate.a = clampf(minf((leaf["y"] + 80.0) / 160.0, (1980.0 - leaf["y"]) / 160.0), 0.0, 0.95)
	for band in _mists:
		var node: TextureRect = band["node"]
		band["x"] += band["speed"] * delta
		if band["x"] > 1180.0:
			band["x"] = -node.size.x - 100.0
		elif band["x"] < -node.size.x - 100.0:
			band["x"] = 1180.0
		node.position = Vector2(band["x"], band["y"] + 18.0 * sin(_clock * 0.25 + band["phase"]))
		node.modulate = moon
		node.modulate.a = band["alpha"] * (0.8 + 0.2 * sin(_clock * 0.4 + band["phase"]))
	for m in _motes:
		var node: TextureRect = m["node"]
		var t: float = _clock * m["speed"] + m["phase"]
		var at: Vector2 = m["at"] + Vector2(cos(t) * m["reach"].x, sin(t * 1.3) * m["reach"].y)
		at.y -= fposmod(_clock * m["rise"] + m["phase"] * 100.0, 300.0)   # drifts upward, then starts again
		node.position = at - node.pivot_offset
		var blink := 0.5 + 0.5 * sin(_clock * (1.2 + m["speed"] * 3.0) + m["phase"] * 2.0)
		var cycle := fposmod(_clock * m["rise"] + m["phase"] * 100.0, 300.0) / 300.0
		node.modulate.a = (0.35 + 0.55 * blink) * minf(1.0, minf(cycle, 1.0 - cycle) * 6.0)
		node.scale = Vector2.ONE * (0.8 + 0.3 * blink)
	for i in _aurora.size():
		var veil := _aurora[i]
		veil.rotation = 0.06 * sin(_clock * 0.15 + i * 2.0)
		veil.scale = Vector2(1.0 + 0.08 * sin(_clock * 0.2 + i), 0.8 + 0.25 * sin(_clock * 0.33 + i * 1.7))
		veil.position.x = 540.0 - veil.pivot_offset.x + 120.0 * sin(_clock * 0.09 + i * 3.0)
		veil.modulate.a = 0.34 + 0.14 * sin(_clock * 0.5 + i * 1.3)
