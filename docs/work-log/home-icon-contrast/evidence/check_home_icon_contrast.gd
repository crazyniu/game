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
	var ui = load("res://game/ui.gd")
	check(home.get_node("Settings").get_node_or_null("Backing") == null, "Settings has no new backing")
	check(home.get_child(0).texture.resource_path.ends_with("background_home_jungle.png"), "approved background retained")
	check(home.get_node("Title").texture.resource_path.ends_with("panel_home_perch_title.png"), "accepted title and perch retained")
	check(home._birds.size() == 4, "all four original birds retained")
	var expected := {
		"Daily": [Vector2(880, 840), "pic_daily", "challenge_requested", home.EVENT_ICON_SIDE, home.EVENT_BACKING_SIDE],
		"LuckyDraw": [Vector2(880, 1060), "icon_home_draw", "lucky_draw_requested", home.EVENT_ICON_SIDE, home.EVENT_BACKING_SIDE],
		"Map": [Vector2(94, 1680), "pic_map", "map_requested", home.NAV_ICON_SIDE, home.NAV_BACKING_SIDE],
		"Shop": [Vector2(338, 1680), "pic_shop", "shop_requested", home.NAV_ICON_SIDE, home.NAV_BACKING_SIDE],
		"Aviary": [Vector2(582, 1680), "pic_aviary", "aviary_requested", home.NAV_ICON_SIDE, home.NAV_BACKING_SIDE],
		"Chest": [Vector2(826, 1680), "pic_chest", "chest_requested", home.NAV_ICON_SIDE, home.NAV_BACKING_SIDE],
	}
	for n in expected:
		var entry: Array = expected[n]
		var button: Button = home.get_node(n)
		var backing: TextureRect = button.get_node("Backing")
		var face: TextureRect = button.get_node("Face")
		var rect := Rect2(button.position, button.size)
		var face_rect := Rect2(face.position, face.size)
		var backing_rect := Rect2(backing.position, backing.size)
		check(button.position == entry[0] and button.size == home.ICON_ENTRY_SIZE, "unchanged hit area: " + n)
		check(face.texture == Art.tex(entry[1]), "original icon texture: " + n)
		var content: Rect2 = home.ICON_CONTENT[entry[1]]
		var scale: float = entry[3] / maxf(content.size.x, content.size.y)
		print("Icon geometry delta %s: position=%.9f px, size=%.9f px" % [n, face.position.distance_to(home.ICON_ENTRY_SIZE * 0.5 - content.get_center() * scale), face.size.distance_to(Art.tex(entry[1]).get_size() * scale)])
		check(face.size.distance_to(Art.tex(entry[1]).get_size() * scale) < 0.0001 and face.position.distance_to(home.ICON_ENTRY_SIZE * 0.5 - content.get_center() * scale) < 0.0001, "unchanged icon size and placement: " + n)
		check(backing.texture.resource_path.ends_with("btn_home_icon_frame.png"), "independent frame loaded: " + n)
		check(backing.size == Vector2.ONE * entry[4] and backing.position == (home.ICON_ENTRY_SIZE - backing.size) * 0.5, "backing centered with retained padding: " + n)
		check(is_equal_approx(backing.modulate.a, home.BACKING_OPACITY), "backing opacity: " + n)
		check(backing.mouse_filter == Control.MOUSE_FILTER_IGNORE and backing.get_index() < face.get_index(), "backing behind icon and ignores input: " + n)
		button.button_down.emit()
		check(backing.self_modulate == ui.PRESSED_TINT and face.self_modulate == ui.PRESSED_TINT, "press tint retains icon and backing family: " + n)
		check(Rect2(button.position, button.size) == rect and Rect2(face.position, face.size) == face_rect and Rect2(backing.position, backing.size) == backing_rect, "press never resizes: " + n)
		button.button_up.emit()
		check(backing.self_modulate == Color.WHITE and face.self_modulate == Color.WHITE, "release restores tint: " + n)
		button.disabled = true
		check(Rect2(button.position, button.size) == rect and Rect2(face.position, face.size) == face_rect and Rect2(backing.position, backing.size) == backing_rect, "disabled geometry unchanged: " + n)
		button.disabled = false
		var observed := {"count": 0}
		home.connect(entry[2], func(): observed["count"] += 1)
		button.pressed.emit()
		button.pressed.emit()
		check(observed["count"] == 2, "repeated signal retained: " + n)
	for language in ["en", "zh", "en"]:
		UiText.set_language(language)
		await process_frame
		check(home.get_node("Play/Face").texture.resource_path.ends_with("btn_home_island_play_" + language + ".png"), "original Play language retained: " + language)
		for balance in [0, home.COIN_DISPLAY_MAX, home.COIN_DISPLAY_MAX + 1]:
			home.profile.coins = balance
			home.setup(home.profile)
			var count := 0
			for child in home.get_children():
				if child.get_node_or_null("Backing") != null:
					count += 1
			check(count == expected.size(), "exactly six backings after rebuild: %s/%d" % [language, balance])
			check(home.get_node("Settings").get_node_or_null("Backing") == null, "Settings remains untouched after rebuild")
	root.get_node("Sound").set_muted(true)
	root.get_node("Sound").set_music_muted(true)
	main.queue_free()
	await process_frame
	await process_frame
	print("home icon contrast checks: %d checks, %d failures" % [checks, failures])
	quit(1 if failures else 0)
