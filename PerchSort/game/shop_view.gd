extends Control
## Shop: two wooden booster shelves, live birds on perches and nests on display stands.
## Cached categories share the sea and icon-only tabs; purchases keep the quantity popup.

signal closed
signal profile_changed
signal booster_purchased(kind: int, count: int)

const AviaryScript := preload("res://game/aviary_view.gd")
const HeadingInk := preload("res://game/shop_heading.gdshader")
const TABS := [Shop.BOOSTER, Shop.OUTFIT, Shop.HOUSE]
const TAB_NAMES := ["Boosters", "Outfits", "Nests"]
const TAB_PICTURE_NAMES := ["boosters", "outfits", "nests"]
## Shop composition, in 1080 x 1920 design pixels; awaiting user visual acceptance.
const CLOSE := Rect2(48, 48, 112, 112)
const TITLE_SIGN := Rect2(324, 48, 432, 112)
const TITLE_WORD := Rect2(372, 64, 336, 80)
const SIGN_ART_CONTENT := Rect2(18, 76, 2080, 610)
const AREA := Rect2(48, 224, 984, 1372)
const BOOSTER_SLOT := Vector2(312, 600)
const BOOSTER_TOP := 48.0
const TILE_GAP := 24.0
const ITEM_TILE := Vector2(312, 440)
const TAB_BAR := Rect2(72, 1660, 936, 160)
const TAB_GAP := 24.0
const TAB_ICON := 112.0
const TAB_SELECTED := Color8(156, 231, 77)
const BOOSTER_PICTURE := Rect2(36, 40, 240, 288)
const BOOSTER_NAME := Rect2(36, 360, 240, 44)
const PRICE_SIZE := Vector2(224, 80)
const BOOSTER_PRICE := Rect2(Vector2(44, 520), PRICE_SIZE)
const ITEM_PICTURE := Rect2(36, 16, 240, 216)
const ITEM_NAME := Rect2(32, 292, 248, 44)
const ITEM_ACTION := Rect2(Vector2(44, 352), PRICE_SIZE)
const STOCK := Vector2(268, 56)
const BOOSTER_LABEL := ["firefly", "sapling", "owl_eye", "morning_bell", "scarecrow", "spring_rain"]
const BOOSTER_ILLUS := ["firefly", "sprout", "owl_eye", "bell", "scarecrow", "rain"]   # island_icon_<name>
const BOOSTER_WORD := ["firefly", "sprout", "owl_eye", "wake_bell", "scarecrow", "spring_rain"]   # painted item_/cap_ words

## Layout (design pixels, 1080x1920; margins 48, 8 px grid).
const POPUP_CAPTION := 56.0
const PILL_EDGE := 8.0
const POPUP_PIC := 280.0
const SINGLE_PIC := 340.0
const POPUP_STAND_HEIGHT := 96.0
const BUY_SIZE := Vector2(560, 132)
const QUANTITY_WIDTH := 212.0
const STEP := Vector2(150, 112)            # - and + are short pills (no round buttons in popups)
const QUANTITY_GAP := 24.0

var profile: Profile
var tab := 0
var page := 0
var instant := false   # captures skip the pop-in motion
var _bar: Control
var _tabs: Control
var _grid: Control
var _content: Control
var _pages := {}
var _warm_ticket := 0
var _scroll: ScrollContainer
var _coins: FeatherBar
var _coins_shown := -1
var _title: TextureRect
var _heading_ink: ShaderMaterial
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
		var frame := StyleBoxEmpty.new()
		add_theme_stylebox_override("normal", frame)
		add_theme_stylebox_override("read_only", frame)
		ShopSkin.card(self, Rect2(Vector2.ZERO, size))
		var focus := UI._box(Color.TRANSPARENT, UI.INK, 28.0, 0.0, false)
		focus.set_border_width_all(2)
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
	_heading_ink = ShaderMaterial.new()
	_heading_ink.shader = HeadingInk
	UI.image(self, "background_shop_sea", Rect2(0, 0, 1080, 1920), false)
	_scroll = ScrollContainer.new()
	_scroll.name = "Scroll"
	_scroll.horizontal_scroll_mode = ScrollContainer.SCROLL_MODE_DISABLED
	_scroll.vertical_scroll_mode = ScrollContainer.SCROLL_MODE_SHOW_NEVER
	_scroll.scroll_deadzone = 12
	UI.place(_scroll, AREA)
	add_child(_scroll)
	_scroll.gui_input.connect(_on_scroll_input)
	_content = Control.new()
	_content.name = "Content"
	_content.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_content.custom_minimum_size = AREA.size
	_scroll.add_child(_content)
	_tabs = _layer("Tabs")
	_bar = _layer("Bar")
	_build_header()
	_refresh(false)
	if not instant:
		_warm_outfits.call_deferred()

