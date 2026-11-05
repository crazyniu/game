using System.Collections;
using NUnit.Framework;
using PerchSort.Core;
using PerchSort.Game;
using UnityEngine;
using UnityEngine.TestTools;

namespace PerchSort.PlayModeTests
{
    public sealed class ShopQuickPurchaseTests : CollectionScreenTests
    {
        [UnityTest]
        public IEnumerator ImmediateConfirmationCompletesWithoutTweenExceptions()
        {
            root.OpenShop();
            GameMotion.Instant = false;
            int coins = root.Coins;
            int stock = root.BoosterStock(BoosterKind.Firefly);
            root.Shop.TapCard(BoosterKind.Firefly);
            Assert.That(root.Shop.ConfirmOpen, Is.True);
            Button(root.Shop, "Confirm purchase").onClick.Invoke();
            Assert.That(root.Shop.ConfirmOpen, Is.False);
            Assert.That(root.Coins, Is.EqualTo(coins - ShopView.Item(BoosterKind.Firefly).Price));
            Assert.That(root.BoosterStock(BoosterKind.Firefly), Is.EqualTo(stock + 1));
            yield return new WaitForSeconds(0.6f);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator ClosingOrDestroyingDialogStopsItsTween()
        {
            root.OpenShop();
            GameMotion.Instant = false;
            root.Shop.TapCard(BoosterKind.Firefly);
            root.Shop.CloseConfirm();
            root.Shop.TapCard(BoosterKind.Firefly);
            var dialog = Button(root.Shop, "Confirm purchase").transform.parent.parent.gameObject;
            Object.Destroy(dialog);
            yield return null;
            yield return new WaitForSeconds(0.3f);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator DestroyedPanelIsSafeWhileDialogRemainsOpen()
        {
            root.OpenShop();
            GameMotion.Instant = false;
            root.Shop.TapCard(BoosterKind.Firefly);
            Object.Destroy(root.Shop.transform.Find("Purchase dim/Purchase panel").gameObject);
            yield return null;
            yield return new WaitForSeconds(0.3f);
            LogAssert.NoUnexpectedReceived();
        }
    }
}
