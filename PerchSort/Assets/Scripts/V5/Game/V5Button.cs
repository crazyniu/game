using LitMotion;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class V5Button : Button
    {
        private CanvasGroup group;
        private V5Motion motion;

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            if (group == null) group = GetComponent<CanvasGroup>();
            if (motion == null) motion = gameObject.AddComponent<V5Motion>();
            motion.CancelAll();
            bool enabled = state != SelectionState.Disabled && IsInteractable();
            group.alpha = enabled ? 1 : V5Layout.DisabledAlpha;
            float scale = !enabled ? V5Layout.DisabledScale :
                state == SelectionState.Pressed ? V5Layout.ButtonPressScale : 1;
            if (instant || !enabled || !gameObject.activeInHierarchy)
            {
                transform.localScale = Vector3.one * scale;
                return;
            }
            Vector3 start = transform.localScale;
            bool pressed = state == SelectionState.Pressed;
            motion.Play(pressed ? V5Layout.ButtonPressSeconds : V5Layout.ButtonReleaseSeconds,
                t => transform.localScale = Vector3.LerpUnclamped(start, Vector3.one * scale, t),
                ease: pressed ? Ease.Linear : Ease.OutBack);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (motion != null) motion.CancelAll();
        }
    }
}
