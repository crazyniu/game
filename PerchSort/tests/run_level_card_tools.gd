extends SceneTree
## The level card's tool row: the tools that fit the level and the hint, painted stock ("+" when none), a tap opens the
## one-more offer once and the stock refreshes.

var checks := 0
var failures := 0

func check(ok: bool, label: String) -> void:
	checks += 1
	failures += 0 if ok else 1
	print("PASS " if ok else "FAIL ", label)

func _initialize() -> void:
	_run.call_deferred()

func _run() -> void:
	var main: Control = load("res://game/main.tscn").instantiate()
	var fake := Profile.new()
	fake.muted = true
	fake.music_muted = true
	fake.highest_unlocked = Levels.COUNT
	fake.coins = 1000
	main.args["profile"] = JSON.stringify(fake.to_dict())
	main.args["preview"] = "coin_shop_view:normal"
	root.add_child(main)
	main.args["smoke"] = "1"
	main.args["capture"] = "1"
	await process_frame
	var popups: GDScript = load("res://game/popups.gd")
	var board_view: GDScript = load("res://game/board_view.gd")
	var profile: Profile = main.profile
	# A level with eggs and crows.
	var level := 0
	for def in Levels.all():
		var kinds := Progress.fitting_boosters(def)
		if kinds.has(Board.Booster.OWL_EYE) and kinds.has(Board.Booster.SCARECROW):
			level = int(def["id"])
			break
	check(level > 0, "a level with eggs and crows exists: %d" % level)
	profile.inventory = PackedInt32Array([0, 0, 3, 0, 0, 0])
	main.show_level_card(level)
	await process_frame
	var card: Control = main.popups.get_child(main.popups.get_child_count() - 1)
	var row: Control = card.content.get_node_or_null("Tools")
	check(row != null, "the level card has a tool row")
	if row == null:
		quit(1)
		return
	var expected := Progress.fitting_boosters(Levels.get_level(level)) + [Board.Booster.FIREFLY]
	expected = expected.slice(0, 4)
	check(row.get_child_count() == expected.size(), "one slot per fitting tool plus Hint: %d" % row.get_child_count())
	for kind in expected:
		check(row.has_node("Tool_%d" % kind), "slot for tool %d" % kind)
	var owl: Button = row.get_node("Tool_%d" % Board.Booster.OWL_EYE)
	var crow: Button = row.get_node("Tool_%d" % Board.Booster.SCARECROW)
	check(owl.get_node("Stock/Digits").get_meta("value") == "3", "stock shown as a painted number")
	check(crow.get_node("Stock/Digits").get_meta("value") == "+", "zero stock shows +")
	# Same-size slots, inside the card, with at least 16 px between slots and to neighbours.
	var first: Vector2 = row.get_child(0).size
	var same := true
	for slot in row.get_children():
		same = same and slot.size == first and slot.get_node("Slot").size == first
	check(same and first == Vector2(board_view.TOOL_SIZE, board_view.TOOL_SIZE), "slots are the board's tool size")
	var global := Rect2(row.position, row.size)
	check(card.panel_rect.encloses(global), "row inside the card")
	check(absf(global.get_center().x - 540.0) < 0.6, "row centred")
	for i in row.get_child_count() - 1:
		check(row.get_child(i + 1).position.x - row.get_child(i).get_rect().end.x >= 16.0, "slots clear of each other")
	var above: float = 0.0
	for child in card.content.get_children():
		if child == row or not (child is Control) or child.name in ["Close", "Play", "Panel", "TitleGroup"] or child.name.begins_with("action"):
			continue
		var r: Rect2 = child.get_rect()
		if r.end.y <= global.position.y + 1.0 and r.size.x > 0 and r.size.y > 0 and r.end.y > above:
			above = r.end.y
	check(global.position.y - above >= 16.0, "clear of the row above (%.0f px)" % (global.position.y - above))
	var play: Control = card.content.get_node("Play")
	check(play.get_rect().position.y - global.end.y >= 16.0, "clear of Play (%.0f px)" % (play.get_rect().position.y - global.end.y))
	check(play.size == popups.LEVEL_PLAY, "Play keeps the shared wide level-card size")
	# One tap opens the offer once; buying with feathers refreshes the stock.
	var before: int = main.popups.get_child_count()
	crow.pressed.emit()
	await process_frame
	check(main.popups.get_child_count() == before + 1, "a tap opens exactly one offer")
	var offer: Control = main.popups.get_child(main.popups.get_child_count() - 1)
	var coins: int = profile.coins
	offer.content.get_node("Buy").pressed.emit()
	await create_timer(0.8).timeout
	check(profile.inventory[Board.Booster.SCARECROW] == 1 and profile.coins == coins - Progress.booster_price(Board.Booster.SCARECROW), "buying adds one for its price")
	check(crow.get_node("Stock/Digits").get_meta("value") == "1", "the card's stock refreshed")
	check(crow.get_node("Stock").get_child_count() == 1, "old digits removed")
	# Height: the card is exactly one row taller than the same card without tools.
	var plain: Control = popups.level_card(main.popups, level, 0, 0, 9, ["sunny"] as Array[String], [] as Array[String], func(): pass, true)
	var with_row: Control = popups.level_card(main.popups, level, 0, 0, 9, ["sunny"] as Array[String], [] as Array[String], func(): pass, true,
		[Board.Booster.FIREFLY], func(_k): return 1, func(_k, _r): pass)
	var grow: float = with_row.panel_rect.size.y - plain.panel_rect.size.y
	check(absf(grow - (popups.LEVEL_TOOL_ROW + load("res://game/purchase_dialog.gd").GAP)) < 0.6, "the card grows by exactly the row and a gap (%.1f)" % grow)
	check(with_row.content.get_node("Play").size == plain.content.get_node("Play").size, "Play keeps its size with the row")
	# No row on level 1 for a new player; locked tools are not shown.
	for child in main.popups.get_children():
		child.queue_free()
	await process_frame
	var fresh := Profile.new()
	main.profile = fresh
	main.show_level_card(1)
	await process_frame
	var first_card: Control = main.popups.get_child(main.popups.get_child_count() - 1)
	check(not first_card.content.has_node("Tools"), "no row on level 1")
	fresh.highest_unlocked = 2
	for child in main.popups.get_children():
		child.queue_free()
	await process_frame
	main.show_level_card(2)
	await process_frame
	var second: Control = main.popups.get_child(main.popups.get_child_count() - 1)
	var tools: Control = second.content.get_node_or_null("Tools")
	check(tools != null and tools.get_child_count() == 1 and tools.has_node("Tool_%d" % Board.Booster.FIREFLY), "level 2: only the unlocked Hint")
	print("level card tools: %d checks, %d failures" % [checks, failures])
	quit(1 if failures > 0 else 0)
