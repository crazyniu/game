using System;
using PerchSort.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PerchSort.Game
{
    public sealed class BoardControlFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler
    {
        private Button button;
        private BoardPresentation screen;
        private Action clicked;
        public void Configure(Button target, BoardPresentation owner, Action onClick = null)
        { button = target; screen = owner; clicked = onClick; }
        public void OnPointerDown(PointerEventData data)
        {
            if (data.button != PointerEventData.InputButton.Left || !button.IsActive() || !button.IsInteractable()) return;
            if (gameObject != button.gameObject) button.OnPointerDown(data);
            screen.PlaySound(Sfx.Click);
        }
        public void OnPointerUp(PointerEventData data)
        { if (gameObject != button.gameObject) button.OnPointerUp(data); }
        public void OnPointerExit(PointerEventData data)
        { if (gameObject != button.gameObject) button.OnPointerExit(data); }
        public void OnPointerClick(PointerEventData data)
        {
            if (data.button == PointerEventData.InputButton.Left && button.IsActive() && button.IsInteractable()) clicked?.Invoke();
        }
    }
}
