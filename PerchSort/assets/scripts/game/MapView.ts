import { Mask, Node, Sprite, Color, Vec2 } from 'cc';
import { UI, Rect } from './UI';
import { Kit } from './Popups';
import { Flat, ON_DARK, ON_LIGHT, onTone } from './Flat';
import { tr, word } from './I18n';
import { Profile } from '../meta/Profile';
import { Progress } from '../meta/Progress';
import { Levels } from '../core/Levels';

interface MapCallbacks { home: () => void; level: (level: number) => void; gate: (world: number) => void; coins: () => void }
const WORLD = ['green','bamboo','autumn','snow','blossom','mushroom','lakeside','pine','rainforest','wisteria'];
/** fishsort's sea cards (pages/map.ts) x 1.44: two columns of upright cards. */
const CARD_W = 475, CARD_H = 676, GAP_X = 35, GAP_Y = 37;

/**
 * The map (fishsort's sea map and level grid): every world as an upright card in two columns (its board picture, the name on a
 * plate, "Levels a–b" and a progress bar, or a lock with the level that opens it; the current world wears "Now" and breathes).
 * A card opens its world: 21 level tiles in a white panel (cleared blue with stars, current green, locked grey), with arrows to the
 * neighbouring worlds.
 */
export class MapView {
  node: Node;
  page = false;
  private body!: Node;
  constructor(host: Node, private profile: Profile, private callbacks: MapCallbacks, public world = 0, private instant = false) {
    this.node = UI.group(host, 'Map');
    if (!this.world) this.world = Progress.world_of_level(Progress.resume_level(profile));
    this.showWorlds();
  }
  private reset(background: string): void {
    UI.clear(this.node);
    UI.image(this.node, background, [0, 0, 1080, 1920]);
    this.body = UI.group(this.node, 'Body');
  }
  private picture(world: number): string { return `board_w${world}${world === 1 || world === Levels.WORLDS ? '_open' : ''}_bg`; }
  private showWorlds(): void {
    this.page = false;
    this.reset('background_home_jungle');
    const top = 210, view = 1920 - top - 40, rowH = CARD_H + GAP_Y, rows = Math.ceil(Levels.WORLDS / 2);
    const scroll = UI.scroll(this.body, 'Scroll', [0, top, 1080, view], rows * rowH + 20);
    const current = Progress.world_of_level(Progress.resume_level(this.profile));
    for (let w = 1; w <= Levels.WORLDS; w++) {
      const i = w - 1, card = this.card(scroll.content, w, w === current, [540 + (i % 2 === 0 ? -1 : 1) * (CARD_W + GAP_X) / 2 - CARD_W / 2, 10 + Math.floor(i / 2) * rowH, CARD_W, CARD_H]);
      Kit.pop(card, Math.abs(Math.floor(i / 2) - Math.floor((current - 1) / 2)) * .05);
    }
    scroll.scroll.scrollToOffset(new Vec2(0, Math.max(0, Math.floor((current - 1) / 2) * rowH - CARD_H * .25)), 0);
    Kit.close(this.body, [32, Flat.HEAD_Y - 64, 128, 128], this.callbacks.home, true);
    Flat.title(this.body, tr('T_WORLDS'), 540, Flat.HEAD_Y);
  }
  private card(parent: Node, world: number, current: boolean, r: Rect): Node {
    const open = Progress.is_world_unlocked(this.profile, world), n = UI.hit_area(parent, 'World_' + world, r, () => open ? this.open_world(world) : this.callbacks.gate(world));
    Flat.card(n, 'f_block', [0, 0, r[2], r[3]], 240, 110);
    const pw = r[2] - 32, ph = r[3] - 44, pic = UI.group(n, 'Picture', [16, 14, pw, ph]);
    pic.addComponent(Mask).type = Mask.Type.GRAPHICS_RECT;
    const image = UI.image(pic, this.picture(world), [0, (ph - pw * 1.5) / 2 - 20, pw, pw * 1.5]);
    if (!open) { const sprite = image.getComponent(Sprite); if (sprite) { sprite.grayscale = true; sprite.color = new Color(185, 192, 210, 255); } }
    const name = UI.group(n, 'Name', [r[2] / 2 - 194, 30, 388, 90]);
    Flat.plateIn(name, 'f_plate_blue', [0, 0, 388, 90]);
    Flat.text(name, word(`world_${WORLD[world - 1]}`), [24, Flat.face(90) - 32, 340, 64], 46, onTone('plate'));
    const from = (world - 1) * Levels.PER_WORLD + 1, to = world * Levels.PER_WORLD, lowH = 144, low = UI.group(n, 'Low', [16, 14 + ph - lowH, pw, lowH]);
    UI.fill(low, 'Shade', [0, 0, pw, lowH], '#0A1E46A0');
    Flat.text(low, tr('MAP_RANGE', from, to), [0, 8, pw, 64], 40, ON_DARK);
    if (open) {
      let done = 0; for (let level = from; level <= to; level++) if (this.profile.best_stars(level) > 0) done++;
      const bar = UI.group(low, 'Bar', [36, 82, pw - 72, 44]);
      Flat.plateIn(bar, 'f_bar_track', [0, 0, pw - 72, 44]);
      if (done) Flat.plate(bar, done >= Levels.PER_WORLD ? 'f_bar_green' : 'f_bar_blue', [0, 0, Math.max(44, (pw - 72) * done / Levels.PER_WORLD), 44]);
      Flat.text(bar, `${done}/${Levels.PER_WORLD}`, [0, 0, pw - 72, 44], 32, ON_DARK);
    } else {
      Flat.text(low, tr('H_UNLOCKS_AT', from), [60, 76, pw - 60, 56], 38, ON_DARK);
      UI.image(low, 'art/flat/ic_lock', [44, 80, 46, 46]);
      UI.image(n, 'art/flat/ic_lock', [r[2] / 2 - 60, r[3] / 2 - 90, 120, 120]);
    }
    if (current) {
      const tag = UI.group(n, 'Now', [r[2] / 2 - 86, 14 + ph - lowH - 84, 172, 72]);
      Flat.plateIn(tag, 'j_btn_amber', [0, 0, 172, 72]);
      Flat.text(tag, tr('LV_CURRENT'), [10, Flat.face(72) - 28, 152, 56], 38, onTone('amber'));
    }
    return n;
  }
  /** One world's 21 levels: three columns in a white panel, arrows to the neighbouring worlds. */
  open_world(world: number): void {
    if (!Progress.is_world_unlocked(this.profile, world)) { this.callbacks.gate(world); return; }
    this.world = world; this.page = true;
    this.reset(this.picture(world));
    Kit.close(this.body, [32, Flat.HEAD_Y - 64, 128, 128], () => this.closeWorld(), true);
    Flat.title(this.body, word(`world_${WORLD[world - 1]}`), 540, Flat.HEAD_Y, 115, 640);
    const panel: Rect = [64, 220, 952, 1480], cellW = panel[2] / 3, rowH = (panel[3] - 40) / 7;
    Flat.card(this.body, 'f_card', panel, 240, 116);
    for (let i = 0; i < Levels.PER_WORLD; i++) this.levelNode((world - 1) * Levels.PER_WORLD + i + 1, panel[0] + cellW * (i % 3 + .5), panel[1] + 20 + rowH * (Math.floor(i / 3) + .5), i);
    const navY = 1790;
    const prev = Flat.key(this.body, 'WorldPrevious', [540 - 330, navY - 60, 120, 120], 'blue', 'f_g_back', () => this.open_world(world - 1));
    const next = Flat.key(this.body, 'WorldNext', [540 + 210, navY - 60, 120, 120], 'blue', 'f_g_back', () => this.open_world(world + 1), .56, true);
    UI.enabled(prev, world > 1); UI.enabled(next, world < Levels.WORLDS && Progress.is_world_unlocked(this.profile, world + 1));
    const range = UI.group(this.body, 'Range', [540 - 170, navY - 46, 340, 92]);
    Flat.plateIn(range, 'f_dock', [0, 0, 340, 92]);
    Flat.text(range, `${(world - 1) * Levels.PER_WORLD + 1} - ${world * Levels.PER_WORLD}`, [0, 0, 340, 92], 46, ON_LIGHT);
  }
  private levelNode(level: number, x: number, y: number, index: number): void {
    const state = Progress.node_state(this.profile, level), side = 176, n = UI.hit_area(this.body, 'Level_' + level, [x - side / 2, y - side / 2 - 14, side, side + 20], () => state === 'locked' ? this.callbacks.gate(this.world) : this.callbacks.level(level));
    Flat.image(n, state === 'cleared' ? 'f_node_blue' : state === 'current' ? 'f_node_green' : 'f_node_gray', [0, 0, side, side * 310 / 300]);
    Flat.text(n, String(level), [0, side * .16, side, side * .56], 64, state === 'cleared' ? onTone('blue') : state === 'current' ? onTone('green') : onTone('gray'));
    if (state === 'locked') UI.image(n, 'art/flat/ic_lock', [side - 56, -6, 52, 52]);
    else if (state === 'cleared') for (let s = 0; s < 3; s++) UI.art(n, s < this.profile.best_stars(level) ? 'island_star_full' : 'island_star_empty', [side / 2 - 66 + s * 44, side - 22, 44, 44]);
    if (state === 'current' && !this.instant) Kit.breathe(n);
    Kit.pop(n, index * .02);
  }
  closeWorld(): void { this.showWorlds(); }
  refresh(): void { if (this.page) this.open_world(this.world); else this.showWorlds(); }
  destroy(): void { Kit.destroy(this.node); }
  static preview(host: Node, profile: Profile, callbacks: MapCallbacks, state = 'cards'): MapView {
    const sample = profile.duplicate_profile(); sample.coins = 1240; sample.highest_unlocked = 8;
    for (let level = 1; level < 8; level++) sample.stars[level] = level % 3 === 0 ? 2 : 3;
    const view = new MapView(host, sample, callbacks, 1, true); if (state.includes('world')) view.open_world(1); return view;
  }
}
