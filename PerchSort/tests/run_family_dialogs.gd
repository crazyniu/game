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

## The family wears the shop's style: home jungle backdrop, cream card (ButtonKit.PANEL), hanging sign title with
## painted lettering, green ButtonKit actions inside the card.
func _family(dialog: Control, expected_close: bool) -> void:
	check(dialog.has_node("ForestBackground"), "independent jungle background")
	check(dialog.get_node("ForestBackground").texture == Art.tex(load("res://game/dialog_cartoon_skin.gd").BACKGROUND_ART), "home jungle backdrop")
	var skin: NinePatchRect = dialog.content.get_node("Panel/Skin")
	check(skin.texture == Art.tex(load("res://game/dialog_cartoon_skin.gd").CARD_ART), "the shop's cream card")
	check(skin.self_modulate == Color.WHITE and dialog._dim.color == dialog.POPUP_SHADE, "card is not recolored; the backdrop takes the shared popup shade")
	check(dialog.content.has_node("Close") == expected_close, "original close semantics")
	check(dialog.panel_rect.end.y <= 1920, "panel stays in design viewport")
	var title: Control = dialog.content.get_node("TitleGroup")
	check(title.has_node("Sign"), "hanging sign title")
	check(title.size == load("res://game/family_dialog.gd").SIGN_RECT.size, "every sign has the same box")
	check(dialog._title_number.is_empty(), "legacy Modal cannot recreate the wooden title on locale changes")
	for star in dialog.content.find_children("RatingStar*", "TextureRect", false, false):
		var bounds: Rect2 = star.get_meta("visible_box")
		check(bounds.size == Vector2.ONE * load("res://game/family_dialog.gd").STAR_SIZE, "every rating star has equal visible bounds")
	var family: GDScript = load("res://game/family_dialog.gd")
	for button in dialog.content.find_children("*", "Button", false, false):
		if button.name == "Close":
			continue
		check(button.size.is_equal_approx(family.SECONDARY if str(button.name) in ["Home", "Retry"] else family.PRIMARY), "same-role action size is exact in every state: %s %s" % [button.name, button.size])
		check(dialog.panel_rect.encloses(button.get_rect()), "actions sit inside the card")
		check(button.has_node("Face") and button.has_node("Content/Word"), "green plate with a painted word")
	check(family.PRIMARY.x / family.SECONDARY.x <= 1.25 and family.PRIMARY.y / family.SECONDARY.y <= 1.25, "primary remains at most 1.25 times secondary")
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
			check(win.content.get_node("Panel/Skin") is NinePatchRect and win.content.get_node("Panel").visible, "the win stays a popup on the cream card")
			var middle: Control = win.content.get_node("RatingStar1")
			check(middle.get_meta("visible_box").size.x > win.content.get_node("RatingStar0").get_meta("visible_box").size.x and middle.get_meta("visible_box").position.y < win.content.get_node("RatingStar0").get_meta("visible_box").position.y, "the middle star is larger and higher")
			check(win.content.get_node("TitleGroup/Sign") is NinePatchRect, "light-green title banner")
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
				var title_group: Control = win.content.get_node("TitleGroup")
				var lettering: TextureRect = title_group.get_child(1)
				var used: Rect2i = lettering.texture.get_image().get_used_rect()
				var ink_top: float = title_group.position.y + lettering.position.y + used.position.y * lettering.size.y / lettering.texture.get_height()
				var frames: Array = [bird._rest]
				for key in bird._seqs:
					if not str(key).begins_with("flight"):   # flight frames are only shown in the air, while flying in
						frames.append_array(bird._seqs[key])
				for texture in frames:
					var ink: Rect2i = texture.get_image().get_used_rect()
					var end_y: float = bird.position.y + (bird._sprite.offset.y + ink.end.y) * bird._body.scale.y * 1.02
					check(end_y < ink_top, "perched frames keep their feet above the lettering")
			centers.sort()
			check(centers == [300.0, 540.0, 780.0], "bird centers are symmetric around the title")
			var retry: Control = win.content.get_node("Retry")
			var next: Control = win.content.get_node("action_next")
			check(next.size.y <= retry.size.y * 1.25 + 0.01, "Next is at most 1.25 times its sibling keys")
			var home: Control = win.content.get_node("Home")
			check(is_equal_approx(next.position.x, retry.position.x) and is_equal_approx(next.position.x + next.size.x, home.position.x + home.size.x), "Next spans the Replay / Home row edge to edge")
			check(retry.size == home.size, "Replay and Home are the same size")
			check(retry.position.y > next.position.y + next.size.y and is_equal_approx(retry.position.y, home.position.y), "Replay and Home share a row under Next")
			var card_rect: Rect2 = win.get_meta("card_rect")
			check(card_rect.encloses(Rect2(retry.position, retry.size)) and card_rect.encloses(Rect2(home.position, home.size)) and card_rect.encloses(Rect2(next.position, next.size)), "the buttons stay inside the card")
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
			# The level card is a compact purchase-style card over the dimmed map with the home page's Play button.
			check(not level.has_node("ForestBackground"), "level card keeps the map visible behind its shade")
			check(level.content.get_node("Panel/Skin").texture == Art.tex("panel_card_cream"), "level card: the shop's cream card")
			check(level.content.get_node("TitleGroup").has_node("Sign"), "level card: hanging sign title")
			var play: Control = level.content.get_node("Play")
			check(play.size == load("res://game/home_view.gd").PLAY.size, "level Play is the home Play size")
			check(play.get_node("Face").texture == Art.tex("btn_home_island_play_" + UiText.locale()), "level Play is the home Play picture")
			check(level.panel_rect.encloses(play.get_rect()), "Play sits inside the card")
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
		check(translated.content.get_node("TitleGroup").get_child_count() == 3, "live locale change keeps only sign, word and ordinal")
		translated.close()
		await _disposed(translated)
		UiText.set_language(language)
		var friend: Control = popups.new_friend(main.popups, "berry", 25, on_action)
		_family(friend, false)
		check(friend.content.get_node("Reward/Number").get_meta("value") == "+25", "new friend reward quantity")
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
			check(complete.content.get_node("Reward/Number").get_meta("value") == "+200", "world reward quantity")
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
