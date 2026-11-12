class_name PurchaseDialog
extends RefCounted
## Purchase-only cartoon container; shared modal and finalized page skins stay unchanged.

static func open(host: Control, title_id: String, heights: Array, instant: bool,
		action_height := 0.0) -> Modal:
	var dialog := ShopSkin.product_modal(host, title_id, heights, instant, action_height)
	var close := MenuNavigation.replace_close(dialog)
	close.position.y = dialog.panel_rect.position.y - Modal.ACTION_DROP - close.size.y
	if title_id == "coins.preview":
		# This existing title has a soft alpha halo around its painted ink; keep its
		# pixels intact while bringing the visible lettering up to the other headings.
		dialog.content.get_node("TitleGroup").get_child(0).scale = Vector2.ONE * 1.9
	DialogCartoonSkin.apply(dialog)
	return dialog

static func glow(parent: Control, picture: Rect2) -> void:
	var glow := UI.image(parent, "glow_soft", picture.grow(picture.size.x * 0.14))
	glow.name = "ProductGlow"
	glow.modulate = Color(1.0, 0.86, 0.42, 0.58)
