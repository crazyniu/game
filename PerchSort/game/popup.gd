class_name Modal
extends Control
## Shared modal (docs/design/ui-system.md): dim backdrop, one cream panel whose height follows its content, a painted
## localized title at the top inside the panel, an optional close button on the panel's top-right corner, and actions
## in a row centred under the panel. Content uses absolute design coordinates; add it to `content`.

signal closed

const WIDTH := 864.0
const TITLE_CAP := 56.0
const TITLE_TOP := 56.0          # panel top to the title's cap top
const BODY_GAP := 40.0           # title cap bottom to the first content group
const BOTTOM := 56.0             # last content group to the panel's visible bottom (above its edge)
const GAP := 32.0                # between content groups
const INSET := 72.0              # panel edge to content: the wooden frame (48 px) and a little air
const CLOSE := 128.0
const ACTION_DROP := 48.0        # panel bottom to the action row's top
const ACTION_GAP := 56.0
const DIM := Color(0.118, 0.165, 0.102, 0.78)

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

## The title sits on the painted ribbon straddling the panel's top edge, in white; level numbers stay numeral sprites.
const RIBBON_H := 172.0
const RIBBON_TEXT_PAD := 150.0   # ribbon tails and rounded ends on each side of the words

func _title() -> void:
	var cap := 46.0
	var max_text := WIDTH - (224.0 if _has_close else 128.0) - RIBBON_TEXT_PAD * 2.0 + 120.0
	var word_w := UI.label_width(_title_id, cap)
	var num_w := UI.number_width(_title_number, cap) if not _title_number.is_empty() else 0.0
	var natural := word_w + ((20.0 + num_w) if num_w > 0.0 else 0.0)
	var k := minf(1.0, max_text / natural)
	cap *= k
	word_w *= k
	num_w *= k
	var text_w := natural * k
	var ribbon_w := clampf(text_w + RIBBON_TEXT_PAD * 2.0, 520.0, 820.0)
	var top := panel_rect.position.y - RIBBON_H * 0.42
	UI.kit_piece(_title_layer, "ribbon", Rect2(540.0 - ribbon_w * 0.5, top, ribbon_w, RIBBON_H)).name = "Ribbon"
	var cy := top + RIBBON_H * 0.4
	var x := 540.0 - text_w * 0.5
	var title := UI.label(_title_layer, _title_id, Rect2(x, cy - 56.0 * k, word_w, 112.0 * k), cap, Color.WHITE, UI.ribbon_bend(text_w, ribbon_w, RIBBON_H))
	title.name = "Title"
	UI.outline_to(title, "green")
	if num_w > 0.0:
		UI.number(_title_layer, _title_number, Rect2(x + word_w + 20.0 * k, cy - cap * 0.5, num_w, cap), Color.WHITE)
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

## Actions under the panel, every one a long pill with a word (no round buttons in popups). Each item:
## [kind ("green" primary, "orange" secondary), icon, callback] or [kind, icon, callback, label id]; the word comes
## from the label id, else from the icon (ACTION_WORDS). Green pills are PLAY_SIZE and sit on the first row; the
## others are SECONDARY_PILL (1/1.25 of it) on a second row. By default pressing one closes the popup first.
## Returns the buttons in item order.
const SECONDARY_PILL := Vector2(320.0, 116.0)
const ROW_STACK := 28.0
const ACTION_WORDS := {"play": "action.play", "check": "action.ok", "right": "action.next", "retry": "action.replay",
	"home": "action.home", "buy": "action.buy"}

## Height of the action block for these items: one row, or the primary row plus a secondary row.
static func actions_height(items: Array) -> float:
	var primary := false
	var secondary := false
	for it in items:
		if it[0] == "green":
			primary = true
		else:
			secondary = true
	if primary and secondary:
		return UI.PLAY_SIZE.y + ROW_STACK + SECONDARY_PILL.y
	return UI.PLAY_SIZE.y if primary else SECONDARY_PILL.y

func actions(items: Array, closes := true) -> Array:
	var rows := [[], []]
	for i in items.size():
		rows[0 if items[i][0] == "green" else 1].append(i)
	var out := []
	out.resize(items.size())
	var y: float = get_meta("actions_y")
	for r in 2:
		var row: Array = rows[r]
		if row.is_empty():
			continue
		var dimensions: Vector2 = UI.PLAY_SIZE if r == 0 else SECONDARY_PILL
		var total: float = dimensions.x * row.size() + ACTION_GAP * (row.size() - 1)
		var x := 540.0 - total * 0.5
		for i in row:
			var it: Array = items[i]
			var cb: Callable = it[2]
			var word: String = it[3] if it.size() > 3 else ACTION_WORDS.get(it[1], "action.ok")
			var n: String = it[3] if it.size() > 3 else str(it[1]).capitalize()
			out[i] = UI.pill(content, n, Rect2(Vector2(x, y), dimensions), it[0], word, func(): invoke(cb, closes), "",
				52.0 if r == 0 else 44.0)
			x += dimensions.x + ACTION_GAP
		y += dimensions.y + ROW_STACK
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
