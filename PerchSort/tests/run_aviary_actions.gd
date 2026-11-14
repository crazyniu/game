extends SceneTree
## Canonical outfit action surfaces, labels, currency and state without persistent data.
var checks := 0
var failures := 0
var evidence := ""

func _initialize() -> void:
	run.call_deferred()

func check(ok: bool, label: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		push_error(label)

func run() -> void:
	var island: GDScript = load("res://game/island.gd")
	root.content_scale_size = Vector2i(1080, 1920)
	root.content_scale_mode = Window.CONTENT_SCALE_MODE_VIEWPORT
	root.content_scale_aspect = Window.CONTENT_SCALE_ASPECT_KEEP
	for arg in OS.get_cmdline_user_args():
		if arg.begins_with("--evidence="):
			evidence = arg.trim_prefix("--evidence=")
	if not evidence.is_empty():
		DirAccess.make_dir_recursive_absolute(evidence)
	var main: Control = load("res://game/main.tscn").instantiate()
	main.args["preview"] = "aviary_view:sunny"
	root.add_child(main)
	main.args["smoke"] = "1"
	main.args["capture"] = "1"
	await process_frame
	for language in ["zh", "en"]:
		UiText.set_language(language)
		await process_frame
		for species in ["sunny", "hoot", "ember"]:
			for state in ["default", "owned", "equipped"]:
				var p := Profile.new()
				p.language = language
				p.coins = 1280
				p.met = {species: true}
				var first := "outfit.%s.1" % species
				if state != "default":
					p.outfits[first] = true
				if state == "equipped":
					p.equipped[species] = first
				var before := JSON.stringify(p.to_dict())
				var view: Control = load("res://game/aviary_view.gd").new()
				main.set_screen(view)
				view.setup(p)
				view._show_detail(species)
				await process_frame
				for index in Shop.OUTFIT_COUNT + 1:
					var tile: Control = view._tiles.get_node("Tile_%d" % index)
					var owned: bool = index == 0 or (index == 1 and state != "default")
					var selected: bool = index == (1 if state == "equipped" else 0)
					var face: Control = tile.get_node("Status" if owned else "Price")
					check(face.get_rect() == view.SHOP_TILE_ACTION, "One action box %s/%s/%d" % [species, state, index])
					check(tile.get_node("Equip" if owned else "Locked").get_rect().encloses(face.get_rect()), "Whole action face is clickable")
					check(face.get_node("Face").texture == Art.tex("btn_plate_green"), "Shared accepted action skin")
					check(tile.has_node("Equipped") == selected, "Existing equipped check state")
					check(tile.has_node("Lock") == not owned, "Existing lock state")
					if owned:
						var label: TextureRect = face.get_node("Word_white_equipped" if selected else "Word_white_equip")
						check(label.texture != null and label.size.x > 0, "Explicit localized owned state")
						check(tile.get_node("Equip").disabled == selected, "Existing equipped lockout")
					else:
						check(face.has_node("FeatherPrice/Feather") and face.has_node("FeatherPrice/Digits"), "Currency plus canonical price")
						var price := str(Shop.item("outfit.%s.%d" % [species, index])["price"])
						var amount: Control = face.get_node("FeatherPrice/Digits")
						check(amount.get_child_count() == price.length(), "Exact price digit count")
						for digit in price.length():
							check(amount.get_child(digit).texture == Art.tex("digit_ui_" + price[digit]), "Exact canonical price digit")
						check(Rect2(Vector2.ZERO, face.size).encloses(amount.get_rect()), "Price remains within its face")
						check(not tile.get_node("Locked").disabled, "Purchasable tile still opens confirmation")
				check(JSON.stringify(p.to_dict()) == before, "Presentation does not mutate account data")
				await capture("%s_%s_%s" % [species, state, language])
				if language == "en" and species == "sunny" and state == "default":
					var price_face: Control = view._tiles.get_node("Tile_1/Price")
					await click_point(price_face.global_position + Vector2(price_face.size.x * 0.5, price_face.size.y - 2))
					check(view.has_node("Popup"), "Price face bottom edge opens existing confirmation")
					await capture("sunny_price_confirmation_en")
					await click_point(view.get_node("Popup/Content/Close").get_global_rect().get_center())
					check(not view.has_node("Popup"), "Purchase Close returns to the outfit grid")
					p.outfits[first] = true
					p.equipped[species] = first
					view._build_tiles()
					var status: Control = view._tiles.get_node("Tile_0/Status")
					await click_point(status.global_position + Vector2(status.size.x * 0.5, status.size.y - 2))
					check(not p.equipped.has(species), "Equip face bottom edge restores default outfit")
	print("aviary action checks: %d checks, %d failures" % [checks, failures])
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

func click_point(point: Vector2) -> void:
	var move := InputEventMouseMotion.new()
	move.position = point
	root.push_input(move, true)
	var press := InputEventMouseButton.new()
	press.position = point
	press.button_index = MOUSE_BUTTON_LEFT
	press.pressed = true
	root.push_input(press, true)
	var release := press.duplicate()
	release.pressed = false
	root.push_input(release, true)
	await create_timer(0.45).timeout
