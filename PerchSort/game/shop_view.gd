extends Control
## Shop (docs/design/ui-system.md): top bar (close, title, coins), three tabs, a two-column grid of cream cards and
## a purchase confirm popup. Words are localized painted labels and numbers use the shared painted glyphs.

signal closed
signal profile_changed

const TABS := [Shop.BOOSTER, Shop.OUTFIT, Shop.HOUSE]
const TAB_ICONS := ["bulb", "hanger", "aviary"]
const TAB_NAMES := ["Boosters", "Outfits", "Houses"]
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
const CARD_H := {Shop.BOOSTER: 448.0, Shop.OUTFIT: 608.0, Shop.HOUSE: 608.0}
const CARD_RADIUS := 40.0
const CARD_EDGE := 10.0
const CARD_PAD := 32.0
const NAME_CAP := 40.0
const PIC_TOP := 120.0
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
	UI.heading(_bar, "shop.title", Rect2(420, BAR_Y - 52.0, 240, 104), TITLE_CAP)

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
			c.queue_free()
	var x := 540.0 - (TABS.size() - 1) * (UI.SECONDARY + TAB_GAP) * 0.5
	for i in TABS.size():
		var index := i
		UI.button(_tabs, TAB_NAMES[i], Vector2(x + i * (UI.SECONDARY + TAB_GAP), TAB_Y), "blue" if i == tab else "cream",
			TAB_ICONS[i], func(): _select_tab(index))
	var items := _items(TABS[tab])
	var pages := ceili(items.size() / float(PER_PAGE)) if TABS[tab] != Shop.BOOSTER else 1
	page = clampi(page, 0, pages - 1)
	var shown := items if TABS[tab] == Shop.BOOSTER else items.slice(page * PER_PAGE, page * PER_PAGE + PER_PAGE)
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

func _select_tab(i: int) -> void:
	if i != tab:
		tab = i
		page = 0
		_refresh()

## Single boosters; outfits by tier then species so the first page shows one tier-1 outfit per species.
func _items(kind: int) -> Array:
	var out := []
	for it in Shop.catalog():
		if it["kind"] == kind and (kind != Shop.BOOSTER or it["quantity"] == 1):
			out.append(it)
	if kind == Shop.OUTFIT:
		out = [1, 2, 3].reduce(func(acc, tier): return acc + out.filter(func(it): return it["index"] == tier), [])
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
	UI.label(body, _name_id(it), Rect2(CARD_PAD, 24, cs.x - CARD_PAD * 2.0, 80), NAME_CAP)
	var slot := Rect2(Vector2((cs.x - PILL.x) * 0.5, cs.y - PILL_BOTTOM - PILL.y), PILL)
	var pic := Rect2(CARD_PAD, PIC_TOP, cs.x - CARD_PAD * 2.0, slot.position.y - 24.0 - PIC_TOP)
	_picture(body, it, pic, n)
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
	if not instant:
		card.modulate.a = 0.0
		card.scale = Vector2.ONE * 0.94
		var tw := card.create_tween().set_parallel()
		tw.tween_property(card, "modulate:a", 1.0, 0.18).set_delay(n * 0.04)
		tw.tween_property(card, "scale", Vector2.ONE, 0.26).set_delay(n * 0.04).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)

## Item picture: booster illustration, the bird wearing the outfit (live), or the house illustration.
func _picture(parent: Node, it: Dictionary, box: Rect2, n: int) -> void:
	match it["kind"]:
		Shop.BOOSTER:
			var side := minf(box.size.x, box.size.y)
			UI.art(parent, "booster_%s_illus" % BOOSTER_ILLUS[it["booster"]], Rect2(box.get_center() - Vector2.ONE * side * 0.5, Vector2.ONE * side))
		Shop.OUTFIT:
			_bird(parent, it["species"], "%s_outfit_%d" % [it["species"], it["index"]], box.grow(-16.0), n * 0.6)
		_:
			UI.art(parent, "house_style_%s_body" % it["style"], box.grow(-8.0))

## Pill-shaped button in a card slot; a press sinks it by its edge.
func _pill_button(parent: Node, n: String, r: Rect2, kind: String, on_press: Callable) -> Button:
	var b := Button.new()
	b.name = n
	b.focus_mode = Control.FOCUS_NONE
	var up := UI._box(UI.FILL[kind], UI.RIM[kind], r.size.y * 0.5, PILL_EDGE)
	var down := UI._box(UI.FILL[kind], UI.RIM[kind], r.size.y * 0.5, 3.0)
	down.expand_margin_top = -5.0
	for state in ["normal", "hover", "disabled", "focus"]:
		b.add_theme_stylebox_override(state, up)
	b.add_theme_stylebox_override("pressed", down)
	b.add_theme_stylebox_override("hover_pressed", down)
	UI.place(b, r)
	b.button_down.connect(func():
		for c in b.get_children():
			c.position.y += 5.0)
	b.button_up.connect(func():
		for c in b.get_children():
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
	var heights := [picture_size, UI.SECONDARY, BUY_SIZE.y] if repeatable else [picture_size, BUY_SIZE.y]
	_modal = Modal.open(self, _name_id(it), heights, true, 0.0, 0.0, instant)
	var c := _modal.content
	var ys: Array = _modal.get_meta("ys")
	var pic := Rect2(540.0 - picture_size * 0.5, ys[0], picture_size, picture_size)
	_picture(c, it, pic, 0)
	if repeatable:
		_quantity_row(c, ys[1])
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
		"outfits":
			p.coins = 1240
			p.outfits = {"outfit.sky.1": true, "outfit.mint.1": true}
			p.equipped = {"sky": "outfit.sky.1"}
			start = 1
		"houses":
			p.coins = 1240
			p.houses = {"teapot": true, "acorn": true}
			p.equipped_house = "teapot"
			start = 2
		"confirm_outfit":
			p.coins = 1240
			start = 1
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
		"confirm_outfit":
			view._confirm(Shop.item("outfit.berry.1"))
		"confirm_multiple", "confirm_maximum":
			view._confirm(Shop.item("booster.owl_eye"))
			view._quantity_input.set_value(5 if state == "confirm_multiple" else view._quantity_limit)
