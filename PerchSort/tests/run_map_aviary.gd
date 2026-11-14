extends SceneTree
## Actual GUI-input checks of approved map/aviary presentation, using fake data only.

var failures := 0
var main: Control
var evidence := ""

func _initialize() -> void:
	_run.call_deferred()

func check(ok: bool, label: String) -> void:
	print("PASS " if ok else "FAIL ", label)
	failures += 0 if ok else 1

func _run() -> void:
	# Keep a full-resolution render target even when the cloud window manager fits the window.
	root.content_scale_size = Vector2i(1080, 1920)
	root.content_scale_mode = Window.CONTENT_SCALE_MODE_VIEWPORT
	root.content_scale_aspect = Window.CONTENT_SCALE_ASPECT_KEEP
	for arg in OS.get_cmdline_user_args():
		if arg.begins_with("--evidence="):
			evidence = arg.trim_prefix("--evidence=")
	if not evidence.is_empty():
		DirAccess.make_dir_recursive_absolute(evidence)
	main = load("res://game/main.tscn").instantiate()
	var fake := Profile.new()
	fake.coins = 1280
	fake.highest_unlocked = 8
	fake.language = "zh"
	fake.met = {"sunny": true, "sky": true, "berry": true, "mint": true}
	for level in range(1, 8):
		fake.stars[level] = 1 if level < 7 else 2
	main.args["profile"] = JSON.stringify(fake.to_dict())
	main.args["preview"] = "map_view:cards"
	root.add_child(main)
	main.args["smoke"] = "1" # Block persistence without invoking the smoke runner.
	await create_timer(0.5).timeout
	var map: Control = main.screen
	map.setup(fake)
	await create_timer(0.1).timeout
	check(map.find_children("Play", "Button", true, false).size() == 1, "exactly one shared Play")
	check(map._world == 1 and map._page == null, "initial forest selected without entry")
	await capture("map_first")
	var play_size: Vector2 = map._play_button.size
	await click_point(Vector2(1000, 800))
	check(map._world == 2 and map._page == null, "neighbour card selects without opening")
	check(map._play_button.disabled and map._play_button.size == play_size, "locked Play dims without resizing")
	await capture("map_locked")
	await click(map._play_button)
	check(map._page == null, "locked Play cannot enter")
	map.show_world(1, true)
	await click(map._play_button)
	check(map._page != null and not map._play_button.visible, "shared Play opens selected world and hides on trail")
	await capture("map_trail")
	var current: Control = map._page.get_node("Level 8")
	check(current.z_index == 0 and current.get_index() == map._page.get_child_count() - 1,
		"current marker is last only inside the trail, never above modal layers")
	main.show_level_card(8)
	await create_timer(0.25).timeout
	var card: Control = main.popups.get_child(0)
	check(main.popups.get_index() > map.get_index() and card.z_index == 0,
		"level card paints above the complete trail in normal scene order")
	await capture("map_level_card")
	await click(card.content.get_node("Close"))
	check(main.popups.get_child_count() == 0 and map._page != null,
		"level-card Close restores the same trail without leftover overlay")
	await click(map.get_node("Bar/Home"))
	check(map._page == null and map._world == 1 and map._play_button.visible, "Back restores same selected card and shared Play")
	fake.highest_unlocked = Levels.COUNT
	map.setup(fake)
	map.show_world(5, true)
	await capture("map_middle")
	map.show_world(Levels.WORLDS, true)
	check(map._scroll_x == map._max_scroll, "last forest centres at strip end")
	await capture("map_last")
	map.show_world(1, true)
	await drag(Vector2(850, 800), Vector2(150, 800))
	check(map._world == 2 and map._page == null, "swipe selects next forest without entry")
	await click(map._play_button)
	check(map._page != null and map._world == 2, "click after swipe opens chosen forest")
	# World switching on the page itself: no trip back to the cards.
	var previous: Button = map._page.get_node("WorldPrevious")
	var next: Button = map._page.get_node("WorldNext")
	check(previous.size == next.size and previous.size == map.WORLD_ARROW and not previous.disabled and not next.disabled,
		"previous and next world are one size, enabled mid-list")
	await click(next)
	check(map._world == 3 and map._page.get_node_or_null("Level %d" % (2 * Levels.PER_WORLD + 1)) != null, "Next opens the following world's levels")
	await capture("map_trail_next")
	await click(map._page.get_node("WorldPrevious"))
	await click(map._page.get_node("WorldPrevious"))
	check(map._world == 1 and map._page.get_node_or_null("Level 1") != null, "Previous steps back to the first world")
	check(map._page.get_node("WorldPrevious").disabled and map._page.get_node("WorldPrevious").size == map.WORLD_ARROW,
		"first world dims Previous without resizing it")
	await click(map._page.get_node("Bar/Home") if map._page.has_node("Bar/Home") else map.get_node("Bar/Home"))
	check(map._page == null and map._world == 1, "Back from a switched world lands on that world's card")
	map.open_world(Levels.WORLDS)
	check(map._page.get_node("WorldNext").disabled and not map._page.get_node("WorldPrevious").disabled, "last world dims Next only")
	map.close_world()
	await create_timer(0.1).timeout   # the closed page leaves the tree at frame end
	# Tapping an open card enters it at once.
	map.show_world(1, true)
	await click_point(Vector2(540, 800))
	check(map._page != null and map._world == 1, "tapping the centred open card opens its world")
	map.close_world()
	await create_timer(0.1).timeout
	await click_point(Vector2(1000, 800))
	check(map._page != null and map._world == 2, "tapping an open neighbour card opens that world")
	# A closed next world does not open.
	fake.highest_unlocked = 8
	map.setup(fake)
	map.open_world(1)
	await click(map._page.get_node("WorldNext"))
	check(map._world == 1 and map._page.get_node_or_null("Level %d" % (Levels.PER_WORLD + 1)) == null, "Next into a closed world stays put")
	fake.highest_unlocked = Levels.COUNT
	map.setup(fake)
	map.close_world()
	map.show_world(Levels.WORLDS, true)
	UiText.set_language("en")
	await create_timer(0.15).timeout
	check(map._world == Levels.WORLDS, "language refresh retains map selection")
	await capture("map_english")
	map.open_world(Levels.WORLDS)
	await create_timer(0.1).timeout
	UiText.set_language("zh")
	await create_timer(0.15).timeout
	check(map._page != null and map._world == Levels.WORLDS, "language refresh retains open world trail")
	map.close_world()
	UiText.set_language("zh")
	await create_timer(0.15).timeout
	var aviary: Control = load("res://game/aviary_view.gd").new()
	main.set_screen(aviary)
	aviary.setup(fake)
	await capture("aviary_list")
	var list_before := JSON.stringify(fake.to_dict())
	await click_point(Vector2(280, 1470))
	check(aviary._species.is_empty() and JSON.stringify(fake.to_dict()) == list_before, "unknown bird is non-destructive and does not open")
	var before := JSON.stringify(fake.to_dict())
	await click_point(Vector2(280, 410))
	check(aviary._species == "sunny", "met bird enters detail")
	check(JSON.stringify(fake.to_dict()) == before, "opening detail does not mutate profile")
	check(aviary._tiles.get_child_count() == Shop.OUTFIT_COUNT + 1, "six equal outfit choices retained")
	await capture("aviary_sunny")
	await click(aviary._tiles.get_node("Tile_1/Locked"))
	await capture("aviary_purchase")
	var balance: int = fake.coins
	await click(aviary.get_node("Popup/Content/Buy"))
	check(fake.owns_outfit("outfit.sunny.1") and fake.equipped.get("sunny") == "outfit.sunny.1", "existing outfit purchase equips")
	check(fake.coins == balance - Shop.item("outfit.sunny.1")["price"], "purchase preserves canonical price")
	await capture("aviary_equipped")
	var owned_before := JSON.stringify(fake.outfits)
	fake.coins = 0
	aviary._build_coins()
	await click(aviary._tiles.get_node("Tile_2/Locked"))
	var disabled_buy: Button = aviary.get_node("Popup/Content/Buy")
	check(disabled_buy.disabled, "unaffordable outfit action is disabled")
	await capture("aviary_insufficient")
	await click(disabled_buy)
	check(fake.coins == 0 and JSON.stringify(fake.outfits) == owned_before, "unaffordable action does not purchase")
	await click(aviary.get_node("Popup/Content/Close"))
	fake.coins = balance - Shop.item("outfit.sunny.1")["price"]
	await click(aviary.get_node("Page/Back"))
	check(aviary._species.is_empty() and aviary._scroll == 0.0, "detail Back restores list")
	aviary._set_scroll(560)
	var previous_scroll: float = aviary._scroll
	aviary._show_detail("mint")
	await click(aviary.get_node("Page/Back"))
	check(aviary._scroll == previous_scroll, "detail Back preserves list scroll")
	await capture("aviary_scrolled")
	var feather_bar: GDScript = load("res://game/feather_bar.gd")
	fake.coins = feather_bar.DISPLAY_MAX
	aviary._show_detail("hoot")
	await capture("aviary_maximum")
	aviary._show_detail("crow")
	check(aviary.get_node_or_null("Page/Outfits") == null, "crow keeps no outfit choices")
	await capture("aviary_crow")
	UiText.set_language("en")
	await create_timer(0.15).timeout
	aviary._show_detail("sunny")
	await capture("aviary_english")
	var hero: Node2D = aviary._hero
	var motion_scales := {}
	var motion_frames := {}
	var motion_start := Time.get_ticks_msec()
	for frame in 20:
		await create_timer(0.25).timeout
		motion_scales[str(hero._body.scale)] = true
		motion_frames[str(hero._sprite.texture.resource_path)] = true
		print("MOTION %.2f scale=%s texture=%s" % [(Time.get_ticks_msec() - motion_start) / 1000.0, hero._body.scale, hero._sprite.texture.resource_path])
		await capture("motion_%02d" % frame)
	check(motion_scales.size() > 1, "live hero breathes continuously")
	check(motion_frames.size() > 1, "live hero uses distinct drawn poses")
	var sound: Node = root.get_node("Sound")
	sound.set_music_muted(false)
	sound.music("bgm_menu")
	await create_timer(0.2).timeout
	check(sound._music.playing, "menu music player active")
	sound.set_music_muted(true)
	check(not sound._music.playing, "mute stops music player")
	sound.set_music_muted(false)
	check(sound._music.playing, "unmute restores menu music")
	# Drain the dummy audio mixer before teardown so a playing WAV is not held at exit.
	sound.set_music_muted(true)
	await create_timer(0.25).timeout
	sound._music.stream = null
	sound._old_music.stream = null
	print("map/aviary GUI checks: %d failures" % failures)
	main.queue_free()
	await process_frame
	await process_frame
	quit.call_deferred(1 if failures else 0)

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

func drag(from: Vector2, to: Vector2) -> void:
	var press := InputEventMouseButton.new()
	press.position = from
	press.button_index = MOUSE_BUTTON_LEFT
	press.pressed = true
	root.push_input(press, true)
	var motion := InputEventMouseMotion.new()
	motion.position = to
	motion.relative = to - from
	motion.button_mask = MOUSE_BUTTON_MASK_LEFT
	root.push_input(motion, true)
	var release := press.duplicate()
	release.position = to
	release.pressed = false
	root.push_input(release, true)
	await create_timer(0.45).timeout

func click(control: Control) -> void:
	await click_point(control.get_global_rect().get_center())

func capture(name: String) -> void:
	if evidence.is_empty():
		return
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png(evidence.path_join(name + ".png"))
