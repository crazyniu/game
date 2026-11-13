extends SceneTree
## Outside-frame Buy placement and real viewport hit tests over canonical preview states.

const STATES := ["confirm_booster", "confirm_multiple", "confirm_maximum", "insufficient",
	"confirm_nest", "confirm_nest_short", "confirm_outfit", "confirm_outfit_short"]
var checks := 0
var failures := 0
var main: Control
var evidence := ""
var gui := false
var outcomes: Array = []

func _initialize() -> void:
	root.content_scale_size = Vector2i(1080, 1920)
	root.content_scale_mode = Window.CONTENT_SCALE_MODE_VIEWPORT
	_run.call_deferred()

func check(ok: bool, label: String) -> void:
	checks += 1
	failures += 0 if ok else 1
	print("PASS " if ok else "FAIL ", label)

func _click(control: Control) -> void:
	var point := control.get_global_rect().get_center()
	var motion := InputEventMouseMotion.new()
	motion.position = point
	root.push_input(motion, true)
	var press := InputEventMouseButton.new()
	press.position = point
	press.button_index = MOUSE_BUTTON_LEFT
	press.pressed = true
	root.push_input(press, true)
	var release := press.duplicate()
	release.pressed = false
	root.push_input(release, true)
	await create_timer(0.24).timeout

func _run() -> void:
	gui = DisplayServer.get_name() != "headless"
	for arg in OS.get_cmdline_user_args():
		if arg.begins_with("--purchase-placement-evidence="):
			evidence = arg.trim_prefix("--purchase-placement-evidence=")
	if not evidence.is_empty():
		DirAccess.make_dir_recursive_absolute(evidence)
	main = load("res://game/main.tscn").instantiate()
	var initial := Profile.new()
	initial.muted = true
	initial.music_muted = true
	main.args["profile"] = JSON.stringify(initial.to_dict())
	main.args["preview"] = "shop_view:boosters"
	root.add_child(main)
	main.args["smoke"] = "1"
	main.args["capture"] = "1"
	await process_frame
	var shop_script: GDScript = load("res://game/shop_view.gd")
	var modal_script: GDScript = load("res://game/popup.gd")
	for language in ["en", "zh"]:
		UiText.set_language(language)
		for state in STATES:
			shop_script.preview(main, state)
			await process_frame
			var shop: Control = main.screen
			var outfit: bool = state.begins_with("confirm_outfit")
			var host: Control = shop._outfit_page if outfit else shop
			var modal: Control = host.get_node("Popup")
			var buy: Button = modal.content.get_node("Buy")
			var bounds := buy.get_rect()
			var panel_bounds: Rect2 = modal.panel_rect
			var label: String = state + " " + language
			var unavailable: bool = state == "insufficient" or state.ends_with("_short")
			var expected_size: Vector2 = load("res://game/button_kit.gd").WIDE
			check(is_equal_approx(bounds.position.y, modal.panel_rect.end.y + modal_script.ACTION_DROP), label + " Buy is below unchanged frame")
			check(not bounds.intersects(modal.panel_rect), label + " hit area does not overlap frame")
			check(bounds.size == expected_size and is_equal_approx(bounds.get_center().x, 540.0), label + " size and horizontal center preserved")
			check(bounds.end.y <= main.size.y and bounds.position.x >= 0 and bounds.end.x <= main.size.x, label + " entire Buy remains on screen")
			check(buy.disabled == unavailable, label + " affordability state preserved")
			if not buy.disabled:
				buy.button_down.emit()
				buy.button_up.emit()
			check(buy.get_rect() == bounds, label + " pressed geometry unchanged")
			var quantity: int = 1 if outfit else shop._quantity
			var item: Dictionary = Shop.item("outfit.berry.3") if outfit else shop._purchase_item
			var cost: int = item.price * quantity
			var before := JSON.stringify(shop.profile.to_dict())
			var coins_before: int = shop.profile.coins
			if gui and not evidence.is_empty():
				await create_timer(0.35).timeout
				await RenderingServer.frame_post_draw
				root.get_texture().get_image().save_png(evidence.path_join(state + "-" + language + ".png"))
			await _click(buy)
			if unavailable:
				check(is_instance_valid(modal) and JSON.stringify(shop.profile.to_dict()) == before, label + " real disabled hit does not purchase")
				await _click(modal.content.get_node("Close"))
				check(host.get_node_or_null("Popup") == null, label + " Close still dismisses")
			else:
				check(host.get_node_or_null("Popup") == null, label + " real outside-frame Buy hit invokes callback")
				check(shop.profile.coins == coins_before - cost, label + " exact existing quantity and price charged")
			outcomes.append({"state": state, "locale": language, "buy": [bounds.position.x, bounds.position.y, bounds.size.x, bounds.size.y],
				"panel": [panel_bounds.position.x, panel_bounds.position.y, panel_bounds.size.x, panel_bounds.size.y],
				"disabled": unavailable, "cost": cost, "coins_before": coins_before, "coins_after": shop.profile.coins})
	if not evidence.is_empty():
		var file := FileAccess.open(evidence.path_join("placement-results.json"), FileAccess.WRITE)
		file.store_string(JSON.stringify({"checks": checks, "failures": failures, "gui": gui, "outcomes": outcomes}, "\t"))
	main.queue_free()
	await process_frame
	print("purchase button checks: %d checks, %d failures" % [checks, failures])
	quit(1 if failures else 0)
