extends SceneTree
## Gold title ordinals and the existing Close remain consistent across the complete level range.
var checks := 0
var failures := 0
var calls := 0

func _initialize() -> void:
	_run.call_deferred()

func check(ok: bool, message: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		print("FAIL ", message)

func check_heading(heading: Control, ordinal: String) -> void:
	var word: TextureRect = heading.get_child(0)
	var source := word.texture.get_image().get_used_rect()
	var scale := word.size / word.texture.get_size()
	var ink := Rect2(word.position + Vector2(source.position) * scale, Vector2(source.size) * scale)
	var number: Control = heading.get_node("TitleNumber")
	check(number.get_child_count() == ordinal.length(), "one painted gold glyph per ordinal character")
	check(absf(number.get_rect().get_center().y - ink.get_center().y) < 0.01, "title word and ordinal visible centerline")
	check(is_equal_approx(number.size.y, ink.size.y), "title word and ordinal visible height")
	check(absf(number.position.x - ink.end.x - 24.0) < 0.01, "visible word-number gap")
	var union := ink.merge(number.get_rect())
	check(union.get_center().distance_to(heading.size * 0.5) < 0.01, "complete visible heading centered")
	check(union.position.x >= 0 and union.end.x <= heading.size.x, "complete heading fits")
	var index := 0
	var right := 0.0
	for glyph: TextureRect in number.get_children():
		check(glyph.texture.atlas == UiText.picture("number_gold_" + ordinal[index]), "retained localized gold source")
		check(glyph.texture.region == Rect2(glyph.texture.atlas.get_image().get_used_rect()), "glyph fitted by exact painted ink")
		check(glyph.material == null and glyph.modulate == Color.WHITE, "no recolor or extra outline")
		check(absf(glyph.size.y - ink.size.y) < 0.01, "same visible glyph cap")
		check(absf(glyph.position.x - right) < 0.01, "uniform ordinal spacing")
		right = glyph.get_rect().end.x + ink.size.y * 0.06
		index += 1

func _run() -> void:
	var family: GDScript = load("res://game/family_dialog.gd")
	var main: Control = load("res://game/main.tscn").instantiate()
	var fake := Profile.new()
	fake.coins = 1280
	fake.muted = true
	fake.music_muted = true
	main.args["profile"] = JSON.stringify(fake.to_dict())
	main.args["preview"] = "coin_shop_view:normal"
	root.add_child(main)
	main.args["smoke"] = "1"
	main.args["capture"] = "1"
	await process_frame
	var saved := JSON.stringify(main.profile.to_dict())
	for language in ["en", "zh"]:
		UiText.set_language(language)
		for ordinal in range(1, Levels.COUNT + 1):
			var heading: Control = family.Heading.new()
			heading.size = Vector2(680, 112)
			heading.label_id = "popup.level.title"
			heading.ordinal = str(ordinal)
			root.add_child(heading)
			heading.refresh()
			check_heading(heading, str(ordinal))
			heading.free()
		var popups: GDScript = load("res://game/popups.gd")
		var rosters: Array[Array] = [[], ["sunny"], ["sunny", "sky", "berry", "mint", "cocoa", "snow", "ember"]]
		for roster in rosters:
			var species: Array[String] = []
			species.assign(roster)
			for playable in [false, true]:
				var mechanics: Array[String] = ["sleep", "egg"]
				var dialog: Control = popups.level_card(main.popups, 100, 3, 20, 12, species, mechanics, func(): calls += 1, playable)
				var close: Button = dialog.content.get_node("Close")
				var title: Control = dialog.content.get_node("TitleGroup")
				print("CLOSE ", close.size, " expected ", load("res://game/menu_navigation.gd").SIZE)
				check(close.size.is_equal_approx(load("res://game/menu_navigation.gd").SIZE), "same-role Close hit size")
				check(close.get_node("Face").texture == Art.tex("btn_purchase_close"), "same accepted Close image")
				# The approved jungle frame uses its complete bounds; the Close stays in its top-right corner.
				var painted_panel: Rect2 = dialog.content.get_node("Panel").get_rect()
				check(painted_panel == dialog.panel_rect, "painted jungle frame matches the level card bounds")
				check(painted_panel.encloses(close.get_rect()), "Close sits inside the painted card corner")
				check(close.get_rect().end.x > dialog.panel_rect.end.x - 180.0, "Close hugs the card's right edge")
				check(close.position.x >= 0 and close.position.y >= 24 and close.get_rect().end.x <= 1080, "Close touch bounds on screen with top clearance")
				check(not close.get_rect().intersects(title.get_rect()), "Close hit area never overlaps heading")
				check(dialog.content.get_node("Play").disabled == not playable, "Play availability preserved")
				close.pressed.emit()
				close.pressed.emit()
				var deadline := Time.get_ticks_msec() + 2000
				while is_instance_valid(dialog) and Time.get_ticks_msec() < deadline:
					await process_frame
				check(calls == 0, "Close and repeated Close never dispatch Play")
				check(not is_instance_valid(dialog), "Close disposes modal")
	check(JSON.stringify(main.profile.to_dict()) == saved, "profile unchanged")
	print("level heading checks: %d checks, %d failures" % [checks, failures])
	main.queue_free()
	await process_frame
	quit(1 if failures else 0)
