class_name MenuNavigation
extends RefCounted
## Explicit non-gameplay navigation. Accepted image pixels and frozen screens stay unchanged.

const CLOSE_ART := "btn_purchase_close"
const BACK_ART := "btn_forest_back"
const DIALOG_RIGHT_PAD := 56.0
const SIZE := Vector2(116, 119) * DrawPurchaseArt.SCALE

static func close(parent: Node, name: String, at: Vector2, callback: Callable,
		coordinate_scale := 1.0) -> Button:
	return DrawPurchaseArt.button(parent, name, CLOSE_ART,
		Rect2(at, SIZE / coordinate_scale), callback)

static func back(parent: Node, name: String, at: Vector2, callback: Callable,
		coordinate_scale := 1.0) -> Button:
	return DrawPurchaseArt.button(parent, name, BACK_ART,
		Rect2(at, SIZE / coordinate_scale), callback)

## Opt in a specific modal; never reskin the shared Modal or finalized Settings globally.
static func replace_close(dialog: Modal) -> Button:
	var old := dialog.content.get_node("Close") as Control
	var at := old.get_rect().get_center() - SIZE * 0.5
	dialog.content.remove_child(old)
	old.queue_free()
	return close(dialog.content, "Close", at, dialog.close)
