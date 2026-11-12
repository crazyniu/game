extends SceneTree
## Measure complete visible numeral unions on canonical Shop/purchase views, without using their centering helper.

const STATES := ["boosters", "outfits", "outfits_some_unmet", "bird_outfits", "boosters_empty", "boosters_maximum", "locked_tools", "houses",
	"confirm_booster", "confirm_multiple", "confirm_maximum", "insufficient",
	"confirm_nest", "confirm_nest_short", "confirm_outfit", "confirm_outfit_short"]
var checks := 0
var failures := 0
var main: Control
var evidence := ""
var measurements: Array = []
var island_script: GDScript

func _initialize() -> void:
	root.content_scale_size = Vector2i(1080, 1920)
	root.content_scale_mode = Window.CONTENT_SCALE_MODE_VIEWPORT
	_run.call_deferred()

func check(ok: bool, label: String) -> void:
	checks += 1
	failures += 0 if ok else 1
	print("PASS " if ok else "FAIL ", label)

func _ink(digits: Control) -> Rect2:
	var union := Rect2()
	for glyph: TextureRect in digits.get_children():
		var source := Rect2(glyph.texture.get_image().get_used_rect())
		var ratio := glyph.size / glyph.texture.get_size()
		var ink := Rect2(glyph.position + source.position * ratio, source.size * ratio)
		union = ink if not union.has_area() else union.merge(ink)
	return union

func _measure(digits: Control, label: String, owner: Control = null) -> void:
	var union := _ink(digits)
	var offset := union.get_center() - digits.size * 0.5
	check(offset.length() < 0.01, label + " visible union centered")
	check(Rect2(Vector2.ZERO, digits.size).encloses(union), label + " ink fits unchanged box")
	var text := ""
	for glyph: TextureRect in digits.get_children():
		text += glyph.texture.resource_path.get_file().get_basename().trim_prefix(island_script.DIGIT)
	var fixture := Control.new()
	var original: Control = island_script.digits(fixture, text, Rect2(Vector2.ZERO, digits.size))
	var unchanged := original.get_child_count() == digits.get_child_count()
	var shift: Vector2 = digits.get_child(0).position - original.get_child(0).position
	for i in original.get_child_count():
		var current: TextureRect = digits.get_child(i)
		var before: TextureRect = original.get_child(i)
		unchanged = unchanged and current.texture == before.texture and current.size.distance_to(before.size) < 0.001
		unchanged = unchanged and current.material == before.material and current.self_modulate == before.self_modulate
		unchanged = unchanged and (current.position - before.position - shift).length() < 0.001
	check(unchanged, label + " only common translation; art, material, size and spacing preserved")
	if owner != null:
		var center := digits.position + union.get_center()
		check(center.distance_to(owner.size * 0.5) < 0.01, label + " complete amount centered in owning face")
	measurements.append({"node": label, "text": text, "offset": [offset.x, offset.y],
		"translation": [shift.x, shift.y], "box": [digits.size.x, digits.size.y]})
	fixture.free()

func _shop_amounts(shop: Control, label: String) -> void:
	_measure(shop._coins.get_node("Digits").get_child(0), label + " balance", shop._coins.get_node("Digits"))
	for card in shop._grid.get_children():
		var price: Control = card.get_node_or_null("Body/Price")
		if price != null:
			_measure(price.get_node("Digits"), label + " " + str(card.name) + " price", price)
		var stock: Control = card.get_node_or_null("Body/Stock")
		if stock != null:
			_measure(stock.get_node("Digits"), label + " " + str(card.name) + " stock", stock)

func _run() -> void:
	for arg in OS.get_cmdline_user_args():
		if arg.begins_with("--amount-evidence="):
			evidence = arg.trim_prefix("--amount-evidence=")
	if not evidence.is_empty():
		DirAccess.make_dir_recursive_absolute(evidence)
	main = load("res://game/main.tscn").instantiate()
	var initial := Profile.new()
	initial.muted = true
	initial.music_muted = true
	main.args["profile"] = JSON.stringify(initial.to_dict())
	main.args["preview"] = "shop_view:boosters"
	root.add_child(main)
	main.args["smoke"] = "1"
	main.args["capture"] = "1"
	await process_frame
	island_script = load("res://game/island.gd")
	var art_script: GDScript = load("res://game/art.gd")
	var digit_hashes := {}
	for digit in "0123456789":
		digit_hashes[digit] = hash(art_script.tex(island_script.DIGIT + digit).get_image().get_data())
	var shop_script: GDScript = load("res://game/shop_view.gd")
	for language in ["en", "zh"]:
		UiText.set_language(language)
		for state in STATES:
			shop_script.preview(main, state)
			await process_frame
			if state.ends_with("_short"):
				await create_timer(0.36).timeout
			var shop: Control = main.screen
			var label: String = state + " " + language
			_shop_amounts(shop, label)
			if state.begins_with("confirm") or state == "insufficient":
				var outfit: bool = state.begins_with("confirm_outfit")
				var host: Control = shop._outfit_page if outfit else shop
				var modal: Control = host.get_node("Popup")
				var buy: Control = modal.content.get_node("Buy")
				var amount: Control = buy.get_node("Digits" if outfit else "Contents/Digits")
				_measure(amount, label + " buy", buy if outfit else buy.get_node("Contents"))
			if DisplayServer.get_name() != "headless" and not evidence.is_empty():
				await create_timer(0.36).timeout
				await RenderingServer.frame_post_draw
				root.get_texture().get_image().save_png(evidence.path_join(state + "-" + language + ".png"))
		for state in ["buy_outfit", "buy_outfit_short"]:
			load("res://game/aviary_view.gd").preview(main, state)
			await process_frame
			await create_timer(0.36).timeout
			var buy: Control = main.screen.get_node("Popup/Content/Buy")
			_measure(buy.get_node("Digits"), "aviary " + state + " " + language, buy)
			if DisplayServer.get_name() != "headless" and not evidence.is_empty():
				await create_timer(0.36).timeout
				await RenderingServer.frame_post_draw
				root.get_texture().get_image().save_png(evidence.path_join("aviary-" + state + "-" + language + ".png"))
		# Existing quantity changes and balance rebuilds must reapply local alignment.
		shop_script.preview(main, "confirm_booster")
		await process_frame
		var shop: Control = main.screen
		for quantity in [1, 2, 4, 10, 40]:
			shop._quantity_input.set_value(quantity)
			_measure(shop._purchase_contents.get_node("Digits"), language + " changed quantity " + str(quantity), shop._purchase_contents)
		for balance in [0, 1, 30, 100, 1200, 1240, 50270, 99999, 100000]:
			shop.profile.coins = balance
			shop._build_coins()
			_measure(shop._coins.get_node("Digits").get_child(0), language + " changed balance " + str(balance), shop._coins.get_node("Digits"))
		UiText.set_language("zh" if language == "en" else "en")
		await process_frame
		await process_frame
		_shop_amounts(shop, language + " after live locale change")
	for digit in digit_hashes:
		check(hash(art_script.tex(island_script.DIGIT + digit).get_image().get_data()) == digit_hashes[digit],
			"Shop and Aviary measurement preserves source image " + digit)
	if not evidence.is_empty():
		var file := FileAccess.open(evidence.path_join("amount-results.json"), FileAccess.WRITE)
		file.store_string(JSON.stringify({"checks": checks, "failures": failures, "measurements": measurements}, "\t"))
	main.queue_free()
	await process_frame
	print("shop amount checks: %d checks, %d failures" % [checks, failures])
	quit(1 if failures else 0)
