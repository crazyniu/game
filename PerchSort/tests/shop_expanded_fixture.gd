extends "res://game/shop_view.gd"
func _items(kind: int) -> Array:
	var items := super._items(kind)
	if kind == Shop.BOOSTER:
		for index in 2:
			var extra: Dictionary = items[index].duplicate()
			extra["id"] += "_layout_fixture"
			items.append(extra)
	return items
