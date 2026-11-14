class_name ChestOpening
extends RefCounted
## Distinct painted hinge poses, grounded to the same footline before the reward is committed.
const FRAME_INK := [Rect2(76, 169, 480, 392), Rect2(684, 152, 500, 409), Rect2(73, 716, 487, 446), Rect2(694, 685, 495, 477)]
const STEP := 0.18
const HOLD := 0.32
const ANTICIPATION := 0.16
const DURATION := ANTICIPATION + STEP * 4 + HOLD

static func play(dialog: Modal, footline: float, width: float, finished: Callable) -> void:
	var original: Control = dialog.content.get_node("ChestHero")
	original.pivot_offset = original.size * Vector2(0.5, 1.0)
	var hero := TextureRect.new()
	hero.name = "ChestOpeningHero"
	hero.mouse_filter = Control.MOUSE_FILTER_IGNORE
	hero.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	hero.stretch_mode = TextureRect.STRETCH_SCALE
	dialog.content.add_child(hero)
	var motion := dialog.create_tween()
	motion.tween_property(original, "rotation_degrees", -2.5, ANTICIPATION * 0.5).set_trans(Tween.TRANS_SINE)
	motion.tween_property(original, "rotation_degrees", 0.0, ANTICIPATION * 0.5).set_trans(Tween.TRANS_SINE)
	for i in FRAME_INK.size():
		motion.tween_callback(func():
			if dialog._closing:
				return
			original.hide()
			if i == 0:
				Sound.play("crate_open", 1.0, -4.0)
			elif i == FRAME_INK.size() - 1:
				Sound.play("star_1", 1.1, -8.0)
			var ink: Rect2 = FRAME_INK[i]
			var frame := AtlasTexture.new()
			frame.atlas = Art.tex("chest_opening_frames")
			frame.region = ink
			hero.texture = frame
			hero.size = Vector2(width, ink.size.y * width / ink.size.x)
			hero.position = Vector2(540.0 - width * 0.5, footline - hero.size.y)
			hero.pivot_offset = Vector2(hero.size.x * 0.5, hero.size.y)
			hero.scale = Vector2(1.015, 0.985)
			dialog.create_tween().tween_property(hero, "scale", Vector2.ONE, STEP).set_trans(Tween.TRANS_SINE)
			hero.set_meta("pose", i))
		motion.tween_interval(STEP)
	motion.tween_interval(HOLD)
	motion.tween_callback(func():
		if not dialog._closing:
			finished.call())
