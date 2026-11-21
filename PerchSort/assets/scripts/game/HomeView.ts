import { Node, Vec3, tween } from 'cc';
import { UI, Rect } from './UI';
import { Kit } from './Popups';
import { Flat, ON_LIGHT, onTone } from './Flat';
import { tr } from './I18n';
import { Profile } from '../meta/Profile';
import { Progress } from '../meta/Progress';
import { Worlds } from '../meta/Worlds';
import { Tasks } from '../meta/Tasks';
import { Levels } from '../core/Levels';

export interface HomeCallbacks { play: (level: number) => void; map: () => void; shop: () => void; aviary: () => void; settings: () => void; daily: () => void; draw: () => void; tasks: () => void; chest: () => void; coins: () => void; freeCoins: () => boolean }

/**
 * Home in fishsort's flat style: the feather capsule on top, the Bird Sort sign, daily things down the left (check-in, lucky draw, star
 * chest) and standing systems down the right (shop, album, tasks), each a glass tile with a blue label plate, one branch with three birds
 * of the world being played between the columns; Settings, Play (with the level) and Levels along the bottom. Entries pop in one after another.
 */
export class HomeView {
  node: Node;
  constructor(host: Node, private profile: Profile, private callbacks: HomeCallbacks, private instant = false) { this.node = UI.group(host, 'Home'); this.build(); }
  private build(): void {
    const p = this.node, level = Progress.resume_level(this.profile), world = Levels.world_of(Levels.get_level(level)), today = Progress.today();
    UI.image(p, `board_w${world}${world === 1 || world === 10 ? '_open' : ''}_bg`, [0, 0, 1080, 1920]);
    const left: [string, string, Rect | undefined, string, () => void, boolean][] = [
      ['DailyReward', 'pic_calendar', [49, 39, 154, 194], 'H_FEAT_SIGNIN', this.callbacks.daily, Progress.daily_reward_available(this.profile, today)],
      ['LuckyDraw', 'icon_home_draw', [175, 75, 958, 972], 'H_DRAW', this.callbacks.draw, Progress.draws_available(this.profile, today)],
      ['Chest', 'pic_chest', [36, 50, 185, 170], 'H_CHEST', this.callbacks.chest, Progress.chests_ready(this.profile) > 0],
    ];
    const right: typeof left = [
      ['Shop', 'pic_shop', [18, 17, 196, 221], 'UI_TAB_SHOP', this.callbacks.shop, false],
      ['Aviary', 'pic_aviary', [21, 49, 200, 164], 'H_ALBUM', this.callbacks.aviary, false],
      ['Tasks', 'pic_tasks', undefined, 'UI_TAB_TASKS', this.callbacks.tasks, Tasks.ready(this.profile, today) > 0],
    ];
    UI.image(p, 'panel_home_title_sign', [220, 120, 640, 476]);
    this.capsule();
    this.birds(world);
    left.forEach((e, i) => this.entry(e, 122, 380 + i * 270, i * .08));
    right.forEach((e, i) => this.entry(e, 958, 380 + i * 270, .04 + i * .08));
    this.bottom(level);
  }
  private capsule(): void { Flat.capsule(this.node, [32, 64, 320, 88], this.profile.coins, this.callbacks.freeCoins() ? this.callbacks.coins : undefined); }
  private entry([name, icon, content, word, action, dot]: [string, string, Rect | undefined, string, () => void, boolean], cx: number, top: number, delay: number): void {
    const n = UI.hit_area(this.node, name, [cx - 90, top, 180, 194], action);
    Flat.image(n, 'f_card_glass', [10, 0, 161, 161]);
    UI.art(n, icon, [25, 8, 130, 130], content);
    const plate = UI.group(n, 'Label', [0, 136, 180, 58]);
    Flat.plateIn(plate, 'f_plate_blue', [0, 0, 180, 58]);
    Flat.text(plate, tr(word), [10, Flat.face(58) - 25, 160, 50], 35, onTone('plate'));
    if (dot) Flat.dot(n, 164, 8);
    Kit.pop(n, delay);
  }
  /** One branch between the entry columns with three birds of the world being played (with their skins); a tap makes one perform. */
  private birds(world: number): void {
    Kit.perch(this.node, [540, 1110], 680);
    (['sky', 'sunny', 'berry'] as const).forEach((slot, i) => {
      const species = Worlds.look(world, slot), worn = this.profile.equipped[species], r: Rect = [240 + i * 200, 870, 200, 240];
      const bird = Kit.liveBird(this.node, species, r, worn ? `${species}_outfit_${worn.split('.')[2]}` : '');
      UI.hit_area(this.node, 'Bird_' + slot, r, () => bird.showcase());
    });
  }
  private bottom(level: number): void {
    const y = 1920 - 72 - 225, h = 225;
    this.key('Settings', [34, y, 202, h], 'f_g_gear', 'UI_SETTINGS', this.callbacks.settings);
    const holder = UI.group(this.node, 'PlayPivot', [540, y + h / 2, 0, 0]);
    const play = Flat.button(holder, 'Play', [-281, -h / 2, 562, h], 'green', '', () => this.callbacks.play(level));
    const face = Flat.face(h);
    Flat.text(play, tr('UI_PLAY'), [40, face - 32 - 60, 482, 120], 95, onTone('green'));
    Flat.text(play, tr('UI_LEVEL', level), [40, face + 55 - 30, 482, 60], 43, { color: '#E9FFE0', depth: '#268F1B' });
    if (!this.instant) tween(holder).repeatForever(tween().to(1.2, { scale: new Vec3(1.03, 1.03, 1) }, { easing: 'sineInOut' }).to(1.2, { scale: new Vec3(1, 1, 1) }, { easing: 'sineInOut' })).start();
    this.key('Levels', [844, y, 202, h], 'pic_map', 'H_FEAT_LEVELS', this.callbacks.map);
  }
  /** A blue key with a picture above its word. */
  private key(name: string, r: Rect, icon: string, word: string, action: () => void): void {
    const n = Flat.button(this.node, name, r, 'blue', '', action);
    if (icon.startsWith('f_g_')) Flat.image(n, icon, [r[2] / 2 - 52, 22, 104, 104]);
    else UI.art(n, icon, [r[2] / 2 - 62, 14, 124, 116], [18, 36, 226, 177]);
    Flat.text(n, tr(word), [10, 132, r[2] - 20, 56], 35, onTone('blue'));
  }
  refresh(): void { UI.clear(this.node); this.build(); }
  destroy(): void { this.node.destroy(); }
  static preview(host: Node, profile: Profile, callbacks: HomeCallbacks, state = 'home'): HomeView { const p = profile.duplicate_profile(); p.coins = state === 'home_new' ? 0 : 50270; return new HomeView(host, p, callbacks, true); }
}
