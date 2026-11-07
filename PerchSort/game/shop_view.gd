extends Control
## Shop: boosters, outfits and houses in a two-column card grid, with a purchase confirm popup.

signal closed
signal profile_changed

const TABS := [Shop.BOOSTER, Shop.OUTFIT, Shop.HOUSE]
const TAB_IDS := ["tab_boosters", "tab_outfits", "tab_home"]
const TAB_ICONS := ["icon_bulb", "icon_hanger", "icon_home"]
const BirdViewScript := preload("res://game/bird_view.gd")
const PER_PAGE := 4
const BOOSTER_KEYS := ["hint", "branch", "reveal", "bell", "shoo", "rain"]
const TOOL_ART := ["tool_hint", "tool_branch", "tool_owl_eye", "tool_bell", "tool_scarecrow", "tool_rain"]
# Booster cards use the same names as their unlock and confirm titles.
const BOOSTER_LABEL := ["label_booster_firefly", "label_booster_sapling", "label_booster_owl_eye", "label_booster_morning_bell",
	"label_booster_scarecrow", "label_booster_spring_rain"]
const HOUSE_LABEL := {"mushroom": "label_card_mushroom", "teapot": "label_card_teapot", "lantern": "label_card_lantern",
	"acorn": "label_card_acorn"}
const OUTFIT_LABEL := {
	"sunny": ["straw_hat", "blue_bow_tie", "little_crown"], "sky": ["cozy_scarf", "aviator_goggles", "tiny_top_hat"],
	"berry": ["round_glasses", "pink_bow", "sparkle_tiara"], "mint": ["flower", "mushroom_cap", "acorn_cap"],
	"cocoa": ["red_beanie", "plaid_bow_tie", "monocle"], "snow": ["earmuffs", "snowflake_clip", "ice_tiara"],
	"ember": ["red_bandana", "flame_headband", "knight_helmet"], "ink": ["star_nightcap", "silver_glasses", "little_star_hat"],
	"hoot": ["graduation_cap", "reading_glasses", "wizard_cape"]}
## Layout constants (ui-system.md sections 1-5 and 8). Design space 1080x1920.
const MARGIN := 40.0
const GUTTER := 24.0
const CARD_W := 488.0
const CARD_PAD_IN := 24.0                 # inner padding of a card
const CARD_H := {Shop.BOOSTER: 456.0, Shop.OUTFIT: 672.0, Shop.HOUSE: 672.0}
const CARD_TOP := 344.0
const CARD_RIBBON_W := 380.0              # every card name ribbon has this width
const PILL_H := 80.0                      # pills inside cards and popups
const PRICE_PILL_W := 232.0
const COIN_IN_PILL := 64.0
const PRICE_CAP := 40.0
const DISC := 128.0
const DISC_ROW_Y := 168.0
const DISC_GAP := 64.0
const CLOSE_DISC := 112.0                 # same role and size as the board's gear disc
const HUD_PILL_H := 96.0
const COIN_ICON := 104.0
const RING_D := 224.0                     # booster medallion on a card
const PAGER_DISC := 96.0
const PAGER_Y := 1752.0
const POPUP_W := 720.0
const POPUP_RIBBON_W := 520.0
const POPUP_PIC := 320.0
const BORDER_SCALE := 0.55   # card frames use thinner borders than the nine-slice default
const CARD_PAD := Vector4(15, 48, 15, 4)    # transparent margins of card_wood (left, top, right, bottom)
## Solid pixels of each house sprite (its faint shadow would otherwise shrink the fitted picture).
const HOUSE_BODY := {"mushroom": Rect2(41, 51, 427, 402), "teapot": Rect2(29, 55, 459, 399),
	"lantern": Rect2(50, 61, 409, 394), "acorn": Rect2(56, 51, 398, 404)}
const BOOSTER_TITLE := ["label_title_booster_firefly", "label_title_booster_sapling", "label_title_booster_owl_eye",
	"label_title_booster_morning_bell", "label_title_booster_scarecrow", "label_title_booster_spring_rain"]
const MEDALLION := Color(0.62, 0.4, 0.3)

var profile: Profile
var tab := 0
var page := 0
var _page_node: Control
var _tab_node: Control
var _coin_number: Control
var _modal: Modal
var instant := false   # captures skip the modal pop-in


