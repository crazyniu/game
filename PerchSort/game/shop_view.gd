extends Control
## Shop: header (close, feathers, title), one content area that fits each tab on a single screen, and a tab bar docked
## at the bottom. Boosters are a list of rows (picture, name and use, price); outfits are a 3 x 3 grid of bird kinds
## (a met kind opens that bird's outfit page, the aviary's detail page, above the shop); nests are a 3 x 3 grid of
## houses. Words are baked labels (tools/island/bake_labels.py), numbers the shared painted glyphs.

signal closed
signal profile_changed
signal booster_purchased(kind: int, count: int)

const AviaryScript := preload("res://game/aviary_view.gd")
const TABS := [Shop.BOOSTER, Shop.OUTFIT, Shop.HOUSE]
const TAB_NAMES := ["Boosters", "Outfits", "Nests"]
const TAB_PICTURE_NAMES := ["boosters", "outfits", "nests"]   # island_tab_<name> pictures and tab_<name> words
## Layout (1080 x 1920). Header (close, feathers, title), one content area that fits every tab without scrolling, and
## a tab bar docked at the bottom. Boosters are a list of rows; outfits and nests are a 3 x 3 grid of tiles.
const CLOSE := Rect2(36, 56, 112, 112)
const TITLE_Y := 250.0
const SCREEN_TITLE_CAP := 100.0
const AREA := Rect2(32, 330, 1016, 1300)          # the light veil behind the content
const AREA_PAD := 24.0
const ROW_GAP := 16.0
const TILE_GAP := 20.0
const TAB_BAR := Rect2(32, 1662, 1016, 210)
const TAB_ICON := 104.0
const TAB_WORD_CAP := 42.0
const TAB_PILL := Vector2(214, 82)
const TILE_SLOT := Vector2(240, 78)               # price, Equip or the state word on a tile
const TILE_NAME_CAP := 50.0
const ROW_SLOT := Vector2(250, 82)
const ROW_NAME_CAP := 58.0
const ROW_CAPTION_CAP := 36.0
const BOOSTER_LABEL := ["firefly", "sapling", "owl_eye", "morning_bell", "scarecrow", "spring_rain"]
const BOOSTER_ILLUS := ["firefly", "sprout", "owl_eye", "bell", "scarecrow", "rain"]   # island_icon_<name>
const BOOSTER_WORD := ["firefly", "sprout", "owl_eye", "wake_bell", "scarecrow", "spring_rain"]   # painted item_/cap_ words

## Layout (design pixels, 1080x1920; margins 48, 8 px grid).
const MARGIN := 32.0
const POPUP_CAPTION := 80.0               # booster buy popup: the same caption under the title
const POPUP_CAPTION_CAP := 56.0
const PILL := Vector2(300, 98)             # price, equip button and equipped marker share this slot
const PILL_EDGE := 8.0
const PRICE_COIN := 64.0
const PRICE_CAP := 44.0
const POPUP_PIC := 360.0
const SINGLE_PIC := 420.0
const BUY_SIZE := Vector2(560, 150)
const QUANTITY_WIDTH := 180.0
const STEP := Vector2(150, 112)            # - and + are short pills (no round buttons in popups)
const QUANTITY_GAP := 20.0
const COIN_W := 430.0   # the feather bar is the homepage counter at its homepage size

var profile: Profile
var tab := 0
var page := 0
var instant := false   # captures skip the pop-in motion
var _bar: Control
var _tabs: Control
var _grid: Control
var _scroll: ScrollContainer
var _coins: FeatherBar
var _coins_shown := -1
var _modal: Modal
var _purchase_item: Dictionary
var _quantity := 1
var _quantity_limit := 1
var _quantity_input: QuantityInput
const SIGN_BAR := Vector2(48, 14)
var _minus: Button
var _plus: Button
var _purchase_button: Button
var _purchase_contents: Control
var _outfit_page: Control   # a bird's outfit page opened from the outfits tab
var _drag_travel := 0.0
const TAP_SLOP := 14.0


