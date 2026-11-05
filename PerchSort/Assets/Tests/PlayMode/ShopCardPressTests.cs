using System.Collections;
using NUnit.Framework;
using PerchSort.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace PerchSort.PlayModeTests
{
    public sealed class ShopCardPressTests : CollectionScreenTests
    {
        [UnityTest]
        public IEnumerator UnlockedCardRespondsToPointer() => CheckCard("Firefly card");

        [UnityTest]
        public IEnumerator LockedCardRespondsToPointer() => CheckCard("SpringRain card");

        private IEnumerator CheckCard(string name)
        {
            root.OpenShop();
            GameMotion.Instant = false;
            var card = root.Shop.transform.Find(name);
            Assert.That(card, Is.Not.Null);
            Assert.That(card.localScale.x, Is.EqualTo(1).Within(0.01f));
            var pointer = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left };
            ExecuteEvents.Execute(card.gameObject, pointer, ExecuteEvents.pointerEnterHandler);
            ExecuteEvents.Execute(card.gameObject, pointer, ExecuteEvents.pointerDownHandler);
            yield return new WaitForSecondsRealtime(0.1f);
            float pressed = card.localScale.x;
            ExecuteEvents.Execute(card.gameObject, pointer, ExecuteEvents.pointerUpHandler);
            yield return new WaitForSecondsRealtime(0.3f);
            Assert.That(card.localScale.x, Is.EqualTo(1).Within(0.01f), name + " release");
            Assert.That(pressed, Is.EqualTo(0.96f).Within(0.01f), name + " press");
            Assert.That(pressed, Is.EqualTo(GameLayout.ButtonPressScale).Within(0.01f), name + " shared scale");
        }
    }
}
