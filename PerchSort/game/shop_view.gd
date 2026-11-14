extends Control
## Shop: single-column forest shelves with painted names and live outfit previews.
## Cached categories share a scalable scroll viewport and three icon-only tabs.

signal closed
signal profile_changed
signal booster_purchased(kind: int, count: int)

const AviaryScript := preload("res://game/aviary_view.gd")
const ChestLettering := preload("res://game/chest_lettering.gd")
const TABS := [Shop.BOOSTER, Shop.OUTFIT, Shop.HOUSE]
const TAB_NAMES := ["Boosters", "Outfits", "Nests"]
const TAB_PICTURE_NAMES := ["boosters", "outfits", "nests"]
## Shop composition, in 1080 x 1920 design pixels. Every button size comes from ButtonKit's size table.
const CLOSE := Rect2(48, 48, 112, 112)
const SIGN_RECT := Rect2(220, 150, 640, 223)
const TITLE_WORD := Rect2(284, 232, 512, 132)
const AREA := Rect2(48, 384, 984, 1284)
const BOOSTER_SLOT := Vector2(984, 236)
const TILE_GAP := 20.0
const ITEM_TILE := BOOSTER_SLOT
const COLUMNS := 1
## Outfit and nest cards: two columns, a picture over its name and one secondary button.
const CARD_TILE := Vector2(482, 412)
const CARD_GAP := Vector2(20, 20)
const CARD_COLUMNS := 2
const CARD_PICTURE := Rect2(81, 12, 320, 180)
const CARD_NAME := Rect2(41, 210, 400, 66)
const CARD_ACTION := Rect2(Vector2(97, 284), ButtonKit.SECONDARY)
const TAB_BAR := Rect2(198, 1680, 684, 192)
const TAB_GAP := 48.0
const TAB_ICON := 132.0
const TAB_SELECTED := Color(1.0, 1.0, 1.0)
## Booster rows: picture, name, price button; the owned count is a badge on the picture.
const BOOSTER_PICTURE := Rect2(36, 18, 200, 200)
const BOOSTER_NAME := Rect2(262, 78, 380, 80)
const PRICE_SIZE := ButtonKit.SECONDARY
const BOOSTER_PRICE := Rect2(Vector2(660, 64), PRICE_SIZE)
const BADGE := 76.0
## Visible contours exclude near-transparent export noise without modifying the source art.
const HOUSE_INK := {
	"house_style_acorn_body": Rect2(56, 51, 398, 404),
	"house_style_beehive_body": Rect2(54, 53, 401, 402),
	"house_style_cage_body": Rect2(61, 29, 384, 427),
	"house_style_lantern_body": Rect2(50, 61, 409, 394),
	"house_style_mushroom_body": Rect2(41, 50, 427, 403),
	"house_style_pinecone_body": Rect2(59, 14, 390, 441),
	"house_style_pumpkin_body": Rect2(49, 37, 413, 423),
	"house_style_teacup_body": Rect2(83, 136, 389, 324),
	"house_style_teapot_body": Rect2(29, 55, 459, 399),
}
const ITEM_PICTURE := CARD_PICTURE
const ITEM_NAME := CARD_NAME
const ITEM_ACTION := CARD_ACTION
const STOCK := Vector2(222, 196)
const BOOSTER_LABEL := ["firefly", "sapling", "owl_eye", "morning_bell", "scarecrow", "spring_rain"]
const BOOSTER_ILLUS := ["firefly", "sprout", "owl_eye", "bell", "scarecrow", "rain"]   # island_icon_<name>
const BOOSTER_WORD := ["firefly", "sprout", "owl_eye", "wake_bell", "scarecrow", "spring_rain"]   # painted item_/cap_ words

