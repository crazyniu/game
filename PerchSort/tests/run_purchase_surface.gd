extends SceneTree
## Purchase material contracts; compare historical geometry only when an explicit recorded baseline is supplied.

const STATES := ["confirm_booster", "confirm_multiple", "confirm_maximum", "insufficient",
	"confirm_nest", "confirm_nest_short", "confirm_outfit", "confirm_outfit_short"]
var main: Control
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
	var shared: GDScript = load("res://game/button_kit.gd")
	var purchase: GDScript = load("res://game/purchase_dialog.gd")
	var card: Rect2 = modal.get_meta("card_rect")
	var toy := modal.has_meta("actions_y")
	var painted: Rect2 = card if toy else card.grow_side(SIDE_TOP, purchase.TITLE_ROOM)
	check(panel.get_rect() == painted and panel.get_index() == 0,
		key + " purchase surface preserves its approved shell bounds")
	var corner_scale: float = card.size.x / 1136.0 if toy else shared.PANEL_SCALE
	var margin: int = 150 if toy else shared.PANEL_MARGIN
	if toy:
		check(skin.texture is AtlasTexture and skin.texture.atlas == Art.tex("panel_shop_toy_rim") and
			skin.texture.region == Rect2(60, 80, 1136, 1100) and not skin.draw_center,
			key + " shop uses the approved rigid toy rim")
		var face: Control = panel.get_node("OpaqueFace")
		check(face.mouse_filter == Control.MOUSE_FILTER_IGNORE and panel.get_rect().size.x > face.size.x,
			key + " opaque inset retains a separate noninteractive face")
	else:
		check(skin.texture == Art.tex(shared.PANEL), key + " purchase uses the shared panel material")
	check((skin.size * skin.scale).is_equal_approx(painted.size) and
		skin.scale == Vector2.ONE * corner_scale and
		skin.patch_margin_left == margin and skin.patch_margin_top == margin and
		skin.patch_margin_right == margin and skin.patch_margin_bottom == margin,
		key + " nine-patch preserves its shell dimensions and corner scale")
	check(modal.get_node_or_null("ForestBackground") == null,
		key + " purchase retains its host scenery rather than adding an obsolete full-screen backdrop")
	check(modal._dim.color == modal.POPUP_SHADE and modal._dim.mouse_filter == Control.MOUSE_FILTER_STOP,
		key + " shared popup shade blocks background input")
	check(panel.mouse_filter == Control.MOUSE_FILTER_IGNORE and skin.mouse_filter == Control.MOUSE_FILTER_IGNORE,
		key + " decorative surface does not intercept pointer input")
	if baseline_file.is_empty():
		return
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
	if record and baseline_file.is_empty():
		push_error("Supply --surface-baseline when recording geometry")
		quit(1)
		return
	if not record and not baseline_file.is_empty():
		var loaded = JSON.parse_string(FileAccess.get_file_as_string(baseline_file))
		if not loaded is Dictionary:
			push_error("Geometry baseline must contain a JSON dictionary")
			quit(1)
			return
		baselines = loaded
	elif not record:
		print("Surface contracts only; historical geometry comparison NOT RUN (no baseline supplied)")
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
			main.screen.show_purchase(pack, func(_bought): pass)
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
