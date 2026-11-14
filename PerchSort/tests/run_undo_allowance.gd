extends SceneTree
## Undo allowance: three free undos per level attempt (restart keeps the count), then the "More undos" offer pays one
## more for coins or for an ad; the stand-in ad pays its reward exactly once, only from its close button.
var checks := 0
var failures := 0

func _initialize() -> void:
	_run.call_deferred()

func check(ok: bool, message: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		print("FAIL ", message)

func _run() -> void:
	# Core: free undos, restart, grants.
	var session := GameSession.new(Levels.get_level(12))
	check(session.undos_left == GameSession.FREE_UNDOS, "attempt starts with the free undos")
	var made := 0
	for from in session.state.branch_count():
		for to in session.state.branch_count():
			if made < 5 and Rules.can_transfer(session.state, from, to) and session.try_move(from, to):
				made += 1
	check(made >= 4, "moves to undo")
	for i in GameSession.FREE_UNDOS:
		check(session.undo(), "free undo %d" % i)
	check(session.can_undo() and not session.undo(), "no undo past the free ones")
	session.restart()
	check(session.undos_left == 0, "restart does not refill undos")
	session.grant_undos(1)
	check(session.undos_left == 1, "a granted undo is counted")

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
	# The real view restart (it builds a new session): the allowance survives it.
	var board: Control = load("res://game/board_view.gd").new()
	board.instant = true
	root.add_child(board)
	var player := Profile.new()
	board.start(Levels.get_level(1), player, false)
	board.session.undos_left = 0
	board.restart()
	check(board.session.undos_left == 0 and board.session.move_count() == 0, "view restart keeps zero undos and resets the board")
	board.session.undos_left = 7
	board.restart()
	check(board.session.undos_left == 7, "view restart keeps a purchased allowance above three")
	board.queue_free()

	var offers: GDScript = load("res://game/undo_offer.gd")
	var ads: GDScript = load("res://game/ad_service.gd")
	var kit: GDScript = load("res://game/button_kit.gd")
	# The offer: two equal green actions, each dispatching once and closing the card.
	var bought := [0]
	var watched := [0]
	var offer: Control = offers.open(main.popups, Progress.UNDO_PRICE, func(): bought[0] += 1, func(): watched[0] += 1, true)
	var buy: Button = offer.content.get_node("Buy")
	var ad: Button = offer.content.get_node("WatchAd")
	check(buy.size == kit.PRIMARY and ad.size == kit.PRIMARY, "both offer actions are the same size")
	check(absf(buy.get_rect().get_center().y - ad.get_rect().get_center().y) < 0.01, "actions share one row")
	check(buy.position.y >= offer.panel_rect.end.y + 40.0 and ad.position.y >= offer.panel_rect.end.y + 40.0, "actions below the approved frame")
	check(buy.get_node("Content/FeatherPrice/Digits").get_meta("value") == str(Progress.UNDO_PRICE), "price on the buy action")
	check(offer.content.get_node("TitleGroup").has_node("Sign"), "hanging sign title")
	buy.pressed.emit()
	buy.pressed.emit()
	await create_timer(0.6).timeout
	check(bought[0] == 1 and watched[0] == 0, "buy dispatches once")
	offer = offers.open(main.popups, Progress.UNDO_PRICE, func(): bought[0] += 1, func(): watched[0] += 1, true)
	offer.content.get_node("WatchAd").pressed.emit()
	await create_timer(0.6).timeout
	check(watched[0] == 1 and bought[0] == 1, "watch ad dispatches once")
	# The stand-in ad: full screen, reward only from Close, once.
	var rewards := [0]
	var layer: Control = ads.show_rewarded(main, func(): rewards[0] += 1, true)
	check(layer.size == Vector2(1080, 1920) and layer.mouse_filter == Control.MOUSE_FILTER_STOP, "ad covers and blocks the screen")
	check(rewards[0] == 0, "no reward before closing")
	var close: Button = layer.get_node("Close")
	close.pressed.emit()
	close.pressed.emit()
	await process_frame
	check(rewards[0] == 1, "ad reward paid exactly once")
	var timed: Control = ads.show_rewarded(main, func(): rewards[0] += 1)
	check(not timed.has_node("Close") and timed.has_node("Countdown"), "a playing ad shows its countdown, not Close")
	timed.queue_free()
	print("undo allowance checks: %d checks, %d failures" % [checks, failures])
	main.queue_free()
	await process_frame
	quit(1 if failures else 0)
