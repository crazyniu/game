using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using System.IO;
using PerchSort.Core;
using PerchSort.Audio;
using PerchSort.View;
using UnityEngine;

namespace PerchSort.UI
{
    public sealed class GameController : MonoBehaviour
    {
        [SerializeField] private BoardView board;
        [SerializeField] private GameHud hud;
        private LevelCatalog catalog;
        private ProgressStore store;
        private SettingsStore settingsStore;
        private AudioManager audioManager;
        private readonly Haptics haptics = new Haptics();
        public BoardView Board => board;
        public GameHud Hud => hud;
        public int LevelId { get; private set; }
        public int LevelCount => catalog.levels.Count;
        public int OptimalMoves => catalog.levels[LevelId - 1].solverMinMoves;
        public TutorialState Tutorial { get; private set; }
        public int TutorialMoveBaseline {get;private set;}
        public bool HintBusy { get; private set; }
        private Task<Move?> hintTask;
        private int hintGeneration;
        public static string SystemLocale => Application.systemLanguage==SystemLanguage.Chinese || Application.systemLanguage==SystemLanguage.ChineseSimplified || Application.systemLanguage==SystemLanguage.ChineseTraditional ? "zh" : "en";
        public const string PrivacyUrl = "https://example.com/perch-sort/privacy"; // T14 replaces this placeholder.
        public static string ReleaseVersion => Application.version;
        public ProgressData Progress { get; private set; }
        public SettingsData Settings { get; private set; }
        public static string ProgressPathOverride { get; set; }
        public static string SettingsPathOverride { get; set; }
        public void Configure(BoardView view, GameHud gameHud) { board = view; hud = gameHud; }

