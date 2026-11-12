extends SceneTree
## The purchase material opt-in may replace only Panel/background, never the existing content geometry.

const STATES := ["confirm_booster", "confirm_multiple", "confirm_maximum", "insufficient",
	"confirm_nest", "confirm_nest_short", "confirm_outfit", "confirm_outfit_short"]
var main: Control
var skin_script: GDScript
var baseline_file := ""
var result_file := ""
var record := false
var checks := 0
var failures := 0
var baselines := {}
var snapshots := {}

func _initialize() -> void:
	root.content_scale_size = Vector2i(1080, 1920)
	root.content_scale_mode = Window.CONTENT_SCALE_MODE_VIEWPORT
	_run.call_deferred()

func check(ok: bool, label: String) -> void:
	checks += 1
	failures += 0 if ok else 1
	print("PASS " if ok else "FAIL ", label)

func _geometry(node: Node, key: String, found: Dictionary) -> void:
	if node.name == "Panel":
		return
	if node is Control:
		found[key] = [node.position.x, node.position.y, node.size.x, node.size.y,
			node.scale.x, node.scale.y, node.pivot_offset.x, node.pivot_offset.y, node.rotation]
	for i in node.get_child_count():
		var child := node.get_child(i)
		_geometry(child, key + "/" + str(i) + ":" + child.get_class(), found)

func _verify(modal: Control, key: String) -> void:
	var snapshot := {"panel": [modal.panel_rect.position.x, modal.panel_rect.position.y,
		modal.panel_rect.size.x, modal.panel_rect.size.y]}
	_geometry(modal.content, "Content", snapshot)
	snapshots[key] = snapshot
	if record:
		return
	var panel: Control = modal.content.get_node("Panel")
	var skin: NinePatchRect = panel.get_node("Skin")
	var background: TextureRect = modal.get_node("ForestBackground")
	check(panel.get_rect() == modal.panel_rect and panel.get_index() == 0,
		key + " replacement panel preserves bounds and content stacking")
	check(skin.texture.atlas == Art.tex(skin_script.PANEL_ART) and
		skin.texture.region == skin_script.PANEL_CONTENT,
		key + " frozen cartoon panel texture and crop are exact")
	check((skin.size * skin.scale).is_equal_approx(modal.panel_rect.size) and
		skin.scale == Vector2.ONE * skin_script.CORNER_SCALE and
		skin.patch_margin_left == int(skin_script.CORNERS.x) and
		skin.patch_margin_top == int(skin_script.CORNERS.y) and
		skin.patch_margin_right == int(skin_script.CORNERS.z) and
		skin.patch_margin_bottom == int(skin_script.CORNERS.w),
		key + " frozen nine-patch measurements are retained")
	check(background.texture == Art.tex(skin_script.BACKGROUND_ART) and
		background.get_rect() == Rect2(0, 0, 1080, 1920) and
		background.stretch_mode == TextureRect.STRETCH_KEEP_ASPECT_COVERED and
		background.get_index() == 0, key + " unchanged forest art fills behind the dialog")
	check(modal._dim.color.a == 0.0, key + " background is not darkened")
	check(panel.mouse_filter == Control.MOUSE_FILTER_IGNORE and
		skin.mouse_filter == Control.MOUSE_FILTER_IGNORE and
		background.mouse_filter == Control.MOUSE_FILTER_IGNORE,
		key + " surface and background do not intercept pointer input")
	check(baselines.has(key), key + " has recorded pre-material geometry")
	if not baselines.has(key):
		return
	var expected: Dictionary = baselines[key]
	check(snapshot.size() == expected.size(), key + " content node structure preserved")
	var same := true
	for path in expected:
		if not snapshot.has(path) or snapshot[path].size() != expected[path].size():
			same = false
			continue
		for i in expected[path].size():
			if absf(snapshot[path][i] - expected[path][i]) > 0.001:
				print("GEOMETRY_MISMATCH ", key, " ", path, " ", expected[path], " -> ", snapshot[path])
				same = false
	check(same, key + " all non-panel content geometry unchanged")

func _run() -> void:
	for arg in OS.get_cmdline_user_args():
		if arg.begins_with("--surface-baseline="):
			baseline_file = arg.trim_prefix("--surface-baseline=")
		elif arg.begins_with("--surface-result="):
			result_file = arg.trim_prefix("--surface-result=")
		elif arg == "--record-surface-baseline":
			record = true
	if baseline_file.is_empty():
		push_error("Supply --surface-baseline for the recorded geometry")
		quit(1)
		return
	if not record:
		baselines = JSON.parse_string(FileAccess.get_file_as_string(baseline_file))
	main = load("res://game/main.tscn").instantiate()
	var fake := Profile.new()
	fake.muted = true
	fake.music_muted = true
	main.args["profile"] = JSON.stringify(fake.to_dict())
	main.args["preview"] = "shop_view:boosters"
	root.add_child(main)
	main.args["capture"] = "1"
	main.args["smoke"] = "1"
	await process_frame
	skin_script = load("res://game/dialog_cartoon_skin.gd")
	var shop_script: GDScript = load("res://game/shop_view.gd")
	var coin_script: GDScript = load("res://game/coin_shop_view.gd")
	for language in ["en", "zh"]:
		UiText.set_language(language)
		for state in STATES:
			shop_script.preview(main, state)
			await process_frame
			# Let the existing insufficient-funds shake return to its unchanged origin.
			if state.ends_with("_short"):
				await create_timer(0.36).timeout
			var shop: Control = main.screen
			var host: Control = shop._outfit_page if state.begins_with("confirm_outfit") else shop
			_verify(host.get_node("Popup"), state + " " + language)
		for pack in coin_script.PACKS:
			coin_script.preview(main, "packages")
			await process_frame
			main.screen.show_preview_purchase(pack)
			await process_frame
			_verify(main.screen.get_node("Popup"), "preview_" + str(pack.id) + " " + language)
	var path := baseline_file if record else result_file
	if not path.is_empty():
		var file := FileAccess.open(path, FileAccess.WRITE)
		file.store_string(JSON.stringify(snapshots if record else {"checks": checks, "failures": failures, "geometry": snapshots}, "\t"))
	main.queue_free()
	await process_frame
	print("purchase surface checks: %d checks, %d failures; %d states" % [checks, failures, snapshots.size()])
	quit(1 if failures else 0)
