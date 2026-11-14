extends RefCounted
## A blocked save never enters Home or starts rewards. Reuses the existing dialog kit.
const TITLE_INK := {"zh": Rect2(264, 67, 1248, 346), "en": Rect2(121, 559, 1537, 269)}

const INK := {
	"en": [Rect2(56, 122, 911, 129), Rect2(52, 500, 924, 413), Rect2(324, 1127, 377, 158), Rect2(365, 1353, 292, 137)],
	"zh": [Rect2(83, 109, 859, 128), Rect2(47, 445, 930, 518), Rect2(382, 1128, 264, 129), Rect2(381, 1365, 263, 130)],
}

static func word(parent: Node, group: int, bounds: Rect2) -> void:
	var locale := UiText.locale()
	var region: Rect2 = TITLE_INK[locale] if group == 0 else INK[locale][group]
	var cut := AtlasTexture.new()
	cut.atlas = Art.tex("label_save_error" if group == 0 else "label_save_recovery_" + locale)
	cut.region = region
	var label := TextureRect.new()
	label.name = "PaintedWord"
	label.texture = cut
	label.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var factor := minf(bounds.size.x / region.size.x, bounds.size.y / region.size.y)
	label.size = region.size * factor
	label.position = bounds.get_center() - label.size * 0.5
	parent.add_child(label)

static func open(main: Control) -> Modal:
	var dialog := JungleDialog.open(main.popups, "", "", [400], main.instant(), 150, false)
	dialog.name = "SaveRecovery"
	var background := UI.image(dialog, DialogCartoonSkin.BACKGROUND_ART, Rect2(0, 0, 1080, 1920))
	background.name = "ForestBackground"
	background.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_COVERED
	dialog.move_child(background, 0)
	var ys: Array = dialog.get_meta("ys")
	var sign: Control = dialog._title_layer
	word(sign, 0, Rect2(sign.size * ButtonKit.SIGN_FACE.position, sign.size * ButtonKit.SIGN_FACE.size))
	word(dialog.content, 1, Rect2(150, ys[0] + 30, 780, 340))
	var y: float = dialog.get_meta("actions_y")
	var retry := ButtonKit.button(dialog.content, "Retry", Rect2(96, y, 420, 150), "green",
		func(): main.get_tree().reload_current_scene())
	var leave := ButtonKit.button(dialog.content, "Exit", Rect2(564, y, 420, 150), "green",
		func(): main.get_tree().quit())
	word(retry.get_node("Content"), 2, Rect2(65, 30, 290, 90))
	word(leave.get_node("Content"), 3, Rect2(65, 30, 290, 90))
	Sound.music("bgm_menu")
	return dialog

static func preview(main: Control, _state: String) -> void:
	open(main)