func _notification(what: int) -> void:
	if what == NOTIFICATION_TRANSLATION_CHANGED and is_node_ready():
		_warm_ticket += 1
		for page in _pages.values():
			_content.remove_child(page)
			page.queue_free()
		_pages.clear()
		_build_header()
		_refresh(false)
		if not instant and tab != 1:
			_warm_outfits.call_deferred()

func _build_header() -> void:
	for child in _bar.get_children():
		_bar.remove_child(child)
		child.queue_free()
	_title = null
	_coins = null
	_coins_shown = -1
	Island.button(_bar, "Close", "island_close_tile", CLOSE, func(): closed.emit())
	UI.art(_bar, "panel_shop_sign", TITLE_SIGN, SIGN_ART_CONTENT)
	_build_title()

## The plaque names the visible category in the active locale, using the same painted word family for all tabs.
func _build_title() -> void:
	if _title != null:
		_bar.remove_child(_title)
		_title.queue_free()
	_title = UiText.painted(_bar, "white_tab_" + TAB_PICTURE_NAMES[tab], TITLE_WORD)
	_title.material = _heading_ink

func _layer(n: String) -> Control:
	var c := Control.new()
	c.name = n
	c.size = size
	c.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(c)
	return c

func _refresh(rebuild := true) -> void:
	_build_coins()
	for c in _tabs.get_children():
		_tabs.remove_child(c)
		c.queue_free()
	_tab_bar()
	if rebuild and _pages.has(tab):
		if tab == 1:
			_warm_ticket += 1
		var old: Control = _pages[tab]
		_content.remove_child(old)
		old.queue_free()
		_pages.erase(tab)
	for page in _pages.values():
		page.hide()
		page.process_mode = Node.PROCESS_MODE_DISABLED
	if not _pages.has(tab):
		_build_page(tab)
	_grid = _pages[tab]
	_grid.show()
	_grid.process_mode = Node.PROCESS_MODE_INHERIT
	_scroll.scroll_vertical = 0

func _build_page(category: int) -> void:
	if category == 1 and not instant:
		_warm_outfits()
		return
	var page := Control.new()
	page.name = "Grid_" + TAB_NAMES[category]
	page.size = AREA.size
	page.mouse_filter = Control.MOUSE_FILTER_IGNORE
	page.hide()
	page.process_mode = Node.PROCESS_MODE_DISABLED
	_content.add_child(page)
	_pages[category] = page
	_grid = page
	if TABS[category] == Shop.BOOSTER:
		_booster_shelves()
		var items := _items(Shop.BOOSTER)
		for n in items.size():
			var origin := Vector2((n % 3) * (BOOSTER_SLOT.x + TILE_GAP), BOOSTER_TOP + (n / 3) * (BOOSTER_SLOT.y + TILE_GAP))
			_booster_card(items[n], Rect2(origin, BOOSTER_SLOT), n)
	else:
		var tiles: Array = outfit_species() if TABS[category] == Shop.OUTFIT else _items(Shop.HOUSE)
		for n in tiles.size():
			var r := Rect2(Vector2((n % 3) * (ITEM_TILE.x + TILE_GAP), (n / 3) * (ITEM_TILE.y + TILE_GAP)), ITEM_TILE)
			if TABS[category] == Shop.OUTFIT:
				_kind_tile(tiles[n], r, n)
			else:
				_card(tiles[n], r, n)

