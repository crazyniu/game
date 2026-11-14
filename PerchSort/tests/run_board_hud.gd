extends SceneTree
## Board HUD layout in every world: the status capsule hugs its content, ends clear of the bird house and centres
## its stars, footprint and count on the capsule body; the tool slots share one size and one painted style.
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
	var main: Control = load("res://game/main.tscn").instantiate()
	var fake := Profile.new()
	fake.muted = true
	fake.music_muted = true
	fake.highest_unlocked = Levels.COUNT
	for kind in Board.Booster.values():
		fake.inventory[kind] = 3
	main.args["profile"] = JSON.stringify(fake.to_dict())
	main.args["preview"] = "coin_shop_view:normal"
	root.add_child(main)
	await process_frame
	var script: GDScript = load("res://game/board_view.gd")
	for world in range(1, 11):
		var board: Control = script.new()
		board.instant = true
		main.set_screen(board)
		board.start(Levels.get_level(world * 10 - 5), main.profile, false)
		await process_frame
		var tag := "world %d" % world
		var status: Control = board._status
		var plate: NinePatchRect = status.get_node("Plate")
		check(plate.texture is AtlasTexture, tag + " jungle capsule uses trimmed artwork")
		check(plate.texture.atlas == Art.tex("panel_hud_jungle"), tag + " approved jungle material")
		check(plate.texture.region == script.CAPSULE_REGION, tag + " transparent padding excluded")
		var house: Rect2 = board._house_rect()
		check(status.position.x + status.size.x <= house.position.x - script.STATUS_HOUSE_CLEAR + 0.5,
			tag + " capsule ends clear of the house (%.0f vs house %.0f)" % [status.position.x + status.size.x, house.position.x])
		check(is_equal_approx(plate.size.y * plate.scale.y, status.size.y), tag + " capsule fills the status height")
		var mid: float = script._capsule_mid(status.size.y)
		for node: Control in board._status_stars + [board._moves_icon, board._moves_number]:
			var centre := node.position.y + node.size.y * 0.5
			check(absf(centre - mid) < 1.5, "%s %s centred on the capsule body (%.1f vs %.1f)" % [tag, node.name, centre, mid])
		var right: float = board._moves_number.position.x + load("res://game/button_kit.gd").number_width("88", script.STATUS_CAP)
		check(right <= status.size.x - script.STATUS_INSET + 0.5, tag + " two-digit count fits inside the capsule")
		var slots: Array = []
		for child in board._tool_bar.get_children():
			if child is Button:
				slots.append(child)
		check(slots.size() >= 2, tag + " tool row present")
		for b: Button in slots:
			check(b.size == slots[0].size, tag + " %s slot same size" % b.name)
			check(b.get_node("Slot").texture == Art.tex("hud_tool_slot"), tag + " %s painted slot" % b.name)
			var icon: TextureRect = b.get_node("Icon")
			check(str(icon.texture.resource_path).get_file().begins_with("island_icon_"), tag + " %s painted icon" % b.name)
	print("board HUD checks: %d checks, %d failures" % [checks, failures])
	quit.call_deferred(1 if failures else 0)
