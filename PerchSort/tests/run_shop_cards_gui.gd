extends SceneTree
## Real viewport/input evidence for fixed navigation and clipped, scrollable Shop cards.

var failures := 0
var view: Control
var evidence := ""

func _initialize() -> void:
	_run.call_deferred()

func check(ok: bool, label: String) -> void:
	print("PASS " if ok else "FAIL ", label)
	failures += 0 if ok else 1

func _run() -> void:
	for arg in OS.get_cmdline_user_args():
		if arg.begins_with("--shop-card-evidence="):
			evidence = arg.trim_prefix("--shop-card-evidence=")
	DirAccess.make_dir_recursive_absolute(evidence)
	var profile := Profile.new()
	profile.coins = 50000
	profile.highest_unlocked = Levels.COUNT
	for species in Board.SPECIES:
		profile.met[species] = true
	profile.inventory = PackedInt32Array([99, 2, 28, 11, 8, 36])
	view = load("res://game/shop_view.gd").new()
	view.instant = true
	view.setup(profile)
	root.add_child(view)
	await create_timer(0.5).timeout
	await capture("boosters_en")
	var tabs_rect: Rect2 = view.get_node("Tabs/Boosters").get_global_rect()
	await click(view.get_node("Tabs/Outfits"))
	check(view.tab == 1, "real input opens outfits")
	await capture("outfits_top_en")
	await scroll_to_end()
	check(view._scroll.scroll_vertical > 0, "real wheel input scrolls outfits")
	check(view.get_node("Tabs/Boosters").get_global_rect() == tabs_rect, "outfit scroll leaves tab fixed")
	await capture("outfits_bottom_en")
	await click(view.get_node("Tabs/Nests"))
	check(view.tab == 2 and view._scroll.scroll_vertical == 0, "nests open from top")
	await capture("nests_top_en")
	await scroll_to_end()
	check(view._scroll.scroll_vertical > 0, "real wheel input scrolls nests")
	check(view.get_node("Tabs/Boosters").get_global_rect() == tabs_rect, "nest scroll leaves tab fixed")
	await capture("nests_bottom_en")
	await click(view.get_node("Tabs/Boosters"))
	check(view.tab == 0 and view._scroll.scroll_vertical == 0, "boosters restore top and remain reachable after scroll")
	await capture("boosters_return_en")
	UiText.set_language("zh")
	await create_timer(0.3).timeout
	await capture("boosters_zh")
	await click(view.get_node("Tabs/Outfits"))
	await capture("outfits_top_zh")
	await click(view.get_node("Tabs/Nests"))
	await capture("nests_top_zh")
	check(view._modal == null, "scroll and category navigation never trigger purchase")
	# Stock remains readable without a notification badge, including zero and capped large values.
	for language in ["en", "zh"]:
		UiText.set_language(language)
		view._select_tab(0)
		for count in [0, 5000]:
			profile.inventory = PackedInt32Array([count, count, count, count, count, count])
			view._refresh()
			await create_timer(0.2).timeout
			var stock: Control = view._grid.get_child(0).get_node("Body/Stock")
			check(stock.get_node("Count").has_meta("value"), "inventory count uses painted numerals")
			check(stock.get_node("Count").get_meta("value") == str(mini(count, load("res://game/island.gd").BADGE_CAP)), "inventory count preserved")
			await capture("boosters_" + ("empty_" if count == 0 else "maximum_") + language)
	var closed := [false]
	view.closed.connect(func(): closed[0] = true)
	await click(view.get_node("Bar/Close"))
	check(closed[0], "unified close remains functional")
	print("shop GUI check: %d failures" % failures)
	view.queue_free()
	await process_frame
	quit(1 if failures else 0)

func click(control: Control) -> void:
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
	await create_timer(0.25).timeout

func scroll_to_end() -> void:
	var point: Vector2 = view._scroll.get_global_rect().get_center()
	var move := InputEventMouseMotion.new()
	move.position = point
	root.push_input(move, true)
	for i in 35:
		var wheel := InputEventMouseButton.new()
		wheel.position = point
		wheel.button_index = MOUSE_BUTTON_WHEEL_DOWN
		wheel.pressed = true
		root.push_input(wheel, true)
		var release := wheel.duplicate()
		release.pressed = false
		root.push_input(release, true)
		await process_frame
	await create_timer(0.2).timeout

func capture(name: String) -> void:
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png(evidence.path_join(name + ".png"))