## LineEdit handles keyboard input; only the existing painted digits are visible.
class QuantityInput extends LineEdit:
	signal value_changed(value: int)

	const CAP := 60.0
	var limit := 1
	var _digits: Control
	var _caret: ColorRect
	var _selection: ColorRect

	func _ready() -> void:
		alignment = HORIZONTAL_ALIGNMENT_CENTER
		virtual_keyboard_type = LineEdit.KEYBOARD_TYPE_NUMBER
		select_all_on_focus = true
		context_menu_enabled = false
		drag_and_drop_selection_enabled = false
		max_length = str(limit).length()
		text = "1"
		for key in ["font_color", "font_selected_color", "font_uneditable_color", "font_outline_color", "caret_color", "selection_color"]:
			add_theme_color_override(key, Color.TRANSPARENT)
		add_theme_font_size_override("font_size", 60)
		add_theme_constant_override("minimum_character_width", 1)
		var frame := UI._box(UI.FILL["cream"], UI.RIM["cream"], 32.0, 0.0, false)
		frame.set_border_width_all(3)
		add_theme_stylebox_override("normal", frame)
		add_theme_stylebox_override("read_only", frame)
		var focus := frame.duplicate() as StyleBoxFlat
		focus.bg_color = Color.TRANSPARENT
		focus.border_color = UI.INK
		add_theme_stylebox_override("focus", focus)
		_selection = ColorRect.new()
		_selection.color = UI.RIM["cream"]
		_selection.mouse_filter = Control.MOUSE_FILTER_IGNORE
		_selection.hide()
		add_child(_selection)
		_digits = UI.number(self, text, Rect2(16, (size.y - CAP) * 0.5, size.x - 32, CAP))
		_caret = ColorRect.new()
		_caret.color = UI.INK
		_caret.size = Vector2(3, 52)
		_caret.mouse_filter = Control.MOUSE_FILTER_IGNORE
		_caret.hide()
		add_child(_caret)
		text_changed.connect(_edited)
		text_submitted.connect(func(_text: String): release_focus())
		focus_entered.connect(func(): set_process(true))
		focus_exited.connect(func():
			if text.is_empty():
				set_value(1)
			_caret.hide()
			_selection.hide()
			set_process(false))
		set_process(false)

	func set_value(value: int) -> void:
		text = str(clampi(value, 1, limit))
		caret_column = text.length()
		_refresh_digits()
		value_changed.emit(int(text))

	func _edited(value: String) -> void:
		var digits := ""
		for character in value:
			if character >= "0" and character <= "9":
				digits += character
		var column := caret_column
		var cleaned := "" if digits.is_empty() else str(clampi(int(digits), 1, limit))
		if text != cleaned:
			text = cleaned
			caret_column = mini(column, text.length())
		_refresh_digits()
		value_changed.emit(0 if text.is_empty() else int(text))

	func _refresh_digits() -> void:
		_digits.visible = not text.is_empty()
		if _digits.visible:
			UI.set_number(_digits, text)

	func _process(_delta: float) -> void:
		var width := UI.number_width(text, CAP)
		var cap := CAP if width == 0.0 else CAP * minf(1.0, _digits.size.x / width)
		var left := (size.x - UI.number_width(text, cap)) * 0.5
		_caret.visible = is_editing() and not has_selection()
		_caret.position = Vector2(left + UI.number_width(text.left(caret_column), cap), 34)
		_selection.visible = is_editing() and has_selection()
		if _selection.visible:
			var start := UI.number_width(text.left(get_selection_from_column()), cap)
			var end := UI.number_width(text.left(get_selection_to_column()), cap)
			_selection.position = Vector2(left + start, 32)
			_selection.size = Vector2(end - start, 56)


func setup(p: Profile, start_tab := 0) -> void:
	profile = p
	tab = start_tab
	size = Vector2(1080, 1920)
	UI.image(self, "island_map_backdrop", Rect2(0, 0, 1080, 1920), false)
	_scroll = ScrollContainer.new()
	_scroll.name = "Scroll"
	_scroll.horizontal_scroll_mode = ScrollContainer.SCROLL_MODE_DISABLED
	_scroll.vertical_scroll_mode = ScrollContainer.SCROLL_MODE_SHOW_NEVER
	_scroll.scroll_deadzone = 12
	UI.place(_scroll, AREA.grow(-AREA_PAD))
	add_child(_scroll)
	_scroll.gui_input.connect(_on_scroll_input)
	_grid = Control.new()
	_grid.name = "Grid"
	_grid.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_scroll.add_child(_grid)
	_tabs = _layer("Tabs")
	_bar = _layer("Bar")
	_build_header()
	_refresh()

func _notification(what: int) -> void:
	if what == NOTIFICATION_TRANSLATION_CHANGED and is_node_ready():
		_build_header()
		_refresh()

func _build_header() -> void:
	for child in _bar.get_children():
		child.queue_free()
	_coins = null
	_coins_shown = -1
	Island.button(_bar, "Close", "island_close_tile", CLOSE, func(): closed.emit())
	Simple.Words.new().place(_bar, "shop", "title", Vector2(540, TITLE_Y), SCREEN_TITLE_CAP, 0)

