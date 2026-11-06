using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.Game
{
    public sealed class TutorialHand : MonoBehaviour
    {
        private Image hand;
        private GameMotion motion;
        private Vector2 target;
        private bool visible;
        public void Configure(GameArt art)
        {
            hand = GameUI.Image(transform, "Tutorial hand", art.Get("hand_pointer"), Vector2.zero, new Vector2(160, 160));
            hand.rectTransform.pivot = new Vector2(0.115f, 0.97f);
            motion = gameObject.AddComponent<GameMotion>(); hand.gameObject.SetActive(false);
        }
        public void Render(TutorialPresentation data)
        {
            bool show = data != null && data.Visible && !data.Completed;
            if (!show) { Hide(); return; }
            Vector2 requested = data.Point;
            if (data.TargetKind == "tool")
            {
                for (int count = 3; count <= 4; count++)
                    for (int i = 0; i < count; i++)
                        if (Vector2.SqrMagnitude(data.Point - GameLayout.ToolButton(i, count).center) < 4)
                            requested = HudView.ToolBounds(Mathf.Min(i, 2)).center;
            }
            if (visible && target == requested) return;
            target = requested; visible = true; motion.CancelAll();
            var rect = hand.rectTransform;
            Vector2 point = new Vector2(Mathf.Clamp(target.x, 24, 1080 - 24 - (1 - rect.pivot.x) * 160),
                Mathf.Clamp(target.y, (1 - rect.pivot.y) * 160 + 24, 1920 - rect.pivot.y * 160 - 24));
            rect.anchoredPosition = new Vector2(point.x, -point.y); rect.localScale = Vector3.one;
            hand.gameObject.SetActive(true);
            motion.Loop(0.9f, t => rect.localScale = Vector3.one * (0.96f + 0.04f * Mathf.Cos(t * Mathf.PI * 2)));
        }
        public void Hide()
        {
            visible = false; motion?.CancelAll();
            if (hand != null) hand.gameObject.SetActive(false);
        }
        private void OnDisable() => Hide();
        private void OnDestroy() => Hide();
    }
}
