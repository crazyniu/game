using System;
using PerchSort.Core;

namespace PerchSort.Game
{
    // Test/capture hooks only: the normal game always loads the authored resource catalog.
    public static class GameFixtures
    {
        public static readonly string[] Names = { "eggs", "sleep", "crow_twig", "swivel", "growing", "night" };
        private static BranchDef B(int index, string[] birds, string kind = "normal", int seats = 0) =>
            new BranchDef { side = index % 2 == 0 ? "L" : "R", row = index / 2, birds = birds, kind = kind, seats = seats };
        public static LevelDef Create(string name)
        {
            var level = new LevelDef { id = 33, world = 1, variant = "noon", capacity = 4, par = 8 };
            switch (name)
            {
                case "eggs": level.branches = new[] {
                    B(0, new[] { "egg:sky", "sky", "sunny" }), B(1, new[] { "sunny", "sunny" }),
                    B(2, new[] { "sky", "sky", "sunny" }), B(3, Array.Empty<string>()) }; break;
                case "sleep": level.branches = new[] {
                    B(0, new[] { "sunny", "sunny", "sunny:sleep" }), B(1, new[] { "sunny" }),
                    B(2, new[] { "sky", "sky" }), B(3, new[] { "sky", "sky" }) }; break;
                case "crow_twig": level.branches = new[] {
                    B(0, new[] { "sunny", "sunny", "sunny", "crow" }), B(1, new[] { "sky", "sky", "sky", "sunny" }),
                    B(2, new[] { "sky" }), B(3, Array.Empty<string>()), B(4, new[] { "crow" }, "twig"), B(5, Array.Empty<string>()) }; break;
                case "swivel": level.branches = new[] {
                    B(0, new[] { "egg:sky", "sunny", "sky" }, "swivel"), B(1, new[] { "sunny", "sunny", "sky" }),
                    B(2, new[] { "sunny", "sky" }), B(3, Array.Empty<string>()) }; break;
                case "growing": level.branches = new[] {
                    B(0, new[] { "sunny", "sunny", "sunny" }), B(1, new[] { "sunny" }),
                    B(2, new[] { "sky", "sky", "sky" }), B(3, Array.Empty<string>(), "growing", 2),
                    B(4, new[] { "sky" }), B(5, Array.Empty<string>()) }; break;
                case "night": level.dayLength = 1; level.branches = new[] {
                    B(0, new[] { "hoot", "hoot", "hoot" }), B(1, new[] { "hoot", "sunny" }),
                    B(2, new[] { "sunny", "sunny", "sunny" }), B(3, Array.Empty<string>()) }; break;
                default: throw new ArgumentException("Unknown mechanic fixture: " + name);
            }
            return level;
        }
        public static LevelDef Combined() => new LevelDef
        {
            id = 33, world = 1, variant = "morning", capacity = 4, par = 8,
            branches = new[] {
                B(0, new[] { "egg:sky", "sunny", "sky" }, "swivel"),
                B(1, new[] { "sunny:sleep", "sunny", "sky" }),
                B(2, new[] { "sunny", "sky", "crow" }), B(3, Array.Empty<string>(), "twig"),
                B(4, Array.Empty<string>()), B(5, Array.Empty<string>()) }
        };
    }
}
