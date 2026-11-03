using LitMotion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public sealed class WinPanel : MonoBehaviour
    {
        private V5Art art;
        private V5GameRoot owner;
        private RectTransform body;
        private TextMeshProUGUI rewardLabel, chorusLabel;
        private GameObject chorusBonus;
        private readonly Image[] stars = new Image[V6Layout.StarCount];
        private V5Motion motion;
        private Button next, replay, home;
        public bool IsOpen => gameObject.activeSelf;

        public void Configure(V5GameRoot root, V5Art assets, TMP_FontAsset font)
        {
            art = assets; owner = root;
            var dim = gameObject.AddComponent<Image>();
            dim.color = new Color(0, 0, 0, V6Layout.PanelDimAlpha); dim.raycastTarget = true;
            body = V5UI.Surface(transform, "Win panel", art, "ui_panel_cream", V6Layout.WinPanel).rectTransform;
            body.GetComponent<Image>().raycastTarget = true;
            V5UI.Fit(body, "Ribbon", art.Get("ui_ribbon_green"), V6Layout.InParent(V6Layout.WinPanel, V6Layout.Ribbon.center), V6Layout.Ribbon.size);
            for (int i = 0; i < V6Layout.StarCount; i++)
            {
                Rect bounds = V6Layout.Star(i);
                Vector2 point = V6Layout.InParent(V6Layout.WinPanel, bounds.center);
                V5UI.Image(body, "Star slot " + i, art.Get("star_empty"), point, bounds.size);
                stars[i] = V5UI.Image(body, "Earned star " + i, art.Get("star_full"), point, bounds.size);
            }
            V5UI.Surface(body, "Reward chip", art, "ui_chip_coin", new Rect(V6Layout.RewardChip.position - V6Layout.WinPanel.position, V6Layout.RewardChip.size));
            V5UI.Image(body, "Reward coin", art.Icon("ico_coin"),
                V6Layout.InParent(V6Layout.WinPanel, V6Layout.RewardCoin.center), V6Layout.RewardCoin.size);
            rewardLabel = V5UI.Text(body, "Coin reward", "+25", font,
                V6Layout.InParent(V6Layout.WinPanel, V6Layout.RewardText.center), V6Layout.RewardText.size, V6Layout.RewardFontSize);
            rewardLabel.alignment = TextAlignmentOptions.MidlineLeft;
            chorusBonus = V5UI.Rect(body, "Chain bonus", V6Layout.InParent(V6Layout.WinPanel, V6Layout.ChorusBonus.center), V6Layout.ChorusBonus.size).gameObject;
            V5UI.Fit(chorusBonus.transform, "Chain", art.Get("gift_encore"), V6Layout.BonusIconCenter, Vector2.one * V6Layout.BonusIconSize);
            chorusLabel = V5UI.Text(chorusBonus.transform, "Chorus coin bonus", "", font, V6Layout.BonusValueCenter, V6Layout.BonusValueSize, V6Layout.BonusFontSize);
            next = V5UI.Button(body, art, "Next level", "ico_next", Local(V6Layout.NextButton), root.NextLevel, primary: true);
            replay = V5UI.Button(body, art, "Replay level", "ico_replay", Local(V6Layout.ReplayButton), root.Restart);
            home = V5UI.Button(body, art, "Home", "ico_home", Local(V6Layout.HomeButton), root.OpenMap);
            motion = gameObject.AddComponent<V5Motion>();
            gameObject.SetActive(false);
        }
        private static Rect Local(Rect rect) => new Rect(rect.position - V6Layout.WinPanel.position, rect.size);

        public void Show(int earnedStars, int reward, int chorusCoins)
        {
            transform.SetAsLastSibling();
            gameObject.SetActive(true);
            motion.CancelAll();
            rewardLabel.text = "+" + reward;
            chorusLabel.text = "+" + chorusCoins;
            chorusBonus.SetActive(chorusCoins > 0);
            next.interactable = replay.interactable = home.interactable = false;
            body.localScale = Vector3.one * V6Layout.WinInitialScale;
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].gameObject.SetActive(i < earnedStars);
                stars[i].rectTransform.localScale = Vector3.zero;
            }
            motion.Play(0.35f, t => body.localScale = Vector3.one * Mathf.LerpUnclamped(V6Layout.WinInitialScale, 1, t), () =>
            {
                for (int i = 0; i < earnedStars; i++)
                {
                    var star = stars[i];
                    motion.Play(0.25f, t => star.rectTransform.localScale = Vector3.one * t, delay: i * 0.2f, ease: Ease.OutBack);
                }
                if (reward > 0) FlyCoins(earnedStars * 0.2f);
                else EnableActions();
            }, ease: Ease.OutBack);
        }

        private void FlyCoins(float delay)
        {
            if (V5Motion.Instant)
            {
                EnableActions();
                return;
            }
            for (int i = 0; i < 5; i++)
            {
                var coin = V5UI.Image(transform, "Flying reward coin", art.Icon("ico_coin"), V6Layout.RewardCoin.center, V6Layout.RewardCoin.size);
                Vector2 start = V6Layout.RewardCoin.center + Vector2.right * (i * 12);
                bool last = i == 4;
                motion.Play(0.65f, t =>
                {
                    Vector2 point = Vector2.Lerp(start, V6Layout.CoinIcon.center, t) + Vector2.up * (-150 * Mathf.Sin(t * Mathf.PI));
                    coin.rectTransform.anchoredPosition = new Vector2(point.x, -point.y);
                    coin.rectTransform.localScale = Vector3.one * Mathf.Lerp(1, 0.65f, t);
                }, () =>
                {
                    V5UI.Dispose(coin.gameObject);
                    if (last) EnableActions();
                }, delay + i * 0.08f);
            }
        }

        private void EnableActions()
        {
            next.interactable = owner.LevelId < LevelLoader.Catalog.levels.Length;
            replay.interactable = home.interactable = true;
        }
        public void Hide()
        {
            foreach (var item in GetComponentsInChildren<V5Motion>(true)) item.CancelAll();
            for (int i = transform.childCount - 1; i >= 0; i--)
                if (transform.GetChild(i).name == "Flying reward coin") V5UI.Dispose(transform.GetChild(i).gameObject);
            gameObject.SetActive(false);
        }
    }
}
