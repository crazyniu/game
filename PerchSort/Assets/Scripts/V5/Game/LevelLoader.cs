using System;
using PerchSort.V5.Core;
using UnityEngine;

namespace PerchSort.V5.Game
{
    public static class LevelLoader
    {
        public static LevelDef Load(int id)
        {
            var resource = Resources.Load<TextAsset>("V5/levels");
            if (resource == null) throw new InvalidOperationException("Missing V5/levels.json.");
            return Parse(resource.text, id);
        }
        public static LevelDef Parse(string json, int id)
        {
            var catalog = JsonUtility.FromJson<LevelCatalog>(json);
            if (catalog == null || catalog.levels == null) throw new ArgumentException("Invalid V5 level catalog.");
            foreach (var level in catalog.levels)
                // GameSession validates and constructs the board once from this DTO.
                if (level != null && level.id == id) return level;
            throw new ArgumentException("V5 level not found: " + id);
        }
    }
}
