using System;
using LitMotion;
using PerchSort.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public sealed class WinPanel : MonoBehaviour
    {
        private V5Art art;
        private V5GameRoot owner;
        private RectTransform body, ribbon, rewardChip;
        private TextMeshProUGUI rewardLabel, chorusLabel;
        private GameObject chorusBonus;
        private Image dim, rays, leftBird, rightBird, heroBird;
        private readonly Image[] stars = new Image[V7Layout.StarCount];
        private readonly Image[] slots = new Image[V7Layout.StarCount];
        private V7PopupMotion presentation;
        private Button next, replay, home;
        public bool IsOpen => gameObject.activeSelf;

        public void Configure(V5GameRoot root, V5Art assets, TMP_FontAsset font)
        {
            art = assets; owner = root;
            dim = gameObject.AddComponent<Image>();
            dim.color = new Color(0, 0, 0, V7Layout.PanelDimAlpha); dim.raycastTarget = true;
            presentation = gameObject.AddComponent<V7PopupMotion>();
            rays = V5UI.Fit(transform, "Win light rays", art.Get("light_rays"), V7Layout.WinPanel.center, Vector2.one * 1400);
            leftBird = Bird("left_bird", "sky"); rightBird = Bird("right_bird", "berry"); heroBird = Bird("hero_bird", "sunny");
            body = V5UI.Surface(transform, "Win panel", art, "panel_wood", V7Layout.WinPanel).rectTransform;
            body.GetComponent<Image>().raycastTarget = true;
            ribbon = V5UI.Fit(body, "Ribbon", art.Get("ribbon_green"), V7Layout.InParent(V7Layout.WinPanel, V7Layout.Ribbon.center), V7Layout.Ribbon.size).rectTransform;
            for (int i = 0; i < V7Layout.StarCount; i++)
            {
                Rect bounds = V7Layout.Star(i); Vector2 point = V7Layout.InParent(V7Layout.WinPanel, bounds.center);
                slots[i] = V5UI.Image(body, "Star slot " + i, art.Get("star_empty"), point, bounds.size);
                stars[i] = V5UI.Image(body, "Earned star " + i, art.Get("star_full"), point, bounds.size);
            }
            rewardChip = V5UI.Rect(body, "Reward chip", V7Layout.RewardChip.center - V7Layout.WinPanel.position, V7Layout.RewardChip.size);
            V5UI.Surface(rewardChip, "Reward surface", art, "stat_pill", new Rect(Vector2.zero, V7Layout.RewardChip.size));
            V5UI.Image(rewardChip, "Reward coin", art.Icon("coin"), V7Layout.RewardCoin.center - V7Layout.RewardChip.position, V7Layout.RewardCoin.size);
            rewardLabel = V5UI.Text(rewardChip, "Coin reward", "+0", font, V7Layout.RewardText.center - V7Layout.RewardChip.position, V7Layout.RewardText.size, V7Layout.RewardFontSize);
            rewardLabel.alignment = TextAlignmentOptions.MidlineLeft;
            chorusBonus = V5UI.Rect(body, "Chain bonus", V7Layout.InParent(V7Layout.WinPanel, V7Layout.ChorusBonus.center), V7Layout.ChorusBonus.size).gameObject;
            // The approved chain reward is a readable coin line, not the tiny feather mechanic badge.
            V5UI.Fit(chorusBonus.transform, "Chain", art.Get("chain_bonus"), new Vector2(22, 32), Vector2.one * 72);
            chorusLabel = V5UI.Text(chorusBonus.transform, "Chorus coin bonus", "", font, new Vector2(126, 32), new Vector2(140, 72), 48);
            next = V5UI.Button(body, art, "Next level", "icon_next_arrow", Local(V7Layout.NextButton), () => presentation.Close(root.NextLevel), true);
            replay = V5UI.Button(body, art, "Replay level", "icon_replay", Local(V7Layout.ReplayButton), () => presentation.Close(root.Restart));
            home = V5UI.Button(body, art, "Home", "icon_home", Local(V7Layout.HomeButton), () => presentation.Close(root.OpenMap));
            gameObject.SetActive(false);
        }
        private Image Bird(string part, string species)
        {
            Rect bounds = V7Layout.Screen("win", part);
            return V5UI.Fit(transform, part, art.Get(species + "_idle"), bounds.center, bounds.size * 1.6f);
        }
        private static Rect Local(Rect rect) => new Rect(rect.position - V7Layout.WinPanel.position, rect.size);
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
            body.SetAsLastSibling(); leftBird.transform.SetSiblingIndex(body.GetSiblingIndex()); rightBird.transform.SetSiblingIndex(body.GetSiblingIndex()); heroBird.transform.SetSiblingIndex(body.GetSiblingIndex());
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
                        presentation.Sparkles(art, body, V7Layout.Star(index).center - V7Layout.WinPanel.position, 100, 0);
                    }
                }, 0.8f + i * 0.2f, Ease.OutBack);
            }
            presentation.Pop(rewardChip, 1.45f); presentation.Count(rewardLabel, reward, 1.5f);
            if (reward > 0)
            {
                presentation.Coins(art, transform, V7Layout.RewardCoin.center, V7Layout.CoinIcon.center, 1.55f);
                if (coinFlightStarted != null) presentation.After(1.55f, coinFlightStarted);
            }
            if (chorusCoins > 0) presentation.Pop((RectTransform)chorusBonus.transform, 1.7f);
            Emerge(leftBird, "sky", new Vector2(V7Layout.WinPanel.x - 8, V7Layout.Screen("win", "left_bird").center.y), 1.8f, -1);
            Emerge(rightBird, "berry", new Vector2(V7Layout.WinPanel.xMax + 8, V7Layout.Screen("win", "right_bird").center.y), 1.9f, 1);
            Rect hero = V7Layout.Screen("win", "hero_bird");
            presentation.FlyBird(art, heroBird, "sunny", new Vector2(hero.center.x, -240), hero.center, 0.6f,
                () => { heroBird.sprite = art.Get("sunny_cheer"); presentation.Bob(heroBird, 8); });
            presentation.Confetti(art, transform, 1.7f);
            presentation.ButtonsLast(body, 2.2f);
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
