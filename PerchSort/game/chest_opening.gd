class_name ChestOpening
extends RefCounted
## Every rendered frame projects the lid around its hinge; the box never morphs.
const ANTICIPATION := 0.16
const SWING := 0.68
const SETTLE := 0.28
const LAND := 0.28
const DURATION := ANTICIPATION + SWING + SETTLE + LAND

static func prepare() -> void:
	Sound.prepare_effects(["crate_open", "star_1"])

static func play(dialog: Modal, finished: Callable) -> void:
	var layout = load("res://game/popups.gd")
	var hero: ChestLid = dialog.content.get_node("ChestHero")
	if hero.has_meta("breath"):
		(hero.get_meta("breath") as Tween).kill()
	hero.setup(layout.CHEST_HERO.end.y, layout.CHEST_HERO.size.x)
	var motion := dialog.create_tween()
	motion.tween_property(hero, "angle", ChestLid.CLOSED_ANGLE - 3.0, ANTICIPATION * 0.5).set_trans(Tween.TRANS_SINE)
	motion.tween_property(hero, "angle", ChestLid.CLOSED_ANGLE, ANTICIPATION * 0.5).set_trans(Tween.TRANS_SINE)
	motion.tween_callback(func():
		if not dialog._closing:
			Sound.play("crate_open", 1.0, -4.0))
	motion.tween_property(hero, "angle", ChestLid.OPEN_ANGLE + 3.0, SWING).set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)
	motion.tween_callback(func():
		if not dialog._closing:
			Sound.play("star_1", 1.1, -8.0))
	motion.tween_property(hero, "angle", ChestLid.OPEN_ANGLE, SETTLE).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	var support: Control = dialog.content.get_node("ChestSupport")
	var support_start := support.position
	motion.tween_method(func(t: float):
		hero.setup(lerpf(layout.CHEST_HERO.end.y, layout.CHEST_RESULT_HERO.end.y, t), lerpf(layout.CHEST_HERO.size.x, layout.CHEST_RESULT_HERO.size.x, t))
		support.position = support_start + Vector2(0, layout.CHEST_RESULT_SUPPORT.position.y - layout.CHEST_SUPPORT.position.y) * t,
		0.0, 1.0, LAND).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	motion.tween_callback(func():
		if not dialog._closing:
			dialog.set_meta("chest_landed", true)
			finished.call())
