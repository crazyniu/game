class_name Modal
extends Control
## Shared modal (docs/design/island-style.md): dim backdrop, one wide wooden-framed panel whose height follows its
## content, a wooden name plank with the gold title straddling the panel's top edge, an optional close tile on the
## panel's top-right corner, and the actions inside the panel at its bottom. Content uses absolute design coordinates;
## add it to `content`.

signal closed

const WIDTH := 960.0
const HEAD := 150.0              # panel top to the first content group: the lower half of the plank and some air
const BOTTOM := 64.0             # last content group to the panel's visible bottom (no actions)
const GAP := 36.0                # between content groups
const INSET := 72.0              # panel edge to content: the wooden frame and a little air
const CLOSE := 120.0
const ACTION_DROP := 48.0        # last content group to the action block
const ACTION_BOTTOM := 104.0     # action block to the panel's bottom edge, clear of the frame's flowers
const ACTION_GAP := 32.0
const SIGN_W := 760.0            # the wooden title plank
const SIGN_ABOVE := 250.0        # existing title reservation, kept for callers' vertical layout
const DIM := Color(0.08, 0.09, 0.24, 0.68)   # dusk over the island, as in the designs
## The shade outside every shop-style popup (dialogs, purchases, win): dark enough that the card stands apart from the
## bright page behind it.
const POPUP_SHADE := Color(0.02, 0.07, 0.03, 0.55)

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
var _decorate_title := true

## Opens a modal whose panel holds `heights` content groups below an optional title. Returns the modal; read
## `ys` (top y of each group) and `actions_y` (top of the action row) from its metadata.
static func open(host: Control, title_id: String, heights: Array, close_button := false, action_h := 0.0, above := 0.0,
		instant := false, quick_motion := false, title_number := "", decorate_title := true) -> Modal:
	var inner := 0.0
	for h in heights:
		inner += h
	inner += GAP * maxf(0.0, heights.size() - 1)
	var head := HEAD if not title_id.is_empty() else BOTTOM
	var tail := (ACTION_DROP + action_h + ACTION_BOTTOM) if action_h > 0.0 else BOTTOM
	var panel_h := roundf(head + inner + tail)
	var lift := maxf(above, SIGN_ABOVE if not title_id.is_empty() else 0.0)
	var top := clampf(roundf((1920.0 - (lift + panel_h)) * 0.5 + lift), lift + 20.0, maxf(lift + 20.0, 1900.0 - panel_h))
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
	p._decorate_title = decorate_title
	p._build()
	var ys := []
	var y := top + head
	for h in heights:
		ys.append(y)
		y += h + GAP
	p.set_meta("ys", ys)
	p.set_meta("actions_y", panel.end.y - ACTION_BOTTOM - action_h)
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
	frame(content, panel_rect)
	_title_layer = Control.new()
	_title_layer.name = "TitleGroup"
	_title_layer.size = size
	_title_layer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	content.add_child(_title_layer)
	if not _title_id.is_empty() and _decorate_title:
		_title()
	if _has_close:
		MenuNavigation.close(content, "Close", Vector2(panel_rect.end.x - 74.0, panel_rect.position.y - 38.0), close)

## Keep all frame art under Panel so page-specific modals can replace the complete surface.
static func frame(parent: Control, bounds: Rect2) -> Control:
	var panel := Control.new()
	panel.name = "Panel"
	panel.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(panel, bounds)
	parent.add_child(panel)
	UI.kit_piece(panel, "panel", Rect2(Vector2.ZERO, bounds.size), 1.0).name = "Skin"
	return panel

## The native plank keeps its wood grain and flowers intact; only its central clear area holds lettering.
static func title_plank(parent: Control, center: Vector2, width: float) -> Rect2:
	var tex := Art.tex("island_name_plank")
	var height := width * tex.get_height() / tex.get_width()
	Island.piece(parent, "island_name_plank", center - Vector2(width, height) * 0.5, width)
	return Rect2(center - Vector2(width * 0.28, height * 0.33), Vector2(width * 0.56, height * 0.58))

## Gold painted heading and optional numeral sprites on the wooden plank.
const TITLE_CAP := 112.0
const TITLE_CENTER_Y := 24.0

func _title() -> void:
	var cap := TITLE_CAP
	var plank := title_plank(_title_layer, Vector2(540.0, panel_rect.position.y + TITLE_CENTER_Y), SIGN_W)
	var max_text := plank.size.x
	var word_w := UI.heading_width(_title_id, cap)
	var num_w := UI.number_width(_title_number, cap * 1.0) if not _title_number.is_empty() else 0.0
	var natural := word_w + ((20.0 + num_w) if num_w > 0.0 else 0.0)
	var k := minf(1.0, max_text / natural)
	cap *= k
	word_w *= k
	num_w *= k
	var text_w := natural * k
	var cy := plank.get_center().y
	var x := 540.0 - text_w * 0.5
	var title := UI.heading(_title_layer, _title_id, Rect2(x, cy - cap * 1.1, word_w, cap * 2.2), cap)
	title.name = "Title"
	if num_w > 0.0:
		var num := UI.number(_title_layer, _title_number, Rect2(x + word_w + 20.0 * k, cy - cap * 0.5, num_w, cap * 1.0), UI.GOLD)
		num.name = "TitleNumber"

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

## Actions at the bottom inside the panel, every one a long pill with a word (no round buttons in popups). Each item:
## [kind ("green" primary, "orange" secondary), icon, callback] or [kind, icon, callback, label id]; the word comes
## from the label id, else from the icon (ACTION_WORDS). Green pills are PLAY_SIZE and sit on the first row; the
## others are SECONDARY_PILL (1/1.25 of it) on a second row. By default pressing one closes the popup first.
## Returns the buttons in item order.
const SECONDARY_PILL := Vector2(400.0, 152.0)   # UI.PLAY_SIZE 188 / 152 = 1.24 (at most 1.25)
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
				64.0 if r == 0 else 50.0)
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
