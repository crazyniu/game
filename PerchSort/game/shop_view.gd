extends Control
## Shop (docs/design/ui-system.md): top bar (close, title, coins), three painted picture tabs, a two-column grid of
## cream cards and a purchase confirm popup. Boosters carry a one-line caption of what they do. The outfits tab is a
## grid of bird kinds; a met kind opens that bird's outfit page (the aviary's detail page) above the shop. Words are
## localized painted labels and numbers use the shared painted glyphs.

signal closed
signal profile_changed

const AviaryScript := preload("res://game/aviary_view.gd")
const TABS := [Shop.BOOSTER, Shop.OUTFIT, Shop.HOUSE]
const TAB_ART := ["tab_boosters", "tab_outfits", "tab_houses"]   # full-colour pictures on the round tab
const TAB_NAMES := ["Boosters", "Outfits", "Houses"]
const TAB_PICTURE := 96.0
const PER_PAGE := 4
const BOOSTER_LABEL := ["firefly", "sapling", "owl_eye", "morning_bell", "scarecrow", "spring_rain"]
const BOOSTER_ILLUS := ["hint", "sapling", "owl_eye", "bell", "scarecrow", "rain"]

## Layout (design pixels, 1080x1920; margins 48, 8 px grid).
const MARGIN := 48.0
const BAR_Y := 112.0
const TITLE_CAP := 56.0
const TAB_Y := 272.0
const TAB_GAP := 48.0
const GRID_TOP := 384.0
const GAP := 32.0
const CARD_W := (1080.0 - MARGIN * 2.0 - GAP) * 0.5
const CARD_H := {Shop.BOOSTER: 472.0, Shop.HOUSE: 608.0}
const CARD_RADIUS := 40.0
const CARD_EDGE := 10.0
const CARD_PAD := 32.0
const NAME_CAP := 40.0
const PIC_TOP := 120.0
const CAPTION := Rect2(24, 88, 428, 48)   # booster card: what it does, under the name
const CAPTION_CAP := 28.0
const BOOSTER_PIC_TOP := 148.0
const POPUP_CAPTION := 72.0               # booster buy popup: the same caption under the title
const POPUP_CAPTION_CAP := 44.0
## Outfits tab: three columns of bird kinds (every species with outfits), all on one page.
const KIND_COLUMNS := 3
const KIND_GAP := 24.0
const KIND_W := (1080.0 - MARGIN * 2.0 - KIND_GAP * (KIND_COLUMNS - 1)) / KIND_COLUMNS
const KIND_H := 456.0
const KIND_BIRD := Rect2(28, 28, 256, 256)
const KIND_NAME := Rect2(16, 292, 280, 72)
const KIND_NAME_CAP := 44.0
const KIND_COUNT_Y := 404.0               # centre of the hanger + owned/total row
const KIND_COUNT_CAP := 40.0
const KIND_HANGER := 52.0
const PILL := Vector2(240, 96)            # price, equip button and equipped marker share this slot
const PILL_BOTTOM := 40.0                 # card bottom (with its edge) to the pill bottom
const PILL_EDGE := 8.0
const PRICE_COIN := 64.0
const PRICE_CAP := 44.0
const MARK := 96.0                        # equipped check badge (flat, no shadow: a state, not a button)
const LOCK := 96.0
const PAGER_Y := 1792.0                   # bottom action row centre
const PAGE_PILL := Vector2(176, 96)
const POPUP_PIC := 240.0
const SINGLE_PIC := 320.0
const BUY_SIZE := Vector2(512, 128)
const QUANTITY_WIDTH := 208.0
const QUANTITY_GAP := 24.0
const WASH := Color(1.0, 0.969, 0.91, 0.42)   # cream veil over the scene so cards read

var profile: Profile
var tab := 0
var page := 0
var instant := false   # captures skip the pop-in motion
var _bar: Control
var _tabs: Control
var _grid: Control
var _coins: Panel
var _coins_shown := -1
var _modal: Modal
var _purchase_item: Dictionary
var _quantity := 1
var _quantity_limit := 1
var _quantity_input: QuantityInput
var _minus: Button
var _plus: Button
var _purchase_button: Button
var _purchase_contents: Control
var _outfit_page: Control   # a bird's outfit page opened from the outfits tab


