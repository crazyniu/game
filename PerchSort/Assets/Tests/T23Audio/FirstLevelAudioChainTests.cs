#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PerchSort.Audio;
using PerchSort.FirstLevelRuntime;
using Unity.Collections;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PerchSort.T23Tests
{
    // Runs only inside a disposable headless Editor, never against an existing Player.
    public sealed class FirstLevelAudioChainTests
    {
        Scene scene;
        FirstLevelEntry entry;
        AudioManager manager;
        string directory, previousDirectory;
        EngineAudioSample capture;
        GameObject cleanupListener;
        const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        AudioSource[] Voices => manager.GetComponents<AudioSource>().Where(s => !s.loop).ToArray();
        AudioSource[] Songs => manager.GetComponents<AudioSource>().Where(s => s.loop).ToArray();

        [UnitySetUp]
        public IEnumerator LoadDisposableScene()
        {
            Assert.That(AudioManager.Instance, Is.Null, "This fixture requires an isolated Editor.");
            previousDirectory = FirstLevelEntry.SaveDirectoryOverride;
            directory = Path.Combine(Path.GetTempPath(), "t23-audio-" + Guid.NewGuid().ToString("N"));
            FirstLevelEntry.SaveDirectoryOverride = directory;
            scene = EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/FirstLevel.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return Wait(() => (entry = Object.FindAnyObjectByType<FirstLevelEntry>()) != null &&
                entry.Flow != null && entry.Flow.Phase == FirstLevelPhase.Idle, "first-level entrance");
            manager = AudioManager.Instance;
            Assert.That(manager, Is.Not.Null);
            Assert.That(Object.FindObjectsByType<AudioListener>().Count(l => l.isActiveAndEnabled), Is.EqualTo(1));
        }

        [UnityTearDown]
        public IEnumerator CleanOwnedObjectsOnly()
        {
            if (capture != null) capture.Finish();
            if (scene.IsValid() && scene.isLoaded)
            {
                SceneManager.CreateScene("T23 cleanup");
                yield return SceneManager.UnloadSceneAsync(scene);
            }
            if (manager != null) Object.Destroy(manager.gameObject);
            if (cleanupListener != null) Object.Destroy(cleanupListener);
            yield return null;
            Assert.That(AudioManager.Instance, Is.Null);
            FirstLevelEntry.SaveDirectoryOverride = previousDirectory;
            if (directory != null && Directory.Exists(directory)) Directory.Delete(directory, true);
        }

        [UnityTest]
        public IEnumerator RequiredClipsDecodeToFiniteNonSilentSamples()
        {
            foreach (string key in new[] { "chirp_01", "chirp_02", "chirp_03", "chirp_04", "chirp_05",
                "flutter", "nest_home", "win_fanfare", "ui_tap", "ui_back" })
            {
                var clip = Resources.Load<AudioClip>(key);
                Assert.That(clip, Is.Not.Null, key);
                Assert.That(clip.LoadAudioData(), Is.True, key);
                yield return Wait(() => clip.loadState != AudioDataLoadState.Loading, key + " decode");
                Assert.That(clip.loadState, Is.EqualTo(AudioDataLoadState.Loaded), key);
                var samples = new float[clip.samples * clip.channels];
                Assert.That(clip.GetData(samples, 0), Is.True, key);
                Assert.That(samples.All(v => !float.IsNaN(v) && !float.IsInfinity(v)), Is.True, key);
                Assert.That(samples.Max(v => Mathf.Abs(v)), Is.GreaterThan(.001f), key);
                Debug.Log("T23 decoded " + key + " samples=" + clip.samples + " channels=" + clip.channels + " hz=" + clip.frequency);
            }
            Assert.That(manager.CurrentMusic, Is.EqualTo(MusicPage.Game));
            Assert.That(Songs.Single(s => s.clip != null && s.clip.name == "bgm_game").loop, Is.True);
            Assert.That(manager.GetComponents<AudioSource>().Length, Is.EqualTo(10));
        }

        [UnityTest]
        public IEnumerator HudClickAndTwoMoveEventsReachTheAudioPool()
        {
            // This assertion reproduces the missing HUD audio before the one-line fix.
            Click("Pause");
            Assert.That(Voices.Any(s => s.clip != null && s.clip.name == "ui_tap"), Is.True,
                "The real first-level HUD must dispatch its existing button sound.");
            yield return Wait(() => FindButton("Continue").IsInteractable(), "pause panel");
            Click("Continue");
            yield return Wait(() => entry.Flow.IsInteractive, "continue");
            capture = manager.gameObject.AddComponent<EngineAudioSample>();
            capture.Begin();
            yield return new WaitForSecondsRealtime(.7f);
            if (capture.Samples.Count > 0)
                Assert.That(capture.Samples.Max(v => Mathf.Abs(v)), Is.GreaterThan(.0001f), "BGM-only engine output");
            Click("Mute");
            Assert.That(entry.AudioMuted, Is.True);
            Assert.That(Voices.All(s => s.clip == null && s.mute), Is.True);
            Assert.That(Songs.All(s => s.mute), Is.True);
            Assert.That(entry.Flow.Pick(1), Is.True);
            Assert.That(Voices.All(s => s.clip == null), Is.True, "muted bird selection");
            entry.Flow.CancelSelection();
            yield return new WaitForSecondsRealtime(.05f);
            int mutedStart = capture.Samples.Count;
            yield return new WaitForSecondsRealtime(.15f);
            if (capture.Samples.Count > mutedStart)
                Assert.That(capture.Samples.Skip(mutedStart).Max(v => Mathf.Abs(v)), Is.LessThan(.000001f), "muted engine output");
            Click("Unmute");
            Assert.That(entry.AudioMuted, Is.False);
            Assert.That(Voices.Any(s => s.clip != null && s.clip.name == "ui_tap"), Is.True);
            Assert.That(Songs.All(s => !s.mute), Is.True);
            Assert.That(entry.Flow.Pick(1), Is.True);
            Assert.That(Voices.Any(s => s.clip != null && s.clip.name.StartsWith("chirp_")), Is.True);
            Assert.That(entry.Flow.Pick(0), Is.True);
            Assert.That(Voices.Any(s => s.clip != null && s.clip.name == "flutter"), Is.True);
            var observed = new HashSet<string>();
            yield return ObserveUntil(() => entry.Flow.IsInteractive, observed);
            Assert.That(observed.Any(s => s.StartsWith("chirp_")), Is.True, "landing chirp");
            Assert.That(observed.Contains("nest_home"), Is.True, "first home");
            Assert.That(entry.Flow.Pick(1), Is.True);
            Assert.That(entry.Flow.Pick(2), Is.True);
            yield return ObserveUntil(() => entry.Flow.Phase == FirstLevelPhase.Won, observed);
            Assert.That(observed.Contains("win_fanfare"), Is.True, "one victory");
            yield return new WaitForSecondsRealtime(2.1f);
            capture.Finish();
            Assert.That(entry.Flow.Current.MoveCount, Is.EqualTo(2));
            Assert.That(entry.Flow.Current.HomedSpecies.Count, Is.EqualTo(2));
            Assert.That(manager.GetComponents<AudioSource>().Length, Is.EqualTo(10));
            if (capture.Samples.Count > 0)
                Assert.That(capture.Samples.Max(v => Mathf.Abs(v)), Is.GreaterThan(.0001f), "internal engine output");
            Debug.Log("T23 events=" + string.Join(",", observed.OrderBy(x => x)) + " outputSamples=" + capture.Samples.Count);
        }

        [UnityTest]
        public IEnumerator FailedSaveKeepsSessionMuteAndUnloadCancelsStaleAudio()
        {
            var store = new FirstLevelPersistence(directory, new DenyWrite());
            store.Load();
            typeof(FirstLevelEntry).GetField("persistence", Private).SetValue(entry, store);
            entry.ToggleMute();
            Assert.That(entry.AudioMuted, Is.True);
            Assert.That(manager.SfxEnabled || manager.MusicEnabled, Is.False);
            Assert.That(Voices.All(s => s.mute && s.clip == null), Is.True);
            Assert.That(Songs.All(s => s.mute), Is.True);
            Assert.That(File.Exists(store.SavePath), Is.False);
            Assert.That(entry.Flow.IsInteractive, Is.True, "confirmed failed settings save must not block gameplay");
            entry.Home(); entry.PlayFresh();
            Assert.That(entry.AudioMuted, Is.True, "Home/Play retains the unsaved session preference");
            Assert.That(manager.SfxEnabled || manager.MusicEnabled, Is.False);
            entry.ToggleMute();
            Assert.That(entry.AudioMuted, Is.False);
            Assert.That(manager.SfxEnabled && manager.MusicEnabled, Is.True);
            manager.SendMessage("OnApplicationFocus", false);
            Assert.That(manager.GetComponents<AudioSource>().All(s => s.mute), Is.True);
            manager.SendMessage("OnApplicationPause", true);
            manager.SendMessage("OnApplicationFocus", true);
            Assert.That(manager.GetComponents<AudioSource>().All(s => s.mute), Is.True);
            manager.SendMessage("OnApplicationPause", false);
            Assert.That(manager.GetComponents<AudioSource>().All(s => !s.mute), Is.True);
            Assert.That(entry.Flow.Pick(1), Is.True); Assert.That(entry.Flow.Pick(0), Is.True);
            // Unload while the committed move is presenting, before Home/Win callbacks.
            var cleanup = SceneManager.CreateScene("T23 unload probe");
            cleanupListener = new GameObject("T23 cleanup listener", typeof(AudioListener));
            SceneManager.MoveGameObjectToScene(cleanupListener, cleanup);
            yield return SceneManager.UnloadSceneAsync(scene);
            yield return new WaitForSecondsRealtime(1.5f);
            Assert.That(Voices.All(s => s.clip == null || (s.clip.name != "nest_home" && s.clip.name != "win_fanfare")), Is.True);
            Assert.That(manager.GetComponents<AudioSource>().Length, Is.EqualTo(10));
        }

        IEnumerator ObserveUntil(Func<bool> done, HashSet<string> observed)
        {
            float deadline = Time.realtimeSinceStartup + 7;
            while (!done() && Time.realtimeSinceStartup < deadline)
            {
                foreach (var source in Voices) if (source.clip != null) observed.Add(source.clip.name);
                yield return null;
            }
            foreach (var source in Voices) if (source.clip != null) observed.Add(source.clip.name);
            Assert.That(done(), Is.True, "event presentation timeout");
        }
        static IEnumerator Wait(Func<bool> ready, string message)
        {
            float deadline = Time.realtimeSinceStartup + 8;
            while (!ready() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(ready(), Is.True, message);
        }
        Button FindButton(string name) => entry.Hud.GetComponentsInChildren<Button>(true).Single(b => b.name == name && b.IsActive());
        void Click(string name)
        {
            var button = FindButton(name); Assert.That(button.IsActive() && button.IsInteractable(), Is.True, name);
            var data = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left };
            ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerClickHandler);
        }
        sealed class DenyWrite : IFirstLevelFileOps
        {
            public bool Exists(string p) => File.Exists(p);
            public byte[] Read(string p) => File.ReadAllBytes(p);
            public void CreateDirectory(string p) => throw new IOException("T23 injected full disk");
            public void WriteDurable(string p, byte[] b) => throw new IOException("T23 injected full disk");
            public void Move(string a, string b) => throw new IOException("T23 injected full disk");
            public void Replace(string a, string b, string c) => throw new IOException("T23 injected full disk");
        }
    }

    // Engine audio specialist sample, NOT a live Player recording or microphone capture.
    public sealed class EngineAudioSample : MonoBehaviour
    {
        public readonly List<float> Samples = new List<float>();
        bool recording;
        int channels, frequency;
        public void Begin()
        {
            var config = AudioSettings.GetConfiguration();
            frequency = config.sampleRate;
            switch (config.speakerMode)
            {
                case AudioSpeakerMode.Mono: channels = 1; break;
                case AudioSpeakerMode.Stereo: channels = 2; break;
                case AudioSpeakerMode.Quad: channels = 4; break;
                case AudioSpeakerMode.Surround: channels = 5; break;
                case AudioSpeakerMode.Mode5point1: channels = 6; break;
                case AudioSpeakerMode.Mode7point1: channels = 8; break;
                default: channels = 0; break;
            }
            if (frequency <= 0 || channels == 0) { Debug.Log("T23 internal render unavailable: audio configuration"); return; }
            recording = AudioRenderer.Start();
            Debug.Log("T23 AudioRenderer.Start=" + recording + " hz=" + frequency + " channels=" + channels);
        }
        void LateUpdate()
        {
            if (!recording) return;
            int frames = AudioRenderer.GetSampleCountForCaptureFrame();
            using (var buffer = new NativeArray<float>(frames * channels, Allocator.Temp))
            {
                if (!AudioRenderer.Render(buffer)) throw new InvalidOperationException("T23 engine render failed");
                foreach (float sample in buffer) Samples.Add(sample);
            }
        }
        public void Finish()
        {
            if (!recording) return;
            recording = false; AudioRenderer.Stop();
            if (Samples.Count == 0 || !Samples.Any(v => Mathf.Abs(v) > .0001f))
            { Debug.Log("T23 no non-silent internal sample; physical output remains unverified"); return; }
            string folder = Path.Combine(Path.GetTempPath(), "t23-audio-evidence"); Directory.CreateDirectory(folder);
            using (var writer = new BinaryWriter(File.Create(Path.Combine(folder, "ENGINE-SAMPLE-NOT-PLAYER.wav"))))
            {
                int bytes = Samples.Count * 2;
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + bytes);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16);
                writer.Write((short)1); writer.Write((short)channels); writer.Write(frequency);
                writer.Write(frequency * channels * 2); writer.Write((short)(channels * 2)); writer.Write((short)16);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(bytes);
                foreach (float sample in Samples) writer.Write((short)(Mathf.Clamp(sample, -1, 1) * 32767));
            }
        }
        void OnDestroy() => Finish();
    }
}
#endif
