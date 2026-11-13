extends SceneTree
## Visible painted ink checks; fake profiles and current page sources only.
var failures := 0
var checks := 0
var evidence := ""
var measurements: Array = []

func _initialize() -> void:
	run.call_deferred()

func check(ok: bool, label: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		push_error(label)

func near(actual: Vector2, expected: Vector2, label: String) -> void:
	check(actual.distance_to(expected) < 3.0, label + ": " + str(actual) + " / " + str(expected))

func centered(actual: Vector2, expected: Vector2, label: String) -> void:
	check(actual.distance_to(expected) < 0.05, label + ": " + str(actual) + " / " + str(expected))
	measurements.append({"label": label, "actual": [actual.x, actual.y], "expected": [expected.x, expected.y], "error": actual.distance_to(expected)})

func run() -> void:
	root.content_scale_size = Vector2i(1080, 1920)
	root.content_scale_mode = Window.CONTENT_SCALE_MODE_VIEWPORT
	root.content_scale_aspect = Window.CONTENT_SCALE_ASPECT_KEEP
	for arg in OS.get_cmdline_user_args():
		if arg.begins_with("--evidence="):
			evidence = arg.trim_prefix("--evidence=")
	if not evidence.is_empty():
		DirAccess.make_dir_recursive_absolute(evidence)
	var main: Control = load("res://game/main.tscn").instantiate()
	main.args["preview"] = "aviary_view:detail_sunny"
	root.add_child(main)
	main.args["capture"] = "1"
	main.args["smoke"] = "1"
	await process_frame
	var script: GDScript = load("res://game/aviary_view.gd")
	for language in ["en", "zh"]:
		UiText.set_language(language)
		await process_frame
		for species in Aviary.SPECIES:
			var id: String = species[0]
			var p := Profile.new()
			p.language = language
			p.coins = 1280
			p.met[id] = true
			var before := JSON.stringify(p.to_dict())
			var view: Control = script.new()
			view.instant = true
			main.set_screen(view)
			view.setup(p)
			view._show_detail(id)
			await process_frame
			var tag: String = id + "_" + language
			var title: Control = view._page.get_node("TitleGroup")
			centered(title.position + title.size * 0.5, Vector2(540, script.HEADER_CENTER_Y), tag + " title banner")
			var coins: Control = view._coins
			var coin_rect: Rect2 = script._sprite_ink(coins.get_child(0))
			check(absf(coins.position.y + coin_rect.get_center().y - script.HEADER_CENTER_Y) < 0.05, tag + " header coin Y")
			var back: Button = view._page.get_node("Back")
			check(absf(back.position.y + script._sprite_ink(back.get_node("Face")).get_center().y - script.HEADER_CENTER_Y) < 0.05, tag + " Back Y")
			var ability: TextureRect = view._page.get_node("AbilityWord")
			var plaque: TextureRect = view._page.get_node("AbilityStrip")
			centered(script._sprite_ink(ability).get_center(), script._sprite_ink(plaque).get_center(), tag + " ability")
			if id != "crow":
				var label: TextureRect = view._tiles.get_node("Tile_0/Status/Word_white_equipped")
				near(script._sprite_ink(label, true).get_center(), script.SHOP_TILE_ACTION.size * 0.5, tag + " equipped")
				check(label.material != null, tag + " white button word treatment")
				for index in range(1, 6):
					var face: Control = view._tiles.get_node("Tile_%d/Price" % index)
					var amount: Control = face.get_node("FeatherPrice/Digits")
					var number_rect: Rect2 = script._digits_ink(amount)
					var group: Rect2 = script._sprite_ink(face.get_node("FeatherPrice/Feather")).merge(Rect2(amount.position + number_rect.position, number_rect.size))
					near(group.get_center(), face.size * 0.5, tag + " price group " + str(index))
			check(JSON.stringify(p.to_dict()) == before, tag + " no profile mutation")
			await capture(tag)
			if id == "sunny":
				var other: String = "zh" if language == "en" else "en"
				UiText.set_language(other)
				await process_frame
				await process_frame
				check(view._species == id, "Locale rebuild retains selected species")
				var rebuilt: TextureRect = view._tiles.get_node("Tile_0/Status/Word_white_equipped")
				near(script._sprite_ink(rebuilt, true).get_center(), script.SHOP_TILE_ACTION.size * 0.5, "Locale rebuild centers visible ownership ink")
				check(rebuilt.material != null, "Locale rebuild keeps the button word treatment")
				check(JSON.stringify(p.to_dict()) == before, "Locale rebuild leaves fake profile unchanged")
				UiText.set_language(language)
				await process_frame
				await process_frame
	# Exercise first/last glyph asymmetry and every price length without changing economy data.
	var fixture: Control = script.new()
	root.add_child(fixture)
	for price in [1, 8, 9, 10, 11, 18, 80, 88, 99, 100, 101, 111, 808, 999, 1000, 8888, 10000, 99999]:
		var tile := Control.new()
		fixture.add_child(tile)
		fixture._tile_action(tile, false, false, price)
		var face: Control = tile.get_node("Price")
		var amount: Control = face.get_node("FeatherPrice/Digits")
		var rect: Rect2 = script._digits_ink(amount)
		var all: Rect2 = script._sprite_ink(face.get_node("FeatherPrice/Feather")).merge(Rect2(amount.position + rect.position, rect.size))
		near(all.get_center(), face.size * 0.5, "fixture price group " + str(price))
		check(Rect2(Vector2.ZERO, face.size).encloses(all), "fixture containment " + str(price))
		tile.queue_free()
	fixture.queue_free()
	var output := evidence if not evidence.is_empty() else ProjectSettings.globalize_path("res://../build/text-centering")
	DirAccess.make_dir_recursive_absolute(output)
	var file := FileAccess.open(output.path_join("visible-ink.json"), FileAccess.WRITE)
	file.store_string(JSON.stringify(measurements, "\t"))
	print("aviary text checks: %d checks, %d failures" % [checks, failures])
	var sound: Node = root.get_node("Sound")
	sound.set_music_muted(true)
	await create_timer(0.25).timeout
	sound._music.stream = null
	sound._old_music.stream = null
	main.queue_free()
	await process_frame
	await process_frame
	quit.call_deferred(1 if failures else 0)

func capture(label: String) -> void:
	if evidence.is_empty():
		return
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png(evidence.path_join(label + ".png"))
