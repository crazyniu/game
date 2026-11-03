using System;
using System.Collections.Generic;

namespace PerchSort.V5.Meta
{
    public sealed class SpeciesCard
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string PersonalityKey { get; }
        public string QuirkKey { get; }
        public int DebutLevel { get; }
        public bool CanWearOutfits => Id != "crow";
        internal SpeciesCard(string id, string name, int level)
        {
            Id = id; DisplayName = name; DebutLevel = level;
            PersonalityKey = "aviary." + id + ".personality"; QuirkKey = "aviary." + id + ".quirk";
        }
    }

    public readonly struct SpeciesMetResult
    {
        public PlayerProfile Profile { get; }
        public IReadOnlyList<SpeciesCard> NewSpecies { get; }
        public WinPayout Payout { get; }
        internal SpeciesMetResult(PlayerProfile profile, IReadOnlyList<SpeciesCard> species)
        { Profile = profile; NewSpecies = species; Payout = new WinPayout(species: checked(species.Count * 20)); }
    }

    public static class Aviary
    {
        private static readonly SpeciesCard[] cards =
        {
            new SpeciesCard("sunny", "Sunny", 1), new SpeciesCard("sky", "Skye", 1),
            new SpeciesCard("berry", "Berry", 3), new SpeciesCard("mint", "Minty", 6),
            new SpeciesCard("cocoa", "Cocoa", 9), new SpeciesCard("snow", "Flurry", 14),
            new SpeciesCard("ember", "Ember", 19), new SpeciesCard("crow", "Rook", 21),
            new SpeciesCard("ink", "Inky", 26), new SpeciesCard("hoot", "Hoot", 33)
        };
        private static readonly IReadOnlyList<SpeciesCard> noNewSpecies = Array.AsReadOnly(Array.Empty<SpeciesCard>());
        public static IReadOnlyList<SpeciesCard> Species { get; } = Array.AsReadOnly(cards);
        public static bool TryGetSpecies(string id, out SpeciesCard card)
        {
            foreach (var candidate in cards) if (candidate.Id == id) { card = candidate; return true; }
            card = null; return false;
        }

        public static SpeciesMetResult OnLevelStarted(PlayerProfile profile, IEnumerable<string> levelSpeciesIds)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (levelSpeciesIds == null) throw new ArgumentNullException(nameof(levelSpeciesIds));
            var present = new HashSet<string>(StringComparer.Ordinal);
            foreach (string id in levelSpeciesIds)
            {
                if (!TryGetSpecies(id, out _)) throw new ArgumentException("Unknown species: " + id, nameof(levelSpeciesIds));
                present.Add(id);
            }
            PlayerProfile next = profile;
            List<SpeciesCard> newlyMet = null;
            // Canonical debut order, independent of board traversal order and duplicates.
            foreach (var card in cards)
            {
                if (!present.Contains(card.Id) || profile.HasMetSpecies(card.Id)) continue;
                if (newlyMet == null) { newlyMet = new List<SpeciesCard>(); next = new PlayerProfile(profile); }
                next.Met.Add(card.Id); newlyMet.Add(card);
            }
            if (newlyMet == null) return new SpeciesMetResult(profile, noNewSpecies);
            next.Coins = checked(next.Coins + newlyMet.Count * 20);
            return new SpeciesMetResult(next, newlyMet.AsReadOnly());
        }

        // Integrate committed winning-attempt homecomings, never animation events or undoable previews.
        public static PlayerProfile IncrementFlocksHome(PlayerProfile profile, string speciesId, int count = 1)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (!TryGetSpecies(speciesId, out _) || speciesId == "crow")
                throw new ArgumentException("Only flock species can go home.", nameof(speciesId));
            if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));
            var next = new PlayerProfile(profile);
            next.Homes[speciesId] = checked(profile.FlocksHome(speciesId) + count); return next;
        }
    }
}
