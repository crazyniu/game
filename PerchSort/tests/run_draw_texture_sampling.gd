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
	var popups = load("res://game/popups.gd")
	var draw = popups.lucky_draw(main.popups, 3, 5, func(): return {})
	var selected := 0
	for i in 8:
		var tile = draw.find_child("Prize%d" % i, true, false)
		var image = tile.get_node("PrizeArt")
		var is_selected: bool = popups.DRAW_PRIZE_KEYS[i] == "icon_draw_prize_four_hundred"
		selected += int(is_selected)
		check(image.texture_filter == (CanvasItem.TEXTURE_FILTER_LINEAR_WITH_MIPMAPS if is_selected else CanvasItem.TEXTURE_FILTER_PARENT_NODE), "actual draw tile %d uses only its intended filter" % i)
	check(selected == 1, "main draw exercises the high-resolution prize")
	draw.queue_free()
	await process_frame
	var daily = popups.daily_reward(main.popups, 7, true, 100, func(): return {})
	for day in [1, 7]:
		var tile = daily.find_child("Day%d" % day, true, false)
		var image = tile.get_node("Body/Prize0")
		check(image.texture_filter == (CanvasItem.TEXTURE_FILTER_LINEAR_WITH_MIPMAPS if day == 7 else CanvasItem.TEXTURE_FILTER_PARENT_NODE), "actual daily day %d uses only its intended filter" % day)
	daily.queue_free()
	await process_frame
	main.queue_free()
	await process_frame
	print("Draw texture sampling: %d checks, %d failures" % [checks, failures])
	quit(1 if failures else 0)
