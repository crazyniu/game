import { Board, Kind } from './Board';

export interface BranchDefinition { side: string; row: number; kind?: string; seats?: number; birds: string[] }
export interface LevelDefinition { id: number; capacity: number; par: number; world?: number; variant?: string; branches: BranchDefinition[]; goal?: string[]; [key: string]: unknown }
export type LevelCatalog = { levels: LevelDefinition[] };

export class Levels {
  static readonly COUNT = 210;
  static readonly WORLDS = 10;
  static readonly PER_WORLD = 21;
  private static catalog: LevelDefinition[] = [];
  private static trials: LevelDefinition[] = [];
  static configure(catalog: LevelCatalog | LevelDefinition[], trials?: LevelCatalog): void {
    this.catalog = Array.isArray(catalog) ? catalog : catalog.levels;
    this.trials = trials?.levels ?? [];
  }
  static all(): LevelDefinition[] { return this.catalog; }
  static get_level(id: number): LevelDefinition {
    const level = this.catalog.find(level => level.id === id);
    if (!level) throw new Error(`Unknown level ${id}`);
    return level;
  }
  static trial(number: number): LevelDefinition | undefined { return this.trials[number - 1]; }
  static trial_count(): number { return this.trials.length; }
  static world_of(level: LevelDefinition): number { return level.world || 1; }
  static create_board(level: LevelDefinition, inventory: number[] = []): Board {
    if (level.id < 1 || level.capacity < 1 || level.par < 0 || !level.branches.length) throw new Error('Invalid level header');
    const board = new Board();
    board.level_id = level.id; board.world = this.world_of(level); board.variant = level.variant || 'noon'; board.capacity = level.capacity;
    const counts = new Map<number, number>();
    const slots = new Set<string>();
    let id = 0;
    for (const branch of level.branches) {
      const slot = branch.side + branch.row;
      if (!['L', 'R'].includes(branch.side) || branch.row < 0 || slots.has(slot)) throw new Error('Invalid branch definition');
      slots.add(slot);
      const kind = this.kind(branch.kind ?? '');
      const authored = branch.seats ?? 0;
      const seats = kind === Kind.TWIG ? 2 : kind === Kind.GROWING ? authored : level.capacity;
      if (seats < 1 || (kind === Kind.GROWING && (seats < 2 || seats > level.capacity)) || (kind !== Kind.GROWING && authored !== 0) || branch.birds.length > seats) throw new Error('Invalid branch seats');
      const birds = branch.birds.map(token => {
        const code = this.parse_bird(id++, token);
        const species = Board.species_of(code);
        if (species !== Board.CROW) counts.set(species, (counts.get(species) ?? 0) + 1);
        return code;
      });
      board.add_branch(seats, branch.side === 'R', branch.row, kind, birds);
    }
    if (!counts.size) throw new Error('A level needs a real species');
    for (const name of level.goal ?? []) {
      const species = Board.species_index(name);
      if (species < 0 || species === Board.CROW || !counts.has(species) || (board.goal_mask & (1 << species))) throw new Error(`Invalid goal species: ${name}`);
      board.goal_mask |= 1 << species;
    }
    for (const [species, count] of counts) if (count > level.capacity || (count !== level.capacity && board.in_goal(species))) throw new Error('Each goal species must have exactly capacity birds');
    board.species_count = counts.size;
    if (inventory.length === Board.BOOSTER_COUNT) board.inventory = inventory.slice();
    return board;
  }
  private static kind(name: string): Kind {
    const value = ({ '': Kind.NORMAL, normal: Kind.NORMAL, twig: Kind.TWIG, growing: Kind.GROWING, swivel: Kind.SWIVEL } as Record<string, Kind>)[name];
    if (value === undefined) throw new Error(`Unknown branch kind: ${name}`);
    return value;
  }
  private static parse_bird(id: number, token: string): number {
    const parts = token.split(':');
    let species = parts[0], flags = 0;
    if (!token.trim() || parts.length > 2) throw new Error(`Invalid bird token: ${token}`);
    if (parts.length === 2) {
      if (species === 'egg') { species = parts[1]; flags = Board.EGG; }
      else {
        flags = ({ sleep: Board.ASLEEP, mama: Board.MAMA, chick: Board.CHICK } as Record<string, number>)[parts[1]];
        if (flags === undefined) throw new Error(`Invalid bird token: ${token}`);
      }
    }
    const index = Board.species_index(species);
    if (index < 0 || (species === 'crow' && flags !== 0)) throw new Error(`Invalid bird species: ${token}`);
    return Board.bird(id, index, flags);
  }
  static species_ids(board: Board, include_crow = false): string[] {
    return Array.from(new Set(board.items.flat().map(code => Board.SPECIES[Board.species_of(code)]).filter(name => include_crow || name !== 'crow')));
  }
}
