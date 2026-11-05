using System.Collections;
using System.IO;
using UnityEngine;

namespace Game.UI
{
    // Saves every rendered frame at a fixed frame rate together with a UiTiming log.
    // Time.captureFramerate makes game time advance exactly 1 / fps per frame, so the
    // frames show real motion timing even when rendering is slower than real time.
    // Needs a graphics device: run in the Editor or in batch mode without -nographics.
    public sealed class TimedCapture : MonoBehaviour
    {
        private string folder;
        private int frame;

        public static TimedCapture Begin(string folder, int fps = 30)
        {
            Directory.CreateDirectory(folder);
            UiMotion.Instant = false;
            Time.captureFramerate = fps;
            UiTiming.Open(Path.Combine(folder, "timing.jsonl"));

            var capture = new GameObject(nameof(TimedCapture)).AddComponent<TimedCapture>();
            capture.folder = folder;
            capture.StartCoroutine(capture.CaptureFrames());
            return capture;
        }

        public void End()
        {
            StopAllCoroutines();
            Time.captureFramerate = 0;
            UiTiming.Close();
            Destroy(gameObject);
        }

        private IEnumerator CaptureFrames()
        {
            var endOfFrame = new WaitForEndOfFrame();
            while (true)
            {
                yield return endOfFrame;
                var texture = ScreenCapture.CaptureScreenshotAsTexture();
                File.WriteAllBytes(Path.Combine(folder, $"frame_{frame:D4}.png"), texture.EncodeToPNG());
                Destroy(texture);
                frame++;
            }
        }

        private void OnDestroy()
        {
            if (Time.captureFramerate != 0) Time.captureFramerate = 0;
        }
    }
}
