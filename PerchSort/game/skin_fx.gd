class_name SkinFx
extends Node2D
## Living effects of the advanced costumes (outfits 3 to 5), drawn around a BirdView body in its 320 px canvas space
## (feet at the origin). Outfit 3 breathes a soft aura with a few twinkles; outfit 4 sheds its species' own particles
## (embers, petals, leaves, snow...); outfit 5 adds a strong pulsing aura and a ring of orbiting sparkles on top.
## Everything is moving every frame (no still picture); the bird's painted frames are untouched.

const AURA_Y := -125.0
## species -> [outfit 4 spec, outfit 5 spec]; mode: rise | fall | twinkle | orbit.
const SPECS := {
	"ember": [{"mode": "rise", "tex": "firefly", "color": Color("ff8a3c"), "n": 8, "size": 0.8},
		{"mode": "rise", "tex": "sparkle", "color": Color("ff5a22"), "n": 11, "size": 0.38}],
	"berry": [{"mode": "fall", "tex": "fall_petal_pink", "color": Color("ffffff"), "n": 6, "size": 0.26},
		{"mode": "fall", "tex": "fall_petal_pink", "color": Color("ffd0e8"), "n": 9, "size": 0.3}],
	"cocoa": [{"mode": "rise", "tex": "glow_soft", "color": Color("f4dcc0"), "n": 6, "size": 0.12},
		{"mode": "twinkle", "tex": "sparkle", "color": Color("ffd58a"), "n": 9, "size": 0.34}],
	"hoot": [{"mode": "fall", "tex": "fall_leaf_gold", "color": Color("ffffff"), "n": 6, "size": 0.24},
		{"mode": "orbit", "tex": "sparkle", "color": Color("ffe27a"), "n": 7, "size": 0.32}],
	"ink": [{"mode": "twinkle", "tex": "sparkle", "color": Color("b79cff"), "n": 8, "size": 0.34},
		{"mode": "orbit", "tex": "sparkle", "color": Color("8fa6ff"), "n": 8, "size": 0.34}],
	"mint": [{"mode": "rise", "tex": "firefly", "color": Color("9bff9b"), "n": 8, "size": 0.8},
		{"mode": "fall", "tex": "fall_leaf_green", "color": Color("ffffff"), "n": 9, "size": 0.24}],
	"sky": [{"mode": "fall", "tex": "fall_petal_cream", "color": Color("e8f6ff"), "n": 6, "size": 0.26},
		{"mode": "orbit", "tex": "sparkle", "color": Color("9fdcff"), "n": 8, "size": 0.34}],
	"snow": [{"mode": "fall", "tex": "sparkle", "color": Color("ffffff"), "n": 9, "size": 0.26},
		{"mode": "fall", "tex": "sparkle", "color": Color("cfeaff"), "n": 13, "size": 0.32}],
	"sunny": [{"mode": "rise", "tex": "sparkle", "color": Color("ffe066"), "n": 8, "size": 0.3},
		{"mode": "orbit", "tex": "sparkle", "color": Color("ffd23a"), "n": 8, "size": 0.36}],
}
const AURA_COLOR := {"ember": Color("ff6a28"), "berry": Color("ff8cc4"), "cocoa": Color("e0a868"), "hoot": Color("ffd75a"),
	"ink": Color("7a68ff"), "mint": Color("6cf0a0"), "sky": Color("7cc8ff"), "snow": Color("b8e4ff"), "sunny": Color("ffd23a")}

var _species := ""
var _grade := 0
var _aura: Sprite2D
var _back: Node2D          # layer behind the bird that holds the aura
var _items: Array = []     # [Sprite2D, mode, phase, speed, seed]
var _clock := 0.0
var _awake := true


## Grade is the outfit number (1 to 5); species the species id. Grades below 3 have no effect.
static func wanted(outfit_key: String) -> bool:
	return outfit_key.contains("_outfit_") and int(outfit_key.get_slice("_outfit_", 1)) >= 3

