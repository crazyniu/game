extends Control
## Root: owns the profile, switches screens and hosts popups.
## Capture: godot --path PerchSort -- --capture=<file.png> [--level=N | --preview=<script>:<state>] [--wait=S] [--motion]
## Tests: -- --smoke (tests/smoke.gd), -- --autoplay --level=N, -- --frames=<dir> --wait=S

var profile: Profile
var screen: Control
var popups: Control
var args := {}
var _booted := false
var _wipe: Control
var _wipe_leaves: Array[TextureRect] = []
var _wipe_back: ColorRect
var _wiping := false

const WIPE_SECONDS := 0.4


func _ready() -> void:
	size = Vector2(1080, 1920)
	for a in OS.get_cmdline_user_args():
		var kv := a.trim_prefix("--").split("=", true, 1)
		args[kv[0]] = kv[1] if kv.size() > 1 else "1"
	profile = Profile.new() if sandboxed() else Profile.load_saved()
	if args.has("profile"):
		profile = Profile.from_dict(JSON.parse_string(args["profile"]))
	Sound.set_muted(profile.muted or sandboxed())
	Sound.set_music_muted(profile.music_muted or sandboxed())
	popups = Control.new()
	popups.name = "Popups"
	popups.mouse_filter = Control.MOUSE_FILTER_IGNORE
	popups.size = size
	add_child(popups)
	if args.has("smoke"):
		load("res://tests/smoke.gd").run(self)
		return
	if args.has("preview"):
		# --preview=<script basename>:<state>; the script's static preview(main, state) builds it with fake data.
		var parts: PackedStringArray = args["preview"].split(":", true, 1)
		var script: GDScript = load("res://game/%s.gd" % parts[0])
		script.call("preview", self, parts[1] if parts.size() > 1 else "")
	elif args.has("level"):
		open_board(int(args["level"]), not args.has("capture"))
	else:
		open_home()
		_offer_daily_gift()
	_booted = true
	_build_wipe()
	if args.has("autoplay"):
		_autoplay()
	if args.has("frames"):
		_frames()
	elif args.has("capture"):
		_capture()

func _capture() -> void:
	await get_tree().create_timer(float(args.get("wait", "0.5"))).timeout
	await RenderingServer.frame_post_draw
	get_viewport().get_texture().get_image().save_png(args["capture"])
	get_tree().quit()

func set_screen(node: Control) -> void:
	if screen:
		screen.queue_free()
	screen = node
	add_child(node)
	move_child(node, 0)

## Screen scripts are loaded on demand so each can be built and previewed independently.
var Popups: GDScript:
	get: return load("res://game/popups.gd")
var MapScreen: GDScript:
	get: return load("res://game/map_view.gd")
var ShopScreen: GDScript:
	get: return load("res://game/shop_view.gd")
var AviaryScreen: GDScript:
	get: return load("res://game/aviary_view.gd")
var Tutorial: GDScript:
	get: return load("res://game/tutorial.gd")

var _overlay: Control   # shop or aviary opened above the board

func instant() -> bool:
	return args.has("capture") and not args.has("motion")

func open_home() -> void:
	_go(_open_home)

func _open_home() -> void:
	_clear_popups()
	var home := HomeView.new()
	home.instant = instant()
	set_screen(home)
	home.setup(profile)
	home.play_requested.connect(_start_level)
	home.map_requested.connect(open_map)
	home.shop_requested.connect(func(): open_shop(0))
	home.aviary_requested.connect(open_aviary)
	home.settings_requested.connect(func(): show_settings(false))
	home.help_requested.connect(func(): Tutorial.open(popups, Callable(), instant()))
	home.checkin_requested.connect(show_checkin)
	home.chest_requested.connect(show_chest)
	Sound.music("bgm_menu")

func open_map() -> void:
	_go(_open_map)

func _open_map() -> void:
	var map: Control = MapScreen.new()
	set_screen(map)
	map.setup(profile)
	map.level_tapped.connect(show_level_card)
	map.shop_requested.connect(func(): open_shop(0))
	map.aviary_requested.connect(open_aviary)
	map.settings_requested.connect(func(): show_settings(false))
	map.help_requested.connect(func(): Tutorial.open(popups, Callable(), instant()))
	map.home_requested.connect(open_home)
	Sound.music("bgm_menu")

