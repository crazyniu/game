extends SceneTree
## The launcher's test mode (--unlimited): chest, check-in, lucky draw and daily challenge never run out; off by default.

var failures := 0
var checks := 0

func check(ok: bool, label: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		print("FAIL ", label)

func _initialize() -> void:
	var today := "2026-11-13"
	# Normal rules: a fresh profile has no chest, and today's draws and challenge are used up once taken.
	Progress.unlimited = false
	var p := Profile.new()
	check(Progress.chests_ready(p) == 0, "no chest without stars")
	check(Progress.open_chest(p).is_empty(), "no chest opens without stars")
	Progress.begin_draw_day(p, today)
	var spins := p.draws_left
	for i in spins:
		check(not Progress.draw(p, today).is_empty(), "draw %d pays" % i)
	check(Progress.draw(p, today).is_empty() and not Progress.draws_available(p, today), "draws run out")
	check(not Progress.complete_challenge(p, today).is_empty(), "challenge pays once")
	check(not Progress.challenge_available(p, today), "challenge done for today")
	# Test mode: everything repeats, and earned chests are not spent by the extra ones.
	Progress.unlimited = true
	var q := Profile.new()
	for i in 5:
		var coins := q.coins
		check(Progress.chests_ready(q) >= 1, "chest always ready")
		check(not Progress.open_chest(q).is_empty() and q.coins > coins, "chest %d pays" % i)
	check(q.chests_opened == 0, "extra chests do not spend earned stars")
	for i in 20:
		check(Progress.draws_available(q, today), "draws always available")
		check(not Progress.draw(q, today).is_empty(), "test draw %d pays" % i)
	for i in 3:
		check(not Progress.complete_challenge(q, today).is_empty(), "challenge pays again %d" % i)
	check(Progress.checkin_day(q, today) > 0, "check-in can be claimed again today")
	Progress.unlimited = false
	print("unlimited checks: %d checks, %d failures" % [checks, failures])
	quit(1 if failures else 0)