func _layer(n: String) -> Control:
	var c := Control.new()
	c.name = n
	c.size = size
	c.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(c)
	return c

func _refresh() -> void:
	_build_coins()
	for node in [_tabs, _grid]:
		for c in node.get_children():
			node.remove_child(c)   # rebuilt cards keep their names
			c.queue_free()
	_tab_bar()
	var inner := AREA.grow(-AREA_PAD)
	if TABS[tab] == Shop.BOOSTER:
		var items := _items(Shop.BOOSTER)
		var row_h := (inner.size.y - ROW_GAP * (items.size() - 1)) / items.size()
		for n in items.size():
			_row(items[n], Rect2(0, n * (row_h + ROW_GAP), inner.size.x, row_h), n)
	else:
		var tiles: Array = outfit_species() if TABS[tab] == Shop.OUTFIT else _items(Shop.HOUSE)
		var w := (inner.size.x - TILE_GAP * 2.0) / 3.0
		var rows := ceili(tiles.size() / 3.0)
		var h := (inner.size.y - TILE_GAP * (rows - 1)) / maxf(3.0, rows)
		for n in tiles.size():
			var r := Rect2((n % 3) * (w + TILE_GAP), (n / 3) * (h + TILE_GAP), w, h)
			if TABS[tab] == Shop.OUTFIT:
				_kind_tile(tiles[n], r, n)
			else:
				_card(tiles[n], r, n)
	_grid.custom_minimum_size = Vector2(inner.size.x, inner.size.y)
	_scroll.scroll_vertical = 0

