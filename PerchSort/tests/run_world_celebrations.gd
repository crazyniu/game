extends SceneTree
## World completion reuses perched results; doors project around fixed hinges.
var checks := 0
var failures := 0
var calls := 0
func check(ok: bool, label: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		print("FAIL ", label)
func _initialize() -> void:
	_run.call_deferred()
func _run() -> void:
	var main = load("res://game/main.tscn").instantiate()
	main.args["profile"] = JSON.stringify(Profile.new().to_dict())
	main.args["preview"] = "map_view:cards"
	root.add_child(main)
	main.args["capture"] = "1"
	await process_frame
	main.args["smoke"] = "1"
	var popups = load("res://game/popups.gd")
	var before := JSON.stringify(main.profile.to_dict())
	for locale in ["zh", "en"]:
		UiText.set_language(locale)
		for world in range(1, 11):
			var page = popups.world_complete(main.popups, world, 500, func(): calls += 1)
			check(page.content.get_node_or_null("gate_arch") == null, "completion has no empty gate")
			check(page.panel_rect.size.x == 880, "completion matches compact result width")
			check(page.content.get_node("Reward/Number").get_meta("value") == "+500", "reward unchanged")
			var perched := 0
			for child in page.content.get_children():
				if child.has_meta("visible_box") and child.get_script() == load("res://game/bird_view.gd"):
					var box: Rect2 = child.get_meta("visible_box")
					check(is_equal_approx(box.end.y, page.panel_rect.position.y + 28), "bird feet meet frame")
					perched += 1
			check(perched == 3, "three perched birds")
			page.content.get_node("Check").pressed.emit()
			page.content.get_node("Check").pressed.emit()
			await process_frame
		check(calls == (20 if locale == "en" else 10), "completion confirmation dispatches once")
	main.args.erase("capture")
	for world in [1, 4, 10]:
		var gate = popups.world_unlock(main.popups, world, func(): calls += 1)
		var left = gate.content.get_node("GateClip/Gate/gate_door_left")
		var right = gate.content.get_node("GateClip/Gate/gate_door_right")
		check(is_equal_approx((left.get_transform() * Vector2(512, 700)).x, (right.get_transform() * Vector2(512, 700)).x), "closed leaves meet at center")
		var last := 1.0
		for frame in range(8):
			await create_timer(0.25).timeout
			check(left.scale.x <= last and left.scale.x > 0, "continuous non-mirrored opening")
			check(is_equal_approx(left.scale.x, right.scale.x), "symmetric doors")
			check(is_equal_approx((left.get_transform() * Vector2(234, 700)).x, 234), "left hinge fixed")
			check(is_equal_approx((right.get_transform() * Vector2(790, 700)).x, 790), "right hinge fixed")
			last = left.scale.x
			if DisplayServer.get_name() != "headless" and world == 4:
				await RenderingServer.frame_post_draw
				root.get_texture().get_image().save_png("res://../build/world-completion/gate_frame_%02d.png" % frame)
		check(is_equal_approx(last, cos(deg_to_rad(82))), "door reaches fully open")
		gate.content.get_node("Play").pressed.emit()
		await create_timer(0.4).timeout
	check(calls == 23, "world play callbacks unchanged")
	check(JSON.stringify(main.profile.to_dict()) == before, "presentation does not modify profile")
	main.queue_free()
	await process_frame
	print("World celebrations: %d checks, %d failures" % [checks, failures])
	quit(1 if failures else 0)
