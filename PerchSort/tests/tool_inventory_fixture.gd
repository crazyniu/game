extends "res://game/board_view.gd"
## Layout-only fixture; all callbacks remain canonical.
func _visible_tools() -> Array:
	return [-1, 0, 1, 2, 3, 4, 5]
