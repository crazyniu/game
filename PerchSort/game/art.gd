class_name Art
extends RefCounted
## Texture lookup by bare name across the art roots, cached. Missing names return null.

const ROOTS: PackedStringArray = ["res://art/", "res://art/icons/", "res://art/words/", "res://art/letters/", "res://art/labels/",
	"res://art/glyphs/", "res://birds/", "res://world/", "res://world/scene/"]

static var _cache: Dictionary = {}
static var _metrics: Dictionary = {}
static var _glyphs: Dictionary = {}


static func tex(name: String) -> Texture2D:
	if _cache.has(name):
		return _cache[name]
	var found: Texture2D = null
	for root in ROOTS:
		var path := root + name + ".png"
		if ResourceLoader.exists(path):
			found = load(path)
			break
	_cache[name] = found
	return found

static func has(name: String) -> bool:
	return tex(name) != null

## Frames name_01, name_02 ... until one is missing.
static func frames(name: String) -> Array[Texture2D]:
	var out: Array[Texture2D] = []
	var i := 1
	while true:
		var t := tex("%s_%02d" % [name, i])
		if t == null:
			break
		out.append(t)
		i += 1
	return out

## Nine-slice metrics from art/layout_metrics.json: {"size", "opaque", "inner"}.
static func metrics(name: String) -> Dictionary:
	if _metrics.is_empty():
		_metrics = JSON.parse_string(FileAccess.get_file_as_string("res://art/layout_metrics.json"))
	return _metrics.get(name, {})

static func glyph_metrics(color: String) -> Dictionary:
	if _glyphs.is_empty():
		_glyphs = JSON.parse_string(FileAccess.get_file_as_string("res://art/glyphs/glyph_metrics.json"))
	return _glyphs.get(color, {})
