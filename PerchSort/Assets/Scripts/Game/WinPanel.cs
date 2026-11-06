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
        private Image dim, heroBird;
        private readonly Image[] stars = new Image[GameLayout.StarCount];
        private readonly Image[] slots = new Image[GameLayout.StarCount];
        private readonly PaintedNumber[] thresholds = new PaintedNumber[GameLayout.StarCount];
        private PopupMotion presentation;
        private Button next, replay, home;
        public bool IsOpen => gameObject.activeSelf;
        private static readonly Rect PanelBounds = new Rect(110, 856, 860, 514);
        private static Vector2 StarPoint(int index) => new Vector2(229.5f + index * 200, 127);

        public void Configure(GameRoot root, GameArt assets, TMP_FontAsset font)
        {
            art = assets; owner = root;
            dim = gameObject.AddComponent<Image>(); dim.raycastTarget = true;
            ModalLayer.Configure(gameObject);
            presentation = gameObject.AddComponent<PopupMotion>();

            Rect R(string key) => GameLayout.Screen("board_win_panel", key);
            Rect L(string key) => GameUI.Local(R(key), PanelBounds);
            var panel = GameUI.MeasuredSurface(transform, "Win panel", art, "panel_wood", PanelBounds);
            panel.raycastTarget = true; panel.gameObject.AddComponent<EmptyClick>();
            body = PopupMotion.Lift(panel);
            Rect heroRect = R("hero");
            var hero = GameUI.Rect(transform, "hero_bird", heroRect.center, heroRect.size);
            hero.gameObject.AddComponent<RectMask2D>();
            hero.SetSiblingIndex(body.GetSiblingIndex());
            var heroSprite = art.Measured(art.Get("sunny_cheer_05"));
            var heroSize = new Vector2(heroRect.width, heroRect.width * heroSprite.rect.height / heroSprite.rect.width);
            heroBird = GameUI.Image(hero, "Celebrating bird", heroSprite, heroSize * 0.5f, heroSize);
            ribbon = PanelsView.Title(body, "label_title_well_done", L("win_ribbon"), art).rectTransform;
            for (int i = 0; i < stars.Length; i++)
            {
                Rect star = L("win_star_" + (i + 1));
                slots[i] = GameUI.MeasuredImage(body, "Star slot " + i, art, art.Get("star_empty"), star);
                stars[i] = GameUI.MeasuredImage(body, "Earned star " + i, art, art.Get("star_full"), star);
                GameUI.MeasuredSurface(body, "Star threshold pill " + i, art, "stat_pill", L("threshold_pill_" + (i + 1)));
                Rect value = L("threshold_value_" + (i + 1));
                thresholds[i] = PanelsView.Number(body, "Star threshold " + i, "0", value.center, value.size, value.height);
            }
            GameUI.MeasuredSurface(body, "Moves chip", art, "stat_pill", L("move_pill"));
            GameUI.MeasuredImage(body, "Moves footprints", art, art.Get("footprints"), L("move_footprint"));
            Rect movesRect = GameUI.Local(new Rect(557, 1154, 80, 44), PanelBounds);
            movesLabel = PanelsView.Number(body, "Moves", "0", movesRect.center, movesRect.size, movesRect.height);
            // The two approved reward plates differ where the coin occludes their left edge.
            rewardChip = GameUI.MeasuredSurface(body, "Reward chip", art, "stat_pill",
                GameUI.Local(new Rect(397, 1237, 303, 93), PanelBounds)).rectTransform;
            GameUI.MeasuredImage(body, "Reward coin", art, art.Get("coin"), L("reward_coin"));
            Rect rewardRect = L("reward_value");
            rewardLabel = PanelsView.Number(body, "Coin reward", "+0", rewardRect.center, rewardRect.size, rewardRect.height);
            // Shared secondary diameters stay identical in every result state.
            replay = GameUI.MeasuredButton(transform, art, "Replay level", "icon_replay",
                new Rect(334, 1432, 96, 96), R("action_icon_retry"), () => Close(root.Restart));
            next = GameUI.MeasuredButton(transform, art, "Next level", "icon_play",
                R("action_play"), R("action_icon_play"), () => Close(root.NextLevel), true);
            home = GameUI.MeasuredButton(transform, art, "Home", "icon_home",
                new Rect(654, 1432, 96, 96), R("action_icon_home"), () => Close(root.OpenMap));
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
            thresholds[2].Rect.GetChild(0).localScale = Vector3.one * (100f / 64f);
            next.interactable = nextEnabled; replay.interactable = home.interactable = true;

            ModalLayer.Enter(presentation, body);
            presentation.Track(ribbon); Vector2 rest = ribbon.anchoredPosition;
            presentation.Play(0.18f, t => ribbon.anchoredPosition = rest + Vector2.up * (36 * (1 - t)), ease: Ease.OutCubic);
            owner.Collection.EquippedOutfits.TryGetValue("sunny", out string outfit);
            presentation.CelebratingBird(art, heroBird, art.FrontKey("sunny", outfit));
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
            GameUI.DumpPopupRects(transform, earnedStars == 2 ? "board_win_panel_two_stars" : "board_win_panel");
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
