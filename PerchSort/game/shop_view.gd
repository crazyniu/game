extends Control
## Shop: boosters, outfits and houses in a two-column card grid, with a purchase confirm popup.

signal closed
signal profile_changed

const SCREEN := "shop_boosters"
const TABS := [Shop.BOOSTER, Shop.OUTFIT, Shop.HOUSE]
const TAB_IDS := ["tab_boosters", "tab_outfits", "tab_home"]
const TAB_ICONS := ["icon_bulb", "icon_hanger", "icon_home"]
const TAB_ICON_RATIO := [0.56, 0.62, 0.58]
const PER_PAGE := 4
const BOOSTER_KEYS := ["hint", "branch", "reveal", "bell", "shoo", "rain"]
const TOOL_ART := ["tool_hint", "tool_branch", "tool_owl_eye", "tool_bell", "tool_scarecrow", "tool_rain"]
const BOOSTER_LABEL := ["label_card_hint", "label_card_extra_branch", "label_card_reveal", "label_card_bell",
	"label_card_shoo", "label_card_rain"]
const HOUSE_LABEL := {"mushroom": "label_card_mushroom", "teapot": "label_card_teapot", "lantern": "label_card_lantern",
	"acorn": "label_card_acorn"}
const OUTFIT_LABEL := {
	"sunny": ["straw_hat", "blue_bow_tie", "little_crown"], "sky": ["cozy_scarf", "aviator_goggles", "tiny_top_hat"],
	"berry": ["round_glasses", "pink_bow", "sparkle_tiara"], "mint": ["flower", "mushroom_cap", "acorn_cap"],
	"cocoa": ["red_beanie", "plaid_bow_tie", "monocle"], "snow": ["earmuffs", "snowflake_clip", "ice_tiara"],
	"ember": ["red_bandana", "flame_headband", "knight_helmet"], "ink": ["star_nightcap", "silver_glasses", "little_star_hat"],
	"hoot": ["graduation_cap", "reading_glasses", "wizard_cape"]}
## Card geometry relative to the card rect, measured from the three mockups.
const GEO := {
	Shop.BOOSTER: {"size": Vector2(459, 420), "ribbon": Rect2(53, 24, 355, 92), "title_h": 44.0,
		"pic": Rect2(117, 116, 216, 194), "ring": Rect2(95, 99, 260, 227), "pill": Rect2(120, 320, 241, 75),
		"coin": Rect2(85, 310, 105, 92), "num": Rect2(216, 335, 100, 45)},
	Shop.OUTFIT: {"size": Vector2(468, 646), "ribbon": Rect2(43, 39, 382, 102), "title_h": 53.0,
		"pic": Rect2(64, 144, 348, 368), "pill": Rect2(124, 517, 255, 96), "coin": Rect2(87, 510, 126, 109),
		"num": Rect2(231, 540, 114, 50)},
	Shop.HOUSE: {"size": Vector2(484, 654), "ribbon": Rect2(81, 39, 313, 101), "title_h": 46.0,
		"pic": Rect2(50, 150, 384, 350), "pill": Rect2(99, 520, 298, 95), "coin": Rect2(96, 514, 121, 110),
		"num": Rect2(242, 542, 103, 44)},
}
const COLUMN_X := {Shop.BOOSTER: [60.0, 560.0], Shop.OUTFIT: [54.0, 553.0], Shop.HOUSE: [47.0, 548.0]}
const ROW_Y := {Shop.BOOSTER: [430.0, 880.0, 1330.0], Shop.OUTFIT: [424.0, 1095.0], Shop.HOUSE: [422.0, 1090.0]}
const BORDER_SCALE := 0.55   # mockup frames have thinner borders than the nine-slice default
const CARD_PAD := Vector4(15, 48, 15, 4)    # transparent margins of card_wood (left, top, right, bottom)
const PANEL_PAD := Vector4(4, 21, 4, 21)    # transparent margins of panel_wood
## Solid pixels of each house sprite (its faint shadow would otherwise shrink the fitted picture).
const HOUSE_BODY := {"mushroom": Rect2(41, 51, 427, 402), "teapot": Rect2(29, 55, 459, 399),
	"lantern": Rect2(50, 61, 409, 394), "acorn": Rect2(56, 51, 398, 404)}
