using System.Collections.Generic;
using PerchSort.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PerchSort.View
{
    public sealed class PerchView : MonoBehaviour, IPointerClickHandler
    {
        public readonly List<BirdView> Birds = new List<BirdView>();
        public bool IsBasket { get; private set; }
        public bool IsAsleep { get; private set; }
        public bool HasCrate => crate.gameObject.activeSelf;
        public int Capacity { get; private set; }
        public float SlotWidth => width / Capacity;
        public float SurfaceHeight => IsBasket ? 0 : -.1f + 23f/1024 * width;
        public ShelfSide Side { get; private set; }
        public SpriteRenderer Crate => crate;
        private BoardView board;
        private int index;
        private float width;
        private SpriteRenderer branch, crate;
        private SpriteRenderer z;

        public void Initialize(BoardView owner, int location, Sprite art, float perchWidth, int slots, ShelfSide side, bool basket)
        {
            board = owner; index = location; width = perchWidth; Capacity = slots; Side = side; IsBasket = basket;
            branch = ViewArt.Sprite(basket ? "Basket" : "Branch", transform, basket ? ViewArt.Get("env/basket") : art,
                Vector3.down * (basket ? 0 : .1f), Vector3.one, basket ? 15 : 5);
            branch.transform.localScale = basket ? Vector3.one * 1.5f : Vector3.one * width / art.bounds.size.x;
            branch.flipX = side == ShelfSide.Right;
            crate = ViewArt.Sprite("Crate", transform, ViewArt.Get("env/crate"), new Vector3(0,-.4f,0), new Vector3(width / .97265625f,5.44f,1), 20);
            crate.gameObject.SetActive(false);
            z = ViewArt.Sprite("Sleep Z", transform, ViewArt.Get("fx/zzz"), Vector3.zero, Vector3.one * 1.152f, 23);
            z.gameObject.SetActive(false);
            var hit = gameObject.AddComponent<BoxCollider2D>(); hit.offset = new Vector2(0,.35f); hit.size = new Vector2(width,1.35f);
        }
        public Vector3 Slot(int slot) => transform.localPosition + new Vector3(IsBasket ? 0 : ((slot + .5f) / Capacity - .5f) * width * (Side == ShelfSide.Right ? -1 : 1), SurfaceHeight, 0);
        public void Highlight(bool selected) => branch.color = selected ? new Color(1,.87f,.53f) : Color.white;
        public void Sleep(bool asleep)
        {
            IsAsleep = asleep; z.gameObject.SetActive(asleep);
            foreach (var bird in Birds) bird.Sleep(asleep);
        }
        public void Cover(bool covered)
        {
            crate.gameObject.SetActive(covered); crate.transform.localPosition = new Vector3(0,-.4f,0);
            crate.transform.localRotation = Quaternion.identity; crate.color = Color.white;
            foreach (var bird in Birds) bird.gameObject.SetActive(!covered);
        }
        private void Update()
        {
            if (!IsAsleep) return;
            float t = Mathf.Repeat(Time.unscaledTime * .48f + index * .23f, 1);
            z.transform.localPosition = new Vector3(.1f + (board.ReducedMotion ? 0 : Mathf.Sin(t*Mathf.PI)*.16f),.9f + (board.ReducedMotion ? 0 : t * .45f),0);
            z.color = new Color(1,1,1, board.ReducedMotion ? 1 : Mathf.Sin(t * Mathf.PI));
        }
        public void OnPointerClick(PointerEventData eventData) => board.SelectPerch(index);
    }
}
