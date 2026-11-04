using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.Game
{
    public sealed class MapMotion : MonoBehaviour
    {
        private GameMotion idleMotion, tapMotion;
        private RectTransform node, shadow, chick, tapped;
        private Image bird;
        private Sprite idle, blink, quirk1, quirk2;
        private Vector2 nodeRest;
        private Vector3 birdScale, tapRest;
        private Color shadowColor;

        public void Configure()
        {
            idleMotion = gameObject.AddComponent<GameMotion>();
            tapMotion = gameObject.AddComponent<GameMotion>();
        }

        public void Current(RectTransform holder, Image nodeShadow, Image sunny, GameArt art)
        {
            Cancel();
            node = holder; nodeRest = holder.anchoredPosition;
            shadow = nodeShadow.rectTransform; shadowColor = nodeShadow.color;
            bird = sunny; chick = sunny.rectTransform; birdScale = chick.localScale;
            idle = art.Get("sunny_idle"); blink = art.Get("sunny_blink");
            quirk1 = art.Get("sunny_quirk1"); quirk2 = art.Get("sunny_quirk2");
            idleMotion.Loop(72, t =>
            {
                float seconds = t * 72, idleTime = seconds % 12;
                float hop = (1 - Mathf.Cos(seconds / GameLayout.MapBounceSeconds * Mathf.PI * 2)) * 0.5f;
                node.anchoredPosition = nodeRest + Vector2.up * (GameLayout.MapBounceHeight * hop);
                shadow.localScale = new Vector3(1 - 0.12f * hop, 1 - 0.08f * hop, 1);
                nodeShadow.color = new Color(shadowColor.r, shadowColor.g, shadowColor.b, shadowColor.a * (1 - 0.2f * hop));
                float breath = Mathf.Sin(seconds * Mathf.PI * 2 / 1.6f) * 0.012f;
                chick.localScale = Vector3.Scale(birdScale, new Vector3(1 - breath * 0.5f, 1 + breath, 1));
                float blinkTime = seconds % 4;
                bird.sprite = idleTime >= 8 && idleTime < 8.35f ? quirk1 :
                    idleTime >= 8.35f && idleTime < 8.7f ? quirk2 : blinkTime > 3.86f ? blink : idle;
            });
        }

        public void Tap(RectTransform holder)
        {
            StopTap(); tapped = holder; tapRest = holder.localScale;
            tapMotion.Play(0.24f, t =>
                holder.localScale = tapRest * (1 + 0.08f * Mathf.Sin(t * Mathf.PI) * (1 - t)), StopTap);
        }

        private void StopTap()
        {
            if (tapMotion != null) tapMotion.CancelAll();
            if (tapped != null) tapped.localScale = tapRest;
            tapped = null;
        }

        public void Cancel()
        {
            if (idleMotion != null) idleMotion.CancelAll();
            StopTap();
            if (node != null) node.anchoredPosition = nodeRest;
            if (shadow != null)
            {
                shadow.localScale = Vector3.one;
                shadow.GetComponent<Image>().color = shadowColor;
            }
            if (chick != null) chick.localScale = birdScale;
            if (bird != null) bird.sprite = idle;
            node = shadow = chick = null; bird = null;
        }

        private void OnDisable() => Cancel();
        private void OnDestroy() => Cancel();
    }
}
