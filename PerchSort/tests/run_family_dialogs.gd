extends SceneTree
## The canonical modal family with fake data; no profile writes or rule changes.

var checks := 0
var failures := 0
var calls := 0
var main: Control
var popups: GDScript
var shop: GDScript

func check(ok: bool, label: String) -> void:
	checks += 1
	failures += 0 if ok else 1
	print("PASS " if ok else "FAIL ", label)

func _initialize() -> void:
	_run.call_deferred()

func _disposed(dialog: Control) -> void:
	var reference: WeakRef = weakref(dialog)
	var deadline := Time.get_ticks_msec() + 2000
	while reference.get_ref() != null and Time.get_ticks_msec() < deadline:
		await process_frame
	check(reference.get_ref() == null, "modal exit disposes all owned nodes")

func _family(dialog: Control, expected_close: bool) -> void:
	check(dialog.has_node("ForestBackground"), "independent jungle background")
	check(dialog.content.get_node("Panel/Skin") is NinePatchRect, "forest panel preserves fixed corner material")
	check(dialog.content.has_node("Close") == expected_close, "original close semantics")
	check(dialog.panel_rect.end.y <= 1920, "panel stays in design viewport")
	check(dialog.get_node("ForestBackground").texture == Art.tex(load("res://game/dialog_cartoon_skin.gd").BACKGROUND_ART), "same accepted bright Aviary forest background")
	var skin: NinePatchRect = dialog.content.get_node("Panel/Skin")
	var cartoon: GDScript = load("res://game/dialog_cartoon_skin.gd")
	var reference: Control = cartoon.panel(dialog.content, dialog.panel_rect)
	var reference_skin: NinePatchRect = reference.get_node("Skin")
	check(skin.texture == reference_skin.texture, "same fixed independent cartoon border and inner field")
	check(skin.texture.atlas == Art.tex(cartoon.PANEL_ART) and skin.texture.region == cartoon.PANEL_CONTENT, "self-reviewed independent panel and exact alpha region")
	check(skin.scale == reference_skin.scale and skin.size == reference_skin.size, "same border scale and inner field extent at equal bounds")
	check(Vector4(skin.patch_margin_left, skin.patch_margin_top, skin.patch_margin_right, skin.patch_margin_bottom) == Vector4(reference_skin.patch_margin_left, reference_skin.patch_margin_top, reference_skin.patch_margin_right, reference_skin.patch_margin_bottom), "same fixed edge and corner patches")
	check(skin.self_modulate == Color.WHITE and dialog._dim.color.a == 0.0, "neither border, inner field nor forest is darkened or recolored")
	reference.free()
	check(not dialog.content.get_node("TitleGroup") is TextureRect, "localized transparent heading group")
	check(dialog._title_number.is_empty(), "legacy Modal cannot recreate the wooden title on locale changes")
	check(dialog.content.get_node("TitleGroup").size == Vector2(680, 112), "every heading has the same box")
	for star in dialog.content.find_children("RatingStar*", "TextureRect", false, false):
		var bounds: Rect2 = star.get_meta("visible_box")
		check(bounds.size == Vector2.ONE * load("res://game/family_dialog.gd").STAR_SIZE, "every rating star has equal visible bounds")
	for button in dialog.content.find_children("*", "Button", false, false):
		if button.name == "Close":
			continue
		check(button.get_node("Face").texture == Art.tex("home_play_plate"), "every action has the accepted Home face")
		var family: GDScript = load("res://game/family_dialog.gd")
		check(button.size.is_equal_approx(family.SECONDARY if str(button.name) in ["Home", "Retry"] else family.PRIMARY), "same-role action size is exact in every state: %s %s" % [button.name, button.size])
		check(button.position.y >= dialog.panel_rect.end.y + family.ACTION_CLEARANCE - 0.01, "all action rows are outside the frame with the canonical clear gap")
		check(not button.get_rect().intersects(dialog.panel_rect), "action hit area does not overlap the panel")
		check(button.get_child_count() == 2, "every action contains only its accepted face and painted word")
		var word: TextureRect = button.get_child(1)
		var word_bounds: Rect2 = word.get_meta("visible_ink") if word.has_meta("visible_ink") else word.get_rect()
		check(word_bounds.get_center().distance_to(button.size * 0.5) < 1.0, "action word is centered by visible ink")
		check(word_bounds.position.x >= 48 and word_bounds.end.x <= button.size.x - 48 + 0.01, "action word retains side clearance")
		if button.name == "Retry":
			check(word.name == "ReplayWord" and word.texture.atlas == Art.tex("label_dialog_replay_" + UiText.locale()), "Replay is the new independently painted localised word, never the icon")
		elif button.name == "Home":
			check(word.painted_id == "white_settings_home", "Home reuses the exact accepted localised white word")

	var secondary: Vector2 = load("res://game/family_dialog.gd").SECONDARY
	var primary: Vector2 = load("res://game/family_dialog.gd").PRIMARY
	check(primary.x / secondary.x <= 1.25 and primary.y / secondary.y <= 1.25, "primary remains at most 1.25 times secondary")
	if expected_close:
		check(dialog.content.get_node("Close/Face").texture == Art.tex("btn_purchase_close"), "coin-shop close picture")
	for button in dialog.content.find_children("*", "Button", true, false):
		check(button.get_rect().end.y <= 1920, "action remains visible")

