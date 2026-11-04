using System;
using System.Threading.Tasks;
using PerchSort.Audio;
using PerchSort.V5.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IClock = PerchSort.V5.Meta.IClock;

namespace PerchSort.V5.Game
{
    public sealed class V5GameRoot : MonoBehaviour
    {
        [SerializeField] private TMP_FontAsset font;
        private V5Art art;
        private TutorialHand tutorial;
        private SettingsPanel settings;
        private V5Panels panels;
        private V7LevelEntrance entrance;
        private bool notificationsEnabled, notificationTransition;
        public IClock Clock { get; set; } = new PerchSort.V5.Meta.SystemClock();
        public V7Notification CurrentNotification => profile?.Notifications.Current;
        public bool IsIntroducing => entrance != null && entrance.IsPlaying;
        private AudioManager audioManager;
        private CanvasGroup hudInput, boardInput, mapInput;
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
        public V6MapView Map { get; private set; }
        public bool IsMap { get; private set; }
        public bool HasModal => Modal;
        public int BestMoves(int level) => profile.BestMoves[level];
        public bool IsBusy { get; private set; }
        public bool IsAirborne => Board != null && Board.IsAirborne;
        public bool HintPending => hintPending || (hintSearch != null && !hintSearch.IsCompleted);
        public int Coins => profile.DisplayCoins;
        public bool Muted => profile.Muted;
        public int HighestUnlocked => profile.AccessibleLevel;
        public int BestStars(int level) => profile.BestStars(level);
        public int DisplayStock(int stock) => profile.DisplayStock(stock);
        public int LevelId => Session.State.LevelId;
        public Camera RenderCamera { get; private set; }
        public Canvas Canvas { get; private set; }
        public bool IsReady => initialized;
        private bool Modal => settings.IsOpen || Win.IsOpen || panels.IsOpen || IsIntroducing || notificationTransition;
        public void ConfigureFont(TMP_FontAsset value) => font = value;
        private void Start() => Initialize();

