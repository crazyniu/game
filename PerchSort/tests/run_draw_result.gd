extends SceneTree
## Completion coverage uses the canonical draw page and isolated profiles, never a saved user profile.

var checks := 0
var failures := 0
var main: Control
var evidence := ""
var restores := 0
var draw_calls := 0

func _initialize() -> void:
	_run.call_deferred()

func check(ok: bool, label: String) -> void:
	checks += 1
	failures += 0 if ok else 1
	print("PASS " if ok else "FAIL ", label)

func _wait_gone(item: WeakRef, timeout_ms := 1500) -> bool:
	var deadline := Time.get_ticks_msec() + timeout_ms
	while item.get_ref() != null and Time.get_ticks_msec() < deadline:
		await process_frame
	return item.get_ref() == null

func _run() -> void:
	for arg in OS.get_cmdline_user_args():
		if arg.begins_with("--draw-result-evidence="):
			evidence = arg.trim_prefix("--draw-result-evidence=")
	if not evidence.is_empty():
		DirAccess.make_dir_recursive_absolute(evidence)
	main = load("res://game/main.tscn").instantiate()
	var fake := Profile.new()
	fake.coins = 1280
	fake.highest_unlocked = Levels.COUNT
	fake.language = "zh"
	main.args["profile"] = JSON.stringify(fake.to_dict())
	root.add_child(main)
	main.args["smoke"] = "1"
	main.args["capture"] = "1"
	await process_frame
	var before := JSON.stringify(main.profile.to_dict())
	var popups: GDScript = load("res://game/popups.gd")
	var sound: Node = root.get_node("Sound")
	check(sound._music_name == "bgm_menu", "result uses the home menu music")
	for locale in ["zh", "en"]:
		UiText.set_language(locale)
		for i in Progress.DRAW_PRIZES.size():
			var prize: Array = Progress.DRAW_PRIZES[i]
			if prize[0] == 0:
				continue
			var result: Control = popups.rewards(main.popups, "draw.title", prize[0], [], func(): restores += 1)
			_check_result(result, popups.DRAW_PRIZE_KEYS[i], prize[0], locale)
			if prize[0] == Progress.DRAW_PRIZES[-1][0]:
				await capture("coins_" + locale)
			await _dismiss(result, "Confirm")
		for kind in Board.Booster.size():
			for count in [1, 2]:
				var boosters := []
				for n in count:
					boosters.append(kind)
				var result: Control = popups.rewards(main.popups, "draw.title", 0, boosters, func(): restores += 1)
				_check_result(result, "island_icon_" + popups.DRAW_BOOSTER_ART[kind], count, locale)
				if kind == Board.Booster.SAPLING:
					await capture("booster_%d_%s" % [count, locale])
				await _dismiss(result, "Confirm")
	# The rendering surface supports distinct entries without duplicating a repeated booster.
	var mixed: Control = popups.rewards(main.popups, "draw.title", 120, [0, 1, 1], func(): restores += 1)
	check(mixed.content.get_node("ApprovedLayout").find_children("Reward*", "Control", false, false).size() == 3, "mixed result combines repeated boosters")
	await capture("mixed_en")
	await _dismiss(mixed, "Confirm")
	# Exercise the real economy callback through two successive results, including exhaustion.
	UiText.set_language("zh")
	var economy := Profile.new()
	economy.highest_unlocked = Levels.COUNT
	economy.draws_date = "2026-11-12"
	economy.draws_left = 2
	var rng := RandomNumberGenerator.new()
	rng.seed = 103
	var draw: Control = popups.lucky_draw(main.popups, economy.draws_left, Progress.DRAW_MAX_SPINS, func():
		draw_calls += 1
		return Progress.draw(economy, "2026-11-12", rng))
	var go: Button = draw.find_child("Draw", true, false)
	for remaining in [1, 0]:
		var old_coins: int = economy.coins
		var old_inventory := economy.inventory.duplicate()
		go.pressed.emit()
		go.pressed.emit()
		check(economy.draws_left == remaining and draw_calls == 2 - remaining, "production callback consumes exactly one attempt")
		check(go.disabled and draw.find_child("Close", true, false).disabled, "draw controls locked during result")
		var result: Control = main.popups.get_child(-1)
		check(result.name == "DrawResult" and not draw.content.visible, "completion replaces only the draw content")
		var reward: Control = result.content.get_node("ApprovedLayout/Reward0")
		var inventory_gain := 0
		for i in economy.inventory.size():
			inventory_gain += economy.inventory[i] - old_inventory[i]
		check(economy.coins - old_coins + inventory_gain == reward.get_meta("amount"), "result equals actual production grant")
		var after_grant := JSON.stringify(economy.to_dict())
		await capture("actual_%d_left" % remaining)
		var weak: WeakRef = weakref(result)
		result.content.get_node("Confirm").pressed.emit()
		result.content.get_node("Confirm").pressed.emit()
		check(await _wait_gone(weak), "production result exit completes")
		check(JSON.stringify(economy.to_dict()) == after_grant, "dismissal never grants again")
		check(draw.content.visible and go.disabled == (remaining == 0), "same draw restores with correct availability")
		check(not draw.find_child("Close", true, false).disabled, "draw Close restored")
	go.pressed.emit()
	check(draw_calls == 2, "exhausted action does not call production")
	draw.find_child("Close", true, false).pressed.emit()
	check(await _wait_gone(weakref(draw)), "exhausted draw can close")
	check(main.popups.get_child_count() == 0, "no leftover completion overlay")
	# Owner removal cancels a visible result without invoking its restore callback.
	var owner: Control = popups.lucky_draw(main.popups, 1, Progress.DRAW_MAX_SPINS, func():
		return {"cell": 5, "coins": 0, "booster": Board.Booster.SAPLING, "count": 2})
	owner.find_child("Draw", true, false).pressed.emit()
	owner.queue_free()
	await process_frame
	await process_frame
	check(main.popups.get_child_count() == 0, "removing owner removes visible completion")
	# Animate an actual roll, sample the result rays, then remove its owner.
	main.args.erase("capture")
	var animated: Control = popups.lucky_draw(main.popups, 1, Progress.DRAW_MAX_SPINS, func():
		return {"cell": 5, "coins": 0, "booster": Board.Booster.SAPLING, "count": 2})
	animated.find_child("Draw", true, false).pressed.emit()
	var deadline := Time.get_ticks_msec() + 8000
	var frame := 0
	while main.popups.get_child_count() < 2 and Time.get_ticks_msec() < deadline:
		await capture("roll_%02d" % frame)
		frame += 1
		await create_timer(0.5).timeout
	check(main.popups.get_child_count() == 2, "animated roll reaches completion")
	if main.popups.get_child_count() == 2:
		var result: Control = main.popups.get_child(-1)
		var rays: Control = result.content.get_node("ApprovedLayout/Reward0/Rays")
		var initial := rays.rotation
		await create_timer(0.25).timeout
		await capture("result_motion_start")
		await create_timer(0.75).timeout
		await capture("result_motion_end")
		check(rays.rotation > initial, "completion rays continue rotating")
	animated.queue_free()
	await process_frame
	await process_frame
	check(main.popups.get_child_count() == 0, "animated owner cleanup leaves no result")
	check(JSON.stringify(main.profile.to_dict()) == before, "all tests leave main profile unchanged")
	print("draw result checks: %d checks, %d failures" % [checks, failures])
	main.queue_free()
	await process_frame
	await process_frame
	for player in sound.get_children():
		if player is AudioStreamPlayer:
			player.stop()
			player.stream = null
	quit(1 if failures else 0)

