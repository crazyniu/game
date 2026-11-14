extends SceneTree
## Pages that stay open over another page follow the profile: the shop balance after a feather purchase, the board's
## house and bird outfits after the shop equips them, and a level's popup queue never outlives its board.
var checks := 0
var failures := 0
var main: Control

func _initialize() -> void:
	_run.call_deferred()

func check(ok: bool, message: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		print("FAIL ", message)

func _fresh(motion := false) -> void:
	if main != null:
		main.queue_free()
		await process_frame
	main = load("res://game/main.tscn").instantiate()
	var fake := Profile.new()
	fake.muted = true
	fake.music_muted = true
	fake.highest_unlocked = Levels.COUNT
	fake.houses["teapot"] = true
	main.args["profile"] = JSON.stringify(fake.to_dict())
	main.args["preview"] = "coin_shop_view:normal"
	if motion:
		main.args["motion"] = "1"
	root.add_child(main)
	await process_frame
	main.args["smoke"] = "1"
	main.args["capture"] = "1"

func _run() -> void:
	await _fresh()
	# Feather purchase: the open shop shows the credited balance.
	main.profile.coins = 0
	main._open_shop(0)
	await process_frame
	var shop: Control = main._overlay
	check(shop != null and shop._coins_shown == 0, "shop opens on a zero balance")
	main.profile.coins += 600
	main._refresh_screen()
	check(shop._coins_shown == 600, "the open shop shows the purchased balance")
	main._close_overlay()
	await process_frame

	# The shop equips a house and an outfit while a level is open.
	main.profile.coins = 5000
	main.profile.outfits["outfit.sunny.1"] = true
	main._open_board(1, false)
	await process_frame
	var board: Control = main.screen
	var before := {}
	for id in board._birds:
		before[id] = board._birds[id].position
	var undos: int = board.session.undos_left
	var moves: int = board.session.move_count()
	var old_house: Control = board._house
	main.profile.equipped_house = "teapot"
	main.profile.equipped["sunny"] = "outfit.sunny.1"
	main._refresh_screen()
	await process_frame
	check(board._house != old_house and board._house_style == "teapot", "the board hangs the equipped house")
	check(board._hud.get_child(2) == board._house, "the new house keeps its place behind the status and tools")
	check(board._house.size.is_equal_approx(board._house_rect().size) and board._door_rect.size.x > 0.0, "house and doorway measured for the new style")
	var worn := 0
	var same: bool = board._birds.size() == before.size()
	for id in board._birds:
		var bird = board._birds[id]
		same = same and before.has(id) and bird.position.is_equal_approx(before[id])
		if bird.species == "sunny" and bird.outfit == "sunny_outfit_1":
			worn += 1
	check(worn > 0, "sunny birds on the board wear the equipped outfit")
	check(same, "birds keep their ids and seats")
	check(board.session.undos_left == undos and board.session.move_count() == moves, "moves and undos are untouched")
	main._refresh_screen()
	check(board._house_style == "teapot", "a second refresh changes nothing")

	# Leaving the level during its entrance: its popups do not open on the page reached.
	await _fresh(true)
	check(not main.instant(), "entrance runs in motion")
	main._open_board(1, true)
	await create_timer(0.3).timeout
	check(main.screen.has_method("tap_branch") and main.screen.busy, "the entrance is playing")
	main._open_home()
	await create_timer(6.0).timeout
	check(main.popups.get_child_count() == 0, "no level popup opens on the home page")
	check(not main.profile.has_tutorial(load("res://game/tutorial.gd").FLAG), "the unseen how-to-play stays due")
	print("board shop refresh checks: %d checks, %d failures" % [checks, failures])
	quit.call_deferred(1 if failures else 0)
