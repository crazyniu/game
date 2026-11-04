using System;
using System.IO;
using UnityEngine;

namespace PerchSort.UI
{
    [Serializable]
    public sealed class SettingsData
    {
        public int version = 2;
        public float soundVolume = 1;
        public float musicVolume = 1;
        public bool soundEnabled = true;
        public bool musicEnabled = true;
        public bool vibrationEnabled = true;
        public bool reducedMotion = false;
    }

    public sealed class SettingsStore
    {
        public static string DefaultPath => Path.Combine(Application.persistentDataPath, "settings.json");
        public static event Action<SettingsData> Changed;
        private readonly string path;
        public SettingsStore(string filePath) => path = filePath;
        public SettingsData Load()
        {
            if (!File.Exists(path)) return new SettingsData();
            var data = JsonUtility.FromJson<SettingsData>(File.ReadAllText(path));
            if (data.version == 1)
            {
                data.version = 2; data.soundVolume = data.musicVolume = 1;
                Save(data);
            }
            if (data.version != 2) throw new InvalidDataException($"Unsupported settings version: {data.version}");
            return data;
        }
        public void Save(SettingsData data)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path + ".tmp", JsonUtility.ToJson(data, true));
            if (File.Exists(path)) File.Replace(path + ".tmp", path, path + ".bak");
            else File.Move(path + ".tmp", path);
            Changed?.Invoke(data);
        }
    }
}
