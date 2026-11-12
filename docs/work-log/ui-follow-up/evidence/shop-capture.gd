extends "res://tests/run_shop_cards_gui.gd"
## QA-only design-resolution viewport, leaving production project settings unchanged.
func _initialize() -> void:
	root.content_scale_size = Vector2i(1080, 1920)
	root.content_scale_mode = Window.CONTENT_SCALE_MODE_VIEWPORT
	root.content_scale_aspect = Window.CONTENT_SCALE_ASPECT_KEEP
	super._initialize()
