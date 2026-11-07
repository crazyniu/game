extends SceneTree
## First-run evidence for the how-to-play tutorial (docs/work-log/polish-round/tutorial-report.md).
## Opens level 1 twice through the real main scene (_start_level, leaf wipe and entrance), on a sandboxed profile that is never written:
##   xvfb-run -a godot --path PerchSort --audio-driver Dummy --resolution 1080x1920 --fixed-fps 20 \
##     -s $PWD/docs/work-log/polish-round/tutorial_first_run.gd -- --capture=<dir>/unused.png --motion --wait=120 \
##     --out=<dir> --profile='{"speciesMet":["sunny","sky"],"lastDailyGiftDate":99999999}'
## Prints FIRST_RUN lines and saves first_open.png and second_open.png into --out.

var main: Control
var out := ""


func _initialize() -> void:
	for a in OS.get_cmdline_user_args():
		if a.begins_with("--out="):
			out = a.trim_prefix("--out=")
	main = load("res://game/main.tscn").instantiate()
	root.add_child(main)
	_drive()

func _tutorial() -> Node:
	for modal in main.popups.get_children():
		var t: Node = modal.find_child("Tutorial", true, false)
		if t != null and not modal.is_queued_for_deletion():
			return t
	return null

func _settle(seconds: float) -> void:
	await create_timer(seconds).timeout

func _shot(name: String) -> void:
	await RenderingServer.frame_post_draw
	root.get_viewport().get_texture().get_image().save_png(out.path_join(name))

func _drive() -> void:
	await _settle(1.0)
	var flag: String = main.Tutorial.FLAG
	print("FIRST_RUN flag_before=%s" % main.profile.has_tutorial(flag))
	main._start_level(1)
	await _settle(6.0)
	var first := _tutorial()
	print("FIRST_RUN first_open tutorial_shown=%s step=%d flag_after=%s" % [first != null,
		first.step + 1 if first else 0, main.profile.has_tutorial(flag)])
	await _shot("first_open.png")
	if first:
		first.modal.close()
	await _settle(1.0)
	main._open_map()
	await _settle(1.0)
	main._start_level(1)
	await _settle(6.0)
	var second := _tutorial()
	print("FIRST_RUN second_open tutorial_shown=%s popups=%d" % [second != null, main.popups.get_child_count()])
	await _shot("second_open.png")
	quit()
