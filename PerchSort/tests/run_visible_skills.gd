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
	print("visible skills: %d checks, %d failures" % [checks, failures])
	quit(1 if failures else 0)
