class_name Layout
extends RefCounted
## Measured mockup rects (res://layout/<screen>.json, 1080x1920, top-left pixels).

static var _screens: Dictionary = {}


static func r(screen: String, id: String) -> Rect2:
	var elements := _load(screen)
	if not elements.has(id):
		push_error("Unmeasured layout element %s/%s" % [screen, id])
		return Rect2()
	var a: Array = elements[id]
	return Rect2(a[0], a[1], a[2], a[3])

static func has(screen: String, id: String) -> bool:
	return _load(screen).has(id)

## Rect relative to another element of the same screen.
static func local(screen: String, id: String, parent_id: String) -> Rect2:
	var rr := r(screen, id)
	rr.position -= r(screen, parent_id).position
	return rr

static func _load(screen: String) -> Dictionary:
	if not _screens.has(screen):
		var out := {}
		var path := "res://layout/%s.json" % screen
		if FileAccess.file_exists(path):
			var data = JSON.parse_string(FileAccess.get_file_as_string(path))
			for e in data.get("elements", []):
				out[e["id"]] = e["rect"]
		_screens[screen] = out
	return _screens[screen]
