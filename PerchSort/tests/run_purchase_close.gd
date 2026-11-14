extends SceneTree
## Purchase-only Close placement, visible title clearance and actual center/edge pointer dispatch.

const STATES := ["confirm_booster", "confirm_multiple", "confirm_maximum", "insufficient",
	"confirm_nest", "confirm_nest_short", "confirm_outfit", "confirm_outfit_short"]
var checks := 0
var failures := 0
var main: Control
var evidence := ""
var only_language := ""
var outcomes: Array = []
var shop_script: GDScript
var coin_script: GDScript
var purchase_script: GDScript
var modal_script: GDScript
var navigation_script: GDScript

func _initialize() -> void:
	root.content_scale_size = Vector2i(1080, 1920)
	root.content_scale_mode = Window.CONTENT_SCALE_MODE_VIEWPORT
	_run.call_deferred()

func check(ok: bool, label: String) -> void:
	checks += 1
	failures += 0 if ok else 1
	print("PASS " if ok else "FAIL ", label)

func _title_ink(modal: Control) -> Rect2:
	var group: Control = modal.content.get_node("TitleGroup")
	var sprite: TextureRect = group.get_child(group.get_child_count() - 1)   # the word, drawn over the sign
	var ink := Rect2(sprite.texture.get_image().get_used_rect())
	var scale := sprite.size / sprite.texture.get_size()
	return sprite.get_global_transform() * Rect2(ink.position * scale, ink.size * scale)

func _geometry(modal: Control, label: String) -> void:
	var close: Button = modal.content.get_node("Close")
	var bounds := close.get_rect()
	var title := _title_ink(modal)
	var logical_card: Rect2 = modal.get_meta("card_rect")
	var card: Rect2 = modal.content.get_node("Panel").get_rect()
	var skin: Texture2D = modal.content.get_node("Panel/Skin").texture
	var toy: bool = skin is AtlasTexture and skin.atlas == Art.tex("panel_shop_toy_rim")
	var expected_card: Rect2 = logical_card if toy else logical_card.grow_side(SIDE_TOP, load("res://game/purchase_dialog.gd").TITLE_ROOM)
	check(card == expected_card, label + " painted surface follows its approved shell")
	var close_at: Vector2 = Vector2(card.end.x - 156.0, card.position.y + 48.0) if toy else Vector2(card.end.x - 150.0, card.position.y + 30.0)
	check(bounds.position.distance_to(close_at) < 1.0, label + " Close sits at its shell's top-right shoulder")
	check(card.encloses(bounds), label + " Close lies within the card")
	check(Rect2(0, 0, 1080, 1920).encloses(bounds), label + " entire Close remains on screen")
	check(bounds.size.is_equal_approx(navigation_script.SIZE), label + " accepted Close size preserved")
	check(not close.get_global_rect().intersects(title), label + " visible title has no hit-area collision")
	check(close.get_node("Face").texture.resource_path.ends_with("/btn_purchase_close.png"), label + " accepted Close artwork preserved")
	close.button_down.emit()
	close.button_up.emit()
	check(close.get_rect() == bounds, label + " pressed geometry unchanged")
	outcomes.append({"state": label, "close": [bounds.position.x, bounds.position.y, bounds.size.x, bounds.size.y],
		"panel": [modal.panel_rect.position.x, modal.panel_rect.position.y, modal.panel_rect.size.x, modal.panel_rect.size.y],
		"title_ink": [title.position.x, title.position.y, title.size.x, title.size.y]})

func _tap(point: Vector2) -> void:
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