## Every new press starts a fresh tap-or-drag decision.
func _input(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.pressed:
		_drag_travel = 0.0

## Mouse drag scrolls the card grid (touch screens drag through ScrollContainer itself). Motion over a card reaches
## here because the card buttons pass what they do not use.
func _on_scroll_input(event: InputEvent) -> void:
	if DisplayServer.is_touchscreen_available():
		return
	if event is InputEventMouseMotion and event.button_mask & MOUSE_BUTTON_MASK_LEFT:
		_drag_travel += absf(event.relative.y)
		if _drag_travel >= TAP_SLOP:
			_scroll.scroll_vertical -= roundi(event.relative.y)

## The feather bar stays at the right margin: the homepage counter, plain digits, never past 99999.
func _build_coins() -> void:
	if _coins == null:
		_coins = FeatherBar.create(_bar, Vector2(1080.0 - MARGIN - COIN_W, 60.0), COIN_W, profile.coins)
		_coins_shown = profile.coins
	elif _coins_shown != profile.coins:
		_coins.set_amount(profile.coins, not instant)
		_coins_shown = profile.coins

## The tab bar docked at the bottom: one glossy cream bar, three cells (picture over word). The current tab's picture
## sits a little higher and its word is white on a green pill; every cell keeps one size.
func _tab_bar() -> void:
	_glass(_tabs, TAB_BAR, 44.0, false, Color(1, 1, 1, 0.96)).name = "TabBar"   # nearly solid: the flowers stay behind it
	var words := Simple.Words.new()
	var cell := TAB_BAR.size.x / TABS.size()
	for i in TABS.size():
		var on := i == tab
		var cx := TAB_BAR.position.x + cell * (i + 0.5)
		var word_y := TAB_BAR.end.y - 52.0
		var icon_y := TAB_BAR.position.y + 70.0 - (10.0 if on else 0.0)
		var size := TAB_ICON * (1.12 if on else 1.0)
		_fit_visible(_tabs, "island_tab_" + TAB_PICTURE_NAMES[i], Vector2(cx, icon_y), size)
		if on:
			Island.piece_fit(_tabs, "island_pill_glossy", Vector2(cx, word_y), TAB_PILL)
		words.place(_tabs, "tab_" + TAB_PICTURE_NAMES[i], "white" if on else "ink", Vector2(cx, word_y - 3.0), TAB_WORD_CAP, 0)
		var hit := UI.hit_area(_tabs, Rect2(cx - cell * 0.5, TAB_BAR.position.y, cell, TAB_BAR.size.y), func():
			if i != tab:
				Sound.play("ui_tap")
			_select_tab(i))
		hit.name = TAB_NAMES[i]

func _select_tab(i: int) -> void:
	if i != tab:
		tab = i
		page = 0
		_refresh()

## Single boosters, or the houses.
func _items(kind: int) -> Array:
	var out := []
	for it in Shop.catalog():
		if it["kind"] == kind and (kind != Shop.BOOSTER or it["quantity"] == 1):
			out.append(it)
	return out

# ---------------------------------------------------------------- cards

func _owned(it: Dictionary) -> bool:
	return it["kind"] != Shop.BOOSTER and (profile.owns_outfit(it["id"]) if it["kind"] == Shop.OUTFIT else profile.owns_house(it["style"]))

func _equipped(it: Dictionary) -> bool:
	if it["kind"] == Shop.OUTFIT:
		return profile.equipped.get(it["species"], "") == it["id"]
	return it["kind"] == Shop.HOUSE and profile.equipped_house == it["style"]

func _locked(it: Dictionary) -> bool:
	return it["kind"] == Shop.BOOSTER and not Progress.is_booster_unlocked(profile, it["booster"])

func _name_id(it: Dictionary) -> String:
	if it["kind"] == Shop.BOOSTER:
		return "booster.%s.name" % BOOSTER_LABEL[it["booster"]]
	return str(it["id"]) + ".name"

## The baked label of an item's name (tools/island/bake_labels.py).
func _label_id(it: Dictionary) -> String:
	if it["kind"] == Shop.BOOSTER:
		return "item_" + BOOSTER_WORD[it["booster"]]
	return "house_" + str(it["style"])

## A nest tile: picture, name, then its price, Equip, or the quiet "Equipped" state (with the green outline and check).
func _card(it: Dictionary, r: Rect2, n: int) -> void:
	var card := _item_holder(it, r)
	var cs := r.size
	_flat_card(card, Rect2(Vector2.ZERO, cs), _equipped(it))
	var body: Control = card.get_node("Body")
	card.move_child(body, -1)
	var words := Simple.Words.new()
	var slot := Rect2(Vector2((cs.x - TILE_SLOT.x) * 0.5, cs.y - 26.0 - TILE_SLOT.y), TILE_SLOT)
	var name_y := slot.position.y - 40.0
	_picture(body, it, Rect2(22, 18, cs.x - 44.0, name_y - 52.0))
	words.place(body, _label_id(it), "ink", Vector2(cs.x * 0.5, name_y), TILE_NAME_CAP, 0)
	if _equipped(it):
		words.place(body, "equipped", "soft", slot.get_center() + Vector2(0, -2), 38.0, 0)   # a state, not a button
		Island.check_badge(body, Vector2(cs.x - 36.0, 36.0))
	elif _owned(it):
		Island.word_picture(body, "btn_equip", slot).name = "Pill"
	else:
		Island.price_pill(body, slot, it["price"], _locked(it))
	_pop_in(card, n)

## A booster row: picture with its owned count, name over its one-line use, and the price on the right.
func _row(it: Dictionary, r: Rect2, n: int) -> void:
	var card := _item_holder(it, r)
	var body: Control = card.get_node("Body")
	_glass(card, Rect2(Vector2.ZERO, r.size), 34.0)
	card.move_child(body, -1)
	var words := Simple.Words.new()
	var mid := r.size.y * 0.5
	var pic := r.size.y - 36.0
	_picture(body, it, Rect2(28, mid - pic * 0.5, pic, pic))
	var text_x := 28.0 + pic + 28.0
	words.place(body, _label_id(it), "ink", Vector2(text_x, mid - 24.0), ROW_NAME_CAP, -1)
	words.place(body, "cap_" + BOOSTER_WORD[it["booster"]], "soft", Vector2(text_x, mid + 34.0), ROW_CAPTION_CAP, -1)
	var slot := Rect2(Vector2(r.size.x - 28.0 - ROW_SLOT.x, mid - ROW_SLOT.y * 0.5), ROW_SLOT)
	Island.price_pill(body, slot, it["price"], _locked(it))
	if _locked(it):
		body.modulate.a = UI.DISABLED_ALPHA
		Island.piece_fit(card, "island_padlock", Vector2(28.0 + pic - 6.0, mid - pic * 0.5 + 18.0), Vector2(56, 56)).name = "Lock"
	else:
		var stock := Island.badge(card, Vector2(28.0 + pic - 6.0, mid - pic * 0.5 + 16.0), profile.inventory[it["booster"]])
		if stock != null:
			stock.name = "Stock"
	_pop_in(card, n)

## A picture whose visible pixels (not its padded canvas) fill a `side` x `side` box centred on `center`.
func _fit_visible(parent: Node, picture: String, center: Vector2, side: float) -> TextureRect:
	var used := UI.used_rect(picture)
	var tex := Art.tex(picture)
	var k := side / maxf(used.size.x, used.size.y)
	var offset := (used.get_center() - tex.get_size() * 0.5) * k
	return UI.image(parent, picture, Rect2(center - offset - tex.get_size() * k * 0.5, tex.get_size() * k), false)

## The holder of one item: its tap area (passing drags on) and the Body that dims when the item is locked.
func _item_holder(it: Dictionary, r: Rect2) -> Control:
	var card := Control.new()
	card.name = it["id"]
	card.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(card, r)
	_grid.add_child(card)
	var hit := UI.hit_area(card, Rect2(Vector2.ZERO, r.size), func(): _tap(it))
	hit.mouse_filter = Control.MOUSE_FILTER_PASS
	hit.disabled = _locked(it)
	var body := Control.new()
	body.name = "Body"
	body.size = r.size
	body.mouse_filter = Control.MOUSE_FILTER_IGNORE
	card.add_child(body)
	return card

## A soft cream card; the chosen one carries the green outline.
func _flat_card(parent: Node, r: Rect2, selected := false) -> Panel:
	return _glass(parent, r, 34.0, selected)

## Frosted white glass over the sea: the one card surface of the shop (rows, tiles, tab bar). The chosen card has a
## green rim instead of the white one.
const GLASS := Color(1.0, 1.0, 1.0, 0.86)
const GLASS_RIM := Color(1.0, 1.0, 1.0, 1.0)

func _glass(parent: Node, r: Rect2, radius: float, selected := false, fill := GLASS) -> Panel:
	var style := Simple._box(fill, radius, Color.TRANSPARENT, 0.0, 6.0 if selected else 4.0,
		Simple.GREEN if selected else GLASS_RIM, 14.0)
	return Simple._panel(parent, r, style, "Shape")

func _pop_in(card: Control, n: int) -> void:
	if instant:
		return
	card.modulate.a = 0.0
	card.scale = Vector2.ONE * 0.94
	var tw := card.create_tween().set_parallel()
	tw.tween_property(card, "modulate:a", 1.0, 0.18).set_delay(n * 0.04)
	tw.tween_property(card, "scale", Vector2.ONE, 0.26).set_delay(n * 0.04).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)

