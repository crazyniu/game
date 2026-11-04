using LitMotion;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.Game
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class GameButton : Button
    {
        private CanvasGroup group;
        private GameMotion motion;
        public Sprite RestSprite, PressedSprite;
        private bool dimWhenDisabled = true;

        internal void SetDisabledAppearance(bool dim)
        {
            if (dimWhenDisabled == dim) return;
            dimWhenDisabled = dim;
            DoStateTransition(currentSelectionState, true);
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            if (group == null) group = GetComponent<CanvasGroup>();
            if (motion == null) motion = gameObject.AddComponent<GameMotion>();
            motion.CancelAll();
            bool enabled = state != SelectionState.Disabled && IsInteractable();
            if (targetGraphic is Image image && RestSprite != null)
                image.sprite = state == SelectionState.Pressed && PressedSprite != null ? PressedSprite : RestSprite;
            bool dim = !enabled && dimWhenDisabled;
            group.alpha = dim ? GameLayout.DisabledAlpha : 1;
            float scale = dim ? GameLayout.DisabledScale :
                state == SelectionState.Pressed ? GameLayout.ButtonPressScale : 1;
            if (instant || !enabled || !gameObject.activeInHierarchy)
            {
                transform.localScale = Vector3.one * scale;
                return;
            }
            Vector3 start = transform.localScale;
            bool pressed = state == SelectionState.Pressed;
            motion.Play(pressed ? GameLayout.ButtonPressSeconds : GameLayout.ButtonReleaseSeconds,
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
