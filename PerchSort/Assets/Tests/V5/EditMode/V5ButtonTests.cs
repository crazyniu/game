using NUnit.Framework;
using PerchSort.V5.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PerchSort.V5.Tests
{
    public sealed class V5ButtonTests
    {
        [Test]
        public void DisabledButtonKeepsItsArtRejectsClicksAndCanBeEnabledAgain()
        {
            bool previousInstant = V5Motion.Instant;
            V5Motion.Instant = true;
            var art = new V5Art();
            Button button = null;
            try
            {
                int clicks = 0;
                button = V5UI.Button(null, art, "Undo probe", "icon_undo", Vector2.zero, () => clicks++);
                var background = button.GetComponent<Image>().sprite;
                var pointer = new PointerEventData(null) { button = PointerEventData.InputButton.Left };
                button.interactable = false;
                button.OnPointerClick(pointer);
                button.OnPointerExit(pointer);
                Assert.That(clicks, Is.Zero);
                Assert.That(button.IsInteractable(), Is.False);
                Assert.That(button.GetComponent<CanvasGroup>().alpha, Is.EqualTo(0.45f));
                Assert.That(button.transform.localScale, Is.EqualTo(Vector3.one * 0.9f));
                Assert.That(button.GetComponent<Image>().sprite, Is.SameAs(background));

                button.interactable = true;
                button.OnPointerClick(pointer);
                Assert.That(button.IsInteractable(), Is.True);
                Assert.That(clicks, Is.EqualTo(1));
                Assert.That(button.GetComponent<CanvasGroup>().alpha, Is.EqualTo(1));
                Assert.That(button.transform.localScale, Is.EqualTo(Vector3.one));
            }
            finally
            {
                if (button != null) Object.DestroyImmediate(button.gameObject);
                art.Dispose();
                V5Motion.Instant = previousInstant;
            }
        }
    }
}
