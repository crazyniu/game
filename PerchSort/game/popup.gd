class_name Modal
extends Control
## Shared modal (docs/design/ui-system.md): dim backdrop, one cream panel whose height follows its content, a painted
## title word at the top inside the panel, an optional close button on the panel's top-right corner, and actions
## in a row centred under the panel. Content uses absolute design coordinates; add it to `content`.

signal closed

const WIDTH := 864.0
const TITLE_CAP := 64.0
const TITLE_TOP := 56.0          # panel top to the title's cap top
const BODY_GAP := 40.0           # title cap bottom to the first content group
const BOTTOM := 56.0             # last content group to the panel's visible bottom (above its edge)
const GAP := 32.0                # between content groups
const CLOSE := 96.0
const ACTION_DROP := 48.0        # panel bottom to the action row's top
const ACTION_GAP := 56.0
const DIM := Color(0.118, 0.165, 0.102, 0.55)

var content: Control
var panel_rect: Rect2
var quick := false
var _dim: ColorRect
var _closing := false

## Opens a modal whose panel holds `heights` content groups below an optional title. Returns the modal; read
## `ys` (top y of each group) and `actions_y` (top of the action row) from its metadata.
static func open(host: Control, title: String, heights: Array, close_button := false, action_h := 0.0, above := 0.0,
		instant := false, quick_motion := false) -> Modal:
	var inner := 0.0
	for h in heights:
		inner += h
	inner += GAP * maxf(0.0, heights.size() - 1)
	var head := (TITLE_TOP + TITLE_CAP + BODY_GAP) if not title.is_empty() else BOTTOM
	var panel_h := roundf(head + inner + BOTTOM + 12.0)
	var below := (ACTION_DROP + action_h) if action_h > 0.0 else 0.0
	var top := roundf((1920.0 - (above + panel_h + below)) * 0.5 + above)
	var panel := Rect2((1080.0 - WIDTH) * 0.5, top, WIDTH, panel_h)
	var p := Modal.new()
	p.name = "Popup"
	p.size = Vector2(1080, 1920)
	p.panel_rect = panel
	p.quick = quick_motion
	host.add_child(p)
	host.move_child(p, host.get_child_count() - 1)
	p._build(title, close_button)
	var ys := []
	var y := top + head
	for h in heights:
		ys.append(y)
		y += h + GAP
	p.set_meta("ys", ys)
	p.set_meta("actions_y", panel.end.y + ACTION_DROP)
	if not instant:
		p._appear()
	return p

func _build(title: String, close_button: bool) -> void:
	_dim = ColorRect.new()
	_dim.color = DIM
	_dim.size = size
	_dim.mouse_filter = Control.MOUSE_FILTER_STOP
	add_child(_dim)
	content = Control.new()
	content.name = "Content"
	content.size = size
	content.mouse_filter = Control.MOUSE_FILTER_IGNORE
	content.pivot_offset = panel_rect.get_center()
	add_child(content)
	var panel := UI.shape(content, panel_rect, "cream", 56.0, 12.0)
	panel.name = "Panel"
	if not title.is_empty():
		_title(title, panel_rect.position.y + TITLE_TOP + TITLE_CAP * 0.5)
	if close_button:
		var c := Vector2(panel_rect.end.x - 32.0, panel_rect.position.y + 32.0)
		UI.button(content, "Close", c, "red", "close", close, CLOSE)

## Title word centred on the panel. "Level 12" is the painted word "Level" followed by the painted number.
func _title(title: String, cy: float) -> void:
	var parts := title.split(" ")
	if parts.size() == 2 and parts[0] == "Level" and parts[1].is_valid_int():
		var num_w := UI.number_width(parts[1], TITLE_CAP)
		var w := UI.word_width("Level", TITLE_CAP) + TITLE_CAP * 0.35 + num_w
		var x := 540.0 - w * 0.5
		UI.word(content, "Level", Vector2(x, cy), TITLE_CAP, UI.INK, -1).name = "Title"
		UI.number(content, parts[1], Rect2(x + w - num_w - 1.0, cy - TITLE_CAP * 0.5, num_w + 2.0, TITLE_CAP), UI.INK, 1)
		return
	UI.word(content, title, Vector2(540, cy), TITLE_CAP).name = "Title"

func _appear() -> void:
	Sound.play("ui_tap")
	_dim.modulate.a = 0.0
	content.scale = Vector2.ONE * (0.92 if quick else 0.85)
	content.modulate.a = 0.0
	var tw := create_tween().set_parallel()
	tw.tween_property(_dim, "modulate:a", 1.0, 0.2)
	tw.tween_property(content, "modulate:a", 1.0, 0.15)
	tw.tween_property(content, "scale", Vector2.ONE, 0.18 if quick else 0.3).set_trans(
		Tween.TRANS_QUAD if quick else Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)

## Row of round actions centred under the panel. Each item: [kind ("green" primary or "orange"/"cream"), icon,
## callback]; by default pressing one closes the popup first. Returns the buttons.
func actions(items: Array, closes := true) -> Array:
	var sizes := []
	var total := 0.0
	for it in items:
		var s := UI.PRIMARY if it[0] == "green" else UI.SECONDARY
		sizes.append(s)
		total += s
	total += ACTION_GAP * (items.size() - 1)
	var x := 540.0 - total * 0.5
	var cy: float = get_meta("actions_y") + UI.PRIMARY * 0.5
	var out := []
	for i in items.size():
		var cb: Callable = items[i][2]
		var b := UI.button(content, str(items[i][1]).capitalize(), Vector2(x + sizes[i] * 0.5, cy), items[i][0], items[i][1],
			func():
				if closes:
					close()
				cb.call())
		out.append(b)
		x += sizes[i] + ACTION_GAP
	return out

func close() -> void:
	if _closing:
		return
	_closing = true
	Sound.play("ui_back")
	var t := 0.12
	var tw := create_tween().set_parallel()
	tw.tween_property(self, "modulate:a", 0.0, t)
	tw.tween_property(content, "scale", Vector2.ONE * 0.9, t).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_IN)
	await tw.finished
	closed.emit()
	queue_free()
