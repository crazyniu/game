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
            int local = (level - 1) % WorldMap.LevelsPerWorld + 1;
            Vector2 point = local >= 6 ? GameLayout.Screen("map", "level_" + local).center :
                local == 1 ? new Vector2(610, 2750) : local == 2 ? new Vector2(310, 2530) :
                local == 3 ? new Vector2(550, 2240) : local == 4 ? new Vector2(780, 2050) : new Vector2(350, 1810);
            return point + Vector2.up * GameLayout.WorldTop((level - 1) / WorldMap.LevelsPerWorld + 1);
        }
        // Crop and place the existing scenery in contiguous bands. These source rows put each
        // measured node on the painted trail, while retaining both world transition bands.
        public static readonly float[] SceneryRows = { 0, 160, 470, 610, 795, 1140, 1505, 1760, 2010, 2280, 2500, 2750, 2840, 3000 };
        public static readonly float[] LayoutRows = { 0, 160, 656, 841, 1030, 1241, 1436, 1810, 2050, 2240, 2530, 2750, 2840, 3000 };
        public static Vector2 GatePosition(int world)
        {
            return GameLayout.Screen("map", "gate").center + Vector2.up * GameLayout.WorldTop(world - 1);
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
