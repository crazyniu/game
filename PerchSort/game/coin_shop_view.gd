extends Control
## Static coin-package UI shell. The host handles purchase requests through the same view interface.

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
const DISPLAY_SIZE := Vector2(480, 392)
const DISPLAY_ORIGIN := Vector2(48, 352)
const DISPLAY_STEP := Vector2(504, 416)
const PICTURES := {
	"stack": Rect2(120, 32, 240, 208),
	"cluster": Rect2(104, 24, 272, 216),
	"pouch": Rect2(96, 24, 288, 216),
	"basket": Rect2(96, 24, 288, 216),
	"chest": Rect2(88, 16, 304, 224),
	"hoard": Rect2(88, 16, 304, 224),
}
const AMOUNT := Rect2(48, 252, 384, 52)
const PRICE := Rect2(104, 308, 272, 72)
const TITLE := Rect2(280, 208, 520, 112)
const YUAN_INK := Rect2(277, 47, 229, 253)

var instant := false

func setup(balance: int) -> void:
	size = Vector2(1080, 1920)
	mouse_filter = Control.MOUSE_FILTER_STOP
	UI.image(self, ShopSkin.FOREST_BACKGROUND, Rect2(Vector2.ZERO, size), false)
	Island.button(self, "Close", "island_close_tile", Rect2(48, 48, 112, 112), func(): closed.emit())
	FeatherBar.create_right(self, Vector2(1032, 48), balance, ShopSkin.COUNTER_INK, false)
	ShopSkin.product_heading(self, "coins.title", TITLE)
	for i in PACKS.size():
		var pack: Dictionary = PACKS[i]
		var at := DISPLAY_ORIGIN + Vector2(i % 2, i / 2) * DISPLAY_STEP
		var display := ShopSkin.product_card(self, Rect2(at, DISPLAY_SIZE))
		display.name = "Pack_" + pack["id"]
		_package_picture(display, pack["id"], PICTURES[pack["id"]])
		UiText.number(display, str(pack["coins"]), AMOUNT, 52.0, Color.WHITE)
		var button := ShopSkin.product_button(display, "Buy_" + pack["id"], PRICE, func(): purchase_requested.emit(pack))
		_price(button, pack["yuan"])
	if not instant:
		modulate.a = 0.0
		create_tween().tween_property(self, "modulate:a", 1.0, 0.18)

func _price(button: Button, yuan: int) -> void:
	var value := str(yuan)
	var width := UiText.number_width(value, 44.0, "cream")
	var left := (PRICE.size.x - width - 42.0) * 0.5
	var symbol := UI.art(button, "icon_coin_yuan", Rect2(left, 16, 30, 40), YUAN_INK)
	symbol.material = ShopSkin.product_ink()
	UiText.number(button, value, Rect2(left + 42, 14, width, 44), 44.0, Color.WHITE)

func _package_picture(parent: Node, pack_id: String, bounds: Rect2) -> void:
	var picture := UI.art(parent, "icon_coin_" + pack_id, bounds)
	if pack_id == "chest":
		# Exclude two detached atlas remnants without moving or stretching the chest.
		var chest := AtlasTexture.new()
		chest.atlas = picture.texture
		chest.region = Rect2(0, 0, 480, 496)
		picture.texture = chest
		picture.size.x *= 480.0 / 512.0

## A labelled simulation result: no payment call, currency grant, or persisted state.
func show_preview_purchase(pack: Dictionary) -> void:
	var modal := ShopSkin.product_modal(self, "coins.preview", [280.0, 88.0], instant, 112.0)
	var ys: Array = modal.get_meta("ys")
	_package_picture(modal.content, pack["id"], Rect2(280, ys[0], 520, 280))
	UiText.number(modal.content, "+%d" % pack["coins"], Rect2(280, ys[1], 520, 88), 80.0, Color.WHITE)
	var confirm := ShopSkin.product_button(modal.content, "Confirm", Rect2(372, modal.get_meta("actions_y"), 336, 112), modal.close, "green")
	ShopSkin.product_word(confirm, "action.ok", Rect2(48, 18, 240, 76))

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
