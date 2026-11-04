using System;
using System.Collections.Generic;

namespace PerchSort.Meta
{
    public enum MapNodeState { Locked, Current, Cleared }
    public readonly struct MapPosition
    {
        public double X { get; }
        public double Y { get; }
        internal MapPosition(double x, double y) { X = x; Y = y; }
    }
    public readonly struct LevelNode
    {
        public int Level { get; }
        public int World => (Level - 1) / 10 + 1;
        public MapNodeState State { get; }
        public int Stars { get; }
        public MapPosition Position => WorldMap.NodePosition(Level);
        internal LevelNode(int level, MapNodeState state, int stars) { Level = level; State = state; Stars = stars; }
    }
    public sealed class MapWorld
    {
        public int Id { get; }
        public string NameKey { get; }
        public bool IsUnlocked { get; }
        public bool IsComplete { get; }
        public IReadOnlyList<LevelNode> Nodes { get; }
        internal MapWorld(PlayerProfile profile, int id, string nameKey)
        {
            Id = id; NameKey = nameKey; IsUnlocked = Unlocks.IsWorldUnlocked(profile, id);
            IsComplete = Unlocks.IsWorldComplete(profile, id);
            var nodes = new LevelNode[10];
            for (int i = 0; i < nodes.Length; i++) nodes[i] = WorldMap.Node(profile, (id - 1) * 10 + i + 1);
            Nodes = Array.AsReadOnly(nodes);
        }
    }
    public static class WorldMap
    {
        public const int WorldCount = 4;
        public const int LevelsPerWorld = 10;
        public static int? NextLevelId(PlayerProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            for (int level = 1; level <= 40; level++)
                if (profile.BestStars(level) == 0 && Unlocks.IsLevelUnlocked(profile, level)) return level;
            return null;
        }
        public static LevelNode Node(PlayerProfile profile, int level)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            int stars = profile.BestStars(level);
            return new LevelNode(level, stars > 0 ? MapNodeState.Cleared :
                Unlocks.IsLevelUnlocked(profile, level) ? MapNodeState.Current : MapNodeState.Locked, stars);
        }
        public static IReadOnlyList<MapWorld> Build(PlayerProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            return Array.AsReadOnly(new[]
            {
                new MapWorld(profile, 1, "world.springWoods"), new MapWorld(profile, 2, "world.bambooGrove"),
                new MapWorld(profile, 3, "world.mapleAutumn"), new MapWorld(profile, 4, "world.snowPine")
            });
        }
        // Full 40-level scroll space: L1 at the bottom; four smooth lateral cycles, no world-boundary reset.
        public static MapPosition NodePosition(int level)
        {
            PlayerProfile.CheckLevel(level);
            double progress = (level - 1) / 39.0;
            return new MapPosition(0.5 + 0.22 * Math.Sin(progress * Math.PI * 8), 0.95 - 0.9 * progress);
        }
    }
}