func setup(outfit_key: String, back: Node2D) -> void:
	_back = back
	_species = outfit_key.get_slice("_outfit_", 0)
	_grade = int(outfit_key.get_slice("_outfit_", 1))
	for child in get_children() + _back.get_children():
		child.queue_free()
	_items.clear()
	var add := CanvasItemMaterial.new()
	add.blend_mode = CanvasItemMaterial.BLEND_MODE_ADD
	_aura = Sprite2D.new()
	_aura.texture = Art.tex("glow_soft")
	_aura.position = Vector2(0, AURA_Y)
	_aura.modulate = Color(AURA_COLOR.get(_species, Color.WHITE), 0.0)
	_back.add_child(_aura)
	var specs: Array = SPECS.get(_species, SPECS["sunny"])
	if _grade == 3:
		_particles(add, {"mode": "twinkle", "tex": "sparkle", "color": Color(AURA_COLOR.get(_species, Color.WHITE)).lightened(0.4), "n": 3, "size": 0.26})
	elif _grade == 4:
		_particles(add, specs[0])
	elif _grade >= 5:
		_particles(add, specs[1])
		var ring: Dictionary = {"mode": "orbit", "tex": "sparkle", "color": Color(AURA_COLOR.get(_species, Color.WHITE)).lightened(0.5), "n": 4, "size": 0.3}
		if specs[1]["mode"] != "orbit":
			_particles(add, ring)
	set_process(true)

func set_awake(value: bool) -> void:
	_awake = value
	visible = value
	_back.visible = value

func _particles(add: Material, spec: Dictionary) -> void:
	var tex: Texture2D = Art.tex(spec["tex"])
	var glowing: bool = spec["tex"] in ["sparkle", "firefly", "glow_soft"]
	for i in int(spec["n"]):
		var s := Sprite2D.new()
		s.texture = tex
		s.modulate = Color(spec["color"], 0.0)
		var base: float = float(spec["size"]) * 2.3 * 128.0 / maxf(tex.get_width(), 1.0)
		s.set_meta("base", base)
		add_child(s)
		_items.append([s, spec["mode"], randf(), randf_range(0.7, 1.3), randf() * TAU])

func _process(delta: float) -> void:
	if not _awake:
		return
	_clock += delta
	var pulse := 0.5 + 0.5 * sin(_clock * (1.6 if _grade >= 5 else 1.1))
	var aura_alpha := 0.0
	if _grade == 3:
		aura_alpha = 0.16 + 0.10 * pulse
	elif _grade == 4:
		aura_alpha = 0.22 + 0.12 * pulse
	elif _grade >= 5:
		aura_alpha = 0.50 + 0.22 * pulse
	_aura.modulate.a = aura_alpha
	_aura.scale = Vector2.ONE * (0.62 + 0.10 * pulse) * (1.2 if _grade >= 5 else 1.0)
	for it in _items:
		var s: Sprite2D = it[0]
		var phase := fposmod(it[2] + _clock * 0.22 * it[3], 1.0)   # 0..1 life of this particle
		var base: float = s.get_meta("base")
		var fade := sin(phase * PI)                                   # in and out
		var c := s.modulate
		match it[1]:
			"rise":
				s.position = Vector2(signf(sin(it[4])) * (62.0 + 30.0 * absf(cos(it[4] * 3.0))) + sin(_clock * 1.7 + it[4]) * 14.0, -20.0 - phase * 250.0)
				s.scale = Vector2.ONE * base * (1.0 - 0.5 * phase)
				c.a = fade * 0.95
			"fall":
				s.position = Vector2(signf(sin(it[4])) * (72.0 + 30.0 * absf(cos(it[4] * 3.0))) + sin(_clock * 1.3 + it[4] * 2.0) * 22.0, -290.0 + phase * 290.0)
				s.rotation = _clock * 0.9 * it[3] + it[4]
				s.scale = Vector2.ONE * base
				c.a = fade * 0.9
			"twinkle":
				s.position = Vector2(signf(cos(it[4])) * (88.0 + 24.0 * absf(sin(it[4] * 2.0))), AURA_Y + sin(it[4] * 1.7) * 125.0)
				var k := pow(fade, 2.0)
				s.scale = Vector2.ONE * base * (0.3 + 0.9 * k)
				s.rotation = _clock * 0.8 + it[4]
				c.a = k
			"orbit":
				var ang: float = it[4] + _clock * 1.3 * it[3]
				s.position = Vector2(cos(ang) * 112.0, AURA_Y + sin(ang) * 46.0 - 30.0 + sin(_clock * 2.0 + it[4]) * 10.0)
				s.scale = Vector2.ONE * base * (0.75 + 0.35 * sin(_clock * 4.0 + it[4]))
				s.rotation = _clock * 1.5
				c.a = 0.95 if sin(ang) < 0.0 else 0.0   # only the back half of the ring shows, never over the face
		s.modulate = c
