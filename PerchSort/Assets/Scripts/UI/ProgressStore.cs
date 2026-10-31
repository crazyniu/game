using System;
using System.Collections.Generic;
using System.IO;
using PerchSort.Core;
using UnityEngine;

namespace PerchSort.UI
{
    [Serializable] public sealed class SavedMove { public int from; public int to; }

    [Serializable]
    public sealed class LevelProgress
    {
        public int id, stars, hintsUsed, undos, restarts;
    }

    public static class StarRating
    {
        public static int Calculate(int undos, int restarts) => undos + restarts == 0 ? 3 : undos + restarts <= 3 ? 2 : 1;
    }

    [Serializable]
    public sealed class ProgressData
    {
        public int version = 4;
        public string language = "en";
        public List<int> tutorialsSeen = new List<int>();
        public List<LevelProgress> levels = new List<LevelProgress>();
        public LevelProgress ForLevel(int id)
        {
            var entry = levels.Find(level => level.id == id);
            if (entry == null) { entry = new LevelProgress { id = id }; levels.Add(entry); }
            return entry;
        }
        public int TotalStars { get { int count = 0; foreach (var level in levels) count += level.stars; return count; } }
        public int levelId = 1;
        public List<SavedMove> moves = new List<SavedMove>();
        public List<int> completedLevels = new List<int>();
        public bool IsCompleted(int id) => completedLevels.Contains(id);
        public bool IsUnlocked(int id)
        {
            for (int previous = 1; previous < id; previous++)
                if (!IsCompleted(previous)) return false;
            return true;
        }
    }

    public sealed class ProgressStore
    {
        private readonly string path;
        public ProgressStore(string filePath) => path = filePath;
        public ProgressData Load()
        {
            if (!File.Exists(path)) return new ProgressData();
            var data = JsonUtility.FromJson<ProgressData>(File.ReadAllText(path));
            if (data.version == 1 || data.version == 2)
            {
                // Both old save versions encode v1 single-bird moves and incompatible level layouts.
                data = new ProgressData();
                Save(data);
            }
            if (data.version == 3)
            {
                data.version = 4;
                data.language = "en";
                data.tutorialsSeen = new List<int>();
                data.levels = new List<LevelProgress>();
                // Historical completions have no assistance history: award one baseline star.
                foreach (int id in data.completedLevels) data.ForLevel(id).stars = 1;
                Save(data);
            }
            if (data.version != 4) throw new InvalidDataException($"Unsupported progress version: {data.version}");
            return data;
        }
        public void Save(ProgressData data)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temporary = path + ".tmp";
            File.WriteAllText(temporary, JsonUtility.ToJson(data, true));
            if (File.Exists(path)) File.Replace(temporary, path, path + ".bak");
            else File.Move(temporary, path);
        }
        public void Save(int levelId, IReadOnlyList<Move> moves)
        {
            var data = Load();
            data.levelId = levelId;
            data.moves.Clear();
            foreach (var move in moves) data.moves.Add(new SavedMove { from = move.From, to = move.To });
            Save(data);
        }
    }
}
