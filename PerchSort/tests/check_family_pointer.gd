extends SceneTree
## Canonical controls driven by actual desktop pointer input; fake data never saves.
var calls := 0

func _initialize() -> void:
	_run.call_deferred()

func _run() -> void:
	root.content_scale_size = Vector2i(1080, 1920)
	root.content_scale_mode = Window.CONTENT_SCALE_MODE_VIEWPORT
	var main: Control = load("res://game/main.tscn").instantiate()
	var fake := Profile.new()
	fake.muted = true
	fake.music_muted = true
	main.args["profile"] = JSON.stringify(fake.to_dict())
	main.args["preview"] = "map_view:cards"
	root.add_child(main)
	main.args["smoke"] = "1"
	main.args["capture"] = "1"
	UiText.set_language("en")
	var popups: GDScript = load("res://game/popups.gd")
	var species: Array[String] = ["sunny", "sky", "berry"]
	var mechanics: Array[String] = ["sleep", "egg"]
	var dialog: Control = popups.level_card(main.popups, 12, 0, 0, 9, species, mechanics, func(): calls += 100, false)
	print("POINTER READY: disabled Play then Close")
	await dialog.closed
	assert(calls == 0)
	print("POINTER PASS: disabled Play and Close preserve callback count")
	await process_frame
	for action in ["Home", "Retry", "action_next"]:
		dialog = popups.win(main.popups, 3, 12, 12, 120, [0, 3],
			func(): calls += 1, func(): calls += 10, func(): calls += 100, true)
		print("POINTER READY: click ", action)
		await dialog.closed
		await process_frame
		print("POINTER PASS: ", action, " callbacks=", calls)
	assert(calls == 111)
	dialog = popups.world_unlock(main.popups, 2, func(): calls += 1000)
	print("POINTER READY: world Play")
	await dialog.closed
	assert(calls == 1111)
	print("POINTER PASS: world Play; all callbacks exact; preserved balance=", main.profile.coins)
	main.queue_free()
	await process_frame
	quit()