## The seven-day check-in opens by itself on the first launch of a day.
func _offer_daily_gift() -> void:
	if Progress.checkin_day(profile, Progress.today()) > 0:
		show_checkin()

func show_checkin() -> void:
	var day := Progress.checkin_day(profile, Progress.today())
	Popups.checkin(popups, day, day - 1 if day > 0 else profile.checkin_streak, func():
		var got := Progress.claim_checkin(profile, Progress.today())
		if got.is_empty():
			return
		Sound.play("collect")
		save()
		Popups.rewards(popups, "checkin.title", got["coins"], got["boosters"], _refresh_screen))

func show_chest() -> void:
	Popups.chest(popups, Progress.chest_progress(profile), Progress.chests_ready(profile), func():
		var got := Progress.open_chest(profile)
		if got.is_empty():
			return
		Sound.play("collect")
		save()
		Popups.rewards(popups, "chest.title", got["coins"], got["boosters"], _refresh_screen))

func _refresh_screen() -> void:
	if screen and screen.has_method("setup") and not screen is BoardView:
		screen.setup(profile)
	elif screen is BoardView:
		screen.refresh_hud()

static func mechanics_of(level_def: Dictionary) -> Array[String]:
	var found: Array[String] = []
	var add := func(m: String): if not found.has(m): found.append(m)
	for b in level_def["branches"]:
		match str(b.get("kind", "")):
			"twig": add.call("twig")
			"growing": add.call("growing")
			"swivel": add.call("swivel")
		for token in b["birds"]:
			var t := str(token)
			if t.begins_with("egg:"): add.call("egg")
			if t.ends_with(":sleep"): add.call("sleep")
			if t == "crow": add.call("crow")
	if int(level_def.get("dayLength", 0)) > 0:
		add.call("daynight")
	return found

func show_level_card(level_id: int) -> void:
	var def := Levels.get_level(level_id)
	var board := Levels.create_board(def)
	var species: Array[String] = []
	species.assign(Array(Levels.species_ids(board)))
	Popups.level_card(popups, level_id, profile.best_stars(level_id), profile.best_moves[level_id], int(def["par"]), species,
		mechanics_of(def), func(): _start_level(level_id), Progress.is_level_unlocked(profile, level_id))

func _start_level(level_id: int) -> void:
	_go(func():
		_clear_popups()
		_open_board(level_id, not instant()))

## Runs a screen change under the leaf wipe: the action fires while the curtain covers the screen.
func _go(action: Callable) -> void:
	if not _booted or instant() or _wiping:
		action.call()
		return
	_wiping = true
	_wipe.visible = true
	move_child(_wipe, get_child_count() - 1)
	var tw := create_tween()
	tw.tween_method(_pose_wipe, 0.0, 1.0, WIPE_SECONDS * 0.5)
	tw.tween_callback(action)
	tw.tween_method(_pose_wipe, 1.0, 0.0, WIPE_SECONDS * 0.5)
	await tw.finished
	_wipe.visible = false
	_wiping = false

## Six leaf sprites slide in from both sides and close over the screen; the right-hand ones are mirrored scenery.
func _build_wipe() -> void:
	_wipe = Control.new()
	_wipe.name = "LeafWipe"
	_wipe.size = size
	_wipe.visible = false
	add_child(_wipe)
	_wipe_back = ColorRect.new()
	_wipe_back.color = Color(0.22, 0.38, 0.09, 0.0)
	_wipe_back.size = size
	_wipe_back.mouse_filter = Control.MOUSE_FILTER_STOP
	_wipe.add_child(_wipe_back)
	for i in 6:
		var leaf := TextureRect.new()
		leaf.texture = Art.tex("leaf_wipe")
		leaf.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
		leaf.size = Vector2.ONE * maxf(size.x * 1.7, size.y * 1.2)
		leaf.pivot_offset = leaf.size * 0.5
		leaf.mouse_filter = Control.MOUSE_FILTER_IGNORE
		if i % 2 == 1:
			leaf.scale.x = -1.0
		_wipe.add_child(leaf)
		_wipe_leaves.append(leaf)

