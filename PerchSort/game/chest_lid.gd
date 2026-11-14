class_name ChestLid
extends Control
## Two-sided lid projection around the rear hinge. The approved box/lock never morph.
const BODY := Rect2(0, 515, 1254, 739)
const INNER := Rect2(179, 86, 959, 429)
const OUTER := Rect2(28, 84, 1714, 691)
const CLOSED_ANGLE := -70.0
const OPEN_ANGLE := 70.0
# Polygon follows the fixed front lock on the clean source, not a screenshot.
const LOCK := [Vector2(561, 550), Vector2(620, 640), Vector2(720, 650),
	Vector2(755, 700), Vector2(681, 750), Vector2(690, 860), Vector2(640, 903),
	Vector2(560, 850), Vector2(475, 906), Vector2(423, 870), Vector2(435, 760),
	Vector2(370, 714), Vector2(380, 665), Vector2(491, 643)]
var angle := CLOSED_ANGLE:
	set(value):
		angle = value
		queue_redraw()
var inside: Texture2D
var outside: Texture2D

func setup(footline: float, width: float) -> void:
	inside = Art.tex("chest_star_open")
	outside = Art.tex("chest_lid_outer")
	mouse_filter = Control.MOUSE_FILTER_IGNORE
	var k := width / 1029.0
	size = Vector2(1254, 1254)
	scale = Vector2.ONE * k
	pivot_offset = Vector2(614.5, 1090.0)
	position = Vector2(540.0, footline) - pivot_offset

func _draw() -> void:
	if inside == null or outside == null:
		return
	draw_texture_rect_region(inside, BODY, BODY)
	# Two textured faces meet edge-on. No whole-image blend or discrete pose timer.
	var openness := inverse_lerp(CLOSED_ANGLE, OPEN_ANGLE, angle)
	var face := sin(deg_to_rad(angle)) / sin(deg_to_rad(OPEN_ANGLE))
	var extent := absf(face)
	var bottom := 515.0 + 125.0 * (1.0 - openness)
	var height := (420.0 if face < 0.0 else INNER.size.y) * extent
	var width := 959.0 + 32.0 * (1.0 - openness)
	var left := 658.5 - width * 0.5
	var inset := 24.0 * (1.0 - extent)
	var polygon := PackedVector2Array([Vector2(left+inset,bottom-height),Vector2(left+width-inset,bottom-height),Vector2(left+width,bottom),Vector2(left,bottom)])
	var ink := OUTER if face < 0.0 else INNER
	var texture := outside if face < 0.0 else inside
	var dims := texture.get_size()
	var uv := PackedVector2Array([ink.position/dims,Vector2(ink.end.x,ink.position.y)/dims,ink.end/dims,Vector2(ink.position.x,ink.end.y)/dims])
	if height > 0.1:
		draw_polygon(polygon, PackedColorArray([Color.WHITE]), uv, texture)
	# The front star lock belongs to the fixed box, not the moving lid.
	var lock_uv := PackedVector2Array()
	for point in LOCK:
		lock_uv.append(point / inside.get_size())
	draw_polygon(PackedVector2Array(LOCK), PackedColorArray([Color.WHITE]), lock_uv, inside)
