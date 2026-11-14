extends SceneTree
## Single card actions fill their frame proportion, without resizing paired or item controls.
var checks := 0
var failures := 0
var main: Control
var kit: GDScript

func check(ok: bool, label: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		print("FAIL ", label)

func _initialize() -> void:
	root.content_scale_size = Vector2i(1080, 1920)
	root.content_scale_mode = Window.CONTENT_SCALE_MODE_VIEWPORT
	_run.call_deferred()

func verify(dialog: Control, button: Button, label: String, height := 162.0, width := 0.0) -> void:
	var expected: Vector2 = kit.card_action_size(dialog.panel_rect.size.x, height)
	if width > 0:
		expected.x = width
	check(button.size.is_equal_approx(expected), label + " width proportion and preserved height")
	check(is_equal_approx(button.get_rect().get_center().x, 540.0), label + " horizontal center")
	check(button.position.y >= dialog.panel_rect.end.y + 32, label + " outside frame")
	check(button.get_rect().end.y <= 1920, label + " viewport")
	var face: Control = button.get_node("Face")
	check(face.size.is_equal_approx(button.size), label + " visible face fills hit area")
	check(face.has_node("Skin"), label + " face uses nine-patch not stretched words")
	var bounds := button.get_rect()
	button.button_down.emit()
	button.button_up.emit()
	check(button.get_rect() == bounds, label + " pressed hit region unchanged")
	var word: TextureRect = button.get_node_or_null("Content/Word")
	if word != null:
		var ink := Rect2(word.texture.get_image().get_used_rect())
		var center := word.position + ink.get_center() * word.size / word.texture.get_size()
		check(center.distance_to(button.size * 0.5) < 4, label + " bilingual ink centered")

func tap_action(dialog: Control, button: Button, right_edge: bool) -> void:
	var closed := [0]
	dialog.closed.connect(func(): closed[0] += 1)
	var bounds := button.get_global_rect()
	var point := Vector2(bounds.end.x - 8 if right_edge else bounds.position.x + 8, bounds.get_center().y)
	var move := InputEventMouseMotion.new()
	move.position = point
	root.push_input(move, true)
	var down := InputEventMouseButton.new()
	down.position = point
	down.button_index = MOUSE_BUTTON_LEFT
	down.pressed = true
	root.push_input(down, true)
	var up := down.duplicate()
	up.pressed = false
	root.push_input(up, true)
	await create_timer(0.35).timeout
	check(closed[0] == 1, "extended edge pointer closes exactly once")

func clear() -> void:
	for child in main.popups.get_children():
		child.queue_free()
	await process_frame

func _run() -> void:
	main = load("res://game/main.tscn").instantiate()
	main.args["profile"] = JSON.stringify(Profile.new().to_dict())
	main.args["preview"] = "map_view:cards"
	root.add_child(main)
	main.args["smoke"] = "1"
	main.args["capture"] = "1"
	await process_frame
	kit = load("res://game/button_kit.gd")
	var popups: GDScript = load("res://game/popups.gd")
	for language in ["en", "zh"]:
		UiText.set_language(language)
		for state in ["goal_info_mint", "goal_info_crow", "new_friend_berry", "unlock_firefly", "world_complete_1", "world_unlock_2", "rewards_daily"]:
			popups.preview(main, state)
			await process_frame
			var dialog: Control = main.popups.get_child(main.popups.get_child_count() - 1)
			var found := 0
			var action: Button
			for child in dialog.content.get_children():
				if child is Button and child.name != "Close":
					found += 1
					action = child
					verify(dialog, child, language + " " + state)
			check(found == 1, state + " exactly one action")
			await tap_action(dialog, action, language == "zh")
			await clear()
		var tutorial: GDScript = load("res://game/tutorial.gd")
		var t: Control = tutorial.open(main.popups, Callable(), true)
		for step in tutorial.STEPS:
			t.show_step(step)
			verify(t.modal, t._buttons[0], language + " tutorial")
		await clear()
		var lesson: Control = tutorial.open_lesson(main.popups, tutorial.LESSONS[0], 3, Callable(), true)
		verify(lesson.modal, lesson._buttons[0], language + " lesson")
		await clear()
		var rescue_script: GDScript = load("res://game/stuck_offer.gd")
		var rescued := [0]
		var before := JSON.stringify(main.profile.to_dict())
		var rescue: Control = rescue_script.open(main.popups, func(): rescued[0] += 1, true)
		await process_frame
		var action: Button = rescue.content.get_node("Rescue")
		verify(rescue, action, language + " rescue", 162, 540)
		check(rescue.panel_rect.size.x == 760, "rescue uses its compact frame only")
		check(not action.has_node("Content/Icon"), "rescue does not repeat its illustration")
		check(action.get_node("Content/Word").painted_id == "white_free_rewind", "rescue has localized explicit wording")
		var close: Button = rescue.content.get_node("Close")
		check(is_equal_approx(close.position.y - rescue.panel_rect.position.y, 48), "rescue close sits at title shoulder")
		check(not close.get_rect().intersects(rescue.content.get_node("Picture").get_rect()), "rescue close clears illustration")
		var illustration: Control = rescue.content.get_node("Picture")
		check(illustration.has_node("Perch") and illustration.has_node("PerchedBird0") and illustration.has_node("PerchedBird1"), "rescue uses native forest perch and two living birds")
		check(illustration.size == Vector2(rescue_script.PERCH_WIDTH, rescue_script.PICTURE), "forest illustration stays compact")
		check(illustration.mouse_filter == Control.MOUSE_FILTER_IGNORE, "illustration never steals input")
		for name in ["PerchedBird0", "PerchedBird1"]:
			var bird: Node2D = illustration.get_node(name)
			check(bird.quiet and bird.scale.x > 0, "instant illustration is deterministic and never mirrored")
		await tap_action(rescue, close, language == "zh")
		check(rescued[0] == 0, "cancel does not perform rescue")
		rescue = rescue_script.open(main.popups, func(): rescued[0] += 1, true)
		await process_frame
		await tap_action(rescue, rescue.content.get_node("Rescue"), language == "zh")
		check(rescued[0] == 1, "free rewind dispatches once")
		check(JSON.stringify(main.profile.to_dict()) == before, "UI dismissal never changes profile")
		await clear()
		var shop_script: GDScript = load("res://game/shop_view.gd")
		for state in ["confirm_booster", "confirm_nest", "confirm_outfit", "insufficient"]:
			shop_script.preview(main, state)
			await process_frame
			var shop: Control = main.screen
			var host: Control = shop._outfit_page if state.begins_with("confirm_outfit") else shop
			var modal: Control = host.get_node("Popup")
			verify(modal, modal.content.get_node("Buy"), language + " " + state, 162, 680)
		var coin_script: GDScript = load("res://game/coin_shop_view.gd")
		for pack in coin_script.PACKS:
			var shop: Control = coin_script.new()
			main.set_screen(shop)
			shop.setup(0)
			var paid := [0]
			shop.show_purchase(pack, func(chosen):
				paid[0] += 1
				check(chosen.id == pack.id, "currency callback preserves selected pack"))
			await process_frame
			var modal: Control = shop.get_node("Popup")
			verify(modal, modal.content.get_node("Confirm"), language + " currency " + pack.id, coin_script.CONFIRM_SIZE.y, 552)
			check(modal.content.get_node("Confirm/Content/Number").get_meta("value") == "¥%d" % pack.yuan, "currency amount preserved")
			var confirm: Button = modal.content.get_node("Confirm")
			var number: Control = confirm.get_node("Content/Number")
			var first := true
			var ink_bounds := Rect2()
			for glyph in number.get_children():
				var ink := Rect2(glyph.texture.get_image().get_used_rect())
				var used := Rect2(glyph.position + ink.position * glyph.size / glyph.texture.get_size(), ink.size * glyph.size / glyph.texture.get_size())
				ink_bounds = used if first else ink_bounds.merge(used)
				first = false
			check((number.position + ink_bounds.get_center()).distance_to(confirm.size * 0.5) < 4, "currency painted glyphs centered")
			await tap_action(modal, confirm, language == "zh")
			check(paid[0] == 1, "extended currency edge invokes mock callback exactly once")
	check(kit.PRIMARY == Vector2(430, 162), "global primary untouched")
	check(kit.SECONDARY == Vector2(288, 108), "item buttons untouched")
	main.queue_free()
	await process_frame
	await process_frame
	print("Single card actions: %d checks, %d failures" % [checks, failures])
	quit(0 if failures == 0 else 1)
