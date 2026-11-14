extends Control
## Six approved open-space package displays. A price opens the purchase card; paying goes through PaymentService.

signal closed
signal purchase_requested(pack: Dictionary)

const PACKS := [
	{"id": "stack", "coins": 600, "yuan": 6},
	{"id": "cluster", "coins": 2000, "yuan": 18},
	{"id": "pouch", "coins": 3600, "yuan": 30},
	{"id": "basket", "coins": 9000, "yuan": 68},
	{"id": "chest", "coins": 20000, "yuan": 128},
	{"id": "hoard", "coins": 33000, "yuan": 198},
]
const PICTURES := [Rect2(138, 328, 239, 165), Rect2(511, 323, 286, 175), Rect2(122, 668, 271, 237), Rect2(513, 669, 286, 231), Rect2(119, 1088, 266, 247), Rect2(496, 1073, 325, 265)]
const PRICE_KEYS := ["six", "eighteen", "thirty", "sixty_eight", "one_twenty_eight", "one_ninety_eight"]
const AMOUNT_Y := [506.0, 915.0, 1346.0]
const PRICE_Y := [560.0, 961.0, 1395.0]
const PRICE_SIZE := Vector2(276, 108)

var instant := false

func setup(balance: int) -> void:
	size = Vector2(1080, 1920)
	mouse_filter = Control.MOUSE_FILTER_STOP
	DrawPurchaseArt.background(self)
	var page := DrawPurchaseArt.canvas(self)
	DrawPurchaseArt.button(page, "Close", "btn_purchase_close", Rect2(23, 50, 116, 119), func(): closed.emit())
	DrawPurchaseArt.fit(page, "panel_purchase_balance", Rect2(619, 48, 270, 92)).name = "Balance"
	DrawPurchaseArt.number(page, str(clampi(balance, 0, FeatherBar.DISPLAY_MAX)), BALANCE_NUMBER).name = "BalanceNumber"
	# The shared light-green title banner with painted lettering, as every other page title.
	ButtonKit.sign(page, "coin_shop_title", Rect2(118, 64, 680, 250))
	for i in PACKS.size():
		var pack: Dictionary = PACKS[i]
		var display := Control.new()
		display.name = "Pack_" + pack["id"]
		display.mouse_filter = Control.MOUSE_FILTER_IGNORE
		page.add_child(display)
		UI.image(display, "icon_purchase_" + pack["id"], PICTURES[i])
		var column: int = i % 2
		var row: int = i / 2
		DrawPurchaseArt.number(display, str(pack["coins"]), Rect2(137 + column * 401, AMOUNT_Y[row], 236, 50))
		DrawPurchaseArt.button(display, "Buy_" + pack["id"], "btn_purchase_" + PRICE_KEYS[i], Rect2(Vector2(117 + column * 401, PRICE_Y[row]), PRICE_SIZE), func(): purchase_requested.emit(pack))
	if not instant:
		modulate.a = 0.0
		create_tween().tween_property(self, "modulate:a", 1.0, 0.18)

## The purchase card: pack picture, "+coins" and the pack's own price button. `on_confirm(pack)` runs on the price.
func show_purchase(pack: Dictionary, on_confirm: Callable) -> void:
	if get_node_or_null("Popup") != null:
		return
	var modal := PurchaseDialog.open_shop(self, "coins_buy", [300.0, 88.0], instant, CONFIRM_SIZE.y)
	var ys: Array = modal.get_meta("ys")
	var picture := Rect2(280, ys[0], 520, 300)
	PurchaseDialog.glow(modal.content, picture)
	DrawPurchaseArt.fit(modal.content, "icon_purchase_" + pack["id"], picture)
	ButtonKit.number(modal.content, "+%d" % pack["coins"], Rect2(280, ys[1], 520, 88)).name = "Number"
	var at := Vector2(540.0 - CONFIRM_SIZE.x * 0.5, modal.panel_rect.end.y + Modal.ACTION_DROP)
	var confirm := ButtonKit.button(modal.content, "Confirm", Rect2(at, CONFIRM_SIZE), "wide",
		func(): modal.invoke(on_confirm.bind(pack)))
	# Keep currency and price as painted glyphs, separate from the stretchable button face.
	var price := DrawPurchaseArt.number(confirm.get_node("Content"), "¥%d" % pack["yuan"],
		Rect2(80, (CONFIRM_SIZE.y - 66.0) * 0.5, CONFIRM_SIZE.x - 160, 66))
	var paint := ShaderMaterial.new()
	paint.shader = preload("res://game/chest_play_word.gdshader")
	for glyph in price.get_children():
		glyph.material = paint

const Popups := preload("res://game/popups.gd")
const BALANCE_NUMBER := Rect2(707, 73, 157, 44)
const CONFIRM_SIZE := Vector2(758, 136)   # Standalone action under the 960-pixel purchase card.

## The balance after a purchase.
func set_balance(balance: int) -> void:
	var page := get_node("ApprovedLayout")
	var old := page.get_node("BalanceNumber")
	var at := old.get_index()
	page.remove_child(old)
	old.queue_free()
	var fresh := DrawPurchaseArt.number(page, str(clampi(balance, 0, FeatherBar.DISPLAY_MAX)), BALANCE_NUMBER)
	fresh.name = "BalanceNumber"
	page.move_child(fresh, at)

## A pack was paid for: coins fan out of the middle of the page and fly into the balance, which counts up from
## `before` to `after` and pops as they land.
func celebrate_purchase(before: int, after: int) -> void:
	if instant:
		set_balance(after)
		return
	Sound.play("collect")
	var page := get_node("ApprovedLayout") as Control
	var counter := page.get_node("Balance") as Control
	var layer := Control.new()
	layer.name = "PurchaseCoins"
	layer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	page.add_child(layer)
	var target := counter.position + Vector2(counter.size.y * 0.5, counter.size.y * 0.5)
	Popups.coin_flight(layer, page.size * 0.5, target, 0.25, func():
		counter.pivot_offset = counter.size * 0.5
		var pop := counter.create_tween()
		pop.tween_property(counter, "scale", Vector2.ONE * 1.12, 0.12).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
		pop.tween_property(counter, "scale", Vector2.ONE, 0.25).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
		var count := create_tween()
		count.tween_method(func(v: float): set_balance(roundi(v)), float(before), float(after), 0.7))
	get_tree().create_timer(1.8).timeout.connect(layer.queue_free)

## Fake-data states use the canonical page and its purchase-request contract.
static func preview(main: Control, state: String) -> void:
	var view: Control = load("res://game/coin_shop_view.gd").new()
	view.instant = main.instant()
	main.set_screen(view)
	var balance := 1280
	if state == "empty_balance":
		balance = 0
	elif state == "max_balance":
		balance = FeatherBar.DISPLAY_MAX
	view.setup(balance)
	var paid := func(pack: Dictionary):
		balance += int(pack["coins"])
		view.celebrate_purchase(balance - int(pack["coins"]), balance)
	view.purchase_requested.connect(func(pack): view.show_purchase(pack, paid))
	view.closed.connect(main.open_home)
	Sound.music("bgm_menu")
	if state == "purchase_preview":
		view.show_purchase(PACKS[2], paid)
