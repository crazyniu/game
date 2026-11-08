extends Node
## Autoload "Sound": background music per page and one-shot effects, following the mute setting.

var muted := false          # sound effects off
var music_muted := false    # background music off
var vibration := true       # haptic pulses (profile setting); only handheld devices vibrate
const MUSIC_DB := -8.0
const CROSSFADE := 1.0
var _music: AudioStreamPlayer      # the track that is current (fading in or full)
var _old_music: AudioStreamPlayer  # the previous track while it fades out
var _fade: Tween
var _music_name := ""
var _effects: Array[AudioStreamPlayer] = []
var _cache := {}
var _last_chirp := -1
var _last_play := {}   # effect name -> msec of its last start, to drop duplicates within one frame burst

## Effect names used by the game mapped to audio files (the former AudioManager effect table).
## Boosters each get a distinct clip; "select" and "move" rotate through the chirps.
const ALIASES := {"collect": "nest_home", "win": "win_fanfare", "click": "ui_tap", "sleep": "sleep_z",
	"sapling": "crate_open", "owl_eye": "star_1", "morning_bell": "chirp_05", "scarecrow": "star_3",
	"spring_rain": "star_2"}


func _ready() -> void:
	_music = _music_player()
	_old_music = _music_player()
	for i in 8:
		var p := AudioStreamPlayer.new()
		add_child(p)
		_effects.append(p)

func _music_player() -> AudioStreamPlayer:
	var p := AudioStreamPlayer.new()
	p.volume_db = MUSIC_DB
	add_child(p)
	p.finished.connect(func(): if not music_muted and p.stream and p == _music: p.play())
	return p

func _stream(name: String) -> AudioStream:
	if not _cache.has(name):
		var path := "res://audio/%s.wav" % name
		if not ResourceLoader.exists(path):
			path = "res://audio/Audio/%s.wav" % name
		_cache[name] = load(path) if ResourceLoader.exists(path) else null
	return _cache[name]

## "bgm_game" on the board, "bgm_menu" elsewhere. Switching crossfades over one second; asking for the
## track that already plays does nothing, so popups and reopened screens never restart the music.
func music(name: String) -> void:
	if name == _music_name and (_music.playing or music_muted):
		return
	_music_name = name
	var stream := _stream(name)
	if _fade:
		_fade.kill()
	var swap := _old_music
	_old_music = _music
	_music = swap
	_music.stream = stream
	if music_muted or stream == null:
		_music.stop()
		_old_music.stop()
		return
	_music.volume_db = -60.0
	_music.play()
	_fade = create_tween().set_parallel()
	_fade.tween_property(_music, "volume_db", MUSIC_DB, CROSSFADE)
	if _old_music.playing:
		_fade.tween_property(_old_music, "volume_db", -60.0, CROSSFADE)
		_fade.chain().tween_callback(_old_music.stop)

func play(name: String, pitch := 1.0) -> void:
	if muted:
		return
	if name == "select" or name == "move":
		var pick := randi() % 4
		_last_chirp = pick if pick < _last_chirp else pick + 1
		name = "chirp_0%d" % (_last_chirp + 1)
	var now := Time.get_ticks_msec()
	if now - _last_play.get(name, -1000) < 50:
		return
	_last_play[name] = now
	var stream := _stream(ALIASES.get(name, name))
	if stream == null:
		return
	for p in _effects:
		if not p.playing:
			p.stream = stream
			p.pitch_scale = pitch
			p.play()
			return

## Haptic pulse lengths (ms) for board feedback.
const PULSE_PICK := 15
const PULSE_LAND := 25
const PULSE_HOME := 40

## A short vibration on phones and tablets when the setting is on; nothing elsewhere.
func vibrate(ms: int) -> void:
	if vibration and OS.has_feature("mobile"):
		Input.vibrate_handheld(clampi(ms, 15, 40))

## Sound effects on or off.
func set_muted(value: bool) -> void:
	muted = value

## Background music on or off.
func set_music_muted(value: bool) -> void:
	music_muted = value
	if _fade:
		_fade.kill()
	_old_music.stop()
	_music.volume_db = MUSIC_DB
	if music_muted:
		_music.stop()
	elif _music.stream:
		_music.play()
