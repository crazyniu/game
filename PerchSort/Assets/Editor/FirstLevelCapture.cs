using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using PerchSort.Core.FirstLevel;
using PerchSort.FirstLevelRuntime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class FirstLevelCapture
{
    private const string Prefix = "PerchSort.FirstLevelCapture.";
    private const string ScenePath = "Assets/Scenes/FirstLevel.unity";
    private const int Width = 540, Height = 960;
    private const double Interval = 1.0 / 24.0;
    private static FirstLevelEntry entry;
    private static RenderTexture target;
    private static Texture2D pixels;
    private static CaptureDocument document;
    private static double started, lastSample, nextAction;
    private static int stage, lastUnityFrame = -1;
    private static string initialLayout, fatalLog;
    private static bool Running => SessionState.GetBool(Prefix + "Running", false);
    private static string Output => SessionState.GetString(Prefix + "Output", "");

    static FirstLevelCapture()
    {
        EditorApplication.playModeStateChanged += ModeChanged;
        EditorApplication.update += Update;
        EditorApplication.delayCall += FinishEditorExit;
    }

    [MenuItem("Perch Sort/First Level/Capture Runtime Review")]
    public static void Run()
    {
        if (Running || EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Capture requires an idle disposable Editor.");
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            throw new InvalidOperationException("Actual runtime capture needs graphics; omit -nographics.");
        if (!File.Exists(ScenePath))
            throw new FileNotFoundException("Build and review FirstLevel.unity before capture.", ScenePath);

        string output = Argument("-firstLevelCaptureOutput") ??
            Path.GetFullPath(Path.Combine(Application.dataPath, "../../build/firstlevel-capture",
                DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture)));
        output = Path.GetFullPath(output);
        if (Directory.Exists(output) && Directory.EnumerateFileSystemEntries(output).Any())
            throw new InvalidOperationException("Capture output must be new or empty: " + output);
        Directory.CreateDirectory(output);
        SessionState.SetString(Prefix + "Output", output);
        SessionState.SetBool(Prefix + "Quit", Environment.GetCommandLineArgs().Contains("-firstLevelCaptureQuit"));
        SessionState.SetBool(Prefix + "Finishing", false);
        SessionState.SetBool(Prefix + "Running", true);
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        EditorApplication.isPlaying = true;
    }

    private static string Argument(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        int i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }

    private static void ModeChanged(PlayModeStateChange mode)
    {
        if (mode == PlayModeStateChange.EnteredPlayMode && Running)
        {
            started = Time.realtimeSinceStartupAsDouble;
            lastSample = -Interval;
            lastUnityFrame = -1;
            stage = 0;
            entry = null;
            fatalLog = null;
            initialLayout = null;
            document = new CaptureDocument {
                utcStarted = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                unityVersion = Application.unityVersion,
                outputDirectory = Output,
                graphicsDevice = SystemInfo.graphicsDeviceName
            };
            Application.logMessageReceived += LogReceived;
        }
        else if (mode == PlayModeStateChange.ExitingPlayMode && Running)
            Finish(false, "Play mode stopped before the capture route completed.");
        else if (mode == PlayModeStateChange.EnteredEditMode)
            FinishEditorExit();
    }

    private static void Update()
    {
        if (!Running || !EditorApplication.isPlaying || document == null) return;
        try
        {
            double now = Time.realtimeSinceStartupAsDouble - started;
            if (now > 45) throw new TimeoutException("First-level capture exceeded 45 real seconds.");
            if (!string.IsNullOrEmpty(fatalLog)) throw new InvalidOperationException(fatalLog);
            if (!entry) entry = UnityEngine.Object.FindAnyObjectByType<FirstLevelEntry>();
            Camera camera = Camera.main;
            if (camera && now - lastSample >= Interval && Time.frameCount != lastUnityFrame)
            {
                double costStart = EditorApplication.timeSinceStartup;
                string filename = "render_" + document.frames.Count.ToString("D6", CultureInfo.InvariantCulture) + ".png";
                Render(camera, Width, Height, Path.Combine(Output, filename), true);
                document.frames.Add(Sample(camera, now, filename,
                    EditorApplication.timeSinceStartup - costStart));
                if (document.frames.Count <= 4)
                    document.visualSnapshots.Add(DescribeVisuals("first_frames", filename, now));
                lastSample = now;
                lastUnityFrame = Time.frameCount;
                if (document.frames.Count % 24 == 0) WriteMetadata();
            }
            if (!entry || !entry.IsReady || entry.Flow == null) return;
            if (entry.Flow.Phase == FirstLevelPhase.Faulted)
                throw new InvalidOperationException("FirstLevelFlow entered Faulted.");
            Drive(now);
        }
        catch (Exception exception)
        {
            Finish(false, exception.ToString());
        }
    }

    private static void Drive(double now)
    {
        var flow = entry.Flow;
        switch (stage)
        {
            case 0:
                if (flow.Phase != FirstLevelPhase.Idle) return;
                var opening = flow.Current;
                if (opening.FlockSize != 4 || opening.SpeciesCount != 2 || opening.Branches.Count != 3)
                    throw new InvalidOperationException("Capture expects the approved N4/K2/P3 first level.");
                initialLayout = Layout(opening);
                Key("intro_complete", now); nextAction = now + 0.45; stage = 1;
                break;
            case 1:
                if (now < nextAction) return;
                Pick(1, now); nextAction = now + 0.35; stage = 2;
                break;
            case 2:
                if (now < nextAction) return;
                Key("selected_s2", now); Pick(0, now); stage = 3;
                break;
            case 3:
                if (flow.Phase != FirstLevelPhase.Idle) return;
                if (flow.Current.MoveCount != 1 || flow.Current.HomedSpecies.Count != 1 ||
                    flow.Current.HomedSpecies[0] != 0 || !flow.Current.Branches[0].Retired)
                    throw new InvalidOperationException("First Home did not match the approved route.");
                Key("first_home_stable", now); nextAction = now + 0.45; stage = 4;
                break;
            case 4:
                if (now < nextAction) return;
                Pick(2, now); nextAction = now + 0.35; stage = 5;
                break;
            case 5:
                if (now < nextAction) return;
                Key("selected_s3", now); Pick(1, now); stage = 6;
                break;
            case 6:
                if (flow.Phase != FirstLevelPhase.Won) return;
                if (!flow.Current.IsWon || flow.Current.MoveCount != 2)
                    throw new InvalidOperationException("Victory is not the two-move first-level finish.");
                Key("victory_stable", now); nextAction = now + 2.30; stage = 7;
                break;
            case 7:
                if (now < nextAction) return;
                document.actions.Add(new ActionRecord { elapsedSeconds = now, action = "Flow.Restart() / Replay" });
                if (!flow.Restart()) throw new InvalidOperationException("Replay was rejected.");
                stage = 8;
                break;
            case 8:
                if (flow.Phase != FirstLevelPhase.Idle) return;
                if (flow.Current.MoveCount != 0 || flow.Current.HomedSpecies.Count != 0 ||
                    Layout(flow.Current) != initialLayout)
                    throw new InvalidOperationException("Replay did not restore the exact fixed opening.");
                Key("replay_stable", now); nextAction = now + 0.60; stage = 9;
                break;
            case 9:
                if (now >= nextAction) Finish(true, "Intro, two Homes, victory and Replay completed.");
                break;
        }
    }

    private static void Pick(int index, double now)
    {
        document.actions.Add(new ActionRecord { elapsedSeconds = now,
            action = "Flow.Pick(" + index.ToString(CultureInfo.InvariantCulture) + ") / S" + (index + 1) });
        if (!entry.Flow.Pick(index)) throw new InvalidOperationException("Programmatic source/target pick was rejected.");
    }

    private static string Layout(State state) => string.Join("|", state.Branches.Select(branch =>
        branch.Id + ":" + branch.HomeSpecies + ":" +
        string.Join(",", branch.Tokens.Select(token => token.Id + "=" + token.Species))));

    private static FrameRecord Sample(Camera camera, double now, string filename, double cost)
    {
        var flow = entry ? entry.Flow : null;
        var state = flow?.Current;
        return new FrameRecord {
            file = filename, elapsedSeconds = now, utc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            unityFrame = Time.frameCount, unityTime = Time.timeAsDouble, timeScale = Time.timeScale,
            phase = flow == null ? "AwaitingEntry" : flow.Phase.ToString(), stage = stage,
            stateKey = state?.GetStateKey() ?? "", moveCount = state?.MoveCount ?? -1,
            homedCount = state?.HomedSpecies.Count ?? 0, sequence = state?.Sequence ?? -1,
            cameraName = camera.name, cameraPosition = camera.transform.position,
            orthographicSize = camera.orthographicSize, cameraAspect = camera.aspect,
            scene = SceneManager.GetActiveScene().path, encodeAndWriteSeconds = cost
        };
    }

    private static void Key(string label, double now)
    {
        Camera camera = Camera.main;
        if (!camera) throw new InvalidOperationException("Main camera missing at a key stage.");
        string filename = "key_" + label + "_1080x1920.png";
        Render(camera, 1080, 1920, Path.Combine(Output, filename), false);
        document.keyFrames.Add(new ActionRecord { elapsedSeconds = now, action = label, file = filename });
        if (label == "intro_complete")
            document.visualSnapshots.Add(DescribeVisuals("first_stable", filename, now));
    }

    private static void Render(Camera camera, int width, int height, string path, bool reuse)
    {
        RenderTexture texture;
        Texture2D image;
        if (reuse)
        {
            if (!target) { target = new RenderTexture(width, height, 24); target.Create(); }
            if (!pixels) pixels = new Texture2D(width, height, TextureFormat.RGB24, false);
            texture = target; image = pixels;
        }
        else
        {
            texture = new RenderTexture(width, height, 24); texture.Create();
            image = new Texture2D(width, height, TextureFormat.RGB24, false);
        }
        RenderTexture previousActive = RenderTexture.active, previousTarget = camera.targetTexture;
        try
        {
            camera.targetTexture = texture;
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture.active = texture;
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply();
            File.WriteAllBytes(path, image.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = previousTarget; RenderTexture.active = previousActive;
            if (!reuse) { texture.Release(); UnityEngine.Object.DestroyImmediate(texture); UnityEngine.Object.DestroyImmediate(image); }
        }
    }


    private static bool IsKnownEditorSearchStartup(string message, string stack, LogType type)
    {
        if (type != LogType.Exception && type != LogType.Error) return false;
        string text = stack ?? "";
        string normalized = text.Replace('\\', '/');
        string condition = message ?? "";
        return condition.StartsWith("ArgumentOutOfRangeException:", StringComparison.Ordinal) &&
            condition.Contains("Index was out of range.") &&
            condition.Contains("Parameter name: index") &&
            text.Contains("UnityEditor.Search.SearchDatabase") &&
            text.Contains("UnityEditor.Search.SearchInit.IndexationOnStartup") &&
            !normalized.Contains("Assets/Scripts/FirstLevel") &&
            !text.Contains("PerchSort.FirstLevelRuntime") &&
            !text.Contains("PerchSort.Core.FirstLevel");
    }

    private static VisualSnapshot DescribeVisuals(string label, string frame, double now)
    {
        return new VisualSnapshot {
            label = label, frame = frame, elapsedSeconds = now,
            spriteRenderers = UnityEngine.Object.FindObjectsByType<SpriteRenderer>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Select(renderer => DescribeSprite(renderer.gameObject, renderer.sprite,
                    renderer.sharedMaterial, renderer.enabled, "SpriteRenderer"))
                .OrderBy(reference => reference.objectPath).ToArray(),
            uiImages = UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Image>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Select(image => DescribeSprite(image.gameObject, image.sprite,
                    image.canvasRenderer.materialCount > 0 ? image.canvasRenderer.GetMaterial(0) : image.material,
                    image.enabled, "UI.Image"))
                .OrderBy(reference => reference.objectPath).ToArray()
        };
    }

    private static SpriteReference DescribeSprite(GameObject owner, Sprite sprite, Material material,
        bool enabled, string kind)
    {
        var path = new List<string>();
        for (Transform current = owner.transform; current != null; current = current.parent)
            path.Add(current.name);
        path.Reverse();
        Texture2D texture = sprite ? sprite.texture : null;
        return new SpriteReference {
            kind = kind, objectPath = string.Join("/", path), scene = owner.scene.path, enabled = enabled,
            spriteName = sprite ? sprite.name : "", spriteInstanceId = sprite ? sprite.GetInstanceID() : 0,
            spriteAssetPath = sprite ? AssetDatabase.GetAssetPath(sprite) : "",
            textureName = texture ? texture.name : "", textureInstanceId = texture ? texture.GetInstanceID() : 0,
            textureAssetPath = texture ? AssetDatabase.GetAssetPath(texture) : "",
            materialName = material ? material.name : "",
            shaderName = material && material.shader ? material.shader.name : ""
        };
    }

    private static void LogReceived(string message, string stack, LogType type)
    {
        if (!Running || document == null) return;
        bool environment = IsKnownEditorSearchStartup(message, stack, type);
        document.logs.Add(new LogRecord {
            type = type.ToString(), message = message, stack = stack,
            elapsedSeconds = Time.realtimeSinceStartupAsDouble - started,
            classification = environment ? "editor_environment_search_startup" : "runtime_or_other"
        });
        if (environment) document.environmentDiagnosticCount++;
        else if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            fatalLog = message + "\n" + stack;
    }

    private static void Finish(bool success, string message)
    {
        if (!Running) return;
        SessionState.SetBool(Prefix + "Running", false);
        Application.logMessageReceived -= LogReceived;
        if (document != null)
        {
            document.success = success; document.result = message;
            document.elapsedSeconds = Time.realtimeSinceStartupAsDouble - started;
            document.finalStateKey = entry && entry.Flow != null ? entry.Flow.Current.GetStateKey() : "";
            WriteMetadata();
        }
        if (target) { target.Release(); UnityEngine.Object.DestroyImmediate(target); target = null; }
        if (pixels) { UnityEngine.Object.DestroyImmediate(pixels); pixels = null; }
        SessionState.SetInt(Prefix + "ExitCode", success ? 0 : 1);
        SessionState.SetBool(Prefix + "Finishing", true);
        Debug.Log("FirstLevelCapture " + (success ? "PASS: " : "FAIL: ") + message + " Output: " + Output);
        if (EditorApplication.isPlayingOrWillChangePlaymode) EditorApplication.isPlaying = false;
        else FinishEditorExit();
    }

    private static void WriteMetadata() =>
        File.WriteAllText(Path.Combine(Output, "capture.json"), JsonUtility.ToJson(document, true));

    private static void FinishEditorExit()
    {
        if (!SessionState.GetBool(Prefix + "Finishing", false) || EditorApplication.isPlayingOrWillChangePlaymode) return;
        SessionState.SetBool(Prefix + "Finishing", false);
        if (SessionState.GetBool(Prefix + "Quit", false))
            EditorApplication.Exit(SessionState.GetInt(Prefix + "ExitCode", 1));
    }

    [Serializable] private sealed class CaptureDocument
    {
        public string mode = "Automated public Flow API runtime replay; not manual or pointer-input acceptance";
        public string renderSource = "Live FirstLevel.unity Camera.Render with LitMotion/Unity clocks; no CPU scene composition";
        public string stateAuthority = "Core committed state may be ahead of in-progress presentation";
        public string utcStarted, unityVersion, graphicsDevice, outputDirectory, result, finalStateKey;
        public bool success;
        public int environmentDiagnosticCount;
        public string environmentDiagnosticBoundary =
            "Only the exact Editor Search startup ArgumentOutOfRange/index stack is classified as environment; " +
            "all logs remain recorded and runtime/other errors remain fatal. Success is not a clean Editor-console claim.";
        public int width = Width, height = Height, requestedFps = 24;
        public double elapsedSeconds;
        public List<FrameRecord> frames = new List<FrameRecord>();
        public List<ActionRecord> actions = new List<ActionRecord>(), keyFrames = new List<ActionRecord>();
        public List<LogRecord> logs = new List<LogRecord>();
        public List<VisualSnapshot> visualSnapshots = new List<VisualSnapshot>();
    }
    [Serializable] private sealed class FrameRecord
    {
        public string file, utc, phase, stateKey, cameraName, scene;
        public double elapsedSeconds, unityTime, encodeAndWriteSeconds;
        public float timeScale, orthographicSize, cameraAspect;
        public Vector3 cameraPosition;
        public int unityFrame, stage, moveCount, homedCount;
        public long sequence;
    }
    [Serializable] private sealed class ActionRecord { public double elapsedSeconds; public string action, file; }
    [Serializable] private sealed class VisualSnapshot
    {
        public string label, frame;
        public double elapsedSeconds;
        public SpriteReference[] spriteRenderers, uiImages;
    }
    [Serializable] private sealed class SpriteReference
    {
        public string kind, objectPath, scene, spriteName, spriteAssetPath, textureName, textureAssetPath;
        public string materialName, shaderName;
        public int spriteInstanceId, textureInstanceId;
        public bool enabled;
    }
    [Serializable] private sealed class LogRecord
    {
        public string type, message, stack, classification;
        public double elapsedSeconds;
    }
}
