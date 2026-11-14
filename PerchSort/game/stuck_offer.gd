class_name StuckOffer
extends RefCounted
## "Stuck": shown once per level attempt when the position can never be won. The purchase card with the undo
## picture and one green action that steps back to the latest position that can still be won, for free.
## Later stuck moments get the pointer at the rescuing tool instead (BoardView.show_stuck_hint).

const PICTURE := 192.0
const CARD_WIDTH := 760.0
const ACTION_GAP := 32.0

static func open(host: Control, on_rescue: Callable, instant: bool) -> Modal:
	var p := JungleDialog.open(host, "stuck", "", [PICTURE], instant, ButtonKit.PRIMARY.y, true, false, true, true, CARD_WIDTH)
	var c := p.content
	var ys: Array = p.get_meta("ys")
	var picture := Rect2(540.0 - PICTURE * 0.5, ys[0], PICTURE, PICTURE)
	UI.art(c, UI.PICTURES["undo"], picture).name = "Picture"
	var size := ButtonKit.WIDE
	var y := p.panel_rect.end.y + ACTION_GAP
	var go := ButtonKit.button(c, "Rescue", Rect2(Vector2(540.0 - size.x * 0.5, y), size), "wide", func(): p.invoke(on_rescue))
	# Explicit free rewind wording, without duplicating the body illustration.
	ButtonKit.word(go.get_node("Content"), "white_free_rewind", size).name = "Word"
	p.panel_rect = p.get_meta("card_rect")
	return p

## A level whose opening has moves that can never be recovered from (preview and tests).
const DEAD_END_LEVEL := {"id": 41, "world": 5, "variant": "morning", "capacity": 4, "par": 12, "branches": [{"side": "R", "row": 0, "seats": 0, "birds": ["sky", "berry", "sunny", "berry"]}, {"side": "L", "row": 0, "seats": 0, "birds": ["berry", "cocoa", "egg:cocoa", "cocoa"]}, {"side": "R", "row": 1, "seats": 0, "birds": ["sky", "sunny", "sunny"]}, {"side": "L", "row": 1, "seats": 0, "birds": ["sky", "sunny"]}, {"side": "R", "row": 2, "seats": 0, "birds": ["berry"]}, {"side": "L", "row": 2, "seats": 0, "birds": ["sky", "cocoa"]}]}

## Fake-data states for captures: "normal" (the offer over a board), "rescue" (DEAD_END_LEVEL after a losing opening move
## and one more; the offer opens, its action is pressed after a moment and the board steps back).
static func preview(main: Control, state: String) -> void:
	var board: Control = load("res://game/board_view.gd").new()
	main.set_screen(board)
	if state != "rescue":
		board.start(Levels.get_level(12), main.profile, false)
		board.session.try_move(2, 3)
		board.refresh_hud()
		open(main.popups, func(): board.rescue(), true)
		return
	board.start(DEAD_END_LEVEL, main.profile, false)
	var made := 0
	for from in board.session.state.branch_count():
		for to in board.session.state.branch_count():
			if made >= 2 or not Rules.can_transfer(board.session.state, from, to):
				continue
			var probe: Board = board.session.state.clone(false)
			Rules.new().transfer(probe, from, to, null)
			if made == 1 or Solver.hint(probe, [], 24000, 5000)["status"] == Solver.Status.UNSOLVABLE:
				board.session.try_move(from, to)
				made += 1
	board._layout_branches()
	board.refresh_hud()
	await main.get_tree().create_timer(1.0).timeout
	var offer := open(main.popups, func(): board.rescue(), false)
	await main.get_tree().create_timer(1.5).timeout
	offer.content.get_node("Rescue").pressed.emit()
