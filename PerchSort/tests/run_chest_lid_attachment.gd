extends SceneTree
var checks := 0
var failures := 0
func check(ok: bool, label: String) -> void:
 checks += 1
 if not ok:
  failures += 1
  print("FAIL ",label)
func _initialize() -> void:
 var Lid = load("res://game/chest_lid.gd")
 var lid = Lid.new()
 var previous := PackedVector2Array()
 for degree in range(-70,71):
  lid.angle = float(degree)
  var points: PackedVector2Array = lid.projected_lid()
  check(points.size()==4,"four projected corners")
  check(points[2].y-points[1].y >= Lid.RIM_THICKNESS-0.001,"rim never disappears edge on")
  if degree >= 0:
   check(is_equal_approx(points[2].y,Lid.HINGE_Y),"inner lid fixed to rear hinge")
  if not previous.is_empty():
   check(points[2].y<=previous[2].y,"front lip approaches rear rim without reversal")
   check(points[2].distance_to(previous[2])<4,"no positional jump between faces")
  previous=points
 lid.angle=0
 check(is_equal_approx(lid.projected_lid()[2].y,Lid.HINGE_Y),"edge-on rim attached to box")
 lid.free()
 print("Chest lid attachment: %d checks, %d failures" %[checks,failures])
 quit(1 if failures else 0)
