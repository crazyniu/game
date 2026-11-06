using System;
using System.Threading.Tasks;
using PerchSort.Audio;
using PerchSort.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using IClock = PerchSort.Meta.IClock;

namespace PerchSort.Game
{
    public sealed class GameRoot : MonoBehaviour
    {
        [SerializeField] private TMP_FontAsset font;
        private GameArt art;
        private BoardTutorial tutorial;
        private PanelsView panels;
        public ShopView Shop { get; private set; }
        public AviaryView Aviary { get; private set; }
        public PerchSort.Meta.PlayerProfile Collection => profile.Notifications.RewardLedger;
        public int BoosterStock(BoosterKind kind) => profile.DisplayStock(Session.Inventory[kind]);
        private LevelEntrance entrance;
        private LeafTransition leafTransition;
        private bool notificationsEnabled, notificationTransition;
        private bool suspended, resumeWin;
        private bool focusEntrance, focusResolution;
        public IClock Clock { get; set; } = new PerchSort.Meta.SystemClock();
        public Notification CurrentNotification => profile?.Notifications.Current;
        public bool IsIntroducing => entrance != null && entrance.IsPlaying;
        private AudioManager audioManager;
        private CanvasGroup hudInput, boardInput, mapInput;
        private bool ownsAudio, initialized, persistProgress = true, rewardGranted, hintPending;
        private bool previousSfx, previousMusicEnabled;
        private MusicPage previousMusic;
        private GameProfile profile;
        private int revision, transactionGeneration;
        private int pendingWinReward, winCoinStart;
        private bool winCountPending;
        private Task<SolveResult> hintSearch;
        private SleepBubble sleepFeedback;
        private GameSession lastMoveSession;
        private Move? lastMove;
        private string lastMoveReason;
        private Action entranceCompleted, resolutionCompleted;
        private readonly OverlayPresentation boardOverlay = new OverlayPresentation();
        public BoardPresentation BoardScreen { get; private set; }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public BoardFakeData BoardStates { get; private set; }
        private GameProfile savedProfile;
        private GameSession savedSession;
        private BoardSnapshot savedBoard;
        private Action previewNotificationNext;
        private bool savedPersistence, savedNotifications, savedMap, savedBusy, savedIntroduction, savedWin;
        public bool IsBoardPreview => BoardStates != null && BoardStates.Active;
#else
        public bool IsBoardPreview => false;
#endif
        public GameSession Session { get; private set; }
        public BoardView Board { get; private set; }
        public HudView Hud { get; private set; }
        public WinPanel Win { get; private set; }
        public SceneView Scene { get; private set; }
        public MapView Map { get; private set; }
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
        private bool IsWiping => leafTransition != null && leafTransition.IsPlaying;
        private bool Modal => Win.IsOpen || panels.IsOpen || Shop.IsOpen || Aviary.IsOpen || IsIntroducing || notificationTransition || IsWiping;
        public void ConfigureFont(TMP_FontAsset value) => font = value;
        private void Start() => Initialize();
        private void Update()
        {
            bool pressed = (Pointer.current != null && Pointer.current.press.wasPressedThisFrame) ||
                (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame);
            if (!pressed) return;
            audioManager?.ResumeMusicOnUserInteraction();
            Board?.FastForwardPlayback();
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame && initialized && !IsMap && !IsBusy && !IsIntroducing)
            {
                if (panels.IsSettings) CloseSettings();
                else if (Shop.IsOpen || panels.IsOpen) BoardScreen.RequestDismissTop();
                else if (!Win.IsOpen) BoardScreen.RequestSettings();
            }
        }

