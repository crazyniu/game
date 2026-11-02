using LitMotion;
using PerchSort.V5.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public sealed class HudView : MonoBehaviour
    {
        public static readonly Vector2 DoorPoint = new Vector2(540, 255);
        public static readonly Vector2 CoinPoint = new Vector2(95, 150);
        private V5GameRoot root;
        private V5Art art;
        private TextMeshProUGUI coins;
        private RectTransform house;
        private V5Motion houseMotion;
        private Image[] homeIcons;
        private Button undo, hint, add, settings;
        private GameObject stuck;

        public void Configure(V5GameRoot owner, V5Art assets, TMP_FontAsset font)
        {
            root = owner; art = assets;
            V5UI.Image(transform, "Coin pill", art.Get("pill_cream"), new Vector2(170, 150), new Vector2(260, 90));
            V5UI.Image(transform, "Coin", art.Get("coin"), CoinPoint, new Vector2(76, 76));
            coins = V5UI.Text(transform, "Coins", "0", font, new Vector2(205, 150), new Vector2(160, 75), 46);
            settings = V5UI.Button(transform, art, "Settings", "icon_settings", "btn_round_purple",
                new Vector2(980, 150), new Vector2(120, 120), root.OpenSettings);
            house = V5UI.Image(transform, "Birdhouse", art.Get("birdhouse"), new Vector2(540, 230), new Vector2(300, 340)).rectTransform;
            houseMotion = house.gameObject.AddComponent<V5Motion>();
            homeIcons = new[]
            {
                V5UI.Image(transform, "Sunny home", art.Get("bird_sunny_idle"), new Vector2(460, 420), new Vector2(48, 48)),
                V5UI.Image(transform, "Sky home", art.Get("bird_sky_idle"), new Vector2(620, 420), new Vector2(48, 48))
            };
            V5UI.Image(transform, "Level pill", art.Get("pill_cream"), new Vector2(540, 430), new Vector2(100, 65));
            V5UI.Text(transform, "Level", "1", font, new Vector2(540, 430), new Vector2(90, 60), 42);
            undo = V5UI.Button(transform, art, "Undo", "icon_undo", "btn_square_wood", new Vector2(300, 1760), new Vector2(170, 170), () => root.Undo());
            hint = V5UI.Button(transform, art, "Hint", "icon_hint", "btn_square_wood", new Vector2(540, 1760), new Vector2(170, 170), root.Hint);
            add = V5UI.Button(transform, art, "Add branch", "icon_add_branch", "btn_square_green", new Vector2(780, 1760), new Vector2(170, 170), () => root.AddBranch());
            var bubble = V5UI.Image(transform, "Stuck assistance", art.Get("pill_cream"), new Vector2(540, 1580), new Vector2(740, 100));
            V5UI.Text(bubble.transform, "Stuck message", "Try Undo or Add Branch", font, new Vector2(370, 50), new Vector2(700, 90), 36);
            V5UI.Image(bubble.transform, "Undo pointer", art.Get("hand_pointer"), new Vector2(130, 115), new Vector2(70, 70));
            V5UI.Image(bubble.transform, "Add pointer", art.Get("hand_pointer"), new Vector2(610, 115), new Vector2(70, 70));
            stuck = bubble.gameObject;
            stuck.SetActive(false);
        }

        public void Refresh(GameSession session, int coinCount, bool locked)
        {
            coins.text = coinCount.ToString();
            undo.interactable = !locked && session.CanUndo;
            hint.interactable = !locked && !session.State.IsWon;
            add.interactable = !locked && !session.State.IsWon && !session.State.AddedBranch;
            settings.interactable = !locked;
            for (int i = 0; i < homeIcons.Length; i++)
            {
                string species = i == 0 ? "sunny" : "sky";
                bool home = false;
                foreach (var homed in session.State.HomedSpecies) if (homed.Id == species) home = true;
                homeIcons[i].color = home ? Color.white : new Color(0.3f, 0.3f, 0.3f, 0.35f);
            }
            stuck.SetActive(!locked && !session.State.IsWon && !GameSession.HasLegalMove(session.State));
        }

        public void HomeArrival()
        {
            houseMotion.CancelAll();
            houseMotion.Play(0.2f, t => house.localScale = Vector3.one * (1 + Mathf.Sin(t * Mathf.PI) * 0.1f));
            if (V5Motion.Instant) return;
            for (int i = 0; i < 6; i++)
            {
                float angle = i * Mathf.PI / 3;
                Vector2 delta = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 95;
                var sparkle = V5UI.Image(transform, "Home sparkle", art.Get("sparkle"), DoorPoint, new Vector2(38, 38));
                sparkle.gameObject.AddComponent<V5Motion>().Play(0.4f, t =>
                {
                    Vector2 point = DoorPoint + delta * t;
                    sparkle.rectTransform.anchoredPosition = new Vector2(point.x, -point.y);
                    sparkle.color = new Color(1, 1, 1, 1 - t);
                }, () => V5UI.Dispose(sparkle.gameObject));
            }
        }

        public void Cancel()
        {
            foreach (var motion in GetComponentsInChildren<V5Motion>(true)) motion.CancelAll();
            house.localScale = Vector3.one;
            for (int i = transform.childCount - 1; i >= 0; i--)
                if (transform.GetChild(i).name == "Home sparkle") V5UI.Dispose(transform.GetChild(i).gameObject);
        }
    }
}
