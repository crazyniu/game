extends SceneTree
## Mounted daily-reward and nested outfit pages follow purchases without rebuilding or losing their state.
var main: Control
var checks := 0
var failures := 0

func _initialize() -> void:
	_run.call_deferred()

func check(ok: bool, label: String) -> void:
	checks += 1
	failures += 0 if ok else 1
	print("PASS " if ok else "FAIL ", label)

func _purchase(bar: Control, confirm := true) -> void:
	var before: int = main.profile.coins
	bar.purchase_requested.emit(bar.amount)
	var shop: Control = main._coin_shop
	var credit: int = shop.PACKS[0]["coins"]
	shop.purchase_requested.emit(shop.PACKS[0])
	var popup: Control = shop.get_node("Popup")
	if confirm:
		popup.content.get_node("Confirm").pressed.emit()
	else:
		popup.close()
	await create_timer(0.2).timeout
	shop.closed.emit()
	await process_frame
	check(main.profile.coins == before + (credit if confirm else 0), "purchase/cancel changes the balance exactly once")

func _run() -> void:
	main = load("res://game/main.gd").new()
	# Force the production main's fresh, never-saved capture profile without depending on command-line flags.
	main.args = {"capture": "/tmp/live-feather-unused.png", "wait": "120"}
	root.add_child(main)
	await process_frame
	main.profile.coins = 100
	main.show_daily_reward()
	var daily: Control = main.popups.get_node("DailyReward")
	var bar: Control = daily.content.get_node("Coins")
	await _purchase(bar)
	check(bar.amount == main.profile.coins, "daily page shows the top-up")
	check(main.popups.get_node("DailyReward") == daily, "daily page stays mounted")
	await _purchase(bar, false)
	check(bar.amount == main.profile.coins, "daily cancel preserves balance")
	daily.content.get_node("Claim").pressed.emit()
	await process_frame
	check(bar.amount == main.profile.coins, "claim uses live balance, not the opening balance")
	check(daily.content.get_node("Claim").disabled, "claimed state is retained")
	main._clear_popups()
	await process_frame
	main.profile.met["sunny"] = true
	main._open_shop(0)
	var shop: Control = main._overlay
	shop.instant = true
	shop._select_tab(1)
	var bird: Control = shop.open_outfits("sunny")
	await _purchase(bird._coins)
	check(bird._coins.amount == main.profile.coins, "nested outfit detail shows the top-up")
	check(shop._coins.amount == main.profile.coins, "outer shop remains in sync")
	check(shop._outfit_page == bird, "outfit page and selection stay mounted")
	await _purchase(bird._coins, false)
	check(bird._coins.amount == main.profile.coins, "outfit cancel preserves balance")
	main.profile.coins = load("res://game/feather_bar.gd").DISPLAY_MAX
	bird.refresh_balance()
	await _purchase(bird._coins)
	check(bird._coins.amount == main.profile.coins, "full balance is retained beyond display cap")
	check(bird._coins.get_meta("display_value") == str(load("res://game/feather_bar.gd").DISPLAY_MAX), "display cap remains presentation-only")
	main.queue_free()
	await process_frame
	print("Live feather balances: %d checks, %d failures" % [checks, failures])
	quit(1 if failures else 0)