func setup(p: Profile, start_tab := 0) -> void:
	profile = p
	tab = start_tab
	size = Vector2(1080, 1920)
	UI.image(self, "bg_level_w1_noon", Rect2(0, 0, 1080, 1920), false)
	_build_header()
	_page_node = Control.new()
	_page_node.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(_page_node)
	_refresh()

## Coin pill (top-left, same as every screen) and the close disc (top-right, same size as the board's gear).
func _build_header() -> void:
	var digits := str(profile.coins).length()
	var number_w := 36.0 * maxi(digits, 3)
	var coin := Rect2(MARGIN, MARGIN + (HUD_PILL_H - COIN_ICON) * 0.5, COIN_ICON, COIN_ICON)
	UI.panel(self, "stat_pill", Rect2(coin.get_center().x, MARGIN, COIN_ICON * 0.5 + 24.0 + number_w + 28.0, HUD_PILL_H))
	UI.art(self, "coin", coin)
	_coin_number = UI.number(self, str(profile.coins), Rect2(coin.end.x + 24.0, MARGIN + 22.0, number_w, 52.0), UI.INK, -1)
	UI.disc(self, "Close", Rect2(1080.0 - MARGIN - CLOSE_DISC, MARGIN, CLOSE_DISC, CLOSE_DISC), "icon_close", func(): closed.emit(), "btn_wood", 0.5)
	_tab_node = Control.new()
	_tab_node.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(_tab_node)

func _refresh() -> void:
	UI.set_number(_coin_number, str(profile.coins))
	for node in [_page_node, _tab_node]:
		for c in node.get_children():
			c.queue_free()
	for i in TABS.size():
		var cx := 540.0 + (i - 1) * (DISC + DISC_GAP)
		var r := Rect2(cx - DISC * 0.5, DISC_ROW_Y, DISC, DISC)
		if i == tab:
			UI.art(_tab_node, "tab_selected_ring", Rect2(r.position + Vector2(0, 6), r.size).grow(18))
		var index := i
		UI.disc(_tab_node, TAB_IDS[i], r, TAB_ICONS[i], func(): _select_tab(index), "btn_wood", 0.5)
	match TABS[tab]:
		Shop.BOOSTER: _build_boosters()
		Shop.OUTFIT: _build_cosmetics(_items(Shop.OUTFIT))
		Shop.HOUSE: _build_cosmetics(_items(Shop.HOUSE))

func _select_tab(i: int) -> void:
	if i != tab:
		tab = i
		page = 0
		_refresh()

## Outfits are listed by tier then species so the first page shows one tier-1 outfit per species.
func _items(kind: int) -> Array:
	var out := []
	for it in Shop.catalog():
		if it["kind"] == kind:
			out.append(it)
	if kind == Shop.OUTFIT:
		out = [1, 2, 3].reduce(func(acc, tier): return acc + out.filter(func(it): return it["index"] == tier), [])
	return out

func _card_pos(kind: int, n: int) -> Vector2:
	var h: float = CARD_H[kind]
	return Vector2(MARGIN + (n % 2) * (CARD_W + GUTTER), CARD_TOP + (n / 2) * (h + GUTTER))

func _build_boosters() -> void:
	var singles := _items(Shop.BOOSTER).filter(func(it): return it["quantity"] == 1)
	for i in singles.size():
		_card(singles[i], _card_pos(Shop.BOOSTER, i), i)

func _build_cosmetics(items: Array) -> void:
	var kind: int = TABS[tab]
	var pages := ceili(items.size() / float(PER_PAGE))
	for n in mini(PER_PAGE, items.size() - page * PER_PAGE):
		_card(items[page * PER_PAGE + n], _card_pos(kind, n), n)
	if pages > 1:
		_build_pager(pages)