## Prepare one live bird per frame; switching to a cached page never reloads nine animation sets at once.
func _warm_outfits() -> void:
	if _pages.has(1):
		return
	var shown := _grid
	var page := Control.new()
	page.name = "Grid_Outfits"
	page.size = AREA.size
	page.mouse_filter = Control.MOUSE_FILTER_IGNORE
	page.visible = tab == 1
	page.process_mode = Node.PROCESS_MODE_INHERIT if tab == 1 else Node.PROCESS_MODE_DISABLED
	_content.add_child(page)
	_pages[1] = page
	var ticket := _warm_ticket
	var species := outfit_species()
	for n in species.size():
		await get_tree().process_frame
		if ticket != _warm_ticket:
			return
		shown = _grid
		_grid = page
		_kind_tile(species[n], Rect2(Vector2((n % 3) * (ITEM_TILE.x + TILE_GAP), (n / 3) * (ITEM_TILE.y + TILE_GAP)), ITEM_TILE), n)
		_grid = page if tab == 1 else shown

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

## A compact shop counter leaves the category plaque and content as the main focus.
func _build_coins() -> void:
	if _coins == null:
		_coins = ShopSkin.counter(_bar, profile)
		_coins_shown = profile.coins
	elif _coins_shown != profile.coins:
		_coins.set_amount(profile.coins, not instant)
		_coins_shown = profile.coins

## Three separate painted tiles. Selection changes the face, never the tile or icon size.
func _tab_bar() -> void:
	var width := (TAB_BAR.size.x - TAB_GAP * (TABS.size() - 1)) / TABS.size()
	for i in TABS.size():
		var on := i == tab
		var bounds := Rect2(TAB_BAR.position + Vector2(i * (width + TAB_GAP), 0), Vector2(width, TAB_BAR.size.y))
		var hit := UI.hit_area(_tabs, bounds, func():
			if i != tab:
				Sound.play("ui_tap")
			_select_tab(i))
		hit.name = TAB_NAMES[i]
		var face := ShopSkin.card(hit, Rect2(Vector2.ZERO, bounds.size))
		var tint := TAB_SELECTED if on else Color.WHITE
		face.modulate = tint
		_fit_visible(hit, "island_tab_" + TAB_PICTURE_NAMES[i], bounds.size * 0.5, TAB_ICON)
		if on:
			Island.check_badge(hit, Vector2(width - 36, 36))
		hit.button_down.connect(func(): face.modulate = tint * Island.PRESSED)
		hit.button_up.connect(func(): face.modulate = tint)

func _select_tab(i: int) -> void:
	if i != tab:
		tab = i
		page = 0
		_build_title()
		_refresh(false)
		if not instant:
			var face: Control = _tabs.get_node(TAB_NAMES[i]).get_child(0)
			face.modulate = Color.WHITE
			face.create_tween().tween_property(face, "modulate", TAB_SELECTED, 0.14)

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

## Nests rest on a low wooden stand; only the compact action carries a state face.
func _card(it: Dictionary, r: Rect2, n: int) -> void:
	var card := _item_holder(it, r)
	var body: Control = card.get_node("Body")
	_support(body, "prop_shop_nest_stand", Vector2(r.size.x * 0.5, ITEM_PICTURE.end.y), 264, 0.20)
	_picture(body, it, ITEM_PICTURE)
	ShopSkin.word(body, _name_id(it), ITEM_NAME)
	if _owned(it):
		var equip := ShopSkin.card(body, ITEM_ACTION)
		equip.get_node("Skin").self_modulate = ShopSkin.GREEN
		UiText.painted(body, "white_equipped" if _equipped(it) else "white_equip", ITEM_ACTION.grow(-16))
		if _equipped(it):
			Island.check_badge(body, Vector2(r.size.x - 40, 40))
	else:
		ShopSkin.price(body, ITEM_ACTION, it["price"])
	var hit: Button = card.get_node("Select")
	hit.button_down.connect(func(): body.modulate = Island.PRESSED)
	hit.button_up.connect(func(): body.modulate = Color.WHITE)
	_pop_in(card, n)

