extends SceneTree
## Focused check of the canonical homepage's real Settings language action.

var failures := 0

func check(ok: bool, label: String) -> void:
	print("PASS " if ok else "FAIL ", label)
	if not ok:
		failures += 1

func _initialize() -> void:
	_run.call_deferred()

func _run() -> void:
	var main: Control = load("res://game/main.tscn").instantiate()
	main.args["profile"] = '{"coins":50585,"language":"en"}'
	root.add_child(main)
	# Block persistence without triggering the smoke runner during _ready.
	main.args["smoke"] = "1"
	await create_timer(0.8).timeout
	var home: Control = main.screen
	var face: TextureRect = home.get_node("Play/Face")
	var touch_size: Vector2 = home.get_node("Play").size
	var face_size := face.size
	check(face.texture.resource_path.ends_with("btn_home_island_play_en.png"), "initial English complete button")
	check(home.get_node_or_null("Play/PlayWord") == null, "no separately positioned word node")
	check(face.stretch_mode == TextureRect.STRETCH_KEEP_ASPECT_CENTERED, "native artwork aspect is retained")
	for locale in ["zh", "en"]:
		home.get_node("Settings").pressed.emit()
		await create_timer(0.25).timeout
		var language: Button = main.popups.find_child("Language", true, false)
		check(language != null, "real Settings language action exists")
		if language == null:
			break
		language.pressed.emit()
		await create_timer(0.25).timeout
		check(main.profile.language == locale, "Settings selected " + locale)
		check(main.screen == home, "same homepage survives switch to " + locale)
		check(home.get_node("Play/Face") == face, "same button face survives switch to " + locale)
		check(face.texture.resource_path.ends_with("btn_home_island_play_" + locale + ".png"), "complete button switches to " + locale)
		check(home.get_node("Play").size == touch_size and face.size == face_size, "button bounds unchanged in " + locale)
		main._clear_popups()
		await process_frame
	print("Focused Settings language check: %d failures" % failures)
	root.get_node("Sound").set_music_muted(true)
	root.get_node("Sound").set_muted(true)
	await create_timer(0.1).timeout
	main.queue_free()
	await process_frame
	quit.call_deferred(1 if failures else 0)
