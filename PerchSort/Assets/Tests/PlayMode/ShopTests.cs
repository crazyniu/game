using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PerchSort.Core;
using PerchSort.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Meta = PerchSort.Meta;
using Object = UnityEngine.Object;

namespace PerchSort.PlayModeTests
{
    public abstract class CollectionScreenTests
    {
        protected GameRoot root;
        private readonly Dictionary<string, int?> saved = new Dictionary<string, int?>();
        private string notifications;
        private DevOptions options;
        private bool instant;
        protected static string Evidence => Environment.GetEnvironmentVariable("PERCH_COLLECTION_EVIDENCE");
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            instant = GameMotion.Instant; GameMotion.Instant = true;
            options = DevOptions.OverrideForTests; DevOptions.OverrideForTests = new DevOptions();
            notifications = PlayerPrefs.HasKey(ProgressStore.NotificationsKey) ? PlayerPrefs.GetString(ProgressStore.NotificationsKey) : null;
            saved.Clear();
            foreach (var key in ProgressStore.Keys())
            {
                if (key != ProgressStore.NotificationsKey) saved[key] = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : (int?)null;
                PlayerPrefs.DeleteKey(key);
            }
            var ledger = Meta.Aviary.OnLevelStarted(new Meta.PlayerProfile(), new[] { "sunny", "sky", "berry", "mint", "cocoa", "snow" }).Profile;
            ledger = Meta.Aviary.IncrementFlocksHome(ledger, "berry", 7);
            ledger = Meta.Economy.ClaimDailyGift(ledger, new Meta.SystemClock()).Profile;
            var profile = new GameProfile(notificationReceipts: ledger.WithCoins(240)) { Coins = 240, HighestUnlocked = 22 };
            int[] stock = { 3, 2, 1, 1, 0, 0 };
            for (int i = 0; i < stock.Length; i++) { profile.Boosters[i] = stock[i]; profile.StarterGranted[i] = profile.UnlockSeen[i] = true; }
            for (int i = 1; i <= 40; i++) profile.Tutorials[i] = true;
            ProgressStore.Save(profile);
            yield return Reload();
        }
        protected IEnumerator Reload()
        {
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            for (int i = 0; i < 120; i++)
            {
                root = Object.FindAnyObjectByType<GameRoot>(); if (root != null && root.IsReady) break; yield return null;
            }
            Assert.That(root != null && root.IsReady, Is.True); Assert.That(root.HasModal, Is.False);
        }
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var staging = SceneManager.CreateScene("Collection test cleanup"); SceneManager.SetActiveScene(staging);
            if (root != null) yield return SceneManager.UnloadSceneAsync(root.gameObject.scene);
            foreach (var pair in saved) if (pair.Value.HasValue) PlayerPrefs.SetInt(pair.Key, pair.Value.Value); else PlayerPrefs.DeleteKey(pair.Key);
            if (notifications == null) PlayerPrefs.DeleteKey(ProgressStore.NotificationsKey); else PlayerPrefs.SetString(ProgressStore.NotificationsKey, notifications);
            PlayerPrefs.Save(); GameMotion.Instant = instant; DevOptions.OverrideForTests = options;
        }
        protected static Button Button(Component owner, string name) => owner.GetComponentsInChildren<Button>().Single(button => button.name == name);
        // The retained test assembly does not reference TMPro; inspect rendered text through UI's base graphic.
        protected static string Text(Transform target)
        {
            var graphic = target.GetComponent<Graphic>();
            return (string)graphic.GetType().GetProperty("text").GetValue(graphic);
        }
        protected void Capture(string name)
        {
            if (string.IsNullOrEmpty(Evidence)) return;
            Directory.CreateDirectory(Evidence); Canvas.ForceUpdateCanvases();
            var camera = root.RenderCamera; var previous = camera.targetTexture;
            var target = RenderTexture.GetTemporary(1080, 1920, 24); var active = RenderTexture.active;
            camera.targetTexture = target;
            Canvas.ForceUpdateCanvases(); root.Canvas.GetComponent<PerchSort.Game.SafeArea>().Apply();
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)root.Canvas.transform);
            Canvas.ForceUpdateCanvases(); camera.Render(); RenderTexture.active = target;
            var image = new Texture2D(1080, 1920, TextureFormat.RGB24, false); image.ReadPixels(new Rect(0, 0, 1080, 1920), 0, 0); image.Apply();
            string path = Path.Combine(Evidence, name + ".png"); Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllBytes(path, image.EncodeToPNG());
            var coinLabel = root.Shop.IsOpen ? root.Shop.transform.Find("Coins") : root.Aviary.transform.Find("Coins");
            var badge = root.Shop.IsOpen ? root.Shop.transform.Find("Firefly card/Owned badge") : null;
            var bird = root.Aviary.DetailOpen ? root.Aviary.transform.Find("Species detail/Living bird/Bird") : null;
            string pose = bird == null ? "-" : bird.Find("Drawn action frames").GetComponent<Image>().sprite.name;
            File.AppendAllText(Path.Combine(Evidence, "capture-times.tsv"), name + "\t" + Time.time.ToString("F4", System.Globalization.CultureInfo.InvariantCulture) +
                "\t" + root.Coins + "\t" + (coinLabel == null ? "-" : Text(coinLabel)) + "\t" + (badge == null ? "-" : badge.localScale.x.ToString("F4")) +
                "\t" + pose + "\t" + (bird == null ? "-" : bird.localScale.x.ToString("F4")) + "\n");
            camera.targetTexture = previous; RenderTexture.active = active; RenderTexture.ReleaseTemporary(target); Object.Destroy(image);
        }
        protected static void AssertPositiveScale(Component owner)
        {
            foreach (var rect in owner.GetComponentsInChildren<RectTransform>(true)) Assert.That(rect.localScale.x, Is.GreaterThanOrEqualTo(0), rect.name);
        }
    }

    public sealed class ShopTests : CollectionScreenTests
    {
        [UnityTest]
        public IEnumerator SinglePurchaseDeductsCatalogPriceAndPersistsOneUnit()
        {
            Button(root.Map, "Shop").onClick.Invoke(); root.Shop.TapCard(BoosterKind.Firefly);
            Assert.That(root.Shop.ConfirmOpen, Is.True); Assert.That(root.Coins, Is.EqualTo(240));
            Button(root.Shop, "Confirm purchase").onClick.Invoke();
            Assert.That(root.Shop.ConfirmOpen, Is.False); Assert.That(root.Coins, Is.EqualTo(210)); Assert.That(root.BoosterStock(BoosterKind.Firefly), Is.EqualTo(4));
            yield return Reload();
            Assert.That(root.Coins, Is.EqualTo(210)); Assert.That(root.BoosterStock(BoosterKind.Firefly), Is.EqualTo(4));
            Assert.That(ProgressStore.Load().Boosters[(int)BoosterKind.Firefly], Is.EqualTo(4));
        }
        [UnityTest]
        public IEnumerator InsufficientAndLockedCardsNeverOpenConfirmationOrMutateProfile()
        {
            root.LoadFixture(GameFixtures.Combined(), coins: 20); root.OpenShop();
            root.Shop.TapCard(BoosterKind.Firefly);
            Assert.That(root.Shop.ConfirmOpen, Is.False); Assert.That(root.Coins, Is.EqualTo(20)); Assert.That(root.BoosterStock(BoosterKind.Firefly), Is.Zero);
            Capture("shop-insufficient"); root.ClosePanel();
            yield return Reload(); root.OpenShop();
            root.Shop.TapCard(BoosterKind.SpringRain);
            Assert.That(root.Shop.ConfirmOpen, Is.False); Assert.That(root.Coins, Is.EqualTo(240)); Assert.That(root.BuyBooster(BoosterKind.SpringRain), Is.False);
            Assert.That(root.BoosterStock(BoosterKind.SpringRain), Is.Zero); Capture("shop-locked");
            AssertPositiveScale(root.Shop);
        }
        [UnityTest]
        public IEnumerator ZeroStockPlusOpensFocusedShopAndCloseLeavesBoardUntouched()
        {
            root.LoadFixture(GameFixtures.Combined());
            var session = root.Session; var state = session.State; var birds = root.Board.Birds.Values.ToArray();
            root.TapBranch(0);
            var tool = Button(root.Hud, "Firefly");
            Assert.That(tool.transform.Find("Inventory badge/Zero stock plus").gameObject.activeSelf, Is.True);
            Assert.That(tool.transform.Find("Inventory badge/Owned").gameObject.activeSelf, Is.False);
            Capture("hud-plus"); tool.onClick.Invoke();
            Assert.That(root.Shop.IsOpen, Is.True); Assert.That(root.Shop.ConfirmOpen, Is.False); Assert.That(session.SelectedBranch, Is.EqualTo(-1));
            root.ClosePanel(); Assert.That(root.HasModal, Is.False); Assert.That(root.Session, Is.SameAs(session)); Assert.That(root.Session.State, Is.SameAs(state));
            Assert.That(root.Board.Birds.Values.ToArray(), Is.EqualTo(birds)); Assert.That(root.Session.MoveCount, Is.Zero);
            Assert.That(root.TapBranch(0), Is.True); yield return null;
        }
        [UnityTest]
        public IEnumerator PurchasePresentationCountsCoinsAndPopsOwnedBadge()
        {
            root.OpenShop(); Capture("ui_shop_boosters"); root.Shop.TapCard(BoosterKind.Firefly); Capture("ui_shop_confirm");
            AssertPositiveScale(root.Shop); GameMotion.Instant = false;
            Capture("purchase/open"); yield return new WaitForSeconds(0.1f);
            Button(root.Shop, "Confirm purchase").onClick.Invoke();
            var count = root.Shop.transform.Find("Coins");
            var badge = root.Shop.transform.Find("Firefly card/Owned badge");
            Assert.That(root.Shop.ConfirmOpen, Is.False); Assert.That(badge.localScale.x, Is.GreaterThan(1));
            for (int i = 0; i <= 8; i++) { Capture("purchase/" + (i * 100).ToString("D4") + "ms"); yield return new WaitForSeconds(0.1f); }
            Assert.That(Text(count), Is.EqualTo("210")); Assert.That(badge.localScale, Is.EqualTo(Vector3.one));
            AssertPositiveScale(root.Shop);
        }
    }
}
