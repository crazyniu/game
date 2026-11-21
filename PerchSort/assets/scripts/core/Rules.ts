import { Board, Booster, Kind, GameEvent } from './Board';
export type RuleEvent = GameEvent;
export enum Gift { WAKE_UP_SONG, GENTLE_BREEZE, MOVE_REFUND, GARDENER, GRUMPY_STARE, SPARE_UNDO, WARM_GLOW, NIGHT_CALL, MOONBEAM }
type Events = GameEvent[] | null;

export class Rules {
  static readonly Gift = Gift;
  static readonly GIFT_BY_SPECIES: Record<number, Gift> = { 0: Gift.WAKE_UP_SONG, 1: Gift.GENTLE_BREEZE, 2: Gift.MOVE_REFUND, 3: Gift.GARDENER, 4: Gift.GRUMPY_STARE, 5: Gift.SPARE_UNDO, 6: Gift.WARM_GLOW, 8: Gift.MOVE_REFUND, 9: Gift.WAKE_UP_SONG };
  track_economy = true;
  static can_transfer(s: Board, from: number, to: number): boolean {
    if (s.is_won() || from === to || from < 0 || to < 0 || from >= s.branch_count() || to >= s.branch_count()) return false;
    if (!s.run_count(from) || s.free_slots(to) <= 0) return false;
    const target = s.items[to];
    if (!target.length) return true;
    const tip = target[target.length - 1], source = s.items[from];
    return !(tip & Board.EGG) && Board.species_of(tip) === Board.species_of(source[source.length - 1]);
  }
  static can_swivel(s: Board, b: number): boolean {
    if (s.is_won() || s.kinds[b] !== Kind.SWIVEL) return false;
    const birds = s.items[b];
    return birds.some((code, i) => (code & 1023) !== (birds[birds.length - 1 - i] & 1023));
  }
  static can_home(s: Board, b: number): boolean { return s.kinds[b] !== Kind.TWIG && s.items[b].length === s.capacity && Board.species_of(s.items[b][0]) !== Board.CROW && s.run_count(b) === s.capacity; }
  static has_legal_move(s: Board): boolean {
    for (let from = 0; from < s.branch_count(); from++) {
      if (this.can_swivel(s, from)) return true;
      for (let to = 0; to < s.branch_count(); to++) if (this.can_transfer(s, from, to)) return true;
    }
    return false;
  }
  transfer(s: Board, from: number, to: number, out: Events = null): void {
    s.moves++; s.counted_moves++;
    const before = s.homed.length, source = s.items[from], target = s.items[to];
    const count = Math.min(s.run_count(from), s.free_slots(to)), start = source.length - count, targetStart = target.length;
    const species = Board.species_of(source[start]);
    const landingWake = targetStart > 0 && Board.species_of(target[targetStart - 1]) === species ? Board.id_of(target[targetStart - 1]) : -1;
    const moved = source.splice(start);
    target.push(...moved);
    out?.push({ type: 'transferred', from, to, from_seat: start, to_seat: targetStart, ids: moved.map(Board.id_of) });
    if (moved.some(code => !!(code & Board.MAMA))) {
      const followers: { id: number; from_seat: number; to_seat: number }[] = [];
      for (let i = source.length - 1; i >= 0 && target.length < s.caps[to]; i--) {
        const code = source[i];
        if ((code & Board.CHICK) && !(code & Board.EGG) && !s.is_asleep(code) && Board.species_of(code) === species) {
          followers.push({ id: Board.id_of(code), from_seat: i, to_seat: target.length });
          target.push(code); source.splice(i, 1);
        }
      }
      if (followers.length) out?.push({ type: 'chicks_followed', from, to, birds: followers });
    }
    for (let k = 0; k < targetStart; k++) if ((target[k] & Board.EGG) && Board.species_of(target[k]) === species) out?.push({ type: 'egg_wobbled', id: Board.id_of(target[k]), branch: to, seat: k });
    this.finish(s, before, true, to, landingWake, out);
  }
  swivel(s: Board, b: number, out: Events = null): void {
    s.moves++; s.counted_moves++;
    const before = s.homed.length, list = s.items[b];
    out?.push({ type: 'swivelled', branch: b, birds: list.map((code, i) => ({ id: Board.id_of(code), from_seat: i, to_seat: list.length - 1 - i })) });
    list.reverse();
    this.finish(s, before, true, -1, -1, out);
  }
  private finish(s: Board, before: number, move: boolean, landingBranch: number, landingBird: number, out: Events): void {
    this.resolve(s, landingBranch, landingBird, out);
    this.auto_home(s, out);
    if (move) this.rescue_deadlock(s, out);
    if (this.track_economy) {
      const homes = s.homed.length - before;
      if (!homes && move) s.chain = 0;
      for (let i = 0; i < homes; i++) if (++s.chain > 1) s.chorus_coins += 5;
    }
    if (s.is_won()) out?.push({ type: 'won' });
  }
  private rescue_deadlock(s: Board, out: Events): void {
    if (s.is_won() || Rules.has_legal_move(s)) return;
    if (s.items.some(list => list.some(code => !!(code & Board.ASLEEP)))) {
      this.wake_rows(s, 0, false, out); this.resolve(s, -1, -1, out); this.auto_home(s, out);
    }
  }
  private auto_home(s: Board, out: Events): boolean {
    if (s.is_won() || s.homed.length !== s.species_count - 1) return false;
    let species = -1, count = 0;
    for (const list of s.items) for (const code of list) {
      const kind = Board.species_of(code);
      if (kind === Board.CROW || (code & Board.EGG) || s.is_asleep(code) || (species >= 0 && kind !== species)) return false;
      species = kind; count++;
    }
    if (species < 0 || count !== s.capacity) return false;
    const ids = s.items.flat().map(Board.id_of), first = s.items.findIndex(list => list.length > 0);
    s.items = s.items.map(() => []);
    s.kinds = s.kinds.map(kind => kind !== Kind.TWIG ? Kind.NORMAL : kind);
    out?.push({ type: 'flock_home', branch: first, species: Board.SPECIES[species], ids });
    s.homed.push(species);
    // The last flock gives its gift like any flock; on the emptied board only a move refund has an effect.
    this.apply_gift(s, first, species, out);
    return true;
  }
  private resolve(s: Board, landingBranch: number, landingBird: number, out: Events): void {
    let changed = true;
    while (changed) {
      out?.push({ type: 'wave' }); changed = false;
      for (let b = 0; b < s.branch_count(); b++) {
        const tip = s.items[b].length - 1;
        if (tip >= 0 && (s.items[b][tip] & Board.EGG)) { this.hatch(s, b, tip, out); changed = true; }
      }
      const homes: [number, number][] = [];
      for (let b = 0; b < s.branch_count(); b++) {
        if (!Rules.can_home(s, b)) continue;
        const species = Board.species_of(s.items[b][0]);
        out?.push({ type: 'flock_home', branch: b, species: Board.SPECIES[species], ids: s.items[b].map(Board.id_of) });
        s.items[b] = []; s.kinds[b] = Kind.NORMAL; s.homed.push(species); homes.push([b, species]); changed = true;
      }
      if (landingBird >= 0) {
        const target = s.items[landingBranch], seat = target.findIndex(code => Board.id_of(code) === landingBird);
        if (seat >= 0) changed = this.wake(s, landingBranch, seat, out) || changed;
        landingBird = -1;
      }
      for (const [branch, species] of homes) {
        this.apply_gift(s, branch, species, out); this.wake_rows(s, s.rows[branch], true, out); this.grow(s, false, out);
      }
    }
  }
  private apply_gift(s: Board, b: number, species: number, out: Events): void {
    const effect = Rules.GIFT_BY_SPECIES[species];
    if (effect === undefined) return;
    out?.push({ type: 'gift', branch: b, species: Board.SPECIES[species], effect });
    switch (effect) {
      case Gift.WAKE_UP_SONG: this.wake_rows(s, 0, false, out); break;
      case Gift.GENTLE_BREEZE: this.reveal_eggs(s, out); break;
      case Gift.MOVE_REFUND: s.counted_moves = Math.max(0, s.counted_moves - 1); break;
      case Gift.GARDENER:
        this.grow(s, false, out);
        for (let t = 0; t < s.branch_count(); t++) if (s.kinds[t] === Kind.TWIG) {
          const before = s.caps[t]; s.kinds[t] = Kind.NORMAL; s.caps[t] = s.capacity;
          out?.push({ type: 'twig_upgraded', branch: t, before, after: s.capacity }); break;
        }
        break;
      case Gift.GRUMPY_STARE: this.remove_crows(s, s.rows[b], true, out); break;
      case Gift.WARM_GLOW:
        for (let t = 0; t < s.branch_count(); t++) for (let i = 0; i < s.items[t].length; i++) if (s.items[t][i] & Board.EGG) this.hatch(s, t, i, out);
        break;
    }
  }
  private hatch(s: Board, b: number, seat: number, out: Events): void {
    const code = s.items[b][seat];
    s.set_bird(b, seat, Board.with_flags(code, Board.flags_of(code) & ~(Board.EGG | Board.REVEALED)));
    out?.push({ type: 'hatched', id: Board.id_of(code), branch: b, seat });
  }
  private wake(s: Board, b: number, seat: number, out: Events): boolean {
    const code = s.items[b][seat];
    if (!(code & Board.ASLEEP)) return false;
    const awake = Board.with_flags(code, Board.flags_of(code) & ~Board.ASLEEP);
    s.set_bird(b, seat, awake);
    if (!s.is_asleep(awake)) out?.push({ type: 'woke', id: Board.id_of(code), branch: b, seat });
    return true;
  }
  private wake_rows(s: Board, row: number, limited: boolean, out: Events): void {
    for (let b = 0; b < s.branch_count(); b++) {
      if (limited && Math.abs(s.rows[b] - row) > 1) continue;
      for (let i = 0; i < s.items[b].length; i++) this.wake(s, b, i, out);
    }
  }
  private reveal_eggs(s: Board, out: Events): void {
    for (let b = 0; b < s.branch_count(); b++) for (let i = 0; i < s.items[b].length; i++) {
      const code = s.items[b][i];
      if (!(code & Board.EGG) || (code & Board.REVEALED)) continue;
      s.set_bird(b, i, code | Board.REVEALED); out?.push({ type: 'egg_revealed', id: Board.id_of(code), branch: b, seat: i });
    }
  }
  private grow(s: Board, full: boolean, out: Events): void {
    for (let b = 0; b < s.branch_count(); b++) if (s.kinds[b] === Kind.GROWING && s.caps[b] < s.capacity) {
      const before = s.caps[b]; s.caps[b] = full ? s.capacity : before + 1;
      out?.push({ type: 'branch_grew', branch: b, before, after: s.caps[b] });
    }
  }
  private remove_crows(s: Board, row: number, limited: boolean, out: Events): void {
    for (let b = 0; b < s.branch_count(); b++) {
      if (limited && Math.abs(s.rows[b] - row) > 1) continue;
      const list = s.items[b], departed: { id: number; from_seat: number }[] = [];
      for (let i = list.length - 1; i >= 0; i--) if (Board.species_of(list[i]) === Board.CROW) {
        departed.unshift({ id: Board.id_of(list[i]), from_seat: i }); list.splice(i, 1);
      }
      if (departed.length) out?.push({ type: 'crows_left', branch: b, birds: departed });
    }
  }
  static booster_has_effect(s: Board, kind: number): boolean {
    if (s.is_won()) return false;
    if (kind === Booster.SAPLING) return !s.added_branch;
    for (let b = 0; b < s.branch_count(); b++) {
      if (kind === Booster.SPRING_RAIN && s.kinds[b] === Kind.GROWING && s.caps[b] < s.capacity) return true;
      for (const code of s.items[b]) if ((kind === Booster.OWL_EYE && (code & Board.EGG) && !(code & Board.REVEALED)) || (kind === Booster.MORNING_BELL && (code & Board.ASLEEP)) || (kind === Booster.SCARECROW && Board.species_of(code) === Board.CROW)) return true;
    }
    return false;
  }
  booster(s: Board, kind: number, consume: boolean, out: Events = null): void {
    const before = s.homed.length;
    if (consume) s.inventory[kind]--;
    out?.push({ type: 'booster_used', kind, consumed: consume, remaining: s.inventory[kind] });
    switch (kind) {
      case Booster.FIREFLY: return;
      case Booster.SAPLING: {
        let slot = 0;
        while (s.rows.some((row, b) => row === Math.floor(slot / 2) && s.rights[b] === slot % 2)) slot++;
        s.add_branch(s.capacity, slot % 2 === 1, Math.floor(slot / 2), Kind.NORMAL, []); s.added_branch = true; break;
      }
      case Booster.OWL_EYE: this.reveal_eggs(s, out); break;
      case Booster.MORNING_BELL: this.wake_rows(s, 0, false, out); break;
      case Booster.SCARECROW: this.remove_crows(s, 0, false, out); break;
      case Booster.SPRING_RAIN: this.grow(s, true, out); break;
    }
    this.finish(s, before, false, -1, -1, out);
  }
}
