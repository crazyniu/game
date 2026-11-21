import { Node } from 'cc';
import { UI, Rect } from './UI';
import { Kit, Popups, Dialog } from './Popups';
import { Flat, ON_LIGHT, onTone } from './Flat';
import { tr, word } from './I18n';
import { Worlds } from '../meta/Worlds';

/** One daily task: done of target; claimed once its points were credited. */
export interface TaskRow { id: string; done: number; target: number; points: number; claimed: boolean }
/** A chest on a points bar; outfit (`<bird>_outfit_<n>`) is a skin prize. */
export interface TaskChest { at: number; coins: number; boosters: number[]; outfit?: string; opened: boolean }
export interface TaskBoard { points: number; chests: TaskChest[]; week: number; weekChests: TaskChest[]; tasks: TaskRow[] }
export interface TaskActions {
  board(): TaskBoard;
  /** Open a reached chest; returns what it held. */
  open(weekly: boolean, index: number): TaskChest;
  /** Go and do a task (play the current level, or open the lucky draw). */
  go(id: string): void;
}

const ICON: Record<string, [string, Rect?]> = {
  levels: ['pic_map', [18, 36, 226, 177]], stars: ['island_star_full'], flocks: ['pic_aviary', [21, 49, 200, 164]], combos: [''],
  tools: ['island_icon_firefly'], draw: ['icon_home_draw', [175, 75, 958, 972]],
};
const NAVY = '#1F56B0';

/**
 * The task center (fishsort's docs/ux/tasks.md): one white panel switching between today and this week, its points, and a bar with the
 * chests spaced evenly along it; below, the six tasks as cards in two columns. Unfinished cards are blue with a progress bar and Go;
 * finished ones are white with a tick and move to the end. Points are credited when a task is done; only chests are opened by hand.
 */