func _close(host: Control, modal: Control, profile: Profile, label: String, edge := "center", capture := false) -> void:
	_geometry(modal, label)
	var close: Control = modal.content.get_node("Close")
	var bounds := close.get_global_rect()
	var point := bounds.get_center()
	match edge:
		"left": point.x = bounds.position.x + 2
		"right": point.x = bounds.end.x - 2
		"top": point.y = bounds.position.y + 2
		"bottom": point.y = bounds.end.y - 2
	var before := JSON.stringify(profile.to_dict())
	var calls := [0]
	modal.closed.connect(func(): calls[0] += 1)
	if capture and DisplayServer.get_name() != "headless" and not evidence.is_empty():
		await create_timer(0.36).timeout
		await RenderingServer.frame_post_draw
		root.get_texture().get_image().save_png(evidence.path_join(label.replace(" ", "-") + ".png"))
	_tap(point)
	_tap(point)
	await create_timer(0.20).timeout
	if calls[0] != 1 or host.get_node_or_null("Popup") != null:
		print("CLOSE_DIAGNOSTIC ", label, " edge=", edge, " calls=", calls[0], " closing=", modal._closing if is_instance_valid(modal) else false, " point=", point)
	check(calls[0] == 1 and host.get_node_or_null("Popup") == null, label + " " + edge + " double pointer closes exactly once")
	check(JSON.stringify(profile.to_dict()) == before, label + " Close changes no profile data")

func _shop_case(state: String, language: String, edge := "center", capture := false) -> void:
	shop_script.preview(main, state)
	await process_frame
	var shop: Control = main.screen
	var host: Control = shop._outfit_page if state.begins_with("confirm_outfit") else shop
	await _close(host, host.get_node("Popup"), shop.profile, state + " " + language + (" " + edge if edge != "center" else ""), edge, capture)

func _coin_case(pack: Dictionary, language: String, edge := "center", capture := false) -> void:
	coin_script.preview(main, "packages")
	await process_frame
	var page: Control = main.screen
	var balance: String = str(page.get_node("ApprovedLayout/BalanceNumber").get_meta("value"))
	page.show_purchase(pack, func(_bought): pass)
	await process_frame
	await _close(page, page.get_node("Popup"), main.profile, "preview_" + str(pack.id) + " " + language + (" " + edge if edge != "center" else ""), edge, capture)
	check(str(page.get_node("ApprovedLayout/BalanceNumber").get_meta("value")) == balance, "Preview close preserves displayed balance")

func _run() -> void:
	for arg in OS.get_cmdline_user_args():
		if arg.begins_with("--close-language="):
			only_language = arg.trim_prefix("--close-language=")
		elif arg.begins_with("--close-evidence="):
			evidence = arg.trim_prefix("--close-evidence=")
	if not evidence.is_empty():
		DirAccess.make_dir_recursive_absolute(evidence)
	main = load("res://game/main.tscn").instantiate()
	var fake := Profile.new()
	fake.muted = true
	fake.music_muted = true
	main.args["profile"] = JSON.stringify(fake.to_dict())
	main.args["preview"] = "shop_view:boosters"
	root.add_child(main)
	main.args["smoke"] = "1"
	main.args["capture"] = "1"
	await process_frame
	shop_script = load("res://game/shop_view.gd")
	coin_script = load("res://game/coin_shop_view.gd")
	purchase_script = load("res://game/purchase_dialog.gd")
	modal_script = load("res://game/popup.gd")
	navigation_script = load("res://game/menu_navigation.gd")
	for language in ["en", "zh"]:
		if not only_language.is_empty() and language != only_language:
			continue
		UiText.set_language(language)
		for state in STATES:
			await _shop_case(state, language, "center", true)
		for pack in coin_script.PACKS:
			await _coin_case(pack, language, "center", true)
		for edge in ["left", "right", "top", "bottom"]:
			for state in ["confirm_booster", "confirm_nest", "confirm_outfit"]:
				await _shop_case(state, language, edge)
			await _coin_case(coin_script.PACKS[5], language, edge)
		# Every existing item title goes through the canonical PurchaseDialog, not a mocked title renderer.
		shop_script.preview(main, "boosters")
		await process_frame
		var shop: Control = main.screen
		for item in Shop.catalog():
			var title: String = shop._title_key(item)
			var modal: Control = purchase_script.open(shop, title, [400.0], true, 132.0)
			_geometry(modal, "title " + title + " " + language)
			modal.free()
	if not evidence.is_empty():
		var file := FileAccess.open(evidence.path_join("close-results.json"), FileAccess.WRITE)
		file.store_string(JSON.stringify({"checks": checks, "failures": failures, "outcomes": outcomes}, "\t"))
	main.queue_free()
	await process_frame
	print("purchase Close checks: %d checks, %d failures" % [checks, failures])
	quit(1 if failures else 0)
