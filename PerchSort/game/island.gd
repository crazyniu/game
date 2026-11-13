class_name Island
extends RefCounted
## Shared builders for the island-style screens: the glossy kit pieces (art/island), painted digits and badges.
## Every screen uses these so one role always has one size and one look. Rects are design pixels (1080 x 1920).

const DIGIT := "icon_home_island_digit_"
const BADGE_CAP := 99                     # owned counts and notification numbers never show more than 99
const BADGE_SIZE := 66.0                  # one diameter for every round badge, in every state
const PRESSED := Color(0.82, 0.82, 0.82)


## A texture stretched with fixed corners (nine-patch). `margin` is in texture pixels.
static func nine(parent: Node, name: String, r: Rect2, margin: Vector4) -> NinePatchRect:
	var n := NinePatchRect.new()
	n.name = name
	n.texture = Art.tex(name)
	n.patch_margin_left = int(margin.x)
	n.patch_margin_top = int(margin.y)
	n.patch_margin_right = int(margin.z)
	n.patch_margin_bottom = int(margin.w)
	n.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(n, r)
	parent.add_child(n)
	return n

## The blank rope sign, hung from the top edge: its ropes are lengthened up to the screen top and the plank's centre
## lands at `plank_center_y`. Returns the rectangle where a painted word fits on the plank.
const SIGN_PLANK_CENTER := 0.68                    # plank centre as a share of the sign picture's height
const SIGN_WORD := Rect2(0.27, 0.50, 0.46, 0.38)   # word area on the plank, shares of the picture
const SIGN_ROPES := [302.0, 1386.0]                # left x of each rope, in sign-picture pixels

static func hanging_sign(parent: Node, center_x: float, plank_center_y: float, width: float) -> Rect2:
	var tex := Art.tex("island_sign_blank")
	var k := width / tex.get_width()
	var h := tex.get_height() * k
	var top := plank_center_y - SIGN_PLANK_CENTER * h
	var left := center_x - width * 0.5
	var holder := Control.new()
	holder.name = "Sign"
	holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	parent.add_child(holder)
	if top > 0.0:   # a plain slice of the rope, stretched up to the screen edge
		for x0 in SIGN_ROPES:
			var slice := AtlasTexture.new()
			slice.atlas = tex
			slice.region = Rect2(x0, 4.0, 48.0, 26.0)
			var rope := TextureRect.new()
			rope.texture = slice
			rope.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
			rope.stretch_mode = TextureRect.STRETCH_SCALE
			rope.mouse_filter = Control.MOUSE_FILTER_IGNORE
			UI.place(rope, Rect2(left + x0 * k, 0.0, 48.0 * k, top + 30.0 * k))
			holder.add_child(rope)
	UI.image(holder, "island_sign_blank", Rect2(Vector2(left, top), Vector2(width, h)), false)
	return Rect2(Vector2(left + width * SIGN_WORD.position.x, top + h * SIGN_WORD.position.y), Vector2(width, h) * SIGN_WORD.size)

## A piece drawn whole at the given width, height following its own ratio. Returns the sprite.
static func piece(parent: Node, name: String, top_left: Vector2, width: float) -> TextureRect:
	var t := Art.tex(name)
	var h := width * t.get_height() / t.get_width()
	var s := UI.image(parent, name, Rect2(top_left, Vector2(width, h)), false)
	return s

## A piece drawn whole, centred on `center`, scaled to fit `box` while keeping its ratio.
static func piece_fit(parent: Node, name: String, center: Vector2, box: Vector2) -> TextureRect:
	var t := Art.tex(name)
	var k := minf(box.x / t.get_width(), box.y / t.get_height())
	var size := t.get_size() * k
	return UI.image(parent, name, Rect2(center - size * 0.5, size), false)

## Painted digits, plain (no separators), fitted into `bounds` and centred on their visible ink.
static func digits(parent: Node, value: String, bounds: Rect2, tint := Color.WHITE) -> Control:
	var box := Control.new()
	box.name = "Digits"
	box.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(box, bounds)
	parent.add_child(box)
	var cell := Art.tex(DIGIT + "0").get_size()
	var fit := minf(bounds.size.x / (cell.x * value.length()), bounds.size.y / cell.y)
	var glyph := cell * fit
	var origin := (bounds.size - Vector2(glyph.x * value.length(), glyph.y)) * 0.5
	var ink := UI.used_rect(DIGIT + value[0])
	for d in value:
		ink = ink.merge(UI.used_rect(DIGIT + d))
	origin.y = bounds.size.y * 0.5 - ink.get_center().y * fit
	for i in value.length():
		var g := UI.image(box, DIGIT + value[i], Rect2(origin + Vector2(glyph.x * i, 0), glyph), false)
		g.self_modulate = tint
	return box

## Round count (green) or notification (red) badge with a painted number; hidden at zero.
## The number never exceeds BADGE_CAP. Returns null when there is nothing to show.
static func badge(parent: Node, center: Vector2, count: int, red := false) -> Control:
	if count <= 0:
		return null
	var holder := Control.new()
	holder.name = "BadgeRed" if red else "BadgeGreen"
	holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(holder, Rect2(center - Vector2.ONE * BADGE_SIZE * 0.5, Vector2.ONE * BADGE_SIZE))
	parent.add_child(holder)
	UI.image(holder, "island_badge_red" if red else "island_badge_green", Rect2(Vector2.ZERO, Vector2.ONE * BADGE_SIZE), false)
	var text := str(mini(count, BADGE_CAP))
	var inner := BADGE_SIZE * (0.66 if text.length() == 1 else 0.76)
	digits(holder, text, Rect2(Vector2.ONE * (BADGE_SIZE - inner) * 0.5 + Vector2(0, 1), Vector2(inner, inner * 0.9)))
	return holder