## LineEdit handles keyboard input; only the existing painted digits are visible.
class QuantityInput extends LineEdit:
	signal value_changed(value: int)

	const CAP := 44.0
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
		_digits = UI.number(self, text, Rect2(16, 38, size.x - 32, CAP))
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
	UI.image(self, "board_w1_bg", Rect2(0, 0, 1080, 1920), false)
	var wash := ColorRect.new()
	wash.name = "Wash"
	wash.color = WASH
	wash.size = size
	wash.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(wash)
	_grid = _layer("Grid")
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
	UI.button(_bar, "Close", Vector2(112, BAR_Y), "red", "close", func(): closed.emit())
	UI.title_ribbon(_bar, "shop.title", Vector2(540, BAR_Y), 52.0, 460.0)

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
	var x := 540.0 - (TABS.size() - 1) * (UI.SECONDARY + TAB_GAP) * 0.5
	for i in TABS.size():
		_tab_button(i, Vector2(x + i * (UI.SECONDARY + TAB_GAP), TAB_Y))
	if TABS[tab] == Shop.OUTFIT:
		_kind_cards()
		return
	var items := _items(TABS[tab])
	var pages := ceili(items.size() / float(PER_PAGE)) if TABS[tab] == Shop.HOUSE else 1
	page = clampi(page, 0, pages - 1)
	var shown := items if pages == 1 else items.slice(page * PER_PAGE, page * PER_PAGE + PER_PAGE)
	for n in shown.size():
		_card(shown[n], n)
	if pages > 1:
		_build_pager(pages)

## The compact counter stays at the right margin; the title has its own bounded space.
func _build_coins() -> void:
	if _coins_shown == profile.coins:
		return
	var bump := _coins_shown >= 0
	if _coins:
		_coins.queue_free()
	var value := str(profile.coins)
	var w := UI.NUMBER_X + UI.number_width(value, 44.0) + 32.0
	_coins = UI.counter(_bar, Vector2(1080.0 - MARGIN - w, BAR_Y - UI.COUNTER_H * 0.5), "coin", value)
	_coins.name = "Coins"
	_coins.pivot_offset = _coins.size * 0.5
	_coins_shown = profile.coins
	if bump and not instant:
		var tw := _coins.create_tween()
		tw.tween_property(_coins, "scale", Vector2.ONE * 1.1, 0.1).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
		tw.tween_property(_coins, "scale", Vector2.ONE, 0.2).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)

## Round tab with its painted picture: blue behind the selected tab, cream behind the others; same size in every state.
func _tab_button(i: int, center: Vector2) -> Button:
	var b := UI.button(_tabs, TAB_NAMES[i], center, "blue" if i == tab else "cream", "", func(): _select_tab(i))
	var side := UI.SECONDARY
	var pic := UI.art(b, TAB_ART[i], Rect2(Vector2(side - TAB_PICTURE, side - TAB_PICTURE - 10.0) * 0.5, Vector2.ONE * TAB_PICTURE))
	pic.name = "Picture"
	b.button_down.connect(func(): pic.position.y += 6.0)
	b.button_up.connect(func(): pic.position.y -= 6.0)
	return b

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

func _build_pager(pages: int) -> void:
	var half := PAGE_PILL.x * 0.5 + GAP + UI.SECONDARY * 0.5
	var prev := UI.button(_grid, "Previous", Vector2(540.0 - half, PAGER_Y), "cream", "left", func(): _turn(-1))
	UI.set_active(prev, page > 0)
	var pill := UI.shape(_grid, Rect2(Vector2(540, PAGER_Y) - PAGE_PILL * 0.5, PAGE_PILL), "cream", PAGE_PILL.y * 0.5, PILL_EDGE)
	pill.name = "Page"
	UI.number(pill, "%d/%d" % [page + 1, pages], Rect2(16, (PAGE_PILL.y - PILL_EDGE - 44.0) * 0.5, PAGE_PILL.x - 32.0, 44.0))
	var next := UI.button(_grid, "Next", Vector2(540.0 + half, PAGER_Y), "cream", "right", func(): _turn(1))
	UI.set_active(next, page < pages - 1)

