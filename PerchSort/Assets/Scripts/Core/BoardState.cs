using System;
using System.Collections.Generic;

namespace PerchSort.Core
{
    public readonly struct Species : IEquatable<Species>
    {
        public string Id { get; }
        public bool IsCrow => Id == "crow";
        public bool IsNocturnal => Id == "ink" || Id == "hoot";
        public Species(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Species needs an id.", nameof(id));
            Id = id;
        }
        public bool Equals(Species other) => string.Equals(Id, other.Id, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is Species other && Equals(other);
        public override int GetHashCode() => Id == null ? 0 : StringComparer.Ordinal.GetHashCode(Id);
        public override string ToString() => Id;
    }

    [Flags]
    public enum BirdFlags { None = 0, Egg = 1, Asleep = 2, Mama = 4, Chick = 8, RevealedEgg = 16 }
    public enum BranchKind { Normal, Twig, Growing, Swivel }
    public enum DayPhase { Day, Night }
    public enum BoosterKind { Firefly, Sapling, OwlEye, MorningBell, Scarecrow, SpringRain }

    public readonly struct Bird
    {
        public int Id { get; }
        public Species Species { get; }
        public BirdFlags Flags { get; }
        public bool IsEgg => (Flags & BirdFlags.Egg) != 0;
        // Authored sleep only; use BoardState.IsAsleep for effective day/night sleep.
        public bool IsAsleep => (Flags & BirdFlags.Asleep) != 0;
        public bool IsMama => (Flags & BirdFlags.Mama) != 0;
        public bool IsChick => (Flags & BirdFlags.Chick) != 0;
        public bool IsRevealedEgg => (Flags & BirdFlags.RevealedEgg) != 0;
        public Bird(int id, Species species, BirdFlags flags = BirdFlags.None)
        { Id = id; Species = species; Flags = flags; }
        internal Bird WithFlags(BirdFlags flags) => new Bird(Id, Species, flags);
    }

    public sealed class BoosterInventory
    {
        internal const int KindCount = 6;
        private readonly int[] counts;
        public int this[BoosterKind kind] => counts[Index(kind)];
        public BoosterInventory(IReadOnlyDictionary<BoosterKind, int> values = null)
        {
            counts = new int[KindCount];
            if (values == null) return;
            foreach (var pair in values)
            {
                if (pair.Value < 0) throw new ArgumentOutOfRangeException(nameof(values));
                counts[Index(pair.Key)] = pair.Value;
            }
        }
        private BoosterInventory(int[] values) { counts = (int[])values.Clone(); }
        private static int Index(BoosterKind kind)
        {
            if ((uint)kind >= KindCount) throw new ArgumentOutOfRangeException(nameof(kind));
            return (int)kind;
        }
        internal void Add(BoosterKind kind, int count) { counts[Index(kind)] += count; }
        public BoosterInventory Clone() => new BoosterInventory(counts);
    }

    public sealed class BranchState
    {
        internal readonly List<Bird> Items;
        // The owner supplies derived sleep without duplicating phase state on every branch.
        internal BoardState Owner;
        public IReadOnlyList<Bird> Birds { get; }
        public bool IsRight { get; }
        public int Row { get; }
        public BranchKind Kind { get; internal set; }
        public int Capacity { get; internal set; }
        public int FreeSlots => Capacity - Items.Count;
        public int RunCount
        {
            get
            {
                int count = Items.Count;
                if (count == 0) return 0;
                var tip = Items[count - 1];
                if (tip.IsEgg || Owner.IsAsleep(tip)) return 0;
                int start = count - 1;
                while (start > 0)
                {
                    var bird = Items[start - 1];
                    if (bird.IsEgg || Owner.IsAsleep(bird) || !bird.Species.Equals(tip.Species)) break;
                    start--;
                }
                return count - start;
            }
        }
        internal BranchState(int capacity, bool right, int row, IEnumerable<Bird> birds, BranchKind kind = BranchKind.Normal)
        {
            Capacity = capacity; IsRight = right; Row = row; Kind = kind;
            Items = new List<Bird>(capacity);
            Items.AddRange(birds);
            Birds = Items.AsReadOnly();
        }
        internal BranchState Clone() => new BranchState(Capacity, IsRight, Row, Items, Kind);
    }

    public sealed class BoardState
    {
        internal readonly List<BranchState> BranchItems;
        internal readonly List<Species> HomeItems;
        public IReadOnlyList<BranchState> Branches { get; }
        public IReadOnlyList<Species> HomedSpecies { get; }
        public int LevelId { get; }
        public int World { get; }
        public string Variant { get; }
        public int SpeciesCount { get; }
        public int Capacity { get; }
        public int DayLength { get; }
        public int Moves { get; internal set; }
        // Stars/display exclude Encore; solver depth and par still count every transfer/swivel in Moves.
        public int CountedMoves { get; internal set; }
        public bool Encore { get; internal set; }
        public bool AddedBranch { get; internal set; }
        public bool LightStep { get; internal set; }
        public DayPhase Phase { get; internal set; }
        public int PhaseCounter { get; internal set; }
        public int MoonbeamMovesLeft { get; internal set; }
        public int Chain { get; internal set; }
        public int ChorusCoins { get; internal set; }
        public BoosterInventory Inventory { get; internal set; }
        public bool IsWon => HomeItems.Count == SpeciesCount;
        public bool NocturnalDaySleep => DayLength > 0 && Phase == DayPhase.Day && MoonbeamMovesLeft == 0;
        public bool IsAsleep(Bird bird) => !bird.IsEgg && (bird.IsAsleep || (bird.Species.IsNocturnal && NocturnalDaySleep));
        internal BoardState(int levelId, int world, string variant, int capacity, int speciesCount,
            int dayLength, List<BranchState> branches, BoosterInventory inventory = null)
        {
            LevelId = levelId; World = world; Variant = variant; Capacity = capacity;
            SpeciesCount = speciesCount; DayLength = dayLength; BranchItems = branches;
            HomeItems = new List<Species>(speciesCount);
            Branches = BranchItems.AsReadOnly(); HomedSpecies = HomeItems.AsReadOnly();
            Inventory = inventory ?? new BoosterInventory();
            foreach (var branch in branches) branch.Owner = this;
        }
        public BoardState Clone() => Clone(true);
        // Search disables all economy mutations and never uses boosters, so its inventory is read-only.
        internal BoardState CloneForSearch() => Clone(false);
        private BoardState Clone(bool copyInventory)
        {
            var branches = new List<BranchState>(BranchItems.Count + 1);
            foreach (var branch in BranchItems) branches.Add(branch.Clone());
            var copy = new BoardState(LevelId, World, Variant, Capacity, SpeciesCount, DayLength, branches,
                copyInventory ? Inventory.Clone() : Inventory)
            {
                Moves = Moves, CountedMoves = CountedMoves, Encore = Encore,
                AddedBranch = AddedBranch, LightStep = LightStep, Phase = Phase,
                PhaseCounter = PhaseCounter, MoonbeamMovesLeft = MoonbeamMovesLeft,
                Chain = Chain, ChorusCoins = ChorusCoins
            };
            copy.HomeItems.AddRange(HomeItems);
            return copy;
        }
    }
}
