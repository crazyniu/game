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

func _text(digits: Control) -> String:
	var result := ""
	for glyph: TextureRect in digits.get_children():
		result += glyph.texture.resource_path.get_file().get_basename().trim_prefix(island_script.DIGIT)
	return result

func _source_art(digits: Control, label: String) -> void:
	var preserved := true
	for glyph: TextureRect in digits.get_children():
		var digit := glyph.texture.resource_path.get_file().get_basename().trim_prefix(island_script.DIGIT)
		preserved = preserved and digit in "0123456789" and digit.length() == 1
		preserved = preserved and glyph.texture == Art.tex(island_script.DIGIT + digit)
		preserved = preserved and absf(glyph.size.aspect() - glyph.texture.get_size().aspect()) < 0.001
	check(preserved, label + " original painted digit textures and aspect preserved")

func _measure(digits: Control, label: String, owner: Control = null, expected := -1) -> void:
	var union := _ink(digits)
	var offset := union.get_center() - digits.size * 0.5
	check(offset.length() < 0.01, label + " visible union centered")
	check(Rect2(Vector2.ZERO, digits.size).encloses(union), label + " ink fits amount box")
	_source_art(digits, label)
	if expected >= 0:
		check(_text(digits) == str(expected), label + " exact displayed value")
	if owner != null:
		check((digits.position + union.get_center()).distance_to(owner.size * 0.5) < 0.01,
			label + " complete amount centered in owning face")
	measurements.append({"node": label, "text": _text(digits), "offset": [offset.x, offset.y]})

func _sprite_ink(sprite: TextureRect) -> Rect2:
	var source := Rect2(sprite.texture.get_image().get_used_rect())
	var ratio := sprite.size / sprite.texture.get_size()
	return Rect2(sprite.position + source.position * ratio, source.size * ratio)

func _feather(group: Control, label: String, owner: Control, expected: int) -> void:
	check(group != null, label + " currency group exists")
	if group == null:
		return
	var digits: Control = group.get_node("Digits")
	var feather: TextureRect = group.get_node("Feather")
	var numeral_ink := _ink(digits)
	numeral_ink.position += digits.position
	var feather_ink := _sprite_ink(feather)
	var union := numeral_ink.merge(feather_ink)
	# Transparent art is fitted into its icon box; allow at most two raster pixels of optical slack.
	check(union.get_center().distance_to(group.size * 0.5) <= 2.0, label + " full feather amount visually centered")
	check(Rect2(Vector2.ZERO, group.size).grow(0.01).encloses(union), label + " full feather amount fits group")
	check((group.position + union.get_center()).distance_to(owner.size * 0.5) <= 2.0,
		label + " full feather amount centered in action")
	check(feather_ink.end.x < numeral_ink.position.x, label + " icon and digits do not overlap")
	check(_text(digits) == str(expected), label + " exact catalog or quantity total")
	_source_art(digits, label)
	var previous_end := -INF
	var tight := true
	for glyph: TextureRect in digits.get_children():
		var ink := _sprite_ink(glyph)
		if previous_end != -INF:
			var gap := ink.position.x - previous_end
			tight = tight and gap >= 0 and gap <= minf(group.size.y, 80.0) * 0.08
		previous_end = ink.end.x
	check(tight, label + " compact painted digits retain nonoverlapping spacing")
	measurements.append({"node": label, "text": _text(digits), "box": [union.position.x, union.position.y, union.size.x, union.size.y]})

func _purchase(owner: Control, label: String, expected: int) -> void:
	_feather(owner.get_node_or_null("FeatherPrice"), label, owner, expected)

func _shop_amounts(shop: Control, label: String) -> void:
	_measure(shop._coins.get_node("Digits").get_child(0), label + " balance", shop._coins.get_node("Digits"), mini(shop.profile.coins, load("res://game/feather_bar.gd").DISPLAY_MAX))
	var catalog: Array = shop._items(shop.TABS[shop.tab])
	for card in shop._grid.get_children():
		var index: int = card.get_index()
		var item: Dictionary = catalog[index] if index < catalog.size() else {}
		var price: Control = card.get_node_or_null("Body/Price")
		if price != null:
			_feather(price.get_node_or_null("FeatherPrice"), label + " " + str(card.name) + " price", price, item["price"])
		var stock: Control = card.get_node_or_null("Body/Stock")
		if stock != null:
			var count = stock.get_node("Count")
			check(count.value == str(mini(shop.profile.inventory[item["booster"]], island_script.BADGE_CAP)), label + " exact stock " + str(card.name))
			check(count.align == 0, label + " stock numerals align centrally")
			check(count.position.x == 0.0 and count.size.x == stock.size.x, label + " stock numerals span the count badge")

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
				var expected: int = Shop.item("outfit.berry.3")["price"] if outfit else shop._purchase_item["price"] * shop._quantity
				_purchase(buy.get_node("Content"), label + " buy", expected)
			if DisplayServer.get_name() != "headless" and not evidence.is_empty():
				await create_timer(0.36).timeout
				await RenderingServer.frame_post_draw
				root.get_texture().get_image().save_png(evidence.path_join(state + "-" + language + ".png"))
		for state in ["buy_outfit", "buy_outfit_short"]:
			load("res://game/aviary_view.gd").preview(main, state)
			await process_frame
			await create_timer(0.36).timeout
			var buy: Control = main.screen.get_node("Popup/Content/Buy")
			_purchase(buy.get_node("Content"), "aviary " + state + " " + language, Shop.item("outfit.hoot.1")["price"])
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
			_purchase(shop._purchase_contents, language + " changed quantity " + str(quantity), shop._purchase_item["price"] * shop._quantity)
		for balance in [0, 1, 30, 100, 1200, 1240, 50270, 99999, 100000]:
			shop.profile.coins = balance
			shop._build_coins()
			_measure(shop._coins.get_node("Digits").get_child(0), language + " changed balance " + str(balance), shop._coins.get_node("Digits"), mini(balance, load("res://game/feather_bar.gd").DISPLAY_MAX))
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
