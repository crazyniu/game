extends SceneTree
const Store = preload("res://core/save_store.gd")
const Rules = preload("res://core/first_level_rules.gd")
var checks := 0
var failures := 0
var directories: Array[String] = []

class FailingStore extends Store:
	var fail_write := true
	var hide_after_replace := false
	var hidden := false
	func _write_bytes(path: String, bytes: PackedByteArray) -> Error:
		return ERR_FILE_CANT_WRITE if fail_write else super._write_bytes(path, bytes)
	func _replace_file(source: String, destination: String) -> Error:
		var result := super._replace_file(source, destination)
		if hide_after_replace and destination == save_path:
			hidden = true
		return result
	func _read_bytes(path: String) -> PackedByteArray:
		return PackedByteArray() if hidden and path == save_path else super._read_bytes(path)

func check(condition: bool, label: String) -> void:
	checks += 1
	if not condition:
		failures += 1
		printerr("FAIL: " + label)

func directory(label: String) -> String:
	var path := "/tmp/bird-save-tests-%s-%s-%s" % [OS.get_process_id(), Time.get_ticks_usec(), label]
	directories.append(path)
	DirAccess.make_dir_recursive_absolute(path)
	return path

func write(path: String, bytes: PackedByteArray) -> void:
	var file := FileAccess.open(path, FileAccess.WRITE)
	file.store_buffer(bytes)
	file.close()

func next_record(store: Store, session, source: int, target: int) -> Dictionary:
	var tx: Dictionary = session.prepare_transfer(source, target)
	return {"tx": tx, "record": Store.make_record(tx.state, tx.history, store.confirmed_record())}

