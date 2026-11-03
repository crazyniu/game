using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public sealed class V5LongPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private Action action;
        private V5Motion motion;
        public void Configure(Action callback) { action = callback; motion = gameObject.AddComponent<V5Motion>(); }
        public void OnPointerDown(PointerEventData data)
        {
            if (data.button != PointerEventData.InputButton.Left) return;
            motion.CancelAll(); motion.Play(0.65f, t => { }, () => action());
        }
        public void OnPointerUp(PointerEventData data) => motion.CancelAll();
        public void OnPointerExit(PointerEventData data) => motion.CancelAll();
    }
    public sealed class V5Panels : MonoBehaviour
    {
        private V5GameRoot root;
        private V5Art art;
        private TMP_FontAsset font;
        public bool IsOpen => gameObject.activeSelf;
        public void Configure(V5GameRoot owner, V5Art assets, TMP_FontAsset typeface)
        {
            root = owner; art = assets; font = typeface;
            var dim = gameObject.AddComponent<Image>(); dim.color = new Color(0, 0, 0, 0.6f); dim.raycastTarget = true;
            gameObject.SetActive(false);
        }
        private RectTransform Body(Vector2 size)
        {
            Hide(); gameObject.SetActive(true);
            var panel = V5UI.Image(transform, "Panel", art.Get("panel_win"), new Vector2(540, 1010), size);
            panel.raycastTarget = true;
            V5UI.Button(panel.transform, art, "Close", "icon_close", new Vector2(size.x - 99, 99), root.ClosePanel);
            return panel.rectTransform;
        }
        public void Buy(PerchSort.V5.Core.BoosterKind kind)
        {
            var body = Body(new Vector2(700, 650));
            V5UI.Image(body, "Booster", art.Get(V5ViewModel.Icon(kind)), new Vector2(240, 260), Vector2.one * 120);
            V5UI.Image(body, "Coin price", art.Get("coin"), new Vector2(400, 260), Vector2.one * 64);
            V5UI.Text(body, "Price", V5ViewModel.Price(kind).ToString(), font, new Vector2(510, 260), new Vector2(120, 70), 46);
            var buy = V5UI.Button(body, art, "Buy one booster", "icon_next", new Vector2(350, 490),
                () => { if (root.BuyBooster(kind)) root.ClosePanel(); }, primary: true);
            buy.interactable = root.Coins >= V5ViewModel.Price(kind);
        }
        public void Picker(int unlocked)
        {
            var body = Body(new Vector2(1000, 1700));
            int count = LevelLoader.Catalog.levels.Length;
            for (int i = 0; i < count; i++)
            {
                int id = i + 1;
                var button = V5UI.Button(body, art, "Level " + id, "btn_square_wood",
                    new Vector2(120 + i % 5 * 190, 290 + i / 5 * 190), () => root.LoadLevel(id));
                button.transform.GetChild(0).gameObject.SetActive(false);
                V5UI.Text(button.transform, "Number", id.ToString(), font, new Vector2(75, 52), new Vector2(130, 80), 44);
                int stars = root.BestStars(id);
                if (stars > 0)
                    for (int star = 0; star < 3; star++)
                    {
                        var image = V5UI.Image(button.transform, "Star " + (star + 1), art.Get("star"), new Vector2(40 + star * 35, 112), Vector2.one * 30);
                        image.color = new Color(1, 1, 1, star < stars ? 1 : 0.3f);
                    }
                button.interactable = id <= unlocked;
            }
        }
        public void Hide()
        {
            for (int i = transform.childCount - 1; i >= 0; i--) V5UI.Dispose(transform.GetChild(i).gameObject);
            gameObject.SetActive(false);
        }
    }
}