func _caption_id(it: Dictionary) -> String:
	return "booster.%s.description" % BOOSTER_LABEL[it["booster"]]

## Item picture: booster icon or house illustration (outfits are shown on the bird's own page).
func _picture(parent: Node, it: Dictionary, box: Rect2) -> void:
	if it["kind"] == Shop.BOOSTER:
		var side := minf(box.size.x, box.size.y)
		UI.art(parent, "island_icon_%s" % BOOSTER_ILLUS[it["booster"]], Rect2(box.get_center() - Vector2.ONE * side * 0.5, Vector2.ONE * side))
	else:
		UI.art(parent, "house_style_%s_body" % it["style"], box.grow(18.0))

## Pill-shaped button in a card slot; a press darkens its face (UI._skin_button).
func _pill_button(parent: Node, n: String, r: Rect2, kind: String, on_press: Callable) -> Button:
	var b := Button.new()
	b.name = n
	b.focus_mode = Control.FOCUS_NONE
	# Painted kit faces: square buttons are discs (cream = the white disc with the leaf ring), others pills.
	var square := absf(r.size.x - r.size.y) < 2.0
	var piece := ("disc_cream" if kind == "cream" else "disc_" + kind) if square \
		else ("capsule_plain" if kind == "cream" else "pill_" + kind)
	UI._skin_button(b, piece, Rect2(Vector2.ZERO, r.size))
	UI.place(b, r)
	b.pressed.connect(func():
		Sound.play("ui_tap")
		on_press.call())
	parent.add_child(b)
	return b

## Coin and white price, centred together inside the orange pill.
func _price_parts(pill: Control, price: int, width := PILL.x) -> void:
	var text := str(price)
	var num_w := UI.number_width(text, PRICE_CAP)
	var slot_w := maxf(num_w, UI.number_width("000", PRICE_CAP))   # coin and digits keep one place whatever the price
	var x := (width - (PRICE_COIN + 12.0 + slot_w)) * 0.5
	var mid := (PILL.y - PILL_EDGE) * 0.5
	UI.art(pill, "coin", Rect2(x, mid - PRICE_COIN * 0.5, PRICE_COIN, PRICE_COIN))
	UI.number(pill, text, Rect2(x + PRICE_COIN + 12.0 + slot_w - num_w, mid - PRICE_CAP * 0.5, num_w + 2.0, PRICE_CAP), UI.INK, -1)

