using System;
using PerchSort.V5.Core;

namespace PerchSort.V5.Game
{
    // Presentation/economy policy only. Legality and transactions remain in Core.
    public static class V5ViewModel
    {
        public const int BoosterCount = 6;
        private static readonly int[] unlocks = { 2, 4, 7, 12, 22, 32 };
        private static readonly int[] prices = { 30, 60, 80, 80, 100, 100 };
        private static readonly string[] icons = { "icon_firefly", "icon_sapling", "icon_owl_eye", "icon_bell", "icon_scarecrow", "icon_rain" };
        public static int Unlock(BoosterKind kind) => unlocks[(int)kind];
        public static int Price(BoosterKind kind) => prices[(int)kind];
        public static int Starter(BoosterKind kind) => kind == BoosterKind.Firefly ? 3 : 2;
        public static string Icon(BoosterKind kind) => icons[(int)kind];
        public static BoosterKind WorldBooster(int world) => (BoosterKind)(Math.Max(1, Math.Min(4, world)) + 1);
        public static BoosterKind ContextBooster(BoardState board, out bool mechanicPresent)
        {
            int present = 0;
            foreach (var branch in board.Branches)
            {
                if (branch.Kind == BranchKind.Growing) present |= 1 << (int)BoosterKind.SpringRain;
                foreach (var bird in branch.Birds)
                {
                    if (bird.IsEgg) present |= 1 << (int)BoosterKind.OwlEye;
                    if (bird.IsAsleep || bird.Species.IsNocturnal) present |= 1 << (int)BoosterKind.MorningBell;
                    if (bird.Species.IsCrow) present |= 1 << (int)BoosterKind.Scarecrow;
                }
            }
            var world = WorldBooster(board.World);
            mechanicPresent = present != 0;
            if ((present & (1 << (int)world)) != 0) return world;
            for (int kind = (int)BoosterKind.OwlEye; kind <= (int)BoosterKind.SpringRain; kind++)
                if ((present & (1 << kind)) != 0) return (BoosterKind)kind;
            return world;
        }

        public static string GiftIcon(string species)
        {
            switch (species)
            {
                case "sunny": return "gift_wake";
                case "sky": return "gift_breeze";
                case "berry": return "gift_encore";
                case "mint": return "gift_gardener";
                case "cocoa": return "gift_stare";
                case "snow": return "gift_lightstep";
                case "ember": return "gift_warm";
                case "ink": return "gift_night";
                case "hoot": return "gift_moonbeam";
                default: throw new ArgumentException("Species has no homecoming gift: " + species);
            }
        }
        public static string Background(string variant)
        {
            switch (variant)
            {
                case "morning": case "mist": return "bg_w1_morning";
                case "golden": case "lanterns": case "sunset": case "bluehour": case "aurora": return "bg_w1_golden";
                case "rain": case "drizzle": return "bg_w1_rain";
                default: return "bg_forest_day";
            }
        }
        public static bool Rain(string variant) => variant == "rain" || variant == "drizzle";
        public static string RestFrame(Bird bird, bool asleep) => bird.IsEgg ? "egg_idle" :
            "bird_" + bird.Species.Id + (asleep ? "_sleep" : "_idle");

        public static Move? GreedyHint(BoardState board)
        {
            Move? best = null;
            int bestScore = int.MinValue;
            for (int from = 0; from < board.Branches.Count; from++)
            {
                var source = board.Branches[from];
                for (int to = 0; to < board.Branches.Count; to++)
                {
                    if (!GameSession.CanMove(board, from, to)) continue;
                    var target = board.Branches[to];
                    var species = source.Birds[source.Birds.Count - 1].Species;
                    int count = Math.Min(source.RunCount, target.FreeSlots);
                    int arrivals = count;
                    bool mama = false;
                    for (int i = source.Birds.Count - count; i < source.Birds.Count; i++) mama |= source.Birds[i].IsMama;
                    if (mama)
                        for (int i = source.Birds.Count - count - 1; i >= 0 && arrivals < target.FreeSlots; i--)
                        {
                            var chick = source.Birds[i];
                            if (chick.IsChick && !chick.IsEgg && !board.IsAsleep(chick) && chick.Species.Equals(species)) arrivals++;
                        }
                    bool allSame = !species.IsCrow;
                    for (int i = 0; i < target.Birds.Count; i++)
                    {
                        var bird = target.Birds[i];
                        bool landingWake = i == target.Birds.Count - 1 && bird.IsAsleep &&
                            !(bird.Species.IsNocturnal && board.NocturnalDaySleep);
                        allSame &= !bird.IsEgg && (!board.IsAsleep(bird) || landingWake) && bird.Species.Equals(species);
                    }
                    int score = allSame && target.Kind != BranchKind.Twig && target.Birds.Count + arrivals == board.Capacity ? 10000 : 0;
                    if (target.Birds.Count > 0 && target.Birds[target.Birds.Count - 1].Species.Equals(species)) score += 100 + count;
                    if (source.Birds.Count > count && source.Birds[source.Birds.Count - count - 1].IsEgg) score += 10;
                    if (score > bestScore) { bestScore = score; best = new Move(from, to); }
                }
                if (!best.HasValue && GameSession.CanSwivel(board, from)) best = Move.Swivel(from);
            }
            return best;
        }
    }
}
