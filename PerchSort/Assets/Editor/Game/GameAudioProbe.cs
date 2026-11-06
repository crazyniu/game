using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using PerchSort.Audio;
using PerchSort.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AudioManager = PerchSort.Audio.AudioManager;
using Object = UnityEngine.Object;

namespace PerchSort.Editor
{
    // Batch-only evidence runner. The launcher isolates PlayerPrefs before Unity starts.
    [InitializeOnLoad]
    public static class GameAudioProbe
    {
        private const string ActiveKey = "PerchSort.SoundProbe.Active";
        private const string FailedKey = "PerchSort.SoundProbe.Failed";
        private const string VolumeKey = "PerchSort.SoundProbe.ListenerVolume";
        private const string ProfileKey = "PerchSort.SoundProbe.Profile";
        private static string LogPath => Path.GetFullPath(Path.Combine(Application.dataPath, "../../build/sound-probe/probe.log"));
        private static GameRoot root;
        private static AudioManager audio;
        private static int stage;
        private static double next;
        private static float playhead;
        private static string previewProfile;

        static GameAudioProbe() => EditorApplication.playModeStateChanged += ModeChanged;

        public static void Run()
        {
            if (!Application.isBatchMode || PlayerSettings.companyName != "PerchSortSoundProbe")
                throw new InvalidOperationException("Run through scripts/probe_sound.sh in batch mode with isolated PlayerPrefs.");
            SessionState.SetBool(ActiveKey, true);
            SessionState.SetBool(FailedKey, false);
            SessionState.SetFloat(VolumeKey, AudioListener.volume);
            Log($"preflight unity={Application.unityVersion} editorMasterMute={EditorUtility.audioMasterMute} " +
                $"listenerVolume={AudioListener.volume:F3} listenerPause={AudioListener.pause} savedMute={PlayerPrefs.GetInt(ProgressStore.MutedKey, 0)}");
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/AudioManager.asset")[0]);
            Log($"project disableAudio={settings.FindProperty("m_DisableAudio").boolValue} masterVolume={settings.FindProperty("m_Volume").floatValue:F3}");
            EditorSceneManager.OpenScene(GameSceneBuilder.ScenePath, OpenSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {
            if (!SessionState.GetBool(ActiveKey, false)) return;
            AudioListener.volume = 0;
            SceneManager.sceneLoaded += SceneLoaded;
        }

        private static void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= SceneLoaded;
            root = Object.FindAnyObjectByType<GameRoot>();
            // Real saved-profile initialization; only notification popups are suppressed.
            root.Initialize(true, false);
            audio = AudioManager.Instance;
            audio.EffectPlayed += effect => Log($"effect event={effect}");
            Log($"scene path={scene.path} manager={audio.name} sources={audio.GetComponents<AudioSource>().Length} " +
                $"profileMuted={root.Muted} preview={root.IsBoardPreview} instantMotion={GameMotion.Instant}");
        }

        private static void ModeChanged(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(ActiveKey, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                stage = 0; next = EditorApplication.timeSinceStartup + 1.4;
                EditorApplication.update += Tick;
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.update -= Tick;
                bool failed = SessionState.GetBool(FailedKey, false);
                if (!failed)
                {
                    bool unchanged = ProfileFingerprint() == SessionState.GetString(ProfileKey, "");
                    Log($"exit-while-preview-muted persistedProfileUnchanged={unchanged} savedMute={PlayerPrefs.GetInt(ProgressStore.MutedKey, 0)}");
                    failed = !unchanged;
                }
                AudioListener.volume = SessionState.GetFloat(VolumeKey, 1);
                Log($"finish result={(failed ? "FAIL" : "PASS")} listenerVolumeRestored={AudioListener.volume:F3} editorMasterMute={EditorUtility.audioMasterMute}");
                SessionState.EraseBool(ActiveKey);
                EditorApplication.Exit(failed ? 1 : 0);
            }
        }

        private static void Tick()
        {
            if (EditorApplication.timeSinceStartup < next) return;
            next = EditorApplication.timeSinceStartup + .25;
            try
            {
                switch (stage++)
                {
                    case 0:
                        Snapshot("real-map-saved-setting");
                        Require(root.IsMap && !root.IsBoardPreview, "real map");
                        Require(root.Muted == ProgressStore.Load().Muted, "saved mute loaded");
                        foreach (string key in AudioManager.SoundKeys.Concat(new[] { "bgm_menu", "bgm_game", "Audio/error", "Audio/undo" }))
                        {
                            var clip = Resources.Load<AudioClip>(key);
                            Require(clip != null, "resource " + key);
                            Log($"resource key={key} clip={clip.name} seconds={clip.length:F3} loadState={clip.loadState}");
                        }
                        // Batch has no Game view focus. Exercise the documented foreground state.
                        audio.SendMessage("OnApplicationPause", false);
                        audio.SendMessage("OnApplicationFocus", true);
                        Log("probe foreground callbacks pause=False focus=True; listener remains silenced");
                        SetFromSettings(false);
                        next = EditorApplication.timeSinceStartup + 1.3;
                        break;
                    case 1:
                        Snapshot("real-map-unmuted");
                        RequirePlaying("bgm_menu");
                        playhead = Song().time;
                        root.PlaySound(Sfx.Click);
                        next = EditorApplication.timeSinceStartup + .04;
                        break;
                    case 2:
                        Snapshot("real-map-click");
                        Require(audio.GetComponents<AudioSource>().Any(s => !s.loop && s.isPlaying && !s.mute), "map effect playing");
                        next = EditorApplication.timeSinceStartup + .35;
                        break;
                    case 3:
                        Require(Song().time > playhead, "map playhead advances");
                        Snapshot("real-map-advancing");
                        previewProfile = ProfileFingerprint();
                        new PopupFakeData(root).Apply("Settings / Muted");
                        Require(root.Muted && ProfileFingerprint() == previewProfile, "muted preview never saves");
                        Snapshot("preview-settings-muted");
                        root.EndBoardPreview();
                        Require(!root.Muted && ProfileFingerprint() == previewProfile, "preview restores real mute");
                        Log("preview-return profileUnchanged=True realMute=False");
                        root.LoadLevel(1);
                        next = EditorApplication.timeSinceStartup + 1.3;
                        break;
                    case 4:
                        Require(!root.IsMap && !root.IsBoardPreview && root.LevelId == 1, "real first board");
                        Snapshot("real-first-board");
                        RequirePlaying("bgm_game");
                        playhead = Song().time;
                        root.PlaySound(Sfx.Select);
                        next = EditorApplication.timeSinceStartup + .04;
                        break;
                    case 5:
                        Snapshot("real-first-board-select");
                        Require(audio.GetComponents<AudioSource>().Any(s => !s.loop && s.isPlaying && !s.mute), "board effect playing");
                        next = EditorApplication.timeSinceStartup + .35;
                        break;
                    case 6:
                        Require(Song().time > playhead, "board playhead advances");
                        SetFromSettings(true);
                        Snapshot("real-board-settings-muted");
                        Require(audio.GetComponents<AudioSource>().All(s => s.mute), "Settings mutes every source");
                        Require(!audio.SfxEnabled && !audio.MusicEnabled, "Settings disables music and effects");
                        break;
                    case 7:
                        SetFromSettings(false);
                        Snapshot("real-board-settings-unmuted");
                        RequirePlaying("bgm_game");
                        int savedMute = PlayerPrefs.GetInt(ProgressStore.MutedKey);
                        PlayerPrefs.DeleteKey(ProgressStore.MutedKey);
                        bool defaultMuted = ProgressStore.Load().Muted;
                        PlayerPrefs.SetInt(ProgressStore.MutedKey, savedMute);
                        PlayerPrefs.Save();
                        Log($"missing-mute-key defaultMuted={defaultMuted} existingMuteRestored={savedMute}");
                        Require(!defaultMuted, "missing-key mute default OFF");
                        SessionState.SetString(ProfileKey, ProfileFingerprint());
                        new PopupFakeData(root).Apply("Settings / Muted");
                        Snapshot("preview-muted-before-exit");
                        EditorApplication.update -= Tick;
                        EditorApplication.ExitPlaymode();
                        break;
                }
            }
            catch (Exception error)
            {
                Log("FAIL " + error);
                SessionState.SetBool(FailedKey, true);
                EditorApplication.update -= Tick;
                EditorApplication.ExitPlaymode();
            }
        }

        private static void SetFromSettings(bool muted)
        {
            root.OpenSettings();
            var button = root.GetComponentsInChildren<Button>().Single(b => b.name == "Toggle mute");
            if (root.Muted != muted) button.onClick.Invoke();
            Require(root.Muted == muted && ProgressStore.Load().Muted == muted, "Settings persists selected mute");
            Log($"settings-disc requestedMute={muted} icon={button.transform.Find("Icon").GetComponent<Image>().sprite.name}");
            root.CloseSettings();
        }

        private static AudioSource Song() => audio.GetComponents<AudioSource>().Single(s => s.loop && s.clip != null && s.clip.name == (root.IsMap ? "bgm_menu" : "bgm_game"));
        private static void RequirePlaying(string clip)
        {
            var source = Song();
            Require(source.clip.name == clip && source.isPlaying && !source.mute && source.volume > .19f, clip + " playing at normal gain");
        }

        private static void Snapshot(string phase)
        {
            var listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            Require(listeners.Count(l => l.isActiveAndEnabled) == 1, "one enabled listener");
            Require(AudioListener.volume == 0, "probe speakers silenced");
            Log($"t={Time.realtimeSinceStartup:F3} phase={phase} page={(root.IsMap ? "map" : "board")} level={root.LevelId} " +
                $"preview={root.IsBoardPreview} muted={root.Muted} savedMute={PlayerPrefs.GetInt(ProgressStore.MutedKey, 0)} " +
                $"sfxEnabled={audio.SfxEnabled} musicEnabled={audio.MusicEnabled} musicPage={audio.CurrentMusic} " +
                $"listeners={listeners.Length} listenerEnabled={listeners[0].isActiveAndEnabled} listenerPause={AudioListener.pause} " +
                $"listenerVolume={AudioListener.volume:F3} editorMasterMute={EditorUtility.audioMasterMute} applicationFocused={Application.isFocused}");
            int index = 0;
            foreach (var source in audio.GetComponents<AudioSource>())
                Log($"source={index++} clip={(source.clip == null ? "none" : source.clip.name)} isPlaying={source.isPlaying} " +
                    $"volume={source.volume:F3} mute={source.mute} loop={source.loop} enabled={source.enabled} " +
                    $"time={(source.clip == null ? 0 : source.time):F3} spatialBlend={source.spatialBlend:F1} " +
                    $"mixer={(source.outputAudioMixerGroup == null ? "direct" : source.outputAudioMixerGroup.name)}");
        }

        private static string ProfileFingerprint()
        {
            string value = string.Join("\n", ProgressStore.Keys().Select(key => key + "=" + (PlayerPrefs.HasKey(key)
                ? key == ProgressStore.NotificationsKey ? PlayerPrefs.GetString(key) : PlayerPrefs.GetInt(key).ToString()
                : "<absent>")));
            using (var hash = SHA256.Create()) return Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(value)));
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static void Log(string message)
        {
            File.AppendAllText(LogPath, "[SoundProbe] " + message + "\n");
            Debug.Log("[SoundProbe] " + message);
        }
    }
}
