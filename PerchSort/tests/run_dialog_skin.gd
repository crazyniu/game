extends SceneTree
## Explicit material replacement never moves or rewires caller-owned UI.
var checks := 0
var failures := 0
func _initialize() -> void:
	_run.call_deferred()
func check(ok: bool, message: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		print("FAIL ", message)
func _run() -> void:
	var skin: GDScript = load("res://game/dialog_cartoon_skin.gd")
	var modal: GDScript = load("res://game/popup.gd")
	var host := Control.new()
	host.size = Vector2(1080, 1920)
	root.add_child(host)
	for language in ["en", "zh"]:
		UiText.set_language(language)
		for height in [300.0, 500.0, 800.0, 1100.0]:
			var view: Control = modal.open(host, "popup.level.title", [height], true, 176.0, 0, true, false, "100")
			var original_panel: Rect2 = view.panel_rect
			var original_title: Control = view.content.get_node("TitleGroup")
			var original_close: Control = view.content.get_node("Close")
			var title_position: Vector2 = original_title.position
			var close_bounds: Rect2 = original_close.get_rect()
			var close_connections := original_close.get_signal_connection_list("pressed")
			for repeated in 2:
				var surface: Control = skin.apply(view)
				var face: NinePatchRect = surface.get_node("Skin")
				check(view.panel_rect == original_panel, "panel geometry preserved")
				check(view.content.get_node("TitleGroup") == original_title and original_title.position == title_position, "title instance and geometry preserved")
				check(view.content.get_node("Close") == original_close and original_close.get_rect() == close_bounds, "Close instance and geometry preserved")
				check(original_close.get_signal_connection_list("pressed") == close_connections, "Close callback connections preserved")
				check(surface.get_rect() == original_panel, "material fits exact caller panel")
				check(face.texture == Art.tex(skin.CARD_ART), "the shop's cream card material")
				check(face.scale == Vector2.ONE * skin.CARD_SCALE, "one corner scale across all sizes")
				check(face.size.is_equal_approx(original_panel.size / skin.CARD_SCALE), "nine-patch scales only edges and center")
				check(face.texture.get_size().x > face.patch_margin_left + face.patch_margin_right and face.texture.get_size().y > face.patch_margin_top + face.patch_margin_bottom, "nine-patch retains positive center area")
				check(view.get_node("ForestBackground").texture == Art.tex(skin.BACKGROUND_ART), "accepted bright forest pixels reused")
				check(view._dim.color.a == 0.0 and view._dim.mouse_filter == Control.MOUSE_FILTER_STOP, "bright background preserves modal input blocking")
				check(surface.mouse_filter == Control.MOUSE_FILTER_IGNORE and face.mouse_filter == Control.MOUSE_FILTER_IGNORE, "new surface never intercepts caller controls")
			view.free()
	host.free()
	await process_frame
	print("dialog skin checks: %d checks, %d failures" % [checks, failures])
	quit(1 if failures else 0)
