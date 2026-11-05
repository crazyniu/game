using System;
using UnityEngine;
using PerchSort.Meta;

namespace PerchSort.Game
{
    public enum LevelNodeState { Locked, Open, Current, Cleared }
    public static class MapModel
    {
        public static bool WorldOpen(GameProfile profile, int world) => world == 1 || profile.DevOptions.unlockAllLevels || profile.BestStars((world - 1) * WorldMap.LevelsPerWorld) > 0;
        public static bool Selectable(GameProfile profile, int level) => level >= 1 && level <= 40 && level <= profile.AccessibleLevel && WorldOpen(profile, (level - 1) / 10 + 1);
        public static int CurrentLevel(GameProfile profile)
        {
            for (int i = 1; i <= 40; i++) if (Selectable(profile, i) && profile.BestStars(i) == 0) return i;
            return Math.Min(40, profile.AccessibleLevel);
        }
        public static LevelNodeState State(GameProfile profile, int level)
        {
            if (!Selectable(profile, level)) return LevelNodeState.Locked;
            if (profile.BestStars(level) > 0) return LevelNodeState.Cleared;
            return level == CurrentLevel(profile) ? LevelNodeState.Current : LevelNodeState.Open;
        }
        public static Vector2 Position(int level)
        {
            return GameLayout.MapNode((level - 1) / WorldMap.LevelsPerWorld + 1, (level - 1) % WorldMap.LevelsPerWorld);
        }
        public static Vector2 GatePosition(int world)
        {
            // The shared band is too short for a gate between the authored boundary nodes.
            // Keep the gate beside the path, clear of both node faces and the current chick.
            return new Vector2(180, GameLayout.WorldTop(world - 1) + GameLayout.MapOverlap * 0.5f);
        }
        public static int WorldAt(float contentY)
        {
            int nearest = 1; float distance = float.MaxValue;
            for (int i = 1; i <= 40; i++)
            {
                float delta = Math.Abs(Position(i).y - contentY);
                if (delta < distance) { nearest = i; distance = delta; }
            }
            return (nearest - 1) / 10 + 1;
        }
    }
}
