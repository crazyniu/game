class_name UndoOffer
extends RefCounted
## "More undos": shown when undo is pressed with no undo left in the level attempt. The shop's purchase card with the
## undo picture and "+1"; two equal green actions: the feather price, and watching an ad (TV icon, no words).

const PICTURE := 280.0
const AMOUNT := 88.0

static func open(host: Control, price: int, on_buy: Callable, on_ad: Callable, instant: bool) -> Modal:
	return open_item(host, "undo_more", UI.PICTURES["undo"], price, on_buy, on_ad, instant)

## The same card for one more of any tool: `title_key` is its painted name, `picture` its art. A tool tapped with
## none in stock is offered here, on the board, instead of sending the player to the shop.
static func open_item(host: Control, title_key: String, picture_art: String, price: int, on_buy: Callable,
		on_ad: Callable, instant: bool) -> Modal:
	var p := PurchaseDialog.open(host, title_key, [PICTURE, AMOUNT], instant, ButtonKit.PRIMARY.y, Modal.WIDTH)
	var c := p.content
	var ys: Array = p.get_meta("ys")
	var picture := Rect2(540.0 - PICTURE * 0.5, ys[0], PICTURE, PICTURE)
	PurchaseDialog.glow(c, picture)
	UI.art(c, picture_art, picture).name = "Picture"
	ButtonKit.number(c, "+1", Rect2(340, ys[1], 400, AMOUNT)).name = "Amount"
	var size := ButtonKit.PRIMARY
	var y := p.panel_rect.end.y + Modal.ACTION_DROP
	var left := 540.0 - size.x - Modal.ACTION_GAP * 0.5
	var buy := ButtonKit.button(c, "Buy", Rect2(Vector2(left, y), size), "green", func(): p.invoke(on_buy))
	ButtonKit.price(buy.get_node("Content"), size, price)
	var ad := ButtonKit.button(c, "WatchAd", Rect2(Vector2(540.0 + Modal.ACTION_GAP * 0.5, y), size), "green", func(): p.invoke(on_ad))
	# The TV is as tall as the price's feather and digits on the sibling button and centred the same way, so both faces
	# read as one pair (ButtonKit.price: digits 0.52 of the height, ink centred on the face).
	var icon := size.y * 0.56
	UI.art(ad.get_node("Content"), "icon_watch_ad", Rect2((size - Vector2.ONE * icon) * 0.5, Vector2.ONE * icon)).name = "Icon"
	p.panel_rect = p.get_meta("card_rect")
	return p

## Fake-data state for captures: the offer over a plain host.
static func preview(main: Control, _state: String) -> void:
	var board: Control = load("res://game/board_view.gd").new()
	main.set_screen(board)
	board.start(Levels.get_level(12), main.profile, false)
	board.session.try_move(2, 3)
	board.session.undos_left = 0
	board.refresh_hud()
	if _state == "tool":   # a tool tapped with none in stock
		open_item(main.popups, "item_sprout", UI.PICTURES["sapling"], Progress.booster_price(Board.Booster.SAPLING), func(): pass, func(): pass, true)
		return
	open(main.popups, Progress.UNDO_PRICE, func(): pass, func(): AdService.show_rewarded(main, func(): pass), true)
