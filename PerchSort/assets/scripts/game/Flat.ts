import { BlockInputEvents, Color, Font, HorizontalTextAlignment, Label, Layers, Node, TTFFont, UITransform, Vec3, VerticalTextAlignment, resources, tween } from 'cc';
import { UI, Rect } from './UI';
import { I18n } from './I18n';

/**
 * The flat UI kit, taken from fishsort (its ui/kit.ts, popup.ts and docs/03): flat candy buttons with a darker bottom edge, blue
 * label plates, white cards and popups with navy words, the KN Maiyuan game font with a white-on-dark outline and a hard shadow.
 * Coordinates are this game's: 1080 x 1920, rects from the top left. fishsort's sizes (750 wide) are multiplied by K.
 */
export interface Ink { color: string; outline?: string; depth?: string }
export type Tone = 'green' | 'blue' | 'amber' | 'red' | 'gray';
const DEPTH: Record<string, string> = { green: '#268F1B', blue: '#1E55C4', amber: '#C47400', red: '#BC3320', gray: '#7C889C', plate: '#1A52BE' };
/** Navy words on white cards and popups (no outline). */
export const ON_LIGHT: Ink = { color: '#1F56B0' };
/** White words with a navy outline on the backdrop. */
export const ON_DARK: Ink = { color: '#FFFFFF', outline: '#0B2A73' };
/** White words on a flat button or plate: no outline, a solid line of the plate's bottom colour underneath. */
export const onTone = (tone: string): Ink => ({ color: '#FFFFFF', depth: DEPTH[tone] });
export interface Popup { node: Node; body: Node; rect: Rect; close: () => void }

const hex = (s: string) => new Color().fromHEX(s);
const ART = 'art/flat/';

