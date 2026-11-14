extends SceneTree
## Run only with a dedicated XDG_DATA_HOME and explicit isolation marker.
var failures := 0

func _initialize() -> void:
	run.call_deferred()

func check(value: bool, label: String) -> void:
	print("PASS " if value else "FAIL ", label)
	if not value:
		failures += 1

func write(path: String, content: String) -> void:
	var file := FileAccess.open(path, FileAccess.WRITE)
	file.store_string(content)

func remove_fixtures() -> void:
	for suffix in ["", ".bak", ".tmp"]:
		var path: String = Profile.SAVE_PATH + suffix
		if FileAccess.file_exists(path):
			DirAccess.remove_absolute(path)

func start():
	var main = load("res://game/main.tscn").instantiate()
	root.add_child(main)
	current_scene = main
	return main

func dispose(main) -> void:
	root.remove_child(main)
	main.free()
	current_scene = null

func run() -> void:
	var isolated := OS.get_environment("PERCH_RECOVERY_TEST")
	if isolated.is_empty() or not OS.get_user_data_dir().begins_with(isolated + "/"):
		push_error("Refusing to touch a non-isolated profile")
		quit(2)
		return
	remove_fixtures()
	write(Profile.SAVE_PATH, "{broken")
	write(Profile.SAVE_PATH + ".bak", "not-json")
	var main = start()
	check(main.profile.needs_recovery() and not main._booted and main.screen == null, "damaged pair blocks Home and boot")
	var popup = main.popups.get_node("SaveRecovery")
	check(popup.content.has_node("Retry") and popup.content.has_node("Exit") and not popup.content.has_node("Close"), "only retry and exit controls")
	for action in ["Retry", "Exit"]:
		var button = popup.content.get_node(action)
		var label = button.get_node("Content/PaintedWord")
		check(button.size == Vector2(420, 150), action + " uses the widened shared action size")
		check(button.position.y >= popup.panel_rect.end.y + 40, action + " sits outside the panel")
		check((label.position + label.size * 0.5).distance_to(button.size * 0.5) < 0.1, action + " lettering ink centered on both axes")
	check(popup.content.get_node("Exit").position.x - popup.content.get_node("Retry").get_rect().end.x >= 48, "action gap remains at least 48 pixels")
	if OS.get_environment("PERCH_RECOVERY_EXIT") == "1":
		print("save recovery exit requested")
		popup.content.get_node("Exit").pressed.emit()
		return
	popup.content.get_node("Retry").pressed.emit()
	await process_frame
	await process_frame
	main = current_scene
	check(main.profile.needs_recovery() and main.screen == null, "failed retry remains blocked")
	check(FileAccess.get_file_as_string(Profile.SAVE_PATH) == "{broken" and FileAccess.get_file_as_string(Profile.SAVE_PATH + ".bak") == "not-json", "failed retry preserves both bytes")
	var valid := Profile.new()
	valid.coins = 147
	valid.last_daily_gift = Progress.today()
	write(Profile.SAVE_PATH + ".bak", JSON.stringify(valid.to_dict()))
	main.popups.get_node("SaveRecovery").content.get_node("Retry").pressed.emit()
	await process_frame
	await process_frame
	main = current_scene
	check(not main.profile.needs_recovery() and main._booted and main.screen != null and main.profile.coins == 147, "repaired backup retry enters Home with restored balance")
	check(FileAccess.get_file_as_string(Profile.SAVE_PATH) == "{broken", "successful load does not silently rewrite primary")
	dispose(main)
	remove_fixtures()
	write(Profile.SAVE_PATH + ".bak", "bad backup")
	main = start()
	check(main.profile.needs_recovery() and main.screen == null, "missing primary plus bad backup blocks")
	dispose(main)
	remove_fixtures()
	main = start()
	check(not main.profile.needs_recovery() and main.screen != null, "genuine first launch remains normal")
	dispose(main)
	remove_fixtures()
	valid.save()
	main = start()
	check(not main.profile.needs_recovery() and main.screen != null and main.profile.coins == 147, "valid primary remains normal")
	dispose(main)
	remove_fixtures()
	print("save recovery UI checks: %d failures" % failures)
	quit(1 if failures else 0)