## Live BirdView wearing `outfit`, its front frame fitted in the box with the feet on the box bottom.
func _bird(parent: Node, species: String, outfit: String, box: Rect2, phase: float, fill := 0.88) -> BirdView:
	var front := outfit + "_front" if Art.has(outfit + "_front") else species + "_front"
	var used := UI.used_rect(front)
	var k := minf(box.size.x / used.size.x, box.size.y / used.size.y) * fill   # 0.88 leaves headroom for crests, horns and cheers
	var bird := BirdView.new()
	parent.add_child(bird)
	bird.position = Vector2(box.get_center().x - (used.get_center().x - BirdView.CANVAS * 0.5) * k,
		box.end.y - (used.end.y - BirdView.FEET_Y) * k)
	bird.setup(Board.bird(0, Board.species_index(species), 0), false, k * BirdView.CANVAS / BirdView.DISPLAY, outfit)
	bird.quiet = instant
	if not instant:
		# Painted cheer poses accompany BirdView's continuous breathing and blinks.
		var tw := bird.create_tween().set_loops()
		tw.tween_interval(1.4 + phase)
		tw.tween_callback(bird.cheer)
		tw.tween_interval(3.0)
	return bird


# ---------------------------------------------------------------- outfits tab: bird kinds

## Species that wear outfits, in aviary order (the crow has none).
static func outfit_species() -> Array:
	var out := []
	for card in Aviary.SPECIES:
		if card[0] != "crow":
			out.append(card[0])
	return out

## Owned outfits of a species, counting the plain look that every bird has.
func owned_outfits(species: String) -> int:
	var owned := 1
	for index in range(1, Shop.OUTFIT_COUNT + 1):
		if profile.owns_outfit("outfit.%s.%d" % [species, index]):
			owned += 1
	return owned

## An outfit tile: the bird in its current outfit, its name and how many looks it owns; a tap opens its outfit page.
## An unmet kind is the "?" silhouette and does nothing.
func _kind_tile(species: String, r: Rect2, n: int) -> void:
	var met := profile.has_met(species)
	var tile := Control.new()
	tile.name = "Kind_" + species
	tile.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(tile, r)
	_grid.add_child(tile)
	_flat_card(tile, Rect2(Vector2.ZERO, r.size))
	var slot := Rect2(Vector2((r.size.x - TILE_SLOT.x) * 0.5, r.size.y - 26.0 - TILE_SLOT.y), TILE_SLOT)
	var name_y := slot.position.y - 40.0
	_kind_face(tile, species, Rect2(30, 22, r.size.x - 60.0, name_y - 60.0), n, 1.0)
	if met:
		var words := Simple.Words.new()
		words.place(tile, "bird_" + species, "ink", Vector2(r.size.x * 0.5, name_y), TILE_NAME_CAP, 0)
		var text := "%d/%d" % [owned_outfits(species), Shop.OUTFIT_COUNT + 1]
		var cap := 40.0
		var nw := UI.number_width(text, cap)
		var x0 := r.size.x * 0.5 - (40.0 + 10.0 + nw) * 0.5
		var cy := slot.get_center().y
		UI.icon(tile, "hanger", Vector2(x0 + 20.0, cy), 40.0, Simple.SOFT_INK)
		UI.number(tile, text, Rect2(x0 + 50.0, cy - cap * 0.5, nw + 4.0, cap), UI.INK, -1)
		var hit := UI.hit_area(tile, Rect2(Vector2.ZERO, r.size), func():
			if _drag_travel < TAP_SLOP:
				Sound.play("ui_tap")
				_open_kind(species))
		hit.mouse_filter = Control.MOUSE_FILTER_PASS
	_pop_in(tile, n)

## A met kind wears its current outfit and keeps living; an unmet one is the "?" silhouette.
func _kind_face(parent: Control, species: String, box: Rect2, phase: int, fill := 0.88) -> void:
	if profile.has_met(species):
		var current: String = profile.equipped.get(species, "")
		var outfit := "" if current.is_empty() else "%s_outfit_%s" % [species, current.get_slice(".", 2)]
		_bird(parent, species, outfit, box, phase * 0.45, fill)
	else:
		Island.piece_fit(parent, "island_unknown_bird", box.get_center(), box.size * 0.9)

func _open_kind(species: String) -> void:
	if not profile.has_met(species) or _outfit_page != null:
		return
	if instant:
		open_outfits(species)
	else:
		get_tree().create_timer(0.08).timeout.connect(func(): if _outfit_page == null: open_outfits(species))

## The aviary's detail page for one bird (equip, buy, preview) over the shop; its Back returns here.
func open_outfits(species: String) -> Control:
	var page: Control = AviaryScript.new()
	page.name = "BirdOutfits"
	page.instant = instant
	add_child(page)
	page.setup(profile, species)
	page.profile_changed.connect(func(): profile_changed.emit())
	page.closed.connect(_close_outfits)
	_outfit_page = page
	if not instant:
		page.modulate.a = 0.0
		page.create_tween().tween_property(page, "modulate:a", 1.0, 0.18)
	return page

