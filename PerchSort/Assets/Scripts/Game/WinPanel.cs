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
        private RectTransform body, ribbon, rewardChip, nextPresentation;
        private GameMotion nextPulse;
        private TextMeshProUGUI rewardLabel, chorusLabel;
        private GameObject chorusBonus;
        private Image dim, rays, leftBird, rightBird, heroBird;
        private readonly Image[] stars = new Image[GameLayout.StarCount];
        private readonly Image[] slots = new Image[GameLayout.StarCount];
        private PopupMotion presentation;
        private Button next, replay, home;
        public bool IsOpen => gameObject.activeSelf;

        public void Configure(GameRoot root, GameArt assets, TMP_FontAsset font)
        {
            art = assets; owner = root;
            dim = gameObject.AddComponent<Image>();
            dim.color = new Color(0, 0, 0, GameLayout.PanelDimAlpha); dim.raycastTarget = true;
            presentation = gameObject.AddComponent<PopupMotion>();
            rays = GameUI.Fit(transform, "Win light rays", art.Get("light_rays"), GameLayout.WinPanel.center, Vector2.one * 1400);
            leftBird = Bird("left_bird", "sky"); rightBird = Bird("right_bird", "berry"); heroBird = Bird("hero_bird", "sunny");
            body = GameUI.Surface(transform, "Win panel", art, "panel_wood", GameLayout.WinPanel).rectTransform;
            body.GetComponent<Image>().raycastTarget = true;
            ribbon = GameUI.Fit(body, "Ribbon", art.Get("ribbon_green"), GameLayout.InParent(GameLayout.WinPanel, GameLayout.Ribbon.center), GameLayout.Ribbon.size).rectTransform;
            for (int i = 0; i < GameLayout.StarCount; i++)
            {
                Rect bounds = GameLayout.Star(i); Vector2 point = GameLayout.InParent(GameLayout.WinPanel, bounds.center);
                slots[i] = GameUI.Image(body, "Star slot " + i, art.Get("star_empty"), point, bounds.size);
                stars[i] = GameUI.Image(body, "Earned star " + i, art.Get("star_full"), point, bounds.size);
            }
            rewardChip = GameUI.Rect(body, "Reward chip", GameLayout.RewardChip.center - GameLayout.WinPanel.position, GameLayout.RewardChip.size);
            GameUI.Surface(rewardChip, "Reward surface", art, "stat_pill", new Rect(Vector2.zero, GameLayout.RewardChip.size));
            GameUI.Image(rewardChip, "Reward coin", art.Icon("coin"), GameLayout.RewardCoin.center - GameLayout.RewardChip.position, GameLayout.RewardCoin.size);
            rewardLabel = GameUI.Text(rewardChip, "Coin reward", "+0", font, GameLayout.RewardText.center - GameLayout.RewardChip.position, GameLayout.RewardText.size, GameLayout.RewardFontSize);
            rewardLabel.alignment = TextAlignmentOptions.MidlineLeft;
            chorusBonus = GameUI.Rect(body, "Chain bonus", GameLayout.InParent(GameLayout.WinPanel, GameLayout.ChorusBonus.center), GameLayout.ChorusBonus.size).gameObject;
            // The approved chain reward is a readable coin line, not the tiny feather mechanic badge.
            GameUI.Fit(chorusBonus.transform, "Chain", art.Get("chain_bonus"), new Vector2(22, 32), Vector2.one * 72);
            chorusLabel = GameUI.Text(chorusBonus.transform, "Chorus coin bonus", "", font, new Vector2(126, 32), new Vector2(140, 72), 48);
            Rect nextBounds = Local(GameLayout.NextButton);
            nextPresentation = GameUI.Rect(body, "Next button presentation", nextBounds.center, nextBounds.size);
            nextPulse = nextPresentation.gameObject.AddComponent<GameMotion>();
            next = GameUI.Button(nextPresentation, art, "Next level", "icon_next_arrow", new Rect(Vector2.zero, nextBounds.size), () => Close(root.NextLevel), true);
            replay = GameUI.Button(body, art, "Replay level", "icon_replay", Local(GameLayout.ReplayButton), () => Close(root.Restart));
            home = GameUI.Button(body, art, "Home", "icon_home", Local(GameLayout.HomeButton), () => Close(root.OpenMap));
            gameObject.SetActive(false);
        }
        private Image Bird(string part, string species)
        {
            Rect bounds = GameLayout.Screen("win", part);
            return GameUI.Fit(transform, part, art.Get(species + "_idle"), bounds.center, bounds.size * 1.6f);
        }
        private static Rect Local(Rect rect) => new Rect(rect.position - GameLayout.WinPanel.position, rect.size);
        public void Show(int earnedStars, int reward, int chorusCoins) => Show(earnedStars, reward, chorusCoins, null);
        // The optional callback synchronizes an already-committed HUD count, never a grant.
        public void Show(int earnedStars, int reward, int chorusCoins, Action coinFlightStarted)
        {
            presentation.Cancel(); transform.SetAsLastSibling(); gameObject.SetActive(true); presentation.Begin(dim);
            int count = Mathf.Clamp(earnedStars, 0, stars.Length);
            rewardLabel.text = "+0"; chorusLabel.text = "+" + chorusCoins; chorusBonus.SetActive(chorusCoins > 0);
            next.interactable = replay.interactable = home.interactable = true;
            rays.transform.SetAsFirstSibling();
            presentation.Fade(rays.rectTransform, 0); presentation.Rotate(rays.rectTransform, 24);
            // Establish back-to-front order before Emerge hands side birds to the foreground.
            leftBird.transform.SetAsLastSibling();
            rightBird.transform.SetAsLastSibling();
            heroBird.transform.SetAsLastSibling();
            body.SetAsLastSibling();
            presentation.Track(body); Vector2 rest = body.anchoredPosition;
            presentation.Play(0.55f, t =>
            {
                body.anchoredPosition = rest + Vector2.up * (650 * (1 - t));
                body.localScale = Vector3.one * Mathf.LerpUnclamped(0.7f, 1, t);
                body.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * Mathf.PI * 4) * 3 * (1 - t));
            }, delay: 0.12f, ease: Ease.OutBack);
            presentation.Track(ribbon);
            presentation.Play(0.35f, t => ribbon.localScale = new Vector3(t, 1, 1), delay: 0.55f, ease: Ease.OutBack);
            for (int i = 0; i < stars.Length; i++)
            {
                int index = i;
                stars[i].gameObject.SetActive(i < count); slots[i].gameObject.SetActive(i >= count);
                var star = i < count ? stars[i] : slots[i];
                presentation.Track(star.rectTransform); Vector2 destination = star.rectTransform.anchoredPosition;
                presentation.Play(0.32f, t =>
                {
                    star.rectTransform.anchoredPosition = destination + Vector2.down * (260 * (1 - t));
                    star.rectTransform.localScale = Vector3.one * t;
                    star.rectTransform.localRotation = Quaternion.Euler(0, 0, (1 - t) * -25);
                }, () =>
                {
                    if (index < count)
                    {
                        owner.PlaySound(index == 0 ? Sfx.Star1 : index == 1 ? Sfx.Star2 : Sfx.Star3);
                        presentation.Sparkles(art, body, GameLayout.Star(index).center - GameLayout.WinPanel.position, 100, 0);
                    }
                }, 0.8f + i * 0.2f, Ease.OutBack);
            }
            presentation.Pop(rewardChip, 1.45f); presentation.Count(rewardLabel, reward, 1.5f);
            if (reward > 0)
            {
                presentation.Coins(art, transform, GameLayout.RewardCoin.center, GameLayout.CoinIcon.center, 1.55f);
                if (coinFlightStarted != null) presentation.After(1.55f, coinFlightStarted);
            }
            if (chorusCoins > 0) presentation.Pop((RectTransform)chorusBonus.transform, 1.7f);
            Emerge(leftBird, "sky", new Vector2(GameLayout.WinPanel.x - 8, GameLayout.Screen("win", "left_bird").center.y), 1.8f, -1);
            Emerge(rightBird, "berry", new Vector2(GameLayout.WinPanel.xMax + 8, GameLayout.Screen("win", "right_bird").center.y), 1.9f, 1);
            Rect hero = GameLayout.Screen("win", "hero_bird");
            presentation.FlyBird(art, heroBird, "sunny", new Vector2(hero.center.x, -240), hero.center, 0.6f,
                () => { heroBird.sprite = art.Get("sunny_cheer"); presentation.Bob(heroBird, 8); });
            presentation.Confetti(art, transform, 1.7f);
            presentation.ButtonsLast(body, 2.2f);
            presentation.Track(nextPresentation);
            // NEXT is first: its 2.2 s + 0.23 s pop finishes before the wrapper starts pulsing.
            presentation.After(2.44f, StartNextPulse);
        }
        private void StartNextPulse()
        {
            if (GameMotion.Instant) return;
            nextPulse.Loop(1.6f, t => nextPresentation.localScale = Vector3.one * (1 + 0.0125f * (1 - Mathf.Cos(t * Mathf.PI * 2))));
        }
        private void Close(Action action)
        {
            nextPulse.CancelAll(); nextPresentation.localScale = Vector3.one;
            presentation.Close(action);
        }
        private void Emerge(Image bird, string species, Vector2 destination, float delay, float direction)
        {
            presentation.Track(bird.rectTransform); var canvas = bird.GetComponent<CanvasGroup>();
            Vector2 start = destination - Vector2.right * (direction * 190);
            Sprite cheer = art.Get(species + "_cheer"); bool emerged = false;
            presentation.Play(0.38f, t =>
            {
                canvas.alpha = Mathf.Clamp01(t * 6);
                Vector2 point = Vector2.Lerp(start, destination, t) + Vector2.up * (-35 * Mathf.Sin(t * Mathf.PI));
                bird.rectTransform.anchoredPosition = new Vector2(point.x, -point.y);
                bird.sprite = cheer;
                if (t > 0.45f && !emerged) { emerged = true; bird.transform.SetAsLastSibling(); }
            }, () => presentation.Bob(bird, 12, direction > 0 ? 0.3f : 0), delay);
        }
        public void Hide()
        {
            if (presentation != null) presentation.Cancel();
            gameObject.SetActive(false);
        }
        private void OnDisable() { if (presentation != null) presentation.Cancel(); }
    }
}
