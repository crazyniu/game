using System;
using PerchSort.Core;

namespace PerchSort.Game
{
    // Presentation/economy policy only. Legality and transactions remain in Core.
    public static class ViewModel
    {
        public const int BoosterCount = 6;
        private static readonly int[] unlocks = { 2, 4, 7, 12, 22, 32 };
        private static readonly int[] prices = { 30, 60, 80, 80, 100, 100 };
        private static readonly string[] icons = { "icon_bulb", "icon_branch_plus", "icon_egg_eye", "icon_bell", "icon_scarecrow", "icon_rain_sprout" };
        private static readonly string[] illustrations = { "booster_hint_illus", "booster_sapling_illus", "booster_owl_eye_illus", "booster_bell_illus", "booster_scarecrow_illus", "booster_rain_illus" };
        private static readonly string[] toolGlyphs = { "tool_hint", "tool_branch", "tool_owl_eye", "tool_bell", "tool_scarecrow", "tool_rain" };
        public static int Unlock(BoosterKind kind) => unlocks[(int)kind];
        public static int Price(BoosterKind kind) => prices[(int)kind];
        public static int Starter(BoosterKind kind) => kind == BoosterKind.Firefly ? 3 : 2;
        public static string Icon(BoosterKind kind) => icons[(int)kind];
        public static string Illustration(BoosterKind kind) => illustrations[(int)kind];
        public static string ToolGlyph(BoosterKind kind) => toolGlyphs[(int)kind];
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
                case "sunny": return "gift_sun_note";
                case "sky": return "gift_wind";
                case "berry": return "gift_feather_counter";
                case "mint": return "gift_watering_can";
                case "cocoa": return "gift_grumpy_eyes";
                case "snow": return "gift_feather";
                case "ember": return "gift_flame";
                case "ink": return "gift_moon";
                case "hoot": return "gift_moon_ray";
                default: throw new ArgumentException("Species has no homecoming gift: " + species);
            }
        }
        public static bool Rain(string variant) => variant == "rain" || variant == "drizzle";

    }
}
