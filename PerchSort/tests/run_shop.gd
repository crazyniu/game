extends SceneTree
## Short shop check through real input; fake profile, no persistence.

var failures := 0
var main: Control
var shots: Array[Dictionary] = []
var evidence := ""

func _initialize() -> void:
	_run.call_deferred()

func check(ok: bool, label: String) -> void:
	print("PASS " if ok else "FAIL ", label)
	failures += 0 if ok else 1

func _run() -> void:
	for arg in OS.get_cmdline_user_args():
		if arg.begins_with("--shop-evidence="):
			evidence = arg.trim_prefix("--shop-evidence=")
	DirAccess.make_dir_recursive_absolute(evidence)
	main = load("res://game/main.tscn").instantiate()
	var fake := Profile.new()
	fake.coins = 50000
	fake.highest_unlocked = Levels.COUNT
	fake.inventory = PackedInt32Array([3, 2, 1, 1, 0, 2])
	fake.language = "zh"
	fake.outfits = {"outfit.sky.5": true, "outfit.berry.4": true}
	fake.equipped = {"sky": "outfit.sky.5", "berry": "outfit.berry.4"}
	for species in Board.SPECIES:
		fake.met[species] = true
	main.args["profile"] = JSON.stringify(fake.to_dict())
	root.add_child(main)
	main.args["smoke"] = "1"
	await create_timer(0.6).timeout
	await click(main.screen.get_node("Shop"))
	var shop: Control = main._overlay
	check(shop._grid.get_child_count() == 6, "six booster cards")
	await capture("shop_zh")
	var start := Time.get_ticks_usec()
	tap(shop.get_node("Tabs/Outfits"))
	var handler := (Time.get_ticks_usec() - start) / 1000.0
	await RenderingServer.frame_post_draw
	print("OUTFITS handler_ms=%.2f input_to_draw_ms=%.2f" % [handler, (Time.get_ticks_usec() - start) / 1000.0])
	await create_timer(0.6).timeout
	check(shop._title.painted_id == "lettering_tab_outfits", "outfit category and heading")
	await capture("shop_outfits_zh")
	var cached: Control = shop._grid
	await click(shop.get_node("Tabs/Boosters"))
	await click(shop.get_node("Tabs/Outfits"))
	check(shop._grid == cached, "outfit tab retains live content")
	await click(shop._grid.get_node("Kind_sunny").find_children("*", "Button", true, false)[0])
	var detail: Control = shop._outfit_page
	await capture("outfit_detail_zh")
	await click(detail._tiles.get_node("Tile_1/Locked"))
	await capture("outfit_purchase")
	var balance: int = main.profile.coins
	await click(detail.get_node("Popup/Content/Buy"))
	check(main.profile.owns_outfit("outfit.sunny.1") and main.profile.equipped["sunny"] == "outfit.sunny.1" and main.profile.coins == balance - 300, "outfit purchase and equip")
	UiText.set_language("en")
	await create_timer(0.4).timeout
	check(detail.get_node_or_null("Page/Back") != null, "detail survives language change")
	await capture("outfit_detail")
	UiText.set_language("zh")
	await create_timer(0.4).timeout
	await click(detail.get_node("Page/Back"))
	check(shop._outfit_page == null, "detail returns to shop")
	await click(shop.get_node("Tabs/Nests"))
	await capture("shop_nests_zh")
	shop._scroll.ensure_control_visible(shop._grid.get_child(7))
	await process_frame
	await click(shop._grid.get_child(7).get_node("Select"))
	await capture("nest_purchase")
	balance = main.profile.coins
	await click(shop._purchase_button)
	check(main.profile.owns_house("pinecone") and main.profile.coins == balance - 600, "nest purchase")
	shop._scroll.ensure_control_visible(shop._grid.get_child(7))
	await process_frame
	await click(shop._grid.get_child(7).get_node("Select"))
	check(main.profile.equipped_house == "pinecone", "nest equips")
	await click(shop.get_node("Tabs/Boosters"))
	await click(shop._grid.get_child(0).get_node("Select"))
	await click(shop._plus)
	await capture("purchase_quantity")
	balance = main.profile.coins
	await click(shop._purchase_button)
	check(main.profile.coins == balance - 60 and main.profile.inventory[0] == 5, "booster quantity and purchase")
	await click(shop.get_node("Bar/Close"))
	check(main._overlay == null, "shop closes to home")
	for shot in shots:
		shot.image.save_png(evidence.path_join(shot.name + ".png"))
	print("shop quick check: %d failures" % failures)
	main.queue_free()
	quit(1 if failures else 0)

func tap(control: Control) -> void:
	var point := control.get_global_rect().get_center()
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

func click(control: Control) -> void:
	tap(control)
	await create_timer(0.6).timeout

func capture(name: String) -> void:
	await RenderingServer.frame_post_draw
	shots.append({"name": name, "image": root.get_texture().get_image()})
