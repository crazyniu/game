class_name Modal
extends Control
## Shared modal (docs/design/ui-system.md): dim backdrop, one cream panel whose height follows its content, a painted
## localized title at the top inside the panel, an optional close button on the panel's top-right corner, and actions
## in a row centred under the panel. Content uses absolute design coordinates; add it to `content`.

signal closed

const WIDTH := 864.0
const TITLE_CAP := 64.0
const TITLE_TOP := 56.0          # panel top to the title's cap top
const BODY_GAP := 40.0           # title cap bottom to the first content group
const BOTTOM := 56.0             # last content group to the panel's visible bottom (above its edge)
const GAP := 32.0                # between content groups
const CLOSE := 128.0
const ACTION_DROP := 48.0        # panel bottom to the action row's top
const ACTION_GAP := 56.0
const DIM := Color(0.118, 0.165, 0.102, 0.55)

var content: Control
var panel_rect: Rect2
var quick := false
var _dim: ColorRect
var _closing := false
var _appearance: Tween
var _title_layer: Control
var _title_id := ""
var _title_number := ""
var _has_close := false

## Opens a modal whose panel holds `heights` content groups below an optional title. Returns the modal; read
## `ys` (top y of each group) and `actions_y` (top of the action row) from its metadata.
static func open(host: Control, title_id: String, heights: Array, close_button := false, action_h := 0.0, above := 0.0,
		instant := false, quick_motion := false, title_number := "") -> Modal:
	var inner := 0.0
	for h in heights:
		inner += h
	inner += GAP * maxf(0.0, heights.size() - 1)
	var head := (TITLE_TOP + TITLE_CAP + BODY_GAP) if not title_id.is_empty() else BOTTOM
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
	p._title_id = title_id
	p._title_number = title_number
	p._has_close = close_button
	p._build()
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

func _build() -> void:
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
	_title_layer = Control.new()
	_title_layer.name = "TitleGroup"
	_title_layer.size = size
	_title_layer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	content.add_child(_title_layer)
	if not _title_id.is_empty():
		_title()
	if _has_close:
		var c := Vector2(panel_rect.end.x - 32.0, panel_rect.position.y + 40.0)
		UI.button(content, "Close", c, "red", "close", close, CLOSE)

## Whole-phrase titles have a fixed safe width, clear of the close button. Level numbers remain numeral sprites.
func _title() -> void:
	var cy := panel_rect.position.y + TITLE_TOP + TITLE_CAP * 0.5
	var width := WIDTH - (224.0 if _has_close else 128.0)
	if not _title_number.is_empty():
		var natural := UI.label_width(_title_id, TITLE_CAP) + 24.0 + UI.number_width(_title_number, TITLE_CAP)
		var k := minf(1.0, width / natural)
		var cap := TITLE_CAP * k
		var word_w := UI.label_width(_title_id, cap)
		var num_w := UI.number_width(_title_number, cap)
		var x := 540.0 - natural * k * 0.5
		UI.label(_title_layer, _title_id, Rect2(x, cy - 56.0, word_w, 112.0), cap, UI.INK).name = "Title"
		UI.number(_title_layer, _title_number, Rect2(x + word_w + 24.0 * k, cy - cap * 0.5, num_w, cap), UI.INK)
		return
	UI.label(_title_layer, _title_id, Rect2(540.0 - width * 0.5, cy - 56.0, width, 112.0), TITLE_CAP, UI.INK).name = "Title"

func _notification(what: int) -> void:
	if what == NOTIFICATION_TRANSLATION_CHANGED and _title_layer != null and not _title_number.is_empty():
		for child in _title_layer.get_children():
			_title_layer.remove_child(child)
			child.queue_free()
		_title()

func _appear() -> void:
	Sound.play("ui_tap")
	_dim.modulate.a = 0.0
	content.scale = Vector2.ONE * (0.92 if quick else 0.85)
	content.modulate.a = 0.0
	_appearance = create_tween().set_parallel()
	_appearance.tween_property(_dim, "modulate:a", 1.0, 0.2)
	_appearance.tween_property(content, "modulate:a", 1.0, 0.15)
	_appearance.tween_property(content, "scale", Vector2.ONE, 0.18 if quick else 0.3).set_trans(
		Tween.TRANS_QUAD if quick else Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)

## Row of actions centred under the panel, with a wide Play pill. Each item: [kind ("green" primary or "orange"/"cream"), icon,
## callback]; by default pressing one closes the popup first. Returns the buttons.
func actions(items: Array, closes := true) -> Array:
	var sizes := []
	var total := 0.0
	var height := 0.0
	for it in items:
		var s := UI.PRIMARY if it[0] == "green" else UI.SECONDARY
		var dimensions := UI.PLAY_SIZE if it[1] == "play" else Vector2.ONE * s
		sizes.append(dimensions.x)
		total += dimensions.x
		height = maxf(height, dimensions.y)
	total += ACTION_GAP * (items.size() - 1)
	var x := 540.0 - total * 0.5
	var cy: float = get_meta("actions_y") + height * 0.5
	var out := []
	for i in items.size():
		var cb: Callable = items[i][2]
		var b := UI.button(content, str(items[i][1]).capitalize(), Vector2(x + sizes[i] * 0.5, cy), items[i][0], items[i][1],
			func(): invoke(cb, closes))
		out.append(b)
		x += sizes[i] + ACTION_GAP
	return out

## A closing modal cannot dispatch another action during its exit animation.
func invoke(callback: Callable, closes := true) -> void:
	if _closing:
		return
	if closes:
		close()
	callback.call()

func close() -> void:
	if _closing:
		return
	_closing = true
	if _appearance != null:
		_appearance.kill()
	Sound.play("ui_back")
	var t := 0.12
	var tw := create_tween().set_parallel()
	tw.tween_property(self, "modulate:a", 0.0, t)
	tw.tween_property(content, "scale", Vector2.ONE * 0.9, t).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_IN)
	await tw.finished
	closed.emit()
	queue_free()