func _turn(step: int) -> void:
	page += step
	_refresh()


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

func _card(it: Dictionary, n: int) -> void:
	var kind: int = it["kind"]
	var cs := Vector2(CARD_W, CARD_H[kind])
	var card := Control.new()
	card.name = it["id"]
	card.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(card, Rect2(Vector2(MARGIN + (n % 2) * (CARD_W + GAP), GRID_TOP + (n / 2) * (cs.y + GAP)), cs))
	_grid.add_child(card)
	UI.shape(card, Rect2(Vector2.ZERO, cs), "cream", CARD_RADIUS, CARD_EDGE)
	var hit := UI.hit_area(card, Rect2(Vector2.ZERO, cs), func(): _tap(it))
	hit.disabled = _locked(it)
	var body := Control.new()   # everything that dims when the card is locked
	body.name = "Body"
	body.size = cs
	body.mouse_filter = Control.MOUSE_FILTER_IGNORE
	card.add_child(body)
	UI.label(body, _name_id(it), Rect2(CARD_PAD, 24, cs.x - CARD_PAD * 2.0, 80 if kind != Shop.BOOSTER else 64), NAME_CAP)
	var top := PIC_TOP
	if kind == Shop.BOOSTER:
		pass   # what a booster does is told in its buy popup; cards carry the name only
		top = BOOSTER_PIC_TOP
	var slot := Rect2(Vector2((cs.x - PILL.x) * 0.5, cs.y - PILL_BOTTOM - PILL.y), PILL)
	var pic := Rect2(CARD_PAD, top, cs.x - CARD_PAD * 2.0, slot.position.y - 24.0 - top)
	_picture(body, it, pic)
	if _equipped(it):
		var mark := UI.shape(body, Rect2(slot.get_center() - Vector2.ONE * MARK * 0.5, Vector2.ONE * MARK), "green", MARK * 0.5, 6.0, false)
		mark.name = "Equipped"
		UI.icon(mark, "check", Vector2.ONE * MARK * 0.5 - Vector2(0, 3), MARK * 0.56)
	elif _owned(it):
		var equip := _pill_button(body, "Equip", slot, "green", func(): _tap(it))
		UI.icon(equip, "hanger", PILL * 0.5 - Vector2(0, PILL_EDGE * 0.5), 64.0).name = "Icon"
	else:
		var buy := _pill_button(body, "Price", slot, "orange", func(): _tap(it))
		_price_parts(buy, it["price"])
		buy.disabled = _locked(it)   # the whole card dims below
	if kind == Shop.BOOSTER and profile.inventory[it["booster"]] > 0:
		var d := UI.BADGE
		var at := Vector2(pic.get_center().x + pic.size.y * 0.42, pic.position.y + d * 0.5)
		var badge := UI.shape(body, Rect2(at - Vector2.ONE * d * 0.5, Vector2.ONE * d), "red", d * 0.5, 5.0, false)
		badge.name = "Stock"
		UI.number(badge, str(profile.inventory[it["booster"]]), Rect2(8, d * 0.5 - 18.0, d - 16.0, 30), Color.WHITE)
	if _locked(it):
		body.modulate.a = UI.DISABLED_ALPHA
		UI.icon(card, "lock", pic.get_center(), LOCK, UI.INK).name = "Lock"
	_pop_in(card, n)

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

## Item picture: booster illustration or house illustration (outfits are shown on the bird's own page).
func _picture(parent: Node, it: Dictionary, box: Rect2) -> void:
	if it["kind"] == Shop.BOOSTER:
		var side := minf(box.size.x, box.size.y)
		UI.art(parent, "booster_%s_illus" % BOOSTER_ILLUS[it["booster"]], Rect2(box.get_center() - Vector2.ONE * side * 0.5, Vector2.ONE * side))
	else:
		UI.art(parent, "house_style_%s_body" % it["style"], box.grow(-8.0))

