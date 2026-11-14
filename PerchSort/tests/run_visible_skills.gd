extends SceneTree
## Visible skills: the bird card shows "four go home" before the effect; a level-card ring and a finger held on a
## bird open that card; a homecoming badge is aimed at what it changes.
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
	# The card: trigger row (four faces, arrow, house) above the ability row, inside the panel, not overlapping.
	var card: Control = popups.goal_info(main.popups, "berry")
	var trigger: Control = card.content.get_node_or_null("Trigger")
	check(trigger != null, "a bird with an effect shows the trigger row")
	if trigger != null:
		var house: Control = trigger.get_node("House")
		var ability: Control = card.content.get_node("Ability")
		check(trigger.has_node("Face0") and trigger.has_node("Face3") and trigger.has_node("Arrow"), "four faces and an arrow")
		check(card.panel_rect.encloses(house.get_rect()) and card.panel_rect.encloses(trigger.get_node("Face0").get_rect()), "trigger row inside the card")
		check(house.get_rect().end.y <= ability.get_rect().position.y, "trigger row clear of the ability row")
		var left: float = trigger.get_node("Face0").get_rect().position.x
		check(absf((left + house.get_rect().end.x) * 0.5 - 540.0) < 0.6, "trigger row centred")
	card.queue_free()
	var crow: Control = popups.goal_info(main.popups, "crow")
	check(crow.content.get_node_or_null("Trigger") == null, "a bird without an effect has no trigger row")
	crow.queue_free()
	await process_frame
	# A level-card ring opens the card.
	var species: Array[String] = ["sunny", "berry"]
	var none: Array[String] = []
	var level: Control = popups.level_card(main.popups, 3, 0, 0, 6, species, none, func(): pass, true)
	var ring: Button = level.content.get_node_or_null("Bird_berry")
	check(ring != null, "level card rings are tappable")
	var before: int = main.popups.get_child_count()
	if ring != null:
		ring.pressed.emit()
		await process_frame
		check(main.popups.get_child_count() == before + 1, "a ring opens the bird card over the level card")
	for child in main.popups.get_children():
		child.queue_free()
	await process_frame
	# Holding a bird asks for its card; an egg and a lifted finger do not.
	var board: Control = load("res://game/board_view.gd").new()
	main.set_screen(board)
	board.start({"id": 1, "capacity": 4, "par": 3, "world": 1, "variant": "noon", "branches": [
		{"side": "L", "row": 0, "birds": ["sunny", "sunny", "sunny"]},
		{"side": "R", "row": 0, "birds": ["berry:sleep", "sunny"]},
		{"side": "L", "row": 1, "birds": ["egg:berry", "berry:sleep"]},
		{"side": "R", "row": 1, "birds": ["berry"]}]}, main.profile, false)
	var asked := []
	board.bird_info_requested.connect(func(sp: String): asked.append(sp))
	var s: Board = board.session.state
	var sunny: Node2D = board._birds[Board.id_of(s.items[0][2])]
	var egg: Node2D = board._birds[Board.id_of(s.items[2][0])]
	var at := func(bird: Node2D) -> Vector2: return bird.global_position - board.global_position + Vector2(0, -60)
	check(board.bird_at(at.call(sunny)) == sunny, "the bird under the finger is found")
	board._begin_hold(at.call(sunny))
	await create_timer(board.HOLD_SECONDS + 0.15).timeout
	check(asked == ["sunny"], "a held bird asks for its card: %s" % [asked])
	board._begin_hold(at.call(egg))
	await create_timer(board.HOLD_SECONDS + 0.15).timeout
	check(asked.size() == 1, "an egg keeps its secret")
	board._begin_hold(at.call(sunny))
	board._hold_id += 1   # the finger lifted or moved
	await create_timer(board.HOLD_SECONDS + 0.15).timeout
	check(asked.size() == 1, "a short tap opens nothing")
	# The row of bird keys under the house: one per kind, centred, clear of the house and of the top birds; a tap
	# asks for that bird's card; a flock at home gets its check.
	var row: Control = board._skill_row
	check(row.get_child_count() == 2 and row.has_node("Key_sunny") and row.has_node("Key_berry"), "one key per kind of bird")
	check(absf(row.get_rect().get_center().x - 540.0) < 0.6, "the row is centred")
	check(row.get_rect().position.y >= board._house_rect().end.y + 8.0, "the row hangs clear of the house")
	var top_bird := 1e9
	for bird in board._birds.values():
		top_bird = minf(top_bird, bird.global_position.y - board.global_position.y - 165.0 * board.BIRD_SCALE)
	check(row.get_rect().end.y + 8.0 <= top_bird, "the row is clear of the highest birds: %.0f under %.0f" % [top_bird, row.get_rect().end.y])
	check(row.get_node("Key_sunny").has_node("Badge") and not row.get_node("Key_sunny/Home").visible, "a key shows the badge, no check yet")
	row.get_node("Key_berry/Tap").pressed.emit()
	check(asked == ["sunny", "berry"], "a tapped key asks for its bird's card: %s" % [asked])
	# The sunny flock goes home: its wake badge is aimed at both sleepers.
	check(board.session.try_move(1, 0), "the flock completes")
	var gift := {}
	for e in board.session.events:
		if e["type"] == "gift":
			gift = e
	check(not gift.is_empty() and gift["species"] == "sunny", "the homecoming has a gift")
	if not gift.is_empty():
		var reach: Array = board._gift_targets(board.session.events, gift)
		check(reach.size() == 2, "the badge reaches both sleepers: %d" % reach.size())
	board.refresh_hud()
	check(row.get_node("Key_sunny/Home").visible and not row.get_node("Key_berry/Home").visible, "the flock at home gets its check")
	# Flurry's gift: one more undo at once, aimed at the undo slot; a flock that goes home again pays nothing.
	var snow: Control = load("res://game/board_view.gd").new()
	main.set_screen(snow)
	snow.start({"id": 1, "capacity": 4, "par": 3, "world": 1, "variant": "noon", "branches": [
		{"side": "L", "row": 0, "birds": ["snow", "snow", "snow"]},
		{"side": "R", "row": 0, "birds": ["snow"]},
		{"side": "L", "row": 1, "birds": ["sunny", "sunny", "sunny", "sky"]},
		{"side": "R", "row": 1, "birds": ["sky", "sky", "sky", "sunny"]},
		{"side": "L", "row": 2, "birds": []}]}, main.profile, false)
	var undos: int = snow.session.undos_left
	check(snow.session.try_move(2, 4), "a plain move first")
	snow.refresh_hud()
	check(snow.session.try_move(1, 0) and snow.session.undos_left == undos + 1, "the snow flock goes home and adds one undo")
	var snow_gift := {}
	for e in snow.session.events:
		if e["type"] == "gift":
			snow_gift = e
	var undo_slot: Control = snow._tool_bar.get_node_or_null("Undo")
	check(undo_slot != null, "the undo slot is on the bar")
	if not snow_gift.is_empty() and undo_slot != null:
		var reach: Array = snow._gift_targets(snow.session.events, snow_gift)
		var centre: Vector2 = undo_slot.get_global_rect().get_center() - snow.global_position
		check(reach.size() == 1 and reach[0].distance_to(centre) < 1.0, "the badge flies to the undo slot")
	check(snow.session.undo() and snow.session.undos_left == undos, "undo spends one")
	check(snow.session.try_move(1, 0) and snow.session.undos_left == undos, "going home again pays nothing")
	snow.restart()
	check(snow.session.try_move(1, 0) and snow.session.undos_left == undos, "nor after a restart")
	# Berry's (and Inky's) gift: the homing move is taken off the count at once, aimed at the move counter.
	for kind in ["berry", "ink"]:
		var refund: Control = load("res://game/board_view.gd").new()
		main.set_screen(refund)
		refund.start({"id": 1, "capacity": 4, "par": 3, "world": 1, "variant": "noon", "branches": [
			{"side": "L", "row": 0, "birds": [kind, kind, kind]},
			{"side": "R", "row": 0, "birds": [kind]},
			{"side": "L", "row": 1, "birds": ["sunny", "sunny", "sunny", "sky"]},
			{"side": "R", "row": 1, "birds": ["sky", "sky", "sky", "sunny"]},
			{"side": "L", "row": 2, "birds": []}]}, main.profile, false)
		check(refund.session.try_move(2, 4) and refund.session.move_count() == 1, "%s: a plain move counts" % kind)
		check(refund.session.try_move(1, 0) and refund.session.move_count() == 1, "%s: the homing move is refunded" % kind)
		var refund_gift := {}
		for e in refund.session.events:
			if e["type"] == "gift":
				refund_gift = e
		var reach: Array = refund._gift_targets(refund.session.events, refund_gift)
		var counter: Vector2 = refund._moves_icon.get_global_rect().get_center() - refund.global_position
		check(reach.size() == 1 and reach[0].distance_to(counter) < 1.0, "%s: the badge flies to the move counter" % kind)
		check(refund.session.try_move(3, 2) and refund.session.move_count() == 2, "%s: the next move counts again" % kind)
	# The first pointer at the bird keys: once, after the popups are gone, stopped by any tap.
	var fresh := Profile.new()
	var pointed: Control = load("res://game/board_view.gd").new()
	main.set_screen(pointed)
	pointed.start(Levels.get_level(1), fresh, false)
	pointed._tick_keys_pointer(0.4)
	check(pointed._keys_hand == null, "the pointer waits a moment")
	pointed._tick_keys_pointer(0.7)
	check(is_instance_valid(pointed._keys_hand) and fresh.has_tutorial("keys.shown"), "the pointer shows and is remembered")
	var tap := InputEventMouseButton.new()
	tap.pressed = true
	pointed._input(tap)
	check(pointed._keys_hand == null, "any tap stops the pointer")
	pointed._tick_keys_pointer(2.0)
	check(pointed._keys_hand == null, "it does not come back")
	print("visible skills: %d checks, %d failures" % [checks, failures])
	quit(1 if failures else 0)