const MEDALLION := Color(0.62, 0.4, 0.3)
const BIRD_SCALE := 1.5
const FEET := Vector2(160, 301)   # visible feet point in the 320 px bird frame

var profile: Profile
var tab := 0
var page := 0
var _page_node: Control
var _tab_node: Control
var _coin_number: Control
var _popup: Control   # confirm modal (dim + content)
var _popup_content: Control
var instant := false   # captures skip the modal pop-in


func setup(p: Profile, start_tab := 0) -> void:
	profile = p
	tab = start_tab
	size = Vector2(1080, 1920)
	UI.image(self, "bg_level_w1_noon", Rect2(0, 0, 1080, 1920), false)
	for right in [false, true]:
		var crown := UI.image(self, "tree_crown_w1", Rect2(-80 + (right as int) * 920, -150, 320, 420))
		crown.flip_h = right
	_build_header()
	_page_node = Control.new()
	_page_node.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(_page_node)
	_refresh()

func _build_header() -> void:
	UI.panel(self, "stat_pill", Layout.r(SCREEN, "balance"))
	UI.art(self, "coin", Layout.r(SCREEN, "balance_coin"))
	_coin_number = UI.number(self, str(profile.coins), Layout.r(SCREEN, "balance_value").grow_individual(20, 0, 20, 0))
	UI.disc(self, "Close", Layout.r(SCREEN, "close"), "icon_close", func(): closed.emit(), "btn_wood", 0.48)
	_tab_node = Control.new()
	_tab_node.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(_tab_node)

func _refresh() -> void:
	UI.set_number(_coin_number, str(profile.coins))
	for node in [_page_node, _tab_node]:
		for c in node.get_children():
			c.queue_free()
	var glow := Layout.r(SCREEN, "tab_glow")
	for i in TABS.size():
		var r := Layout.r(SCREEN, TAB_IDS[i])
		if i == tab:
			UI.art(_tab_node, "tab_selected_ring", Rect2(r.get_center().x - glow.size.x * 0.5, glow.position.y, glow.size.x, 150))
		var index := i
		UI.disc(_tab_node, TAB_IDS[i], r, TAB_ICONS[i], func(): _select_tab(index), "btn_wood", TAB_ICON_RATIO[i])
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

func _build_boosters() -> void:
	var singles := _items(Shop.BOOSTER).filter(func(it): return it["quantity"] == 1)
	for i in singles.size():
		_card(singles[i], Vector2(COLUMN_X[Shop.BOOSTER][i % 2], ROW_Y[Shop.BOOSTER][i / 2]), i)

func _build_cosmetics(items: Array) -> void:
	var kind: int = TABS[tab]
	var pages := ceili(items.size() / float(PER_PAGE))
	for n in mini(PER_PAGE, items.size() - page * PER_PAGE):
		var i := page * PER_PAGE + n
		_card(items[i], Vector2(COLUMN_X[kind][n % 2], ROW_Y[kind][n / 2]), n)
	if pages > 1:
		_build_pager(pages)

func _build_pager(pages: int) -> void:
	var y := 1782.0
	var prev := UI.disc(_page_node, "Previous", Rect2(330, y, 100, 100), "icon_next_arrow", func(): _turn(-1), "btn_wood", 0.5)
	prev.get_node("Icon").rotation = PI
	prev.get_node("Icon").pivot_offset = prev.get_node("Icon").size * 0.5
	UI.set_enabled(prev, page > 0)
	UI.panel(_page_node, "stat_pill", Rect2(450, y + 6, 180, 88))
	UI.number(_page_node, "%d/%d" % [page + 1, pages], Rect2(470, y + 28, 140, 44))
	var next := UI.disc(_page_node, "Next", Rect2(650, y, 100, 100), "icon_next_arrow", func(): _turn(1), "btn_wood", 0.5)
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

## Painted name centered at `center`, `h` px tall (visible pixels).
func _title(parent: Node, label: String, center: Vector2, h: float, max_w := 1e6) -> void:
	var used := UI.used_rect(label)
	var w := minf(h * used.size.x / used.size.y, max_w)
	h = w * used.size.y / used.size.x
	UI.art(parent, label, Rect2(center - Vector2(w, h) * 0.5, Vector2(w, h)))