        // Capture initializes outside play mode without reading/writing the player's profile.
        public void Initialize(bool saveProgress = true, bool? showNotifications = null)
        {
            if (initialized) return;
            persistProgress = saveProgress; profile = saveProgress ? ProgressStore.Load() : new GameProfile();
            notificationsEnabled = showNotifications ?? saveProgress;
            art = new GameArt();
            RenderCamera = Camera.main;
            if (RenderCamera == null) throw new InvalidOperationException("Game scene needs a main camera.");
            var canvasObject = new GameObject("Game Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            Canvas = canvasObject.GetComponent<Canvas>(); Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            Canvas.worldCamera = RenderCamera; Canvas.planeDistance = 5; Canvas.sortingOrder = 100;
            var scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = GameLayout.DesignSize; scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight; scaler.matchWidthOrHeight = 0.5f;
            var safe = GameUI.Rect(Canvas.transform, "Safe area", Vector2.zero, GameLayout.DesignSize);
            var design = GameUI.Rect(safe, "1080x1920 design", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            var safeArea = canvasObject.AddComponent<SafeArea>(); UnityEngine.Canvas.ForceUpdateCanvases();
            safeArea.Configure((RectTransform)Canvas.transform, safe, design);
            BoardScreen = Layer<BoardPresentation>(design, "Board presentation");
            Scene = Layer<SceneView>(design, "Forest"); Scene.Configure(art, () => BoardScreen.RequestCancel());
            Scene.FillScreen(Canvas.transform);
            Board = Layer<BoardView>(design, "Board"); Board.Configure(BoardScreen, art);
            sleepFeedback = gameObject.AddComponent<SleepBubble>(); sleepFeedback.Configure(this, art);
            Map = Layer<MapView>(design, "World map"); Map.Configure(this, art, font);
            Scene.PlaceLighting(Board.transform);
            Hud = Layer<HudView>(design, "HUD"); Hud.Configure(BoardScreen, art, font);
            BoardScreen.transform.SetAsLastSibling();
            var hand = Layer<TutorialHand>(design, "Tutorial"); hand.Configure(art);
            tutorial = new BoardTutorial(BoardScreen);
            Win = Layer<WinPanel>(design, "Win overlay"); Win.Configure(this, art, font);
            panels = Layer<PanelsView>(design, "Information cards"); panels.Configure(this, art, font);
            Shop = Layer<ShopView>(design, "Shop"); Shop.Configure(this, art, font);
            Aviary = Layer<AviaryView>(design, "Aviary"); Aviary.Configure(this, art, font);
            entrance = Layer<LevelEntrance>(design, "Level entrance"); entrance.Configure(BoardScreen);
            var leafLayer = GameUI.Rect(Canvas.transform, "Leaf transition", Vector2.zero, GameLayout.DesignSize);
            GameUI.Stretch(leafLayer);
            leafTransition = leafLayer.gameObject.AddComponent<LeafTransition>(); leafTransition.Configure(art);
            hudInput = Hud.gameObject.AddComponent<CanvasGroup>();
            boardInput = Board.gameObject.AddComponent<CanvasGroup>();
            mapInput = Map.gameObject.AddComponent<CanvasGroup>();
            BoardScreen.Configure(art, Board, Hud, Scene, hand, entrance, boardInput, hudInput);
            BindBoardRequests();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            BoardStates = new BoardFakeData(this, BoardScreen);
#endif
            if (Application.isPlaying)
            {
                audioManager = AudioManager.Instance;
                if (audioManager == null) { audioManager = new GameObject("Game audio").AddComponent<AudioManager>(); ownsAudio = true; }
                previousSfx = audioManager.SfxEnabled; previousMusicEnabled = audioManager.MusicEnabled;
                previousMusic = audioManager.CurrentMusic;
                audioManager.SetSfxEnabled(!profile.Muted); audioManager.SetMusicEnabled(!profile.Muted);
            }
            initialized = true;
            Session = profile.CreateSession(LevelLoader.Load(1));
            OpenMap();
        }
        private void BindBoardRequests()
        {
            BoardScreen.BranchRequested += id => DispatchBoard(new BoardAction { Kind = "branch", BranchId = id });
            BoardScreen.SwivelRequested += id => DispatchBoard(new BoardAction { Kind = "swivel", BranchId = id });
            BoardScreen.SelectionCancelRequested += () => DispatchBoard(new BoardAction { Kind = "cancel" });
            BoardScreen.UndoRequested += () => DispatchBoard(new BoardAction { Kind = "undo" });
            BoardScreen.BoosterRequested += kind => DispatchBoard(new BoardAction { Kind = "tool", Booster = kind });
            BoardScreen.ShopRequested += kind => OpenShop(kind);
            BoardScreen.GoalInfoRequested += OpenGoalInfo;
            BoardScreen.SettingsRequested += OpenSettings;
            BoardScreen.DismissTopRequested += () => { if (panels.IsSettings) CloseSettings(); else ClosePanel(); };
            BoardScreen.EntranceSkipRequested += BoardScreen.SkipEntrance;
            BoardScreen.HintDismissRequested += () => BoardScreen.ShowBubble(null);
            BoardScreen.DeferredActionRequested += action => Board.FastForwardPlayback();
            BoardScreen.EntrancePresented += id =>
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (IsBoardPreview) { BoardStates.EntranceFinished(id); return; }
#endif
                var completed = entranceCompleted; entranceCompleted = null; completed?.Invoke();
            };
            BoardScreen.ResolutionPresented += id =>
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (IsBoardPreview) { BoardStates.ResolutionFinished(id); return; }
#endif
                var completed = resolutionCompleted; resolutionCompleted = null; completed?.Invoke();
            };
            BoardScreen.SoundRequested += PlaySound;
            BoardScreen.InputChanged += PresentationInputChanged;
        }
        private void DispatchBoard(BoardAction action)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (IsBoardPreview) { BoardStates.Handle(action); return; }
#endif
            switch (action.Kind)
            {
                case "branch": TapBranch(action.BranchId); break;
                case "swivel": TapSwivel(action.BranchId); break;
                case "cancel": CancelSelection(); break;
                case "undo": Undo(); break;
                case "tool": TapBooster(action.Booster); break;
                case "settings": OpenSettings(); break;
            }
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void BeginBoardPreview(BoardSnapshot snapshot)
        {
            if (savedProfile == null)
            {
                savedProfile = profile; savedSession = Session; savedBoard = BoardScreen.Snapshot; savedPersistence = persistProgress;
                savedNotifications = notificationsEnabled; savedMap = IsMap;
                savedBusy = IsBusy; savedIntroduction = IsIntroducing; savedWin = Win.IsOpen;
            }
            // Swap before cancellation: fake presentation cannot alter the real notification ledger.
            profile = new GameProfile { Coins = snapshot?.Hud?.Coins ?? 240, HighestUnlocked = 40 };
            persistProgress = false; notificationsEnabled = false;
            if (snapshot?.Hud != null)
                foreach (var tool in snapshot.Hud.Tools) profile.Boosters[(int)tool.Kind] = tool.Stock ?? 0;
            Session = profile.CreateSession(LevelLoader.Load(snapshot?.LevelId > 0 ? snapshot.LevelId : 1));
            CancelPresentation(); IsMap = false;
            if (audioManager != null)
            {
                audioManager.SetSfxEnabled(!(snapshot?.Muted ?? false)); audioManager.SetMusicEnabled(!(snapshot?.Muted ?? false));
                audioManager.SwitchMusic(MusicPage.Game);
            }
            profile.Muted = snapshot?.Muted ?? false;
        }
        public void EndBoardPreview()
        {
            if (savedProfile == null) return;
            BoardStates.Stop(); CancelPresentation();
            profile = savedProfile; Session = savedSession; persistProgress = savedPersistence;
            notificationsEnabled = savedNotifications; IsMap = savedMap; savedProfile = null; savedSession = null;
            BoardScreen.BindResolutionSnapshot(savedBoard); savedBoard = null;
            if (audioManager != null) { audioManager.SetSfxEnabled(!Muted); audioManager.SetMusicEnabled(!Muted); }
            if (IsMap) ShowMapSurface(); else BoardScreen.Show(BoardSessionAdapter.Capture(this));
            if (!IsMap)
            {
                if (savedBusy && !savedIntroduction) FinishAction();
                else if (savedIntroduction) BeginLevelEntrance();
                else if (savedWin) Win.Show(Session.Stars, 0, 0);
            }
            if (audioManager != null) audioManager.SwitchMusic(IsMap ? MusicPage.Menu : MusicPage.Game);
            if (notificationsEnabled) PumpNotifications();
            Refresh();
        }
        public void PreviewOverlay(string kind, string species = "sunny", BoosterKind booster = BoosterKind.Firefly, int stars = 3, int reward = 30)
        {
            switch (kind)
            {
                case "Settings": OpenSettings(); break;
                case "Goal info": OpenGoalInfo(species); break;
                case "Shop": OpenShop(booster); break;
                case "Win": Win.Show(stars, reward, 0); break;
                case "New friend": panels.Popup(PopupKind.NewFriend, species, 20); break;
                case "Booster unlock": panels.Unlock(booster); break;
                case "World complete": panels.Popup(PopupKind.WorldComplete, species, 200); break;
                case "World unlock": panels.Popup(PopupKind.WorldUnlock, species, 0); break;
                case "Daily gift": panels.Popup(PopupKind.DailyGift, species, 50); break;
            }
            BoardScreen.SetOverlay(new OverlayPresentation { Kind = kind, BlocksInput = true });
        }
        public void PreviewMap()
        {
            if (!IsBoardPreview) return;
            Win.Hide(); panels.Hide(); Shop.Hide(); BoardScreen.HideAndCancel(); ShowMapSurface(); Refresh();
        }
        public void PreviewMap(GameProfile fixture)
        {
            if (fixture == null) throw new ArgumentNullException(nameof(fixture));
            if (!IsBoardPreview) BoardStates.Apply("Data / Typical");
            CancelPresentation();
            profile = fixture; persistProgress = false; notificationsEnabled = false;
            Session = profile.CreateSession(LevelLoader.Load(1));
            if (audioManager != null)
            {
                audioManager.SetSfxEnabled(!Muted); audioManager.SetMusicEnabled(!Muted);
            }
            ShowMapSurface(); Refresh();
        }
        public void PreviewCoins(int coins)
        {
            if (!IsBoardPreview) return;
            profile.Coins = coins;
        }
        public void PreviewNotificationSequence()
        {
            if (!IsBoardPreview) return;
            int token = revision, cursor = 0;
            Action advance = null;
            advance = () =>
            {
                if (!IsBoardPreview || token != revision) return;
                panels.Hide();
                switch (cursor++)
                {
                    case 0: panels.Popup(PopupKind.NewFriend, "sunny", 20, advance); break;
                    case 1: previewNotificationNext = advance; panels.Unlock(BoosterKind.Firefly); break;
                    case 2: panels.Popup(PopupKind.NewFriend, "sky", 20, advance); break;
                    default: previewNotificationNext = null; Refresh(); return;
                }
                BoardScreen.SetOverlay(new OverlayPresentation { BlocksInput = true });
            };
            advance();
        }
        public void PreviewWorldSequence(int world)
        {
            if (!IsBoardPreview) return;
            int token = revision;
            BoardScreen.SetOverlay(new OverlayPresentation { BlocksInput = true });
            panels.Popup(PopupKind.WorldComplete, "sunny", 200, () =>
            {
                if (!IsBoardPreview || token != revision) return;
                PreviewMap();
                if (world == 4) return;
                profile.HighestUnlocked = world * 10 + 1;
                Map.Show(profile);
                Map.UnlockGate(world + 1, () =>
                {
                    if (!IsBoardPreview || token != revision) return;
                    panels.Popup(PopupKind.WorldUnlock, "sunny", 0, () =>
                    {
                        if (!IsBoardPreview || token != revision) return;
                        panels.Hide(); panels.Popup(PopupKind.DailyGift, "sunny", 50, () => { panels.Hide(); Refresh(); }, world + 1);
                        Refresh();
                    }, world + 1);
                    Refresh();
                });
            }, world);
        }
#endif
        private static T Layer<T>(Transform parent, string name) where T : Component =>
            GameUI.Rect(parent, name, GameLayout.DesignSize * 0.5f, GameLayout.DesignSize).gameObject.AddComponent<T>();
        private void Save()
        {
            profile.Capture(Session);
            if (persistProgress) ProgressStore.Save(profile);
        }
        public void LoadLevel(int id)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (IsBoardPreview) { BoardStates.OpenLevel(id); return; }
#endif
            if (!initialized || IsIntroducing || notificationTransition || CurrentNotification != null || id < 1 || id > HighestUnlocked) return;
            LoadDefinition(LevelLoader.Load(id), true);
        }
        private void LoadDefinition(LevelDef level, bool starters)
        {
            bool fromMap = IsMap;
            // Cancel before the wipe starts. Its covered rebuild must not invalidate its own completion.
            CancelPresentation();
            if (fromMap) Map.gameObject.SetActive(true);
            ChangeSurface(() => BuildLevel(level, starters), () =>
            {
                if (starters && notificationsEnabled) BeginLevelEntrance();
            }, fromMap);
        }
        private void BuildLevel(LevelDef level, bool starters)
        {
            rewardGranted = false; pendingWinReward = 0;
            IsMap = false; Map.Hide(); Board.gameObject.SetActive(true); Hud.gameObject.SetActive(true);
            if (audioManager != null) audioManager.SwitchMusic(MusicPage.Game);
            if (starters) profile.GrantStarters(Math.Min(level.id, profile.HighestUnlocked));
            Session = profile.CreateSession(level);
            BoardScreen.Show(BoardSessionAdapter.Capture(this));
            if (starters && notificationsEnabled) BoardScreen.PlayEntrance(new EntrancePresentation { PreparedOnly = true });
            tutorial.Begin(level.id, !profile.Tutorials[level.id], Session.Inventory[BoosterKind.Sapling] > 0);
            // Persist on presentation, not consumption: leaving before the first tap must not repeat it.
            if (tutorial.SaplingPromptPending) profile.Tutorials[level.id] = true;
            if (level.id == profile.Notifications.DeferredLevel) profile.Notifications.TakeDeferredLevel();
            Refresh(); Save();
        }
        private static System.Collections.Generic.IEnumerable<string> LevelSpecies(BoardState state)
        {
            foreach (var branch in state.Branches) foreach (var bird in branch.Birds) yield return bird.Species.Id;
        }
        private void BeginLevelEntrance()
        {
            profile.Notifications.BeginLevel(LevelId, LevelSpecies(Session.State)); IsBusy = true;
            int token = ++revision; var session = Session;
            Save(); Refresh();
            entranceCompleted = () =>
            {
                if (!PresentationCurrent(token) || Session != session) return;
                IsBusy = false; profile.Notifications.FinishIntro(); Save(); PumpNotifications(); Refresh();
            };
            BoardScreen.PlayEntrance(new EntrancePresentation { TransactionId = token });
        }
        public void NextLevel()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (IsBoardPreview) { BoardStates.NextFromWin(); return; }
