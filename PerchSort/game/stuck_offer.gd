class_name StuckOffer
extends RefCounted
## "Stuck": shown once per level attempt when the position can never be won.
## In the first forest (FREE_REWIND_WORLD) the card shows a forest illustration and one green action that steps back to
## the latest position that can still be won, for free. From the second forest on it shows the tool that rescues the
## position (the board's pointer stands on it) and one green action that restarts the level for free; tapping the
## shade leaves the pointer. Later stuck moments of the attempt get only the pointer (BoardView.show_stuck_hint).

const PICTURE := 180.0
const PERCH_WIDTH := 320.0
const PERCH_CONTACT_Y := 135.0
const CARD_WIDTH := 760.0
const ACTION_GAP := 32.0
const FREE_REWIND_WORLD := 1

static func open(host: Control, on_rescue: Callable, instant: bool) -> Modal:
	var p := _card(host, "Rescue", "white_free_rewind", on_rescue, instant)
	var c := p.content
	var ys: Array = p.get_meta("ys")
	# A small living perch belongs to the board's forest world; the action explains the rewind.
	var picture := Control.new()
	picture.name = "Picture"
	picture.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(picture, Rect2(540.0 - PERCH_WIDTH * 0.5, ys[0], PERCH_WIDTH, PICTURE))
	c.add_child(picture)
	var k := PERCH_WIDTH / Art.tex("branch_aviary_perch").get_width()
	Island.piece(picture, "branch_aviary_perch", Vector2(0, PERCH_CONTACT_Y - 140.0 * k), PERCH_WIDTH).name = "Perch"
	for i in 2:
		var bird: Node2D = load("res://game/popups.gd")._bird_view(picture, ["sunny", "sky"][i], Rect2(38 + i * 132, 5, 112, 130), instant, 0.0)
		bird.name = "PerchedBird%d" % i
	return p

## The second-forest-on card: `picture` is the rescuing tool's picture name (undo when none rescues).
static func open_restart(host: Control, picture: String, on_restart: Callable, instant: bool) -> Modal:
	var p := _card(host, "Restart", "white_action_replay", on_restart, instant)
	var ys: Array = p.get_meta("ys")
	UI.art(p.content, picture, Rect2(540.0 - PICTURE * 0.5, ys[0], PICTURE, PICTURE)).name = "Picture"
	return p

## The shared frame: title, one wide green action below the frame, and shade dismissal.
static func _card(host: Control, action_name: String, word: String, action: Callable, instant: bool) -> Modal:
	var p := JungleDialog.open(host, "stuck", "", [PICTURE], instant, ButtonKit.PRIMARY.y, false, false, true, true, CARD_WIDTH)
	var c := p.content
	var size := ButtonKit.WIDE
	var y := p.panel_rect.end.y + ACTION_GAP
	var go := ButtonKit.button(c, action_name, Rect2(Vector2(540.0 - size.x * 0.5, y), size), "wide", func(): p.invoke(action))
	# Explicit wording, without duplicating the body illustration.
	ButtonKit.word(go.get_node("Content"), word, size).name = "Word"
	p.panel_rect = p.get_meta("card_rect")
	# No redundant X: tapping the surrounding shade retains dismissal without using the action.
	# The dim layer stops the event, so cancelling never taps the board behind the modal.
	p._dim.gui_input.connect(func(event: InputEvent):
		var pressed: bool = (event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.pressed) or (event is InputEventScreenTouch and event.pressed)
		if pressed:
			if not p.panel_rect.has_point(event.position):
				p.close())
	return p

## A level whose opening has moves that can never be recovered from (preview and tests).
const DEAD_END_LEVEL := {"id": 41, "world": 5, "variant": "morning", "capacity": 4, "par": 12, "branches": [{"side": "R", "row": 0, "seats": 0, "birds": ["sky", "berry", "sunny", "berry"]}, {"side": "L", "row": 0, "seats": 0, "birds": ["berry", "cocoa", "egg:cocoa", "cocoa"]}, {"side": "R", "row": 1, "seats": 0, "birds": ["sky", "sunny", "sunny"]}, {"side": "L", "row": 1, "seats": 0, "birds": ["sky", "sunny"]}, {"side": "R", "row": 2, "seats": 0, "birds": ["berry"]}, {"side": "L", "row": 2, "seats": 0, "birds": ["sky", "cocoa"]}]}

## Fake-data states for captures: "normal" (the offer over a board), "restart" (the second-forest card over a board
## with the Sapling as the rescuing tool), "rescue" (DEAD_END_LEVEL after a losing opening move
## and one more; the offer opens, its action is pressed after a moment and the board steps back).
static func preview(main: Control, state: String) -> void:
	var board: Control = load("res://game/board_view.gd").new()
	main.set_screen(board)
	if state == "restart":
		board.start(Levels.get_level(35), main.profile, false)
		board.session.try_move(2, 3)
		board.refresh_hud()
		open_restart(main.popups, UI.PICTURES[BoardView.TOOL_ICON_NAME[Board.Booster.SAPLING]], func(): board.restart(), true)
		return
	if state != "rescue":
		board.start(Levels.get_level(12), main.profile, false)
		board.session.try_move(2, 3)
		board.refresh_hud()
		open(main.popups, func(): board.rescue(), state != "motion")
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