func _build_pager(pages: int) -> void:
	var pill := Rect2(540.0 - 90.0, PAGER_Y + (PAGER_DISC - PILL_H) * 0.5, 180.0, PILL_H)
	var prev := UI.disc(_page_node, "Previous", Rect2(pill.position.x - GUTTER - PAGER_DISC, PAGER_Y, PAGER_DISC, PAGER_DISC), "icon_next_arrow", func(): _turn(-1), "btn_wood", 0.5)
	prev.get_node("Icon").rotation = PI
	prev.get_node("Icon").pivot_offset = prev.get_node("Icon").size * 0.5
	UI.set_enabled(prev, page > 0)
	UI.panel(_page_node, "stat_pill", pill)
	UI.number(_page_node, "%d/%d" % [page + 1, pages], Rect2(pill.position.x + 24.0, pill.position.y + (PILL_H - PRICE_CAP) * 0.5, pill.size.x - 48.0, PRICE_CAP))
	var next := UI.disc(_page_node, "Next", Rect2(pill.end.x + GUTTER, PAGER_Y, PAGER_DISC, PAGER_DISC), "icon_next_arrow", func(): _turn(1), "btn_wood", 0.5)
	UI.set_enabled(next, page < pages - 1)

func _turn(step: int) -> void:
	page += step
	_refresh()

## ---------- cards ----------

func _owned(it: Dictionary) -> bool:
	return it["kind"] != Shop.BOOSTER and (profile.owns_outfit(it["id"]) if it["kind"] == Shop.OUTFIT else profile.owns_house(it["style"]))

func _equipped(it: Dictionary) -> bool:
	if it["kind"] == Shop.OUTFIT:
		return profile.equipped.get(it["species"], "") == it["id"]
	return it["kind"] == Shop.HOUSE and profile.equipped_house == it["style"]

func _locked(it: Dictionary) -> bool:
	return it["kind"] == Shop.BOOSTER and not Progress.is_booster_unlocked(profile, it["booster"])

func _label(it: Dictionary) -> String:
	match it["kind"]:
		Shop.BOOSTER: return BOOSTER_LABEL[it["booster"]]
		Shop.OUTFIT: return "label_card_" + OUTFIT_LABEL[it["species"]][it["index"] - 1]
	return HOUSE_LABEL[it["style"]]

## Painted ribbon (name included) fitted in a `w` wide box centred at `center`; heights follow the art.
func _ribbon(parent: Node, label: String, center: Vector2, w: float) -> Rect2:
	var used := UI.used_rect(label)
	var h := w * used.size.y / used.size.x
	var r := Rect2(center - Vector2(w, h) * 0.5, Vector2(w, h))
	UI.art(parent, label, r)
	return r

## Nine-slice frame whose visible body (not its transparent margins) fills `body`.
func _frame(parent: Node, name: String, body: Rect2, pad: Vector4) -> NinePatchRect:
	var k := BORDER_SCALE
	return UI.panel(parent, name, body.grow_individual(pad.x * k, pad.y * k, pad.z * k, pad.w * k), k)

## Sprite scaled so its visible pixels fit `area`, centered.
func _fit(parent: Node, name: String, area: Rect2, solid := Rect2()) -> TextureRect:
	return UI.art(parent, name, area, solid)

## Price pill: coin inside its left end, painted number centred in the rest.
func _price(parent: Node, center_x: float, y: float, price: int) -> Array:
	var pill := Rect2(center_x - PRICE_PILL_W * 0.5, y, PRICE_PILL_W, PILL_H)
	var panel := UI.panel(parent, "stat_pill", pill)
	panel.pivot_offset = pill.size * 0.5
	var coin := UI.art(parent, "coin", Rect2(pill.position.x + 16.0, y + (PILL_H - COIN_IN_PILL) * 0.5, COIN_IN_PILL, COIN_IN_PILL))
	var left := pill.position.x + 16.0 + COIN_IN_PILL + 12.0
	var amount := UI.number(parent, str(price), Rect2(left, y + (PILL_H - PRICE_CAP) * 0.5, pill.end.x - 24.0 - left, PRICE_CAP))
	return [panel, coin, amount]