func _close_outfits() -> void:
	if _outfit_page == null:
		return
	var page := _outfit_page
	_outfit_page = null
	_refresh()   # owned counts, worn outfits and coins
	if instant:
		page.queue_free()
		return
	var tw := page.create_tween()
	tw.tween_property(page, "modulate:a", 0.0, 0.16)
	tw.tween_callback(page.queue_free)


# ---------------------------------------------------------------- purchase flow

func _tap(it: Dictionary) -> void:
	if _drag_travel >= TAP_SLOP:   # the press became a scroll
		return
	if _locked(it):
		Sound.play("error")
		return
	if _owned(it):
		if not _equipped(it) and Shop.equip(profile, it["id"]) == "":
			profile_changed.emit()
			_refresh()
		return
	_confirm(it)

## Item, editable quantity and total-price Buy bar share the existing cream panel.
func _confirm(it: Dictionary) -> void:
	_purchase_item = it
	_quantity = 1
	_quantity_input = null
	var repeatable: bool = it["kind"] == Shop.BOOSTER
	_quantity_limit = maxi(1, profile.coins / int(it["price"])) if repeatable else 1
	var picture_size := POPUP_PIC if repeatable else SINGLE_PIC
	var heights := [POPUP_CAPTION, picture_size, UI.SECONDARY, BUY_SIZE.y] if repeatable else [picture_size, BUY_SIZE.y]
	_modal = Modal.open(self, _name_id(it), heights, true, 0.0, 0.0, instant)
	var c := _modal.content
	var ys: Array = _modal.get_meta("ys")
	var row := 0
	if repeatable:
		var inner := Rect2(_modal.panel_rect.position.x + 48.0, ys[0], _modal.panel_rect.size.x - 96.0, POPUP_CAPTION)
		UI.label(c, _caption_id(it), inner, POPUP_CAPTION_CAP).name = "Caption"
		row = 1
	var pic := Rect2(540.0 - picture_size * 0.5, ys[row], picture_size, picture_size)
	var glow := UI.image(c, "glow_soft", pic.grow(picture_size * 0.15))
	glow.modulate = Color(1.0, 0.86, 0.42, 0.8)
	_picture(c, it, pic)
	if repeatable:
		_quantity_row(c, ys[row + 1])
		_modal.closed.connect(_quantity_input.release_focus)
	_purchase_button = _pill_button(c, "Buy", Rect2(Vector2(540.0 - BUY_SIZE.x * 0.5, ys[-1]), BUY_SIZE), "green",
		func(): _modal.invoke(func(): _buy(it, _quantity)))
	_purchase_contents = Control.new()
	_purchase_contents.name = "Contents"
	_purchase_contents.size = BUY_SIZE
	_purchase_contents.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_purchase_button.add_child(_purchase_contents)
	_update_purchase()

func _quantity_row(c: Control, y: float) -> void:
	var row_w := STEP.x * 2.0 + QUANTITY_GAP * 2.0 + QUANTITY_WIDTH
	var x := 540.0 - row_w * 0.5
	var sy := y + (UI.SECONDARY - STEP.y) * 0.5
	_minus = _pill_button(c, "Decrease", Rect2(Vector2(x, sy), STEP), "green", func(): _step_quantity(-1))
	_sign_bars(_minus, false)
	_quantity_input = QuantityInput.new()
	_quantity_input.name = "Quantity"
	_quantity_input.limit = _quantity_limit
	_quantity_input.editable = profile.coins >= _purchase_item["price"]
	UI.place(_quantity_input, Rect2(x + STEP.x + QUANTITY_GAP, y, QUANTITY_WIDTH, UI.SECONDARY))
	c.add_child(_quantity_input)
	_quantity_input.value_changed.connect(func(value: int):
		_quantity = value
		_update_purchase())
	_plus = _pill_button(c, "Increase", Rect2(Vector2(x + row_w - STEP.x, sy), STEP), "green",
		func(): _step_quantity(1))
	_sign_bars(_plus, true)