func _initialize() -> void:
	var path := directory("roundtrip")
	var legacy := path.path_join("save6.json")
	write(legacy, "untouched Unity save6 evidence".to_utf8_buffer())
	var store := Store.new(path)
	check(store.load_record().status == "new", "new installation")
	var session = Rules.Session.new()
	var prepared := next_record(store, session, 1, 0)
	check(store.commit(prepared.record).status == "committed", "first move persisted")
	check(session.commit(prepared.tx), "core commit after save")
	var reopened := Store.new(path)
	var loaded := reopened.load_record()
	check(loaded.status == "loaded" and loaded.record.state == session.snapshot(), "reload exact state")
	check(loaded.record.history == session.history_snapshot(), "reload exact history")
	check(FileAccess.get_file_as_string(legacy) == "untouched Unity save6 evidence", "Unity save6 untouched")
	var audio: Dictionary = store.confirmed_record()
	audio.audio_muted = true
	check(store.commit(audio).status == "committed", "audio preference saved")
	prepared = next_record(store, session, 2, 1)
	check(store.commit(prepared.record).status == "committed", "win persisted")
	check(session.commit(prepared.tx), "win core committed")
	var undo: Dictionary = session.prepare_undo()
	check(store.commit(Store.make_record(undo.state, undo.history, store.confirmed_record())).status == "committed", "undo persisted")
	check(session.commit(undo), "undo core committed")
	var record := store.confirmed_record()
	check(record.completed and record.best == 2 and record.audio_muted and record.state.move_count == 1, "undo preserves best/completed/audio")
	var restart: Dictionary = session.prepare_restart()
	check(store.commit(Store.make_record(restart.state, restart.history, record)).status == "committed", "restart persisted")
	record = Store.new(path).load_record().record
	check(record.completed and record.best == 2 and record.audio_muted and record.history.is_empty(), "restart/reload preserves progress and prefs")

	var retry_store := FailingStore.new(directory("retry"))
	retry_store.load_record()
	session = Rules.Session.new()
	prepared = next_record(retry_store, session, 1, 0)
	check(retry_store.commit(prepared.record).status == "not_committed" and retry_store.has_pending(), "write failure retains candidate")
	check(session.snapshot().move_count == 0 and retry_store.confirmed_record().state.move_count == 0, "failure does not change authoritative state")
	var edited: Dictionary = prepared.record.duplicate(true)
	edited.audio_muted = true
	check(retry_store.commit(edited).status == "not_committed", "different candidate blocked")
	prepared.record.state.move_count = 99
	retry_store.fail_write = false
	check(retry_store.retry().status == "committed", "same bytes retry succeeds despite caller mutation")
	check(retry_store.confirmed_record().state.move_count == 1, "retry exactly one move")
	check(retry_store.retry().status == "not_committed", "repeated retry cannot double move")

	var uncertain := FailingStore.new(directory("uncertain"))
	uncertain.fail_write = false
	uncertain.load_record()
	uncertain.hide_after_replace = true
	prepared = next_record(uncertain, Rules.Session.new(), 1, 0)
	check(uncertain.commit(prepared.record).status == "uncertain", "post-rename unreadable outcome uncertain")
	check(uncertain.confirmed_record().state.move_count == 0 and uncertain.has_pending(), "uncertain keeps old state and candidate")
	uncertain.hidden = false
	check(uncertain.retry().status == "committed" and uncertain.confirmed_record().state.move_count == 1, "retry recognizes already committed bytes")

	var original := FileAccess.get_file_as_bytes(store.save_path)
	var envelope: Dictionary = bytes_to_var(original)
	envelope.record.state.move_count = 20
	write(store.save_path, var_to_bytes(envelope))
	var recovered := Store.new(path).load_record()
	check(recovered.status == "recovered" and recovered.record.state.move_count == 1, "known corrupt main recovers complete previous backup")
	check(FileAccess.get_file_as_bytes(store.save_path) == var_to_bytes(envelope), "corrupt main preserved during load")
	envelope.version = 2
	var unknown := var_to_bytes(envelope)
	write(store.save_path, unknown)
	var readonly := Store.new(path)
	check(readonly.load_record().status == "readonly", "future version does not restore old backup")
	check(readonly.commit(Store.initial_record()).status == "not_committed" and FileAccess.get_file_as_bytes(store.save_path) == unknown, "future version never overwritten")
	envelope.version = 1
	envelope.record.future_economy = 5
	write(store.save_path, var_to_bytes(envelope))
	check(Store.new(path).load_record().status == "readonly", "unknown field masks no corrupt-state recovery")
	envelope = bytes_to_var(original)
	envelope.record.state.sequence = 4.0
	write(store.save_path, var_to_bytes(envelope))
	check(Store.new(path).load_record().status == "readonly", "float integers never coerced")


	var malformed := original.duplicate()
	malformed.append(42)
	write(store.save_path, malformed)
	check(Store.new(path).load_record().status == "readonly", "trailing bytes rejected without overwriting evidence")
	envelope = bytes_to_var(original)
	envelope.record.state.branches[0].future_field = false
	write(store.save_path, var_to_bytes(envelope))
	check(Store.new(path).load_record().status == "readonly", "unknown nested state fields preserved")
	envelope = bytes_to_var(original)
	envelope.record.audio_muted = 1
	write(store.save_path, var_to_bytes(envelope))
	check(Store.new(path).load_record().status == "readonly", "wrong boolean type preserved")
	var backup_bytes := FileAccess.get_file_as_bytes(store.save_path + ".bak")
	DirAccess.remove_absolute(store.save_path)
	var missing_main := Store.new(path)
	check(missing_main.load_record().status == "recovered", "missing main recovers compatible backup")
	var recovered_preferences := missing_main.confirmed_record()
	recovered_preferences.audio_muted = false
	check(missing_main.commit(recovered_preferences).status == "committed", "recovered backup can safely persist next change")
	check(FileAccess.get_file_as_bytes(store.save_path + ".bak") == backup_bytes, "only good backup preserved on recovery commit")
	var forged_progress := missing_main.confirmed_record()
	forged_progress.best = 3
	check(missing_main.commit(forged_progress).status == "not_committed", "best result cannot regress")
	var forged_completion := Store.new(directory("forged"))
	forged_completion.load_record()
	var forged := Store.initial_record()
	forged.completed = true
	forged.best = 2
	check(forged_completion.commit(forged).status == "not_committed", "completion cannot be invented without win")
	var corrupt_only := Store.new(directory("no-backup"))
	envelope = bytes_to_var(original)
	envelope.record.state.move_count = 10
	envelope.record_hash = Store._hash(var_to_bytes(envelope.record))
	write(corrupt_only.save_path, var_to_bytes(envelope))
	check(corrupt_only.load_record().status == "readonly", "semantically corrupt known record without backup is readonly")

	var precision := Store.new(directory("precision"))
	precision.load_record()
	var exact := Store.initial_record()
	exact.state.sequence = 9007199254740993
	write(precision.save_path, Store._encode(exact))
	check(Store.new(precision.save_path.get_base_dir()).load_record().record.state.sequence == 9007199254740993, "int64 beyond double precision is exact")
	var stale := Store.new(directory("stale"))
	stale.load_record()
	var other := Store.new(stale.save_path.get_base_dir())
	other.load_record()
	var preferences := Store.initial_record()
	preferences.audio_muted = true
	check(other.commit(preferences).status == "committed", "other instance saves")
	prepared = next_record(stale, Rules.Session.new(), 1, 0)
	check(stale.commit(prepared.record).status == "uncertain", "stale instance cannot overwrite observed external update")
	for dir in directories:
		for filename in DirAccess.get_files_at(dir):
			DirAccess.remove_absolute(dir.path_join(filename))
		DirAccess.remove_absolute(dir)
	print("SAVE TESTS: %d checks, %d failures" % [checks, failures])
	quit(1 if failures else 0)
