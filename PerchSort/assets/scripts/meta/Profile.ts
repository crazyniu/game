import { Levels } from '../core/Levels';
import { Shop } from './Shop';
import { Aviary } from './Aviary';
import { Progress } from './Progress';
export interface ProfileStorage { getItem(key: string): string | null; setItem(key: string, value: string): void }
export type ProfileData = Record<string, any>;
const clamp = (value: number, min: number, max: number): number => Math.min(max, Math.max(min, value));
const int = (value: unknown): number => Math.trunc(Number(value) || 0);

export class Profile {
  static readonly SCHEMA_VERSION = 6;
  static readonly OLD_BOOSTER_LEVELS = [2, 4, 11, 5, 15, 17];
  static readonly SAVE_PATH = 'perch-sort.profile';
  static readonly BOOSTER_IDS = ['firefly', 'sapling', 'owlEye', 'morningBell', 'scarecrow', 'springRain'];
  static readonly LEVEL_RENUMBER: Record<number, number> = {1: 1, 2: 2, 3: 3, 4: 6, 5: 7, 6: 5, 7: 10, 8: 8, 9: 4, 10: 9, 11: 11, 12: 17, 13: 16, 14: 13, 15: 12, 16: 14, 17: 18, 18: 15, 19: 20, 20: 19, 21: 21, 22: 25, 23: 24, 24: 22, 25: 27, 26: 26, 27: 28, 28: 23, 29: 29, 30: 30, 31: 31, 32: 35, 33: 32, 34: 36, 35: 33, 36: 38, 37: 37, 38: 40, 39: 34, 40: 39};
  static readonly LEVEL_EXPANSION: Record<number, number> = {1: 1, 2: 2, 3: 3, 4: 4, 5: 5, 6: 6, 7: 7, 8: 10, 9: 35, 10: 9, 11: 11, 12: 21, 13: 28, 14: 22, 15: 24, 16: 15, 17: 12, 18: 37, 19: 39, 20: 46, 21: 31, 22: 41, 23: 48, 24: 34, 25: 33, 26: 44, 27: 53, 28: 67, 29: 58, 30: 72, 31: 51, 32: 61, 33: 71, 34: 66, 35: 60, 36: 43, 37: 42, 38: 62, 39: 64, 40: 63};
  static readonly LEVEL_DIFFICULTY: Record<number, number> = {1: 1, 2: 2, 3: 4, 4: 5, 5: 6, 6: 12, 7: 13, 8: 19, 9: 26, 10: 27, 11: 7, 12: 14, 13: 18, 14: 17, 15: 16, 16: 25, 17: 22, 18: 23, 19: 28, 20: 24, 21: 3, 22: 15, 23: 32, 24: 33, 25: 29, 26: 35, 27: 34, 28: 43, 29: 39, 30: 36, 31: 8, 32: 37, 33: 30, 34: 20, 35: 41, 36: 45, 37: 42, 38: 40, 39: 46, 40: 50, 41: 11, 42: 21, 43: 38, 44: 31, 45: 44, 46: 47, 47: 55, 48: 49, 49: 54, 50: 56, 51: 9, 52: 52, 53: 51, 54: 59, 55: 61, 56: 57, 57: 65, 58: 62, 59: 66, 60: 67, 61: 10, 62: 53, 63: 60, 64: 48, 65: 58, 66: 73, 67: 64, 68: 70, 69: 68, 70: 76, 71: 63, 72: 69, 73: 71, 74: 72, 75: 74, 76: 75, 77: 81, 78: 78, 79: 84, 80: 86, 81: 80, 82: 77, 83: 88, 84: 85, 85: 83, 86: 87, 87: 94, 88: 82, 89: 91, 90: 93, 91: 97, 92: 79, 93: 95, 94: 89, 95: 90, 96: 92, 97: 98, 98: 99, 99: 96, 100: 100};
  static readonly LEVEL_SPREAD: Record<number, number> = {1: 1, 2: 3, 3: 5, 4: 7, 5: 9, 6: 11, 7: 13, 8: 15, 9: 17, 10: 19, 11: 22, 12: 24, 13: 26, 14: 28, 15: 30, 16: 32, 17: 34, 18: 36, 19: 38, 20: 40, 21: 43, 22: 45, 23: 47, 24: 49, 25: 51, 26: 53, 27: 55, 28: 57, 29: 59, 30: 61, 31: 64, 32: 66, 33: 68, 34: 70, 35: 72, 36: 74, 37: 76, 38: 78, 39: 80, 40: 82, 41: 85, 42: 87, 43: 89, 44: 91, 45: 93, 46: 95, 47: 97, 48: 99, 49: 101, 50: 103, 51: 106, 52: 108, 53: 110, 54: 112, 55: 114, 56: 116, 57: 118, 58: 120, 59: 122, 60: 124, 61: 127, 62: 129, 63: 131, 64: 133, 65: 135, 66: 137, 67: 139, 68: 141, 69: 143, 70: 145, 71: 148, 72: 150, 73: 152, 74: 154, 75: 156, 76: 158, 77: 160, 78: 162, 79: 164, 80: 166, 81: 169, 82: 171, 83: 173, 84: 175, 85: 177, 86: 179, 87: 181, 88: 183, 89: 185, 90: 187, 91: 190, 92: 192, 93: 194, 94: 196, 95: 198, 96: 200, 97: 202, 98: 204, 99: 206, 100: 208};
  coins = 0;
  highest_unlocked = 1;
  inventory = [0, 0, 0, 0, 0, 0];
  stars = new Array<number>(Levels.COUNT + 1).fill(0);
  clears = new Array<number>(Levels.COUNT + 1).fill(0);
  best_moves = new Array<number>(Levels.COUNT + 1).fill(0);
  outfits: Record<string, boolean> = {};
  equipped: Record<string, string> = {};
  houses: Record<string, boolean> = {};
  equipped_house = 'default';
  met: Record<string, boolean> = {};
  homes: Record<string, number> = {};
  last_daily_gift = '';
  tutorials: Record<string, boolean> = {};
  starters: Record<number, boolean> = {};
  earned_boosters: Record<number, boolean> = {};
  rewarded_worlds: Record<number, boolean> = {};
  muted = false;
  music_muted = false;
  level_gifts: Record<number, boolean> = {};
  chests_opened = 0;
  draws_date = '';
  draws_left = 0;
  checkin_streak = 0;
  challenge_date = '';
  vibration = true;
  language = '';
  last_level = 0;
  private blocked_save_key = '';
  needs_recovery(): boolean { return !!this.blocked_save_key; }
  best_stars(level: number): number { return this.stars[level] ?? 0; }
  owns_outfit(id: string): boolean { return !!this.outfits[id]; }
  owns_house(id: string): boolean { return !!this.houses[id]; }
  has_met(species: string): boolean { return !!this.met[species]; }
  flocks_home(species: string): number { return this.homes[species] ?? 0; }
  has_tutorial(flag: string): boolean { return !!this.tutorials[flag]; }
  set_tutorial(flag: string): void { this.tutorials[flag] = true; }
  record_moves(level: number, moves: number): void { if (!this.best_moves[level] || moves < this.best_moves[level]) this.best_moves[level] = moves; }
  clears_count(): number { return this.clears.reduce((total, n) => total + n, 0); }
  total_stars(): number { return this.stars.reduce((total, n) => total + n, 0); }
  duplicate_profile(): Profile { return Profile.from_dict(this.to_dict()); }
  to_dict(): ProfileData {
    return {
      schemaVersion: Profile.SCHEMA_VERSION, coins: this.coins, highestUnlockedLevel: this.highest_unlocked,
      boosters: Object.fromEntries(Profile.BOOSTER_IDS.map((id, i) => [id, this.inventory[i]])),
      bestStars: this.stars.slice(), clearCounts: this.clears.slice(), bestMoves: this.best_moves.slice(),
      ownedOutfits: Object.keys(this.outfits).sort(), equippedOutfits: { ...this.equipped },
      ownedBirdhouseStyles: Object.keys(this.houses).sort(), equippedBirdhouseStyle: this.equipped_house,
      speciesMet: Object.keys(this.met).sort(), flocksHome: { ...this.homes }, lastDailyGiftDate: this.last_daily_gift,
      tutorialFlags: Object.keys(this.tutorials).sort(), starterGiftsGranted: Profile.BOOSTER_IDS.filter((_, i) => this.starters[i]),
      earnedBoosters: Profile.BOOSTER_IDS.filter((_, kind) => Progress.is_booster_unlocked(this, kind)),
      worldRewardsGranted: Object.keys(this.rewarded_worlds).map(Number).sort((a, b) => a - b),
      muted: this.muted, musicMuted: this.music_muted,
      levelGifts: Object.keys(this.level_gifts).map(Number).sort((a, b) => a - b), chestsOpened: this.chests_opened,
      checkinStreak: this.checkin_streak, drawDate: this.draws_date, drawsLeft: this.draws_left,
      challengeDate: this.challenge_date, vibration: this.vibration, language: this.language, lastLevel: this.last_level,
    };
  }
  static from_dict(data: ProfileData): Profile {
    const p = new Profile();
    p.coins = Math.max(0, int(data.coins)); p.highest_unlocked = clamp(int(data.highestUnlockedLevel ?? 1), 1, Levels.COUNT);
    p.muted = !!data.muted; p.music_muted = !!(data.musicMuted ?? p.muted); p.vibration = data.vibration ?? true;
    const language = String(data.language ?? ''); p.language = /^[a-z-]{2,8}$/.test(language) ? language : '';
    p.inventory = this.BOOSTER_IDS.map(id => Math.max(0, int(data.boosters?.[id])));
    let stars: number[] = data.bestStars ?? [], clears: number[] = data.clearCounts ?? [], moves: number[] = data.bestMoves ?? [], gifts: number[] = data.levelGifts ?? [];
    const schema = int(data.schemaVersion ?? 1);
    let last = int(data.lastLevel);
    const steps: [number, Record<number, number>][] = [[2, this.LEVEL_RENUMBER], [3, this.LEVEL_EXPANSION], [4, this.LEVEL_DIFFICULTY], [5, this.LEVEL_SPREAD]];
    for (const [version, table] of steps) if (schema < version) {
      stars = this.renumbered(stars, table); clears = this.renumbered(clears, table); moves = this.renumbered(moves, table);
      let unlocked = 1;
      for (const [old, current] of Object.entries(table)) if (Number(old) <= p.highest_unlocked) unlocked = Math.max(unlocked, current);
      p.highest_unlocked = unlocked;
      gifts = gifts.filter(old => table[old] !== undefined).map(old => table[old]);
      last = table[last] ?? 0;
    }
    for (let level = 1; level <= Levels.COUNT; level++) {
      const rating = clamp(int(stars[level]), 0, 3);
      p.stars[level] = rating; p.clears[level] = rating ? Math.max(1, int(clears[level])) : 0;
      p.best_moves[level] = rating ? Math.max(0, int(moves[level])) : 0;
      if (rating > 0) p.highest_unlocked = Math.max(p.highest_unlocked, Math.min(Levels.COUNT, level + 1));
    }
    for (const id of data.ownedOutfits ?? []) if (Shop.item(id)?.kind === Shop.OUTFIT) p.outfits[id] = true;
    for (const [species, id] of Object.entries<string>(data.equippedOutfits ?? {})) if (p.owns_outfit(id) && Shop.item(id)?.species === species) p.equipped[species] = id;
    for (const style of data.ownedBirdhouseStyles ?? []) if (Shop.HOUSE_STYLES.includes(style)) p.houses[style] = true;
    if (p.owns_house(data.equippedBirdhouseStyle)) p.equipped_house = data.equippedBirdhouseStyle;
    for (const id of data.speciesMet ?? []) if (Aviary.has_bird(id)) p.met[id] = true;
    for (const [id, count] of Object.entries(data.flocksHome ?? {})) if (Aviary.has_species(id) && id !== 'crow' && int(count) > 0) p.homes[id] = int(count);
    const daily = String(data.lastDailyGiftDate ?? ''); if (/^\d{4}-\d{2}-\d{2}$/.test(daily)) p.last_daily_gift = daily;
    for (const flag of data.tutorialFlags ?? []) if (String(flag)) p.tutorials[String(flag)] = true;
    for (const id of data.starterGiftsGranted ?? []) { const index = this.BOOSTER_IDS.indexOf(id); if (index >= 0) p.starters[index] = true; }
    for (const level of gifts) if (level >= 1 && level <= Levels.COUNT) p.level_gifts[level] = true;
    p.chests_opened = Math.max(0, int(data.chestsOpened)); p.checkin_streak = clamp(int(data.checkinStreak), 0, 7);
    p.draws_date = String(data.drawDate ?? ''); p.draws_left = clamp(int(data.drawsLeft), 0, 99);
    p.challenge_date = String(data.challengeDate ?? ''); p.last_level = clamp(last, 0, Levels.COUNT);
    for (const world of data.worldRewardsGranted ?? []) if (world >= 1 && world <= Levels.WORLDS) p.rewarded_worlds[world] = true;
    const earned = Array.isArray(data.earnedBoosters) ? data.earnedBoosters : [];
    this.BOOSTER_IDS.forEach((id, kind) => {
      const previouslyQualified = schema < 6 && (p.highest_unlocked >= this.OLD_BOOSTER_LEVELS[kind] || p.starters[kind] || p.inventory[kind] > 0);
      if (earned.includes(id) || previouslyQualified) p.earned_boosters[kind] = true;
    });
    return p;
  }
  private static renumbered(list: number[], table: Record<number, number>): number[] {
    const out = new Array<number>(Levels.COUNT + 1).fill(0);
    for (const [old, current] of Object.entries(table)) if (Number(old) < list.length) out[current] = list[Number(old)];
    return out;
  }
  /** Storage.setItem replaces a whole value atomically; retain the previous valid value as backup. */
  save(storage: ProfileStorage, key = Profile.SAVE_PATH): void {
    if (key === this.blocked_save_key) throw new Error('Profile is damaged; automatic saving is blocked');
    const previous = storage.getItem(key);
    if (previous !== null && Profile.read(previous)) storage.setItem(key + '.bak', previous);
    storage.setItem(key, JSON.stringify(this.to_dict()));
  }
  static load_saved(storage: ProfileStorage, key = Profile.SAVE_PATH): Profile {
    const primary = storage.getItem(key), backup = storage.getItem(key + '.bak');
    const saved = this.read(primary) ?? this.read(backup);
    if (saved) return saved;
    const profile = new Profile();
    if (primary !== null || backup !== null) profile.blocked_save_key = key;
    return profile;
  }
  /** A saved value counts only when it carries a schema version, the progress fields every schema has written, and its fields restore a profile; anything else is damaged. */
  private static read(text: string | null): Profile | null {
    if (text === null) return null;
    try {
      const value = JSON.parse(text);
      return value && typeof value === 'object' && !Array.isArray(value) && Number.isInteger(value.schemaVersion) && value.schemaVersion >= 1
        && typeof value.coins === 'number' && typeof value.highestUnlockedLevel === 'number' && Array.isArray(value.bestStars) ? this.from_dict(value) : null;
    } catch { return null; }
  }
}
