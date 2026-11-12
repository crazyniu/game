extends SceneTree
## QA-only full-resolution target. Does not change production window/project settings.
func _initialize() -> void:
	_start.call_deferred()

func _start() -> void:
	root.content_scale_size = Vector2i(1080, 1920)
	root.content_scale_mode = Window.CONTENT_SCALE_MODE_VIEWPORT
	root.content_scale_aspect = Window.CONTENT_SCALE_ASPECT_KEEP
	root.add_child(load("res://game/main.tscn").instantiate())
