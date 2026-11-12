extends SceneTree
## Native desktop clicks on canonical markers; synthetic preview profile never saves.
var tapped: Array[int] = []

func _initialize() -> void:
	run.call_deferred()

func run() -> void:
	root.content_scale_size = Vector2i(1080, 1920)
	root.content_scale_mode = Window.CONTENT_SCALE_MODE_VIEWPORT
	root.content_scale_aspect = Window.CONTENT_SCALE_ASPECT_KEEP
	var main: Control = load("res://game/main.tscn").instantiate()
	var profile := Profile.new()
	profile.highest_unlocked = 8
	profile.music_muted = true
	profile.muted = true
	for level in range(1, 8):
		profile.stars[level] = 2
	main.args["profile"] = JSON.stringify(profile.to_dict())
	main.args["preview"] = "map_view:world_page"
	root.add_child(main)
	main.args["smoke"] = "1"
	var map: Control = main.screen
	map.level_tapped.connect(func(level: int):
		tapped.append(level)
		main.show_level_card(level))
	print("POINTER READY: click locked9, then current8")
	await main.popups.child_entered_tree
	await process_frame
	assert(tapped == [8])
	print("POINTER PASS: locked9 ignored, current8 opened exactly once; close card")
	await main.popups.get_child(0).closed
	print("POINTER READY: click cleared7")
	await main.popups.child_entered_tree
	await process_frame
	assert(tapped == [8, 7])
	print("POINTER PASS: cleared7 opens exactly once; close then Back")
	await main.popups.get_child(0).closed
	while map._page != null:
		await process_frame
	assert(map._world == 1 and map._play_button.visible)
	assert(main.profile.highest_unlocked == 8)
	print("POINTER PASS: Back restores selector, profile unchanged")
	main.queue_free()
	await process_frame
	quit()
