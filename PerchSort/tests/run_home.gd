extends SceneTree
## Home-only regression: real input dispatch, navigation, painted localization and drawn idle frames.

var failures := 0
var main: Control
var evidence := ""
var observed := {}
var breathing := {}

func check(ok: bool, label: String) -> void:
	print("PASS " if ok else "FAIL ", label)
	if not ok:
		failures += 1

func _initialize() -> void:
	_run.call_deferred()

func _run() -> void:
	for arg in OS.get_cmdline_user_args():
		if arg.begins_with("--home-evidence="):
			evidence = arg.trim_prefix("--home-evidence=")
	if not evidence.is_empty():
		DirAccess.make_dir_recursive_absolute(evidence)
	main = load("res://game/main.tscn").instantiate()
	var fake_profile := Profile.new()
	for species in Board.SPECIES:
		fake_profile.met[species] = true
	var tutorial: GDScript = load("res://game/tutorial.gd")
	fake_profile.set_tutorial(tutorial.FLAG)
	for mechanic in tutorial.LESSONS:
		fake_profile.set_tutorial(tutorial.lesson_flag(mechanic))
	main.args["profile"] = JSON.stringify(fake_profile.to_dict())
	root.add_child(main)
	# The normal homepage has loaded. Block persistence before any navigation mutates the fake profile.
	main.args["smoke"] = "1"
	await create_timer(0.6).timeout
	check(main.screen.get_script().resource_path.ends_with("home_view.gd"), "canonical homepage opens")
	var home: Control = main.screen
	var buttons := ["DailyReward", "LuckyDraw", "Map", "Shop", "Aviary", "Chest", "Play", "Settings"]
	for name in buttons:
		check(home.get_node_or_null(name) is Button, "entry exists: " + name)
	check(home.get_node("DailyReward").size.is_equal_approx(home.get_node("LuckyDraw").size), "event siblings match")
	# The UI system explicitly permits an elongated Play pill; its height remains restrained.
	check(home.get_node("Play").size.y <= home.get_node("Map").size.y * 1.25 + 0.01, "primary height respects sibling limit")
	check(home.get_node("Play/Face").size.is_equal_approx(home.PLAY.size), "complete Play picture uses the accepted footprint")
	for name in ["Shop", "Aviary", "Chest"]:
		check(home.get_node("Map").size.is_equal_approx(home.get_node(name).size), "navigation size: " + name)
	for name in ["Map", "Shop", "Aviary", "Chest"]:
		check(home.get_node(name).size.is_equal_approx(home.ICON_ENTRY_SIZE), "navigation touch area retained: " + name)
		var face: TextureRect = home.get_node(name + "/Face")
		var icon: String = {"Map": "pic_map", "Shop": "pic_shop", "Aviary": "pic_aviary", "Chest": "pic_chest"}[name]
		var content: Rect2 = home.ICON_CONTENT[icon]
		var scale: Vector2 = face.size / face.texture.get_size()
		var ink := Rect2(face.position + content.position * scale, content.size * scale)
		check(is_equal_approx(maxf(ink.size.x, ink.size.y), home.NAV_ICON_SIDE), "navigation visible art fits canonical slot: " + name)
		check(ink.get_center().distance_to(home.ICON_ENTRY_SIZE * 0.5) < 0.01, "navigation visible art is centered: " + name)
	check(home._birds.size() == 4, "four live birds on the shared perch")
	for entry in home._birds:
		var bird: AnimatedSprite2D = entry.view
		check(bird.sprite_frames.get_frame_count("default") == 8, "eight drawn frames: " + str(entry.pivot.name))
		check(not bird.flip_h and not bird.flip_v, "no mirrored bird: " + str(entry.pivot.name))
		observed[entry.pivot.name] = {}
		breathing[entry.pivot.name] = {}
	for i in 64:
		for entry in home._birds:
			observed[entry.pivot.name][entry.view.frame] = true
			breathing[entry.pivot.name][snappedf(entry.pivot.scale.y, 0.001)] = true
		if i % 4 == 0:
			await _capture("idle_%02d" % (i / 4))
		await create_timer(0.1).timeout
	for name in observed:
		check(observed[name].size() >= 6, "drawn idle poses observed: %s %s" % [name, observed[name].keys()])
		check(breathing[name].size() >= 8, "continuous breathing: " + str(name))
	var sound := root.get_node("Sound")
	check(sound._music_name == "bgm_menu" and sound._music.playing, "homepage menu music plays")
	sound.set_music_muted(true)
	check(not sound._music.playing, "music mute stops playback")
	sound.set_music_muted(false)
	check(sound._music.playing, "music unmute resumes playback")
	await _capture("home_en")
	var coin_limit: int = home.COIN_DISPLAY_MAX
	for balance in [0, 2480, coin_limit, coin_limit + 1, 1234567]:
		main.profile.coins = balance
		home.setup(main.profile)
		var display := str(home.get_node("Coins").get_meta("display_value"))
		check(display == str(mini(balance, coin_limit)), "coin display cap: " + str(balance))
		check(not display.contains(",") and display.length() <= 5, "coin display has at most five digits and no comma")
		check(main.profile.coins == balance, "coin presentation preserves actual balance: " + str(balance))
	main.profile.coins = coin_limit
	home.setup(main.profile)
	UiText.set_language("zh")
	await process_frame
	check(home.get_node("Play/Face").texture.resource_path.ends_with("btn_home_island_play_zh.png"), "Chinese complete Play refreshes in place")
	await _capture("home_zh_max")
	UiText.set_language("en")
	await process_frame
	check(home.get_node("Play/Face").texture.resource_path.ends_with("btn_home_island_play_en.png"), "English complete Play refreshes in place")
	check(home.get_node("Play").size.is_equal_approx(home.PLAY.size), "localized Play keeps its footprint")
	for name in buttons:
		main._open_home()
		await create_timer(0.5).timeout
		home = main.screen
		var button: Button = home.get_node(name)
		var center := button.get_global_rect().get_center()
		var motion := InputEventMouseMotion.new()
		motion.position = center
		root.push_input(motion, true)
		var press := InputEventMouseButton.new()
		press.position = center
		press.button_index = MOUSE_BUTTON_LEFT
		press.pressed = true
		var original_size := button.size
		root.push_input(press, true)
		check(button.size.is_equal_approx(original_size), "pressed size unchanged: " + name)
		var release := press.duplicate()
		release.pressed = false
		root.push_input(release, true)
		await create_timer(0.6).timeout
		match name:
			"Map": check(main.screen.get_script().resource_path.ends_with("map_view.gd"), "click opens Map")
			"Shop": check(main._overlay != null and main._overlay.get_script().resource_path.ends_with("shop_view.gd"), "click opens Shop")
			"Aviary": check(main._overlay != null and main._overlay.get_script().resource_path.ends_with("aviary_view.gd"), "click opens Aviary")
			"Play":
				check(main.screen.get_script().resource_path.ends_with("board_view.gd"), "click opens board: " + name)
				for tick in 50:
					if not main.screen.busy:
						break
					await create_timer(0.1).timeout
				check(not main.screen.busy, "board entrance finishes: " + name)
			"DailyReward": check(main.popups.find_child("DailyReward", true, false) != null, "click opens Daily Reward page")
			"LuckyDraw": check(main.popups.find_child("DailyDraw", true, false) != null, "click opens Lucky Draw popup")
			"Chest": check(main.popups.find_child("StarChest", true, false) != null, "click opens Chest popup")
			"Settings": check(main.popups.find_child("Word_settings", true, false) != null, "click opens Settings popup")
		await _capture("entry_" + name.to_lower())
	main._open_home()
	await create_timer(0.5).timeout
	check(main.screen.get_script().resource_path.ends_with("home_view.gd"), "home survives repeated navigation")
	sound.set_music_muted(true)
	await create_timer(0.1).timeout
	print("home checks: %d failures" % failures)
	main.queue_free()
	await process_frame
	quit(1 if failures else 0)

func _capture(name: String) -> void:
	if evidence.is_empty():
		return
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png(evidence.path_join(name + ".png"))
	print("CAPTURE %s elapsed_ms=%d" % [name, Time.get_ticks_msec()])
