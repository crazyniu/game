extends SceneTree
## QA-only observer: F9 records the canonical game, never a production layout substitute.
class Evidence extends Node:
	var main: Control
	var folder: String
	var sequence := 0
	var busy := false
	func _input(event: InputEvent) -> void:
		if event is InputEventKey and event.pressed and event.keycode == KEY_F9 and not busy:
			_record.call_deferred()
	func _record() -> void:
		busy = true
		await RenderingServer.frame_post_draw
		sequence += 1
		var image := get_viewport().get_texture().get_image()
		var filename := "native_%02d.png" % sequence
		image.save_png(folder.path_join(filename))
		var state := {"file": filename, "size": [image.get_width(), image.get_height()],
			"time_ms": Time.get_ticks_msec(), "coins": main.profile.coins,
			"screen": main.screen.get_script().resource_path,
			"popups": main.popups.get_child_count()}
		print("NATIVE_EVIDENCE ", JSON.stringify(state))
		busy = false
func _initialize() -> void:
	_start.call_deferred()
func _start() -> void:
	root.title = "Combined non-gameplay UI review"
	var main: Control = load("res://game/main.tscn").instantiate()
	root.add_child(main)
	var observer := Evidence.new()
	observer.main = main
	observer.folder = ProjectSettings.globalize_path("res://../build/non-gameplay-integration/gui/native-return")
	DirAccess.make_dir_recursive_absolute(observer.folder)
	main.add_child(observer)
