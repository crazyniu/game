import { test } from 'node:test';
import * as assert from 'node:assert/strict';
import { readFileSync, existsSync } from 'node:fs';
import { resolve } from 'node:path';
import { Board, Booster, Kind, Move } from '../assets/scripts/core/Board';
import { Levels, LevelDefinition } from '../assets/scripts/core/Levels';
import { Rules } from '../assets/scripts/core/Rules';
import { GameSession } from '../assets/scripts/core/GameSession';
import { Solver } from '../assets/scripts/core/Solver';
import { Profile, ProfileStorage } from '../assets/scripts/meta/Profile';
import { Progress } from '../assets/scripts/meta/Progress';
import { Shop } from '../assets/scripts/meta/Shop';
import { Aviary } from '../assets/scripts/meta/Aviary';
const resources = existsSync(resolve('assets/resources/levels/levels.json')) ? resolve('assets/resources/levels') : resolve('levels');
Levels.configure(JSON.parse(readFileSync(resolve(resources, 'levels.json'), 'utf8')));
const routes: Record<string, [number[], Move][]> = JSON.parse(readFileSync(resolve(resources, 'hints.json'), 'utf8'));
const random = () => 0.1;
const storage = (): ProfileStorage => {
  const values = new Map<string, string>();
  return { getItem: key => values.get(key) ?? null, setItem: (key, value) => { values.set(key, value); } };
};

test('all 210 authored solutions preserve every hint key and win through a real session', () => {
  assert.equal(Levels.all().length, Levels.COUNT); let moves = 0;
  for (const level of Levels.all()) {
    const session = new GameSession(level); assert.ok(routes[level.id], `level ${level.id} has solution`);
    for (const [key, action] of routes[level.id]) {
      assert.deepEqual(session.state.search_key(false), key, `level ${level.id}, move ${session.state.moves + 1}`);
      assert.ok(session.try_action(action), `level ${level.id}, ${action}`); moves++;
    }
    assert.ok(session.state.is_won(), `level ${level.id} wins`);
    assert.ok(Math.max(...session.state.rows) < [4, 5, 6][Math.min(Levels.world_of(level) - 1, 2)], `level ${level.id} fits the painting`);
  }
  console.log(`Replayed ${Levels.COUNT} levels and ${moves} authored actions.`);
});

test('undo and restart restore board without refilling undo allowance or losing purchases', () => {
  const session = new GameSession(Levels.get_level(1), [2, 1, 0, 0, 0, 0]), initial = session.state.search_key();
  assert.ok(session.try_action(routes['1'][0][1])); session.grant_booster(Booster.FIREFLY, 3); assert.ok(session.undo());
  assert.deepEqual(session.state.search_key(), initial); assert.equal(session.state.inventory[0], 5);
  const remaining = session.undos_left; session.restart();
  assert.equal(session.undos_left, remaining); assert.equal(session.state.inventory[0], 5); assert.ok(!session.can_undo());
});

test('hints prove a playable continuation and asynchronous search can be cancelled', async () => {
  const session = new GameSession(Levels.get_level(10));
  const hint = await Solver.hint_async(session.state); assert.equal(hint.status, Solver.Status.SOLVED);
  for (const move of hint.moves) assert.ok(session.try_action(move)); assert.ok(session.state.is_won());
  assert.equal((await Solver.hint_async(Levels.create_board(Levels.get_level(50)), [], 24000, 1500, [true])).status, Solver.Status.BUDGET_EXCEEDED);
});

test('no-effect tools preserve stock and added branches are undoable', () => {
  const session = new GameSession(Levels.get_level(1), [1, 2, 1, 1, 1, 1]), before = session.state.branch_count();
  assert.equal(session.use_booster(Booster.MORNING_BELL).applied, false); assert.equal(session.state.inventory[3], 1);
  assert.ok(session.use_booster(Booster.SAPLING).applied); assert.equal(session.state.branch_count(), before + 1);
  assert.equal(session.use_booster(Booster.SAPLING).applied, false); assert.equal(session.state.inventory[1], 1);
  assert.ok(session.undo()); assert.equal(session.state.branch_count(), before); assert.equal(session.state.inventory[1], 2);
});

test('packed enums, eggs, sleep, crows and partial goals retain their rules', () => {
  assert.deepEqual([Kind.NORMAL, Kind.TWIG, Kind.GROWING, Kind.SWIVEL], [0, 1, 2, 3]);
  const special: LevelDefinition = { id: 999, capacity: 2, par: 4, branches: [
    { side: 'L', row: 0, birds: ['sunny', 'sunny:sleep'] }, { side: 'R', row: 0, birds: ['egg:sky', 'sky'] },
    { side: 'L', row: 1, birds: ['crow'] }, { side: 'R', row: 1, kind: 'growing', seats: 2, birds: [] },
  ] };
  const board = Levels.create_board(special), events: any[] = [], rules = new Rules(); assert.equal(board.run_count(0), 0);
  rules.booster(board, Booster.MORNING_BELL, false, events); assert.ok(events.some(e => e.type === 'woke')); assert.ok(board.homed.includes(0));
  rules.booster(board, Booster.SCARECROW, false, events); assert.ok(events.some(e => e.type === 'crows_left'));
  assert.ok(Rules.can_transfer(board, 1, 2)); rules.transfer(board, 1, 2, events);
  assert.ok(events.some(e => e.type === 'hatched')); assert.ok(board.is_won());
  const partial = Levels.create_board({ id: 1000, capacity: 2, par: 2, goal: ['sunny'], branches: [{ side: 'L', row: 0, birds: ['sunny'] }, { side: 'R', row: 0, birds: ['sunny'] }, { side: 'L', row: 1, birds: ['sky'] }] });
  rules.transfer(partial, 0, 1); assert.ok(partial.is_won());
});

