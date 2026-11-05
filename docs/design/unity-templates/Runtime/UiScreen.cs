using System;
using UnityEngine;

namespace Game.UI
{
    // Base for every full screen and popup managed by ScreenStack. Visibility and input
    // go through one CanvasGroup so a hidden or covered screen never takes taps.
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UiScreen : MonoBehaviour
    {
        [Tooltip("Modal screens keep the screen below visible but block its input.")]
        [SerializeField] private bool isModal;

        [Tooltip("Music for this screen. Leave empty to keep the music of the screen below.")]
        [SerializeField] private AudioClip music;

        private CanvasGroup group;

        public bool IsModal => isModal;
        public AudioClip Music => music;

        // Raised when the player asks to leave (close button, dim tap, back key).
        public event Action CloseRequested;

        protected virtual void Awake()
        {
            group = GetComponent<CanvasGroup>();
            SetVisible(false);
        }

        public virtual void Show()
        {
            SetVisible(true);
            PlayEnter();
        }

        public virtual void Hide()
        {
            group.interactable = false;
            group.blocksRaycasts = false;
            PlayExit(() => SetVisible(false));
        }

        // Another screen opened on top. A modal on top leaves this one visible but inert.
        public virtual void OnCovered(UiScreen top)
        {
            if (top.IsModal)
            {
                group.interactable = false;
                group.blocksRaycasts = false;
            }
            else
            {
                SetVisible(false);
            }
        }

        public virtual void OnRevealed()
        {
            SetVisible(true);
        }

        protected void RequestClose() => CloseRequested?.Invoke();

        // Override with the enter and exit motion from the screen spec. Respect UiMotion.Instant.
        protected virtual void PlayEnter() { }

        protected virtual void PlayExit(Action done) => done();

        private void SetVisible(bool visible)
        {
            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }
    }
}
