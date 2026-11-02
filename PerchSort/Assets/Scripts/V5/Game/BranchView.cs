using PerchSort.V5.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public sealed class BranchView : MonoBehaviour, IPointerClickHandler
    {
        private V5GameRoot root;
        private Image glow;
        private V5Motion motion;
        public int Index { get; private set; }
        public bool IsRight { get; private set; }
        public float BranchY { get; private set; }

        public void Configure(V5GameRoot owner, V5Art art, BranchState branch, int index)
        {
            root = owner; Index = index; IsRight = branch.IsRight;
            BranchY = 720 + branch.Row * 300 + (IsRight ? 40 : 0);
            var hit = gameObject.AddComponent<Image>();
            hit.color = Color.clear;
            hit.raycastTarget = true;
            var wood = V5UI.Image(transform, "Branch", art.Get("branch_left"),
                new Vector2(IsRight ? 240 : 300, 165), new Vector2(600, 112.5f));
            var bush = V5UI.Image(transform, "Root bush", art.Get("bush_edge"),
                new Vector2(IsRight ? 540 : 0, 150), new Vector2(240, 200));
            if (IsRight)
            {
                wood.rectTransform.localScale = new Vector3(-1, 1, 1);
                bush.rectTransform.localScale = new Vector3(-1, 1, 1);
            }
            glow = V5UI.Image(transform, "Hint highlight", art.Get("pill_cream"),
                new Vector2(270, 182), new Vector2(480, 18));
            glow.color = Color.clear;
            motion = gameObject.AddComponent<V5Motion>();
        }

        public Vector2 Slot(int index)
        {
            float x = 110 + 120 * index;
            return new Vector2(IsRight ? 1080 - x : x, BranchY - 87);
        }

        public void OnPointerClick(PointerEventData data)
        {
            if (data != null && data.button != PointerEventData.InputButton.Left) return;
            root.TapBranch(Index);
        }

        public void Highlight(float delay)
        {
            ClearHighlight();
            motion.Play(0.8f, t => glow.color = new Color(1f, 0.9f, 0.2f, Mathf.Sin(t * Mathf.PI) * 0.95f),
                () => glow.color = Color.clear, delay);
        }

        public void ClearHighlight()
        {
            if (motion != null) motion.CancelAll();
            if (glow != null) glow.color = Color.clear;
        }
    }
}
