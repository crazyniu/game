class_name ChestOpening
extends RefCounted
## The star chest opens where it stands and at the size it has: the page's progress and actions fade, the chest shakes
## harder and harder, squashes, and pops its lid open in a flash of light with stars fountaining out of it. The
## reward page then shows the same chest at the same place, and the prizes fly out of it (Popups._chest_prize_burst).
## Every rendered frame projects the lid around its hinge; the box never morphs and never resizes.
const SHAKE := 0.5
const SQUASH := 0.12
const SWING := 0.42
const SETTLE := 0.22
const HOLD := 0.3
const DURATION := SHAKE + SQUASH + SWING + SETTLE + HOLD

static func prepare() -> void:
	Sound.prepare_effects(["crate_open", "star_1"])

static func play(dialog: Modal, finished: Callable) -> void:
	var layout = load("res://game/popups.gd")
	var hero: ChestLid = dialog.content.get_node("ChestHero")
	for meta in ["breath", "idle"]:
		if hero.has_meta(meta):
			(hero.get_meta(meta) as Tween).kill()
	hero.setup(layout.CHEST_HERO.end.y, layout.CHEST_HERO.size.x)
	hero.rotation = 0.0
	var base := hero.scale
	# The page clears down to the chest.
	for name in ["OpenChest", "Play", "Progress", "ReadyCount"]:
		var node := dialog.content.get_node_or_null(name) as CanvasItem
		if node != null:
			node.create_tween().tween_property(node, "modulate:a", 0.0, 0.2)
	var motion := dialog.create_tween()
	# Shake: four swings, each wider than the last, around the chest's feet.
	var swings := [2.5, -4.0, 5.5, -7.0, 0.0]
	for i in swings.size():
		motion.tween_property(hero, "rotation", deg_to_rad(swings[i]), SHAKE / swings.size()).set_trans(Tween.TRANS_SINE)
	motion.tween_property(hero, "scale", base * Vector2(1.08, 0.9), SQUASH).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_IN)
	motion.tween_callback(func():
		if dialog._closing:
			return
		Sound.play("crate_open", 1.0, -4.0)
		_burst(dialog, layout.CHEST_MOUTH))
	# Pop: the lid swings past open while the box springs up, then both settle.
	motion.tween_property(hero, "angle", ChestLid.OPEN_ANGLE + 4.0, SWING).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	motion.parallel().tween_property(hero, "scale", base * Vector2(0.95, 1.08), SWING * 0.45).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
	motion.parallel().tween_property(hero, "scale", base, SWING * 0.55).set_delay(SWING * 0.45).set_trans(Tween.TRANS_ELASTIC).set_ease(Tween.EASE_OUT)
	motion.tween_callback(func():
		if not dialog._closing:
			Sound.play("star_1", 1.1, -8.0))
	motion.tween_property(hero, "angle", ChestLid.OPEN_ANGLE, SETTLE).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	motion.tween_interval(HOLD)
	motion.tween_callback(func():
		if not dialog._closing:
			dialog.set_meta("chest_landed", true)
			finished.call())

## A flash of warm light swells from the chest's mouth and a fountain of stars shoots up and falls away.
static func _burst(dialog: Modal, mouth: Vector2) -> void:
	var flash := UI.image(dialog.content, "glow_soft", Rect2(mouth - Vector2(140, 140), Vector2(280, 280)))
	flash.name = "OpenFlash"
	flash.pivot_offset = flash.size * 0.5
	flash.modulate = Color(1.0, 0.92, 0.55, 0.0)
	var f := flash.create_tween()
	f.tween_property(flash, "modulate:a", 1.0, 0.08)
	f.parallel().tween_property(flash, "scale", Vector2.ONE * 3.2, 0.5).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
	f.tween_property(flash, "modulate:a", 0.0, 0.45)
	f.tween_callback(flash.queue_free)
	for i in 18:
		var star := UI.image(dialog.content, "sparkle" if i % 3 != 0 else "island_star_full", Rect2(mouth - Vector2(26, 26), Vector2(52, 52)))
		star.pivot_offset = star.size * 0.5
		var spread := lerpf(-0.95, 0.95, float(i) / 17.0) + randf_range(-0.12, 0.12)
		var speed := randf_range(620.0, 980.0)
		var velocity := Vector2(sin(spread), -cos(spread)) * speed
		var spin := randf_range(-6.0, 6.0)
		var life := randf_range(0.8, 1.1)
		var size := randf_range(0.7, 1.3)
		var fly := func(t: float) -> void:
			var at := mouth + velocity * t + Vector2(0, 1500.0) * t * t
			star.position = at - star.size * 0.5
			star.rotation = spin * t
			star.scale = Vector2.ONE * size * (0.4 + 0.6 * minf(1.0, t * 6.0))
			star.modulate.a = clampf((life - t) * 3.0, 0.0, 1.0)
		var tw := star.create_tween()
		tw.tween_method(fly, 0.0, life, life)
		tw.tween_callback(star.queue_free)
