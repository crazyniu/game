import { Board, Booster, Move } from './Board';
import { Rules } from './Rules';
export enum Status { SOLVED, UNSOLVABLE, BUDGET_EXCEEDED }
export interface SolveResult { status: Status; moves: Move[]; continuation?: Record<string, Move> }
type SearchNode = { board: Board | null; parent: number; move: Move; depth: number; penalty: number };

export class Solver {
  static readonly Status = Status;
  static solve(start: Board, previous_move: number[] = [], max_states = 100000, max_ms = 3000): SolveResult {
    if (previous_move.length === 2 && previous_move[1] >= 0) {
      const result = this.consume(this.search(start, previous_move, max_states, max_ms, [], false));
      if (result.status === Status.SOLVED) return result;
    }
    return this.consume(this.search(start, [], max_states, max_ms, [], false));
  }
  static hint(start: Board, previous_move: number[] = [], max_states = 24000, max_ms = 1500, cancel: boolean[] = []): SolveResult {
    return this.consume(this.search(start, previous_move, max_states, max_ms, cancel, true));
  }
  /** Yield between small search slices so touch input, bird animation and audio keep running. */
  static async hint_async(start: Board, previous_move: number[] = [], max_states = 24000, max_ms = 1500, cancel: boolean[] = []): Promise<SolveResult> {
    const search = this.search(start, previous_move, max_states, max_ms, cancel, true);
    for (;;) {
      const step = search.next();
      if (step.done) return step.value;
      await new Promise<void>(resolve => setTimeout(resolve, 0));
    }
  }
  static rescue_booster(start: Board, kinds: number[], max_states = 20000, max_ms = 600, cancel: boolean[] = []): number {
    const rules = new Rules(); rules.track_economy = false;
    for (const kind of kinds) {
      if (cancel[0]) return -1;
      if (kind === Booster.FIREFLY || !Rules.booster_has_effect(start, kind)) continue;
      const probe = start.clone(false); rules.booster(probe, kind, false);
      if (this.hint(probe, [], max_states, max_ms, cancel).status === Status.SOLVED) return kind;
    }
    return -1;
  }
  static best_move(start: Board, previous_move: number[] = []): Move | [] {
    const rules = new Rules(); rules.track_economy = false;
    let best: Move | [] = [], bestScore = Infinity;
    for (const move of this.legal_moves(start)) {
      const next = start.clone(false); this.apply(rules, next, move);
      let score = next.is_won() ? -1000000 : this.hint_score(next);
      if (previous_move.length === 2 && previous_move[1] >= 0 && move[0] === previous_move[1] && move[1] === previous_move[0]) score += 24;
      if (score < bestScore) { best = move; bestScore = score; }
    }
    return best;
  }
  private static consume(search: Generator<void, SolveResult>): SolveResult {
    for (;;) { const step = search.next(); if (step.done) return step.value; }
  }
  private static *search(start: Board, previous: number[], maxStates: number, maxMs: number, cancel: boolean[], bestFirst: boolean): Generator<void, SolveResult> {
    if (start.is_won()) return { status: Status.SOLVED, moves: [], continuation: {} };
    const deadline = Date.now() + maxMs;
    let sliceEnd = Date.now() + 6;
    const rules = new Rules(); rules.track_economy = false;
    const nodes: SearchNode[] = [{ board: start.clone(false), parent: -1, move: [0, 0], depth: 0, penalty: 0 }];
    const visited = new Set([start.search_key().join(',')]);
    const heap: [number, number][] = [[this.hint_score(start), 0]];
    let cursor = 0;
    while (bestFirst ? heap.length > 0 : cursor < nodes.length) {
      if (cancel[0]) return { status: Status.BUDGET_EXCEEDED, moves: [] };
      const index = bestFirst ? this.pop(heap) : cursor++;
      const node = nodes[index], board = node.board!;
      node.board = null;
      for (const move of this.legal_moves(board)) {
        if (Date.now() >= deadline || visited.size >= maxStates) return { status: Status.BUDGET_EXCEEDED, moves: [] };
        const reverses = index === 0 && previous.length === 2 && previous[1] >= 0 && move[0] === previous[1] && move[1] === previous[0];
        if (!bestFirst && reverses) continue;
        const next = board.clone(false); this.apply(rules, next, move);
        const key = next.search_key().join(',');
        if (visited.has(key)) continue;
        visited.add(key);
        const nextIndex = nodes.length, depth = node.depth + 1, penalty = reverses ? 24 : node.penalty;
        nodes.push({ board: next, parent: index, move, depth, penalty });
        if (next.is_won()) {
          const moves: Move[] = [];
          for (let i = nextIndex; nodes[i].parent >= 0; i = nodes[i].parent) moves.unshift(nodes[i].move);
          const continuation: Record<string, Move> = {}, replay = start.clone(false);
          for (const action of moves) { continuation[replay.search_key().join(',')] = action; this.apply(rules, replay, action); }
          return { status: Status.SOLVED, moves, continuation };
        }
        if (bestFirst) this.push(heap, [depth + penalty + this.hint_score(next), nextIndex]);
      }
      if (Date.now() >= sliceEnd) { yield; sliceEnd = Date.now() + 6; }
    }
    return { status: Status.UNSOLVABLE, moves: [] };
  }
  private static *legal_moves(board: Board): Generator<Move> {
    for (let from = 0; from < board.branch_count(); from++) {
      for (let to = 0; to < board.branch_count(); to++) if (Rules.can_transfer(board, from, to)) yield [from, to];
      if (Rules.can_swivel(board, from)) yield [from, -1];
    }
  }
  private static apply(rules: Rules, board: Board, move: Move): void {
    if (move[1] < 0) rules.swivel(board, move[0]); else rules.transfer(board, move[0], move[1]);
  }
  private static hint_score(board: Board): number {
    let groups = 0, blockers = 0;
    for (const branch of board.items) {
      let previous = -1;
      for (const code of branch) {
        const species = Board.species_of(code);
        if (species !== Board.CROW && species !== previous) groups++;
        if (code & (Board.EGG | Board.ASLEEP)) blockers++;
        previous = species;
      }
    }
    return groups * 12 + blockers * 2 + (board.species_count - board.homed.length) * 8;
  }
  private static push(heap: [number, number][], entry: [number, number]): void {
    let i = heap.length; heap.push(entry);
    while (i > 0) { const parent = (i - 1) >> 1; if (heap[parent][0] <= entry[0]) break; heap[i] = heap[parent]; i = parent; }
    heap[i] = entry;
  }
  private static pop(heap: [number, number][]): number {
    const index = heap[0][1], last = heap.pop()!;
    if (!heap.length) return index;
    let i = 0;
    while (i * 2 + 1 < heap.length) {
      let child = i * 2 + 1;
      if (child + 1 < heap.length && heap[child + 1][0] < heap[child][0]) child++;
      if (heap[child][0] >= last[0]) break;
      heap[i] = heap[child]; i = child;
    }
    heap[i] = last;
    return index;
  }
}
