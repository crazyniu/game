extends SceneTree
## Canonical purchase presentation and transaction contracts, using only fake profiles.

var checks := 0
var failures := 0
var main: Control

func _initialize() -> void:
	_run.call_deferred()

func check(ok: bool, label: String) -> void:
	checks += 1
	failures += 0 if ok else 1
	print("PASS " if ok else "FAIL ", label)

func _settle() -> void:
	await create_timer(0.24).timeout

func _skin(modal: Control, label: String) -> void:
	var skin: NinePatchRect = modal.content.get_node("Panel/Skin")
	var shop_shell := not label.begins_with("feather ")
	var expected: String = "panel_jungle_dialog" if shop_shell else load("res://game/button_kit.gd").PANEL
	check(skin.texture == Art.tex(expected), label + " uses its approved panel family")
	if shop_shell:
		var card: Rect2 = modal.get_meta("card_rect")
		var action_y: float = modal.get_meta("actions_y")
		check(action_y >= card.end.y + load("res://game/popup.gd").ACTION_DROP, label + " price action stays outside the shell")
		check(skin.patch_margin_top == 330 and skin.patch_margin_bottom == 210,
			label + " painted header and footer retain their proportions")
	var close: Button = modal.content.get_node("Close")
	check(close.get_node("Face").texture == Art.tex("btn_purchase_close"), label + " uses coin-shop Close")
	print("CLOSE_GEOMETRY actual=", close.size, " expected=", load("res://game/menu_navigation.gd").SIZE)
	check(close.size.is_equal_approx(load("res://game/menu_navigation.gd").SIZE), label + " Close matches coin-shop dimensions")
	var bounds := close.get_rect()
	close.button_down.emit()
	close.button_up.emit()
	check(close.get_rect() == bounds, label + " Close press retains geometry")

func _quantity_focus(input, language: String) -> void:
	UiText.set_language(language)
	input.set_value(12)
	input.grab_focus()
	input.edit()
	input.deselect()
	input.caret_column = input.text.length()
	await process_frame
	input._process(0.0)
	var kit: GDScript = load("res://game/button_kit.gd")
	var text: String = input.text
	var width: float = kit.number_width(text, input.CAP)
	var cap: float = input.CAP * minf(1.0, input._digits.size.x / width)
	var painted_width: float = kit.number_width(text, cap)
	var left: float = (input.size.x - painted_width) * 0.5
	check(input.has_focus() and input.is_editing() and input.is_processing(), language + " quantity enters actual editing")
	check(input._digits.get_meta("value") == text, language + " caret uses the same glyphs as visible digits")
	check(input._caret.visible and not input._selection.visible and
		is_equal_approx(input._caret.position.x, left + painted_width), language + " visible caret follows painted digits")
	input.select(0, 1)
	input._process(0.0)
	check(input._selection.visible and not input._caret.visible and
		is_equal_approx(input._selection.position.x, left) and
		is_equal_approx(input._selection.size.x, kit.number_width(text.left(1), cap)),
		language + " partial selection follows first painted digit")
	input.select_all()
	input._process(0.0)
	check(input._selection.visible and is_equal_approx(input._selection.size.x, painted_width),
		language + " full selection follows complete painted number")
	input.release_focus()
	await process_frame
	check(not input._caret.visible and not input._selection.visible and not input.is_processing(),
		language + " blur stops caret processing and clears selection")

