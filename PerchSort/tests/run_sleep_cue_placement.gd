extends SceneTree
## Transient sleep explanations must not hide permanent species keys.
var failures := 0
var checks := 0
func check(ok: bool, label: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		print("FAIL ", label)
func _initialize() -> void:
	_run.call_deferred()
func _run() -> void:
	var BoardView = load("res://game/board_view.gd")
	var view = BoardView.new()
	root.add_child(view)
	view._skill_row = Control.new()
	view.add_child(view._skill_row)
	var width = 2.0 * BoardView.BUBBLE_PAD + 2.0 * BoardView.HUD_ICON + BoardView.BUBBLE_ICON_GAP
	var body := Rect2(-BoardView.BUBBLE_PAD - 8.0, -BoardView.BUBBLE_H - 36.0, width, BoardView.BUBBLE_H)
	for count in range(1, 11):
		var side = BoardView.KEY if count <= 8 else BoardView.KEY_SMALL
		var row_width = count * side + (count - 1) * BoardView.KEY_GAP
		view._skill_row.position = Vector2((1080.0 - row_width) * 0.5, 302.0)
		view._skill_row.size = Vector2(row_width, side)
		for x in [88.0, 377.0, 540.0, 864.0]:
			var tip := Vector2(x, 452.0)
			var moved = view._sleep_cue_tip(tip, body)
			var bounds := Rect2(moved + body.position, body.size)
			check(not bounds.grow(6.0).intersects(view._skill_row.get_rect().grow(16.0)), "row clearance %d/%s" % [count,x])
			check(bounds.position.x >= 48.0 and bounds.end.x <= 1032.0, "screen edges")
			check(not bounds.intersects(Rect2(430, 40, 220, 248)), "house clearance")
			check(bounds.position.y >= BoardView.BUBBLE_TOP, "top HUD clearance")
		var lower := Vector2(600,900)
		check(view._sleep_cue_tip(lower,body) == lower, "unobstructed lower cue unchanged")
	view.queue_free()
	print("Sleep cue placement: %d checks, %d failures" % [checks,failures])
	quit(1 if failures else 0)
