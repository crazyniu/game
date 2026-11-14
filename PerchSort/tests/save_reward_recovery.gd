extends RefCounted
## Regression checks use only a unique temporary directory, never user://profile.json.
static func run(check: Callable) -> void:
	var was_unlimited := Progress.unlimited
	Progress.unlimited = false
	var p := Profile.new()
	p.last_daily_gift = "2026-11-14"
	p.checkin_streak = 5
	p.draws_date = "2026-11-14"
	p.draws_left = 0
	Progress.begin_draw_day(p, "2026-11-13")
	check.call(p.draws_date == "2026-11-14" and p.draws_left == 0, "rollback does not reissue draws")
	check.call(Progress.draw(p, "2026-11-13").is_empty(), "rollback cannot pay a draw")
	Progress.begin_draw_day(p, "2026-11-14")
	check.call(p.draws_left == 0, "restoring date does not reissue draws")
	p.draws_left = 2
	check.call(not Progress.draws_available(p, "2026-11-13") and Progress.draw(p, "2026-11-13").is_empty()
		and p.draws_left == 2, "rollback preserves remaining draws without spending them")
	check.call(not Progress.draw(p, "2026-11-14").is_empty() and p.draws_left == 1, "original date resumes remaining draws")
	Progress.begin_draw_day(p, "2026-11-15")
	check.call(p.draws_left == Progress.DRAW_MAX_SPINS, "next day still grants normal draws")
	Progress.unlimited = true
	p.draws_left = 0
	check.call(not Progress.draw(p, "2026-11-14").is_empty(), "explicit unlimited preview remains supported")
	Progress.unlimited = was_unlimited
	var dir := OS.get_temp_dir().path_join("perch-save-check-%d-%d" % [OS.get_process_id(), Time.get_ticks_usec()])
	DirAccess.make_dir_recursive_absolute(dir)
	var path := dir.path_join("profile.json")
	p = Profile.new()
	p.coins = 5000
	p.highest_unlocked = 40
	p.save(path)
	check.call(Profile.load_saved(path).coins == 5000, "first save round trips")
	p.coins = 5100
	p.save(path)
	check.call(Profile.load_saved(path).coins == 5100 and FileAccess.file_exists(path + ".bak"), "second save keeps backup")
	var file := FileAccess.open(path, FileAccess.WRITE)
	file.store_string('{"schemaVersion":5,')
	file.close()
	var recovered := Profile.load_saved(path)
	check.call(recovered.coins == 5000 and recovered.highest_unlocked == 40, "truncated primary recovers last good backup")
	recovered.save(path)
	check.call(Profile.load_saved(path).coins == 5000, "recovered profile can save safely")
	DirAccess.remove_absolute(path)
	check.call(Profile.load_saved(path).coins == 5000, "missing primary recovers backup")
	DirAccess.remove_absolute(path + ".bak")
	file = FileAccess.open(path, FileAccess.WRITE)
	file.store_string("broken")
	file.close()
	var blocked := Profile.load_saved(path)
	blocked.coins = 900
	blocked.save(path)
	check.call(FileAccess.get_file_as_string(path) == "broken", "unrecoverable primary is preserved instead of overwritten")
	file = FileAccess.open(path + ".bak", FileAccess.WRITE)
	file.store_string("also broken")
	file.close()
	Profile.load_saved(path).save(path)
	check.call(FileAccess.get_file_as_string(path) == "broken" and FileAccess.get_file_as_string(path + ".bak") == "also broken",
		"both damaged files are preserved")
	DirAccess.remove_absolute(path + ".bak")
	DirAccess.remove_absolute(path)
	check.call(Profile.load_saved(path).highest_unlocked == 1, "truly absent save remains new-player flow")
	p.save(path)
	DirAccess.make_dir_absolute(path + ".tmp")
	p.coins = 9999
	p.save(path)
	check.call(Profile.load_saved(path).coins == 5100, "failed temporary write leaves primary untouched")
	DirAccess.remove_absolute(path + ".tmp")
	for suffix in ["", ".tmp", ".bak", ".bak.tmp"]:
		if FileAccess.file_exists(path + suffix):
			DirAccess.remove_absolute(path + suffix)
	DirAccess.remove_absolute(dir)