## A bold white minus (or plus) on a green pill: two rounded bars, centred on the disc face.
func _sign_bars(b: Button, plus: bool) -> void:
	var face := Vector2(STEP.x, STEP.y - PILL_EDGE) * 0.5
	var bars := [Vector2(SIGN_BAR.x, SIGN_BAR.y)]
	if plus:
		bars.append(Vector2(SIGN_BAR.y, SIGN_BAR.x))
	for size in bars:
		var bar := Panel.new()
		bar.name = "Sign"
		bar.mouse_filter = Control.MOUSE_FILTER_IGNORE
		var box := StyleBoxFlat.new()
		box.bg_color = Color.WHITE
		box.set_corner_radius_all(int(SIGN_BAR.y * 0.5))
		box.shadow_color = Color(0.1, 0.35, 0.05, 0.6)
		box.shadow_offset = Vector2(0, 3)
		bar.add_theme_stylebox_override("panel", box)
		UI.place(bar, Rect2(face - size * 0.5, size))
		b.add_child(bar)

func _step_quantity(step: int) -> void:
	_quantity_input.release_focus()
	_quantity_input.set_value(_quantity + step)

func _update_purchase() -> void:
	var total: int = _purchase_item["price"] * maxi(1, _quantity)
	UI.set_active(_purchase_button, _quantity > 0 and profile.coins >= total)
	if _quantity_input != null:
		UI.set_active(_minus, _quantity > 1)
		UI.set_active(_plus, _quantity < _quantity_limit)
	for child in _purchase_contents.get_children():
		_purchase_contents.remove_child(child)
		child.queue_free()
	# "Buy" in white beside the gold price bar (the shop cards' price), centred together on the green pill.
	var cap := 64.0
	var word_w := UI.label_width("shop.buy", cap)
	var price := Vector2(250.0, 96.0)
	var x := (BUY_SIZE.x - word_w - 20.0 - price.x) * 0.5
	var mid := (BUY_SIZE.y - PILL_EDGE) * 0.5
	UI.label(_purchase_contents, "shop.buy", Rect2(x, 0, word_w, BUY_SIZE.y - PILL_EDGE), cap, Color.WHITE)
	Island.price_pill(_purchase_contents, Rect2(Vector2(x + word_w + 20.0, mid - price.y * 0.5), price), total)

func _buy(it: Dictionary, quantity: int) -> void:
	if _quantity_input != null:
		_quantity_input.release_focus()
	var err := Shop.purchase(profile, it["id"], quantity)
	if err != "":
		Sound.play("error")
		return
	if it["kind"] == Shop.BOOSTER:
		booster_purchased.emit(it["booster"], it["quantity"] * quantity)
	Sound.play("collect")
	profile_changed.emit()
	_refresh()


# ---------------------------------------------------------------- capture preview

static func preview(main: Control, state: String) -> void:
	var p := Profile.new()
	p.coins = 240
	p.highest_unlocked = Levels.COUNT
	p.inventory = PackedInt32Array([3, 2, 1, 1, 0, 0])
	var start := 0
	match state:
		"outfits", "outfits_some_unmet", "bird_outfits", "confirm_outfit", "confirm_outfit_short":
			p.coins = 120 if state == "confirm_outfit_short" else 1240
			p.met = {}
			for species in ["sunny", "sky", "berry", "mint", "cocoa", "snow", "ember", "crow", "ink", "hoot"]:
				p.met[species] = true
			if state == "outfits_some_unmet":
				p.met = {"sunny": true, "sky": true, "berry": true, "mint": true}
			p.outfits = {"outfit.sky.1": true, "outfit.mint.1": true, "outfit.berry.1": true, "outfit.berry.2": true}
			p.equipped = {"sky": "outfit.sky.1", "berry": "outfit.berry.2"}
			start = 1
		"houses":
			p.coins = 50270   # a long counter: the title sign must stay clear of it
			p.houses = {"teapot": true, "acorn": true}
			p.equipped_house = "teapot"
			start = 2
		"confirm_multiple", "confirm_maximum":
			p.coins = 1240
		"locked_tools":
			p.highest_unlocked = 3
			p.inventory = PackedInt32Array([2, 0, 0, 0, 0, 0])
		"insufficient":
			p.coins = 20
	var view: Control = load("res://game/shop_view.gd").new()
	view.instant = main.instant()
	view.setup(p, start)
	main.set_screen(view)
	view.closed.connect(func(): main.get_tree().quit())
	Sound.music("bgm_menu")
	match state:
		"confirm_booster", "insufficient":
			view._confirm(Shop.item("booster.firefly"))
		"bird_outfits":
			view.open_outfits("berry")
		"confirm_outfit", "confirm_outfit_short":
			var page: Control = view.open_outfits("berry")
			page._confirm_outfit("outfit.berry.3", 3, page._tiles.get_child(3))
		"confirm_multiple", "confirm_maximum":
			view._confirm(Shop.item("booster.owlEye"))
			view._quantity_input.set_value(5 if state == "confirm_multiple" else view._quantity_limit)
