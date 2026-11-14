extends SceneTree
## Daily check-in timing: the rewards card opens only after the last flying coin has landed.
var checks := 0
var failures := 0

func _initialize() -> void:
	_run.call_deferred()

func check(ok: bool, message: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		print("FAIL ", message)

func _coins_in_flight(layer: Node, coin_texture: Texture2D) -> int:
	var n := 0
	for child in layer.get_children():
		if child is TextureRect and child.texture == coin_texture:
			n += 1
	return n

func _run() -> void:
	var main: Control = load("res://game/main.tscn").instantiate()
	var fake := Profile.new()
	fake.muted = true
	fake.music_muted = true
	fake.highest_unlocked = 10
	main.args["profile"] = JSON.stringify(fake.to_dict())
	main.args["preview"] = "coin_shop_view:normal"
	main.args["motion"] = "1"
	root.add_child(main)
	await create_timer(0.3).timeout
	main.args["smoke"] = "1"
	main.args["capture"] = "1"
	main.show_daily_reward()
	await create_timer(0.5).timeout
	var page: Control = main.popups.get_node("DailyReward")
	var claim: Button = page.content.get_node("Claim")
	check(not claim.disabled, "today's reward can be claimed")
	var coin_texture: Texture2D = Art.tex("coin")
	var t0 := Time.get_ticks_msec()
	claim.pressed.emit()
	var last_flight := 0
	var opened := -1
	var seen_flight := false
	while Time.get_ticks_msec() - t0 < 5000:
		await process_frame
		var now := Time.get_ticks_msec() - t0
		var flying := _coins_in_flight(page.content, coin_texture)
		if flying > 0:
			seen_flight = true
			last_flight = now
		if main.popups.get_child_count() > 1 and opened < 0:
			opened = now
			check(flying == 0, "no coin is still flying when the rewards card opens (%d)" % flying)
			break
	print("DAILY_FLOW coins_seen=%s last_coin_ms=%d rewards_open_ms=%d" % [seen_flight, last_flight, opened])
	check(seen_flight, "coins fly into the counter")
	check(opened > last_flight, "the rewards card opens after the last coin (%d ms vs %d ms)" % [opened, last_flight])
	print("daily reward flow checks: %d checks, %d failures" % [checks, failures])
	quit.call_deferred(1 if failures else 0)