        // Capture initializes outside play mode without reading/writing the player's profile.
        public void Initialize(bool saveProgress = true, bool? showNotifications = null)
        {
            if (initialized) return;
            persistProgress = saveProgress; profile = saveProgress ? V5Progress.Load() : new V5Profile();
            notificationsEnabled = showNotifications ?? saveProgress;
            art = new V5Art();
            RenderCamera = Camera.main;
            if (RenderCamera == null) throw new InvalidOperationException("V5 scene needs a main camera.");
            var canvasObject = new GameObject("V5 Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            Canvas = canvasObject.GetComponent<Canvas>(); Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            Canvas.worldCamera = RenderCamera; Canvas.planeDistance = 5; Canvas.sortingOrder = 100;
            var scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = V7Layout.DesignSize; scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight; scaler.matchWidthOrHeight = 0.5f;
            var safe = V5UI.Rect(Canvas.transform, "Safe area", Vector2.zero, V7Layout.DesignSize);
            var design = V5UI.Rect(safe, "1080x1920 design", V7Layout.DesignSize * 0.5f, V7Layout.DesignSize);
            var safeArea = canvasObject.AddComponent<V5SafeArea>(); UnityEngine.Canvas.ForceUpdateCanvases();
            safeArea.Configure((RectTransform)Canvas.transform, safe, design);
            Scene = Layer<V5SceneView>(design, "Forest"); Scene.Configure(art, CancelSelection);
            Scene.FillScreen(Canvas.transform);
            Board = Layer<BoardView>(design, "Board"); Board.Configure(this, art);
            Map = Layer<V6MapView>(design, "World map"); Map.Configure(this, art, font);
            Scene.PlaceLighting(Board.transform);
            Hud = Layer<HudView>(design, "HUD"); Hud.Configure(this, art, font);
            tutorial = Layer<TutorialHand>(design, "Tutorial"); tutorial.Configure(art);
            settings = Layer<SettingsPanel>(design, "Settings overlay"); settings.Configure(this, art);
            Win = Layer<WinPanel>(design, "Win overlay"); Win.Configure(this, art, font);
            panels = Layer<V5Panels>(design, "Information cards"); panels.Configure(this, art, font);
            entrance = Layer<V7LevelEntrance>(design, "Level entrance"); entrance.Configure(this);
            hudInput = Hud.gameObject.AddComponent<CanvasGroup>();
            boardInput = Board.gameObject.AddComponent<CanvasGroup>();
            mapInput = Map.gameObject.AddComponent<CanvasGroup>();
            if (Application.isPlaying)
            {
                audioManager = AudioManager.Instance;
                if (audioManager == null) { audioManager = new GameObject("V5 audio").AddComponent<AudioManager>(); ownsAudio = true; }
                previousSfx = audioManager.SfxEnabled; previousMusic = audioManager.CurrentMusic;
                audioManager.StopMusic(); audioManager.SetSfxEnabled(!profile.Muted);
            }
            initialized = true;
            Session = profile.CreateSession(LevelLoader.Load(1));
            OpenMap();
        }
        private static T Layer<T>(Transform parent, string name) where T : Component =>
            V5UI.Rect(parent, name, V7Layout.DesignSize * 0.5f, V7Layout.DesignSize).gameObject.AddComponent<T>();
        private void Save()
        {
            profile.Capture(Session);
            if (persistProgress) V5Progress.Save(profile);
        }
        public void LoadLevel(int id)
        {
            if (!initialized || IsIntroducing || notificationTransition || CurrentNotification != null || id < 1 || id > HighestUnlocked) return;
            LoadDefinition(LevelLoader.Load(id), true);
        }
        private void LoadDefinition(LevelDef level, bool starters)
        {
            CancelPresentation(); rewardGranted = false;
            IsMap = false; Map.Hide(); Board.gameObject.SetActive(true); Hud.gameObject.SetActive(true);
            if (starters) profile.GrantStarters(Math.Min(level.id, profile.HighestUnlocked));
            Session = profile.CreateSession(level);
            Board.Rebuild(Session.State); Hud.SetLevel(Session.State); Scene.SetLevel(Session.State);
            tutorial.Begin(level.id, !profile.Tutorials[level.id], Session.Inventory[BoosterKind.Sapling] > 0);
            // Persist on presentation, not consumption: leaving before the first tap must not repeat it.
            if (tutorial.SaplingPromptPending) profile.Tutorials[level.id] = true;
            Refresh(); Save();
            if (starters && notificationsEnabled) BeginLevelEntrance();
        }
        private static System.Collections.Generic.IEnumerable<string> LevelSpecies(BoardState state)
        {
            foreach (var branch in state.Branches) foreach (var bird in branch.Birds) yield return bird.Species.Id;
        }
        private void BeginLevelEntrance()
        {
            profile.Notifications.BeginLevel(LevelId, LevelSpecies(Session.State)); IsBusy = true;
            entrance.Play(() =>
            {
                IsBusy = false; profile.Notifications.FinishIntro(); Save(); PumpNotifications(); Refresh();
            });
        }
        public void NextLevel()
        {
            if (!Session.State.IsWon) return;
            if (CurrentNotification != null || notificationTransition || TryLeaveFinale(true)) return;
            // After the finale there is no next level: return to the map instead of ignoring the tap.
            if (LevelId >= LevelLoader.Catalog.levels.Length) { OpenMap(); return; }
            LoadLevel(LevelId + 1);
        }
        // Fixture loading is explicit, uses real Core rules and never mutates PlayerPrefs.
        public void LoadFixture(LevelDef level, BoosterInventory inventory = null, int coins = 0)
        {
            if (!initialized) Initialize(false);
            persistProgress = false; profile = new V5Profile { Coins = coins, HighestUnlocked = 40 };
            notificationsEnabled = false;
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
            if (!initialized || IsMap || Modal || (uint)branch >= Session.State.Branches.Count) return false;
            InputTap(); tutorial.DismissSaplingPrompt();
            if (Defer(() => TapBranch(branch))) return false;
            int source = Session.SelectedBranch;
            if (source >= 0 && source != branch) return TryMove(source, branch);
            Session.Select(branch); Board.ShowSelection(Session.State, Session.SelectedBranch); PlaySound(Sfx.Select); Refresh();
            return Session.SelectedBranch >= 0;
        }
        public bool TapSwivel(int branch)
        {
            if (!initialized || IsMap || Modal) return false;
            InputTap(); if (Defer(() => TapSwivel(branch))) return false;
            if (!Session.Swivel(branch)) return false;
            profile.RecordAction();
            PlayCommitted(); return true;
        }
        public bool TryMove(int from, int to)
        {
            if (!initialized || IsMap || Modal || IsBusy) return false;
            InputTap(); tutorial.DismissSaplingPrompt(); int selected = Session.SelectedBranch;
            if (!Session.TryMove(from, to))
            {
                Board.ShowSelection(Session.State, -1);
                if (selected >= 0) { Board.Illegal(Session.State, selected); PlaySound(Sfx.Error); }
                Refresh(); return false;
            }
            profile.RecordAction();
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
                if (!rewardGranted)
                {
                    rewardGranted = true; reward = profile.Reward(LevelId, Session.Stars, Session.State.ChorusCoins);
                    profile.RecordBestMoves(LevelId, Session.MoveCount);
                }
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
            if (!initialized || IsMap || Modal) return;
            InputTap(); if (Defer(CancelSelection)) return;
            Session.CancelSelection(); Board.ShowSelection(Session.State, -1); Refresh();
        }
        public bool Undo()
        {
            if (!initialized || IsMap || (Modal && !Win.IsOpen) || !Session.CanUndo) return false;
            CancelPresentation(); Session.Undo(); profile.UndoAction(); Board.Rebuild(Session.State); Hud.SyncState(Session.State); Scene.SetLevel(Session.State);
            tutorial.DisableForAttempt(); PlaySound(Sfx.Undo); Save(); Refresh(); return true;
        }
        public void Restart()
        {
            if (!initialized || IsMap) return;
            CancelPresentation(); Session.Restart(); profile.RestartSession(); rewardGranted = false; tutorial.DisableForAttempt();
            Board.Rebuild(Session.State); Hud.SyncState(Session.State); Scene.SetLevel(Session.State); Save(); Refresh();
            if (notificationsEnabled) BeginLevelEntrance();
        }
        public void TapBooster(BoosterKind kind)
        {
            if (!initialized || IsMap || Modal || HighestUnlocked < V5ViewModel.Unlock(kind)) return;
            if (kind == BoosterKind.Sapling) tutorial.DismissSaplingPrompt();
            InputTap(); if (Defer(() => TapBooster(kind))) return;
            if (Session.Inventory[kind] == 0) { Session.CancelSelection(); Board.ShowSelection(Session.State, -1); panels.Buy(kind); Refresh(); }
            else if (kind == BoosterKind.Firefly) Hint();
            else UseBooster(kind);
        }
        public bool BuyBooster(BoosterKind kind)
        {
            if (!initialized || IsBusy || HighestUnlocked < V5ViewModel.Unlock(kind) || Coins < V5ViewModel.Price(kind)) return false;
            Session.GrantBooster(kind, 1); profile.SpendCoins(V5ViewModel.Price(kind)); Save(); Refresh(); return true;
        }
        public bool UseBooster(BoosterKind kind)
        {
            if (!initialized || IsMap || Modal || IsBusy || kind == BoosterKind.Firefly || HighestUnlocked < V5ViewModel.Unlock(kind)) return false;
            if (kind == BoosterKind.Sapling) tutorial.DismissSaplingPrompt();
            InputTap(); if (!Session.UseBooster(kind).Applied) { Refresh(); return false; }
            profile.RecordAction();
            PlayCommitted(); return true;
        }
        public async void Hint()
        {
            if (!initialized || IsMap || Modal || IsBusy || HintPending || HighestUnlocked < 2 || Session.Inventory[BoosterKind.Firefly] == 0) return;
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
                profile.RecordAction();
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
            if (!IsMap) { Session.CancelSelection(); Board.ShowSelection(Session.State, -1); }
            settings.Show(); PlaySound(Sfx.Click); Refresh();
        }
        public void CloseSettings() { settings.Hide(); PlaySound(Sfx.Back); Refresh(); }
        public void OpenMap()
        {
            if (!initialized || IsAirborne || IsIntroducing || notificationTransition || CurrentNotification != null) return;
            if (TryLeaveFinale(false)) return;
            if (Defer(OpenMap)) return;
            CancelPresentation(); Save(); ShowMapSurface();
            if (notificationsEnabled) { profile.Notifications.BeginMap(Clock); Save(); PumpNotifications(); }
            Refresh();
        }
        private void ShowMapSurface()
        {
            IsMap = true; Session.CancelSelection();
            Board.gameObject.SetActive(false); Hud.gameObject.SetActive(false); tutorial.Hide();
            Map.Show(profile);
        }
        private bool TryLeaveFinale(bool next)
        {
            if (!notificationsEnabled || !Win.IsOpen || !profile.Notifications.HasWorldSequence) return false;
            profile.Notifications.LeaveWin(LevelId, next); Win.Hide(); Save(); PumpNotifications(); Refresh(); return true;
        }
        private void PumpNotifications()
        {
            if (!notificationsEnabled || !initialized || Modal || IsBusy) return;
            var request = profile.Notifications.Next(IsMap);
            if (request == null)
            {
                int next = IsMap ? profile.Notifications.TakeDeferredLevel() : 0;
                if (next > 0) LoadLevel(next);
                return;
            }
            Save();
            if (request.Kind == V7NotificationKind.BoosterUnlock) panels.Unlock(request.Booster);
            else if (request.Kind == V7NotificationKind.WorldUnlock)
            {
                ShowMapSurface(); profile.Notifications.BeginMap(Clock); Save();
                notificationTransition = true; int token = revision; Refresh();
                Map.UnlockGate(request.World, () =>
                {
                    if (!initialized || token != revision) return;
                    notificationTransition = false;
                    panels.Popup(V7PopupKind.WorldUnlock, "sunny", 0, () => AcknowledgeNotification(request), request.World);
                    Refresh();
                });
                return;
            }
            else
            {
                V7PopupKind kind = request.Kind == V7NotificationKind.WorldComplete ? V7PopupKind.WorldComplete :
                    request.Kind == V7NotificationKind.DailyGift ? V7PopupKind.DailyGift : V7PopupKind.NewFriend;
                int reward = kind == V7PopupKind.WorldComplete ? 200 : kind == V7PopupKind.DailyGift ? 50 : 20;
                panels.Popup(kind, request.Species ?? "sunny", reward, () => AcknowledgeNotification(request), request.World);
            }
            Refresh();
        }
        public void AcknowledgeNotification(V7Notification request)
        {
            if (!initialized || notificationTransition || !panels.IsOpen || !ReferenceEquals(CurrentNotification, request)) return;
            int before = Coins, reward = profile.Notifications.Acknowledge(request, Clock);
            panels.Hide(); Save(); Refresh();
            if (reward > 0 && IsMap)
            {
                notificationTransition = true; int token = revision; Refresh();
                Map.CollectCoins(before, Coins, () =>
                {
                    if (!initialized || token != revision) return;
                    notificationTransition = false; PumpNotifications(); Refresh();
                });
                return;
            }
            if (reward > 0) Hud.CountCoins(before, Coins);
            PumpNotifications(); Refresh();
        }
        public void OpenLevelCard(int id)
        {
            if (!initialized || !IsMap || Modal || !V6MapModel.Selectable(profile, id)) return;
            panels.LevelCard(id); Refresh();
        }
        public void OpenPopup(V7PopupKind kind, string species = "mint", int reward = 0)
        {
            if (!initialized || IsAirborne || Modal) return;
            panels.Popup(kind, species, reward); Refresh();
        }
        public void OpenGoalInfo(string species)
        {
            if (!initialized || IsMap || IsBusy || Modal || !Hud.Houses.ContainsKey(species)) return;
            InputTap(); panels.GoalInfo(species); Refresh();
        }
        public void OpenUnlock(BoosterKind kind)
        {
            if (!initialized || IsMap || IsBusy || Modal) return;
            panels.Unlock(kind); Refresh();
        }
        public void AcknowledgeUnlock()
        {
            if (!panels.IsUnlock) return;
            if (CurrentNotification != null && CurrentNotification.Kind == V7NotificationKind.BoosterUnlock)
                AcknowledgeNotification(CurrentNotification);
            else { panels.Hide(); Refresh(); }
        }
        public void ClosePanel() { if (panels.IsUnlock || panels.IsNotification) return; panels.Hide(); Refresh(); }
        public void SetMuted(bool muted)
        {
            profile.Muted = muted; Save(); if (audioManager != null) audioManager.SetSfxEnabled(!muted);
        }
        public void PlaySound(Sfx sound) { if (audioManager != null) audioManager.Play(sound); }
        private void Refresh()
        {
            bool blocked = IsAirborne || Modal;
            Hud.Refresh(Session, Coins, blocked, IsBusy);
            tutorial.Refresh(Session, Board, IsMap || IsBusy || Modal);
            SetRaycasts(hudInput, !blocked && !IsMap);
            SetRaycasts(boardInput, !Modal && !IsMap);
            SetRaycasts(mapInput, !Modal && IsMap);
            if (IsMap) Map.RefreshCoins(Coins);
        }
        private static void SetRaycasts(CanvasGroup group, bool enabled) => group.blocksRaycasts = group.interactable = enabled;
        private void CancelPresentation()
        {
            revision++; hintPending = false; queuedTap = null;
            profile.Notifications.CancelPresentation(); notificationTransition = false; entrance.Cancel();
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
