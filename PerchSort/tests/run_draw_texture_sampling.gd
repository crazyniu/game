extends SceneTree
## Selected downscaled art uses mipmaps; other painted words keep their existing sampling.
var checks := 0
var failures := 0
func check(ok: bool, label: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		print("FAIL ", label)
func _initialize() -> void:
	_run.call_deferred()
func _run() -> void:
	var main = load("res://game/main.tscn").instantiate()
	main.args["preview"] = "map_view:cards"
	main.args["capture"] = "1"
	main.args["profile"] = JSON.stringify(Profile.new().to_dict())
	root.add_child(main)
	await process_frame
	main.args["smoke"] = "1"
	var kit = load("res://game/button_kit.gd")
	var art = load("res://game/draw_purchase_art.gd")
	for locale in ["zh", "en"]:
		UiText.set_language(locale)
		var title = kit.lettering(main, "lucky_spin_title", Rect2(0, 0, 480, 117))
		check(title.texture_filter == CanvasItem.TEXTURE_FILTER_LINEAR_WITH_MIPMAPS, locale + " draw heading filters minification")
		check(title.texture.atlas.get_image().has_mipmaps(), locale + " heading imports mipmaps")
		var other = kit.lettering(main, "win_title", Rect2(0, 0, 480, 117))
		check(other.texture_filter == CanvasItem.TEXTURE_FILTER_PARENT_NODE, locale + " unrelated text unchanged")
		title.queue_free()
		other.queue_free()
	var icon = art.fit(main, "icon_draw_prize_four_hundred", Rect2(0, 0, 412, 356))
	var source: Image = icon.texture.get_image()
	check(source.get_width() >= 1024 and source.get_height() >= 1024, "independent source supports result size")
	check(source.has_mipmaps(), "coin source imports mipmaps")
	check(icon.texture_filter == CanvasItem.TEXTURE_FILTER_LINEAR_WITH_MIPMAPS, "coin filters minification")
	check(icon.size.x < source.get_width(), "no enlargement of the cutout")
	icon.queue_free()
	main.queue_free()
	await process_frame
	print("Draw texture sampling: %d checks, %d failures" % [checks, failures])
	quit(1 if failures else 0)