## Pill-shaped button in a card slot; a press sinks it by its edge.
func _pill_button(parent: Node, n: String, r: Rect2, kind: String, on_press: Callable) -> Button:
	var b := Button.new()
	b.name = n
	b.focus_mode = Control.FOCUS_NONE
	# Painted kit faces: square buttons are discs (cream = the white disc with the leaf ring), others pills.
	var square := absf(r.size.x - r.size.y) < 2.0
	var piece := ("disc_ring" if kind == "cream" else "disc_" + kind) if square \
		else ("capsule_plain" if kind == "cream" else "pill_" + kind)
	UI._skin_button(b, piece, Rect2(Vector2.ZERO, r.size))
	UI.place(b, r)
	b.button_down.connect(func():
		for c in b.get_children():
			if c.name != "Face":
				c.position.y += 5.0)
	b.button_up.connect(func():
		for c in b.get_children():
			if c.name != "Face":
				c.position.y -= 5.0)
	b.pressed.connect(func():
		Sound.play("ui_tap")
		on_press.call())
	parent.add_child(b)
	return b

## Coin and white price, centred together inside the orange pill.
func _price_parts(pill: Control, price: int) -> void:
	var text := str(price)
	var num_w := UI.number_width(text, PRICE_CAP)
	var x := (PILL.x - (PRICE_COIN + 12.0 + num_w)) * 0.5
	var mid := (PILL.y - PILL_EDGE) * 0.5
	UI.art(pill, "coin", Rect2(x, mid - PRICE_COIN * 0.5, PRICE_COIN, PRICE_COIN))
	UI.number(pill, text, Rect2(x + PRICE_COIN + 12.0, mid - PRICE_CAP * 0.5, num_w + 2.0, PRICE_CAP), Color.WHITE, -1)

## Live BirdView wearing `outfit`, its front frame fitted in the box with the feet on the box bottom.
func _bird(parent: Node, species: String, outfit: String, box: Rect2, phase: float) -> BirdView:
	var front := outfit + "_front" if Art.has(outfit + "_front") else species + "_front"
	var used := UI.used_rect(front)
	var k := minf(box.size.x / used.size.x, box.size.y / used.size.y)
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
	for index in range(1, 4):
		if profile.owns_outfit("outfit.%s.%d" % [species, index]):
			owned += 1
	return owned

func _kind_cards() -> void:
	var kinds := outfit_species()
	for n in kinds.size():
		var at := Vector2(MARGIN + (n % KIND_COLUMNS) * (KIND_W + KIND_GAP), GRID_TOP + (n / KIND_COLUMNS) * (KIND_H + KIND_GAP))
		_kind_card(kinds[n], Rect2(at, Vector2(KIND_W, KIND_H)), n)

## A met kind: the bird in its current outfit, its name and owned/total outfits; tapping opens its outfit page.
## An unmet kind: a breathing silhouette with a question mark; a tap only squashes the card.
func _kind_card(species: String, r: Rect2, n: int) -> void:
	var card := UI.shape(_grid, r, "cream", CARD_RADIUS, CARD_EDGE)
	card.name = "Kind_" + species
	var met := profile.has_met(species)
	if met:
		var current: String = profile.equipped.get(species, "")
		var outfit := "" if current.is_empty() else "%s_outfit_%s" % [species, current.get_slice(".", 2)]
		_bird(card, species, outfit, KIND_BIRD, n * 0.45)
		UI.label(card, "bird.%s.name" % species, KIND_NAME, KIND_NAME_CAP).name = "Name"
		var text := "%d/4" % owned_outfits(species)
		var num_w := UI.number_width(text, KIND_COUNT_CAP)
		var x := (KIND_W - (KIND_HANGER + 12.0 + num_w)) * 0.5
		UI.icon(card, "hanger", Vector2(x + KIND_HANGER * 0.5, KIND_COUNT_Y), KIND_HANGER, UI.INK).name = "Hanger"
		UI.number(card, text, Rect2(x + KIND_HANGER + 12.0, KIND_COUNT_Y - KIND_COUNT_CAP * 0.5, num_w + 2.0, KIND_COUNT_CAP),
			UI.INK, -1).name = "Owned"
	else:
		AviaryScript._silhouette(card, species, KIND_BIRD)
		var q := UI.number(card, "?", Rect2(KIND_BIRD.get_center().x - 60.0, KIND_BIRD.get_center().y - 72.0, 120.0, 160.0),
			Color.WHITE)
		q.name = "Question"
	var hit := UI.hit_area(card, Rect2(Vector2.ZERO, r.size), func(): _tap_kind(species, card))
	hit.name = "Open" if met else "Unmet"
	_pop_in(card, n)