func _card(it: Dictionary, at: Vector2, n: int) -> void:
	var kind: int = it["kind"]
	var cs := Vector2(CARD_W, CARD_H[kind])
	var card := Control.new()
	card.name = it["id"]
	card.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(card, Rect2(at, cs))
	_page_node.add_child(card)
	var equipped := _equipped(it)
	if equipped:   # soft gold glow: a blurred gold shadow behind the frame
		var halo := Panel.new()
		var box := StyleBoxFlat.new()
		box.bg_color = Color(1, 0.8, 0.2, 0.0)
		box.set_corner_radius_all(70)
		box.shadow_color = Color(1, 0.78, 0.15, 0.9)
		box.shadow_size = 40
		halo.add_theme_stylebox_override("panel", box)
		halo.mouse_filter = Control.MOUSE_FILTER_IGNORE
		UI.place(halo, Rect2(Vector2(10, 10), cs - Vector2(20, 20)))
		card.add_child(halo)
	_frame(card, "card_wood", Rect2(Vector2.ZERO, cs), CARD_PAD)
	var ribbon := _ribbon(card, _label(it), Vector2(cs.x * 0.5, CARD_PAD_IN + 36.0), CARD_RIBBON_W)
	var bottom_y := cs.y - CARD_PAD_IN - PILL_H
	var pic := Rect2(CARD_PAD_IN, CARD_PAD_IN + 84.0, cs.x - CARD_PAD_IN * 2.0, bottom_y - 16.0 - (CARD_PAD_IN + 84.0))
	var ring := Rect2()
	match kind:
		Shop.BOOSTER:
			ring = Rect2(pic.get_center() - Vector2(RING_D, RING_D) * 0.5, Vector2(RING_D, RING_D))
			UI.art(card, "btn_wood", ring.grow(-8)).modulate = MEDALLION
			UI.art(card, "badge_ring_gold", ring)
			_fit(card, TOOL_ART[it["booster"]], ring.grow(-28))
		Shop.OUTFIT:
			_bird(card, "%s_outfit_%d" % [it["species"], it["index"]], pic.grow_individual(-30, 0, -30, 0), n * 0.7)
		_:
			_fit(card, "house_style_%s_body" % it["style"], pic.grow(-8), HOUSE_BODY[it["style"]])
	if _owned(it):
		if equipped:
			UI.art(card, "badge_check", Rect2(cs.x - 24.0 - 64.0, 8.0, 64, 64))
		else:
			var d := PAGER_DISC
			UI.disc(card, "Equip", Rect2(cs.x * 0.5 - d * 0.5, cs.y - CARD_PAD_IN - d, d, d), "icon_hanger", func(): _tap(it), "btn_wood", 0.5)
	else:
		_price(card, cs.x * 0.5, bottom_y, it["price"])
	if kind == Shop.BOOSTER and profile.inventory[it["booster"]] > 0:
		var badge := Rect2(ring.end.x - 36.0, ring.position.y - 4.0, 52, 52)
		UI.art(card, "badge_red", badge)
		UI.number(card, str(profile.inventory[it["booster"]]), Rect2(badge.position.x + 8, badge.position.y + 11, 36, 30), Color.WHITE)
	if _locked(it):
		card.modulate = Color(1, 1, 1, 0.5)
		UI.art(card, "padlock", Rect2(ring.get_center() - Vector2(40, 44), Vector2(80, 88)))
	UI.hit_area(card, Rect2(Vector2.ZERO, cs), func(): _tap(it))

## A bird in its outfit that never freezes: continuous breathing plus periodic blinks.
## Its visible pixels fit `area` (bottom-centred), never stretched.
func _bird(parent: Node, key: String, area: Rect2, phase: float) -> void:
	var t := TextureRect.new()
	t.texture = Art.tex(key + "_front")
	t.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	t.stretch_mode = TextureRect.STRETCH_SCALE
	t.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var used := UI.used_rect(key + "_front")
	var s := minf(area.size.x / used.size.x, area.size.y / used.size.y)
	t.size = t.texture.get_size() * s
	t.position = Vector2(area.get_center().x - used.get_center().x * s, area.end.y - used.end.y * s)
	t.pivot_offset = Vector2(used.get_center().x, used.end.y) * s
	parent.add_child(t)
	var breath := t.create_tween().set_loops()
	breath.tween_property(t, "scale", Vector2(0.992, 1.022), 1.4 + phase * 0.1).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT).set_delay(phase * 0.2)
	breath.tween_property(t, "scale", Vector2.ONE, 1.4 + phase * 0.1).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	var b1 := Art.tex(key + "_front_blink_01")
	var b2 := Art.tex(key + "_front_blink_02")
	if b1 and b2:
		var blink := t.create_tween().set_loops()
		blink.tween_interval(1.6 + phase * 0.5)
		for frame in [b1, b2, b1, t.texture]:
			blink.tween_callback(func(): t.texture = frame)
			blink.tween_interval(0.07)