#endif
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
            persistProgress = false; profile = new GameProfile { Coins = coins, HighestUnlocked = 40 };
            notificationsEnabled = false;
            if (inventory != null) for (int i = 0; i < profile.Boosters.Length; i++) profile.Boosters[i] = inventory[(BoosterKind)i];
            LoadDefinition(level, false);
        }
        private void InputTap()
        {
            revision++; hintPending = false; Board.ClearHints();
            sleepFeedback.Hide();
        }
        private bool ConsumePlaybackTap()
        {
            if (!IsBusy) return false;
            // Coordinator input-lock policy: consume the tap, accelerate, never queue a logical action.
            Board.FastForwardPlayback(); return true;
        }
        public bool TapBranch(int branch)
        {
            if (!initialized || IsMap || Modal || (uint)branch >= Session.State.Branches.Count) return false;
            InputTap(); tutorial.DismissSaplingPrompt();
            if (ConsumePlaybackTap()) return false;
            int source = Session.SelectedBranch;
            if ((source < 0 || source == branch) && sleepFeedback.Refuse(branch))
            {
                Session.CancelSelection(); BoardScreen.SetSelection(new SelectionPresentation()); PlaySound(Sfx.Sleep); Refresh(); return false;
            }
            if (source >= 0 && source != branch) return TryMove(source, branch);
            Session.Select(branch); BoardScreen.SetSelection(BoardSessionAdapter.Selection(Session, Session.SelectedBranch)); PlaySound(Sfx.Select); Refresh();
            return Session.SelectedBranch >= 0;
        }
        public bool TapSwivel(int branch)
        {
            if (!initialized || IsMap || Modal) return false;
            InputTap(); if (ConsumePlaybackTap()) return false;
            if (!Session.Swivel(branch)) return false;
            RememberHintMove(Move.Swivel(branch));
            profile.RecordAction();
            PlayCommitted(); return true;
        }
        public bool TryMove(int from, int to)
        {
            if (!initialized || IsMap || Modal || IsBusy) return false;
            InputTap(); tutorial.DismissSaplingPrompt(); int selected = Session.SelectedBranch;
            if (!Session.TryMove(from, to))
            {
                BoardScreen.SetSelection(new SelectionPresentation());
                if (selected >= 0) { BoardScreen.SetSelection(BoardSessionAdapter.Selection(Session, selected, true)); PlaySound(Sfx.Error); }
                Refresh(); return false;
            }
            profile.RecordAction();
            RememberHintMove(new Move(from, to));
            PlayCommitted(); return true;
        }
        private void PlayCommitted(Action after = null)
        {
            IsBusy = true; tutorial.Hide();
            var events = new GameEvent[Session.Events.Count];
            for (int i = 0; i < events.Length; i++) events[i] = Session.Events[i];
            bool freeMove = BoardScreen.Snapshot.Hud.CountedMoves == Session.MoveCount;
            int previousChain = BoardScreen.Snapshot.Hud.Chain;
            CommitWin(); Save(); Refresh();
            // Input may invalidate a hint while playback still owns its committed completion.
            int token = ++transactionGeneration; var session = Session;
            bool finished = false;
            resolutionCompleted = () =>
            {
                if (this == null || !initialized || !isActiveAndEnabled ||
                    token != transactionGeneration || Session != session || finished) return;
                finished = true;
                FinishAction(); after?.Invoke();
            };
            BoardScreen.PlayResolution(BoardSessionAdapter.Resolution(this, events, BoardSessionAdapter.Capture(this), freeMove, previousChain), token);
        }
        private void CommitWin()
        {
            if (!Session.State.IsWon || rewardGranted) return;
            rewardGranted = true; winCoinStart = Coins;
            pendingWinReward = profile.Reward(LevelId, Session.Stars, Session.State.ChorusCoins);
            profile.Notifications.RecordHomecomings(Session.State.HomedSpecies);
            profile.RecordBestMoves(LevelId, Session.MoveCount);
            profile.Tutorials[LevelId] = true;
        }
        private void FinishAction()
        {
            IsBusy = false; BoardScreen.Restore(BoardSessionAdapter.Capture(this));
            CommitWin();
            int reward = pendingWinReward, before = winCoinStart;
            pendingWinReward = 0; winCountPending = reward > 0;
            if (Session.State.IsWon)
            {
                PlaySound(Sfx.Win);
                int after = Coins, token = revision; bool counted = false;
                Win.Show(Session.Stars, reward, reward > 0 ? Session.State.ChorusCoins : 0, () =>
                {
                    if (reward <= 0 || counted || !PresentationCurrent(token)) return;
                    counted = true; winCountPending = false;
                    Hud.CountCoins(before, after);
                });
            }
            tutorial.ActionCompleted(Session);
            if (tutorial.Completed) profile.Tutorials[LevelId] = true;
            Save(); Refresh();
        }
        public void PresentationInputChanged()
        {
            if (!initialized) return;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (IsBoardPreview) { BoardStates.PresentationChanged(); return; }
#endif
            Refresh();
        }
        public void CancelSelection()
        {
            if (!initialized || IsMap || Modal) return;
            InputTap(); if (ConsumePlaybackTap()) return;
            Session.CancelSelection(); BoardScreen.SetSelection(new SelectionPresentation()); Refresh();
        }
        public bool Undo()
        {
            if (!initialized || IsMap || (Modal && !Win.IsOpen && !IsIntroducing && !IsWiping) || !Session.CanUndo) return false;
            CancelPresentation(); pendingWinReward = 0; Session.Undo(); profile.UndoAction(); BoardScreen.Restore(BoardSessionAdapter.Capture(this));
            lastMove = null;
            tutorial.DisableForAttempt(); PlaySound(Sfx.Undo); Save(); Refresh(); return true;
        }
        public void Restart()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (IsBoardPreview) { BoardStates.Apply("Entrance / Replay"); return; }
