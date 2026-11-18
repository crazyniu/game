import type { Profile } from './Profile';
export class Aviary {
  static readonly SPECIES: [string, string, number][] = [['sunny', 'Sunny', 1], ['sky', 'Skye', 1], ['berry', 'Berry', 3], ['mint', 'Minty', 6], ['cocoa', 'Cocoa', 9], ['snow', 'Flurry', 14], ['ember', 'Ember', 19], ['crow', 'Rook', 21], ['ink', 'Inky', 26], ['hoot', 'Hoot', 33]];
  static readonly NEW_SPECIES_COINS = 20;
  static has_species(id: string): boolean { return this.SPECIES.some(card => card[0] === id); }
  static display_name(id: string): string { return this.SPECIES.find(card => card[0] === id)?.[1] ?? id; }
  static on_level_started(p: Profile, species: string[]): string[] {
    const met = this.SPECIES.filter(card => species.includes(card[0]) && !p.has_met(card[0])).map(card => card[0]);
    for (const id of met) p.met[id] = true;
    p.coins += met.length * this.NEW_SPECIES_COINS;
    return met;
  }
  static add_flock_home(p: Profile, species: string, count = 1): void {
    if (species !== 'crow' && this.has_species(species)) p.homes[species] = p.flocks_home(species) + count;
  }
}
