extends SceneTree
## Shared shell and scroll bounds for auxiliary document and dormant chapter dialogs.
var checks := 0
var failures := 0
func check(ok: bool, message: String) -> void:
	checks += 1
	if not ok: failures += 1
	print("PASS " if ok else "FAIL ", message)
func _initialize() -> void:
	_run.call_deferred()
func _run() -> void:
	var main: Control = load("res://game/main.tscn").instantiate()
	var fake := Profile.new()
	main.args["profile"] = JSON.stringify(fake.to_dict())
	main.args["preview"] = "coin_shop_view:normal"
	root.add_child(main)
	main.args["smoke"] = "1"
	main.args["capture"] = "1"
	await process_frame
	for language in ["zh", "en"]:
		load("res://game/ui_text.gd").set_language(language)
		for id in ["credits"]:
			var p = main.Popups.document(main.popups, id)
			check(p.content.get_node("Panel/Rim") is NinePatchRect, id + " jungle shell")
			var word: TextureRect = p._title_layer.get_child(-1)
			check(word.texture != null, id + " existing painted localized title")
			var clip: Control = p.content.get_node("Scroll")
			check(clip.get_rect().end.y <= p.panel_rect.end.y - load("res://game/jungle_dialog.gd").FOOT, id + " readable body clears footer")
			var drag := InputEventScreenDrag.new()
			drag.relative.y = -100000
			clip.gui_input.emit(drag)
			var page: Control = clip.get_child(0)
			check(is_equal_approx(page.position.y, -maxf(0, page.size.y - clip.size.y)), id + " scrolling reaches bottom")
			drag.relative.y = 100000
			clip.gui_input.emit(drag)
			check(is_zero_approx(page.position.y), id + " scrolling clamps to top")
			p.queue_free()
			await process_frame
	print("AUX_DIALOGS checks=%d failures=%d" % [checks, failures])
	main.queue_free()
	quit(1 if failures else 0)