## Purchase popup (design pixels).
const POPUP_CAPTION := 80.0
const POPUP_PIC := 440.0
const SINGLE_PIC := 500.0
const BUY_SIZE := ButtonKit.WIDE
const QUANTITY_WIDTH := ButtonKit.FIELD.x
const STEP := ButtonKit.KEY
const QUANTITY_GAP := 24.0
const SIGN_INK := Color(0.16, 0.52, 0.08)

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
var _modal: Modal
var _purchase_item: Dictionary
var _quantity := 1
var _quantity_limit := 1
var _quantity_input: QuantityInput
const SIGN_BAR := Vector2(44, 12)
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

	const CAP := 74.0
	const INK := Color.WHITE
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
		var focus := UI._box(Color.TRANSPARENT, UI.GOLD, 28.0, 0.0, false)
		focus.set_border_width_all(2)
		add_theme_stylebox_override("focus", focus)
		_selection = ColorRect.new()
		_selection.color = Color(1.0, 0.91, 0.55, 0.26)
		_selection.mouse_filter = Control.MOUSE_FILTER_IGNORE
		_selection.hide()
		add_child(_selection)
		_digits = ButtonKit.number(self, text, Rect2(16, (size.y - CAP) * 0.5 - 3, size.x - 32, CAP))
		_caret = ColorRect.new()
		_caret.color = Color.WHITE
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
			var rect := Rect2(_digits.position, _digits.size)
			remove_child(_digits)
			_digits.queue_free()
			_digits = ButtonKit.number(self, text, rect)
			move_child(_digits, 1)

	func _process(_delta: float) -> void:
		var width := ButtonKit.number_width(text, CAP)
		var cap := CAP if width == 0.0 else CAP * minf(1.0, _digits.size.x / width)
		var left := (size.x - ButtonKit.number_width(text, cap)) * 0.5
		_caret.visible = is_editing() and not has_selection()
		_caret.position = Vector2(left + ButtonKit.number_width(text.left(caret_column), cap), 34)
		_selection.visible = is_editing() and has_selection()
		if _selection.visible:
			var start := ButtonKit.number_width(text.left(get_selection_from_column()), cap)
			var end := ButtonKit.number_width(text.left(get_selection_to_column()), cap)
			_selection.position = Vector2(left + start, 32)
			_selection.size = Vector2(end - start, 56)


func setup(p: Profile, start_tab := 0) -> void:
	profile = p
	tab = start_tab
	size = Vector2(1080, 1920)
	UI.image(self, "background_home_jungle", Rect2(0, 0, 1080, 1920), false).name = "JungleBackground"
	_scroll = ScrollContainer.new()
	_scroll.name = "Scroll"
	_scroll.clip_contents = true
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
	MenuNavigation.close(_bar, "Close", CLOSE.position, func(): closed.emit())
	ButtonKit.banner(_bar, Rect2(SIGN_RECT.position.x, SIGN_RECT.position.y + SIGN_RECT.size.y * ButtonKit.BANNER_TOP,
		SIGN_RECT.size.x, SIGN_RECT.size.y * (1.0 - ButtonKit.BANNER_TOP)))
	_build_title()

## Floating category words keep the existing painted lettering and locale updates.
func _build_title() -> void:
	if _title != null:
		_bar.remove_child(_title)
		_title.queue_free()
	_title = ButtonKit.lettering(_bar, "tab_" + TAB_PICTURE_NAMES[tab], TITLE_WORD)

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
	_sync_content_size()
	_scroll.scroll_vertical = 0

func _build_page(category: int) -> void:
	if category == 1 and not instant:
		_warm_outfits()
		return
	var page := Control.new()
	page.name = "Grid_" + TAB_NAMES[category]
	page.size = Vector2(AREA.size.x, _page_height(category))
	page.mouse_filter = Control.MOUSE_FILTER_IGNORE
	page.hide()
	page.process_mode = Node.PROCESS_MODE_DISABLED
	_content.add_child(page)
	_pages[category] = page
	_grid = page
	if TABS[category] == Shop.BOOSTER:
		var items := _items(Shop.BOOSTER)
		for n in items.size():
			_booster_card(items[n], _tile_rect(n, category), n)
	else:
		var tiles: Array = outfit_species() if TABS[category] == Shop.OUTFIT else _items(Shop.HOUSE)
		for n in tiles.size():
			if TABS[category] == Shop.OUTFIT:
				_kind_tile(tiles[n], _tile_rect(n, category), n)
			else:
				_card(tiles[n], _tile_rect(n, category), n)

