class_name Modal
extends Control
## Shared modal: dimmed backdrop, plush panel, painted title ribbon, optional close disc, action discs.
## Content uses absolute design coordinates (same space as res://layout/*.json); add it to `content`.

signal closed

const DIM_ALPHA := 0.72
## Transparent margins of panel_wood around its visible frame (left, top, right, bottom).
const PAD := Vector4(4, 36, 4, 21)
const SHADOW_ALPHA := 0.16

var content: Control
var panel_rect: Rect2   # the visible frame
var quick := false      # plain fade+scale (settings) instead of the springy pop
var _dim: ColorRect
var _closing := false


## panel: the visible frame rect. title: a painted label sprite name (e.g. "label_title_settings"); "" for none.
static func open(host: Control, panel: Rect2, title: String = "", title_rect := Rect2(), close_rect := Rect2(), instant := false,
		quick_motion := false) -> Modal:
	var p := Modal.new()
	p.name = "Popup"
	p.size = Vector2(1080, 1920)
	p.panel_rect = panel
	p.quick = quick_motion
	host.add_child(p)
	host.move_child(p, host.get_child_count() - 1)
	p._build(title, title_rect, close_rect)
	if not instant:
		p._appear()
	return p

func _build(title: String, title_rect: Rect2, close_rect: Rect2) -> void:
	_dim = ColorRect.new()
	_dim.color = Color(0.03, 0.05, 0.02, DIM_ALPHA)
	_dim.size = size
	_dim.mouse_filter = Control.MOUSE_FILTER_STOP
	add_child(_dim)
	content = Control.new()
	content.name = "Content"
	content.size = size
	content.mouse_filter = Control.MOUSE_FILTER_IGNORE
	content.pivot_offset = panel_rect.get_center()
	add_child(content)
	var full := Rect2(panel_rect.position - Vector2(PAD.x, PAD.y), panel_rect.size + Vector2(PAD.x + PAD.z, PAD.y + PAD.w))
	# Soft shadow: two dark copies of the frame, lower and slightly larger, so the modal separates from the scene.
	for grow in [Vector2(10, 14), Vector2(26, 30)]:
		var shadow := UI.panel(content, "panel_wood", Rect2(full.position - grow * 0.5 + Vector2(0, 16), full.size + grow))
		shadow.modulate = Color(0.06, 0.03, 0.01, SHADOW_ALPHA)
		shadow.name = "Shadow"
	UI.panel(content, "panel_wood", full)
	if not title.is_empty():
		UI.art(content, title, title_rect)
	if close_rect.size != Vector2.ZERO:
		UI.disc(content, "Close", close_rect, "icon_close", close, "btn_wood", 0.48)

func _appear() -> void:
	Sound.play("ui_tap")
	_dim.modulate.a = 0.0
	content.scale = Vector2.ONE * (0.9 if quick else 0.7)
	content.modulate.a = 0.0
	var tw := create_tween().set_parallel()
	tw.tween_property(_dim, "modulate:a", 1.0, 0.18 if quick else 0.2)
	tw.tween_property(content, "modulate:a", 1.0, 0.18 if quick else 0.15)
	if quick:
		tw.tween_property(content, "scale", Vector2.ONE, 0.18).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
	else:
		tw.tween_property(content, "scale", Vector2.ONE, 0.4).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)

## Disc button below or on the panel; base "btn_green" for the primary action (124 px), "btn_wood" otherwise.
## An action disc; by default pressing it closes the popup, then runs `on_press`.
func action(rect: Rect2, icon: String, on_press: Callable, base := "btn_wood", closes := true) -> Button:
	return UI.disc(content, icon, rect, icon, func():
		if closes:
			close()
		on_press.call(), base, 0.5)

func close() -> void:
	if _closing:
		return
	_closing = true
	Sound.play("ui_back")
	var t := 0.12
	var tw := create_tween().set_parallel()
	tw.tween_property(self, "modulate:a", 0.0, t)
	tw.tween_property(content, "scale", Vector2.ONE * 0.88, t).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_IN)
	await tw.finished
	closed.emit()
	queue_free()
