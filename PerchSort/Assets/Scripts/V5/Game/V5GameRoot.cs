using PerchSort.Audio;
using PerchSort.V5.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public sealed class V5GameRoot : MonoBehaviour
    {
        [SerializeField] private TMP_FontAsset font;
        private V5Art art;
        private TutorialHand tutorial;
        private SettingsPanel settings;
        private V5Motion feedback;
        private AudioManager audioManager;
        private bool ownsAudio, initialized, persistProgress = true, rewardGranted;
        private bool previousSfx;
        private MusicPage previousMusic;
        private int levelId = 1;
        public GameSession Session { get; private set; }
        public BoardView Board { get; private set; }
        public HudView Hud { get; private set; }
        public WinPanel Win { get; private set; }
        public bool IsBusy { get; private set; }
        public int Coins { get; private set; }
        public Camera RenderCamera { get; private set; }
        public Canvas Canvas { get; private set; }
        public bool IsReady => initialized;
        private bool BlocksBoard => IsBusy || settings.IsOpen || Win.IsOpen;

        public void ConfigureFont(TMP_FontAsset value) => font = value;
        private void Start() => Initialize();

        // Capture calls this explicitly outside play mode with persistence disabled.
        public void Initialize(bool saveProgress = true)
        {
            if (initialized) return;
            persistProgress = saveProgress;
            Session = new GameSession(LevelLoader.Load(levelId));
            Coins = V5Progress.Coins;
            art = new V5Art();
            feedback = gameObject.AddComponent<V5Motion>();
            RenderCamera = Camera.main;
            if (RenderCamera == null) throw new System.InvalidOperationException("V5 scene needs a main camera.");
            var canvasObject = new GameObject("V5 Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            Canvas = canvasObject.GetComponent<Canvas>();
            Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            Canvas.worldCamera = RenderCamera;
            Canvas.planeDistance = 5;
            Canvas.sortingOrder = 100;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = V5UI.DesignSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            var background = V5UI.Image(Canvas.transform, "Forest background", art.Get("bg_forest_day"), V5UI.DesignSize * 0.5f, V5UI.DesignSize);
            V5UI.Stretch(background.rectTransform);
            var fit = background.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectRatio = 1080f / 1920;
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            background.raycastTarget = true;
            background.gameObject.AddComponent<V5EmptyClick>().Click = CancelSelection;
            var safe = V5UI.Rect(Canvas.transform, "Safe area", Vector2.zero, V5UI.DesignSize);
            var design = V5UI.Rect(safe, "1080x1920 design", V5UI.DesignSize * 0.5f, V5UI.DesignSize);
            var safeArea = canvasObject.AddComponent<V5SafeArea>();
            UnityEngine.Canvas.ForceUpdateCanvases();
            safeArea.Configure((RectTransform)Canvas.transform, safe, design);
            Board = V5UI.Rect(design, "Board", V5UI.DesignSize * 0.5f, V5UI.DesignSize).gameObject.AddComponent<BoardView>();
            Board.Configure(this, art);
            Board.Rebuild(Session.State);
            Hud = V5UI.Rect(design, "HUD", V5UI.DesignSize * 0.5f, V5UI.DesignSize).gameObject.AddComponent<HudView>();
            Hud.Configure(this, art, font);
            tutorial = V5UI.Rect(design, "Tutorial", V5UI.DesignSize * 0.5f, V5UI.DesignSize).gameObject.AddComponent<TutorialHand>();
            bool firstPlay = !V5Progress.TutorialSeen;
            tutorial.Configure(art, firstPlay);
            if (firstPlay && persistProgress) V5Progress.SeeTutorial();
            settings = V5UI.Rect(design, "Settings overlay", V5UI.DesignSize * 0.5f, V5UI.DesignSize).gameObject.AddComponent<SettingsPanel>();
            settings.Configure(this, art);
            Win = V5UI.Rect(design, "Win overlay", V5UI.DesignSize * 0.5f, V5UI.DesignSize).gameObject.AddComponent<WinPanel>();
            Win.Configure(this, art, font);
            if (Application.isPlaying)
            {
                audioManager = AudioManager.Instance;
                if (audioManager == null)
                {
                    audioManager = new GameObject("V5 audio").AddComponent<AudioManager>();
                    ownsAudio = true;
                }
                previousSfx = audioManager.SfxEnabled;
                previousMusic = audioManager.CurrentMusic;
                audioManager.StopMusic();
                audioManager.SetSfxEnabled(!V5Progress.Muted);
            }
            initialized = true;
            Refresh();
        }

        public bool TapBranch(int branch)
        {
            if (!initialized || BlocksBoard || branch < 0 || branch >= Session.State.Branches.Count) return false;
            int source = Session.SelectedBranch;
            if (source >= 0 && source != branch) return TryMove(source, branch);
            Session.Select(branch);
            Board.ShowSelection(Session.State, Session.SelectedBranch);
            PlaySound(Sfx.Select);
            Refresh();
            return Session.SelectedBranch >= 0;
        }

        public bool TryMove(int from, int to)
        {
            if (!initialized || BlocksBoard) return false;
            int previousSelection = Session.SelectedBranch;
            if (!Session.TryMove(from, to))
            {
                if (previousSelection >= 0)
                {
                    IsBusy = true;
                    Board.Illegal(Session.State, previousSelection);
                    PlaySound(Sfx.Error);
                    Refresh();
                    feedback.Play(0.25f, t => { }, () => { IsBusy = false; Refresh(); });
                }
                return false;
            }
            IsBusy = true;
            Board.ShowSelection(Session.State, -1);
            Refresh();
            Board.Play(Session.Events, FinishMove);
            return true;
        }

        private void FinishMove()
        {
            Board.Settle(Session.State, Session.Events);
            IsBusy = false;
            if (Session.State.IsWon)
            {
                int reward = rewardGranted ? 0 : 10 + 5 * Session.Stars;
                if (!rewardGranted)
                {
                    rewardGranted = true;
                    if (persistProgress) { V5Progress.Reward(levelId, Session.Stars); Coins = V5Progress.Coins; }
                    else Coins += reward;
                }
                PlaySound(Sfx.Win);
                Win.Show(Session.Stars, reward);
            }
            Refresh();
        }

        public void CancelSelection()
        {
            if (!initialized || BlocksBoard) return;
            Session.CancelSelection(); Board.ShowSelection(Session.State, -1); Refresh();
        }

        public bool Undo()
        {
            if (!initialized || !Session.CanUndo) return false;
            CancelPresentation();
            Session.Undo(); Board.Rebuild(Session.State);
            PlaySound(Sfx.Undo); Refresh();
            return true;
        }

        public void Restart()
        {
            if (!initialized) return;
            CancelPresentation(); Session.Restart(); rewardGranted = false;
            tutorial.DisableForAttempt();
            Board.Rebuild(Session.State); Refresh();
        }

        public bool AddBranch()
        {
            if (!initialized || BlocksBoard || !Session.AddBranch()) return false;
            Board.Rebuild(Session.State); PlaySound(Sfx.Click); Refresh();
            return true;
        }

        public void Hint()
        {
            if (!initialized || BlocksBoard || Session.State.IsWon) return;
            CancelSelection();
            var result = Solver.Solve(Session.State);
            if (result.Status == SolveStatus.Solved && result.Moves.Count > 0)
            { tutorial.Hide(); Board.Hint(result.Moves[0]); PlaySound(Sfx.Hint); }
        }

        public void OpenSettings()
        {
            if (!initialized || BlocksBoard) return;
            CancelSelection(); settings.Show(); PlaySound(Sfx.Click); Refresh();
        }
        public void CloseSettings() { settings.Hide(); PlaySound(Sfx.Back); Refresh(); }
        public void SetMuted(bool muted)
        {
            if (persistProgress) V5Progress.SetMuted(muted);
            if (audioManager != null) audioManager.SetSfxEnabled(!muted);
        }
        public void PlaySound(Sfx sound) { if (audioManager != null) audioManager.Play(sound); }
        private void Refresh()
        {
            Hud.Refresh(Session, Coins, BlocksBoard);
            tutorial.Refresh(Session, Board, BlocksBoard);
        }
        private void CancelPresentation()
        {
            feedback.CancelAll(); Board.Cancel(); Hud.Cancel(); tutorial.Hide(); Win.Hide(); settings.Hide();
            IsBusy = false;
        }
        private void OnDestroy()
        {
            // Each V5Motion cancels itself on disable/destroy; children may already be gone here.
            art?.Dispose();
            if (audioManager == null) return;
            if (ownsAudio) V5UI.Dispose(audioManager.gameObject);
            else
            {
                audioManager.SetSfxEnabled(previousSfx);
                if (previousMusic != MusicPage.None) audioManager.SwitchMusic(previousMusic);
            }
        }
    }
}
