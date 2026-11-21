import { Board, Booster, GameEvent, Move } from './Board';
import { Levels, LevelDefinition } from './Levels';
import { Rules, Gift } from './Rules';
import { Solver, SolveResult } from './Solver';

// Named so no cast reads `as X | Y`, which Creator's Babel can fail to compile (./run.sh check guards it).
type LastMove = Move | [];

export class GameSession {
  static readonly FREE_UNDOS = 3;
  static readonly TWO_STAR_SLACK = 3;
  state: Board;
  par: number;
  events: GameEvent[] = [];
  selected = -1;
  last_move: LastMove = [];
  undos_left = GameSession.FREE_UNDOS;
  free_rescue_used = false;
  undo_gifts_paid = 0;
  private initial: Board;
  private history: [Board, LastMove][] = [];
  private rules = new Rules();
  constructor(public level: LevelDefinition, inventory: number[] = []) {
    this.initial = Levels.create_board(level, inventory); this.state = this.initial.clone(); this.par = level.par;
  }
  static stars_for(moves: number, par: number): number { return moves <= par ? 3 : moves <= par + this.TWO_STAR_SLACK ? 2 : 1; }
  static moves_left_for_star(moves: number, par: number): number { return moves <= par ? par - moves : moves <= par + this.TWO_STAR_SLACK ? par + this.TWO_STAR_SLACK - moves : -1; }
  move_count(): number { return this.state.counted_moves; }
  stars(): number { return GameSession.stars_for(this.move_count(), this.par); }
  can_undo(): boolean { return this.history.length > 0; }
  select(branch: number): boolean {
    this.events.length = 0;
    if (this.state.is_won() || branch === this.selected || branch < 0 || branch >= this.state.branch_count() || !this.state.run_count(branch)) { this.selected = -1; return false; }
    this.selected = branch; return true;
  }
  cancel_selection(): void { this.selected = -1; }
  try_move(from: number, to: number): boolean {
    this.clear(); if (!Rules.can_transfer(this.state, from, to)) return false;
    this.push(); this.rules.transfer(this.state, from, to, this.events); this.pay_undo_gifts(); this.last_move = [from, to]; return true;
  }
  swivel(branch: number): boolean {
    this.clear(); if (!Rules.can_swivel(this.state, branch)) return false;
    this.push(); this.rules.swivel(this.state, branch, this.events); this.pay_undo_gifts(); this.last_move = [branch, -1]; return true;
  }
  try_action(move: number[]): boolean { return move[1] < 0 ? this.swivel(move[0]) : this.try_move(move[0], move[1]); }
  /** Tools used in this level (the task center counts them). */
  tools_used = 0;
  use_booster(kind: number, max_states = 100000, max_ms = 3000): { applied: boolean; hint?: SolveResult } {
    this.clear(); if (this.state.is_won() || !this.state.inventory[kind]) return { applied: false };
    let hint: SolveResult | undefined;
    if (kind === Booster.FIREFLY) {
      hint = Solver.solve(this.state, this.last_move, max_states, max_ms);
      if (hint.status !== Solver.Status.SOLVED || !hint.moves.length) return { applied: false, hint };
    } else if (!Rules.booster_has_effect(this.state, kind)) return { applied: false };
    this.push(); this.rules.booster(this.state, kind, true, this.events); this.pay_undo_gifts(); this.tools_used++; return { applied: true, hint };
  }
  try_use_firefly(move: number[]): boolean {
    this.clear(); if (this.state.is_won() || !this.state.inventory[Booster.FIREFLY]) return false;
    if (!(move[1] < 0 ? Rules.can_swivel(this.state, move[0]) : Rules.can_transfer(this.state, move[0], move[1]))) return false;
    this.push(); this.rules.booster(this.state, Booster.FIREFLY, true, this.events); this.pay_undo_gifts(); this.tools_used++; return true;
  }
  grant_undos(count: number): void { this.undos_left += count; }
  grant_booster(kind: number, count: number): void {
    this.state.inventory[kind] += count; this.initial.inventory[kind] += count;
    for (const [board] of this.history) board.inventory[kind] += count;
  }
  booster_has_effect(kind: number): boolean { return kind === Booster.FIREFLY ? !this.state.is_won() && Rules.has_legal_move(this.state) : Rules.booster_has_effect(this.state, kind); }
  add_branch(): boolean {
    this.clear(); if (!Rules.booster_has_effect(this.state, Booster.SAPLING)) return false;
    this.push(); this.rules.booster(this.state, Booster.SAPLING, false, this.events); this.pay_undo_gifts(); return true;
  }
  undo(): boolean {
    this.clear(); if (!this.history.length || this.undos_left <= 0) return false;
    this.undos_left--; [this.state, this.last_move] = this.history.pop()!; return true;
  }
  history_size(): number { return this.history.length; }
  history_state(back: number): Board { return this.history[this.history.length - back][0]; }
  rewind(steps: number): boolean {
    this.clear(); if (steps < 1 || steps > this.history.length) return false;
    const index = this.history.length - steps;
    [this.state, this.last_move] = this.history[index]; this.history.length = index; return true;
  }
  restart(): void { this.free_rescue_used = false; this.state = this.initial.clone(); this.last_move = []; this.history.length = 0; this.clear(); }
  private pay_undo_gifts(): void {
    const home = this.state.homed.filter(species => Rules.GIFT_BY_SPECIES[species] === Gift.SPARE_UNDO).length;
    if (home > this.undo_gifts_paid) { this.undos_left += home - this.undo_gifts_paid; this.undo_gifts_paid = home; }
  }
  private push(): void { this.history.push([this.state.clone(), this.last_move.slice() as LastMove]); }
  private clear(): void { this.events.length = 0; this.selected = -1; }
}