test('profile migration retains puzzle records, resume level and current save keys', () => {
  const source = new Array(41).fill(0); source[9] = 2;
  const mapped = Profile.LEVEL_SPREAD[Profile.LEVEL_DIFFICULTY[Profile.LEVEL_EXPANSION[Profile.LEVEL_RENUMBER[9]]]];
  const p = Profile.from_dict({ schemaVersion: 1, bestStars: source, clearCounts: source, bestMoves: source, lastLevel: 9, coins: 1234, boosters: { firefly: 8 } });
  assert.equal(p.best_stars(mapped), 2); assert.equal(p.best_moves[mapped], 2); assert.equal(p.last_level, mapped);
  assert.equal(p.total_stars(), 2); assert.equal(p.coins, 1234); assert.equal(p.inventory[0], 8);
  const spread = Profile.from_dict({ schemaVersion: 4, bestStars: Array.from({ length: 101 }, (_, n) => n === 12 ? 3 : 0), highestUnlockedLevel: 13, lastLevel: 12 });
  assert.equal(spread.best_stars(24), 3); assert.equal(spread.last_level, 24); assert.equal(spread.highest_unlocked, 26);
  assert.deepEqual(Profile.from_dict(p.to_dict()).to_dict(), p.to_dict());
});

test('save reload restores whole profile, recovers backup and preserves damaged originals', () => {
  const store = storage(), p = new Profile(); p.coins = 100; p.last_level = 8; p.language = 'zh'; p.save(store); p.coins = 200; p.save(store);
  assert.equal(Profile.load_saved(store).coins, 200); store.setItem(Profile.SAVE_PATH, '{bad json'); assert.equal(Profile.load_saved(store).coins, 100);
  store.setItem(Profile.SAVE_PATH + '.bak', 'broken'); const broken = Profile.load_saved(store);
  assert.ok(broken.needs_recovery()); assert.throws(() => broken.save(store)); assert.equal(store.getItem(Profile.SAVE_PATH), '{bad json');
});

test('wins pay improvement and world rewards once and keep best moves', () => {
  const p = new Profile(); let payout = Progress.on_win(p, 1, 5, 2, 0, Levels.get_level(2), random);
  assert.equal(payout.stars, 2); assert.equal(payout.first_clear, Progress.FIRST_CLEAR_COINS[2]); assert.equal(payout.boosters.length, 1);
  payout = Progress.on_win(p, 1, 1, 2, 0, Levels.get_level(2), random);
  assert.equal(payout.first_clear, 0); assert.equal(payout.improvement, Progress.IMPROVE_COINS); assert.equal(p.best_moves[1], 1);
  for (let level = 2; level <= Levels.PER_WORLD; level++) payout = Progress.on_win(p, level, 1, 2, 0, {}, random);
  assert.equal(payout.world, Progress.WORLD_COMPLETE_COINS); assert.equal(Progress.on_win(p, Levels.PER_WORLD, 1, 2, 0, {}, random).world, 0);
  assert.ok(Progress.is_world_unlocked(p, 2));
});

test('daily rewards, lucky draws and star chests cannot be claimed twice', () => {
  const p = new Profile();
  for (let day = 1; day <= 7; day++) {
    const date = `2026-02-0${day}`, reward = Progress.claim_daily_reward(p, date, random);
    assert.equal(reward.day, day); assert.equal(reward.coins, Progress.DAILY_REWARDS[day - 1][0]); assert.deepEqual(Progress.claim_daily_reward(p, date, random), {});
    let draws = 0; while (Object.keys(Progress.draw(p, date, random)).length) draws++; assert.equal(draws, Math.min(day, Progress.DRAW_MAX_SPINS));
  }
  assert.equal(Progress.checkin_day(p, '2026-02-08'), 1); assert.equal(Progress.checkin_day(p, '2026-02-10'), 1);
  for (let level = 1; level <= Progress.CHEST_STARS / 3; level++) p.stars[level] = 3;
  assert.equal(Progress.chests_ready(p), 1); assert.equal(Progress.open_chest(p, random).coins, Progress.CHEST_COINS); assert.deepEqual(Progress.open_chest(p, random), {});
});

test('shop purchases, equipped cosmetics, discovery and resume survive serialization', () => {
  const p = new Profile(); p.coins = 10000; p.highest_unlocked = Levels.COUNT; const before = p.coins;
  assert.equal(Shop.purchase(p, 'booster.firefly.x3', 2), ''); assert.equal(p.inventory[0], 6); assert.equal(p.coins, before - Shop.item('booster.firefly.x3')!.price * 2);
  assert.equal(Shop.purchase(p, 'booster.firefly', 0.5), 'quantity');
  assert.equal(Shop.purchase(p, 'outfit.sunny.1'), ''); assert.equal(Shop.equip(p, 'outfit.sunny.1'), '');
  assert.equal(Shop.purchase(p, 'house.mushroom'), ''); assert.equal(Shop.equip(p, 'house.mushroom'), '');
  assert.deepEqual(Aviary.on_level_started(p, ['sunny', 'sky']), ['sunny', 'sky']); assert.deepEqual(Aviary.on_level_started(p, ['sunny', 'sky']), []);
  p.last_level = 18; assert.equal(Progress.resume_level(p), 18); p.stars[18] = 3; assert.equal(Progress.resume_level(p), 19);
  const restored = Profile.from_dict(p.to_dict()); assert.equal(restored.equipped.sunny, 'outfit.sunny.1'); assert.equal(restored.equipped_house, 'mushroom');
});
