class_name SaveStore
extends RefCounted
## Isolated Godot save, NOT a Unity save6 adapter. Single writer per physical path.
## Flush/readback + same-directory rename is not a cross-platform fsync guarantee.
const Rules = preload("res://core/first_level_rules.gd")
const FILE_NAME := "first-level.godot-save-v1.bin"
const MAX_BYTES := 4 * 1024 * 1024
const CONTENT_HASH := "4e912c846f652e130ff92430e71d2b6d4f053aa3264ca2c031ddd962945b7d79"
var save_path: String
var is_read_only := false
var _loaded := false
var _confirmed: Dictionary = {}
var _baseline := PackedByteArray()
var _had_main := false
var _pending := PackedByteArray()
var _pending_record: Dictionary = {}

func _init(directory: String = "user://first-level-godot-v1") -> void:
	save_path = directory.path_join(FILE_NAME)

static func initial_record() -> Dictionary:
	return {"state": Rules.initial_state(), "history": [], "completed": false, "best": -1, "audio_muted": false}

static func make_record(state: Dictionary, history: Array, previous: Dictionary) -> Dictionary:
	var record: Dictionary = previous.duplicate(true)
	record.state = state.duplicate(true)
	record.history = history.duplicate(true)
	if Rules.is_won(state):
		record.completed = true
		record.best = state.move_count if record.best < 0 else mini(record.best, state.move_count)
	return record

func confirmed_record() -> Dictionary:
	return _confirmed.duplicate(true)

func has_pending() -> bool:
	return not _pending.is_empty()

static func _keys(value: Variant, names: Array) -> bool:
	if not value is Dictionary or value.size() != names.size():
		return false
	for key in value:
		if not key is String:
			return false
	for key in names:
		if not value.has(key):
			return false
	return true

static func _state_shape(value: Variant) -> bool:
	if not _keys(value, ["flock_size", "species_count", "move_count", "sequence", "branches"]):
		return false
	for key in ["flock_size", "species_count", "move_count", "sequence"]:
		if not value[key] is int:
			return false
	if value.flock_size != 4 or value.species_count != 2 or not value.branches is Array:
		return false
	for branch in value.branches:
		if not _keys(branch, ["id", "tokens", "home_species"]) or not branch.id is String or not branch.home_species is int or not branch.tokens is Array:
			return false
		for token in branch.tokens:
			if not _keys(token, ["id", "species"]) or not token.id is String or not token.species is int:
				return false
	return true

static func _record_shape(record: Variant) -> bool:
	if not _keys(record, ["state", "history", "completed", "best", "audio_muted"]):
		return false
	if not _state_shape(record.state) or not record.history is Array or not record.completed is bool or not record.best is int or not record.audio_muted is bool:
		return false
	for item in record.history:
		if not _keys(item, ["before", "from", "to", "count"]) or not _state_shape(item.before):
			return false
		for key in ["from", "to", "count"]:
			if not item[key] is int:
				return false
	return true

static func _record_valid(record: Dictionary) -> bool:
	var session = Rules.Session.new()
	if not session.restore(record.state, record.history):
		return false
	if record.completed != (record.best >= 2) or (not record.completed and record.best != -1):
		return false
	return not Rules.is_won(record.state) or (record.completed and record.best <= record.state.move_count)

static func _hash(bytes: PackedByteArray) -> String:
	var context := HashingContext.new()
	context.start(HashingContext.HASH_SHA256)
	context.update(bytes)
	return context.finish().hex_encode()

static func _encode(record: Dictionary) -> PackedByteArray:
	return var_to_bytes({"format": "perch-sort-godot", "version": 1, "campaign_id": "bird-home-peer-v4", "content_hash": CONTENT_HASH, "record": record, "record_hash": _hash(var_to_bytes(record))})

static func _decode(bytes: PackedByteArray) -> Dictionary:
	if bytes.is_empty() or bytes.size() > MAX_BYTES:
		return {"status": "unsupported"}
	# bytes_to_var never deserializes objects (unlike bytes_to_var_with_objects).
	var root: Variant = bytes_to_var(bytes)
	# Reject trailing bytes and noncanonical encodings instead of silently dropping data.
	if var_to_bytes(root) != bytes:
		return {"status": "unsupported"}
	if not _keys(root, ["format", "version", "campaign_id", "content_hash", "record", "record_hash"]):
		return {"status": "unsupported"}
	if not root.version is int or root.version != 1 or root.format != "perch-sort-godot" or root.campaign_id != "bird-home-peer-v4" or root.content_hash != CONTENT_HASH or not root.record_hash is String:
		return {"status": "unsupported"}
	# Whole-document preflight BEFORE corruption recovery, so unknown data stays intact.
	if not _record_shape(root.record):
		return {"status": "unsupported"}
	if root.record_hash != _hash(var_to_bytes(root.record)) or not _record_valid(root.record):
		return {"status": "corrupt"}
	return {"status": "valid", "record": root.record}

func _read_bytes(path: String) -> PackedByteArray:
	var file := FileAccess.open(path, FileAccess.READ)
	if file == null or file.get_length() > MAX_BYTES:
		return PackedByteArray()
	return file.get_buffer(file.get_length())

