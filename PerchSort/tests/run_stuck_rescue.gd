extends SceneTree
## Stuck rescue: the first dead end of an attempt offers a free step back to the latest position that can still be
## won (no undo allowance spent); a tool with no stock is offered on the board for coins or an ad.
var checks := 0
var failures := 0
var max_frame_ms := -1.0

func _initialize() -> void:
	_run.call_deferred()

func check(ok: bool, message: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		print("FAIL ", message)

## Cannot be won without boosters; one Sapling makes it winnable (found by a search over shuffled boards).
const SAPLING_ONLY := {"id": 99, "world": 1, "variant": "noon", "capacity": 4, "par": 10, "branches": [
	{"side": "L", "row": 0, "birds": ["sunny", "berry", "sky", "sunny"]},
	{"side": "R", "row": 0, "birds": ["cocoa", "berry", "berry", "berry"]},
	{"side": "L", "row": 1, "birds": ["cocoa", "cocoa", "sky", "sky"]},
	{"side": "R", "row": 1, "birds": ["sunny", "cocoa"]},
	{"side": "L", "row": 2, "birds": ["sky", "sunny"]}]}

## Lets the board finish its move and its background hint search (which may itself raise the stuck pointer).
func _settle(board: Control) -> void:
	await process_frame
	var t0 := Time.get_ticks_msec()
	while (board.busy or board._hint_thread != null) and Time.get_ticks_msec() - t0 < 15000:
		await process_frame
	board.refresh_hud()
	await process_frame

## Runs the stuck pointer's tool search; returns the longest main-thread frame (ms) while it ran.
func _stuck_search(board: Control) -> float:
	var done := [false]
	var run := func():
		await board.show_stuck_hint()
		done[0] = true
	run.call()
	var longest := 0.0
	var last := Time.get_ticks_usec()
	var deadline := Time.get_ticks_msec() + 15000
	while not done[0] and Time.get_ticks_msec() < deadline:
		await process_frame
		var now := Time.get_ticks_usec()
		longest = maxf(longest, (now - last) / 1000.0)
		last = now
	check(done[0], "tool search finishes within its deadline")
	return longest

## Worker dispatch/join cost is distinct from whole-frame scheduling/rendering on the host.
func _check_performance(board: Control, gap: float, label: String) -> void:
	check(board.rescue_tool_main_ms < 16.0, label + " main-thread search slice stays below 16 ms")
	if max_frame_ms > 0.0:
		check(gap < max_frame_ms, label + " entire frame stays below explicit %.2f ms budget (%.2f ms observed)" % [max_frame_ms, gap])

## True when the stuck glow stands on the tool slot of `kind` (-1 undo).
func _pointer_on(board: Control, kind: int) -> bool:
	if not is_instance_valid(board._stuck):
		return false
	var glow: Control = board._stuck.get_node("Glow")
	for slot in board._tool_slots:
		if slot[0] == kind:
			return glow.get_rect().get_center().distance_to(slot[1]) < 1.0
	return false

## The live tool button standing on the slot of `kind`.
func _slot_button(board: Control, kind: int) -> Button:
	for slot in board._tool_slots:
		if slot[0] != kind:
			continue
		for child in board._tool_bar.get_children():
			if child is Button and not child.is_queued_for_deletion() and child.get_rect().get_center().distance_to(slot[1]) < 1.0:
				return child
	return null

## Timed evidence (run under xvfb with `-- --frames=<dir> --wait=S --step=0.25`): the free rescue is already spent,
## one real move into a dead end, then the pointer lands on the Sapling by itself.
func _play_for_frames(main: Control) -> void:
	main.profile.highest_unlocked = Levels.COUNT
	var board: Control = load("res://game/board_view.gd").new()
	main.set_screen(board)
	board.start(SAPLING_ONLY, main.profile, false)
	board.session.free_rescue_used = true
	await create_timer(0.8).timeout
	var first: Array = _legal(board.session.state)[0]
	board.tap_branch(first[0])
	await create_timer(0.4).timeout
	board.tap_branch(first[1])

func _legal(s: Board) -> Array:
	var moves := []
	for from in s.branch_count():
		for to in s.branch_count():
			if Rules.can_transfer(s, from, to):
				moves.append([from, to])
	return moves

func _run() -> void:
	for arg in OS.get_cmdline_user_args():
		if arg.begins_with("--max-frame-ms="):
			max_frame_ms = arg.trim_prefix("--max-frame-ms=").to_float()
			if max_frame_ms <= 0.0:
				push_error("--max-frame-ms requires a positive millisecond budget")
				quit(1)
				return
	if max_frame_ms < 0.0:
		print("Device frame-rate acceptance NOT RUN; provide --max-frame-ms on the target device. Frame gaps remain logged.")
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
	if main.args.has("frames"):
		await _play_for_frames(main)   # main saves the frames and quits
		return
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
		# A second dead end after the free rescue: the stuck pointer names a tool that is proven to rescue it.
		board.session.try_move(fatal[0], fatal[1])
		await _settle(board)
		var gap := await _stuck_search(board)
		var named: int = board.rescue_tool
		print("RESCUE_TOOL_FRAMES main_ms=%.2f longest_frame_ms=%.1f" % [board.rescue_tool_main_ms, gap])
		check(named != board.NO_RESCUE, "a second dead end names a rescuing tool (%d)" % named)
		check(_pointer_on(board, named), "the pointer stands on the named tool")
		_check_performance(board, gap, "rescue search")
		if named != board.NO_RESCUE:
			var used: bool = board.session.undo() if named < 0 else board.session.use_booster(named).get("applied", false)
			check(used and Solver.hint(board.session.state, [], 24000, 5000)["status"] == Solver.Status.SOLVED, "the named tool's use is proven winnable")
		board.session.restart()
		check(not board.session.free_rescue_used, "restart renews the free rescue")

	# A board that only the Sapling can win: undo does not rescue (the opening cannot be won either), the Sapling does.
	main.profile.highest_unlocked = Levels.COUNT
	var sap := Board.Booster.SAPLING
	var only_sapling: Control = load("res://game/board_view.gd").new()
	main.set_screen(only_sapling)
	only_sapling.start(SAPLING_ONLY, main.profile, false)
	only_sapling.session.free_rescue_used = true
	check(Solver.hint(only_sapling.session.state, [], 60000, 5000)["status"] == Solver.Status.UNSOLVABLE, "the Sapling board cannot be won without it")
	var first: Array = _legal(only_sapling.session.state)[0]
	only_sapling.session.try_move(first[0], first[1])
	await _settle(only_sapling)
	var sap_gap := await _stuck_search(only_sapling)
	print("RESCUE_TOOL_FRAMES main_ms=%.2f longest_frame_ms=%.1f" % [only_sapling.rescue_tool_main_ms, sap_gap])
	_check_performance(only_sapling, sap_gap, "Sapling search")
	check(only_sapling.rescue_tool == sap, "the Sapling is named when only it rescues (%d)" % only_sapling.rescue_tool)
	check(_pointer_on(only_sapling, sap), "the pointer stands on the Sapling")
	# Out of stock: tapping the pointed tool offers one more; in stock: tapping uses it.
	var asked := []
	only_sapling.buy_requested.connect(func(kind: int): asked.append(kind))
	only_sapling.session.state.inventory[sap] = 0
	only_sapling._build_tools()
	_slot_button(only_sapling, sap).pressed.emit()
	check(asked == [sap], "tapping the pointed tool without stock offers one more")
	only_sapling.session.grant_booster(sap, 1)
	only_sapling._build_tools()
	_slot_button(only_sapling, sap).pressed.emit()
	await _settle(only_sapling)
	check(only_sapling.session.state.inventory[sap] == 0 and only_sapling.session.state.added_branch, "tapping the pointed tool with stock uses it")
	check(Solver.hint(only_sapling.session.state, [], 24000, 5000)["status"] == Solver.Status.SOLVED, "the Sapling's use is proven winnable")

	# The free rewind belongs to the first forest; later forests offer the rescuing tool and a free restart.
	for world in [1, 2]:
		var def := SAPLING_ONLY.duplicate(true)
		def["world"] = world
		var gated: Control = load("res://game/board_view.gd").new()
		main.set_screen(gated)
		gated.start(def, main.profile, false)
		var offers := []
		gated.rescue_requested.connect(func(): offers.append("rewind"))
		gated.restart_offered.connect(func(picture: String): offers.append(picture))
		var opening_key: PackedInt32Array = gated.session.state.search_key(false)
		gated.session.try_move(first[0], first[1])
		await _settle(gated)
		var waiting := [true]
		var watch := func():
			await gated._watch_stuck()
			waiting[0] = false
		watch.call()
		var until := Time.get_ticks_msec() + 15000
		while waiting[0] and Time.get_ticks_msec() < until:
			await process_frame
		if world == 1:
			check(offers == ["rewind"], "first forest: the dead end offers the free rewind (%s)" % [offers])
		else:
			var sapling_picture: String = load("res://game/ui.gd").PICTURES[gated.TOOL_ICON_NAME[sap]]
			check(offers == [sapling_picture], "later forest: the dead end names the Sapling and offers restart (%s)" % [offers])
			var settle_until := Time.get_ticks_msec() + 5000
			while not _pointer_on(gated, sap) and Time.get_ticks_msec() < settle_until:
				await process_frame   # a second stuck check (after the background hint) may re-run the search
			check(_pointer_on(gated, sap), "later forest: the pointer stands on the Sapling behind the card")
			var card: Control = load("res://game/stuck_offer.gd").open_restart(main.popups, sapling_picture, func(): gated.restart(), true)
			var again: Button = card.content.get_node("Restart")
			check(again.size == load("res://game/button_kit.gd").WIDE, "restart action keeps the rescue action's size")
			check(again.get_node("Content/Word").painted_id == "white_action_replay", "restart action has the replay word")
			again.pressed.emit()
			await create_timer(0.6).timeout
			check(gated.session.state.search_key(false) == opening_key and gated.session.history_size() == 0, "restart returns to the opening")
			check(gated.session.undos_left == GameSession.FREE_UNDOS, "restart keeps the undo allowance")

	# A long search (the last level, nothing known winnable, every tool unlocked) still leaves every frame short.
	var big: Control = load("res://game/board_view.gd").new()
	main.set_screen(big)
	big.start(Levels.get_level(Levels.COUNT), main.profile, false)
	var opening_move: Array = _legal(big.session.state)[0]
	big.session.try_move(opening_move[0], opening_move[1])
	await _settle(big)
	big._winnable.clear()
	var long_gap := await _stuck_search(big)
	print("RESCUE_TOOL_FRAMES main_ms=%.2f longest_frame_ms=%.1f" % [big.rescue_tool_main_ms, long_gap])
	_check_performance(big, long_gap, "long search")

	# No unlocked tool rescues: none is named and the pointer stays on undo.
	main.profile.highest_unlocked = 1
	var hopeless: Control = load("res://game/board_view.gd").new()
	main.set_screen(hopeless)
	hopeless.start(SAPLING_ONLY, main.profile, false)
	hopeless.session.free_rescue_used = true
	hopeless.session.try_move(first[0], first[1])
	await _settle(hopeless)
	await _stuck_search(hopeless)
	check(hopeless.rescue_tool == hopeless.NO_RESCUE, "a position no tool rescues names none")
	check(_pointer_on(hopeless, -1), "with no rescue the pointer stays on undo")

	var kit: GDScript = load("res://game/button_kit.gd")
	var rescued := [0]
	var offer: Control = load("res://game/stuck_offer.gd").open(main.popups, func(): rescued[0] += 1, true)
	var go: Button = offer.content.get_node("Rescue")
	check(go.size == kit.WIDE, "rescue action retains its accepted width")
	check(absf(go.get_rect().get_center().x - 540.0) < 0.01, "rescue action is centred")
	check(go.position.y >= offer.panel_rect.end.y + 32.0, "rescue action below the approved frame")
	check(offer.content.get_node("TitleGroup").has_node("Sign"), "hanging sign title")
	check(go.has_node("Content/Word") and not go.has_node("Content/Icon") and offer.content.has_node("Picture"), "single picture and explicit rescue label")
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
