class_name Popup
extends Control
## Shared modal: dimmed backdrop, plush panel, painted title ribbon, optional close disc, action discs.
## Content uses absolute design coordinates (same space as res://layout/*.json); add it to `content`.

signal closed

const DIM_ALPHA := 0.55

var content: Control
var panel_rect: Rect2
var _dim: ColorRect
var _closing := false


## title: a painted label sprite name (e.g. "label_title_settings"); "" for none.
static func open(host: Control, panel: Rect2, title: String = "", title_rect := Rect2(), close_rect := Rect2(), instant := false) -> Popup:
	var p := Popup.new()
	p.name = "Popup"
	p.size = Vector2(1080, 1920)
	p.panel_rect = panel
	host.add_child(p)
	p._build(title, title_rect, close_rect)
	if not instant:
		p._appear()
	return p

func _build(title: String, title_rect: Rect2, close_rect: Rect2) -> void:
	_dim = ColorRect.new()
	_dim.color = Color(0.05, 0.08, 0.04, DIM_ALPHA)
	_dim.size = size
	_dim.mouse_filter = Control.MOUSE_FILTER_STOP
	add_child(_dim)
	content = Control.new()
	content.name = "Content"
	content.size = size
	content.mouse_filter = Control.MOUSE_FILTER_IGNORE
	content.pivot_offset = panel_rect.get_center()
	add_child(content)
	UI.panel(content, "panel_wood", panel_rect)
	if not title.is_empty():
		UI.art(content, title, title_rect)
	if close_rect.size != Vector2.ZERO:
		UI.disc(content, "Close", close_rect, "icon_close", close, "btn_wood", 0.48)

func _appear() -> void:
	_dim.modulate.a = 0.0
	content.scale = Vector2.ONE * 0.7
	content.modulate.a = 0.0
	var tw := create_tween().set_parallel()
	tw.tween_property(_dim, "modulate:a", 1.0, 0.2)
	tw.tween_property(content, "modulate:a", 1.0, 0.15)
	tw.tween_property(content, "scale", Vector2.ONE, 0.32).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)

## Disc button below or on the panel; base "btn_green" for the primary action (124 px), "btn_wood" otherwise.
func action(rect: Rect2, icon: String, on_press: Callable, base := "btn_wood") -> Button:
	return UI.disc(content, icon, rect, icon, on_press, base, 0.5)

func close() -> void:
	if _closing:
		return
	_closing = true
	var tw := create_tween().set_parallel()
	tw.tween_property(_dim, "modulate:a", 0.0, 0.18)
	tw.tween_property(content, "scale", Vector2.ONE * 0.85, 0.18).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_IN)
	tw.tween_property(content, "modulate:a", 0.0, 0.18)
	await tw.finished
	closed.emit()
	queue_free()
