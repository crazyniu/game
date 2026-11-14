extends SceneTree
## Compact reward containers preserve dynamic content and exterior actions.
var checks := 0
var failures := 0
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
	main.args["capture"] = "1"
	root.add_child(main)
	await process_frame
	main.args["smoke"] = "1"
	var popups = load("res://game/popups.gd")

	for language in ["zh", "en"]:
		UiText.set_language(language)
		for state in ["win_three_stars", "win_milestone", "win_zero_reward", "rewards_daily", "rewards_daily_all", "rewards_daily_coins", "new_friend_berry", "unlock_owl_eye", "world_complete_1", "world_unlock_2"]:
			popups.preview(main, state)
			await process_frame
			var dialog = main.popups.get_child(main.popups.get_child_count() - 1)
			var card: Rect2 = dialog.panel_rect
			check(is_equal_approx(card.get_center().x, 540), state + " centered")
			check(card.size.x <= 900 if not state.begins_with("world_") else card.size.x == 960, state + " content-specific width")
			check(card.position.y >= 0 and card.end.y <= 1920, state + " visible frame")
			for child in dialog.content.get_children():
				if child is Button and child.name != "Close":
					check(child.position.y >= card.end.y + 40, state + " external action")
					check(child.get_rect().end.y <= 1920, state + " visible action")
				if str(child.name).begins_with("Reward"):
					check(card.encloses(child.get_rect()), state + " reward tile fits")
			dialog.queue_free()
			await process_frame
	main.queue_free()
	await process_frame
	print("Reward dialog spacing: %d checks, %d failures" % [checks, failures])
	quit(1 if failures else 0)
