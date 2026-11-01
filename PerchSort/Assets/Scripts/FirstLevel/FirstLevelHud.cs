using System;
using System.Collections.Generic;
using LitMotion;
using PerchSort.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PerchSort.FirstLevelRuntime
{
    [DisallowMultipleComponent]
    public sealed class FirstLevelHud : MonoBehaviour
    {
        private enum Modal { None, Pause, Result, NoMoves, Error, SaveError }

        private sealed class IconButton
        {
            public Button Button;
            public RectTransform Visual;
            public Image Icon;
            public MotionHandle Press;
        }

        private FirstLevelAssets assets;
        private FirstLevelFlow flow;
        private Func<int> lastHomedSpecies;
        private Func<bool> isMuted;
        private Action pause, undo, restart, home, play, toggleMute, retrySave;
        private RectTransform root, gameLayer, homeLayer, modalLayer, panel;
        private CanvasGroup gameGroup, homeGroup, modalGroup;
        private Image overlay, homeBackground, resultBird;
        private RectTransform viewportRoot;
        private readonly Vector3[] viewportCorners = new Vector3[4];
        private TMP_Text errorText, unsavedText;
        private readonly List<IconButton> buttons = new List<IconButton>();
        private readonly List<IconButton> modalButtons = new List<IconButton>();
        private IconButton pauseButton, undoButton, restartButton, muteButton, homeMute, playButton;
        private IconButton continueButton, modalRestart, modalHome, modalMute, replayButton, resultHome;
        private MotionHandle panelMotion, hudFade, resultWingMotion;
        private Modal modal;
        private bool initialized, subscribed, homeVisible = true, modalStable, closing, resultPhase;
        private int modalVersion;
        private string error, unsavedNotice;

        public void Initialize(FirstLevelAssets acceptedAssets, RectTransform safeRoot,
            Action pause, Action undo, Action restart, Action home, Action play,
            Action toggleMute, Func<bool> isMuted)
        {
            if (initialized) throw new InvalidOperationException("First-level HUD is already initialized.");
            assets = acceptedAssets != null ? acceptedAssets : throw new ArgumentNullException(nameof(acceptedAssets));
            if (safeRoot == null) throw new ArgumentNullException(nameof(safeRoot));
            this.pause = pause; this.undo = undo; this.restart = restart;
            this.home = home; this.play = play; this.toggleMute = toggleMute; this.isMuted = isMuted;

            root = Rect("FirstLevelHud", safeRoot, Vector2.zero, Vector2.zero);
            Stretch(root);
            var canvas = safeRoot.GetComponentInParent<Canvas>();
            viewportRoot = canvas != null ? (RectTransform)canvas.rootCanvas.transform : null;
            gameLayer = Rect("GameControls", root, Vector2.zero, Vector2.zero);
            Stretch(gameLayer); gameGroup = gameLayer.gameObject.AddComponent<CanvasGroup>();
            var badge = Picture("LevelBadge", gameLayer, assets.ButtonSkin, new Vector2(0, 850), new Vector2(240, 100));
            badge.type = Image.Type.Sliced;
            badge.color = new Color(1f, .96f, .86f);
            Label("LevelNumber", badge.rectTransform, "1", Vector2.zero, new Vector2(180, 80), 56);
            pauseButton = MakeButton("Pause", gameLayer, assets.PauseIcon, new Vector2(420, 850), 130,
                () => { if (CanRequestPause()) RunAction(pause); });
            undoButton = MakeButton("Undo", gameLayer, assets.UndoIcon, new Vector2(-262, -800), 130,
                () => { if (Interactive() && flow.CanUndo) RunAction(undo); });
            restartButton = MakeButton("Restart", gameLayer, assets.RestartIcon, new Vector2(0, -800), 130,
                () => { if (Interactive()) RunAction(restart); });
            muteButton = MakeButton("Mute", gameLayer, assets.SoundIcon, new Vector2(262, -800), 130, ToggleMute);

            homeLayer = Rect("HomeCover", root, Vector2.zero, Vector2.zero);
            Stretch(homeLayer); homeGroup = homeLayer.gameObject.AddComponent<CanvasGroup>();
            homeBackground = Picture("ViewportHomeBackground", homeLayer, null, Vector2.zero, Vector2.zero);
            homeBackground.color = new Color(0, 0, 0, .28f); homeBackground.raycastTarget = true;
            playButton = MakeButton("Play", homeLayer, assets.PlayIcon, new Vector2(0, -330), 220,
                () => { if (homeVisible && modal == Modal.None) RunAction(play); });
            homeMute = MakeButton("HomeMute", homeLayer, assets.SoundIcon, new Vector2(420, 850), 130, ToggleMute);

            modalLayer = Rect("ModalInputBlock", root, Vector2.zero, Vector2.zero);
            Stretch(modalLayer); modalGroup = modalLayer.gameObject.AddComponent<CanvasGroup>();
            // Only the dimmer spans the camera viewport. Panel layout stays in
            // the safe 1080x1920 design root, including on tall/inset displays.
            overlay = Picture("ViewportOverlay", modalLayer, null, Vector2.zero, Vector2.zero);
            overlay.color = new Color(0, 0, 0, .72f); overlay.raycastTarget = true;
            panel = Rect("Panel", modalLayer, Vector2.zero, new Vector2(760, 760));
            var panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.sprite = assets.PanelSkin; panelImage.type = Image.Type.Sliced;
            panelImage.raycastTarget = true;

            continueButton = ModalButton("Continue", assets.PlayIcon, new Vector2(0, 130), 220,
                () => NavigateModal(pause));
            modalRestart = ModalButton("PauseRestart", assets.RestartIcon, new Vector2(-220, -170), 150,
                () => NavigateModal(restart));
            modalHome = ModalButton("PauseHome", assets.HomeIcon, new Vector2(0, -170), 150,
                () => NavigateModal(home));
            modalMute = ModalButton("PauseMute", assets.SoundIcon, new Vector2(220, -170), 150, ToggleMute);
            replayButton = ModalButton("Replay", assets.RestartIcon, new Vector2(-150, -170), 150,
                () => NavigateModal(restart));
            resultHome = ModalButton("Home", assets.HomeIcon, new Vector2(150, -170), 150,
                () => NavigateModal(home));
            resultBird = Picture("LastHomedBird", panel, null, new Vector2(0, 110), new Vector2(240, 240));
            resultBird.preserveAspect = true;
            errorText = Label("BriefError", panel, "", new Vector2(0, 120), new Vector2(600, 180), 36);
            errorText.richText = false; errorText.textWrappingMode = TextWrappingModes.NoWrap;
            // Necessary failure status only; it never claims a save succeeded.
            unsavedText = Label("UnsavedNotice", root, "", new Vector2(0, -910), new Vector2(900, 60), 32);
            unsavedText.richText = false;
            unsavedText.textWrappingMode = TextWrappingModes.NoWrap;
            unsavedText.overflowMode = TextOverflowModes.Ellipsis;
            unsavedText.color = new Color(1f, .94f, .79f);
            unsavedText.outlineColor = new Color(.25f, .14f, .07f);
            unsavedText.outlineWidth = .15f;

            initialized = true;
            Canvas.willRenderCanvases += FitBackgroundsToViewport;
            FitBackgroundsToViewport();
            ApplyUnsavedNotice();
            HideModal();
            RefreshAudio();
            Refresh();
        }

        public void Bind(FirstLevelFlow controller, Func<int> lastHomedSpecies)
        {
            Unsubscribe();
            flow = controller; this.lastHomedSpecies = lastHomedSpecies;
            error = null;
            Subscribe();
            if (initialized) { HideModal(); Refresh(); }
        }

        public void BindRetrySave(Action retry)
        {
            retrySave = retry;
            if (initialized) RefreshModalButtons();
        }

        public void ShowUnsavedNotice(string message)
        {
            unsavedNotice = string.IsNullOrWhiteSpace(message) ? null :
                message.Substring(0, Math.Min(message.Length, 80));
            if (initialized) ApplyUnsavedNotice();
        }

        private void ApplyUnsavedNotice()
        {
            unsavedText.text = unsavedNotice ?? "";
            unsavedText.gameObject.SetActive(unsavedNotice != null);
            unsavedText.transform.SetAsLastSibling();
        }

        public void SetHomeVisible(bool visible)
        {
            homeVisible = visible;
            if (visible) error = null;
            if (initialized) { HideModal(); Refresh(); }
        }

        public void RefreshAudio()
        {
            if (!initialized) return;
            bool muted = isMuted != null && isMuted();
            Sprite icon = muted ? assets.MutedIcon : assets.SoundIcon;
            muteButton.Icon.sprite = homeMute.Icon.sprite = modalMute.Icon.sprite = icon;
            muteButton.Button.gameObject.name = homeMute.Button.gameObject.name =
                modalMute.Button.gameObject.name = muted ? "Unmute" : "Mute";
        }

        public void ShowError(string message)
        {
            error = string.IsNullOrWhiteSpace(message) ? "Please restart." :
                message.Substring(0, Math.Min(message.Length, 160));
            if (initialized) Refresh();
        }

        private void Refresh()
        {
            if (!initialized || !isActiveAndEnabled) return;
            bool showGame = !homeVisible && flow != null;
            gameLayer.gameObject.SetActive(showGame);
            homeLayer.gameObject.SetActive(homeVisible);
            bool results = showGame && (flow.Phase == FirstLevelPhase.Celebrating || flow.Phase == FirstLevelPhase.Won);
            if (results != resultPhase)
            {
                resultPhase = results; hudFade.TryCancel();
                hudFade = LMotion.Create(gameGroup.alpha, results ? 0f : 1f, .20f)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .Bind(alpha => { if (gameGroup != null) gameGroup.alpha = alpha; });
            }
            // Hiding the object preserves the fixed Undo slot; neighbours never move.
            undoButton.Button.gameObject.SetActive(showGame && flow.UndoCount > 0);

            // Persistence failure has its own recovery action, never presentation Restart.
            Modal wanted = !homeVisible && flow != null && flow.Phase == FirstLevelPhase.SaveError
                ? Modal.SaveError : !string.IsNullOrEmpty(error) ? Modal.Error : Modal.None;
            if (wanted == Modal.None && !homeVisible && flow != null)
            {
                if (flow.Phase == FirstLevelPhase.Paused) wanted = Modal.Pause;
                else if (flow.Phase == FirstLevelPhase.Celebrating || flow.Phase == FirstLevelPhase.Won) wanted = Modal.Result;
                else if (flow.Phase == FirstLevelPhase.NoMoves) wanted = Modal.NoMoves;
                else if (flow.Phase == FirstLevelPhase.Faulted) wanted = Modal.Error;
            }
            if (wanted != modal) { if (wanted == Modal.None) HideModal(); else ShowModal(wanted); }
            // Recompute after modal changes so external Resume/Bind cannot leave stale locks.
            bool canAct = Interactive();
            bool canPause = CanRequestPause();
            // Only Pause remains a live touch target while the committed event chain plays.
            gameGroup.interactable = gameGroup.blocksRaycasts = canAct || canPause;
            SetButton(pauseButton, canPause);
            SetButton(undoButton, canAct && flow != null && flow.CanUndo);
            SetButton(restartButton, canAct); SetButton(muteButton, canAct);
            homeGroup.interactable = homeGroup.blocksRaycasts = homeVisible && modal == Modal.None;
            SetButton(playButton, homeVisible && modal == Modal.None);
            SetButton(homeMute, homeVisible && modal == Modal.None);
            if (modal == Modal.Error) errorText.text = error ?? "Please restart.";
            if (modal == Modal.SaveError) errorText.text = SaveFailureText();
            RefreshModalButtons();
        }

        private bool Interactive() => !homeVisible && flow != null && flow.IsInteractive &&
            modal == Modal.None && !closing;

        private bool CanRequestPause() => !homeVisible && flow != null &&
            modal == Modal.None && !closing &&
            (flow.IsInteractive || flow.Phase == FirstLevelPhase.Presenting);

        private string SaveFailureText() => flow != null && flow.SaveIsUncertain
            ? "Save uncertain" : "Save failed";

        private void ShowModal(Modal kind)
        {
            HideModal();
            modal = kind;
            modalLayer.gameObject.SetActive(true);
            modalGroup.interactable = false; modalGroup.blocksRaycasts = true;
            foreach (var b in modalButtons) b.Button.gameObject.SetActive(false);
            resultBird.gameObject.SetActive(kind == Modal.Result);
            errorText.gameObject.SetActive(kind == Modal.Error || kind == Modal.NoMoves || kind == Modal.SaveError);
            if (kind == Modal.Pause)
            {
                continueButton.Button.gameObject.SetActive(true);
                modalRestart.Button.gameObject.SetActive(true);
                modalHome.Button.gameObject.SetActive(true);
                modalMute.Button.gameObject.SetActive(true);
            }
            else
            {
                replayButton.Button.gameObject.SetActive(true);
                resultHome.Button.gameObject.SetActive(kind != Modal.NoMoves);
                if (kind == Modal.NoMoves && flow != null && flow.UndoCount > 0)
                {
                    continueButton.Button.gameObject.SetActive(true);
                    continueButton.Icon.sprite = assets.UndoIcon;
                }
                replayButton.Button.transform.localPosition = new Vector3(kind == Modal.NoMoves ? (flow.UndoCount == 0 ? 0 : 150) : -150, -170, 0);
                resultHome.Button.transform.localPosition = new Vector3(150, -170, 0);
            }
            var continueRect = (RectTransform)continueButton.Button.transform;
            continueRect.anchoredPosition = kind == Modal.NoMoves ? new Vector2(-150, -170) : new Vector2(0, 130);
            continueRect.sizeDelta = Vector2.one * (kind == Modal.NoMoves ? 230 : 250);
            continueButton.Visual.sizeDelta = Vector2.one * (kind == Modal.NoMoves ? 200 : 220);
            if (kind == Modal.Pause) continueButton.Icon.sprite = assets.PlayIcon;
            if (kind == Modal.NoMoves) errorText.text = "No moves";
            else if (kind == Modal.Error) errorText.text = error ?? "Please restart.";
            else if (kind == Modal.SaveError) errorText.text = SaveFailureText();
            replayButton.Button.gameObject.name = kind == Modal.SaveError ? "RetrySave" : "Replay";
            if (kind == Modal.Result)
            {
                int species = lastHomedSpecies != null ? lastHomedSpecies() : 0;
                species = Mathf.Clamp(species, 0, assets.Species.Length - 1);
                var art = assets.Species[species];
                int wingVersion = modalVersion;
                int[] cycle = { 0, 1, 2, 3, 4, 5, 4, 3, 2, 1 };
                resultBird.sprite = art.WingCycle[0].Sprite;
                resultBird.preserveAspect = true;
                // Real registered silhouettes, using the same ten-phase clock as Bird.
                resultWingMotion = LMotion.Create(0f, 10f, .36f)
                    .WithLoops(-1)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .Bind(value =>
                    {
                        if (this == null || wingVersion != modalVersion ||
                            modal != Modal.Result || resultBird == null) return;
                        resultBird.sprite = art.WingCycle[
                            cycle[Mathf.Min(9, Mathf.FloorToInt(value))]].Sprite;
                    });
            }

            int version = modalVersion;
            panel.localScale = Vector3.one * .85f;
            modalGroup.alpha = 0;
            panelMotion = LMotion.Create(0f, 1f, .25f)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithOnComplete(() =>
                {
                    if (this == null || version != modalVersion) return;
                    panel.localScale = Vector3.one; modalGroup.alpha = 1;
                    modalStable = true;
                    RefreshModalButtons();
                    var first = kind == Modal.Pause ? continueButton :
                        kind == Modal.SaveError && retrySave == null ? resultHome : replayButton;
                    if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(first.Button.gameObject);
                })
                .Bind(t =>
                {
                    if (this == null || version != modalVersion) return;
                    panel.localScale = Vector3.one * (t < .65f ?
                        Mathf.Lerp(.85f, 1.05f, t / .65f) : Mathf.Lerp(1.05f, 1, (t - .65f) / .35f));
                    modalGroup.alpha = Mathf.Clamp01(t * 2.5f);
                });
        }

        private void RefreshModalButtons()
        {
            bool allowed = modalStable && !closing && modal != Modal.None;
            if (modal == Modal.Result) allowed &= flow != null && flow.Phase == FirstLevelPhase.Won;
            modalGroup.interactable = allowed;
            foreach (var b in modalButtons) SetButton(b, allowed);
            if (modal == Modal.SaveError) SetButton(replayButton, allowed && retrySave != null);
        }

        private void NavigateModal(Action action)
        {
            if (!modalStable || closing) return;
            if (modal == Modal.Result && (flow == null || flow.Phase != FirstLevelPhase.Won)) return;
            if (modal == Modal.NoMoves && action == pause) action = undo;
            if (modal == Modal.SaveError)
            {
                // Retry keeps the exact pending transaction; it must not create a new move
                // or invoke the fixed-board Restart path, including on uncertain readback.
                if (action == restart)
                {
                    if (retrySave == null) return;
                    action = retrySave;
                }
                else if (action != home) return;
            }
            bool restartFromError = modal == Modal.Error && action == restart;
            closing = true; RefreshModalButtons();
            int version = ++modalVersion;
            panelMotion.TryCancel();
            panelMotion = LMotion.Create(0f, 1f, .15f)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithOnComplete(() =>
                {
                    if (this == null || version != modalVersion) return;
                    HideModal();
                    // Clear only the accepted Error Replay's stale message. A new failure
                    // raised by the restart callback can immediately open a fresh error.
                    if (restartFromError) error = null;
                    RunAction(action); Refresh();
                })
                .Bind(t =>
                {
                    if (this == null || version != modalVersion) return;
                    panel.localScale = Vector3.one * Mathf.Lerp(1, .9f, t);
                    modalGroup.alpha = 1 - t;
                });
        }

        private void HideModal()
        {
            modalVersion++; panelMotion.TryCancel(); resultWingMotion.TryCancel();
            modal = Modal.None; modalStable = closing = false;
            if (modalLayer != null) modalLayer.gameObject.SetActive(false);
        }

        private void ToggleMute()
        {
            if (!initialized || closing) return;
            bool allowed = homeVisible && modal == Modal.None ||
                Interactive() || modal == Modal.Pause && modalStable;
            if (!allowed) return;
            RunAction(toggleMute); RefreshAudio();
        }

        private void RunAction(Action action)
        {
            try { action?.Invoke(); }
            catch (Exception ex) { Debug.LogException(ex, this); ShowError("Please restart."); }
        }

        private IconButton ModalButton(string name, Sprite icon, Vector2 position, float visualSize, Action action)
        {
            var button = MakeButton(name, panel, icon, position, visualSize, action);
            modalButtons.Add(button); return button;
        }

        private IconButton MakeButton(string name, RectTransform parent, Sprite icon, Vector2 position, float visualSize, Action action)
        {
            var hit = Rect(name, parent, position, Vector2.one * (visualSize + 30));
            var target = hit.gameObject.AddComponent<Image>();
            target.color = Color.clear; target.raycastTarget = true;
            var visual = Picture("Wood", hit, assets.ButtonSkin, Vector2.zero, Vector2.one * visualSize);
            visual.type = Image.Type.Simple;
            visual.preserveAspect = true;
            var image = Picture("Icon", visual.rectTransform, icon, Vector2.zero, Vector2.one * (visualSize * .52f));
            image.preserveAspect = true;
            var value = new IconButton { Button = hit.gameObject.AddComponent<Button>(), Visual = visual.rectTransform, Icon = image };
            value.Button.targetGraphic = visual;
            value.Button.navigation = new Navigation { mode = Navigation.Mode.Automatic };
            var colors = value.Button.colors;
            colors.highlightedColor = new Color(1, 1, .85f);
            colors.pressedColor = new Color(.88f, .78f, .6f);
            colors.disabledColor = Color.white; // Modal overlay alone darkens underlying controls.
            value.Button.colors = colors;
            value.Button.onClick.AddListener(() =>
            {
                if (!value.Button.IsActive() || !value.Button.IsInteractable()) return;
                action?.Invoke();
                // Honor the resulting global mute state, including the mute button itself.
                AudioManager.Instance?.Play(Sfx.Click);
            });
            var trigger = hit.gameObject.AddComponent<EventTrigger>();
            AddPointer(trigger, EventTriggerType.PointerDown, () => Press(value, true));
            AddPointer(trigger, EventTriggerType.PointerUp, () => Press(value, false));
            AddPointer(trigger, EventTriggerType.PointerExit, () => Press(value, false));
            buttons.Add(value); return value;
        }

        private void Press(IconButton button, bool down)
        {
            if (!button.Button.IsActive() || !button.Button.IsInteractable()) return;
            button.Press.TryCancel();
            button.Press = LMotion.Create(button.Visual.localScale.x, down ? .92f : 1f, down ? .08f : .18f)
                .WithEase(down ? Ease.OutQuad : Ease.OutBack)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .Bind(scale => { if (button.Visual != null) button.Visual.localScale = Vector3.one * scale; });
        }

        private static void AddPointer(EventTrigger trigger, EventTriggerType type, Action action)
        {
            var entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(_ => action());
            trigger.triggers.Add(entry);
        }

        private void SetButton(IconButton button, bool enabled)
        {
            if (button.Button.interactable == enabled) return;
            button.Button.interactable = enabled;
            if (!enabled) { button.Press.TryCancel(); button.Visual.localScale = Vector3.one; }
        }

        private TMP_Text Label(string name, RectTransform parent, string text, Vector2 position, Vector2 size, float fontSize)
        {
            var rt = Rect(name, parent, position, size);
            var label = rt.gameObject.AddComponent<TextMeshProUGUI>();
            label.font = assets.Font; label.text = text; label.fontSize = fontSize;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(.35f, .22f, .13f);
            label.raycastTarget = false; return label;
        }

        private static Image Picture(string name, RectTransform parent, Sprite sprite, Vector2 position, Vector2 size)
        {
            var image = Rect(name, parent, position, size).gameObject.AddComponent<Image>();
            image.sprite = sprite; image.raycastTarget = false; return image;
        }

        private static RectTransform Rect(string name, RectTransform parent, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.sizeDelta = size; rect.anchoredPosition = position; return rect;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private void Subscribe()
        {
            if (flow == null || subscribed || !isActiveAndEnabled) return;
            flow.Changed += Refresh; subscribed = true;
        }

        private void Unsubscribe()
        {
            if (flow != null && subscribed) flow.Changed -= Refresh;
            subscribed = false;
        }

        private void FitBackgroundsToViewport()
        {
            if (!initialized || viewportRoot == null) return;
            viewportRoot.GetWorldCorners(viewportCorners);
            FitViewportBackground(overlay);
            FitViewportBackground(homeBackground);
        }

        private void FitViewportBackground(Image background)
        {
            if (background == null) return;
            var parent = background.rectTransform.parent;
            Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            foreach (var corner in viewportCorners)
            {
                Vector2 local = parent.InverseTransformPoint(corner);
                min = Vector2.Min(min, local); max = Vector2.Max(max, local);
            }
            background.rectTransform.anchoredPosition = (min + max) * .5f;
            background.rectTransform.sizeDelta = max - min;
        }

        private void OnEnable()
        {
            Subscribe();
            if (initialized)
            {
                Canvas.willRenderCanvases -= FitBackgroundsToViewport;
                Canvas.willRenderCanvases += FitBackgroundsToViewport;
                root.gameObject.SetActive(true); Refresh();
            }
        }

        private void OnDisable()
        {
            Canvas.willRenderCanvases -= FitBackgroundsToViewport;
            Unsubscribe(); CancelMotion();
            if (root != null) root.gameObject.SetActive(false);
        }

        private void CancelMotion()
        {
            HideModal(); hudFade.TryCancel();
            foreach (var button in buttons)
            {
                button.Press.TryCancel();
                if (button.Visual != null) button.Visual.localScale = Vector3.one;
            }
        }

        private void OnDestroy()
        {
            Canvas.willRenderCanvases -= FitBackgroundsToViewport;
            Unsubscribe(); CancelMotion();
            if (root != null) Destroy(root.gameObject);
        }

        private void Update()
        {
            if (!initialized || Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame ||
                closing || modal != Modal.None && !modalStable) return;
            if (modal == Modal.Pause) NavigateModal(pause);
            else if (CanRequestPause())
                RunAction(pause); // Flow coalesces touch/Escape into its one pending pause request.
        }
    }
}
