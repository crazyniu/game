extends SceneTree
const Scene = preload("res://scenes/first_level.tscn")
const Rules = preload("res://core/first_level_rules.gd")
class FailingStore extends "res://core/save_store.gd":
	var fail_writes := true
	func _write_bytes(path: String, bytes: PackedByteArray) -> Error:
		if fail_writes:
			return ERR_FILE_CANT_WRITE
		return super._write_bytes(path, bytes)

var checks := 0
var failures := 0

func check(ok: bool, message: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		push_error(message)

func _initialize() -> void:
	call_deferred("run")

func settled(scene) -> void:
	var deadline := Time.get_ticks_msec() + 5000
	while scene.busy and Time.get_ticks_msec() < deadline:
		await process_frame
	check(not scene.busy, "Presentation terminates without an await deadlock")

func run() -> void:
	var scene = Scene.instantiate()
	scene.test_save_directory = "/tmp/perch-scene-%d" % Time.get_ticks_usec()
	root.add_child(scene)
	await process_frame
	check(scene.actors.size() == 8, "Eight stable bird actors")
	check(scene.branches.size() == 3, "Three supported branches")
	check(scene.modal.size == scene.get_viewport_rect().size, "Modal blocks the entire viewport")
	check(scene.session.snapshot() == Rules.initial_state(), "Exact authored initial board")
	check(not scene.undo_button.visible, "Undo hidden without history")
	var sample = scene.actors["L01-S2-04"]
	scene._home_pose(0.5, sample, scene.slot(1, 3), Vector2(540, 352), Vector2(540, 425))
	check(sample.scale == Vector2.ONE and sample.z_index == 40, "Approach stays full size in front of rim")
	scene._home_pose(0.9, sample, scene.slot(1, 3), Vector2(540, 352), Vector2(540, 425))
	check(sample.scale.x < 1.0 and sample.z_index == 20, "Entry shrinks only behind rim after mouth")
	scene._sync()
	scene.pick(1)
	check(scene.selected == 1, "Source selected")
	check(scene.actors["L01-S2-04"].flying, "Selected bird uses real wing frames")
	check(not scene.actors["L01-S2-03"].flying, "Internal unlike bird stays perched")
	scene.cancel_selection()
	await create_timer(0.35).timeout
	await process_frame
	await process_frame
	check(not scene.actors["L01-S2-04"].flying, "Cancelled selection lands")
	check(scene.actors["L01-S2-04"].position.is_equal_approx(scene.slot(1,3)), "Cancelled feet return to slot")
	scene.pick(0)
	scene.pick(2)
	check(scene.session.snapshot().move_count == 0, "Illegal move does not mutate")
	scene.pick(1)
	scene.pick(0)
	check(scene.busy, "Transfer animation locks commands")
	check(scene.session.snapshot().move_count == 1, "Durable candidate commits before visual event")
	check(scene.store.confirmed_record().state == scene.session.snapshot(), "Save equals confirmed logic")
	scene.undo()
	check(scene.session.snapshot().move_count == 1, "Undo blocked during animation")
	scene.request_pause()
	check(scene.pending_pause, "Pause is queued during committed group")
	await settled(scene)
	check(scene.mode == "settings", "Queued pause applied after group")
	check(not scene.branches[0].visible, "Retired branch vanishes completely")
	check(scene.actors.size() == 4, "Homed flock removed after real flight")
	scene.close_settings()
	scene.undo()
	await settled(scene)
	check(scene.actors.size() == 8 and scene.branches[0].visible, "Undo restores birds and complete branch")
	check(scene.session.snapshot().move_count == 0, "Undo restores move count")
	scene.pick(1)
	scene.pick(0)
	await settled(scene)
	scene.pick(1)
	scene.pick(2)
	scene.request_pause()
	await settled(scene)
	check(scene.mode == "won", "Win takes priority over pending pause")
	check(scene.session.snapshot().move_count == 2, "Two-transfer witness wins")
	check(scene.store.confirmed_record().completed and scene.store.confirmed_record().best == 2, "Completion and best durable")
	scene.restart()
	await settled(scene)
	check(scene.actors.size() == 8 and scene.session.snapshot().move_count == 0, "Replay restores exact board")
	check(scene.record.completed and scene.record.best == 2, "Replay preserves completion")
	scene.request_pause()
	var muted: bool = scene.record.audio_muted
	scene.toggle_sound()
	check(scene.record.audio_muted != muted, "Mute persisted")
	scene.close_settings()
	scene.pick(1)
	scene._notification(Node.NOTIFICATION_APPLICATION_FOCUS_OUT)
	check(scene.mode == "settings" and scene.focus_lost, "Focus loss cancels selection and opens settings")
	scene._notification(Node.NOTIFICATION_APPLICATION_FOCUS_IN)
	scene.close_settings()
	await create_timer(0.35).timeout
	await process_frame
	await process_frame
	check(not scene.actors["L01-S2-04"].flying, "Focus return settles selected bird")
	var failing := FailingStore.new(scene.test_save_directory)
	failing.load_record()
	scene.store = failing
	var before: Dictionary = scene.session.snapshot()
	scene.pick(1)
	scene.pick(0)
	check(scene.mode == "save_error" and scene.busy, "Save failure blocks gameplay")
	check(scene.session.snapshot() == before, "Save failure does not expose uncommitted board")
	check(scene.store.has_pending() and not scene.pending.is_empty(), "Failed candidate retained")
	scene._home()
	scene._continue()
	check(scene.mode == "save_error" and scene.store.has_pending(), "Home cannot bypass pending save")
	scene.close_settings()
	check(scene.mode == "save_error", "Close cannot bypass SaveError")
	failing.fail_writes = false
	scene.retry_save()
	await settled(scene)
	check(scene.session.snapshot().move_count == 1, "Retry commits same move exactly once")
	check(not scene.store.has_pending() and scene.pending.is_empty(), "Retry clears retained candidate")
	scene.request_pause()
	failing.fail_writes = true
	var prior_mute: bool = scene.record.audio_muted
	scene.toggle_sound()
	check(scene.mode == "save_error" and scene.audio_muted != prior_mute, "Failed mute applies to audio session immediately")
	check(scene.record.audio_muted == prior_mute, "Failed mute does not pretend durable")
	scene._home()
	scene._continue()
	failing.fail_writes = false
	scene.retry_save()
	check(scene.record.audio_muted != prior_mute and scene.mode == "settings", "Mute retry persists same preference")
	scene.close_settings()
	scene.pick(1)
	scene.pick(2)
	scene._notification(Node.NOTIFICATION_APPLICATION_FOCUS_OUT)
	var frozen: Vector2 = scene.actors["L01-S2-01"].position
	await create_timer(0.1).timeout
	check(scene.actors["L01-S2-01"].position == frozen, "Focus loss explicitly pauses native tween")
	scene._notification(Node.NOTIFICATION_APPLICATION_FOCUS_IN)
	await settled(scene)
	check(scene.mode == "won", "Focus-resumed committed group reaches win")
	scene.queue_free()
	await process_frame
	await process_frame
	await create_timer(0.1).timeout
	print("SCENE: %d assertions, %d failures" % [checks, failures])
	quit(0 if failures == 0 else 1)