#endif
            if (!initialized || IsMap || (notificationTransition && !IsWiping) || CurrentNotification != null) return;
            CancelPresentation(); Session.Restart(); profile.RestartSession(); rewardGranted = false; pendingWinReward = 0; tutorial.DisableForAttempt();
            lastMove = null;
            BoardScreen.Restore(BoardSessionAdapter.Capture(this)); Save(); Refresh();
            if (notificationsEnabled) BeginLevelEntrance();
        }
        public void TapBooster(BoosterKind kind)
        {
            if (!initialized || IsMap || Modal || HighestUnlocked < ViewModel.Unlock(kind)) return;
            if (kind == BoosterKind.Sapling) tutorial.DismissSaplingPrompt();
            InputTap(); if (ConsumePlaybackTap()) return;
            if (Session.Inventory[kind] == 0) OpenShop(kind);
            else if (kind == BoosterKind.Firefly) Hint();
            else UseBooster(kind);
        }
        public bool BuyBooster(BoosterKind kind)
        {
            if (!initialized || IsBusy) return false;
            var item = ShopView.Item(kind);
            var snapshot = PerchSort.Meta.ProfileMigrator.FromLegacy(new System.Collections.Generic.Dictionary<string, string>
            {
                { ProgressStore.CoinsKey, Coins.ToString() },
                { ProgressStore.HighestKey, HighestUnlocked.ToString() }
            }).WithBoosterCount(ShopView.MetaKind(kind), Session.Inventory[kind]);
            var result = PerchSort.Meta.Shop.Purchase(snapshot, item.Id);
            if (!result.Succeeded) return false;
            Session.GrantBooster(kind, 1); profile.SpendCoins(item.Price); Save(); PlaySound(Sfx.Collect); Refresh(); return true;
        }
        public bool BuyCosmetic(string itemId)
        {
            if (!initialized || IsBusy || !PerchSort.Meta.Shop.TryGetItem(itemId, out var item) ||
                item.Kind == PerchSort.Meta.ShopItemKind.Booster) return false;
            var result = PerchSort.Meta.Shop.Purchase(Collection.WithCoins(Coins), itemId);
            if (!result.Succeeded) return false;
            // Development stock may subsidize the display, but never creates real saved coins.
            CommitCosmetic(result.Profile.WithCoins(Math.Max(0, profile.Coins - item.Price)));
            PlaySound(Sfx.Collect); return true;
        }
        public bool ToggleCosmetic(string itemId)
        {
            if (!initialized || IsBusy || !PerchSort.Meta.Shop.TryGetItem(itemId, out var item) ||
                item.Kind == PerchSort.Meta.ShopItemKind.Booster) return false;
            string action = itemId;
            if (item.Kind == PerchSort.Meta.ShopItemKind.Outfit &&
                Collection.EquippedOutfits.TryGetValue(item.SpeciesId, out var equipped) && equipped == itemId)
                action = "outfit." + item.SpeciesId + ".default";
            else if (item.Kind == PerchSort.Meta.ShopItemKind.Birdhouse && Collection.EquippedBirdhouseStyle == item.StyleId)
                action = "house.default";
            var result = PerchSort.Meta.Shop.Equip(Collection.WithCoins(profile.Coins), action);
            if (!result.Succeeded) return false;
            CommitCosmetic(result.Profile); PlaySound(Sfx.Click); return true;
        }
        [Serializable]
        private sealed class CosmeticReceipt { public string rewardLedger; }
        private void CommitCosmetic(PerchSort.Meta.PlayerProfile next)
        {
            // Replace only the ledger property; all pending notification fields remain byte-for-byte intact.
            string snapshot = profile.Notifications.ToText();
            string before = JsonUtility.ToJson(new CosmeticReceipt { rewardLedger = Collection.ToText() });
            string after = JsonUtility.ToJson(new CosmeticReceipt { rewardLedger = next.ToText() });
            profile.Notifications.Restore(snapshot.Replace(before.Substring(1, before.Length - 2), after.Substring(1, after.Length - 2)));
            Save();
            Hud.GoalHouse?.ApplyStyle(next.EquippedBirdhouseStyle);
            Refresh();
        }
        public bool UseBooster(BoosterKind kind)
        {
            if (!initialized || IsMap || Modal || IsBusy || kind == BoosterKind.Firefly || HighestUnlocked < ViewModel.Unlock(kind)) return false;
            if (kind == BoosterKind.Sapling) tutorial.DismissSaplingPrompt();
            InputTap(); if (!Session.UseBooster(kind).Applied) { Refresh(); return false; }
            profile.RecordAction();
            switch (kind)
            {
                case BoosterKind.Sapling: PlaySound(Sfx.Sapling); break;
                case BoosterKind.OwlEye: PlaySound(Sfx.OwlEye); break;
                case BoosterKind.MorningBell: PlaySound(Sfx.MorningBell); break;
                case BoosterKind.Scarecrow: PlaySound(Sfx.Scarecrow); break;
                case BoosterKind.SpringRain: PlaySound(Sfx.SpringRain); break;
            }
            PlayCommitted(); return true;
        }
        public async void Hint()
        {
            if (!initialized || IsMap || Modal || IsBusy || HintPending || HighestUnlocked < 2 || Session.Inventory[BoosterKind.Firefly] == 0) return;
            InputTap(); Session.CancelSelection(); BoardScreen.SetSelection(new SelectionPresentation()); tutorial.Hide();
            int token = revision; var session = Session; var snapshot = session.State.Clone(); hintPending = true;
            Refresh();
            try
            {
                hintSearch = Task.Run(() => Solver.Solve(snapshot, maxStates: 200000, maxMilliseconds: 3000));
                var result = await hintSearch;
                if (this == null || !initialized || token != revision || Session != session) return;
                hintPending = false;
                Move? move = result.Status == SolveStatus.Solved && result.Moves.Count > 0 ? result.Moves[0] : (Move?)null;
                if (!move.HasValue || !session.TryUseFirefly(move.Value)) { Refresh(); return; }
                profile.RecordAction();
                var hint = move.Value;
                PlaySound(Sfx.Hint);
                bool reverse = lastMoveSession == session && lastMove.HasValue && hint.Kind == MoveKind.Transfer &&
                    lastMove.Value.Kind == MoveKind.Transfer && hint.From == lastMove.Value.To && hint.To == lastMove.Value.From;
                PlayCommitted(() =>
                {
                    if (token != revision || Session != session || IsBusy) return;
                    BoardScreen.ShowHint(new HintPresentation { Path = hint });
                });
            }
            catch (Exception error)
            {
                if (this != null && token == revision) { hintPending = false; Debug.LogException(error); Refresh(); }
            }
            finally { if (this != null && initialized) Refresh(); }
        }
        private void RememberHintMove(Move move)
        {
            lastMoveSession = Session; lastMove = move;
            lastMoveReason = Session.State.DayLength > 0 ? "Advance the day/night cycle" : "Bring the flock together";
            foreach (var item in Session.Events)
            {
                if (item is Hatched) { lastMoveReason = "Hatch, then gather"; break; }
                if (item is Woke) { lastMoveReason = "Wake, then gather"; break; }
                if (item is BranchGrew || item is TwigUpgraded) lastMoveReason = "Use the new seats";
            }
        }
        public void OpenSettings()
        {
            if (!initialized || Modal) return;
            InputTap(); if (ConsumePlaybackTap()) return;
            if (!IsMap) { Session.CancelSelection(); BoardScreen.SetSelection(new SelectionPresentation()); }
            panels.Settings(); if (IsMap) PlaySound(Sfx.Click); Refresh();
        }
        public void CloseSettings() { panels.Hide(); PlaySound(Sfx.Back); Refresh(); }
        public void OpenMap()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (IsBoardPreview) { PreviewMap(); return; }
