using System;
using System.Collections.Generic;
using System.Text;

namespace PerchSort.Core
{
    public enum ShelfSide { Left, Right }

    [Serializable]
    public sealed class CrateState
    {
        public int? RevealAfter;
        public int? RevealSpecies;
        public CrateState Clone() => new CrateState { RevealAfter = RevealAfter, RevealSpecies = RevealSpecies };
    }

    [Serializable]
    public sealed class PerchState
    {
        // Wall -> aisle on BOTH sides. Rendering mirrors the right column.
        public List<int> Birds = new List<int>();
        public int Capacity = 6;
        public ShelfSide Side;
        public bool Asleep;
        public CrateState Crate;
        public PerchState() { }
        public PerchState(IEnumerable<int> birds) { Birds.AddRange(birds); }
        public PerchState Clone() => new PerchState(Birds)
        { Capacity = Capacity, Side = Side, Asleep = Asleep, Crate = Crate?.Clone() };
    }

    [Serializable]
    public sealed class BasketState
    {
        public List<int> Birds = new List<int>();
        public int Capacity => 1;
        public BasketState Clone() => new BasketState { Birds = new List<int>(Birds) };
    }

    [Serializable]
    public sealed class BoardState
    {
        public const int BirdsPerSpecies = 6;
        public List<PerchState> Perches = new List<PerchState>();
        public List<PerchState> Shelves => Perches;
        public List<BasketState> Baskets = new List<BasketState>();
        public int Capacity = 6;
        public int SpeciesCount;
        public List<int> Targets = new List<int>();
        // A target can be collected out of order within the active pair.
        public List<int> CollectedTargets = new List<int>();
        public int CollectedCount => CollectedTargets.Count;
        public int CompletedCount;
        // Location ids: shelves [0, P), baskets [P, P+B).
        public int LocationCount => Perches.Count + Baskets.Count;
        public int BasketIndex(int basket) => Perches.Count + basket;
        public bool IsBasket(int location) => location >= Perches.Count;
        public List<int> BirdsAt(int location) => IsBasket(location) ? Baskets[location - Perches.Count].Birds : Perches[location].Birds;
        public int CapacityAt(int location) => IsBasket(location) ? 1 : Perches[location].Capacity;
        public List<int> ActiveTargets
        {
            get
            {
                var result = new List<int>(2);
                foreach (int target in Targets)
                    if (!CollectedTargets.Contains(target))
                    { result.Add(target); if (result.Count == 2) break; }
                return result;
            }
        }
        public BoardState() { }
        public BoardState(int capacity, int speciesCount, IEnumerable<IEnumerable<int>> perches)
        {
            Capacity = capacity;
            SpeciesCount = speciesCount;
            for (int i = 0; i < speciesCount; i++) Targets.Add(i);
            foreach (var birds in perches)
                Perches.Add(new PerchState(birds) { Capacity = capacity, Side = Perches.Count % 2 == 0 ? ShelfSide.Left : ShelfSide.Right });
        }
        public BoardState Clone()
        {
            var copy = new BoardState { Capacity = Capacity, SpeciesCount = SpeciesCount,
                CompletedCount = CompletedCount, Targets = new List<int>(Targets),
                CollectedTargets = new List<int>(CollectedTargets) };
            foreach (var perch in Perches) copy.Perches.Add(perch.Clone());
            foreach (var basket in Baskets) copy.Baskets.Add(basket.Clone());
            return copy;
        }
        // Exact, ordered key: no symmetry reduction can change stable auto-home order.
        public string GetStateKey()
        {
            var key = new StringBuilder();
            key.Append(Capacity).Append(':').Append(SpeciesCount).Append(':').Append(CompletedCount).Append('|');
            key.AppendJoin(",", Targets).Append('|');
            foreach (int target in Targets) key.Append(CollectedTargets.Contains(target) ? '1' : '0');
            key.Append('|');
            foreach (var shelf in Perches)
            {
                key.Append(shelf.Capacity).Append(':').Append((int)shelf.Side).Append(':').Append(shelf.Asleep ? '1' : '0').Append(':');
                if (shelf.Crate != null) key.Append(shelf.Crate.RevealAfter?.ToString() ?? "-").Append(',').Append(shelf.Crate.RevealSpecies?.ToString() ?? "-");
                key.Append('[').AppendJoin(",", shelf.Birds).Append(']');
            }
            key.Append('|');
            foreach (var basket in Baskets) key.Append('[').AppendJoin(",", basket.Birds).Append(']');
            return key.ToString();
        }
        public ulong GetStableHash()
        {
            ulong hash = 14695981039346656037UL;
            unchecked { foreach (char c in GetStateKey()) { hash ^= c; hash *= 1099511628211UL; } }
            return hash;
        }
    }
}