func _run() -> void:
	main = load("res://game/main.tscn").instantiate()
	var fake := Profile.new()
	fake.muted = true
	fake.music_muted = true
	fake.coins = 1280
	fake.highest_unlocked = Levels.COUNT
	for species_id in Board.SPECIES:
		fake.met[species_id] = true
	fake.set_tutorial("tutorial.how_to_play")
	for info in Progress.BOOSTERS:
		fake.set_tutorial("unlock.%s" % info[1])
	main.args["profile"] = JSON.stringify(fake.to_dict())
	main.args["preview"] = "map_view:cards"
	root.add_child(main)
	main.args["smoke"] = "1"
	main.args["capture"] = "1"
	await process_frame
	var before := JSON.stringify(main.profile.to_dict())
	popups = load("res://game/popups.gd")
	shop = load("res://game/shop_skin.gd")
	var on_action := func(): calls += 1
	var species: Array[String] = ["sunny", "sky", "berry", "mint", "cocoa", "snow", "ember"]
	var mechanics: Array[String] = ["sleep", "egg", "daynight"]
	for language in ["en", "zh"]:
		UiText.set_language(language)
		for earned in range(1, 4):
			var win: Control = popups.win(main.popups, earned, 12, 12, 120, [0, 3, 0], on_action, on_action, on_action, earned > 1)
			# Win uses the purchase card of the shop (ButtonKit) under the same jungle backdrop as the family.
			check(win.has_node("ForestBackground"), "independent jungle background")
			check(win.content.get_node("Panel/Skin") is NinePatchRect, "cream card keeps fixed corners")
			check(win.content.get_node("TitleGroup/Sign") != null, "hanging wooden title sign")
			check(not win.content.has_node("Close"), "win has no close button")
			var birds: Array[Node] = win.content.find_children("*", "BirdView", false, false)
			check(birds.size() == 3, "win preserves all three animated birds")
			var centers: Array[float] = []
			for bird in birds:
				var box: Rect2 = bird.get_meta("visible_box")
				centers.append(box.get_center().x)
				var sign: Control = win.content.get_node("TitleGroup")
				check(is_equal_approx(box.end.y, sign.position.y + sign.size.y * popups.WIN_PERCH), "all three birds stand on the sign's branch")
				check(box.position.y >= 0 and bird.get_index() > sign.get_index(), "birds stand in front of the sign")
				check(bird._body.scale.x > 0 and bird._sprite.scale.x > 0, "existing live bird is not mirrored")
				var front_ink: Rect2 = load("res://game/ui.gd").used_rect(bird.species + "_front")
				var front_end: Vector2 = bird.position + (bird._sprite.offset + front_ink.end) * bird._body.scale
				check(is_equal_approx(front_end.y, box.end.y), "actual front-frame ink lands on the designed footline")
				var frames: Array = [bird._rest]
				for sequence in bird._seqs.values():
					frames.append_array(sequence)
				for texture in frames:
					var ink: Rect2i = texture.get_image().get_used_rect()
					var end_y: float = bird.position.y + (bird._sprite.offset.y + ink.end.y) * bird._body.scale.y * 1.02
					check(end_y < win.content.get_node("TitleGroup").position.y + win.content.get_node("TitleGroup").size.y * 0.3, "pose frames keep their feet on the branch, clear of the lettering")
			centers.sort()
			check(centers == [300.0, 540.0, 780.0], "bird centers are symmetric around the title")
			var retry: Control = win.content.get_node("Retry")
			var next: Control = win.content.get_node("action_next")
			check(next.size.y <= retry.size.y * 1.25 + 0.01, "Next is at most 1.25 times its sibling keys")
			check(retry.size == win.content.get_node("Home").size, "Replay and Home keys are identical")
			var reward: Control = win.content.get_node("Reward")
			check(reward.get_children().size() == 2, "feather reward has no backing")
			check(reward.get_node("Number").get_meta("value") == "+120", "win reward quantity")
			check(win.content.get_node("action_next").disabled == (earned == 1), "next availability")
			var old := calls
			win.content.get_node("Retry").pressed.emit()
			win.content.get_node("Home").pressed.emit()
			check(calls == old + 1, "repeated win actions dispatch exactly once")
			await _disposed(win)
		var zero: Control = popups.win(main.popups, 3, 12, 12, 0, [], on_action, on_action, on_action, true)
		check(not zero.content.has_node("Reward"), "zero reward suppresses reward group")
		zero.close()
		await _disposed(zero)
		for playable in [false, true]:
			var level: Control = popups.level_card(main.popups, Levels.COUNT, 3, 20, 12, species, mechanics, on_action, playable)
			_family(level, true)
			check(level.content.get_node("Play").disabled == not playable, "level play availability")
			var old := calls
			level.content.get_node("Close").pressed.emit()
			await _disposed(level)
			check(calls == old, "level close never starts play")
		# A language switch while the level card is open must not rebuild the old flower plank.
		var translated: Control = popups.level_card(main.popups, Levels.COUNT, 2, 20, 12, species, mechanics, on_action, true)
		UiText.set_language("zh" if language == "en" else "en")
		await process_frame
		check(translated.content.get_node("TitleGroup").has_node("TitleNumber"), "live locale change retains level ordinal")
		check(translated.content.get_node("TitleGroup").get_child_count() == 2, "live locale change does not restore old title decorations")
		translated.close()
		await _disposed(translated)
		UiText.set_language(language)
		var friend: Control = popups.new_friend(main.popups, "berry", 25, on_action)
		_family(friend, false)
		check(friend.content.get_node("Reward/Number").value == "+25", "new friend reward quantity")
		for ability_language in ["en", "zh"]:
			UiText.set_language(ability_language)
			await process_frame
			var ability: Control = friend.content.get_node("Ability")
			check(ability.get_child_count() == 2, "localized ability remains one badge and one painted word")
			var word: Control = ability.get_child(0)
			var badge: Control = ability.get_child(1)
			var used: Rect2 = load("res://game/ui.gd").used_rect("gift_coin_encore")
			var shown_left: float = badge.position.x + used.position.x * badge.size.x / badge.texture.get_width()
			check(absf(shown_left + word.position.x + word.size.x - ability.size.x) < 1.0, "ability icon and word are centered by visible ink")
		UiText.set_language(language)

		friend.content.get_node("Check").pressed.emit()
		await _disposed(friend)
		for kind in 6:
			var unlock: Control = popups.unlock(main.popups, kind, on_action)
			_family(unlock, false)
			unlock.content.get_node("Check").pressed.emit()
			await _disposed(unlock)
		for world in [1, 4, 10]:
			var complete: Control = popups.world_complete(main.popups, world, 200, on_action)
			_family(complete, false)
			check(complete.content.get_node("Reward/Number").value == "+200", "world reward quantity")
			complete.close()
			await _disposed(complete)
			var unlock: Control = popups.world_unlock(main.popups, world, on_action)
			_family(unlock, false)
			check(unlock.content.get_node("GateClip/Gate/gate_door_left").position.x == -247.0, "instant gate arrives fully open")
			unlock.content.get_node("Play").pressed.emit()
			await _disposed(unlock)
		for boosters in [[], [0, 0, 1], [0, 1, 2, 3, 4, 5]]:
			var result: Control = popups.rewards(main.popups, "challenge.title", 300 if not boosters.is_empty() else 0, boosters, on_action)
			_family(result, false)
			var old := calls
			result.content.get_node("action_ok").pressed.emit()
			result.content.get_node("action_ok").pressed.emit()
			check(calls == old + 1, "challenge repeated confirm dispatches once")
			await _disposed(result)
	check(JSON.stringify(main.profile.to_dict()) == before, "all presentation paths preserve profile")
	# An interrupted live celebration must not dispatch a delayed action.
	main.args.erase("capture")
	var live: Control = popups.win(main.popups, 3, 12, 12, 120, [0, 3], on_action, on_action, on_action, true)
	var old := calls
	live.queue_free()
	await create_timer(0.5).timeout
	check(calls == old and main.popups.get_child_count() == 0, "interrupted live celebration has no callback or overlay")
	var sound := root.get_node("Sound")
	sound.set_music_muted(true)
	for child in sound.get_children():
		if child is AudioStreamPlayer:
			child.stop()
	main.queue_free()
	await process_frame
	print("family dialog checks: %d checks, %d failures" % [checks, failures])
	quit(1 if failures else 0)
