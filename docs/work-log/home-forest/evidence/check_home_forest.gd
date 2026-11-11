extends SceneTree

var failures := 0
var checks := 0

func check(ok: bool, label: String) -> void:
	checks += 1
	print("PASS " if ok else "FAIL ", label)
	if not ok:
		failures += 1

func _initialize() -> void:
	_run.call_deferred()

func _run() -> void:
	var main = load("res://game/main.tscn").instantiate()
	main.args["preview"] = "home_view:home"
	root.add_child(main)
	await process_frame
	await process_frame
	var home = main.screen
	var backdrop_name: String = OS.get_environment("HOME_EXPECTED_BACKGROUND")
	if backdrop_name.is_empty():
		backdrop_name = "background_home_jungle"
	check(home != null, "canonical fake-data homepage opens")
	var backdrop: TextureRect = home.get_child(0)
	check(backdrop.texture.resource_path == "res://art/island/" + backdrop_name + ".png", "approved existing forest texture loaded")
	check(backdrop.position == Vector2.ZERO and backdrop.size == home.CANVAS_SIZE, "background retains full-canvas geometry")
	check(home.get_node("Title").texture.resource_path.ends_with("panel_home_perch_title.png"), "accepted title/perch texture retained")
	check(home._birds.size() == 4, "four existing animated birds retained")
	for entry in home._birds:
		var bird: AnimatedSprite2D = entry.view
		check(bird.sprite_frames.get_frame_count("default") == 8, "eight existing poses: " + str(entry.pivot.name))
		check(not bird.flip_h and not bird.flip_v, "unmirrored original bird: " + str(entry.pivot.name))
	var expected := {
		"Settings": [Vector2(896, 24), "settings_requested"],
		"Daily": [Vector2(880, 840), "challenge_requested"],
		"LuckyDraw": [Vector2(880, 1060), "lucky_draw_requested"],
		"Map": [Vector2(94, 1680), "map_requested"],
		"Shop": [Vector2(338, 1680), "shop_requested"],
		"Aviary": [Vector2(582, 1680), "aviary_requested"],
		"Chest": [Vector2(826, 1680), "chest_requested"],
	}
	for name in expected:
		var button: Button = home.get_node(name)
		check(button.position == expected[name][0] and button.size == home.ICON_ENTRY_SIZE, "unchanged hit geometry: " + name)
		var observed := {"count": 0}
		home.connect(expected[name][1], func(): observed["count"] += 1)
		button.pressed.emit()
		check(observed["count"] == 1, "button signal retained: " + name)
	var play: Button = home.get_node("Play")
	check(Rect2(play.position, play.size) == home.PLAY, "Play geometry retained")
	var level := {"value": 0}
	home.play_requested.connect(func(value: int): level["value"] = value)
	play.pressed.emit()
	check(level["value"] == 12, "Play retains fake-profile next-level signal")
	for language in ["en", "zh", "en"]:
		UiText.set_language(language)
		await process_frame
		check(home.get_node("Play/Face").texture.resource_path.ends_with("btn_home_island_play_" + language + ".png"), "existing complete Play locale texture: " + language)
		check(Rect2(play.position, play.size) == home.PLAY, "locale preserves Play geometry: " + language)
	for balance in [0, 99999, 1234567]:
		home.profile.coins = balance
		home.setup(home.profile)
		check(home.get_child(0).texture.resource_path.ends_with(backdrop_name + ".png"), "rebuild retains forest: " + str(balance))
		check(home.get_node("Coins").get_meta("display_value") == str(mini(balance, 99999)), "existing balance display cap: " + str(balance))
		check(home.profile.coins == balance, "display preserves profile value: " + str(balance))
	root.get_node("Sound").set_muted(true)
	root.get_node("Sound").set_music_muted(true)
	main.queue_free()
	await process_frame
	await process_frame
	print("home forest checks: %d checks, %d failures" % [checks, failures])
	quit(1 if failures else 0)
