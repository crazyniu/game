extends Control
## Root: owns the profile, switches screens and hosts popups.
## Capture: godot --path PerchSort -- --capture=<file.png> [--level=N | --preview=<script>:<state>] [--wait=S] [--motion]

var profile: Profile
var screen: Control
var popups: Control
var args := {}


func _ready() -> void:
	size = Vector2(1080, 1920)
	for a in OS.get_cmdline_user_args():
		var kv := a.trim_prefix("--").split("=", true, 1)
		args[kv[0]] = kv[1] if kv.size() > 1 else "1"
	profile = Profile.new() if args.has("capture") else Profile.load_saved()
	if args.has("profile"):
		profile = Profile.from_dict(JSON.parse_string(args["profile"]))
	Sound.set_muted(profile.muted or args.has("capture"))
	popups = Control.new()
	popups.name = "Popups"
	popups.mouse_filter = Control.MOUSE_FILTER_IGNORE
	popups.size = size
	add_child(popups)
	if args.has("preview"):
		# --preview=<script basename>:<state>; the script's static preview(main, state) builds it with fake data.
		var parts: PackedStringArray = args["preview"].split(":", true, 1)
		var script: GDScript = load("res://game/%s.gd" % parts[0])
		script.call("preview", self, parts[1] if parts.size() > 1 else "")
	elif args.has("level"):
		open_board(int(args["level"]), not args.has("capture"))
	else:
		open_board(Progress.next_level(profile) if Progress.next_level(profile) > 0 else 1, true)
	if args.has("capture"):
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

func open_board(level_id: int, entrance := true) -> void:
	var board := BoardView.new()
	board.instant = args.has("capture") and not args.has("motion")
	set_screen(board)
	board.start(Levels.get_level(level_id), profile, entrance)
	board.profile_changed.connect(save)
	Sound.music("bgm_game")

func save() -> void:
	if not args.has("capture"):
		profile.save()
