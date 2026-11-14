extends Node
## Regression checks for competing bird animation timelines; uses the production BirdView.
var failures := 0
var completions := 0

func check(value: bool, label: String) -> void:
	print("PASS " if value else "FAIL ", label)
	if not value:
		failures += 1

func _ready() -> void:
	_run.call_deferred()

func _run() -> void:
	var bird := BirdView.new()
	bird.setup(0, false, 1.0)
	add_child(bird)
	bird.position = Vector2(160, 260)
	bird.finished.connect(func(): completions += 1)
	bird.fly_to(Vector2(400, 260), 0.0, 0.8)
	await get_tree().create_timer(0.12).timeout
	bird.cheer()
	await get_tree().create_timer(0.8).timeout
	check(bird.acting and completions == 0, "decorative cheer cannot finish an active flight")
	await get_tree().create_timer(0.3).timeout
	check(not bird.acting and completions == 1, "flight completes exactly once after landing")
	bird.set_sleep(true)
	bird.nudge_asleep()
	await get_tree().create_timer(0.12).timeout
	bird.wake()
	await get_tree().create_timer(0.1).timeout
	check(bird._nudge < 0.0, "wake cancels sleeper reaction immediately")
	await get_tree().create_timer(0.8).timeout   # the wake (bell, spring and landing) lasts about 0.8 s
	check(not bird.acting and bird._nudge < 0.0, "wake stays awake without the old reaction resuming")
	completions = 0
	bird.cheer()
	await get_tree().create_timer(0.9).timeout
	check(not bird.acting and completions == 1, "idle decorative cheer still plays and completes")
	bird.set_sleep(true)
	bird.nudge_asleep()
	await get_tree().create_timer(0.12).timeout
	bird.fly_to(Vector2(160, 260), 0.0, 0.3)
	await get_tree().create_timer(0.1).timeout
	check(bird._nudge < 0.0, "flight cancels sleeper reaction")
	await get_tree().create_timer(0.6).timeout
	check(not bird.acting and bird.position.is_equal_approx(Vector2(160, 260)), "interrupted reaction cannot disturb flight landing")
	bird.set_sleep(true)
	bird.nudge_asleep()
	await get_tree().create_timer(0.8).timeout
	check(bird._nudge < 0.0 and bird.asleep, "uninterrupted sleeper reaction still completes")
	bird.queue_free()
	await get_tree().process_frame
	print("BIRD ANIMATION failures=", failures)
	get_tree().quit(1 if failures else 0)
