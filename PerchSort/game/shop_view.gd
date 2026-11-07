extends Control
## Shop (docs/design/ui-system.md): top bar (close, title, coins), three tabs, a two-column grid of cream cards and
## a purchase confirm popup. Words are painted word sprites, numbers painted glyphs, icons the white icon family.

signal closed
signal profile_changed

const TABS := [Shop.BOOSTER, Shop.OUTFIT, Shop.HOUSE]
const TAB_ICONS := ["bulb", "hanger", "aviary"]
const TAB_NAMES := ["Boosters", "Outfits", "Houses"]
const PER_PAGE := 4
const BOOSTER_NAME := ["Firefly", "Sapling", "Owl Eye", "Morning Bell", "Scarecrow", "Spring Rain"]
const BOOSTER_ILLUS := ["hint", "sapling", "owl_eye", "bell", "scarecrow", "rain"]
const HOUSE_NAME := {"mushroom": "Mushroom", "teapot": "Teapot", "lantern": "Lantern", "acorn": "Acorn"}
const OUTFIT_NAME := {
	"sunny": ["Straw Hat", "Blue Bow Tie", "Little Crown"], "sky": ["Cozy Scarf", "Aviator Goggles", "Tiny Top Hat"],
	"berry": ["Round Glasses", "Pink Bow", "Sparkle Tiara"], "mint": ["Flower", "Mushroom Cap", "Acorn Cap"],
	"cocoa": ["Red Beanie", "Plaid Bow Tie", "Monocle"], "snow": ["Earmuffs", "Snowflake Clip", "Ice Tiara"],
	"ember": ["Red Bandana", "Flame Headband", "Knight Helmet"], "ink": ["Star Nightcap", "Silver Glasses", "Little Star Hat"],
	"hoot": ["Graduation Cap", "Reading Glasses", "Wizard Cape"]}

## Layout (design pixels, 1080x1920; margins 48, 8 px grid).
const MARGIN := 48.0
const BAR_Y := 112.0                      # centre of the top bar row (y 48 to 176)
const TITLE_CAP := 64.0
const TAB_Y := 272.0                      # centre of the tab row (y 208 to 336)
const TAB_GAP := 48.0
const GRID_TOP := 384.0
const GAP := 32.0
const CARD_W := (1080.0 - MARGIN * 2.0 - GAP) * 0.5
const CARD_H := {Shop.BOOSTER: 456.0, Shop.OUTFIT: 640.0, Shop.HOUSE: 640.0}
const CARD_RADIUS := 40.0
const CARD_EDGE := 10.0
const CARD_PAD := 32.0
const NAME_CAP := 40.0
const NAME_Y := 64.0                      # centre of the item name inside a card
const PIC_TOP := 104.0
const PILL := Vector2(240, 96)            # price, equip button and equipped marker share this slot
const PILL_BOTTOM := 40.0                 # card bottom (with its edge) to the pill bottom
const PILL_EDGE := 8.0
const PRICE_COIN := 64.0
const PRICE_CAP := 44.0
const MARK := 96.0                        # equipped check badge (flat, no shadow: a state, not a button)
const LOCK := 96.0
const PAGER_Y := 1792.0                   # bottom action row centre
const PAGE_PILL := Vector2(176, 96)
const POPUP_PIC := 320.0
const POPUP_LINE := 72.0
const POPUP_COIN := 72.0
const POPUP_CAP := 52.0
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
	UI.button(_bar, "Close", Vector2(MARGIN + UI.SECONDARY * 0.5, BAR_Y), "red", "close", func(): closed.emit())
	UI.word(_bar, "Shop", Vector2(540, BAR_Y), TITLE_CAP)
	_refresh()

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

## Coins counter at the top right; it bumps when the amount changes.
func _build_coins() -> void:
	if _coins_shown == profile.coins:
		return
	var bump := _coins_shown >= 0
	if _coins:
		_coins.queue_free()
	var value := str(profile.coins)
	var w := 100.0 + UI.number_width(value, 44.0) + 32.0
	_coins = UI.counter(_bar, Vector2(1080.0 - MARGIN - w, BAR_Y - UI.COUNTER_H * 0.5), "coin", value)
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

func _name(it: Dictionary) -> String:
	match it["kind"]:
		Shop.BOOSTER: return BOOSTER_NAME[it["booster"]]
		Shop.OUTFIT: return OUTFIT_NAME[it["species"]][it["index"] - 1]
	return HOUSE_NAME[it["style"]]

