extends SceneTree
## A level card has one wide, centered action; the map's action returns after dismissal.
var failures := 0
var checks := 0
func check(ok: bool, label: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		print("FAIL ",label)
func _initialize() -> void:
	_run.call_deferred()
func _run() -> void:
	var main: Control = load("res://game/main.tscn").instantiate()
	main.args["preview"] = "map_view:cards"
	root.add_child(main)
	await process_frame
	main.args["capture"] = "1"
	main.profile.highest_unlocked = Levels.COUNT
	var popups = load("res://game/popups.gd")
	for locale in ["en","zh"]:
		UiText.set_language(locale)
		main.open_map()
		await process_frame
		var map_play: Control = main.screen.get_node("Play")
		main.show_level_card(71)
		await process_frame
		var card = main.popups.get_child(main.popups.get_child_count()-1)
		var play: Control = card.content.get_node("Play")
		check(play.size == popups.LEVEL_PLAY,"shared level action size")
		check(is_equal_approx(play.get_rect().get_center().x,540.0),"action centered")
		check(play.position.y > card.panel_rect.end.y,"action outside card")
		check(not map_play.visible,"underlying action hidden")
		check(not play.disabled,"unlocked action enabled")
		check(play.get_node("Face").size == play.size,"painted face fills hit bounds")
		card.close()
		await create_timer(0.35).timeout
		await process_frame
		check(map_play.visible,"map action restored")
	main.queue_free()
	await process_frame
	print("Level card action: %d checks, %d failures" % [checks,failures])
	quit(1 if failures else 0)
