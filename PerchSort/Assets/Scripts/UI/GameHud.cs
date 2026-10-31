using System.Collections.Generic;
using PerchSort.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PerchSort.UI
{
    public enum UiPage { Game, Menu, Levels, Settings, Pause, Reset, Exit }

    public sealed partial class GameHud : MonoBehaviour
    {
        [SerializeField] private UiTheme theme;
        [SerializeField] private RectTransform root;
        private readonly Dictionary<UiPage, GameObject> pages = new Dictionary<UiPage, GameObject>();
        private readonly Stack<UiPage> navigation = new Stack<UiPage>();
        private readonly List<Button> levelButtons = new List<Button>();
        private TMP_Text levelLabel, instruction, completedLabel;
        private Button undo, restart, pause, primary;
        private GameObject resultPanel;
        private CanvasGroup gameplayInput,hudControls;
        private ButtonFeedback[] hudFeedback;
        private Coroutine hudMotion;
        private ScreenTransition transition;
        private bool closingModal;
        public bool IsTransitioning => transition.IsRunning || closingModal;
        private GameController game;
        private TutorialOverlay tutorial;
        private CrateLabels crateLabels;
        private LevelGrid levelGrid;
        private Button hintButton;
        private TMP_Text hintButtonLabel, levelSummary;
        private readonly List<Image> starSlots = new List<Image>();
        private readonly List<Image> rewardStars = new List<Image>();
        private RectTransform hintSource, hintTarget;
        private Image winnerBird;
        private GameObject deadlockPanel;
        private Button deadlockUndo;
        private readonly List<SettingsPanel> settingsPanels = new List<SettingsPanel>();
        private ResultAnimation celebration;
        private MenuAnimation menuAnimation;
        private string hintMessageKey;
        public Move? DisplayedHint { get; private set; }
        public bool TutorialVisible => tutorial.gameObject.activeSelf;
        public UiPage Page { get; private set; } = UiPage.Menu;
        public bool WinVisible { get; private set; }
        public bool DeadlockVisible { get; private set; }
        public IReadOnlyList<Button> LevelButtons => levelButtons;
        public void Configure(RectTransform design, UiTheme style) { root = design; theme = style; }
        private string S(string key) => Strings.Get(key);

        public void Bind(GameController controller)
        {
            game = controller;
            BuildGame(); BuildMenu(); BuildLevels(); BuildSettingsPage(false); BuildSettingsPage(true); BuildConfirmations();
            var teaching = theme.Rect("Tutorial", pages[UiPage.Game].transform, Vector2.zero, new Vector2(1080,1920));
            tutorial = teaching.gameObject.AddComponent<TutorialOverlay>(); tutorial.Build(game,theme);
            teaching.SetSiblingIndex(teaching.GetSiblingIndex()-1);
            var veil = theme.Rect("Page fade",root.parent,Vector2.zero,Vector2.zero).gameObject.AddComponent<SolidShade>();
            veil.rectTransform.anchorMin = Vector2.zero;
            veil.rectTransform.anchorMax = Vector2.one;
            veil.rectTransform.offsetMin = veil.rectTransform.offsetMax = Vector2.zero;
            transition = veil.gameObject.AddComponent<ScreenTransition>();
            transition.Configure(veil);
        }
        private RectTransform PageRoot(UiPage page, bool shade = false)
        {
            var rect = theme.Rect(page.ToString(), root, Vector2.zero, new Vector2(1080, 1920));
            if(page!=UiPage.Game)rect.gameObject.AddComponent<CanvasGroup>();
            if (shade)
            {
                var image = rect.gameObject.AddComponent<SolidShade>();
                image.color = new Color(0,0,0,.72f);
            }
            pages.Add(page, rect.gameObject);
            return rect;
        }
        private RectTransform Card(Transform parent, string name, float height, float y = 0) =>
            theme.Panel(name, parent, new Vector2(0, y), new Vector2(900, height));
        public void ShowGame() { navigation.Clear(); Display(UiPage.Game); }
        public void ShowMenu()
        {
            if (game.Board.IsBusy) return;
            if (Page==UiPage.Menu) { navigation.Clear(); Display(UiPage.Menu); }
            else Transition(() => { navigation.Clear(); Display(UiPage.Menu); });
        }
        public void Transition(System.Action change)
        {
            game.Board.InputEnabled = false;
            transition.Play(change, () => game.Board.InputEnabled = Page == UiPage.Game,ActiveModal(),Page==UiPage.Menu);
        }
        public void ShowLevels() => Transition(() => Push(UiPage.Levels));
        public void ShowSettings() => Push(UiPage.Settings);
        public void ShowPause()
        {
            if (!game.Board.IsBusy) Push(UiPage.Pause);
        }
        private PanelMotion ActiveModal()
        {
            if(Page==UiPage.Game)
            {
                if(WinVisible)return resultPanel.GetComponent<PanelMotion>();
                if(DeadlockVisible)return deadlockPanel.GetComponent<PanelMotion>();
            }
            return pages[Page].GetComponent<PanelMotion>();
        }
        public void RestartFromPause()=>StartCoroutine(CloseModal(ActiveModal(),()=>{Display(navigation.Pop());game.Board.Restart();}));
        private void CloseResult(System.Action action)=>StartCoroutine(CloseModal(ActiveModal(),action));
        private System.Collections.IEnumerator CloseModal(PanelMotion modal,System.Action action)
        {
            closingModal=true;yield return modal.Exit();closingModal=false;action();
        }
        private void Push(UiPage page) { navigation.Push(Page); Display(page); }
        public void Back()
        {
            Audio.AudioManager.Instance.Play(Audio.Sfx.Back);
            if (navigation.Count > 0)
            {
                var previous=(RectTransform)pages[Page].transform;var modal=ActiveModal();
                if(modal!=null)StartCoroutine(CloseModal(modal,()=>Display(navigation.Pop())));
                else {Display(navigation.Pop());transition.BlendBack(previous,()=>game.Board.InputEnabled=Page==UiPage.Game);}
            }
            else if (Page == UiPage.Game) ShowPause();
            else if (Page == UiPage.Menu) Push(UiPage.Exit);
        }
        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) Back();
            if (DisplayedHint.HasValue && !game.Settings.reducedMotion)
            {
                float pulse = 1 + .035f*Mathf.Sin(Time.unscaledTime*4);
                hintSource.localScale = hintTarget.localScale = Vector3.one*pulse;
            }
        }
        private void Display(UiPage page)
        {
            transition.Cancel();
            Page = page;
            transition.SetCurrent((RectTransform)pages[page].transform);
            gameplayInput.interactable = gameplayInput.blocksRaycasts = page == UiPage.Game;
            bool modalPage=page==UiPage.Settings||page==UiPage.Pause||page==UiPage.Exit;
            bool menuBehind=modalPage&&navigation.Contains(UiPage.Menu);
            bool gameBehind=modalPage&&navigation.Contains(UiPage.Game);
            foreach (var pair in pages)
            {
                pair.Value.SetActive(pair.Key==page||(pair.Key==UiPage.Menu&&menuBehind)||(pair.Key==UiPage.Game&&gameBehind));
                var group=pair.Value.GetComponent<CanvasGroup>();
                if(group!=null){group.interactable=pair.Key==page;group.blocksRaycasts=pair.Key==page;}
            }
            bool boardVisible = page == UiPage.Game || gameBehind;
            game.Board.gameObject.SetActive(boardVisible);
            game.Board.InputEnabled = page == UiPage.Game && !IsTransitioning;
            if (page == UiPage.Game && game.Board.Session != null) Refresh();
            Audio.AudioManager.Instance.SwitchMusic(boardVisible ? Audio.MusicPage.Game : Audio.MusicPage.Menu);
            if (page == UiPage.Menu)
            {
                completedLabel.text = $"{game.Progress.completedLevels.Count} / {game.LevelCount}     ★ {game.Progress.TotalStars}";
                menuAnimation.Enter();
            }
            if (page == UiPage.Levels) { RefreshLevels(); levelGrid.RevealCurrent(); }
            if (page == UiPage.Settings || page == UiPage.Pause) RefreshSettings();
            var motion = pages[page].GetComponent<PanelMotion>(); if (motion != null) motion.Enter();
            if (page != UiPage.Game) ClearHint();
            UpdateHudTint();
            RefreshTutorial();
            var focus = pages[page].GetComponentsInChildren<Button>();
            foreach (var button in focus) if (button.interactable) { EventSystem.current.SetSelectedGameObject(button.gameObject); break; }
        }
        private void UpdateHudTint()
        {
            bool modal=Page==UiPage.Pause||Page==UiPage.Settings||WinVisible||DeadlockVisible;
            foreach(var feedback in hudFeedback)feedback.KeepColor=modal;
        }
        private System.Collections.IEnumerator FadeHud(bool visible)
        {
            float from=hudControls.alpha,to=visible?1:0;
            for(float time=0;time<.2f;time+=Time.unscaledDeltaTime)
            {hudControls.alpha=Mathf.Lerp(from,to,time/.2f);yield return null;}
            hudControls.alpha=to;hudMotion=null;
        }
        private void RefreshLevels()
        {
            levelSummary.text = game.Progress.TotalStars.ToString();
            levelGrid.Refresh(game.Progress,game.LevelId);
        }
        public void ToggleSound()
        {
            game.Settings.soundEnabled=!game.Settings.soundEnabled;
            game.Settings.musicEnabled=game.Settings.soundEnabled;
            game.SaveSettings();RefreshSettings();
        }
        private void RefreshSettings()
        {
            foreach (var panel in settingsPanels) panel.Refresh();
        }
        private RectTransform HintMarker(Transform parent,string name,string key)
        {
            var marker = theme.Rect(name,parent,Vector2.zero,new Vector2(520,200));
            theme.Image("Ring",marker,theme.Art("tap_ripple"),Vector2.zero,new Vector2(520,200));
            var label = theme.Panel("Badge",marker,new Vector2(0,-130),new Vector2(180,62),theme.green);
            label.GetComponent<Image>().raycastTarget = false;
            theme.Label("Direction",label,S(key),Vector2.zero,new Vector2(170,54),32,UiTheme.Cream);
            marker.gameObject.SetActive(false); return marker;
        }
        private void ClearHint()
        {
            DisplayedHint = null; hintMessageKey = null;
            hintSource.gameObject.SetActive(false); hintTarget.gameObject.SetActive(false);
        }
        public void HintMessage(string key) { ClearHint(); hintMessageKey = key; Refresh(); }
        public void ShowHint(Move move)
        {
            ClearHint(); DisplayedHint = move;
            foreach (var pair in new[] { (hintSource,move.From),(hintTarget,move.To) })
            {
                pair.Item1.gameObject.SetActive(true);
                var perch = game.Board.Locations[pair.Item2];
                pair.Item1.anchoredPosition = (Vector2)(perch.transform.localPosition+game.Board.transform.localPosition)*100+Vector2.up*35;
                var ring = pair.Item1.Find("Ring").GetComponent<Image>(); ring.preserveAspect = false;
                ring.rectTransform.sizeDelta = new Vector2(perch.IsBasket ? 220 : perch.Capacity*80+65,220);
            }
            Audio.AudioManager.Instance.Play(Audio.Sfx.Hint);
            Refresh();
        }
        public void RefreshTutorial() { if (tutorial != null && game.Board.Session != null) tutorial.Refresh(); }
        public void Relocalize()
        {
            foreach (var label in GetComponentsInChildren<LocalizedText>(true)) label.Refresh();
            RefreshSettings(); RefreshLevels(); Refresh();
        }
        public void Refresh()
        {
            var board = game.Board;
            bool resultWasVisible = resultPanel.activeSelf;
            bool won = Core.Rules.IsWon(board.Session.State), deadlock = Core.Rules.IsDeadlock(board.Session.State);
            WinVisible = won && !board.IsBusy; DeadlockVisible = deadlock && !board.IsBusy;
            levelLabel.text = game.LevelId.ToString();
            if (board.IsBusy) ClearHint();
            instruction.text = hintMessageKey != null ? S(hintMessageKey) : DisplayedHint.HasValue ? Strings.Format("hintMove",DisplayedHint.Value.From+1,DisplayedHint.Value.To+1) : S(board.Selected >= 0 ? "target" : "hint");
            hintButtonLabel.text = (HintService.Limit-game.Progress.ForLevel(game.LevelId).hintsUsed).ToString();
            instruction.transform.parent.gameObject.SetActive(game.Tutorial.Active && game.Board.EntranceComplete && !TutorialVisible);
            hintButton.interactable = !board.IsBusy && !game.HintBusy && !won && !deadlock && game.Progress.ForLevel(game.LevelId).hintsUsed < HintService.Limit;
            RefreshTutorial();
            instruction.transform.parent.gameObject.SetActive(game.Tutorial.Active && game.Board.EntranceComplete && !TutorialVisible);
            crateLabels.Refresh(game,theme);
            undo.interactable = !board.IsBusy && !won && !deadlock && board.Session.UndoCount > 0;
            restart.interactable = pause.interactable = !board.IsBusy && !won && !deadlock;
            if(resultWasVisible!=WinVisible)
            {
                if(hudMotion!=null)StopCoroutine(hudMotion);
                hudMotion=StartCoroutine(FadeHud(!WinVisible));
                board.FadePresentation(!WinVisible);
            }
            hudControls.interactable=hudControls.blocksRaycasts=!WinVisible&&!DeadlockVisible;
            UpdateHudTint();
            resultPanel.SetActive(WinVisible);
            deadlockPanel.SetActive(DeadlockVisible);
            bool canUndoDeadlock=board.Session.UndoCount>0;
            deadlockUndo.gameObject.SetActive(canUndoDeadlock);
            deadlockPanel.transform.Find("Deadlock card/restart").GetComponent<RectTransform>().anchoredPosition=new Vector2(canUndoDeadlock?150:0,-130);
            if (!WinVisible) return;
            winnerBird.sprite=theme.frontBirds[board.LastHomedSpecies].stand;
            bool finished = game.LevelId == game.LevelCount;
            int earned = StarRating.Calculate(game.Progress.ForLevel(game.LevelId).undos,game.Progress.ForLevel(game.LevelId).restarts);
            primary.gameObject.SetActive(!finished);
            resultPanel.transform.Find("Result content/Buttons/replay").GetComponent<RectTransform>().anchoredPosition = new Vector2(-150,-420);
            resultPanel.transform.Find("Result content/Buttons/menu").GetComponent<RectTransform>().anchoredPosition = new Vector2(150,-420);
            if (!resultWasVisible) { celebration.Play(earned,game.Settings.reducedMotion); EventSystem.current.SetSelectedGameObject(finished ? resultPanel.transform.Find("Result content/Buttons/menu").gameObject : primary.gameObject); }
        }
    }
}