func _pose_wipe(cover: float) -> void:
	var t := cover * cover * (3.0 - 2.0 * cover)
	_wipe_back.color.a = clampf((t - 0.85) / 0.15, 0.0, 1.0)
	for i in 6:
		var leaf := _wipe_leaves[i]
		var right := i % 2 == 1
		var from := (size.x + leaf.size.x * 0.65) if right else -leaf.size.x * 0.65
		var center := Vector2(lerpf(from, size.x if right else 0.0, t), size.y * (0.2 + (i / 2) * 0.3))
		leaf.position = center - leaf.size * 0.5
		leaf.rotation_degrees = (-1.0 if right else 1.0) * (1.0 - t) * 12.0

## Music pauses with the app and resumes on return.
func _notification(what: int) -> void:
	# Focus notifications also arrive while the window opens or closes, outside the scene tree.
	if not is_inside_tree():
		return
	if what == NOTIFICATION_APPLICATION_FOCUS_OUT and _booted and not sandboxed():
		get_tree().paused = true
	elif what == NOTIFICATION_APPLICATION_FOCUS_IN:
		get_tree().paused = false

func _clear_popups() -> void:
	for child in popups.get_children():
		child.queue_free()

func open_board(level_id: int, entrance := true) -> void:
	_go(_open_board.bind(level_id, entrance))

func _open_board(level_id: int, entrance: bool) -> void:
	_close_overlay()
	var board := BoardView.new()
	board.instant = instant()
	set_screen(board)
	var def := Levels.get_level(level_id)
	# Each level's first open hands out one useful booster, so players learn to use them (shown after the entrance).
	var gift := Progress.level_gift(profile, level_id, def)
	board.start(def, profile, entrance)
	board.profile_changed.connect(save)
	board.settings_requested.connect(func(): show_settings(true))
	board.goal_info_requested.connect(func(sp): Popups.goal_info(popups, sp))
	board.buy_requested.connect(func(kind): open_shop(0, true))
	board.won.connect(_on_won.bind(level_id))
	Sound.music("bgm_game")
	# First meetings and booster unlocks are shown after the entrance.
	var met := Aviary.on_level_started(profile, Levels.species_ids(board.session.state, true))
	var queue: Array[Callable] = []
	for sp in met:
		queue.append(func(done: Callable): Popups.new_friend(popups, sp, Aviary.NEW_SPECIES_COINS, done))
	for info in Progress.BOOSTERS:
		var flag := "unlock.%s" % info[1]
		if info[2] == level_id and not profile.has_tutorial(flag):
			profile.set_tutorial(flag)
			queue.append(func(done: Callable): Popups.unlock(popups, info[0], done))
	if gift >= 0:
		queue.append(func(done: Callable): Popups.level_gift(popups, gift, func():
			if is_instance_valid(board) and board.has_method("point_at_tool"):
				board.point_at_tool(gift)
			done.call()))
	# The first open of level 1 ends with the how-to-play steps, shown once.
	if level_id == 1 and not profile.has_tutorial(Tutorial.FLAG):
		profile.set_tutorial(Tutorial.FLAG)
		queue.append(func(done: Callable): Tutorial.open(popups, done, instant()))
	save()
	if not queue.is_empty():
		while entrance and is_instance_valid(board) and board.busy:
			await get_tree().process_frame
		_run_queue(queue)

## Shows popups one after another; each receives a `done` callable.
func _run_queue(queue: Array[Callable], then := Callable()) -> void:
	if queue.is_empty():
		if then.is_valid():
			then.call()
		return
	var next: Callable = queue.pop_front()
	next.call(func(): _run_queue(queue, then))

func _on_won(result: Dictionary, level_id: int) -> void:
	var session: GameSession = result["session"]
	var world := Progress.world_of_level(level_id)
	var was_complete := profile.rewarded_worlds.has(world)
	for sp in session.state.homed:
		Aviary.add_flock_home(profile, Board.SPECIES[sp])
	var payout := Progress.on_win(profile, level_id, session.move_count(), session.par, session.state.chorus_coins)
	profile.record_moves(level_id, session.move_count())
	profile.inventory = session.state.inventory.duplicate()
	save()
	var next_level := level_id + 1
	var has_next := next_level <= 40 and Progress.is_level_unlocked(profile, next_level)
	var after_win: Array[Callable] = []
	if not was_complete and payout["world"] > 0:
		after_win.append(func(done: Callable): Popups.world_complete(popups, world, payout["world"], done))
		if world < 4:
			after_win.append(func(done: Callable): Popups.world_unlock(popups, world + 1, done))
	Popups.win(popups, payout["stars"], session.move_count(), session.par, payout["total"] - payout["world"],
		func(): _run_queue(after_win, func(): open_board(level_id)),
		func(): _run_queue(after_win, func(): open_board(next_level) if has_next else open_map()),
		func(): _run_queue(after_win, open_home), has_next)
	# The HUD coin counter takes the reward once the win panel's coin burst lands on it.
	var board := screen as BoardView
	if not instant():
		await get_tree().create_timer(2.8).timeout
	if is_instance_valid(board) and screen == board:
		board.refresh_hud()

