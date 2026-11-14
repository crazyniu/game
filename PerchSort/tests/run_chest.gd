extends SceneTree
## Chest regression through the canonical view and real input, with an isolated fake profile.

var failures := 0
var main: Control
var evidence := ""

func _initialize() -> void:
	root.content_scale_size = Vector2i(1080, 1920)
	root.content_scale_mode = Window.CONTENT_SCALE_MODE_VIEWPORT
	_run.call_deferred()

func check(ok: bool, label: String) -> void:
	print("PASS " if ok else "FAIL ", label)
	failures += 0 if ok else 1

func _run() -> void:
	var UI = load("res://game/ui.gd")
	var ChestLid = load("res://game/chest_lid.gd")
	var ChestOpening = load("res://game/chest_opening.gd")
	var Sound = root.get_node("Sound")
	for arg in OS.get_cmdline_user_args():
		if arg.begins_with("--chest-evidence="):
			evidence = arg.trim_prefix("--chest-evidence=")
	if not evidence.is_empty():
		DirAccess.make_dir_recursive_absolute(evidence)
	main = load("res://game/main.tscn").instantiate()
	var profile := Profile.new()
	profile.coins = 1280
	profile.language = "zh"
	var tutorial: GDScript = load("res://game/tutorial.gd")
	profile.set_tutorial(tutorial.FLAG)
	for mechanic in tutorial.LESSONS:
		profile.set_tutorial(tutorial.lesson_flag(mechanic))
	main.args["profile"] = JSON.stringify(profile.to_dict())
	root.add_child(main)
	main.args["smoke"] = "1"
	await create_timer(0.6).timeout
	var sound: Node = root.get_node("Sound")
	sound.set_music_muted(false)
	check(sound._music_name == "bgm_menu" and sound._music.playing, "menu music is preserved")
	await capture("home_lettering_reference")
	for stars in [0, 1, 18, Progress.CHEST_STARS - 1, Progress.CHEST_STARS, Progress.CHEST_STARS * 3 + 2]:
		_set_stars(stars)
		main.show_chest()
		await create_timer(0.35).timeout
		var page: Control = main.popups.get_child(-1)
		check(page.get_node("Content/ChestHero").get_script() == ChestLid, "continuous two-sided lid is prepared before click")
		check(Sound._cache.has("crate_open") and Sound._cache.has("star_1"), "opening sounds are decoded before click")
		var open: Button = page.get_node("Content/OpenChest")
		check(open.disabled == (stars < Progress.CHEST_STARS), "Open availability at %d stars" % stars)
		check(open.size == page.get_node("Content/Play").size, "Open and Play keep equal sizes at %d stars" % stars)
		# Actions wear the shop's green plate with a painted white word (ButtonKit), at ButtonKit.PRIMARY.
		check(open.has_node("Kit/Face") and open.get_node("Kit").size == Vector2(430, 162), "Open wears the shop's primary plate at %d stars" % stars)
		var word := open.get_node("Kit").get_child(1) as TextureRect
		check(word.painted_id == "white_open", "Open uses the shop's painted white word")
		check(not page.has_node("Content/BadgeGreen"), "no floating ready badge")
		check(page.get_node("Content/ReadyCount").get_meta("value") == str(Progress.chests_ready(main.profile)), "plain available count")
		check(page.get_node("Content/ReadyCount").get_rect().end.y < main.Popups.CHEST_HERO.position.y, "quantity above chest")
		check(is_equal_approx(page.get_node("Content/ChestHero").angle, ChestLid.CLOSED_ANGLE), "chest enters closed")
		var chest: Control = page.get_node("Content/ChestHero")
		var feet := chest.get_transform() * Vector2(614.5, 1090.0)
		check(feet.distance_to(Vector2(540.0, main.Popups.CHEST_HERO.end.y)) < 0.1, "closed chest feet stay centered despite pivot/breathing")
		check(page.has_node("background_home_jungle"), "approved jungle background")
		check(not page.has_node("Content/Panel"), "no enclosing chest card")
		var progress: Control = page.get_node("Content/Progress")
		check(progress.get_node("Number").get_meta("value") == "%d/%d" % [mini(stars, Progress.CHEST_STARS), Progress.CHEST_STARS], "progress uses the production threshold")
		check(progress.get_node("Track").texture.atlas.resource_path.ends_with("panel_chest_progress.png"), "progress uses the green rail")
		var digits: Control = progress.get_node("Number")
		check(is_equal_approx(digits.get_rect().get_center().x, progress.size.x * 0.5), "current/total is centered in the approved progress group")
		for glyph in digits.get_children():
			check(glyph.texture.resource_path.contains("icon_home_island_digit_") or glyph.texture.resource_path.ends_with("label_chest_slash.png"), "progress uses Home digits and matching separator")
		if stars > 0:
			var track := progress.get_node("Track") as Control
			var clip := progress.get_node("Fill") as Control
			var track_end := track.position + track.size * track.scale
			var inset: float = main.Popups.CHEST_PROGRESS_INSET
			check(clip.position.x >= track.position.x + inset and clip.get_rect().end.x <= track_end.x - inset + 0.01,
				"green fill stays inside green end caps at %d stars" % stars)
			check(clip.get_child(0) is NinePatchRect, "fill retains fixed rounded end caps")
		await capture("chest_%d_stars" % stars)
		var before := JSON.stringify(main.profile.to_dict())
		if open.disabled:
			await click(open)
			check(is_instance_valid(page) and not page._closing, "disabled Open ignores input")
		await click(page.get_node("Content/Close"))
		check(main.popups.get_child_count() == 0, "Close dismisses chest")
		check(JSON.stringify(main.profile.to_dict()) == before, "Close preserves all profile values")
	# Cancel during opening and destroy the page during opening: neither commits a reward.
	for destroy in [false, true]:
		Sound.muted = not destroy
		Sound._last_play.erase("crate_open")
		_set_stars(Progress.CHEST_STARS)
		var unchanged := JSON.stringify(main.profile.to_dict())
		main.show_chest()
		await create_timer(0.35).timeout
		var canceled: Control = main.popups.get_child(-1)
		tap(canceled.get_node("Content/OpenChest"))
		await create_timer(0.2).timeout
		check(Sound._last_play.has("crate_open") == destroy, "opening sound respects effects mute")
		if destroy:
			canceled.queue_free()
		else:
			tap(canceled.get_node("Content/Close"))
		await create_timer(ChestOpening.DURATION + 0.3).timeout
		check(JSON.stringify(main.profile.to_dict()) == unchanged, "canceled opening preserves claim and profile")
		check(Progress.chests_ready(main.profile) == 1, "canceled chest remains available")
	Sound.muted = true
	_set_stars(Progress.CHEST_STARS * 3 + 2)
	main.show_chest()
	await create_timer(0.35).timeout
	var page: Control = main.popups.get_child(-1)
	var hero: Control = page.get_node("Content/ChestHero")
	var original_scale := hero.scale
	await capture("motion_start")
	await create_timer(0.65).timeout
	await capture("motion_middle")
	check(not hero.scale.is_equal_approx(original_scale), "chest has live prop breathing")
	var before_coins: int = main.profile.coins
	var before_inventory := 0
	for count in main.profile.inventory:
		before_inventory += count
	check(page.get_node("Content/ChestHero").get_script() == ChestLid, "continuous two-sided lid is prepared before click")
	check(Sound._cache.has("crate_open") and Sound._cache.has("star_1"), "opening sounds are decoded before click")
	var open: Button = page.get_node("Content/OpenChest")
	tap(open)
	tap(open)
	await create_timer(0.25).timeout
	check(main.profile.chests_opened == 0, "no reward before opening finishes")
	var angle: float = page.get_node("Content/ChestHero").angle
	check(angle > ChestLid.CLOSED_ANGLE and angle < ChestLid.OPEN_ANGLE, "lid moves through real intermediate geometry, not image opacity")
	check(page.get_node("Content/ChestHero").modulate.a == 1.0, "moving lid stays opaque")
	# Continue tapping through the exact old-page/new-page boundary.
	var deadline := Time.get_ticks_msec() + int((ChestOpening.DURATION + 1.0) * 1000)
	while not main.popups.get_child(-1).content.has_node("ConfirmRewards") and Time.get_ticks_msec() < deadline:
		tap(open)
		await process_frame
	var fresh: Control = main.popups.get_child(-1)
	check(fresh.content.has_node("ConfirmRewards"), "reward becomes visible after continuous Open taps")
	for ignored in range(2):
		tap(fresh.get_node("Content/ConfirmRewards"))
		await create_timer(0.05).timeout
	check(not fresh._closing, "boundary tap burst cannot dismiss the just-shown reward")
	await create_timer(0.25).timeout
	check(main.profile.chests_opened == 1, "repeated input opens exactly one chest")
	check(main.profile.coins == before_coins + Progress.CHEST_COINS, "production coin reward preserved")
	var inventory := 0
	for count in main.profile.inventory:
		inventory += count
	check(inventory == before_inventory + Progress.CHEST_BOOSTERS, "production booster count preserved")
	check(Progress.chests_ready(main.profile) == 2, "remaining ready chests preserved")
	page = main.popups.get_child(-1)
	check(page.has_node("Content/ConfirmRewards"), "open shows reward result")
	var landed: Control = page.get_node("Content/ChestHero")
	var reward_feet := landed.get_transform() * Vector2(614.5, 1090.0)
	check(reward_feet.distance_to(Vector2(540.0, main.Popups.CHEST_RESULT_HERO.end.y)) < 0.1, "reward chest lands at the same centered final anchor")
	check(page.get_node("Content/ConfirmRewards/Kit").get_child(1).painted_id == "white_action_ok", "reward confirmation uses the shop's painted white word")
	await capture("chest_actual_rewards")
	await click(page.get_node("Content/ConfirmRewards"))
	check(main.popups.get_child_count() == 0, "Confirm dismisses result")
	main.show_chest()
	await create_timer(0.3).timeout
	page = main.popups.get_child(-1)
	check(not page.get_node("Content/OpenChest").disabled, "chest can reopen with remaining rewards")
	await click(page.get_node("Content/Close"))
	for picture in main.Popups.CHEST_REWARD_INK:
		var path: String = "res://art/" + ("island/" if picture.begins_with("island_icon_") else "") + picture + ".png"
		var source := Image.load_from_file(path)
		check(Rect2(source.get_used_rect()) == main.Popups.CHEST_REWARD_INK[picture], "reward ink metadata matches source pixels: " + picture)
		UI._used.erase(picture)
	for prizes in [[0, 0, 2], [0, 1, 2], []]:
		page = main.Popups.rewards(main.popups, "chest.title", Progress.CHEST_COINS, prizes, func(): pass, "pic_chest_open", 1580)
		await create_timer(0.3).timeout
		# Prize entrance adds transient sparkles; wait for them to finish before
		# asserting that the settled reward contains only its art and quantity.
		await create_timer(2.0).timeout
		var items: Array[Node] = page.content.find_children("ChestPrize*", "Control", false, false)
		var kinds := {}
		for kind in prizes:
			kinds[kind] = int(kinds.get(kind, 0)) + 1
		check(items.size() == kinds.size() + 1, "duplicate boosters combine in one object")
		for picture in main.Popups.CHEST_REWARD_INK:
			check(not UI._used.has(picture), "reward path avoids live texture readback: " + picture)
		for item in items:
			check(item.get_child_count() == 2, "prize consists only of an icon and painted amount")
		check(items[0].get_meta("picture") == "coin" and items[0].get_meta("amount") == Progress.CHEST_COINS, "card-free feather reward keeps exact amount")
		if prizes == [0, 0, 2]:
			check(items[1].get_meta("amount") == 2, "duplicate booster quantity is exact")
		await capture("reward_%d_kinds" % kinds.size())
		await click(page.get_node("Content/ConfirmRewards"))
	for locale in ["en", "zh"]:
		UiText.set_language(locale)
		main.Popups.chest(main.popups, 18, 0, func(): pass, func(): pass, 99999)
		await create_timer(0.3).timeout
		var localized_page: Control = main.popups.get_child(-1)
		var open_word: TextureRect = localized_page.get_node("Content/OpenChest/Kit").get_child(1)
		var play_word: TextureRect = localized_page.get_node("Content/Play/Kit").get_child(1)
		check(open_word.painted_id == "white_open" and play_word.painted_id == "white_play", "Open and Play use the shop's painted words")
		var shown := open_word.texture
		var other := "zh" if locale == "en" else "en"
		UiText.set_language(other)
		await process_frame
		check(open_word.texture != shown, "existing chest word updates after locale change")
		UiText.set_language(locale)
		await process_frame
		await capture("chest_" + locale)
		await click(main.popups.get_child(-1).get_node("Content/Close"))
	main.show_chest()
	await create_timer(0.3).timeout
	page = main.popups.get_child(-1)
	await click(page.get_node("Content/Coins/BuyCoins"))
	check(main._coin_shop != null and is_instance_valid(page), "balance still opens purchase page over chest")
	main.remove_child(main._coin_shop)
	main._coin_shop.queue_free()
	main._coin_shop = null
	await click(page.get_node("Content/Close"))
	main.show_chest()
	await create_timer(0.3).timeout
	await click(main.popups.get_child(-1).get_node("Content/Play"))
	check(main.screen.get_script().resource_path.ends_with("board_view.gd"), "Play keeps real level navigation")
	if main.screen.get_script().resource_path.ends_with("board_view.gd"):
		for attempt in 60:
			if not main.screen.busy:
				break
			await create_timer(0.1).timeout
		check(not main.screen.busy, "Play reaches an interactive board")
	sound.set_music_muted(true)
	sound.set_muted(true)
	check(not sound._music.playing, "mute still stops music")
	print("chest checks: %d failures" % failures)
	main.queue_free()
	await process_frame
	await process_frame
	for player in sound.get_children():
		if player is AudioStreamPlayer:
			player.stop()
			player.stream = null
	# Let the audio mixer release stopped playback references before the test process exits.
	await create_timer(0.25).timeout
	quit(1 if failures else 0)

func _set_stars(total: int) -> void:
	main.profile.stars.fill(0)
	main.profile.chests_opened = 0
	for level in range(1, Levels.COUNT + 1):
		main.profile.stars[level] = mini(total, 3)
		total -= main.profile.stars[level]
		if total <= 0:
			break

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
	await create_timer(0.5).timeout

func capture(label: String) -> void:
	if evidence.is_empty():
		return
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png(evidence.path_join(label + ".png"))
	print("CAPTURE %s time_ms=%d" % [label, Time.get_ticks_msec()])
