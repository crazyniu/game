extends SceneTree
func _initialize() -> void: call_deferred("run")
func run() -> void:
	var art: GDScript = load("res://game/art.gd")
	var av: GDScript = load("res://game/aviary_view.gd")
	var texture: Texture2D = art.tex("icon_home_island_digit_0")
	var first := texture.get_image()
	print("before: ",first.get_instance_id()," ",hash(first.get_data())," ",first.get_used_rect())
	print("soft: ",av._texture_ink(texture,true))
	var second := texture.get_image()
	print("after: ",second.get_instance_id()," ",hash(second.get_data())," ",second.get_used_rect())
	quit()
