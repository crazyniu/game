using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PerchSort.V5.Core
{
    public readonly struct Species : IEquatable<Species>
    {
        public string Id { get; }
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

    public readonly struct Bird
    {
        public int Id { get; }
        public Species Species { get; }
        public Bird(int id, Species species) { Id = id; Species = species; }
    }

    public sealed class BranchState
    {
        internal readonly List<Bird> Items;
        public IReadOnlyList<Bird> Birds { get; }
        public bool IsRight { get; }
        public int Row { get; }
        public int Capacity { get; }
        public int FreeSlots => Capacity - Items.Count;
        public int RunCount
        {
            get
            {
                int count = Items.Count;
                if (count == 0) return 0;
                int start = count - 1;
                while (start > 0 && Items[start - 1].Species.Equals(Items[count - 1].Species)) start--;
                return count - start;
            }
        }
        internal BranchState(int capacity, bool right, int row, IEnumerable<Bird> birds)
        {
            Capacity = capacity; IsRight = right; Row = row;
            Items = new List<Bird>(birds);
            Birds = Items.AsReadOnly();
        }
        internal BranchState Clone() => new BranchState(Capacity, IsRight, Row, Items);
    }

    public sealed class BoardState
    {
        internal readonly List<BranchState> BranchItems;
        internal readonly List<Species> HomeItems;
        public IReadOnlyList<BranchState> Branches { get; }
        public IReadOnlyList<Species> HomedSpecies { get; }
        public int SpeciesCount { get; }
        public int Capacity { get; }
        public int Moves { get; internal set; }
        public bool AddedBranch { get; internal set; }
        public bool IsWon => HomeItems.Count == SpeciesCount;
        internal BoardState(int capacity, int speciesCount, List<BranchState> branches)
        {
            Capacity = capacity; SpeciesCount = speciesCount; BranchItems = branches;
            HomeItems = new List<Species>();
            Branches = BranchItems.AsReadOnly(); HomedSpecies = HomeItems.AsReadOnly();
        }
        public BoardState Clone()
        {
            var branches = new List<BranchState>(BranchItems.Count);
            foreach (var branch in BranchItems) branches.Add(branch.Clone());
            var copy = new BoardState(Capacity, SpeciesCount, branches) { Moves = Moves, AddedBranch = AddedBranch };
            copy.HomeItems.AddRange(HomeItems);
            return copy;
        }
    }

    [Serializable]
    public sealed class LevelCatalog { public LevelDef[] levels; }

    // Field DTOs are deserialized by the Unity layer; Core has no engine dependency.
    [Serializable]
    public sealed class LevelDef
    {
        public int id;
        public int capacity;
        public int par;
        public BranchDef[] branches;
        public BoardState CreateBoard()
        {
            if (id < 1 || capacity < 1 || par < 0 || branches == null || branches.Length == 0)
                throw new ArgumentException("Invalid level header.");
            var counts = new Dictionary<Species, int>();
            var slots = new HashSet<string>(StringComparer.Ordinal);
            var result = new List<BranchState>(branches.Length + 1);
            int nextId = 0;
            foreach (var branch in branches)
            {
                if (branch == null || (branch.side != "L" && branch.side != "R") || branch.row < 0 ||
                    branch.birds == null || branch.birds.Length > capacity || !slots.Add(branch.side + branch.row))
                    throw new ArgumentException("Invalid branch definition.");
                var birds = new List<Bird>(branch.birds.Length);
                foreach (string id in branch.birds)
                {
                    var species = new Species(id);
                    counts.TryGetValue(species, out int count);
                    counts[species] = count + 1;
                    birds.Add(new Bird(nextId++, species));
                }
                result.Add(new BranchState(capacity, branch.side == "R", branch.row, birds));
            }
            if (counts.Count == 0) throw new ArgumentException("A level needs birds.");
            foreach (int count in counts.Values)
                if (count != capacity) throw new ArgumentException("Each species must have exactly capacity birds.");
            return new BoardState(capacity, counts.Count, result);
        }
    }

    [Serializable]
    public sealed class BranchDef
    {
        public string side;
        public int row;
        public string[] birds;
    }
}
