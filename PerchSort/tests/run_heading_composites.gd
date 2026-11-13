extends SceneTree
## The localized heading and ordinal are centered as one visible-ink group, even after locale changes.
var failures := 0
var checks := 0

func _initialize() -> void:
	_run.call_deferred()

func _check(heading: Control, language: String, ordinal: String) -> void:
	var word: TextureRect = heading.get_child(0)
	var ink := word.texture.get_image().get_used_rect()
	var scale := word.size / word.texture.get_size()
	var word_ink := Rect2(word.position + Vector2(ink.position) * scale, Vector2(ink.size) * scale)
	var number: Control = heading.get_child(1)
	var union := word_ink.merge(number.get_rect())
	var offset := union.get_center() - heading.size * 0.5
	checks += 1
	failures += 0 if offset.length() < 0.01 else 1
	print(language, " level ", ordinal, " union=", union, " center_offset=", offset)

func _run() -> void:
	var family: GDScript = load("res://game/family_dialog.gd")
	for language in ["en", "zh"]:
		UiText.set_language(language)
		for ordinal in ["1", "12", "100"]:
			var heading: Control = family.Heading.new()
			heading.size = Vector2(680, 112)
			heading.label_id = "popup.level.title"
			heading.ordinal = ordinal
			root.add_child(heading)
			heading.refresh()
			_check(heading, language, ordinal)
			var other := "zh" if language == "en" else "en"
			UiText.set_language(other)
			await process_frame
			await process_frame
			_check(heading, other + " live", ordinal)
			UiText.set_language(language)
			await process_frame
			await process_frame
			_check(heading, language + " restored", ordinal)
			heading.queue_free()
			await process_frame
	print("heading composite checks: %d checks, %d failures" % [checks, failures])
	quit(1 if failures else 0)
