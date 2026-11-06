using System;
using System.Collections.Generic;
using PerchSort.Audio;
using PerchSort.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PerchSort.Game
{
    public sealed class BoardPresentation : MonoBehaviour, IBoardView
    {
        private GameArt art;
        private CanvasGroup boardInput, hudInput;
        private RectTransform bubble;
        private GameObject recovery;
        private InputPresentation input = new InputPresentation();
        private TutorialHand tutorial;
        private LevelEntrance entrance;
        private int generation;
        private bool deferred, focused = true, overlay;
        private long entranceTransaction;
        private long completedEntrance = long.MinValue, completedResolution = long.MinValue;
        private float bubbleShown;
        private Vector2 bubblePoint;
        private CanvasGroup bubbleFade;
        public BoardSnapshot Snapshot { get; private set; }
        public BoardView Board { get; private set; }
        public HudView Hud { get; private set; }
        public SceneView Scene { get; private set; }
        public bool IsIntroducing => entrance != null && entrance.IsPlaying;
        public bool BubbleVisible => (bubble != null && bubble.gameObject.activeSelf) || (Hud != null && Hud.ToolHintVisible);
        public event Action<int> BranchRequested;
        public event Action SelectionCancelRequested;
        public event Action<int> SwivelRequested;
        public event Action UndoRequested;
        public event Action<BoosterKind> BoosterRequested;
        public event Action<BoosterKind> ShopRequested;
        public event Action<string> GoalInfoRequested;
        public event Action SettingsRequested;
        public event Action DismissTopRequested;
        public event Action EntranceSkipRequested;
        public event Action<BoardAction> DeferredActionRequested;
        public event Action HintDismissRequested;
        public event Action<long> EntrancePresented;
        public event Action<long> ResolutionPresented;
        public event Action<string> PresentationFailed;
        internal event Action<Sfx> SoundRequested;
        internal event Action InputChanged;

        public void Configure(GameArt assets, BoardView board, HudView hud, SceneView scene, TutorialHand hand, LevelEntrance intro,
            CanvasGroup boardGroup, CanvasGroup hudGroup)
        {
            art = assets; Board = board; Hud = hud; Scene = scene; tutorial = hand; entrance = intro;
            boardInput = boardGroup; hudInput = hudGroup;
        }
        public void Show(BoardSnapshot snapshot)
        {
            HideAndCancel();
            Snapshot = snapshot;
            if (snapshot == null) { Fail("board snapshot"); return; }
            if (!string.IsNullOrEmpty(snapshot.RequiredKey)) { Fail(snapshot.RequiredKey); return; }
            if (snapshot.Hud == null || snapshot.Goals.Length == 0) { Fail("board goals/HUD"); return; }
            try
            {
                focused = snapshot.Focused; overlay = false;
                Board.gameObject.SetActive(true); Hud.gameObject.SetActive(true);
                Scene.SetLevel(snapshot); Hud.SetLevel(snapshot); Board.Rebuild(snapshot);
                SetInput(snapshot.Input);
            }
            catch (Exception error)
            {
                Fail(error.Message);
            }
        }
        public void Restore(BoardSnapshot snapshot)
        {
            if (snapshot == null || Snapshot == null || Snapshot.LevelId != snapshot.LevelId || recovery != null ||
                !Board.gameObject.activeSelf || !Board.CanSettle(snapshot))
            { Show(snapshot); return; }
            generation++; Snapshot = snapshot; overlay = false;
            HideBubble(); Board.Cancel(); Board.Settle(snapshot); Hud.Render(snapshot.Hud); SetInput(snapshot.Input);
        }
        public void UpdateHud(HudSnapshot snapshot) { if (Snapshot == null) return; Snapshot.Hud = snapshot; Hud.Render(snapshot); }
        public void SetSelection(SelectionPresentation selection) { if (Snapshot != null) Board.ShowSelection(selection ?? new SelectionPresentation()); }
        public void BindResolutionSnapshot(BoardSnapshot snapshot) => Snapshot = snapshot;
        public void PlayResolution(IReadOnlyList<BoardResolutionEvent> orderedEvents, long transactionId)
        {
            if (transactionId == completedResolution) return;
            if (Snapshot == null) { Fail("resolution snapshot"); return; }
            int run = ++generation; deferred = false;
            Board.Play(orderedEvents, () =>
            {
                if (run != generation) return;
                completedResolution = transactionId; generation++; ResolutionPresented?.Invoke(transactionId);
            });
        }
        public void SetInput(InputPresentation value)
        {
            input = value ?? new InputPresentation();
            if (!input.AllowDeferred) deferred = false;
            bool active = Snapshot != null && focused && !overlay && recovery == null;
            boardInput.blocksRaycasts = boardInput.interactable = active && input.BoardEnabled;
            hudInput.blocksRaycasts = hudInput.interactable = active && input.HudEnabled;
        }
        public void ShowHint(HintPresentation hint)
        {
            Board.ClearHints(); HideBubble();
            if (hint == null) return;
            if (hint.Path.HasValue) Board.Hint(hint.Path.Value);
            if (hint.Stuck) Hud.ShowToolHint(delay: 0.8f);
            else if (!string.IsNullOrEmpty(hint.ReasonIcon)) Hud.ShowToolHint(hint.ReasonIcon);
        }
        public void ShowBubble(BubblePresentation data)
        {
            HideBubble();
            if (data == null || !data.Visible || overlay || !focused) return;
            var keys = data.IconKeys ?? Array.Empty<string>();
            foreach (string key in keys)
                if (key == "icon_undo" || key == "tool_hint" || key == "tool_branch")
                { Hud.ShowToolHint(keys.Length == 1 ? key : null, data.Delay); return; }
            Vector2 point = data.Point; point.x = Mathf.Clamp(point.x, 120, 960); point.y = Mathf.Clamp(point.y, 550, 1650);
            bubblePoint = point;
            bubble = GameUI.Rect(transform, "Plush sleep explanation", point, new Vector2(200, 136));
            bubbleFade = bubble.gameObject.AddComponent<CanvasGroup>();
            bubbleFade.blocksRaycasts = bubbleFade.interactable = false;
            GameUI.Fit(bubble, "Cream bubble", art.Get("sleep_bubble"), new Vector2(100, 68), new Vector2(200, 136));
            for (int i = 0; i < keys.Length; i++)
                GameUI.Fit(bubble, keys[i], art.Get(keys[i]), new Vector2(100 + (i - (keys.Length - 1) * 0.5f) * 62, 60), Vector2.one * 66);
            BirdView nearest = null; float distance = float.PositiveInfinity;
            foreach (var bird in Board.Birds.Values)
            {
                if (!bird.Sleeping || bird.Bird.IsEgg) continue;
                float candidate = Vector2.SqrMagnitude(data.Point - (bird.Position + new Vector2(0, -200 * bird.DisplayScale)));
                if (candidate < distance) { distance = candidate; nearest = bird; }
            }
            nearest?.ReactToSleepTap();
            bubbleShown = Time.unscaledTime + (GameMotion.Instant ? 0 : data.Delay);
            bubble.gameObject.SetActive(data.Delay <= 0 || GameMotion.Instant);
        }
        private void Update()
        {
            if (bubble == null) return;
            float elapsed = Time.unscaledTime - bubbleShown;
            if (elapsed >= SleepBubble.BubbleSeconds + SleepBubble.FadeSeconds) { HideBubble(); return; }
            if (elapsed >= 0)
            {
                bubble.gameObject.SetActive(true);
                bubbleFade.alpha = 1 - Mathf.Clamp01((elapsed - SleepBubble.BubbleSeconds) / SleepBubble.FadeSeconds);
                float bob = GameMotion.Instant ? 0 : SleepBubble.BobPixels * Mathf.Sin(elapsed * Mathf.PI * 2 / 1.6f);
                bubble.anchoredPosition = new Vector2(bubblePoint.x, -bubblePoint.y + bob);
            }
            if (Time.unscaledTime > bubbleShown && Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            { HideBubble(); HintDismissRequested?.Invoke(); }
        }
        private void HideBubble()
        {
            Hud?.HideToolHint();
            if (bubble != null) GameUI.Dispose(bubble.gameObject);
            bubble = null; bubbleFade = null;
        }
        public void SetTutorial(TutorialPresentation value) => tutorial?.Render(overlay || !focused ? null : value);
        public void PlayEntrance(EntrancePresentation value)
        {
            if (value.TransactionId == completedEntrance && !value.PreparedOnly) return;
            if (Snapshot == null) { Fail("entrance snapshot"); return; }
            int run = ++generation; entranceTransaction = value.TransactionId;
            HideBubble(); tutorial.Hide(); deferred = false;
            if (value.PreparedOnly) { entrance.Prepare(); return; }
            entrance.Play(() =>
            {
                if (run != generation) return;
                completedEntrance = entranceTransaction; generation++; EntrancePresented?.Invoke(entranceTransaction);
            }, value.StartAtSeconds);
        }
        public void SkipEntrance() => entrance.Skip();
        public void SetOverlay(OverlayPresentation value)
        {
            overlay = value != null && value.BlocksInput;
            if (overlay) { HideBubble(); tutorial.Hide(); Board.ClearHints(); }
            SetInput(input);
        }
        public void SetLifecycle(bool focus, BoardMotionMode motionMode)
        {
            focused = focus; GameMotion.Instant = motionMode != BoardMotionMode.Normal;
            if (!focus)
            {
                generation++; entrance.Cancel(); Board.Cancel(); Hud.Cancel(); tutorial.Hide(); HideBubble();
            }
            else if (Snapshot != null) Restore(Snapshot);
            SetInput(input);
        }
        public void HideAndCancel()
        {
            generation++; deferred = false;
            entrance?.Cancel(); Board?.Cancel(); Hud?.Cancel(); tutorial?.Hide(); HideBubble();
            if (recovery != null) GameUI.Dispose(recovery); recovery = null;
            if (Board != null) Board.gameObject.SetActive(false);
            if (Hud != null) Hud.gameObject.SetActive(false);
            if (boardInput != null) boardInput.blocksRaycasts = boardInput.interactable = false;
            if (hudInput != null) hudInput.blocksRaycasts = hudInput.interactable = false;
        }
        public void Fail(string requiredKey)
        {
            Board.gameObject.SetActive(false); Hud.gameObject.SetActive(false);
            if (recovery == null) recovery = GameUI.Fit(transform, "Unavailable board: " + requiredKey, art.Icon("icon_home"), new Vector2(540, 960), Vector2.one * 108).gameObject;
            SetInput(new InputPresentation()); PresentationFailed?.Invoke(requiredKey);
            Debug.LogWarning("Board presentation unavailable: " + requiredKey);
        }
        private bool Request(BoardAction action, bool hud = false)
        {
            if (IsIntroducing || input.Entrance) { RequestEntranceSkip(); return false; }
            if (Snapshot == null || !focused || overlay || recovery != null || (hud ? !input.HudEnabled : !input.BoardEnabled)) return false;
            HideBubble(); HintDismissRequested?.Invoke();
            if (input.Airborne) return false;
            if (input.AllowDeferred)
            {
                if (!deferred) { deferred = true; DeferredActionRequested?.Invoke(action); }
                return false;
            }
            return true;
        }
        public void RequestBranch(int id) { if (Request(new BoardAction { Kind = "branch", BranchId = id })) BranchRequested?.Invoke(id); }
        public void RequestSwivel(int id) { if (Request(new BoardAction { Kind = "swivel", BranchId = id })) SwivelRequested?.Invoke(id); }
        public void RequestCancel() { if (Request(new BoardAction { Kind = "cancel" })) SelectionCancelRequested?.Invoke(); }
        public void RequestUndo() { if (Request(new BoardAction { Kind = "undo" }, true)) UndoRequested?.Invoke(); }
        public void RequestTool(BoosterKind kind)
        {
            if (!Request(new BoardAction { Kind = "tool", Booster = kind }, true)) return;
            foreach (var tool in Snapshot.Hud.Tools) if (tool.Kind == kind && tool.Visible && tool.Enabled)
            { if (tool.ZeroPlus) ShopRequested?.Invoke(kind); else BoosterRequested?.Invoke(kind); return; }
        }
        public void RequestGoal(string species) { if (Request(new BoardAction { Kind = "goal" }, true)) GoalInfoRequested?.Invoke(species); }
        public void RequestSettings() { if (Request(new BoardAction { Kind = "settings" }, true)) SettingsRequested?.Invoke(); }
        public void RequestDismissTop() => DismissTopRequested?.Invoke();
        public void RequestEntranceSkip() { EntranceSkipRequested?.Invoke(); }
        internal void PresentationInputChanged() => InputChanged?.Invoke();
        internal void PlaySound(Sfx sound) => SoundRequested?.Invoke(sound);
        private void OnDisable() { if (Board != null) HideAndCancel(); }
    }
}