func _run() -> void:
	main = load("res://game/main.tscn").instantiate()
	var fake := Profile.new()
	fake.coins = 1280
	fake.highest_unlocked = Levels.COUNT
	fake.language = "en"
	fake.muted = true
	fake.music_muted = true
	fake.met = {"berry": true, "sunny": true}
	main.args["profile"] = JSON.stringify(fake.to_dict())
	main.args["preview"] = "shop_view:boosters"
	root.add_child(main)
	main.args["smoke"] = "1"
	main.args["capture"] = "1"
	await _settle()
	var view: Control = load("res://game/shop_view.gd").new()
	view.instant = true
	main.set_screen(view)
	view.setup(fake)
	var firefly := Shop.item("booster.firefly")
	var before := JSON.stringify(fake.to_dict())
	view._confirm(firefly)
	var modal: Control = view._modal
	_skin(modal, "booster")
	view._confirm(firefly)
	check(view._modal == modal and view.find_children("Popup*", "Modal", false, false).size() == 1,
		"repeated booster open keeps one modal")
	check(view._minus.disabled and not view._plus.disabled, "one-unit quantity bounds")
	await _quantity_focus(view._quantity_input, "en")
	await _quantity_focus(view._quantity_input, "zh")
	UiText.set_language("en")
	view._quantity_input.set_value(5)
	check(view._quantity == 5, "editable quantity updates purchase")
	view._quantity_input.set_value(99999)
	check(view._quantity == fake.coins / int(firefly.price) and view._plus.disabled, "quantity caps at affordability")
	view._quantity_input.text = ""
	view._quantity_input._edited("")
	check(view._quantity == 0 and view._purchase_button.disabled, "empty quantity disables Buy")
	view._quantity_input.focus_exited.emit()
	check(view._quantity == 1 and not view._purchase_button.disabled, "empty quantity restores one on blur")
	view._quantity_input.text = "ab3"
	view._quantity_input._edited("ab3")
	check(view._quantity == 3 and view._quantity_input.text == "3", "quantity strips non-digits")
	UiText.set_language("zh")
	await process_frame
	check(modal.content.get_node("Caption").texture != null, "open quantity caption survives Chinese switch")
	UiText.set_language("en")
	modal.content.get_node("Close").pressed.emit()
	await _settle()
	check(not is_instance_valid(view._modal) and JSON.stringify(fake.to_dict()) == before,
		"Close cancels booster purchase without mutation")
	view._confirm(firefly)
	view._quantity_input.set_value(2)
	var old_coins := fake.coins
	var old_stock: int = fake.inventory[firefly.booster]
	view._purchase_button.pressed.emit()
	view._purchase_button.pressed.emit()
	await _settle()
	check(fake.coins == old_coins - firefly.price * 2 and fake.inventory[firefly.booster] == old_stock + 2,
		"double Buy grants exactly one selected booster transaction")
	fake.coins = 0
	view._confirm(firefly)
	check(view._purchase_button.disabled and not view._quantity_input.editable and view._plus.disabled,
		"insufficient booster funds disable purchase and quantity")
	var buy_bounds: Rect2 = view._purchase_button.get_rect()
	check(buy_bounds.size == view.BUY_SIZE, "disabled booster Buy retains dimensions")
	view._modal.close()
	await _settle()
	fake.coins = 1280
	var nest := Shop.item("house.pinecone")
	view._confirm(nest)
	_skin(view._modal, "nest")
	check(view._quantity_input == null, "single nest has no quantity controls")
	old_coins = fake.coins
	view._purchase_button.pressed.emit()
	view._purchase_button.pressed.emit()
	await _settle()
	check(fake.owns_house("pinecone") and fake.coins == old_coins - nest.price, "nest purchased exactly once")
	view._tap(nest)
	check(fake.equipped_house == "pinecone", "owned nest still equips")
	fake.coins = 1280
	var outfit_page: Control = view.open_outfits("berry")
	var outfit := Shop.item("outfit.berry.3")
	outfit_page._confirm_outfit(outfit.id, 3, outfit_page._tiles.get_child(3))
	modal = outfit_page.get_node("Popup")
	_skin(modal, "outfit")
	check(not modal.find_children("*", "BirdView", true, false).is_empty(), "outfit retains actual live BirdView")
	outfit_page._confirm_outfit(outfit.id, 3, outfit_page._tiles.get_child(3))
	check(outfit_page.get_node("Popup") == modal, "repeated outfit open keeps one modal")
	old_coins = fake.coins
	modal.content.get_node("Buy").pressed.emit()
	modal.content.get_node("Buy").pressed.emit()
	await _settle()
	check(fake.owns_outfit(outfit.id) and fake.equipped.get("berry") == outfit.id and fake.coins == old_coins - outfit.price,
		"outfit purchases once and equips through existing callback")
	fake.coins = 0
	outfit_page._confirm_outfit("outfit.berry.2", 2, outfit_page._tiles.get_child(2))
	modal = outfit_page.get_node("Popup")
	check(modal.content.get_node("Buy").disabled, "unaffordable outfit Buy disabled")
	before = JSON.stringify(fake.to_dict())
	modal.close()
	await _settle()
	check(JSON.stringify(fake.to_dict()) == before, "closing unaffordable outfit preserves profile")
	var coins: Control = load("res://game/coin_shop_view.gd").new()
	coins.instant = true
	main.set_screen(coins)
	coins.setup(fake.coins)
	for pack in coins.PACKS:
		coins.show_purchase(pack, func(_bought): pass)
		modal = coins.get_node("Popup")
		_skin(modal, "feather " + pack.id)
		coins.show_purchase(pack, func(_bought): pass)
		check(coins.get_node("Popup") == modal, "repeated feather preview keeps one modal " + pack.id)
		check(modal._title_id == "coins_buy" and modal.content.get_node("Number").get_meta("value") == "+%d" % pack.coins,
			"coin purchase card title and exact amount " + pack.id)
		var confirm: Button = modal.content.get_node("Confirm")
		check(confirm.position.y == modal.panel_rect.end.y + load("res://game/popup.gd").ACTION_DROP and not confirm.get_rect().intersects(modal.panel_rect),
			"existing preview Confirm and hit area are wholly outside the frame " + pack.id)
		check(confirm.size == coins.CONFIRM_SIZE and confirm.get_rect().end.y <= 1920,
			"Confirm is the pack price at the primary height and fits the viewport " + pack.id)
		modal.content.get_node("Confirm").pressed.emit()
		await _settle()
		check(coins.get_node_or_null("Popup") == null and JSON.stringify(fake.to_dict()) == before,
			"confirm closes the card; paying is the caller's " + pack.id)
	# Through the game: a price opens the card, Confirm pays through PaymentService and the coins arrive.
	var before_coins: int = main.profile.coins
	main.open_coin_shop(before_coins)
	var shop: Control = main.get_node("CoinShop")
	shop.get_node("ApprovedLayout/Pack_pouch/Buy_pouch").pressed.emit()
	await _settle()
	shop.get_node("Popup").content.get_node("Confirm").pressed.emit()
	await _settle()
	check(main.profile.coins == before_coins + 3600, "paying adds the pack's coins")
	check(str(shop.get_node("ApprovedLayout/BalanceNumber").get_meta("value")) == str(mini(before_coins + 3600, load("res://game/feather_bar.gd").DISPLAY_MAX)),
		"the page shows the new balance")
	main.queue_free()
	await process_frame
	print("purchase dialog checks: %d checks, %d failures" % [checks, failures])
	quit(1 if failures else 0)
