extends SceneTree
## Exercise unchanged tutorial actions and the corrected visual bounds without profile writes.
var checks := 0
var failures := 0
var calls := 0

func check(ok: bool, description: String) -> void:
	checks += 1
	if not ok:
		failures += 1
	print("PASS " if ok else "FAIL ", description)

func _initialize() -> void:
	_run.call_deferred()

func _run() -> void:
	var main: Control = load("res://game/main.tscn").instantiate()
	var fake := Profile.new()
	fake.muted = true
	fake.music_muted = true
	main.args["profile"] = JSON.stringify(fake.to_dict())
	main.args["preview"] = "map_view:cards"
	root.add_child(main)
	main.args["smoke"] = "1"
	main.args["capture"] = "1"
	await process_frame
	var before := JSON.stringify(main.profile.to_dict())
	var tutorial: GDScript = load("res://game/tutorial.gd")
	var popups: GDScript = load("res://game/popups.gd")
	for locale in ["en", "zh"]:
		UiText.set_language(locale)
		var t: Control = tutorial.open(main.popups, func(): calls += 1, true)
		for step in tutorial.STEPS:
			check(t.step == step, "tutorial preserves step order")
			var button: Button = t._buttons[0]
			check(button.position.y >= t.modal.panel_rect.end.y + 40, "action below the frame with the shared exterior spacing")
			check(button.position.y + button.size.y <= 1920, "action stays in viewport")
			check(button.get_node("Content").get_child_count() == 1, "one centered painted action")
			button.pressed.emit()
			await process_frame
		await create_timer(0.2).timeout
		check(calls == (1 if locale == "en" else tutorial.LESSONS.size() + 2), "Play invokes completion once")
		for mechanic in tutorial.LESSONS:
			var lesson: Control = tutorial.open_lesson(main.popups, mechanic, 3, func(): calls += 1, true)
			check(lesson.lesson == mechanic, "same lesson identity")
			check(lesson.world == 3, "same lesson world selection")
			lesson._buttons[0].pressed.emit()
			await create_timer(0.2).timeout
		for species in ["berry", "hoot"]:
			var dialog: Control = popups.goal_info(main.popups, species)
			var ability: Control = dialog.content.get_node("Ability")
			check(ability.position.y + ability.size.y <= dialog.panel_rect.end.y, "ability inside panel")
			dialog.content.get_node("Close").pressed.emit()
			await process_frame
	check(calls == (tutorial.LESSONS.size() + 1) * 2, "all completion callbacks preserved")
	check(JSON.stringify(main.profile.to_dict()) == before, "visual actions leave profile unchanged")
	print("gameplay dialog checks: %d checks, %d failures" % [checks, failures])
	quit(0 if failures == 0 else 1)