        private void Start()
        {
            catalog = LevelJson.Deserialize(Resources.Load<TextAsset>("Levels/levels").text);
            store = new ProgressStore(ProgressPathOverride ?? Path.Combine(Application.persistentDataPath, "progress.json"));
            settingsStore = new SettingsStore(SettingsPathOverride ?? SettingsStore.DefaultPath);
            Progress = store.Load();
            Settings = settingsStore.Load();
            Progress.language = Strings.Locale = SystemLocale;
            Settings.vibrationEnabled=true;Settings.reducedMotion=false;
            Settings.soundVolume=Settings.musicVolume=1;Settings.musicEnabled=Settings.soundEnabled;
            Tutorial = new TutorialState(Progress);
            audioManager = AudioManager.Instance != null ? AudioManager.Instance : new GameObject("Audio Manager").AddComponent<AudioManager>();
            ApplySettings(Settings);
            SettingsStore.Changed += ApplySettings;
            board.Feedback += PlayFeedback;
            hud.Bind(this);
            board.Changed += hud.Refresh;
            board.Settled += Save;
            board.Undone += OnUndo;
            board.Restarted += OnRestart;
            var session = new GameSession(catalog.levels[Progress.levelId - 1].ToBoardState());
            var history = new List<Move>();
            foreach (var entry in Progress.moves)
            {
                var move = new Move(entry.from, entry.to);
                if (!session.TryMove(move, out _)) throw new InvalidDataException("Saved move does not match the level.");
                history.Add(move);
            }
            LevelId = Progress.levelId;
            board.Initialize(session, history);
            Save();
            Tutorial.Begin(LevelId);
            hud.ShowMenu();
        }
        private void OnDestroy()
        {
            SettingsStore.Changed -= ApplySettings;
            board.Feedback -= PlayFeedback;
            board.Changed -= hud.Refresh;
            board.Settled -= Save;
            board.Undone -= OnUndo;
            board.Restarted -= OnRestart;
        }
        public void ContinueGame() { hud.ShowGame(); board.PlayEntrance(); }
        public void ChooseLevel(int id)
        {
            if (!Progress.IsUnlocked(id)) return;
            LoadLevel(id);
        }
        public void LoadLevel(int id)
        {
            if (board.IsBusy) return;
            hud.Transition(() =>
            {
                hintGeneration++;
                LevelId = id;
                TutorialMoveBaseline=0;Tutorial.Begin(id);
                if (Progress.IsCompleted(id)) { Progress.ForLevel(id).undos = 0; Progress.ForLevel(id).restarts = 0; }
                board.Initialize(new GameSession(catalog.levels[id - 1].ToBoardState()));
                hud.ShowGame();
                board.PlayEntrance();
                Save();
            });
        }
        public void NextLevel()
        {
            if (board.IsBusy || !Rules.IsWon(board.Session.State)) return;
            if (LevelId == LevelCount) hud.ShowLevels();
            else LoadLevel(LevelId + 1);
        }
        private void ApplySettings(SettingsData settings)
        {
            audioManager.SetSfxEnabled(settings.soundEnabled);
            audioManager.SetMusicEnabled(settings.musicEnabled);
            audioManager.SetVolumes(settings.soundVolume, settings.musicVolume);
            ButtonFeedback.ReducedMotion = settings.reducedMotion;

            haptics.Enabled = settings.vibrationEnabled;
            board.ReducedMotion = settings.reducedMotion;
        }
        private void PlayFeedback(Sfx sfx)
        {
            audioManager.Play(sfx);
            if (sfx == Sfx.Collect || sfx == Sfx.Win) haptics.Pulse();
        }
        public void SaveSettings() => settingsStore.Save(Settings);
        public void ResetProgress()
        {
            hintGeneration++;
            Progress = new ProgressData { language = Progress.language };
            Tutorial = new TutorialState(Progress);
            Tutorial.Begin(1);
            store.Save(Progress);
            LevelId = 1;
            board.Initialize(new GameSession(catalog.levels[0].ToBoardState()));
            hud.ShowMenu();
        }
        private void OnUndo() { hintGeneration++; Progress.ForLevel(LevelId).undos++; }
        private void OnRestart(bool replay)
        {
            hintGeneration++;TutorialMoveBaseline=0;
            var record = Progress.ForLevel(LevelId);
            if (replay) { record.undos = 0; record.restarts = 0; }
            else record.restarts++;
        }
        public void SetLanguage(string language)
        {
            Progress.language = Strings.Locale = language;
            store.Save(Progress); hud.Relocalize();
        }
        public void FinishTutorial() { Tutorial.Finish(); store.Save(Progress); hud.RefreshTutorial(); }
        public void ReviewTutorials()
        {
            TutorialMoveBaseline=board.Moves.Count;Progress.tutorialsSeen.Clear(); Tutorial.Begin(LevelId); store.Save(Progress);
        }
        public void RequestHint()
        {
            if (board.IsBusy || HintBusy || hud.Page != UiPage.Game || Rules.IsWon(board.Session.State)) return;
            if (Progress.ForLevel(LevelId).hintsUsed >= HintService.Limit) { hud.HintMessage("hintLimit"); return; }
            // A timed-out search remains bounded by nodes; do not queue competing searches.
            if (hintTask != null && !hintTask.IsCompleted) { hud.HintMessage("hintNone"); return; }
            StartCoroutine(SolveHint());
        }
        private IEnumerator SolveHint()
        {
            HintBusy = true; hud.HintMessage("hintBusy");
            int generation = hintGeneration, level = LevelId;
            string key = board.Session.State.GetStateKey();
            hintTask = HintService.FindAsync(board.Session.State.Clone());
            float deadline = Time.realtimeSinceStartup + HintService.TimeBudget;
            while (!hintTask.IsCompleted && Time.realtimeSinceStartup < deadline) yield return null;
            HintBusy = false;
            if (generation != hintGeneration || level != LevelId || key != board.Session.State.GetStateKey() || hud.Page != UiPage.Game || board.IsBusy)
            { hud.Refresh(); yield break; }
            if (hintTask.IsCompletedSuccessfully && hintTask.Result.HasValue)
            {
                Progress.ForLevel(LevelId).hintsUsed++;
                store.Save(Progress); hud.ShowHint(hintTask.Result.Value);
            }
            else hud.HintMessage("hintNone");
        }
        private void Save()
        {
            Progress.levelId = LevelId;
            Progress.moves.Clear();
            foreach (var move in board.Moves) Progress.moves.Add(new SavedMove { from = move.From, to = move.To });
            if (Rules.IsWon(board.Session.State))
            {
                if (!Progress.IsCompleted(LevelId)) Progress.completedLevels.Add(LevelId);
                var record = Progress.ForLevel(LevelId);
                record.stars = System.Math.Max(record.stars, StarRating.Calculate(record.undos, record.restarts));
            }
            store.Save(Progress);
        }
    }
}
