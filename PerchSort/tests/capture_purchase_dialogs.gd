extends SceneTree
## Render canonical previews at their full design viewport on a smaller test desktop.

func _initialize() -> void:
	root.content_scale_size = Vector2i(1080, 1920)
	root.content_scale_mode = Window.CONTENT_SCALE_MODE_VIEWPORT
	_start.call_deferred()

func _start() -> void:
	root.add_child(load("res://game/main.tscn").instantiate())
