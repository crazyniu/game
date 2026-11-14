extends SceneTree
## Navigation contracts through canonical views, with an isolated fake profile.

var main: Control
var navigation: GDScript
var island: GDScript
var checks := 0
var failures := 0

func _initialize() -> void:
	_run.call_deferred()

func check(ok: bool, message: String) -> void:
	checks += 1
	failures += 0 if ok else 1
	print("PASS " if ok else "FAIL ", message)

func settled(predicate: Callable) -> bool:
	var end := Time.get_ticks_msec() + 2500
	while not predicate.call():
		if Time.get_ticks_msec() >= end:
			return false
		await process_frame
	return true

func verify(button: Button, picture: String, title: String) -> void:
	var face := button.get_node("Face") as TextureRect
	var scale: Vector2 = button.get_global_transform().get_scale()
	check((button.size * scale).is_equal_approx(navigation.SIZE), title + " uses coin-shop game-space size")
	check(face.texture == Art.tex(picture), title + " reuses accepted original image")
	var bounds := button.get_rect()
	button.button_down.emit()
	check(face.self_modulate == island.PRESSED and button.get_rect() == bounds, title + " press tints without movement")
	button.button_up.emit()
	check(face.self_modulate == Color.WHITE and button.get_rect() == bounds, title + " release restores without movement")

func close_modal(page: Control) -> void:
	var ref: WeakRef = weakref(page)
	page.content.get_node("Close" if page.content.has_node("Close") else "Confirm").pressed.emit()
	page.content.get_node("Close" if page.content.has_node("Close") else "Confirm").pressed.emit()
	check(await settled(func(): return ref.get_ref() == null), "repeated Close completes one modal exit")

func _run() -> void:
	island = load("res://game/island.gd")
	navigation = load("res://game/menu_navigation.gd")
	main = load("res://game/main.tscn").instantiate()
	var fake := Profile.new()
	fake.coins = 1280
	fake.highest_unlocked = 8
	fake.met = {"sunny": true, "sky": true, "berry": true}
	for level in range(1, 8):
		fake.stars[level] = 3
	main.args["profile"] = JSON.stringify(fake.to_dict())
	root.add_child(main)
	main.args["smoke"] = "1"
	main.args["capture"] = "1"
	await create_timer(0.3).timeout
	var before := JSON.stringify(main.profile.to_dict())
	for language in ["zh", "en"]:
		UiText.set_language(language)
		main._open_map()
		var map: Control = main.screen
		verify(map.get_node("Bar/Home"), navigation.BACK_ART, language + " Map Back")
		map.open_world(1)
		map.get_node("Bar/Home").pressed.emit()
		check(map._page == null and map._world == 1, "trail Back retains selected world")
		map.get_node("Bar/Home").pressed.emit()
		check(await settled(func(): return main.screen.get_script().resource_path.ends_with("home_view.gd") and not main._wiping), "Map Back returns Home")
		main._open_aviary()
		var aviary: Control = main._overlay
		verify(aviary.find_child("Close", true, false), navigation.CLOSE_ART, language + " Aviary Close")
		aviary._show_detail("sunny")
		var back := aviary.find_child("Back", true, false) as Button
		verify(back, navigation.BACK_ART, language + " Aviary Back")
		back.pressed.emit()
		check(aviary.find_child("Close", true, false) != null, "detail Back returns collection")
		aviary.find_child("Close", true, false).pressed.emit()
		check(await settled(func(): return main._overlay == null and not main._wiping), "Aviary Close returns Home")
		main.open_shop(0)
		var shop: Control = main._overlay
		verify(shop.find_child("Close", true, false), navigation.CLOSE_ART, language + " Shop Close")
		shop.find_child("Close", true, false).pressed.emit()
		check(await settled(func(): return main._overlay == null and not main._wiping), "Shop Close returns Home")
		main.open_coin_shop(main.profile.coins)
		var coin: Control = main._coin_shop
		verify(coin.get_node("ApprovedLayout/Close"), navigation.CLOSE_ART, language + " coin-shop reference")
		coin.get_node("ApprovedLayout/Close").pressed.emit()
		check(await settled(func(): return main._coin_shop == null), "coin shop retains dismissal callback")
		var chest: Control = main.Popups.chest(main.popups, 0, 0, func(): pass, func(): pass, fake.coins)
		verify(chest.content.get_node("Close"), navigation.CLOSE_ART, language + " Chest Close")
		await close_modal(chest)
		var draw: Control = main.Popups.lucky_draw(main.popups, 0, 5, func(): return {})
		verify(draw.find_child("Close", true, false), navigation.CLOSE_ART, language + " Draw Close")
		var draw_ref: WeakRef = weakref(draw)
		draw.find_child("Close", true, false).pressed.emit()
		check(await settled(func(): return draw_ref.get_ref() == null), "Draw Close retains dismissal callback")
		var result: Control = main.Popups.rewards(main.popups, "draw.title", 400, [], func(): pass)
		check(not result.content.has_node("Close") and result.content.has_node("Confirm"), language + " Draw result bottom confirmation")
		await close_modal(result)
		var document: Control = main.Popups.document(main.popups, "credits")
		check(not document.content.has_node("Close") and document.content.has_node("Confirm"), language + " document bottom confirmation")
		await close_modal(document)
		var chapters: Control = main.Popups.chapters(main.popups, fake, func(_world): pass)
		verify(chapters.content.get_node("Close"), navigation.CLOSE_ART, language + " chapter Close")
		for direction in ["PagePrevious", "PageNext"]:
			var pagination: Control = chapters.content.get_node(direction)
			var label: TextureRect = pagination.get_child(-1)
			check(label.texture != null, language + " " + direction + " retains an existing painted word")
		await close_modal(chapters)
	UiText.set_language("zh")
	check(JSON.stringify(main.profile.to_dict()) == before, "navigation does not mutate profile")
	main.queue_free()
	await process_frame
	await process_frame
	var sound := root.get_node("Sound")
	for player in sound.get_children():
		if player is AudioStreamPlayer:
			player.stop()
			player.stream = null
	print("menu navigation checks: %d checks, %d failures" % [checks, failures])
	quit(1 if failures else 0)