#endif
            if (!initialized || IsAirborne || IsIntroducing || notificationTransition || CurrentNotification != null) return;
            if (TryLeaveFinale(false)) return;
            if (ConsumePlaybackTap()) return;
            bool fromLevel = !IsMap && Board.Branches.Count > 0;
            CancelPresentation(); Save();
            PresentMap(() => { PumpNotifications(); Refresh(); }, fromLevel);
        }
        private void ShowMapSurface()
        {
            IsMap = true; Session.CancelSelection();
            if (audioManager != null) audioManager.SwitchMusic(MusicPage.Menu);
            Board.gameObject.SetActive(false); Hud.gameObject.SetActive(false); tutorial.Hide();
            Map.Show(profile);
        }
        private void PresentMap(Action done, bool animate = true)
        {
            bool change = !IsMap;
            ChangeSurface(() =>
            {
                ShowMapSurface();
                if (notificationsEnabled) profile.Notifications.BeginMap(Clock);
                Save();
            }, done, animate && change);
        }
        private void ChangeSurface(Action covered, Action completed, bool animate)
        {
            if (!animate || !notificationsEnabled || !Application.isPlaying)
            {
                covered(); completed?.Invoke(); return;
            }
            int token = ++revision;
            bool changed = false, finished = false;
            notificationTransition = true; Refresh();
            leafTransition.Play(() =>
            {
                if (!PresentationCurrent(token) || changed) return;
                changed = true; covered();
            }, () =>
            {
                if (!PresentationCurrent(token) || finished) return;
                finished = true;
                notificationTransition = false; completed?.Invoke(); Refresh();
            });
        }
        private bool TryLeaveFinale(bool next)
        {
            if (!notificationsEnabled || !Win.IsOpen || !profile.Notifications.HasWorldSequence) return false;
            profile.Notifications.LeaveWin(LevelId, next);
            Win.Hide(); winCountPending = false; Hud.Cancel();
            Save(); PumpNotifications(); Refresh(); return true;
        }
        private void PumpNotifications()
        {
            if (!notificationsEnabled || !initialized || !isActiveAndEnabled || Modal || IsBusy) return;
            if (!IsMap && profile.Notifications.NeedsMap && !profile.Notifications.HasWorldSequence)
            {
                PresentMap(PumpNotifications); return;
            }
            var request = profile.Notifications.Next(IsMap);
            if (request == null)
            {
                int next = IsMap ? profile.Notifications.DeferredLevel : 0;
                if (next > 0) LoadLevel(next);
                return;
            }
            Save();
            if (request.Kind == NotificationKind.BoosterUnlock) panels.Unlock(request.Booster);
            else if (request.Kind == NotificationKind.WorldUnlock)
            {
                PresentMap(() =>
                {
                    if (!ReferenceEquals(CurrentNotification, request)) return;
                    notificationTransition = true; int token = ++revision; Refresh();
                    Map.UnlockGate(request.World, () =>
                    {
                        if (!PresentationCurrent(token) || !ReferenceEquals(CurrentNotification, request) || !notificationTransition) return;
                        notificationTransition = false;
                        panels.Popup(PopupKind.WorldUnlock, "sunny", 0, () => AcknowledgeNotification(request), request.World);
                        Refresh();
                    });
                });
                return;
            }
            else
            {
                PopupKind kind = request.Kind == NotificationKind.WorldComplete ? PopupKind.WorldComplete :
                    request.Kind == NotificationKind.DailyGift ? PopupKind.DailyGift : PopupKind.NewFriend;
                int reward = kind == PopupKind.WorldComplete ? 200 : kind == PopupKind.DailyGift ? 50 : 20;
                panels.Popup(kind, request.Species ?? "sunny", reward, () => AcknowledgeNotification(request), request.World);
            }
            Refresh();
        }
        public void AcknowledgeNotification(Notification request)
        {
            if (!initialized || request == null || notificationTransition || !panels.IsOpen || !ReferenceEquals(CurrentNotification, request)) return;
            int before = Coins, reward = profile.Notifications.Acknowledge(request, Clock);
            panels.Hide(); Save(); Refresh();
            if (reward > 0 && IsMap)
            {
                notificationTransition = true; int token = ++revision; Refresh();
                Map.CollectCoins(before, Coins, () =>
                {
                    if (!PresentationCurrent(token) || !notificationTransition) return;
                    notificationTransition = false; PumpNotifications(); Refresh();
                });
                return;
            }
            if (reward > 0)
            {
                notificationTransition = true; int token = ++revision; Refresh();
                Hud.CountCoins(before, Coins, () =>
                {
                    if (!PresentationCurrent(token) || !notificationTransition) return;
                    notificationTransition = false; PumpNotifications(); Refresh();
                });
                return;
            }
            PumpNotifications(); Refresh();
        }
        public void OpenLevelCard(int id)
        {
            if (!initialized || !IsMap || Modal || !MapModel.Selectable(profile, id)) return;
            Map.TryGetLevelCardOrigin(id, out Vector2 origin);
            panels.SetLevelCardOrigin(origin);
            panels.LevelCard(id); Refresh();
        }
        public void OpenShop(BoosterKind? focus = null)
        {
            if (!initialized || IsBusy || Modal) return;
            InputTap();
            if (!IsMap) { Session.CancelSelection(); BoardScreen.SetSelection(new SelectionPresentation()); }
            Shop.Show(focus); if (IsMap) PlaySound(Sfx.Click); Refresh();
        }
        public void OpenAviary()
        {
            if (!initialized || IsBusy || Modal) return;
            InputTap();
            if (!IsMap) { Session.CancelSelection(); BoardScreen.SetSelection(new SelectionPresentation()); }
            Aviary.Show(); PlaySound(Sfx.Click); Refresh();
        }
        public void OpenPopup(PopupKind kind, string species = "mint", int reward = 0)
        {
            if (!initialized || IsAirborne || Modal) return;
            if (kind == PopupKind.ShopPlaceholder) { OpenShop(); return; }
            if (kind == PopupKind.AviaryPlaceholder) { OpenAviary(); return; }
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (IsBoardPreview && previewNotificationNext != null)
            {
                var next = previewNotificationNext; previewNotificationNext = null; next(); return;
            }
#endif
            if (!panels.IsUnlock) return;
            if (CurrentNotification != null && CurrentNotification.Kind == NotificationKind.BoosterUnlock)
                AcknowledgeNotification(CurrentNotification);
            else { panels.Hide(); Refresh(); }
        }
        public void ClosePanel()
        {
            if (Shop.IsOpen) Shop.Hide();
            else if (Aviary.IsOpen) { if (Aviary.DetailOpen) { Aviary.Back(); return; } Aviary.Hide(); }
            else
            {
                if (panels.IsUnlock || panels.IsNotification) return;
                if (panels.CloseInformation(() => { panels.Hide(); Refresh(); })) { PlaySound(Sfx.Back); return; }
                panels.Hide();
            }
            PlaySound(Sfx.Back); Refresh();
        }
        public void SetMuted(bool muted)
        {
            profile.Muted = muted; Save();
            if (audioManager != null) { audioManager.SetSfxEnabled(!muted); audioManager.SetMusicEnabled(!muted); }
        }
        public void PlaySound(Sfx sound) { if (audioManager != null) audioManager.Play(sound); }
        private void Refresh()
        {
            bool blocked = IsAirborne || Modal;
            if (!IsMap && !IsBoardPreview && BoardScreen.Snapshot != null)
            {
                BoardScreen.UpdateHud(BoardSessionAdapter.CaptureHud(this, BoardScreen.Snapshot, pendingWinReward > 0 || winCountPending ? winCoinStart : Coins));
                boardOverlay.BlocksInput = Modal && !IsIntroducing; BoardScreen.SetOverlay(boardOverlay);
                BoardScreen.SetInput(BoardSessionAdapter.Input(this, BoardScreen.Snapshot.Input));
                tutorial.Refresh(Session, Board, IsBusy || Modal);
                StuckHint.Observe(this);
            }
            else if (IsMap) { tutorial.Hide(); Map.RefreshCoins(Coins); }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            else if (IsBoardPreview) BoardStates.RefreshOverlay(Modal);
#endif
            SetRaycasts(mapInput, !Modal && IsMap);
        }
        private static void SetRaycasts(CanvasGroup group, bool enabled) => group.blocksRaycasts = group.interactable = enabled;
        private bool PresentationCurrent(int token) => this != null && initialized && isActiveAndEnabled && token == revision;
        private void CancelPresentation()
        {
            revision++; transactionGeneration++; hintPending = false; winCountPending = false;
            entranceCompleted = resolutionCompleted = null;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            previewNotificationNext = null;
#endif
            profile.Notifications.CancelPresentation(); notificationTransition = false; leafTransition.Cancel();
            BoardScreen.HideAndCancel(); Map.Hide(); tutorial.Hide(); Win.Hide(); panels.Hide(); Shop.Hide(); Aviary.Hide(); Scene.Cancel(); IsBusy = false;
        }
        private void OnApplicationFocus(bool focus)
        {
            if (!initialized || IsMap) return;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (IsBoardPreview)
            {
                if (focus) BoardStates.Reapply();
                else BoardScreen.SetLifecycle(false, GameMotion.Instant ? BoardMotionMode.Instant : BoardMotionMode.Normal);
                return;
            }
#endif
            if (!focus)
            {
                focusEntrance = IsIntroducing; focusResolution = IsBusy && !focusEntrance;
                revision++; transactionGeneration++; entranceCompleted = resolutionCompleted = null; IsBusy = false;
                BoardScreen.SetLifecycle(false, GameMotion.Instant ? BoardMotionMode.Instant : BoardMotionMode.Normal);
            }
            else
            {
                BoardScreen.Show(BoardSessionAdapter.Capture(this));
                if (focusResolution) FinishAction();
                else if (focusEntrance) BeginLevelEntrance();
                focusEntrance = focusResolution = false; Refresh();
            }
        }
        private void OnDisable()
        {
            if (!initialized) return;
            suspended = true; resumeWin = Win.IsOpen;
            Save(); CancelPresentation();
        }
        private void OnEnable()
        {
            if (!initialized || !suspended) return;
            suspended = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (IsBoardPreview) { BoardStates.Reapply(); return; }
#endif
            if (IsMap)
            {
                ShowMapSurface();
                if (notificationsEnabled) { profile.Notifications.BeginMap(Clock); Save(); PumpNotifications(); }
            }
            else
            {
                BoardScreen.Restore(BoardSessionAdapter.Capture(this));
                if (Session.State.IsWon && (!rewardGranted || pendingWinReward > 0)) FinishAction();
                else if (resumeWin) Win.Show(Session.Stars, 0, 0);
                else if (notificationsEnabled && (profile.Notifications.NeedsMap || profile.Notifications.HasWorldSequence) && Session.State.IsWon)
                    PumpNotifications();
                else if (notificationsEnabled && !Session.State.IsWon) BeginLevelEntrance();
                else if (Session.State.IsWon) Win.Show(Session.Stars, 0, 0);
            }
            Refresh();
        }
        private void OnDestroy()
        {
            if (initialized && !suspended) CancelPresentation();
            initialized = false; revision++; transactionGeneration++; art?.Dispose();
            if (audioManager == null) return;
            if (ownsAudio) GameUI.Dispose(audioManager.gameObject);
            else
            {
                audioManager.SetSfxEnabled(previousSfx); audioManager.SetMusicEnabled(previousMusicEnabled);
                audioManager.SwitchMusic(previousMusic);
            }
        }
    }
}