## Boosters are full-width rows; outfits and nests are two columns of tall cards.
func _tile_rect(index: int, category: int) -> Rect2:
	if TABS[category] == Shop.BOOSTER:
		return Rect2(Vector2((index % COLUMNS) * (ITEM_TILE.x + TILE_GAP),
			(index / COLUMNS) * (ITEM_TILE.y + TILE_GAP)), ITEM_TILE)
	return Rect2(Vector2((index % CARD_COLUMNS) * (CARD_TILE.x + CARD_GAP.x),
		(index / CARD_COLUMNS) * (CARD_TILE.y + CARD_GAP.y)), CARD_TILE)

func _page_height(category: int) -> float:
	var count := outfit_species().size() if TABS[category] == Shop.OUTFIT else _items(TABS[category]).size()
	if TABS[category] != Shop.BOOSTER:
		var card_rows := ceili(float(count) / CARD_COLUMNS)
		return maxf(AREA.size.y, card_rows * CARD_TILE.y + maxi(0, card_rows - 1) * CARD_GAP.y + 8)
	var rows := ceili(float(count) / COLUMNS)
	return maxf(AREA.size.y, rows * ITEM_TILE.y + maxi(0, rows - 1) * TILE_GAP + 8)

## Only the visible category sizes the scroll content, including during background outfit warming.
func _sync_content_size() -> void:
	_content.custom_minimum_size = Vector2(AREA.size.x, _page_height(tab))
	_content.size = _content.custom_minimum_size

## Prepare one live bird per frame; switching to a cached page never reloads nine animation sets at once.
func _warm_outfits() -> void:
	if _pages.has(1):
		return
	var shown := _grid
	var page := Control.new()
	page.name = "Grid_Outfits"
	page.size = Vector2(AREA.size.x, _page_height(1))
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
		_kind_tile(species[n], _tile_rect(n, 1), n)
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

## The profile's balance changed while this page stays open (a feather purchase): the counter and the cards that
## depend on it follow, on the same page, at the same scroll position.
func refresh_balance() -> void:
	var keep := _scroll.scroll_vertical
	_refresh(true)
	_scroll.scroll_vertical = keep
	if is_instance_valid(_outfit_page):
		_outfit_page.refresh_balance()

## The compact balance leaves the floating heading and cards as the main focus.
func _build_coins() -> void:
	if _coins == null:
		_coins = ShopSkin.counter(_bar, profile)
		_coins_shown = profile.coins
	elif _coins_shown != profile.coins:
		_coins.set_amount(profile.coins, not instant)
		_coins_shown = profile.coins
	AviaryScript._center_digits(_coins.get_node("Digits").get_child(0))

## Three separate painted tiles. Selection changes the face, never the tile or icon size.
func _tab_bar() -> void:
	var width := ButtonKit.TAB.x
	for i in TABS.size():
		var on := i == tab
		var bounds := Rect2(TAB_BAR.position + Vector2(i * (width + TAB_GAP), 0), Vector2(width, TAB_BAR.size.y))
		var hit := UI.hit_area(_tabs, bounds, func():
			if i != tab:
				Sound.play("ui_tap")
			_select_tab(i))
		hit.name = TAB_NAMES[i]
		var face := ButtonKit.face(hit, "key_green" if on else "key_cream", Rect2(Vector2.ZERO, bounds.size))
		_fit_visible(hit, "island_tab_" + TAB_PICTURE_NAMES[i], bounds.size * 0.5 - Vector2(0, 4), TAB_ICON)
		hit.button_down.connect(func(): face.modulate = ButtonKit.PRESSED)
		hit.button_up.connect(func(): face.modulate = Color.WHITE)

