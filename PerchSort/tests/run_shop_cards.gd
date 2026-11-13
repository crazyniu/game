extends SceneTree
## Card geometry, cached-category sizing, scrolling and unchanged shop transactions.

var failures := 0
var view: Control
var ShopView: GDScript

func _initialize() -> void:
	_run.call_deferred()

func check(ok: bool, label: String) -> void:
	print("PASS " if ok else "FAIL ", label)
	failures += 0 if ok else 1

func _run() -> void:
	ShopView = load("res://game/shop_view.gd")
	var profile := Profile.new()
	profile.coins = 50000
	profile.highest_unlocked = Levels.COUNT
	profile.inventory = PackedInt32Array([3, 2, 1, 1, 0, 2])
	for species in Board.SPECIES:
		profile.met[species] = true
	view = ShopView.new()
	view.instant = true
	view.setup(profile)
	root.add_child(view)
	await process_frame
	await process_frame
	check(view._grid.get_child_count() == 6, "six unchanged booster identities")
	check(ShopView.BOOSTER_PRICE.get_center().y == ShopView.ITEM_TILE.y * 0.5, "purchase action centered on row surface")
	check(ShopView.BOOSTER_PRICE.size == ShopView.PRICE_SIZE and ShopView.ITEM_ACTION.size == ShopView.PRICE_SIZE, "list actions use the secondary button size")
	check(ShopView.ITEM_NAME.end.y < ShopView.ITEM_ACTION.position.y, "nest name sits above its action pill")
	check(ShopView.ITEM_ACTION.end.y <= ShopView.CARD_TILE.y, "card action pill stays inside its card")
	check(not ShopView.BOOSTER_NAME.intersects(view._grid.get_child(0).get_node("Body/Stock").get_rect()), "inventory badge does not overlap the name")
	check(view._grid.get_child(0).get_node("Body/Stock/Count").has_meta("value"), "stock uses painted numerals")
	check(view._grid.get_child(0).size == ShopView.ITEM_TILE, "equal forest card size")
	check(view._grid.get_child(2).position == Vector2(0, 2 * (ShopView.ITEM_TILE.y + ShopView.TILE_GAP)), "single-column shelf order")
	check(view._grid.get_child(5).get_rect().end.y > ShopView.AREA.size.y, "booster shelves extend through scroll area")
	check(view._scroll.get_v_scroll_bar().max_value > ShopView.AREA.size.y, "all booster shelves remain scrollable")
	check(view.get_node("Tabs").get_child_count() == 3, "three icon-only category controls")
	check(view._scroll.clip_contents and view._scroll.get_rect().end.y < ShopView.TAB_BAR.position.y, "scroll content clips above fixed navigation")
	check(view.get_node("Bar/Close").size == load("res://game/menu_navigation.gd").SIZE, "close size matches coin shop")
	for item in view._items(Shop.BOOSTER):
		check(item["price"] == Shop.item(item["id"])["price"], "unchanged price " + item["id"])
	view._select_tab(1)
	await process_frame
	await process_frame
	check(view._grid.get_child_count() == 9, "all nine outfit species remain available")
	check(view._content.custom_minimum_size.y > ShopView.AREA.size.y, "outfit content grows beyond six entries")
	view._scroll.scroll_vertical = 99999
	await process_frame
	await process_frame
	var last := view._grid.get_child(8) as Control
	check(view._scroll.scroll_vertical > 0, "outfits scroll to final row")
	check(last.position.y + last.size.y - view._scroll.scroll_vertical <= ShopView.AREA.size.y, "last outfit card is fully reachable")
	var cached: Control = view._grid
	view._select_tab(0)
	await process_frame
	check(view._content.custom_minimum_size.y == view._page_height(0), "short tab clears tall cached page extent")
	view._select_tab(1)
	check(view._grid == cached, "tab switches retain live outfit content")
	view._select_tab(2)
	await process_frame
	await process_frame
	check(view._grid.get_child_count() == view._items(Shop.HOUSE).size(), "all nest products remain available")
	view._scroll.scroll_vertical = 99999
	await process_frame
	await process_frame
	last = view._grid.get_child(view._grid.get_child_count() - 1)
	check(last.position.y + last.size.y - view._scroll.scroll_vertical <= ShopView.AREA.size.y, "last nest card is fully reachable")
	var nest := Shop.item("house.pinecone")
	var balance: int = profile.coins
	view._buy(nest, 1)
	check(profile.owns_house("pinecone") and profile.coins == balance - nest["price"], "nest price and purchase preserved")
	view._tap(nest)
	check(profile.equipped_house == "pinecone", "owned nest tap still equips")
	view._select_tab(0)
	var booster := Shop.item("booster.firefly")
	balance = profile.coins
	var stock: int = profile.inventory[0]
	view._buy(booster, 2)
	check(profile.coins == balance - booster["price"] * 2 and profile.inventory[0] == stock + 2, "booster quantity economics preserved")
	view._drag_travel = ShopView.TAP_SLOP + 1
	view._tap(booster)
	check(view._modal == null, "drag does not purchase or open a modal")
	UiText.set_language("zh")
	await process_frame
	check(view._title.painted_id == "lettering_tab_boosters", "localized heading survives rebuild")
	view.queue_free()
	await process_frame
	view = load("res://tests/shop_expanded_fixture.gd").new()
	view.instant = true
	view.setup(profile)
	root.add_child(view)
	await process_frame
	await process_frame
	check(view._grid.get_child_count() == 8, "future eight-booster fixture builds without layout changes")
	view._scroll.scroll_vertical = 99999
	await process_frame
	await process_frame
	last = view._grid.get_child(7)
	check(view._scroll.scroll_vertical > 0 and last.position.y + last.size.y - view._scroll.scroll_vertical <= ShopView.AREA.size.y,
		"future boosters beyond six are fully scrollable")
	view.queue_free()
	await process_frame
	view = ShopView.new()
	view.setup(profile)
	root.add_child(view)
	await process_frame
	view._select_tab(1)
	view._select_tab(0)
	for i in 14:
		await process_frame
	check(view._pages[1].get_child_count() == 9, "outfits warm asynchronously without loss")
	check(view._content.custom_minimum_size.y == view._page_height(0), "background warming preserves active booster height")
	view.queue_free()
	await process_frame
	print("shop card check: %d failures" % failures)
	quit(1 if failures else 0)