## Three items share each shelf; the smaller names sit on its front rail.
func _booster_shelves() -> void:
	for row in 2:
		var seat_y := BOOSTER_TOP + row * (BOOSTER_SLOT.y + TILE_GAP) + BOOSTER_PICTURE.end.y
		_support(_grid, "prop_shop_shelf", Vector2(AREA.size.x * 0.5, seat_y), AREA.size.x, 0.15)

## Register a support's seat within its visible art, excluding transparent canvas padding.
func _support(parent: Node, picture: String, seat: Vector2, width: float, seat_ratio: float) -> TextureRect:
	var used := UI.used_rect(picture)
	var height := width * used.size.y / used.size.x
	return UI.art(parent, picture, Rect2(seat.x - width * 0.5, seat.y - height * seat_ratio, width, height))

## Six open product slots retain the same stock badge and numeric-only purchase action.
func _booster_card(it: Dictionary, r: Rect2, n: int) -> void:
	var card := _item_holder(it, r)
	var body: Control = card.get_node("Body")
	_picture(body, it, BOOSTER_PICTURE)
	UiText.painted(body, _label_id(it), BOOSTER_NAME)
	ShopSkin.price(body, BOOSTER_PRICE, it["price"])
	if _locked(it):
		body.modulate.a = UI.DISABLED_ALPHA
		Island.piece_fit(card, "island_padlock", STOCK, Vector2(66, 66)).name = "Lock"
	else:
		_stock_badge(body, profile.inventory[it["booster"]])
		var hit: Button = card.get_node("Select")
		hit.button_down.connect(func(): body.modulate = Island.PRESSED)
		hit.button_up.connect(func(): body.modulate = Color.WHITE)
	_pop_in(card, n)

## Zero is useful stock information, so shop badges remain visible even when the item is used up.
func _stock_badge(parent: Node, count: int) -> void:
	var side := 60.0
	var badge := Control.new()
	badge.name = "Stock"
	badge.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(badge, Rect2(STOCK - Vector2.ONE * side * 0.5, Vector2.ONE * side))
	parent.add_child(badge)
	UI.art(badge, "badge_shop_stock", Rect2(Vector2.ZERO, badge.size), Rect2(197, 214, 842, 844))
	Island.digits(badge, str(mini(count, Island.BADGE_CAP)), Rect2(12, 12, side - 24, side - 24))

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
	hit.name = "Select"
	hit.mouse_filter = Control.MOUSE_FILTER_PASS
	hit.disabled = _locked(it)
	var body := Control.new()
	body.name = "Body"
	body.size = r.size
	body.mouse_filter = Control.MOUSE_FILTER_IGNORE
	card.add_child(body)
	return card

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

## Fit visible item art to a shared baseline; its feet/base rests on the display surface.
func _picture(parent: Node, it: Dictionary, box: Rect2) -> void:
	var picture: String = "island_icon_%s" % BOOSTER_ILLUS[it["booster"]] if it["kind"] == Shop.BOOSTER else "house_style_%s_body" % it["style"]
	var used := UI.used_rect(picture)
	var k := minf(box.size.x / used.size.x, box.size.y / used.size.y)
	var fitted := used.size * k
	UI.art(parent, picture, Rect2(Vector2(box.get_center().x - fitted.x * 0.5, box.end.y - fitted.y), fitted))

func _pill_button(parent: Node, n: String, r: Rect2, kind: String, on_press: Callable) -> Button:
	return ShopSkin.button(parent, n, r, on_press, kind)

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
	_support(tile, "island_branch", Vector2(r.size.x * 0.5, ITEM_PICTURE.end.y), 272, 0.47)
	_kind_face(tile, species, ITEM_PICTURE, n, 1.0)
	if met:
		UiText.painted_label(tile, "bird.%s.name" % species, ITEM_NAME, true)
		var slot := Rect2(84, 360, 144, 64)
		ShopSkin.card(tile, slot)
		var text := "%d/%d" % [owned_outfits(species), Shop.OUTFIT_COUNT + 1]
		var cap := 36.0
		var nw := UI.number_width(text, cap)
		var cy := slot.get_center().y
		UI.number(tile, text, Rect2((r.size.x - nw) * 0.5, cy - cap * 0.5, nw + 4.0, cap), UI.INK)
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

