class_name Ambient
extends Control
## Living forest light over every screen: sunbeams that turn and breathe from the top-left corner, and leaves and
## petals that drift down, sway, spin and tumble. Sits above the screen and below the popups; never takes input.

const SPRITES := ["fall_leaf_lime", "fall_leaf_green", "fall_leaf_olive", "fall_petal_cream", "fall_petal_pink", "fall_leaf_gold"]
const SUN_CORNER := Vector2(-40.0, -40.0)

var still := false               # captures: leaves stay where they were seeded
var _clock := 0.0
var _beams: Array[TextureRect] = []
var _glow: TextureRect
var _leaves: Array[Dictionary] = []   # {node, x, speed, sway, freq, phase, spin, tumble, drift}
var _count := 0
var _sun_strength := 1.0

func _ready() -> void:
	name = "Ambient"
	mouse_filter = Control.MOUSE_FILTER_IGNORE
	size = Vector2(1080, 1920)
	var add := CanvasItemMaterial.new()
	add.blend_mode = CanvasItemMaterial.BLEND_MODE_ADD
	for i in 2:
		var beam := TextureRect.new()
		beam.texture = Art.tex("light_rays")
		beam.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
		beam.mouse_filter = Control.MOUSE_FILTER_IGNORE
		beam.material = add
		beam.size = Vector2.ONE * (1500.0 if i == 0 else 1050.0)
		beam.pivot_offset = beam.size * 0.5
		beam.position = SUN_CORNER - beam.size * 0.5
		add_child(beam)
		_beams.append(beam)
	_glow = TextureRect.new()
	_glow.texture = Art.tex("glow_soft")
	_glow.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	_glow.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_glow.material = add
	_glow.size = Vector2.ONE * 520.0
	_glow.position = SUN_CORNER - _glow.size * 0.5
	add_child(_glow)
	set_density(10, 1.0)
	_apply(0.0)

## How many leaves are in the air and how strong the sunbeams are (the board keeps both light).
func set_density(count: int, sun: float) -> void:
	_sun_strength = sun
	_count = count
	while _leaves.size() > count:
		var gone: Dictionary = _leaves.pop_back()
		(gone["node"] as Node).queue_free()
	while _leaves.size() < count:
		_add_leaf()

func _add_leaf() -> void:
	var node := TextureRect.new()
	node.texture = Art.tex(SPRITES[randi() % SPRITES.size()])
	node.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	node.mouse_filter = Control.MOUSE_FILTER_IGNORE
	if node.texture:
		node.size = node.texture.get_size()
	node.pivot_offset = node.size * 0.5
	add_child(node)
	var leaf := {"node": node}
	_reseed(leaf, true)
	_leaves.append(leaf)

func _reseed(leaf: Dictionary, anywhere: bool) -> void:
	var node: TextureRect = leaf["node"]
	var s := randf_range(0.34, 0.62)
	node.scale = Vector2.ONE * s
	leaf["x"] = randf_range(-40.0, 1120.0)
	leaf["y"] = randf_range(-80.0, 1900.0) if anywhere else randf_range(-200.0, -60.0)
	leaf["speed"] = randf_range(70.0, 125.0) * (0.75 + s)
	leaf["sway"] = randf_range(26.0, 70.0)
	leaf["freq"] = randf_range(0.7, 1.4)
	leaf["phase"] = randf_range(0.0, TAU)
	leaf["spin"] = randf_range(-0.9, 0.9)
	leaf["tumble"] = randf_range(1.2, 2.6)
	leaf["drift"] = randf_range(-10.0, 22.0)    # a light breeze pushes most leaves to the right
	leaf["scale"] = s
	node.rotation = randf_range(-PI, PI)

func _process(delta: float) -> void:
	if still:
		return
	_clock += delta
	_apply(delta)

func _apply(delta: float) -> void:
	# Sunbeams: two layers turn slowly in opposite directions and breathe out of step, so the light shimmers.
	_beams[0].rotation = _clock * 0.05
	_beams[1].rotation = -_clock * 0.08 + 0.4
	_beams[0].modulate.a = (0.055 + 0.03 * sin(_clock * 0.7)) * _sun_strength
	_beams[1].modulate.a = (0.04 + 0.03 * sin(_clock * 1.1 + 1.6)) * _sun_strength
	_glow.modulate.a = (0.09 + 0.05 * sin(_clock * 0.9 + 0.8)) * _sun_strength
	_glow.scale = Vector2.ONE * (1.0 + 0.05 * sin(_clock * 0.9 + 0.8))
	_glow.pivot_offset = _glow.size * 0.5
	for leaf in _leaves:
		var node: TextureRect = leaf["node"]
		leaf["y"] += leaf["speed"] * delta
		leaf["x"] += leaf["drift"] * delta
		if leaf["y"] > 1980.0 or leaf["x"] > 1180.0 or leaf["x"] < -140.0:
			_reseed(leaf, false)
		var t: float = _clock * leaf["freq"] + leaf["phase"]
		node.position = Vector2(leaf["x"] + leaf["sway"] * sin(t), leaf["y"]) - node.pivot_offset
		node.rotation += leaf["spin"] * delta + 0.012 * cos(t)
		# Tumbling: the leaf turns edge-on and back, a real change of pose rather than a flip of the picture.
		var squash := 0.45 + 0.55 * absf(cos(_clock * leaf["tumble"] + leaf["phase"]))
		node.scale = Vector2(leaf["scale"], leaf["scale"] * squash)
		node.modulate.a = clampf(minf((leaf["y"] + 80.0) / 160.0, (1980.0 - leaf["y"]) / 160.0), 0.0, 0.95)
