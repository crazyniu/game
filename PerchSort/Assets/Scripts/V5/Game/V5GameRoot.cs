using System;
using System.Threading.Tasks;
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
        private V5Panels panels;
        private AudioManager audioManager;
        private bool ownsAudio, initialized, persistProgress = true, rewardGranted, hintPending;
        private bool previousSfx;
        private MusicPage previousMusic;
        private V5Profile profile;
        private int revision;
        private Action queuedTap;
        private Task<SolveResult> hintSearch;
        public GameSession Session { get; private set; }
        public BoardView Board { get; private set; }
        public HudView Hud { get; private set; }
        public WinPanel Win { get; private set; }
        public V5SceneView Scene { get; private set; }
        public bool IsBusy { get; private set; }
        public bool IsAirborne => Board != null && Board.IsAirborne;
        public bool HintPending => hintPending || (hintSearch != null && !hintSearch.IsCompleted);
        public int Coins => profile.Coins;
        public bool Muted => profile.Muted;
        public int HighestUnlocked => profile.HighestUnlocked;
        public int LevelId => Session.State.LevelId;
        public Camera RenderCamera { get; private set; }
        public Canvas Canvas { get; private set; }
        public bool IsReady => initialized;
        private bool Modal => settings.IsOpen || Win.IsOpen || panels.IsOpen;
        public void ConfigureFont(TMP_FontAsset value) => font = value;
        private void Start() => Initialize();

        // Capture initializes outside play mode without reading/writing the player's profile.
        public void Initialize(bool saveProgress = true)
        {
            if (initialized) return;
            persistProgress = saveProgress; profile = saveProgress ? V5Progress.Load() : new V5Profile();
            art = new V5Art();
            RenderCamera = Camera.main;
            if (RenderCamera == null) throw new InvalidOperationException("V5 scene needs a main camera.");
            var canvasObject = new GameObject("V5 Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            Canvas = canvasObject.GetComponent<Canvas>(); Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            Canvas.worldCamera = RenderCamera; Canvas.planeDistance = 5; Canvas.sortingOrder = 100;
            var scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = V5Layout.DesignSize; scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight; scaler.matchWidthOrHeight = 0.5f;
            var safe = V5UI.Rect(Canvas.transform, "Safe area", Vector2.zero, V5Layout.DesignSize);
            var design = V5UI.Rect(safe, "1080x1920 design", V5Layout.DesignSize * 0.5f, V5Layout.DesignSize);
            var safeArea = canvasObject.AddComponent<V5SafeArea>(); UnityEngine.Canvas.ForceUpdateCanvases();
            safeArea.Configure((RectTransform)Canvas.transform, safe, design);
            Scene = Layer<V5SceneView>(design, "Forest"); Scene.Configure(art, CancelSelection);
            Scene.FillScreen(Canvas.transform);
            Board = Layer<BoardView>(design, "Board"); Board.Configure(this, art);
            Scene.PlaceLighting(Board.transform);
            Hud = Layer<HudView>(design, "HUD"); Hud.Configure(this, art, font);
            tutorial = Layer<TutorialHand>(design, "Tutorial"); tutorial.Configure(art);
            settings = Layer<SettingsPanel>(design, "Settings overlay"); settings.Configure(this, art);
            Win = Layer<WinPanel>(design, "Win overlay"); Win.Configure(this, art, font);
            panels = Layer<V5Panels>(design, "Picker and purchases"); panels.Configure(this, art, font);
            if (Application.isPlaying)
            {
                audioManager = AudioManager.Instance;
                if (audioManager == null) { audioManager = new GameObject("V5 audio").AddComponent<AudioManager>(); ownsAudio = true; }
                previousSfx = audioManager.SfxEnabled; previousMusic = audioManager.CurrentMusic;
                audioManager.StopMusic(); audioManager.SetSfxEnabled(!profile.Muted);
            }
            initialized = true;
            LoadLevel(1);
        }
        private static T Layer<T>(Transform parent, string name) where T : Component =>
            V5UI.Rect(parent, name, V5Layout.DesignSize * 0.5f, V5Layout.DesignSize).gameObject.AddComponent<T>();
        private void Save()
        {
            profile.Capture(Session);
            if (persistProgress) V5Progress.Save(profile);
        }
        public void LoadLevel(int id)
        {
            if (!initialized || id > profile.HighestUnlocked) return;
            LoadDefinition(LevelLoader.Load(id), true);
        }
        private void LoadDefinition(LevelDef level, bool starters)
        {
            CancelPresentation(); rewardGranted = false;
            if (starters) profile.GrantStarters(level.id);
            Session = new GameSession(level, new SongState(profile.SongNotes, profile.PayoutCount), profile.Inventory());
            Board.Rebuild(Session.State); Hud.SetLevel(Session.State); Scene.SetLevel(Session.State);
            tutorial.Begin(level.id, !profile.Tutorials[level.id]); Refresh(); Save();
        }
        public void NextLevel()
        {
            if (!Session.State.IsWon) return;
            LoadLevel(LevelLoader.Next(LevelId));
        }
        // Fixture loading is explicit, uses real Core rules and never mutates PlayerPrefs.
        public void LoadFixture(LevelDef level, BoosterInventory inventory = null, int coins = 0, SongState song = default)
        {
            if (!initialized) Initialize(false);
            persistProgress = false; profile = new V5Profile { Coins = coins, HighestUnlocked = 40, SongNotes = song.Notes, PayoutCount = song.PayoutCount };
            if (inventory != null) for (int i = 0; i < profile.Boosters.Length; i++) profile.Boosters[i] = inventory[(BoosterKind)i];
            LoadDefinition(level, false);
        }
        private void InputTap()
        {
            revision++; hintPending = false; Board.ClearHints();
        }
        private bool Defer(Action tap)
        {
            if (!IsBusy) return false;
            // The transaction is already committed. Accept one landing/effect tap, never mutate its event list.
            if (!IsAirborne && queuedTap == null) queuedTap = tap;
            return true;
        }
        public bool TapBranch(int branch)
        {
            if (!initialized || Modal || (uint)branch >= Session.State.Branches.Count) return false;
            InputTap();
            if (Defer(() => TapBranch(branch))) return false;
            int source = Session.SelectedBranch;
            if (source >= 0 && source != branch) return TryMove(source, branch);
            Session.Select(branch); Board.ShowSelection(Session.State, Session.SelectedBranch); PlaySound(Sfx.Select); Refresh();
            return Session.SelectedBranch >= 0;
        }
        public bool TapSwivel(int branch)
        {
            if (!initialized || Modal) return false;
            InputTap(); if (Defer(() => TapSwivel(branch))) return false;
            if (!Session.Swivel(branch)) return false;
            PlayCommitted(); return true;
        }
        public bool TryMove(int from, int to)
        {
            if (!initialized || Modal || IsBusy) return false;
            InputTap(); int selected = Session.SelectedBranch;
            if (!Session.TryMove(from, to))
            {
                Board.ShowSelection(Session.State, -1);
                if (selected >= 0) { Board.Illegal(Session.State, selected); PlaySound(Sfx.Error); }
                Refresh(); return false;
            }
            PlayCommitted(); return true;
        }
        private void PlayCommitted(Action after = null)
        {
            IsBusy = true; tutorial.Hide();
            var events = new GameEvent[Session.Events.Count];
            for (int i = 0; i < events.Length; i++) events[i] = Session.Events[i];
            Save(); Refresh();
            Board.Play(events, () => { FinishAction(); after?.Invoke(); });
        }
        private void FinishAction()
        {
            Board.Settle(Session.State); IsBusy = false;
            int reward = 0, before = Coins;
            if (Session.State.IsWon)
            {
                if (!rewardGranted) { rewardGranted = true; reward = profile.Reward(LevelId, Session.Stars, Session.State.ChorusCoins); }
                profile.Tutorials[LevelId] = true; PlaySound(Sfx.Win);
                Win.Show(Session.Stars, reward, reward > 0 ? Session.State.ChorusCoins : 0);
            }
            tutorial.ActionCompleted(Session);
            if (tutorial.Completed) profile.Tutorials[LevelId] = true;
            Save(); Refresh();
            if (reward > 0) Hud.CountCoins(before, Coins);
            var next = queuedTap; queuedTap = null;
            if (!Session.State.IsWon) next?.Invoke();
        }
        public void PresentationInputChanged() { if (initialized) Refresh(); }
        public void CancelSelection()
        {
            if (!initialized || Modal) return;
            InputTap(); if (Defer(CancelSelection)) return;
            Session.CancelSelection(); Board.ShowSelection(Session.State, -1); Refresh();
        }
        public bool Undo()
        {
            if (!initialized || !Session.CanUndo) return false;
            CancelPresentation(); Session.Undo(); Board.Rebuild(Session.State); Hud.SyncState(Session.State); Scene.SetLevel(Session.State);
            tutorial.DisableForAttempt(); PlaySound(Sfx.Undo); Save(); Refresh(); return true;
        }
        public void Restart()
        {
            if (!initialized) return;
            CancelPresentation(); Session.Restart(); rewardGranted = false; tutorial.DisableForAttempt();
            Board.Rebuild(Session.State); Hud.SyncState(Session.State); Scene.SetLevel(Session.State); Save(); Refresh();
        }
        public void TapBooster(BoosterKind kind)
        {
            if (!initialized || Modal || LevelId < V5ViewModel.Unlock(kind)) return;
            InputTap(); if (Defer(() => TapBooster(kind))) return;
            if (Session.Inventory[kind] == 0) { Session.CancelSelection(); Board.ShowSelection(Session.State, -1); panels.Buy(kind); Refresh(); }
            else if (kind == BoosterKind.Firefly) Hint();
            else UseBooster(kind);
        }
        public bool BuyBooster(BoosterKind kind)
        {
            if (!initialized || IsBusy || LevelId < V5ViewModel.Unlock(kind) || Coins < V5ViewModel.Price(kind)) return false;
            Session.GrantBooster(kind, 1); profile.Coins -= V5ViewModel.Price(kind); Save(); Refresh(); return true;
        }
        public bool UseBooster(BoosterKind kind)
        {
            if (!initialized || Modal || IsBusy || kind == BoosterKind.Firefly || LevelId < V5ViewModel.Unlock(kind)) return false;
            InputTap(); if (!Session.UseBooster(kind).Applied) { Refresh(); return false; }
            PlayCommitted(); return true;
        }
        public async void Hint()
        {
            if (!initialized || Modal || IsBusy || HintPending || LevelId < 2 || Session.Inventory[BoosterKind.Firefly] == 0) return;
            InputTap(); Session.CancelSelection(); Board.ShowSelection(Session.State, -1); tutorial.Hide();
            int token = revision; var session = Session; var snapshot = session.State.Clone(); hintPending = true;
            Refresh();
            try
            {
                hintSearch = Task.Run(() => Solver.Solve(snapshot, maxStates: 100000, maxMilliseconds: 1500));
                var result = await hintSearch;
                if (this == null || !initialized || token != revision || Session != session) return;
                hintPending = false;
                Move? move = result.Status == SolveStatus.Solved && result.Moves.Count > 0 ? result.Moves[0] : (Move?)null;
                if (result.IsUnknown)
                {
                    move = V5ViewModel.GreedyHint(snapshot);
                    Debug.Log("V5 Firefly: solver Unknown; " + (move.HasValue ? "using a greedy legal move." : "no legal move; not consumed."));
                }
                if (!move.HasValue || !session.TryUseFirefly(move.Value)) { Refresh(); return; }
                var hint = move.Value;
                PlayCommitted(() =>
                {
                    if (token != revision || Session != session || IsBusy) return;
                    Board.Hint(hint); PlaySound(Sfx.Hint);
                });
            }
            catch (Exception error)
            {
                if (this != null && token == revision) { hintPending = false; Debug.LogException(error); Refresh(); }
            }
            finally { if (this != null && initialized) Refresh(); }
        }
        public void OpenSettings()
        {
            if (!initialized || Modal) return;
            InputTap(); if (Defer(OpenSettings)) return;
            Session.CancelSelection(); Board.ShowSelection(Session.State, -1); settings.Show(); PlaySound(Sfx.Click); Refresh();
        }
        public void CloseSettings() { settings.Hide(); PlaySound(Sfx.Back); Refresh(); }
        public void OpenLevelPicker()
        {
            if (!initialized || IsAirborne || settings.IsOpen || panels.IsOpen) return;
            if (Defer(OpenLevelPicker)) return;
            InputTap(); Win.Hide(); Session.CancelSelection(); Board.ShowSelection(Session.State, -1); panels.Picker(profile.HighestUnlocked); Refresh();
        }
        public void ClosePanel() { panels.Hide(); Refresh(); }
        public void SetMuted(bool muted)
        {
            profile.Muted = muted; Save(); if (audioManager != null) audioManager.SetSfxEnabled(!muted);
        }
        public void PlaySound(Sfx sound) { if (audioManager != null) audioManager.Play(sound); }
        private void Refresh()
        {
            Hud.Refresh(Session, Coins, IsAirborne || Modal, IsBusy);
            tutorial.Refresh(Session, Board, IsBusy || Modal);
        }
        private void CancelPresentation()
        {
            revision++; hintPending = false; queuedTap = null;
            Board.Cancel(); Hud.Cancel(); tutorial.Hide(); Win.Hide(); settings.Hide(); panels.Hide(); Scene.Cancel(); IsBusy = false;
        }
        private void OnDestroy()
        {
            initialized = false; revision++; art?.Dispose();
            if (audioManager == null) return;
            if (ownsAudio) V5UI.Dispose(audioManager.gameObject);
            else { audioManager.SetSfxEnabled(previousSfx); if (previousMusic != MusicPage.None) audioManager.SwitchMusic(previousMusic); }
        }
    }
}
