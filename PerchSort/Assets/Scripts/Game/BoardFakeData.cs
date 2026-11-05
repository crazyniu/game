#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using PerchSort.Core;
using UnityEngine;
using Kind = PerchSort.Game.BoardResolutionEvent.EventKind;

namespace PerchSort.Game
{
    // Authored display fixtures, not another game. No solver, rules, save reads, or save writes.
    public sealed class BoardFakeData : IScreenStates
    {
        private sealed class Fixture
        {
            public BoardSnapshot Initial = Build();
            public BoardSnapshot Final;
            public readonly List<BoardResolutionEvent> Events = new List<BoardResolutionEvent>();
            public SelectionPresentation Selection;
            public HintPresentation Hint;
            public BubblePresentation Bubble;
            public TutorialPresentation Tutorial;
            public EntrancePresentation Entrance;
            public string Overlay, Species = "sunny";
            public BoosterKind Booster;
            public int Stars = 3, Reward = 30;
            public Action AfterShow;
        }
        private readonly GameRoot root;
        private readonly IBoardView view;
        private readonly GameMotion timeline;
        private readonly Dictionary<string, Action<Fixture>> states = new Dictionary<string, Action<Fixture>>(StringComparer.Ordinal);
        private readonly List<string> names = new List<string>();
        private BoardSnapshot current, afterResolution, beforeAction;
        private string currentName;
        private long transaction = long.MinValue / 2;
        private int selected = -1;
        private int? levelOverride;
        private bool resolving, entrancePlaying, overlayOpen;
        public bool Active { get; private set; }
        public string ScreenName => "Board";
        public IReadOnlyList<string> StateNames => names;
        public string CurrentState => currentName;
        public BoardFakeData(GameRoot owner, IBoardView board)
        {
            root = owner; view = board; timeline = owner.gameObject.AddComponent<GameMotion>(); RegisterStates();
        }
        private void Add(string name, Action<Fixture> setup) { names.Add(name); states.Add(name, setup); }
        public void Apply(string stateName)
        {
            if (!states.TryGetValue(stateName, out var setup)) throw new ArgumentException("Unknown board state: " + stateName);
            timeline.CancelAll(); transaction++; selected = -1; resolving = entrancePlaying = overlayOpen = false;
            var fixture = new Fixture(); setup(fixture); currentName = stateName;
            if (levelOverride.HasValue && fixture.Initial != null) fixture.Initial.LevelId = levelOverride.Value;
            Active = true; root.BeginBoardPreview(fixture.Initial); current = fixture.Initial;
            view.Show(current);
            if (current == null || current.RequiredKey != null || current.Goals.Length == 0) return;
            if (fixture.Selection != null) { selected = 0; view.SetSelection(fixture.Selection); }
            if (fixture.Hint != null) view.ShowHint(fixture.Hint);
            if (fixture.Bubble != null) view.ShowBubble(fixture.Bubble);
            if (fixture.Tutorial != null) view.SetTutorial(fixture.Tutorial);
            if (fixture.Events.Count > 0)
            {
                beforeAction = current; afterResolution = fixture.Final ?? current; resolving = true;
                view.SetInput(new InputPresentation { BoardEnabled = true, HudEnabled = true, AllowDeferred = true });
                view.PlayResolution(fixture.Events, transaction);
            }
            if (fixture.Entrance != null)
            {
                entrancePlaying = !fixture.Entrance.PreparedOnly; fixture.Entrance.TransactionId = transaction;
                view.SetInput(new InputPresentation { Entrance = true }); view.PlayEntrance(fixture.Entrance);
            }
            if (fixture.Overlay != null)
            {
                overlayOpen = true; root.PreviewOverlay(fixture.Overlay, fixture.Species, fixture.Booster, fixture.Stars, fixture.Reward);
            }
            fixture.AfterShow?.Invoke();
        }
        public void Stop() { Active = false; timeline.CancelAll(); transaction++; current = afterResolution = beforeAction = null; }
        public void Reapply() { if (Active) Apply(currentName); }
        public void OpenLevel(int id)
        {
            levelOverride = id;
            try { Apply("Entrance / First play"); }
            finally { levelOverride = null; }
        }
        public void NextFromWin()
        {
            if (currentName == "Recovery / World ending win") root.PreviewWorldSequence(1);
            else if (currentName == "Recovery / Final level return") root.PreviewWorldSequence(4);
            else OpenLevel(Math.Min(40, (current?.LevelId ?? 1) + 1));
        }
        public void EntranceFinished(long id)
        {
            if (!Active || id != transaction) return;
            entrancePlaying = false; view.SetInput(ReadyInput());
        }
        public void ResolutionFinished(long id)
        {
            if (!Active || id != transaction) return;
            resolving = false; current = afterResolution ?? current; view.Restore(current); view.SetInput(ReadyInput());
        }
        private InputPresentation ReadyInput() => new InputPresentation { BoardEnabled = true, HudEnabled = true };
        public void PresentationChanged()
        {
            if (!Active) return;
            view.SetInput(new InputPresentation { BoardEnabled = !entrancePlaying, HudEnabled = !entrancePlaying,
                Entrance = entrancePlaying, Airborne = root.Board.IsAirborne, AllowDeferred = resolving && !root.Board.IsAirborne });
        }
        public void RefreshOverlay(bool blocked)
        {
            if (!Active) return;
            view.SetOverlay(new OverlayPresentation { BlocksInput = blocked });
            if (overlayOpen && !blocked && current != null)
            {
                overlayOpen = false; current.Hud.Coins = root.Coins;
                foreach (var tool in current.Hud.Tools) { tool.Stock = root.BoosterStock(tool.Kind); tool.ZeroPlus = tool.Stock == 0; }
                view.UpdateHud(current.Hud); view.SetInput(ReadyInput());
            }
        }
        public void Handle(BoardAction action)
        {
            if (!Active || current == null) return;
            switch (action.Kind)
            {
                case "undo":
                    if (beforeAction != null) { current = beforeAction; beforeAction = null; current.Hud.UndoAvailable = false; view.Restore(current); }
                    else Apply("Access / Undo hidden after final undo");
                    break;
                case "cancel": selected = -1; view.SetSelection(new SelectionPresentation()); break;
                case "swivel": Apply("Selection / Swivel drawn hop"); break;
                case "tool":
                    if (action.Booster == BoosterKind.Firefly) Apply("Hints / Successful transfer hint");
                    else Apply("Tools / " + action.Booster + " applied");
                    break;
                case "branch":
                    if (currentName.Contains("Sleeping") || currentName.Contains("sleep"))
                    {
                        view.ShowBubble(Sleep(current, "dial_moon")); root.PlaySound(PerchSort.Audio.Sfx.Error); break;
                    }
                    if (action.BranchId == selected) { selected = -1; view.SetSelection(new SelectionPresentation()); break; }
                    if (selected == 0 && action.BranchId == 3) { Apply("Selection / Legal transfer"); break; }
                    if (selected >= 0) { Apply("Selection / Illegal wrong species"); break; }
                    if (action.BranchId >= current.Branches.Length || current.Branches[action.BranchId].Birds.Length == 0) break;
                    selected = action.BranchId;
                    var branch = current.Branches[selected];
                    view.SetSelection(new SelectionPresentation { BirdIds = new[] { branch.Birds[branch.Birds.Length - 1].Bird.Id } });
                    break;
            }
        }
        private void RegisterStates()
        {
            Add("Data / Empty start - no snapshot", f => f.Initial = null);
            Add("Data / Typical", f => { });
            Add("Data / Empty branch", f => f.Initial.Branches[3].Birds = Array.Empty<BoardSnapshot.BirdData>());
            Add("Data / Empty doorway - zero deliveries", f => { f.Initial.ResidentSpecies = null; f.Initial.Hud.ResidentSpecies = null; });
            foreach (int value in new[] { 0, 240, 99999, int.MaxValue })
            { int n = value; Add("Numbers / Coins " + n, f => f.Initial.Hud.Coins = n); }
            foreach (int count in new[] { 2, 7 })
            { int n = count; Add("Data / " + n + " species", f => f.Initial = Build(n == 7 ? 5 : 2, n)); }
            foreach (int rows in new[] { 2, 3, 4, 5 })
            { int n = rows; Add("Data / " + n + " rows", f => f.Initial = Build(n, n == 2 ? 2 : 7)); }
            Add("Data / Odd branch count", f => f.Initial = Build(3, 4, 5));
            Add("Data / All authored seats occupied", f => { f.Initial = Build(5, 7); FillSeats(f.Initial, 4); });
            Add("Data / Added branch", f => f.Initial = Build(3, 3, 5));
            Add("Data / Eleventh branch - six rows", f => f.Initial = Build(6, 7, 11));
            Add("Data / Zero species - unavailable", f => f.Initial.Goals = Array.Empty<BoardSnapshot.Goal>());
            Add("Data / All home with crow remaining", f => { HomeAll(f.Initial); SetBirds(f.Initial, 3, "crow"); });
            foreach (int value in new[] { 0, 7, 8, 9, 11, 12, 999, int.MaxValue })
            { int n = value; Add("Numbers / Counted moves " + n, f => { f.Initial.Hud.CountedMoves = n; f.Initial.Hud.ReachableStars = n <= 8 ? 3 : n <= 11 ? 2 : 1; }); }
            Add("Numbers / Star limits 19 and 22", f => { f.Initial.Hud.ThreeStarLimit = 19; f.Initial.Hud.TwoStarLimit = 22; });
            Add("Numbers / Coin one to zero", f => f.AfterShow = () => root.Hud.CountCoins(1, 0));
            Add("Numbers / Coin gain", f => f.AfterShow = () => root.Hud.CountCoins(240, 290));
            foreach (int value in new[] { 1, 2, 3 })
            { int n = value; Add("Numbers / Phase countdown " + n, f => { f.Initial.Hud.PhaseEnabled = true; f.Initial.Hud.PhaseMovesLeft = n; }); }
            foreach (int value in new[] { 0, 1, 2, 3, 7 })
            { int n = value; Add("Numbers / Chain " + n, f => f.Initial.Hud.Chain = n); }
            foreach (int value in new[] { 0, 1, 99999, int.MaxValue })
            { int n = value; Add("Numbers / Inventory " + n, f => { foreach (var t in f.Initial.Hud.Tools) { t.Stock = n; t.ZeroPlus = n == 0; } }); }
            Add("Numbers / Inventory one to zero", f => f.AfterShow = () => root.Hud.SetStock(BoosterKind.Firefly, 0));
            Add("Numbers / Inventory gain", f => f.AfterShow = () => root.Hud.SetStock(BoosterKind.Firefly, 4));
            Add("Numbers / Longest name Flurry", f => { f.Initial = Build(2, 2, 4, new[] { "snow", "sky" }); f.Overlay = "Goal info"; f.Species = "snow"; });
            Add("Numbers / Expanded localization boundary", f => { f.Overlay = "Goal info"; });
            Add("Numbers / Missing baked label", f => f.Initial.RequiredKey = "labels/name_sunny_missing");
            Add("Access / First visit before tools unlock", f => { foreach (var t in f.Initial.Hud.Tools) t.Visible = false; });
            Add("Access / Later visit", f => f.Initial.Hud.UndoAvailable = true);
            Add("Access / Undo hidden at start", f => f.Initial.Hud.UndoAvailable = false);
            Add("Access / Undo shown after move", f => f.Initial.Hud.UndoAvailable = true);
            Add("Access / Undo hidden after final undo", f => f.Initial.Hud.UndoAvailable = false);
            Add("Access / Three slots with Undo hidden", f => f.Initial.Hud.UndoAvailable = false);
            Add("Access / Four slots with Undo hidden", f => SetContext(f.Initial, BoosterKind.Scarecrow));
            Add("Access / Sapling already used", f => f.Initial.Hud.Tools[1].Enabled = false);
            Add("Access / Context absent", f => { });
            Add("Access / Context present", f => SetContext(f.Initial, BoosterKind.OwlEye));
            foreach (BoosterKind value in Enum.GetValues(typeof(BoosterKind)))
            {
                BoosterKind kind = value;
                Add("Tools / " + kind + " locked", f => { var tool = Tool(f.Initial, kind); tool.Visible = false; });
                Add("Tools / " + kind + " owned", f => { var tool = Tool(f.Initial, kind); tool.Stock = 3; tool.ZeroPlus = false; });
                Add("Tools / " + kind + " zero plus", f => { var tool = Tool(f.Initial, kind); tool.Stock = 0; tool.ZeroPlus = true; });
                Add("Tools / " + kind + " no effect disabled", f => Tool(f.Initial, kind).Enabled = false);
                Add("Tools / " + kind + " applied", f =>
                {
                    var tool = Tool(f.Initial, kind); tool.Stock = 1; tool.ZeroPlus = false;
                    f.Final = Copy(f.Initial); Tool(f.Final, kind).Stock = 0; Tool(f.Final, kind).ZeroPlus = true;
                    f.Final.Hud.UndoAvailable = true;
                    f.Events.Add(new BoardResolutionEvent { Kind = Kind.Booster, Booster = kind, Remaining = 0, Snapshot = f.Final });
                });
            }
            foreach (string state in new[] { "Pending", "No solution", "Timeout unknown", "Failed request" })
            { string name = state; Add("Hints / " + name, f => { Tool(f.Initial, BoosterKind.Firefly).Enabled = false; f.Hint = new HintPresentation { ReasonIcon = name == "Pending" ? "tool_hint" : "icon_undo" }; }); }
            Add("Hints / Successful transfer hint", f => f.Hint = new HintPresentation { Path = new Move(0, 3) });
            Add("Hints / Swivel hint", f => { f.Initial.Branches[0].Kind = BranchKind.Swivel; f.Hint = new HintPresentation { Path = Move.Swivel(0) }; });
            Add("Hints / Stuck no legal move", f => f.Hint = new HintPresentation { Stuck = true });
            Add("Hints / Stuck bubble dismissed", f => f.AfterShow = () => view.ShowBubble(null));
            Add("Selection / Idle", f => { });
            Add("Selection / Selected exposed run", f => { SetBirds(f.Initial, 0, "sky", "sky"); f.Selection = Select(f.Initial, 0); });
            Add("Selection / Cancel same branch", f => { f.Selection = Select(f.Initial, 0); f.AfterShow = () => Delay(0.6f, () => view.SetSelection(new SelectionPresentation())); });
            Add("Selection / Cancel empty space", f => { f.Selection = Select(f.Initial, 0); f.AfterShow = () => Delay(0.6f, () => view.SetSelection(new SelectionPresentation())); });
            Add("Selection / Empty tip", f => f.Initial.Branches[0].Birds = Array.Empty<BoardSnapshot.BirdData>());
            Add("Selection / Egg tip", f => SetBirds(f.Initial, 0, "egg:sky"));
            Add("Selection / Sleeping bird tapped with bubble", f => { f.Initial = Build(2, 2, 4, new[] { "ink", "sky" }); SetBirds(f.Initial, 0, "ink:sleep"); f.Bubble = Sleep(f.Initial, "dial_moon"); });
            Add("Selection / Legal transfer", f => Transfer(f, 0, 3, 1));
            Add("Selection / Legal partial transfer", f => { SetBirds(f.Initial, 0, "sky", "sky", "sky"); SetBirds(f.Initial, 3, "sky", "sky"); Transfer(f, 0, 3, 2); });
            Add("Selection / Legal full transfer", f => { SetBirds(f.Initial, 0, "sky", "sky"); Transfer(f, 0, 3, 2); });
            foreach (string reason in new[] { "wrong species", "full target", "egg blocked target" })
            {
                string name = reason;
                Add("Selection / Illegal " + name, f =>
                {
                    if (name == "full target") SetBirds(f.Initial, 1, "sunny", "sunny", "sunny", "sunny");
                    if (name == "egg blocked target") SetBirds(f.Initial, 1, "egg:sky");
                    f.Selection = Select(f.Initial, 0); f.Selection.Refused = true;
                });
            }
            Add("Selection / Rapid repeated taps", f => { Transfer(f, 0, 3, 1); f.AfterShow = () => { root.BoardScreen.RequestBranch(1); root.BoardScreen.RequestBranch(2); }; });
            Add("Selection / In flight blocked input", f => Transfer(f, 0, 3, 1));
            Add("Selection / One queued landing tap", f => { Transfer(f, 0, 3, 1); f.AfterShow = () => Delay(0.5f, () => root.BoardScreen.RequestBranch(0)); });
            Add("Selection / Rejected stale queued action", f => { Transfer(f, 0, 3, 1); f.AfterShow = () => Delay(0.15f, () => { view.Restore(f.Initial); transaction++; resolving = false; }); });
            Add("Selection / Swivel drawn hop", f => Swivel(f));
            Add("Resolution / Takeoff flight landing", f => Transfer(f, 0, 3, 1));
            Add("Resolution / Flock home badge lighting", f => Deliver(f, "sky"));
            Add("Resolution / Multiple flocks cascade", f => Deliver(f, "sky", true));
            Add("Resolution / All home", f => HomeAll(f.Initial));
            Add("Resolution / Undo last delivery empty house", f => { f.Initial.Hud.UndoAvailable = true; });
            Add("Resolution / Undo delivery another species home", f => { f.Initial.Goals[1].Home = true; f.Initial.ResidentSpecies = f.Initial.Hud.ResidentSpecies = "sky"; f.Initial.Hud.UndoAvailable = true; });
            foreach (string species in Species)
            {
                string id = species;
                Add("Gifts / " + id, f => { f.Initial = Build(2, 2, 4, new[] { id, id == "sky" ? "sunny" : "sky" }); Deliver(f, id); });
            }
            Add("Gifts / No target", f => { Deliver(f, "sky"); f.Events[f.Events.Count - 1].Targets = new[] { new Vector2(540, 360) }; });
            foreach (string stage in new[] { "grant", "consume", "undo" })
            { string name = stage; Add("Gifts / Light step " + name, f => { f.Initial.Hud.LightStep = name != "consume"; f.Initial.Hud.FreeMove = name == "consume"; }); }
            Add("Gifts / Berry free transfer", f => { f.Initial.Hud.FreeMove = true; Transfer(f, 0, 3, 1, true); });
            Add("Gifts / Berry free swivel", f => { f.Initial.Hud.FreeMove = true; Swivel(f); });
            Add("Mechanics / Mystery egg", f => SetBirds(f.Initial, 0, "egg:sky"));
            Add("Mechanics / Revealed egg", f => { SetBirds(f.Initial, 0, "egg:sky"); SetFlags(f.Initial, 0, BirdFlags.Egg | BirdFlags.RevealedEgg); });
            Add("Mechanics / Egg wobble", f => { SetBirds(f.Initial, 0, "egg:sky"); BirdEvent(f, Kind.EggWobble); });
            Add("Mechanics / Egg hatch", f => { SetBirds(f.Initial, 0, "egg:sky"); f.Final = Copy(f.Initial); SetFlags(f.Final, 0, BirdFlags.None); BirdEvent(f, Kind.Hatch); });
            Add("Mechanics / Authored sleeper", f => SetBirds(f.Initial, 0, "sunny:sleep"));
            Add("Mechanics / Nocturnal day sleeper", f => { f.Initial = Build(2, 2, 4, new[] { "ink", "sky" }); SetBirds(f.Initial, 0, "ink"); Sleeping(f.Initial, 0, true); f.Initial.Hud.PhaseEnabled = true; });
            Add("Mechanics / Both sleep causes", f => { f.Initial = Build(2, 2, 4, new[] { "ink", "sky" }); SetBirds(f.Initial, 0, "ink:sleep"); f.Initial.Hud.PhaseEnabled = true; f.Bubble = Sleep(f.Initial, "dial_moon", "gift_sun_note"); });
            Add("Mechanics / Wake still nocturnally asleep", f => { f.Initial = Build(2, 2, 4, new[] { "ink", "sky" }); f.Initial.Hud.PhaseEnabled = true; SetBirds(f.Initial, 0, "ink:sleep"); f.Final = Copy(f.Initial); SetFlags(f.Final, 0, BirdFlags.None); Sleeping(f.Final, 0, true); BirdEvent(f, Kind.Wake); });
            Add("Mechanics / Mama following chicks", f => { SetBirds(f.Initial, 0, "sky:chick", "sky:chick", "sky:mama"); Transfer(f, 0, 3, 3); f.Events[0].Kind = Kind.Follow; });
            Add("Mechanics / Family target capacity reached", f => { SetBirds(f.Initial, 0, "sky:chick", "sky:chick", "sky:mama"); SetBirds(f.Initial, 3, "sky", "sky"); Transfer(f, 0, 3, 2); f.Events[0].Kind = Kind.Follow; });
            Add("Mechanics / Crow parking", f => { SetBirds(f.Initial, 0, "crow"); f.Initial.Branches[0].Kind = BranchKind.Twig; f.Initial.Branches[0].Capacity = 2; });
            Add("Mechanics / Crow departure", f => { SetBirds(f.Initial, 0, "crow"); f.Final = Copy(f.Initial); f.Final.Branches[0].Birds = Array.Empty<BoardSnapshot.BirdData>(); f.Events.Add(new BoardResolutionEvent { Kind = Kind.CrowsLeave, From = 0, BirdIds = Ids(f.Initial.Branches[0]), Snapshot = f.Final }); });
            Add("Mechanics / Twig", f => { SetBirds(f.Initial, 0, "sunny", "sky"); f.Initial.Branches[0].Kind = BranchKind.Twig; f.Initial.Branches[0].Capacity = 2; });
            Add("Mechanics / Partial growing branch", f => { f.Initial.Branches[3].Kind = BranchKind.Growing; f.Initial.Branches[3].Capacity = 2; });
            Add("Mechanics / Full growing branch", f => { f.Initial.Branches[3].Kind = BranchKind.Growing; f.Initial.Branches[3].Capacity = 4; });
            Add("Mechanics / Upgraded twig", f => { f.Initial.Branches[3].Kind = BranchKind.Twig; f.Initial.Branches[3].Capacity = 2; f.Final = Copy(f.Initial); f.Final.Branches[3].Kind = BranchKind.Normal; f.Final.Branches[3].Capacity = 4; f.Events.Add(new BoardResolutionEvent { Kind = Kind.TwigUpgrade, From = 3, Seats = 4, Snapshot = f.Final }); });
            Add("Mechanics / Swivel", f => f.Initial.Branches[0].Kind = BranchKind.Swivel);
            Add("Phase / Disabled", f => f.Initial.Hud.PhaseEnabled = false);
            foreach (DayPhase value in Enum.GetValues(typeof(DayPhase)))
            { DayPhase phase = value; Add("Phase / " + phase, f => { f.Initial.Hud.PhaseEnabled = true; f.Initial.Hud.Phase = phase; f.Initial.Hud.PhaseMovesLeft = 2; }); }
            Add("Phase / One move until flip", f => { f.Initial.Hud.PhaseEnabled = true; f.Initial.Hud.PhaseMovesLeft = 1; });
            foreach (string cause in new[] { "Clock flip reset", "Gift triggered flip", "Moonbeam expiry" })
            { string name = cause; Add("Phase / " + name, f => { f.Initial.Hud.PhaseEnabled = true; f.Initial.Hud.PhaseMovesLeft = 1; f.Final = Copy(f.Initial); f.Final.Hud.Phase = DayPhase.Night; f.Final.Hud.PhaseMovesLeft = 3; f.Events.Add(new BoardResolutionEvent { Kind = Kind.Phase, Phase = DayPhase.Night, Snapshot = f.Final }); }); }
            Add("Phase / Moonbeam active", f => { f.Initial = Build(2, 2, 4, new[] { "hoot", "sky" }); f.Initial.Hud.PhaseEnabled = true; f.Initial.Hud.MoonbeamActive = true; });
            foreach (string state in new[] { "Day", "Night", "Moonbeam", "Light step" })
            { string name = state; Add("Phase / Undo restoring " + name, f => { f.Initial.Hud.UndoAvailable = true; f.Initial.Hud.PhaseEnabled = name != "Light step"; f.Initial.Hud.Phase = name == "Night" ? DayPhase.Night : DayPhase.Day; f.Initial.Hud.MoonbeamActive = name == "Moonbeam"; f.Initial.Hud.LightStep = name == "Light step"; }); }
            foreach (string phase in new[] { "Prepared covered", "House", "Branches", "Birds", "Settle controls", "Ready", "First play", "Replay" })
            {
                string name = phase;
                Add("Entrance / " + name, f => { if (name != "Ready") f.Entrance = new EntrancePresentation { PreparedOnly = name == "Prepared covered", StartAtSeconds = name == "House" ? 0.2f : name == "Branches" ? 1 : name == "Birds" ? 1.7f : name == "Settle controls" ? 3.2f : 0 }; });
                Add("Entrance / Skip " + name, f => { f.Entrance = new EntrancePresentation(); f.AfterShow = () => Delay(PhaseDelay(name), view.SkipEntrance); });
                Add("Entrance / Cancel " + name, f => { f.Entrance = new EntrancePresentation(); f.AfterShow = () => Delay(PhaseDelay(name), () => { transaction++; entrancePlaying = false; view.Restore(current); }); });
                Add("Entrance / Focus loss " + name, f => { f.Entrance = new EntrancePresentation(); f.AfterShow = () => Delay(PhaseDelay(name), () => view.SetLifecycle(false, BoardMotionMode.Normal)); });
            }
            Add("Entrance / Repeated skip", f => { f.Entrance = new EntrancePresentation(); f.AfterShow = () => Delay(0.3f, () => { view.SkipEntrance(); view.SkipEntrance(); }); });
            Add("Entrance / Instant motion", f => { f.Entrance = new EntrancePresentation(); f.AfterShow = () => { view.SetLifecycle(true, BoardMotionMode.Instant); view.PlayEntrance(new EntrancePresentation { TransactionId = transaction }); }; });
            Add("Entrance / Reduced motion", f => { f.AfterShow = () => { view.SetLifecycle(true, BoardMotionMode.Reduced); view.PlayEntrance(new EntrancePresentation { TransactionId = transaction }); }; });
            foreach (int level in new[] { 1, 2, 3, 6, 11, 15, 21, 24, 31, 33 })
            {
                int id = level;
                Add("Tutorial / Level " + id + " source", f => { f.Initial.LevelId = id; f.Tutorial = Target(f.Initial, 0, 1); });
                Add("Tutorial / Level " + id + " target", f => { f.Initial.LevelId = id; f.Selection = Select(f.Initial, 0); f.Tutorial = Target(f.Initial, 3, 0); });
            }
            Add("Tutorial / Level 1 second source", f => f.Tutorial = Target(f.Initial, 2, 2));
            Add("Tutorial / Level 1 second target", f => f.Tutorial = Target(f.Initial, 0, 0));
            Add("Tutorial / Level 2 hint prompt", f => f.Tutorial = new TutorialPresentation { Visible = true, TargetKind = "tool", Point = GameLayout.ToolButton(1).center });
            Add("Tutorial / Level 4 owned Sapling prompt", f => { f.Initial.LevelId = 4; f.Tutorial = new TutorialPresentation { Visible = true, TargetKind = "tool", Point = GameLayout.ToolButton(2).center }; });
            foreach (string result in new[] { "Expected action", "Off script action", "Undo", "Restart", "Completion" })
            { string name = result; Add("Tutorial / " + name, f => { f.Tutorial = Target(f.Initial, 0, 1); f.AfterShow = () => Delay(0.7f, () => view.SetTutorial(new TutorialPresentation { Completed = true })); }); }
            Add("Tutorial / Modal suppression", f => { f.Tutorial = Target(f.Initial, 0, 1); f.Overlay = "Settings"; });
            Add("Recovery / Stuck is not loss", f => f.Hint = new HintPresentation { Stuck = true });
            foreach (int count in new[] { 1, 2, 3 })
            { int stars = count; Add("Recovery / " + stars + " star clear", f => { HomeAll(f.Initial); f.Initial.Hud.ReachableStars = stars; f.Overlay = "Win"; f.Stars = stars; }); }
            foreach (string outcome in new[] { "Same star replay", "Upgraded star top up", "World ending win", "Final level return" })
            {
                string name = outcome;
                Add("Recovery / " + name, f =>
                {
                    if (name == "Final level return") { f.Initial = Build(5, 7); f.Initial.World = 4; f.Initial.Variant = "aurora"; }
                    HomeAll(f.Initial); f.Overlay = "Win";
                    f.Initial.LevelId = name == "Final level return" ? 40 : name == "World ending win" ? 10 : 1;
                    f.Stars = name == "Same star replay" ? 2 : 3;
                    f.Reward = name == "Same star replay" ? 5 : name == "Upgraded star top up" ? 10 : 30;
                });
            }
            Add("Surfaces / Settings", f => f.Overlay = "Settings");
            foreach (string value in Species)
            {
                string id = value;
                foreach (bool home in new[] { false, true })
                { bool delivered = home; Add("Surfaces / Goal info " + id + (delivered ? " home" : " pending"), f => { f.Initial = Build(2, 2, 4, new[] { id, id == "sky" ? "sunny" : "sky" }); f.Initial.Goals[0].Home = delivered; f.Overlay = "Goal info"; f.Species = id; }); }
            }
            foreach (BoosterKind value in Enum.GetValues(typeof(BoosterKind)))
            {
                BoosterKind kind = value;
                Add("Surfaces / Shop focus " + kind, f => { Tool(f.Initial, kind).Stock = 0; Tool(f.Initial, kind).ZeroPlus = true; f.Overlay = "Shop"; f.Booster = kind; });
                Add("Surfaces / Booster unlock " + kind, f => { f.Overlay = "Booster unlock"; f.Booster = kind; });
            }
            foreach (string status in new[] { "Affordable", "Unaffordable", "Cancel", "Success" })
            {
                string name = status;
                Add("Surfaces / Shop confirmation " + name, f =>
                {
                    f.Initial.Hud.Coins = name == "Unaffordable" ? 0 : 240; f.Overlay = "Shop";
                    f.AfterShow = () =>
                    {
                        if (name == "Unaffordable") root.PreviewCoins(240);
                        root.Shop.TapCard(BoosterKind.Firefly);
                        if (name == "Unaffordable") root.PreviewCoins(0);
                        if (name == "Cancel") root.Shop.CloseConfirm();
                        else if (name == "Success") PressPurchase();
                    };
                });
            }
            Add("Surfaces / New friend single", f => f.Overlay = "New friend");
            Add("Surfaces / Notifications multiple after entrance", f => { f.Entrance = new EntrancePresentation(); f.AfterShow = () => Delay(4.3f, () => root.PreviewNotificationSequence()); });
            Add("Surfaces / World complete", f => f.Overlay = "World complete");
            Add("Surfaces / World unlock", f => f.Overlay = "World unlock");
            Add("Surfaces / Map return", f => f.AfterShow = root.PreviewMap);
            Add("Surfaces / Daily gift map return", f => f.AfterShow = () => { root.PreviewMap(); root.PreviewOverlay("Daily gift"); });
            foreach (int value in new[] { 1, 2, 3, 4 })
            {
                int world = value;
                foreach (string variant in Variants(world))
                { string scene = variant; Add("Environment / World " + world + " " + scene, f => { f.Initial = Build(world == 4 ? 5 : 2, world == 4 ? 7 : 2); f.Initial.World = world; f.Initial.Variant = scene; }); }
            }
            Add("Environment / Normal audio", f => f.Initial.Muted = false);
            Add("Environment / Muted", f => f.Initial.Muted = true);
            Add("Environment / Focus loss", f => f.AfterShow = () => view.SetLifecycle(false, BoardMotionMode.Normal));
            Add("Environment / Focus resume", f => f.AfterShow = () => { view.SetLifecycle(false, BoardMotionMode.Normal); Delay(0.4f, () => view.SetLifecycle(true, BoardMotionMode.Normal)); });
            Add("Environment / Background modal", f => { f.Overlay = "Settings"; f.AfterShow = () => view.SetLifecycle(false, BoardMotionMode.Normal); });
            Add("Environment / Foreground modal", f => { f.Overlay = "Settings"; f.AfterShow = () => { view.SetLifecycle(false, BoardMotionMode.Normal); Delay(0.4f, () => { view.SetLifecycle(true, BoardMotionMode.Normal); view.SetOverlay(new OverlayPresentation { BlocksInput = true }); }); }; });
            Add("Environment / Entrance interrupted restored", f => { f.Entrance = new EntrancePresentation(); f.AfterShow = () => Delay(1.1f, () => { view.SetLifecycle(false, BoardMotionMode.Normal); Delay(0.4f, () => view.SetLifecycle(true, BoardMotionMode.Normal)); }); });
            Add("Environment / Local loading", f => f.Initial = null);
            Add("Environment / Missing required asset", f => f.Initial.RequiredKey = "Art/required_board_asset");
            Add("Environment / Missing required data", f => f.Initial.RequiredKey = "board branches");
            Add("Environment / Offline local data available", f => { });
        }
        private void Delay(float seconds, Action action) => timeline.Play(seconds, _ => { }, action);
        private void PressPurchase()
        {
            foreach (var button in root.Shop.GetComponentsInChildren<UnityEngine.UI.Button>(true))
                if (button.name == "Confirm purchase") { button.onClick.Invoke(); return; }
            throw new InvalidOperationException("Shop confirmation control is missing.");
        }
        private static float PhaseDelay(string phase) => phase == "Prepared covered" ? 0.05f : phase == "House" ? 0.35f : phase == "Branches" ? 1.1f : phase == "Birds" ? 2.0f : 3.3f;
        private static readonly string[] Species = { "sunny", "sky", "berry", "mint", "cocoa", "snow", "ember", "ink", "hoot" };
        private static string[] Variants(int world) => world == 1 ? new[] { "morning", "noon", "golden", "rain" } :
            world == 2 ? new[] { "noon", "mist", "lanterns", "drizzle" } :
            world == 3 ? new[] { "morning", "noon", "sunset", "windy" } : new[] { "morning", "noon", "bluehour", "aurora" };
        private static BoardSnapshot Build(int rows = 2, int speciesCount = 2, int branchCount = 0, string[] ids = null)
        {
            ids = ids ?? Species; if (branchCount == 0) branchCount = rows * 2;
            var data = new BoardSnapshot { LevelId = 1, Rows = rows, Branches = new BoardSnapshot.Branch[branchCount], Goals = new BoardSnapshot.Goal[speciesCount], Input = new InputPresentation { BoardEnabled = true, HudEnabled = true } };
            for (int i = 0; i < speciesCount; i++) data.Goals[i] = new BoardSnapshot.Goal { Species = ids[i], PortraitKey = ids[i] + "_face", NameKey = "labels/name_" + ids[i], GiftKey = BoardAssetKeys.GiftCoin(ids[i]) };
            data.Hud = new HudSnapshot { Coins = 240, CountedMoves = 0, ThreeStarLimit = 8, TwoStarLimit = 11, ReachableStars = 3, PhaseMovesLeft = 3, Goals = data.Goals,
                Tools = new[] { NewTool(BoosterKind.Firefly), NewTool(BoosterKind.Sapling) } };
            for (int i = 0; i < branchCount; i++)
            {
                data.Branches[i] = new BoardSnapshot.Branch { Id = i, Row = i / 2, IsRight = i % 2 != 0, Capacity = 4 };
                if (i != branchCount - 1) SetBirds(data, i, ids[i % speciesCount], ids[(i + 1) % speciesCount], ids[i % speciesCount]);
            }
            return data;
        }
        private static HudSnapshot.Tool NewTool(BoosterKind kind) => new HudSnapshot.Tool { Kind = kind, Visible = true, Enabled = true, Stock = 3 };
        private static void SetContext(BoardSnapshot data, BoosterKind kind) => data.Hud.Tools = new[] { data.Hud.Tools[0], data.Hud.Tools[1], NewTool(kind) };
        private static HudSnapshot.Tool Tool(BoardSnapshot data, BoosterKind kind)
        {
            foreach (var tool in data.Hud.Tools) if (tool.Kind == kind) return tool;
            SetContext(data, kind); return data.Hud.Tools[2];
        }
        private static void SetBirds(BoardSnapshot data, int branchId, params string[] tokens)
        {
            var branch = data.Branches[branchId]; branch.Birds = new BoardSnapshot.BirdData[tokens.Length];
            for (int seat = 0; seat < tokens.Length; seat++)
            {
                string token = tokens[seat], species = token; BirdFlags flags = BirdFlags.None;
                if (token.StartsWith("egg:", StringComparison.Ordinal)) { species = token.Substring(4); flags = BirdFlags.Egg; }
                else if (token.Contains(":"))
                {
                    var parts = token.Split(':'); species = parts[0]; flags = parts[1] == "sleep" ? BirdFlags.Asleep : parts[1] == "mama" ? BirdFlags.Mama : BirdFlags.Chick;
                }
                int id = branchId * 10 + seat;
                branch.Birds[seat] = new BoardSnapshot.BirdData { Bird = new Bird(id, new Species(species), flags), Sleeping = (flags & BirdFlags.Asleep) != 0,
                    Position = GameLayout.BirdSlot(seat, GameLayout.RowY(branch.Row, data.Rows, branch.IsRight), branch.IsRight, GameLayout.BirdScale(data.Rows)) };
                data.Sleeping[id] = branch.Birds[seat].Sleeping;
            }
        }
        private static void FillSeats(BoardSnapshot data, int seats)
        {
            foreach (var branch in data.Branches)
            {
                var tokens = new string[seats]; for (int i = 0; i < seats; i++) tokens[i] = data.Goals[(branch.Id + i) % data.Goals.Length].Species;
                SetBirds(data, branch.Id, tokens);
            }
        }
        private static void SetFlags(BoardSnapshot data, int branchId, BirdFlags flags)
        {
            var value = data.Branches[branchId].Birds[0]; value.Bird = new Bird(value.Bird.Id, value.Bird.Species, flags); value.Sleeping = (flags & BirdFlags.Asleep) != 0; data.Sleeping[value.Bird.Id] = value.Sleeping;
        }
        private static void Sleeping(BoardSnapshot data, int branch, bool sleeping)
        { foreach (var bird in data.Branches[branch].Birds) { bird.Sleeping = sleeping; data.Sleeping[bird.Bird.Id] = sleeping; } }
        private static SelectionPresentation Select(BoardSnapshot data, int branch) => new SelectionPresentation { BirdIds = Ids(data.Branches[branch]) };
        private static int[] Ids(BoardSnapshot.Branch branch)
        { var ids = new int[branch.Birds.Length]; for (int i = 0; i < ids.Length; i++) ids[i] = branch.Birds[i].Bird.Id; return ids; }
        private static BubblePresentation Sleep(BoardSnapshot data, params string[] icons) => new BubblePresentation { Visible = true, Point = data.Branches[0].Birds[0].Position + new Vector2(0, -200), IconKeys = icons };
        private static TutorialPresentation Target(BoardSnapshot data, int branch, int seat) => new TutorialPresentation { Visible = true, TargetKind = "branch", TargetId = branch,
            Point = GameLayout.BirdSlot(seat, GameLayout.RowY(data.Branches[branch].Row, data.Rows, data.Branches[branch].IsRight), data.Branches[branch].IsRight, GameLayout.BirdScale(data.Rows)) };
        private static void HomeAll(BoardSnapshot data)
        {
            foreach (var goal in data.Goals) goal.Home = true;
            foreach (var branch in data.Branches) branch.Birds = Array.Empty<BoardSnapshot.BirdData>();
            data.ResidentSpecies = data.Hud.ResidentSpecies = data.Goals[0].Species;
        }
        private static BoardSnapshot Copy(BoardSnapshot source)
        {
            var copy = Build(source.Rows, source.Goals.Length, source.Branches.Length);
            copy.LevelId = source.LevelId; copy.World = source.World; copy.Variant = source.Variant; copy.ResidentSpecies = source.ResidentSpecies;
            for (int i = 0; i < source.Goals.Length; i++)
            { var g = source.Goals[i]; copy.Goals[i] = new BoardSnapshot.Goal { Species = g.Species, PortraitKey = g.PortraitKey, NameKey = g.NameKey, GiftKey = g.GiftKey, Home = g.Home }; }
            for (int b = 0; b < source.Branches.Length; b++)
            {
                var old = source.Branches[b]; var branch = copy.Branches[b]; branch.Kind = old.Kind; branch.Capacity = old.Capacity;
                branch.Birds = new BoardSnapshot.BirdData[old.Birds.Length];
                for (int s = 0; s < old.Birds.Length; s++)
                { var bird = old.Birds[s]; branch.Birds[s] = new BoardSnapshot.BirdData { Bird = bird.Bird, Sleeping = bird.Sleeping, Position = bird.Position, Outfit = bird.Outfit }; copy.Sleeping[bird.Bird.Id] = bird.Sleeping; }
            }
            var hud = source.Hud; copy.Hud.Coins = hud.Coins; copy.Hud.CountedMoves = hud.CountedMoves; copy.Hud.ThreeStarLimit = hud.ThreeStarLimit; copy.Hud.TwoStarLimit = hud.TwoStarLimit;
            copy.Hud.ReachableStars = hud.ReachableStars; copy.Hud.UndoAvailable = hud.UndoAvailable; copy.Hud.PhaseEnabled = hud.PhaseEnabled; copy.Hud.Phase = hud.Phase; copy.Hud.PhaseMovesLeft = hud.PhaseMovesLeft;
            copy.Hud.LightStep = hud.LightStep; copy.Hud.FreeMove = hud.FreeMove; copy.Hud.MoonbeamActive = hud.MoonbeamActive; copy.Hud.Chain = hud.Chain; copy.Hud.ResidentSpecies = hud.ResidentSpecies;
            copy.Hud.Tools = new HudSnapshot.Tool[hud.Tools.Length];
            for (int i = 0; i < hud.Tools.Length; i++) { var t = hud.Tools[i]; copy.Hud.Tools[i] = new HudSnapshot.Tool { Kind = t.Kind, Stock = t.Stock, ZeroPlus = t.ZeroPlus, Enabled = t.Enabled, Visible = t.Visible }; }
            return copy;
        }
        private static void Transfer(Fixture f, int from, int to, int count, bool free = false)
        {
            f.Final = Copy(f.Initial); var source = f.Final.Branches[from]; var target = f.Final.Branches[to];
            var moved = new BoardSnapshot.BirdData[count]; Array.Copy(source.Birds, source.Birds.Length - count, moved, 0, count);
            var kept = new BoardSnapshot.BirdData[source.Birds.Length - count]; Array.Copy(source.Birds, kept, kept.Length); source.Birds = kept;
            int start = target.Birds.Length; var arrived = new BoardSnapshot.BirdData[start + count]; Array.Copy(target.Birds, arrived, start);
            var ids = new int[count];
            for (int i = 0; i < count; i++)
            { arrived[start + i] = moved[i]; ids[i] = moved[i].Bird.Id; moved[i].Position = GameLayout.BirdSlot(start + i, GameLayout.RowY(target.Row, f.Final.Rows, target.IsRight), target.IsRight, GameLayout.BirdScale(f.Final.Rows)); }
            target.Birds = arrived; f.Final.Hud.CountedMoves += free ? 0 : 1; f.Final.Hud.FreeMove = free; f.Final.Hud.UndoAvailable = true;
            f.Events.Add(new BoardResolutionEvent { Kind = Kind.Transfer, From = from, To = to, TargetStart = start, BirdIds = ids, Snapshot = f.Final });
        }
        private static void Swivel(Fixture f)
        {
            f.Initial.Branches[0].Kind = BranchKind.Swivel; f.Final = Copy(f.Initial);
            Array.Reverse(f.Final.Branches[0].Birds); var ids = Ids(f.Initial.Branches[0]); var seats = new int[ids.Length];
            for (int i = 0; i < ids.Length; i++) { seats[i] = ids.Length - 1 - i; f.Final.Branches[0].Birds[i].Position = GameLayout.BirdSlot(i, GameLayout.RowY(0, f.Final.Rows), false, GameLayout.BirdScale(f.Final.Rows)); }
            f.Final.Hud.UndoAvailable = true;
            f.Events.Add(new BoardResolutionEvent { Kind = Kind.Swivel, From = 0, BirdIds = ids, TargetSeats = seats, Snapshot = f.Final });
        }
        private static void Deliver(Fixture f, string species, bool cascade = false)
        {
            SetBirds(f.Initial, 0, species, species, species, species);
            if (cascade) SetBirds(f.Initial, 1, "sunny", "sunny", "sunny", "sunny");
            f.Final = Copy(f.Initial); f.Final.Branches[0].Birds = Array.Empty<BoardSnapshot.BirdData>();
            foreach (var goal in f.Final.Goals) if (goal.Species == species) goal.Home = true;
            f.Final.ResidentSpecies = f.Final.Hud.ResidentSpecies = species; f.Final.Hud.UndoAvailable = true;
            f.Events.Add(new BoardResolutionEvent { Kind = Kind.FlockHome, From = 0, Species = species, BirdIds = Ids(f.Initial.Branches[0]), Chain = 1, Snapshot = f.Final });
            f.Events.Add(new BoardResolutionEvent { Kind = Kind.Gift, Species = species, Icon = ViewModel.GiftIcon(species), Targets = new[] { new Vector2(540, 1100) }, LightStep = species == "snow", Snapshot = f.Final });
            if (cascade)
            {
                f.Final.Branches[1].Birds = Array.Empty<BoardSnapshot.BirdData>(); f.Final.Goals[0].Home = true; f.Final.Hud.Chain = 2;
                f.Final.ResidentSpecies = f.Final.Hud.ResidentSpecies = "sunny";
                f.Events.Add(new BoardResolutionEvent { Kind = Kind.FlockHome, From = 1, Species = "sunny", BirdIds = Ids(f.Initial.Branches[1]), Chain = 2, Snapshot = f.Final });
            }
        }
        private static void BirdEvent(Fixture f, Kind kind) => f.Events.Add(new BoardResolutionEvent { Kind = kind, BirdId = f.Initial.Branches[0].Birds[0].Bird.Id, Snapshot = f.Final ?? f.Initial });
    }
}
#endif
