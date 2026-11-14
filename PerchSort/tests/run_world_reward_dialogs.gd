extends SceneTree
## World and daily reward shells preserve dynamic contents, bounds and callbacks.
var checks := 0
var failures := 0
var calls := 0
func check(ok: bool, message: String) -> void:
	checks += 1
	if not ok: failures += 1
	print("PASS " if ok else "FAIL ", message)
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
	for locale in ["en", "zh"]:
		TranslationServer.set_locale(locale)
		for state in ["world_unlock_2", "world_unlock_4", "world_complete_1", "rewards_daily", "rewards_daily_coins", "rewards_daily_all"]:
			popups.preview(main, state)
			await process_frame
			var p = main.popups.get_child(main.popups.get_child_count() - 1)
			check(p.content.get_node("Panel/Rim") is NinePatchRect, state + " jungle shell")
			check(p.panel_rect.position.y >= 32, state + " top visible")
			var action := "Play" if state.begins_with("world_unlock") else ("Check" if state.begins_with("world_complete") else "action_ok")
			var button: Control = p.content.get_node(action)
			check(button.position.y >= p.panel_rect.end.y + 40, state + " action outside")
			check(button.get_rect().end.y <= 1900, state + " action on screen")
			check(is_equal_approx(button.get_rect().get_center().x, 540), state + " action centered")
			if state == "rewards_daily_all":
				for tool in 6:
					var prize: Control = p.content.get_node("Reward%d" % (tool + 1))
					var art_name := "island_icon_%s" % load("res://game/shop_view.gd").BOOSTER_ILLUS[tool]
					check(prize.get_child(0).texture == Art.tex(art_name), "daily tool matches shop artwork")
					check(prize.get_meta("amount") == 1, "daily tool quantity preserved")
				check(p.content.has_node("Reward6"), "all seven prizes retained")
				var last: Control = p.content.get_node("Reward6")
				check(last.get_rect().end.y < p.panel_rect.end.y - 100, "last prize clears footer")
			for child in main.popups.get_children(): child.queue_free()
			await process_frame
	for tool in 6:
		var holder := Control.new()
		main.popups.add_child(holder)
		popups._drops(holder, 0.0, [tool, tool], true, 0.0)
		var drop: Control = holder.get_node("Drop%d" % tool)
		var art_name := "island_icon_%s" % load("res://game/shop_view.gd").BOOSTER_ILLUS[tool]
		check(drop.get_child(0).texture == Art.tex(art_name), "win tool matches shop artwork")
		check(drop.get_node("Quantity").get_meta("value") == "+2", "win repeated quantity preserved")
		print("DROP_START ", drop.scale)
		check(drop.scale.length() < 0.001, "win tool entrance begins collapsed")
		await create_timer(0.5).timeout
		print("DROP_END ", drop.scale)
		check(drop.scale.x >= 0.98 and drop.scale.x <= 1.03 and drop.scale.y >= 0.98 and drop.scale.y <= 1.03, "win tool entrance settles into full-size breathing")
		holder.queue_free()
		await process_frame
	for kind in ["unlock", "complete", "reward"]:
		var cb := func(): calls += 1
		var p
		var name: String
		if kind == "unlock":
			p = popups.world_unlock(main.popups, 10, cb)
			name = "Play"
		elif kind == "complete":
			p = popups.world_complete(main.popups, 10, 99999, cb)
			name = "Check"
		else:
			p = popups.rewards(main.popups, "daily_reward.title", 0, [], cb, "pic_calendar")
			name = "action_ok"
		p.content.get_node(name).pressed.emit()
		p.content.get_node(name).pressed.emit()
		await create_timer(0.2).timeout
	check(calls == 3, "all three callbacks fire once despite double tap")
	print("WORLD_REWARDS checks=%d failures=%d" % [checks, failures])
	main.queue_free()
	quit(1 if failures else 0)
