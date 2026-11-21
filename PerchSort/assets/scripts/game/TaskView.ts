import { Node } from 'cc';
import { UI, Rect } from './UI';
import { Kit, Popups, Dialog } from './Popups';
import { Worlds } from '../meta/Worlds';

/** One daily task: done of target, worth points once claimed. */
export interface TaskRow { id: string; done: number; target: number; points: number; claimed: boolean }
/** A chest on a points bar; outfit (`<bird>_outfit_<n>`) is a skin prize. */
export interface TaskChest { at: number; coins: number; boosters: number[]; outfit?: string; opened: boolean }
export interface TaskBoard { points: number; chests: TaskChest[]; week: number; weekChests: TaskChest[]; tasks: TaskRow[] }
export interface TaskActions {
  board(): TaskBoard;
  /** Claim a finished task's points. */
  claim(id: string): void;
  /** Open a reached chest; returns what it held. */
  open(weekly: boolean, index: number): TaskChest;
}

const ICON: Record<string, [string, Rect?]> = {
  levels: ['pic_map', [18, 36, 226, 177]], stars: ['island_star_full'], flocks: ['pic_aviary', [21, 49, 200, 164]], combos: [''],
  tools: ['island_icon_firefly'], draw: ['icon_home_draw', [175, 75, 958, 972]],
};
const WORD: Record<string, string> = { levels: 'task_levels', stars: 'chest.line', flocks: 'task_flocks', combos: 'task_combos', tools: 'task_tools', draw: 'task_draw' };

/** The task center: today's and this week's points bars with their chests, and the daily tasks. */
export class TaskView {
  private body: Node;
  constructor(private popups: Popups, private d: Dialog, private actions: TaskActions) {
    this.body = UI.group(d.content, 'Body', [0, 0, 1080, 1920]);
    this.build();
  }
  static open(popups: Popups, actions: TaskActions, coins: number, onClose?: () => void, onCoins?: () => void): Dialog {
    const d = Kit.page(popups.host, 'Tasks', 'tasks_title', coins, onClose, onCoins);
    new TaskView(popups, d, actions);
    return d;
  }
  private build(): void {
    UI.clear(this.body);
    const b = this.actions.board();
    this.bar(330, 'task_today', b.points, b.chests, false);
    this.bar(650, 'task_week', b.week, b.weekChests, true);
    b.tasks.forEach((task, i) => this.row(980 + i * 150, task));
  }
  private bar(y: number, word: string, points: number, chests: TaskChest[], weekly: boolean): void {
    const box = UI.group(this.body, weekly ? 'Week' : 'Today', [48, y, 984, 300]), max = chests[chests.length - 1].at;
    Kit.panel(box, [0, 0, 984, 300]);
    UI.word(box, word, [52, 22, 400, 60], 'caption', -1);
    const left = 92, width = 820, track = UI.group(box, 'Track', [left, 172, width, 72]);
    UI.nine_region(track, 'panel_chest_progress', [112, 285, 1946, 157], [0, 0, width, 72], [87, 0, 87, 0], 72 / 157);
    const fill = Math.min(1, points / max);
    if (fill > 0) UI.nine_region(track, 'fill_chest_progress', [160, 273, 1846, 177], [8, 8, Math.max(57, (width - 16) * fill), 56], [98, 0, 98, 0], 56 / 177);
    UI.art(box, 'pic_task_point', [left - 56, 160, 96, 96]);
    Kit.number(track, points, [0, 16, width, 40]);
    chests.forEach((chest, i) => {
      const x = left + width * chest.at / max, ready = !chest.opened && points >= chest.at;
      const slot = UI.hit_area(box, 'Chest' + i, [x - 60, 64, 120, 108], () => { if (ready) this.open(weekly, i); });
      if (ready) { const glow = UI.image(slot, 'glow_soft', [-20, -20, 160, 148]); UI.opacity(glow, .8); }
      const prize = chest.outfit
        ? Kit.liveBird(slot, chest.outfit.split('_outfit_')[0], [6, -6, 108, 112], chest.outfit).node
        : UI.art(slot, chest.opened ? 'pic_chest_open' : 'pic_chest', [10, 6, 100, 96]);
      if (chest.opened) { UI.opacity(prize, .5); UI.check_badge(slot, [100, 86]); }
      else if (ready) Kit.breathe(prize);
      UI.number(box, chest.at, [x - 60, 250, 120, 40], 'brown');
    });
  }
  private row(y: number, task: TaskRow): void {
    const done = task.done >= task.target, box = UI.group(this.body, 'Task_' + task.id, [48, y, 984, 134]);
    const face = UI.group(box, 'Face', [0, 0, 984, 134]);
    Kit.panel(face, [0, 0, 984, 134]);
    const [icon, content] = ICON[task.id];
    if (icon) UI.art(face, icon, [24, 17, 100, 100], content);
    else UI.number(face, 'x2', [24, 30, 100, 74], 'gold');
    UI.word(face, WORD[task.id], [148, 16, 440, 60], 'caption', -1);
    UI.number(face, `${Math.min(task.done, task.target)}/${task.target}`, [148, 82, 240, 40], 'brown', -1);
    UI.art(face, 'pic_task_point', [588, 37, 60, 60]);
    UI.number(face, '+' + task.points, [652, 41, 120, 52], 'brown', -1);
    if (task.claimed) { UI.opacity(face, .45); UI.check_badge(box, [900, 67]); return; }
    const claim = Kit.button(box, 'Claim', [776, 15, 192, 104], 'white_claim', () => {
      if (!done) return;
      this.actions.claim(task.id); this.popups.callbacks.sound?.('collect'); this.build();
      const bar = this.body.getChildByName('Today'); if (bar) Kit.pop(bar);
    });
    UI.enabled(claim, done);
  }
  private open(weekly: boolean, index: number): void {
    const chest = this.actions.open(weekly, index);
    this.build();
    if (chest.outfit) this.popups.outfit_gift(chest.outfit);
    else this.popups.rewards('tasks.title', chest.coins, chest.boosters);
  }

