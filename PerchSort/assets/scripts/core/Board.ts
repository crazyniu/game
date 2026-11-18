export enum Kind { NORMAL, TWIG, GROWING, SWIVEL }
export enum Phase { DAY, NIGHT }
export enum Booster { FIREFLY, SAPLING, OWL_EYE, MORNING_BELL, SCARECROW, SPRING_RAIN }
export type Move = [number, number];
export interface GameEvent { type: string; [key: string]: any }

/** A bird is packed as (id << 10) | (species << 5) | flags. */
export class Board {
  static readonly EGG = 1;
  static readonly ASLEEP = 2;
  static readonly MAMA = 4;
  static readonly CHICK = 8;
  static readonly REVEALED = 16;
  static readonly FLAG_MASK = 31;
  static readonly SPECIES = ['sunny', 'sky', 'berry', 'mint', 'cocoa', 'snow', 'ember', 'crow', 'ink', 'hoot'];
  static readonly CROW = 7;
  static readonly INK = 8;
  static readonly HOOT = 9;
  static readonly BOOSTER_COUNT = 6;
  static readonly Kind = Kind;
  static readonly Phase = Phase;
  static readonly Booster = Booster;
  level_id = 0;
  world = 1;
  variant = 'noon';
  capacity = 4;
  species_count = 0;
  goal_mask = 0;
  items: number[][] = [];
  caps: number[] = [];
  kinds: number[] = [];
  rows: number[] = [];
  rights: number[] = [];
  homed: number[] = [];
  moves = 0;
  counted_moves = 0;
  added_branch = false;
  chain = 0;
  chorus_coins = 0;
  inventory = [0, 0, 0, 0, 0, 0];

  static bird(id: number, species: number, flags = 0): number { return (id << 10) | (species << 5) | flags; }
  static species_of(code: number): number { return (code >> 5) & 31; }
  static flags_of(code: number): number { return code & Board.FLAG_MASK; }
  static id_of(code: number): number { return code >> 10; }
  static with_flags(code: number, flags: number): number { return (code & ~Board.FLAG_MASK) | flags; }
  static species_index(name: string): number { return Board.SPECIES.indexOf(name); }
  branch_count(): number { return this.items.length; }
  is_won(): boolean {
    if (this.goal_mask === 0) return this.homed.length === this.species_count;
    const mask = this.homed.reduce((value, species) => value | (1 << species), 0);
    return (mask & this.goal_mask) === this.goal_mask;
  }
  in_goal(species: number): boolean { return this.goal_mask === 0 || (this.goal_mask & (1 << species)) !== 0; }
  is_asleep(code: number): boolean { return !(code & Board.EGG) && !!(code & Board.ASLEEP); }
  free_slots(branch: number): number { return this.caps[branch] - this.items[branch].length; }
  run_count(branch: number): number {
    const list = this.items[branch];
    if (!list.length) return 0;
    const tip = list[list.length - 1];
    if ((tip & Board.EGG) || this.is_asleep(tip)) return 0;
    let start = list.length - 1;
    while (start > 0) {
      const other = list[start - 1];
      if ((other & Board.EGG) || this.is_asleep(other) || Board.species_of(other) !== Board.species_of(tip)) break;
      start--;
    }
    return list.length - start;
  }
  set_bird(branch: number, seat: number, code: number): void { this.items[branch][seat] = code; }
  add_branch(capacity: number, right: boolean, row: number, kind: number, birds: number[]): void {
    this.items.push(birds); this.caps.push(capacity); this.rights.push(right ? 1 : 0); this.rows.push(row); this.kinds.push(kind);
  }
  clone(copy_inventory = true): Board {
    const copy = Object.assign(new Board(), this);
    copy.items = this.items.map(list => list.slice());
    for (const key of ['caps', 'kinds', 'rows', 'rights', 'homed'] as const) copy[key] = this[key].slice();
    copy.inventory = copy_inventory ? this.inventory.slice() : this.inventory;
    return copy;
  }
  search_key(preserve_order = false): number[] {
    const key = preserve_order ? [this.homed.length, ...this.homed] : [this.homed.reduce((mask, s) => mask | (1 << s), 0)];
    this.items.forEach((list, b) => {
      key.push(this.rights[b] | (this.rows[b] << 1) | (this.kinds[b] << 8) | (this.caps[b] << 12) | (list.length << 18));
      key.push(...list.map(code => code & 1023));
    });
    return key;
  }
}
