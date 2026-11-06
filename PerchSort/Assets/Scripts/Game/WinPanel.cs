using System;
using LitMotion;
using PerchSort.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.Game
{
    public sealed class WinPanel : MonoBehaviour
    {
        private GameArt art;
        private GameRoot owner;
        private RectTransform body, ribbon, rewardChip;
        private PaintedNumber rewardLabel, movesLabel;
        private Image dim, rays, heroBird;
        private readonly Image[] stars = new Image[GameLayout.StarCount];
        private readonly Image[] slots = new Image[GameLayout.StarCount];
        private readonly PaintedNumber[] thresholds = new PaintedNumber[GameLayout.StarCount];
        private PopupMotion presentation;
        private Button next, replay, home;
        public bool IsOpen => gameObject.activeSelf;
        private static readonly Rect PanelBounds = new Rect(110, 860, 860, 500);
        private static Vector2 StarPoint(int index) => new Vector2(230 + index * 200, 130);

        public void Configure(GameRoot root, GameArt assets, TMP_FontAsset font)
        {
            art = assets; owner = root;
            dim = gameObject.AddComponent<Image>(); dim.raycastTarget = true;
            ModalLayer.Configure(gameObject);
            presentation = gameObject.AddComponent<PopupMotion>();
            rays = GameUI.Fit(transform, "Win light rays", art.Get("light_rays"), new Vector2(540, 960), Vector2.one * 1200);
            var panel = GameUI.Surface(transform, "Win panel", art, "panel_wood", PanelBounds);
            panel.raycastTarget = true; panel.gameObject.AddComponent<EmptyClick>();
            body = PopupMotion.Lift(panel);
            ribbon = PanelsView.Title(body, "label_title_well_done", new Vector2(430, 0)).rectTransform;
            heroBird = GameUI.Fit(transform, "hero_bird", art.Get("sunny_front"), new Vector2(540, 746), Vector2.one * 270);
            heroBird.transform.SetSiblingIndex(body.GetSiblingIndex());
            for (int i = 0; i < stars.Length; i++)
            {
                slots[i] = GameUI.Fit(body, "Star slot " + i, art.Get("star_empty"), StarPoint(i), Vector2.one * 156);
                stars[i] = GameUI.Fit(body, "Earned star " + i, art.Get("star_full"), StarPoint(i), Vector2.one * 156);
                var pill = GameUI.Surface(body, "Star threshold pill", art, "stat_pill", new Rect(156 + i * 200, 216, 148, 58));
                thresholds[i] = PanelsView.Number(pill.transform, "Star threshold", "0", new Vector2(74, 29), new Vector2(140, 58), 30);
            }
            var moves = GameUI.Surface(body, "Moves chip", art, "stat_pill", new Rect(280, 298, 300, 76));
            GameUI.Fit(moves.transform, "Moves footprints", art.Get("footprints"), new Vector2(56, 38), Vector2.one * 50);
            movesLabel = PanelsView.Number(moves.transform, "Moves", "0", new Vector2(190, 38), new Vector2(190, 76), 38);
            rewardChip = GameUI.Surface(body, "Reward chip", art, "stat_pill", new Rect(230, 396, 400, 80)).rectTransform;
            GameUI.Fit(rewardChip, "Reward coin", art.Get("coin"), new Vector2(40, 40), Vector2.one * 92);
            rewardLabel = PanelsView.Number(rewardChip, "Coin reward", "+0", new Vector2(236, 40), new Vector2(300, 80), 42);
            Vector2 row = new Vector2(540, PanelBounds.yMax + 110);
            replay = GameUI.Button(transform, art, "Replay level", "icon_undo", new Rect(row.x - 210, row.y - 50, 100, 100), () => Close(root.Restart));
            next = PopupMotion.PrimaryBar(transform, art, "Next level", "icon_play", row, () => Close(root.NextLevel));
            home = GameUI.Button(transform, art, "Home", "icon_home", new Rect(row.x + 110, row.y - 50, 100, 100), () => Close(root.OpenMap));
            gameObject.SetActive(false);
        }

        public void Show(int earnedStars, int reward, int chorusCoins) => Show(earnedStars, reward, chorusCoins, null);
        // Rewards are already committed by the owner; this view only displays their total.
        public void Show(int earnedStars, int reward, int chorusCoins, Action coinFlightStarted)
        {
            ShowContent(earnedStars, reward, owner.Session.MoveCount, owner.Session.Par, true, coinFlightStarted);
        }

        private void ShowContent(int earnedStars, int reward, int moves, int par, bool nextEnabled, Action coinFlightStarted)
        {
            presentation.Cancel(); transform.SetAsLastSibling(); gameObject.SetActive(true); presentation.Begin(dim);
            int count = Mathf.Clamp(earnedStars, 0, stars.Length);
            movesLabel.SetValue(moves); rewardLabel.SetText("+0");
            thresholds[0].SetValue(par); thresholds[1].SetValue(par + 3); thresholds[2].SetText("∞");
            next.interactable = nextEnabled; replay.interactable = home.interactable = true;
            presentation.Fade(rays.rectTransform, 0); presentation.Rotate(rays.rectTransform, 60);
            ModalLayer.Enter(presentation, body);
            presentation.Track(ribbon); Vector2 rest = ribbon.anchoredPosition;
            presentation.Play(0.18f, t => ribbon.anchoredPosition = rest + Vector2.up * (36 * (1 - t)), ease: Ease.OutCubic);
            owner.Collection.EquippedOutfits.TryGetValue("sunny", out string outfit);
            presentation.FrontBird(art, heroBird, art.FrontKey("sunny", outfit));
            for (int i = 0; i < stars.Length; i++)
            {
                int index = i;
                stars[i].gameObject.SetActive(i < count); slots[i].gameObject.SetActive(i >= count);
                var star = i < count ? stars[i] : slots[i];
                presentation.Track(star.rectTransform);
                presentation.Play(0.28f, t => star.rectTransform.localScale = Vector3.one * Mathf.LerpUnclamped(0.7f, 1, t), () =>
                {
                    if (index < count)
                    {
                        owner.PlaySound(index == 0 ? Sfx.Star1 : index == 1 ? Sfx.Star2 : Sfx.Star3);
                        presentation.Sparkles(art, body, StarPoint(index), 90, 0);
                    }
                }, 0.18f + i * 0.18f, Ease.OutBack);
            }
            presentation.Count(rewardLabel, reward, 0.7f);
            if (reward > 0)
            {
                presentation.Coins(art, transform, PanelBounds.position + new Vector2(270, 436), GameLayout.CoinIcon.center, 1);
                if (coinFlightStarted != null) presentation.After(1, coinFlightStarted);
            }
            presentation.Confetti(art, transform, 0);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void Preview(int stars, int reward, int moves, int par, bool nextEnabled = true) =>
            ShowContent(stars, reward, moves, par, nextEnabled, null);
#endif
        private void Close(Action action) => presentation.Close(action);
        public void Hide()
        {
            if (presentation != null) presentation.Cancel();
            gameObject.SetActive(false);
        }
        private void OnDisable() { if (presentation != null) presentation.Cancel(); }
    }
}