func _select_tab(i: int) -> void:
	if i != tab:
		tab = i
		page = 0
		_build_title()
		_refresh(false)
		if not instant:
			var face: Control = _tabs.get_node(TAB_NAMES[i]).get_node("Face")
			face.scale = Vector2.ONE * 0.9
			face.create_tween().tween_property(face, "scale", Vector2.ONE, 0.18).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)

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

## The painted lettering key of any catalog item's title (boosters, nests and outfits).
func _title_key(it: Dictionary) -> String:
	return str(it["id"]).replace(".", "_") if it["kind"] == Shop.OUTFIT else _label_id(it)

## The baked label of an item's name (tools/island/bake_labels.py).
func _label_id(it: Dictionary) -> String:
	if it["kind"] == Shop.BOOSTER:
		return "item_" + BOOSTER_WORD[it["booster"]]
	return "house_" + str(it["style"])

## Nests share the booster card geometry, with a consistent numeric or equip action.
func _card(it: Dictionary, r: Rect2, n: int) -> void:
	var card := _item_holder(it, r)
	var body: Control = card.get_node("Body")
	_picture(body, it, ITEM_PICTURE)
	ButtonKit.lettering(body, _label_id(it), ITEM_NAME)
	var action := ButtonKit.plate(body, ITEM_ACTION)
	action.name = "Price" if not _owned(it) else "Action"
	if _owned(it):
		ButtonKit.word(action, "white_equipped" if _equipped(it) else "white_equip", ITEM_ACTION.size)
		if _equipped(it):
			Island.check_badge(body, Vector2(r.size.x - 44, 44))
	else:
		ButtonKit.price(action, ITEM_ACTION.size, it["price"])
	var hit: Button = card.get_node("Select")
	hit.button_down.connect(func(): body.modulate = Island.PRESSED)
	hit.button_up.connect(func(): body.modulate = Color.WHITE)
	_pop_in(card, n)

## Equal grouped product rows keep inline stock counts and feather-price actions.
func _booster_card(it: Dictionary, r: Rect2, n: int) -> void:
	var card := _item_holder(it, r)
	var body: Control = card.get_node("Body")
	_picture(body, it, BOOSTER_PICTURE)
	ButtonKit.lettering(body, _label_id(it), BOOSTER_NAME)
	var price := ButtonKit.plate(body, BOOSTER_PRICE)
	price.name = "Price"
	ButtonKit.price(price, BOOSTER_PRICE.size, it["price"])
	if _locked(it):
		body.modulate.a = UI.DISABLED_ALPHA
		Island.piece_fit(card, "island_padlock", STOCK, Vector2(66, 66)).name = "Lock"
	else:
		_stock_count(body, profile.inventory[it["booster"]])
		var hit: Button = card.get_node("Select")
		hit.button_down.connect(func(): body.modulate = Island.PRESSED)
		hit.button_up.connect(func(): body.modulate = Color.WHITE)
	_pop_in(card, n)

## The owned count is a green badge on the picture's corner; zero stays visible as stock information.
func _stock_count(parent: Node, count: int) -> void:
	var stock := Control.new()
	stock.name = "Stock"
	stock.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(stock, Rect2(STOCK - Vector2.ONE * BADGE * 0.5, Vector2.ONE * BADGE))
	parent.add_child(stock)
	UI.image(stock, "island_badge_green", Rect2(Vector2.ZERO, stock.size), false)
	var digits := ButtonKit.number(stock, str(mini(count, Island.BADGE_CAP)), Rect2(BADGE * 0.14, BADGE * 0.2, BADGE * 0.72, BADGE * 0.56))
	digits.name = "Count"

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
	_shelf(body, r.size)
	return card

