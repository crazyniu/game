class_name StuckOffer
extends RefCounted
## "Stuck": shown once per level attempt when the position can never be won. The purchase card with the undo
## picture and one green action that steps back to the latest position that can still be won, for free.
## Later stuck moments get the pointer at the rescuing tool instead (BoardView.show_stuck_hint).

const PICTURE := 280.0

static func open(host: Control, on_rescue: Callable, instant: bool) -> Modal:
	var p := PurchaseDialog.open(host, "stuck", [PICTURE], instant, ButtonKit.PRIMARY.y, Modal.WIDTH)
	var c := p.content
	var ys: Array = p.get_meta("ys")
	var picture := Rect2(540.0 - PICTURE * 0.5, ys[0], PICTURE, PICTURE)
	PurchaseDialog.glow(c, picture)
	UI.art(c, UI.PICTURES["undo"], picture).name = "Picture"
	var size := ButtonKit.PRIMARY
	var y := p.panel_rect.end.y + Modal.ACTION_DROP
	var go := ButtonKit.button(c, "Rescue", Rect2(Vector2(540.0 - size.x * 0.5, y), size), "green", func(): p.invoke(on_rescue))
	# The action wears the same undo picture: "go back", no words and no price.
	var icon := size.y * 0.6
	UI.art(go.get_node("Content"), UI.PICTURES["undo"], Rect2((size - Vector2.ONE * icon) * 0.5, Vector2.ONE * icon)).name = "Icon"
	p.panel_rect = p.get_meta("card_rect")
	return p

## Fake-data states for captures: "normal" (the offer over a board), "rescue" (level 41 after a losing opening move
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
	board.start(Levels.get_level(41), main.profile, false)
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
