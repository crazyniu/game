extends Node
## Production draw/claim/win timelines with explicit mutation counters and interruption checks.
var main: Control
var popups: GDScript
var failures := 0
var checks := 0

func check(ok: bool, label: String) -> void:
	checks += 1
	failures += 0 if ok else 1
	print("PASS " if ok else "FAIL ", label)

func _ready() -> void:
	_run.call_deferred()

func _run() -> void:
	main = load("res://game/main.tscn").instantiate()
	var fake := Profile.new()
	fake.muted = true
	fake.music_muted = true
	main.args["profile"] = JSON.stringify(fake.to_dict())
	main.args["preview"] = "coin_shop_view:normal"
	add_child(main)
	await get_tree().create_timer(0.4).timeout
	main.args["smoke"] = "1"
	popups = load("res://game/popups.gd")
	await _claim_boundary()
	if OS.get_cmdline_user_args().has("--boundary-only"):
		_finish()
		return
	for target in range(8):
		await _draw_case(target)
	await _draw_exit()
	await _claim_normal()
	await _win_case()
	await _win_exit()
	_finish()

func _finish() -> void:
	print("REWARD MOTION checks=%d failures=%d" % [checks, failures])
	main.queue_free()
	get_tree().quit.call_deferred(1 if failures else 0)

func _claim_boundary() -> void:
	var calls := [0]
	var page: Modal = popups.daily_reward(main.popups, 1, true, 0, func():
		calls[0] += 1
		return {"day": 1, "coins": 0, "boosters": []})
	await get_tree().create_timer(0.3).timeout
	var claim := page.content.get_node("Claim") as Button
	claim.pressed.emit()
	claim.pressed.emit()
	await get_tree().create_timer(1.02).timeout
	page.close()
	await get_tree().create_timer(0.5).timeout
	check(calls[0] == 1, "boundary close preserves exactly one already-settled claim")
	check(main.popups.get_child_count() == 0, "closing in last 0.12 seconds cannot reopen claim results")
	main._clear_popups()
	await get_tree().process_frame

func _draw_case(target: int) -> void:
	var calls := [0]
	var page: Modal = popups.lucky_draw(main.popups, 3, 5, func():
		calls[0] += 1
		return {"cell": target, "coins": 120, "booster": -1, "count": 0})
	await get_tree().create_timer(0.3).timeout
	var draw := page.find_child("Draw", true, false) as Button
	var began := Time.get_ticks_msec()
	draw.pressed.emit()
	draw.pressed.emit()
	var selected := false
	while main.popups.get_node_or_null("DrawResult") == null and Time.get_ticks_msec() - began < 5000:
		await get_tree().process_frame
		var card := page.find_child("Prize%d" % target, true, false) as Control
		selected = selected or (card.scale.x > 1.08 and card.get_node("Highlight").modulate.a > 0.9)
	var elapsed := Time.get_ticks_msec() - began
	var result := main.popups.get_node_or_null("DrawResult") as Modal
	print("DRAW target=%d result_ms=%d" % [target, elapsed])
	check(calls[0] == 1 and draw.disabled, "draw %d duplicate press settles once" % target)
	check(result != null and selected, "draw %d actual target visibly pops before result" % target)
	check(elapsed < 3600, "draw %d result has bounded decorative delay" % target)
	if result:
		result.close()
		result.close()
		await get_tree().create_timer(0.22).timeout
		check(not draw.disabled and page.content.visible, "draw %d dismiss restores board and controls" % target)
		for i in 8:
			var card := page.find_child("Prize%d" % i, true, false) as Control
			check(card.scale.is_equal_approx(Vector2.ONE) and is_equal_approx(card.modulate.a, 1.0), "draw %d card %d resets" % [target, i])
	page.close()
	await get_tree().create_timer(0.22).timeout

func _draw_exit() -> void:
	var calls := [0]
	var page: Modal = popups.lucky_draw(main.popups, 3, 5, func():
		calls[0] += 1
		return {"cell": 7, "coins": 200, "booster": -1, "count": 0})
	await get_tree().create_timer(0.3).timeout
	(page.find_child("Draw", true, false) as Button).pressed.emit()
	await get_tree().create_timer(0.1).timeout
	page.queue_free()
	await get_tree().create_timer(3.0).timeout
	check(calls[0] == 1 and main.popups.get_child_count() == 0, "removing draw during roll neither repays nor opens stale results")

func _claim_normal() -> void:
	var calls := [0]
	var balance := [100]
	var page: Modal = popups.daily_reward(main.popups, 1, true, balance[0], func():
		calls[0] += 1
		balance[0] += 50
		return {"day": 1, "coins": 50, "boosters": []}, func(): return balance[0])
	await get_tree().create_timer(0.3).timeout
	(page.content.get_node("Claim") as Button).pressed.emit()
	(page.content.get_node("Claim") as Button).pressed.emit()
	var day := page.content.get_node("Day1") as Control
	await get_tree().create_timer(0.14).timeout
	check(day.scale.x > 1.05, "daily claim visibly lifts the claimed card")
	balance[0] += 600
	await get_tree().create_timer(1.8).timeout
	check(calls[0] == 1 and balance[0] == 750 and page.content.get_node("Coins").amount == 750, "daily claim animation shows the current balance including later credit")
	check(day.scale.is_equal_approx(Vector2.ONE), "daily claim card returns to its layout")
	main._clear_popups()
	await get_tree().process_frame

func _win_case() -> void:
	var actions := [0]
	var page: Modal = popups.win(main.popups, 3, 12, 12, 120, [0, 1], func(): actions[0] += 1,
		func(): actions[0] += 1, func(): actions[0] += 1, true)
	await get_tree().create_timer(0.55).timeout
	check(page.content.get_node("RatingStar0").modulate.a > 0.9 and page.content.get_node("RatingStar2").modulate.a < 0.1, "win stars reveal in order, not all at once")
	await get_tree().create_timer(2.2).timeout
	for i in 3:
		check(page.content.get_node("RatingStar%d" % i).scale.is_equal_approx(Vector2.ONE), "win star %d settles" % i)
	check(page.content.get_node("Reward/Number").get_meta("value") == "+120", "win displayed reward counts to the exact award")
	page.invoke(func(): actions[0] += 1)
	page.invoke(func(): actions[0] += 1)
	await get_tree().create_timer(0.22).timeout
	check(actions[0] == 1, "win navigation still dispatches exactly once")

func _win_exit() -> void:
	var actions := [0]
	var page: Modal = popups.win(main.popups, 3, 12, 12, 120, [0, 1], func(): actions[0] += 1,
		func(): actions[0] += 1, func(): actions[0] += 1, true)
	await get_tree().create_timer(0.03).timeout
	var next := page.content.get_node("action_next") as Button
	check(not next.disabled, "win Next is usable before decoration completes")
	next.pressed.emit()
	next.pressed.emit()
	await get_tree().create_timer(2.5).timeout
	check(actions[0] == 1 and not is_instance_valid(page), "early win exit cancels owned decoration and dispatches once")
