using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using PerchSort.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PerchSort.Editor
{
    [InitializeOnLoad]
    public static class UiStateCapture
    {
        private const string RunningKey = "PerchSort.UiStateCapture.Running";
        private const int Width = 1080, Height = 1920, SettleFrames = 4;
        private sealed class CaptureState
        {
            public string Screen, State, File;
            public IScreenStates Provider;
            public Action Apply;
        }
        private static readonly List<CaptureState> captures = new List<CaptureState>();
        private static readonly HashSet<string> paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static GameRoot root;
        private static RenderTexture target;
        private static IScreenStates activeProvider;
        private static CaptureState current;
        private static bool discovered;
        private static int cursor, appliedFrame = -1, written, failures;
        private static double deadline;
        private static string DirectoryPath => Path.GetFullPath(Path.Combine(Application.dataPath, "../../build/screens/states"));
        private static bool Running => SessionState.GetBool(RunningKey, false);

        static UiStateCapture()
        {
            // SessionState survives the domain reload when Play mode starts.
            if (Running) Attach();
        }

        public static void Run()
        {
            Directory.CreateDirectory(DirectoryPath);
            File.WriteAllText(Path.Combine(DirectoryPath, "index.txt"),
                "# UI state captures, 1080x1920\n# status\tfile\tscreen\tstate\tdetail\n");
            SessionState.SetBool(RunningKey, true);
            Attach();
            try
            {
                if (!Application.isBatchMode || EditorApplication.isPlaying)
                    throw new InvalidOperationException("Run UiStateCapture in an Editor batch process, starting in edit mode.");
                if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
                    throw new InvalidOperationException("UI state capture needs graphics. Remove -nographics.");
                Filter();
                EditorSceneManager.OpenScene(GameSceneBuilder.ScenePath, OpenSceneMode.Single);
                Silence();
                EditorApplication.EnterPlaymode();
            }
            catch (Exception error) { Failure("Startup", "Run", error); Finish(); }
        }

        private static void Attach()
        {
            Silence();
            deadline = EditorApplication.timeSinceStartup + 120;
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            Application.logMessageReceived -= RuntimeLog;
            Application.logMessageReceived += RuntimeLog;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {
            if (!Running) return;
            Silence();
            GameMotion.Instant = true;
            Debug.Log("UiStateCapture: audio silenced before scene Awake; listener volume=0, paused=true.");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AfterSceneLoad()
        {
            if (!Running) return;
            try
            {
                Silence();
                GameMotion.Instant = true;
                Application.runInBackground = true;
                target = new RenderTexture(Width, Height, 24);
                target.Create();
                Camera.main.targetTexture = target;
                root = Object.FindAnyObjectByType<GameRoot>();
                // Initialize before Start so the real map/board never read or write PlayerPrefs.
                root.Initialize(false, false);
                Silence();
                Debug.Log("UiStateCapture: GameRoot initialized in Play mode with a nonpersistent profile.");
            }
            catch (Exception error) { Failure("Startup", "GameRoot", error); Finish(); }
        }

        private static int Silence()
        {
            AudioListener.volume = 0;
            AudioListener.pause = true;
            var sources = Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var source in sources)
            {
                source.ignoreListenerPause = false;
                source.ignoreListenerVolume = false;
                source.mute = true;
            }
            return sources.Length;
        }

        private static string Filter()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(arguments, "-states");
            if (index < 0) return "";
            if (index + 1 == arguments.Length || arguments[index + 1].StartsWith("-", StringComparison.Ordinal))
                throw new ArgumentException("-states requires a filter substring.");
            return arguments[index + 1];
        }

        private static void Discover()
        {
            // Keep these discovery rules identical to UiStateSwitcherWindow.Discover.
            var providers = new List<IScreenStates> { root.BoardStates };
            foreach (Type type in TypeCache.GetTypesDerivedFrom<IScreenStates>())
            {
                if (type == typeof(BoardFakeData) || !type.IsPublic || !type.IsSealed || type.IsAbstract) continue;
                var constructor = type.GetConstructor(new[] { typeof(GameRoot) });
                if (constructor == null) continue;
                try { providers.Add((IScreenStates)constructor.Invoke(new object[] { root })); }
                catch (Exception error)
                {
                    Failure(type.Name, "Discovery", error is TargetInvocationException invocation ? invocation.InnerException ?? error : error);
                }
            }
            providers.Sort((left, right) => StringComparer.Ordinal.Compare(left.ScreenName, right.ScreenName));
            string filter = Filter();
            foreach (var provider in providers)
            {
                Index("PROVIDER", "-", provider.ScreenName, "-", provider.StateNames.Count + " states");
                foreach (string state in provider.StateNames)
                    Add(provider.ScreenName, state, () => provider.Apply(state), provider, filter);
            }
            Add("Real game", "Map", () => { root.EndBoardPreview(); root.OpenMap(); }, null, filter);
            Add("Real game", "First board", () =>
            {
                root.EndBoardPreview(); root.LoadLevel(1);
                if (root.IsMap || root.IsBoardPreview || root.LevelId != 1)
                    throw new InvalidOperationException("Return to real game did not load the first board.");
            }, null, filter);
            if (captures.Count == 0) Failure("Selection", filter, new InvalidOperationException("No states match the filter."));
            discovered = true;
            Debug.Log("UiStateCapture: selected " + captures.Count + " states; filter='" + filter + "'.");
        }

        private static void Add(string screen, string state, Action apply, IScreenStates provider, string filter)
        {
            if (screen.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0 &&
                state.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0) return;
            string file = SafeName(screen) + "/" + SafeName(state) + ".png";
            if (!paths.Add(file))
            {
                Failure(screen, state, new InvalidOperationException("Duplicate capture filename: " + file));
                return;
            }
            captures.Add(new CaptureState { Screen = screen, State = state, File = file, Provider = provider, Apply = apply });
        }

        private static string SafeName(string name) => Regex.Replace(name, "[^A-Za-z0-9_-]+", "_").Trim('_');

        private static void Tick()
        {
            Silence();
            GameMotion.Instant = true;
            EditorApplication.QueuePlayerLoopUpdate();
            try
            {
                if (EditorApplication.timeSinceStartup > deadline)
                    throw new TimeoutException("Timed out waiting for Play mode, GameRoot, or layout frames.");
                if (!EditorApplication.isPlaying || root == null || !root.IsReady) return;
                if (!discovered) Discover();
                if (cursor == captures.Count) { Finish(); return; }
                current = captures[cursor];
                if (appliedFrame < 0)
                {
                    if (activeProvider != current.Provider) root.EndBoardPreview();
                    activeProvider = current.Provider;
                    current.Apply();
                    Silence();
                    GameMotion.Instant = true;
                    SettleLayout();
                    appliedFrame = Time.frameCount;
                    deadline = EditorApplication.timeSinceStartup + 60;
                    return;
                }
                if (Time.frameCount - appliedFrame < SettleFrames) return;
                Capture(current);
                cursor++; appliedFrame = -1; current = null;
            }
            catch (Exception error)
            {
                Failure(current?.Screen ?? "Startup", current?.State ?? "Ready", error);
                if (current == null || error is TimeoutException) { Finish(); return; }
                cursor++; appliedFrame = -1; current = null;
            }
        }

        private static void SettleLayout()
        {
            Canvas.ForceUpdateCanvases();
            root.Canvas.GetComponent<PerchSort.Game.SafeArea>().Apply();
            foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
                if (canvas.isRootCanvas) LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)canvas.transform);
            Canvas.ForceUpdateCanvases();
        }

        private static void Capture(CaptureState state)
        {
            var cameras = Camera.allCameras.Where(camera => camera.targetTexture == null || camera.targetTexture == target)
                .OrderBy(camera => camera.depth).ToArray();
            var previousTargets = cameras.Select(camera => camera.targetTexture).ToArray();
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            var overlays = canvases.Where(canvas => canvas.isRootCanvas && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                .OrderBy(canvas => canvas.sortingOrder).ToArray();
            var overlaySettings = overlays.Select(canvas => (canvas.worldCamera, canvas.planeDistance)).ToArray();
            var sorting = canvases.Select(canvas => (canvas.sortingLayerID, canvas.sortingOrder)).ToArray();
            var previousActive = RenderTexture.active;
            var image = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            try
            {
                foreach (var camera in cameras) camera.targetTexture = target;
                int topOrder = canvases.Max(canvas => canvas.sortingOrder);
                int topLayer = canvases.OrderBy(canvas => SortingLayer.GetLayerValueFromID(canvas.sortingLayerID)).Last().sortingLayerID;
                foreach (var overlay in overlays)
                {
                    var group = canvases.Where(canvas => canvas.transform.IsChildOf(overlay.transform)).ToArray();
                    int offset = topOrder + 1 - group.Min(canvas => canvas.sortingOrder);
                    overlay.renderMode = RenderMode.ScreenSpaceCamera;
                    overlay.worldCamera = cameras[cameras.Length - 1];
                    overlay.planeDistance = overlay.worldCamera.nearClipPlane + 1;
                    foreach (var canvas in group) { canvas.sortingLayerID = topLayer; canvas.sortingOrder += offset; }
                    topOrder = group.Max(canvas => canvas.sortingOrder);
                }
                SettleLayout();
                // Manual rendering includes camera canvases and the temporarily attached overlays in batch mode.
                foreach (var camera in cameras) camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                image.Apply();
                string path = Path.Combine(DirectoryPath, state.File);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, image.EncodeToPNG());
                int sources = Silence();
                written++;
                Index("FILE", state.File, state.Screen, state.State,
                    "frames=" + (Time.frameCount - appliedFrame) + "; canvases=" + canvases.Length + "; overlays=" + overlays.Length +
                    "; mutedSources=" + sources + "; listenerVolume=0; listenerPaused=true");
                Debug.Log("UiStateCapture: " + written + "/" + captures.Count + " " + state.File);
            }
            finally
            {
                for (int i = 0; i < overlays.Length; i++)
                {
                    overlays[i].renderMode = RenderMode.ScreenSpaceOverlay;
                    overlays[i].worldCamera = overlaySettings[i].worldCamera;
                    overlays[i].planeDistance = overlaySettings[i].planeDistance;
                }
                for (int i = 0; i < canvases.Length; i++)
                { canvases[i].sortingLayerID = sorting[i].sortingLayerID; canvases[i].sortingOrder = sorting[i].sortingOrder; }
                for (int i = 0; i < cameras.Length; i++) cameras[i].targetTexture = previousTargets[i];
                RenderTexture.active = previousActive;
                Object.DestroyImmediate(image);
            }
        }

        private static void RuntimeLog(string message, string stack, LogType type)
        {
            if (type != LogType.Error && type != LogType.Assert && type != LogType.Exception) return;
            // Background Editor services are diagnostics, not exceptions thrown by the selected state.
            if (!stack.Contains("PerchSort."))
            {
                Index("UNITY_ERROR", "-", "Editor", "Diagnostic", message + " " + stack);
                return;
            }
            failures++;
            Index("ERROR", current?.File ?? "-", current?.Screen ?? "Startup", current?.State ?? "Runtime", message + " " + stack);
        }

        private static void Failure(string screen, string state, Exception error)
        {
            failures++;
            Index("ERROR", current?.File ?? "-", screen, state, error.ToString());
            Debug.Log("UiStateCapture failed: " + screen + " / " + state + ": " + error.Message);
        }

        private static void Index(params string[] fields) => File.AppendAllText(Path.Combine(DirectoryPath, "index.txt"),
            string.Join("\t", fields.Select(field => field.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' '))) + "\n");

        private static void Finish()
        {
            EditorApplication.update -= Tick;
            try { if (root != null && root.IsReady) root.EndBoardPreview(); }
            catch (Exception error) { Failure("Cleanup", "Return to real game", error); }
            Silence();
            SessionState.SetBool(RunningKey, false);
            Application.logMessageReceived -= RuntimeLog;
            if (target != null)
            {
                if (root != null && root.RenderCamera != null) root.RenderCamera.targetTexture = null;
                target.Release(); Object.DestroyImmediate(target);
            }
            Index("SUMMARY", "captured=" + written, "selected=" + captures.Count, "errors=" + failures);
            Debug.Log("UiStateCapture finished: " + written + " captures, " + failures + " errors. " + DirectoryPath);
            EditorApplication.Exit(failures == 0 ? 0 : 1);
        }
    }
}
