extends Node
## Autoload "Sound": background music per page and one-shot effects, following the mute setting.

var muted := false
var _music: AudioStreamPlayer
var _music_name := ""
var _effects: Array[AudioStreamPlayer] = []
var _cache := {}


func _ready() -> void:
	_music = AudioStreamPlayer.new()
	_music.volume_db = -8.0
	add_child(_music)
	_music.finished.connect(func(): if not muted and _music.stream: _music.play())
	for i in 8:
		var p := AudioStreamPlayer.new()
		add_child(p)
		_effects.append(p)

func _stream(name: String) -> AudioStream:
	if not _cache.has(name):
		var path := "res://audio/%s.wav" % name
		if not ResourceLoader.exists(path):
			path = "res://audio/Audio/%s.wav" % name
		_cache[name] = load(path) if ResourceLoader.exists(path) else null
	return _cache[name]

## "bgm_game" on the board, "bgm_menu" elsewhere.
func music(name: String) -> void:
	if name == _music_name and (_music.playing or muted):
		return
	_music_name = name
	_music.stream = _stream(name)
	if not muted and _music.stream:
		_music.play()

func play(name: String, pitch := 1.0) -> void:
	if muted:
		return
	var stream := _stream(name)
	if stream == null:
		return
	for p in _effects:
		if not p.playing:
			p.stream = stream
			p.pitch_scale = pitch
			p.play()
			return

func set_muted(value: bool) -> void:
	muted = value
	if muted:
		_music.stop()
	elif _music.stream:
		_music.play()
