class_name FeatherBar
extends Control
## The one feather counter every screen shows: the island homepage's gold feather coin, dark pill and green plus
## with painted digits. Digits are plain (no separators) and the display never exceeds DISPLAY_MAX; the cap is
## presentation-only and never written back to the profile. The counter opens the coin-package page.

signal purchase_requested(balance: int)

const DISPLAY_MAX := 99999
const PANEL := "panel_home_island_coin"
## Layout inside the artwork, in the 940 x 1674 homepage reference pixels (panel 350 x 117).
const PANEL_REF := Vector2(350, 117)
const WIDTH := 320.0
const SIZE := Vector2(WIDTH, WIDTH * PANEL_REF.y / PANEL_REF.x)
## The source canvas includes padding past its painted right edge.
const RIGHT_EDGE := 2008.0 / 2172.0
const DIGITS_REF := Rect2(117.5, 40.5, 135, 38)

var amount := -1
var digit_tint := Color.WHITE
var purchase_enabled := true
var _digits: Control


## Every page uses the shop's fixed counter dimensions.
static func create(parent: Node, top_left: Vector2, balance: int, tint := Color.WHITE, can_purchase := true) -> FeatherBar:
	var bar := FeatherBar.new()
	bar.name = "Coins"
	bar.digit_tint = tint
	bar.purchase_enabled = can_purchase
	bar.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(bar, Rect2(top_left, SIZE))
	parent.add_child(bar)
	bar._build()
	bar.set_amount(balance)
	return bar

## Align the visible artwork rather than the source's transparent right padding.
static func create_right(parent: Node, right_top: Vector2, balance: int, tint := Color.WHITE, can_purchase := true) -> FeatherBar:
	return create(parent, right_top - Vector2(WIDTH * RIGHT_EDGE, 0), balance, tint, can_purchase)

## The text the bar shows for a balance: capped, plain digits.
static func display_text(balance: int) -> String:
	return str(clampi(balance, 0, DISPLAY_MAX))

func _build() -> void:
	UI.image(self, PANEL, Rect2(Vector2.ZERO, size), false)
	_digits = Control.new()
	_digits.name = "Digits"
	_digits.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var k := size.x / PANEL_REF.x
	UI.place(_digits, Rect2(DIGITS_REF.position * k, DIGITS_REF.size * k))
	add_child(_digits)
	if purchase_enabled:
		var hit := UI.hit_area(self, Rect2(0, 0, size.x, maxf(112.0, size.y)), func():
			Sound.play("ui_tap")
			purchase_requested.emit(amount))
		hit.name = "BuyCoins"
		hit.button_down.connect(func(): modulate = Island.PRESSED)
		hit.button_up.connect(func(): modulate = Color.WHITE)

## Redraws the painted digits; `bump` adds a short pop when the balance changes on screen.
func set_amount(balance: int, bump := false) -> void:
	var changed := balance != amount
	amount = balance
	var value := display_text(balance)
	set_meta("display_value", value)
	for child in _digits.get_children():
		_digits.remove_child(child)
		child.queue_free()
	Island.digits(_digits, value, Rect2(Vector2.ZERO, _digits.size), digit_tint)
	if bump and changed and is_inside_tree():
		pivot_offset = size * 0.5
		var tw := create_tween()
		tw.tween_property(self, "scale", Vector2.ONE * 1.1, 0.1).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
		tw.tween_property(self, "scale", Vector2.ONE, 0.2).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