## Nine-slice frame whose visible body (not its transparent margins) fills `body`.
func _frame(parent: Node, name: String, body: Rect2, pad: Vector4) -> NinePatchRect:
	var k := BORDER_SCALE
	return UI.panel(parent, name, body.grow_individual(pad.x * k, pad.y * k, pad.z * k, pad.w * k), k)

## Sprite scaled so its visible pixels fit `area`, centered.
func _fit(parent: Node, name: String, area: Rect2, solid := Rect2()) -> TextureRect:
	var used := solid if solid.size != Vector2.ZERO else UI.used_rect(name)
	var s := minf(area.size.x / used.size.x, area.size.y / used.size.y)
	return UI.art(parent, name, Rect2(area.get_center() - used.size * s * 0.5, used.size * s), used)

func _card(it: Dictionary, at: Vector2, n: int) -> void:
	var kind: int = it["kind"]
	var g: Dictionary = GEO[kind]
	var card := Control.new()
	card.name = it["id"]
	card.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(card, Rect2(at, g["size"]))
	_page_node.add_child(card)
	var equipped := _equipped(it)
	if equipped:   # soft gold glow: a blurred gold shadow behind the frame
		var halo := Panel.new()
		var box := StyleBoxFlat.new()
		box.bg_color = Color(1, 0.8, 0.2, 0.0)
		box.set_corner_radius_all(70)
		box.shadow_color = Color(1, 0.78, 0.15, 0.9)
		box.shadow_size = 46
		halo.add_theme_stylebox_override("panel", box)
		halo.mouse_filter = Control.MOUSE_FILTER_IGNORE
		UI.place(halo, Rect2(Vector2(10, 10), g["size"] - Vector2(20, 20)))
		card.add_child(halo)
	_frame(card, "card_wood", Rect2(Vector2.ZERO, g["size"]), CARD_PAD)
	var ribbon: Rect2 = g["ribbon"]
	UI.art(card, "ribbon_green", ribbon)
	_title(card, _label(it), ribbon.get_center() + Vector2(0, -4), g["title_h"])
	var pic: Rect2 = g["pic"]
	match kind:
		Shop.BOOSTER:
			var ring: Rect2 = g["ring"]
			UI.art(card, "btn_wood", ring.grow(-8)).modulate = MEDALLION
			UI.art(card, "badge_ring_gold", ring)
			_fit(card, TOOL_ART[it["booster"]], ring.grow(-26))
		Shop.OUTFIT:
			_bird(card, "%s_outfit_%d" % [it["species"], it["index"]], Vector2(pic.get_center().x, pic.end.y), BIRD_SCALE, n * 0.7)
		_:
			_fit(card, "house_style_%s_body" % it["style"], pic.grow(10), HOUSE_BODY[it["style"]])
	var pill: Rect2 = g["pill"]
	if _owned(it):
		if equipped:
			UI.art(card, "badge_check", Rect2(g["size"].x - 82, -9, 56, 56))
		else:
			var disc := Rect2(pill.get_center() - Vector2(54, 54), Vector2(108, 108))
			UI.disc(card, "Equip", disc, "icon_hanger", func(): _tap(it), "btn_wood", 0.6)
	else:
		_price(card, pill, g["coin"], g["num"], it["price"])
	if kind == Shop.BOOSTER and profile.inventory[it["booster"]] > 0:
		var badge := Rect2(g["size"].x - 92, -1, 74, 74)
		UI.art(card, "badge_red", badge)
		UI.number(card, str(profile.inventory[it["booster"]]), badge.grow_individual(-24, -22, -24, -22), "white")
	if _locked(it):
		card.modulate = Color(1, 1, 1, 0.5)
		UI.art(card, "padlock", Rect2(pic.get_center() - Vector2(40, 44), Vector2(80, 88)))
	UI.hit_area(card, Rect2(Vector2.ZERO, g["size"]), func(): _tap(it))