func _tap_kind(species: String, card: Control) -> void:
	card.pivot_offset = card.size * 0.5
	var tw := card.create_tween()
	tw.tween_property(card, "scale", Vector2.ONE * 0.95, 0.06).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
	tw.tween_property(card, "scale", Vector2.ONE, 0.2).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	if not profile.has_met(species) or _outfit_page != null:
		return
	Sound.play("ui_tap")
	if instant:
		open_outfits(species)
	else:
		get_tree().create_timer(0.12).timeout.connect(func(): if _outfit_page == null: open_outfits(species))

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
	_picture(c, it, pic)
	if repeatable:
		_quantity_row(c, ys[row + 1])
		_modal.closed.connect(_quantity_input.release_focus)
	_purchase_button = _pill_button(c, "Buy", Rect2(Vector2(540.0 - BUY_SIZE.x * 0.5, ys[-1]), BUY_SIZE), "orange",
		func(): _modal.invoke(func(): _buy(it, _quantity)))
	_purchase_contents = Control.new()
	_purchase_contents.name = "Contents"
	_purchase_contents.size = BUY_SIZE
	_purchase_contents.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_purchase_button.add_child(_purchase_contents)
	_update_purchase()

func _quantity_row(c: Control, y: float) -> void:
	var x := 540.0 - BUY_SIZE.x * 0.5
	_minus = _pill_button(c, "Decrease", Rect2(x, y, UI.SECONDARY, UI.SECONDARY), "cream", func(): _step_quantity(-1))
	UI.number(_minus, "-", Rect2(0, 38, UI.SECONDARY, PRICE_CAP))
	_quantity_input = QuantityInput.new()
	_quantity_input.name = "Quantity"
	_quantity_input.limit = _quantity_limit
	_quantity_input.editable = profile.coins >= _purchase_item["price"]
	UI.place(_quantity_input, Rect2(x + UI.SECONDARY + QUANTITY_GAP, y, QUANTITY_WIDTH, UI.SECONDARY))
	c.add_child(_quantity_input)
	_quantity_input.value_changed.connect(func(value: int):
		_quantity = value
		_update_purchase())
	_plus = _pill_button(c, "Increase", Rect2(x + BUY_SIZE.x - UI.SECONDARY, y, UI.SECONDARY, UI.SECONDARY), "cream",
		func(): _step_quantity(1))
	UI.number(_plus, "+", Rect2(0, 38, UI.SECONDARY, PRICE_CAP))

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
	var word_w := UI.label_width("shop.buy", PRICE_CAP)
	var num_w := UI.number_width(str(total), PRICE_CAP)
	var x := (BUY_SIZE.x - word_w - 24.0 - PRICE_COIN - 12.0 - num_w) * 0.5
	var mid := (BUY_SIZE.y - PILL_EDGE) * 0.5
	UI.label(_purchase_contents, "shop.buy", Rect2(x, 0, word_w, BUY_SIZE.y - PILL_EDGE), PRICE_CAP, Color.WHITE)
	x += word_w + 24.0
	UI.art(_purchase_contents, "coin", Rect2(x, mid - PRICE_COIN * 0.5, PRICE_COIN, PRICE_COIN))
	UI.number(_purchase_contents, str(total), Rect2(x + PRICE_COIN + 12.0, mid - PRICE_CAP * 0.5, num_w + 2.0, PRICE_CAP), Color.WHITE, -1)

func _buy(it: Dictionary, quantity: int) -> void:
	if _quantity_input != null:
		_quantity_input.release_focus()
	var err := Shop.purchase(profile, it["id"], quantity)
	if err != "":
		Sound.play("error")
		return
	Sound.play("collect")
	profile_changed.emit()
	_refresh()


# ---------------------------------------------------------------- capture preview

static func preview(main: Control, state: String) -> void:
	var p := Profile.new()
	p.coins = 240
	p.highest_unlocked = 40
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
			p.coins = 1240
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