func _check_result(result: Control, picture: String, amount: int, locale: String) -> void:
	var canvas: Control = result.content.get_node("ApprovedLayout")
	var reward: Control = canvas.get_node("Reward0")
	check(result.get_node("ForestBackground").texture == Art.tex("background_shop_forest"), "approved jungle result background")
	check(result.content.get_node("TitleGroup/Sign") != null and result.content.get_node("TitleGroup").get_child(1).painted_id == "lettering_lucky_spin_title", "hanging sign title %s" % locale)
	check(not result.content.has_node("Close"), "result has no duplicate corner exit")
	check(reward.get_meta("picture") == picture and reward.get_node("PrizeArt").texture == Art.tex(picture), "actual prize art %s" % picture)
	check(reward.get_meta("amount") == amount and reward.get_node("Number").get_meta("value") == "+%d" % amount, "exact painted amount %d" % amount)
	check(not result.content.has_node("Panel"), "result has no generic modal frame")

func _dismiss(result: Control, control_name: String) -> void:
	var before := restores
	var weak: WeakRef = weakref(result)
	var button: Button = result.content.get_node(control_name)
	var bounds := button.get_rect()
	button.button_down.emit()
	check(button.get_rect() == bounds, "result press keeps fixed hit geometry")
	button.button_up.emit()
	button.pressed.emit()
	button.pressed.emit()
	result.content.get_node("Confirm").pressed.emit()
	check(await _wait_gone(weak), "result dismissal completes")
	check(restores == before + 1, "Repeated confirmation restores exactly once")

func capture(label: String) -> void:
	if evidence.is_empty():
		return
	await RenderingServer.frame_post_draw
	var screenshot := root.get_texture().get_image()
	screenshot.save_png(evidence.path_join(label + ".png"))
	print("CAPTURE %s time_ms=%d size=%s" % [label, Time.get_ticks_msec(), screenshot.get_size()])
