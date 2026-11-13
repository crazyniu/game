extends SceneTree
## Currency amounts must retain exact values while icon and compact numerals center together.
var checks := 0
var failures := 0
func _initialize() -> void:
	_run.call_deferred()
func check(ok: bool, title: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		push_error(title)
func _run() -> void:
	var skin = load("res://game/shop_skin.gd")
	var art = load("res://game/art.gd")
	for size in [Vector2(212,58), Vector2(240,56), Vector2(432,76)]:
		for value in [0,1,30,600,1000,1300,99999,1234567]:
			var host := Control.new()
			root.add_child(host)
			var group: Control = skin.feather_amount(host, Rect2(Vector2.ZERO,size),value)
			var feather: TextureRect = group.get_node("Feather")
			check(feather.texture == art.tex("icon_purchase_feather"),"Standalone golden feather")
			var source := Rect2(feather.texture.get_image().get_used_rect())
			var k := feather.size/feather.texture.get_size()
			var bounds := Rect2(feather.position+source.position*k,source.size*k)
			var text := ""
			var digits: Control = group.get_node("Digits")
			for glyph in digits.get_children():
				text += glyph.texture.resource_path.get_file().get_basename().right(1)
				source = Rect2(glyph.texture.get_image().get_used_rect())
				k = glyph.size/glyph.texture.get_size()
				bounds = bounds.merge(Rect2(digits.position+glyph.position+source.position*k,source.size*k))
			check(text==str(value),"Exact amount")
			check(bounds.get_center().distance_to(size*0.5)<1.0,"Visible group centered")
			check(Rect2(Vector2.ZERO,size).grow(1).encloses(bounds),"Visible group fits")
			host.free()
	print("feather prices: %d checks, %d failures" % [checks,failures])
	quit(1 if failures else 0)
