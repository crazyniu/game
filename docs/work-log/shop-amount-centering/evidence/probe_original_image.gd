extends SceneTree
func _initialize() -> void: call_deferred("run")
func run() -> void:
	var art: GDScript = load("res://game/art.gd")
	var source := GDScript.new()
	source.source_code = "extends RefCounted\nstatic var _visible_ink := {}\nstatic func _texture_ink(texture: Texture2D, soften := false) -> Rect2:\n\tvar key := str(texture.get_instance_id()) + (\"soft\" if soften else \"painted\")\n\tif not _visible_ink.has(key):\n\t\tvar image := texture.get_image()\n\t\tif soften:\n\t\t\tfor y in image.get_height():\n\t\t\t\tfor x in image.get_width():\n\t\t\t\t\tvar pixel := image.get_pixel(x, y)\n\t\t\t\t\tvar light := pixel.r * 0.299 + pixel.g * 0.587 + pixel.b * 0.114\n\t\t\t\t\tpixel.a *= smoothstep(0.48, 0.64, light)\n\t\t\t\t\timage.set_pixel(x, y, pixel)\n\t\t_visible_ink[key] = Rect2(image.get_used_rect())\n\treturn _visible_ink[key]\n\n"
	source.reload()
	var texture: Texture2D = art.tex("icon_home_island_digit_0")
	var first := texture.get_image()
	print("before: ",first.get_instance_id()," ",hash(first.get_data())," ",first.get_used_rect())
	print("soft: ",source._texture_ink(texture,true))
	var second := texture.get_image()
	print("after: ",second.get_instance_id()," ",hash(second.get_data())," ",second.get_used_rect())
	quit()