## One reusable cream card: the painted panel shared with the purchase popup.
func _shelf(parent: Node, bounds: Vector2) -> Control:
	var holder := Control.new()
	holder.name = "Shelf"
	holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	holder.size = bounds
	parent.add_child(holder)
	ButtonKit.panel(holder, Rect2(Vector2.ZERO, bounds)).name = "Surface"
	return holder

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
	var used: Rect2 = HOUSE_INK.get(picture, UI.used_rect(picture))
	var k := minf(box.size.x / used.size.x, box.size.y / used.size.y)
	var fitted := used.size * k
	UI.art(parent, picture, Rect2(Vector2(box.get_center().x - fitted.x * 0.5, box.end.y - fitted.y), fitted), used)

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

## An outfit tile: the bird on its branch, its name and one pill with how many looks it owns; a tap opens its
## outfit page. An unmet kind is the "?" silhouette and does nothing.
func _kind_tile(species: String, r: Rect2, n: int) -> void:
	var met := profile.has_met(species)
	var tile := Control.new()
	tile.name = "Kind_" + species
	tile.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(tile, r)
	_grid.add_child(tile)
	var face := _shelf(tile, r.size)
	if met:
		var width := ITEM_PICTURE.size.x * 0.94
		var k := width / Art.tex("branch_aviary_perch").get_width()
		Island.piece(tile, "branch_aviary_perch", Vector2(ITEM_PICTURE.get_center().x, ITEM_PICTURE.end.y - 4.0)
			- Vector2(width * 0.5, AviaryScript.PERCH_CONTACT_Y * k), width).name = "Perch"
	_kind_face(tile, species, ITEM_PICTURE, n, 1.0)
	if met:
		ButtonKit.lettering(tile, "bird_" + species, ITEM_NAME)
		var action := ButtonKit.plate(tile, ITEM_ACTION)
		action.name = "Action"
		var text := "%d/%d" % [owned_outfits(species), Shop.OUTFIT_COUNT + 1]
		var cap := ITEM_ACTION.size.y * ButtonKit.CAP
		cap = ITEM_ACTION.size.y * 0.52
		ButtonKit.number(action, text, Rect2(ITEM_ACTION.size.x * 0.16, ITEM_ACTION.size.y * 0.47 - cap * 0.5,
			ITEM_ACTION.size.x * 0.5, cap)).name = "Count"
		var paint := ShaderMaterial.new()
		paint.shader = preload("res://game/chest_play_word.gdshader")
		var arrow := _fit_visible(action, "island_chevron", Vector2(ITEM_ACTION.size.x * 0.78, ITEM_ACTION.size.y * 0.47), cap * 0.8)
		arrow.pivot_offset = arrow.size * 0.5
		arrow.rotation = -PI * 0.5
		arrow.material = paint
		var hit := UI.hit_area(tile, Rect2(Vector2.ZERO, r.size), func():
			if _drag_travel < TAP_SLOP:
				Sound.play("ui_tap")
				_open_kind(species))
		hit.name = "Select"
		hit.mouse_filter = Control.MOUSE_FILTER_PASS
		hit.button_down.connect(func(): face.modulate = Island.PRESSED)
		hit.button_up.connect(func(): face.modulate = Color.WHITE)
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
			Sound.play("equip")
			profile_changed.emit()
			_refresh()
		return
	_confirm(it)