## The tick badge for an equipped or finished item.
static func check_badge(parent: Node, center: Vector2) -> TextureRect:
	return UI.image(parent, "island_badge_check", Rect2(center - Vector2.ONE * BADGE_SIZE * 0.5, Vector2.ONE * BADGE_SIZE), false)

## Feather prices use cream-white painted glyphs; locked faces retain their dimensions.
const PRICE_INK := Color.WHITE

static func price_pill(parent: Node, r: Rect2, price: int, locked := false) -> Control:
	var holder := _coin_bar(parent, r, "Price")
	if locked:
		holder.get_child(0).modulate = Color(0.72, 0.7, 0.68)
	var shown := holder.size
	digits(holder, str(clampi(price, 0, 99999)), Rect2(shown.x * 0.38, shown.y * 0.22, shown.x * 0.50, shown.y * 0.56), PRICE_INK)
	return holder

## A gained amount ("+30") on the same gold coin bar; the number box is the child named "Number".
static func reward_bar(parent: Node, r: Rect2, text: String) -> Control:
	var holder := _coin_bar(parent, r, "Reward")
	var shown := holder.size
	var cap := shown.y * 0.46
	var w := minf(shown.x * 0.56, UI.number_width(text, cap))
	var box := UI.number(holder, text, Rect2(shown.x * 0.36 + (shown.x * 0.56 - w) * 0.5, (shown.y - cap) * 0.5 - 2.0, w, cap), PRICE_INK)
	box.name = "Number"
	return holder

static func _coin_bar(parent: Node, r: Rect2, n: String) -> Control:
	var tex := Art.tex("island_price_gold")
	var k := minf(r.size.x / tex.get_width(), r.size.y / tex.get_height())
	var shown := tex.get_size() * k
	var holder := Control.new()
	holder.name = n
	holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(holder, Rect2(r.get_center() - shown * 0.5, shown))
	parent.add_child(holder)
	UI.image(holder, "island_price_gold", Rect2(Vector2.ZERO, shown), false)
	return holder

## Green "+N" badge (a gained count on a prize picture).
static func plus_badge(parent: Node, center: Vector2, count: int) -> Control:
	var s := BADGE_SIZE * 1.15
	var holder := Control.new()
	holder.name = "PlusBadge"
	holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(holder, Rect2(center - Vector2.ONE * s * 0.5, Vector2.ONE * s))
	parent.add_child(holder)
	UI.image(holder, "island_badge_green", Rect2(Vector2.ZERO, Vector2.ONE * s), false)
	UI.number(holder, "+%d" % count, Rect2(s * 0.14, s * 0.3, s * 0.72, s * 0.38), Color.WHITE)
	return holder

## Star progress bar: a dark track, a green fill for `fill` (0..1), the gold star over its left end and the count.
static func progress_bar(parent: Node, r: Rect2, fill: float, text: String) -> Control:
	var holder := Control.new()
	holder.name = "Progress"
	holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(holder, r)
	parent.add_child(holder)
	var star := r.size.y * 1.25
	var track := Rect2(star * 0.45, r.size.y * 0.12, r.size.x - star * 0.45, r.size.y * 0.76)
	nine(holder, "island_progress_track", track, Vector4(44, 40, 44, 40))
	if fill > 0.0:
		var inner := track.grow(-track.size.y * 0.18)
		var clip := Control.new()
		clip.clip_contents = true
		clip.mouse_filter = Control.MOUSE_FILTER_IGNORE
		UI.place(clip, Rect2(inner.position, Vector2(inner.size.x * fill, inner.size.y)))
		holder.add_child(clip)
		# Use only green paint: the source button's rim must not become a second track frame.
		var paint := AtlasTexture.new()
		paint.atlas = Art.tex("island_pill_green")
		paint.region = Rect2(26, 24, 362, 70)
		var face := UI.image(clip, "island_pill_green", Rect2(Vector2.ZERO, inner.size), false)
		face.texture = paint
	# The combined star-and-bar picture leaves a rectangular gold stub when cropped.
	UI.art(holder, "island_star_full", Rect2(-star * 0.1, (r.size.y - star) * 0.5, star, star))
	var cap := r.size.y * 0.78
	ButtonKit.number(holder, text, Rect2(track.position.x + star * 0.4, (r.size.y - cap) * 0.5, track.size.x - star * 0.6, cap)).name = "Number"
	return holder

## A painted-picture button: the whole picture is the face and a press tints it. Sizes never change.
## A worded button baked into one complete picture per language (tools/island/bake_buttons.py), so its word is part of
## the art and always sits on the middle of the face. `picture` is the name without the language suffix.
static func word_button(parent: Node, n: String, picture: String, r: Rect2, on_press: Callable) -> Button:
	return UI.art_button(parent, n, "%s_%s" % [picture, UiText.locale()], r, on_press)

## The same baked picture as a plain state marker (not a button).
static func word_picture(parent: Node, picture: String, r: Rect2) -> TextureRect:
	return UI.image(parent, "%s_%s" % [picture, UiText.locale()], r, false)

static func button(parent: Node, n: String, picture: String, r: Rect2, on_press: Callable) -> Button:
	return UI.art_button(parent, n, picture, r, on_press)