export class TaskView {
  private body: Node;
  private weekly = false;
  constructor(private popups: Popups, d: Dialog, private actions: TaskActions) {
    this.body = UI.group(d.content, 'Body', [0, 0, 1080, 1920]);
    this.build();
  }
  static open(popups: Popups, actions: TaskActions, onClose?: () => void): Dialog {
    const d = Kit.page(popups.host, 'Tasks', 'tasks_title', undefined, onClose);
    new TaskView(popups, d, actions);
    return d;
  }
  private build(): void {
    UI.clear(this.body);
    const b = this.actions.board();
    this.panel(b);
    const order = b.tasks.slice().sort((x, y) => Number(x.claimed) - Number(y.claimed));
    order.forEach((task, i) => this.card(task, [43 + (i % 2) * 509, 700 + Math.floor(i / 2) * 404, 485, 380], i));
  }
  private panel(b: TaskBoard): void {
    const box = UI.group(this.body, 'Points', [43, 290, 994, 380]), points = this.weekly ? b.week : b.points, chests = this.weekly ? b.weekChests : b.chests;
    Flat.card(box, 'f_card', [0, 0, 994, 380], 240, 116);
    // Today | This week; a red dot on a side with a chest to open
    const seg = UI.group(box, 'Switch', [36, 30, 460, 92]);
    Flat.plateIn(seg, 'f_bar_track_light', [0, 0, 460, 92]);
    Flat.plate(seg, 'j_btn_blue', [this.weekly ? 230 : 0, 0, 230, 92]);
    ([['UI_TODAY', false], ['UI_WEEK', true]] as [string, boolean][]).forEach(([key, weekly], i) => {
      Flat.text(seg, tr(key), [i * 230, 0, 230, 86], 40, weekly === this.weekly ? onTone('blue') : { color: '#7E9CCB' });
      UI.hit_area(seg, 'Tab_' + i, [i * 230, 0, 230, 92], () => { if (this.weekly !== weekly) { this.weekly = weekly; this.build(); } });
      const list = weekly ? b.weekChests : b.chests, have = weekly ? b.week : b.points;
      if (list.some(c => !c.opened && have >= c.at)) Flat.dot(seg, i * 230 + 216, 12, 34);
    });
    const max = chests[chests.length - 1].at;
    Flat.text(box, `${Math.min(points, max)}/${max}`, [560, 30, 398, 92], 50, ON_LIGHT, 1);
    // The bar: chests evenly spaced, the fill measured by tier (fishsort: 80 and 100 would crowd together by value).
    const x0 = 70, w = 840, y = 236;
    let fill = 0, prev = 0;
    for (let i = 0; i < chests.length; i++) { if (points >= chests[i].at) { fill = (i + 1) / chests.length; prev = chests[i].at; continue; } fill = (i + (points - prev) / (chests[i].at - prev)) / chests.length; break; }
    const track = UI.group(box, 'Bar', [x0, y - 19, w, 38]);
    Flat.plateIn(track, 'f_bar_track_light', [0, 0, w, 38]);
    if (fill > 0) Flat.plate(track, 'f_bar_yellow', [0, 0, Math.max(38, w * fill), 38]);
    chests.forEach((chest, i) => {
      const cx = x0 + w * (i + 1) / chests.length, ready = !chest.opened && points >= chest.at;
      const slot = UI.hit_area(box, 'Chest' + i, [cx - 66, y - 92, 132, 132], () => ready ? this.open(i) : this.preview(chest));
      const prize = chest.outfit && !chest.opened
        ? Kit.liveBird(slot, chest.outfit.split('_outfit_')[0], [6, 0, 120, 128], chest.outfit).node
        : UI.art(slot, chest.opened ? 'pic_chest_open' : 'pic_chest', [10, 14, 112, 104]);
      if (!ready && !chest.opened) UI.opacity(prize, .6);
      if (ready) { Kit.breathe(prize); Flat.dot(slot, 116, 14, 34); }
      if (chest.opened) UI.check_badge(slot, [104, 100]);
      Flat.text(box, String(chest.at), [cx - 70, y + 50, 140, 56], 40, ON_LIGHT);
    });
  }
  private card(task: TaskRow, r: Rect, index: number): void {
    const done = task.done >= task.target, n = UI.group(this.body, 'Task_' + task.id, r), ink = done ? ON_LIGHT : onTone('plate');
    if (done) Flat.card(n, 'f_card', [0, 0, r[2], r[3]], 240, 116); else Flat.card(n, 'f_block', [0, 0, r[2], r[3]], 240, 110);
    const [icon, content] = ICON[task.id];
    if (icon) UI.art(n, icon, [r[2] / 2 - 66, 22, 132, 132], content);
    else Flat.text(n, '×2', [r[2] / 2 - 80, 30, 160, 116], 96, { color: '#FFD640', outline: '#C47400' });
    // the points on an amber tag at the icon's corner
    const tag = UI.group(n, 'Points', [r[2] / 2 + 46, 24, 120, 58]);
    Flat.plateIn(tag, 'j_btn_amber', [0, 0, 120, 58]);
    Flat.text(tag, '+' + task.points, [8, Flat.face(58) - 24, 104, 48], 36, onTone('amber'));
    Flat.text(n, word('task_' + task.id), [20, 166, r[2] - 40, 64], 46, ink);
    if (done) { Flat.image(n, 'f_check', [r[2] / 2 - 50, 250, 100, 100]); Kit.pop(n, index * .05); return; }
    const bar = UI.group(n, 'Progress', [40, 240, r[2] - 80, 44]);
    Flat.plateIn(bar, 'f_bar_track', [0, 0, r[2] - 80, 44]);
    if (task.done > 0) Flat.plate(bar, 'f_bar_yellow', [0, 0, Math.max(44, (r[2] - 80) * task.done / task.target), 44]);
    Flat.text(bar, `${task.done}/${task.target}`, [0, 0, r[2] - 80, 44], 34, { color: '#FFFFFF', outline: '#0B2A73' });
    Flat.button(n, 'Go', [r[2] / 2 - 120, 300, 240, 64], 'green', tr('TASK_GO'), () => this.actions.go(task.id), 36);
    Kit.pop(n, index * .05);
  }
  private open(index: number): void {
    const chest = this.actions.open(this.weekly, index);
    this.build();
    if (chest.outfit) this.popups.outfit_gift(chest.outfit);
    else this.popups.rewards('tasks.title', chest.coins, chest.boosters);
  }
  /** A chest not reached yet: show what it holds. */
  private preview(chest: TaskChest): void {
    if (chest.opened) return;
    if (chest.outfit) this.popups.outfit_gift(chest.outfit);
    else this.popups.rewards('tasks.title', chest.coins, chest.boosters, () => {}, 'pic_chest');
  }

  /** Fake data for the screen check: some tasks done, today's first chest reached, the week's skin prize ahead. */
  static preview(popups: Popups, state = 'tasks'): Dialog {
    const outfit = Worlds.birds(2)[0] + '_outfit_1';
    const board: TaskBoard = {
      points: 60, week: 260,
      chests: [{ at: 20, coins: 60, boosters: [], opened: true }, { at: 50, coins: 60, boosters: [0], opened: false }, { at: 80, coins: 120, boosters: [], opened: false }, { at: 100, coins: 100, boosters: [0, 1], opened: false }],
      weekChests: [{ at: 150, coins: 200, boosters: [], opened: true }, { at: 350, coins: 0, boosters: [0, 0, 1], opened: false }, { at: 550, coins: 0, boosters: [], outfit, opened: false }],
      tasks: [
        { id: 'levels', done: 3, target: 3, points: 20, claimed: true }, { id: 'stars', done: 6, target: 6, points: 20, claimed: true },
        { id: 'flocks', done: 12, target: 20, points: 20, claimed: false }, { id: 'combos', done: 5, target: 5, points: 20, claimed: true },
        { id: 'tools', done: 0, target: 2, points: 20, claimed: false }, { id: 'draw', done: 0, target: 1, points: 20, claimed: false },
      ],
    };
    if (state === 'tasks_week') board.week = 560;
    const actions: TaskActions = {
      board: () => board,
      open: (weekly, i) => { const c = (weekly ? board.weekChests : board.chests)[i]; c.opened = true; return c; },
      go: () => {},
    };
    return TaskView.open(popups, actions);
  }
}