func _price(parent: Node, pill: Rect2, coin: Rect2, num: Rect2, price: int) -> Control:
	var group := Control.new()
	group.mouse_filter = Control.MOUSE_FILTER_IGNORE
	UI.place(group, pill.merge(coin))
	parent.add_child(group)
	var off := -group.position
	UI.panel(group, "stat_pill", Rect2(pill.position + off, pill.size))
	UI.art(group, "coin", Rect2(coin.position + off, coin.size))
	UI.number(group, str(price), Rect2(num.position + off, num.size))
	return group

## A bird in its outfit that never freezes: continuous breathing plus periodic blinks.
## `feet` is the visible feet point in the parent's coordinates.
func _bird(parent: Node, key: String, feet: Vector2, s: float, phase: float) -> void:
	var t := TextureRect.new()
	t.texture = Art.tex(key + "_front")
	t.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	t.stretch_mode = TextureRect.STRETCH_SCALE
	t.mouse_filter = Control.MOUSE_FILTER_IGNORE
	t.size = Vector2(320, 320) * s
	t.position = feet - FEET * s
	t.pivot_offset = FEET * s
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

## Confirm modal built inside the shop (its own layout and pill shake).
func _open_modal(c: String) -> Control:
	_popup = Control.new()
	_popup.size = Vector2(1080, 1920)
	add_child(_popup)
	var dim := ColorRect.new()
	dim.color = Color(0.05, 0.08, 0.04, 0.55)
	dim.size = _popup.size
	_popup.add_child(dim)
	_popup_content = Control.new()
	_popup_content.size = _popup.size
	_popup_content.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_popup_content.pivot_offset = Layout.r(c, "panel").get_center()
	_popup.add_child(_popup_content)
	_frame(_popup_content, "panel_wood", Layout.r(c, "panel"), PANEL_PAD)
	UI.disc(_popup_content, "Close", Layout.r(c, "close"), "icon_close", _close_modal, "btn_wood", 0.48)
	if instant:
		return _popup_content
	_popup.modulate.a = 0.0
	_popup_content.scale = Vector2.ONE * 0.7
	var tw := create_tween().set_parallel()
	tw.tween_property(_popup, "modulate:a", 1.0, 0.18)
	tw.tween_property(_popup_content, "scale", Vector2.ONE, 0.3).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	return _popup_content

func _close_modal() -> void:
	var modal := _popup
	var tw := create_tween().set_parallel()
	tw.tween_property(modal, "modulate:a", 0.0, 0.15)
	tw.tween_property(_popup_content, "scale", Vector2.ONE * 0.85, 0.15)
	tw.chain().tween_callback(modal.queue_free)

func _confirm(it: Dictionary) -> void:
	var c := "shop_confirm"
	var box := _open_modal(c)
	UI.art(box, "ribbon_green", Layout.r(c, "ribbon"))
	_title(box, _label(it), Layout.r(c, "title").get_center(), 80.0, 400.0)
	var ring := Layout.r(c, "hintdisc")
	UI.art(box, "btn_wood", ring.grow(-14)).modulate = MEDALLION
	UI.art(box, "badge_ring_gold", ring)
	match it["kind"]:
		Shop.BOOSTER: _fit(box, TOOL_ART[it["booster"]], ring.grow(-50))
		Shop.OUTFIT: _bird(box, "%s_outfit_%d" % [it["species"], it["index"]], Vector2(ring.get_center().x, ring.end.y - 45), 1.3, 0.0)
		_: _fit(box, "house_style_%s_body" % it["style"], ring.grow(-30), HOUSE_BODY[it["style"]])
	var pill := UI.panel(box, "stat_pill", Layout.r(c, "price"))
	pill.pivot_offset = pill.size * 0.5
	var coin := UI.art(box, "coin", Layout.r(c, "pricecoin"))
	var amount := UI.number(box, str(it["price"]), Layout.r(c, "pricenum"))
	var shaker := [pill, coin, amount]
	var accept := UI.disc(box, "Confirm", Layout.r(c, "confirm"), "icon_check", func(): _buy(it, shaker), "btn_green", 0.5)
	if profile.coins < it["price"]:
		accept.modulate = Color(1, 1, 1, 0.45)

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
	_close_modal()
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
