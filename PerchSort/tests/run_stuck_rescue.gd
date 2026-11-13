extends SceneTree
## Stuck rescue: the first dead end of an attempt offers a free step back to the latest position that can still be
## won (no undo allowance spent); a tool with no stock is offered on the board for coins or an ad.
var checks := 0
var failures := 0

func _initialize() -> void:
	_run.call_deferred()

func check(ok: bool, message: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		print("FAIL ", message)

func _legal(s: Board) -> Array:
	var moves := []
	for from in s.branch_count():
		for to in s.branch_count():
			if Rules.can_transfer(s, from, to):
				moves.append([from, to])
	return moves

func _run() -> void:
	# Core: rewind steps back without touching the undo allowance.
	var session := GameSession.new(Levels.get_level(12))
	var start_key := session.state.search_key(false)
	for i in 3:
		var m: Array = _legal(session.state)[0]
		check(session.try_move(m[0], m[1]), "move %d" % i)
	check(session.history_size() == 3, "three positions behind")
	check(not session.rewind(0) and not session.rewind(4), "rewind refuses steps outside the history")
	check(session.rewind(3) and session.state.search_key(false) == start_key, "rewind reaches the opening position")
	check(session.undos_left == GameSession.FREE_UNDOS and session.history_size() == 0, "rewind spends no undo")

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
	# The dead-end level has opening moves that can never be recovered from: play one, then one more.
	var board: Control = load("res://game/board_view.gd").new()
	main.set_screen(board)
	board.start(load("res://game/stuck_offer.gd").DEAD_END_LEVEL, main.profile, false)
	var fatal := []
	for m in _legal(board.session.state):
		var probe: Board = board.session.state.clone(false)
		Rules.new().transfer(probe, m[0], m[1], null)
		if Solver.hint(probe, [], 24000, 5000)["status"] == Solver.Status.UNSOLVABLE:
			fatal = m
			break
	check(not fatal.is_empty(), "the dead-end level has a losing opening move")
	if not fatal.is_empty():
		var opening: PackedInt32Array = board.session.state.search_key(false)
		board.session.try_move(fatal[0], fatal[1])
		var next: Array = _legal(board.session.state)
		if not next.is_empty():
			board.session.try_move(next[0][0], next[0][1])
		var behind: int = board.session.history_size()
		await board.rescue()
		check(board.session.state.search_key(false) == opening, "rescue returns to the last winnable position (%d back)" % behind)
		check(board.session.free_rescue_used, "the free rescue is used up")
		check(board.session.undos_left == GameSession.FREE_UNDOS, "rescue spends no undo")
		check(Solver.hint(board.session.state, [], 24000, 5000)["status"] == Solver.Status.SOLVED, "the rescued position can be won")
		board.session.restart()
		check(not board.session.free_rescue_used, "restart renews the free rescue")

	var kit: GDScript = load("res://game/button_kit.gd")
	var rescued := [0]
	var offer: Control = load("res://game/stuck_offer.gd").open(main.popups, func(): rescued[0] += 1, true)
	var go: Button = offer.content.get_node("Rescue")
	check(go.size == kit.PRIMARY, "rescue action is the primary size")
	check(absf(go.get_rect().get_center().x - 540.0) < 0.01, "rescue action is centred")
	check(offer.panel_rect.encloses(go.get_rect()), "rescue action inside the card")
	check(offer.content.get_node("TitleGroup").has_node("Sign"), "hanging sign title")
	check(go.has_node("Content/Icon") and offer.content.has_node("Picture"), "picture and action icon")
	go.pressed.emit()
	go.pressed.emit()
	await create_timer(0.6).timeout
	check(rescued[0] == 1, "rescue dispatches once")

	var bought := [0]
	var tool: Control = load("res://game/undo_offer.gd").open_item(main.popups, "item_sprout", load("res://game/ui.gd").PICTURES["sapling"],
		Progress.booster_price(Board.Booster.SAPLING), func(): bought[0] += 1, func(): pass, true)
	var buy: Button = tool.content.get_node("Buy")
	check(buy.size == tool.content.get_node("WatchAd").size, "tool offer actions are the same size")
	check(buy.get_node("Content/FeatherPrice/Digits").get_meta("value") == str(Progress.booster_price(Board.Booster.SAPLING)), "tool price on the buy action")
	buy.pressed.emit()
	await create_timer(0.6).timeout
	check(bought[0] == 1, "tool buy dispatches once")
	print("stuck rescue: %d checks, %d failures" % [checks, failures])
	quit(1 if failures else 0)
