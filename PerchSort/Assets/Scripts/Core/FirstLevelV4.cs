using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace PerchSort.Core.FirstLevel
{
    public enum BranchSide { Left, Right }
    public enum EventKind { Moved, Home, Retired }

    public sealed class Token
    {
        public string Id { get; }
        public int Species { get; }
        public string Form => "bird";

        public Token(string id, int species)
        {
            if (string.IsNullOrEmpty(id) || id.Length > 64 ||
                !id.All(c => c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' ||
                    c >= '0' && c <= '9' || c == '.' || c == '_' || c == '-') ||
                !char.IsLetterOrDigit(id[0]))
                throw new ArgumentException("A token requires a stable ASCII identifier.", nameof(id));
            if (species < 0) throw new ArgumentOutOfRangeException(nameof(species));
            Id = id;
            Species = species;
        }

        public Token Clone() => new Token(Id, Species);
    }

    public sealed class Branch
    {
        public string Id { get; }
        public BranchSide Side { get; }
        public IReadOnlyList<Token> Tokens { get; }
        public int HomeSpecies { get; }
        public bool Retired => HomeSpecies >= 0;

        public Branch(string id, IEnumerable<Token> tokens, int homeSpecies = -1)
        {
            if (string.IsNullOrEmpty(id) || id[0] != 'S' ||
                !int.TryParse(id.Substring(1), NumberStyles.None, CultureInfo.InvariantCulture, out int number) ||
                number < 1 || id != "S" + number.ToString(CultureInfo.InvariantCulture))
                throw new ArgumentException("Branch identifiers are S1, S2, ... in array order.", nameof(id));
            if (tokens == null) throw new ArgumentNullException(nameof(tokens));
            if (homeSpecies < -1) throw new ArgumentOutOfRangeException(nameof(homeSpecies));
            Id = id;
            Side = number % 2 == 1 ? BranchSide.Left : BranchSide.Right;
            Tokens = new ReadOnlyCollection<Token>(tokens.Select(t =>
                t?.Clone() ?? throw new ArgumentException("A token cannot be null.")).ToList());
            HomeSpecies = homeSpecies;
        }

        public Branch Clone() => new Branch(Id, Tokens, HomeSpecies);
    }

    // A stable immutable bird-only slice. This does not load/migrate a campaign or a save.
    public sealed class State
    {
        public int FlockSize { get; }
        public int SpeciesCount { get; }
        public IReadOnlyList<Branch> Branches { get; }
        public IReadOnlyList<int> HomedSpecies { get; }
        public int MoveCount { get; }
        public long Sequence { get; }
        public bool IsWon => HomedSpecies.Count == SpeciesCount && Branches.All(b => b.Tokens.Count == 0);
        public bool IsDeadlock => !IsWon && Rules.GetLegalMoves(this).Count == 0;

        public State(int flockSize, int speciesCount, IEnumerable<Branch> branches,
            int moveCount = 0, long sequence = 0)
        {
            if (flockSize != 4 && flockSize != 6 && flockSize != 8)
                throw new ArgumentOutOfRangeException(nameof(flockSize));
            if (speciesCount < 1 || speciesCount > 8) throw new ArgumentOutOfRangeException(nameof(speciesCount));
            if (branches == null) throw new ArgumentNullException(nameof(branches));
            if (moveCount < 0 || sequence < moveCount) throw new ArgumentOutOfRangeException(nameof(moveCount));
            var copy = branches.Select(b => b?.Clone() ??
                throw new ArgumentException("A branch cannot be null.")).ToList();
            if (copy.Count < speciesCount || copy.Count > 10)
                throw new ArgumentException("The slice supports a fixed array of at most ten branches.");
            var counts = new int[speciesCount];
            var tokenIds = new HashSet<string>(StringComparer.Ordinal);
            var home = new HashSet<int>();
            for (int i = 0; i < copy.Count; i++)
            {
                var b = copy[i];
                if (b.Id != "S" + (i + 1).ToString(CultureInfo.InvariantCulture) ||
                    b.Tokens.Count > flockSize || b.HomeSpecies >= speciesCount)
                    throw new ArgumentException("Invalid fixed branch order, capacity or completion.");
                if (b.Retired)
                {
                    if (b.Tokens.Count != 0 || !home.Add(b.HomeSpecies))
                        throw new ArgumentException("A completed species has exactly one empty retired branch.");
                    counts[b.HomeSpecies] += flockSize;
                }
                else if (b.Tokens.Count == flockSize && b.Tokens.All(t => t.Species == b.Tokens[0].Species))
                    throw new ArgumentException("A stable full uniform branch must already be homed and retired.");
                foreach (var token in b.Tokens)
                {
                    if (token.Species >= speciesCount || !tokenIds.Add(token.Id))
                        throw new ArgumentException("Invalid species or duplicate stable token ID.");
                    counts[token.Species]++;
                }
            }
            if (counts.Any(n => n != flockSize))
                throw new ArgumentException("Remaining and homed birds must conserve each species exactly.");
            FlockSize = flockSize;
            SpeciesCount = speciesCount;
            Branches = new ReadOnlyCollection<Branch>(copy);
            HomedSpecies = new ReadOnlyCollection<int>(home.OrderBy(n => n).ToList());
            MoveCount = moveCount;
            Sequence = sequence;
        }

        // Approved L01 B: a narrow N4/K2/P3 tutorial exception to the later-level shape.
        public static State CreateDocumentedFirstLevel()
        {
            int[][] species = { new[] { 0, 0, 0 }, new[] { 1, 1, 1, 0 }, new[] { 1 } };
            var state = new State(4, 2, species.Select((birds, index) =>
                new Branch("S" + (index + 1), birds.Select((s, slot) =>
                    new Token("L01-S" + (index + 1) + "-" + (slot + 1).ToString("D2", CultureInfo.InvariantCulture), s)))));
            state.ValidateInitialLevelContract(1);
            return state;
        }

        public static State CreateInitial() => CreateDocumentedFirstLevel();

        // Bird-only opening validation for this isolated slice, not a 40-level/schema loader.
        // Later documented N6/N8 configurations remain valid; egg support is outside this type.
        public void ValidateInitialLevelContract(int levelId)
        {
            if (levelId < 1 || levelId > 40) throw new ArgumentOutOfRangeException(nameof(levelId));
            if (MoveCount != 0 || Sequence != 0 || Branches.Any(b => b.Retired))
                throw new ArgumentException("An initial level must have zero counters and no retired branches.");
            if (levelId != 1)
            {
                if (SpeciesCount < 3 || Branches.Count != SpeciesCount + 2 ||
                    Branches.Count(b => b.Tokens.Count == 0) != 2)
                    throw new ArgumentException("Later bird-only openings require K3..8, P=K+2 and two empty branches.");
                return;
            }

            int[][] expected = { new[] { 0, 0, 0 }, new[] { 1, 1, 1, 0 }, new[] { 1 } };
            if (FlockSize != 4 || SpeciesCount != 2 || Branches.Count != 3)
                throw new ArgumentException("L01 alone uses the approved N4/K2/P3 tutorial exception.");
            for (int i = 0; i < expected.Length; i++)
            {
                var b = Branches[i];
                if (!b.Tokens.Select(t => t.Species).SequenceEqual(expected[i]) ||
                    !b.Tokens.Select(t => t.Id).SequenceEqual(expected[i].Select((_, slot) =>
                        "L01-S" + (i + 1) + "-" + (slot + 1).ToString("D2", CultureInfo.InvariantCulture))))
                    throw new ArgumentException("L01 must preserve its approved fixed arrays and token IDs.");
            }
        }

        public State Clone() => new State(FlockSize, SpeciesCount, Branches, MoveCount, Sequence);
        internal State WithCounters(int moves, long sequence) => new State(FlockSize, SpeciesCount, Branches, moves, sequence);

        // Exact slice key, including transaction identity. It is not a contentHash or save format.
        public string GetStateKey()
        {
            var key = new StringBuilder();
            key.Append("rules4:bird-only:").Append(FlockSize.ToString(CultureInfo.InvariantCulture)).Append(':')
                .Append(SpeciesCount.ToString(CultureInfo.InvariantCulture)).Append(':')
                .Append(MoveCount.ToString(CultureInfo.InvariantCulture)).Append(':')
                .Append(Sequence.ToString(CultureInfo.InvariantCulture)).Append('|');
            foreach (var b in Branches)
            {
                key.Append(b.Id).Append(':').Append(b.HomeSpecies.ToString(CultureInfo.InvariantCulture)).Append('[');
                foreach (var t in b.Tokens) key.Append(t.Id).Append('=').Append(t.Species.ToString(CultureInfo.InvariantCulture)).Append(',');
                key.Append("]|");
            }
            return key.ToString();
        }
    }

    public sealed class MoveEvent
    {
        public EventKind Kind { get; }
        public int FromIndex { get; }
        public int ToIndex { get; }
        public int BranchIndex { get; }
        public int Species { get; }
        public int SourceStartSlot { get; }
        public int TargetStartSlot { get; }
        public IReadOnlyList<Token> Tokens { get; }
        public IReadOnlyList<string> TokenIds { get; }

        internal MoveEvent(EventKind kind, int from, int to, int branch, int species,
            int sourceSlot, int targetSlot, IEnumerable<Token> tokens)
        {
            Kind = kind; FromIndex = from; ToIndex = to; BranchIndex = branch; Species = species;
            SourceStartSlot = sourceSlot; TargetStartSlot = targetSlot;
            var copy = tokens.Select(t => t.Clone()).ToList();
            Tokens = new ReadOnlyCollection<Token>(copy);
            TokenIds = new ReadOnlyCollection<string>(copy.Select(t => t.Id).ToList());
        }
    }

    public sealed class MoveResult
    {
        public int FromIndex { get; }
        public int ToIndex { get; }
        public int Count { get; }
        public int Run { get; }
        public State Before { get; }
        public State After { get; }
        public IReadOnlyList<MoveEvent> Events { get; }

        internal MoveResult(int from, int to, int count, int run, State before, State after, List<MoveEvent> events)
        {
            FromIndex = from; ToIndex = to; Count = count; Run = run;
            Before = before; After = after;
            Events = events.AsReadOnly();
        }
    }

    public readonly struct Move
    {
        public int FromIndex { get; }
        public int ToIndex { get; }
        public int Count { get; }
        public Move(int from, int to, int count) { FromIndex = from; ToIndex = to; Count = count; }
    }

    public static class Rules
    {
        public static int GetRun(State state, int from)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (from < 0 || from >= state.Branches.Count) return 0;
            var b = state.Branches[from];
            if (b.Retired || b.Tokens.Count == 0) return 0;
            int count = 1, species = b.Tokens[b.Tokens.Count - 1].Species;
            while (count < b.Tokens.Count && b.Tokens[b.Tokens.Count - count - 1].Species == species) count++;
            return count;
        }

        public static int LegalCount(State state, int from, int to)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (from < 0 || to < 0 || from >= state.Branches.Count || to >= state.Branches.Count || from == to) return 0;
            var source = state.Branches[from];
            var target = state.Branches[to];
            if (source.Retired || target.Retired || source.Tokens.Count == 0 || target.Tokens.Count == state.FlockSize) return 0;
            if (target.Tokens.Count != 0 &&
                target.Tokens[target.Tokens.Count - 1].Species != source.Tokens[source.Tokens.Count - 1].Species) return 0;
            return Math.Min(GetRun(state, from), state.FlockSize - target.Tokens.Count);
        }

        public static IReadOnlyList<Move> GetLegalMoves(State state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            var moves = new List<Move>();
            for (int from = 0; from < state.Branches.Count; from++)
                for (int to = 0; to < state.Branches.Count; to++)
                {
                    int count = LegalCount(state, from, to);
                    if (count > 0) moves.Add(new Move(from, to, count));
                }
            return moves.AsReadOnly();
        }

        public static bool TryApply(State before, int from, int to, out State candidate, out MoveResult result)
        {
            int count = LegalCount(before, from, to);
            candidate = null; result = null;
            if (count == 0) return false;
            int run = GetRun(before, from);
            var branches = before.Branches.Select(b => b.Clone()).ToArray();
            var source = branches[from].Tokens.ToList();
            var target = branches[to].Tokens.ToList();
            int sourceSlot = source.Count - count, targetSlot = target.Count;
            var moved = source.GetRange(sourceSlot, count);
            int species = moved[0].Species;
            source.RemoveRange(sourceSlot, count);
            target.AddRange(moved);
            var events = new List<MoveEvent>
            {
                new MoveEvent(EventKind.Moved, from, to, -1, species, sourceSlot, targetSlot, moved)
            };
            branches[from] = new Branch(branches[from].Id, source);
            bool home = target.Count == before.FlockSize && target.All(t => t.Species == species);
            if (home)
            {
                events.Add(new MoveEvent(EventKind.Home, to, -1, to, species, 0, -1, target));
                branches[to] = new Branch(branches[to].Id, Array.Empty<Token>(), species);
                events.Add(new MoveEvent(EventKind.Retired, -1, -1, to, species, -1, -1, Array.Empty<Token>()));
            }
            else branches[to] = new Branch(branches[to].Id, target);
            candidate = new State(before.FlockSize, before.SpeciesCount, branches,
                checked(before.MoveCount + 1), checked(before.Sequence + 1));
            result = new MoveResult(from, to, count, run, before, candidate, events);
            return true;
        }
    }

    // In-memory stable transactions only. Persistence must confirm a candidate before presentation.
    public sealed class Session
    {
        private readonly State initial;
        private readonly Stack<State> history = new Stack<State>();
        public State State { get; private set; }
        public int UndoCount => history.Count;

        public Session() : this(global::PerchSort.Core.FirstLevel.State.CreateDocumentedFirstLevel()) { }
        public Session(State initialState)
        {
            initial = initialState?.Clone() ?? throw new ArgumentNullException(nameof(initialState));
            State = initial.Clone();
        }

        public bool TryMove(int from, int to, out MoveResult result)
        {
            if (!Rules.TryApply(State, from, to, out var candidate, out result)) return false;
            history.Push(State);
            State = candidate;
            return true;
        }

        public bool Undo()
        {
            if (history.Count == 0) return false;
            var restored = history.Peek().WithCounters(history.Peek().MoveCount, checked(State.Sequence + 1));
            history.Pop();
            State = restored;
            return true;
        }

        public void Restart()
        {
            State = initial.WithCounters(0, checked(State.Sequence + 1));
            history.Clear();
        }
    }
}
