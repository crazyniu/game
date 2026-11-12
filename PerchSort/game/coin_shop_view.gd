extends Control
## Six approved open-space package displays. Purchases remain an explicit simulation.

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
	DrawPurchaseArt.number(page, str(clampi(balance, 0, FeatherBar.DISPLAY_MAX)), Rect2(707, 73, 157, 44)).name = "BalanceNumber"
	DrawPurchaseArt.localized(page, "label_purchase_title", Rect2(135, 115, 648, 199))
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

## A labelled simulation result: no payment call, currency grant, or persisted state.
func show_preview_purchase(pack: Dictionary) -> void:
	if get_node_or_null("Popup") != null:
		return
	var modal := PurchaseDialog.open(self, "coins.preview", [360.0, 88.0], instant, 112.0)
	var ys: Array = modal.get_meta("ys")
	var picture := Rect2(280, ys[0], 520, 360)
	PurchaseDialog.glow(modal.content, picture)
	DrawPurchaseArt.fit(modal.content, "icon_purchase_" + pack["id"], picture)
	DrawPurchaseArt.number(modal.content, "+%d" % pack["coins"], Rect2(280, ys[1], 520, 88))
	var confirm := ShopSkin.product_button(modal.content, "Confirm", Rect2(372, modal.panel_rect.end.y + Modal.ACTION_DROP, 336, 112), modal.close, "green")
	UiText.painted_label(confirm, "action.ok", Rect2(48, 18, 240, 76))

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
	view.purchase_requested.connect(view.show_preview_purchase)
	view.closed.connect(main.open_home)
	Sound.music("bgm_menu")
	if state == "purchase_preview":
		view.show_preview_purchase(PACKS[2])