  /** The approved-shell data: some tasks done, one claimed, today's first chest reached, the week's skin prize ahead. */
  static preview(popups: Popups, state = 'tasks'): Dialog {
    const outfit = Worlds.birds(2)[0] + '_outfit_1';
    const board: TaskBoard = {
      points: 40, week: 260,
      chests: [{ at: 20, coins: 60, boosters: [], opened: false }, { at: 50, coins: 60, boosters: [1], opened: false }, { at: 80, coins: 120, boosters: [], opened: false }, { at: 100, coins: 100, boosters: [0, 3], opened: false }],
      weekChests: [{ at: 150, coins: 200, boosters: [], opened: true }, { at: 350, coins: 0, boosters: [0, 1, 2], opened: false }, { at: 550, coins: 0, boosters: [], outfit, opened: false }],
      tasks: [
        { id: 'levels', done: 3, target: 3, points: 20, claimed: true }, { id: 'stars', done: 6, target: 6, points: 20, claimed: false },
        { id: 'flocks', done: 12, target: 20, points: 20, claimed: false }, { id: 'combos', done: 5, target: 5, points: 20, claimed: false },
        { id: 'tools', done: 0, target: 2, points: 20, claimed: false }, { id: 'draw', done: 1, target: 1, points: 20, claimed: false },
      ],
    };
    if (state === 'tasks_week') { board.week = 560; board.weekChests[1].opened = true; }
    const actions: TaskActions = {
      board: () => board,
      claim: id => { const t = board.tasks.find(x => x.id === id)!; t.claimed = true; board.points += t.points; board.week += t.points; },
      open: (weekly, i) => { const c = (weekly ? board.weekChests : board.chests)[i]; c.opened = true; return c; },
    };
    return TaskView.open(popups, actions, popups.profile.coins);
  }
}