## One wooden frame contains the supported item, necessary caption, quantity and total.
func _confirm(it: Dictionary) -> void:
	_purchase_item = it
	_quantity = 1
	_quantity_input = null
	var repeatable: bool = it["kind"] == Shop.BOOSTER
	_quantity_limit = maxi(1, profile.coins / int(it["price"])) if repeatable else 1
	var picture_size := POPUP_PIC if repeatable else SINGLE_PIC
	var display_height := picture_size + POPUP_STAND_HEIGHT
	var heights := [display_height, POPUP_CAPTION, UI.SECONDARY, BUY_SIZE.y] if repeatable else [display_height, BUY_SIZE.y]
	_modal = _purchase_modal(_name_id(it), heights)
	var c := _modal.content
	var ys: Array = _modal.get_meta("ys")
	var pic := Rect2(540.0 - picture_size * 0.5, ys[0], picture_size, picture_size)
	var glow := UI.image(c, "glow_soft", pic.grow(picture_size * 0.15))
	glow.modulate = Color(1.0, 0.86, 0.42, 0.8)
	_support(c, "prop_shop_nest_stand", Vector2(540, pic.end.y), picture_size * 1.15, 0.20)
	_picture(c, it, pic)
	if repeatable:
		var inner := Rect2(_modal.panel_rect.position.x + 96, ys[1], _modal.panel_rect.size.x - 192, POPUP_CAPTION)
		UiText.painted_label(c, _caption_id(it), inner).name = "Caption"
		_quantity_row(c, ys[2])
		_modal.closed.connect(_quantity_input.release_focus)
	_purchase_button = _pill_button(c, "Buy", Rect2(Vector2(540.0 - BUY_SIZE.x * 0.5, ys[-1]), BUY_SIZE), "gold",
		func(): _modal.invoke(func(): _buy(it, _quantity)))
	_purchase_contents = Control.new()
	_purchase_contents.name = "Contents"
	_purchase_contents.size = BUY_SIZE
	_purchase_contents.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_purchase_button.add_child(_purchase_contents)
	_update_purchase()

## Keep the shared modal lifecycle, with a page-local wood frame and compact item plaque.
func _purchase_modal(title_id: String, heights: Array) -> Modal:
	var view := Modal.open(self, title_id, heights, true, 0.0, 0.0, instant, false, "", false)
	for node_name in ["Panel", "TitleGroup"]:
		var old := view.content.get_node(node_name)
		view.content.remove_child(old)
		old.queue_free()
	var frame := UI.kit_piece(view.content, "panel", view.panel_rect, 0.58)
	frame.name = "Panel"
	view.content.move_child(frame, 0)
	var title := Control.new()
	title.name = "TitleGroup"
	title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(title, Rect2(280, view.panel_rect.position.y - 56, 520, 132))
	view.content.add_child(title)
	UI.art(title, "panel_shop_sign", Rect2(Vector2.ZERO, title.size), SIGN_ART_CONTENT)
	ShopSkin.word(title, title_id, Rect2(56, 38, 408, 56))
	view._title_layer = title
	return view

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
	UI.place(_quantity_input, Rect2(x + STEP.x + QUANTITY_GAP, sy, QUANTITY_WIDTH, STEP.y))
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
	ShopSkin.amount(_purchase_contents, Rect2(24, 22, BUY_SIZE.x - 48, BUY_SIZE.y - 44), total)

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
	p.coins = 1280
	p.highest_unlocked = Levels.COUNT
	p.inventory = PackedInt32Array([3, 2, 1, 1, 0, 2])
	var start := 0
	match state:
		"boosters_empty":
			p.coins = 0
			p.inventory = PackedInt32Array([0, 0, 0, 0, 0, 0])
		"boosters_maximum":
			p.coins = FeatherBar.DISPLAY_MAX
			p.inventory = PackedInt32Array([99, 99, 99, 99, 99, 99])
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
