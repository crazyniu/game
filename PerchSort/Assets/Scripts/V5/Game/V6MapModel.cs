using System;
using UnityEngine;
using PerchSort.V5.Meta;

namespace PerchSort.V5.Game
{
    public enum V6NodeState { Locked, Open, Current, Cleared }
    public static class V6MapModel
    {
        public static bool WorldOpen(V5Profile profile, int world) => world == 1 || profile.DevOptions.unlockAllLevels || profile.BestStars((world - 1) * WorldMap.LevelsPerWorld) > 0;
        public static bool Selectable(V5Profile profile, int level) => level >= 1 && level <= 40 && level <= profile.AccessibleLevel && WorldOpen(profile, (level - 1) / 10 + 1);
        public static int CurrentLevel(V5Profile profile)
        {
            for (int i = 1; i <= 40; i++) if (Selectable(profile, i) && profile.BestStars(i) == 0) return i;
            return Math.Min(40, profile.AccessibleLevel);
        }
        public static V6NodeState State(V5Profile profile, int level)
        {
            if (!Selectable(profile, level)) return V6NodeState.Locked;
            if (profile.BestStars(level) > 0) return V6NodeState.Cleared;
            return level == CurrentLevel(profile) ? V6NodeState.Current : V6NodeState.Open;
        }
        public static Vector2 Position(int level)
        {
            var point = WorldMap.NodePosition(level);
            return new Vector2((float)point.X * V6Layout.DesignWidth, (float)point.Y * V6Layout.MapHeight);
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
