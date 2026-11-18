import { Board, Booster } from '../core/Board';
import { Levels, LevelDefinition } from '../core/Levels';
import { GameSession } from '../core/GameSession';
import type { Profile } from './Profile';
export type RandomSource = () => number;
export type Reward = Record<string, any>;
export class Progress {
  static readonly WORLD_COMPLETE_COINS = 500;
  static readonly FIRST_CLEAR_COINS = [0, 50, 80, 120];
  static readonly REPLAY_COINS = 20;
  static readonly IMPROVE_COINS = 30;
  static readonly UNDO_PRICE = 30;
  static readonly CHEST_STARS = 30;
  static readonly CHEST_COINS = 300;
  static readonly CHEST_BOOSTERS = 3;
  static unlimited = false;
  static readonly DRAW_PRIZES = [[40, 0], [80, 0], [0, 1], [120, 0], [200, 0], [0, 2], [60, 0], [400, 0]];
  static readonly DRAW_MAX_SPINS = 5;
  static readonly BOOSTERS: [number, string, number, number, number][] = [[0, 'firefly', 2, 30, 3], [1, 'sapling', 4, 60, 2], [2, 'owlEye', 0, 80, 2], [3, 'morningBell', 0, 80, 2], [4, 'scarecrow', 0, 100, 2], [5, 'springRain', 0, 100, 2]];
  static readonly WORLD_NAMES = ['', 'Green Forest', 'Bamboo Forest', 'Autumn Forest', 'Snow Forest', 'Blossom Forest', 'Mushroom Forest', 'Lakeside Forest', 'Misty Pine Forest', 'Rainforest', 'Wisteria Forest'];
  static readonly DROP_MILESTONE = 5;
  static readonly DROP_MILESTONE_COUNT = 3;
  static readonly DROP_REPLAY_CHANCE = 0.3;
  static readonly DAILY_REWARDS = [[50, 0], [80, 1], [120, 0], [150, 1], [200, 0], [250, 2], [500, 3]];
  private static booster_levels: number[] = [];
  private static catalog_ref: LevelDefinition[] | null = null;
  static booster_unlock_level(kind: number): number {
    if (this.catalog_ref !== Levels.all()) {
      this.catalog_ref = Levels.all();
      this.booster_levels = [this.BOOSTERS[0][2], this.BOOSTERS[1][2], Levels.COUNT + 1, Levels.COUNT + 1, Levels.COUNT + 1, Levels.COUNT + 1];
      for (const level of Levels.all()) for (const fitting of this.fitting_boosters(level)) this.booster_levels[fitting] = Math.min(this.booster_levels[fitting], level.id);
    }
    return this.booster_levels[kind];
  }
  static booster_price(kind: number): number { return this.BOOSTERS[kind][3]; }
  static world_of_level(level: number): number { return Math.floor((level - 1) / Levels.PER_WORLD) + 1; }
  static is_world_unlocked(p: Profile, world: number): boolean {
    const first = (world - 1) * Levels.PER_WORLD + 1;
    if (first <= p.highest_unlocked || p.best_stars(first - 1) > 0) return true;
    for (let level = first; level <= world * Levels.PER_WORLD; level++) if (p.best_stars(level) > 0) return true;
    return false;
  }
  static is_level_unlocked(p: Profile, level: number): boolean {
    return level >= 1 && level <= Levels.COUNT && (level <= p.highest_unlocked || p.best_stars(level) > 0 || (this.is_world_unlocked(p, this.world_of_level(level)) && p.best_stars(level - 1) > 0));
  }
  static is_world_complete(p: Profile, world: number): boolean {
    for (let level = (world - 1) * Levels.PER_WORLD + 1; level <= world * Levels.PER_WORLD; level++) if (!p.best_stars(level)) return false;
    return true;
  }
  static is_booster_unlocked(p: Profile, kind: number): boolean { return !!p.earned_boosters[kind] || !!p.starters[kind] || p.inventory[kind] > 0 || p.highest_unlocked >= this.booster_unlock_level(kind); }
  static grant_starter_gifts(p: Profile): number[] {
    const granted: number[] = [];
    for (const [kind, , , , count] of this.BOOSTERS) if (this.is_booster_unlocked(p, kind) && !p.starters[kind]) {
      p.inventory[kind] += count; p.starters[kind] = true; granted.push(kind);
    }
    return granted;
  }
  static stars_for_moves(counted: number, par: number): number { return GameSession.stars_for(counted, par); }
  static on_win(p: Profile, level: number, counted_moves: number, par: number, chorus_coins: number, next_def: Partial<LevelDefinition> = {}, rng: RandomSource = Math.random): Reward {
    const stars = this.stars_for_moves(counted_moves, par), old = p.best_stars(level);
    p.stars[level] = Math.max(old, stars); p.clears[level]++;
    p.record_moves(level, counted_moves);
    p.highest_unlocked = Math.max(p.highest_unlocked, Math.min(Levels.COUNT, level + 1));
    const world = this.world_of_level(level), worldBonus = !p.rewarded_worlds[world] && this.is_world_complete(p, world);
    if (worldBonus) p.rewarded_worlds[world] = true;
    const payout: Reward = { stars, first_clear: old === 0 ? this.FIRST_CLEAR_COINS[stars] : 0, replay: old > 0 ? this.REPLAY_COINS : 0, improvement: old > 0 ? this.IMPROVE_COINS * Math.max(0, stars - old) : 0, chorus: chorus_coins, world: worldBonus ? this.WORLD_COMPLETE_COINS : 0 };
    payout.total = payout.first_clear + payout.replay + payout.improvement + payout.chorus + payout.world;
    p.coins += payout.total; payout.starters = this.grant_starter_gifts(p); payout.boosters = this.win_drops(p, level, stars, old === 0, next_def, rng);
    return payout;
  }
  static win_drops(p: Profile, level: number, stars: number, first_clear: boolean, next_def: Partial<LevelDefinition>, rng: RandomSource): number[] {
    const count = first_clear ? level % this.DROP_MILESTONE === 0 ? this.DROP_MILESTONE_COUNT : stars === 3 ? 2 : 1 : rng() < this.DROP_REPLAY_CHANCE ? 1 : 0;
    const fitting = this.fitting_boosters(next_def).filter(kind => this.is_booster_unlocked(p, kind)), pool = this.unlocked_boosters(p), out: number[] = [];
    for (let i = 0; i < count; i++) {
      const choices = fitting.length && (i === 0 || rng() < 0.5) ? fitting : pool;
      const kind = choices[Math.floor(rng() * choices.length)]; p.inventory[kind]++; out.push(kind);
    }
    return out;
  }
  static fitting_boosters(def: Partial<LevelDefinition>): number[] {
    const fitting = new Set<number>();
    for (const branch of def.branches ?? []) {
      if (branch.kind === 'growing' && (branch.seats ?? 0) < def.capacity!) fitting.add(Booster.SPRING_RAIN);
      for (const bird of branch.birds) {
        if (bird.endsWith(':sleep')) fitting.add(Booster.MORNING_BELL);
        else if (bird.startsWith('egg:')) fitting.add(Booster.OWL_EYE);
        else if (bird === 'crow') fitting.add(Booster.SCARECROW);
      }
    }
    return [Booster.MORNING_BELL, Booster.OWL_EYE, Booster.SCARECROW, Booster.SPRING_RAIN].filter(kind => fitting.has(kind));
  }
  static booster_to_point_at(p: Profile, def: LevelDefinition): number { return this.fitting_boosters(def).find(kind => p.inventory[kind] > 0 && !p.has_tutorial(this.used_flag(kind))) ?? -1; }
  static used_flag(kind: number): string { return `used.${this.BOOSTERS[kind][1]}`; }
  static guided_flag(kind: number): string { return `guided.${this.BOOSTERS[kind][1]}`; }
  static guide_unit(p: Profile, kind: number): void { this.grant_starter_gifts(p); if (p.inventory[kind] <= 0) p.inventory[kind] = 1; }
  static unlocked_boosters(p: Profile): number[] {
    const out = this.BOOSTERS.filter(info => this.is_booster_unlocked(p, info[0])).map(info => info[0]);
    return out.length ? out : [Booster.FIREFLY];
  }
  private static random_boosters(p: Profile, count: number, rng: RandomSource): number[] {
    const pool = this.unlocked_boosters(p), out: number[] = [];
    for (let i = 0; i < count; i++) { const kind = pool[Math.floor(rng() * pool.length)]; p.inventory[kind]++; out.push(kind); }
    return out;
  }
  static chest_progress(p: Profile): number { return p.total_stars() - p.chests_opened * this.CHEST_STARS; }
  static chests_ready(p: Profile): number { return Math.max(this.unlimited ? 1 : 0, Math.floor(this.chest_progress(p) / this.CHEST_STARS)); }
  static open_chest(p: Profile, rng: RandomSource = Math.random): Reward {
    if (this.chests_ready(p) <= 0) return {};
    if (this.chest_progress(p) >= this.CHEST_STARS) p.chests_opened++;
    p.coins += this.CHEST_COINS; return { coins: this.CHEST_COINS, boosters: this.random_boosters(p, this.CHEST_BOOSTERS, rng) };
  }
  static daily_reward_available(p: Profile, today: string): boolean { return this.checkin_day(p, today) > 0; }
  static daily_reward_day(p: Profile, today: string): number { return this.checkin_day(p, today) || Math.min(7, Math.max(1, p.checkin_streak)); }
  static claim_daily_reward(p: Profile, today: string, rng: RandomSource = Math.random): Reward {
    const day = this.checkin_day(p, today); if (!day) return {};
    p.last_daily_gift = today; p.checkin_streak = day;
    const prize = this.DAILY_REWARDS[day - 1]; p.coins += prize[0];
    return { day, coins: prize[0], boosters: this.random_boosters(p, prize[1], rng) };
  }
  static checkin_day(p: Profile, today: string): number {
    if (this.unlimited && today <= p.last_daily_gift) return p.checkin_streak % 7 + 1;
    if (today <= p.last_daily_gift) return 0;
    const yesterday = new Date(Date.parse(today + 'T00:00:00Z') - 86400000).toISOString().slice(0, 10);
    return p.last_daily_gift === yesterday && p.checkin_streak < 7 ? p.checkin_streak + 1 : 1;
  }
  static begin_draw_day(p: Profile, today: string): void {
    if (this.unlimited && p.draws_left <= 0) p.draws_left = this.DRAW_MAX_SPINS;
    if (today <= p.draws_date) return;
    const day = this.checkin_day(p, today) || Math.max(1, p.checkin_streak);
    p.draws_date = today; p.draws_left = Math.min(day, this.DRAW_MAX_SPINS);
  }
  static draws_available(p: Profile, today: string): boolean { return this.unlimited || today > p.draws_date || (today === p.draws_date && p.draws_left > 0); }
  static draw(p: Profile, today: string, rng: RandomSource = Math.random): Reward {
    this.begin_draw_day(p, today);
    if ((!this.unlimited && today < p.draws_date) || p.draws_left <= 0) return {};
    if (!this.unlimited) p.draws_left--;
    const cell = Math.floor(rng() * this.DRAW_PRIZES.length), prize = this.DRAW_PRIZES[cell];
    p.coins += prize[0]; let kind = -1;
    if (prize[1] > 0) { const pool = this.unlocked_boosters(p); kind = pool[Math.floor(rng() * pool.length)]; p.inventory[kind] += prize[1]; }
    return { cell, coins: prize[0], booster: kind, count: prize[1] };
  }
  static today(): string {
    const now = new Date(); return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`;
  }
  static next_level(p: Profile): number {
    for (let level = 1; level <= Levels.COUNT; level++) if (!p.best_stars(level) && this.is_level_unlocked(p, level)) return level;
    return 0;
  }
  static resume_level(p: Profile): number {
    const last = p.last_level;
    if (last >= 1 && last <= Levels.COUNT && this.is_level_unlocked(p, last)) return p.best_stars(last) > 0 && last < Levels.COUNT && this.is_level_unlocked(p, last + 1) ? last + 1 : last;
    return this.next_level(p) || Levels.COUNT;
  }
  static node_state(p: Profile, level: number): 'locked' | 'current' | 'cleared' { return p.best_stars(level) > 0 ? 'cleared' : this.is_level_unlocked(p, level) ? 'current' : 'locked'; }
}
