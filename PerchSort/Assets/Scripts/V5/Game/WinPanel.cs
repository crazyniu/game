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
        private readonly Image[] stars = new Image[3];
        private V5Motion motion;
        private Button next, replay, home;
        public bool IsOpen => gameObject.activeSelf;

        public void Configure(V5GameRoot root, V5Art assets, TMP_FontAsset font)
        {
            art = assets;
            var dim = gameObject.AddComponent<Image>();
            dim.color = new Color(0, 0, 0, 0.6f); dim.raycastTarget = true;
            body = V5UI.Image(transform, "Win panel", art.Get("panel_win"), new Vector2(540, 960), new Vector2(860, 900)).rectTransform;
            body.GetComponent<Image>().raycastTarget = true;
            V5UI.Image(body, "Ribbon", art.Get("ribbon"), new Vector2(430, 75), new Vector2(900, 200));
            for (int i = 0; i < 3; i++)
            {
                Vector2 point = new Vector2(230 + i * 200, 280);
                V5UI.Image(body, "Star slot " + i, art.Get("star_empty"), point, new Vector2(170, 170));
                stars[i] = V5UI.Image(body, "Earned star " + i, art.Get("star_full"), point, new Vector2(170, 170));
            }
            V5UI.Image(body, "Reward coin", art.Get("coin"), new Vector2(350, 455), new Vector2(80, 80));
            rewardLabel = V5UI.Text(body, "Coin reward", "+25", font, new Vector2(465, 455), new Vector2(160, 90), 64);
            next = V5UI.Button(body, art, "Next level", "icon_next", "btn_square_green", new Vector2(430, 650), new Vector2(230, 170), root.Restart);
            replay = V5UI.Button(body, art, "Replay level", "icon_restart", "btn_square_wood", new Vector2(205, 675), new Vector2(140, 140), root.Restart);
            home = V5UI.Button(body, art, "Home", "icon_home", "btn_square_wood", new Vector2(655, 675), new Vector2(140, 140), root.Restart);
            motion = gameObject.AddComponent<V5Motion>();
            gameObject.SetActive(false);
        }

        public void Show(int earnedStars, int reward)
        {
            gameObject.SetActive(true);
            motion.CancelAll();
            rewardLabel.text = "+" + reward;
            next.interactable = replay.interactable = home.interactable = false;
            body.localScale = Vector3.one * 0.6f;
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].gameObject.SetActive(i < earnedStars);
                stars[i].rectTransform.localScale = Vector3.zero;
            }
            motion.Play(0.35f, t => body.localScale = Vector3.one * Mathf.LerpUnclamped(0.6f, 1, t), () =>
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
                var coin = V5UI.Image(transform, "Flying reward coin", art.Get("coin"), new Vector2(460, 965), new Vector2(65, 65));
                Vector2 start = new Vector2(460 + i * 12, 965);
                bool last = i == 4;
                motion.Play(0.65f, t =>
                {
                    Vector2 point = Vector2.Lerp(start, HudView.CoinPoint, t) + Vector2.up * (-150 * Mathf.Sin(t * Mathf.PI));
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
