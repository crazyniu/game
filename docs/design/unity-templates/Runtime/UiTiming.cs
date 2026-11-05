using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace Game.UI
{
    // Timing log for animation and sound evidence. Call Mark at the start and end of every
    // animation and when every sound starts. Compiled out of release builds.
    public static class UiTiming
    {
        private static StreamWriter writer;

        public static void Open(string path)
        {
            Close();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            writer = new StreamWriter(path, false) { AutoFlush = true };
        }

        public static void Close()
        {
            writer?.Dispose();
            writer = null;
        }

        // phase is "start", "end" or "sound". Time is game time, which advances exactly
        // 1 / captureFramerate per frame while TimedCapture runs.
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Mark(string name, string phase)
        {
            if (writer == null) return;
            writer.WriteLine(
                $"{{\"t\":{Time.time:F4},\"frame\":{Time.frameCount},\"name\":\"{name}\",\"phase\":\"{phase}\"}}");
        }
    }
}
