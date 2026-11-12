extends SceneTree
## Full-resolution renders of the canonical previews, using the real GL renderer.

const STATES := ["win_three_stars", "win_two_stars", "win_one_star", "win_zero_reward", "win_milestone",
	"level_card_first_visit", "level_card_full_roster", "level_card_locked", "new_friend_berry", "unlock_owl_eye",
	"unlock_sapling", "world_complete_1", "world_unlock_2", "world_unlock_4", "rewards_challenge",
	"rewards_challenge_empty", "rewards_challenge_all", "unlock_firefly", "unlock_morning_bell", "unlock_scarecrow", "unlock_spring_rain"]
var main: Control
var output: String
var surfaces: Array = []

func _initialize() -> void:
	_run.call_deferred()

func _capture(path: String) -> void:
	await RenderingServer.frame_post_draw
	var image := root.get_texture().get_image()
	assert(image.get_size() == Vector2i(1080, 1920), "Evidence must be rendered at full design resolution")
	assert(image.save_png(path) == OK)
	if path.contains("/screens/"):
		var panel := main.find_child("Panel", true, false)
		if panel != null and panel.has_node("Skin"):
			var skin: NinePatchRect = panel.get_node("Skin")
			surfaces.append({"file": path.get_file(), "bounds": [panel.position.x, panel.position.y, panel.size.x, panel.size.y],
				"corner_scale": skin.scale.x, "margins": [skin.patch_margin_left, skin.patch_margin_top, skin.patch_margin_right, skin.patch_margin_bottom],
				"modulation": [skin.self_modulate.r, skin.self_modulate.g, skin.self_modulate.b, skin.self_modulate.a]})


func _run() -> void:
	root.content_scale_size = Vector2i(1080, 1920)
	root.content_scale_mode = Window.CONTENT_SCALE_MODE_VIEWPORT
	root.content_scale_aspect = Window.CONTENT_SCALE_ASPECT_KEEP
	output = ProjectSettings.globalize_path("res://../build/dialog-layout")
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--family-output="):
			output = argument.trim_prefix("--family-output=")
	DirAccess.make_dir_recursive_absolute(output + "/screens")
	main = load("res://game/main.tscn").instantiate()
	var fake := Profile.new()
	fake.muted = true
	fake.music_muted = true
	fake.highest_unlocked = Levels.COUNT
	for species in Board.SPECIES:
		fake.met[species] = true
	fake.set_tutorial("tutorial.how_to_play")
	for info in Progress.BOOSTERS:
		fake.set_tutorial("unlock.%s" % info[1])
	main.args["profile"] = JSON.stringify(fake.to_dict())
	main.args["preview"] = "map_view:cards"
	root.add_child(main)
	main.args["smoke"] = "1"
	main.args["capture"] = "1"
	var popups: GDScript = load("res://game/popups.gd")
	var filter := str(main.args.get("family-filter", ""))
	var mode := str(main.args.get("family-mode", "all"))
	for state in STATES if mode != "motion" else []:
		if not filter.is_empty() and (state != filter if main.args.has("family-exact") else not state.begins_with(filter)):
			continue
		for language in ["en", "zh"]:
			UiText.set_language(language)
			if state in ["unlock_firefly", "unlock_morning_bell", "unlock_scarecrow", "unlock_spring_rain"]:
				main._clear_popups()
				var booster := {"unlock_firefly": 0, "unlock_morning_bell": 3, "unlock_scarecrow": 4, "unlock_spring_rain": 5}
				popups.unlock(main.popups, booster[state], func(): pass)
			else:
				popups.preview(main, state)
			await create_timer(0.25).timeout
			await _capture(output + "/screens/%s_%s.png" % [state, language])
			print("CAPTURE ", state, " ", language, " 1080x1920")
	# Canonical reference pages are captured in the same session, renderer and locale.
	if mode != "motion" and filter in ["", "reference"]:
		for reference in [["shop_view", "boosters"], ["coin_shop_view", "normal"], ["coin_shop_view", "purchase_preview"], ["popups", "chest_ready"], ["aviary_view", "detail_sunny"]]:
			UiText.set_language("en")
			main._clear_popups()
			load("res://game/%s.gd" % reference[0]).preview(main, reference[1])
			await create_timer(0.25).timeout
			await _capture(output + "/screens/reference_%s_%s.png" % [reference[0], reference[1]])
			print("REFERENCE ", reference, " 1080x1920")
	main.args["motion"] = "1"
	UiText.set_language("en")
	for state in ["win_three_stars", "new_friend_berry", "world_unlock_2"] if mode != "still" else []:
		if not filter.is_empty() and (state != filter if main.args.has("family-exact") else not state.begins_with(filter)):
			continue
		popups.preview(main, state)
		var directory: String = output + "/motion/" + state
		DirAccess.make_dir_recursive_absolute(directory)
		var start := Time.get_ticks_msec()
		var times: Array = []
		for i in 16:
			await _capture(directory + "/f%03d.png" % i)
			times.append(Time.get_ticks_msec() - start)
			await create_timer(0.25).timeout
		var file := FileAccess.open(directory + "/times.json", FileAccess.WRITE)
		file.store_string(JSON.stringify(times))
		print("MOTION ", state, " ", times)
	var sound := root.get_node("Sound")
	sound.set_music_muted(true)
	for child in sound.get_children():
		if child is AudioStreamPlayer:
			child.stop()
	main.queue_free()
	await process_frame
	var suffix := "" if filter.is_empty() else "-" + filter + "-" + mode
	var surface_file := FileAccess.open(output + "/surfaces" + suffix + ".json", FileAccess.WRITE)
	surface_file.store_string(JSON.stringify(surfaces, "\t"))
	print("FAMILY CAPTURE COMPLETE")
	quit()