func _write_bytes(path: String, bytes: PackedByteArray) -> Error:
	var file := FileAccess.open(path, FileAccess.WRITE)
	if file == null:
		return FileAccess.get_open_error()
	file.store_buffer(bytes)
	file.flush()
	var error := file.get_error()
	file.close()
	if error != OK:
		return error
	return OK if _read_bytes(path) == bytes else ERR_FILE_CORRUPT

func _replace_file(source: String, destination: String) -> Error:
	return DirAccess.rename_absolute(source, destination)

func load_record() -> Dictionary:
	if _loaded:
		return {"status": "readonly" if is_read_only else "loaded", "record": confirmed_record(), "message": "Store already loaded."}
	_loaded = true
	_confirmed = initial_record()
	_had_main = FileAccess.file_exists(save_path)
	_baseline = _read_bytes(save_path) if _had_main else PackedByteArray()
	var main: Dictionary = _decode(_baseline) if _had_main else {"status": "missing"}
	if main.status == "valid":
		_confirmed = main.record
		return {"status": "loaded", "record": confirmed_record(), "message": ""}
	if main.status == "unsupported":
		return _readonly("Unknown or unreadable save preserved; this save cannot be changed.")
	var backup_path := save_path + ".bak"
	if FileAccess.file_exists(backup_path):
		var backup: Dictionary = _decode(_read_bytes(backup_path))
		if backup.status == "valid":
			if _had_main and _write_bytes(save_path + ".corrupt-" + _hash(_baseline), _baseline) != OK:
				return _readonly("Damaged save could not be preserved; recovery is read-only.")
			_confirmed = backup.record
			return {"status": "recovered", "record": confirmed_record(), "message": "Recovered complete backup; original preserved."}
		return _readonly("No compatible backup; original files preserved.")
	if _had_main:
		return _readonly("Damaged save preserved; no compatible backup.")
	return {"status": "new", "record": confirmed_record(), "message": "Godot uses a separate save. Unity progress is not imported."}

func _readonly(message: String) -> Dictionary:
	is_read_only = true
	_confirmed.audio_muted = true
	return {"status": "readonly", "record": confirmed_record(), "message": message}

func _outcome(status: String, message: String = "") -> Dictionary:
	return {"status": status, "message": message}

func commit(record: Dictionary) -> Dictionary:
	if not _loaded or is_read_only:
		return _outcome("not_committed", "Storage is read-only or not loaded.")
	if has_pending():
		if record != _pending_record:
			return _outcome("not_committed", "Retry the unchanged pending candidate first.")
		return retry()
	if not _record_shape(record) or not _record_valid(record):
		return _outcome("not_committed", "Invalid save candidate.")
	var same_game: bool = record.state == _confirmed.state and record.history == _confirmed.history
	if not same_game and (_confirmed.state.sequence == 9223372036854775807 or record.state.sequence != _confirmed.state.sequence + 1):
		return _outcome("not_committed", "Candidate must use the next sequence.")
	var expected_progress := make_record(record.state, record.history, _confirmed)
	if record.completed != expected_progress.completed or record.best != expected_progress.best:
		return _outcome("not_committed", "Progress must preserve confirmed results and derive wins from the candidate.")
	_pending_record = record.duplicate(true)
	_pending = _encode(_pending_record)
	if _pending.size() > MAX_BYTES:
		_pending.clear()
		_pending_record.clear()
		return _outcome("not_committed", "Save exceeds size limit; history was not truncated.")
	return retry()

func _matches_baseline() -> bool:
	return FileAccess.file_exists(save_path) == _had_main and (not _had_main or _read_bytes(save_path) == _baseline)

func _accept_pending() -> Dictionary:
	_confirmed = _pending_record.duplicate(true)
	_baseline = _pending.duplicate()
	_had_main = true
	_pending.clear()
	_pending_record.clear()
	is_read_only = false
	return _outcome("committed")

func retry() -> Dictionary:
	if not has_pending():
		return _outcome("not_committed", "No pending candidate.")
	if FileAccess.file_exists(save_path) and _read_bytes(save_path) == _pending:
		return _accept_pending()
	if not _matches_baseline():
		is_read_only = true
		return _outcome("uncertain", "Save changed or cannot be read. Pending candidate retained.")
	is_read_only = false
	if DirAccess.make_dir_recursive_absolute(save_path.get_base_dir()) != OK:
		return _outcome("not_committed", "Save directory unavailable. Retry is safe.")
	var temporary := save_path + ".tmp"
	if _write_bytes(temporary, _pending) != OK:
		return _outcome("not_committed", "Temporary save failed. Candidate retained.")
	if not _matches_baseline():
		is_read_only = true
		return _outcome("uncertain", "Save changed during staging. Candidate retained.")
	# Keep the previous valid snapshot, never replace a good backup with corrupt data.
	if _had_main and _decode(_baseline).status == "valid":
		if _write_bytes(save_path + ".bak.tmp", _baseline) != OK or _replace_file(save_path + ".bak.tmp", save_path + ".bak") != OK:
			return _outcome("not_committed", "Backup failed. Candidate retained.")
	var error := _replace_file(temporary, save_path)
	if FileAccess.file_exists(save_path) and _read_bytes(save_path) == _pending:
		return _accept_pending()
	if _matches_baseline():
		return _outcome("not_committed", "Replacement failed (%d). Candidate retained." % error)
	is_read_only = true
	return _outcome("uncertain", "Replacement could not be verified. Retry reads back the same candidate.")
