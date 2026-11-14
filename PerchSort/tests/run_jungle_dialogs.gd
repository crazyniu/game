extends SceneTree
## Opt-in green/wood shell preserves both dialogs' states and action callbacks.
var checks := 0
var failures := 0
var calls := 0

func check(ok: bool, label: String) -> void:
	checks += 1
	if not ok:
		failures += 1
	print("PASS " if ok else "FAIL ", label)

func _initialize() -> void:
	_run.call_deferred()

func _run() -> void:
	var main: Control = load("res://game/main.tscn").instantiate()
	var fake := Profile.new()
	fake.muted = true
	fake.music_muted = true
	main.args["profile"] = JSON.stringify(fake.to_dict())
	main.args["preview"] = "coin_shop_view:normal"
	root.add_child(main)
	main.args["smoke"] = "1"
	main.args["capture"] = "1"
	await process_frame
	var popups: GDScript = load("res://game/popups.gd")
	var navigation: GDScript = load("res://game/menu_navigation.gd")
	for locale in ["en", "zh"]:
		TranslationServer.set_locale(locale)
		for state in ["level_card_first_visit", "level_card_full_roster", "level_card_locked", "level_card_tools_two", "level_card_tools_four", "win_three_stars", "win_two_stars", "win_one_star", "win_zero_reward", "win_milestone"]:
			popups.preview(main, state)
			await process_frame
			var dialog: Control = main.popups.get_child(main.popups.get_child_count() - 1)
			check(dialog.content.get_node("Panel/Rim") is NinePatchRect, locale + " " + state + " opts into new shell")
			check(dialog.panel_rect.position.y >= 20.0, state + " frame stays below top edge")
			var face: Panel = dialog.content.get_node("Panel/CleanFace")
			var rim: NinePatchRect = dialog.content.get_node("Panel/Rim")
			check(face.get_theme_stylebox("panel").bg_color.a == 1.0, "opaque warm face")
			check(face.mouse_filter == Control.MOUSE_FILTER_IGNORE, "face never intercepts input")
			check(rim.mouse_filter == Control.MOUSE_FILTER_IGNORE, "rim never intercepts input")
			check(not rim.draw_center, "contaminated image center never drawn")
			check(rim.patch_margin_left == rim.patch_margin_right, "balanced border margins")
			check(not dialog.content.has_node("LeavesUpper"), "no old leafy frame ornaments")
			if state.begins_with("level_card"):
				var close: Control = dialog.content.get_node("Close")
				check(is_equal_approx(dialog.panel_rect.end.x - close.get_rect().end.x, navigation.DIALOG_RIGHT_PAD), state + " Close has 56px right inset")
				check(close.position.y == dialog.panel_rect.position.y + 48.0, state + " Close sits in header")
				check(dialog.panel_rect.encloses(close.get_rect()), state + " entire Close hit area inside frame")
				check(close.get_rect().end.y < float(dialog.get_meta("ys")[0]), state + " Close clears stars")
			var names := ["Play"] if state.begins_with("level_card") else ["action_next", "Retry", "Home"]
			for n in names:
				var button: Control = dialog.content.get_node(n)
				check(button.position.y >= dialog.panel_rect.end.y + 40.0, state + " " + n + " outside frame")
				check(button.get_rect().end.y <= 1900, state + " " + n + " fully on screen")
			if state.begins_with("win"):
				for button_name in ["action_next", "Retry", "Home"]:
					var b: Control = dialog.content.get_node(button_name)
					var word_path := "Content/NextWord" if button_name == "action_next" else "Word"
					var word: TextureRect = b.get_node(word_path)
					var ink := Rect2(word.texture.get_image().get_used_rect())
					var ink_center := word.position + ink.get_center() * word.size / word.texture.get_size()
					check(ink_center.distance_to(b.size * 0.5) < 4, locale + " " + button_name + " visible glyph centered")
				for n in ["Retry", "Home"]:
					check(dialog.content.get_node(n).has_node("LowerRim") and dialog.content.get_node(n).has_node("ContactShadow"), n + " has restrained wooden depth")
			if state == "level_card_locked":
				check(dialog.content.get_node("Play").disabled, "locked Play remains disabled")
			if state == "win_one_star":
				check(dialog.content.get_node("action_next").disabled, "disabled Next remains disabled")
			if state == "win_zero_reward":
				check(not dialog.content.has_node("Reward"), "zero reward stays omitted")
			for child in main.popups.get_children():
				child.queue_free()
			await process_frame
	for locale in ["en", "zh"]:
		TranslationServer.set_locale(locale)
		var shell: GDScript = load("res://game/jungle_dialog.gd")
		var empty: Control = shell.open(main.popups, "", "", [400], true, 130, false)
		check(empty.content.get_node("TitleGroup").get_child_count() == 1, "empty title has banner only")
		empty.queue_free()
		await process_frame
		for species in Board.SPECIES:
			var friend: Control = popups.new_friend(main.popups, species, 25, func(): calls += 1)
			await process_frame
			check(friend.content.get_node("Panel/Rim") is NinePatchRect, species + " uses jungle shell")
			check(friend.content.has_node("Ability"), species + " retains ability")
			var ok: Control = friend.content.get_node("Check")
			check(ok.position.y >= friend.panel_rect.end.y + 40, "OK outside frame")
			var word: TextureRect = ok.get_node("Content/Word")
			var ink := Rect2(word.texture.get_image().get_used_rect())
			var ink_center := word.position + ink.get_center() * word.size / word.texture.get_size()
			check(ink_center.distance_to(ok.size * 0.5) < 4, locale + " OK visible glyph centered")
			ok.pressed.emit()
			ok.pressed.emit()
			await create_timer(0.2).timeout
	check(calls == 20, "all species in both locales invoke once")
	calls = 0
	for action in ["action_next", "Retry", "Home"]:
		var cb := func(): calls += 1
		var dialog: Control = popups.win(main.popups, 3, 12, 12, 120, [], cb, cb, cb, true)
		dialog.content.get_node(action).pressed.emit()
		dialog.content.get_node(action).pressed.emit()
		await create_timer(0.2).timeout
	check(calls == 3, "Next Replay Home each invoke once despite repeated presses")
	var level: Control = popups.level_card(main.popups, 1, 0, 0, 6, ["sunny"] as Array[String], [] as Array[String], func(): calls += 1, true)
	level.content.get_node("Play").pressed.emit()
	await create_timer(0.2).timeout
	check(calls == 4, "Play retains its callback")
	for locale in ["en", "zh"]:
		TranslationServer.set_locale(locale)
		main.show_level_card(71)
		await process_frame
		var entry: Control = main.popups.get_child(main.popups.get_child_count() - 1)
		check(entry.panel_rect.size.y <= 1200.0, locale + " actual level 71 entry fits a compact card")
		var sign: Control = entry.content.get_node("TitleGroup")
		var close: Control = entry.content.get_node("Close")
		var word: TextureRect = sign.get_child(1)
		var ink := Rect2(word.texture.get_image().get_used_rect())
		var visible := word.get_global_transform() * Rect2(ink.position * word.size / word.texture.get_size(), ink.size * word.size / word.texture.get_size())
		check(not visible.intersects(close.get_global_rect()), locale + " entry title clears Close")
		check(not sign.get_node("TitleNumber").get_global_rect().intersects(close.get_global_rect()), locale + " entry ordinal clears Close")
		entry.queue_free()
		await process_frame
	for count in range(5, 9):
		var species: Array[String] = []
		for i in count:
			species.append(Board.SPECIES[i])
		var entry: Control = popups.level_card(main.popups, 71, 1, 16, 16, species, [] as Array[String], func(): pass, true)
		var birds: Array[Node] = entry.content.find_children("Bird_*", "Button", false, false)
		check(birds.size() == count, "compact roster preserves all %d bird actions" % count)
		for i in birds.size():
			var bounds: Rect2 = birds[i].get_rect()
			check(entry.panel_rect.encloses(bounds), "compact bird tile stays within frame")
			for j in range(i):
				check(not bounds.intersects(birds[j].get_rect()), "compact bird tiles do not overlap")
		entry.queue_free()
		await process_frame
	print("JUNGLE_DIALOGS checks=%d failures=%d" % [checks, failures])
	main.queue_free()
	await process_frame
	await process_frame
	quit.call_deferred(1 if failures else 0)
