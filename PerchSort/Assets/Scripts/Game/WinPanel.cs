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
        private UiBirdLife leftLife, rightLife;
        private float nextHop;
        private bool leftHops;
        private TextMeshProUGUI rewardLabel, chorusLabel;
        private GameObject chorusBonus;
        private Image dim, rays, leftBird, rightBird, heroBird;
        private readonly Image[] stars = new Image[GameLayout.StarCount];
        private readonly Image[] slots = new Image[GameLayout.StarCount];
        private PopupMotion presentation;
        private Button next, replay, home;
        public bool IsOpen => gameObject.activeSelf;
        private static readonly Rect PanelBounds = new Rect(220, 650, 640, 850);
        private static Vector2 StarPoint(int index) => new Vector2(160 + index * 160, 250);

        public void Configure(GameRoot root, GameArt assets, TMP_FontAsset font)
        {
            art = assets; owner = root;
            dim = gameObject.AddComponent<Image>(); dim.raycastTarget = true;
            presentation = gameObject.AddComponent<PopupMotion>();
            rays = GameUI.Fit(transform, "Win light rays", art.Get("light_rays"), PanelBounds.center, Vector2.one * 1400);
            leftBird = Bird("left_bird", "sky"); rightBird = Bird("right_bird", "berry"); heroBird = Bird("hero_bird", "sunny");
            leftLife = leftBird.GetComponent<UiBirdLife>(); rightLife = rightBird.GetComponent<UiBirdLife>();
            var panel = GameUI.Surface(transform, "Win panel", art, "panel_wood", PanelBounds);
            panel.raycastTarget = true;
            body = PopupMotion.Lift(panel);
            ribbon = GameUI.Fit(body, "Ribbon", art.Get("ribbon_green"), new Vector2(320, 35), new Vector2(600, 180)).rectTransform;
            for (int i = 0; i < GameLayout.StarCount; i++)
            {
                slots[i] = GameUI.Image(body, "Star slot " + i, art.Get("star_empty"), StarPoint(i), Vector2.one * 140);
                stars[i] = GameUI.Image(body, "Earned star " + i, art.Get("star_full"), StarPoint(i), Vector2.one * 140);
            }
            rewardChip = GameUI.Surface(body, "Reward chip", art, "stat_pill", new Rect(160, 374, 320, 88)).rectTransform;
            GameUI.Image(rewardChip, "Reward coin", art.Icon("coin"), new Vector2(56, 44), Vector2.one * 56);
            rewardLabel = TextFit.Create(rewardChip, "Coin reward", "+0", font, new Rect(96, 6, 200, 76), 54, rewardChip);
            rewardLabel.color = new Color32(101, 31, 12, 255);
            chorusBonus = GameUI.Surface(body, "Chain bonus", art, "stat_pill", new Rect(206, 482, 228, 64)).gameObject;
            GameUI.Fit(chorusBonus.transform, "Chain", art.Get("chain_bonus"), new Vector2(40, 32), Vector2.one * 44);
            chorusLabel = TextFit.Create(chorusBonus.transform, "Chorus coin bonus", "", font, new Rect(76, 4, 134, 56), 36);
            next = PopupMotion.PrimaryBar(body, art, "Next level", "icon_next_arrow", new Vector2(320, 626), () => Close(root.NextLevel));
            replay = GameUI.Button(body, art, "Replay level", "icon_undo", new Rect(210, 726, 90, 90), () => Close(root.Restart));
            home = GameUI.Button(body, art, "Home", "icon_home", new Rect(340, 726, 90, 90), () => Close(root.OpenMap));
            gameObject.SetActive(false);
        }
        private Image Bird(string part, string species)
        {
            Vector2 center = part == "hero_bird" ? new Vector2(540, 570) :
                new Vector2(part == "left_bird" ? 235 : 845, 1068);
            var bird = GameUI.Fit(transform, part, art.Get(species + "_idle"), center, Vector2.one * (part == "hero_bird" ? 240 : 210));
            bird.gameObject.AddComponent<UiBirdLife>().Configure(bird, art, species, part != "hero_bird", part == "hero_bird", feetPivot: false);
            return bird;
        }
        public void Show(int earnedStars, int reward, int chorusCoins) => Show(earnedStars, reward, chorusCoins, null);
        // The optional callback synchronizes an already-committed HUD count, never a grant.
        public void Show(int earnedStars, int reward, int chorusCoins, Action coinFlightStarted)
        {
            presentation.Cancel(); transform.SetAsLastSibling(); gameObject.SetActive(true); presentation.Begin(dim);
            int count = Mathf.Clamp(earnedStars, 0, stars.Length);
            rewardLabel.text = "+0"; chorusLabel.text = "+" + chorusCoins; chorusBonus.SetActive(chorusCoins > 0);
            next.interactable = replay.interactable = home.interactable = true;
            rays.transform.SetAsFirstSibling();
            presentation.Fade(rays.rectTransform, 0); presentation.Rotate(rays.rectTransform, 60);
            // Establish back-to-front order before Emerge hands side birds to the foreground.
            leftBird.transform.SetAsLastSibling();
            rightBird.transform.SetAsLastSibling();
            heroBird.transform.SetAsLastSibling();
            body.SetAsLastSibling();
            presentation.Track(body);
            presentation.Play(0.35f, t => body.localScale = Vector3.one * Mathf.LerpUnclamped(0.6f, 1, t), ease: Ease.OutBack);
            presentation.Track(ribbon); Vector2 ribbonRest = ribbon.anchoredPosition;
            presentation.Play(0.30f, t => ribbon.anchoredPosition = ribbonRest + Vector2.up * (80 * (1 - t)), delay: 0.10f);
            for (int i = 0; i < stars.Length; i++)
            {
                int index = i;
                stars[i].gameObject.SetActive(i < count); slots[i].gameObject.SetActive(i >= count);
                var star = i < count ? stars[i] : slots[i];
                presentation.Track(star.rectTransform);
                presentation.Play(0.35f, t =>
                {
                    float scale = t < 0.65f ? 1.25f * (1 - Mathf.Pow(1 - t / 0.65f, 3))
                        : Mathf.Lerp(1.25f, 1, Mathf.SmoothStep(0, 1, (t - 0.65f) / 0.35f));
                    star.rectTransform.localScale = Vector3.one * scale;
                }, () =>
                {
                    if (index < count)
                    {
                        owner.PlaySound(index == 0 ? Sfx.Star1 : index == 1 ? Sfx.Star2 : Sfx.Star3);
                        presentation.Sparkles(art, body, StarPoint(index), 100, 0);
                    }
                }, 0.45f + i * 0.18f);
            }
            presentation.Pop(rewardChip, 0.8f); presentation.Count(rewardLabel, reward, 0.8f);
            if (reward > 0)
            {
                presentation.Coins(art, transform, PanelBounds.position + new Vector2(216, 418), GameLayout.CoinIcon.center, 1.0f);
                if (coinFlightStarted != null) presentation.After(1.0f, coinFlightStarted);
            }
            if (chorusCoins > 0)
            {
                presentation.Pop((RectTransform)chorusBonus.transform, 1.0f);
                presentation.Count(chorusLabel, chorusCoins, 1.0f);
            }
            Emerge(leftBird, "sky", 0.15f, -1);
            Emerge(rightBird, "berry", 0.25f, 1);
            Vector2 heroPosition = heroBird.rectTransform.anchoredPosition;
            Vector2 heroDestination = new Vector2(heroPosition.x, -heroPosition.y);
            presentation.FlyBird(art, heroBird, "sunny", new Vector2(heroDestination.x, -240), heroDestination, 0.10f,
                () => heroBird.GetComponent<UiBirdLife>().Begin(), duration: 0.6f);
            presentation.Confetti(art, transform, 0);
            presentation.ButtonsLast(body, 1.16f);
            nextHop = Time.time + 0.95f + UnityEngine.Random.Range(1.4f, 2.2f); leftHops = true;
        }
        private void Update()
        {
            if (GameMotion.Instant || Time.time < nextHop || leftLife == null) return;
            (leftHops ? leftLife : rightLife).Play("hop");
            leftHops = !leftHops;
            nextHop = Time.time + UnityEngine.Random.Range(1.4f, 2.2f);
        }
        private void Close(Action action)
        {
            nextHop = float.PositiveInfinity;
            presentation.Close(action);
        }
        private void Emerge(Image bird, string species, float delay, float direction)
        {
            Vector2 position = bird.rectTransform.anchoredPosition;
            Vector2 destination = new Vector2(position.x, -position.y);
            Vector2 start = new Vector2(direction < 0 ? -bird.rectTransform.rect.width : GameLayout.DesignWidth + bird.rectTransform.rect.width,
                destination.y - 180);
            presentation.FlyBird(art, bird, species, start, destination, delay, () =>
            {
                bird.transform.SetAsLastSibling();
                bird.GetComponent<UiBirdLife>().Begin();
            }, duration: 0.6f);
        }
        public void Hide()
        {
            if (presentation != null) presentation.Cancel();
            gameObject.SetActive(false);
        }
        private void OnDisable() { if (presentation != null) presentation.Cancel(); }
    }
}
