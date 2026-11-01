extends Node2D
## Every actor has independent pose phase; true wing silhouettes are never synthesized.
var token_id := ""
var species := 0
var flying := false
var hovering := false
var phase := 0.0
var age := 0.0
var body: Sprite2D
var idle: Texture2D
var wings: Array[Texture2D] = []
const CYCLE := [0, 1, 2, 3, 4, 5, 4, 3, 2, 1]

func configure(token: Dictionary) -> void:
	token_id = token.id
	species = token.species
	phase = float(absi(token_id.hash()) % 1000) / 159.0
	var color := "red" if species == 0 else "blue"
	idle = load("res://assets/art/%s_idle.png" % color)
	for i in range(1, 7):
		wings.append(load("res://assets/art/%s_flight_%02d.png" % [color, i]))
	body = Sprite2D.new()
	body.texture = idle
	body.centered = false
	body.position = Vector2(-125, -439.0 * 250.0 / 512.0)
	body.scale = Vector2.ONE * (250.0 / 512.0)
	add_child(body)

func _process(delta: float) -> void:
	age += delta
	if flying:
		body.texture = wings[CYCLE[int(age / 0.036) % CYCLE.size()]]
		rotation = 0.0
		if hovering:
			body.position.y = -439.0 * 250.0 / 512.0 + sin(age * 8.0 + phase) * 2.0
	else:
		body.texture = idle
		# Tiny independent posture, not a painted/fake blink or a moving foot pivot.
		body.position.y = -439.0 * 250.0 / 512.0
		rotation = sin(age * 1.4 + phase) * 0.006

func set_flying(value: bool, hover: bool = false) -> void:
	flying = value
	hovering = hover
	if not value:
		body.texture = idle
