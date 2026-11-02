using System;
using System.Collections.Generic;

namespace PerchSort.V5.Core
{
    [Serializable]
    public sealed class LevelCatalog { public LevelDef[] levels; }

    // Field DTOs are deserialized by the Unity layer; Core has no engine dependency.
    [Serializable]
    public sealed class LevelDef
    {
        public int id;
        public int world;
        public string variant;
        public int capacity;
        public int par;
        public int dayLength;
        public BranchDef[] branches;

        public BoardState CreateBoard(SongState song = default, BoosterInventory inventory = null)
        {
            if (id < 1 || world < 0 || world > 4 || capacity < 1 || par < 0 || dayLength < 0 ||
                branches == null || branches.Length == 0)
                throw new ArgumentException("Invalid level header.");
            var counts = new Dictionary<Species, int>();
            var slots = new HashSet<string>(StringComparer.Ordinal);
            var result = new List<BranchState>(branches.Length + 1);
            int nextId = 0;
            foreach (var branch in branches)
            {
                if (branch == null || (branch.side != "L" && branch.side != "R") || branch.row < 0 ||
                    branch.birds == null || !slots.Add(branch.side + branch.row))
                    throw new ArgumentException("Invalid branch definition.");
                BranchKind kind = ParseKind(branch.kind);
                int seats = kind == BranchKind.Twig ? 2 : kind == BranchKind.Growing ? branch.seats : capacity;
                if (seats < 1 || (kind == BranchKind.Growing && (seats < 2 || seats > capacity)) ||
                    (kind != BranchKind.Growing && branch.seats != 0) || branch.birds.Length > seats)
                    throw new ArgumentException("Invalid branch seats.");
                var birds = new List<Bird>(branch.birds.Length);
                foreach (string token in branch.birds)
                {
                    var bird = ParseBird(nextId++, token);
                    if (!bird.Species.IsCrow)
                    {
                        counts.TryGetValue(bird.Species, out int count);
                        counts[bird.Species] = count + 1;
                    }
                    birds.Add(bird);
                }
                result.Add(new BranchState(seats, branch.side == "R", branch.row, birds, kind));
            }
            if (counts.Count == 0) throw new ArgumentException("A level needs a real species.");
            foreach (int count in counts.Values)
                if (count != capacity) throw new ArgumentException("Each real species must have exactly capacity birds.");
            // Authored-start restrictions belong to the level validator. Construction does not resolve actions.
            return new BoardState(id, world == 0 ? 1 : world, string.IsNullOrEmpty(variant) ? "noon" : variant,
                capacity, counts.Count, dayLength, result, inventory?.Clone())
            { SongNotes = song.Notes, PayoutCount = song.PayoutCount };
        }

        private static BranchKind ParseKind(string kind)
        {
            switch (kind)
            {
                case null: case "": case "normal": return BranchKind.Normal;
                case "twig": return BranchKind.Twig;
                case "growing": return BranchKind.Growing;
                case "swivel": return BranchKind.Swivel;
                default: throw new ArgumentException("Unknown branch kind: " + kind);
            }
        }

        private static Bird ParseBird(int id, string token)
        {
            if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException("Empty bird token.");
            string species = token;
            BirdFlags flags = BirdFlags.None;
            int colon = token.IndexOf(':');
            if (colon >= 0)
            {
                string prefix = token.Substring(0, colon);
                string suffix = token.Substring(colon + 1);
                if (suffix.IndexOf(':') >= 0) throw new ArgumentException("Invalid bird token: " + token);
                if (prefix == "egg") { species = suffix; flags = BirdFlags.Egg; }
                else
                {
                    species = prefix;
                    switch (suffix)
                    {
                        case "sleep": flags = BirdFlags.Asleep; break;
                        case "mama": flags = BirdFlags.Mama; break;
                        case "chick": flags = BirdFlags.Chick; break;
                        default: throw new ArgumentException("Invalid bird token: " + token);
                    }
                }
            }
            if (species == "egg" || (species == "crow" && flags != BirdFlags.None))
                throw new ArgumentException("Invalid special bird token: " + token);
            return new Bird(id, new Species(species), flags);
        }
    }

    [Serializable]
    public sealed class BranchDef
    {
        public string side;
        public int row;
        public string kind;
        public int seats;
        public string[] birds;
    }
}
