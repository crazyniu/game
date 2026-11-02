using LitMotion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public sealed class WinPanel : MonoBehaviour
    {
        private V5Art art;
        private RectTransform body;
        private TextMeshProUGUI rewardLabel;
        private readonly Image[] stars = new Image[V5Layout.StarCount];
        private V5Motion motion;
        private Button next, replay, home;
        public bool IsOpen => gameObject.activeSelf;

        public void Configure(V5GameRoot root, V5Art assets, TMP_FontAsset font)
        {
            art = assets;
            var dim = gameObject.AddComponent<Image>();
            dim.color = new Color(0, 0, 0, V5Layout.PanelDimAlpha); dim.raycastTarget = true;
            body = V5UI.Image(transform, "Win panel", art.Get("panel_win"), V5Layout.WinPanel.center, V5Layout.WinPanel.size).rectTransform;
            body.GetComponent<Image>().raycastTarget = true;
            V5UI.Image(body, "Ribbon", art.Get("ribbon"), V5Layout.InParent(V5Layout.WinPanel, V5Layout.Ribbon.center), V5Layout.Ribbon.size);
            for (int i = 0; i < V5Layout.StarCount; i++)
            {
                Rect bounds = V5Layout.Star(i);
                Vector2 point = V5Layout.InParent(V5Layout.WinPanel, bounds.center);
                V5UI.Image(body, "Star slot " + i, art.Get("star_empty"), point, bounds.size);
                stars[i] = V5UI.Image(body, "Earned star " + i, art.Get("star_full"), point, bounds.size);
            }
            V5UI.Image(body, "Reward coin", art.Get("coin"),
                V5Layout.InParent(V5Layout.WinPanel, V5Layout.RewardCoin.center), V5Layout.RewardCoin.size);
            rewardLabel = V5UI.Text(body, "Coin reward", "+25", font,
                V5Layout.InParent(V5Layout.WinPanel, V5Layout.RewardText.center), V5Layout.RewardText.size, V5Layout.RewardFontSize);
            rewardLabel.alignment = TextAlignmentOptions.MidlineLeft;
            next = V5UI.Button(body, art, "Next level", "icon_next",
                V5Layout.InParent(V5Layout.WinPanel, V5Layout.NextButton.center), root.Restart, primary: true);
            replay = V5UI.Button(body, art, "Replay level", "icon_restart",
                V5Layout.InParent(V5Layout.WinPanel, V5Layout.ReplayButton.center), root.Restart);
            home = V5UI.Button(body, art, "Home", "icon_home",
                V5Layout.InParent(V5Layout.WinPanel, V5Layout.HomeButton.center), root.Restart);
            motion = gameObject.AddComponent<V5Motion>();
            gameObject.SetActive(false);
        }

        public void Show(int earnedStars, int reward)
        {
            gameObject.SetActive(true);
            motion.CancelAll();
            rewardLabel.text = "+" + reward;
            next.interactable = replay.interactable = home.interactable = false;
            body.localScale = Vector3.one * V5Layout.WinInitialScale;
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].gameObject.SetActive(i < earnedStars);
                stars[i].rectTransform.localScale = Vector3.zero;
            }
            motion.Play(0.35f, t => body.localScale = Vector3.one * Mathf.LerpUnclamped(V5Layout.WinInitialScale, 1, t), () =>
            {
                for (int i = 0; i < earnedStars; i++)
                {
                    var star = stars[i];
                    motion.Play(0.25f, t => star.rectTransform.localScale = Vector3.one * t, delay: i * 0.2f, ease: Ease.OutBack);
                }
                if (reward > 0) FlyCoins(earnedStars * 0.2f);
                else next.interactable = replay.interactable = home.interactable = true;
            }, ease: Ease.OutBack);
        }

        private void FlyCoins(float delay)
        {
            if (V5Motion.Instant)
            {
                next.interactable = replay.interactable = home.interactable = true;
                return;
            }
            for (int i = 0; i < 5; i++)
            {
                var coin = V5UI.Image(transform, "Flying reward coin", art.Get("coin"), V5Layout.RewardCoin.center, V5Layout.RewardCoin.size);
                Vector2 start = V5Layout.RewardCoin.center + Vector2.right * (i * 12);
                bool last = i == 4;
                motion.Play(0.65f, t =>
                {
                    Vector2 point = Vector2.Lerp(start, V5Layout.CoinIcon.center, t) + Vector2.up * (-150 * Mathf.Sin(t * Mathf.PI));
                    coin.rectTransform.anchoredPosition = new Vector2(point.x, -point.y);
                    coin.rectTransform.localScale = Vector3.one * Mathf.Lerp(1, 0.65f, t);
                }, () =>
                {
                    V5UI.Dispose(coin.gameObject);
                    if (last) next.interactable = replay.interactable = home.interactable = true;
                }, delay + i * 0.08f);
            }
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