func _card(it: Dictionary, n: int) -> void:
	var kind: int = it["kind"]
	var cs := Vector2(CARD_W, CARD_H[kind])
	var card := Control.new()
	card.name = it["id"]
	card.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(card, Rect2(Vector2(MARGIN + (n % 2) * (CARD_W + GAP), GRID_TOP + (n / 2) * (cs.y + GAP)), cs))
	_grid.add_child(card)
	UI.shape(card, Rect2(Vector2.ZERO, cs), "cream", CARD_RADIUS, CARD_EDGE)
	UI.hit_area(card, Rect2(Vector2.ZERO, cs), func(): _tap(it))
	var body := Control.new()   # everything that dims when the card is locked
	body.name = "Body"
	body.size = cs
	body.mouse_filter = Control.MOUSE_FILTER_IGNORE
	card.add_child(body)
	_title(body, _name(it), Vector2(cs.x * 0.5, NAME_Y), NAME_CAP, cs.x - CARD_PAD * 2.0)
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

## Painted name centred on `at`, cap height shrunk if the phrase is wider than `max_w`.
func _title(parent: Node, text: String, at: Vector2, cap: float, max_w: float) -> void:
	var w := UI.word_width(text, cap)
	if w > max_w:
		cap *= max_w / w
	UI.word(parent, text, at, cap)

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

## Coin and white price, centred together inside the pill.
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
		# Breathing and blinks run inside BirdView; a cheer and a hop alternate on top of them.
		var rest := bird.position
		var tw := bird.create_tween().set_loops()
		tw.tween_interval(1.4 + phase)
		tw.tween_callback(bird.cheer)
		tw.tween_interval(2.2)
		tw.tween_property(bird, "position:y", rest.y - 22.0, 0.16).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
		tw.tween_property(bird, "position:y", rest.y, 0.16).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_IN)
		tw.tween_property(bird, "position:y", rest.y - 12.0, 0.12).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
		tw.tween_property(bird, "position:y", rest.y, 0.12).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_IN)
		tw.tween_interval(1.6)
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

## Confirm popup: the item name as title, its picture, the price line and a green check under the panel.
## Without enough coins the check is dimmed and the price shakes once.
func _confirm(it: Dictionary) -> void:
	_modal = Modal.open(self, _name(it), [POPUP_PIC, POPUP_LINE], true, UI.PRIMARY, 0.0, instant)
	var c := _modal.content
	var ys: Array = _modal.get_meta("ys")
	var pic := Rect2(540.0 - POPUP_PIC * 0.5, ys[0], POPUP_PIC, POPUP_PIC)
	_picture(c, it, pic, 0)
	var affordable: bool = profile.coins >= it["price"]
	var line := _price_line(c, ys[1], it["price"], UI.INK if affordable else UI.FILL["red"])
	var accept: Button = _modal.actions([["green", "check", func(): _buy(it)]])[0]
	UI.set_active(accept, affordable)
	if not affordable:
		var tw := line.create_tween()
		tw.tween_interval(0.0 if instant else 0.35)
		tw.tween_callback(func(): Sound.play("error"))
		for dx in [16.0, -16.0, 10.0, -10.0, 4.0, 0.0]:
			tw.tween_property(line, "position:x", dx, 0.05)

## Coin and price centred on one line; returns the holder that shakes.
static func _price_line(c: Control, y: float, price: int, color: Color) -> Control:
	var holder := Control.new()
	holder.name = "PriceLine"
	holder.size = Vector2(1080, 1920)
	holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	c.add_child(holder)
	var text := str(price)
	var num_w := UI.number_width(text, POPUP_CAP)
	var x := 540.0 - (POPUP_COIN + 16.0 + num_w) * 0.5
	UI.art(holder, "coin", Rect2(x, y + (POPUP_LINE - POPUP_COIN) * 0.5, POPUP_COIN, POPUP_COIN))
	UI.number(holder, text, Rect2(x + POPUP_COIN + 16.0, y + (POPUP_LINE - POPUP_CAP) * 0.5, num_w + 2.0, POPUP_CAP), color, -1)
	return holder

func _buy(it: Dictionary) -> void:
	var err := Shop.purchase(profile, it["id"])
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
		"locked_tools":
			p.highest_unlocked = 3
			p.inventory = PackedInt32Array([2, 0, 0, 0, 0, 0])
		"insufficient":
			p.coins = 20
	var view: Control = load("res://game/shop_view.gd").new()
	view.instant = main.instant()
	view.setup(p, start)
	main.set_screen(view)
	match state:
		"confirm_booster", "insufficient":
			view._confirm(Shop.item("booster.firefly"))
		"confirm_outfit":
			view._confirm(Shop.item("outfit.berry.1"))
