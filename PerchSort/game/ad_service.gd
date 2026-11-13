class_name AdService
extends RefCounted
## Rewarded video ads. Every ad in the game goes through show_rewarded(); connecting a real ad network means replacing
## only its body with the network's rewarded-video call (reward on completion, nothing on cancel or failure).
## Until then it shows a stand-in ad: a full-screen picture, a countdown, then a close button that pays the reward.

const SECONDS := 5
const BADGE := Vector2(132, 132)

static func show_rewarded(host: Control, on_reward: Callable, instant := false) -> Control:
	var layer := Control.new()
	layer.name = "Ad"
	layer.mouse_filter = Control.MOUSE_FILTER_STOP   # nothing under the ad can be pressed
	UI.place(layer, Rect2(0, 0, 1080, 1920))
	host.add_child(layer)
	var dark := ColorRect.new()
	dark.color = Color.BLACK
	dark.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(dark, Rect2(0, 0, 1080, 1920))
	layer.add_child(dark)
	var picture := UI.image(layer, "ad_mock", Rect2(0, 0, 1080, 1920), false)
	picture.name = "Picture"
	picture.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_COVERED
	var corner := Vector2(1080.0 - 40.0 - BADGE.x, 56.0)
	var badge := Control.new()
	badge.name = "Countdown"
	badge.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(badge, Rect2(corner, BADGE))
	layer.add_child(badge)
	UI.image(badge, "island_badge_green", Rect2(Vector2.ZERO, BADGE), false)
	ButtonKit.number(badge, str(SECONDS), Rect2(BADGE * 0.22, BADGE * 0.56))
	var music: String = Sound._music_name   # the game's music pauses while the ad plays, as with a real ad
	var finish := func():
		if not is_instance_valid(layer) or layer.is_queued_for_deletion():   # a double tap pays once
			return
		layer.queue_free()
		if not instant:
			Sound.music(music)
		on_reward.call()
	var allow_close := func():
		if not is_instance_valid(layer):
			return
		badge.queue_free()
		MenuNavigation.close(layer, "Close", corner, finish)
	if instant:
		allow_close.call()
		return layer
	Sound.music("")
	var tween := layer.create_tween()
	for left in range(SECONDS - 1, 0, -1):
		tween.tween_interval(1.0)
		tween.tween_callback(func(): ButtonKit.set_number(badge.get_node("Digits"), str(left)))
	tween.tween_interval(1.0)
	tween.tween_callback(allow_close)
	return layer

## Fake-data state for captures: "counting" (first second) or "done" (close shown).
static func preview(main: Control, state: String) -> void:
	show_rewarded(main, func(): pass, state == "done")
