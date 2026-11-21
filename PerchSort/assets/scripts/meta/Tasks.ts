import { Profile } from './Profile';
import { Shop } from './Shop';
import { Worlds } from './Worlds';

/** One day's and one week's task progress (Profile.tasks). */
export interface TaskState { day: string; done: Record<string, number>; claimed: string[]; points: number; chests: number[]; week: string; weekPoints: number; weekChests: number[] }
export interface TaskDef { id: string; target: number; points: number }
/** A chest on a points bar; skin: the prize is a common skin (docs/work-log/task-center/spec.md). */
export interface TaskChest { at: number; coins: number; boosters: number[]; skin?: boolean }
export interface TaskPrize { coins: number; boosters: number[]; outfit: string }

/**
 * The task center (from fishsort's tasks): six daily tasks worth 20 points each, credited when done; today's points open four chests, the week's sum
 * (Monday to Sunday) three more, the last of them a common skin. Nothing needs an ad, so no task is stuck without ads.
 */
export class Tasks {
  static readonly DAILY: TaskDef[] = [
    { id: 'levels', target: 3, points: 20 }, { id: 'stars', target: 6, points: 20 }, { id: 'flocks', target: 20, points: 20 },
    { id: 'combos', target: 5, points: 20 }, { id: 'tools', target: 2, points: 20 }, { id: 'draw', target: 1, points: 20 },
  ];
  static readonly DAY_CHESTS: TaskChest[] = [{ at: 20, coins: 60, boosters: [] }, { at: 50, coins: 60, boosters: [0] }, { at: 80, coins: 120, boosters: [] }, { at: 100, coins: 100, boosters: [0, 1] }];
  static readonly WEEK_CHESTS: TaskChest[] = [{ at: 150, coins: 200, boosters: [] }, { at: 350, coins: 0, boosters: [0, 0, 1] }, { at: 550, coins: 0, boosters: [], skin: true }];
  /** Feathers instead of a skin once every common skin of the met birds is owned. */
  static readonly SKIN_FEATHERS = 500;

  static fresh(): TaskState { return { day: '', done: {}, claimed: [], points: 0, chests: [], week: '', weekPoints: 0, weekChests: [] }; }
  /** The Monday of a date's week (YYYY-MM-DD). */
  static week_of(today: string): string {
    const [y, m, d] = today.split('-').map(Number), date = new Date(Date.UTC(y, m - 1, d));
    date.setUTCDate(date.getUTCDate() - (date.getUTCDay() + 6) % 7);
    return date.toISOString().slice(0, 10);
  }
  /** Start a new day or week when the date moved on. */
  static roll(p: Profile, today: string): TaskState {
    const t = p.tasks;
    if (t.day !== today) { t.day = today; t.done = {}; t.claimed = []; t.points = 0; t.chests = []; }
    const week = this.week_of(today);
    if (t.week !== week) { t.week = week; t.weekPoints = 0; t.weekChests = []; }
    return t;
  }
  /** Add progress; a task that reaches its target credits its points at once (only chests are opened by hand, as in fishsort). */
  static count(p: Profile, id: string, n: number, today: string): void {
    if (n <= 0) return;
    const t = this.roll(p, today), def = this.DAILY.find(d => d.id === id);
    t.done[id] = (t.done[id] ?? 0) + n;
    if (def && this.is_done(p, def) && !t.claimed.includes(id)) { t.claimed.push(id); t.points += def.points; t.weekPoints += def.points; }
  }
  static is_done(p: Profile, def: TaskDef): boolean { return (p.tasks.done[def.id] ?? 0) >= def.target; }
  static chest_ready(p: Profile, weekly: boolean, index: number): boolean {
    const t = p.tasks, list = weekly ? this.WEEK_CHESTS : this.DAY_CHESTS;
    return !(weekly ? t.weekChests : t.chests).includes(index) && (weekly ? t.weekPoints : t.points) >= list[index].at;
  }
  /** Open a reached chest and pay it out. */
  static open(p: Profile, weekly: boolean, index: number, today: string): TaskPrize | null {
    const t = this.roll(p, today), chest = (weekly ? this.WEEK_CHESTS : this.DAY_CHESTS)[index];
    if (!chest || !this.chest_ready(p, weekly, index)) return null;
    (weekly ? t.weekChests : t.chests).push(index);
    const prize: TaskPrize = { coins: chest.coins, boosters: chest.boosters.slice(), outfit: '' };
    if (chest.skin) { prize.outfit = this.give_skin(p); if (!prize.outfit) prize.coins += this.SKIN_FEATHERS; }
    p.coins += prize.coins; for (const kind of prize.boosters) p.inventory[kind]++;
    return prize;
  }
  /** Chests that can open: the home badge. */
  static ready(p: Profile, today: string): number {
    this.roll(p, today);
    return this.DAY_CHESTS.filter((_, i) => this.chest_ready(p, false, i)).length + this.WEEK_CHESTS.filter((_, i) => this.chest_ready(p, true, i)).length;
  }
  /**
   * Give a common skin: the cheapest skin not yet owned of a bird the player has met (of one world when given). Returns
   * `<bird>_outfit_<n>` or '' when there is none.
   */
  static give_skin(p: Profile, world = 0): string {
    const pick = this.next_skin(p, world);
    if (pick) p.outfits[`outfit.${pick.split('_outfit_')[0]}.${pick.split('_outfit_')[1]}`] = true;
    return pick;
  }
  /** The skin give_skin would give, without giving it (shown on the week's last chest). */
  static next_skin(p: Profile, world = 0): string {
    const birds = new Set(world ? Worlds.birds(world) : Object.keys(p.met));
    const pick = Shop.catalog().filter(it => it.kind === Shop.OUTFIT && birds.has(it.species!) && p.has_met(it.species!) && !p.owns_outfit(it.id))
      .sort((a, b) => a.price - b.price)[0];
    return pick ? `${pick.species}_outfit_${pick.index}` : '';
  }
}
