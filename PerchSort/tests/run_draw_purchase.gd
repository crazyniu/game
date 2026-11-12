extends SceneTree
## Page contracts with fake state. Signal-driven checks supplement actual desktop pointer QA.

var failures := 0
var checks := 0
var main: Control
var calls := 0

func _initialize() -> void:
	_run.call_deferred()

func check(ok: bool, label: String) -> void:
	checks += 1
	failures += 0 if ok else 1
	print("PASS " if ok else "FAIL ", label)

func _wait_until(predicate: Callable, timeout_ms := 1000) -> bool:
	var deadline := Time.get_ticks_msec() + timeout_ms
	while not predicate.call():
		if Time.get_ticks_msec() >= deadline:
			return false
		await process_frame
	return true

func _run() -> void:
	main = load("res://game/main.tscn").instantiate()
	var fake := Profile.new()
	fake.coins = 1280
	fake.highest_unlocked = Levels.COUNT
	fake.language = "zh"
	main.args["profile"] = JSON.stringify(fake.to_dict())
	root.add_child(main)
	# Set after ready: prevent saves without starting the aggregate smoke driver.
	main.args["smoke"] = "1"
	main.args["capture"] = "1"
	await create_timer(0.3).timeout
	var home: Control = main.screen
	var before := JSON.stringify(main.profile.to_dict())
	var popups_script: GDScript = load("res://game/popups.gd")
	var display_max: int = load("res://game/feather_bar.gd").DISPLAY_MAX
	for balance in [0, 1280, display_max, display_max + 1]:
		main.open_coin_shop(balance)
		var view: Control = main._coin_shop
		main.open_coin_shop(balance)
		check(main._coin_shop == view, "purchase re-entry preserves one page")
		var page := view.get_node("ApprovedLayout")
		check(page.get_node("BalanceNumber").get_meta("value") == str(mini(balance, display_max)), "balance cap %d" % balance)
		check(view.find_children("*", "FeatherBar", true, false).is_empty(), "page counter is read-only")
		for i in view.PACKS.size():
			var pack: Dictionary = view.PACKS[i]
			var display := page.get_node("Pack_" + pack.id)
			var button := display.get_node("Buy_" + pack.id) as Button
			check(display.get_node("Number").get_meta("value") == str(pack.coins), "amount %s" % pack.id)
			var bounds := button.get_rect()
			button.button_down.emit()
			check(button.get_rect() == bounds and button.size == view.PRICE_SIZE, "fixed press geometry %s" % pack.id)
			button.button_up.emit()
			button.pressed.emit()
			await create_timer(0.03).timeout
			var modal := view.get_node("Popup") as Control
			check(modal._title_id == "coins.preview", "explicit simulation %s" % pack.id)
			check(modal.content.get_node("Number").get_meta("value") == "+%d" % pack.coins, "simulation amount %s" % pack.id)
			check(JSON.stringify(main.profile.to_dict()) == before, "no profile mutation %s" % pack.id)
			modal.content.get_node("Confirm" if i % 2 == 0 else "Close").pressed.emit()
			await create_timer(0.2).timeout
			check(view.get_node_or_null("Popup") == null, "simulation dismissed %s" % pack.id)
		UiText.set_language("en")
		check(page.get_node("label_purchase_title").texture == Art.tex("label_purchase_title_en"), "live English purchase title")
		UiText.set_language("zh")
		check(page.get_node("label_purchase_title").texture == Art.tex("label_purchase_title_zh"), "live Chinese purchase title")
		page.get_node("Close").pressed.emit()
		await process_frame
		check(main._coin_shop == null and main.screen == home, "purchase restores same Home")
	# A single instant draw still blocks duplicates while its reward is open.
	var draw: Control = popups_script.lucky_draw(main.popups, 1, Progress.DRAW_MAX_SPINS, func():
		calls += 1
		return {"cell": 2, "coins": 0, "booster": Board.Booster.SAPLING, "count": 1})
	var action := draw.find_child("Draw", true, false) as Button
	var rect := action.get_rect()
	action.pressed.emit()
	action.pressed.emit()
	check(calls == 1 and action.disabled, "draw duplicate lockout")
	check(action.get_rect() == rect, "disabled Draw retains geometry")
	var reward := main.popups.get_child(main.popups.get_child_count() - 1) as Control
	var reward_ref: WeakRef = weakref(reward)
	reward.content.get_node("Confirm").pressed.emit()
	# The restoration is connected to the exit tween's completion, not wall-clock
	# time. Slow renderer frames may outlive a fixed 0.2-second test timer.
	check(await _wait_until(func(): return reward_ref.get_ref() == null), "result exit completes within timeout")
	check(draw.content.visible and action.disabled, "zero attempts after result")
	check(draw.find_child("Prize2", true, false).get_node("PrizeArt").texture == Art.tex("icon_draw_mystery_single"), "accepted mystery sprite restored")
	UiText.set_language("en")
	check(draw.content.get_node("DrawHeading").texture == load("res://art/locale/en/island_words/title_lucky_spin.png"), "live English draw title")
	check(action.get_node("Face").texture == Art.tex("btn_lucky_spin_action_en"), "live English draw action")
	UiText.set_language("zh")
	draw.find_child("Close", true, false).pressed.emit()
	await create_timer(0.2).timeout
	check(main.popups.get_child_count() == 0, "draw closes without leftover overlay")
	var empty: Control = popups_script.lucky_draw(main.popups, 0, Progress.DRAW_MAX_SPINS, func(): calls += 1; return {})
	var empty_action := empty.find_child("Draw", true, false) as Button
	check(empty_action.disabled, "no-attempt action disabled")
	empty_action.pressed.emit()
	check(calls == 1, "no-attempt callback not invoked")
	empty.queue_free()
	await process_frame
	# Destroying the owner cancels the animated ring before a result can be created.
	main.args.erase("capture")
	var interrupted: Control = popups_script.lucky_draw(main.popups, 3, Progress.DRAW_MAX_SPINS, func():
		return {"cell": 4, "coins": 200, "booster": -1, "count": 0})
	interrupted.find_child("Draw", true, false).pressed.emit()
	interrupted.queue_free()
	await create_timer(0.4).timeout
	check(main.popups.get_child_count() == 0, "removed draw cancels animated callbacks")
	check(JSON.stringify(main.profile.to_dict()) == before, "all fake flows preserve profile")
	main.queue_free()
	await process_frame
	print("draw/purchase checks: %d checks, %d failures" % [checks, failures])
	quit(1 if failures else 0)