## ---------- purchase flow ----------

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

## Confirm popup: 720 wide, ribbon across the top edge, picture, price pill, primary disc below the panel.
func _confirm(it: Dictionary) -> void:
	var is_booster: bool = it["kind"] == Shop.BOOSTER
	var title: String = BOOSTER_TITLE[it["booster"]] if is_booster else _label(it)
	var title_h := POPUP_RIBBON_W * UI.used_rect(title).size.y / UI.used_rect(title).size.x
	var top_gap := maxf(56.0, title_h * 0.5 + 32.0)
	var pic_h := POPUP_PIC
	var height := top_gap + pic_h + 32.0 + PILL_H + 56.0
	var panel := Rect2((1080.0 - POPUP_W) * 0.5, 760.0 - height * 0.5, POPUP_W, height)
	var close := Rect2(panel.end.x - 24.0 - 48.0, panel.position.y + 24.0 - 48.0, 96, 96)
	var title_rect := Rect2(Vector2(540.0 - POPUP_RIBBON_W * 0.5, panel.position.y - title_h * 0.5), Vector2(POPUP_RIBBON_W, title_h))
	_modal = Modal.open(self, "", [pic_h, PILL_H], true, UI.PRIMARY, 0.0, instant)   # interim until the shop is redrawn
	var box := _modal.content
	var pic := Rect2(540.0 - pic_h * 0.5, panel.position.y + top_gap, pic_h, pic_h)
	match it["kind"]:
		Shop.BOOSTER:
			var ring := Rect2(pic.get_center() - Vector2(1, 1) * (POPUP_PIC - 24.0) * 0.5, Vector2(1, 1) * (POPUP_PIC - 24.0))
			UI.art(box, "btn_wood", ring.grow(-10)).modulate = MEDALLION
			UI.art(box, "badge_ring_gold", ring)
			_fit(box, TOOL_ART[it["booster"]], ring.grow(-34))
		Shop.OUTFIT:
			_live_bird(box, it["species"], "%s_outfit_%d" % [it["species"], it["index"]], pic)
		_:
			_fit(box, "house_style_%s_body" % it["style"], pic, HOUSE_BODY[it["style"]])
	var shaker := _price(box, 540.0, pic.end.y + 32.0, it["price"])
	var primary := Rect2(540.0 - 80.0, panel.end.y + 40.0, 160, 160)
	var accept := UI.disc(box, "Confirm", primary, "icon_check", func(): _buy(it, shaker), "btn_green", 0.5)
	if profile.coins < it["price"]:
		accept.modulate = Color(1, 1, 1, 0.45)

## A live BirdView in its outfit whose visible pixels fit `fit` (bottom-centred).
func _live_bird(parent: Node, species: String, outfit: String, fit: Rect2) -> void:
	var used := UI.used_rect(outfit + "_front")
	var s := minf(fit.size.x / used.size.x, fit.size.y / used.size.y)
	var bird: BirdView = BirdViewScript.new()
	bird.position = Vector2(fit.get_center().x, fit.end.y + (BirdView.FEET_Y - used.end.y) * s)
	parent.add_child(bird)
	bird.setup(Board.bird(0, Board.species_index(species), 0), false, s * BirdView.CANVAS / BirdView.DISPLAY, outfit)

func _buy(it: Dictionary, shaker: Array) -> void:
	var err := Shop.purchase(profile, it["id"])
	if err == "coins":
		Sound.play("error")
		for node in shaker:
			var x: float = node.position.x
			var tw: Tween = node.create_tween()
			for dx in [14, -14, 9, -9, 0]:
				tw.tween_property(node, "position:x", x + dx, 0.05)
		return
	if err != "":
		return
	Sound.play("ui_tap")
	profile_changed.emit()
	_modal.close()
	_refresh()

## ---------- capture preview ----------

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
			start = 2
		"confirm_outfit":
			p.coins = 1240
			start = 1
		"locked_tools":
			p.highest_unlocked = 3
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