## The shared purchase card (PurchaseDialog): the item on a soft glow, a short caption, the quantity row and one
## primary Buy button inside the card.
func _confirm(it: Dictionary) -> void:
	if is_instance_valid(_modal):
		return
	_purchase_item = it
	_quantity = 1
	_quantity_input = null
	var repeatable: bool = it["kind"] == Shop.BOOSTER
	_quantity_limit = maxi(1, profile.coins / int(it["price"])) if repeatable else 1
	var pic_size := POPUP_PIC if repeatable else SINGLE_PIC
	var heights := [pic_size, POPUP_CAPTION, STEP.y] if repeatable else [pic_size]
	_modal = PurchaseDialog.open(self, _title_key(it), heights, instant, BUY_SIZE.y)
	_modal.closed.connect(func(): _modal = null)
	var c := _modal.content
	var ys: Array = _modal.get_meta("ys")
	var pic := Rect2(540.0 - pic_size * 0.5, ys[0], pic_size, pic_size)
	PurchaseDialog.glow(c, pic)
	_picture(c, it, pic)
	if repeatable:
		var card: Rect2 = _modal.get_meta("card_rect")
		var caption := ButtonKit.lettering(c, "cap_" + BOOSTER_WORD[it["booster"]], Rect2(card.position.x + 64, ys[1], card.size.x - 128, POPUP_CAPTION))
		caption.name = "Caption"
		_quantity_row(c, ys[2])
		_modal.closed.connect(_quantity_input.release_focus)
	_purchase_button = ButtonKit.button(c, "Buy", Rect2(Vector2(540.0 - BUY_SIZE.x * 0.5, _modal.panel_rect.end.y + Modal.ACTION_DROP), BUY_SIZE),
		"wide", func(): _modal.invoke(func(): _buy(it, _quantity)))
	_purchase_contents = _purchase_button.get_node("Content")
	_update_purchase()

func _quantity_row(c: Control, y: float) -> void:
	var row_w := STEP.x * 2.0 + QUANTITY_GAP * 2.0 + QUANTITY_WIDTH
	var x := 540.0 - row_w * 0.5
	var field := Rect2(x + STEP.x + QUANTITY_GAP, y + (STEP.y - ButtonKit.FIELD.y) * 0.5, QUANTITY_WIDTH, ButtonKit.FIELD.y)
	ButtonKit.face(c, "wide", field).name = "QuantityWell"
	_minus = ButtonKit.button(c, "Decrease", Rect2(Vector2(x, y), STEP), "key_cream", func(): _step_quantity(-1))
	_sign_bars(_minus, false)
	_quantity_input = QuantityInput.new()
	_quantity_input.name = "Quantity"
	_quantity_input.limit = _quantity_limit
	_quantity_input.editable = profile.coins >= _purchase_item["price"]
	UI.place(_quantity_input, field)
	c.add_child(_quantity_input)
	_quantity_input.value_changed.connect(func(value: int):
		_quantity = value
		_update_purchase())
	_plus = ButtonKit.button(c, "Increase", Rect2(Vector2(x + row_w - STEP.x, y), STEP), "key_cream", func(): _step_quantity(1))
	_sign_bars(_plus, true)

## A bold green minus (or plus) on a cream key: rounded bars centred on the key face.
func _sign_bars(b: Button, plus: bool) -> void:
	var centre := Vector2(STEP.x * 0.5, STEP.y * 0.47)
	var bars := [Vector2(SIGN_BAR.x, SIGN_BAR.y)]
	if plus:
		bars.append(Vector2(SIGN_BAR.y, SIGN_BAR.x))
	for size in bars:
		var bar := Panel.new()
		bar.name = "Sign"
		bar.mouse_filter = Control.MOUSE_FILTER_IGNORE
		var box := StyleBoxFlat.new()
		box.bg_color = SIGN_INK
		box.set_corner_radius_all(int(SIGN_BAR.y * 0.5))
		bar.add_theme_stylebox_override("panel", box)
		UI.place(bar, Rect2(centre - size * 0.5, size))
		b.get_node("Content").add_child(bar)

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
	ButtonKit.price(_purchase_contents, BUY_SIZE, total)

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
		"houses", "confirm_nest", "confirm_nest_short":
			p.coins = 50270   # a long counter: the title sign must stay clear of it
			if state == "confirm_nest_short":
				p.coins = 20
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
		"confirm_nest", "confirm_nest_short":
			view._confirm(Shop.item("house.pinecone"))