func show_settings(on_board: bool) -> void:
	Popups.settings(popups, not profile.music_muted, not profile.muted, {
		"music": func(on: bool):
			profile.music_muted = not on
			Sound.set_music_muted(profile.music_muted)
			save(),
		"sound": func(on: bool):
			profile.muted = not on
			Sound.set_muted(profile.muted)
			save(),
		"restart": func():
			_clear_popups()
			if screen is BoardView:
				var board: BoardView = screen
				board.session.restart()
				board.start(board.level, profile, false),
		"home": func(): _go(func():
			_clear_popups()
			_open_home()),
		"help": func(): Tutorial.open(popups, Callable(), instant()),
	}, on_board)

func open_shop(tab := 0, over_board := false) -> void:
	_go(_open_shop.bind(tab))

func _open_shop(tab: int) -> void:
	var shop: Control = ShopScreen.new()
	shop.setup(profile, tab)
	shop.profile_changed.connect(func():
		save()
		if screen is BoardView:
			var board: BoardView = screen
			for kind in Board.BOOSTER_COUNT:
				var gained := profile.inventory[kind] - board.session.state.inventory[kind]
				if gained > 0:
					board.granted(kind, gained)
		_refresh_screen())
	shop.closed.connect(func(): _go(_close_overlay))
	_show_overlay(shop)

func open_aviary() -> void:
	_go(_open_aviary)

func _open_aviary() -> void:
	var aviary: Control = AviaryScreen.new()
	aviary.setup(profile)
	aviary.profile_changed.connect(save)
	aviary.closed.connect(func(): _go(_close_overlay))
	_show_overlay(aviary)

func _show_overlay(node: Control) -> void:
	_close_overlay()
	_overlay = node
	add_child(node)
	move_child(node, popups.get_index())  # below the popup layer
	Sound.music("bgm_menu")

func _close_overlay() -> void:
	if _overlay and is_instance_valid(_overlay):
		_overlay.queue_free()
	_overlay = null
	if screen is BoardView:
		Sound.music("bgm_game")
		screen.refresh_hud()
	elif screen:
		_refresh_screen()

## Captures and tests use a fresh, silent profile that is never written.
func sandboxed() -> bool:
	return args.has("capture") or args.has("smoke")

func save() -> void:
	if not sandboxed():
		profile.save()

## Test driver: taps the solver's solution through the real board, logging timings and the result.
func _autoplay() -> void:
	var board := screen as BoardView
	var t0 := Time.get_ticks_msec()
	while board.busy:
		await get_tree().process_frame
	print("AUTOPLAY entrance_ms=%d" % (Time.get_ticks_msec() - t0))
	var plan: Dictionary = Solver.solve(board.session.state, [], 400000, 60000)
	for move in plan["moves"]:
		if move[1] < 0:
			board.tap_branch(move[0])
			if board.session.selected >= 0:
				board.tap_branch(move[0])
		else:
			board.tap_branch(move[0])
			board.tap_branch(move[1])
		await get_tree().process_frame
		while board.busy and not board.session.state.is_won():
			await get_tree().process_frame
	print("AUTOPLAY won=%s moves=%d par=%d total_ms=%d" % [board.session.state.is_won(), board.session.move_count(), board.session.par, Time.get_ticks_msec() - t0])
	await get_tree().create_timer(1.0).timeout
	get_tree().quit()

## Timed evidence: saves a frame every --step seconds (default 0.25) for --frames=<dir> over --wait seconds.
func _frames() -> void:
	var dir: String = args["frames"]
	DirAccess.make_dir_recursive_absolute(dir)
	var step := float(args.get("step", "0.25"))
	var count := int(float(args.get("wait", "5")) / step)
	for i in count:
		await RenderingServer.frame_post_draw
		get_viewport().get_texture().get_image().save_png("%s/f%03d.png" % [dir, i])
		await get_tree().create_timer(step).timeout
	get_tree().quit()
