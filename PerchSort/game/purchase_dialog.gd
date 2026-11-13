class_name PurchaseDialog
extends RefCounted
## Purchase-only cartoon container: one compact cream card with the gold title sign over its top edge, the close
## button on its corner and the action pill inside the card. Shared modal and finalized page skins stay unchanged.

const CARD_W := 900.0
const HEAD := 120.0
const FOOT := 56.0
const GAP := 24.0
const ACTION_LIFT := 36.0

## `heights` are the content groups; `ys` metadata holds their tops. The caller puts its action pill at
## `panel_rect.end.y + Modal.ACTION_DROP` (inside the visible card); `action_height` reserves its room.
static func open(host: Control, title_key: String, heights: Array, instant: bool,
		action_height := 0.0, width := CARD_W) -> Modal:
	var inner := 0.0
	for h in heights:
		inner += h
	inner += GAP * maxf(0.0, heights.size() - 1)
	var below := (ACTION_LIFT + action_height) if action_height > 0.0 else 0.0
	var total := HEAD + inner + below + FOOT
	var card := Rect2(Vector2((1080.0 - width) * 0.5, roundf((1920.0 - total) * 0.5 + 60.0)), Vector2(width, total))
	var dialog := Modal.open(host, "", [], false, 0.0, 0.0, instant, false, "", false)
	dialog.panel_rect = card
	dialog.content.pivot_offset = card.get_center()
	dialog._dim.color = Modal.POPUP_SHADE
	var old := dialog.content.get_node("Panel")
	dialog.content.remove_child(old)
	old.queue_free()
	var surface := ButtonKit.panel(dialog.content, card)
	dialog.content.move_child(surface, 0)
	dialog._title_id = title_key
	var layer := dialog.content.get_node("TitleGroup")
	dialog.content.remove_child(layer)
	layer.queue_free()
	var sign := ButtonKit.sign(dialog.content, title_key, Rect2(190, card.position.y - 150, 700, 244))
	dialog._title_layer = sign
	MenuNavigation.close(dialog.content, "Close", Vector2(card.end.x - 150, card.position.y + 30), dialog.close)
	var ys := []
	var y := card.position.y + HEAD
	for h in heights:
		ys.append(y)
		y += h + GAP
	dialog.set_meta("ys", ys)
	# The action row sits ACTION_DROP below panel_rect's end, so end the logical rect that far above it.
	var action_y := card.position.y + HEAD + inner + ACTION_LIFT
	dialog.panel_rect = Rect2(card.position, Vector2(card.size.x, action_y - Modal.ACTION_DROP - card.position.y))
	dialog.set_meta("card_rect", card)
	return dialog

static func glow(parent: Control, picture: Rect2) -> void:
	var glow := UI.image(parent, "glow_soft", picture.grow(picture.size.x * 0.22))
	glow.name = "ProductGlow"
	glow.modulate = Color(0.55, 0.95, 0.30, 0.95)