export class Flat {
  static readonly K = 1080 / 750;
  /** Centre line of a page header: the back key on the left and the title plate share it (fishsort). */
  static readonly HEAD_Y = 112;
  /** Plate pictures (buttons, label plates, the capsule) are 376 x 300 with 186-pixel round ends. */
  private static readonly PLATE: [number, number, number] = [376, 300, 186];
  static font: Font | null = null;
  private static fonts = new Map<string, Font>();
  /** The game font of a language (Korean has its own). Fails loudly: the screens never fall back to a system font. */
  static async loadFont(lang: string): Promise<void> {
    const path = lang === 'ko' ? 'font/game_ko' : 'font/game';
    if (!this.fonts.has(path)) this.fonts.set(path, await new Promise<Font>((resolve, reject) => resources.load(path, TTFFont, (e, f) => e ? reject(e) : resolve(f))));
    this.font = this.fonts.get(path)!;
  }
  /** One line of words centred (align 0), left (-1) or right (1) in rect, shrunk only if it would leave the rect. */
  static text(parent: Node, s: string, rect: Rect, size: number, ink: Ink = ON_LIGHT, align = 0): Label {
    const box = UI.group(parent, 'Text', rect), n = new Node('Label');
    n.layer = Layers.Enum.UI_2D; box.addChild(n);
    const t = n.addComponent(UITransform); t.setAnchorPoint(align < 0 ? 0 : align > 0 ? 1 : .5, .5);
    n.setPosition(align < 0 ? 0 : align > 0 ? rect[2] : rect[2] / 2, -rect[3] / 2, 0);
    const l = n.addComponent(Label);
    l.useSystemFont = false; l.font = this.font; l.fontSize = size; l.lineHeight = Math.round(size * 1.25);
    l.color = hex(ink.color);
    if (ink.outline) {
      l.enableOutline = true; l.outlineColor = hex(ink.outline); l.outlineWidth = Math.max(2, Math.round(size / 18));
      if (size >= 40) { const c = hex(ink.outline); l.enableShadow = true; l.shadowColor = new Color(c.r * .7, c.g * .7, c.b * .7, 235); l.shadowOffset.set(0, -Math.max(1, Math.round(size / 26))); l.shadowBlur = 1; }
    } else if (ink.depth) { l.enableShadow = true; l.shadowColor = hex(ink.depth); l.shadowOffset.set(0, -Math.max(2, Math.round(size / 16))); l.shadowBlur = 0; }
    l.horizontalAlign = align < 0 ? HorizontalTextAlignment.LEFT : align > 0 ? HorizontalTextAlignment.RIGHT : HorizontalTextAlignment.CENTER;
    l.verticalAlign = VerticalTextAlignment.CENTER; l.overflow = Label.Overflow.NONE;
    l.string = s; this.fit(l, rect[2], size);
    return l;
  }
  static width(l: Label): number { l.updateRenderData(true); return l.node.getComponent(UITransform)!.width; }
  static fit(l: Label, room: number, size: number): void {
    l.fontSize = size; const w = this.width(l);
    if (w > room) { l.fontSize = Math.max(18, Math.floor(size * room / w)); l.updateRenderData(true); }
  }
  /** A plate picture scaled to the rect's height; only its middle stretches, so the round ends never deform. */
  static plate(parent: Node, skin: string, rect: Rect, name = skin): Node {
    const [w, h, end] = this.PLATE;
    return UI.nine_region(parent, ART + skin, [0, 0, w, h], rect, [end, 0, end, 0], rect[3] / h);
  }
  /** A nine-slice card (f_popup, f_inset, f_card ...): the pictures are drawn three times fishsort's size, so corners scale by K / 3. */
  static card(parent: Node, skin: string, rect: Rect, source: number, border: number): Node {
    return UI.nine_region(parent, ART + skin, [0, 0, source, source], rect, [border, border, border, border], this.K / 3);
  }
  static image(parent: Node, skin: string, rect: Rect): Node { return UI.image(parent, ART + skin, rect); }
  /** Where words sit on a plate: the face is half a bottom edge above the plate's middle. */
  static face(h: number): number { return h * .445; }
  /** A flat candy button with one line of words. */
  static button(parent: Node, name: string, rect: Rect, tone: Tone, words: string, action: () => void, size = Math.round(rect[3] * .4)): Node {
    const n = UI.hit_area(parent, name, rect, action);
    this.plateIn(n, 'j_btn_' + tone, rect);
    if (words) this.text(n, words, [24, this.face(rect[3]) - size * .7, rect[2] - 48, size * 1.4], size, onTone(tone));
    return n;
  }
  /** A flat button with a white glyph before its words (watch an ad, buy). */
  static iconButton(parent: Node, name: string, rect: Rect, tone: Tone, glyph: string, words: string, action: () => void, size = Math.round(rect[3] * .36)): Node {
    const n = this.button(parent, name, rect, tone, '', action), g = size * 1.3, face = this.face(rect[3]);
    const l = this.text(n, words, [0, face - size * .7, rect[2], size * 1.4], size, onTone(tone)), lw = Math.min(this.width(l), rect[2] - g - 70), x = (rect[2] - g - 14 - lw) / 2;
    UI.place(l.node.parent!, [x + g + 14, face - size * .7, lw, size * 1.4]); l.node.setPosition(lw / 2, -size * .7, 0);
    this.image(n, glyph, [x, face - g / 2, g, g]);
    return n;
  }
  /** A square key with a white glyph (close, settings, plus). */
  static key(parent: Node, name: string, rect: Rect, tone: 'blue' | 'green' | 'red', glyph: string, action: () => void, glyphScale = .56, mirror = false): Node {
    const n = UI.hit_area(parent, name, rect, action), g = rect[2] * glyphScale;
    UI.image(n, ART + 'f_key_' + tone, [0, 0, rect[2], rect[3]]);
    const icon = UI.image(n, ART + glyph, [(rect[2] - g) / 2, rect[3] * .445 - g / 2, g, g]);
    if (mirror) { icon.getComponent(UITransform)!.setAnchorPoint(.5, .5); icon.setPosition(rect[2] / 2, -rect[3] * .445, 0); icon.setScale(-1, 1, 1); }
    return n;
  }
  /** The plate of a group laid over its whole rect. */
  static plateIn(n: Node, skin: string, rect: Rect): Node { return this.plate(n, skin, [0, 0, rect[2], rect[3]]); }
  /** A page or popup title (fishsort pageTitle): white words on a blue plate as wide as the words, centred on (cx, cy). */
  static title(parent: Node, words: string, cx: number, cy: number, h = 115, maxW = 900): Node {
    const size = Math.round(h * .56), probe = new Node(), w = Math.min(maxW, Math.max(374, this.width(this.text(probe, words, [0, 0, 4000, h], size, onTone('plate'))) + h * 1.2));
    probe.destroy();
    const n = UI.group(parent, 'Title', [cx - w / 2, cy - h / 2, w, h]);
    this.plateIn(n, 'f_plate_blue', [0, 0, w, h]);
    this.text(n, words, [h * .5, this.face(h) - size * .7, w - h, size * 1.4], size, onTone('plate'));
    return n;
  }
  /** The feather balance: a white capsule with the feather and the number; onPlus adds the green plus key (free feathers). */
  static capsule(parent: Node, rect: Rect, value: number, onPlus?: () => void): Node {
    const n = UI.group(parent, 'Coins', rect);
    this.plateIn(n, 'f_dock', rect);
    UI.art(n, 'icon_purchase_feather', [8, rect[3] * .07, rect[3] * .86, rect[3] * .86]);
    this.text(n, String(Math.min(99999, value)), [rect[3] + 4, 0, rect[2] - rect[3] - 36, rect[3]], Math.round(rect[3] * .57), ON_LIGHT, -1);
    if (onPlus) this.key(parent, 'FreeCoins', [rect[0] + rect[2] - 34, rect[1] + 8, rect[3] - 16, rect[3] - 16], 'green', 'f_g_plus', onPlus);
    return n;
  }
  /** fishsort's level-kind tag: an orange (hard) or purple (super hard) plate with the flame and the word; returns its width. */
  static kindTag(parent: Node, kind: string, x: number, y: number, h = 64): number {
    const label = kind === 'boss' ? 'UI_TAG_SUPER' : 'UI_CHALLENGE_TAG', probe = new Node(), size = Math.round(h * .55);
    const tw = this.width(this.text(probe, I18n.tr(label), [0, 0, 2000, h], size)); probe.destroy();
    const w = Math.ceil(h * .32 + h * .82 + 6 + tw + h * .4), n = UI.group(parent, 'KindTag', [x, y, w, h]);
    this.plateIn(n, kind === 'boss' ? 'f_tag_boss' : 'f_tag_hard', [0, 0, w, h]);
    UI.image(n, ART + 'kind_hard', [h * .32, h * .09, h * .82, h * .82]);
    this.text(n, I18n.tr(label), [h * .32 + h * .82 + 6, 0, tw + 4, h], size, { color: '#FFFFFF', depth: kind === 'boss' ? '#4C1D95' : '#B23A0E' }, -1);
    return w;
  }
  /** The red dot on an entry with something to collect. */
  static dot(parent: Node, x: number, y: number, side = 40): Node { return UI.image(parent, ART + 'f_dot', [x - side / 2, y - side / 2, side, side]); }
  /** A popup (fishsort popup.ts): dimmed backdrop, white card growing a head for the blue title plate, red close key on the corner. */
  static popup(host: Node, name: string, w: number, h: number, title: string, onClose?: () => void, closable = true): Popup {
    const node = UI.group(host, name), K = this.K, head = title ? Math.round(64 * K) : 0;
    node.addComponent(BlockInputEvents);
    UI.fill(node, 'Dim', [0, 0, 1080, 1920], '#00123CA0');
    const pivot = UI.group(node, 'Pivot', [540, 960, 0, 0]), content = UI.group(pivot, 'Content', [-540, -960, 1080, 1920]);
    const x = (1080 - w) / 2, top = Math.round((1920 - h - head) / 2);
    this.card(content, 'f_popup', [x, top, w, h + head], 300, 146);
    const body = UI.group(content, 'Body', [x, top + head, w, h]);
    let closed = false;
    const close = () => { if (closed) return; closed = true; node.destroy(); onClose?.(); };
    if (title) {
      const ph = Math.round(88 * K), probe = this.text(content, title, [0, -999, 2000, 100], 66, onTone('plate'));
      const pw = Math.min(w - 216, Math.max(374, this.width(probe) + 138)); probe.node.parent!.destroy();
      const plate = UI.group(content, 'Title', [540 - pw / 2, top + 12 - ph / 2, pw, ph]);
      this.plateIn(plate, 'f_plate_blue', [0, 0, pw, ph]);
      this.text(plate, title, [36, this.face(ph) - 46, pw - 72, 92], 66, onTone('plate'));
    }
    if (closable) this.key(content, 'Close', [x + w - 78, top - 26, 104, 104], 'red', 'f_g_x', close, .5);
    pivot.setScale(.8, .8, 1);
    tween(pivot).to(.12, { scale: new Vec3(1.04, 1.04, 1) }, { easing: 'quadOut' }).to(.08, { scale: new Vec3(1, 1, 1) }).start();
    return { node, body, rect: [x, top + head, w, h], close };
  }
}
