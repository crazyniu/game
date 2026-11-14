class_name ChestOpening
extends RefCounted
## Prepare art/audio on page entry, then blend authored poses on rendered frames.
const FRAME_INK := [Rect2(76, 169, 480, 392), Rect2(684, 152, 500, 409), Rect2(73, 716, 487, 446), Rect2(694, 685, 495, 477)]
const STEP := 0.18
const HOLD := 0.32
const ANTICIPATION := 0.16
const DURATION := ANTICIPATION + STEP * 4 + HOLD

static func prepare(dialog: Modal, footline: float, width: float) -> void:
	var hero := Control.new()
	hero.name = "ChestOpeningHero"
	hero.mouse_filter = Control.MOUSE_FILTER_IGNORE
	dialog.content.add_child(hero)
	var atlas := Art.tex("chest_opening_frames")
	for ink in FRAME_INK:
		var pose := TextureRect.new()
		pose.mouse_filter = Control.MOUSE_FILTER_IGNORE
		pose.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
		pose.stretch_mode = TextureRect.STRETCH_SCALE
		var frame := AtlasTexture.new()
		frame.atlas = atlas
		frame.region = ink
		pose.texture = frame
		pose.size = Vector2(width, ink.size.y * width / ink.size.x)
		pose.position = Vector2(540.0 - width * 0.5, footline - pose.size.y)
		pose.modulate.a = 0.0
		hero.add_child(pose)
	Sound.prepare_effects(["crate_open", "star_1"])

static func play(dialog: Modal, finished: Callable) -> void:
	var original: Control = dialog.content.get_node("ChestHero")
	original.pivot_offset = original.size * Vector2(0.5, 1.0)
	var hero: Control = dialog.content.get_node("ChestOpeningHero")
	var motion := dialog.create_tween()
	motion.tween_property(original, "rotation_degrees", -2.5, ANTICIPATION * 0.5).set_trans(Tween.TRANS_SINE)
	motion.tween_property(original, "rotation_degrees", 0.0, ANTICIPATION * 0.5).set_trans(Tween.TRANS_SINE)
	for i in FRAME_INK.size():
		var previous: Control = original if i == 0 else hero.get_child(i - 1)
		var pose: Control = hero.get_child(i)
		motion.tween_callback(func():
			if dialog._closing:
				return
			hero.set_meta("pose", i)
			if i == 0:
				Sound.play("crate_open", 1.0, -4.0)
			elif i == FRAME_INK.size() - 1:
				Sound.play("star_1", 1.1, -8.0))
		# Incoming ink replaces the preceding pose continuously; no timer-held hard cuts.
		motion.tween_property(pose, "modulate:a", 1.0, STEP).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
		motion.parallel().tween_property(previous, "modulate:a", 0.0, STEP).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	motion.tween_interval(HOLD)
	motion.tween_callback(func():
		if not dialog._closing:
			finished.call())
