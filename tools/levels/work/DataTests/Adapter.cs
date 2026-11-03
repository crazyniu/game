using System;
using System.IO;
using System.Text.Json;

namespace UnityEngine
{
    public sealed class TextAsset
    {
        public string text;
    }
    public static class Resources
    {
        public static T Load<T>(string name) where T : class
        {
            if (typeof(T) != typeof(TextAsset) || name != "V5/levels") return null;
            string path = Environment.GetEnvironmentVariable("PERCH_LEVELS_FILE") ?? "PerchSort/Assets/Resources/V5/levels.json";
            return new TextAsset { text = File.ReadAllText(path) } as T;
        }
    }
    public static class JsonUtility
    {
        public static T FromJson<T>(string text)
            => JsonSerializer.Deserialize<T>(text, new JsonSerializerOptions { IncludeFields = true });
    }
}
