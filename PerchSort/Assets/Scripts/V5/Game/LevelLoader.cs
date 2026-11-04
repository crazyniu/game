using System;
using PerchSort.V5.Core;
using UnityEngine;

namespace PerchSort.V5.Game
{
    public static class LevelLoader
    {
        private static LevelCatalog cached;
        public static LevelCatalog Catalog
        {
            get
            {
                if (cached != null) return cached;
                var resource = Resources.Load<TextAsset>("V5/levels");
                if (resource == null) throw new InvalidOperationException("Missing V5/levels.json.");
                var catalog = ParseCatalog(resource.text);
                for (int i = 0; i < catalog.levels.Length; i++)
                    if (catalog.levels[i].id != i + 1) throw new ArgumentException("V5 resource ids must be contiguous from 1.");
                cached = catalog; return cached;
            }
        }
        public static LevelDef Load(int id) => Find(Catalog, id);
        public static LevelDef Parse(string json, int id) => Find(ParseCatalog(json), id);
        public static LevelCatalog ParseCatalog(string json)
        {
            var catalog = JsonUtility.FromJson<LevelCatalog>(json);
            if (catalog == null || catalog.levels == null || catalog.levels.Length == 0) throw new ArgumentException("Invalid V5 level catalog.");
            foreach (var level in catalog.levels)
                if (level == null || level.id < 1) throw new ArgumentException("Invalid V5 level id.");
            Array.Sort(catalog.levels, (a, b) => a.id.CompareTo(b.id));
            for (int i = 1; i < catalog.levels.Length; i++)
                if (catalog.levels[i].id == catalog.levels[i - 1].id) throw new ArgumentException("Duplicate V5 level id.");
            return catalog;
        }
        private static LevelDef Find(LevelCatalog catalog, int id)
        {
            foreach (var level in catalog.levels) if (level.id == id) return level;
            throw new ArgumentException("V5 level not found: " + id);
        }
    }
}
